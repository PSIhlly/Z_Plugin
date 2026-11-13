using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Run only in the runner's isolated project, without game scenes or user saves.
public static class ObjectCollisionTilesRegression
{
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static MapManager map;
    private static InstancePoolManager pool;
    private static TimeManager clock;
    private static string tilePrefab;
    private static int checks, nextUid = 81000;

    public static void Run()
    {
        try
        {
            foreach (var type in new[] { typeof(UnitForm), typeof(TileUnitForm), typeof(ObjectUnitForm),
                         typeof(MapMainForm), typeof(ProductForm), typeof(ObjectProductForm) })
            {
                type.GetMethod("Register", BindingFlags.NonPublic | BindingFlags.Static)?.Invoke(null, null);
                type.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }
            map = Manager<MapManager>();
            pool = Manager<InstancePoolManager>();
            pool.defaultRoot = pool.transform;
            pool.Init();
            clock = Manager<TimeManager>();
            var tileRoot = Go("collision-empty-tile");
            tileRoot.SetActive(false);
            tileRoot.AddComponent<TileInstance>();
            pool.AddPool(tileRoot);
            tilePrefab = tileRoot.name;
            Grid(Vector3.one);
            SmallAndScaled();
            OffsetAndRotation();
            ContinuousSweep();
            SeparatePartsAndTriggers();
            SphereAndRotatedBox();
            EventDispatch();
            Grid(new Vector3(2, 3, .5f));
            var nonUnit = Unit(new Vector3(4, 1.5f, 1), out _, out _);
            Check(Columns(nonUnit, new Vector3(4, 0, 0)).SetEquals(new[] { (2, 2), (3, 2), (4, 2) }),
                "nonuniform map cell sizes convert world displacement and geometry to the correct columns");
            Debug.Log("OBJECT_COLLISION_TILES_REGRESSION_PASS checks=" + checks);
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

    private static void SmallAndScaled()
    {
        var unit = Unit(new Vector3(2, .5f, 2), out _, out _);
        Check(Columns(unit, Vector3.right * .1f).SetEquals(new[] { (2, 2) }),
            "a small Object queries only its swept column rather than the surrounding nine columns");
        unit.data.scale = new Vector3(16, 1, 8);
        var scaled = Columns(unit, Vector3.zero);
        var expected = new HashSet<(int, int)>();
        for (int x = 0; x <= 4; x++)
        for (int z = 1; z <= 3; z++) expected.Add((x, z));
        Check(scaled.SetEquals(expected), "changing root scale expands collision candidates beyond one neighboring Tile");
        var tiles = map.utilCtrl.GetObjectCollisionTiles(unit, Vector3.zero, CollideType.All);
        Check(tiles.Count == tiles.Distinct().Count(), "overlapping meshes and floor fallback return each Tile once");
    }

    private static void OffsetAndRotation()
    {
        var unit = Unit(new Vector3(2, .5f, 2), out var body, out var trigger);
        body.center = trigger.center = new Vector3(3, 0, 0);
        unit.data.euler = new Vector3(0, 90, 0);
        Check(Columns(unit, Vector3.zero).SetEquals(new[] { (2, -1) }),
            "rotated Collider center offsets choose the physical footprint, excluding the anchor column");
        unit.data.pos += Vector3.right * 2;
        Check(Columns(unit, Vector3.zero).SetEquals(new[] { (4, -1) }),
            "translated cached geometry immediately changes the footprint");
    }

    private static void ContinuousSweep()
    {
        var unit = Unit(new Vector3(0, .5f, 0), out _, out _);
        var forward = Columns(unit, Vector3.right * 4);
        Check(forward.SetEquals(Enumerable.Range(0, 5).Select(x => (x, 0))),
            "long movement includes intermediate columns, not only the two endpoints");
        unit.data.pos += Vector3.right * 4;
        Check(Columns(unit, Vector3.left * 4).SetEquals(forward), "negative displacement visits the same swept columns");
        unit.data.pos = new Vector3(0, .5f, 0);
        var diagonal = Columns(unit, new Vector3(4, 0, 4));
        Check(Enumerable.Range(0, 5).All(x => diagonal.Contains((x, x))), "diagonal sweep retains every crossed diagonal Tile");
        Check(!diagonal.Contains((0, 4)) && !diagonal.Contains((4, 0)) && !diagonal.Contains((0, 2)),
            "diagonal sweep excludes columns inside its enclosing rectangle but outside its path");
        unit.data.pos += new Vector3(4, 0, 4);
        Check(Columns(unit, new Vector3(-4, 0, -4)).SetEquals(diagonal), "diagonal sweep is direction symmetric");
    }

    private static void SeparatePartsAndTriggers()
    {
        var unit = Unit(new Vector3(0, .5f, 0), out var body, out var trigger);
        body.center = new Vector3(-3, 0, 0);
        trigger.center = new Vector3(3, 0, 0);
        Check(Columns(unit, Vector3.zero).SetEquals(new[] { (-3, 0), (3, 0) }),
            "separate Collider and Trigger footprints do not fill the empty area between them");
        Check(Columns(unit, Vector3.zero, CollideType.CollideOnly).SetEquals(new[] { (-3, 0) })
            && Columns(unit, Vector3.zero, CollideType.TriggerOnly).SetEquals(new[] { (3, 0) }),
            "Collider and Trigger selection respects CollideType");
        trigger.enabled = false;
        unit.InvalidateCollisionGeometry();
        Check(Columns(unit, Vector3.zero).SetEquals(new[] { (-3, 0) }), "disabled Trigger footprints are excluded");
        body.enabled = false;
        unit.InvalidateCollisionGeometry();
        Check(Columns(unit, Vector3.right).Count == 0, "Objects without physical or Trigger meshes add no candidates");
    }

    private static void SphereAndRotatedBox()
    {
        var unit = Unit(new Vector3(0, .5f, 0), out var body, out var trigger);
        body.size = trigger.size = new Vector3(4, .2f, .2f);
        unit.data.euler = new Vector3(0, 45, 0);
        var columns = Columns(unit, Vector3.zero);
        Check(columns.Contains((1, -1)) && !columns.Contains((1, 1)),
            "a rotated thin Box excludes the empty corners of its AABB");

        body.enabled = trigger.enabled = false;
        var sphere = body.gameObject.AddComponent<SphereCollider>();
        sphere.radius = .6f;
        unit.InvalidateCollisionGeometry();
        columns = Columns(unit, Vector3.zero);
        Check(columns.SetEquals(new[] { (0, 0), (-1, 0), (1, 0), (0, -1), (0, 1) }),
            "a rotated Sphere uses its physical radius and rejects untouched corner Tiles");
        columns = Columns(unit, new Vector3(3, 0, 3));
        Check(columns.Contains((1, 1)) && columns.Contains((3, 3)) && !columns.Contains((0, 3)),
            "Sphere capsule sweep includes intermediate contacts and rejects diagonal rectangle corners");
    }

    private static void EventDispatch()
    {
        var mover = Unit(new Vector3(0, .5f, 0), out var body, out var trigger);
        body.size = trigger.size = new Vector3(4, .2f, .2f);
        var target = Unit(new Vector3(3, .5f, 0), out _, out _);
        map.updateCtrl.objectTileDic.Add(mover, map.utilCtrl.GetTile(0, 0, 0));
        map.updateCtrl.objectTileDic.Add(target, map.utilCtrl.GetTile(3, 0, 0));
        map.updateCtrl.objectTileDic.Add(target, map.utilCtrl.GetTile(3, 1, 0));
        map.updateCtrl.CheckCollideEvent(mover, Vector3.right * 1.2f);
        clock.LateUpdate();
        Check(mover.data.collidingUnitUid.Count(id => id == target.data.uid) == 1
            && target.data.collidingUnitUid.Contains(mover.data.uid),
            "CheckCollideEvent reaches a large Object's remote Trigger and deduplicates multiply indexed targets");
        mover.data.pos += Vector3.right * 1.2f;
        map.updateCtrl.CheckCollideEvent(mover, Vector3.left * 1.2f);
        clock.LateUpdate();
        // Keep the existing Cube SAT state semantics: departure is reported
        // once the next sweep starts outside the former overlapping pair.
        mover.data.pos -= Vector3.right * 1.2f;
        Check(!Columns(mover, Vector3.left * .1f).Contains((3, 0)),
            "a former contact's Tile stays outside the new swept Tile set");
        map.updateCtrl.CheckCollideEvent(mover, Vector3.left * .1f);
        clock.LateUpdate();
        Check(!mover.data.collidingUnitUid.Contains(target.data.uid) && !target.data.collidingUnitUid.Contains(mover.data.uid),
            "return movement preserves the delayed Trigger Exit state chain");
    }

    private static HashSet<(int, int)> Columns(ObjectUnit unit, Vector3 step, CollideType type = CollideType.All) =>
        new HashSet<(int, int)>(map.utilCtrl.GetObjectCollisionTiles(unit, step, type).Select(t => (t.data.mapPos.x, t.data.mapPos.z)));

    private static ObjectUnit Unit(Vector3 position, out BoxCollider body, out BoxCollider trigger)
    {
        var root = Go("collision-object-" + nextUid);
        root.SetActive(false);
        root.AddComponent<ObjectInstance>();
        var part = Go("body");
        part.transform.SetParent(root.transform, false);
        body = part.AddComponent<BoxCollider>();
        body.size = Vector3.one * .2f;
        trigger = part.AddComponent<BoxCollider>();
        trigger.isTrigger = true;
        trigger.size = body.size;
        pool.AddPool(root);
        var row = ObjectUnitForm.GetDataByJo(new JObject());
        row.uid = nextUid++;
        row.prefabName = root.name;
        row.pos = position;
        row.euler = Vector3.zero;
        row.scale = Vector3.one;
        row.collidingUnitUid = new List<int>();
        ObjectUnitForm.AddData(row);
        return row.unit;
    }

    private static void Grid(Vector3 size)
    {
        map.enable = true;
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, size, new Vector3Int(30, 5, 30), new Vector3Int(8, 3, 8), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        for (int x = -10; x <= 14; x++)
        for (int z = -10; z <= 14; z++)
        for (int y = 0; y <= 3; y++)
        {
            var row = TileUnitForm.GetDataByJo(new JObject());
            row.uid = nextUid++;
            row.mapPos = new Vector3Int(x, y, z);
            row.pos = Vector3.Scale(row.mapPos, size);
            row.prefabName = tilePrefab;
            row.scale = Vector3.one;
            map.data.RegisterMap(row);
        }
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var go = Go(typeof(T).Name);
        go.SetActive(false);
        var value = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.NonPublic | BindingFlags.Static).SetValue(null, value);
        return value;
    }

    private static GameObject Go(string name) { var go = new GameObject(name); owned.Add(go); return go; }
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); checks++; }
}
