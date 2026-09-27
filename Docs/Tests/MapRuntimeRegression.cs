using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Mesh;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Copied into an isolated Editor project by Run-MapRuntimeRegression.ps1.
// Never run this fixture inside a loaded story: it installs temporary singletons.
public static class MapRuntimeRegression
{
    private static int checks;
    private static MapManager map;
    private static InstancePoolManager pool;
    private static GameObject source;
    private static BoxCollider box;
    private static readonly List<GameObject> objects = new List<GameObject>();

    public static void Run()
    {
        try
        {
            SetUp();
            DictionaryQueries();
            Geometry();
            CaptureCastWholeSegment();
            MapGroundCoverageCaching();
            Coverage();
            LegacyPassTypeSceneData();
            PassTypeMovement();
            NavigationEndpoints();
            WangTileAnimationSelection();
            ObjectWangTileNeighbourSelection();
            SharedAnimationClock();
            CanvasHolderResetAfterPoolTeardown();
            SparseViewRefreshAfterErase();
            EraseBrushOwnedUnits();
            IncrementalViewRefresh();
            HeightProjectedViewRefresh();
            Visibility();
            TileFirstVisibility();
            OcclusionTransparencyAndPriority();
            Events();
            ObjectCenterPassTypeRefresh();
            SavedBridgePassTypeCoverage();
            ObjectTileUpwardProbeRange();
            BridgeObstacleNavigation();
            WholeTileObjectNavigation();
            ObjectCollisionScalePassTypeCoverage();
            ObjectMarksAndMissionCoordinates();
            EventOperatorRendering();
            Debug.Log("MAP_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunViewRefresh()
    {
        try
        {
            SetUp();
            IncrementalViewRefresh();
            HeightProjectedViewRefresh();
            Debug.Log("MAP_VIEW_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    private static GameObject Go(string name)
    {
        var go = new GameObject(name);
        objects.Add(go);
        return go;
    }

    private static void SetUp()
    {
        var mapGo = Go("TestMap");
        mapGo.SetActive(false);
        map = mapGo.AddComponent<MapManager>();
        typeof(Z_MonoSingleton<MapManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, map);
        map.navigationCtrl = new Z_Map.Analysis.NavigationController(map);
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        map.enable = true;
        DynamicGlobalSettings.playing = true;
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, new Vector3(1, 1.5f, 1),
                new Vector3Int(20, 5, 20), new Vector3Int(8, 3, 8), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };

        var poolGo = Go("TestPool");
        poolGo.SetActive(false);
        pool = poolGo.AddComponent<InstancePoolManager>();
        typeof(Z_MonoSingleton<InstancePoolManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, pool);
        pool.pools = new List<InstancePool>();
        pool.defaultRoot = poolGo.transform;

        source = Go("regression-prefab");
        source.transform.position = new Vector3(7, -3, 2);
        source.transform.eulerAngles = new Vector3(17, 31, 9);
        source.transform.localScale = new Vector3(1.2f, 0.8f, 1.6f);
        var child = Go("nested-box");
        child.transform.SetParent(source.transform, false);
        child.transform.localPosition = new Vector3(0.13f, 0.6f, -0.1f);
        child.transform.localEulerAngles = new Vector3(12, 38, 6);
        child.transform.localScale = new Vector3(0.7f, 1.2f, 0.9f);
        box = child.AddComponent<BoxCollider>();
        box.center = new Vector3(0.1f, -0.2f, 0.15f);
        box.size = new Vector3(0.6f, 1, 0.4f);
        var sphereGo = Go("nested-trigger");
        sphereGo.transform.SetParent(source.transform, false);
        sphereGo.transform.localPosition = new Vector3(-0.15f, 0.4f, 0.2f);
        var sphere = sphereGo.AddComponent<SphereCollider>();
        sphere.center = new Vector3(0.05f, 0.1f, -0.2f);
        sphere.radius = 0.35f;
        sphere.isTrigger = true;
        pool.AddPool(source);
        for (int x = 0; x < 13; x++)
        for (int y = 0; y < 3; y++)
        for (int z = 0; z < 13; z++)
        {
            int uid = 1 + x * 100 + y * 20 + z;
            var tile = new TileUnitForm.Data(uid, "", new Dictionary<int, int>(), new Vector3Int(x, y, z),
                source.name, new Vector3(x, y * 1.5f, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(tile);
        }

        map.navigationCtrl.navUnits = new Dictionary<(int, int, int), Z_Map.Analysis.NavUnit>();
        foreach (var tile in map.data.maps.Values)
        {
            var navUnit = NavUnit(tile.mapPos.x, tile.mapPos.z, tile.mapPos.y);
            navUnit.links.Add(navUnit);
            map.navigationCtrl.navUnits[(tile.mapPos.x, tile.mapPos.y, tile.mapPos.z)] = navUnit;
        }
    }

    private static UnitForm.Data Data(int uid, Vector3 pos)
    {
        return new UnitForm.Data(uid, "", source.name, pos, Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "");
    }

    private static CharacterUnit Character()
    {
        return new CharacterUnitForm.Data(9001, false, Vector3.zero, 1, 1, 1, false, "",
            source.name, new Vector3(5, 0, 5), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>()).unit;
    }

    private static void Geometry()
    {
        var unit = new MapUnit(Data(9000, new Vector3(3, 1, 7)));
        foreach (var scale in new[] { Vector3.one, Vector3.one * 2, Vector3.one * 3, new Vector3(1.3f, 0.8f, 2.1f) })
        foreach (var rotation in new[] { Vector3.zero, new Vector3(23, 67, 11) })
        {
            unit.data.scale = scale;
            unit.data.euler = rotation;
            CompareGeometry(unit);
            var oldList = unit.GetMeshes(CollideType.All);
            var oldMesh = oldList[0];
            var oldVertices = oldMesh.positions;
            for (int i = 0; i < 20; i++)
            {
                unit.data.pos = new Vector3(3 + i * 0.123f, 1 - i * 0.01f, 7 - i * 0.137f);
                CompareGeometry(unit);
                Require(ReferenceEquals(oldList, unit.GetMeshes(CollideType.All)), "translation retained list");
                Require(ReferenceEquals(oldMesh, oldList[0]) && ReferenceEquals(oldVertices, oldList[0].positions),
                    "translation retained mesh and vertex array");
            }
            Require(ReferenceEquals(unit.GetMeshes(CollideType.CollideOnly)[0], oldList[0]), "All shares physical mesh");
            Require(ReferenceEquals(unit.GetMeshes(CollideType.TriggerOnly)[0], oldList[1]), "All shares trigger mesh");
        }

        var old = unit.GetMeshes(CollideType.All);
        box.size *= 1.1f;
        unit.InvalidateCollisionGeometry();
        CompareGeometry(unit);
        Require(!ReferenceEquals(old, unit.GetMeshes(CollideType.All)), "explicit geometry invalidation");
        var alternate = Object.Instantiate(source);
        objects.Add(alternate);
        alternate.name = "alternate-prefab";
        alternate.GetComponentInChildren<BoxCollider>().size *= 0.8f;
        pool.AddPool(alternate);
        unit.data.prefabName = alternate.name;
        CompareGeometry(unit);
    }

    private static void CaptureCastWholeSegment()
    {
        var middleTile = map.utilCtrl.GetTile(5, 0, 2);
        var upperTile = map.utilCtrl.GetTile(5, 1, 2);
        var upperPrefab = Object.Instantiate(source);
        objects.Add(upperPrefab);
        upperPrefab.name = "downward-tile-collider";
        var upperBox = upperPrefab.GetComponentInChildren<BoxCollider>();
        upperBox.transform.localPosition = new Vector3(0, -0.8f, 0);
        upperBox.transform.localEulerAngles = Vector3.zero;
        upperBox.transform.localScale = Vector3.one;
        upperBox.center = Vector3.zero;
        upperBox.size = Vector3.one;
        pool.AddPool(upperPrefab);

        var originalPrefabName = upperTile.data.prefabName;
        try
        {
            upperTile.data.prefabName = upperPrefab.name;
            upperTile.InvalidateCollisionGeometry();
            var hits = map.utilCtrl.CaptureCast(new Vector3(2, 0.6f, 2), new Vector3(8, 0.6f, 2), 0.1f);
            Require(hits.Contains(middleTile), "CaptureCast includes a Tile collider between its endpoints");
            Require(hits.Contains(upperTile), "CaptureCast includes a Tile collider extending down from a higher layer");
        }
        finally
        {
            upperTile.data.prefabName = originalPrefabName;
            upperTile.InvalidateCollisionGeometry();
        }
    }

    private sealed class HashProbe
    {
        public int reads;
        public override int GetHashCode() { reads++; return 71; }
    }

    private static void DictionaryQueries()
    {
        var dic = new Z_DoubleDictionary.DoubleDictionary<HashProbe, string>();
        var key = new HashProbe();
        dic.Add(key, "owner");
        key.reads = 0;
        var values = dic.Get(key);
        Require(key.reads == 1 && values.SequenceEqual(new[] { "owner" }), "Get hit hashes once");
        key.reads = 0;
        Require(dic.GetFirst(key) == "owner" && key.reads == 1, "GetFirst hit hashes once");
        key.reads = 0;
        Require(dic.TryGet(key, out var same) && ReferenceEquals(values, same) && key.reads == 1,
            "read-only query hashes once and retains list");
        key.reads = 0;
        Require(dic.TryGetFirst(key, out var owner) && owner == "owner" && key.reads == 1,
            "read-only first hashes once");
        Require(dic.TryGet("owner", out var reverse) && reverse.Single() == key, "reverse read-only query");
        Require(ReferenceEquals(reverse, dic.Get("owner")), "reverse Get retains list");
        var missing = new HashProbe();
        int forwardCount = dic.GetDicT1().Count, reverseCount = dic.GetDicT2().Count;
        Require(!dic.TryGet(missing, out _) && !dic.TryGetFirst(missing, out _) && !dic.TryGet("missing", out _),
            "read-only missing queries");
        Require(dic.GetDicT1().Count == forwardCount && dic.GetDicT2().Count == reverseCount,
            "read-only misses do not create empty keys");
        Require(dic.GetFirst(missing) == null, "legacy GetFirst miss returns default");
        Require(dic.TryGet(missing, out var empty) && empty.Count == 0,
            "legacy GetFirst miss still creates a list");
        Require(!dic.TryGetFirst(missing, out _) && ReferenceEquals(empty, dic.Get(missing)),
            "empty first query and stable empty list");
        Require(dic.Get("new").Count == 0 && dic.GetDicT2().ContainsKey("new"), "legacy reverse miss creates a list");
        dic.Move(key, "moved");
        Require(dic.GetFirst(key) == "moved" && dic.Get("owner").Count == 0, "Move preserves reverse links");
        dic.Del(key, "moved");
        Require(dic.Get(key).Count == 0 && dic.Get("moved").Count == 0, "pair removal remains bidirectional");
    }

    private static void CompareGeometry(MapUnit unit)
    {
        var expected = map.utilCtrl.GetCollidersMesh(unit.prefab, unit.data.pos, unit.data.euler, unit.data.scale, CollideType.CollideOnly);
        expected.AddRange(map.utilCtrl.GetCollidersMesh(unit.prefab, unit.data.pos, unit.data.euler, unit.data.scale, CollideType.TriggerOnly));
        var actual = unit.GetMeshes(CollideType.All);
        Require(expected.Count == actual.Count, "mesh count");
        for (int i = 0; i < expected.Count; i++)
        {
            Require(Vector3.Distance(expected[i].center, actual[i].center) < 0.0001f, "world mesh center");
            for (int j = 0; j < expected[i].positions.Length; j++)
                Require(Vector3.Distance(expected[i].positions[j], actual[i].positions[j]) < 0.0001f, "world vertex");
        }
    }

    private static void Coverage()
    {
        var unit = Character();
        var ctrl = map.updateCtrl;
        var owner = map.utilCtrl.GetTile(5, 0, 5);
        ctrl.characterTileDic.Add(unit, owner);
        ctrl.RefreshCharacterOverlap(unit);
        var retainedList = ctrl.characterOverlapTileDic.Get(unit);
        var reverseOrder = ctrl.characterOverlapTileDic.Get(owner).ToArray();
        ctrl.RefreshCharacterOverlap(unit);
        Require(ReferenceEquals(retainedList, ctrl.characterOverlapTileDic.Get(unit)), "unchanged coverage retained forward list");
        Require(reverseOrder.SequenceEqual(ctrl.characterOverlapTileDic.Get(owner)), "unchanged coverage retained reverse order");
        Require(retainedList.Any(t => t.data.mapPos.y > 0), "upper-layer candidates retained");

        foreach (int size in new[] { 1, 2, 3 })
        foreach (float angle in new[] { 0f, 45f })
        {
            unit.data.scale = Vector3.one * size;
            unit.data.euler = new Vector3(0, angle, 0);
            unit.data.pos += new Vector3(0.6f, 0, 0);
            ctrl.RefreshCharacterOverlap(unit);
            var expected = new HashSet<TileUnit>(map.utilCtrl.GetCharacterCollisionTiles(unit, Vector3.zero, CollideType.All));
            var actual = ctrl.characterOverlapTileDic.Get(unit);
            Require(expected.SetEquals(actual) && actual.Count == expected.Count, "complete and unique coverage");
            foreach (var pair in ctrl.characterOverlapTileDic.GetDicT2())
                Require(pair.Value.Count(c => c == unit) == (expected.Contains(pair.Key) ? 1 : 0), "reverse coverage synchronized");
        }
        ctrl.characterTileDic.Del(unit);
        ctrl.RefreshCharacterOverlap(unit);
        Require(!ctrl.characterOverlapTileDic.GetDicT1().ContainsKey(unit), "unowned character removed");
        Require(ctrl.characterOverlapTileDic.GetDicT2().Values.All(v => !v.Contains(unit)), "unowned reverse links removed");
    }

    private static void MapGroundCoverageCaching()
    {
        var groundPrefab = Go(MapInfo.GetPrefabName("mapground"));
        var groundCollider = groundPrefab.AddComponent<BoxCollider>();
        groundCollider.center = new Vector3(0f, -1f, 0f);
        groundCollider.size = new Vector3(0.8f, 2f, 0.8f);
        pool.AddPool(groundPrefab);

        var tile = map.utilCtrl.GetTile(5, 1, 5);
        string originalPrefabName = tile.data.prefabName;
        Vector3 originalPos = tile.data.pos;
        Vector3 originalEuler = tile.data.euler;
        Vector3 originalScale = tile.data.scale;
        tile.data.prefabName = groundPrefab.name;
        tile.data.pos = new Vector3(5f, 1.5f, 5f);
        tile.data.euler = Vector3.zero;
        tile.data.scale = Vector3.one;
        tile.InvalidateCollisionGeometry();

        var mark = typeof(Z_Map.Analysis.NavigationController).GetMethod(
            "MarkMapGroundCoveredNavUnits",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var cacheField = typeof(Z_Map.Analysis.NavigationController).GetField(
            "mapGroundCoverageCaches",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var blocked = new HashSet<(int, int, int)>();
        mark.Invoke(map.navigationCtrl, new object[] { tile.data, blocked });
        Require(blocked.Contains((5, 0, 5)),
            "mapground coverage still blocks the lower NavUnit");

        var caches = (System.Collections.IDictionary)cacheField.GetValue(map.navigationCtrl);
        object firstCache = caches[tile.data.uid];
        blocked.Clear();
        mark.Invoke(map.navigationCtrl, new object[] { tile.data, blocked });
        Require(ReferenceEquals(firstCache, caches[tile.data.uid]),
            "unchanged mapground reuses its covered-NavUnit cache");

        groundCollider.center = new Vector3(0f, -2.31f, 0f); // top at lower floor + 0.19
        tile.InvalidateCollisionGeometry();
        caches.Remove(tile.data.uid); // simulate the prefab-edit cache invalidation
        blocked.Clear();
        mark.Invoke(map.navigationCtrl, new object[] { tile.data, blocked });
        Require(!blocked.Contains((5, 0, 5)),
            "mapground protruding 0.19 above the lower floor remains passable");

        groundCollider.center = new Vector3(0f, -2.29f, 0f); // top at lower floor + 0.21
        tile.InvalidateCollisionGeometry();
        caches.Remove(tile.data.uid);
        blocked.Clear();
        mark.Invoke(map.navigationCtrl, new object[] { tile.data, blocked });
        Require(blocked.Contains((5, 0, 5)),
            "mapground protruding 0.21 above the lower floor blocks navigation");

        groundCollider.center = new Vector3(0f, -1f, 0f);
        tile.InvalidateCollisionGeometry();
        tile.data.pos += Vector3.up * 2f;
        tile.InvalidateCollisionGeometry();
        blocked.Clear();
        mark.Invoke(map.navigationCtrl, new object[] { tile.data, blocked });
        Require(!ReferenceEquals(firstCache, caches[tile.data.uid]),
            "mapground transform changes invalidate its coverage cache");
        Require(!blocked.Contains((5, 0, 5)) && blocked.Contains((5, 2, 5)),
            "rebuilt mapground coverage follows the changed transform");

        tile.data.prefabName = originalPrefabName;
        tile.data.pos = originalPos;
        tile.data.euler = originalEuler;
        tile.data.scale = originalScale;
        tile.InvalidateCollisionGeometry();
        caches.Remove(tile.data.uid);
    }

    private static void PassTypeMovement()
    {
        var unit = new CharacterUnitForm.Data(9050, false, Vector3.zero, 1, 1, 1, false, "",
            source.name, new Vector3(5, 0, 5), Vector3.zero, Vector3.one * 3,
            UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>()).unit;
        var owner = map.utilCtrl.GetTile(5, 0, 5);
        var restricted = map.utilCtrl.GetTile(6, 0, 5);
        map.updateCtrl.characterTileDic.Add(unit, owner);
        restricted.SetPassTypes(new[] { 7001 });

        Vector3 oldPos = unit.data.pos;
        Vector3 sameCenterTile = new Vector3(5.49f, 0, 5);
        Require(Vector3.Distance(unit.ClampMoveToPassType(oldPos, sameCenterTile), sameCenterTile) < 0.0001f,
            "character size and Collider do not affect center-Tile pass type checks");

        Vector3 enterRestricted = new Vector3(5.51f, 0, 5);
        Vector3 corrected = unit.ClampMoveToPassType(oldPos, enterRestricted);
        Vector3Int correctedMapPos = map.utilCtrl.RealPos2MapPosInt(corrected);
        TileUnit correctedTile = map.utilCtrl.GetTile(correctedMapPos.x, correctedMapPos.y, correctedMapPos.z);
        Require(unit.CanPass(correctedTile) && Mathf.Abs(corrected.x - 5.49f) < 0.0001f,
            "restricted center Tile moves the character to the closest legal position");

        Vector3 restrictedRightSide = new Vector3(6.4f, 0, 5);
        corrected = unit.ClampMoveToPassType(oldPos, restrictedRightSide);
        Require(Mathf.Abs(corrected.x - 6.51f) < 0.0001f,
            "closest legal position is selected from the nearest side of the restricted Tile");

        var navigation = new Z_Map.Analysis.NavigationController(map)
        {
            navUnits = new Dictionary<(int, int, int), Z_Map.Analysis.NavUnit>()
        };
        var navFrom = NavUnit(5, 5);
        var navTarget = NavUnit(6, 5);
        var navFootprint = NavUnit(7, 5);
        navFootprint.passTypes.Add(7001);
        navFrom.links.Add(navTarget);
        navTarget.links.Add(navFootprint);
        navigation.navUnits[(5, 0, 5)] = navFrom;
        navigation.navUnits[(6, 0, 5)] = navTarget;
        navigation.navUnits[(7, 0, 5)] = navFootprint;
        var bfs = new Z_Map.Analysis.Bfs(navigation);
        var clearance = new List<Vector2Int> { Vector2Int.zero, Vector2Int.right };
        Require(bfs.CanPass(navFrom, navTarget, clearance, Array.Empty<int>()),
            "navigation pass type ignores restricted neighbouring footprint Tiles");
        navTarget.passTypes.Add(7001);
        Require(!bfs.CanPass(navFrom, navTarget, clearance, Array.Empty<int>()),
            "navigation pass type still rejects a restricted center Tile");
        Require(navigation.IsBaseWalkable(6, 0, 5),
            "base navigation walkability ignores pass type requirements");
        navTarget.links.Clear();
        Require(!navigation.IsBaseWalkable(6, 0, 5),
            "base navigation walkability rejects a NavUnit without connections");
        navTarget.links.Add(navFootprint);

        unit.SetPassTypes(new[] { 7001 });
        Require(Vector3.Distance(unit.ClampMoveToPassType(oldPos, enterRestricted), enterRestricted) < 0.0001f,
            "matching character pass type permits entry");

        restricted.SetPassTypes(null);
        map.updateCtrl.characterTileDic.Del(unit);
    }

    private static void NavigationEndpoints()
    {
        var navigation = new Z_Map.Analysis.NavigationController(map)
        {
            navUnits = new Dictionary<(int, int, int), Z_Map.Analysis.NavUnit>(),
            step = 0.5f
        };
        for (int x = 1; x <= 7; x++)
        for (int z = 1; z <= 5; z++)
            navigation.navUnits[(x, 0, z)] = NavUnit(x, z);
        foreach (var unit in navigation.navUnits.Values)
            foreach (var offset in new[] { Vector3Int.right, Vector3Int.left, Vector3Int.forward, Vector3Int.back })
                if (navigation.navUnits.TryGetValue((unit.pos.x + offset.x, 0, unit.pos.z + offset.z), out var next))
                    unit.links.Add(next);
        Action<Z_Map.Analysis.NavUnit> block = unit =>
        {
            unit.objectBlocked = true;
            unit.links.Clear();
            foreach (var other in navigation.navUnits.Values)
                other.links.Remove(unit);
        };
        var start = navigation.navUnits[(2, 0, 3)];
        var destination = navigation.navUnits[(6, 0, 3)];
        block(start);
        block(destination);
        var bfs = new Z_Map.Analysis.Bfs(navigation);
        var state = new Z_Map.Analysis.NavigationEndpointState();
        Vector3 target = destination.realPos + Vector3.right * 0.1f;
        Vector3 position = start.realPos + Vector3.right * 0.1f;
        Vector3 direction = bfs.GetNextDir(position, target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == new Vector3(3, 0, 3)
            && Vector3.Dot(direction, Vector3.right) > 0.999f,
            "blocked start moves straight to its nearest walkable cell, not toward the destination");
        bfs.GetNextDir(new Vector3(2.65f, 0, 3), target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == new Vector3(3, 0, 3),
            "escape continues to its anchor center after crossing into a walkable cell");
        bool usedGraph = false;
        bool usedFinalSegment = false;
        for (int frame = 0; frame < 100 && Vector3.Distance(position, target) > 0.0001f; frame++)
        {
            direction = bfs.GetNextDir(position, target, 100, 0.5f, Array.Empty<int>(), state);
            if (!state.directEndpoint) usedGraph = true;
            if (state.directEndpoint && state.moveTarget == target) usedFinalSegment = true;
            position += direction * Mathf.Min(0.15f, (state.moveTarget - position).magnitude);
        }
        Require(usedGraph && usedFinalSegment && Vector3.Distance(position, target) < 0.0001f,
            "both blocked endpoints use escape, a graph route and the final direct segment");
        direction = bfs.GetNextDir(new Vector3(5.65f, 0, 3), target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == target && direction.x > 0.99f,
            "entering the blocked destination cell does not reverse the final approach");
        Require(bfs.GetNextDir(target, target, 100, 0.5f, Array.Empty<int>(), state) == Vector3.zero,
            "the final approach stops at the original target coordinate");
        Vector3 movingTarget = target + Vector3.forward * 0.1f;
        direction = bfs.GetNextDir(new Vector3(5.65f, 0, 3), movingTarget, 100, 0.500005f,
            Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == movingTarget
            && Vector3.Dot(direction, (movingTarget - new Vector3(5.65f, 0, 3)).normalized) > 0.999f,
            "a moving target in the same blocked cell and radius rounding do not reset the final phase");

        var otherState = new Z_Map.Analysis.NavigationEndpointState();
        bfs.GetNextDir(start.realPos + Vector3.right * 0.1f, new Vector3(1, 0, 1), 100, 0.5f,
            Array.Empty<int>(), otherState);
        Require(otherState.moveTarget == new Vector3(3, 0, 3), "another agent gets its own escape state");
        direction = bfs.GetNextDir(new Vector3(5.65f, 0, 3), target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.moveTarget == target && direction.x > 0.99f, "shared BFS does not leak another agent's phase");
        bfs.GetNextDir(destination.realPos, new Vector3(7, 0, 5), 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget != new Vector3(7, 0, 5),
            "changing destination cancels the old final segment and rechecks the blocked start");

        state.Reset();
        bfs.GetNextDir(start.realPos + Vector3.right * 0.1f, target, 100, 1f, Array.Empty<int>(), state);
        Require(state.moveTarget == new Vector3(4, 0, 3),
            "a large character escapes to a cell with a clear physical footprint");
        state.Reset();
        var restricted = navigation.navUnits[(3, 0, 3)];
        restricted.passTypes.Add(7201);
        bfs.GetNextDir(restricted.realPos + Vector3.right * 0.1f, new Vector3(7, 0, 5), 100, 0.5f,
            Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == new Vector3(4, 0, 3),
            "a pass-type restricted start also escapes to a legal center cell");
        state.Reset();
        bfs.GetNextDir(restricted.realPos, restricted.realPos + Vector3.right * 0.1f, 100, 0.5f,
            new[] { 7201 }, state);
        Require(!state.directEndpoint && state.moveTarget == restricted.realPos + Vector3.right * 0.1f,
            "a character with the required pass type retains ordinary same-cell movement");
        restricted.passTypes.Clear();
        var restrictedTarget = navigation.navUnits[(7, 0, 5)];
        restrictedTarget.passTypes.Add(7202);
        state.Reset();
        Vector3 restrictedCoordinate = restrictedTarget.realPos + Vector3.right * 0.1f;
        bfs.GetNextDir(new Vector3(7, 0, 4), restrictedCoordinate, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == restrictedCoordinate,
            "a pass-restricted destination is approached from the nearest legal cell");
        direction = bfs.GetNextDir(new Vector3(7, 0, 4.65f), restrictedCoordinate, 100, 0.5f,
            Array.Empty<int>(), state);
        Require(state.directEndpoint && direction.z > 0f,
            "the final phase remains active inside a pass-restricted destination");
        restrictedTarget.passTypes.Clear();

        state.Reset();
        bfs.GetNextDir(new Vector3(0, 0, 3), target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == new Vector3(1, 0, 3),
            "a missing start cell escapes to the nearest actual walkable node");

        state.Reset();
        bfs.GetNextDir(new Vector3(7, 0, 3), target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == target,
            "a blocked target begins its final leg from the nearest reachable center");
        state.Reset();
        bfs.GetNextDir(new Vector3(3, 0, 2), target, 0, 0.5f, Array.Empty<int>(), state);
        Require(!state.directEndpoint && state.moveTarget == new Vector3(3, 0, 2),
            "a truncated BFS cannot mistake its start for the final anchor and cut through the map");

        var isolated = NavUnit(6, 4);
        isolated.realPos = target - Vector3.forward * 0.05f;
        isolated.links.Add(isolated);
        navigation.navUnits[(6, 0, 4)] = isolated;
        state.Reset();
        bfs.GetNextDir(new Vector3(7, 0, 3), target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget == target,
            "a geometrically closer isolated cell is not selected as the destination anchor");
        state.Reset();
        bfs.GetNextDir(new Vector3(4, 0, 3), new Vector3(8, 0, 3), 100, 0.5f, Array.Empty<int>(), state);
        Require(!state.directEndpoint && state.moveTarget != new Vector3(4, 0, 3),
            "a missing destination cell routes through the graph before any final segment");
        state.Reset();
        var nullStart = navigation.navUnits[(2, 0, 1)];
        nullStart.isNull = true;
        bfs.GetNextDir(nullStart.realPos, target, 100, 0.5f, Array.Empty<int>(), state);
        Require(state.directEndpoint && state.moveTarget != nullStart.realPos,
            "a zero-scale start with no floor does not throw during escape resolution");
        navigation.navUnits.Clear();
        state.Reset();
        Require(bfs.GetNextDir(Vector3.zero, Vector3.one, 100, 0.5f, Array.Empty<int>(), state) == Vector3.zero,
            "a map with no walkable cells returns no movement safely");

        // Real ApplyMove must preserve ordinary/manual restrictions while only
        // the explicitly scoped endpoint move can enter a restricted center Tile.
        var character = Character();
        var tile = map.utilCtrl.GetTile(6, 0, 5);
        tile.SetPassTypes(new[] { 7201 });
        var endpointFlag = typeof(CharacterUnit).GetProperty("isNavEndpointMove", BindingFlags.Instance | BindingFlags.NonPublic);
        try
        {
            endpointFlag.GetSetMethod(true).Invoke(character, new object[] { true });
            map.updateCtrl.ApplyMove(character, new Vector3(5.7f, 0, 5), Vector3.zero);
            Require(Mathf.Abs(character.data.pos.x - 5.7f) < 0.0001f && character.belongTile == tile,
                "direct endpoint application enters a restricted target without teleporting");
            endpointFlag.GetSetMethod(true).Invoke(character, new object[] { false });
            map.updateCtrl.ApplyMove(character, new Vector3(5.8f, 0, 5), Vector3.zero);
            Require(Mathf.Abs(character.data.pos.x - 5.49f) < 0.0001f,
                "manual movement still clamps to the nearest legal Tile");
            var directMove = typeof(CharacterUnit).GetMethod("MoveForNavigation", BindingFlags.Instance | BindingFlags.NonPublic);
            directMove.Invoke(character, new object[] { Vector3.zero, true });
            Require(!(bool)endpointFlag.GetValue(character), "endpoint permission is reset after Move returns");
            var characterState = (Z_Map.Analysis.NavigationEndpointState)typeof(CharacterUnit)
                .GetField("navigationEndpointState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(character);
            typeof(Z_Map.Analysis.NavigationEndpointState).GetField("finalApproach", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(characterState, true);
            map.updateCtrl.ApplyMove(character, character.data.pos, Vector3.zero, true);
            Require(!(bool)typeof(Z_Map.Analysis.NavigationEndpointState)
                .GetField("finalApproach", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(characterState),
                "teleport cancels any previous endpoint phase");
        }
        finally
        {
            endpointFlag.GetSetMethod(true).Invoke(character, new object[] { false });
            tile.SetPassTypes(null);
            map.updateCtrl.characterTileDic.Del(character);
            foreach (var covered in map.updateCtrl.characterOverlapTileDic.Get(character).ToList())
                map.updateCtrl.characterOverlapTileDic.Del(character, covered);
        }
    }

    private static void ObjectCenterPassTypeRefresh()
    {
        // The regular fixture registers Tiles only in MapInfo. Full Nav rebuilds
        // also consult the generated Form registry, so install it here in memory.
        TileUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        foreach (var tileData in map.data.maps.Values)
            TileUnitForm.AddData(tileData);
        map.navigationCtrl.navUnits = new Dictionary<(int, int, int), Z_Map.Analysis.NavUnit>();
        var tile = map.utilCtrl.GetTile(12, 0, 12);
        tile.SetPassTypes(new[] { 7001 });
        var solidTile = map.utilCtrl.GetTile(6, 1, 6);
        string solidOriginalPrefab = solidTile.data.prefabName;
        solidTile.data.prefabName = MapInfo.GetPrefabName("mapground");
        solidTile.InvalidateCollisionGeometry();
        map.navigationCtrl.step = map.data.mainData.mapUnitSize.y / 3f;
        map.navigationCtrl.RebuildNow();
        Require(!map.navigationCtrl.IsBaseWalkable(6, 0, 6),
            "solid terrain blocks the lower Nav cell before local Object updates");
        var distant = map.navigationCtrl.navUnits[(0, 0, 0)];
        var distantGround = distant.dirGroundY;
        var distantHeights = distant.dirMaxY;
        var distantPassTypes = distant.passTypes;
        var first = new ObjectUnitForm.Data(25001, false, "", source.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        var second = new ObjectUnitForm.Data(25002, false, "", source.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;

        map.updateCtrl.RefreshObjectOverlap(first);
        Require(tile.passTypes.Count == 0
            && map.navigationCtrl.navUnits[(12, 0, 12)].passTypes.Count == 0,
            "Object covering a Tile center immediately applies default pass type to Tile and Nav");
        Require(ReferenceEquals(distantGround, distant.dirGroundY)
            && ReferenceEquals(distantHeights, distant.dirMaxY)
            && ReferenceEquals(distantPassTypes, distant.passTypes),
            "Object refresh does not rebuild distant Nav cells or terrain samples");

        map.updateCtrl.RefreshObjectOverlap(second);
        map.updateCtrl.RemoveObjectOverlap(first);
        Require(tile.passTypes.Count == 0
            && map.navigationCtrl.navUnits[(12, 0, 12)].passTypes.Count == 0,
            "removing one of two covering Objects keeps the Tile unrestricted");

        var destination = map.utilCtrl.GetTile(11, 0, 12);
        destination.SetPassTypes(new[] { 7002 });
        second.data.pos = destination.data.pos;
        map.updateCtrl.RefreshObjectOverlap(second);
        Require(tile.passTypes.SequenceEqual(new[] { 7001 })
            && destination.passTypes.Count == 0
            && map.navigationCtrl.navUnits[(12, 0, 12)].passTypes.SetEquals(new[] { 7001 })
            && map.navigationCtrl.navUnits[(11, 0, 12)].passTypes.Count == 0,
            "moving an Object restores old and refreshes new Tile/Nav coverage");

        map.updateCtrl.RemoveObjectOverlap(second);
        Require(destination.passTypes.SequenceEqual(new[] { 7002 })
            && map.navigationCtrl.navUnits[(11, 0, 12)].passTypes.SetEquals(new[] { 7002 }),
            "removing the last covering Object restores terrain pass type in Tile and Nav");

        var rotated = new ObjectUnitForm.Data(25003, false, "", source.name,
            new Vector3(11.4f, 0f, 12.4f), new Vector3(0f, 45f, 0f), Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        map.updateCtrl.RefreshObjectOverlap(rotated);
        Require(tile.passTypes.SequenceEqual(new[] { 7001 }),
            "rotated Object AABB overlap alone does not cover the Tile center");
        map.updateCtrl.RemoveObjectOverlap(rotated);

        var obstaclePrefab = Go("nav-obstacle");
        obstaclePrefab.AddComponent<BoxCollider>().center = Vector3.up * 0.5f;
        pool.AddPool(obstaclePrefab);
        var obstacle = new ObjectUnitForm.Data(25004, true, "", obstaclePrefab.name,
            new Vector3(6, 0, 6), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0);
        var nav = map.navigationCtrl.navUnits[(6, 0, 6)];
        var groundHeights = (float[])nav.dirMaxY.Clone();
        ObjectUnitForm.AddData(obstacle);
        map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
        Require(nav.objectBlocked && nav.links.Count == 0,
            "creating a tall obstacle immediately blocks the entire Nav cell");
        RequireLocalNavMatchesFullBuild("obstacle creation");

        var stationary = new ObjectUnitForm.Data(25005, true, "", obstaclePrefab.name,
            new Vector3(6, 0, 6), Vector3.zero, new Vector3(0.8f, 0.7f, 0.8f),
            UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.AddData(stationary);
        map.updateCtrl.RefreshObjectOverlap(stationary.unit);
        foreach (var position in new[] { new Vector3(6.1f, 0, 6), new Vector3(7.2f, 0, 6.3f), new Vector3(7, 1.5f, 6) })
        {
            obstacle.pos = position;
            obstacle.euler = new Vector3(0, 31, 0);
            obstacle.scale = new Vector3(2.2f, 2.4f, 1.4f);
            map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
            RequireLocalNavMatchesFullBuild("overlapping obstacle move/rotation/scale " + position);
        }
        var rebuildMethod = typeof(Z_Map.Analysis.NavigationController).GetMethod(
            "UpdateInternal", BindingFlags.Instance | BindingFlags.NonPublic);
        var incremental = (System.Collections.IEnumerator)rebuildMethod.Invoke(map.navigationCtrl, new object[] { 64 });
        incremental.MoveNext();
        incremental.MoveNext();
        obstacle.pos = new Vector3(8.2f, 0, 5.8f);
        map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
        while (incremental.MoveNext()) { }
        RequireLocalNavMatchesFullBuild("Object moved during periodic Nav rebuild");
        Require(!map.navigationCtrl.IsBaseWalkable(6, 0, 6),
            "local Object updates preserve solid-terrain blockage");
        ObjectUnitForm.RemoveData(obstacle.uid);
        map.updateCtrl.RemoveObjectOverlap(obstacle.unit);
        RequireLocalNavMatchesFullBuild("moving obstacle removal with stationary overlap");
        ObjectUnitForm.RemoveData(stationary.uid);
        map.updateCtrl.RemoveObjectOverlap(stationary.unit);
        Require(!nav.objectBlocked && nav.dirMaxY.Where((height, index) => Mathf.Abs(height - groundHeights[index]) < 0.0001f).Count() == 4,
            "removing an obstacle clears whole-cell blockage and preserves slope ground heights");
        tile.SetPassTypes(null);
        destination.SetPassTypes(null);
        solidTile.data.prefabName = solidOriginalPrefab;
        solidTile.InvalidateCollisionGeometry();
    }

    private static void SavedBridgePassTypeCoverage()
    {
        // Numeric snapshot of local story 2's bridge 1: root (516,750,497),
        // model base Y=-0.3 and size (2,0.1,1). Do not load/change the real save.
        var fixtureTiles = new List<TileUnitForm.Data>();
        void AddWaterTile(int x, int y, int z)
        {
            var tile = new TileUnitForm.Data(26000 + fixtureTiles.Count, "",
                new Dictionary<int, int>(), new Vector3Int(x, y, z), source.name,
                new Vector3(x, y * 1.5f, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int> { 2 });
            tile.unit.SetPassTypes(tile.passType);
            map.data.RegisterMap(tile);
            TileUnitForm.AddData(tile);
            fixtureTiles.Add(tile);
        }
        for (int x = 514; x <= 518; x++)
        for (int z = 496; z <= 498; z++)
            AddWaterTile(x, 500, z);
        AddWaterTile(516, 501, 497);

        var prefab = Go("saved-bridge-1");
        prefab.AddComponent<ObjectInstance>();
        var model = Go("bridge-model").transform;
        model.SetParent(prefab.transform, false);
        model.localPosition = new Vector3(0, -0.3f + 0.5f, 0);
        model.localScale = new Vector3(2, 0.1f, 1);
        pool.AddPool(prefab);
        var saved = new ObjectUnitForm.Data(26020, false, "bridge 1", prefab.name,
            new Vector3(516, 750, 497), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0);
        var bridge = ObjectUnitForm.GetDataByJo(ObjectUnitForm.GetJoByData(saved));
        ObjectUnitForm.AddData(bridge);
        // Scene-entry order: restore overlap/Tile state before the full Nav build.
        map.updateCtrl.RefreshObjectOverlap(bridge.unit);
        map.navigationCtrl.RebuildNow();
        var character = new CharacterUnitForm.Data(26021, false, Vector3.zero,
            1, 1, 1, false, "", source.name, bridge.pos, Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>()).unit;
        var bfs = new Z_Map.Analysis.Bfs(map.navigationCtrl);
        var footprint = new List<Vector2Int> { Vector2Int.zero };
        for (int x = 515; x <= 517; x++)
        {
            var tile = map.utilCtrl.GetTile(x, 500, 497);
            Require(tile.passTypes.Count == 0
                && map.navigationCtrl.navUnits[(x, 500, 497)].passTypes.Count == 0
                && character.CanPass(tile),
                "loaded thin/down-offset bridge clears Tile/Nav/manual pass type at " + x);
        }
        Require(bfs.CanPass(map.navigationCtrl.navUnits[(515, 500, 497)],
            map.navigationCtrl.navUnits[(516, 500, 497)], footprint, Array.Empty<int>()),
            "character without water pass type can navigate across the loaded bridge");
        Require(map.utilCtrl.GetTile(516, 501, 497).passTypes.Contains(2)
            && map.utilCtrl.GetTile(516, 500, 496).passTypes.Contains(2)
            && map.utilCtrl.GetTile(514, 500, 497).passTypes.Contains(2),
            "bridge upward probe leaves other floors and horizontally uncovered water restricted");

        bridge.pos += Vector3.forward;
        map.updateCtrl.RefreshObjectOverlap(bridge.unit);
        Require(map.utilCtrl.GetTile(516, 500, 497).passTypes.Contains(2)
            && map.navigationCtrl.navUnits[(516, 500, 497)].passTypes.Contains(2)
            && map.utilCtrl.GetTile(516, 500, 498).passTypes.Count == 0
            && map.navigationCtrl.navUnits[(516, 500, 498)].passTypes.Count == 0,
            "moving the saved bridge restores old water and clears new water locally");
        bridge.pos -= Vector3.forward;
        bridge.euler = new Vector3(0, 90, 0);
        map.updateCtrl.RefreshObjectOverlap(bridge.unit);
        Require(map.utilCtrl.GetTile(516, 500, 496).passTypes.Count == 0
            && map.utilCtrl.GetTile(515, 500, 497).passTypes.Contains(2),
            "rotated thin bridge uses its exact body, not only the AABB");
        RequireLocalNavMatchesFullBuild("saved bridge movement/rotation");
        ObjectUnitForm.RemoveData(bridge.uid);
        map.updateCtrl.RemoveObjectOverlap(bridge.unit);
        Require(fixtureTiles.All(tile => tile.unit.passTypes.Contains(2)
            && map.navigationCtrl.navUnits[(tile.mapPos.x, tile.mapPos.y, tile.mapPos.z)].passTypes.Contains(2)),
            "removing the saved bridge immediately restores all water requirements");
        foreach (var tile in fixtureTiles)
        {
            map.data.UnRegisterMap(tile);
            TileUnitForm.RemoveData(tile.uid);
            map.navigationCtrl.navUnits.Remove((tile.mapPos.x, tile.mapPos.y, tile.mapPos.z));
        }
    }

    private static void ObjectTileUpwardProbeRange()
    {
        var tile = map.utilCtrl.GetTile(10, 0, 10);
        tile.SetPassTypes(new[] { 7004 });
        map.navigationCtrl.RebuildNow();
        var distant = map.navigationCtrl.navUnits[(0, 0, 0)];
        var distantGround = distant.dirGroundY;
        var distantPassTypes = distant.passTypes;
        var prefab = Go("probe-body");
        prefab.AddComponent<ObjectInstance>();
        var model = Go("thin-probe-model").transform;
        model.SetParent(prefab.transform, false);
        model.localScale = new Vector3(1, 0.1f, 1);
        pool.AddPool(prefab);
        var obj = new ObjectUnitForm.Data(26030, false, "", prefab.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        foreach (var sample in new[]
        {
            (centerY: 0.05f, covered: true), // body bottom touches probe start 0
            (centerY: 0.7f, covered: true),  // above the old 0.5 limit
            (centerY: 1.05f, covered: true), // body bottom touches probe end 1
            (centerY: 1.06f, covered: false),
            (centerY: -0.05f, covered: true), // body top touches probe start 0
            (centerY: -0.06f, covered: false)
        })
        {
            model.localPosition = Vector3.up * sample.centerY;
            map.updateCtrl.RefreshObjectOverlap(obj.unit);
            Require((tile.passTypes.Count == 0) == sample.covered
                && (map.navigationCtrl.navUnits[(10, 0, 10)].passTypes.Count == 0) == sample.covered,
                "upward probe includes exactly 0..1 world units at model Y=" + sample.centerY);
        }
        model.localScale = new Vector3(2, 0.1f, 1);
        model.localEulerAngles = new Vector3(0, 0, 45);
        model.localPosition = new Vector3(-0.6f, 1.2f, 0);
        map.updateCtrl.RefreshObjectOverlap(obj.unit);
        Require(tile.passTypes.Contains(7004),
            "tilted body AABB reaches probe but actual body at center column is above 1");
        model.localPosition = new Vector3(0.6f, 1.2f, 0);
        map.updateCtrl.RefreshObjectOverlap(obj.unit);
        Require(tile.passTypes.Count == 0,
            "tilted body crossing the middle of upward probe clears the requirement");
        model.localPosition = Vector3.up * 0.5f;
        model.localEulerAngles = Vector3.zero;
        model.localScale = new Vector3(1, 0, 1);
        map.updateCtrl.RefreshObjectOverlap(obj.unit);
        Require(tile.passTypes.Count == 0,
            "zero-thickness model crossing upward probe is detected without an inverse matrix");
        map.updateCtrl.RemoveObjectOverlap(obj.unit);

        var abovePrefab = Go("probe-above-physical-body");
        abovePrefab.AddComponent<BoxCollider>().center = Vector3.up * 0.5f;
        pool.AddPool(abovePrefab);
        var above = new ObjectUnitForm.Data(26031, false, "", abovePrefab.name,
            tile.data.pos + Vector3.up * 0.99f, Vector3.zero, new Vector3(4, 0.02f, 1),
            UpdateType.ShowOnly, new List<int>(), "", false, 0);
        var neighbour = map.utilCtrl.GetTile(9, 0, 10);
        neighbour.SetPassTypes(new[] { 7005 });
        Require(!map.utilCtrl.GetVisionOverlap(above).Contains(neighbour),
            "probe-only neighbour is not in the Object visual/owner index");
        map.updateCtrl.RefreshObjectOverlap(above.unit);
        Require(neighbour.passTypes.Count == 0
            && map.navigationCtrl.navUnits[(9, 0, 10)].passTypes.Count == 0,
            "upward probe independently finds reachable Object above neighbouring Tile");
        map.updateCtrl.RemoveObjectOverlap(above.unit);
        Require(tile.passTypes.Contains(7004) && neighbour.passTypes.Contains(7005)
            && map.navigationCtrl.navUnits[(9, 0, 10)].passTypes.Contains(7005),
            "removing probe-only coverage restores Tile and Nav requirements");
        Require(ReferenceEquals(distantGround, distant.dirGroundY)
            && ReferenceEquals(distantPassTypes, distant.passTypes),
            "upward-probe refreshes do not rebuild distant Nav or terrain samples");
        tile.SetPassTypes(null);
        neighbour.SetPassTypes(null);
    }

    private static void BridgeObstacleNavigation()
    {
        // Play overwrites saved isObstacle=false with product.collision=true.
        // Reproduce both complete bridge corridors, including their dry-bank caps.
        var tiles = new List<TileUnitForm.Data>();
        var bridges = new List<ObjectUnitForm.Data>();
        var flatPrefab = Go("bridge-nav-flat-tile");
        pool.AddPool(flatPrefab);
        void AddTile(int x, int z, bool water, int y = 500)
        {
            var requirements = water ? new List<int> { 2 } : new List<int>();
            var tile = new TileUnitForm.Data(27000 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), flatPrefab.name, new Vector3(x, y * 1.5f, z),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "",
                false, false, requirements);
            tile.unit.SetPassTypes(requirements);
            map.data.RegisterMap(tile);
            TileUnitForm.AddData(tile);
            tiles.Add(tile);
        }
        for (int x = 514; x <= 518; x++)
        for (int z = 494; z <= 500; z++)
            AddTile(x, z, z == 497 || z == 498);
        for (int x = 508; x <= 514; x++)
        for (int z = 490; z <= 493; z++)
            AddTile(x, z, x == 511 || x == 512);
        AddTile(516, 497, false, 501);
        AddTile(517, 497, false, 501);
        GameObject BridgePrefab(string name, Vector3 size)
        {
            var prefab = Go(name);
            prefab.AddComponent<ObjectInstance>();
            var model = Go(name + "-model");
            model.transform.SetParent(prefab.transform, false);
            model.transform.localPosition = Vector3.up * 0.2f;
            model.transform.localScale = size;
            model.AddComponent<BoxCollider>();
            var trigger = model.AddComponent<BoxCollider>();
            trigger.isTrigger = true;
            trigger.size = Vector3.one + new Vector3(0.02f / size.x, 0.02f / size.y, 0.02f / size.z);
            pool.AddPool(prefab);
            return prefab;
        }
        var bridge1 = BridgePrefab("nav-bridge-1", new Vector3(2, 0.1f, 1));
        var bridge2 = BridgePrefab("nav-bridge-2", new Vector3(1, 0.1f, 2));
        void AddBridge(GameObject prefab, Vector3 position)
        {
            var obj = new ObjectUnitForm.Data(27100 + bridges.Count, false, "", prefab.name,
                position, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
            ObjectUnitForm.AddData(obj);
            map.updateCtrl.RefreshObjectOverlap(obj.unit);
            obj.isObstacle = true; // Play scene correction after the Map has begun.
            map.updateCtrl.RefreshObjectOverlap(obj.unit);
            bridges.Add(obj);
        }
        for (int z = 496; z <= 498; z++)
            AddBridge(bridge1, new Vector3(516, 750, z));
        for (int x = 510; x <= 513; x++)
            AddBridge(bridge2, new Vector3(x, 750, 492));
        map.navigationCtrl.Build();

        foreach (var obj in bridges)
        {
            var p = map.utilCtrl.RealPos2MapPosInt(obj.pos);
            var nav = map.navigationCtrl.navUnits[(p.x, p.y, p.z)];
            float physicalTop = obj.unit.GetMeshes(CollideType.CollideOnly).Max(mesh => mesh.GetMaxY());
            Require(!nav.objectBlocked && nav.dirMaxY.All(height => Mathf.Abs(height - 750f) < 0.0001f)
                && Mathf.Abs(physicalTop - 750.25f) < 0.0001f,
                "bridge below 0.3 stays passable without replacing Tile ground heights: " + obj.prefabName);
        }
        var bfs = new Z_Map.Analysis.Bfs(map.navigationCtrl);
        var predecessors = (Dictionary<Z_Map.Analysis.NavUnit, Z_Map.Analysis.NavUnit>)typeof(Z_Map.Analysis.Bfs)
            .GetField("pre", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(bfs);
        void RequireRoute(Vector3 from, Vector3 to, string name)
        {
            Vector3 expected = (to - from).normalized;
            Require(Vector3.Dot(bfs.GetNextDir(from, to, 100, 0.4f, Array.Empty<int>()), expected) > 0.99f,
                name + " navigates from dry bank through bridge to the opposite bank");
            var target = map.utilCtrl.RealPos2MapPosInt(to);
            Require(predecessors.ContainsKey(map.navigationCtrl.navUnits[(target.x, target.y, target.z)]),
                name + " actually reaches the opposite bank, not just the closest reachable cell");
            Require(Vector3.Dot(bfs.GetNextDir(to, from, 100, 0.4f, Array.Empty<int>()), -expected) > 0.99f,
                name + " also navigates in the opposite direction");
            target = map.utilCtrl.RealPos2MapPosInt(from);
            Require(predecessors.ContainsKey(map.navigationCtrl.navUnits[(target.x, target.y, target.z)]),
                name + " actually reaches the starting bank in reverse");
        }
        RequireRoute(new Vector3(516, 750, 495), new Vector3(516, 750, 499), "bridge 1");
        RequireRoute(new Vector3(509, 750, 492), new Vector3(514, 750, 492), "bridge 2");
        bridges[1].pos += Vector3.right;
        map.updateCtrl.RefreshObjectOverlap(bridges[1].unit);
        RequireLocalNavMatchesFullBuild("bridge collider local movement");

        // Physical bodies can be offset outside both visual and pass-probe cells.
        // Reapplying an overlapping body's height must use the physical index.
        var offsetPrefab = Go("nav-offset-body");
        var offsetCollider = offsetPrefab.AddComponent<BoxCollider>();
        offsetCollider.center = new Vector3(1, 1.75f, 3);
        offsetCollider.size = new Vector3(0.8f, 0.5f, 0.8f);
        pool.AddPool(offsetPrefab);
        AddBridge(offsetPrefab, new Vector3(516, 750, 494));
        var offsetObject = bridges[bridges.Count - 1];
        var offsetTile = map.data.maps[(517, 501, 497)].unit;
        Require(!map.updateCtrl.objectTileDic.Get(offsetObject.unit).Contains(offsetTile),
            "offset physical body is outside its visual/owner cells");
        Require(map.navigationCtrl.navUnits[(517, 501, 497)].objectBlocked,
            "offset Collider creation immediately refreshes its actual upper-layer cell");
        RequireLocalNavMatchesFullBuild("offset physical body creation");
        offsetObject.pos += Vector3.left;
        map.updateCtrl.RefreshObjectOverlap(offsetObject.unit);
        Require(!map.navigationCtrl.navUnits[(517, 501, 497)].objectBlocked
            && map.navigationCtrl.navUnits[(516, 501, 497)].objectBlocked,
            "offset Collider movement restores the old footprint and updates the new footprint");
        RequireLocalNavMatchesFullBuild("offset physical body movement");
        foreach (var obj in bridges)
        {
            ObjectUnitForm.RemoveData(obj.uid);
            map.updateCtrl.RemoveObjectOverlap(obj.unit);
        }
        RequireLocalNavMatchesFullBuild("bridge and offset physical body removal");
        var negativeMesh = Z_Mesh.Mesh.GetMesh(new Vector3(-2.2f, -1.2f, -3.6f), Vector3.zero,
            new Vector3(1, 0.1f, 1));
        Require(map.utilCtrl.TryGetObjectNavigationRange(negativeMesh, out var low, out var high)
            && low == new Vector3Int(-3, -1, -4) && high == new Vector3Int(-2, -1, -3),
            "navigation body range handles negative positions without world-integer truncation");
        var originalCellSize = map.data.mainData.mapUnitSize;
        try
        {
            map.data.mainData.mapUnitSize = new Vector3(2, 1.5f, 3);
            var scaledCellMesh = Z_Mesh.Mesh.GetMesh(new Vector3(-4, 1.5f, -6), Vector3.zero,
                new Vector3(3, 1, 4));
            Require(map.utilCtrl.TryGetObjectNavigationRange(scaledCellMesh, out low, out high)
                && low == new Vector3Int(-3, 1, -3) && high == new Vector3Int(-1, 1, -1),
                "navigation body range respects non-unit map cell dimensions");
        }
        finally
        {
            map.data.mainData.mapUnitSize = originalCellSize;
        }
        foreach (var tile in tiles)
        {
            map.data.UnRegisterMap(tile);
            TileUnitForm.RemoveData(tile.uid);
            map.navigationCtrl.navUnits.Remove((tile.mapPos.x, tile.mapPos.y, tile.mapPos.z));
        }
    }

    private static void WholeTileObjectNavigation()
    {
        var controller = map.navigationCtrl;
        var method = typeof(Z_Map.Analysis.NavigationController).GetMethod("IsObjectMeshBlocking", BindingFlags.Instance | BindingFlags.NonPublic);
        var blocks = (Func<MeshInfo, Z_Map.Analysis.NavUnit, bool>)Delegate.CreateDelegate(
            typeof(Func<MeshInfo, Z_Map.Analysis.NavUnit, bool>), controller, method);
        var node = new Z_Map.Analysis.NavUnit
        {
            realPos = Vector3.zero, dirGroundY = new float[4]
        };
        MeshInfo Body(Vector3 center, Vector3 size, Vector3 rotation = default) => Z_Mesh.Mesh.GetMesh(center, rotation, size);
        foreach (float top in new[] { 0.25f, 0.299f, 0.3f, 0.3002f, 0.31f })
        {
            var body = Body(new Vector3(0.4f, top * 0.5f, 0.4f), new Vector3(0.1f, top, 0.1f));
            Require(blocks(body, node) == (top > 0.3f), "whole-cell corner body threshold " + top);
        }
        Require(blocks(Body(new Vector3(0, 0.5f, 0), new Vector3(0.05f, 1, 0.05f)), node),
            "small center body missed by all four old samples blocks whole cell");
        Require(!blocks(Body(new Vector3(0.6f, 0.5f, 0), new Vector3(0.2f, 1, 0.2f)), node),
            "body only touching cell edge does not occupy its area");
        Require(!blocks(Body(new Vector3(0, 0.5f, 0), new Vector3(0, 1, 1)), node),
            "zero horizontal collisionScale does not block navigation");
        Require(!blocks(Body(new Vector3(0, -0.5f, 0), Vector3.one), node),
            "body below ground does not block navigation");
        Require(!blocks(Body(new Vector3(0.9f, 0.5f, 0.9f), new Vector3(1.5f, 1, 0.05f), new Vector3(0, 45, 0)), node),
            "rotated AABB reaching a corner does not imply physical overlap");
        // High end is outside this Tile; only the clipped body's local height counts.
        var tilted = Body(new Vector3(0.7f, 0.1f, 0), new Vector3(1, 0.01f, 0.2f), new Vector3(0, 0, 45));
        Require(tilted.GetMaxY() > 0.3f && !blocks(tilted, node),
            "Object top outside Tile does not block a low intersecting portion");
        node.dirGroundY = new[] { 0.25f, -0.25f, 0f, 0f }; // y=x plane
        Require(!blocks(Body(new Vector3(0.4f, 0.3f, 0), new Vector3(0.1f, 0.2f, 0.1f)), node),
            "height measured against slope ground under actual body, not Tile minimum");
        Require(blocks(Body(new Vector3(-0.4f, -0.1f, 0), new Vector3(0.1f, 0.2f, 0.1f)), node),
            "low-side slope body exceeding relative threshold blocks whole cell");
        node.realPos = new Vector3(500, 751.5f, 500);
        node.dirGroundY = Enumerable.Repeat(751.5f, 4).ToArray();
        Require(!blocks(Body(node.realPos + new Vector3(0, 0.15f, 0), new Vector3(0.1f, 0.3f, 0.1f)), node),
            "0.3 equality survives large-world float rounding");
        node.realPos = Vector3.zero;
        node.dirGroundY = new float[4];
        Require(blocks(Z_Mesh.Mesh.GetMesh(new Vector3(0.4f, 0.3f, 0.4f), 0.1f, Vector3.zero, Vector3.one), node),
            "Sphere corner body blocks entire cell");
        Require(!blocks(Z_Mesh.Mesh.GetMesh(new Vector3(0.65f, 0.4f, 0.65f), 0.2f, Vector3.zero, Vector3.one), node),
            "Sphere AABB corner without actual overlap is not blocked");
        var allocationBody = Body(new Vector3(0.4f, 0.5f, 0.4f), new Vector3(0.1f, 1, 0.1f));
        for (int i = 0; i < 20; i++) blocks(allocationBody, node);
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) blocks(allocationBody, node);
        Require(GC.GetAllocatedBytesForCurrentThread() == allocationStart,
            "whole-cell body clipping allocates no per-cell temporary geometry");

        // Dedicated flat terrain; the general geometry fixture deliberately uses
        // a rotated/sloped prefab and is not a zero-height ground plane.
        var flat = Go("whole-cell-flat-terrain");
        pool.AddPool(flat);
        var fixtureTiles = new List<TileUnitForm.Data>();
        for (int x = 610; x <= 613; x++)
        for (int z = 610; z <= 613; z++)
        {
            var data = new TileUnitForm.Data(28120 + fixtureTiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, 500, z), flat.name, new Vector3(x, 750, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(data);
            TileUnitForm.AddData(data);
            fixtureTiles.Add(data);
        }
        controller.RebuildNow();
        var tile = map.utilCtrl.GetTile(611, 500, 611);
        var nav = controller.navUnits[(611, 500, 611)];
        var from = controller.navUnits[(611, 500, 610)];
        var far = controller.navUnits[(0, 0, 0)];
        var farGround = far.dirGroundY;
        var farLinks = far.links;
        var prefab = Go("whole-tile-corner-obstacle");
        var collider = prefab.AddComponent<BoxCollider>();
        collider.center = new Vector3(0.4f, 0.5f, 0.4f);
        collider.size = new Vector3(0.1f, 1, 0.1f);
        var trigger = prefab.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = Vector3.one * 10;
        pool.AddPool(prefab);
        var first = new ObjectUnitForm.Data(28101, true, "", prefab.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        var second = new ObjectUnitForm.Data(28102, true, "", prefab.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.AddData(first);
        ObjectUnitForm.AddData(second);
        try
        {
            map.updateCtrl.RefreshObjectOverlap(first.unit);
            map.updateCtrl.RefreshObjectOverlap(second.unit);
            Require(nav.objectBlocked && nav.links.Count == 0 && !from.links.Contains(nav)
                && !controller.IsBaseWalkable(611, 500, 611), "corner obstacle removes outgoing and incoming links immediately");
            var bfs = new Z_Map.Analysis.Bfs(controller);
            Require(!bfs.CanPass(from, nav, new List<Vector2Int> { Vector2Int.zero }, Array.Empty<int>()),
                "BFS cannot enter whole-cell obstacle from another direction");
            var endpointState = new Z_Map.Analysis.NavigationEndpointState();
            Vector3 escapeDirection = bfs.GetNextDir(tile.data.pos + Vector3.right * 0.1f,
                tile.data.pos + Vector3.right * 0.2f, 100, 0f, Array.Empty<int>(), endpointState);
            Require(!bfs.CanPass(nav) && endpointState.directEndpoint
                && escapeDirection != Vector3.zero
                && endpointState.moveTarget != tile.data.pos + Vector3.right * 0.2f,
                "blocked same-cell query first escapes to a walkable cell rather than bypassing the obstacle");
            RequireLocalNavMatchesFullBuild("whole-cell corner obstacle");
            farGround = far.dirGroundY;
            farLinks = far.links;
            ObjectUnitForm.RemoveData(first.uid);
            map.updateCtrl.RemoveObjectOverlap(first.unit);
            Require(nav.objectBlocked, "removing one overlap preserves other obstacle blockage");
            second.pos += Vector3.right;
            map.updateCtrl.RefreshObjectOverlap(second.unit);
            Require(!nav.objectBlocked && from.links.Contains(nav) && controller.navUnits[(612, 500, 611)].objectBlocked,
                "movement restores old cell and blocks new cell without full rebuild");
            Require(ReferenceEquals(farGround, far.dirGroundY) && ReferenceEquals(farLinks, far.links),
                "local object update preserves distant ground and link containers");
            RequireLocalNavMatchesFullBuild("whole-cell movement");
            second.scale = new Vector3(0, 1, 0);
            map.updateCtrl.UpdateSingleOne(second.unit);
            Require(!controller.navUnits[(612, 500, 611)].objectBlocked,
                "zero collision footprint immediately restores Nav cell");
            second.scale = Vector3.one;
            map.updateCtrl.UpdateSingleOne(second.unit);
            Require(controller.navUnits[(612, 500, 611)].objectBlocked,
                "restoring physical footprint immediately restores blockage");
            collider.size = new Vector3(0.1f, 0.3f, 0.1f);
            collider.center = new Vector3(0.4f, 0.15f, 0.4f);
            map.updateCtrl.UpdateSingleOne(second.unit);
            Require(!controller.navUnits[(612, 500, 611)].objectBlocked, "0.3 equality becomes passable despite enlarged Trigger");
            RequireLocalNavMatchesFullBuild("whole-cell threshold equality");
            var spherePrefab = Go("whole-tile-sphere-obstacle");
            spherePrefab.AddComponent<SphereCollider>().center = new Vector3(0.4f, 0.5f, 0.4f);
            pool.AddPool(spherePrefab);
            second.prefabName = spherePrefab.name;
            map.updateCtrl.UpdateSingleOne(second.unit);
            Require(controller.navUnits[(612, 500, 611)].objectBlocked, "Sphere enters physical navigation index and blocks immediately");
            RequireLocalNavMatchesFullBuild("whole-cell sphere");
            second.scale = Vector3.one * 0.25f;
            map.updateCtrl.UpdateSingleOne(second.unit);
            Require(!controller.navUnits[(612, 500, 611)].objectBlocked,
                "scaled Sphere top below 0.3 updates its old physical coverage");
            RequireLocalNavMatchesFullBuild("whole-cell scaled Sphere");
        }
        finally
        {
            if (ObjectUnitForm.DataByUid.ContainsKey(first.uid)) ObjectUnitForm.RemoveData(first.uid);
            map.updateCtrl.RemoveObjectOverlap(first.unit);
            ObjectUnitForm.RemoveData(second.uid);
            map.updateCtrl.RemoveObjectOverlap(second.unit);
        }
        Require(!controller.navUnits[(612, 500, 611)].objectBlocked, "removal restores whole-cell navigation");
        RequireLocalNavMatchesFullBuild("whole-cell final removal");
        foreach (var data in fixtureTiles)
        {
            map.data.UnRegisterMap(data);
            TileUnitForm.RemoveData(data.uid);
            controller.navUnits.Remove((data.mapPos.x, data.mapPos.y, data.mapPos.z));
        }
    }

    private static void ObjectCollisionScalePassTypeCoverage()
    {
        var tile = map.utilCtrl.GetTile(6, 0, 10);
        var neighbour = map.utilCtrl.GetTile(5, 0, 10);
        tile.SetPassTypes(new[] { 7101 });
        neighbour.SetPassTypes(new[] { 7102 });
        map.navigationCtrl.RebuildNow();
        var farGround = map.navigationCtrl.navUnits[(0, 0, 0)].dirGroundY;
        var farPass = map.navigationCtrl.navUnits[(0, 0, 0)].passTypes;

        var prefab = Go("collision-scale-pass-body");
        prefab.AddComponent<ObjectInstance>();
        var model = Go("collision-scale-model");
        model.transform.SetParent(prefab.transform, false);
        model.transform.localPosition = new Vector3(-0.3f, 0.2f, 0);
        model.transform.localScale = new Vector3(2, 0.1f, 1);
        var body = Go("collision-scale-collider");
        body.transform.SetParent(model.transform, false);
        body.AddComponent<BoxCollider>();
        var trigger = body.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = new Vector3(10, 10, 10);
        var inactiveBody = Go("authored-disabled-collider");
        inactiveBody.transform.SetParent(prefab.transform, false);
        inactiveBody.AddComponent<BoxCollider>().size = Vector3.one * 20;
        inactiveBody.SetActive(false);
        prefab.SetActive(false);
        pool.AddPool(prefab);
        var obj = new ObjectUnitForm.Data(28001, false, "", prefab.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.AddData(obj);
        var character = new CharacterUnitForm.Data(28002, false, Vector3.zero, 1, 1, 1, false, "",
            source.name, tile.data.pos, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, 0, new List<int>()).unit;
        var bfs = new Z_Map.Analysis.Bfs(map.navigationCtrl);
        var footprint = new List<Vector2Int> { Vector2Int.zero };
        try
        {
            foreach (float scale in new[] { 1f, 0.25f, 0.5f, 0f, 1f })
            {
                // Same nested Collider transform produced by MapModel.colliderScale.
                body.transform.localScale = new Vector3(scale, 1, scale);
                map.updateCtrl.UpdateSingleOne(obj.unit);
                Require(obj.unit.GetMeshes(CollideType.CollideOnly).Count == 1,
                    "inactive prefab root retains physical body but excludes authored disabled child");
                bool centerCovered = scale >= 0.5f;
                bool neighbourCovered = scale == 1f;
                Require(map.utilCtrl.IsTileCenterCoveredByObject(tile, obj) == centerCovered
                    && map.utilCtrl.IsTileCenterCoveredByObject(neighbour, obj) == neighbourCovered,
                    "collisionScale controls exact body coverage without Trigger/visual expansion: " + scale);
                Require((tile.passTypes.Count == 0) == centerCovered
                    && (neighbour.passTypes.Count == 0) == neighbourCovered
                    && (map.navigationCtrl.navUnits[(6, 0, 10)].passTypes.Count == 0) == centerCovered
                    && (map.navigationCtrl.navUnits[(5, 0, 10)].passTypes.Count == 0) == neighbourCovered,
                    "collisionScale change immediately updates old/new Tile and Nav pass types");
                Require(character.CanPass(tile) == centerCovered
                    && bfs.CanPass(map.navigationCtrl.navUnits[(6, 0, 9)], map.navigationCtrl.navUnits[(6, 0, 10)],
                        footprint, Array.Empty<int>()) == centerCovered,
                    "manual movement and navigation share the scaled pass requirement");
                Require(ReferenceEquals(farGround, map.navigationCtrl.navUnits[(0, 0, 0)].dirGroundY)
                    && ReferenceEquals(farPass, map.navigationCtrl.navUnits[(0, 0, 0)].passTypes),
                    "collisionScale refresh does not rebuild distant Nav cells");
                Require(map.utilCtrl.TryGetVisionBounds(obj, out var visual) && visual.size.x > 1.9f,
                    "collisionScale never shrinks visual bounds");
            }
            RequireLocalNavMatchesFullBuild("collisionScale pass update");
            obj.pos += Vector3.right * 0.3f;
            body.transform.localScale = new Vector3(0, 1, 0);
            map.updateCtrl.UpdateSingleOne(obj.unit);
            Require(!map.utilCtrl.IsTileCenterCoveredByObject(tile, obj) && tile.passTypes.Contains(7101),
                "collisionScale zero does not override pass types even directly above the probe");
            body.transform.localScale = new Vector3(0.25f, 1, 0.25f);
            map.updateCtrl.UpdateSingleOne(obj.unit);
            Require(tile.passTypes.Count == 0 && neighbour.passTypes.Contains(7102),
                "small shifted Collider covers the actual center only");
            obj.euler = new Vector3(0, 90, 0);
            map.updateCtrl.UpdateSingleOne(obj.unit);
            Require(map.utilCtrl.GetObjectPassTypeOverlap(obj).All(t => map.utilCtrl.IsTileCenterCoveredByObject(t, obj)),
                "rotated scaled body index contains only exact probe hits");
            map.RemoveObject(obj);
            Require(tile.passTypes.Contains(7101) && neighbour.passTypes.Contains(7102)
                && map.navigationCtrl.navUnits[(6, 0, 10)].passTypes.Contains(7101),
                "removing a scaled body restores original terrain pass requirements");
        }
        finally
        {
            if (ObjectUnitForm.DataByUid.ContainsKey(obj.uid)) map.RemoveObject(obj);
            tile.SetPassTypes(null);
            neighbour.SetPassTypes(null);
        }

        var spherePrefab = Go("collision-scale-sphere");
        var sphereBody = Go("collision-scale-sphere-body");
        sphereBody.transform.SetParent(spherePrefab.transform, false);
        sphereBody.transform.localPosition = new Vector3(-0.3f, 0.5f, 0);
        sphereBody.AddComponent<SphereCollider>().radius = 0.5f;
        var sphereTrigger = sphereBody.AddComponent<SphereCollider>();
        sphereTrigger.radius = 10;
        sphereTrigger.isTrigger = true;
        spherePrefab.SetActive(false);
        pool.AddPool(spherePrefab);
        var sphere = new ObjectUnitForm.Data(28003, false, "", spherePrefab.name, tile.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.AddData(sphere);
        tile.SetPassTypes(new[] { 7101 });
        try
        {
            foreach (var sample in new[] { (scale: 1f, covered: true), (scale: 0.5f, covered: false), (scale: 0.6f, covered: true) })
            {
                sphereBody.transform.localScale = Vector3.one * sample.scale;
                map.updateCtrl.UpdateSingleOne(sphere.unit);
                Require(map.utilCtrl.IsTileCenterCoveredByObject(tile, sphere) == sample.covered
                    && (tile.passTypes.Count == 0) == sample.covered,
                    "sphere collisionScale uses actual radius, excluding enlarged Trigger");
            }
            sphere.pos += Vector3.right * 0.3f;
            sphereBody.transform.localScale = Vector3.zero;
            map.updateCtrl.UpdateSingleOne(sphere.unit);
            Require(!map.utilCtrl.IsTileCenterCoveredByObject(tile, sphere) && tile.passTypes.Contains(7101),
                "zero-radius sphere directly on probe does not override terrain requirements");
        }
        finally
        {
            map.RemoveObject(sphere);
            tile.SetPassTypes(null);
        }
    }

    private static void ObjectMarksAndMissionCoordinates()
    {
        var markPrefab = Go("MapPrefab$mapMark");
        markPrefab.AddComponent<MeshRenderer>();
        markPrefab.AddComponent<BoxCollider>();
        markPrefab.transform.position = new Vector3(334, 130, 95);
        markPrefab.transform.rotation = Quaternion.Euler(90, 0, 0);
        var objectPrefab = Go("marker-owner-template");
        var model = Go("marker-owner-model");
        model.transform.SetParent(objectPrefab.transform, false);
        model.AddComponent<MeshRenderer>();
        model.AddComponent<BoxCollider>();
        objectPrefab.AddComponent<ObjectInstance>();
        objectPrefab.SetActive(false);
        var instancePool = new InstancePool(objectPrefab, Go("marker-live-instance-root").transform);
        var root = instancePool.Get();
        var ins = root.GetComponent<ObjectInstance>();
        // This fixture is EditMode; drive the runtime MonoBehaviour callbacks
        // explicitly, as Unity does for enabled/disabled instances in Play.
        var onEnable = typeof(ObjectInstance).GetMethod("OnEnable", BindingFlags.Instance | BindingFlags.NonPublic);
        var onDisable = typeof(ObjectInstance).GetMethod("OnDisable", BindingFlags.Instance | BindingFlags.NonPublic);
        onEnable.Invoke(ins, null);
        var originalRenderers = ins.renderers;
        try
        {
            ins.EnsureMapMark(markPrefab, false);
            var mark = root.transform.Find(markPrefab.name);
            Require(mark != null && mark.parent == root.transform
                && mark.localScale == Vector3.one && mark.localPosition == Vector3.zero,
                "UGC marker attaches at Object root with unit local scale and no prefab world offset");
            Require(Quaternion.Angle(mark.localRotation, markPrefab.transform.localRotation) < 0.01f,
                "marker preserves authored plane rotation");
            Require(!mark.gameObject.activeSelf && !mark.GetComponent<Collider>().enabled,
                "map-edit marker hidden and never participates in physics");
            ins.EnsureMapMark(markPrefab, true);
            Require(root.transform.childCount == 2 && ReferenceEquals(ins.renderers, originalRenderers)
                && ins.renderers.Length == 1 && objectPrefab.transform.childCount == 1,
                "marker is unique, excludes model renderer slots, and never modifies source prefab geometry");
            Z_EventHelper.Invoke(new ObjectMarkVisibilityEvent { visible = false });
            Require(!mark.gameObject.activeSelf, "leaving Event mode hides marker through event");
            Z_EventHelper.Invoke(new ObjectMarkVisibilityEvent { visible = true });
            Require(mark.gameObject.activeSelf, "entering Event mode shows marker through event");
            instancePool.Push(root);
            onDisable.Invoke(ins, null);
            Require(!mark.gameObject.activeSelf, "pool disable hides marker and unregisters listener");
            Z_EventHelper.Invoke(new ObjectMarkVisibilityEvent { visible = true });
            Require(!mark.gameObject.activeSelf, "inactive pooled root ignores mode event");
            var reused = instancePool.Get();
            onEnable.Invoke(ins, null);
            Require(reused == root && !mark.gameObject.activeSelf && root.transform.childCount == 2,
                "pool restores original renderer slots without overflow or duplicate marker");
            ins.EnsureMapMark(markPrefab, true);
            Z_EventHelper.Invoke(new ObjectMarkVisibilityEvent { visible = false });
            Require(!mark.gameObject.activeSelf, "reused Object listener re-registers for mode changes");
            pool.AddPool(markPrefab);
            var mod = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
            typeof(ModSceneController).GetField("enable", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mod, true);
            mod.designType = DesignType.Event;
            Require(mark.gameObject.activeSelf, "ModScene mode setter publishes Event marker visibility");
            mod.layer = 2;
            Require(!mark.gameObject.activeSelf, "ModScene layer selection exits Event and hides marker");
            var objectData = new ObjectUnitForm.Data(29002, false, "", objectPrefab.name, Vector3.zero,
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
            objectData.unit.ins = ins;
            mod.designType = DesignType.Event;
            mod.OnEvent(new ObjectEvent { type = MapEventType.Show, unit = objectData.unit });
            Require(mark.gameObject.activeSelf && root.transform.childCount == 2,
                "Object Show in Event mode attaches/shows marker without duplication");
            typeof(ModSceneController).GetField("enable", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mod, false);
            mod.designType = DesignType.Event;
            Require(!mark.gameObject.activeSelf, "disabled ModScene never leaves markers in Play");
        }
        finally
        {
            onDisable.Invoke(ins, null);
            Object.DestroyImmediate(root);
        }

        var mission = new Form.MissionForm.Data(29001, "coordinates", 0, "", true, false, false, true,
            1, new Vector3(3, 2, -4), 0);
        var oldSize = map.data.mainData.mapUnitSize;
        try
        {
            foreach (var size in new[] { Vector3.one, new Vector3(2, 1.5f, 3) })
            {
                map.data.mainData.mapUnitSize = size;
                var world = map.utilCtrl.MapPos2RealPos(GameManager.PlayerPosToMapPos(mission.targetPos));
                Require(MissionGuide.GetTargetWorldPosition(mission) == world,
                    "mission target converts player coordinates with origin offset and map cell size");
                Require(Mathf.Abs(MissionGuide.GetDistance(mission, world)) < 0.0001f,
                    "standing at mission destination displays zero distance");
                Require(Mathf.Abs(MissionGuide.GetDistance(mission, world + new Vector3(3, 4, 0)) - 5) < 0.0001f,
                    "mission distance is Euclidean world meters, independent of map cell size");
            }
        }
        finally { map.data.mainData.mapUnitSize = oldSize; }
        MissionHudEverySecond(mission);
    }

    private static void MissionHudEverySecond(Form.MissionForm.Data mission)
    {
        Form.MissionForm.InitInternal();
        Form.ProgressForm.InitInternal();
        Form.SceneForm.InitInternal();
        Z_Text.Form.TextBaseForm.InitInternal();
        Z_Text.Form.TextBaseForm.AddData(new Z_Text.Form.TextBaseForm.Data(29001, "m", "m", "米"));
        Z_Text.Form.TextBaseForm.AddData(new Z_Text.Form.TextBaseForm.Data(29002, "go to ", "go to ", "前往"));
        mission.received = true;
        mission.fail = false;
        Form.MissionForm.AddData(mission);
        var progress = (Form.ProgressForm.Data)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Form.ProgressForm.Data));
        progress.uid = 1;
        progress.enableMission = true;
        progress.curMissionId = mission.id;
        Form.ProgressForm.AddData(progress);
        var scene = new Form.SceneForm.Data(1, "destination", 0, Vector2.zero, true, false,
            new Dictionary<string, Form.EventTriggerForm.Data>(), new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>>(), false);
        Form.SceneForm.AddData(scene);
        var gameGo = Go("mission-test-game");
        gameGo.SetActive(false);
        var game = gameGo.AddComponent<GameManager>();
        typeof(Z_MonoSingleton<GameManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, game);
        game.curScene = scene;
        var playGo = Go("mission-test-play");
        playGo.SetActive(false);
        var play = playGo.AddComponent<PlayManager>();
        typeof(Z_MonoSingleton<PlayManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, play);
        var sceneCtrl = new PlaySceneController(play);
        play.sceneCtrl = sceneCtrl;
        var player = new CharacterUnitForm.Data(29003, false, Vector3.zero, 1, 1, 1, false, "",
            source.name, MissionGuide.GetTargetWorldPosition(mission), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>());
        typeof(PlaySceneController).GetField("_playerM", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(sceneCtrl, player);
        var hudGo = Go("mission-test-hud");
        var hudHolder = hudGo.AddComponent<Z_Ui.Base.UiHolder>();
        var widgetGo = Go("mission-test-widget");
        widgetGo.transform.SetParent(hudGo.transform, false);
        var widgetHolder = widgetGo.AddComponent<Z_Ui.Base.UiHolder>();
        var distanceGo = Go("mission-test-distance");
        distanceGo.transform.SetParent(widgetGo.transform, false);
        // Test controller text writes without loading font assets or opening
        // TMP's resource-import window in this headless EditMode project.
        distanceGo.SetActive(false);
        var distance = distanceGo.AddComponent<Z_Ui.Base.Txt>();
        distance.languageTranslatable = false;
        var descGo = Go("mission-test-description");
        descGo.transform.SetParent(widgetGo.transform, false);
        descGo.SetActive(false);
        var desc = descGo.AddComponent<Z_Ui.Base.Txt>();
        desc.languageTranslatable = false;
        var widgetView = (Ui.PlaySceneMain.PlaySceneMission.UiPlaySceneMissionView)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Ui.PlaySceneMain.PlaySceneMission.UiPlaySceneMissionView));
        widgetView.txt_distance = distance;
        widgetView.txt_ = desc;
        var widget = new Ui.PlaySceneMain.PlaySceneMission.UiPlaySceneMissionCtrl
        { uiHolder = widgetHolder, inited = true, view = widgetView };
        var hudView = (Ui.PlaySceneMain.UiPlaySceneMainView)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(Ui.PlaySceneMain.UiPlaySceneMainView));
        hudView.page_PlaySceneMission = widget;
        var hud = new Ui.PlaySceneMain.UiPlaySceneMainCtrl { uiHolder = hudHolder, inited = true, view = hudView };
        hud.Register<StoryLifeEvent>();
        try
        {
            widget.Refresh();
            Require(distance.text == "0m", "HUD starts at correct zero distance rather than origin-offset distance");
            for (int second = 1; second <= 3; second++)
            {
                player.pos = MissionGuide.GetTargetWorldPosition(mission) + Vector3.right * second;
                Z_EventHelper.Invoke(new StoryLifeEvent { type = StoryLifeEventType.EverySecond });
                Require(distance.text == second + "m", "EverySecond updates HUD using current player data: " + second);
            }
            player.pos += Vector3.right;
            Z_EventHelper.Invoke(new StoryLifeEvent { type = StoryLifeEventType.Enter });
            Require(distance.text == "3m", "other lifecycle events do not refresh mission distance");
            var otherScene = new Form.SceneForm.Data(2, "other destination", 0, Vector2.zero, true, false,
                new Dictionary<string, Form.EventTriggerForm.Data>(), new Dictionary<int, List<string>>(),
                new Dictionary<int, List<string>>(), false);
            Form.SceneForm.AddData(otherScene);
            mission.targetSceneId = 2;
            Z_EventHelper.Invoke(new StoryLifeEvent { type = StoryLifeEventType.EverySecond });
            Require(distance.text == "go to other destination", "cross-scene mission shows destination rather than meaningless distance");
            mission.targetSceneId = 999;
            Z_EventHelper.Invoke(new StoryLifeEvent { type = StoryLifeEventType.EverySecond });
            Require(distance.text == "", "missing target scene clears stale distance");
            mission.received = false;
            hud.OnEvent(new MissionEvent { type = MissionEventType.Done, data = mission });
            Require(!widgetGo.activeSelf, "no available mission hides widget");
            mission.received = true;
            mission.targetSceneId = 1;
            hud.OnEvent(new MissionEvent { type = MissionEventType.Add, data = mission });
            Require(widgetGo.activeSelf && distance.text == "4m", "active parent restores hidden mission widget on mission event");
            hud.OnHide();
            player.pos += Vector3.right;
            Z_EventHelper.Invoke(new StoryLifeEvent { type = StoryLifeEventType.EverySecond });
            Require(distance.text == "4m", "HUD OnHide unregisters per-second listener");
        }
        finally { hud.Unregister<StoryLifeEvent>(); }
    }

    private static void EventOperatorRendering()
    {
        // Exercise the actual Unit controller's queued child params without loading
        // an authored panel or touching the real story/event registries.
        var root = Go("event-operator-parts");
        root.SetActive(false);
        var rect = root.AddComponent<RectTransform>();
        var holder = root.AddComponent<Z_Ui.Base.UiHolder>();
        holder.elementTrsLst = Enumerable.Repeat<Transform>(rect, 8).ToList();
        var unit = new Ui.ModStoryEventEditWindow.UiUnitCtrl
        {
            view = new Ui.ModStoryEventEditWindow.UiUnitView(holder),
            model = new Ui.ModStoryEventEditWindow.UiUnitModel()
        };
        unit.model.con = new Z_Ui.Base.UiContainer<Ui.ModStoryEventEditWindow.UiUnitCtrl>(unit, root, false);
        var render = unit.GetType().GetMethod("RenderOperator", BindingFlags.Instance | BindingFlags.NonPublic);
        var decompiler = new Z_Code.Decompiler();
        Z_Code.SyntaxNode Parse(string expression)
        {
            var errors = new List<Z_Code.CompileError>();
            var tokens = new Z_Code.LexicalAnalysis().Execute(expression + ";", errors);
            var nodes = new Z_Code.SyntaxAnalysis().Execute(tokens, errors);
            Require(errors.Count == 0 && nodes.Count == 1, "event operator fixture parses: " + expression);
            return nodes[0];
        }
        string Render(Z_Code.SyntaxNode node)
        {
            if (node.desc.type != Z_Code.CodeType.Operator)
                return decompiler.ResetStatement(node);
            unit.model.con.paramLst.Clear();
            unit.model.node = node;
            render.Invoke(unit, null);
            var parts = unit.model.con.paramLst.Cast<Ui.ModStoryEventEditWindow.UiUnitParam>().ToArray();
            foreach (var child in node.subNodes)
                Require(parts.Count(part => ReferenceEquals(part.node, child)) == 1,
                    "every operator operand remains an editable Unit exactly once: " + node.desc.code);
            Require(parts.All(part => part.parent == rect), "operator children stay inside their owning Unit");
            return string.Concat(parts.Select(part => part.node == null ? part.txt : Render(part.node)));
        }
        var cases = new[]
        {
            ("a&&b", "a && b"), ("a||b", "a || b"), ("!a", "!a"),
            ("a%b", "a % b"), ("-a", "-a"), ("+a", "+a"),
            ("i++", "i++"), ("a+=b", "a += b"),
            ("(a||b)&&c", "(a || b) && c"), ("a&&(b||c)", "a && (b || c)"),
            ("!(a&&b)", "!(a && b)"), ("-(a+b)", "-(a + b)"),
            ("a-(b-c)", "a - (b - c)"), ("a==\"\"", "a equal to \"\""),
            ("lst[i]%2", "(lst[i]) % 2")
        };
        foreach (var sample in cases)
        {
            var node = Parse(sample.Item1);
            string original = decompiler.ResetStatement(node);
            for (int i = 0; i < 2; i++)
                Require(Render(node) == sample.Item2, "event Unit renders complete grouped syntax: " + sample.Item1);
            Require(decompiler.ResetStatement(node) == original, "rendering leaves the stored syntax tree unchanged");
        }
        var enemyCondition = Parse("GetVectorLength(GetCharacterPosition(lst[i])-GetCharacterPosition(param1))"
            + "<GetCharacterParameter(param1,\"射程\")&&IsUnobstructed(GetCharacterPosition(lst[i]),GetCharacterPosition(param1))");
        string enemyText = Render(enemyCondition);
        Require(enemyText.Contains("GetVectorLength") && enemyText.Contains("射程")
            && enemyText.Contains(" && ") && enemyText.Contains("IsUnobstructed"),
            "enemy acquisition condition retains range and line-of-sight expressions in Item Units");

        // Even existing operators remain complete when their description is absent.
        var forms = Z_Code.Form.CmdDataForm.DataByName;
        var plus = forms["+"];
        forms.Remove("+");
        try
        {
            Require(Render(Parse("left+right")) == "left + right", "missing operator metadata preserves source order");
        }
        finally
        {
            forms["+"] = plus;
        }
        var call = Parse("Example(\"\",,a)");
        string originalCall = decompiler.ResetStatement(call);
        unit.model.node = call;
        unit.model.con.paramLst.Clear();
        var createNode = unit.GetType().GetMethod("CreateNode", BindingFlags.Instance | BindingFlags.NonPublic);
        foreach (var argument in call.subNodes)
            createNode.Invoke(unit, new object[] { argument });
        Require(unit.model.con.paramLst.Count == 1
            && ReferenceEquals(((Ui.ModStoryEventEditWindow.UiUnitParam)unit.model.con.paramLst[0]).node, call.subNodes[2]),
            "ordinary call arguments still hide empty strings and omitted slots");
        Require(decompiler.ResetStatement(call) == originalCall, "hidden call arguments remain in stored syntax");
    }

    private static void RequireLocalNavMatchesFullBuild(string context)
    {
        var local = map.navigationCtrl.navUnits.ToDictionary(pair => pair.Key, pair => (
            blocked: pair.Value.objectBlocked,
            heights: (float[])pair.Value.dirMaxY.Clone(),
            passTypes: pair.Value.passTypes.ToArray(),
            links: pair.Value.links.Select(link => link.pos).ToArray()));
        map.navigationCtrl.RebuildNow();
        foreach (var pair in local)
        {
            var full = map.navigationCtrl.navUnits[pair.Key];
            Require(full.objectBlocked == pair.Value.blocked
                && full.dirMaxY.Where((height, index) => Mathf.Abs(height - pair.Value.heights[index]) < 0.0001f).Count() == 4
                && full.passTypes.SetEquals(pair.Value.passTypes)
                && full.links.Select(link => link.pos).SequenceEqual(pair.Value.links),
                "local Nav matches full build at " + pair.Key + ": " + context);
        }
    }

    private static void LegacyPassTypeSceneData()
    {
        var tile = new TileUnitForm.Data(9051, "", new Dictionary<int, int>(), new Vector3Int(4, 0, 4),
            source.name, new Vector3(4, 0, 4), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
        var json = TileUnitForm.GetJoByData(tile);
        json["passType"] = 7001;
        var loaded = map.data.GetTileDatasByJa(new JArray(json).ToString());
        Require(loaded.Count == 1 && loaded[0].passType.SequenceEqual(new[] { 7001 }),
            "legacy scalar Tile passType loads as a one-element list");
        json["passType"] = 0;
        loaded = map.data.GetTileDatasByJa(new JArray(json).ToString());
        Require(loaded.Count == 1 && loaded[0].passType.Count == 0,
            "legacy unrestricted Tile passType loads as an empty list");
    }

    private static Z_Map.Analysis.NavUnit NavUnit(int x, int z, int y = 0)
    {
        return new Z_Map.Analysis.NavUnit
        {
            pos = new Vector3Int(x, y, z),
            realPos = new Vector3(x, y * 1.5f, z),
            links = new List<Z_Map.Analysis.NavUnit>(),
            passTypes = new HashSet<int>()
        };
    }

    private static void WangTileAnimationSelection()
    {
        const int mapTextureId = 7701;
        var clear = typeof(GameMapController).GetMethod(
            "ClearWangTileAnimationTextures",
            BindingFlags.Static | BindingFlags.NonPublic);
        var register = typeof(GameMapController).GetMethod(
            "RegisterWangTileAnimationTexture",
            BindingFlags.Static | BindingFlags.NonPublic);
        var resolve = typeof(GameMapController).GetMethod(
            "TryGetWangTileAnimationTextures",
            BindingFlags.Static | BindingFlags.NonPublic);

        clear.Invoke(null, null);
        register.Invoke(null, new object[] { mapTextureId, false, 0, 9101 });
        register.Invoke(null, new object[] { mapTextureId, false, 0, 9102 });
        object[] baseArgs = { mapTextureId, false, 0, null };
        Require((bool)resolve.Invoke(null, baseArgs)
            && ((List<int>)baseArgs[3]).SequenceEqual(new[] { 9101, 9102 }),
            "base WangTile retains every generated frame in source order");

        register.Invoke(null, new object[] { mapTextureId, true, 0, 9201 });
        register.Invoke(null, new object[] { mapTextureId, true, 0, 9202 });
        object[] frontArgs = { mapTextureId, true, 0, null };
        Require((bool)resolve.Invoke(null, frontArgs)
            && ((List<int>)frontArgs[3]).SequenceEqual(new[] { 9201, 9202 }),
            "front WangTile keeps an independent ordered animation sequence");

        clear.Invoke(null, null);
    }

    private static void ObjectWangTileNeighbourSelection()
    {
        var bits = typeof(GameMapController).GetMethod(
            "GetObjectNeighbourBits", BindingFlags.Static | BindingFlags.NonPublic);
        var center = new Bounds(Vector3.zero, new Vector3(2f, 1f, 4f));
        int GetBits(Vector3 position, Vector3 size)
            => (int)bits.Invoke(null, new object[] { center, new Bounds(position, size) });

        var neighbours = new[]
        {
            (new Vector3(-2f, 0f, 4f), 0), (new Vector3(0f, 0f, 4f), 1),
            (new Vector3(2f, 0f, 4f), 2), (new Vector3(-2f, 0f, 0f), 3),
            (new Vector3(2f, 0f, 0f), 4), (new Vector3(-2f, 0f, -4f), 5),
            (new Vector3(0f, 0f, -4f), 6), (new Vector3(2f, 0f, -4f), 7)
        };
        foreach (var (position, bit) in neighbours)
            Require(GetBits(position, new Vector3(2f, 1f, 4f)) == 1 << bit,
                "Object WangTile maps each side and corner to the TileHelper bit order");
        Require(GetBits(new Vector3(2.1f, 0f, 0f), new Vector3(2f, 1f, 4f)) == 0,
            "Object WangTile does not connect across a gap");
        Require(GetBits(new Vector3(2f, 2f, 0f), new Vector3(2f, 1f, 4f)) == 0,
            "Object WangTile does not connect at another height");

        var clear = typeof(GameMapController).GetMethod(
            "ClearWangTileAnimationTextures", BindingFlags.Static | BindingFlags.NonPublic);
        var register = typeof(GameMapController).GetMethod(
            "RegisterObjectWangTileAnimationTexture", BindingFlags.Static | BindingFlags.NonPublic);
        var field = typeof(GameMapController).GetField(
            "objectWangTileAnimationTextures", BindingFlags.Static | BindingFlags.NonPublic);
        clear.Invoke(null, null);
        register.Invoke(null, new object[] { 7601, AnimDirecton.Up, 1 << 2, 9701 });
        register.Invoke(null, new object[] { 7601, AnimDirecton.Up, 1 << 2, 9702 });
        var variants = (Dictionary<(int, AnimDirecton, int), List<int>>)field.GetValue(null);
        Require(variants[(7601, AnimDirecton.Up, 1 << 2)].SequenceEqual(new[] { 9701, 9702 }),
            "Object WangTile retains animation frames by direction and mask");
        clear.Invoke(null, null);
        Require(variants.Count == 0, "Object WangTile variants clear between scenes");
    }

    private static void SharedAnimationClock()
    {
        var previousGetter = TimeManager.animationTimeGetter;
        try
        {
            TimeManager.animationTimeGetter = () => 3.25f;
            Require(Mathf.Approximately(TimeManager.GetAnimationTime(), 3.25f),
                "lower-level animation clock uses its configured gameplay-time delegate");

            var getFrame = typeof(GameMapController).GetMethod(
                "GetAnimationFrameIndex",
                BindingFlags.Static | BindingFlags.NonPublic);
            Require((int)getFrame.Invoke(null, new object[] { TimeManager.GetAnimationTime(), 0.5f, 4 }) == 2,
                "non-character animation frame is selected from absolute gameplay time");

            TimeManager.animationTimeGetter = () => 4f;
            Require((int)getFrame.Invoke(null, new object[] { TimeManager.GetAnimationTime(), 0.5f, 4 }) == 0,
                "absolute animation phase wraps deterministically");
        }
        finally
        {
            TimeManager.animationTimeGetter = previousGetter;
        }
    }

    private static void CanvasHolderResetAfterPoolTeardown()
    {
        var holder = Go("canvas-holder").AddComponent<CanvasHolder>();
        var slider = Go("destroyed-slider").AddComponent<Slider>();
        var sliders = (Dictionary<int, Slider>)typeof(CanvasHolder)
            .GetField("sliderDic", BindingFlags.Instance | BindingFlags.NonPublic)
            .GetValue(holder);
        sliders.Add(1, slider);

        Object.DestroyImmediate(slider.gameObject);
        holder.Reset();
        Require(sliders.Count == 0,
            "CanvasHolder reset tolerates Slider children already destroyed by pool teardown");
    }

    private static void Visibility()
    {
        var go = Go("vision-probe");
        for (int i = 0; i < 6; i++)
        {
            var rendererGo = Go("layer-" + i);
            rendererGo.transform.SetParent(go.transform, false);
            rendererGo.AddComponent<MeshRenderer>();
        }
        var instance = go.AddComponent<RegressionTileInstance>();
        var tile = map.utilCtrl.GetTile(5, 1, 5);
        tile.ins = instance;
        instance.unit = tile;
        var block = new MaterialPropertyBlock();
        for (int i = 0; i < instance.renderers.Length; i++)
        {
            block.Clear();
            block.SetFloat("_RegressionTextureProperty", i + 10);
            block.SetFloat("_LightSensitivity", i + 2);
            instance.renderers[i].SetPropertyBlock(block);
        }

        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        map.updateCtrl.curCenterPos = new Vector3(5, 0, 5);
        map.updateCtrl.curTileLst.Add(tile.data);
        var update = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        update.Invoke(map.updateCtrl, null);
        Require(ReadShow(instance, 0) == 0f, "high Tile occlusion is fully transparent");
        int reads = instance.rendererReads;
        update.Invoke(map.updateCtrl, null);
        Require(instance.rendererReads == reads, "unchanged frame skips renderer submissions");

        instance.displayLayer = 0;
        update.Invoke(map.updateCtrl, null);
        for (int i = 0; i < 6; i++)
        {
            instance.renderers[i].GetPropertyBlock(block);
            Require(block.GetFloat("_Show") == 0f, "all high Tile renderers are hidden");
            Require(block.GetFloat("_RegressionTextureProperty") == i + 10, "other property preserved per renderer");
        }
        map.updateCtrl.curCenterPos = new Vector3(5, 4.5f, 5);
        update.Invoke(map.updateCtrl, null);
        Require(ReadShow(instance, 0) == 1f, "departed occlusion restored");
        for (int i = 0; i < 6; i++)
        {
            Require(ReadShow(instance, i) == (i == 0 || i == 3 ? 1f : 0f), "restored normal/front layer pairing");
            Require(ReadLightSensitivity(instance, i) == i + 2, "layer 0 keeps every initial light sensitivity");
        }
        instance.displayLayer = 1;
        instance.VisOn();
        for (int i = 0; i < 6; i++)
        {
            bool lowerLayer = i == 0 || i == 3;
            bool displayed = lowerLayer || i == 1 || i == 4;
            Require(ReadShow(instance, i) == (displayed ? 1f : 0f), "layer 1 keeps the cumulative display ceiling");
            Require(ReadLightSensitivity(instance, i) == (i + 2) * (lowerLayer ? 0.5f : 1f),
                "layer 1 halves only lower-layer light sensitivity");
        }
        instance.displayLayer = 2;
        instance.VisOn();
        for (int i = 0; i < 6; i++)
        {
            bool currentLayer = i == 2 || i == 5;
            Require(ReadShow(instance, i) == 1f, "layer 2 displays all layers cumulatively");
            Require(ReadLightSensitivity(instance, i) == (i + 2) * (currentLayer ? 1f : 0.5f),
                "layer 2 halves both lower layers from their recorded initial values");
        }
        instance.displayLayer = int.MaxValue;
        instance.VisOn();
        for (int i = 0; i < 6; i++)
            Require(ReadLightSensitivity(instance, i) == i + 2, "unrestricted display restores initial light sensitivity");
        instance.displayLayer = 0;
        instance.VisOn();
        var reusedBlock = typeof(MapInstance).GetField("visionBlock", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance);
        instance.ApplyVision(0.5f);
        Require(ReferenceEquals(reusedBlock, typeof(MapInstance).GetField("visionBlock", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(instance)), "property block reused");
        go.SetActive(false);
        foreach (var renderer in instance.renderers)
            renderer.SetPropertyBlock(null);
        go.SetActive(true);
        instance.ApplyVision(0.5f);
        Require(ReadShow(instance, 0) == 0.5f, "pool re-enable invalidates visibility cache");
        instance.VisOff();
        Require(!instance.vising && ReadShow(instance, 0) == 0, "hidden state");
        instance.VisOn();
        Require(instance.vising && ReadShow(instance, 0) == 1, "show after hide");
        map.updateCtrl.End();
        Require(map.updateCtrl.characterOverlapTileDic.GetDicT1().Count == 0, "end clears index");
    }

    private static float ReadShow(MapInstance instance, int index)
    {
        var block = new MaterialPropertyBlock();
        instance.renderers[index].GetPropertyBlock(block);
        return block.GetFloat("_Show");
    }

    private static float ReadLightSensitivity(MapInstance instance, int index)
    {
        var block = new MaterialPropertyBlock();
        instance.renderers[index].GetPropertyBlock(block);
        return block.GetFloat("_LightSensitivity");
    }

    private static RegressionTileInstance Probe(Unit unit)
    {
        var go = Go("attached-vision-probe");
        var child = Go("renderer");
        child.transform.SetParent(go.transform, false);
        child.AddComponent<MeshRenderer>();
        var instance = go.AddComponent<RegressionTileInstance>();
        unit.ins = instance;
        ((Instance)instance).unit = unit;
        return instance;
    }

    private static void TileFirstVisibility()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var high = map.utilCtrl.GetTile(5, 1, 5);
        var ground = map.utilCtrl.GetTile(5, 0, 4);
        var outside = map.utilCtrl.GetTile(12, 0, 12);
        foreach (var tile in map.data.maps.Values)
            if (tile.unit != outside)
                ctrl.curTileLst.Add(tile);
        typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(ctrl, new Vector3Int(5, 0, 5));
        ctrl.curCenterPos = new Vector3(5, 0, 5);
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;

        var obj = new ObjectUnitForm.Data(9101, false, "", source.name, high.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.objectTileDic.Add(obj, high);
        ctrl.objectTileDic.Add(obj, ground);
        ctrl.objectTileDic.Add(obj, map.utilCtrl.GetTile(6, 1, 5));
        ctrl.curObjectLst.Add(obj.data);
        var objIns = Probe(obj);
        var item = new ItemUnitForm.Data(9102, "", source.name, high.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.itemTileDic.Add(item, high);
        ctrl.curItemLst.Add(item.data);
        var itemIns = Probe(item);
        var character = Character();
        ctrl.characterTileDic.Add(character, ground);
        ctrl.characterOverlapTileDic.Add(character, high);
        ctrl.curCharacterLst.Add(character.data);
        var characterIns = Probe(character);
        var orphan = new ItemUnitForm.Data(9103, "", source.name, Vector3.zero,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.curItemLst.Add(orphan.data);
        var orphanIns = Probe(orphan);
        var tall = new ObjectUnitForm.Data(9104, false, "", source.name, new Vector3(5, 0, 3),
            Vector3.zero, new Vector3(2, 4.5f, 4), UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.objectTileDic.Add(tall, outside);
        ctrl.objectTileDic.Add(tall, ground);
        ctrl.objectTileDic.Add(tall, high);
        ctrl.curObjectLst.Add(tall.data);
        var tallIns = Probe(tall);

        var updateMethod = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl, updateMethod);
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        int objectsBefore = ctrl.objectTileDic.GetDicT2().Count;
        int itemsBefore = ctrl.itemTileDic.GetDicT2().Count;
        int charactersBefore = ctrl.characterTileDic.GetDicT2().Count;
        update();
        var final = (Dictionary<MapUnit, float>)finalField.GetValue(ctrl);
        Require(final[high] == 0f && final[obj] == 0.5f && final[item] == 0f,
            "high Tile/items are hidden but an Object covering both layers stays half transparent");
        Require(final[character] == 1 && ReadShow(characterIns, 0) == 1,
            "character visibility uses owner, not collision coverage");
        Require(final[tall] == 0.5f && ReadShow(tallIns, 0) == 0.5f,
            "mixed-layer visual coverage wins even with an out-of-view owner");
        Require(final[orphan] == 1 && !ctrl.itemTileDic.GetDicT1().ContainsKey(orphan),
            "missing owner restores opaque without creating ownership");
        Require(ctrl.objectTileDic.GetDicT2().Count == objectsBefore && ctrl.itemTileDic.GetDicT2().Count == itemsBefore
            && ctrl.characterTileDic.GetDicT2().Count == charactersBefore, "empty Tiles do not populate attachment indexes");

        int objectReads = objIns.rendererReads, itemReads = itemIns.rendererReads;
        for (int i = 0; i < 10; i++) update();
        long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) update();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - bytesBefore;
        Require(allocated == 0, "warm tile-first visibility allocates no managed memory: " + allocated);
        Require(objIns.rendererReads == objectReads && itemIns.rendererReads == itemReads,
            "unchanged attached units do not resubmit materials");

        var bound = new MapUnit(Data(9105, Vector3.zero));
        obj.Bind(bound);
        var boundIns = Probe(bound);
        update();
        Require(ReadShow(boundIns, 0) == 0.5f, "new bound instance inherits unchanged parent degree");
        ctrl.characterTileDic.Move(character, high);
        ctrl.itemTileDic.Move(item, outside);
        update();
        Require(ReadShow(characterIns, 0) == 0f && ReadShow(itemIns, 0) == 1,
            "moving ownership refreshes degree even outside current Tile set");

        var late = new ItemUnitForm.Data(9106, "", source.name, high.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.itemTileDic.Add(late, high);
        ctrl.curItemLst.Add(late.data);
        update();
        var lateIns = Probe(late);
        update();
        Require(ReadShow(lateIns, 0) == 0f, "newly shown instance receives previously collected degree");

        DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
        // Special visual-footprint candidates must work even outside the visible list.
        ctrl.curObjectLst.Remove(tall.data);
        update();
        Require(ReadShow(tallIns, 0) == 0.5f, "visual coverage occludes tall Object with offscreen owner");
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        update();
        Require(ReadShow(tallIns, 0) == 0.5f, "mixed-layer precedence also applies outside side view and visible list");
        ctrl.objectTileDic.Del(tall, high);
        update();
        Require(ReadShow(tallIns, 0) == 1, "departed visual-footprint override restores omitted visible Object");

        ctrl.SetGroupVision(high, 0.25f);
        Require(ReadShow(objIns, 0) == 0.25f && ReadShow(characterIns, 0) == 0.25f
            && ReadShow(lateIns, 0) == 0.25f && ReadShow(boundIns, 0) == 0.25f,
            "public SetGroupVision remains immediate and includes bound units");
        Require(ReadShow(tallIns, 0) == 1, "public group operation ignores non-owner overlap links");
        ctrl.SetGroupVision(map.utilCtrl.GetTile(11, 0, 11), 0.5f);
        Require(ctrl.objectTileDic.GetDicT2().Count == objectsBefore, "public empty group query remains read-only");
        ctrl.End();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl)).Count == 0, "End clears final visibility state");
        ctrl.curTileLst.Add(high.data);
        ctrl.curObjectLst.Add(obj.data);
        ctrl.objectTileDic.Add(obj, high);
        update();
        Require(ReadShow(objIns, 0) == 0f, "visibility rebuilds high-only Object after End and re-entry");
        ctrl.End();
    }

    private static void SparseViewRefreshAfterErase()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var column = new List<TileUnitForm.Data>();
        for (int y = 0; y <= 2; y++)
        {
            var tile = new TileUnitForm.Data(22000 + y, "", new Dictionary<int, int>(), new Vector3Int(14, y, 14),
                source.name, new Vector3(14, y * 1.5f, 14), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(tile);
            column.Add(tile);
        }

        map.data.UnRegisterMap(column[1]);
        Require(map.data.mapXZ2Y.TryGetValue((14, 14), out var sparseLevels)
            && sparseLevels.SetEquals(new[] { 0, 2 }),
            "erasing a middle Tile keeps a sparse height index");

        Probe(column[0].unit);
        Probe(column[2].unit);
        var visible = new HashSet<TileUnitForm.Data>();
        var showAndAdd = typeof(MapUpdateController).GetMethod(
            "ShowAndAddLst",
            BindingFlags.Instance | BindingFlags.NonPublic);
        showAndAdd.Invoke(ctrl, new object[] { visible, 14, 15, 0, 3, 14, 15 });
        Require(visible.Count == 2 && visible.Contains(column[0]) && visible.Contains(column[2]),
            "view refresh skips erased gaps in sparse height columns");

        map.data.UnRegisterMap(column[0]);
        map.data.UnRegisterMap(column[2]);
        Require(!map.data.mapXZ2Y.ContainsKey((14, 14)),
            "erasing the last Tile removes the empty X/Z height index");
        visible.Clear();
        showAndAdd.Invoke(ctrl, new object[] { visible, 14, 15, 0, 3, 14, 15 });
        Require(visible.Count == 0,
            "view refresh remains safe after erasing the whole height column");
        ctrl.End();
    }

    private sealed class EraseRemovalProbe : IZ_Listener<ObjectEvent>
    {
        public readonly List<ObjectUnit> removed = new List<ObjectUnit>();
        public TileUnit owner;
        public void OnEvent(ObjectEvent evt)
        {
            if (evt.type != MapEventType.Remove) return;
            removed.Add(evt.unit);
            Require(evt.unit.belongTile == owner && map.data.maps.ContainsValue(owner.data),
                "erase removes Objects before clearing Tile ownership/terrain");
        }
    }

    private static void EraseBrushOwnedUnits()
    {
        // EditMode does not invoke the runtime child-Form registration hooks.
        Form.MapEraseForm.InitInternal();
        TileUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        CharacterUnitForm.InitInternal();
        ItemUnitForm.InitInternal();
        var ctrl = map.updateCtrl;
        var erase = typeof(ModSceneController).GetMethod("EraseTile", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(erase != null, "real ModScene erase operation exists");
        // Avoid registering input/Tile listeners: only the erase operation is exercised.
        var mod = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
        var brushes = new[]
        {
            Form.MapEraseForm.DataByName["all erase"],
            Form.MapEraseForm.DataByName["remain terrain"],
            new Form.MapEraseForm.Data(0, "objects only", 0, 0, false, true, false, false, false),
            new Form.MapEraseForm.Data(0, "characters only", 0, 0, false, false, false, true, false),
            new Form.MapEraseForm.Data(0, "items only", 0, 0, false, false, true, false, false),
            new Form.MapEraseForm.Data(0, "terrain only", 0, 0, true, false, false, false, false),
            new Form.MapEraseForm.Data(0, "none", 0, 0, false, false, false, false, false)
        };
        var passIndex = (Z_DoubleDictionary.DoubleDictionary<ObjectUnit, TileUnit>)typeof(MapUpdateController)
            .GetField("objectPassTypeTileDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctrl);
        var navigationIndex = (Z_DoubleDictionary.DoubleDictionary<ObjectUnit, TileUnit>)typeof(MapUpdateController)
            .GetField("objectNavigationTileDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(ctrl);
        for (int scenario = 0; scenario < brushes.Length; scenario++)
        {
            ctrl.End();
            var brush = brushes[scenario];
            int firstId = 30000 + scenario * 20;
            TileUnitForm.Data Tile(int offset, int x)
            {
                var data = new TileUnitForm.Data(firstId + offset, "", new Dictionary<int, int> { [0] = 123, [1] = 456, [3] = 789 },
                    new Vector3Int(x, 0, 40), source.name, new Vector3(x, 0, 40), Vector3.zero, Vector3.one,
                    UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
                TileUnitForm.AddData(data);
                map.data.RegisterMap(data);
                return data;
            }
            var tile = Tile(0, 40);
            var neighbour = Tile(1, 43); // Outside terrain-neighbour refresh; overlap links are explicit below.
            ctrl.curTileLst.Add(tile);
            var units = new List<MapUnit>();
            for (int i = 0; i < 3; i++)
            {
                bool foreign = i == 2;
                var owner = foreign ? neighbour.unit : tile.unit;
                var other = foreign ? tile.unit : neighbour.unit;
                var obj = new ObjectUnitForm.Data(firstId + 2 + i * 3, false, "", source.name, owner.data.pos,
                    Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
                ObjectUnitForm.AddData(obj.data);
                ctrl.objectTileDic.Add(obj, owner);
                ctrl.objectTileDic.Add(obj, other);
                passIndex.Add(obj, tile.unit);
                navigationIndex.Add(obj, tile.unit);
                ctrl.curObjectLst.Add(obj.data);
                units.Add(obj);
                var character = new CharacterUnitForm.Data(firstId + 3 + i * 3, false, Vector3.zero, 1, 1, 1, false, "",
                    source.name, owner.data.pos, Vector3.zero, Vector3.one * 3,
                    UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>()).unit;
                CharacterUnitForm.AddData(character.data);
                ctrl.characterTileDic.Add(character, owner);
                ctrl.characterOverlapTileDic.Add(character, tile.unit);
                ctrl.characterOverlapTileDic.Add(character, neighbour.unit);
                ctrl.curCharacterLst.Add(character.data);
                units.Add(character);
                var item = new ItemUnitForm.Data(firstId + 4 + i * 3, "", source.name, owner.data.pos,
                    Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
                ItemUnitForm.AddData(item.data);
                ctrl.itemTileDic.Add(item, owner);
                ctrl.curItemLst.Add(item.data);
                units.Add(item);
            }
            Require(tile.unit.subUnits.Count == 0, "erase fixture owners are not explicit bound subUnits");
            var probe = new EraseRemovalProbe { owner = tile.unit };
            probe.Register<ObjectEvent>();
            try
            {
                erase.Invoke(mod, new object[] { tile, brush });
                for (int i = 0; i < units.Count; i++)
                {
                    var unit = units[i];
                    bool foreign = i >= 6;
                    bool removed = !foreign && (unit is ObjectUnit ? brush.mObject : unit is CharacterUnit ? brush.character : brush.item);
                    bool registered;
                    bool visible;
                    if (unit is ObjectUnit obj)
                    {
                        registered = ObjectUnitForm.DataByUid.ContainsKey(obj.data.uid);
                        visible = ctrl.curObjectLst.Contains(obj.data);
                        if (removed)
                            Require(!ctrl.objectTileDic.TryGet(obj, out _) && !passIndex.TryGet(obj, out _) && !navigationIndex.TryGet(obj, out _),
                                "erase clears Object visual/pass/navigation indexes");
                    }
                    else if (unit is CharacterUnit character)
                    {
                        registered = CharacterUnitForm.DataByUid.ContainsKey(character.data.uid);
                        visible = ctrl.curCharacterLst.Contains(character.data);
                        if (removed)
                            Require(!ctrl.characterTileDic.TryGet(character, out _) && !ctrl.characterOverlapTileDic.TryGet(character, out _),
                                "erase clears Character owner/overlap indexes");
                    }
                    else
                    {
                        var item = (ItemUnit)unit;
                        registered = ItemUnitForm.DataByUid.ContainsKey(item.data.uid);
                        visible = ctrl.curItemLst.Contains(item.data);
                        if (removed) Require(!ctrl.itemTileDic.TryGet(item, out _), "erase clears Item owner index");
                    }
                    Require(registered == !removed && visible == !removed, brush.name + " correct Form/view removal for " + unit.GetType().Name);
                    if (foreign) Require(unit.belongTile == neighbour.unit, "neighbour-owned overlap survives erase");
                }
                Require(probe.removed.Count == (brush.mObject ? 2 : 0) && probe.removed.Distinct().Count() == probe.removed.Count,
                    "each erased Object emits one removal event");
                Require(map.data.maps.ContainsValue(tile) == !brush.terrain && TileUnitForm.DataByUid.ContainsKey(tile.uid) == !brush.terrain
                    && ctrl.curTileLst.Contains(tile) == !brush.terrain, "erase terrain flag and sparse map cleanup");
                Require(tile.texDic.Count == 3 && tile.texDic[0] == 123 && tile.texDic[1] == 456 && tile.texDic[3] == 789,
                    "entity erase preserves terrain textures and masks");
                if (!brush.terrain)
                {
                    erase.Invoke(mod, new object[] { tile, brush });
                    Require(probe.removed.Count == (brush.mObject ? 2 : 0), "repeat erase does not remove neighbours or emit duplicate events");
                }
            }
            finally
            {
                probe.Unregister<ObjectEvent>();
                ctrl.End();
                foreach (var unit in units)
                {
                    if (unit is ObjectUnit obj) ObjectUnitForm.RemoveData(obj.data.uid);
                    else if (unit is CharacterUnit character) CharacterUnitForm.RemoveData(character.data.uid);
                    else ItemUnitForm.RemoveData(unit.data.uid);
                }
                map.data.UnRegisterMap(tile);
                map.data.UnRegisterMap(neighbour);
                TileUnitForm.RemoveData(tile.uid);
                TileUnitForm.RemoveData(neighbour.uid);
            }
        }
    }

    private static void IncrementalViewRefresh()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var originalViewSize = map.data.mainData.viewSize;
        map.data.mainData.viewSize = new Vector3Int(2, 1, 1);
        var tiles = new List<TileUnitForm.Data>();
        for (int x = 30; x <= 34; x++)
        {
            var tile = new TileUnitForm.Data(23000 + x, "", new Dictionary<int, int>(), new Vector3Int(x, 0, 30),
                source.name, new Vector3(x, 0, 30), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(tile);
            Probe(tile.unit);
            tiles.Add(tile);
        }
        var spanningObject = new ObjectUnitForm.Data(24000, false, "", source.name, tiles[0].pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.objectTileDic.Add(spanningObject, tiles[0].unit);
        ctrl.objectTileDic.Add(spanningObject, tiles[1].unit);
        Probe(spanningObject);

        var viewCenterField = typeof(MapUpdateController).GetField(
            "viewCenter",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var freshMethod = typeof(MapUpdateController).GetMethod(
            "FreshMap",
            BindingFlags.Instance | BindingFlags.NonPublic);
        var fresh = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), ctrl, freshMethod);

        viewCenterField.SetValue(ctrl, new Vector3Int(32, 0, 30));
        fresh(true);
        Require(ctrl.curTileLst.SetEquals(tiles.Take(4))
            && ctrl.newMapLst.Count == 4 && ctrl.delMapLst.Count == 0,
            "first view refresh records only its visible Tiles as additions");

        fresh(false);
        Require(ctrl.newMapLst.Count == 0 && ctrl.delMapLst.Count == 0,
            "unchanged view bounds skip Tile additions and removals");

        viewCenterField.SetValue(ctrl, new Vector3Int(33, 0, 30));
        fresh(false);
        Require(ctrl.curTileLst.Count == 4
            && ctrl.newMapLst.SetEquals(new[] { tiles[4] })
            && ctrl.delMapLst.SetEquals(new[] { tiles[0] }),
            "one-cell movement updates only the entering and leaving slabs");
        Require(spanningObject.isShowing && ctrl.curObjectLst.Contains(spanningObject.data),
            "unordered Tile changes keep an Object visible while any covered Tile remains visible");

        ctrl.End();
        foreach (var tile in tiles)
        {
            tile.unit.Hide();
            map.data.UnRegisterMap(tile);
        }
        map.data.mainData.viewSize = originalViewSize;
    }

    private static void HeightProjectedViewRefresh()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var originalSize = map.data.mainData.viewSize;
        var originalMode = DynamicGlobalSettings.cameraMode;
        map.data.mainData.viewSize = new Vector3Int(2, 3, 2);
        var tiles = new List<TileUnitForm.Data>();
        int uid = 600000;
        for (int x = 60; x <= 64; x++)
        for (int y = -2; y <= 2; y++)
        for (int z = 60; z <= 70; z++)
        {
            var tile = new TileUnitForm.Data(uid++, "", new Dictionary<int, int>(), new Vector3Int(x, y, z),
                source.name, new Vector3(x, y, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(tile);
            Probe(tile.unit);
            tiles.Add(tile);
        }
        var centerField = typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic);
        var fresh = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), ctrl,
            typeof(MapUpdateController).GetMethod("FreshMap", BindingFlags.Instance | BindingFlags.NonPublic));
        Action<Vector3Int, CameraMode, bool> check = (center, mode, force) =>
        {
            var before = new HashSet<TileUnitForm.Data>(ctrl.curTileLst);
            // The shared geometry prefab has no TileInstance. Supply inactive
            // probes for entering/re-entering Tiles, like instances waiting in a pool.
            foreach (var tile in tiles)
            {
                if (tile.unit.ins == null)
                    Probe(tile.unit);
                if (!before.Contains(tile))
                    tile.unit.ins.gameObject.SetActive(false);
            }
            DynamicGlobalSettings.cameraMode = mode;
            centerField.SetValue(ctrl, center);
            fresh(force);
            var expected = new HashSet<TileUnitForm.Data>(tiles.Where(tile =>
            {
                var p = tile.mapPos;
                int layerCenterZ = center.z;
                if (mode == CameraMode.Isometric && p.y < center.y)
                    layerCenterZ += center.y - p.y;
                return p.x >= center.x - 2 && p.x < center.x + 2
                    && p.y >= center.y - 3 && p.y < center.y + 3
                    && p.z >= layerCenterZ - 2 && p.z < layerCenterZ + 2;
            }));
            Require(ctrl.curTileLst.SetEquals(expected), "projected layer rectangles match visible Tiles: " + center + "/" + mode);
            Require(ctrl.newMapLst.SetEquals(expected.Except(before))
                && ctrl.delMapLst.SetEquals(before.Except(expected)), "projected entering/leaving sets match full visibility difference");
            Require(tiles.All(tile => tile.unit.isShowing == expected.Contains(tile)), "projected bounds also update pooled Tile visibility");
        };
        check(new Vector3Int(62, 0, 63), CameraMode.Isometric, true);
        check(new Vector3Int(62, 0, 63), CameraMode.Isometric, false);
        check(new Vector3Int(63, 0, 64), CameraMode.Isometric, false);
        check(new Vector3Int(63, 1, 64), CameraMode.Isometric, false);
        check(new Vector3Int(62, -1, 63), CameraMode.Isometric, false);
        check(new Vector3Int(62, -1, 63), CameraMode.Overhead, false);
        check(new Vector3Int(62, -1, 63), CameraMode.Isometric, false);
        check(new Vector3Int(70, 1, 72), CameraMode.Isometric, false);
        check(new Vector3Int(62, 0, 63), CameraMode.Isometric, false);
        check(new Vector3Int(62, 0, 63), CameraMode.Isometric, true);
        ctrl.End();
        foreach (var tile in tiles)
        {
            tile.unit.Hide();
            map.data.UnRegisterMap(tile);
        }
        map.data.mainData.viewSize = originalSize;
        DynamicGlobalSettings.cameraMode = originalMode;
    }

    private static void OcclusionTransparencyAndPriority()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var originalViewSize = map.data.mainData.viewSize;
        map.data.mainData.viewSize = new Vector3Int(20, 9, 20);
        for (int y = 5; y <= 6; y++)
        {
            var tile = new TileUnitForm.Data(20000 + y, "", new Dictionary<int, int>(), new Vector3Int(6, y, 6),
                source.name, new Vector3(6, y * 1.5f, 6), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(tile);
        }
        var layerRelativeSeedData = new TileUnitForm.Data(20003, "", new Dictionary<int, int>(), new Vector3Int(6, 3, 1),
            source.name, new Vector3(6, 4.5f, 1), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
        map.data.RegisterMap(layerRelativeSeedData);
        var surroundedPositions = new[]
        {
            new Vector3Int(5, 4, 6),
            new Vector3Int(7, 4, 6),
            new Vector3Int(6, 4, 5),
            new Vector3Int(6, 4, 7),
        };
        for (int i = 0; i < surroundedPositions.Length; i++)
        {
            var mapPos = surroundedPositions[i];
            var tile = new TileUnitForm.Data(20010 + i, "", new Dictionary<int, int>(), mapPos,
                source.name, new Vector3(mapPos.x, mapPos.y * 1.5f, mapPos.z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(tile);
        }
        foreach (var tile in map.data.maps.Values)
            ctrl.curTileLst.Add(tile);
        typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(ctrl, new Vector3Int(6, 0, 6));
        ctrl.curCenterPos = new Vector3(6, 0, 6);
        var high = map.utilCtrl.GetTile(6, 1, 5);
        var ground = map.utilCtrl.GetTile(6, 0, 5);
        var highOnly = OcclusionObject(9201, high.data.pos, high);
        var currentOnly = OcclusionObject(9202, ground.data.pos, ground);
        var highOwnerMixed = OcclusionObject(9203, high.data.pos, high, ground);
        var groundOwnerMixed = OcclusionObject(9204, ground.data.pos, ground, high);
        var omittedMixed = OcclusionObject(9205, high.data.pos, high, ground);
        var omittedHigh = OcclusionObject(9206, high.data.pos, high);
        ctrl.curObjectLst.Remove(omittedMixed.data);
        ctrl.curObjectLst.Remove(omittedHigh.data);
        var fartherObjectTile = map.utilCtrl.GetTile(6, 0, 0);
        var fartherObject = OcclusionObject(9207, fartherObjectTile.data.pos, fartherObjectTile);
        var layerRelativeSeedTile = map.utilCtrl.GetTile(6, 3, 1);
        var halfHighTile = map.utilCtrl.GetTile(6, 5, 6);
        var halfHighObject = OcclusionObject(9208, halfHighTile.data.pos, halfHighTile);
        var frontProjectionTile = map.utilCtrl.GetTile(6, 6, 6);
        var frontProjectionInstance = ProbeTile(frontProjectionTile);
        var surroundedHighTile = map.utilCtrl.GetTile(5, 4, 6);
        var surroundedHighObject = OcclusionObject(9209, surroundedHighTile.data.pos, surroundedHighTile);
        map.navigationCtrl.navUnits[(6, 0, 11)].links.Clear();
        map.navigationCtrl.navUnits[(5, 0, 10)].links.Clear();
        map.navigationCtrl.navUnits[(6, 0, 12)].passTypes.Add(7001);

        var updateMethod = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl, updateMethod);
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(!map.utilCtrl.ContainsTile(6, 4, 6),
            "four-side full transparency does not require a center Tile above the player");
        foreach (var mode in new[] { CameraMode.Overhead, CameraMode.Isometric })
        {
            DynamicGlobalSettings.cameraMode = mode;
            update();
            var final = (Dictionary<MapUnit, float>)finalField.GetValue(ctrl);
            Require(final[map.utilCtrl.GetTile(12, 1, 6)] == 0f, "connected high Tile searches the full view width: " + mode);
            Require(final[map.utilCtrl.GetTile(10, 1, 10)] == 0f, "connected diagonal high Tile is not radius-limited: " + mode);
            Require(final[halfHighTile] == 1f && final[frontProjectionTile] == 0.5f,
                "half-transparent high Tiles require a walkable projected NavUnit and ignore pass type: " + mode);
            for (int i = 0; i < 6; i++)
                Require(ReadShow(frontProjectionInstance, i) == (i < 3 ? 0.5f : 1f),
                    "front renderers stay opaque over their one-cell-nearer blocked projection: " + mode);
            Require(final[surroundedHighTile] == 1f && final[map.utilCtrl.GetTile(7, 4, 6)] == 0f
                && final[map.utilCtrl.GetTile(6, 4, 5)] == 0f && final[map.utilCtrl.GetTile(6, 4, 7)] == 1f,
                "full-transparent high Tiles also stay opaque over an unwalkable projected NavUnit: " + mode);
            Require(final[halfHighObject] == 1f && final[surroundedHighObject] == 1f,
                "Objects on projection-blocked high Tiles remain opaque: " + mode);
            Require(final[layerRelativeSeedTile] == 1f,
                "isolated high Tile beyond layer-gap-plus-one seed distance stays opaque: " + mode);
            Require(final[highOnly] == 0f && ReadShow(((MapUnit)highOnly).ins, 0) == 0f,
                "high-only Object fully hidden: " + mode);
            Require(final[highOwnerMixed] == 0.5f && final[groundOwnerMixed] == 0.5f,
                "current-layer priority is independent of owner/link order: " + mode);
            Require(final[omittedMixed] == 0.5f && final[omittedHigh] == 0f,
                "high-layer visual-footprint candidates work outside the visible Object list: " + mode);
            Require(final[currentOnly] == (mode == CameraMode.Isometric ? 0.5f : 1f),
                "current-only Object retains side-view occlusion rule: " + mode);
            Require(final[fartherObject] == (mode == CameraMode.Isometric ? 0.5f : 1f),
                "current-layer forward search uses the full configured view range: " + mode);
        }

        DynamicGlobalSettings.playing = false;
        foreach (var mode in new[] { CameraMode.Overhead, CameraMode.Isometric })
        {
            DynamicGlobalSettings.cameraMode = mode;
            update();
            var final = (Dictionary<MapUnit, float>)finalField.GetValue(ctrl);
            Require(final[map.utilCtrl.GetTile(12, 1, 6)] == 0.5f
                && final[map.utilCtrl.GetTile(6, 6, 6)] == 0.5f
                && final[layerRelativeSeedTile] == 0.5f
                && final[surroundedHighTile] == 0.5f,
                "Mod mode makes every higher Tile half transparent: " + mode);
            Require(final[highOnly] == 0.5f && final[omittedHigh] == 0.5f
                && final[halfHighObject] == 0.5f && final[surroundedHighObject] == 0.5f,
                "Mod mode makes higher Objects half transparent, including offscreen owners: " + mode);
            Require(final[ground] == 1f,
                "Mod higher-layer option does not change current-layer Tiles: " + mode);
            Require(ReadShow(frontProjectionInstance, 3) == 0.5f,
                "Mod higher-layer opacity resets the Play-only front projection override: " + mode);
        }
        DynamicGlobalSettings.playing = true;

        DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
        ctrl.curCenterPos = new Vector3(6, 0, 5);
        update();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[layerRelativeSeedTile] == 1f,
            "valid layer-gap-plus-one seed outside the three-Tile radius stays opaque");
        ctrl.curCenterPos = new Vector3(6, 0, 4);
        update();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[layerRelativeSeedTile] == 0.5f,
            "non-surrounding high Tile at the three-Tile radius boundary is half transparent");
        ctrl.curCenterPos = new Vector3(6, 0, 6);
        update();

        int objectsBefore = ctrl.objectTileDic.GetDicT2().Count;
        for (int i = 0; i < 10; i++) update();
        long allocatedBefore = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) update();
        Require(GC.GetAllocatedBytesForCurrentThread() == allocatedBefore, "full-view occlusion is allocation-free after warmup");
        Require(ctrl.objectTileDic.GetDicT2().Count == objectsBefore, "full-view search does not create reverse-index keys");
        ctrl.curCenterPos = new Vector3(12, 4.5f, 12);
        update();
        Require(ReadShow(((MapUnit)omittedHigh).ins, 0) == 1f && ReadShow(((MapUnit)omittedMixed).ins, 0) == 1f,
            "leaving the occluding layers restores omitted high/mixed Objects from full/half transparency");
        ctrl.End();
        map.data.mainData.viewSize = originalViewSize;
    }

    private static ObjectUnit OcclusionObject(int uid, Vector3 pos, params TileUnit[] tiles)
    {
        var unit = new ObjectUnitForm.Data(uid, false, "", source.name, pos,
            Vector3.zero, new Vector3(1, 12, 1), UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        foreach (var tile in tiles)
            map.updateCtrl.objectTileDic.Add(unit, tile);
        map.updateCtrl.curObjectLst.Add(unit.data);
        Probe(unit);
        return unit;
    }

    private static RegressionTileInstance ProbeTile(TileUnit unit)
    {
        var go = Go("tile-vision-probe");
        for (int i = 0; i < 6; i++)
        {
            var rendererGo = Go("renderer-" + i);
            rendererGo.transform.SetParent(go.transform, false);
            rendererGo.AddComponent<MeshRenderer>();
        }

        var instance = go.AddComponent<RegressionTileInstance>();
        unit.ins = instance;
        instance.unit = unit;
        return instance;
    }

    private static void Events()
    {
        var ctrl = new GameEventSceneTriggerController(null);
        var tile = map.utilCtrl.GetTile(5, 0, 5);
        var character = Character();
        var item = new ItemUnit(new ItemUnitForm.Data(Data(9002, Vector3.zero)));
        var obj = new ObjectUnit(new ObjectUnitForm.Data(Data(9003, Vector3.zero)));
        foreach (MapEventType type in Enum.GetValues(typeof(MapEventType)))
        {
            ctrl.evts = null;
            ctrl.OnEvent(new TileEvent { unit = tile, type = type });
            Require((ctrl.evts != null) == (type == MapEventType.Create), "tile lifecycle filter");
            ctrl.evts = null;
            ctrl.OnEvent(new ItemEvent { unit = item, type = type });
            Require((ctrl.evts != null) == (type == MapEventType.Create), "item lifecycle filter");
            ctrl.evts = null;
            // Remove requires the full GameManager event/task runtime; this isolated
            // fixture only verifies the other Object lifecycle filters.
            if (type != MapEventType.Remove)
            {
                ctrl.OnEvent(new ObjectEvent { unit = obj, type = type });
                Require((ctrl.evts != null) == (type == MapEventType.Create || type == MapEventType.BoundaryTouch),
                    "object lifecycle filter");
            }
            ctrl.evts = null;
            ctrl.OnEvent(new CharacterEvent { unit = character, type = type });
            bool expected = type == MapEventType.Create || type == MapEventType.BoundaryTouch;
            Require((ctrl.evts != null) == expected, "character lifecycle filter");
            if (expected)
            {
                var callback = ctrl.evts.GetInvocationList().Single();
                var capture = callback.Target;
                Require((bool)capture.GetType().GetField("characterSelf").GetValue(capture) == (type == MapEventType.Create),
                    "character self reference semantics");
            }
        }
        ctrl.evts = null;
        ctrl.OnEvent(new StoryLifeEvent { type = StoryLifeEventType.Enter });
        Require(ctrl.evts == null, "unused story lifecycle filtered");
        ctrl.OnEvent(new StoryLifeEvent { type = StoryLifeEventType.EverySecond });
        Require(ctrl.evts != null, "per-second callback retained");
        ctrl.evts = null;
        var ignored = new TileEvent { unit = tile, type = MapEventType.AfterUpdate };
        ctrl.OnEvent(ignored);
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++)
            ctrl.OnEvent(ignored);
        Require(GC.GetAllocatedBytesForCurrentThread() == before && ctrl.evts == null, "ignored events allocate no closure");
        ctrl.OnEvent(new CollideEvent { a = character, b = tile, type = CollideEventType.TriggerExit });
        Require(ctrl.evts == null, "unused tile exit callback filtered");
        ctrl.OnEvent(new CollideEvent { a = character, b = tile, type = CollideEventType.TriggerEnter });
        Require(ctrl.evts != null, "tile touch callback retained");
        // This fixture passes null for its owner; it must not receive later
        // real removal events from the erase/collision-scale fixtures.
        ctrl.Unregister<CollideEvent>();
        ctrl.Unregister<TileEvent>();
        ctrl.Unregister<ItemEvent>();
        ctrl.Unregister<ObjectEvent>();
        ctrl.Unregister<CharacterEvent>();
        ctrl.Unregister<StoryLifeEvent>();
    }

    private static void Require(bool condition, string message)
    {
        checks++;
        if (!condition)
            throw new InvalidOperationException("Regression failed: " + message);
    }
}

[ExecuteAlways]
public class RegressionTileInstance : TileInstance
{
    public int rendererReads;
    public override Renderer[] renderers
    {
        get { rendererReads++; return base.renderers; }
    }
}
