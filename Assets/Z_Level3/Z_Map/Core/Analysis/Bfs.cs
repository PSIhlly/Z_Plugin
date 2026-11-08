using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Z_Map.Analysis
{
    /// <summary>Per-agent, frame-budgeted BFS with an independently consumed completed route.</summary>
    public class Bfs : NaviComponent
    {
        public const int CellsPerFrame = 10;
        private readonly NavigationController nc;
        private readonly NavigationEndpointState scratchEndpointState = new NavigationEndpointState();
        protected virtual int SearchFrame => Time.frameCount;

        internal struct EndpointCandidate
        {
            internal NavUnit unit;
            internal Vector2Int column;
            internal float distance;
        }

        // Growing buffers belong to the Character, never to the shared Bfs.
        internal sealed class SearchState
        {
            internal readonly Dictionary<NavUnit, int> steps = new Dictionary<NavUnit, int>();
            internal readonly Dictionary<NavUnit, NavUnit> pre = new Dictionary<NavUnit, NavUnit>();
            internal readonly Queue<NavUnit> queue = new Queue<NavUnit>();
            internal List<NavUnit> path = new List<NavUnit>();
            // Both lists run end-to-start. Swap them on publication, never copy
            // a whole route synchronously or clear it when a new search starts.
            internal List<NavUnit> route = new List<NavUnit>();
            internal int routeIndex = -1;
            internal bool routeReachesTarget;
            internal Dictionary<NavUnit, int> pathIndices = new Dictionary<NavUnit, int>();
            internal Dictionary<NavUnit, int> routeIndices = new Dictionary<NavUnit, int>();
            internal readonly List<NavUnit> fromFootprint = new List<NavUnit>();
            internal Dictionary<(int, int, int), NavUnit> graph;
            internal IEnumerator routine;
            internal int layoutVersion, graphCount, maxStep, lastFrame = -1, work;
            internal Vector3 current, resultTarget, resultDestination;
            internal bool resultDirect, resultFollowsTarget, resultStationary;
            internal NavUnit nearest;
            internal readonly List<EndpointCandidate> endpointHeap = new List<EndpointCandidate>();
            internal readonly HashSet<Vector2Int> endpointColumns = new HashSet<Vector2Int>();
            internal Vector3 roundRouteDestination;
            internal bool roundTargetAdjusted, resultTargetAdjusted;

            internal void Cancel()
            {
                (routine as System.IDisposable)?.Dispose();
                routine = null;
                steps.Clear(); pre.Clear(); queue.Clear(); path.Clear(); pathIndices.Clear(); fromFootprint.Clear();
                nearest = null;
                endpointHeap.Clear(); endpointColumns.Clear();
            }

            internal void Reset()
            {
                Cancel();
                ClearRoute();
                graph = null;
                // A reset/target change in the same frame must not grant another
                // ten cells. The next real frame refreshes the work allowance.
            }

            internal void ClearRoute()
            {
                route.Clear();
                routeIndices.Clear();
                routeIndex = -1;
                routeReachesTarget = false;
            }
        }

        public Bfs(NavigationController nc) { this.nc = nc; }

        /// <summary>
        /// Advance at most ten cells once per real frame. maxStep still limits
        /// graph depth, independently of the per-frame work budget.
        /// Callers sharing a Bfs must supply their own endpointState.
        /// </summary>
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep, float agentRadius,
            IReadOnlyCollection<int> passTypes, NavigationEndpointState endpointState = null, int agentSize = 1)
        {
            agentSize = Mathf.Max(1, agentSize);
            var state = endpointState ?? scratchEndpointState;
            var navigationOffset = nc.GetNavigationOffset(agentSize);
            state.Prepare(nc, tar, agentRadius, passTypes, agentSize, navigationOffset);
            cur -= navigationOffset;
            tar -= navigationOffset;
            var search = state.searchState ?? (state.searchState = new SearchState());
            search.current = cur;
            if (nc.navUnits == null || nc.navUnits.Count == 0)
            {
                search.Cancel();
                search.ClearRoute();
                state.isSearching = false;
                state.hasResult = false;
                state.localMoveTarget = cur;
                state.moveTarget = cur + navigationOffset;
                state.directEndpoint = false;
                return Vector3.zero;
            }

            // A concrete dictionary enumerator can span frames only while its
            // membership is unchanged. Ordinary value/link refreshes need not
            // starve a long search by restarting it every periodic rebuild.
            if (search.graph != nc.navUnits || search.layoutVersion != nc.LayoutVersion
                || search.graphCount != nc.navUnits.Count || search.maxStep != maxStep)
            {
                search.Cancel();
                search.ClearRoute();
                state.hasResult = false;
                search.graph = nc.navUnits;
                search.layoutVersion = nc.LayoutVersion;
                search.graphCount = nc.navUnits.Count;
                search.maxStep = maxStep;
                state.isSearching = true;
            }

            int frame = SearchFrame;
            if (search.lastFrame != frame)
            {
                search.lastFrame = frame;
                search.work = 0;
                if (search.routine == null)
                {
                    search.routine = Search(cur, tar, maxStep, nc.GetClearanceOffsets(agentRadius, agentSize), state, search);
                }
                state.isSearching = true;
                if (!search.routine.MoveNext())
                {
                    (search.routine as System.IDisposable)?.Dispose();
                    search.routine = null;
                    state.isSearching = false;
                    state.completedSearchCount++;
                    // Start the next round NEXT frame, retaining this result.
                }
            }
            else if (!state.hasResult)
                state.isSearching = true;
            var direction = ReadResult(cur, tar, state, search);
            state.moveTarget = state.localMoveTarget + navigationOffset;
            return direction;
        }

        private Vector3 ReadResult(Vector3 cur, Vector3 tar, NavigationEndpointState state, SearchState search)
        {
            if (state.hasResult && search.routeIndex >= 0)
            {
                // Reaching a waypoint consumes the completed route immediately;
                // the background round is not involved in this transition.
                while (search.routeIndex > 0 && AtRoutePoint(cur, search.route[search.routeIndex].realPos))
                    search.routeIndex--;

                var node = search.route[search.routeIndex];
                var offsets = nc.GetClearanceOffsets(state.radius, state.agentSize);
                Vector3 routeTarget = search.routeIndex == 0 && search.routeReachesTarget
                    ? (!search.resultTargetAdjusted && SameCell(search.resultDestination, tar) ? tar : search.resultDestination) : node.realPos;
                bool valid = CanFollowRoute(cur, node, routeTarget, offsets, state.passTypes);
                if (!valid && TryGetGroundUnit(cur, offsets, out var current)
                    && search.routeIndices.TryGetValue(current, out int rawIndex) && rawIndex > search.routeIndex)
                {
                    // A shortcut may no longer be safe while the completed raw
                    // route still goes around it. Resume that route locally,
                    // rather than repeating the same rejected shortcut forever.
                    int nextIndex = AtRoutePoint(cur, current.realPos) ? rawIndex - 1 : rawIndex;
                    var next = search.route[nextIndex];
                    Vector3 nextTarget = nextIndex == 0 && search.routeReachesTarget
                        ? (!search.resultTargetAdjusted && SameCell(search.resultDestination, tar) ? tar : search.resultDestination) : next.realPos;
                    if (CanFollowRoute(cur, next, nextTarget, offsets, state.passTypes))
                    {
                        search.routeIndex = nextIndex;
                        node = next;
                        valid = true;
                    }
                }
                if (!valid)
                {
                    search.ClearRoute();
                    state.hasResult = false;
                }
                else
                {
                    search.resultTarget = search.routeIndex == 0 && search.routeReachesTarget
                        ? search.resultDestination : node.realPos;
                    search.resultFollowsTarget = search.routeIndex == 0 && search.routeReachesTarget && !search.resultTargetAdjusted;
                }
            }
            state.localMoveTarget = !state.hasResult || search.resultStationary ? cur
                : search.resultFollowsTarget && SameCell(search.resultDestination, tar) ? tar : search.resultTarget;
            state.directEndpoint = state.hasResult && search.resultDirect && !search.resultStationary;
            return nc.GetNormalWithoutY(state.localMoveTarget - cur);
        }

        private static bool AtRoutePoint(Vector3 current, Vector3 target)
        {
            current.y = target.y;
            return (current - target).sqrMagnitude <= 1e-8f;
        }

        private bool CanFollowRoute(Vector3 position, NavUnit target, Vector3 worldTarget, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes)
        {
            if (!IsRegistered(target) || !IsWalkable(target, offsets, passTypes)
                || !TryGetGroundUnit(position, offsets, out var current) || !IsWalkable(current, offsets, passTypes))
                return false;
            if (current == target) return true;
            if (Mathf.Abs(target.pos.x - current.pos.x) + Mathf.Abs(target.pos.z - current.pos.z) <= 1)
                return CanPass(current, target, offsets, passTypes);

            // The first leg may be a previously smoothed shortcut. Recheck only
            // its local next-cell transitions, not the entire distant rectangle.
            // Character navigation subdivides large frame moves at half a cell.
            int dx = System.Math.Sign(target.pos.x - current.pos.x);
            int dz = System.Math.Sign(target.pos.z - current.pos.z);
            NavUnit nextX = null, nextZ = null;
            if (dx != 0 && (!nc.TryGetOffsetUnit(current, new Vector2Int(dx, 0), out nextX)
                || !CanPass(current, nextX, offsets, nextX.passTypes))) return false;
            if (dz != 0 && (!nc.TryGetOffsetUnit(current, new Vector2Int(0, dz), out nextZ)
                || !CanPass(current, nextZ, offsets, nextZ.passTypes))) return false;
            if (dx != 0 && dz != 0
                && (!nc.TryGetOffsetUnit(current, new Vector2Int(dx, dz), out var corner)
                    || !CanPass(nextX, corner, offsets, corner.passTypes)
                    || !CanPass(nextZ, corner, offsets, corner.passTypes))) return false;

            // Neighbouring footprint cells only constrain physical clearance;
            // actor pass types constrain the actual center-line cells instead.
            Vector3 delta = worldTarget - position;
            delta.y = 0f;
            Vector3 step = delta.normalized * Mathf.Min(nc.MaxRouteMoveStep, delta.magnitude);
            if (!TryGetGroundUnit(position + step, offsets, out var entered) || !IsWalkable(entered, offsets, passTypes))
                return false;
            if (entered.pos.x != current.pos.x && entered.pos.z != current.pos.z)
            {
                float tx = (current.realPos.x + (nextX.realPos.x - current.realPos.x) * 0.5f - position.x) / step.x;
                float tz = (current.realPos.z + (nextZ.realPos.z - current.realPos.z) * 0.5f - position.z) / step.z;
                if (tx < tz - 0.0001f && !HasAllPassTypes(nextX, passTypes)) return false;
                if (tz < tx - 0.0001f && !HasAllPassTypes(nextZ, passTypes)) return false;
            }
            return true;
        }

        private bool SameCell(Vector3 first, Vector3 second) =>
            nc.RealPos2MapPosInt(first) == nc.RealPos2MapPosInt(second);

        private void Publish(NavigationEndpointState state, SearchState search, Vector3 target,
            bool direct = false, bool followsTarget = false, bool stationary = false,
            NavUnit escape = null, int routeIndex = -1, bool routeReachesTarget = false)
        {
            if (routeIndex >= 0)
            {
                var previous = search.route;
                search.route = search.path;
                search.path = previous;
                var previousIndices = search.routeIndices;
                search.routeIndices = search.pathIndices;
                search.pathIndices = previousIndices;
                search.routeIndex = routeIndex;
                search.routeReachesTarget = routeReachesTarget;
            }
            else
                search.ClearRoute();
            search.resultTarget = target;
            search.resultDestination = search.roundRouteDestination;
            search.resultTargetAdjusted = search.roundTargetAdjusted;
            search.resultDirect = direct;
            search.resultFollowsTarget = followsTarget && !search.roundTargetAdjusted;
            search.resultStationary = stationary;
            state.escapeAnchor = escape;
            state.hasResult = true;
        }

        private IEnumerator Search(Vector3 cur, Vector3 tar, int maxStep, IReadOnlyList<Vector2Int> offsets,
            NavigationEndpointState state, SearchState search)
        {
            search.pre.Clear(); search.steps.Clear(); search.queue.Clear();
            search.path.Clear(); search.pathIndices.Clear();
            search.roundRouteDestination = tar;
            search.roundTargetAdjusted = false;
            var passTypes = state.passTypes;
            TryGetGroundUnit(cur, offsets, out var first);
            TryGetGroundUnit(tar, offsets, out var requestedEnd);

            bool startWalkable = IsWalkable(first, offsets, passTypes);
            var escape = state.escapeAnchor;
            // Even-sized agents first align with the corner represented by
            // their lower-right Tile, then consume ordinary cached graph legs.
            if ((state.agentSize & 1) == 0 && escape == null && !state.hasResult
                && startWalkable && !IsAtAnchor(cur, first))
                escape = first;
            if (escape != null && (!nc.navUnits.TryGetValue((escape.pos.x, escape.pos.y, escape.pos.z), out var registered)
                || registered != escape || !IsWalkable(escape, offsets, passTypes)))
                escape = null;
            if (escape == null && !startWalkable)
            {
                using (var nearest = FindStartAnchor(cur, offsets, passTypes, search))
                    while (nearest.MoveNext()) yield return nearest.Current;
                escape = search.nearest;
            }
            if (escape != null)
            {
                if (!IsAtAnchor(search.current, escape))
                {
                    Publish(state, search, escape.realPos, true, escape: escape);
                    yield break;
                }
                first = escape;
                startWalkable = true;
            }
            if (!startWalkable)
            {
                Publish(state, search, cur, stationary: true);
                yield break;
            }
            if (!IsWalkable(requestedEnd, offsets, passTypes))
            {
                // Select a walkable target by Euclidean distance BEFORE BFS.
                // Reachability is the following BFS's responsibility.
                using (var nearest = FindTarget(tar, offsets, passTypes, search))
                    while (nearest.MoveNext()) yield return nearest.Current;
                requestedEnd = search.nearest;
                if (requestedEnd == null)
                {
                    Publish(state, search, cur, stationary: true);
                    yield break;
                }
                tar = requestedEnd.realPos;
                search.roundRouteDestination = tar;
                search.roundTargetAdjusted = true;
            }
            if ((state.agentSize & 1) == 0)
            {
                search.roundTargetAdjusted = true;
                tar = requestedEnd.realPos;
                search.roundRouteDestination = tar;
            }
            if (first == requestedEnd)
            {
                Publish(state, search, tar, followsTarget: true);
                yield break;
            }

            var end = requestedEnd;
            NavUnit minUnit = first;
            float minDis2 = (first.realPos - tar).sqrMagnitude;
            search.queue.Enqueue(first);
            search.steps[first] = 0;
            bool reachedTarget = false;
            while (search.queue.Count > 0 && !reachedTarget)
            {
                if (search.work >= CellsPerFrame) yield return null;
                search.work++;
                var now = search.queue.Dequeue();
                if (!IsRegistered(now)) continue;
                if (!PrepareFootprint(now, offsets, passTypes, search.fromFootprint)) continue;
                int step = search.steps[now];
                if (IsCloser(now, minUnit, tar))
                {
                    minDis2 = (now.realPos - tar).sqrMagnitude;
                    minUnit = now;
                }
                for (int n = 0; n < now.links.Count; n++)
                {
                    var next = now.links[n];
                    // Visit rejection precedes all expensive traversal checks.
                    if (search.steps.ContainsKey(next)
                        || !IsRegistered(next)
                        || !CanPassLinked(next, offsets, passTypes, search.fromFootprint)) continue;
                    if (step >= maxStep)
                    {
                        continue;
                    }
                    search.steps[next] = step + 1;
                    search.pre[next] = now;
                    search.queue.Enqueue(next);
                    if (next == end)
                    {
                        reachedTarget = true;
                        break;
                    }
                }
            }

            // Recheck the selected point after the multi-frame graph search.
            if (!IsWalkable(requestedEnd, offsets, passTypes)) yield break;
            if (!reachedTarget)
            {
                if (minDis2 > 4f)
                {
                    Publish(state, search, cur, stationary: true);
                    yield break;
                }
                end = minUnit;
                if (end == first)
                {
                    Publish(state, search, cur, stationary: true);
                    yield break;
                }
            }

            // Reconstruction and smoothing share the same ten-cell allowance.
            var cursor = end;
            while (true)
            {
                if (search.work >= CellsPerFrame) yield return null;
                search.work++;
                if (!IsRegistered(cursor)) yield break;
                search.pathIndices[cursor] = search.path.Count;
                search.path.Add(cursor);
                if (cursor == first) break;
                if (!search.pre.TryGetValue(cursor, out var parent)
                    || !CanPass(parent, cursor, offsets, passTypes)) yield break;
                if (GlobalSettings.NAV_DEBUG)
                    Debug.DrawLine(cursor.realPos + Vector3.up, parent.realPos + Vector3.up);
                cursor = parent;
            }

            // Movement continues on the previous completed route. Rebase the
            // new route to the actor's current cell, never back to its old start.
            if (!TryGetGroundUnit(search.current, offsets, out var current)
                || !search.pathIndices.TryGetValue(current, out int startIndex)) yield break;
            if (startIndex == 0)
            {
                Publish(state, search, reachedTarget ? tar : end.realPos, followsTarget: reachedTarget);
                yield break;
            }
            int minX = 0, maxX = 0, minZ = 0, maxZ = 0;
            for (int o = 0; o < offsets.Count; o++)
            {
                minX = Mathf.Min(minX, offsets[o].x); maxX = Mathf.Max(maxX, offsets[o].x);
                minZ = Mathf.Min(minZ, offsets[o].y); maxZ = Mathf.Max(maxZ, offsets[o].y);
            }
            if (offsets.Count == 1) { minX = minZ = -1; maxX = maxZ = 1; }
            var origin = current.pos;
            float groundY = current.realPos.y;
            int left = origin.x, right = origin.x, back = origin.z, forward = origin.z;
            int waypointIndex = 0;
            bool smoothedToEnd = true;
            bool rectanglePassTypesClear = true;
            for (int i = startIndex - 1; i >= 0; i--)
            {
                var candidate = search.path[i].pos;
                int checkLeft = left, checkRight = right, checkBack = back, checkForward = forward;
                if (candidate.x > right) { right = candidate.x; checkLeft = checkRight = right; }
                if (candidate.x < left) { left = candidate.x; checkLeft = checkRight = left; }
                if (candidate.z > forward) { forward = candidate.z; checkForward = checkBack = forward; }
                if (candidate.z < back) { back = candidate.z; checkForward = checkBack = back; }
                bool valid = true;
                for (int x = checkLeft + minX; x <= checkRight + maxX && valid; x++)
                for (int z = checkBack + minZ; z <= checkForward + maxZ; z++)
                {
                    if (search.work >= CellsPerFrame) yield return null;
                    search.work++;
                    if (!CheckCell(x, z, origin.y, groundY, checkRight + maxX, checkForward + maxZ, out var cell))
                    { valid = false; break; }
                    if (!HasAllPassTypes(cell, passTypes)) rectanglePassTypesClear = false;
                }

                // Pass types apply to the center line, not neighbouring footprint
                // cells. If the whole checked rectangle accepts these types,
                // every center line inside it does too: no repeated O(path) scan.
                // Otherwise retain the exact line test, not a rectangle veto.
                int lineX = origin.x, lineZ = origin.z;
                int dx = Mathf.Abs(candidate.x - origin.x), dz = Mathf.Abs(candidate.z - origin.z);
                int sx = origin.x < candidate.x ? 1 : -1, sz = origin.z < candidate.z ? 1 : -1;
                int error = dx - dz;
                while (valid && !rectanglePassTypesClear)
                {
                    if (search.work >= CellsPerFrame) yield return null;
                    search.work++;
                    if (!nc.navUnits.TryGetValue((lineX, origin.y, lineZ), out var center)
                        || !HasAllPassTypes(center, passTypes)) { valid = false; break; }
                    if (lineX == candidate.x && lineZ == candidate.z) break;
                    int twice = error * 2;
                    if (twice > -dz) { error -= dz; lineX += sx; }
                    if (twice < dx) { error += dx; lineZ += sz; }
                }
                if (!valid)
                {
                    waypointIndex = i == startIndex - 1 ? i : i + 1;
                    smoothedToEnd = false;
                    break;
                }
            }
            if (!TryGetGroundUnit(search.current, offsets, out current)
                || !search.pathIndices.TryGetValue(current, out int latestIndex)) yield break;
            if (latestIndex == 0)
            {
                Publish(state, search, reachedTarget ? tar : end.realPos, followsTarget: reachedTarget);
                yield break;
            }
            if (waypointIndex >= latestIndex) waypointIndex = latestIndex - 1;
            if (!CanPass(current, search.path[latestIndex - 1], offsets, passTypes)) yield break;
            bool toRequestedTarget = reachedTarget && waypointIndex == 0 && smoothedToEnd;
            Vector3 nextTarget = toRequestedTarget ? tar : search.path[waypointIndex].realPos;
            if (state.hasResult && search.routeIndex >= 0 && !AtRoutePoint(search.current, state.localMoveTarget))
            {
                // Entering a rounded cell does not mean its turn-center was
                // reached. Retain that center instead of cutting its corner.
                Vector3 oldDirection = state.localMoveTarget - search.current;
                Vector3 newDirection = nextTarget - search.current;
                oldDirection.y = newDirection.y = 0f;
                if (Vector3.Dot(oldDirection.normalized, newDirection.normalized) < 0.9999f)
                {
                    if (AtRoutePoint(state.localMoveTarget, current.realPos))
                    {
                        waypointIndex = latestIndex;
                        nextTarget = current.realPos;
                        toRequestedTarget = false;
                    }
                    else if (SameCell(search.resultDestination, tar)
                        && CanFollowRoute(search.current, search.route[search.routeIndex], state.localMoveTarget, offsets, passTypes))
                    {
                        // A same-target refresh must not reverse a still-valid
                        // smoothed leg. New targets or a blocked leg may reroute.
                        yield break;
                    }
                }
            }
            Publish(state, search, nextTarget,
                followsTarget: toRequestedTarget, routeIndex: waypointIndex, routeReachesTarget: reachedTarget);
        }

        private static void PushEndpoint(SearchState search, EndpointCandidate candidate)
        {
            var heap = search.endpointHeap;
            int at = heap.Count;
            heap.Add(candidate);
            while (at > 0)
            {
                int parent = (at - 1) / 2;
                if (heap[parent].distance <= candidate.distance) break;
                heap[at] = heap[parent]; at = parent;
            }
            heap[at] = candidate;
        }

        private static EndpointCandidate PopEndpoint(SearchState search)
        {
            var heap = search.endpointHeap;
            var result = heap[0]; var last = heap[heap.Count - 1];
            heap.RemoveAt(heap.Count - 1);
            int at = 0;
            while (at * 2 + 1 < heap.Count)
            {
                int child = at * 2 + 1;
                if (child + 1 < heap.Count && heap[child + 1].distance < heap[child].distance) child++;
                if (heap[child].distance >= last.distance) break;
                heap[at] = heap[child]; at = child;
            }
            if (heap.Count > 0) heap[at] = last;
            return result;
        }

        private void QueueEndpointColumn(SearchState search, Vector3 position, int x, int z,
            Vector2Int min, Vector2Int max)
        {
            if (x < min.x || x > max.x || z < min.y || z > max.y) return;
            var column = new Vector2Int(x, z);
            if (!search.endpointColumns.Add(column)) return;
            var center = nc.MapPos2RealPos(new Vector3Int(x, 0, z));
            float dx = Mathf.Max(0f, Mathf.Abs(center.x - position.x) - .0002f);
            float dz = Mathf.Max(0f, Mathf.Abs(center.z - position.z) - .0002f);
            PushEndpoint(search, new EndpointCandidate { column = column, distance = dx * dx + dz * dz });
        }

        private IEnumerator<object> FindTarget(Vector3 position, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes, SearchState search)
        {
            search.nearest = null;
            search.endpointHeap.Clear(); search.endpointColumns.Clear();
            if (nc.TryGetColumnBounds(out var min, out var max))
            {
                var center = nc.RealPos2MapPosInt(position);
                QueueEndpointColumn(search, position, Mathf.Clamp(center.x, min.x, max.x),
                    Mathf.Clamp(center.z, min.y, max.y), min, max);
                int columns = 0;
                bool fallback = false;
                while (search.endpointHeap.Count > 0)
                {
                    if (search.work >= CellsPerFrame) yield return null;
                    search.work++;
                    if (!nc.CanSearchNearbyColumns || columns > Mathf.Max(32, nc.navUnits.Count))
                    { fallback = true; break; }
                    var candidate = PopEndpoint(search);
                    if (candidate.unit != null)
                    {
                        if (IsRegistered(candidate.unit) && IsWalkable(candidate.unit, offsets, passTypes))
                        { search.nearest = candidate.unit; yield break; }
                        continue;
                    }
                    columns++;
                    int x = candidate.column.x, z = candidate.column.y;
                    if (nc.TryGetColumnHeights(x, z, out var heights))
                        foreach (int y in heights)
                        {
                            if (search.work >= CellsPerFrame) yield return null;
                            search.work++;
                            if (nc.navUnits.TryGetValue((x, y, z), out var node))
                                PushEndpoint(search, new EndpointCandidate { unit = node,
                                    distance = (node.realPos - position).sqrMagnitude });
                        }
                    QueueEndpointColumn(search, position, x + 1, z, min, max);
                    QueueEndpointColumn(search, position, x - 1, z, min, max);
                    QueueEndpointColumn(search, position, x, z + 1, min, max);
                    QueueEndpointColumn(search, position, x, z - 1, min, max);
                }
                if (!fallback) yield break;
            }
            // Irregular centers or very sparse bounds: order actual nodes by
            // Euclidean distance instead of expanding an enormous empty area.
            search.endpointHeap.Clear(); search.endpointColumns.Clear();
            foreach (var node in nc.navUnits.Values)
            {
                if (search.work >= CellsPerFrame) yield return null;
                search.work++;
                PushEndpoint(search, new EndpointCandidate { unit = node,
                    distance = (node.realPos - position).sqrMagnitude });
            }
            while (search.endpointHeap.Count > 0)
            {
                if (search.work >= CellsPerFrame) yield return null;
                search.work++;
                var node = PopEndpoint(search).unit;
                if (IsRegistered(node) && IsWalkable(node, offsets, passTypes))
                { search.nearest = node; yield break; }
            }
        }

        private IEnumerator<object> FindStartAnchor(Vector3 position, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes, SearchState search)
        {
            search.nearest = null;
            if (nc.CanSearchNearbyColumns)
            {
                var center = nc.RealPos2MapPosInt(position);
                // Reuse the map's cached height-column index. Near an edge or
                // small obstacle, a few columns suffice to prove the global
                // nearest point, including candidates on other height layers.
                for (int ring = 0; ring <= 3; ring++)
                {
                    for (int x = center.x - ring; x <= center.x + ring; x++)
                    for (int z = center.z - ring; z <= center.z + ring; z++)
                    {
                        if (ring > 0 && x > center.x - ring && x < center.x + ring
                            && z > center.z - ring && z < center.z + ring) continue;
                        if (!nc.TryGetColumnHeights(x, z, out var heights) || heights.Count == 0)
                        {
                            if (search.work >= CellsPerFrame) yield return null;
                            search.work++;
                            continue;
                        }
                        foreach (int y in heights)
                        {
                            if (search.work >= CellsPerFrame) yield return null;
                            search.work++;
                            if (nc.navUnits.TryGetValue((x, y, z), out var candidate)
                                && IsCloser(candidate, search.nearest, position)
                                && IsWalkable(candidate, offsets, passTypes)) search.nearest = candidate;
                        }
                    }
                    var low = nc.MapPos2RealPos(new Vector3Int(center.x - ring - 1, 0, center.z - ring - 1));
                    var high = nc.MapPos2RealPos(new Vector3Int(center.x + ring + 1, 0, center.z + ring + 1));
                    float gap = Mathf.Min(Mathf.Min(Mathf.Abs(low.x - position.x), Mathf.Abs(high.x - position.x)),
                        Mathf.Min(Mathf.Abs(low.z - position.z), Mathf.Abs(high.z - position.z))) - .0002f;
                    if (search.nearest != null && gap > 0f
                        && (search.nearest.realPos - position).sqrMagnitude + .000001f < gap * gap)
                        yield break;
                }
            }
            // An unproven start anchor retains the exhaustive nearest lookup.
            foreach (var pair in nc.navUnits)
            {
                if (search.work >= CellsPerFrame) yield return null;
                search.work++;
                if (IsCloser(pair.Value, search.nearest, position)
                    && IsWalkable(pair.Value, offsets, passTypes)) search.nearest = pair.Value;
            }
        }

        private static bool IsAtAnchor(Vector3 cur, NavUnit unit)
        {
            var delta = cur - unit.realPos;
            delta.y = 0f;
            return delta.sqrMagnitude <= 0.05f * 0.05f;
        }

        private bool TryGetGroundUnit(Vector3 position, IReadOnlyList<Vector2Int> offsets, out NavUnit unit)
        {
            bool corner = offsets.Count > 0 && offsets[0].x + offsets[offsets.Count - 1].x != 0;
            return nc.TryGetNavigationGroundUnit(position, corner, out unit);
        }

        private bool IsRegistered(NavUnit unit) => unit != null
            && nc.navUnits.TryGetValue((unit.pos.x, unit.pos.y, unit.pos.z), out var registered)
            && registered == unit;

        internal bool IsWalkable(NavUnit unit, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes)
        {
            if (!nc.IsBaseWalkable(unit) || !HasAllPassTypes(unit, passTypes)) return false;
            for (int i = 0; i < offsets.Count; i++)
            {
                var offset = offsets[i];
                if (offset == Vector2Int.zero) continue;
                if (!nc.TryGetOffsetUnit(unit, offset, out var footprint) || !nc.IsBaseWalkable(footprint)) return false;
            }
            return true;
        }

        private bool PrepareFootprint(NavUnit from, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes, List<NavUnit> footprint)
        {
            footprint.Clear();
            if (!nc.IsBaseWalkable(from) || !HasAllPassTypes(from, passTypes)) return false;
            for (int i = 0; i < offsets.Count; i++)
            {
                if (offsets[i] == Vector2Int.zero) continue;
                if (!nc.TryGetOffsetUnit(from, offsets[i], out var unit) || !nc.IsBaseWalkable(unit)) return false;
                footprint.Add(unit);
            }
            return true;
        }

        // from is already walkable and next comes directly from from.links.
        // Reuse from's prepared footprint for every outgoing edge of this cell.
        private bool CanPassLinked(NavUnit next, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes, List<NavUnit> fromFootprint)
        {
            if (!nc.IsBaseWalkable(next) || !HasAllPassTypes(next, passTypes)) return false;
            int index = 0;
            for (int i = 0; i < offsets.Count; i++)
            {
                if (offsets[i] == Vector2Int.zero) continue;
                if (!nc.TryGetOffsetUnit(next, offsets[i], out var to) || !nc.IsBaseWalkable(to)
                    || !fromFootprint[index++].links.Contains(to)) return false;
            }
            return true;
        }

        public bool CanPass(NavUnit target) => nc.IsBaseWalkable(target);

        /// <summary>Standalone edge validation. Visit records belong to each agent's search.</summary>
        public bool CanPass(NavUnit from, NavUnit target, IReadOnlyList<Vector2Int> offsets,
            IReadOnlyCollection<int> passTypes)
        {
            if (!nc.IsBaseWalkable(from) || !nc.IsBaseWalkable(target)
                || !from.links.Contains(target) || !HasAllPassTypes(target, passTypes)) return false;
            for (int i = 0; i < offsets.Count; i++)
            {
                var offset = offsets[i];
                if (offset == Vector2Int.zero) continue;
                if (!nc.TryGetOffsetUnit(from, offset, out var fromUnit)
                    || !nc.TryGetOffsetUnit(target, offset, out var toUnit)
                    || !nc.IsBaseWalkable(fromUnit) || !nc.IsBaseWalkable(toUnit)
                    || !fromUnit.links.Contains(toUnit)) return false;
            }
            return true;
        }

        private static bool IsCloser(NavUnit candidate, NavUnit current, Vector3 position)
        {
            if (current == null) return true;
            float candidateDistance = (candidate.realPos - position).sqrMagnitude;
            float currentDistance = (current.realPos - position).sqrMagnitude;
            if (candidateDistance != currentDistance) return candidateDistance < currentDistance;
            if (candidate.pos.y != current.pos.y) return candidate.pos.y < current.pos.y;
            if (candidate.pos.x != current.pos.x) return candidate.pos.x < current.pos.x;
            return candidate.pos.z < current.pos.z;
        }

        public bool Check(int startX, int endX, int mapY, float realY, int startZ, int endZ)
        {
            for (int x = startX; x <= endX; x++)
            for (int z = startZ; z <= endZ; z++)
                if (!CheckCell(x, z, mapY, realY, endX, endZ)) return false;
            return true;
        }

        private bool CheckCell(int x, int z, int y, float realY, int endX, int endZ)
            => CheckCell(x, z, y, realY, endX, endZ, out _);

        private bool CheckCell(int x, int z, int y, float realY, int endX, int endZ, out NavUnit unit)
        {
            if (!nc.navUnits.TryGetValue((x, y, z), out unit) || !nc.IsBaseWalkable(unit)
                || Mathf.Abs(unit.realPos.y - realY) > nc.step) return false;
            if (x < endX && (!nc.navUnits.TryGetValue((x + 1, y, z), out var right)
                || !nc.IsBaseWalkable(right) || !unit.links.Contains(right) || !right.links.Contains(unit))) return false;
            if (z < endZ && (!nc.navUnits.TryGetValue((x, y, z + 1), out var forward)
                || !nc.IsBaseWalkable(forward) || !unit.links.Contains(forward) || !forward.links.Contains(unit))) return false;
            return true;
        }

        private static bool HasAllPassTypes(NavUnit unit, IReadOnlyCollection<int> passTypes)
        {
            if (unit.passTypes == null || unit.passTypes.Count == 0) return true;
            if (passTypes == null || passTypes.Count == 0) return false;
            foreach (int requiredType in unit.passTypes)
                if (!passTypes.Contains(requiredType)) return false;
            return true;
        }

        public void DebugPath(List<NavUnit> path)
        {
            for (int i = 1; i < path.Count; i++)
                Debug.DrawLine(path[i - 1].realPos + Vector3.up, path[i].realPos + Vector3.up);
        }
    }
}
