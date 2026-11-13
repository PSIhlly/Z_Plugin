using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Analysis;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Isolated runtime-assembly fixture; no current scene or user save is opened.
public static class NavigationObjectStateRegression
{
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static readonly FieldInfo ListVersion = typeof(List<NavUnit>).GetField("_version", BindingFlags.Instance | BindingFlags.NonPublic);
    private static MapManager map;
    private static InstancePoolManager pool;
    private static GameMapController appearance;
    private static string groundPrefab;
    private static int checks, nextUid = 74000;

    public static void Run()
    {
        bool oldPlaying = DynamicGlobalSettings.playing, oldDrop = DynamicGlobalSettings.allowDrop;
        try
        {
            foreach (var type in new[] { typeof(UnitForm), typeof(TileUnitForm), typeof(ObjectUnitForm), typeof(MapMainForm),
                         typeof(ProductForm), typeof(ObjectProductForm), typeof(MapModelForm), typeof(EventTriggerForm),
                         typeof(ParamForm), typeof(GameParamForm), typeof(MapObjectParamForm) })
            {
                type.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
                type.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }
            Check(ListVersion != null, "the native List version is available to detect even content-identical link rebuilds");
            DynamicGlobalSettings.playing = false;
            DynamicGlobalSettings.allowDrop = true;
            map = Manager<MapManager>();
            pool = Manager<InstancePoolManager>(); pool.Init(); pool.defaultRoot = pool.transform;
            var ground = Go("navigation-state-flat"); ground.SetActive(false); pool.AddPool(ground); groundPrefab = ground.name;
            NonCollidingObjects();
            StableObstacleAndOverlap();
            WithinCellEdgeChange();
            PassOnlyChangeAndDisable();
            TerrainStateAndReset();
            ProductCollisionSynchronization();
            Debug.Log("NAVIGATION_OBJECT_STATE_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        finally
        {
            if (appearance != null)
            {
                Z_EventHelper.Unregister<TileEvent>(appearance);
                Z_EventHelper.Unregister<ObjectEvent>(appearance);
                Z_EventHelper.Unregister<ItemEvent>(appearance);
            }
            DynamicGlobalSettings.playing = oldPlaying; DynamicGlobalSettings.allowDrop = oldDrop;
            if (pool?.pools != null) pool.pools.Clear();
            for (int i = owned.Count - 1; i >= 0; i--) if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private static void NonCollidingObjects()
    {
        Grid();
        var tile = Tile(3, 0, 3); tile.SetPassTypes(new[] { 77 });
        map.navigationCtrl.RefreshObjectTiles(new[] { tile });
        var unit = Unit(new Vector3(3, .5f, 3), new Vector3(4, 1, 4), false);
        var versions = Versions(); var state = GraphState();
        for (int i = 0; i < 100; i++)
        {
            unit.data.pos += Vector3.right * (i % 2 == 0 ? .01f : -.01f);
            map.updateCtrl.RefreshObjectOverlap(unit);
        }
        Check(map.updateCtrl.objectTileDic.Get(unit).Count > 1, "noncolliding visual coverage still includes its model footprint");
        Check(!HasNavigationCoverage(unit) && tile.passTypes.Contains(77), "disabled collision creates no navigation or pass-type coverage");
        Check(Changed(versions).Count == 0 && SameState(state, GraphState()), "100 noncolliding moves leave all navigation state and link lists untouched");
        ObjectUnitForm.RemoveData(unit.data.uid); map.updateCtrl.RemoveObjectOverlap(unit);
        Check(Changed(versions).Count == 0, "removing a noncolliding Object does not rebuild links");
    }

    private static void StableObstacleAndOverlap()
    {
        Grid();
        var unit = Unit(new Vector3(3, .55f, 3), Vector3.one * .1f, true);
        var before = Versions(); map.updateCtrl.RefreshObjectOverlap(unit);
        Check(Node(3, 0, 3).objectBlocked && Changed(before).Count > 0, "a new colliding body immediately blocks its central cell and related links");
        EqualFull("initial obstacle");
        before = Versions();
        for (int i = 0; i < 100; i++)
        {
            unit.data.pos += Vector3.right * (i % 2 == 0 ? .001f : -.001f);
            map.updateCtrl.RefreshObjectOverlap(unit);
        }
        Check(Changed(before).Count == 0, "100 within-cell moves with identical blocking state do not clear or rebuild any link list");
        var second = Unit(unit.data.pos, Vector3.one * .1f, true);
        map.updateCtrl.RefreshObjectOverlap(second);
        Check(Changed(before).Count == 0, "an overlapping blocker with the same resulting state does not rebuild links");
        ObjectUnitForm.RemoveData(unit.data.uid); map.updateCtrl.RemoveObjectOverlap(unit);
        Check(Node(3, 0, 3).objectBlocked && Changed(before).Count == 0, "removing one overlap preserves the other blocker without rebuilding identical links");
        ObjectUnitForm.RemoveData(second.data.uid); map.updateCtrl.RemoveObjectOverlap(second);
        var changed = Changed(before);
        Check(!Node(3, 0, 3).objectBlocked && Node(2, 0, 3).links.Contains(Node(3, 0, 3)), "removing the final blocker restores incoming and outgoing links");
        Check(changed.Count > 0 && changed.All(n => Mathf.Abs(n.pos.y) <= 1 && Mathf.Abs(n.pos.x - 3) + Mathf.Abs(n.pos.z - 3) <= 1),
            "link rebuilding stays within the genuinely changed cell and its possible incoming neighbors");
        EqualFull("last blocker removed");
    }

    private static void WithinCellEdgeChange()
    {
        Grid();
        var unit = Unit(new Vector3(3.45f, .24f, 3), new Vector3(.04f, .1f, .15f), true);
        map.updateCtrl.RefreshObjectOverlap(unit);
        var coverage = NavigationCoverage(unit); var versions = Versions();
        Check(!Node(3, 0, 3).objectBlocked && Node(3, 0, 3).objectBlockedDirections == 0, "an edge obstacle ending at height 0.29 is passable");
        unit.data.pos += Vector3.up * .05f;
        map.updateCtrl.RefreshObjectOverlap(unit);
        Check(coverage.SetEquals(NavigationCoverage(unit)), "crossing the obstacle height threshold retains the exact same covered cells");
        Check(!Node(3, 0, 3).objectBlocked && (Node(3, 0, 3).objectBlockedDirections & 1) != 0
            && !Node(3, 0, 3).links.Contains(Node(4, 0, 3)) && !Node(4, 0, 3).links.Contains(Node(3, 0, 3)),
            "a same-coverage move above 0.3 immediately removes both directions of the blocked edge");
        Check(Changed(versions).Count > 0 && !Changed(versions).Contains(Node(0, 0, 0)), "real edge changes rebuild locally without touching distant cells");
        EqualFull("same-cell directional change");
        versions = Versions(); map.updateCtrl.RefreshObjectOverlap(unit);
        Check(Changed(versions).Count == 0, "repeating an identical edge update reuses published links");
        unit.data.isObstacle = false; map.updateCtrl.RefreshObjectOverlap(unit);
        Check(!HasNavigationCoverage(unit) && Node(3, 0, 3).links.Contains(Node(4, 0, 3)), "disabling collision clears the old indexed blocker immediately");
        versions = Versions(); unit.data.pos += Vector3.right * .01f; map.updateCtrl.RefreshObjectOverlap(unit);
        Check(Changed(versions).Count == 0, "subsequent disabled moves do not enter navigation refresh");
        EqualFull("collision disabled");
    }

    private static void PassOnlyChangeAndDisable()
    {
        Grid();
        var tile = Tile(3, 0, 3); tile.SetPassTypes(new[] { 77 });
        map.navigationCtrl.RefreshObjectTiles(new[] { tile });
        var versions = Versions();
        var bridge = Unit(new Vector3(3, .1f, 3), new Vector3(.2f, .1f, .2f), true);
        map.updateCtrl.RefreshObjectOverlap(bridge);
        Check(!tile.passTypes.Contains(77) && Node(3, 0, 3).passTypes.Count == 0 && !Node(3, 0, 3).objectBlocked,
            "an enabled low bridge updates the live traversal requirements without blocking its central cell");
        Check(Changed(versions).Count == 0, "pass-type-only changes update live requirements without rebuilding geometry-only links");
        bridge.data.isObstacle = false; map.updateCtrl.RefreshObjectOverlap(bridge);
        Check(tile.passTypes.Contains(77) && Node(3, 0, 3).passTypes.Contains(77) && !HasNavigationCoverage(bridge),
            "disabling a bridge restores Tile and Nav pass requirements and removes its physical navigation index");
        Check(Changed(versions).Count == 0, "restoring pass requirements alone does not clear link lists");
        EqualFull("pass coverage disabled");
    }

    private static void TerrainStateAndReset()
    {
        Grid();
        var tile = Tile(3, 0, 3); tile.data.pos += Vector3.up * .6f;
        map.navigationCtrl.RefreshTerrainTiles(new[] { tile.data.mapPos });
        Check(!Node(2, 0, 3).links.Contains(Node(3, 0, 3)), "cached Object-link comparisons do not suppress terrain height changes");
        EqualFull("terrain height change");
        var versions = Versions(); map.navigationCtrl.RefreshObjectTiles(new[] { tile });
        Check(Changed(versions).Count == 0, "terrain rebuilding publishes the new baseline for later Object refreshes");

        Grid();
        var solid = Go("MapPrefab$mapground"); solid.SetActive(false);
        var body = solid.AddComponent<BoxCollider>(); body.center = Vector3.down * .5f; pool.AddPool(solid);
        var roof = Tile(3, 1, 3); roof.data.prefabName = solid.name;
        map.navigationCtrl.RefreshTerrainTiles(new[] { roof.data.mapPos });
        Check(!Node(3, 0, 3).links.Any(), "solid upper terrain blocks the underlying cell");
        roof.data.prefabName = groundPrefab;
        map.navigationCtrl.RefreshTerrainTiles(new[] { roof.data.mapPos });
        Check(Node(3, 0, 3).links.Contains(Node(2, 0, 3)), "solid-ground state changes invalidate an otherwise identical cached node");
        EqualFull("solid volume removed");
        var previous = Node(3, 0, 3);
        Grid();
        Check(!ReferenceEquals(previous, Node(3, 0, 3)) && Node(3, 0, 3).links.Count > 0, "a new map creates independent published state and links");
    }

    private static void ProductCollisionSynchronization()
    {
        var unit = Unit(new Vector3(3, .55f, 3), Vector3.one * .1f, true);
        var product = ObjectProductForm.GetDataByJo(new JObject()); product.uid = -1;
        product.name = "navigation-state-product"; product.collision = false;
        product.model = new MapModelForm.Data(0, new List<int> { 1 }, new List<Vector3> { Vector3.down * .5f },
            new List<Vector3> { Vector3.one * .1f }, new List<List<int>> { new List<int>() }, .1f, false, 1f);
        product.EnsureDirectionData(); ObjectProductForm.AddData(product);
        unit.productInfo = (product.uid, -1);
        var ja = new JArray(ObjectUnitForm.GetJoByData(unit.data)).ToString();
        var data = new GameMapData { mainData = new MapMainForm.Data(1, Vector3.one,
            new Vector3Int(7, 3, 7), Vector3Int.one, "", ja, "", "") };
        // Set explicitly because MapMain's generated constructor order is not a persistence shortcut.
        data.mainData.objectJa = ja;
        Check(!data.GetObjectDatasByJa(ja)[0].isObstacle, "scene loading applies current Product collision before any navigation build");
        appearance = new GameMapController(null);
        product.collision = true; appearance.RefreshObjectWangTileAppearances(product.uid);
        Check(unit.data.isObstacle && Node(3, 0, 3).objectBlocked, "explicit Product appearance/config refresh enables collision and indexes its blocker");
        product.collision = false; appearance.RefreshObjectWangTileAppearances(product.uid);
        Check(!unit.data.isObstacle && !Node(3, 0, 3).objectBlocked && !HasNavigationCoverage(unit),
            "config refresh disables collision and clears the previously published obstacle");
    }

    private static bool HasNavigationCoverage(ObjectUnit unit) => NavigationCoverage(unit).Count > 0;
    private static HashSet<TileUnit> NavigationCoverage(ObjectUnit unit)
    {
        var field = typeof(MapUpdateController).GetField("objectNavigationTileDic", BindingFlags.NonPublic | BindingFlags.Instance);
        var index = (Z_DoubleDictionary.DoubleDictionary<ObjectUnit, TileUnit>)field.GetValue(map.updateCtrl);
        return index.TryGet(unit, out var rows) ? new HashSet<TileUnit>(rows) : new HashSet<TileUnit>();
    }
    private static Dictionary<NavUnit, int> Versions() => map.navigationCtrl.navUnits.Values.ToDictionary(n => n, n => (int)ListVersion.GetValue(n.links));
    private static HashSet<NavUnit> Changed(Dictionary<NavUnit, int> versions) => new HashSet<NavUnit>(versions.Where(p => (int)ListVersion.GetValue(p.Key.links) != p.Value).Select(p => p.Key));
    private static Dictionary<Vector3Int, string> GraphState() => map.navigationCtrl.navUnits.Values.ToDictionary(n => n.pos, n =>
        n.objectBlocked + "/" + n.objectBlockedDirections + "/" + string.Join(",", n.dirMaxY.Select(y => y.ToString("R", CultureInfo.InvariantCulture)))
        + "/" + string.Join(",", n.passTypes.OrderBy(x => x)) + "/" + string.Join(";", n.links.Select(l => l.pos.ToString()).OrderBy(x => x)));
    private static bool SameState(Dictionary<Vector3Int, string> a, Dictionary<Vector3Int, string> b) => a.Count == b.Count && a.All(p => b.TryGetValue(p.Key, out var value) && value == p.Value);
    private static void EqualFull(string reason) { var expected = GraphState(); map.navigationCtrl.RebuildNow(); Check(SameState(expected, GraphState()), "incremental state agrees with a full build: " + reason); }
    private static TileUnit Tile(int x, int y, int z) => map.utilCtrl.GetTileData(x, y, z).unit;
    private static NavUnit Node(int x, int y, int z) => map.navigationCtrl.navUnits[(x, y, z)];

    private static ObjectUnit Unit(Vector3 position, Vector3 size, bool collision)
    {
        var root = Go("navigation-state-object-" + nextUid); root.SetActive(false); root.AddComponent<ObjectInstance>();
        var model = Go("model"); model.transform.SetParent(root.transform, false); model.transform.localScale = size;
        model.AddComponent<BoxCollider>(); pool.AddPool(root);
        var row = ObjectUnitForm.GetDataByJo(new JObject()); row.uid = nextUid++; row.isObstacle = collision;
        row.prefabName = root.name; row.pos = position; row.scale = Vector3.one; row.euler = Vector3.zero;
        ObjectUnitForm.AddData(row); return row.unit;
    }

    private static void Grid()
    {
        ObjectUnitForm.Clear(); TileUnitForm.Clear();
        map.enable = true; map.utilCtrl = new MapUtilController(map); map.updateCtrl = new MapUpdateController(map);
        map.data = new MapInfo { mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(7, 3, 7), Vector3Int.one, "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(), mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>() };
        for (int x = 0; x < 7; x++) for (int y = 0; y < 2; y++) for (int z = 0; z < 7; z++)
        {
            var row = TileUnitForm.GetDataByJo(new JObject()); row.uid = nextUid++; row.mapPos = new Vector3Int(x, y, z);
            row.pos = row.mapPos; row.scale = Vector3.one; row.prefabName = groundPrefab;
            TileUnitForm.AddData(row); map.data.RegisterMap(row);
        }
        map.navigationCtrl = new NavigationController(map); map.navigationCtrl.Build();
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var go = Go(typeof(T).Name); go.SetActive(false); var value = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value); return value;
    }
    private static GameObject Go(string name) { var go = new GameObject(name); owned.Add(go); return go; }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
