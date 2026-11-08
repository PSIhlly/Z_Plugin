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
            RotatingCharacterKeepsNavigationRadius();
            LargeCharacterStartup();
            ColumnIndexLifecycle();
            EvenSizedCornerNavigation();
            TargetRingSelection();
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

    private static void RotatingCharacterKeepsNavigationRadius()
    {
        var poolGo = new GameObject("rotating-character-pool"); owned.Add(poolGo); poolGo.SetActive(false);
        pool = poolGo.AddComponent<InstancePoolManager>();
        typeof(Z_MonoSingleton<InstancePoolManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, pool);
        pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal); pool.defaultRoot = poolGo.transform;
        var prefab = new GameObject("rotating-character"); owned.Add(prefab); prefab.SetActive(false);
        var body = prefab.AddComponent<SphereCollider>(); body.radius = .4f; body.center = Vector3.up * .4f;
        var trigger = prefab.AddComponent<SphereCollider>(); trigger.radius = 10f; trigger.isTrigger = true;
        pool.AddPool(prefab);
        var data = new CharacterUnitForm.Data(98701, true, Vector3.zero, 6, 999, 999, true, "", prefab.name,
            Vector3.zero, Vector3.zero, Vector3.one * 2, UpdateType.Always, new List<int>(), "", false, 0, new List<int>());
        var character = data.unit;
        var radiusMethod = typeof(CharacterUnit).GetMethod("GetNavigationRadius", BindingFlags.Instance | BindingFlags.NonPublic);
        float Radius() => (float)radiusMethod.Invoke(character, null);
        foreach (int size in new[] { 1, 2, 3, 4 })
        foreach (var position in new[] { Vector3.zero, new Vector3(507.1035f, 498.8025f, 417.5f) })
        foreach (float yaw in new[] { 0f, 45f, 90f, 96.621f, 135f, 180f, 225f, 270f, 315f })
        {
            data.scale = Vector3.one * size; data.pos = position; data.euler = Vector3.up * yaw;
            Check(Mathf.Abs(Radius() - size * .4f) < .0001f,
                "a physical Sphere keeps its radius when turning or moving, excluding its Trigger: size="
                + size + " yaw=" + yaw + " pos=" + position + " radius=" + Radius());
        }

        var previousNavigation = map.navigationCtrl; var previousUpdate = map.updateCtrl;
        bool oldEnabled = map.enable, oldPause = DynamicGlobalSettings.pauseNav, oldDrop = DynamicGlobalSettings.allowDrop;
        try
        {
            map.enable = false; DynamicGlobalSettings.pauseNav = false; DynamicGlobalSettings.allowDrop = true;
            var navigation = new NavigationController(map) { step = .5f,
                navUnits = new Dictionary<(int, int, int), NavUnit>() };
            for (int x = 500; x < 540; x++)
            for (int z = 414; z < 422; z++)
            {
                var p = new Vector3Int(x, 499, z);
                navigation.navUnits[(x, 499, z)] = new NavUnit { pos = p, realPos = p,
                    links = new List<NavUnit>(), passTypes = new HashSet<int>(), dirMaxY = new float[4], dirGroundY = new float[4] };
            }
            foreach (var node in navigation.navUnits.Values)
            foreach (var delta in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back })
                if (navigation.navUnits.TryGetValue((node.pos.x + delta.x, 499, node.pos.z + delta.z), out var next))
                    node.links.Add(next);
            var bfs = new ControlledBfs(navigation);
            typeof(NavigationController).GetField("bfs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(navigation, bfs);
            map.navigationCtrl = navigation; map.updateCtrl = new MapUpdateController(map);
            var state = (NavigationEndpointState)typeof(CharacterUnit)
                .GetField("navigationEndpointState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(character);
            var move = typeof(CharacterUnit).GetMethod("MoveAlongNavigation", BindingFlags.Instance | BindingFlags.NonPublic);
            foreach (float distance in new[] { 3f, 18f })
            {
                data.scale = Vector3.one * 2; data.pos = new Vector3(505.5f, 499, 416.5f);
                data.destination = data.pos + Vector3.right * distance; data.euler = Vector3.up * 90f;
                state.Reset();
                for (int frame = 0; frame < 1500 && !state.hasResult; frame++)
                {
                    bfs.Advance();
                    map.updateCtrl.GetNavDir(data.pos, data.destination, 999, Radius(), NoTypes, state, 2);
                }
                Check(state.hasResult && !state.directEndpoint, "the rotating Character starts with a completed regular route");
                Vector3 start = data.pos;
                for (int frame = 1; frame <= 240; frame++)
                {
                    if (frame % 30 == 0) data.destination += Vector3.right;
                    // Replay the observed command-facing versus movement-facing changes.
                    data.euler = Vector3.up * (frame % 30 < 15 ? 96.621f : 90f);
                    bfs.Advance();
                    var direction = map.updateCtrl.GetNavDir(data.pos, data.destination, 999, Radius(), NoTypes, state, 2);
                    Check(state.hasResult && !state.directEndpoint && direction.x >= -.0001f,
                        "turning and retargeting every 30 frames retain the route without backward corner alignment: frame="
                        + frame + " distance=" + distance + " pos=" + data.pos + " target=" + state.moveTarget);
                    Vector3 before = data.pos;
                    move.Invoke(character, new object[] { .06f, direction });
                    Check(data.pos.x >= before.x - .0001f && Mathf.Abs(data.pos.z - start.z) < .0001f,
                        "the real size=2 Character moves forward without twitching in open terrain");
                    Check(state.searchedCellsThisFrame <= Bfs.CellsPerFrame,
                        "rotation and periodic target changes do not refill the search budget");
                }
                Check(data.pos.x - start.x > 10f, "near and distant pursuit both make sustained progress while turning");
            }
        }
        finally
        {
            map.navigationCtrl = previousNavigation; map.updateCtrl = previousUpdate; map.enable = oldEnabled;
            DynamicGlobalSettings.pauseNav = oldPause; DynamicGlobalSettings.allowDrop = oldDrop;
        }
    }

    private static void LargeCharacterStartup()
    {
        var navigation = new NavigationController(map) { step = .5f,
            navUnits = new Dictionary<(int, int, int), NavUnit>() };
        for (int x = 0; x < 40; x++)
        for (int z = 0; z < 40; z++)
            navigation.navUnits[(x, 0, z)] = new NavUnit { pos = new Vector3Int(x, 0, z),
                realPos = new Vector3(x, 0, z), links = new List<NavUnit>(), passTypes = new HashSet<int>(),
                dirGroundY = new float[4], dirMaxY = new float[4] };
        foreach (var node in navigation.navUnits.Values)
        foreach (var offset in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back })
            if (navigation.navUnits.TryGetValue((node.pos.x + offset.x, 0, node.pos.z + offset.z), out var next))
                node.links.Add(next);
        var bfs = new ControlledBfs(navigation);
        int Await(Vector3 start, Vector3 target, float radius, out NavigationEndpointState state, int maxStep = 999)
        {
            state = new NavigationEndpointState();
            for (int frame = 1; frame < 3000; frame++)
            {
                bfs.Advance();
                bfs.GetNextDir(start, target, maxStep, radius, NoTypes, state);
                Check(state.searchedCellsThisFrame <= Bfs.CellsPerFrame, "large-character startup retains the frame budget");
                if (state.hasResult) return frame;
            }
            throw new InvalidOperationException("large-character startup did not finish");
        }
        var origin = new Vector3(12, 0, 12);
        int ordinary = Await(origin, new Vector3(24, 0, 12), .4f, out _);
        int large = Await(origin, new Vector3(24, 0, 12), .8f, out _);
        var target = new Vector3(0, 0, 12);
        int edge = Await(origin, target, .8f, out var reference);
        Vector3 expectedAnchor = reference.moveTarget;
        int escape = Await(target, origin, .8f, out _);
        int reentered = Await(origin, target, .8f, out _);
        var previousColumns = map.data.mapXZ2Y;
        map.data.mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>();
        foreach (var node in navigation.navUnits.Values)
            map.data.mapXZ2Y[(node.pos.x, node.pos.z)] = new SortedSet<int> { 0 };
        void Index(bool regular = true)
        {
            typeof(NavigationController).GetField("columnIndexMin", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(navigation, Vector2Int.zero);
            typeof(NavigationController).GetField("columnIndexMax", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(navigation, new Vector2Int(39, 39));
            typeof(NavigationController).GetMethod("PublishColumnIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(navigation, new object[] { regular });
        }
        try
        {
            Index();
            int fastEdge = Await(origin, target, .8f, out var optimized);
            Check(fastEdge <= ordinary + 3 && fastEdge < edge / 2 && optimized.moveTarget == expectedAnchor,
                "size=2 edge targeting reaches the same anchor without exhausting the entire open graph");
            int fastEscape = Await(target, origin, .8f, out optimized);
            Check(fastEscape <= 2 && optimized.directEndpoint && optimized.moveTarget == new Vector3(1, 0, 12),
                "nearest size=2 escape uses cached height columns instead of scanning all 1600 cells");
            optimized.Reset();
            Check(!optimized.hasResult && !optimized.isSearching, "explicit navigation stop still clears the route");
            Index(); // Publishing an unchanged periodic refresh preserves the spatial index.
            int fastReentered = Await(origin, target, .8f, out optimized);
            Check(fastReentered == fastEdge && optimized.moveTarget == expectedAnchor,
                "a fresh query after stop/reentry reuses the map index, without retaining its old route");
            Await(origin, target, .8f, out optimized, maxStep: 0);
            Check(!optimized.directEndpoint && optimized.moveTarget == origin,
                "nearest geometric candidate never bypasses the graph-depth limit");

            var nearest = navigation.navUnits[(1, 0, 12)];
            nearest.links.Clear();
            foreach (var node in navigation.navUnits.Values) node.links.Remove(nearest);
            Index(false);
            Await(origin, target, .8f, out reference);
            Index();
            Await(origin, target, .8f, out optimized);
            Check(optimized.moveTarget == reference.moveTarget && optimized.directEndpoint == reference.directEndpoint,
                "an unreachable nearest candidate falls back to the same graph-reachable anchor as exhaustive BFS");

            nearest.objectBlocked = true;
            Index(false); Await(origin, target, .8f, out reference);
            Index(); Await(origin, target, .8f, out optimized);
            Check(optimized.moveTarget == reference.moveTarget,
                "live obstacle changes are reflected even though the cached column geometry is unchanged");
            Debug.Log($"LARGE_CHARACTER_NAV_STARTUP frames ordinary={ordinary} size2={large} edgeTarget={edge}->{fastEdge} blockedStart={escape}->{fastEscape} reentered={reentered}->{fastReentered}");
        }
        finally { map.data.mapXZ2Y = previousColumns; }
    }

    private static void ColumnIndexLifecycle()
    {
        var oldData = map.data; var oldUpdate = map.updateCtrl; var oldNavigation = map.navigationCtrl;
        bool oldEnabled = map.enable;
        var rows = new List<TileUnitForm.Data>();
        var poolGo = new GameObject("nav-column-index-pool"); owned.Add(poolGo); poolGo.SetActive(false);
        pool = poolGo.AddComponent<InstancePoolManager>();
        typeof(Z_MonoSingleton<InstancePoolManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, pool);
        pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal); pool.defaultRoot = poolGo.transform;
        var prefab = new GameObject("MapPrefab$nav-column-floor"); owned.Add(prefab); prefab.SetActive(false);
        prefab.AddComponent<TileInstance>();
        var box = prefab.AddComponent<BoxCollider>(); box.center = Vector3.down * .05f; box.size = new Vector3(1, .1f, 1);
        pool.AddPool(prefab);
        try
        {
            map.data = new MapInfo { mainData = new MapMainForm.Data(98600, Vector3.one,
                new Vector3Int(10, 3, 10), new Vector3Int(5, 3, 5), "", "", "", ""),
                maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
                mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>() };
            map.enable = true; map.updateCtrl = new MapUpdateController(map);
            UnitForm.InitInternal(); TileUnitForm.InitInternal(); ObjectUnitForm.InitInternal();
            for (int x = 0; x < 5; x++)
            for (int z = 0; z < 5; z++)
            {
                var pos = new Vector3Int(x, 0, z);
                var data = new TileUnitForm.Data(98601 + rows.Count, "floor", new Dictionary<int, int>(), pos,
                    prefab.name, pos, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
                TileUnitForm.AddData(data); rows.Add(data);
                map.data.maps[(x, 0, z)] = data; map.data.mapXZ2Y[(x, z)] = new SortedSet<int> { 0 };
            }
            var navigation = new NavigationController(map); map.navigationCtrl = navigation;
            var ready = typeof(NavigationController).GetProperty("CanSearchNearbyColumns", BindingFlags.Instance | BindingFlags.NonPublic);
            bool IsReady() => (bool)ready.GetValue(navigation);
            navigation.Build();
            Check(IsReady(), "real scene Build publishes the existing complete height-column index");
            var graph = navigation.navUnits; var center = graph[(2, 0, 2)];
            navigation.RebuildNow();
            Check(IsReady() && ReferenceEquals(graph, navigation.navUnits) && ReferenceEquals(center, graph[(2, 0, 2)]),
                "an unchanged periodic rebuild retains graph/node identities and usable column indexing");
            map.data.mainData.mapUnitSize = Vector3.one * 2;
            Check(!IsReady(), "changed map cell dimensions cannot reuse a stale spatial proof");
            map.data.mainData.mapUnitSize = Vector3.one;
            navigation.RefreshTerrainTiles(new[] { new Vector3Int(2, 0, 2) });
            Check(!IsReady(), "an explicit terrain refresh retires its previous column-index proof");
            navigation.RebuildNow(); Check(IsReady(), "completed terrain rebuilding restores the column index");
            rows[0].pos += Vector3.right * .1f;
            navigation.RebuildNow();
            Check(!IsReady(), "non-grid Tile centers fall back to exhaustive nearest search");
            rows[0].pos -= Vector3.right * .1f;
            navigation.RebuildNow(); Check(IsReady(), "restored regular terrain can use the index again");
        }
        finally
        {
            foreach (var row in rows) TileUnitForm.RemoveData(row.uid);
            map.data = oldData; map.updateCtrl = oldUpdate; map.navigationCtrl = oldNavigation; map.enable = oldEnabled;
        }
    }

    private static void TargetRingSelection()
    {
        var oldColumns = map.data.mapXZ2Y;
        NavigationController Grid(int extent, out ControlledBfs bfs)
        {
            var nav = new NavigationController(map) { step = .5f, navUnits = new Dictionary<(int, int, int), NavUnit>() };
            map.data.mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>();
            for (int x = 0; x <= extent; x++)
            for (int z = 0; z <= extent; z++)
            {
                nav.navUnits[(x, 0, z)] = new NavUnit { pos = new Vector3Int(x, 0, z), realPos = new Vector3(x, 0, z),
                    links = new List<NavUnit>(), passTypes = new HashSet<int>(), dirMaxY = new float[4], dirGroundY = new float[4] };
                map.data.mapXZ2Y[(x, z)] = new SortedSet<int> { 0 };
            }
            foreach (var node in nav.navUnits.Values)
            foreach (var dir in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back })
                if (nav.navUnits.TryGetValue((node.pos.x + dir.x, 0, node.pos.z + dir.z), out var next)) node.links.Add(next);
            typeof(NavigationController).GetField("columnIndexMin", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(nav, Vector2Int.zero);
            typeof(NavigationController).GetField("columnIndexMax", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(nav, new Vector2Int(extent, extent));
            typeof(NavigationController).GetMethod("PublishColumnIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(nav, new object[] { true });
            bfs = new ControlledBfs(nav); return nav;
        }
        try
        {
            var nav = Grid(4, out var bfs);
            foreach (var node in nav.navUnits.Values) node.objectBlocked = true;
            nav.navUnits[(3, 0, 3)].objectBlocked = false;
            nav.navUnits[(4, 0, 0)].objectBlocked = false;
            var state = new NavigationEndpointState();
            AwaitEndpointRound(bfs, new Vector3(4, 0, 0), Vector3.zero, state, radius: .4f);
            Check(SelectedTarget(state) == new Vector3(4, 0, 0) && state.moveTarget == new Vector3(4, 0, 0),
                "Euclidean rings select distance 4 before the square-ring-3 diagonal at sqrt(18)");

            nav = Grid(20, out bfs);
            var original = new Vector3(10, 0, 10);
            foreach (var node in nav.navUnits.Values) node.objectBlocked = (node.realPos - original).sqrMagnitude < 16f;
            state = new NavigationEndpointState(); bfs.Advance();
            EndpointStep(bfs, new Vector3(5, 0, 10), original, state, radius: .4f);
            var search = typeof(NavigationEndpointState).GetField("searchState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(state);
            var steps = (System.Collections.IDictionary)search.GetType().GetField("steps", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(search);
            Check(state.isSearching && !state.hasResult && steps.Count == 0,
                "BFS does not expand any node before budgeted target selection has finished");
            AwaitEndpointRound(bfs, new Vector3(5, 0, 10), original, state, radius: .4f);
            Check(Mathf.Abs((SelectedTarget(state) - original).sqrMagnitude - 16f) < .0001f,
                "target search expands beyond three cells and may choose any legal equal-distance point");
            var selected = SelectedTarget(state);
            Check(AwaitEndpointRound(bfs, selected, original, state, radius: .4f) == Vector3.zero && !state.directEndpoint,
                "the selected target remains the final endpoint even when the original destination stays blocked");
            state.Reset(); AwaitEndpointRound(bfs, new Vector3(5, 0, 10), original, state, radius: .8f);
            var large = nav.RealPos2MapPosInt(SelectedTarget(state));
            bool clearance = true;
            for (int x = -1; x <= 1; x++)
            for (int z = -1; z <= 1; z++)
                clearance &= nav.navUnits.TryGetValue((large.x + x, 0, large.z + z), out var footprint) && !footprint.objectBlocked;
            Check(clearance, "size=2 ring candidates include the entire physical footprint");

            nav = Grid(4, out bfs);
            foreach (var node in nav.navUnits.Values) node.objectBlocked = true;
            nav.navUnits[(3, 0, 2)].objectBlocked = false;
            var upper = new NavUnit { pos = new Vector3Int(2, 5, 2), realPos = new Vector3(2, 5, 2),
                links = new List<NavUnit>(), passTypes = new HashSet<int>() };
            nav.navUnits[(2, 5, 2)] = upper; map.data.mapXZ2Y[(2, 2)].Add(5);
            typeof(NavigationController).GetMethod("PublishColumnIndex", BindingFlags.Instance | BindingFlags.NonPublic).Invoke(nav, new object[] { true });
            state = new NavigationEndpointState();
            AwaitEndpointRound(bfs, new Vector3(3, 0, 2), new Vector3(2, 0, 2), state, radius: .4f);
            Check(SelectedTarget(state) == new Vector3(3, 0, 2),
                "full Euclidean distance chooses a nearby lower point before a high point in the same column");
        }
        finally { map.data.mapXZ2Y = oldColumns; }
    }

    private static void EvenSizedCornerNavigation()
    {
        NavigationController Graph(int width, bool horizontal, out ControlledBfs bfs)
        {
            var nav = new NavigationController(map) { step = .5f, navUnits = new Dictionary<(int, int, int), NavUnit>() };
            for (int x = 0; x < 6; x++)
            for (int z = 0; z < 13; z++)
            {
                if (z >= 4 && z <= 8 && (x < 2 || x >= 2 + width)) continue;
                var p = horizontal ? new Vector3Int(z, 0, x) : new Vector3Int(x, 0, z);
                nav.navUnits[(p.x, 0, p.z)] = new NavUnit { pos = p, realPos = p,
                    links = new List<NavUnit>(), passTypes = new HashSet<int>(), dirMaxY = new float[4], dirGroundY = new float[4] };
            }
            foreach (var node in nav.navUnits.Values)
            foreach (var d in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back })
                if (nav.navUnits.TryGetValue((node.pos.x + d.x, 0, node.pos.z + d.z), out var next)) node.links.Add(next);
            bfs = new ControlledBfs(nav); return nav;
        }
        Vector3 Await(ControlledBfs bfs, Vector3 position, Vector3 target, NavigationEndpointState state, int size)
        {
            int before = state.completedSearchCount;
            for (int frame = 0; frame < 1500; frame++)
            {
                bfs.Advance();
                var direction = bfs.GetNextDir(position, target, 999, size * .4f, NoTypes, state, size);
                Check(state.searchedCellsThisFrame <= 10, "corner anchoring retains the per-agent frame budget");
                if (state.hasResult && state.completedSearchCount > before) return direction;
            }
            throw new InvalidOperationException("corner route never completed");
        }
        var nav = Graph(2, false, out var search);
        var footprint = nav.GetClearanceOffsets(.8f, 2);
        Check(footprint.Count == 4 && footprint[0] == new Vector2Int(-1, 0)
            && footprint[3] == new Vector2Int(0, 1), "size=2 covers the 2x2 block around its lower-right anchor Tile");
        Check(nav.GetNavigationOffset(2) == new Vector3(-.5f, 0, .5f)
            && nav.GetNavigationOffset(3) == Vector3.zero && nav.GetClearanceOffsets(1.6f, 4).Count == 16,
            "even sizes use top-left corners, odd sizes keep centers, and size=4 retains four-cell clearance");
        foreach (bool horizontal in new[] { false, true })
        {
            nav = Graph(2, horizontal, out search);
            Vector3 start = horizontal ? new Vector3(1.5f, 0, 2.5f) : new Vector3(2.5f, 0, 1.5f);
            Vector3 target = horizontal ? new Vector3(10.5f, 0, 2.5f) : new Vector3(2.5f, 0, 10.5f);
            var state = new NavigationEndpointState(); Vector3 position = start;
            Await(search, position, target, state, 2);
            for (int frame = 0; frame < 500 && (position - target).sqrMagnitude > 1e-8f; frame++)
            {
                search.Advance();
                Vector3 direction = search.GetNextDir(position, target, 999, .8f, NoTypes, state, 2);
                position += direction * Mathf.Min(.15f, (state.moveTarget - position).magnitude);
                float side = horizontal ? position.z : position.x;
                Check(Mathf.Abs(side - 2.5f) < .0001f && side - .8f > 1.5f && side + .8f < 3.5f,
                    "the physical diameter fits both sides of the two-cell passage throughout navigation");
            }
            Check((position - target).sqrMagnitude < 1e-8f, "size=2 crosses a two-cell corridor in either orientation");
            Check(Await(search, position, target, state, 2) == Vector3.zero && state.moveTarget == target,
                "arrival remains at the anchor Tile's top-left corner");
        }
        nav = Graph(2, false, out search);
        var turning = new NavigationEndpointState(); var turnPosition = new Vector3(.5f, 0, 1.5f);
        var turnTarget = new Vector3(4.5f, 0, 10.5f);
        Await(search, turnPosition, turnTarget, turning, 2);
        for (int frame = 0; frame < 600 && (turnPosition - turnTarget).sqrMagnitude > 1e-8f; frame++)
        {
            search.Advance(); var direction = search.GetNextDir(turnPosition, turnTarget, 999, .8f, NoTypes, turning, 2);
            turnPosition += direction * Mathf.Min(.1f, (turning.moveTarget - turnPosition).magnitude);
            bool fits = true;
            for (int x = Mathf.FloorToInt(turnPosition.x - .8f + .50001f); x <= Mathf.FloorToInt(turnPosition.x + .8f + .49999f); x++)
            for (int z = Mathf.FloorToInt(turnPosition.z - .8f + .50001f); z <= Mathf.FloorToInt(turnPosition.z + .8f + .49999f); z++)
                fits &= nav.navUnits.ContainsKey((x, 0, z));
            Check(fits, "the full body stays inside the broad room and narrow entrance while turning: " + turnPosition);
        }
        Check((turnPosition - turnTarget).sqrMagnitude < 1e-8f, "an off-center room start steers into the half-cell corridor centerline");
        nav = Graph(4, false, out search);
        var four = new NavigationEndpointState(); var fourPosition = new Vector3(3.5f, 0, 1.5f);
        var fourTarget = new Vector3(3.5f, 0, 10.5f);
        Await(search, fourPosition, fourTarget, four, 4);
        for (int frame = 0; frame < 300 && (fourPosition - fourTarget).sqrMagnitude > 1e-8f; frame++)
        {
            search.Advance(); var direction = search.GetNextDir(fourPosition, fourTarget, 999, 1.6f, NoTypes, four, 4);
            fourPosition += direction * Mathf.Min(.15f, (four.moveTarget - fourPosition).magnitude);
        }
        Check((fourPosition - fourTarget).sqrMagnitude < 1e-8f, "size=4 uses the same corner convention with a four-cell-wide footprint");
        nav = Graph(1, false, out search);
        var blocked = new NavigationEndpointState(); var blockedStart = new Vector3(2.5f, 0, 1.5f);
        Await(search, blockedStart, new Vector3(2.5f, 0, 10.5f), blocked, 2);
        Check(blocked.moveTarget == blockedStart, "size=2 still rejects a one-cell-wide corridor");
        var small = new NavigationEndpointState(); Vector3 smallPosition = new Vector3(2, 0, 1), smallTarget = new Vector3(2, 0, 11);
        Await(search, smallPosition, smallTarget, small, 1);
        for (int frame = 0; frame < 500 && (smallPosition - smallTarget).sqrMagnitude > 1e-8f; frame++)
        {
            search.Advance(); var direction = search.GetNextDir(smallPosition, smallTarget, 999, .4f, NoTypes, small, 1);
            smallPosition += direction * Mathf.Min(.15f, (small.moveTarget - smallPosition).magnitude);
        }
        Check((smallPosition - smallTarget).sqrMagnitude < 1e-8f, "size=1 retains center navigation through a one-cell passage");
        nav = Graph(2, false, out search);
        var corner = new Vector3(2.5f, 0, 5.5f); var anchor = nav.navUnits[(3, 0, 5)];
        var same = new NavigationEndpointState(); Await(search, corner, anchor.realPos, same, 2);
        Check(same.moveTarget == corner && !same.directEndpoint, "a Tile-center target resolves to that Tile's top-left corner");
        anchor.passTypes.Add(7901);
        same.Reset(); Await(search, corner, corner, same, 2);
        Check(same.directEndpoint && same.moveTarget != corner, "the exact junction uses its lower-right Tile's pass requirement");
        same.Reset();
        for (int frame = 0; frame < 20 && !same.hasResult; frame++)
        { search.Advance(); search.GetNextDir(corner, corner, 999, .8f, new[] { 7901 }, same, 2); }
        Check(same.moveTarget == corner && !same.directEndpoint, "a capable even-sized actor stands at the same junction");
        anchor.passTypes.Clear();
        int previousWork = same.searchedCellsThisFrame;
        search.GetNextDir(corner, corner, 999, .8f, new[] { 7901 }, same, 1);
        Check(!same.hasResult && same.searchedCellsThisFrame == previousWork,
            "changing size/parity invalidates cached coordinates without replenishing the same-frame budget");
        Await(search, corner, corner, same, 2);
        previousWork = same.searchedCellsThisFrame;
        var cellSize = map.data.mainData.mapUnitSize;
        try
        {
            map.data.mainData.mapUnitSize = new Vector3(2, 1, 1);
            search.GetNextDir(corner, corner, 999, .8f, NoTypes, same, 2);
            Check(!same.hasResult && same.searchedCellsThisFrame == previousWork,
                "changed half-cell offset retires cached coordinates without granting more work");
        }
        finally { map.data.mainData.mapUnitSize = cellSize; }

        // Exercise real Character Move/ApplyMove and same-frame substeps with a
        // scaled physical Sphere; synthetic graph walls above verify clearance.
        var oldNav = map.navigationCtrl; var oldUpdate = map.updateCtrl; bool oldEnabled = map.enable;
        bool oldPause = DynamicGlobalSettings.pauseNav, oldDrop = DynamicGlobalSettings.allowDrop;
        var poolGo = new GameObject("corner-character-pool"); owned.Add(poolGo); poolGo.SetActive(false);
        pool = poolGo.AddComponent<InstancePoolManager>();
        typeof(Z_MonoSingleton<InstancePoolManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, pool);
        pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal); pool.defaultRoot = poolGo.transform;
        var prefab = new GameObject("corner-character"); owned.Add(prefab); prefab.SetActive(false);
        var body = prefab.AddComponent<SphereCollider>(); body.radius = .4f; body.center = Vector3.up * .4f; pool.AddPool(prefab);
        try
        {
            map.enable = false; DynamicGlobalSettings.pauseNav = false; DynamicGlobalSettings.allowDrop = true;
            map.navigationCtrl = nav; map.updateCtrl = new MapUpdateController(map);
            typeof(NavigationController).GetField("bfs", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(nav, search);
            var start = new Vector3(2.5f, 0, 1.5f); var target = new Vector3(2.5f, 0, 10.5f);
            var data = new CharacterUnitForm.Data(98700, true, target, 6, 999, 999, true, "", prefab.name,
                start, Vector3.zero, Vector3.one * 2, UpdateType.Always, new List<int>(), "", false, 0, new List<int>());
            var character = data.unit;
            var state = (NavigationEndpointState)typeof(CharacterUnit).GetField("navigationEndpointState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(character);
            var move = typeof(CharacterUnit).GetMethod("MoveAlongNavigation", BindingFlags.Instance | BindingFlags.NonPublic);
            Await(search, data.pos, target, state, 2);
            for (int frame = 0; frame < 300 && (data.pos - target).sqrMagnitude > 1e-8f; frame++)
            {
                search.Advance(); var direction = map.updateCtrl.GetNavDir(data.pos, target, 999, .8f, NoTypes, state, 2);
                move.Invoke(character, new object[] { .15f, direction });
                Check(state.searchedCellsThisFrame <= 10, "real even-sized Character substeps share the frame budget");
            }
            Check((data.pos - target).sqrMagnitude < 1e-8f, "the real scaled Character executes the two-cell corridor route");
        }
        finally
        { map.navigationCtrl = oldNav; map.updateCtrl = oldUpdate; map.enable = oldEnabled; DynamicGlobalSettings.pauseNav = oldPause; DynamicGlobalSettings.allowDrop = oldDrop; }
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
        Check(state.isSearching && !state.hasResult && direction == Vector3.zero,
            "a pending nearest-start scan cannot invent movement");
        AwaitEndpointRound(bfs, position, target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(3, 0, 3),
            "blocked starts still escape to their nearest legal center");
        AwaitEndpointRound(bfs, new Vector3(2.65f, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(3, 0, 3),
            "escape keeps its center until arrival");
        var replacement = new Vector3(7, 0, 3);
        bool graph = false;
        for (int frame = 0; frame < 160 && (position - replacement).sqrMagnitude > 1e-8f; frame++)
        {
            direction = AwaitEndpointRound(bfs, position, target, state);
            graph |= !state.directEndpoint;
            Check(!state.directEndpoint || state.moveTarget != target,
                "target replacement never grants a direct leg into the blocked original");
            position += direction * Mathf.Min(.15f, (state.moveTarget - position).magnitude);
        }
        Check(graph && (position - replacement).sqrMagnitude < 1e-8f,
            "escape then BFS ends at the preselected walkable target center: " + position);
        Check(AwaitEndpointRound(bfs, replacement, target, state) == Vector3.zero && !state.directEndpoint,
            "arriving at the replacement stops instead of continuing to the original target");
        AwaitEndpointRound(bfs, replacement, target + new Vector3(.1f, 0, .05f), state, radius: .500005f);
        Check(state.moveTarget == replacement && !state.directEndpoint,
            "same-cell target motion and radius tolerance cannot overwrite the legal replacement");
        var other = new NavigationEndpointState();
        AwaitEndpointRound(bfs, start.realPos + Vector3.right * .1f, Vector3.one, other);
        Check(other.directEndpoint && other.moveTarget == new Vector3(3, 0, 3), "each Character retains its own escape");
        state.Reset();
        AwaitEndpointRound(bfs, start.realPos + Vector3.right * .1f, target, state, radius: 1f);
        Check(state.directEndpoint && state.moveTarget == new Vector3(4, 0, 3),
            "large Characters escape only to footprint-safe anchors");
        var restrictedStart = navigation.navUnits[(3, 0, 3)]; restrictedStart.passTypes.Add(7201);
        state.Reset(); AwaitEndpointRound(bfs, restrictedStart.realPos + Vector3.right * .1f, target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(4, 0, 3), "pass-restricted starts still escape");
        state.Reset(); AwaitEndpointRound(bfs, restrictedStart.realPos, restrictedStart.realPos + Vector3.right * .1f,
            state, types: new[] { 7201 });
        Check(!state.directEndpoint && state.moveTarget == restrictedStart.realPos + Vector3.right * .1f,
            "capable Characters retain normal exact same-cell targets");
        restrictedStart.passTypes.Clear();
        var restrictedTarget = navigation.navUnits[(7, 0, 5)]; restrictedTarget.passTypes.Add(7202);
        state.Reset(); AwaitEndpointRound(bfs, new Vector3(7, 0, 4), restrictedTarget.realPos + Vector3.right * .1f, state);
        Check(!state.directEndpoint && state.moveTarget == new Vector3(7, 0, 4),
            "a pass-restricted destination is replaced before BFS and does not bypass the restriction");
        restrictedTarget.passTypes.Clear();
        state.Reset(); AwaitEndpointRound(bfs, new Vector3(0, 0, 3), target, state);
        Check(state.directEndpoint && state.moveTarget == new Vector3(1, 0, 3), "missing starts keep nearest-node escape");
        state.Reset(); var limitedStart = new Vector3(3, 0, 2);
        AwaitEndpointRound(bfs, limitedStart, target, state, maxStep: 0);
        Check(!state.directEndpoint && state.moveTarget == limitedStart, "replacement does not bypass BFS depth limits");
        var isolated = new NavUnit { pos = new Vector3Int(6, 0, 4), realPos = target - Vector3.forward * .05f,
            links = new List<NavUnit>(), passTypes = new HashSet<int>(), dirMaxY = new float[4], dirGroundY = new float[4] };
        navigation.navUnits[(6, 0, 4)] = isolated;
        state.Reset(); AwaitEndpointRound(bfs, replacement, target, state);
        Check(SelectedTarget(state) == isolated.realPos && !state.directEndpoint,
            "preprocessing selects geometric walkability; subsequent BFS handles disconnected candidates normally");
        navigation.navUnits.Remove((6, 0, 4));
        state.Reset(); AwaitEndpointRound(bfs, new Vector3(4, 0, 3), new Vector3(8, 0, 3), state);
        Check(SelectedTarget(state) == replacement && !state.directEndpoint, "missing destinations use a legal replacement");
        var nullStart = navigation.navUnits[(2, 0, 1)]; nullStart.isNull = true;
        state.Reset(); AwaitEndpointRound(bfs, nullStart.realPos, target, state);
        Check(state.directEndpoint && state.moveTarget != nullStart.realPos, "zero-scale starts retain escape");
        state.Reset(); bfs.Advance(); EndpointStep(bfs, replacement, target, state);
        Check(state.isSearching && !state.hasResult, "target preprocessing is frame-budgeted");
        Vector3 freshTarget = new Vector3(1, 0, 1);
        AwaitEndpointRound(bfs, replacement, freshTarget, state);
        AwaitEndpointRound(bfs, replacement, freshTarget, state);
        Check(state.hasResult && !state.directEndpoint && SelectedTarget(state) == freshTarget,
            "the next round captures the latest target while preserving safe cached movement");
        navigation.navUnits.Clear(); state.Reset(); bfs.Advance();
        Check(EndpointStep(bfs, Vector3.zero, Vector3.one, state) == Vector3.zero && !state.hasResult,
            "empty navigation safely clears endpoint and route state");
    }

    private static Vector3 SelectedTarget(NavigationEndpointState state)
    {
        var search = typeof(NavigationEndpointState).GetField("searchState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(state);
        return (Vector3)search.GetType().GetField("roundRouteDestination", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(search);
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
