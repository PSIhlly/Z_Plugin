using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Mesh;
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
            Coverage();
            Visibility();
            TileFirstVisibility();
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
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        map.enable = true;
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
                UpdateType.ShowOnly, new List<int>(), "", false, false, 0);
            map.data.RegisterMap(tile);
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
            UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
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
            instance.renderers[i].SetPropertyBlock(block);
        }

        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        map.updateCtrl.curCenterPos = new Vector3(5, 0, 5);
        map.updateCtrl.curTileLst.Add(tile.data);
        var update = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        update.Invoke(map.updateCtrl, null);
        Require(ReadShow(instance, 0) == 0.5f, "occlusion final half opacity");
        int reads = instance.rendererReads;
        update.Invoke(map.updateCtrl, null);
        Require(instance.rendererReads == reads, "unchanged frame skips renderer submissions");

        instance.displayLayer = 0;
        update.Invoke(map.updateCtrl, null);
        for (int i = 0; i < 6; i++)
        {
            instance.renderers[i].GetPropertyBlock(block);
            Require(block.GetFloat("_Show") == (i == 0 || i == 3 ? 0.5f : 0f), "normal/front layer pairing");
            Require(block.GetFloat("_RegressionTextureProperty") == i + 10, "other property preserved per renderer");
        }
        map.updateCtrl.curCenterPos = new Vector3(5, 4.5f, 5);
        update.Invoke(map.updateCtrl, null);
        Require(ReadShow(instance, 0) == 1f, "departed occlusion restored");
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
        Require(final[high] == 0.5f && final[obj] == 0.5f && final[item] == 0.5f,
            "attached units inherit final BFS degree instead of Tile initialization order");
        Require(final[character] == 1 && ReadShow(characterIns, 0) == 1,
            "character visibility uses owner, not collision coverage");
        Require(final[tall] == 1 && ReadShow(tallIns, 0) == 1, "out-of-view Object owner falls back to opaque");
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
        Require(ReadShow(characterIns, 0) == 0.5f && ReadShow(itemIns, 0) == 1,
            "moving ownership refreshes degree even outside current Tile set");

        var late = new ItemUnitForm.Data(9106, "", source.name, high.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.itemTileDic.Add(late, high);
        ctrl.curItemLst.Add(late.data);
        update();
        var lateIns = Probe(late);
        update();
        Require(ReadShow(lateIns, 0) == 0.5f, "newly shown instance receives previously collected degree");

        DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
        // Special visual-footprint candidates must work even outside the visible list.
        ctrl.curObjectLst.Remove(tall.data);
        update();
        Require(ReadShow(tallIns, 0) == 0.5f, "visual coverage occludes tall Object with offscreen owner");
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
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
        Require(ReadShow(objIns, 0) == 0.5f, "visibility rebuilds after End and re-entry");
        ctrl.End();
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
            ctrl.OnEvent(new ObjectEvent { unit = obj, type = type });
            Require((ctrl.evts != null) == (type == MapEventType.Create || type == MapEventType.BoundaryTouch),
                "object lifecycle filter");
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
