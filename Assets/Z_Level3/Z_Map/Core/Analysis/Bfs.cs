using System.Collections;
using System.Collections.Generic;
using System.Linq;
using UnityEngine;

namespace Z_Map.Analysis
{
    /// <summary>Per-agent, frame-budgeted BFS. Only complete rounds replace the published waypoint.</summary>
    public class Bfs : NaviComponent
    {
        public const int CellsPerFrame = 10;
        private readonly NavigationController nc;
        private readonly NavigationEndpointState scratchEndpointState = new NavigationEndpointState();
        protected virtual int SearchFrame => Time.frameCount;

        // Growing buffers belong to the Character, never to the shared Bfs.
        internal sealed class SearchState
        {
            internal readonly Dictionary<NavUnit, int> steps = new Dictionary<NavUnit, int>();
            internal readonly Dictionary<NavUnit, NavUnit> pre = new Dictionary<NavUnit, NavUnit>();
            internal readonly Queue<NavUnit> queue = new Queue<NavUnit>();
            internal readonly List<NavUnit> path = new List<NavUnit>();
            internal readonly Dictionary<NavUnit, int> pathIndices = new Dictionary<NavUnit, int>();
            internal readonly List<NavUnit> fromFootprint = new List<NavUnit>();
            internal Dictionary<(int, int, int), NavUnit> graph;
            internal IEnumerator routine;
            internal int layoutVersion, graphCount, maxStep, lastFrame = -1, work;
            internal Vector3 current, roundDestination, resultTarget, resultDestination;
            internal bool resultDirect, resultFollowsTarget, resultStationary;

            internal void Cancel()
            {
                (routine as System.IDisposable)?.Dispose();
                routine = null;
                steps.Clear(); pre.Clear(); queue.Clear(); path.Clear(); pathIndices.Clear(); fromFootprint.Clear();
            }

            internal void Reset()
            {
                Cancel();
                graph = null;
                // A reset/target change in the same frame must not grant another
                // ten cells. The next real frame refreshes the work allowance.
            }
        }

        public Bfs(NavigationController nc) { this.nc = nc; }

        /// <summary>
        /// Advance at most ten cells once per real frame. maxStep still limits
        /// graph depth, independently of the per-frame work budget.
        /// Callers sharing a Bfs must supply their own endpointState.
        /// </summary>
        public Vector3 GetNextDir(Vector3 cur, Vector3 tar, int maxStep, float agentRadius,
            IReadOnlyCollection<int> passTypes, NavigationEndpointState endpointState = null)
        {
            var state = endpointState ?? scratchEndpointState;
            state.Prepare(nc, tar, agentRadius, passTypes);
            var search = state.searchState ?? (state.searchState = new SearchState());
            search.current = cur;
            if (nc.navUnits == null || nc.navUnits.Count == 0)
            {
                search.Cancel();
                state.isSearching = false;
                state.hasResult = false;
                state.moveTarget = cur;
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
                    search.roundDestination = tar;
                    search.routine = Search(cur, tar, maxStep, nc.GetClearanceOffsets(agentRadius), state, search);
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
            return ReadResult(cur, tar, state, search);
        }

        private Vector3 ReadResult(Vector3 cur, Vector3 tar, NavigationEndpointState state, SearchState search)
        {
            state.moveTarget = !state.hasResult || search.resultStationary ? cur
                : search.resultFollowsTarget && SameCell(search.resultDestination, tar) ? tar : search.resultTarget;
            state.directEndpoint = state.hasResult && search.resultDirect && !search.resultStationary;
            return nc.GetNormalWithoutY(state.moveTarget - cur);
        }

        private bool SameCell(Vector3 first, Vector3 second) =>
            nc.RealPos2MapPosInt(first) == nc.RealPos2MapPosInt(second);

        private void Publish(NavigationEndpointState state, SearchState search, Vector3 target,
            bool direct = false, bool followsTarget = false, bool stationary = false,
            NavUnit escape = null, bool finalApproach = false)
        {
            // A completed ordinary route to the snapshot is still useful, but
            // an obsolete blocked-target direct leg must never become active.
            if (finalApproach && !SameCell(search.roundDestination, state.destination)) return;
            search.resultTarget = target;
            search.resultDestination = search.roundDestination;
            search.resultDirect = direct;
            search.resultFollowsTarget = followsTarget;
            search.resultStationary = stationary;
            state.escapeAnchor = escape;
            state.finalApproach = finalApproach;
            state.hasResult = true;
        }

        private IEnumerator Search(Vector3 cur, Vector3 tar, int maxStep, IReadOnlyList<Vector2Int> offsets,
            NavigationEndpointState state, SearchState search)
        {
            search.pre.Clear(); search.steps.Clear(); search.queue.Clear();
            search.path.Clear(); search.pathIndices.Clear();
            var passTypes = state.passTypes;
            nc.TryGetGroundUnit(cur, out var first);
            nc.TryGetGroundUnit(tar, out var requestedEnd);
            bool targetWalkable = IsWalkable(requestedEnd, offsets, passTypes);
            if (state.finalApproach && !targetWalkable)
            {
                Publish(state, search, tar, true, true, finalApproach: true);
                yield break;
            }

            bool startWalkable = IsWalkable(first, offsets, passTypes);
            var escape = state.escapeAnchor;
            if (escape != null && (!nc.navUnits.TryGetValue((escape.pos.x, escape.pos.y, escape.pos.z), out var registered)
                || registered != escape || !IsWalkable(escape, offsets, passTypes)))
                escape = null;
            if (escape == null && !startWalkable)
            {
                // Finding the nearest legal start is budgeted too.
                foreach (var pair in nc.navUnits)
                {
                    if (search.work >= CellsPerFrame) yield return null;
                    search.work++;
                    if (IsCloser(pair.Value, escape, cur) && IsWalkable(pair.Value, offsets, passTypes))
                        escape = pair.Value;
                }
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
            if (targetWalkable && first == requestedEnd)
            {
                Publish(state, search, tar, followsTarget: true);
                yield break;
            }

            var end = targetWalkable ? requestedEnd : null;
            NavUnit minUnit = first;
            float minDis2 = (first.realPos - tar).sqrMagnitude;
            search.queue.Enqueue(first);
            search.steps[first] = 0;
            bool searchLimited = false;
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
                        searchLimited = true;
                        continue;
                    }
                    search.steps[next] = step + 1;
                    search.pre[next] = now;
                    search.queue.Enqueue(next);
                    if (next == end) { reachedTarget = true; break; }
                }
            }

            // A live target changing eligibility must not publish a stale
            // endpoint leg. Retry on the next round while keeping the old result.
            if (IsWalkable(requestedEnd, offsets, passTypes) != targetWalkable) yield break;
            if (!reachedTarget)
            {
                if (targetWalkable && minDis2 > 4f)
                {
                    Publish(state, search, cur, stationary: true);
                    yield break;
                }
                end = minUnit;
                if (end == first)
                {
                    if (targetWalkable || searchLimited)
                        Publish(state, search, cur, stationary: true);
                    else if (!IsAtAnchor(search.current, end))
                        Publish(state, search, end.realPos);
                    else
                        Publish(state, search, tar, true, true, finalApproach: true);
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

            // Movement continues on the previous completed waypoint. Rebase the
            // new route to the actor's current cell, never back to its old start.
            if (!nc.TryGetGroundUnit(search.current, out var current)
                || !search.pathIndices.TryGetValue(current, out int startIndex)) yield break;
            if (startIndex == 0)
            {
                Publish(state, search, reachedTarget ? tar : end.realPos, followsTarget: reachedTarget);
                yield break;
            }
            int radiusX = 1, radiusZ = 1;
            for (int o = 0; o < offsets.Count; o++)
            {
                radiusX = Mathf.Max(radiusX, Mathf.Abs(offsets[o].x));
                radiusZ = Mathf.Max(radiusZ, Mathf.Abs(offsets[o].y));
            }
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
                for (int x = checkLeft - radiusX; x <= checkRight + radiusX && valid; x++)
                for (int z = checkBack - radiusZ; z <= checkForward + radiusZ; z++)
                {
                    if (search.work >= CellsPerFrame) yield return null;
                    search.work++;
                    if (!CheckCell(x, z, origin.y, groundY, checkRight + radiusX, checkForward + radiusZ, out var cell))
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
            if (!nc.TryGetGroundUnit(search.current, out current)
                || !search.pathIndices.TryGetValue(current, out int latestIndex)) yield break;
            if (latestIndex == 0)
            {
                Publish(state, search, reachedTarget ? tar : end.realPos, followsTarget: reachedTarget);
                yield break;
            }
            if (waypointIndex >= latestIndex) waypointIndex = latestIndex - 1;
            if (!CanPass(current, search.path[latestIndex - 1], offsets, passTypes)) yield break;
            bool toRequestedTarget = reachedTarget && waypointIndex == 0 && smoothedToEnd;
            Publish(state, search, toRequestedTarget ? tar : search.path[waypointIndex].realPos,
                followsTarget: toRequestedTarget);
        }

        private static bool IsAtAnchor(Vector3 cur, NavUnit unit)
        {
            var delta = cur - unit.realPos;
            delta.y = 0f;
            return delta.sqrMagnitude <= 0.05f * 0.05f;
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
