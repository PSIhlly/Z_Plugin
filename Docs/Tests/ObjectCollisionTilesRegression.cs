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
using Z_Math;
using Z_Mesh;
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
            StateOnlyQueriesAndRotationBuffers();
            CastQueries();
            NoopEventsAndWorkReuse();
            Grid(new Vector3(2, 3, .5f));
            var nonUnit = Unit(new Vector3(4, 1.5f, 1), out _, out _);
            Check(Columns(nonUnit, new Vector3(4, 0, 0)).SetEquals(new[] { (2, 2), (3, 2), (4, 2) }),
                "nonuniform map cell sizes convert world displacement and geometry to the correct columns");
            SubWorldUnitCast();
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

    private static void StateOnlyQueriesAndRotationBuffers()
    {
        var source = Unit(new Vector3(1, .8f, 2), out var body, out var trigger);
        body.center = new Vector3(.3f, .1f, -.2f); body.size = new Vector3(.8f, .6f, 1.4f);
        trigger.size = Vector3.one * 1.5f;
        var sphere = body.gameObject.AddComponent<SphereCollider>(); sphere.radius = .3f;
        var target = Unit(new Vector3(1.2f, .9f, 2.3f), out _, out var targetTrigger);
        var targetSphere = targetTrigger.gameObject.AddComponent<SphereCollider>(); targetSphere.isTrigger = true; targetSphere.radius = .5f;
        source.data.scale = new Vector3(1.7f, .6f, 1.2f);
        target.data.scale = new Vector3(.8f, 1.2f, 1.4f);
        var sourceMeshes = source.GetMeshes(CollideType.All);
        var points = sourceMeshes.Select(mesh => mesh.positions).ToArray();
        var method = typeof(MapUpdateController).GetMethod("CheckEventCollision", BindingFlags.Instance | BindingFlags.NonPublic);
        var work = Activator.CreateInstance(method.GetParameters()[4].ParameterType, true);
        var random = new System.Random(77103);
        for (int i = 0; i < 128; i++)
        {
            source.data.euler = new Vector3((float)random.NextDouble() * 120, (float)random.NextDouble() * 360, (float)random.NextDouble() * 90);
            target.data.euler = new Vector3(0, (float)random.NextDouble() * 360, 0);
            var actual = source.GetMeshes(CollideType.All);
            var expected = map.utilCtrl.GetCollidersMesh(source.prefab, source.data.pos, source.data.euler, source.data.scale, CollideType.CollideOnly);
            expected.AddRange(map.utilCtrl.GetCollidersMesh(source.prefab, source.data.pos, source.data.euler, source.data.scale, CollideType.TriggerOnly));
            Check(ReferenceEquals(actual, sourceMeshes) && actual.Select((mesh, j) => ReferenceEquals(mesh.positions, points[j])).All(value => value),
                "pure rotation retains MeshInfo lists and vertex buffers");
            for (int m = 0; m < actual.Count; m++)
            {
                Check((actual[m].center - expected[m].center).sqrMagnitude < .000001f, "rotated canonical centers match fresh Collider geometry");
                for (int v = 0; v < actual[m].positions.Length; v++)
                    Check((actual[m].positions[v] - expected[m].positions[v]).sqrMagnitude < .000001f, "rotated canonical vertices match fresh Collider geometry");
            }
            Vector3 step = new Vector3((float)random.NextDouble() * 4 - 2, (float)random.NextDouble() - .5f, (float)random.NextDouble() * 4 - 2);
            foreach (bool oldIn in new[] { false, true })
            foreach (var kind in new[] { CollideType.CollideOnly, CollideType.TriggerOnly, CollideType.All })
            {
                source.data.collidingUnitUid.Clear(); if (oldIn) source.data.collidingUnitUid.Add(target.data.uid);
                var legacy = new Graph.IntersectAssisant(oldIn);
                foreach (var mesh in source.GetMeshes(kind))
                {
                    map.updateCtrl.CheckCollide(mesh, target, step, kind, out _, out var inner);
                    legacy.Merge(inner);
                }
                var state = (Graph.IntersectType)method.Invoke(map.updateCtrl, new object[] { source, target, step, kind, work });
                Check(state == legacy.GetRes(), "state-only dispatch preserves Add/Merge and old-contact semantics");
            }
        }
        source.InvalidateCollisionGeometry();
        Check(!ReferenceEquals(source.GetMeshes(CollideType.All), sourceMeshes), "explicit configuration invalidation still rebuilds geometry");
    }

    private static List<MapUnit> LegacyCast(Vector3 from, Vector3 to, float radius)
    {
        var result = new List<MapUnit>(); var seen = new HashSet<int>(); var columns = new HashSet<(int, int)>();
        var sphere = Z_Mesh.Mesh.GetMesh(from, radius, Vector3.zero, Vector3.one);
        var end = Z_Mesh.Mesh.GetMesh(to, radius, Vector3.zero, Vector3.one);
        foreach (var point in Graph.GetRoughOverlapIntPos(sphere.positions.Concat(end.positions).ToArray()))
        {
            var pos = map.utilCtrl.RealPos2MapPosInt(point);
            if (!columns.Add((pos.x, pos.z)) || !map.data.mapXZ2Y.TryGetValue((pos.x, pos.z), out var levels)) continue;
            foreach (int y in levels)
            {
                var tile = map.data.maps[(pos.x, y, pos.z)].unit;
                var units = new List<MapUnit> { tile };
                if (map.updateCtrl.objectTileDic.TryGet(tile, out var objects)) units.AddRange(objects);
                if (map.updateCtrl.characterOverlapTileDic.TryGet(tile, out var characters)) units.AddRange(characters);
                foreach (var unit in units)
                {
                    if (!seen.Add(unit.data.uid)) continue;
                    map.updateCtrl.CheckCollide(sphere, unit, to - from, CollideType.CollideOnly, out _, out var assist);
                    if (assist.GetRes() != Graph.IntersectType.None) result.Add(unit);
                }
            }
        }
        return result;
    }

    private static void CastQueries()
    {
        var near = Unit(new Vector3(1, .5f, 0), out _, out _); near.data.isObstacle = true;
        var far = Unit(new Vector3(4, .5f, 0), out _, out _); far.data.isObstacle = true;
        map.updateCtrl.objectTileDic.Add(near, map.utilCtrl.GetTile(1, 0, 0));
        map.updateCtrl.objectTileDic.Add(far, map.utilCtrl.GetTile(4, 0, 0));
        far.GetMeshes(CollideType.All); far._zMeshes = null;
        Check(map.utilCtrl.HasBlockingCast(new Vector3(0, .5f, 0), new Vector3(5, .5f, 0), .05f) && far._zMeshes == null,
            "boolean cast stops at the first actual obstacle before querying later geometry");
        near.data.isObstacle = false;
        Check(map.utilCtrl.HasBlockingCast(new Vector3(0, .5f, 0), new Vector3(5, .5f, 0), .05f) && far._zMeshes != null,
            "boolean cast skips non-obstacle Objects and continues to a blocking Object");
        var random = new System.Random(41107);
        for (int i = 0; i < 80; i++)
        {
            var from = new Vector3((float)random.NextDouble() * 12 - 6, .5f, (float)random.NextDouble() * 12 - 6);
            var to = new Vector3((float)random.NextDouble() * 12 - 6, .5f, (float)random.NextDouble() * 12 - 6);
            float radius = (float)random.NextDouble() * .6f;
            var legacy = LegacyCast(from, to, radius);
            var current = map.utilCtrl.CaptureCast(from, to, radius);
            Check(new HashSet<int>(legacy.Select(unit => unit.data.uid)).SetEquals(current.Select(unit => unit.data.uid)),
                "buffered full cast preserves legacy sweep hits in both directions and negative coordinates");
            Check(map.utilCtrl.HasBlockingCast(from, to, radius) == legacy.Any(unit => unit is TileUnit || unit is ObjectUnit obj && obj.data.isObstacle),
                "boolean early-stop result equals filtering the full hit list");
        }
        map.utilCtrl.CaptureCast(Vector3.zero, Vector3.one, .1f, out var ids, out var columns);
        int count = ids.Count, columnCount = columns.Count;
        map.utilCtrl.CaptureCast(Vector3.one * 3, Vector3.zero, .2f);
        Check(ids.Count == count && columns.Count == columnCount, "diagnostic snapshots remain caller-owned across pooled casts");
    }

    private static void SubWorldUnitCast()
    {
        Grid(Vector3.one * .25f);
        var middle = Unit(new Vector3(.5f, .5f, .5f), out _, out _);
        middle.data.isObstacle = true;
        map.updateCtrl.objectTileDic.Add(middle, map.utilCtrl.GetTile(2, 2, 2));
        Check(map.utilCtrl.HasBlockingCast(new Vector3(0, .5f, .5f), new Vector3(1, .5f, .5f), .05f),
            "sub-world-unit cells retain intermediate columns skipped by integer world sampling");
        Check(map.utilCtrl.CaptureCast(new Vector3(1, .5f, .5f), new Vector3(0, .5f, .5f), .05f).Contains(middle),
            "full cast retains the same small-cell coverage in reverse");
    }

    private static void NoopEventsAndWorkReuse()
    {
        var a = Unit(new Vector3(5, .5f, 5), out _, out _); var b = Unit(new Vector3(6, .5f, 5), out _, out _);
        var queue = (System.Collections.IList)typeof(TimeManager).GetField("CurLateUpdateWithoutCheckList", BindingFlags.Static | BindingFlags.NonPublic).GetValue(null);
        int before = queue.Count;
        var manage = typeof(MapUpdateController).GetMethod("ManageTriggerEvent", BindingFlags.Instance | BindingFlags.NonPublic);
        manage.Invoke(map.updateCtrl, new object[] { a, b, Graph.IntersectType.Inner });
        manage.Invoke(map.updateCtrl, new object[] { a, b, Graph.IntersectType.None });
        Check(queue.Count == before, "None/Inner do not allocate/enqueue a no-op deferred transition");
        manage.Invoke(map.updateCtrl, new object[] { a, b, Graph.IntersectType.Cross }); clock.LateUpdate();
        Check(!a.data.collidingUnitUid.Contains(b.data.uid), "Cross still performs paired deferred Enter and Exit");
        int objectKeys = map.updateCtrl.objectTileDic.GetDicT2().Count;
        int itemKeys = map.updateCtrl.itemTileDic.GetDicT2().Count;
        int characterKeys = map.updateCtrl.characterOverlapTileDic.GetDicT2().Count;
        map.updateCtrl.CheckCollideEvent(a, Vector3.right * .1f); clock.LateUpdate();
        var stack = typeof(MapUpdateController).GetField("collisionEventWork", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(map.updateCtrl);
        var first = stack.GetType().GetMethod("Peek").Invoke(stack, null);
        map.updateCtrl.CheckCollideEvent(b, Vector3.left * .1f); clock.LateUpdate();
        Check(ReferenceEquals(first, stack.GetType().GetMethod("Peek").Invoke(stack, null)), "successive event queries reuse the same cleared work buffers");
        Check(objectKeys == map.updateCtrl.objectTileDic.GetDicT2().Count
            && itemKeys == map.updateCtrl.itemTileDic.GetDicT2().Count
            && characterKeys == map.updateCtrl.characterOverlapTileDic.GetDicT2().Count,
            "event candidate reads never insert empty reverse-index lists");
    }

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
