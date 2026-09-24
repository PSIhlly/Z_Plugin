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
            WangTileAnimationSelection();
            ObjectWangTileNeighbourSelection();
            SharedAnimationClock();
            CanvasHolderResetAfterPoolTeardown();
            SparseViewRefreshAfterErase();
            IncrementalViewRefresh();
            Visibility();
            TileFirstVisibility();
            OcclusionTransparencyAndPriority();
            Events();
            Debug.Log("MAP_RUNTIME_REGRESSION_PASS checks=" + checks);
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
