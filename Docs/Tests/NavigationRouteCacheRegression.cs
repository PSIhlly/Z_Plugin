using System;
using System.Collections.Generic;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Run only in the isolated regression Editor project. No story, scene or save
// is read, and logical frames deliberately do not depend on Editor frame time.
public static class NavigationRouteCacheRegression
{
    private static readonly int[] NoTypes = Array.Empty<int>();
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static int checks, pendingMovementFrames;
    private static MapManager map;
    private static InstancePoolManager pool;

    public static void Run()
    {
        try
        {
            var go = new GameObject("route-cache-map");
            owned.Add(go);
            go.SetActive(false);
            map = go.AddComponent<MapManager>();
            typeof(Z_MonoSingleton<MapManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, map);
            map.data = new MapInfo
            {
                mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(40, 5, 40),
                    new Vector3Int(8, 3, 8), "", "", "", ""),
                maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
                mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
            };
            map.utilCtrl = new MapUtilController(map);
            Check(Bfs.CellsPerFrame == 10, "the search work allowance remains ten cells");
            ContinuousMovementAtSix();
            ConsumePublishedRouteWhileSearchIsSuspended();
            TwoCharactersDoNotSharePublishedRoutes();
            MovingTargetRetainsSafeRoute();
            foreach (Invalidation kind in Enum.GetValues(typeof(Invalidation)))
                RefuseInvalidNextEdge(kind);
            ResetAndCapabilityChangesDiscardRoutes();
            CompleteFinalCoordinate();
            RealCharacterSpendsRemainingMovement();
            EndpointPhaseSemantics();
            SmoothedPassTypesAndRetargeting();
            Debug.Log("NAVIGATION_ROUTE_CACHE_REGRESSION_PASS checks=" + checks
                + " pendingMovementFrames=" + pendingMovementFrames);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (pool?.pools != null) pool.pools.Clear();
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private sealed class ControlledBfs : Bfs
    {
        private int frame;
        protected override int SearchFrame => frame;
        public ControlledBfs(NavigationController controller) : base(controller) { }
        public void Advance() { frame++; }
    }

    private sealed class Corridor
    {
        public readonly List<NavUnit> cells = new List<NavUnit>();
        public readonly NavigationController navigation;
        public readonly ControlledBfs bfs;
        public Vector3 start => cells[0].realPos;
        public Vector3 target => cells[cells.Count - 1].realPos;

        public Corridor()
        {
            navigation = new NavigationController(map)
            {
                navUnits = new Dictionary<(int, int, int), NavUnit>(), step = .5f
            };
            // One-cell-wide serpentine corridor. The smoothing rectangle needs
            // absent neighbours, so every initial smoothed waypoint is only one
            // edge away. Its full 118-edge route takes many budgeted frames.
            int x = 5, z = 5;
            Add(x, z);
            for (int stripe = 0; stripe < 12; stripe++)
            {
                int sign = (stripe & 1) == 0 ? 1 : -1;
                for (int edge = 0; edge < 8; edge++) Add(x += sign, z);
                if (stripe < 11) { Add(x, ++z); Add(x, ++z); }
            }
            bfs = new ControlledBfs(navigation);
        }

        private void Add(int x, int z)
        {
            var node = new NavUnit
            {
                pos = new Vector3Int(x, 0, z), realPos = new Vector3(x, 0, z),
                links = new List<NavUnit>(), passTypes = new HashSet<int>(),
                dirMaxY = new float[4], dirGroundY = new float[4]
            };
            if (cells.Count > 0)
            {
                var previous = cells[cells.Count - 1];
                previous.links.Add(node);
                node.links.Add(previous);
            }
            cells.Add(node);
            navigation.navUnits.Add((x, 0, z), node);
        }
    }

    private static Vector3 Query(Corridor corridor, Vector3 position, Vector3 target,
        NavigationEndpointState state, float radius = .4f, IReadOnlyCollection<int> types = null)
    {
        Vector3 direction = corridor.bfs.GetNextDir(position, target, 999, radius, types ?? NoTypes, state);
        Check(state.searchedCellsThisFrame <= 10, "all searches and same-frame route reads stay within ten cells");
        return direction;
    }

    private static void Warm(Corridor corridor, Vector3 position, Vector3 target, NavigationEndpointState state)
    {
        int frames = 0;
        do
        {
            corridor.bfs.Advance();
            Query(corridor, position, target, state);
            if (++frames > 2000) throw new InvalidOperationException("the route did not finish within 2,000 frames");
        } while (!state.hasResult || state.isSearching);
        Check(frames > 10 && (state.moveTarget - position).magnitude <= 1.001f,
            "the regression starts with a multi-frame search and one-edge smoothed first waypoint");
    }

    private static float MoveFrame(Corridor corridor, ref Vector3 position, Vector3 target,
        NavigationEndpointState state, float distance)
    {
        float remaining = distance;
        for (int calls = 0; calls < 16 && remaining > .00001f; calls++)
        {
            Vector3 direction = Query(corridor, position, target, state);
            Vector3 delta = state.moveTarget - position;
            delta.y = 0;
            if (direction.sqrMagnitude <= .00000001f || delta.magnitude <= .000001f) break;
            float step = Mathf.Min(remaining, delta.magnitude);
            position += direction * step;
            remaining -= step;
        }
        return distance - remaining;
    }

    private static void ContinuousMovementAtSix()
    {
        var corridor = new Corridor();
        var state = new NavigationEndpointState();
        Vector3 position = corridor.start;
        Warm(corridor, position, corridor.target, state);
        float travelled = 0;
        for (int frame = 0; frame < 180; frame++)
        {
            corridor.bfs.Advance();
            float movement = MoveFrame(corridor, ref position, corridor.target, state, 6f / 60f);
            travelled += movement;
            Check(Mathf.Abs(movement - .1f) < .0001f,
                "a warm speed-six/60-FPS route cannot pause at an intermediate waypoint (frame " + frame + ")");
            if (state.isSearching) pendingMovementFrames++;
        }
        Check(Mathf.Abs(travelled - 18f) < .001f && pendingMovementFrames > 100,
            "continuous movement crosses several corners while background rounds remain pending");
    }

    private static void ConsumePublishedRouteWhileSearchIsSuspended()
    {
        var corridor = new Corridor();
        var state = new NavigationEndpointState();
        Warm(corridor, corridor.start, corridor.target, state);
        corridor.bfs.Advance();
        Vector3 current = corridor.start;
        Query(corridor, current, corridor.target, state);
        Check(state.isSearching, "a fresh background round is suspended before completion");
        int work = state.searchedCellsThisFrame, completed = state.completedSearchCount;
        for (int edge = 0; edge < 24; edge++)
        {
            Vector3 direction = Query(corridor, current, corridor.target, state);
            Check(direction.sqrMagnitude > .5f && (state.moveTarget - current).magnitude <= 1.001f,
                "the independent published path can supply another adjacent waypoint during suspension");
            current = state.moveTarget;
        }
        Check(state.searchedCellsThisFrame == work && state.completedSearchCount == completed
            && Vector3.Distance(current, corridor.start) > 1f,
            "consuming cached waypoints neither clears the published route nor refills search work in one frame");
    }

    private static void TwoCharactersDoNotSharePublishedRoutes()
    {
        var corridor = new Corridor();
        var first = new NavigationEndpointState();
        var second = new NavigationEndpointState();
        Warm(corridor, corridor.start, corridor.target, first);
        Warm(corridor, corridor.target, corridor.start, second);
        corridor.bfs.Advance();
        Query(corridor, corridor.start, corridor.target, first);
        Query(corridor, corridor.target, corridor.start, second);
        Check(first.isSearching && second.isSearching, "characters own concurrent background rounds in one shared BFS");
        Vector3 secondWaypoint = second.moveTarget;
        Vector3 current = corridor.start;
        for (int edge = 0; edge < 12; edge++)
        {
            Check(Query(corridor, current, corridor.target, first).sqrMagnitude > .5f,
                "advancing one character retains its own full route");
            current = first.moveTarget;
            Query(corridor, corridor.target, corridor.start, second);
            Check(second.moveTarget == secondWaypoint,
                "another character cannot consume or overwrite this character's published waypoints");
        }
    }

    private static void MovingTargetRetainsSafeRoute()
    {
        var corridor = new Corridor();
        var state = new NavigationEndpointState();
        Warm(corridor, corridor.start, corridor.target, state);
        corridor.bfs.Advance();
        Vector3 nearerTarget = corridor.cells[64].realPos;
        Query(corridor, corridor.start, nearerTarget, state);
        Check(state.hasResult && state.isSearching && state.moveTarget == corridor.cells[1].realPos,
            "an ordinary target-cell change retains the safe route while a new captured round is pending");
        Vector3 current = corridor.start;
        for (int edge = 0; edge < 12; edge++)
        {
            Check(Query(corridor, current, nearerTarget, state).sqrMagnitude > .5f,
                "changing a destination does not discard all consumable safe route nodes");
            Check((state.moveTarget - current).magnitude <= 1.001f,
                "an old target-following shortcut cannot redirect straight across target cells");
            current = state.moveTarget;
        }
    }

    private enum Invalidation { Edge, BlockedCenter, PassType, RemovedNode }

    private static void RefuseInvalidNextEdge(Invalidation kind)
    {
        var corridor = new Corridor();
        var state = new NavigationEndpointState();
        Warm(corridor, corridor.start, corridor.target, state);
        corridor.bfs.Advance();
        Query(corridor, corridor.start, corridor.target, state);
        var first = corridor.cells[0]; var next = corridor.cells[1];
        switch (kind)
        {
            case Invalidation.Edge: first.links.Remove(next); next.links.Remove(first); break;
            case Invalidation.BlockedCenter: next.objectBlocked = true; break;
            case Invalidation.PassType: next.passTypes.Add(7401); break;
            case Invalidation.RemovedNode: corridor.navigation.navUnits.Remove((next.pos.x, next.pos.y, next.pos.z)); break;
        }
        Vector3 direction = Query(corridor, corridor.start, corridor.target, state);
        Check(direction == Vector3.zero && state.moveTarget == corridor.start,
            "a cached route never crosses an invalid next edge: " + kind);
    }

    private static void ResetAndCapabilityChangesDiscardRoutes()
    {
        var corridor = new Corridor();
        var state = new NavigationEndpointState();
        Warm(corridor, corridor.start, corridor.target, state);
        int work = state.searchedCellsThisFrame;
        state.Reset();
        Check(!state.hasResult && !state.isSearching && !state.directEndpoint,
            "stop/teleport lifecycle Reset clears pending and published navigation state");
        Check(Query(corridor, corridor.start, corridor.target, state) == Vector3.zero
            && state.searchedCellsThisFrame == work, "Reset cannot reuse an old route or refill the same-frame allowance");
        Warm(corridor, corridor.start, corridor.target, state);
        Check(Query(corridor, corridor.start, corridor.target, state, .41f) == Vector3.zero && !state.hasResult,
            "a changed physical radius discards an old clearance route");
        Warm(corridor, corridor.start, corridor.target, state);
        Check(Query(corridor, corridor.start, corridor.target, state, .4f, new[] { 7401 }) == Vector3.zero
            && !state.hasResult, "changed pass capabilities discard the old route");
    }

    private static void CompleteFinalCoordinate()
    {
        var corridor = new Corridor();
        var state = new NavigationEndpointState();
        Vector3 position = corridor.start, target = corridor.target + Vector3.left * .2f;
        Warm(corridor, position, target, state);
        float travelled = 0;
        for (int frame = 0; frame < 1600 && (position - target).sqrMagnitude > .00000001f; frame++)
        {
            corridor.bfs.Advance();
            float movement = MoveFrame(corridor, ref position, target, state, 6f / 60f);
            travelled += movement;
            Check(movement > .00001f, "the cached route reaches its final non-center destination without intermediate waits");
        }
        Check((position - target).sqrMagnitude < .00000001f && Mathf.Abs(travelled - 118.2f) < .03f,
            "the complete path retains its exact final coordinate and does not cut corners: position="
            + position.ToString("F6") + " target=" + target.ToString("F6") + " travelled=" + travelled.ToString("F6"));
        corridor.bfs.Advance();
        Check(Query(corridor, position, target, state) == Vector3.zero,
            "arriving at the exact final coordinate remains stationary rather than revisiting consumed waypoints");
    }

    private static void RealCharacterSpendsRemainingMovement()
    {
        // Move is intentionally nonvirtual. Exercise the real Character Move /
        // ApplyMove chain, with an actual registered Sphere prefab and no Tiles
        // or obstacles; this keeps collision and Trigger concerns out of this
        // focused remaining-distance test instead of overriding game movement.
        var poolGo = new GameObject("route-cache-pool"); owned.Add(poolGo); poolGo.SetActive(false);
        pool = poolGo.AddComponent<InstancePoolManager>();
        typeof(Z_MonoSingleton<InstancePoolManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, pool);
        pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal);
        pool.defaultRoot = poolGo.transform;
        var prefab = new GameObject("route-cache-character"); owned.Add(prefab); prefab.SetActive(false);
        var body = prefab.AddComponent<SphereCollider>();
        body.radius = .4f; body.center = Vector3.up * .4f;
        pool.AddPool(prefab);

        var corridor = new Corridor();
        var previousNavigation = map.navigationCtrl;
        var previousUpdate = map.updateCtrl;
        bool oldPause = DynamicGlobalSettings.pauseNav, oldDrop = DynamicGlobalSettings.allowDrop;
        bool oldEnabled = map.enable;
        try
        {
            map.enable = false; // Do not clamp a synthetic query graph to absent Tile data.
            DynamicGlobalSettings.pauseNav = false;
            DynamicGlobalSettings.allowDrop = true;
            map.navigationCtrl = corridor.navigation;
            map.updateCtrl = new MapUpdateController(map);
            typeof(NavigationController).GetField("bfs", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(corridor.navigation, corridor.bfs);
            var data = new CharacterUnitForm.Data(98522, true, corridor.target, 6, 999, 999, true,
                "", prefab.name, corridor.start, Vector3.zero, Vector3.one, UpdateType.Always,
                new List<int>(), "", false, 0, new List<int>());
            var character = data.unit;
            var endpoint = (NavigationEndpointState)typeof(CharacterUnit)
                .GetField("navigationEndpointState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(character);
            var move = typeof(CharacterUnit).GetMethod("MoveAlongNavigation", BindingFlags.Instance | BindingFlags.NonPublic);
            Check(move != null, "the real Character remaining-distance entry point exists");
            Warm(corridor, data.pos, data.destination, endpoint);
            corridor.bfs.Advance();
            Vector3 direction = Query(corridor, data.pos, data.destination, endpoint);
            int work = endpoint.searchedCellsThisFrame, completed = endpoint.completedSearchCount;
            var arguments = new object[] { 2.4f, direction };
            Check((bool)move.Invoke(character, arguments), "the actual Character reports movement after crossing multiple cached waypoints");
            Check((data.pos - (corridor.start + Vector3.right * 2.4f)).sqrMagnitude < .000001f,
                "one actual Character navigation call spends all distance instead of stopping at its first waypoint: " + data.pos);
            Check(endpoint.searchedCellsThisFrame == work && work <= 10
                && endpoint.completedSearchCount == completed && endpoint.isSearching,
                "real Character substeps reuse one frame's search budget while its new round remains pending");

            arguments = new object[] { 8.2f, (Vector3)arguments[1] };
            Check((bool)move.Invoke(character, arguments), "the actual Character can continue through consecutive corners");
            Vector3 expected = corridor.start + Vector3.right * 7.4f + Vector3.forward * 2f;
            Check((data.pos - expected).sqrMagnitude < .000001f,
                "real remaining-distance movement follows turn centers and uses its whole arc length: position="
                + data.pos.ToString("F6") + " expected=" + expected.ToString("F6"));
            Check(endpoint.searchedCellsThisFrame == work && endpoint.completedSearchCount == completed,
                "many real Move calls and route advances in one frame never replenish search work");

            var endpointPermission = typeof(CharacterUnit).GetProperty("isNavEndpointMove", BindingFlags.Instance | BindingFlags.NonPublic);
            typeof(CharacterUnit).GetMethod("MoveForNavigation", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(character, new object[] { Vector3.zero, true });
            Check(!(bool)endpointPermission.GetValue(character),
                "the actual endpoint Move scope returns its special movement permission");
            map.updateCtrl.ApplyMove(character, data.pos, data.euler, true);
            Check(!endpoint.hasResult && !endpoint.isSearching && !endpoint.directEndpoint,
                "an actual teleport ApplyMove discards the full cached route and endpoint phase");
        }
        finally
        {
            map.navigationCtrl = previousNavigation;
            map.updateCtrl = previousUpdate;
            map.enable = oldEnabled;
            DynamicGlobalSettings.pauseNav = oldPause;
            DynamicGlobalSettings.allowDrop = oldDrop;
        }
    }

    private static NavigationController EndpointGraph(out ControlledBfs bfs)
    {
        var navigation = new NavigationController(map)
        {
            navUnits = new Dictionary<(int, int, int), NavUnit>(), step = .5f
        };
        for (int x = 1; x <= 7; x++)
        for (int z = 1; z <= 5; z++)
            navigation.navUnits[(x, 0, z)] = new NavUnit
            {
                pos = new Vector3Int(x, 0, z), realPos = new Vector3(x, 0, z),
                links = new List<NavUnit>(), passTypes = new HashSet<int>(),
                dirMaxY = new float[4], dirGroundY = new float[4]
            };
        foreach (var node in navigation.navUnits.Values)
        foreach (var offset in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back })
            if (navigation.navUnits.TryGetValue((node.pos.x + offset.x, 0, node.pos.z + offset.z), out var neighbour))
                node.links.Add(neighbour);
        bfs = new ControlledBfs(navigation);
        return navigation;
    }

    private static void BlockEndpoint(NavigationController navigation, NavUnit node)
    {
        node.objectBlocked = true;
        node.links.Clear();
        foreach (var other in navigation.navUnits.Values) other.links.Remove(node);
    }

    private static Vector3 EndpointStep(ControlledBfs bfs, Vector3 position, Vector3 target,
        NavigationEndpointState state, int maxStep = 100, float radius = .5f, IReadOnlyCollection<int> types = null)
    {
        Vector3 direction = bfs.GetNextDir(position, target, maxStep, radius, types ?? NoTypes, state);
        Check(state.searchedCellsThisFrame <= 10, "all endpoint phases share the same ten-cell frame allowance");
        return direction;
    }

    private static Vector3 AwaitEndpointRound(ControlledBfs bfs, Vector3 position, Vector3 target,
        NavigationEndpointState state, int maxStep = 100, float radius = .5f, IReadOnlyCollection<int> types = null)
    {
        int previous = state.completedSearchCount;
        for (int frame = 0; frame < 2000; frame++)
        {
            bfs.Advance();
            Vector3 direction = EndpointStep(bfs, position, target, state, maxStep, radius, types);
            if (state.hasResult && state.completedSearchCount > previous) return direction;
        }
        throw new InvalidOperationException("an endpoint round did not publish within 2,000 budgeted frames");
    }

    private static void EndpointPhaseSemantics()
    {
        var navigation = EndpointGraph(out var bfs);
        var start = navigation.navUnits[(2, 0, 3)];
        var blockedTarget = navigation.navUnits[(6, 0, 3)];
        BlockEndpoint(navigation, start); BlockEndpoint(navigation, blockedTarget);
        var state = new NavigationEndpointState();
        Vector3 position = start.realPos + Vector3.right * .1f;
        Vector3 target = blockedTarget.realPos + Vector3.right * .1f;

        bfs.Advance();
        Vector3 direction = EndpointStep(bfs, position, target, state);
        Check(state.isSearching && !state.hasResult && !state.directEndpoint && direction == Vector3.zero,
            "a pending nearest-legal-start scan cannot invent an escape or final direct leg");
        direction = AwaitEndpointRound(bfs, position, target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(3, 0, 3) && direction.x > .999f,
            "a blocked start escapes to its nearest legal center, not toward the destination");
        AwaitEndpointRound(bfs, new Vector3(2.65f, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(3, 0, 3),
            "entering the legal anchor's cell does not skip its center during escape");
        bool graph = false, final = false;
        for (int frame = 0; frame < 100 && (position - target).sqrMagnitude > .00000001f; frame++)
        {
            direction = AwaitEndpointRound(bfs, position, target, state);
            if (!state.directEndpoint) graph = true;
            if (state.directEndpoint && state.moveTarget == target) final = true;
            position += direction * Mathf.Min(.15f, (state.moveTarget - position).magnitude);
        }
        Check(graph && final && (position - target).sqrMagnitude < .00000001f,
            "two blocked endpoints retain escape, cached graph movement and the proven final direct segment: " + position);
        direction = AwaitEndpointRound(bfs, new Vector3(5.65f, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == target && direction.x > .99f,
            "entering a blocked target never reverses its already-proven final approach");
        Check(AwaitEndpointRound(bfs, target, target, state) == Vector3.zero,
            "a final direct leg stops at its original exact coordinate");
        Vector3 movedTarget = target + Vector3.forward * .1f;
        direction = AwaitEndpointRound(bfs, new Vector3(5.65f, 0, 3), movedTarget, state,
            radius: .500005f);
        Check(state.directEndpoint && state.moveTarget == movedTarget
            && Vector3.Dot(direction, (movedTarget - new Vector3(5.65f, 0, 3)).normalized) > .999f,
            "same-blocked-cell target movement and radius tolerance preserve the final phase");

        var other = new NavigationEndpointState();
        AwaitEndpointRound(bfs, start.realPos + Vector3.right * .1f, new Vector3(1, 0, 1), other);
        Check(other.directEndpoint && other.moveTarget == new Vector3(3, 0, 3),
            "a second Character has its own escape anchor");
        AwaitEndpointRound(bfs, new Vector3(5.65f, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == target,
            "another Character's escape cannot replace this Character's final phase");
        Vector3 changedDestination = new Vector3(7, 0, 5);
        AwaitEndpointRound(bfs, blockedTarget.realPos, changedDestination, state);
        Check(state.directEndpoint && state.moveTarget != changedDestination && state.moveTarget != target,
            "a different target cell cancels the old final leg and rechecks its blocked start");

        state.Reset();
        AwaitEndpointRound(bfs, start.realPos + Vector3.right * .1f, target, state, radius: 1f);
        Check(state.directEndpoint && state.moveTarget == new Vector3(4, 0, 3),
            "a large Character escapes only to an anchor with a clear physical footprint");
        var restrictedStart = navigation.navUnits[(3, 0, 3)];
        restrictedStart.passTypes.Add(7201);
        state.Reset();
        AwaitEndpointRound(bfs, restrictedStart.realPos + Vector3.right * .1f, changedDestination, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(4, 0, 3),
            "a pass-restricted start escapes to a legal center");
        state.Reset();
        AwaitEndpointRound(bfs, restrictedStart.realPos, restrictedStart.realPos + Vector3.right * .1f,
            state, types: new[] { 7201 });
        Check(!state.directEndpoint && state.moveTarget == restrictedStart.realPos + Vector3.right * .1f,
            "a capable Character uses normal same-cell movement instead of endpoint permission");
        restrictedStart.passTypes.Clear();
        var restrictedTarget = navigation.navUnits[(7, 0, 5)];
        restrictedTarget.passTypes.Add(7202);
        state.Reset();
        Vector3 restrictedPosition = restrictedTarget.realPos + Vector3.right * .1f;
        AwaitEndpointRound(bfs, new Vector3(7, 0, 4), restrictedPosition, state);
        Check(state.directEndpoint && state.moveTarget == restrictedPosition,
            "a pass-restricted target approaches from its nearest reachable legal center");
        direction = AwaitEndpointRound(bfs, new Vector3(7, 0, 4.65f), restrictedPosition, state);
        Check(state.directEndpoint && direction.z > 0,
            "entering a pass-restricted target preserves final-approach direction");
        restrictedTarget.passTypes.Clear();

        state.Reset();
        AwaitEndpointRound(bfs, new Vector3(0, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(1, 0, 3),
            "a missing start escapes to the nearest actual node");
        state.Reset();
        AwaitEndpointRound(bfs, new Vector3(7, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == target,
            "a blocked target starts its final leg from the closest reachable center");
        state.Reset();
        Vector3 limitedStart = new Vector3(3, 0, 2);
        AwaitEndpointRound(bfs, limitedStart, target, state, maxStep: 0);
        Check(!state.directEndpoint && state.moveTarget == limitedStart,
            "an exhausted graph depth never mistakes a truncated route for a final anchor");

        // The closest geometric node is deliberately unreachable. It must not
        // replace a farther but graph-reachable endpoint anchor.
        var isolated = new NavUnit
        {
            pos = new Vector3Int(6, 0, 4), realPos = target - Vector3.forward * .05f,
            links = new List<NavUnit>(), passTypes = new HashSet<int>(),
            dirMaxY = new float[4], dirGroundY = new float[4]
        };
        isolated.links.Add(isolated); navigation.navUnits[(6, 0, 4)] = isolated;
        state.Reset();
        AwaitEndpointRound(bfs, new Vector3(7, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == target,
            "a nearer isolated node cannot become the blocked destination's anchor");
        state.Reset();
        Vector3 missingTarget = new Vector3(8, 0, 3), ordinaryStart = new Vector3(4, 0, 3);
        AwaitEndpointRound(bfs, ordinaryStart, missingTarget, state);
        Check(!state.directEndpoint && state.moveTarget != ordinaryStart,
            "a missing destination consumes a graph route before allowing its final segment");
        var nullStart = navigation.navUnits[(2, 0, 1)]; nullStart.isNull = true;
        state.Reset();
        AwaitEndpointRound(bfs, nullStart.realPos, target, state);
        Check(state.directEndpoint && state.moveTarget != nullStart.realPos,
            "a null/zero-scale start without a floor safely searches for an escape anchor");

        // Change the target while its old blocked-destination round is still
        // pending. Completing that round cannot publish the obsolete direct leg.
        state.Reset();
        Vector3 anchorPosition = new Vector3(7, 0, 3);
        bfs.Advance();
        EndpointStep(bfs, anchorPosition, target, state);
        Check(state.isSearching && !state.hasResult && !state.directEndpoint,
            "a pending closest-reachable-target scan cannot grant a final segment early");
        int before = state.completedSearchCount;
        Vector3 freshTarget = new Vector3(1, 0, 1);
        EndpointStep(bfs, anchorPosition, freshTarget, state);
        for (int frame = 0; frame < 100 && state.completedSearchCount == before; frame++)
        {
            bfs.Advance(); EndpointStep(bfs, anchorPosition, freshTarget, state);
        }
        Check(state.completedSearchCount > before && !state.directEndpoint && state.moveTarget != target,
            "an obsolete captured blocked-target round cannot publish its old direct approach");
        AwaitEndpointRound(bfs, anchorPosition, freshTarget, state);
        Check(state.hasResult && !state.directEndpoint && state.moveTarget != target,
            "the next completed round routes to the latest destination normally");

        navigation.navUnits.Clear(); state.Reset(); bfs.Advance();
        Check(EndpointStep(bfs, Vector3.zero, Vector3.one, state) == Vector3.zero && !state.hasResult,
            "an empty navigation graph safely discards endpoint and cached path state");
    }

    private static void SmoothedPassTypesAndRetargeting()
    {
        var navigation = EndpointGraph(out var bfs);
        var state = new NavigationEndpointState();
        Vector3 start = new Vector3(2, 0, 2), target = new Vector3(4, 0, 4);
        navigation.navUnits[(3, 0, 2)].passTypes.Add(7301);
        Vector3 direction = AwaitEndpointRound(bfs, start, target, state);
        Check(direction.x > .6f && direction.z > .6f && !state.directEndpoint,
            "a restricted side neighbour does not veto a legal smoothed diagonal center line");
        direction = EndpointStep(bfs, new Vector3(2.4f, 0, 2.4f), target, state);
        Check(state.hasResult && direction.x > .6f && direction.z > .6f,
            "crossing a diagonal corner does not require the adjacent side cell's pass type");

        navigation = EndpointGraph(out bfs); state = new NavigationEndpointState();
        navigation.navUnits[(2, 0, 3)].passTypes.Add(7302);
        // The integer cell-center diagonal is legal, but this actual target's
        // offset would send its ray through restricted (2,3) before (3,3).
        target = new Vector3(3.6f, 0, 4.4f);
        AwaitEndpointRound(bfs, start, target, state);
        Check(state.moveTarget == target, "the offset-target regression begins with a complete smoothed first leg");
        Vector3 position = new Vector3(2.3f, 0, 2.45f);
        int completed = state.completedSearchCount, work = state.searchedCellsThisFrame;
        direction = EndpointStep(bfs, position, target, state);
        Check(state.hasResult && direction.sqrMagnitude > .5f && state.moveTarget != target
            && state.completedSearchCount == completed && state.searchedCellsThisFrame == work,
            "an invalid offset shortcut immediately falls back to its cached raw path without waiting for another round");
        for (int frame = 0; frame < 120 && (position - target).sqrMagnitude > .00000001f; frame++)
        {
            bfs.Advance();
            direction = EndpointStep(bfs, position, target, state);
            Vector3 next = position + direction * Mathf.Min(.1f, (state.moveTarget - position).magnitude);
            Check((next - position).sqrMagnitude > .0000000001f,
                "the raw fallback cannot enter a repeat-search/wait loop before its valid offset target");
            bool enteredRestricted = false;
            for (int sample = 1; sample <= 4; sample++)
            {
                Vector3 point = Vector3.Lerp(position, next, sample / 4f);
                enteredRestricted |= point.x > 1.5001f && point.x < 2.4999f
                    && point.z > 2.5001f && point.z < 3.4999f;
            }
            Check(!enteredRestricted, "movement toward an offset target must not enter the restricted neighboring cell");
            position = next;
        }
        Check((position - target).sqrMagnitude < .00000001f,
            "cached raw fallback reaches the exact offset target instead of endlessly rebuilding the rejected shortcut: " + position);

        navigation = EndpointGraph(out bfs); state = new NavigationEndpointState();
        position = new Vector3(3, 0, 3); target = new Vector3(6, 0, 3);
        AwaitEndpointRound(bfs, position, target, state);
        Check(state.moveTarget == target, "the retarget regression begins with a distant smoothed leg");
        position += Vector3.right * .25f;
        Vector3 changedTarget = new Vector3(2, 0, 4);
        direction = AwaitEndpointRound(bfs, position, changedTarget, state);
        Check(state.moveTarget == changedTarget && Vector3.Dot(direction, (changedTarget - position).normalized) > .999f,
            "a new target direction can replace an unfinished far smoothed leg rather than being rejected by the turn guard");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
