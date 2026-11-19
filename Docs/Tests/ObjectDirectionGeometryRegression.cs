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
using Z_Mesh;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Appearance = Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectAppearance;
using Object = UnityEngine.Object;
using static Z_DesignStyle.Z_DoubleDictionary;

// Run only in the runner's isolated project; never open game scenes or saves.
public static class ObjectDirectionGeometryRegression
{
    private static readonly AnimDirecton[] Directions =
        { AnimDirecton.Fixed, AnimDirecton.Up, AnimDirecton.Down, AnimDirecton.Left, AnimDirecton.Right };
    private const int CubeId = 89401;
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly List<GameObject> objects = new List<GameObject>();
    private static int checks;
    private static int nextUnit = 89420;
    private static MapManager map;
    private static InstancePoolManager pool;
    private static GameUtilController util;
    private static GameMapController appearance;
    private static Material material;

    public static void Run()
    {
        bool oldPlaying = DynamicGlobalSettings.playing;
        CameraMode oldCamera = DynamicGlobalSettings.cameraMode;
        try
        {
            RegisterForms();
            CheckLegacyAndPartialData();
            CheckPersistenceAndCopy();
            CheckEditorGeometry();
            SetUpRuntime();
            CheckPreviewsAndOffscreenGeometry();
            CheckLiveInstancesAndPoolReuse();
            CheckZeroToNonzeroDirection();
            CheckFittedRearAnchoredDirections();
            CheckDirectionOverlapRefresh();
            CheckGeneratedInitialization();
            Debug.Log("OBJECT_DIRECTION_GEOMETRY_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            DynamicGlobalSettings.playing = oldPlaying;
            DynamicGlobalSettings.cameraMode = oldCamera;
            // The owned manager root destroys every cloned child below. Avoid
            // production pool.Destroy's deferred Destroy calls in EditMode.
            if (pool != null && pool.pools != null) pool.pools.Clear();
            for (int i = objects.Count - 1; i >= 0; i--)
                if (objects[i] != null) Object.DestroyImmediate(objects[i]);
            if (material != null) Object.DestroyImmediate(material);
        }
    }

    private static void RegisterForms()
    {
        foreach (var type in new[] { typeof(ProductForm), typeof(ObjectProductForm), typeof(MapModelForm),
                     typeof(EventTriggerForm), typeof(MapObjectParamForm), typeof(GameParamForm), typeof(ParamForm),
                     typeof(MapMainForm), typeof(UnitForm), typeof(TileUnitForm), typeof(ObjectUnitForm),
                     typeof(TexAssetForm), typeof(GameObjectAssetForm) })
            type.GetMethod("Register", PrivateStatic)?.Invoke(null, null);
        ProductForm.InitInternal();
        ObjectProductForm.InitInternal();
        MapModelForm.InitInternal();
        MapMainForm.InitInternal();
        UnitForm.InitInternal();
        TileUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        TexAssetForm.InitInternal();
        GameObjectAssetForm.InitInternal();
    }

    private static ObjectProductForm.Data NewProduct()
    {
        var product = ObjectProductForm.GetDataByJo(new JObject());
        product.uid = -1;
        product.name = "direction-geometry";
        product.faceType = FaceType.FourDirection;
        product.model = new MapModelForm.Data(0, new List<int> { CubeId },
            new List<Vector3> { new Vector3(.2f, .3f, -.4f) },
            new List<Vector3> { new Vector3(1.2f, .8f, 2.4f) },
            new List<List<int>> { new List<int>() }, .1f, true, 1f);
        product.animClip = new Dictionary<AnimDirecton, List<int>>();
        product.modelSizes = new Dictionary<AnimDirecton, Vector3>();
        product.modelPositions = new Dictionary<AnimDirecton, Vector3>();
        product.boundsCollision = false;
        product.centerCollider = true;
        return product;
    }

    private static void CheckLegacyAndPartialData()
    {
        var source = NewProduct();
        var legacy = ObjectProductForm.GetJoByData(source);
        legacy.Remove("modelSizes");
        legacy.Remove("modelPositions");
        var loaded = ObjectProductForm.GetDataByJo(legacy);
        foreach (var dir in Directions)
        {
            Near(loaded.GetModelSize(dir), source.model.subPrefabUnitScale[0], "old JSON uses legacy size before normalization");
            Near(loaded.GetModelPosition(dir), source.model.subPrefabUnitPos[0], "old JSON uses legacy position before normalization");
        }
        loaded.EnsureDirectionData();
        Check(loaded.modelSizes.Count == 5 && loaded.modelPositions.Count == 5,
            "normalization seeds all five independent geometry entries");
        var before = loaded.GetModelSize(AnimDirecton.Down);
        loaded.SetModelSize(AnimDirecton.Up, new Vector3(3, .1f, 4));
        loaded.SetModelPosition(AnimDirecton.Up, new Vector3(-.4f, .7f, 1));
        Near(loaded.GetModelSize(AnimDirecton.Down), before, "editing one direction cannot alter another legacy-seeded direction");
        loaded.SyncLegacyModelTransform(AnimDirecton.Up);
        Near(loaded.model.subPrefabUnitScale[0], loaded.GetModelSize(AnimDirecton.Up), "legacy mirror uses the chosen direction");
        Near(loaded.GetModelSize(AnimDirecton.Fixed), before, "mirroring cannot overwrite any already seeded facing");

        var partial = NewProduct();
        partial.modelSizes[AnimDirecton.Left] = new Vector3(4, 2, .3f);
        partial.modelPositions[AnimDirecton.Right] = new Vector3(3, -2, 1);
        partial.EnsureDirectionData();
        Near(partial.GetModelSize(AnimDirecton.Left), new Vector3(4, 2, .3f), "partial size entry survives normalization");
        Near(partial.GetModelPosition(AnimDirecton.Right), new Vector3(3, -2, 1), "independent partial position entry survives normalization");
        Near(partial.GetModelSize(AnimDirecton.Up), partial.model.subPrefabUnitScale[0], "missing partial entry uses legacy rather than another facing");
        partial.model = null;
        partial.modelSizes = null;
        partial.modelPositions = null;
        Near(partial.GetModelSize(AnimDirecton.Down), Vector3.one, "absent model uses neutral size");
        Near(partial.GetModelPosition(AnimDirecton.Down), Vector3.zero, "absent model uses neutral position");

        var missingModel = ObjectProductForm.GetDataByJo(new JObject());
        var untouchedDefaults = ObjectProductForm.GetDataByJo(new JObject());
        Vector3 defaultSize = untouchedDefaults.GetModelSize(AnimDirecton.Fixed);
        missingModel.SetModelSize(AnimDirecton.Fixed, Vector3.one * 7);
        missingModel.SyncLegacyModelTransform(AnimDirecton.Fixed);
        Near(untouchedDefaults.GetModelSize(AnimDirecton.Fixed), defaultSize,
            "mirroring a legacy missing-model Product never changes shared Form defaults");

        foreach (var type in new[] { FaceType.Fixed, FaceType.Flexible, FaceType.FourDirection })
        {
            loaded.faceType = type;
            Check(loaded.GetDefaultAnimDirection() == (type == FaceType.FourDirection ? AnimDirecton.Up : AnimDirecton.Fixed),
                "default geometry follows the configured facing mode");
            foreach (var c in new[] { (yaw: 0f, dir: AnimDirecton.Up), (yaw: 90f, dir: AnimDirecton.Right),
                         (yaw: 180f, dir: AnimDirecton.Down), (yaw: 270f, dir: AnimDirecton.Left),
                         (yaw: 450f, dir: AnimDirecton.Right), (yaw: -90f, dir: AnimDirecton.Left) })
                Check(loaded.GetAnimDirection(c.yaw) == (type == FaceType.FourDirection ? c.dir : AnimDirecton.Fixed),
                    "Fixed/Flexible share Fixed; four-direction geometry uses the existing yaw mapping");
        }
        loaded.faceType = FaceType.FourDirection;
        Near(loaded.GetModelSize(loaded.GetAnimDirection(0)), new Vector3(3, .1f, 4),
            "switching back to four directions restores the authored Up size");
        Near(loaded.GetModelPosition(loaded.GetAnimDirection(0)), new Vector3(-.4f, .7f, 1),
            "switching back to four directions restores the authored Up position");
        loaded.faceType = FaceType.Flexible;
        Near(loaded.GetModelSize(loaded.GetAnimDirection(90)), before, "Flexible retains independent Fixed geometry");
    }

    private static void CheckPersistenceAndCopy()
    {
        var source = DirectionalProduct();
        var json = ObjectProductForm.GetJoByData(source);
        Check(json["modelSizes"] != null && json["modelPositions"] != null, "both dictionaries are saved fields");
        var restored = ObjectProductForm.GetDataByJo(json);
        foreach (var dir in Directions)
        {
            Near(restored.GetModelSize(dir), source.GetModelSize(dir), "directional size survives JSON round-trip");
            Near(restored.GetModelPosition(dir), source.GetModelPosition(dir), "directional position survives JSON round-trip");
        }
        var copy = source.Copy(false);
        Check(!ReferenceEquals(source.modelSizes, copy.modelSizes) && !ReferenceEquals(source.modelPositions, copy.modelPositions),
            "generated Copy owns both dictionary containers");
        copy.SetModelSize(AnimDirecton.Left, Vector3.one * 99);
        Check(source.GetModelSize(AnimDirecton.Left) != Vector3.one * 99, "copy editing cannot leak into prototype size");
        ObjectProductForm.AddData(source);
        var runtime = source.Copy(false);
        runtime.ToProduct(source.uid);
        Check(runtime.protoUid == source.uid && !ReferenceEquals(runtime.model, source.model)
            && !ReferenceEquals(runtime.modelSizes, source.modelSizes) && !ReferenceEquals(runtime.modelPositions, source.modelPositions),
            "runtime Product deeply owns legacy model and geometry dictionaries");
        runtime.SetModelPosition(AnimDirecton.Down, Vector3.one * 98);
        runtime.SyncLegacyModelTransform(AnimDirecton.Down);
        Check(source.GetModelPosition(AnimDirecton.Down) != Vector3.one * 98
            && source.model.subPrefabUnitPos[0] != Vector3.one * 98,
            "runtime edits and legacy mirroring cannot mutate authored Product");
    }

    private static void CheckEditorGeometry()
    {
        var product = DirectionalProduct();
        var editor = new Appearance.UiModStoryMapObjectObjectAppearanceModel { data = product };
        foreach (var dir in Directions)
        {
            editor.dir = dir;
            var other = dir == AnimDirecton.Up ? AnimDirecton.Down : AnimDirecton.Up;
            Vector3 otherSize = product.GetModelSize(other), otherPos = product.GetModelPosition(other);
            float width = editor.size.x, length = editor.size.z;
            foreach (float bottom in new[] { 0f, .7f, -.3f })
            {
                editor.posHeight = bottom;
                foreach (float height in new[] { .1f, 2f, .5f, 1f })
                {
                    editor.height = height;
                    Near(editor.posHeight, bottom, "editing height retains this direction's bottom-relative position");
                    Near(editor.size, new Vector3(width, height, length), "height editing preserves this direction's width/length");
                    Near(editor.position.y + .5f - height * .5f, bottom, "saved center offset keeps the actual model bottom");
                }
            }
            editor.SetHorizontalPosition(new Vector2(-.8f, .6f));
            Near(editor.position.x, -.8f, "axis updates this direction's X");
            Near(editor.position.z, .6f, "axis updates this direction's Z");
            Near(product.GetModelSize(other), otherSize, "editor height does not leak into another direction");
            Near(product.GetModelPosition(other), otherPos, "editor axis/height do not leak into another direction");
        }
    }

    private static void SetUpRuntime()
    {
        DynamicGlobalSettings.playing = false;
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        map = CreateManager<MapManager>();
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(20, 5, 20),
                new Vector3Int(8, 3, 8), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        pool = CreateManager<InstancePoolManager>();
        pool.defaultRoot = pool.transform;
        pool.Init();
        util = new GameUtilController(null);
        appearance = new GameMapController(null);
        var shader = Shader.Find("DisFadeCode");
        Check(shader != null, "isolated shader import supports the real composition path");
        material = new Material(shader);
        var cube = Go("direction-cube-source");
        cube.AddComponent<MeshRenderer>().sharedMaterial = material;
        var body = Go("body");
        body.transform.SetParent(cube.transform, false);
        body.AddComponent<BoxCollider>();
        GameObjectAssetForm.DataById[CubeId] = new GameObjectAssetForm.Data(CubeId, cube.name, null, null, null, cube, 0);
    }

    private static ObjectProductForm.Data DirectionalProduct()
    {
        var data = NewProduct();
        data.EnsureDirectionData();
        for (int i = 0; i < Directions.Length; i++)
        {
            data.SetModelSize(Directions[i], new Vector3(1 + .4f * i, .2f + .3f * i, 2.4f - .35f * i));
            data.SetModelPosition(Directions[i], new Vector3(-.2f + .13f * i, -.3f + .15f * i, .4f - .17f * i));
        }
        return data;
    }

    private static ObjectUnit NewUnit(ObjectProductForm.Data product, Vector3 pos, float yaw)
    {
        if (product.uid <= 0) ObjectProductForm.AddData(product);
        var data = ObjectUnitForm.GetDataByJo(new JObject());
        data.uid = nextUnit++;
        data.name = "direction-unit";
        data.pos = pos;
        data.euler = new Vector3(0, yaw, 0);
        data.scale = Vector3.one;
        data.unit.productInfo = (product.uid, -1);
        data.prefabName = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(ObjectProductForm.GetTemplateUid(product));
        ObjectUnitForm.AddData(data);
        return data.unit;
    }

    private static void CheckPreviewsAndOffscreenGeometry()
    {
        var product = DirectionalProduct();
        ObjectProductForm.AddData(product);
        string name = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid);
        var prefab = util.CombineNewObjectByPrefabs(name, product, true);
        objects.Add(prefab);
        pool.AddPool(prefab);
        Vector3 baselinePosition = prefab.transform.GetChild(0).localPosition;
        Vector3 baselineSize = prefab.transform.GetChild(0).localScale;
        var unit = NewUnit(product, new Vector3(4, 2, 5), 0);
        foreach (var c in new[] { (yaw: 0f, dir: AnimDirecton.Up), (yaw: 90f, dir: AnimDirecton.Right),
                     (yaw: 180f, dir: AnimDirecton.Down), (yaw: 270f, dir: AnimDirecton.Left),
                     (yaw: 450f, dir: AnimDirecton.Right), (yaw: -90f, dir: AnimDirecton.Left) })
        {
            unit.data.euler = new Vector3(0, c.yaw, 0);
            Check(unit.TryGetFirstPartTransform(out Vector3 position, out Vector3 size), "Object exposes its current direction without a live instance");
            Near(position, product.GetModelPosition(c.dir), "offscreen position uses current euler");
            Near(size, product.GetModelSize(c.dir), "offscreen size uses current euler");
            var preview = util.CombineNewObjectByPrefabs("direction-preview", product, false, previewDirection: c.dir);
            objects.Add(preview);
            Near(preview.transform.GetChild(0).localPosition, position + Vector3.up * .5f, "preview selects the requested direction's offset");
            Near(preview.transform.GetChild(0).localScale, size, "preview selects the requested direction's dimensions");
            foreach (var kind in new[] { CollideType.CollideOnly, CollideType.TriggerOnly })
                SameMeshes(unit.GetMeshes(kind), map.utilCtrl.GetCollidersMesh(preview, unit.data.pos, unit.data.euler, unit.data.scale, kind),
                    "offscreen meshes agree with an independently built direction preview");
            Near(MeshBounds(unit.GetMeshes(CollideType.TriggerOnly).Single()).size
                - MeshBounds(unit.GetMeshes(CollideType.CollideOnly).Single()).size,
                Vector3.one * .1f, "automatic Trigger adds 0.1 per world axis independently of direction size");
            Check(map.utilCtrl.TryGetVisionBounds(unit.data, out Bounds bounds), "offscreen visual bounds resolve current direction");
            // Ordinary previews have no ObjectInstance; the runtime combined-prefab
            // path is the authored cube-volume contract rather than Renderer bounds.
            var combinedPreview = preview.AddComponent<ObjectInstance>();
            Check(map.utilCtrl.TryGetVisionBounds(preview, unit.data.pos, unit.data.euler, unit.data.scale, out Bounds previewBounds),
                "combined direction preview visual bounds are available");
            Near(bounds.center, previewBounds.center, "visual center agrees with independently built direction");
            Near(bounds.size, previewBounds.size, "visual size agrees with independently built direction");
            Object.DestroyImmediate(combinedPreview);
            Near(prefab.transform.GetChild(0).localPosition, baselinePosition, "offscreen query cannot mutate shared prefab position");
            Near(prefab.transform.GetChild(0).localScale, baselineSize, "offscreen query cannot mutate shared prefab dimensions");
        }
    }

    private static void CheckLiveInstancesAndPoolReuse()
    {
        var product = DirectionalProduct();
        ObjectProductForm.AddData(product);
        var prefab = util.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid), product, true);
        objects.Add(prefab);
        pool.AddPool(prefab);
        var a = NewUnit(product, new Vector3(3, 1, 4), 0);
        var b = NewUnit(product, new Vector3(7, 2, 5), 180);
        a.Show();
        b.Show();
        foreach (float yaw in new[] { 0f, 90f, 180f, 270f, 0f, 270f })
        {
            a.data.euler = new Vector3(0, yaw, 0);
            a.ins.transform.eulerAngles = a.data.euler;
            appearance.OnEvent(new ObjectEvent { type = MapEventType.Refresh, unit = a });
            AnimDirecton dir = product.GetAnimDirection(yaw);
            Near(a.ins.transform.GetChild(0).localScale, product.GetModelSize(dir), "shown model changes size immediately with direction");
            Near(a.ins.transform.GetChild(0).localPosition, product.GetModelPosition(dir) + Vector3.up * .5f,
                "shown model changes position immediately with direction");
            Near(b.ins.transform.GetChild(0).localScale, product.GetModelSize(AnimDirecton.Down), "changing one instance leaves another facing unchanged");
            Near(a.ins.transform.position, a.data.pos, "direction offsets never move the Unit root");
            Near(a.ins.transform.localScale, a.data.scale, "direction dimensions never change Unit root scale");
            foreach (var kind in new[] { CollideType.CollideOnly, CollideType.TriggerOnly })
                SameMeshes(a.GetMeshes(kind), map.utilCtrl.GetCollidersMesh(a.ins.gameObject, a.data.pos, a.data.euler, a.data.scale, kind),
                    "shown and offscreen collision agree after direction change");
        }
        var reused = a.ins;
        a.Hide();
        var c = NewUnit(product, new Vector3(2, 3, 1), 90);
        c.Show();
        Check(ReferenceEquals(reused, c.ins), "test exercises a real pooled instance handoff");
        Near(c.ins.transform.GetChild(0).localScale, product.GetModelSize(AnimDirecton.Right), "pool handoff restores new owner's direction size");
        Near(c.ins.transform.GetChild(0).localPosition, product.GetModelPosition(AnimDirecton.Right) + Vector3.up * .5f,
            "pool handoff restores new owner's direction position");
        Near(prefab.transform.GetChild(0).localScale, product.GetModelSize(product.GetDefaultAnimDirection()), "live direction changes never overwrite template size");
        Near(prefab.transform.GetChild(0).localPosition, product.GetModelPosition(product.GetDefaultAnimDirection()) + Vector3.up * .5f,
            "live direction changes never overwrite template position");
        c.Hide();
        b.Hide();
    }

    private static void CheckDirectionOverlapRefresh()
    {
        for (int x = 0; x <= 10; x++)
        for (int z = 0; z <= 10; z++)
        {
            var tile = TileUnitForm.GetDataByJo(new JObject());
            tile.uid = nextUnit++;
            tile.mapPos = new Vector3Int(x, 0, z);
            tile.pos = new Vector3(x, 0, z);
            map.data.RegisterMap(tile);
        }
        var product = DirectionalProduct();
        product.SetModelSize(AnimDirecton.Up, new Vector3(.8f, .8f, .8f));
        product.SetModelPosition(AnimDirecton.Up, new Vector3(0, -.1f, 0));
        product.SetModelSize(AnimDirecton.Right, new Vector3(3.8f, .8f, .8f));
        product.SetModelPosition(AnimDirecton.Right, new Vector3(2, -.1f, 0));
        ObjectProductForm.AddData(product);
        var prefab = util.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid), product, true);
        objects.Add(prefab);
        pool.AddPool(prefab);
        var unit = NewUnit(product, new Vector3(5, 0, 5), 0);
        unit.Show();
        map.updateCtrl.RefreshObjectOverlap(unit);
        var physicalIndex = (DoubleDictionary<ObjectUnit, TileUnit>)typeof(MapUpdateController)
            .GetField("objectNavigationTileDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(map.updateCtrl);
        var passIndex = (DoubleDictionary<ObjectUnit, TileUnit>)typeof(MapUpdateController)
            .GetField("objectPassTypeTileDic", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(map.updateCtrl);
        var oldVisual = new HashSet<TileUnit>(map.updateCtrl.objectTileDic.Get(unit));
        var oldPhysical = new HashSet<TileUnit>(physicalIndex.Get(unit));
        Check(oldVisual.Count > 0 && oldPhysical.Count > 0, "initial geometry has registered visual/physical coverage");
        map.updateCtrl.ApplyMove(unit, unit.data.pos, new Vector3(0, 90, 0), teleport: true);
        appearance.OnEvent(new ObjectEvent { type = MapEventType.Refresh, unit = unit });
        var newVisual = new HashSet<TileUnit>(map.utilCtrl.GetVisionOverlap(unit.data));
        var newPhysical = new HashSet<TileUnit>(map.utilCtrl.GetObjectNavigationOverlap(unit.data));
        Check(!newVisual.SetEquals(oldVisual) && !newPhysical.SetEquals(oldPhysical),
            "directional dimensions/offset actually change visual and physical coverage");
        Check(newVisual.SetEquals(map.updateCtrl.objectTileDic.Get(unit)), "visual index exactly matches new direction footprint");
        Check(newPhysical.SetEquals(physicalIndex.Get(unit)), "navigation candidate index exactly matches new direction bodies");
        foreach (var tile in oldVisual.Union(newVisual))
            Check(map.updateCtrl.objectTileDic.Get(tile).Contains(unit) == newVisual.Contains(tile),
                "visual reverse index removes departed and adds newly covered Tiles");
        foreach (var tile in oldPhysical.Union(newPhysical))
            Check(physicalIndex.Get(tile).Contains(unit) == newPhysical.Contains(tile),
                "physical reverse index removes departed and adds newly covered Tiles");
        var newPass = new HashSet<TileUnit>(map.utilCtrl.GetObjectPassTypeOverlap(unit.data));
        Check(newPass.SetEquals(passIndex.Get(unit)), "terrain pass-type probe index also follows current geometry");
        unit.Hide();
    }

    private sealed class GeneratedProbe : IZ_Listener<ObjectEvent>
    {
        public int templateUid;
        public readonly Dictionary<ObjectUnit, List<MeshInfo>> created = new Dictionary<ObjectUnit, List<MeshInfo>>();
        public void OnEvent(ObjectEvent evt)
        {
            if (evt.type != MapEventType.Create || ObjectProductForm.GetTemplateUid(evt.unit.product) != templateUid) return;
            Check(evt.unit.product.protoUid == templateUid, "Create observes an independently bound runtime Product");
            Check(new HashSet<TileUnit>(map.utilCtrl.GetVisionOverlap(evt.unit.data))
                .SetEquals(map.updateCtrl.objectTileDic.Get(evt.unit)), "Create observes final visual associations");
            created.Add(evt.unit, evt.unit.GetMeshes(CollideType.All));
        }
    }

    private static void CheckGeneratedInitialization()
    {
        var product = NewProduct();
        product.faceType = FaceType.Fixed;
        product.collision = true;
        product.isWangTile = true;
        product.boundsCollision = true;
        product.EnsureDirectionData();
        product.SetModelSize(AnimDirecton.Fixed, new Vector3(.8f, .8f, .8f));
        product.SetModelPosition(AnimDirecton.Fixed, Vector3.zero);
        ObjectProductForm.AddData(product);
        var prefab = util.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid), product, true);
        objects.Add(prefab);
        pool.AddPool(prefab);
        var support = map.utilCtrl.GetTile(5, 0, 5);
        var oldInstance = support.ins;
        var visible = Go("generated-visible-support");
        visible.SetActive(false);
        var instance = visible.AddComponent<TileInstance>();
        instance.enabled = false;
        support.ins = instance;
        visible.SetActive(true);
        var probe = new GeneratedProbe { templateUid = product.uid };
        Z_EventHelper.Register(probe);
        bool oldPlaying = DynamicGlobalSettings.playing;
        DynamicGlobalSettings.playing = true;
        try
        {
            var first = appearance.GenerateObject(product, new Vector3(5, 0, 5)).unit;
            var second = appearance.GenerateObject(product, new Vector3(5.8f, 0, 5)).unit;
            Check(first.isShowing && second.isShowing, "generated placements become visible on a showing Tile");
            Check(probe.created.Count == 2 && !ReferenceEquals(first.product, second.product),
                "each generated placement creates once and owns its Product");
            foreach (var unit in new[] { first, second })
            {
                Check(ReferenceEquals(probe.created[unit], unit.GetMeshes(CollideType.All)),
                    "Show/Refresh retain final geometry built before Create");
                Check(new HashSet<TileUnit>(map.utilCtrl.GetVisionOverlap(unit.data))
                    .SetEquals(map.updateCtrl.objectTileDic.Get(unit)), "generated visibility index matches final footprint");
                Check(unit.data.isObstacle && unit.product.boundsCollision, "generation preserves collision configuration");
            }
            var masks = (Dictionary<ObjectUnit, (Bounds, int, int)>)typeof(GameMapController)
                .GetField("objectWangTileMasks", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(appearance);
            Check(masks[first].Item3 != 0 && masks[second].Item3 != 0,
                "generation updates both adjacent WangTile masks after registration");
        }
        finally
        {
            DynamicGlobalSettings.playing = oldPlaying;
            Z_EventHelper.Unregister(probe);
            support.ins = oldInstance;
        }
    }

    private static void CheckZeroToNonzeroDirection()
    {
        var product = DirectionalProduct();
        product.SetModelSize(AnimDirecton.Up, new Vector3(1, 0, 1));
        product.SetModelPosition(AnimDirecton.Up, Vector3.zero);
        product.SetModelSize(AnimDirecton.Right, new Vector3(2, 1, 3));
        product.SetModelPosition(AnimDirecton.Right, Vector3.zero);
        ObjectProductForm.AddData(product);
        var prefab = util.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid), product, true);
        objects.Add(prefab);
        pool.AddPool(prefab);
        var unit = NewUnit(product, new Vector3(3, 2, 4), 0);
        unit.Show();
        unit.data.euler = new Vector3(0, 90, 0);
        unit.ins.transform.eulerAngles = unit.data.euler;
        appearance.OnEvent(new ObjectEvent { type = MapEventType.Refresh, unit = unit });
        var preview = util.CombineNewObjectByPrefabs("zero-to-nonzero-preview", product, false, previewDirection: AnimDirecton.Right);
        objects.Add(preview);
        foreach (var kind in new[] { CollideType.CollideOnly, CollideType.TriggerOnly })
        {
            var expected = map.utilCtrl.GetCollidersMesh(preview, unit.data.pos, unit.data.euler, Vector3.one, kind);
            var offscreen = unit.GetMeshes(kind);
            var live = map.utilCtrl.GetCollidersMesh(unit.ins.gameObject, unit.data.pos, unit.data.euler, Vector3.one, kind);
            foreach (var mesh in offscreen.Concat(live))
            {
                foreach (Vector3 point in mesh.positions)
                    Check(Finite(point.x) && Finite(point.y) && Finite(point.z), "growing a zero-height direction keeps body/Trigger vertices finite");
                Vector3 size = MeshBounds(mesh).size;
                Check(size.x < 10 && size.y < 10 && size.z < 10,
                    "growing a zero-height direction cannot retain huge/infinite automatic Trigger padding");
            }
            SameMeshes(offscreen, expected, "zero-height template grows to independently built Right geometry");
            SameMeshes(live, expected, "live zero-height template grows to independently built Right geometry");
        }
        unit.Hide();
    }

    private static bool Finite(float value) => !float.IsNaN(value) && !float.IsInfinity(value);

    private static void CheckFittedRearAnchoredDirections()
    {
        const int sphereId = CubeId + 1, textureId = CubeId + 2;
        var sphere = Go("direction-sphere-source");
        sphere.AddComponent<MeshRenderer>().sharedMaterial = material;
        var body = Go("sphere-body");
        body.transform.SetParent(sphere.transform, false);
        body.AddComponent<SphereCollider>().radius = .5f;
        GameObjectAssetForm.DataById[sphereId] = new GameObjectAssetForm.Data(sphereId, sphere.name, null, null, null, sphere, 0);
        var texture = new Texture2D(10, 10, TextureFormat.RGBA32, false);
        var pixels = new Color32[100];
        for (int y = 1; y < 6; y++)
        for (int x = 2; x < 8; x++) pixels[y * 10 + x] = new Color32(255, 255, 255, 255);
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        TexAssetForm.DataById[textureId] = new TexAssetForm.Data(textureId, "direction-fitted-texture", null, null, null, texture, 0);
        try
        {
            DynamicGlobalSettings.playing = true;
            foreach (int shape in new[] { CubeId, sphereId })
            {
                var product = DirectionalProduct();
                product.boundsCollision = true;
                product.centerCollider = false;
                product.model.colliderScale = .4f;
                product.model.subPrefabUnitName[0] = shape;
                product.model.subUnitTexsName[0] = new List<int> { textureId };
                foreach (var dir in Directions) product.animClip[dir] = new List<int> { textureId };
                ObjectProductForm.AddData(product);
                var prefab = util.CombineNewObjectByPrefabs(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.uid), product, true);
                objects.Add(prefab);
                pool.AddPool(prefab);
                var unit = NewUnit(product, new Vector3(4, 2, 6), 0);
                var live = Object.Instantiate(prefab);
                objects.Add(live);
                unit.ins = live.GetComponent<ObjectInstance>();
                unit.ins.unit = unit;
                live.SetActive(true);
                Check(prefab.GetComponentsInChildren<SphereCollider>(true).All(c => !c.enabled),
                    "bounds-fitted templates use Boxes, including original Sphere models");
                foreach (Vector3 rootScale in new[] { Vector3.one, new Vector3(1.3f, .7f, 1.6f) })
                foreach (float yaw in new[] { 0f, 90f, 180f, 270f, 0f })
                {
                    unit.data.euler = new Vector3(0, yaw, 0);
                    unit.data.scale = rootScale;
                    live.transform.position = unit.data.pos;
                    live.transform.eulerAngles = unit.data.euler;
                    live.transform.localScale = rootScale;
                    appearance.OnEvent(new ObjectEvent { type = MapEventType.Refresh, unit = unit });
                    var preview = util.CombineNewObjectByPrefabs("fitted-rear-direction-preview", product, false,
                        previewDirection: product.GetAnimDirection(yaw));
                    objects.Add(preview);
                    foreach (var kind in new[] { CollideType.CollideOnly, CollideType.TriggerOnly })
                    {
                        var expected = map.utilCtrl.GetCollidersMesh(preview, unit.data.pos, unit.data.euler, rootScale, kind);
                        SameMeshes(unit.GetMeshes(kind), expected,
                            "fitted/rear-anchored offscreen body and Trigger match independent direction preview");
                        SameMeshes(map.utilCtrl.GetCollidersMesh(live, unit.data.pos, unit.data.euler, rootScale, kind), expected,
                            "fitted/rear-anchored live geometry matches preview after repeated direction/root-scale changes");
                    }
                    Near(prefab.transform.GetChild(0).localScale, product.GetModelSize(product.GetDefaultAnimDirection()),
                        "fitting/rear anchoring never overwrite shared template size");
                }
                unit.ins = null;
            }
        }
        finally
        {
            DynamicGlobalSettings.playing = false;
            Object.DestroyImmediate(texture);
            TexAssetForm.DataById.Remove(textureId);
        }
    }

    private static Bounds MeshBounds(MeshInfo mesh)
    {
        var bounds = new Bounds(mesh.positions[0], Vector3.zero);
        foreach (Vector3 point in mesh.positions) bounds.Encapsulate(point);
        return bounds;
    }

    private static void SameMeshes(IReadOnlyList<MeshInfo> actual, IReadOnlyList<MeshInfo> expected, string context)
    {
        Check(actual.Count == expected.Count && actual.Count > 0, context + ": same nonempty mesh count");
        for (int i = 0; i < actual.Count; i++)
        {
            Near(actual[i].center, expected[i].center, context + ": center");
            Check(actual[i].positions.Length == expected[i].positions.Length, context + ": vertex count");
            for (int j = 0; j < actual[i].positions.Length; j++)
                Near(actual[i].positions[j], expected[i].positions[j], context + ": vertex");
        }
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = Go(typeof(T).Name);
        root.SetActive(false);
        var result = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", PrivateStatic).SetValue(null, result);
        return result;
    }

    private static GameObject Go(string name)
    {
        var go = new GameObject(name);
        objects.Add(go);
        return go;
    }

    private static void Near(Vector3 a, Vector3 b, string context) => Check((a - b).sqrMagnitude < .000001f, context + ": " + a + " != " + b);
    private static void Near(float a, float b, string context) => Check(Mathf.Abs(a - b) < .0001f, context + ": " + a + " != " + b);
    private static void Check(bool condition, string context)
    {
        if (!condition) throw new InvalidOperationException(context);
        checks++;
    }
}
