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
using LabForm = Z_DataSystem.Form.LabForm;
using TexAssetForm = Z_DataSystem.Form.TexAssetForm;
using ObjectList = Ui.ModStory.ModStoryMapObject.ModStoryMapObjectList;
using ObjectAppearance = Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectAppearance;
using ModTool = Ui.ModSceneMain.ModTool;

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
            BoundsCollisionPrefabs();
            ObjectRearAnchoredCollider();
            WangTileBoundsCollision();
            ObjectBottomAnchoredHeight();
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
            VisibilityRegressions();
            Events();
            ObjectCenterPassTypeRefresh();
            SavedBridgePassTypeCoverage();
            ObjectTileUpwardProbeRange();
            BridgeObstacleNavigation();
            WholeTileObjectNavigation();
            DirectionalObjectNavigation();
            ObjectCollisionScalePassTypeCoverage();
            ObjectMarksAndMissionCoordinates();
            EventOperatorRendering();
            FormListOrdering();
            ObjectEmptyTextureThumbnails();
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

    private static void ObjectRearAnchoredCollider()
    {
        var product = Form.MapObjectForm.GetDataByJo(new JObject());
        Require(product.centerCollider && Form.MapObjectForm.defaultData.centerCollider, "old saves default to centered Colliders");
        product.centerCollider = false;
        Require(!product.Copy().centerCollider && !Form.MapObjectForm.GetDataByJo(Form.MapObjectForm.GetJoByData(product)).centerCollider,
            "rear anchor survives copying and save/load");
        var reset = Form.MapObjectForm.defaultData;
        reset.Reset(product);
        Require(!reset.centerCollider, "reset retains rear anchor");
        var apply = typeof(GameUtilController).GetMethod("ApplyFirstPartCollisionBounds", BindingFlags.Static | BindingFlags.NonPublic);
        var util = new GameUtilController(null);
        foreach (int prefabId in new[] { 99101, 99102 })
        foreach (bool fit in new[] { false, true })
        foreach (float scale in new[] { 0f, .2f, .25f, .5f, 1f, 1.5f })
        {
            product.model = Form.MapModelForm.defaultData;
            product.model.subPrefabUnitName = new List<int> { prefabId };
            product.model.subPrefabUnitPos = new List<Vector3> { new Vector3(.4f, .7f, .3f) };
            product.model.subPrefabUnitScale = new List<Vector3> { new Vector3(2, 2, 3) };
            product.model.subUnitTexsName = new List<List<int>> { new List<int> { 99103 } };
            product.model.colliderScale = scale;
            product.animClip = new Dictionary<AnimDirecton, List<int>> { [AnimDirecton.Fixed] = new List<int> { 99103 } };
            product.boundsCollision = fit;
            var preview = util.CombineNewObjectByPrefabs("rear-preview", product, false);
            var baseline = util.CombineNewObjectByPrefabs("rear-pool", product, true);
            objects.Add(preview); objects.Add(baseline);
            var body = map.utilCtrl.GetCollidersMesh(preview, Vector3.zero, Vector3.zero, Vector3.one, CollideType.CollideOnly).Single();
            float rear = body.type == MeshType.Sphere ? body.center.z -
                (body.positions[(int)Z_Math.Graph.SphereSixPoint.Right] - body.positions[(int)Z_Math.Graph.SphereSixPoint.Left]).magnitude * .5f
                : body.positions.Min(point => point.z);
            Require(Mathf.Abs(rear + 1.2f) < .0001f, "final rear remains fixed for all scales, shapes and bounds modes");
            RequireActualBoxRearEdge(preview, -1.2f);
            var bounds = fit ? new Rect(.6f, .1f, .4f, .4f) : new Rect(0, 0, 1, 1);
            var live = Object.Instantiate(baseline);
            objects.Add(live);
            foreach (var rootScale in new[] { Vector3.one, new Vector3(2, 3, .5f) })
            {
                live.transform.localScale = rootScale;
                live.transform.rotation = Quaternion.Euler(0, 37, 0);
                for (int repeat = 0; repeat < 2; repeat++)
                {
                    apply.Invoke(null, new object[] { live, baseline, bounds, scale, false });
                    RequireActualBoxRearEdge(live, -1.2f);
                    var expected = map.utilCtrl.GetCollidersMesh(baseline, Vector3.zero, live.transform.eulerAngles, rootScale,
                        CollideType.All, fit ? bounds : (Rect?)null, scale, false);
                    var actual = map.utilCtrl.GetCollidersMesh(live, Vector3.zero, live.transform.eulerAngles, rootScale);
                    Require(expected.Count == actual.Count, "rear anchor retains body/Trigger count");
                    for (int mesh = 0; mesh < expected.Count; mesh++)
                    {
                        Require((expected[mesh].center - actual[mesh].center).sqrMagnitude < .000001f, "live/off-screen centers agree");
                        for (int point = 0; point < expected[mesh].positions.Length; point++)
                            Require((expected[mesh].positions[point] - actual[mesh].positions[point]).sqrMagnitude < .000001f,
                                "live/off-screen vertices agree after root transform and repeated pool restore");
                    }
                }
                apply.Invoke(null, new object[] { live, baseline, bounds, scale, true });
                var restored = map.utilCtrl.GetCollidersMesh(live, Vector3.zero, live.transform.eulerAngles, rootScale);
                var centered = map.utilCtrl.GetCollidersMesh(baseline, Vector3.zero, live.transform.eulerAngles, rootScale,
                    CollideType.All, fit ? bounds : (Rect?)null, scale);
                Require((restored[0].center - centered[0].center).sqrMagnitude < .000001f, "center toggle removes rear offset");
            }
        }

        // Independent unit-Box example: shrinking must move the CENTER by only
        // half the removed depth. Measuring Collider transforms (not our Mesh
        // conversion) catches accidentally placing that center on the rear edge.
        var unitCube = Go("rear-unit-cube-source");
        unitCube.AddComponent<MeshRenderer>();
        var colliderGo = Go("rear-unit-cube-collider");
        colliderGo.transform.SetParent(unitCube.transform, false);
        colliderGo.AddComponent<BoxCollider>();
        const int unitCubeId = 99110;
        Z_DataSystem.Form.GameObjectAssetForm.DataById[unitCubeId] = new Z_DataSystem.Form.GameObjectAssetForm.Data(
            unitCubeId, unitCube.name, null, null, null, unitCube, 0);
        product.model = Form.MapModelForm.defaultData;
        product.model.subPrefabUnitName = new List<int> { unitCubeId };
        product.model.subPrefabUnitPos = new List<Vector3> { Vector3.zero };
        product.model.subPrefabUnitScale = new List<Vector3> { Vector3.one };
        product.model.subUnitTexsName = new List<List<int>> { new List<int> { 99103 } };
        product.model.colliderScale = .2f;
        product.boundsCollision = false;
        var smallBox = util.CombineNewObjectByPrefabs("rear-unit-cube-preview", product, false);
        objects.Add(smallBox);
        RequireActualBoxRearEdge(smallBox, -.5f);
        var smallBody = smallBox.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        float smallCenter = smallBox.transform.InverseTransformPoint(smallBody.transform.TransformPoint(smallBody.center)).z;
        Require(Mathf.Abs(smallCenter + .4f) < .0001f, "scale 0.2 anchors the rear at -0.5, NOT the center (which is -0.4)");
    }

    private static void RequireActualBoxRearEdge(GameObject root, float expectedRear)
    {
        foreach (var collider in root.GetComponentsInChildren<BoxCollider>(true).Where(c => c.enabled && !c.isTrigger))
        {
            float minZ = float.PositiveInfinity, maxZ = float.NegativeInfinity;
            for (int x = -1; x <= 1; x += 2)
            for (int y = -1; y <= 1; y += 2)
            for (int z = -1; z <= 1; z += 2)
            {
                var corner = collider.center + Vector3.Scale(collider.size * .5f, new Vector3(x, y, z));
                float actualZ = root.transform.InverseTransformPoint(collider.transform.TransformPoint(corner)).z;
                minZ = Mathf.Min(minZ, actualZ);
                maxZ = Mathf.Max(maxZ, actualZ);
            }
            float centerZ = root.transform.InverseTransformPoint(collider.transform.TransformPoint(collider.center)).z;
            Require(Mathf.Abs(minZ - expectedRear) < .0001f, "actual BoxCollider rear matches Object rear independently of cached Meshes");
            Require(Mathf.Abs(centerZ - expectedRear - (maxZ - minZ) * .5f) < .0001f,
                "actual center is half the FINAL Collider depth in front of the Object rear");
            if (maxZ - minZ > .0001f)
                Require(centerZ > expectedRear + .00001f, "nonzero Collider center must not lie on the Object rear");
        }
    }

    private static void BoundsCollisionPrefabs()
    {
        // RuntimeInitializeOnLoadMethod registration does not run in EditMode.
        Z_DataSystem.Form.GameObjectAssetForm.InitInternal();
        Z_DataSystem.Form.TexAssetForm.InitInternal();
        typeof(Form.MapModelForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        const int cubeId = 99101, sphereId = 99102, textureId = 99103, otherTextureId = 99104,
            emptyTextureId = 99105, fullTextureId = 99106, animatedId = 99107, webPId = 99109;
        var cube = Go("bounds-cube-source");
        cube.AddComponent<MeshRenderer>();
        var bodyGo = Go("bounds-cube-collider");
        bodyGo.transform.SetParent(cube.transform, false);
        var originalBody = bodyGo.AddComponent<BoxCollider>();
        originalBody.size = new Vector3(2, 4, 3);
        originalBody.center = new Vector3(.1f, .2f, -.2f);
        var sphere = Go("bounds-sphere-source");
        sphere.AddComponent<MeshRenderer>();
        var sphereGo = Go("bounds-sphere-collider");
        sphereGo.transform.SetParent(sphere.transform, false);
        var originalSphere = sphereGo.AddComponent<SphereCollider>();
        originalSphere.radius = .6f;
        Z_DataSystem.Form.GameObjectAssetForm.DataById[cubeId] = new Z_DataSystem.Form.GameObjectAssetForm.Data(
            cubeId, cube.name, null, null, null, cube, 0);
        Z_DataSystem.Form.GameObjectAssetForm.DataById[sphereId] = new Z_DataSystem.Form.GameObjectAssetForm.Data(
            sphereId, sphere.name, null, null, null, sphere, 0);
        var right = BoundsTexture(textureId, 6, 1, 10, 5, 77);
        var rightPixels = right.GetPixels32();
        rightPixels[0] = new Color32(255, 255, 255, 1);
        rightPixels[99] = new Color32(255, 255, 255, 76);
        right.SetPixels32(rightPixels);
        right.Apply(false, false);
        var left = BoundsTexture(otherTextureId, 1, 6, 3, 8);
        BoundsTexture(emptyTextureId, 0, 0, 0, 0);
        BoundsTexture(fullTextureId, 0, 0, 10, 10);
        var animated = new Z_DataSystem.Form.TexAssetForm.Data(animatedId, "bounds-gif", null,
            new byte[] { (byte)'G', (byte)'I', (byte)'F' }, null, right, 0);
        typeof(Z_DataSystem.Form.TexAssetForm.Data).GetField("_gifFrames", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(animated, new List<Z_Texture.GifFrameData> {
                new Z_Texture.GifFrameData { texture = right, delaySeconds = .1f },
                new Z_Texture.GifFrameData { texture = left, delaySeconds = .1f } });
        Z_DataSystem.Form.TexAssetForm.DataById[animatedId] = animated;
        var webP = new Z_DataSystem.Form.TexAssetForm.Data(webPId, "bounds-webp", null,
            new byte[] { (byte)'R', (byte)'I', (byte)'F', (byte)'F', 0, 0, 0, 0, (byte)'W', (byte)'E', (byte)'B', (byte)'P' }, null, right, 0);
        typeof(Z_DataSystem.Form.TexAssetForm.Data).GetField("_webPFrames", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(webP, new List<Z_Texture.AnimatedFrameData> {
                new Z_Texture.AnimatedFrameData { texture = right, delaySeconds = .1f },
                new Z_Texture.AnimatedFrameData { texture = left, delaySeconds = .1f } });
        Z_DataSystem.Form.TexAssetForm.DataById[webPId] = webP;
        var model = new Form.MapModelForm.Data(0, new List<int> { cubeId },
            new List<Vector3> { new Vector3(.2f, .1f, -.3f) }, new List<Vector3> { new Vector3(2, 3, 4) },
            new List<List<int>> { new List<int> { textureId } }, .1f, true, .5f);
        var util = new GameUtilController(null);
        Func<bool, GameObject> combine = fitted => {
            var result = util.CombineNewObjectByPrefabs("bounds-generated", model, false, boundsCollision: fitted);
            objects.Add(result);
            return result;
        };
        var unchanged = combine(false);
        var unchangedBody = unchanged.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require((unchangedBody.size - originalBody.size).sqrMagnitude < .000001f &&
            (unchangedBody.center - originalBody.center).sqrMagnitude < .000001f,
            "disabled boundsCollision preserves authored size and center");
        Require(unchangedBody.transform.localScale == new Vector3(.5f, 1, .5f),
            "disabled boundsCollision preserves original colliderScale behavior");
        var fittedGo = combine(true);
        var fittedBody = fittedGo.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        var fittedTrigger = fittedGo.GetComponentsInChildren<BoxCollider>(true).Single(c => c.isTrigger);
        Require((fittedBody.size - new Vector3(.8f, 1.6f, 1.2f)).sqrMagnitude < .000001f,
            "non-square source bounds include alpha=77 edges but exclude alpha=1/76 distant speckles");
        Require((fittedBody.center - new Vector3(1.3f, -.6f, -1.4f)).sqrMagnitude < .000001f,
            "offset fits artwork without multiplying image-center displacement by colliderScale");
        Require((fittedBody.transform.position - unchangedBody.transform.position).sqrMagnitude < .000001f &&
            fittedGo.transform.GetChild(0).localScale == unchanged.transform.GetChild(0).localScale,
            "boundsCollision never moves/scales renderers or model parts");
        var originalTrigger = unchanged.GetComponentsInChildren<BoxCollider>(true).Single(c => c.isTrigger);
        Require((fittedTrigger.size - fittedBody.size - (originalTrigger.size - unchangedBody.size)).sqrMagnitude < .000001f &&
            fittedTrigger.center == fittedBody.center, "auto Trigger retains original padding around the fitted body");
        var beforeSize = fittedBody.size;
        pool.AddPool(fittedGo);
        var unit = new ObjectUnitForm.Data(99108, false, "", fittedGo.name, new Vector3(3, 2, 4),
            new Vector3(0, 90, 0), new Vector3(2, 1, 3), UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        var physical = unit.GetMeshes(CollideType.CollideOnly);
        Vector3 relativeCenter = fittedGo.transform.InverseTransformPoint(fittedBody.transform.TransformPoint(fittedBody.center));
        Vector3 expectedCenter = unit.data.pos + Quaternion.Euler(unit.data.euler) * Vector3.Scale(relativeCenter, unit.data.scale);
        Require(physical.Count == 1 && (physical[0].center - expectedCenter).sqrMagnitude < .000001f,
            "physical cache consumes fitted center with model/root rotation, scale and offset");
        Require(unit.GetMeshes(CollideType.TriggerOnly).Count == 1, "fitted Trigger uses the shared cached geometry pipeline");
        Require(fittedBody.size == beforeSize && originalBody.size == new Vector3(2, 4, 3) &&
            originalBody.center == new Vector3(.1f, .2f, -.2f) && cube.GetComponentsInChildren<Collider>().Length == 1,
            "generation and geometry reads do not mutate source prefabs or reapply shrinking");
        fittedBody.enabled = false;
        unit.InvalidateCollisionGeometry();
        Require(unit.GetMeshes(CollideType.CollideOnly).Count == 0,
            "disabled/deferred-destroy Collider is excluded from physical meshes");
        fittedBody.enabled = true;

        model.subUnitTexsName[0] = new List<int> { fullTextureId };
        var full = combine(true).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require(full.size == originalBody.size && full.center == originalBody.center, "opaque source keeps its Box unchanged");
        model.subUnitTexsName[0] = new List<int> { emptyTextureId };
        var empty = combine(true);
        Require(empty.GetComponentsInChildren<BoxCollider>(true).All(c => c.size == Vector3.zero),
            "fully transparent image has zero physical and Trigger size on all three axes");
        model.subUnitTexsName[0] = new List<int> { 0, GlobalDefaultHelper.DefaultTexId, 999999999 };
        var missing = combine(true).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require(missing.size == originalBody.size && missing.center == originalBody.center,
            "missing/default frame slots preserve authored Collider instead of collapsing it");
        const int faintTextureId = 99110;
        BoundsTexture(faintTextureId, 0, 0, 10, 10, 76);
        model.subUnitTexsName[0] = new List<int> { faintTextureId };
        var faint = combine(true);
        Require(faint.GetComponentsInChildren<BoxCollider>(true).All(c => c.size == Vector3.zero),
            "imported image entirely below alpha .3 has zero physical/Trigger XYZ, not a full-size fallback");
        model.subUnitTexsName[0] = new List<int> { animatedId };
        var animationBody = combine(true).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require((animationBody.size - new Vector3(1.8f, 2.8f, 2.1f)).sqrMagnitude < .000001f,
            "animated texture combines every composited frame's alpha bounds");
        model.subUnitTexsName[0] = new List<int> { webPId };
        var webPBody = combine(true).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require((webPBody.size - animationBody.size).sqrMagnitude < .000001f &&
            (webPBody.center - animationBody.center).sqrMagnitude < .000001f, "composited WebP frame bounds use the same union as GIF");
        model.subUnitTexsName[0] = new List<int> { textureId, otherTextureId };
        var sequence = combine(true).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require((sequence.size - animationBody.size).sqrMagnitude < .000001f &&
            (sequence.center - animationBody.center).sqrMagnitude < .000001f, "explicit animation list uses the same union");
        model.subPrefabUnitName.Add(cubeId);
        model.subPrefabUnitPos.Add(Vector3.zero);
        model.subPrefabUnitScale.Add(Vector3.one);
        model.subUnitTexsName.Add(new List<int> { fullTextureId });
        var multiple = combine(true);
        Require(multiple.transform.GetChild(1).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size == originalBody.size,
            "combined model parts fit their own textures independently");
        model.subPrefabUnitName.RemoveAt(1);
        model.subPrefabUnitPos.RemoveAt(1);
        model.subPrefabUnitScale.RemoveAt(1);
        model.subUnitTexsName.RemoveAt(1);
        var product = Form.MapObjectForm.GetDataByJo(new JObject());
        Require(!product.boundsCollision && !Form.MapObjectForm.defaultData.boundsCollision, "old saves and default row keep boundsCollision off");
        product.boundsCollision = true;
        Require(product.Copy().boundsCollision, "copy retains the saved boundsCollision option");
        var reset = Form.MapObjectForm.defaultData;
        reset.Reset(product);
        Require(reset.boundsCollision, "reset retains the saved boundsCollision option");
        var saved = Form.MapObjectForm.GetJoByData(product);
        Require(saved.Value<bool>("boundsCollision") && Form.MapObjectForm.GetDataByJo(saved).boundsCollision,
            "boundsCollision is persisted and round-trips through generated Form JSON");
        product.model = model;
        product.faceType = FaceType.FourDirection;
        product.animClip = new Dictionary<AnimDirecton, List<int>> {
            [AnimDirecton.Up] = new List<int> { textureId }, [AnimDirecton.Down] = new List<int> { otherTextureId },
            [AnimDirecton.Left] = new List<int> { textureId }, [AnimDirecton.Right] = new List<int> { textureId } };
        var productGo = util.CombineNewObjectByPrefabs("bounds-product", product, false);
        objects.Add(productGo);
        Require((productGo.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size - fittedBody.size).sqrMagnitude < .000001f,
            "four-direction preview defaults to Up, never unions other facing clips");
        var downPreview = util.CombineNewObjectByPrefabs("bounds-product-down", product, false, previewDirection: AnimDirecton.Down);
        objects.Add(downPreview);
        var downBody = downPreview.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require((downBody.size - new Vector3(.4f, .8f, .6f)).sqrMagnitude < .000001f &&
            (downBody.center - new Vector3(-1.1f, 1, 1)).sqrMagnitude < .000001f,
            "preview selection uses only that facing's alpha bounds and offset");
        product.animClip[AnimDirecton.Down].Clear();
        var emptyClipPreview = util.CombineNewObjectByPrefabs("bounds-product-empty-clip", product, false, previewDirection: AnimDirecton.Down);
        objects.Add(emptyClipPreview);
        Require(emptyClipPreview.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size == originalBody.size,
            "an empty facing preserves authored bounds without falling back to another facing's legacy texture");
        var productPool = util.CombineNewObjectByPrefabs("bounds-product-pool", product, true);
        objects.Add(productPool);
        Require(productPool.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size == originalBody.size,
            "ordinary directional pool retains a full first-part baseline for per-unit fitting");

        // A normalized vertical interval is a fraction of the whole diagonal,
        // not two fractions weighted by Y/(Y+Z) and Z/(Y+Z) a second time.
        var originalModelScale = model.subPrefabUnitScale[0];
        foreach (var modelScale in new[] { new Vector3(2, 10, 1), new Vector3(2, 1, 10), new Vector3(1.5f, 2, 3) })
        {
            model.subPrefabUnitScale[0] = modelScale;
            model.subUnitTexsName[0] = new List<int> { textureId };
            var aspectGo = combine(true);
            var aspectOriginal = combine(false);
            var aspectBody = aspectGo.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
            foreach (var rootScale in new[] { Vector3.one, new Vector3(2, 3, .5f) })
            {
                var fittedMesh = map.utilCtrl.GetCollidersMesh(aspectGo, Vector3.zero, Vector3.zero,
                    rootScale, CollideType.CollideOnly).Single();
                var originalMesh = map.utilCtrl.GetCollidersMesh(aspectOriginal, Vector3.zero, Vector3.zero,
                    rootScale, CollideType.CollideOnly).Single();
                var physicalSize = new Vector3(
                    fittedMesh.positions.Max(p => p.x) - fittedMesh.positions.Min(p => p.x),
                    fittedMesh.positions.Max(p => p.y) - fittedMesh.positions.Min(p => p.y),
                    fittedMesh.positions.Max(p => p.z) - fittedMesh.positions.Min(p => p.z));
                var expectedSize = Vector3.Scale(new Vector3(.4f, 1.6f, .6f), Vector3.Scale(modelScale, rootScale));
                Require((physicalSize - expectedSize).sqrMagnitude < .00001f,
                    "tall/deep Objects distribute image height over Y/Z using the model and root dimensions");
                var expectedOffset = Vector3.Scale(new Vector3(.6f, -.8f, -.6f), Vector3.Scale(modelScale, rootScale));
                Require((fittedMesh.center - originalMesh.center - expectedOffset).sqrMagnitude < .00001f,
                    "tall/deep Object bounds shift both Y/Z centers in the unshrunk image diagonal");
                Require(Mathf.Abs(physicalSize.y / physicalSize.z - expectedSize.y / expectedSize.z) < .0001f &&
                    (aspectBody.size - new Vector3(.8f, 1.6f, 1.2f)).sqrMagnitude < .000001f,
                    "nonuniform scale changes the physical Y/Z ratio without changing the normalized bounds fraction");
            }
        }
        model.subPrefabUnitScale[0] = originalModelScale;
        model.subPrefabUnitName[0] = sphereId;
        model.subPrefabUnitPos[0] = Vector3.zero;
        model.subPrefabUnitScale[0] = Vector3.one;
        model.subUnitTexsName[0] = new List<int> { textureId };
        var originalSphereGo = combine(false);
        Require(originalSphereGo.GetComponentsInChildren<SphereCollider>(true).Length == 2,
            "disabled fitting leaves the original physical and Trigger spheres");
        var fittedSphere = combine(true);
        var sphereBox = fittedSphere.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require(fittedSphere.GetComponentsInChildren<SphereCollider>(true).All(c => !c.enabled),
            "fitted spheres are replaced or disabled before deferred destruction");
        Require((sphereBox.size - Vector3.one * .48f).sqrMagnitude < .000001f &&
            sphereBox.transform.localScale == Vector3.one * .5f,
            "sphere fitting creates a Box with both Y and Z fitted to the vertical image bounds");
        Require((sphereBox.center - new Vector3(.72f, -.48f, -.48f)).sqrMagnitude < .000001f,
            "converted Sphere offsets Y in unshrunk image space despite its scaled Collider transform");
        Require(originalSphere.radius == .6f && sphere.GetComponentsInChildren<BoxCollider>(true).Length == 0,
            "sphere source prefab remains unchanged");
        model.colliderScale = 0;
        var zero = combine(true).GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require(!float.IsNaN(zero.center.x) && !float.IsInfinity(zero.center.x) &&
            !float.IsNaN(zero.center.y) && !float.IsInfinity(zero.center.y) && zero.transform.localScale == Vector3.zero,
            "zero colliderScale stays zero without invalid center arithmetic");
    }

    private static void ObjectBottomAnchoredHeight()
    {
        const int cubeId = 99111;
        var cube = Go("bottom-anchor-cube-source");
        cube.AddComponent<MeshRenderer>();
        var body = Go("bottom-anchor-cube-body");
        body.transform.SetParent(cube.transform, false);
        body.AddComponent<BoxCollider>();
        Z_DataSystem.Form.GameObjectAssetForm.DataById[cubeId] = new Z_DataSystem.Form.GameObjectAssetForm.Data(
            cubeId, cube.name, null, null, null, cube, 0);
        var util = new GameUtilController(null);
        var product = Form.MapObjectForm.GetDataByJo(new JObject());
        product.model = new Form.MapModelForm.Data(0, new List<int> { cubeId },
            new List<Vector3> { new Vector3(.2f, 0, -.3f) },
            new List<Vector3> { new Vector3(1.7f, 1, 2.4f) },
            new List<List<int>> { new List<int>() }, .1f, true, 1);
        product.faceType = FaceType.Fixed;
        product.animClip = new Dictionary<AnimDirecton, List<int>> { [AnimDirecton.Fixed] = new List<int>() };
        var editor = new ObjectAppearance.UiModStoryMapObjectObjectAppearanceModel { data = product };

        void CheckGeometry(float bottomHeight)
        {
            Require(Mathf.Abs(editor.posHeight - bottomHeight) < .00001f,
                "Object editor displays bottom-relative height rather than saved center offset");
            foreach (bool forGame in new[] { false, true })
            {
                var generated = util.CombineNewObjectByPrefabs("bottom-anchor-generated", product, forGame);
                objects.Add(generated);
                var part = generated.transform.GetChild(0);
                Require(Mathf.Abs(part.localPosition.y - (bottomHeight + Mathf.Abs(editor.height) / 2)) < .00001f,
                    "preview/runtime center follows half the current Object height");
                var meshes = map.utilCtrl.GetCollidersMesh(generated, Vector3.zero, Vector3.zero,
                    Vector3.one, CollideType.CollideOnly);
                Require(meshes.Count == 1 && Mathf.Abs(meshes[0].positions.Min(vertex => vertex.y) - bottomHeight) < .00001f,
                    "preview/runtime physical bottom remains at requested height: " + editor.height);
                Require(part.localPosition.x == product.model.subPrefabUnitPos[0].x
                    && part.localPosition.z == product.model.subPrefabUnitPos[0].z
                    && part.localScale.x == 1.7f && part.localScale.z == 2.4f,
                    "height editing preserves Object horizontal position and dimensions");
            }
        }

        try
        {
            // Old raw center offsets must still load at their exact authored position.
            product.model.subPrefabUnitScale[0] = new Vector3(1.7f, .1f, 2.4f);
            product.model.subPrefabUnitPos[0] = new Vector3(.2f, -.3f, -.3f);
            string before = Form.MapObjectForm.GetJoByData(product).ToString();
            CheckGeometry(.15f);
            Require(Form.MapObjectForm.GetJoByData(product).ToString() == before,
                "reading the bottom-relative editor height does not migrate or mutate old saves");

            foreach (bool fitted in new[] { false, true })
            {
                product.boundsCollision = fitted;
                foreach (float bottomHeight in new[] { 0f, .7f, -.2f, 0f })
                {
                    editor.posHeight = bottomHeight;
                    foreach (float height in new[] { 1f, 2f, .5f, 3f, .25f, 2f, 1f })
                    {
                        editor.height = height;
                        CheckGeometry(bottomHeight);
                    }
                    editor.SetHorizontalPosition(new Vector2(-.6f, .8f));
                    Require(product.model.subPrefabUnitPos[0].x == -.6f && product.model.subPrefabUnitPos[0].z == .8f,
                        "Object preview axis applies horizontal position");
                    CheckGeometry(bottomHeight);
                }
            }

            editor.height = 2;
            editor.posHeight = 0;
            var restored = Form.MapObjectForm.GetDataByJo(Form.MapObjectForm.GetJoByData(product));
            Require(restored.model.subPrefabUnitScale[0].y == 2 && restored.model.subPrefabUnitPos[0].y == .5f,
                "saved center offset includes height compensation without changing the model schema");
            editor.data = product = restored;
            CheckGeometry(0);
            editor.height = 1;
            CheckGeometry(0);
            product.model.subPrefabUnitScale[0] = Vector3.one;
            product.model.subPrefabUnitPos[0] = Vector3.zero;
            Require(editor.height == 1 && editor.posHeight == 0, "Object Reset remains unit height and grounded");
        }
        finally
        {
            Z_DataSystem.Form.GameObjectAssetForm.DataById.Remove(cubeId);
        }
    }

    private static Texture2D BoundsTexture(int id, int minX, int minY, int maxX, int maxY, byte alpha = 255)
    {
        var texture = new Texture2D(10, 10, TextureFormat.RGBA32, false);
        var pixels = new Color32[100];
        for (int y = minY; y < maxY; y++)
            for (int x = minX; x < maxX; x++) pixels[y * 10 + x] = new Color32(255, 255, 255, alpha);
        texture.SetPixels32(pixels);
        texture.Apply(false, false);
        Z_DataSystem.Form.TexAssetForm.DataById[id] = new Z_DataSystem.Form.TexAssetForm.Data(id, "bounds-texture", null, null, null, texture, 0);
        return texture;
    }

    private static void WangTileBoundsCollision()
    {
        Form.MapBaseForm.InitInternal();
        Form.MapObjectForm.InitInternal();
        UnitForm.InitInternal();
        TileUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        foreach (var tile in map.data.maps.Values) TileUnitForm.AddData(tile);
        var util = new GameUtilController(null);
        var controller = new GameMapController(null);
        var register = typeof(GameMapController).GetMethod("RegisterObjectWangTileAnimationTexture", BindingFlags.Static | BindingFlags.NonPublic);
        var clear = typeof(GameMapController).GetMethod("ClearObjectWangTileAnimationTextures", BindingFlags.Static | BindingFlags.NonPublic);
        var generatedIds = new List<int>();
        var atlas = new Texture2D(20, 30, TextureFormat.RGBA32, false);
        var pixels = new Color32[600];
        void PartPixel(int part, int x, int y, byte alpha = 255)
        {
            int row = part <= 8 ? (part - 5) / 2 : 2 + (part - 9) / 4;
            int col = part <= 8 ? 2 + (part - 5) % 2 : (part - 9) % 4;
            pixels[(30 - (row + 1) * 5 + y) * 20 + col * 5 + x] = new Color32(255, 255, 255, alpha);
        }
        // Isolated tile: 2x2 center pixels. E/W: asymmetric 4x6 bounds.
        PartPixel(9, 4, 0); PartPixel(12, 0, 0); PartPixel(21, 4, 4); PartPixel(24, 0, 4);
        PartPixel(10, 2, 2); PartPixel(22, 2, 2); PartPixel(11, 2, 2); PartPixel(23, 2, 2);
        // Detached, near-invisible atlas noise survives assembly but must not
        // expand either preview bounds or the actual scene mask's Collider.
        PartPixel(9, 0, 4, 76); PartPixel(10, 4, 4, 1);
        pixels[29 * 20] = new Color32(255, 255, 255, 255); // unused atlas quarter, not rendered
        atlas.SetPixels32(pixels);
        atlas.Apply();
        const int atlasId = 99201, cubeId = 99202;
        Z_DataSystem.Form.TexAssetForm.DataById[atlasId] = new Z_DataSystem.Form.TexAssetForm.Data(atlasId, "bounds-quarter-atlas", null, null, null, atlas, 0);
        var sourceCube = Go("wang-bounds-source");
        sourceCube.AddComponent<MeshRenderer>();
        var bodyGo = Go("wang-bounds-body");
        bodyGo.transform.SetParent(sourceCube.transform, false);
        bodyGo.AddComponent<BoxCollider>().size = new Vector3(1, 4, 1);
        Z_DataSystem.Form.GameObjectAssetForm.DataById[cubeId] = new Z_DataSystem.Form.GameObjectAssetForm.Data(cubeId, sourceCube.name, null, null, null, sourceCube, 0);
        var product = Form.MapObjectForm.GetDataByJo(new JObject());
        product.id = -1;
        product.model = new Form.MapModelForm.Data(0, new List<int> { cubeId }, new List<Vector3> { Vector3.zero },
            new List<Vector3> { Vector3.one }, new List<List<int>> { new List<int> { atlasId } }, .1f, true, .5f);
        product.faceType = FaceType.Fixed;
        product.collision = true;
        product.animClip = new Dictionary<AnimDirecton, List<int>> { [AnimDirecton.Fixed] = new List<int> { atlasId } };
        product.isWangTile = product.boundsCollision = true;
        Form.MapObjectForm.AddData(product);
        var sprites = Z_Map.TileHelper.GetAutoTileSprites(atlas);
        var spriteIds = new Dictionary<Sprite, int>();
        int nextId = 99300;
        foreach (var pair in sprites)
        {
            if (!spriteIds.TryGetValue(pair.Value, out int id))
            {
                id = nextId++;
                spriteIds.Add(pair.Value, id);
                generatedIds.Add(id);
                Z_DataSystem.Form.TexAssetForm.DataById[id] = new Z_DataSystem.Form.TexAssetForm.Data(id, "assembled-bounds", null, null, null, pair.Value.texture, 0);
            }
            register.Invoke(null, new object[] { product.id, AnimDirecton.Fixed, pair.Key, id });
        }
        var prefab = util.CombineNewObjectByPrefabs("wang-bounds-runtime", product, true);
        objects.Add(prefab);
        pool.AddPool(prefab);
        var baseline = prefab.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
        Require(baseline.size == new Vector3(1, 4, 1) && baseline.center == Vector3.zero,
            "WangTile pool keeps a full baseline, never fits the original quarter atlas");
        var preview = util.CombineNewObjectByPrefabs("wang-bounds-preview", product, false);
        objects.Add(preview);
        Require((preview.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
            "WangTile preview fits the assembled isolated tile, excluding faint noise and unused atlas quarters");
        map.navigationCtrl.RebuildNow();
        var owner = map.utilCtrl.GetTile(6, 0, 6);
        var farGround = map.navigationCtrl.navUnits[(0, 0, 0)].dirGroundY;
        ObjectUnitForm.Data Add(int uid, Vector3 pos)
        {
            var data = new ObjectUnitForm.Data(uid, true, "", prefab.name, pos, Vector3.zero, new Vector3(2, 1, 2),
                UpdateType.ShowOnly, new List<int>(), "", false, 0);
            ObjectUnitForm.AddData(data);
            map.updateCtrl.RefreshObjectOverlap(data.unit);
            controller.RegisterObject(data, product);
            return data;
        }
        Vector3 PhysicalSize(ObjectUnit unit)
        {
            var mesh = unit.GetMeshes(CollideType.CollideOnly).Single();
            var bounds = new Bounds(mesh.positions[0], Vector3.zero);
            foreach (var point in mesh.positions) bounds.Encapsulate(point);
            return bounds.size;
        }
        var first = Add(99210, owner.data.pos);
        ObjectUnitForm.Data second = null;
        Texture2D cornerAtlas = null;
        try
        {
            Require(first.unit.ins == null && (PhysicalSize(first.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
                "hidden WangTile physical cache uses actual assembled isolated bounds with root/collider scale");
            Require(!map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked && map.utilCtrl.IsTileCenterCoveredByObject(owner, first),
                "vertically trimmed assembled tile retains pass coverage but no longer blocks above the ground threshold");
            var originalRootScale = first.scale;
            var isolatedGeometry = first.unit.GetMeshes(CollideType.All);
            first.scale = new Vector3(2, 10, 1);
            Require((PhysicalSize(first.unit) - new Vector3(.2f, 8, .1f)).sqrMagnitude < .000001f &&
                !ReferenceEquals(isolatedGeometry, first.unit.GetMeshes(CollideType.All)),
                "hidden Object root resizing rebuilds fitted diagonal geometry with the same cached texture bounds");
            first.unit.Show();
            var tallLive = first.unit.ins.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
            Require((tallLive.size - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f &&
                (Vector3.Scale(tallLive.size, tallLive.transform.lossyScale) - PhysicalSize(first.unit)).sqrMagnitude < .000001f &&
                (tallLive.transform.TransformPoint(tallLive.center) - first.unit.GetMeshes(CollideType.CollideOnly)[0].center).sqrMagnitude < .000001f,
                "live resized Collider and off-screen diagonal geometry agree");
            first.unit.Hide();
            first.scale = new Vector3(2, .25f, 8);
            Require((PhysicalSize(first.unit) - new Vector3(.2f, .2f, .8f)).sqrMagnitude < .000001f,
                "deep flat Object assigns most of the physical image span to Z instead of Y");
            first.scale = originalRootScale;
            map.updateCtrl.RefreshObjectOverlap(first.unit);
            second = Add(99211, owner.data.pos + Vector3.right * 2);
            Require(first.unit.ins == null && (PhysicalSize(first.unit) - new Vector3(.4f, 2.4f, .6f)).sqrMagnitude < .000001f,
                "adding a neighbour updates the off-screen unit's physical geometry");
            Require(map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked,
                "taller connected image bounds immediately restore navigation blocking after Y fitting");
            Require((first.unit.GetMeshes(CollideType.CollideOnly)[0].center - (owner.data.pos + new Vector3(.2f, .5f, 0))).sqrMagnitude < .000001f,
                "assembled asymmetric bounds offset follows the unshrunk image space");
            first.unit.Show();
            var live = first.unit.ins.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
            Require((live.size - new Vector3(.4f, 2.4f, .6f)).sqrMagnitude < .000001f && Mathf.Abs(live.center.x - .2f) < .00001f,
                "shown physical Collider agrees with WangTile cached geometry");
            var initialGeometry = first.unit.GetMeshes(CollideType.All);
            for (int i = 0; i < 4; i++) controller.OnEvent(new ObjectEvent { unit = first.unit, type = MapEventType.Refresh });
            Require(ReferenceEquals(initialGeometry, first.unit.GetMeshes(CollideType.All)) && live.size.x == .4f,
                "unchanged mask reuses geometry and never cumulatively shrinks the live Collider");
            var trigger = first.unit.ins.GetComponentsInChildren<BoxCollider>(true).Single(c => c.isTrigger);
            var baselineTrigger = prefab.GetComponentsInChildren<BoxCollider>(true).Single(c => c.isTrigger);
            Require(trigger.center == live.center && (trigger.size - live.size - (baselineTrigger.size - baseline.size)).sqrMagnitude < .000001f,
                "assembled mask keeps automatic Trigger padding");
            var connectedCenter = first.unit.GetMeshes(CollideType.CollideOnly)[0].center;
            DynamicGlobalSettings.playing = false;
            foreach (var collider in prefab.GetComponentsInChildren<BoxCollider>(true)) collider.transform.localScale = Vector3.one;
            controller.RefreshObjectWangTileAppearances(product.id);
            Require((PhysicalSize(first.unit) - new Vector3(.8f, 2.4f, 1.2f)).sqrMagnitude < .000001f
                && (first.unit.GetMeshes(CollideType.CollideOnly)[0].center - connectedCenter).sqrMagnitude < .000001f
                && live.transform.localScale == Vector3.one && Mathf.Abs(live.center.x - .1f) < .00001f,
                "Mod collider-scale reset rebuilds geometry and live Colliders without moving the artwork bounds center");
            DynamicGlobalSettings.playing = true;
            foreach (var collider in prefab.GetComponentsInChildren<BoxCollider>(true)) collider.transform.localScale = new Vector3(.5f, 1, .5f);
            controller.RefreshObjectWangTileAppearances(product.id);
            first.unit.Hide();
            second.unit.Move(Vector3.right * 2);
            Require((PhysicalSize(first.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
                "moving a neighbour away refreshes the former off-screen connection");
            second.unit.Move(Vector3.left * 2);
            Require(Mathf.Abs(PhysicalSize(first.unit).x - .4f) < .00001f,
                "moving back restores the assembled connected bounds");
            first.unit.Show();
            live = first.unit.ins.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
            live.size = Vector3.one * .01f; // simulate a reused Instance carrying another mask
            first.unit.Hide();
            first.unit.Show();
            live = first.unit.ins.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
            Require(Mathf.Abs(live.size.x - .4f) < .00001f, "pool reuse restores geometry even for the same mask");
            map.RemoveObject(second);
            second = null;
            Require((PhysicalSize(first.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f && Mathf.Abs(live.size.x - .2f) < .00001f,
                "neighbour deletion refreshes both live and cached isolated collision bounds");
            Require(!map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked,
                "removing the taller WangTile variant immediately clears its height-based navigation block");
            int wideId = nextId++;
            generatedIds.Add(wideId);
            BoundsTexture(wideId, 2, 3, 8, 9);
            register.Invoke(null, new object[] { product.id, AnimDirecton.Fixed, 0, wideId });
            controller.RefreshObjectWangTileAppearances(product.id);
            Require((PhysicalSize(first.unit) - new Vector3(.6f, 2.4f, .6f)).sqrMagnitude < .000001f,
                "all frames of an assembled mask are unioned; new registration invalidates its cached bounds");
            product.faceType = FaceType.FourDirection;
            int cornerId = nextId++;
            generatedIds.Add(cornerId);
            BoundsTexture(cornerId, 0, 0, 1, 1);
            register.Invoke(null, new object[] { product.id, AnimDirecton.Up, 0, spriteIds[sprites[0]] });
            register.Invoke(null, new object[] { product.id, AnimDirecton.Down, 0, wideId });
            register.Invoke(null, new object[] { product.id, AnimDirecton.Right, 0, cornerId });
            register.Invoke(null, new object[] { product.id, AnimDirecton.Left, 0, wideId });
            // A second source atlas renders only the bottom-left corner of the
            // isolated tile, matching Right's registered assembled variant.
            int cornerAtlasId = nextId++;
            generatedIds.Add(cornerAtlasId);
            cornerAtlas = new Texture2D(20, 30, TextureFormat.RGBA32, false);
            var cornerPixels = new Color32[600];
            cornerPixels[0] = new Color32(255, 255, 255, 255);
            cornerAtlas.SetPixels32(cornerPixels);
            cornerAtlas.Apply();
            Z_DataSystem.Form.TexAssetForm.DataById[cornerAtlasId] = new Z_DataSystem.Form.TexAssetForm.Data(
                cornerAtlasId, "bounds-right-quarter-atlas", null, null, null, cornerAtlas, 0);
            product.EnsureDirectionData();
            product.animClip[AnimDirecton.Right] = new List<int> { cornerAtlasId };
            var upPreview = util.CombineNewObjectByPrefabs("wang-bounds-up-preview", product, false, previewDirection: AnimDirecton.Up);
            var rightPreview = util.CombineNewObjectByPrefabs("wang-bounds-right-preview", product, false, previewDirection: AnimDirecton.Right);
            objects.Add(upPreview); objects.Add(rightPreview);
            Require((upPreview.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f &&
                (rightPreview.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger).size - new Vector3(.1f, .4f, .1f)).sqrMagnitude < .000001f,
                "WangTile appearance preview assembles only its selected facing's source atlas");
            controller.RefreshObjectWangTileAppearances(product.id);
            Require((PhysicalSize(first.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f &&
                (live.size - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
                "WangTile uses only the current Up facing of the assembled mask");
            void Turn(ObjectUnit unit, float yaw)
            {
                map.updateCtrl.ApplyMove(unit, unit.data.pos, Vector3.up * yaw, true);
                controller.OnEvent(new ObjectEvent { unit = unit, type = MapEventType.Move });
            }
            Turn(first.unit, 90);
            Require((PhysicalSize(first.unit) - new Vector3(.1f, .4f, .1f)).sqrMagnitude < .000001f &&
                (live.size - new Vector3(.1f, .4f, .1f)).sqrMagnitude < .000001f &&
                (first.unit.GetMeshes(CollideType.CollideOnly)[0].center -
                    (owner.data.pos + new Vector3(-.9f, -1.3f, .9f))).sqrMagnitude < .000001f,
                "rotation selects Right bounds in both cached/live geometry with the correct rotated center");
            Require(trigger.center == live.center &&
                !map.utilCtrl.IsTileCenterCoveredByObject(owner, first) &&
                !map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked,
                "direction change synchronizes Trigger, pass-probe and local navigation coverage");
            second = Add(99211, map.utilCtrl.GetTile(10, 0, 10).data.pos);
            Require(second.unit.ins == null && (PhysicalSize(second.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f &&
                Mathf.Abs(PhysicalSize(first.unit).x - .1f) < .00001f,
                "same product/mask at different facings keeps independent off-screen collision bounds");
            Turn(first.unit, 180);
            Require((PhysicalSize(first.unit) - new Vector3(.6f, 2.4f, .6f)).sqrMagnitude < .000001f &&
                map.utilCtrl.IsTileCenterCoveredByObject(owner, first),
                "Down uses its wider assembled image and restores pass coverage");
            Turn(first.unit, 270);
            Require((PhysicalSize(first.unit) - new Vector3(.6f, 2.4f, .6f)).sqrMagnitude < .000001f &&
                (first.unit.GetMeshes(CollideType.CollideOnly)[0].center -
                    (owner.data.pos + new Vector3(-.2f, .9f, 0))).sqrMagnitude < .000001f,
                "Left uses its own clip and rotates the image-relative offset");
            Turn(first.unit, 0);
            Require((PhysicalSize(first.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
                "turning back restores Up without accumulating scale or offset");
            map.RemoveObject(second);
            second = null;
            product.faceType = FaceType.Fixed;
            clear.Invoke(null, new object[] { product.id });
            int emptyId = nextId++;
            generatedIds.Add(emptyId);
            BoundsTexture(emptyId, 0, 0, 0, 0);
            register.Invoke(null, new object[] { product.id, AnimDirecton.Fixed, 0, emptyId });
            controller.RefreshObjectWangTileAppearances(product.id);
            Require(PhysicalSize(first.unit).x == 0 && PhysicalSize(first.unit).z == 0
                && !map.utilCtrl.IsTileCenterCoveredByObject(owner, first)
                && !map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked,
                "transparent assembled tile clears physical, pass-probe and navigation coverage without atlas fallback");
            Require(ReferenceEquals(farGround, map.navigationCtrl.navUnits[(0, 0, 0)].dirGroundY),
                "mask refresh never rebuilds distant navigation ground");

            // Ordinary four-direction objects share the same per-unit pipeline,
            // but their texture frames must not reuse WangTile mask-zero bounds.
            product.isWangTile = false;
            product.faceType = FaceType.FourDirection;
            product.animClip = new Dictionary<AnimDirecton, List<int>> {
                [AnimDirecton.Up] = new List<int> { spriteIds[sprites[0]] },
                [AnimDirecton.Right] = new List<int> { cornerId },
                [AnimDirecton.Down] = new List<int> { wideId, cornerId },
                [AnimDirecton.Left] = new List<int> { emptyId } };
            clear.Invoke(null, new object[] { product.id });
            controller.RefreshObjectWangTileAppearances(product.id);
            Require((PhysicalSize(first.unit) - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
                "ordinary Up bounds are distinct from the previous transparent WangTile variant");
            Turn(first.unit, 44.9f);
            Require((live.size - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f,
                "ordinary Up keeps the same bounds until the direction threshold");
            Turn(first.unit, 45);
            Require((live.size - new Vector3(.1f, .4f, .1f)).sqrMagnitude < .000001f &&
                trigger.center == live.center &&
                !map.utilCtrl.IsTileCenterCoveredByObject(owner, first),
                "ordinary direction threshold immediately selects Right for live/Trigger/pass geometry");
            product.animClip[AnimDirecton.Right] = new List<int> { wideId };
            clear.Invoke(null, new object[] { product.id });
            controller.RefreshObjectWangTileAppearances(product.id);
            Require((live.size - new Vector3(.6f, 2.4f, .6f)).sqrMagnitude < .000001f,
                "ordinary clip regeneration invalidates cached facing bounds");
            Turn(first.unit, 180);
            Require((PhysicalSize(first.unit) - new Vector3(.8f, 3.6f, .9f)).sqrMagnitude < .000001f,
                "ordinary Down unions only its own animation frames");
            first.unit.Hide();
            Turn(first.unit, 270);
            Require(PhysicalSize(first.unit).x == 0 && PhysicalSize(first.unit).z == 0 &&
                !map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked,
                "hidden ordinary Left uses its transparent image and clears navigation immediately");
            first.unit.Show();
            live = first.unit.ins.GetComponentsInChildren<BoxCollider>(true).Single(c => !c.isTrigger);
            Require(live.size.x == 0 && live.size.z == 0, "pool Show restores the current ordinary facing's bounds");
            Turn(first.unit, 0);
            Require((live.size - new Vector3(.2f, .8f, .2f)).sqrMagnitude < .000001f &&
                !map.navigationCtrl.navUnits[(6, 0, 6)].objectBlocked && map.utilCtrl.IsTileCenterCoveredByObject(owner, first),
                "ordinary turn back restores fitted height and pass/navigation state without cumulative fitting");
            controller.ResetSceneUnitCaches();
            Require((PhysicalSize(first.unit) - new Vector3(1, 4, 1)).sqrMagnitude < .000001f,
                "scene reset releases ordinary directional per-unit bounds overrides as well as WangTile ones");
            controller.OnEvent(new ObjectEvent { unit = first.unit, type = MapEventType.Refresh });
            product.boundsCollision = false;
            controller.RefreshObjectWangTileAppearances(product.id);
            Require(PhysicalSize(first.unit) == new Vector3(1, 4, 1) && live.size == new Vector3(1, 4, 1),
                "turning fitting off restores baseline geometry");
        }
        finally
        {
            DynamicGlobalSettings.playing = true;
            if (second != null) map.RemoveObject(second);
            map.RemoveObject(first);
            clear.Invoke(null, new object[] { product.id });
            controller.ResetSceneUnitCaches();
            controller.Unregister<TileEvent>(); controller.Unregister<ObjectEvent>(); controller.Unregister<ItemEvent>();
            Form.MapObjectForm.RemoveData(product.id);
            foreach (int id in generatedIds) Z_DataSystem.Form.TexAssetForm.DataById.Remove(id);
            Z_DataSystem.Form.TexAssetForm.DataById.Remove(atlasId);
            Z_DataSystem.Form.GameObjectAssetForm.DataById.Remove(cubeId);
            foreach (var sprite in spriteIds.Keys) { Object.DestroyImmediate(sprite.texture); Object.DestroyImmediate(sprite); }
            if (cornerAtlas != null) Object.DestroyImmediate(cornerAtlas);
            Object.DestroyImmediate(atlas);
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
        Require(navigation.IsBaseWalkable(6, 0, 5),
            "a standable center remains walkable even when all directional connections are absent");
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

    private static void DirectionalObjectNavigation()
    {
        var controller = map.navigationCtrl;
        var type = typeof(Z_Map.Analysis.NavigationController);
        var centerBlocked = (Func<MeshInfo, Z_Map.Analysis.NavUnit, bool>)Delegate.CreateDelegate(
            typeof(Func<MeshInfo, Z_Map.Analysis.NavUnit, bool>), controller,
            type.GetMethod("IsObjectMeshBlocking", BindingFlags.Instance | BindingFlags.NonPublic));
        var directions = (Func<MeshInfo, Z_Map.Analysis.NavUnit, int>)Delegate.CreateDelegate(
            typeof(Func<MeshInfo, Z_Map.Analysis.NavUnit, int>), controller,
            type.GetMethod("GetObjectBlockingDirections", BindingFlags.Instance | BindingFlags.NonPublic));
        var node = new Z_Map.Analysis.NavUnit { realPos = Vector3.zero, dirGroundY = new float[4] };
        MeshInfo Body(Vector3 center, Vector3 size, Vector3 rotation = default) => Z_Mesh.Mesh.GetMesh(center, rotation, size);
        var axes = new[] { Vector3.right, Vector3.left, Vector3.forward, Vector3.back };
        for (int dir = 0; dir < 4; dir++)
        {
            var edgeBody = Body(axes[dir] * .45f + Vector3.up * .5f,
                dir < 2 ? new Vector3(.04f, 1, .15f) : new Vector3(.15f, 1, .04f));
            Require(!centerBlocked(edgeBody, node) && directions(edgeBody, node) == (1 << dir),
                "central 0.8 square stays walkable while a high body blocks just edge " + dir);
        }
        var cornerBody = Body(new Vector3(.45f, .5f, .45f), new Vector3(.04f, 1, .04f));
        Require(!centerBlocked(cornerBody, node) && directions(cornerBody, node) == 5,
            "1-by-0.4 edge strips span the full Tile side, including both adjoining corner edges");
        Require(!centerBlocked(Body(new Vector3(.42f, .5f, 0), new Vector3(.04f, 1, .1f)), node),
            "touching the central 0.8 square boundary does not occupy its interior");
        Require(centerBlocked(Body(new Vector3(.39f, .5f, 0), new Vector3(.04f, 1, .1f)), node),
            "body crossing inside the central 0.8 square blocks standability");
        foreach (float top in new[] { .25f, .3f, .3002f, 1f })
        {
            var edgeBody = Body(new Vector3(.45f, top * .5f, 0), new Vector3(.04f, top, .15f));
            Require(!centerBlocked(edgeBody, node) && directions(edgeBody, node) == (top > .3f ? 1 : 0),
                "directional blockers retain the strict relative-ground 0.3 threshold: " + top);
        }
        Require(directions(Body(new Vector3(.52f, .5f, 0), new Vector3(.04f, 1, .1f)), node) == 0,
            "body touching the outside Tile edge does not block its inward strip");
        Require(!centerBlocked(Z_Mesh.Mesh.GetMesh(new Vector3(.46f, .5f, 0), .03f, Vector3.zero, Vector3.one), node)
            && directions(Z_Mesh.Mesh.GetMesh(new Vector3(.46f, .5f, 0), .03f, Vector3.zero, Vector3.one), node) == 1,
            "Sphere edge occupancy uses the physical radius and keeps the central square clear");
        Require(directions(Body(new Vector3(.45f, .5f, 0), new Vector3(0, 1, .15f)), node) == 0,
            "zero-width physical bodies never block an edge");
        var diagonalBody = Body(new Vector3(.65f, .5f, .65f), new Vector3(.45f, 1, .03f), Vector3.up * 45);
        Require(directions(diagonalBody, node) == 0,
            "rotated Box AABB alone cannot mark an edge as occupied");
        var originalCellSize = map.data.mainData.mapUnitSize;
        try
        {
            map.data.mainData.mapUnitSize = new Vector3(2, 1.5f, 3);
            var wideCellBody = Body(new Vector3(.95f, .5f, 0), new Vector3(.04f, 1, .15f));
            Require(!centerBlocked(wideCellBody, node) && directions(wideCellBody, node) == 1,
                "non-unit cells retain the central 0.8 world square and place strips at the real Tile edges");
            map.data.mainData.mapUnitSize = new Vector3(.5f, 1.5f, .5f);
            var narrowCellBody = Body(new Vector3(.4f, .5f, 0), new Vector3(.04f, 1, .1f));
            Require(centerBlocked(narrowCellBody, node)
                && map.utilCtrl.TryGetObjectNavigationRange(narrowCellBody, out var min, out var max)
                && min.x == 0 && max.x == 1,
                "narrow-cell physical candidate range includes the central probe beyond the Tile's own border");
        }
        finally { map.data.mainData.mapUnitSize = originalCellSize; }
        for (int i = 0; i < 20; i++) { centerBlocked(cornerBody, node); directions(cornerBody, node); }
        long allocationStart = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) { centerBlocked(cornerBody, node); directions(cornerBody, node); }
        Require(GC.GetAllocatedBytesForCurrentThread() == allocationStart,
            "central and four-edge body clipping reuse scratch storage without per-cell allocations");

        var flat = Go("directional-flat-terrain");
        pool.AddPool(flat);
        var tiles = new List<TileUnitForm.Data>();
        for (int x = 640; x <= 644; x++)
        for (int z = 640; z <= 644; z++)
        {
            var data = new TileUnitForm.Data(28500 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, 500, z), flat.name, new Vector3(x, 750, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            map.data.RegisterMap(data); TileUnitForm.AddData(data); tiles.Add(data);
        }
        controller.RebuildNow();
        var owner = map.utilCtrl.GetTile(642, 500, 642);
        var nav = controller.navUnits[(642, 500, 642)];
        var right = controller.navUnits[(643, 500, 642)];
        var back = controller.navUnits[(642, 500, 641)];
        var far = controller.navUnits[(0, 0, 0)];
        var farGround = far.dirGroundY;
        var farLinks = far.links;
        var prefab = Go("directional-edge-obstacle");
        var collider = prefab.AddComponent<BoxCollider>();
        collider.center = new Vector3(.45f, .5f, 0);
        collider.size = new Vector3(.04f, 1, .15f);
        var trigger = prefab.AddComponent<BoxCollider>();
        trigger.isTrigger = true; trigger.size = Vector3.one * 10;
        pool.AddPool(prefab);
        var obstacle = new ObjectUnitForm.Data(28540, true, "", prefab.name, owner.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.Data overlap = null;
        ObjectUnitForm.AddData(obstacle);
        void AssertBarrier(string message)
        {
            Require(!nav.objectBlocked && !right.objectBlocked && controller.IsBaseWalkable(642, 500, 642)
                && controller.IsBaseWalkable(643, 500, 642), message + ": both centers remain standable");
            Require(!nav.links.Contains(right) && !right.links.Contains(nav) && nav.links.Contains(back),
                message + ": shared edge rejects both directions but unrelated edges remain connected");
        }
        try
        {
            map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
            Require(nav.objectBlockedDirections == 1 && right.objectBlockedDirections == 0,
                "source right-side body has an independent edge flag, not a whole-cell flag");
            AssertBarrier("source-side high obstacle");
            var bfs = new Z_Map.Analysis.Bfs(controller);
            var offsets = new List<Vector2Int> { Vector2Int.zero };
            Require(!bfs.CanPass(nav, right, offsets, Array.Empty<int>())
                && !bfs.CanPass(right, nav, offsets, Array.Empty<int>()),
                "BFS enforces both cells' shared-edge flags");
            Require(!bfs.Check(641, 644, 500, 750, 641, 643),
                "path smoothing cannot shortcut the edge barrier through two standable centers");
            var state = new Z_Map.Analysis.NavigationEndpointState();
            var direction = bfs.GetNextDir(owner.data.pos, right.realPos, 100, 0f, Array.Empty<int>(), state);
            Require(!state.directEndpoint && direction != Vector3.zero && Mathf.Abs(direction.z) > .9f,
                "BFS takes a real detour instead of direct endpoint escape or smoothing across the wall");
            RequireLocalNavMatchesFullBuild("source edge blocking");
            farGround = far.dirGroundY; farLinks = far.links;

            obstacle.pos += Vector3.right * .1f;
            map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
            Require(nav.objectBlockedDirections == 0 && right.objectBlockedDirections == 2,
                "moving to the target's left strip resets the old and new edge flags locally");
            AssertBarrier("target-side high obstacle");
            Require(ReferenceEquals(farGround, far.dirGroundY) && ReferenceEquals(farLinks, far.links),
                "edge-only refresh preserves distant navigation containers");
            RequireLocalNavMatchesFullBuild("target edge blocking");

            overlap = new ObjectUnitForm.Data(28541, true, "", prefab.name, obstacle.pos,
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
            ObjectUnitForm.AddData(overlap); map.updateCtrl.RefreshObjectOverlap(overlap.unit);
            ObjectUnitForm.RemoveData(overlap.uid); map.updateCtrl.RemoveObjectOverlap(overlap.unit); overlap = null;
            AssertBarrier("removing one of two edge obstacles");
            obstacle.isObstacle = false; map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
            Require(nav.links.Contains(right) && right.links.Contains(nav),
                "non-obstacle body and enlarged Trigger do not block edge passage");
            obstacle.isObstacle = true; map.updateCtrl.RefreshObjectOverlap(obstacle.unit);
            AssertBarrier("restoring obstacle flag");
            collider.size = new Vector3(.04f, .3f, .15f); collider.center = new Vector3(.45f, .15f, 0);
            map.updateCtrl.UpdateSingleOne(obstacle.unit);
            Require(nav.links.Contains(right) && right.links.Contains(nav),
                "lowering an edge obstacle to exactly 0.3 restores both links immediately");
            collider.size = new Vector3(.04f, 1, .15f); collider.center = new Vector3(.45f, .5f, 0);
            obstacle.pos = owner.data.pos;
            obstacle.euler = Vector3.up * 90;
            map.updateCtrl.UpdateSingleOne(obstacle.unit);
            Require(nav.objectBlockedDirections == 8 && !nav.links.Contains(back) && !back.links.Contains(nav)
                && nav.links.Contains(right), "rotation moves the edge block to Back and restores Right");
            RequireLocalNavMatchesFullBuild("rotated edge blocker");

            // Four edge-only bodies enclose a clear center. It stays standable,
            // but graph queries must not use blocked-endpoint direct movement.
            collider.center = new Vector3(.45f, .5f, 0);
            obstacle.euler = Vector3.zero;
            foreach (var axis in new[] { Vector3.left, Vector3.forward, Vector3.back })
            {
                var body = prefab.AddComponent<BoxCollider>();
                body.center = axis * .45f + Vector3.up * .5f;
                body.size = axis.x != 0 ? new Vector3(.04f, 1, .15f) : new Vector3(.15f, 1, .04f);
            }
            map.updateCtrl.UpdateSingleOne(obstacle.unit);
            Require(!nav.objectBlocked && nav.objectBlockedDirections == 15 && nav.links.Count == 0
                && controller.IsBaseWalkable(642, 500, 642), "all four blocked edges do not make the center unstandable");
            direction = bfs.GetNextDir(owner.data.pos, right.realPos, 100, 0f, Array.Empty<int>(), state);
            Require(direction == Vector3.zero && !state.directEndpoint,
                "enclosed standable start cannot escape through a directional wall via endpoint compensation");
            RequireLocalNavMatchesFullBuild("enclosed central square");
        }
        finally
        {
            if (overlap != null) { ObjectUnitForm.RemoveData(overlap.uid); map.updateCtrl.RemoveObjectOverlap(overlap.unit); }
            ObjectUnitForm.RemoveData(obstacle.uid); map.updateCtrl.RemoveObjectOverlap(obstacle.unit);
            foreach (var data in tiles)
            {
                map.data.UnRegisterMap(data); TileUnitForm.RemoveData(data.uid);
                controller.navUnits.Remove((data.mapPos.x, data.mapPos.y, data.mapPos.z));
            }
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

    public sealed class FormListRowParam : Z_Ui.Base.UiParam
    {
        public int id;
    }

    public sealed class FormListRowCtrl : Z_Ui.Base.UiCtrl
    {
        public FormListRowParam param;
        public override void SetParam(Z_Ui.Base.UiParam value) => param = (FormListRowParam)value;
    }

    private static void FormListOrdering()
    {
        var root = Go("form-list-ordering");
        root.SetActive(false);
        var parentHolder = root.AddComponent<Z_Ui.Base.UiHolder>();
        var parentCtrl = new Z_Ui.Base.UiCtrl();
        parentCtrl.BindHolderRecursively(parentHolder);
        var template = Go("form-list-template");
        template.transform.SetParent(root.transform, false);
        template.AddComponent<Z_Ui.Base.UiHolder>();
        var container = new Z_Ui.Base.UiContainer<FormListRowCtrl>(parentCtrl, template);
        void Render(params int[] ids)
        {
            container.Clear();
            foreach (int id in ids) container.Add(new FormListRowParam { id = id });
            container.Refresh();
            var rendered = root.GetComponentsInChildren<Z_Ui.Base.UiHolder>(true)
                .Where(holder => holder.gameObject.activeSelf && holder.ctrl is FormListRowCtrl)
                .Select(holder => ((FormListRowCtrl)holder.ctrl).param.id);
            Require(rendered.SequenceEqual(ids), "pooled sibling order matches sorted Form rows and the trailing New row");
        }
        // Shrinking leaves unused rows queued ahead of the just-rendered rows.
        // Re-expansion must not place new IDs at their former sibling positions.
        Render(1, 2, 3, 0);
        Render(2, 0);
        Render(1, 2, 3, 0);
        Render(3, 0);
        Render(1, 2, 3, 4, 0);
        Render(1, 2, 3, 4, 0);

        typeof(LabForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        LabForm.Init();
        typeof(Form.EventProgramDataForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Form.EventProgramDataForm.Init();
        const string belong = "RegressionFormList";
        var labs = new[] {
            new LabForm.Data(99602, "a-high", "", "", belong),
            new LabForm.Data(99603, "z-low", "", "", belong),
            new LabForm.Data(99601, "z-low", "", "", belong),
            new LabForm.Data(99620, "Regression-a-high", "", "", nameof(Form.EventProgramDataForm)),
            new LabForm.Data(99630, "Regression-z-low", "", "", nameof(Form.EventProgramDataForm)),
            new LabForm.Data(99610, "Regression-z-low", "", "", nameof(Form.EventProgramDataForm)) };
        var programs = new[] {
            new Form.EventProgramDataForm.Data(99730, "Regression-high", "", new List<string>(), new List<int>(), 0, "void", 99610),
            new Form.EventProgramDataForm.Data(99720, "Regression-middle", "", new List<string>(), new List<int>(), 0, "void", 99620),
            new Form.EventProgramDataForm.Data(99710, "Regression-low", "", new List<string>(), new List<int>(), 0, "void", 99630) };
        try
        {
            foreach (var lab in labs) LabForm.AddData(lab);
            foreach (var program in programs) Form.EventProgramDataForm.AddData(program);
            Require(Ui.UiLabRenderHelper.GetLabIds(belong).SequenceEqual(new[] { 99601, 99602 }),
                "Lab rows sort by ID rather than name/insertion and retain the lowest-ID path alias");
            var categoryMethod = typeof(Ui.ModStory.ModStoryEvent.ModStoryEventCustom.UiModStoryEventCustomCtrl)
                .GetMethod("GetCategories", BindingFlags.Static | BindingFlags.NonPublic);
            var categories = (List<string>)categoryMethod.Invoke(null, null);
            Require(categories.Where(category => category.StartsWith("Regression-", StringComparison.Ordinal))
                .SequenceEqual(new[] { "Regression-z-low", "Regression-a-high" }),
                "Program category rows sort by the lowest Lab ID, not alphabetically");
            var entries = new GameEventController(null).GetEventEntry(SceneEventType.All, "void");
            Require(entries.subs.Keys.Where(category => category.StartsWith("Regression-", StringComparison.Ordinal))
                .SequenceEqual(new[] { "Regression-z-low", "Regression-a-high" }),
                "Program chooser category order follows Lab IDs");
            Require(entries.subs["Regression-z-low"].subs.Values.Select(item => item.id).SequenceEqual(new[] { 99710, 99730 }),
                "merged Program category rows follow Data UID even across different Lab aliases");
        }
        finally
        {
            foreach (var program in programs) Form.EventProgramDataForm.RemoveData(program.uid);
            foreach (var lab in labs) LabForm.RemoveData(lab.id);
            container.Clear();
        }
    }

    private static void ObjectEmptyTextureThumbnails()
    {
        const int textureId = 99801, missingId = 99802, defaultTextureId = 99803;
        BoundsTexture(textureId, 0, 0, 10, 10);
        BoundsTexture(defaultTextureId, 0, 0, 0, 0);
        // GameManager.Init normally assigns these runtime asset IDs. This fixture
        // must distinguish a real default-placeholder ID from an unassigned 0.
        GlobalDefaultHelper.DefaultTexId = defaultTextureId;
        GlobalDefaultHelper.ExternDefaultTexId = textureId;
        var texture = TexAssetForm.DataById[textureId];
        var product = Form.MapObjectForm.defaultData;
        product.name = "empty-thumbnail-object";
        product.icon = textureId;
        product.faceType = FaceType.FourDirection;
        product.model.subUnitTexsName = new List<List<int>> { new List<int> { textureId } };
        product.animClip = Enum.GetValues(typeof(AnimDirecton)).Cast<AnimDirecton>()
            .ToDictionary(direction => direction, direction => new List<int>());
        var clip = product.GetAnimClip(AnimDirecton.Up);
        clip.Add(textureId);

        (Z_Ui.Base.UiHolder holder, Z_Ui.Base.Img image, Z_Ui.Base.Txt text,
            Z_Ui.Base.Sta exist, Z_Ui.Base.Sta selected) Row(string name)
        {
            var root = Go(name);
            root.SetActive(false);
            var holder = root.AddComponent<Z_Ui.Base.UiHolder>();
            holder.elementTrsLst = Enumerable.Repeat(root.transform, 8).ToList();
            GameObject Child(string suffix)
            {
                var child = Go(name + suffix);
                child.transform.SetParent(root.transform, false);
                return child;
            }
            Z_Ui.Base.Sta State(string suffix)
            {
                var state = Child(suffix).AddComponent<Z_Ui.Base.Sta>();
                typeof(Z_Trick.BaseFunc.State).GetField("stateGo", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(state, new[] { Child(suffix + "-0"), Child(suffix + "-1") });
                return state;
            }
            return (holder, Child("-image").AddComponent<Z_Ui.Base.Img>(),
                Child("-text").AddComponent<Z_Ui.Base.Txt>(), State("-exist"), State("-selection"));
        }
        void Empty(Z_Ui.Base.Img image, string context)
        {
            Require(!image.enabled && image.sprite == null && image.overrideSprite == null,
                context + ": no old Sprite, override or white Image fallback");
            foreach (string field in new[] { "_texData", "_animationFrames", "_animationSprites", "_animationCoroutine" })
                Require(typeof(Z_Ui.Base.Img).GetField(field, BindingFlags.Instance | BindingFlags.NonPublic)
                    .GetValue(image) == null, context + ": releases " + field);
        }
        void Visible(Z_Ui.Base.Img image, string context)
        {
            Require(image.enabled && image.sprite == texture.GetSprite(), context + ": restores the chosen texture");
        }

        var listRow = Row("object-list-empty-texture");
        var list = new ObjectList.UiBigItemCtrl {
            view = new ObjectList.UiBigItemView(listRow.holder) { img_ = listRow.image, txt_ = listRow.text, sta_exist = listRow.exist },
            model = new ObjectList.UiBigItemModel { data = product } };
        list.Refresh();
        Visible(listRow.image, "Object list first binding");
        listRow.image.overrideSprite = texture.GetSprite();
        clip.Clear();
        list.Refresh();
        Empty(listRow.image, "Empty default-facing clip does not reuse icon/legacy texture");
        foreach (int id in new[] { 0, missingId, GlobalDefaultHelper.DefaultTexId })
        {
            clip.Clear();
            clip.Add(id);
            list.Refresh();
            Empty(listRow.image, "Empty/missing/default texture ID " + id);
        }
        foreach (int animatedId in new[] { 99107, 99109 }) // GIF and WebP assets installed by BoundsCollisionPrefabs.
        {
            clip[0] = animatedId;
            list.Refresh();
            Require(listRow.image.enabled && listRow.image.sprite == TexAssetForm.DataById[animatedId].GetAnimationSprites()[0],
                "Animated Object thumbnail binds its first frame");
            list.model.data = null;
            list.Refresh();
            Empty(listRow.image, "Animated pooled thumbnail becomes New/empty");
            list.model.data = product;
        }
        clip[0] = textureId;
        list.Refresh();
        Visible(listRow.image, "Object list rebinding after empty");

        var appearanceRow = Row("object-frame-empty-texture");
        appearanceRow.holder.parent = Go("object-appearance-parent").AddComponent<Z_Ui.Base.UiHolder>();
        var appearance = new ObjectAppearance.UiModStoryMapObjectObjectAppearanceCtrl {
            model = new ObjectAppearance.UiModStoryMapObjectObjectAppearanceModel { data = product, dir = AnimDirecton.Up, id = 0 } };
        appearanceRow.holder.parent.ctrl = appearance;
        var frame = new ObjectAppearance.UiItemCtrl {
            uiHolder = appearanceRow.holder,
            view = new ObjectAppearance.UiItemView(appearanceRow.holder) {
                img_ = appearanceRow.image, txt_ = appearanceRow.text, sta_exist = appearanceRow.exist, sta_ = appearanceRow.selected },
            model = new ObjectAppearance.UiItemModel { id = 0 } };
        frame.Refresh();
        Visible(appearanceRow.image, "Appearance frame first binding");
        appearance.model.dir = AnimDirecton.Right;
        frame.Refresh();
        Empty(appearanceRow.image, "Explicitly empty facing stays blank despite populated legacy clip");
        product.GetAnimClip(AnimDirecton.Right).Add(0);
        frame.Refresh();
        Empty(appearanceRow.image, "Unassigned frame stays blank");
        appearance.model.dir = AnimDirecton.Up;
        frame.Refresh();
        Visible(appearanceRow.image, "Appearance facing rebinding");
        frame.model.id = -1;
        frame.Refresh();
        Empty(appearanceRow.image, "Pooled frame becomes the New button");

        var toolRow = Row("object-tool-empty-texture");
        toolRow.holder.parent = Go("object-tool-parent").AddComponent<Z_Ui.Base.UiHolder>();
        toolRow.holder.parent.ctrl = new ModTool.UiModToolCtrl { model = new ModTool.UiModToolModel() };
        var tool = new ModTool.UiToolItemCtrl {
            uiHolder = toolRow.holder,
            view = new ModTool.UiToolItemView(toolRow.holder) {
                img_ = toolRow.image, txt_ = toolRow.text, sta_exist = toolRow.exist, sta_ = toolRow.selected },
            model = new ModTool.UiToolItemModel(), param = new ModTool.UiToolItemParam { data = product } };
        var modManager = ModManager.instance;
        var originalSceneCtrl = modManager.sceneCtrl;
        var sceneCtrl = new ModSceneController(modManager);
        modManager.sceneCtrl = sceneCtrl;
        try
        {
            tool.OnShow();
            Visible(toolRow.image, "Mod Tool Object first binding");
            clip.Clear();
            tool.OnShow();
            Empty(toolRow.image, "Mod Tool empty Object cannot fall back to icon/another facing");
            var other = Form.MapTextureForm.defaultData;
            other.icon = textureId;
            tool.param.data = other;
            tool.OnShow();
            Visible(toolRow.image, "Non-Object pooled Tool row restores enabled Image");
        }
        finally
        {
            modManager.sceneCtrl = originalSceneCtrl;
            sceneCtrl.Unregister<Z_Input.InputKeyEvent>();
            sceneCtrl.Unregister<Z_Input.InputMouseEvent>();
            sceneCtrl.Unregister<Z_Input.InputMouseDownEvent>();
            sceneCtrl.Unregister<Z_Input.InputMouseUpEvent>();
            sceneCtrl.Unregister<Z_Input.InputMouseScrollEvent>();
            sceneCtrl.Unregister<TileEvent>();
            sceneCtrl.Unregister<ObjectEvent>();
        }

        Form.MapObjectParamForm.InitInternal();
        var assets = new ModAssetCtrl(null);
        const string name = "regression-new-empty-object";
        assets.CreateObject(name: name);
        var created = Form.MapObjectForm.DataByName[name];
        try
        {
            Require(created.animClip.Values.All(frames => frames.Count == 0) && created.model.subUnitTexsName[0].Count == 0,
                "New Object starts with no texture in any facing or legacy clip");
            assets.CreateObjectUnitTex(created.id, AnimDirecton.Fixed);
            Require(created.GetAnimClip(AnimDirecton.Fixed).SequenceEqual(new[] { 0 })
                && created.model.subUnitTexsName[0].SequenceEqual(new[] { 0 }),
                "New Object frame is stored as unassigned ID 0, including the legacy mirror");
        }
        finally { Form.MapObjectForm.RemoveData(created.id); }
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
            directions: pair.Value.objectBlockedDirections,
            heights: (float[])pair.Value.dirMaxY.Clone(),
            passTypes: pair.Value.passTypes.ToArray(),
            links: pair.Value.links.Select(link => link.pos).ToArray()));
        map.navigationCtrl.RebuildNow();
        foreach (var pair in local)
        {
            var full = map.navigationCtrl.navUnits[pair.Key];
            Require(full.objectBlocked == pair.Value.blocked
                && full.objectBlockedDirections == pair.Value.directions
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

    private static void VisibilityRegressions()
    {
        // These fixtures represent thin ceilings, not the tall generic Collider
        // used by the SAT tests. Keep their projection exception opt-in.
        var thin = Go("visibility-thin-tile");
        thin.AddComponent<BoxCollider>().size = new Vector3(1, .1f, 1);
        var trigger = thin.AddComponent<SphereCollider>();
        trigger.radius = 2;
        trigger.isTrigger = true;
        pool.AddPool(thin);
        var originals = map.data.maps.Values.Select(tile => (data: tile, prefab: tile.prefabName)).ToArray();
        try
        {
            foreach (var original in originals)
                original.data.prefabName = thin.name;
            Visibility();
            TileFirstVisibility();
            OcclusionTransparencyAndPriority();
            HighLayerHeadTileTrigger();
            ProjectedUpperColliderVisibility();
            SparseUpperLayerOcclusion();
            UnifiedTileOcclusion();
            NearbyObjectOcclusion();
            ObjectProjectionOcclusion();
            ObjectColliderOcclusion();
            ElevatedOwnerObjectOcclusion();
        }
        finally
        {
            foreach (var original in originals)
            {
                original.data.prefabName = original.prefab;
                original.data.unit.InvalidateCollisionGeometry();
            }
        }
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
        Require(ReadVisionDegree(instance, 0) == 0f, "high Tile occlusion is fully transparent");
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
        Require(ReadVisionDegree(instance, 0) == 1f, "departed occlusion restored");
        for (int i = 0; i < 6; i++)
        {
            Require(ReadVisionDegree(instance, i) == (i == 0 || i == 3 ? 1f : 0f), "restored normal/front layer pairing");
            Require(ReadLightSensitivity(instance, i) == i + 2, "layer 0 keeps every initial light sensitivity");
        }
        instance.displayLayer = 1;
        instance.VisOn();
        for (int i = 0; i < 6; i++)
        {
            bool lowerLayer = i == 0 || i == 3;
            bool displayed = lowerLayer || i == 1 || i == 4;
            Require(ReadVisionDegree(instance, i) == (displayed ? 1f : 0f), "layer 1 keeps the cumulative display ceiling");
            Require(ReadLightSensitivity(instance, i) == (i + 2) * (lowerLayer ? 0.5f : 1f),
                "layer 1 halves only lower-layer light sensitivity");
        }
        instance.displayLayer = 2;
        instance.VisOn();
        for (int i = 0; i < 6; i++)
        {
            bool currentLayer = i == 2 || i == 5;
            Require(ReadVisionDegree(instance, i) == 1f, "layer 2 displays all layers cumulatively");
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
        for (int i = 0; i < 6; i++)
        {
            instance.renderers[i].GetPropertyBlock(block);
            bool displayed = i == 0 || i == 3;
            Require(block.GetFloat("_Show") == (displayed ? 1f : 0f)
                && block.GetFloat("_FadeCenter") == (displayed ? 1f : 0f),
                "semi-transparent slots submit full Show and a camera-center fade flag");
            Require(block.GetFloat("_RegressionTextureProperty") == i + 10,
                "fade submission preserves unrelated renderer properties");
        }
        reads = instance.rendererReads;
        instance.ApplyVision(0.5f);
        Require(instance.rendererReads == reads, "unchanged fade flags skip renderer submissions");
        instance.ApplyVision(0.5f, 1f);
        Require(ReadVisionDegree(instance, 0) == .5f && ReadVisionDegree(instance, 3) == 1f,
            "kept-visible front clears only its own fade flag");
        instance.ApplyVision(1f, 0.5f);
        Require(ReadVisionDegree(instance, 0) == 1f && ReadVisionDegree(instance, 3) == .5f,
            "normal and front can fade independently although both submit Show=1");
        instance.ApplyVision(1f);
        Require(ReadVisionDegree(instance, 0) == 1f && ReadVisionDegree(instance, 3) == 1f,
            "ordinary visibility clears cached fade flags despite unchanged Show");
        instance.ApplyVision(0.5f);
        go.SetActive(false);
        foreach (var renderer in instance.renderers)
            renderer.SetPropertyBlock(null);
        go.SetActive(true);
        instance.ApplyVision(0.5f);
        Require(ReadVisionDegree(instance, 0) == 0.5f, "pool re-enable invalidates visibility cache");
        instance.VisOff();
        Require(!instance.vising && ReadVisionDegree(instance, 0) == 0, "hidden state");
        instance.VisOn();
        Require(instance.vising && ReadVisionDegree(instance, 0) == 1, "show after hide");
        Vector3 savedCellSize = map.data.mainData.mapUnitSize;
        try
        {
            map.data.mainData.mapUnitSize = new Vector3(2f, 1.5f, 4f);
            map.updateCtrl.curCenterPos = new Vector3(5.25f, 4.5f, 5.75f);
            update.Invoke(map.updateCtrl, null);
            Require(Shader.GetGlobalVector("_MapFadeCenterRange") == new Vector4(6f, 12f, 0f, 0f),
                "fade range uses three real X/Z cells, never cell height");
            Require((Vector3)Shader.GetGlobalVector("_MapFadeCenterPosition") == map.updateCtrl.curCenterPos,
                "perspective reference depth follows the exact current map center");
        }
        finally
        {
            map.data.mainData.mapUnitSize = savedCellSize;
            update.Invoke(map.updateCtrl, null);
        }
        map.updateCtrl.End();
        Require(map.updateCtrl.characterOverlapTileDic.GetDicT1().Count == 0, "end clears index");
    }

    private static float ReadVisionDegree(MapInstance instance, int index)
    {
        var block = new MaterialPropertyBlock();
        instance.renderers[index].GetPropertyBlock(block);
        float show = block.GetFloat("_Show");
        float fadeCenter = block.GetFloat("_FadeCenter");
        Require(fadeCenter == 0f || fadeCenter == 1f, "camera-center fade flag is binary");
        Require(show != .5f, "automatic semi-transparent vision never submits fixed Show=0.5");
        Require(fadeCenter == 0f || show == 1f, "a fade flag requires full shader Show");
        Require(show != 0f || fadeCenter == 0f, "hidden/display-disabled slots clear the fade flag");
        // Existing geometry assertions still compare logical occlusion states.
        return fadeCenter == 1f ? .5f : show;
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
        Require(final[character] == 1 && ReadVisionDegree(characterIns, 0) == 1,
            "character visibility uses owner, not collision coverage");
        Require(final[tall] == 0.5f && ReadVisionDegree(tallIns, 0) == 0.5f,
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
        Require(ReadVisionDegree(boundIns, 0) == 0.5f, "new bound instance inherits unchanged parent degree");
        ctrl.characterTileDic.Move(character, high);
        ctrl.itemTileDic.Move(item, outside);
        update();
        Require(ReadVisionDegree(characterIns, 0) == 0f && ReadVisionDegree(itemIns, 0) == 1,
            "moving ownership refreshes degree even outside current Tile set");

        var late = new ItemUnitForm.Data(9106, "", source.name, high.data.pos,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        ctrl.itemTileDic.Add(late, high);
        ctrl.curItemLst.Add(late.data);
        update();
        var lateIns = Probe(late);
        update();
        Require(ReadVisionDegree(lateIns, 0) == 0f, "newly shown instance receives previously collected degree");

        DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
        // Special visual-footprint candidates must work even outside the visible list.
        ctrl.curObjectLst.Remove(tall.data);
        update();
        Require(ReadVisionDegree(tallIns, 0) == 0.5f, "visual coverage occludes tall Object with offscreen owner");
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        update();
        Require(ReadVisionDegree(tallIns, 0) == 0.5f, "mixed-layer precedence also applies outside side view and visible list");
        ctrl.objectTileDic.Del(tall, high);
        update();
        Require(ReadVisionDegree(tallIns, 0) == 1, "departed visual-footprint override restores omitted visible Object");

        ctrl.SetGroupVision(high, 0.25f);
        Require(ReadVisionDegree(objIns, 0) == 0.25f && ReadVisionDegree(characterIns, 0) == 0.25f
            && ReadVisionDegree(lateIns, 0) == 0.25f && ReadVisionDegree(boundIns, 0) == 0.25f,
            "public SetGroupVision remains immediate and includes bound units");
        Require(ReadVisionDegree(tallIns, 0) == 1, "public group operation ignores non-owner overlap links");
        ctrl.SetGroupVision(map.utilCtrl.GetTile(11, 0, 11), 0.5f);
        Require(ctrl.objectTileDic.GetDicT2().Count == objectsBefore, "public empty group query remains read-only");
        ctrl.End();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl)).Count == 0, "End clears final visibility state");
        ctrl.curTileLst.Add(high.data);
        ctrl.curObjectLst.Add(obj.data);
        ctrl.objectTileDic.Add(obj, high);
        update();
        Require(ReadVisionDegree(objIns, 0) == 0f, "visibility rebuilds high-only Object after End and re-entry");
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
            new Vector3Int(6, 4, 6),
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
        var surroundedHighInstance = ProbeTile(surroundedHighTile);
        var surroundedFrontInstance = ProbeTile(map.utilCtrl.GetTile(7, 4, 6));
        var surroundedHighObject = OcclusionObject(9209, surroundedHighTile.data.pos, surroundedHighTile);
        map.navigationCtrl.navUnits[(6, 0, 11)].links.Clear();
        map.navigationCtrl.navUnits[(5, 0, 10)].links.Clear();
        map.navigationCtrl.navUnits[(7, 0, 9)].links.Clear();
        map.navigationCtrl.navUnits[(6, 0, 12)].passTypes.Add(7001);

        var updateMethod = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl, updateMethod);
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        Require(map.utilCtrl.ContainsTile(6, 4, 6),
            "full transparency requires an occluding center Tile above the player");
        foreach (var mode in new[] { CameraMode.Overhead, CameraMode.Isometric })
        {
            DynamicGlobalSettings.cameraMode = mode;
            update();
            var final = (Dictionary<MapUnit, float>)finalField.GetValue(ctrl);
            Require(final[map.utilCtrl.GetTile(12, 1, 6)] == 0f, "connected high Tile searches the full view width: " + mode);
            Require(final[map.utilCtrl.GetTile(10, 1, 10)] == 0f, "connected diagonal high Tile is not radius-limited: " + mode);
            Require(final[halfHighTile] == 0f && final[frontProjectionTile] == 0f,
                "isolated overhead Tiles fully hide, ignoring blocked navigation projections and pass types: " + mode);
            for (int i = 0; i < 6; i++)
                Require(ReadVisionDegree(frontProjectionInstance, i) == 0f,
                    "front renderers share isolated overhead Tile full transparency: " + mode);
            Require(final[surroundedHighTile] == 0f && final[map.utilCtrl.GetTile(7, 4, 6)] == 0f
                && final[map.utilCtrl.GetTile(6, 4, 5)] == 0f && final[map.utilCtrl.GetTile(6, 4, 7)] == 0f,
                "surrounded upper-layer Tiles are hidden even over blocked projected navigation cells: " + mode);
            for (int i = 0; i < 6; i++)
            {
                Require(ReadVisionDegree(surroundedHighInstance, i) == 0f,
                    "surrounded Tile body and front renderers are all hidden regardless of projection: " + mode);
                Require(ReadVisionDegree(surroundedFrontInstance, i) == 0f,
                    "full hiding does not leave front renderers opaque over their independently blocked projection: " + mode);
            }
            float nearbyObjectDegree = mode == CameraMode.Isometric ? .5f : 0f;
            Require(final[halfHighObject] == nearbyObjectDegree && final[surroundedHighObject] == nearbyObjectDegree,
                "nearby side-view Objects stay half transparent over fully hidden Tiles: " + mode);
            Require(final[layerRelativeSeedTile] == (mode == CameraMode.Isometric ? .5f : 1f),
                "an isolated high Tile whose projection reaches the fade region is half transparent only in side view: " + mode);
            Require(final[highOnly] == nearbyObjectDegree && ReadVisionDegree(((MapUnit)highOnly).ins, 0) == nearbyObjectDegree,
                "nearby Object occlusion ignores the owner's layer height: " + mode);
            Require(final[highOwnerMixed] == 0.5f && final[groundOwnerMixed] == 0.5f,
                "current-layer priority is independent of owner/link order: " + mode);
            Require(final[omittedMixed] == 0.5f && final[omittedHigh] == nearbyObjectDegree,
                "high-layer visual-footprint candidates work outside the visible Object list: " + mode);
            Require(final[currentOnly] == (mode == CameraMode.Isometric ? 0.5f : 1f),
                "current-only Object retains side-view occlusion rule: " + mode);
            Require(final[fartherObject] == 1f,
                "an Object outside the horizontal half-opacity radius stays opaque: " + mode);
        }

        var navigation = map.navigationCtrl;
        map.navigationCtrl = null;
        try
        {
            update();
            var independentVision = (Dictionary<MapUnit, float>)finalField.GetValue(ctrl);
            Require(independentVision[halfHighTile] == 0f && independentVision[surroundedHighTile] == 0f,
                "isolated and connected overhead full occlusion require no navigation controller");
        }
        finally { map.navigationCtrl = navigation; }

        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        ctrl.curCenterPos = new Vector3(6, 0, 2);
        update();
        var restored = (Dictionary<MapUnit, float>)finalField.GetValue(ctrl);
        Require(restored[surroundedHighTile] == 1f && restored[surroundedHighObject] == 1f,
            "leaving the surrounded region restores projection-blocked Tile and Object visibility");
        for (int i = 0; i < 6; i++)
            Require(ReadVisionDegree(surroundedHighInstance, i) == 1f,
                "leaving the surrounded region restores both Tile body and front renderers");
        ctrl.curCenterPos = new Vector3(6, 0, 6);

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
            Require(ReadVisionDegree(frontProjectionInstance, 3) == 0.5f,
                "Mod higher-layer opacity applies equally to front renderers: " + mode);
        }
        DynamicGlobalSettings.playing = true;

        DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
        ctrl.curCenterPos = new Vector3(6, 0, 5);
        update();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[layerRelativeSeedTile] == .5f,
            "a distant source Tile still half-fades when its player-layer projection is within radius three");
        ctrl.curCenterPos = new Vector3(6, 0, 4);
        update();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[layerRelativeSeedTile] == .5f,
            "a Tile at height equal to backward depth is a direct half hit");
        ctrl.curCenterPos = new Vector3(6, 0, 3);
        update();
        Require(((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[layerRelativeSeedTile] == .5f,
            "an isolated height-three Tile at backward depth two is half transparent");
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
        Require(ReadVisionDegree(((MapUnit)omittedHigh).ins, 0) == 1f && ReadVisionDegree(((MapUnit)omittedMixed).ins, 0) == 1f,
            "leaving the occluding layers restores omitted high/mixed Objects from full/half transparency");
        ctrl.End();
        map.data.mainData.viewSize = originalViewSize;
    }

    private static void HighLayerHeadTileTrigger()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        map.data.mainData.viewSize = new Vector3Int(20, 10, 20);
        var player = new Vector3Int(6, 0, 6);
        string thinPrefab = map.utilCtrl.GetTileData(6, 1, 6).prefabName;
        var added = new List<TileUnitForm.Data>();
        TileUnitForm.Data Add(int x, int y, int z)
        {
            var data = new TileUnitForm.Data(20200 + added.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), thinPrefab, new Vector3(x, y * 1.5f, z), Vector3.zero,
                Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            added.Add(data);
            map.data.RegisterMap(data);
            return data;
        }
        var center = Add(6, 7, 6);
        var sides = new[] { Add(5, 7, 6), Add(7, 7, 6), Add(6, 7, 5), Add(6, 7, 7) };
        Add(6, 7, 8); Add(6, 7, 9);
        var distantConnected = Add(6, 7, 10);
        var separate = Add(6, 7, 3);
        var centerUpper = Add(6, 1, 13);
        var backUpper = map.utilCtrl.GetTileData(6, 1, 12);
        var oldBackScale = backUpper.scale;
        var centerInstance = ProbeTile(center.unit);
        var updateMethod = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl, updateMethod);
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(TileUnitForm.Data tile) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[tile.unit];
        void Remove(TileUnitForm.Data tile)
        {
            ctrl.curTileLst.Remove(tile);
            map.data.UnRegisterMap(tile);
        }
        void Restore(TileUnitForm.Data tile)
        {
            map.data.RegisterMap(tile);
            ctrl.curTileLst.Add(tile);
        }
        try
        {
            foreach (var tile in map.data.maps.Values) ctrl.curTileLst.Add(tile);
            typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, player);
            foreach (var mode in new[] { CameraMode.Overhead, CameraMode.Isometric })
            {
                DynamicGlobalSettings.cameraMode = mode;
                ctrl.curCenterPos = new Vector3(6, 0, 6);
                foreach (var missingSide in sides)
                {
                    Remove(missingSide);
                    update();
                    Require(Degree(center) == 0 && Degree(distantConnected) ==
                        (missingSide == sides[3] ? 1 : 0),
                        "overhead Tile fully hides its component after removing any cardinal neighbour: " + mode);
                    Require(Degree(separate) == 1f,
                        "a disconnected upper component retains its ordinary opacity: " + mode);
                    Restore(missingSide);
                }
                Remove(sides[0]); Remove(sides[1]);
                update();
                Require(Degree(center) == 0 && Degree(distantConnected) == 0,
                    "only two cardinal neighbours still fully hide the overhead component beyond the half-opacity radius: " + mode);
                Restore(sides[0]); Restore(sides[1]);
                foreach (var side in sides) Remove(side);
                update();
                Require(Degree(center) == 0 && Degree(distantConnected) == 1,
                    "an isolated overhead Tile fully hides without any cardinal neighbour: " + mode);
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(centerInstance, i) == 0,
                        "isolated overhead Tile body/front hide without center fade: " + mode);
                Restore(sides[3]);
                update();
                Require(Degree(center) == 0 && Degree(distantConnected) == 0,
                    "a single cardinal neighbour connects the overhead Tile to the fully hidden component: " + mode);
                for (int i = 0; i < 3; i++) Restore(sides[i]);
                Remove(center);
                update();
                Require(sides.All(side => Degree(side) != 0) && Degree(distantConnected) != 0,
                    "four existing cardinal Tiles without an occluding center no longer cause full hiding: " + mode);
                Restore(center);

                backUpper.scale = new Vector3(1, 6, 1);
                update();
                Require(Degree(center) == 0 && Degree(sides[2]) == 1 && ReadVisionDegree(centerInstance, 3) == 1,
                    "head-triggered full hiding preserves cardinal body and independent front exceptions: " + mode);
                Remove(sides[0]);
                update();
                Require(Degree(center) == 0 && Degree(distantConnected) == 0,
                    "a kept-visible neighbour does not prevent head-triggered full hiding: " + mode);
                Restore(sides[0]);
                centerUpper.scale = new Vector3(1, 6, 1);
                update();
                Require(Degree(center) == 1 && Degree(distantConnected) == 1,
                    "a center kept visible by projected Collider height prevents full-component hiding: " + mode);
                centerUpper.scale = Vector3.one;
                backUpper.scale = oldBackScale;
                update();
                Require(Degree(center) == 0 && Degree(distantConnected) == 0,
                    "removing visibility exceptions restores full hiding on the next frame: " + mode);
                ctrl.curCenterPos = new Vector3(6, 0, 10);
                update();
                Require(Degree(center) == 0 && Degree(distantConnected) == 0,
                    "moving under another Tile in the same component keeps the component fully hidden: " + mode);
                ctrl.curCenterPos = new Vector3(6, 0, 11);
                update();
                Require(Degree(center) == 1 && Degree(distantConnected) == 1f,
                    "leaving all overhead Tiles restores ordinary opacity and the half-opacity radius: " + mode);
            }
        }
        finally
        {
            backUpper.scale = oldBackScale;
            ctrl.End();
            foreach (var tile in added)
            {
                tile.unit.Hide();
                map.data.UnRegisterMap(tile);
            }
            map.data.mainData.viewSize = oldViewSize;
            DynamicGlobalSettings.cameraMode = oldMode;
        }
    }

    private static void ProjectedUpperColliderVisibility()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var originalViewSize = map.data.mainData.viewSize;
        var originalMode = DynamicGlobalSettings.cameraMode;
        map.data.mainData.viewSize = new Vector3Int(20, 9, 20);
        ctrl.curCenterPos = new Vector3(6, 0, 6);
        typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(ctrl, new Vector3Int(6, 0, 6));
        foreach (var tile in map.data.maps.Values) ctrl.curTileLst.Add(tile);
        var hidden = map.utilCtrl.GetTile(5, 4, 6);
        var hiddenInstance = ProbeTile(hidden);
        var highObject = OcclusionObject(9300, hidden.data.pos, hidden);
        var half = map.utilCtrl.GetTile(6, 5, 6);
        var halfInstance = ProbeTile(half);
        var upper = map.utilCtrl.GetTileData(5, 1, 10);
        var frontUpper = map.utilCtrl.GetTileData(5, 1, 9);
        var halfUpper = map.utilCtrl.GetTileData(6, 1, 11);
        var halfFrontUpper = map.utilCtrl.GetTileData(6, 1, 10);
        var ground = map.utilCtrl.GetTileData(5, 0, 10);
        var higher = map.utilCtrl.GetTileData(5, 2, 10);
        var originals = new[] { upper, frontUpper, halfUpper, halfFrontUpper, ground, higher }
            .Select(tile => (data: tile, scale: tile.scale, euler: tile.euler, prefab: tile.prefabName)).ToArray();
        string thinPrefab = upper.prefabName;
        var triggerOnly = Go("projection-trigger-only");
        var trigger = triggerOnly.AddComponent<SphereCollider>();
        trigger.radius = 3;
        trigger.isTrigger = true;
        pool.AddPool(triggerOnly);
        var sphereBody = Go("projection-sphere-body");
        sphereBody.AddComponent<SphereCollider>().radius = .25f;
        pool.AddPool(sphereBody);
        var updateMethod = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl, updateMethod);
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        Func<MapUnit, float> degree = unit => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[unit];
        try
        {
            // Tall bodies on the projected player layer or a still higher layer
            // do not substitute for exactly playerLayer + 1.
            ground.scale = higher.scale = new Vector3(1, 20, 1);
            foreach (var mode in new[] { CameraMode.Overhead, CameraMode.Isometric })
            {
                DynamicGlobalSettings.cameraMode = mode;
                upper.scale = frontUpper.scale = halfUpper.scale = halfFrontUpper.scale = Vector3.one;
                upper.euler = Vector3.zero;
                upper.prefabName = thinPrefab;
                update();
                Require(degree(hidden) == 0f && degree(half) == 0f,
                    "thin upper Tiles and tall Triggers do not exempt body occlusion; other layers are ignored: " + mode);
                upper.scale = new Vector3(1, 5, 1);
                update();
                Require(degree(hidden) == 0f, "exactly .5 world-Y Collider height stays occludable: " + mode);
                upper.scale = new Vector3(1, 6, 1);
                update();
                Require(degree(hidden) == 1f && degree(highObject) == (mode == CameraMode.Isometric ? .5f : 1f),
                    "projected upper Collider restores the Tile but does not suppress independent nearby Object occlusion: " + mode);
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(hiddenInstance, i) == 1f, "body exemption keeps all Tile renderers visible: " + mode);
                upper.scale = Vector3.one;
                frontUpper.scale = new Vector3(1, 6, 1);
                update();
                Require(degree(hidden) == 0f && degree(highObject) == (mode == CameraMode.Isometric ? .5f : 0f),
                    "front-only exemption preserves the Tile body and independent nearby Object degrees: " + mode);
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(hiddenInstance, i) == (i < 3 ? 0f : 1f),
                        "front uses the independent one-cell-nearer upper Tile Collider: " + mode);
                frontUpper.scale = Vector3.one;
                halfUpper.scale = new Vector3(1, 6, 1);
                update();
                Require(degree(half) == 1f, "upper Collider exemption also applies to an isolated overhead Tile: " + mode);
                halfUpper.scale = Vector3.one;
                halfFrontUpper.scale = new Vector3(1, 6, 1);
                update();
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(halfInstance, i) == (i < 3 ? 0f : 1f),
                        "fully hidden isolated overhead Tile can retain opaque front parts independently: " + mode);
                halfFrontUpper.scale = Vector3.one;
                upper.euler = new Vector3(90, 0, 0);
                update();
                Require(degree(hidden) == 1f, "Collider world-Y extent includes Tile rotation: " + mode);
                upper.euler = Vector3.zero;
                upper.prefabName = triggerOnly.name;
                update();
                Require(degree(hidden) == 0f, "a trigger-only upper Tile is not a visibility exemption: " + mode);
                upper.prefabName = sphereBody.name;
                update();
                Require(degree(hidden) == 0f, "Sphere Collider diameter .5 stays occludable: " + mode);
                upper.scale = Vector3.one * 1.2f;
                upper.euler = new Vector3(45, 0, 0);
                update();
                Require(degree(hidden) == 1f, "rotated Sphere uses its physical diameter, not rotated sample Y extrema: " + mode);
                upper.euler = Vector3.zero;
                upper.scale = Vector3.one * 2;
                update();
                Require(degree(hidden) == 1f, "scaled Sphere physical height above .5 keeps the projected Tile visible: " + mode);
            }
            upper.prefabName = thinPrefab;
            upper.scale = Vector3.one;
            map.data.UnRegisterMap(upper);
            ctrl.curTileLst.Remove(upper);
            try
            {
                update();
                Require(degree(hidden) == 0f, "missing exact upper Tile does not restore visibility");
            }
            finally
            {
                map.data.RegisterMap(upper);
                ctrl.curTileLst.Add(upper);
            }
        }
        finally
        {
            foreach (var original in originals)
            {
                original.data.scale = original.scale;
                original.data.euler = original.euler;
                original.data.prefabName = original.prefab;
                original.data.unit.InvalidateCollisionGeometry();
            }
            ctrl.End();
            map.data.mainData.viewSize = originalViewSize;
            DynamicGlobalSettings.cameraMode = originalMode;
        }
    }

    private static void SparseUpperLayerOcclusion()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        bool oldPlaying = DynamicGlobalSettings.playing;
        string thinPrefab = map.utilCtrl.GetTileData(6, 1, 6).prefabName;
        var added = new List<TileUnitForm.Data>();
        TileUnitForm.Data Add(int x, int y, int z)
        {
            var data = new TileUnitForm.Data(21200 + added.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), thinPrefab, map.utilCtrl.MapPos2RealPos(new Vector3(x, y, z)),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            added.Add(data);
            map.data.RegisterMap(data);
            ctrl.curTileLst.Add(data);
            return data;
        }
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic));
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(TileUnitForm.Data tile) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[tile.unit];
        try
        {
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            map.data.mainData.viewSize = new Vector3Int(8, 5, 8);
            foreach (int playerY in new[] { 0, 6 })
            {
                var player = new Vector3Int(80, playerY, 80);
                typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(ctrl, player);
                ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(player);
                var ground = Add(80, playerY, 80);
                var gapped = Add(80, playerY + 2, 79);
                var topVisible = Add(80, playerY + 4, 79);
                var horizontalBoundary = Add(80, playerY + 4, 77);
                var horizontalOutside = Add(80, playerY + 4, 76);
                var outside = Add(80, playerY + 5, 79);
                var instance = ProbeTile(gapped.unit);
                Require(!map.utilCtrl.ContainsTile(80, playerY + 1, 79),
                    "the foreground column has an empty immediate upper layer");
                update();
                Require(Degree(gapped) == .5f && ReadVisionDegree(instance, 0) == .5f,
                    "a foreground Tile two layers up remains an occluder across an empty intermediate layer");
                Require(Degree(topVisible) == .5f && Degree(outside) == .5f && Degree(ground) == 1f,
                    "sparse direct occlusion includes the fifth upper layer relative to the player layer");
                Require(Degree(horizontalBoundary) == .5f && Degree(horizontalOutside) == .5f,
                    "direct Tile half hits include height equal to depth outside the old anchor-radius cutoff");

                Add(80, playerY + 2, 80);
                update();
                Require(Degree(gapped) == 0f && ReadVisionDegree(instance, 0) == 0f,
                    "one overhead Tile fully hides its foreground component across an empty intermediate layer");
                var topHead = Add(80, playerY + 4, 80);
                var outsideHead = Add(80, playerY + 5, 80);
                update();
                Require(Degree(topHead) == 0f && Degree(topVisible) == 0f
                    && Degree(horizontalBoundary) == .5f && Degree(outsideHead) == .5f && Degree(outside) == .5f,
                    "legacy head triggers use displayed layers while direct half hits include the fifth layer");
                ctrl.End();
                foreach (var tile in added) map.data.UnRegisterMap(tile);
                added.Clear();
            }
        }
        finally
        {
            ctrl.End();
            foreach (var tile in added) map.data.UnRegisterMap(tile);
            map.data.mainData.viewSize = oldViewSize;
            DynamicGlobalSettings.cameraMode = oldMode;
            DynamicGlobalSettings.playing = oldPlaying;
        }
    }

    private static void UnifiedTileOcclusion()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        bool oldPlaying = DynamicGlobalSettings.playing;
        string thinPrefab = map.utilCtrl.GetTileData(6, 1, 6).prefabName;
        var tiles = new List<TileUnitForm.Data>();
        TileUnit Tile(int x, int y, int z)
        {
            var data = new TileUnitForm.Data(21300 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), thinPrefab, map.utilCtrl.MapPos2RealPos(new Vector3Int(x, y, z)),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            tiles.Add(data);
            map.data.RegisterMap(data);
            ctrl.curTileLst.Add(data);
            return data.unit;
        }
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic));
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(MapUnit unit) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[unit];
        try
        {
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            // The requested five-layer scan does not stop at viewSize.y - 1.
            map.data.mainData.viewSize = new Vector3Int(8, 3, 8);
            foreach (int layer in new[] { 0, 6 })
            {
                var center = new Vector3Int(100, layer, 100);
                ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(center);
                typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(ctrl, center);
                var ground = Tile(100, layer, 100);
                var depth1 = Tile(97, layer + 2, 99);
                var depth2 = Tile(103, layer + 3, 98);
                var depth4 = Tile(98, layer + 5, 96);
                var touching = Tile(102, layer + 5, 95);
                var equalHeight = Tile(100, layer + 1, 99);
                var belowThreshold = Tile(99, layer + 2, 97);
                var mirroredProjection = Tile(101, layer + 2, 97);
                var projectedBoundary = Tile(100, layer + 1, 96);
                var projectedOutside = Tile(99, layer + 1, 95);
                var outsideX = Tile(104, layer + 2, 99);
                var outsideY = Tile(99, layer + 6, 98);
                var outsideZ = Tile(101, layer + 5, 94);
                var front = ProbeTile(depth2);
                var projectedInstance = ProbeTile(belowThreshold);
                update();
                Require(Degree(depth1) == .5f && Degree(depth2) == .5f && Degree(depth4) == .5f
                    && Degree(touching) == .5f && Degree(equalHeight) == .5f,
                    "direct Tile half hits include height equal to depth through the fifth layer/depth");
                Require(Degree(belowThreshold) == .5f && Degree(mirroredProjection) == .5f,
                    "left/right one, backward three, height two projects to backward one and must half-fade");
                Require(Degree(projectedBoundary) == .5f && Degree(projectedOutside) == 1f,
                    "fallback eligibility uses projected X/Z radius, not source Tile distance or height/depth alone");
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(projectedInstance, i) == .5f,
                        "projected-region hit submits half opacity to actual body/front renderers");
                Require(Degree(ground) == 1f
                    && Degree(outsideX) == 1f && Degree(outsideY) == 1f && Degree(outsideZ) == 1f,
                    "current layer and outside seven-column/five-depth/five-height bounds stay opaque");

                var bodyUpper = Tile(97, layer + 1, 101);
                var frontUpper = Tile(103, layer + 1, 100);
                bodyUpper.data.scale = frontUpper.data.scale = new Vector3(1, 6, 1);
                update();
                Require(Degree(depth1) == 1f && Degree(depth2) == .5f,
                    "projected upper Collider height wins over direct Tile half hits");
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(front, i) == (i < 3 ? .5f : 1f),
                        "front renderers retain their independent projected Collider exception");
                bodyUpper.data.scale = frontUpper.data.scale = Vector3.one;
                update();
                Require(Degree(depth1) == .5f && ReadVisionDegree(front, 3) == .5f,
                    "same-frame Collider cache is cleared before the next visibility update");
                var projectedUpper = Tile(99, layer + 1, 99);
                var projectedFrontUpper = Tile(99, layer + 1, 98);
                projectedUpper.data.scale = new Vector3(1, 6, 1);
                update();
                Require(Degree(belowThreshold) == 1f,
                    "upper Collider opacity exemption still wins over the new projected-region fallback");
                projectedUpper.data.scale = Vector3.one;
                projectedFrontUpper.data.scale = new Vector3(1, 6, 1);
                update();
                Require(Degree(belowThreshold) == .5f,
                    "front-only Collider exemption does not suppress a projected-region body hit");
                for (int i = 0; i < 6; i++)
                    Require(ReadVisionDegree(projectedInstance, i) == (i < 3 ? .5f : 1f),
                        "projected-region fallback retains independent opaque front renderers");
                projectedFrontUpper.data.scale = Vector3.one;
                update();
                for (int i = 0; i < 10; i++) update();
                long before = GC.GetAllocatedBytesForCurrentThread();
                for (int i = 0; i < 100; i++) update();
                Require(GC.GetAllocatedBytesForCurrentThread() == before, "unified Tile scan is allocation-free after warmup");
                DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
                update();
                Require(Degree(belowThreshold) == 1f, "Overhead does not enable side-view projected-region fallback");
                DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
                update();
                Require(Degree(belowThreshold) == .5f, "returning to side view restores projected-region fallback");
                ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(center + new Vector3Int(20, 0, 20));
                update();
                Require(Degree(belowThreshold) == 1f && ReadVisionDegree(projectedInstance, 0) == 1f,
                    "leaving the projected occlusion region restores opacity and clears fade flags");
                ctrl.End();
                foreach (var tile in tiles) map.data.UnRegisterMap(tile);
                tiles.Clear();
            }
        }
        finally
        {
            ctrl.End();
            foreach (var tile in tiles) map.data.UnRegisterMap(tile);
            map.data.mainData.viewSize = oldViewSize;
            DynamicGlobalSettings.cameraMode = oldMode;
            DynamicGlobalSettings.playing = oldPlaying;
        }
    }

    private static void NearbyObjectOcclusion()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        bool oldPlaying = DynamicGlobalSettings.playing;
        string thinPrefab = map.utilCtrl.GetTileData(6, 1, 6).prefabName;
        var tiles = new List<TileUnitForm.Data>();
        var units = new List<ObjectUnit>();
        var prefab = Go("nearby-object-occlusion");
        prefab.AddComponent<ObjectInstance>();
        var model = Go("nearby-object-occlusion-model");
        model.transform.SetParent(prefab.transform, false);
        model.transform.localPosition = Vector3.up * .5f;
        model.AddComponent<BoxCollider>();
        pool.AddPool(prefab);
        TileUnit Tile(int x, int y, int z)
        {
            var data = new TileUnitForm.Data(21400 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), thinPrefab, map.utilCtrl.MapPos2RealPos(new Vector3(x, y, z)),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            tiles.Add(data);
            map.data.RegisterMap(data);
            ctrl.curTileLst.Add(data);
            return data.unit;
        }
        ObjectUnit Member(TileUnit owner, float height = 12)
        {
            var unit = new ObjectUnitForm.Data(21500 + units.Count, false, "", prefab.name, owner.data.pos,
                Vector3.zero, new Vector3(1, height, .1f), UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
            units.Add(unit);
            ctrl.objectTileDic.Add(unit, owner);
            ctrl.curObjectLst.Add(unit.data);
            Probe(unit);
            return unit;
        }
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic));
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(MapUnit unit) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[unit];
        try
        {
            map.data.mainData.viewSize = new Vector3Int(8, 5, 8);
            var center = new Vector3Int(80, 0, 80);
            typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ctrl, center);
            ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(center);
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            DynamicGlobalSettings.playing = true;
            var leftTile = Tile(79, 0, 79);
            var left = Member(leftTile);
            var sameOwnerShort = Member(leftTile, .1f);
            var below = Member(Tile(80, -1, 78));
            var elevated = Member(Tile(81, 2, 79), .8f);
            var highest = Member(Tile(80, 4, 79), .8f);
            var boundary = Member(Tile(80, 0, 77));
            var elevatedBoundary = Member(Tile(80, 4, 77), .8f);
            var outside = Member(Tile(80, 0, 76));
            var elevatedOutside = Member(Tile(80, 4, 76), .8f);
            var outsideDiagonal = Member(Tile(83, 0, 78));
            var behind = Member(Tile(80, 0, 81));
            var shortObject = Member(Tile(82, 0, 79), .1f);
            var oldForwardBoundary = Member(Tile(80, 0, 75));
            var oldRectangleCorner = Member(Tile(77, 0, 75));
            var beyondForward = Member(Tile(80, 0, 74));
            var beyondLateral = Member(Tile(84, 0, 78));
            var lowest = Member(Tile(80, -5, 78));
            var belowHeightRange = Member(Tile(80, -6, 78));
            var highestBoundary = Member(Tile(80, 5, 78));
            var aboveHeightRange = Member(Tile(81, 6, 78));
            var projectionTouching = Member(Tile(78, 0, 79), .45f);
            var sameRowContained = Member(Tile(81, 0, 80), .1f);
            var leftThird = Member(Tile(77, 0, 80));
            var rightThird = Member(Tile(83, 0, 80));
            var leftThirdBelow = Member(Tile(77, 0, 79));
            var rightThirdBelow = Member(Tile(83, 0, 79));
            var leftSecondBoundary = Member(projectionTouching.belongTile);
            var rightSecondBoundary = Member(shortObject.belongTile);
            var leftSecondBelow = Member(Tile(78, 0, 78));
            var rightSecondBelow = Member(Tile(82, 0, 78));
            var leftFirstBoundary = Member(Tile(79, 0, 78));
            var rightFirstBoundary = Member(Tile(81, 0, 78));
            var leftFirstBeyond = Member(Tile(79, 0, 77));
            var rightFirstBeyond = Member(Tile(81, 0, 77));
            var opaque = new[] { sameOwnerShort, behind, shortObject, beyondForward, beyondLateral,
                below, lowest, belowHeightRange, aboveHeightRange, projectionTouching, sameRowContained };
            var near = new[] { left, elevated, highest, boundary, elevatedBoundary, highestBoundary,
                outside, elevatedOutside, outsideDiagonal, oldForwardBoundary, oldRectangleCorner,
                leftThird, rightThird, leftThirdBelow, rightThirdBelow, leftSecondBoundary, rightSecondBoundary,
                leftSecondBelow, rightSecondBelow, leftFirstBoundary, rightFirstBoundary, leftFirstBeyond, rightFirstBeyond };
            ctrl.curObjectLst.Remove(left.data);
            Require(!map.utilCtrl.ContainsTile(81, 1, 79), "elevated Object fixture has an empty intermediate layer");
            update();
            foreach (var unit in near)
                Require(Degree(unit) == .5f && ReadVisionDegree(((MapUnit)unit).ins, 0) == .5f,
                    "unified Object scan includes X +/-3, Z 0..-5 and Y 0..+5: " + unit.data.uid);
            foreach (var unit in opaque)
                Require(Degree(unit) == 1f,
                    "out-of-range and projections not past the enumerated Tile front stay opaque: " + unit.data.uid);
            Require(Degree(leftTile) == 1f && Degree(elevated.belongTile) == .5f,
                "one lookup processes Object projection and high Tile occlusion independently");

            var item = new ItemUnitForm.Data(21590, "", prefab.name, leftTile.data.pos, Vector3.zero,
                Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
            ctrl.itemTileDic.Add(item, leftTile);
            ctrl.curItemLst.Add(item.data);
            Probe(item);
            var character = Character();
            ctrl.characterTileDic.Add(character, leftTile);
            ctrl.curCharacterLst.Add(character.data);
            Probe(character);
            update();
            Require(Degree(item) == 1f && Degree(character) == 1f,
                "nearby Object occlusion does not change Item/Character owner inheritance");
            int forwardKeys = ctrl.objectTileDic.GetDicT1().Count;
            int reverseKeys = ctrl.objectTileDic.GetDicT2().Count;
            for (int i = 0; i < 10; i++) update();
            long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++) update();
            Require(GC.GetAllocatedBytesForCurrentThread() == bytesBefore, "warmed nearby Object occlusion is allocation-free");
            Require(ctrl.objectTileDic.GetDicT1().Count == forwardKeys && ctrl.objectTileDic.GetDicT2().Count == reverseKeys,
                "nearby Object queries never create attachment-index keys");
            DynamicGlobalSettings.playing = false;
            update();
            Require(Degree(left) == .5f && Degree(below) == 1f,
                "Mod retains independent nearby Object occlusion");
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
            update();
            Require(ReadVisionDegree(((MapUnit)left).ins, 0) == 1f,
                "Overhead restores independent current-layer Object overrides");
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            update();
            ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(new Vector3Int(90, 0, 90));
            update();
            foreach (var unit in units)
                Require(ReadVisionDegree(((MapUnit)unit).ins, 0) == 1f,
                    "leaving the unified foreground range restores listed and out-of-list Objects");
        }
        finally
        {
            ctrl.End();
            foreach (var tile in tiles) map.data.UnRegisterMap(tile);
            map.data.mainData.viewSize = oldViewSize;
            DynamicGlobalSettings.cameraMode = oldMode;
            DynamicGlobalSettings.playing = oldPlaying;
        }
    }

    private static void ObjectProjectionOcclusion()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldCellSize = map.data.mainData.mapUnitSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        bool oldPlaying = DynamicGlobalSettings.playing;
        string thinPrefab = map.utilCtrl.GetTileData(6, 1, 6).prefabName;
        var tiles = new List<TileUnitForm.Data>();
        var units = new List<ObjectUnit>();
        var prefab = Go("object-projection-occlusion");
        prefab.AddComponent<ObjectInstance>();
        var model = Go("object-projection-model");
        model.transform.SetParent(prefab.transform, false);
        model.transform.localPosition = Vector3.up * .5f;
        model.AddComponent<BoxCollider>().size = new Vector3(1, 1, .1f);
        pool.AddPool(prefab);
        TileUnit Tile(int x, int y, int z)
        {
            var data = new TileUnitForm.Data(22400 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), thinPrefab, map.utilCtrl.MapPos2RealPos(new Vector3Int(x, y, z)),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            tiles.Add(data);
            map.data.RegisterMap(data);
            ctrl.curTileLst.Add(data);
            return data.unit;
        }
        ObjectUnit Member(TileUnit owner, Vector3 scale, Vector3 offset = default, Vector3 euler = default,
            string prefabName = null)
        {
            var unit = new ObjectUnitForm.Data(22500 + units.Count, false, "", prefabName ?? prefab.name,
                owner.data.pos + offset, euler, scale, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
            units.Add(unit);
            ctrl.objectTileDic.Add(unit, owner);
            ctrl.curObjectLst.Add(unit.data);
            Probe(unit);
            return unit;
        }
        void Center(Vector3Int pos)
        {
            typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ctrl, pos);
            ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(pos);
        }
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic));
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(MapUnit unit) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[unit];
        try
        {
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            map.data.mainData.viewSize = new Vector3Int(8, 5, 8);
            Center(new Vector3Int(80, 0, 80));
            var owner = Tile(80, 0, 80);
            var forward = Tile(80, 0, 79);
            var contained = Member(owner, new Vector3(.1f, .8f, .1f), Vector3.back * .5f);
            var touching = Member(owner, new Vector3(.1f, .8f, .2f), Vector3.back * .4f);
            var beyond = Member(owner, new Vector3(.1f, .801f, .2f), Vector3.back * .4f);
            var shifted = Member(owner, new Vector3(.1f, .8f, .2f), Vector3.back * .4f);
            var rotatedInside = Member(owner, new Vector3(.1f, 1.2f, .2f), Vector3.back * .3f, Vector3.left * 30);
            var rotatedOutside = Member(owner, new Vector3(.1f, 1.2f, .2f), euler: Vector3.right * 30);
            var wideForeground = Member(forward, new Vector3(.1f, .8f, 2));
            var negativeOverflow = Member(owner, new Vector3(.1f, .6f, .1f), Vector3.back * .46f);
            var awayFromPlayer = Member(owner, Vector3.one * .1f, Vector3.back * .6f);
            ctrl.curObjectLst.Remove(beyond.data);
            update();
            foreach (var unit in new[] { touching, shifted, rotatedInside, awayFromPlayer, contained, negativeOverflow })
                Require(Degree(unit) == 1f && ReadVisionDegree(((MapUnit)unit).ins, 0) == 1f,
                    "projection not strictly past the enumerated Tile front stays opaque: " + unit.data.uid);
            foreach (var unit in new[] { beyond, rotatedOutside, wideForeground })
                Require(Degree(unit) == .5f && ReadVisionDegree(((MapUnit)unit).ins, 0) == .5f,
                    "projection beyond the Tile front includes rotated/wide/out-of-list Objects: " + unit.data.uid);

            ctrl.curCenterPos += Vector3.forward * .15f;
            update();
            Require(Degree(contained) == 1f && Degree(beyond) == .5f,
                "fractional player movement does not replace the enumerated Tile boundary");
            ctrl.curCenterPos -= Vector3.forward * .15f;

            shifted.data.pos += Vector3.forward * .01f;
            update();
            Require(Degree(shifted) == .5f, "a Z-offset change that passes the Tile front enables half opacity immediately");
            shifted.data.pos -= Vector3.forward * .01f;
            beyond.data.scale = new Vector3(.1f, .3f, .2f);
            update();
            Require(Degree(shifted) == 1f && ReadVisionDegree(((MapUnit)beyond).ins, 0) == 1f,
                "position/scale changes restore opacity and clear fade flags, even outside curObjectLst");

            ctrl.objectTileDic.Del(contained);
            ctrl.objectTileDic.Add(contained, forward);
            update();
            Require(Degree(contained) == .5f, "rebinding to a lower Tile boundary changes projection eligibility");
            ctrl.objectTileDic.Del(contained);
            ctrl.objectTileDic.Add(contained, owner);
            update();
            Require(Degree(contained) == 1f, "restoring the higher Tile boundary restores opacity");

            // X is enumerated first: the first linked Tile fails, a later lower boundary passes.
            var later = Tile(81, 0, 79);
            ctrl.objectTileDic.Add(contained, later);
            update();
            Require(Degree(contained) == .5f, "a failed first boundary does not suppress a later linked Tile");
            ctrl.objectTileDic.Del(contained);
            ctrl.objectTileDic.Add(contained, owner);

            var multipart = Go("object-projection-multipart");
            multipart.AddComponent<ObjectInstance>();
            var partA = Go("object-projection-part-a");
            partA.transform.SetParent(multipart.transform, false);
            partA.transform.localPosition = new Vector3(0, .5f, -.5f);
            partA.transform.localScale = new Vector3(.1f, .8f, .1f);
            partA.AddComponent<BoxCollider>();
            var partB = Go("object-projection-part-b");
            partB.transform.SetParent(multipart.transform, false);
            partB.transform.localPosition = new Vector3(0, .5f, .25f);
            partB.transform.localScale = Vector3.one * .1f;
            pool.AddPool(multipart);
            // Keep this elevated-body fixture visually linked to the tested Tile,
            // but owned away from the player, to isolate the projection rule from
            // the new center-owned roof full-hide override.
            var projectionOnlyOwner = Tile(84, 0, 80);
            var composed = Member(projectionOnlyOwner, Vector3.one, new Vector3(-4, 0, .05f), prefabName: multipart.name);
            ctrl.objectTileDic.Add(composed, owner);
            update();
            Require(Degree(composed) == 1f, "multipart projection does not combine unrelated maximum Y/Z corners");
            partB.transform.localPosition += Vector3.forward * .11f;
            update();
            Require(Degree(composed) == .5f, "authored model offset changes are included in projection");

            var hiddenOwner = Tile(80, 1, 80);
            var hiddenContained = Member(hiddenOwner, new Vector3(.1f, .8f, .1f));
            var halfOwner = Tile(80, 2, 79);
            var halfContained = Member(halfOwner, new Vector3(.1f, .8f, .1f));
            var halfAway = Member(halfOwner, new Vector3(.1f, .8f, .1f), Vector3.back * 4f);
            ctrl.objectTileDic.Add(contained, hiddenOwner);
            update();
            Require(Degree(hiddenOwner) == 0f && Degree(hiddenContained) == .5f,
                "nearby projection overrides full Tile hiding without requiring owner overflow");
            Require(Degree(halfOwner) == .5f && Degree(halfContained) == .5f && Degree(contained) == 1f
                && Degree(halfAway) == 1f,
                "high Objects retain player-plane projection; mixed current-layer Objects still use the Tile boundary");
            contained.data.pos += Vector3.up;
            update();
            Require(Degree(contained) == .5f,
                "Object bottom touching the current layer's upper boundary retains ordinary half-opacity");
            contained.data.pos += Vector3.up * .001f;
            update();
            Require(Degree(contained) == 0f,
                "raising the player-owned Object strictly above the layer's upper boundary fully hides it");
            contained.data.pos -= Vector3.up * 1.001f;
            update();
            Require(Degree(contained) == 1f,
                "returning to the current layer restores the strict Tile-front test regardless of higher overlaps");
            DynamicGlobalSettings.playing = false;
            update();
            Require(Degree(hiddenContained) == .5f && Degree(halfContained) == .5f,
                "Mod's explicit higher-layer half-opacity preview is retained");
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
            update();
            Require(Degree(shifted) == 1f && Degree(rotatedOutside) == 1f,
                "Overhead does not enable independent side-view projection occlusion");

            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            map.data.mainData.mapUnitSize = new Vector3(1, 3, 2);
            Center(new Vector3Int(100, 2, 100));
            var largeOwner = Tile(100, 2, 100);
            var largeContained = Member(largeOwner, new Vector3(.1f, .6f, .2f));
            var largeTouching = Member(largeOwner, new Vector3(.1f, .9f, .2f));
            var largeBeyond = Member(largeOwner, new Vector3(.1f, .901f, .2f));
            update();
            Require(Degree(largeContained) == 1f && Degree(largeTouching) == 1f && Degree(largeBeyond) == .5f,
                "Tile-front projection uses actual world geometry at nonzero layers and non-unit Y/Z cell sizes");
            for (int i = 0; i < 10; i++) update();
            long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++) update();
            Require(GC.GetAllocatedBytesForCurrentThread() == bytesBefore, "warmed Tile-front projection occlusion is allocation-free");
        }
        finally
        {
            ctrl.End();
            foreach (var tile in tiles) map.data.UnRegisterMap(tile);
            map.data.mainData.viewSize = oldViewSize;
            map.data.mainData.mapUnitSize = oldCellSize;
            DynamicGlobalSettings.cameraMode = oldMode;
            DynamicGlobalSettings.playing = oldPlaying;
        }
    }

    private static void ObjectColliderOcclusion()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        bool oldPlaying = DynamicGlobalSettings.playing;
        string thinPrefab = map.utilCtrl.GetTileData(6, 1, 6).prefabName;
        var tiles = new List<TileUnitForm.Data>();
        var units = new List<ObjectUnit>();
        GameObject Prefab(string name, Vector3? bodySize, Vector3 bodyScale = default, bool trigger = false, bool sphere = false)
        {
            var prefab = Go("object-collider-" + name);
            prefab.AddComponent<ObjectInstance>();
            var model = Go(name + "-model");
            model.transform.SetParent(prefab.transform, false);
            model.transform.localPosition = Vector3.up * .5f;
            if (bodySize.HasValue)
            {
                var bodyGo = Go(name + "-body");
                bodyGo.transform.SetParent(model.transform, false);
                bodyGo.transform.localScale = bodyScale == default ? Vector3.one : bodyScale;
                if (sphere)
                    bodyGo.AddComponent<SphereCollider>().radius = .6f;
                else
                    bodyGo.AddComponent<BoxCollider>().size = bodySize.Value;
                bodyGo.GetComponent<Collider>().isTrigger = trigger;
            }
            pool.AddPool(prefab);
            return prefab;
        }
        void ExtraBox(GameObject prefab, Vector3 size, Vector3 pos, bool trigger = false, bool active = true)
        {
            var go = Go("extra-collider");
            go.transform.SetParent(prefab.transform.GetChild(0), false);
            go.transform.localPosition = pos;
            var box = go.AddComponent<BoxCollider>();
            box.size = size;
            box.isTrigger = trigger;
            go.SetActive(active);
        }
        TileUnit Tile(int x, int y, int z)
        {
            var data = new TileUnitForm.Data(23400 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, y, z), thinPrefab, map.utilCtrl.MapPos2RealPos(new Vector3Int(x, y, z)),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            tiles.Add(data);
            map.data.RegisterMap(data);
            ctrl.curTileLst.Add(data);
            return data.unit;
        }
        ObjectUnit Member(GameObject prefab, TileUnit owner, Vector3 scale = default,
            Vector3 offset = default, Vector3 euler = default)
        {
            var unit = new ObjectUnitForm.Data(23500 + units.Count, false, "", prefab.name,
                owner.data.pos + offset, euler, scale == default ? Vector3.one : scale,
                UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
            units.Add(unit);
            ctrl.objectTileDic.Add(unit, owner);
            ctrl.curObjectLst.Add(unit.data);
            Probe(unit);
            return unit;
        }
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic));
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(MapUnit unit) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[unit];
        try
        {
            map.data.mainData.viewSize = new Vector3Int(8, 5, 8);
            // Keep the half-opacity shape fixtures next to the player, not
            // owned by its Tile: elevated flat bodies now fully hide when owned
            // by the player-center roof component, independently of Y > Z.
            var center = new Vector3Int(81, 0, 80);
            typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(ctrl, center);
            ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(center);
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            DynamicGlobalSettings.playing = true;
            var owner = Tile(80, 0, 80);
            Tile(81, 0, 80);
            var highOwner = Tile(81, 1, 79);
            var lowOwner = Tile(80, -1, 79);
            var equalPrefab = Prefab("equal", Vector3.one);
            var flatPrefab = Prefab("flat", new Vector3(1, .6f, 1));
            var tallPrefab = Prefab("tall", new Vector3(1, 1.2f, 1));
            var tallerPrefab = Prefab("taller", new Vector3(1, 2, 1));
            var equal = Member(equalPrefab, owner);
            var flat = Member(flatPrefab, owner);
            var tall = Member(tallPrefab, owner);
            var justGreater = Member(Prefab("just-greater", new Vector3(1, 1.001f, 1)), owner);
            var scaleNotEnough = Member(Prefab("scale-not-enough", new Vector3(1, .5f, 2),
                new Vector3(.8f, 1, .8f)), owner);
            var scaled = Member(Prefab("horizontal-shrink", Vector3.one, new Vector3(.8f, 1, .8f)), owner);
            // Saved 草4: unit model (1,.4,1), colliderScale=0 keeps height but has no X/Z footprint.
            var grass4 = Member(Prefab("grass4-zero-horizontal", Vector3.one, new Vector3(0, 1, 0)),
                owner, new Vector3(1, .4f, 1));
            var zeroWidth = Member(Prefab("zero-width", new Vector3(0, 2, 1)), owner,
                euler: Vector3.up * 90);
            var zeroDepth = Member(Prefab("zero-depth", new Vector3(1, 2, 0)), owner,
                euler: Vector3.up * 15);
            var fitted = Member(scaled.prefab, owner);
            fitted.SetFirstPartCollisionBounds(new Rect(0, 0, 1, .1f), .8f);
            var tinyTall = Member(tallerPrefab, owner, new Vector3(.1f, .2f, .1f), Vector3.forward * .4f);
            var rotatedTall = Member(tallerPrefab, owner, euler: Vector3.right * 30);
            var rotatedEqual = Member(tallerPrefab, owner, euler: Vector3.right * 45);
            var negativeScale = Member(tallPrefab, owner, new Vector3(1, -1, 1), Vector3.forward * .2f);
            var missing = Member(Prefab("missing", null), owner);
            var triggerOnly = Member(Prefab("trigger-only", new Vector3(1, 12, 1), trigger: true), owner);
            var triggerPrefab = Prefab("flat-with-trigger", new Vector3(1, .2f, 1));
            ExtraBox(triggerPrefab, new Vector3(1, 20, 1), Vector3.zero, trigger: true);
            var triggerOverFlat = Member(triggerPrefab, owner);
            var inactivePrefab = Prefab("inactive-child", new Vector3(1, .2f, 1));
            ExtraBox(inactivePrefab, new Vector3(1, 20, 1), Vector3.zero, active: false);
            var inactiveTall = Member(inactivePrefab, owner);
            var disjointPrefab = Prefab("disjoint-flat", new Vector3(1, .2f, 1));
            ExtraBox(disjointPrefab, new Vector3(1, .2f, 1), Vector3.up * 4);
            var disjoint = Member(disjointPrefab, owner);
            var mixedPrefab = Prefab("mixed-bodies", Vector3.one);
            ExtraBox(mixedPrefab, new Vector3(1, 2, 1), Vector3.zero);
            var mixed = Member(mixedPrefab, owner);
            var spherePrefab = Prefab("sphere", Vector3.one, new Vector3(1, 2, 1), sphere: true);
            var sphere = Member(spherePrefab, owner, euler: new Vector3(35, 25, 10));
            var highFlat = Member(flatPrefab, highOwner, new Vector3(.1f, .1f, 1));
            var highMissing = Member(missing.prefab, highOwner);
            var highSphere = Member(spherePrefab, highOwner);
            var highGrass4 = Member(grass4.prefab, highOwner, new Vector3(1, .4f, 1));
            var lowTall = Member(tallerPrefab, lowOwner, new Vector3(1, 12, .1f));
            ctrl.objectTileDic.Add(lowTall, owner);
            ctrl.curObjectLst.Remove(tall.data);
            update();
            foreach (var unit in new[] { equal, flat, scaleNotEnough, missing, triggerOnly,
                triggerOverFlat, inactiveTall, disjoint, sphere, rotatedEqual, lowTall, grass4, zeroWidth, zeroDepth })
                Require(Degree(unit) == 1f && ReadVisionDegree(((MapUnit)unit).ins, 0) == 1f,
                    "current-layer final Collider Y <= Z, absent/zero-footprint body, or lower root stays opaque: " + unit.data.uid);
            foreach (var unit in new[] { tall, justGreater, scaled, fitted, tinyTall, rotatedTall,
                negativeScale, mixed, highFlat, highMissing, highSphere, highGrass4 })
                Require(Degree(unit) == .5f && ReadVisionDegree(((MapUnit)unit).ins, 0) == .5f,
                    "high layer or current-layer physical Y > Z qualifies for half opacity: " + unit.data.uid);
            Require(Degree(highOwner) == .5f, "Object shape filtering does not change Tile half opacity");

            scaled.prefab.GetComponentInChildren<BoxCollider>(true).transform.localScale = new Vector3(0, 1, 0);
            scaled.InvalidateCollisionGeometry();
            fitted.InvalidateCollisionGeometry();
            update();
            Require(Degree(scaled) == 1f && Degree(fitted) == 1f
                && ReadVisionDegree(((MapUnit)scaled).ins, 0) == 1f,
                "collapsing a previous occluder's footprint clears ordinary/fitted half-opacity next frame");
            scaled.prefab.GetComponentInChildren<BoxCollider>(true).transform.localScale = new Vector3(.8f, 1, .8f);
            scaled.InvalidateCollisionGeometry();
            fitted.InvalidateCollisionGeometry();
            update();
            Require(Degree(scaled) == .5f && Degree(fitted) == .5f,
                "restoring nonzero physical dimensions restores the strict Y > Z shape rule");

            tall.data.scale = new Vector3(1, 1, 1.3f);
            update();
            Require(ReadVisionDegree(((MapUnit)tall).ins, 0) == 1f,
                "root resizing clears an out-of-list Object's prior fade flags next frame");
            tall.data.scale = Vector3.one;
            tallPrefab.GetComponentInChildren<BoxCollider>(true).transform.localScale = new Vector3(1, .5f, 1);
            tall.InvalidateCollisionGeometry();
            update();
            Require(ReadVisionDegree(((MapUnit)tall).ins, 0) == 1f,
                "nested Collider scaling is included rather than root/visual scale alone");
            tallPrefab.GetComponentInChildren<BoxCollider>(true).transform.localScale = Vector3.one;
            tall.InvalidateCollisionGeometry();
            update();
            Require(Degree(tall) == .5f, "refreshed Collider geometry and per-frame eligibility restore half opacity");
            equal.data.pos += Vector3.up * map.data.mainData.mapUnitSize.y;
            update();
            Require(Degree(equal) == .5f, "moving the Object root to a higher layer bypasses the shape gate");
            equal.data.pos -= Vector3.up * map.data.mainData.mapUnitSize.y;
            update();
            Require(Degree(equal) == 1f, "returning to the current layer restores strict Collider Y > Z");

            DynamicGlobalSettings.playing = false;
            update();
            Require(Degree(highFlat) == .5f && Degree(highMissing) == .5f && Degree(highOwner) == .5f
                && Degree(equal) == 1f && Degree(lowTall) == 1f && Degree(grass4) == 1f && Degree(highGrass4) == .5f,
                "Mod preview keeps all high Objects eligible but cannot bypass current/lower shape filtering");
            DynamicGlobalSettings.playing = true;
            DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
            var headOwner = Tile(81, 1, 80);
            ctrl.objectTileDic.Add(equal, headOwner);
            ctrl.objectTileDic.Add(rotatedTall, headOwner);
            ctrl.objectTileDic.Add(grass4, headOwner);
            update();
            Require(Degree(headOwner) == 0f && Degree(equal) == 1f && Degree(rotatedTall) == .5f && Degree(grass4) == 1f,
                "Overhead mixed-layer half-opacity inheritance uses the same shape gate");
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            var fullyHidden = Member(flatPrefab, headOwner, offset: Vector3.back * 4);
            update();
            Require(Degree(headOwner) == 0f && Degree(fullyHidden) == 0f,
                "full Tile hiding outside independent projection eligibility is unchanged");
            ctrl.SetGroupVision(owner, .25f);
            Require(ReadVisionDegree(((MapUnit)equal).ins, 0) == .25f,
                "explicit non-half group visibility bypasses automatic shape filtering");
            ctrl.SetGroupVision(owner, .5f);
            Require(ReadVisionDegree(((MapUnit)equal).ins, 0) == .5f,
                "explicit group half opacity remains immediate");
            update();
            Require(Degree(equal) == 1f && ReadVisionDegree(((MapUnit)equal).ins, 0) == 1f,
                "the next automatic pass restores an ineligible Object's opaque classification");
            int forwardKeys = ctrl.objectTileDic.GetDicT1().Count;
            int reverseKeys = ctrl.objectTileDic.GetDicT2().Count;
            for (int i = 0; i < 10; i++) update();
            long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++) update();
            Require(GC.GetAllocatedBytesForCurrentThread() == bytesBefore,
                "warmed per-frame physical shape and projection checks are allocation-free");
            Require(ctrl.objectTileDic.GetDicT1().Count == forwardKeys && ctrl.objectTileDic.GetDicT2().Count == reverseKeys,
                "Collider eligibility and projection queries never mutate attachment indexes");
            ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(new Vector3Int(90, 0, 90));
            update();
            foreach (var unit in units)
                Require(ReadVisionDegree(((MapUnit)unit).ins, 0) == 1f,
                    "leaving occlusion restores listed and out-of-list Objects");
        }
        finally
        {
            ctrl.End();
            foreach (var tile in tiles) map.data.UnRegisterMap(tile);
            map.data.mainData.viewSize = oldViewSize;
            DynamicGlobalSettings.cameraMode = oldMode;
            DynamicGlobalSettings.playing = oldPlaying;
        }
    }

    private static void ElevatedOwnerObjectOcclusion()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldViewSize = map.data.mainData.viewSize;
        var oldMode = DynamicGlobalSettings.cameraMode;
        bool oldPlaying = DynamicGlobalSettings.playing;
        var tiles = new List<TileUnitForm.Data>();
        var units = new List<ObjectUnit>();
        var raisedPrefab = Go("raised-owner-roof");
        var model = Go("raised-owner-roof-model");
        model.transform.SetParent(raisedPrefab.transform, false);
        model.transform.localPosition = Vector3.up * 2;
        model.AddComponent<BoxCollider>().size = new Vector3(.8f, .4f, .8f);
        raisedPrefab.AddComponent<SphereCollider>().isTrigger = true;
        raisedPrefab.GetComponent<SphereCollider>().radius = 3;
        pool.AddPool(raisedPrefab);
        var groundPrefab = Go("ground-owner-object");
        var groundModel = Go("ground-owner-model");
        groundModel.transform.SetParent(groundPrefab.transform, false);
        groundModel.transform.localPosition = Vector3.up * .5f;
        groundModel.AddComponent<BoxCollider>();
        pool.AddPool(groundPrefab);
        var withinLayerPrefab = Go("raised-within-owner-layer");
        var withinLayerModel = Go("raised-within-owner-layer-model");
        withinLayerModel.transform.SetParent(withinLayerPrefab.transform, false);
        withinLayerModel.transform.localPosition = Vector3.up * .5f;
        withinLayerModel.AddComponent<BoxCollider>().size = new Vector3(.8f, .4f, .8f);
        pool.AddPool(withinLayerPrefab);
        var mixedPrefab = Object.Instantiate(raisedPrefab);
        mixedPrefab.name = "raised-and-ground-owner-object";
        objects.Add(mixedPrefab);
        var mixedLow = Go("low-body");
        mixedLow.transform.SetParent(mixedPrefab.transform, false);
        mixedLow.transform.localPosition = Vector3.up * .75f;
        mixedLow.AddComponent<BoxCollider>().size = Vector3.one * .5f;
        pool.AddPool(mixedPrefab);
        var spherePrefab = Go("raised-owner-sphere");
        spherePrefab.AddComponent<SphereCollider>().center = Vector3.up * 2;
        pool.AddPool(spherePrefab);
        var triggerPrefab = Go("raised-owner-trigger-only");
        var onlyTrigger = triggerPrefab.AddComponent<BoxCollider>();
        onlyTrigger.isTrigger = true;
        onlyTrigger.center = Vector3.up * 2;
        pool.AddPool(triggerPrefab);
        var emptyPrefab = Go("raised-owner-missing-geometry");
        pool.AddPool(emptyPrefab);
        var update = (Action)Delegate.CreateDelegate(typeof(Action), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic));
        var finalField = typeof(MapUpdateController).GetField("previousVision", BindingFlags.Instance | BindingFlags.NonPublic);
        float Degree(MapUnit unit) => ((Dictionary<MapUnit, float>)finalField.GetValue(ctrl))[unit];
        TileUnit Tile(int x, int layer, int z)
        {
            var row = new TileUnitForm.Data(24000 + tiles.Count, "", new Dictionary<int, int>(),
                new Vector3Int(x, layer, z), source.name, map.utilCtrl.MapPos2RealPos(new Vector3(x, layer, z)),
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
            tiles.Add(row);
            map.data.RegisterMap(row);
            ctrl.curTileLst.Add(row);
            return row.unit;
        }
        ObjectUnit Member(TileUnit owner, GameObject prefab)
        {
            var unit = new ObjectUnitForm.Data(24500 + units.Count, false, "", prefab.name, owner.data.pos,
                Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
            units.Add(unit);
            ctrl.objectTileDic.Add(unit, owner);
            ctrl.curObjectLst.Add(unit.data);
            Probe(unit);
            return unit;
        }
        try
        {
            map.data.mainData.viewSize = new Vector3Int(8, 4, 8);
            DynamicGlobalSettings.playing = true;
            foreach (int layer in new[] { 0, 4 })
            {
                var center = new Vector3Int(180, layer, 180);
                ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(center);
                typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic)
                    .SetValue(ctrl, center);
                var owners = new TileUnit[9];
                var roofs = new ObjectUnit[9];
                for (int i = 0; i < owners.Length; i++)
                {
                    owners[i] = Tile(180 + i, layer, 180);
                    roofs[i] = Member(owners[i], i == 5 ? groundPrefab : raisedPrefab);
                }
                var seed = roofs[0];
                var grounded = Member(owners[0], groundPrefab);
                var withinLayer = Member(owners[0], withinLayerPrefab);
                var mixed = Member(owners[0], mixedPrefab);
                var sphere = Member(Tile(180, layer, 181), spherePrefab);
                sphere.data.euler = new Vector3(35, 20, 15);
                var triggerOnly = Member(Tile(180, layer, 179), triggerPrefab);
                var missing = Member(owners[0], emptyPrefab);
                var diagonal = Member(Tile(179, layer, 182), raisedPrefab);
                var falseBridge = Tile(179, layer, 181);
                ctrl.objectTileDic.Add(diagonal, falseBridge);
                // The far Object's overlap cannot bridge the low/empty owner Tile.
                ctrl.objectTileDic.Add(roofs[6], owners[5]);
                ctrl.curObjectLst.Remove(roofs[4].data);
                var tileInstance = ProbeTile(owners[0]);
                var character = new CharacterUnitForm.Data(24801, false, Vector3.zero, 1, 1, 1, false, "",
                    source.name, owners[0].data.pos, Vector3.zero, Vector3.one,
                    UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>()).unit;
                var item = new ItemUnitForm.Data(24802, "", source.name, owners[0].data.pos,
                    Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
                ctrl.characterTileDic.Add(character, owners[0]);
                ctrl.itemTileDic.Add(item, owners[0]);
                ctrl.curCharacterLst.Add(character.data);
                ctrl.curItemLst.Add(item.data);
                Probe(character);
                Probe(item);
                foreach (var mode in new[] { CameraMode.Overhead, CameraMode.Isometric })
                {
                    DynamicGlobalSettings.cameraMode = mode;
                    update();
                    for (int i = 0; i <= 4; i++)
                        Require(Degree(roofs[i]) == 0f && ReadVisionDegree(((MapUnit)roofs[i]).ins, 0) == 0f,
                            "elevated same-layer owner component fully hides beyond half radius, including out-of-list Objects: " + mode);
                    Require(Degree(sphere) == 0f && Degree(triggerOnly) == 0f,
                        "four-neighbour roof connectivity supports rotated physical Spheres and Trigger-only prefabs");
                    Require(Degree(grounded) == 1f && Degree(withinLayer) == 1f && Degree(mixed) == 1f && Degree(missing) == 1f,
                        "grounded/within-layer/multipart/missing physical bounds are not above the layer's upper boundary");
                    Require(Degree(owners[0]) == 1f && ReadVisionDegree(tileInstance, 0) == 1f
                        && Degree(character) == 1f && Degree(item) == 1f,
                        "roof hiding leaves current-layer Tile, front, Character and Item visibility unchanged");
                    Require(Degree(roofs[6]) == 1f && Degree(diagonal) == 1f,
                        "ground owner, diagonal adjacency and foreign visual overlap cannot connect roof components");

                    Vector3 spherePos = sphere.data.pos;
                    sphere.data.pos += Vector3.up * (1.5f - (Quaternion.Euler(sphere.data.euler) * Vector3.up * 2).y);
                    update();
                    Require(Degree(sphere) == 1f,
                        "rotated Sphere touching the layer upper boundary uses its physical radius, not elevated sample-point minima");
                    sphere.data.pos = spherePos;
                    Vector3 seedPos = seed.data.pos;
                    seed.data.pos -= Vector3.up * (1.8f - .5f);
                    update();
                    Require(Degree(seed) == 1f && Degree(roofs[1]) == 1f,
                        "Collider bottom between Tile ground and upper boundary does not seed full hiding");
                    seed.data.pos = seedPos - Vector3.up * (1.8f - 1f);
                    update();
                    Require(Degree(seed) == 1f && Degree(roofs[1]) == 1f,
                        "Collider bottom exactly at the layer upper boundary does not seed full hiding");
                    seed.data.pos = seedPos - Vector3.up * (1.8f - 1.00005f);
                    update();
                    Require(Degree(seed) == 1f && Degree(roofs[1]) == 1f,
                        "Collider bottom within upper-boundary tolerance does not seed full hiding");
                    seed.data.pos = seedPos - Vector3.up * (1.8f - 1.0002f);
                    update();
                    Require(Degree(seed) == 0f && Degree(roofs[1]) == 0f,
                        "Collider bottom strictly above Tile height plus one seeds full hiding even with a lower root");
                    seed.data.pos = seedPos;
                    ctrl.objectTileDic.Del(seed);
                    ctrl.objectTileDic.Add(seed, owners[1]);
                    ctrl.objectTileDic.Add(seed, owners[0]);
                    update();
                    Require(Degree(seed) == 1f && Degree(roofs[1]) == 1f,
                        "rebinding the seed owner away from the player breaks connectivity despite visual coverage");
                    ctrl.objectTileDic.Del(seed);
                    ctrl.objectTileDic.Add(seed, owners[0]);

                    roofs[5].data.prefabName = raisedPrefab.name;
                    update();
                    Require(Degree(roofs[6]) == 0f && Degree(roofs[7]) == 0f && Degree(roofs[8]) == 1f,
                        "new elevated bridge connects the component only within the configured view");
                    roofs[5].data.prefabName = withinLayerPrefab.name;
                    update();
                    Require(Degree(roofs[6]) == 1f && Degree(roofs[7]) == 1f,
                        "lowering a bridge into its owner layer restores detached roofs next frame");

                    seed.data.prefabName = groundPrefab.name;
                    update();
                    Require(Degree(roofs[1]) == 1f && Degree(sphere) == 1f && Degree(triggerOnly) == 1f
                        && ReadVisionDegree(((MapUnit)roofs[4]).ins, 0) == 1f,
                        "no elevated owner at the player's exact Tile prevents full hiding and restores omitted instances");
                    seed.data.prefabName = raisedPrefab.name;
                    update();
                    Require(Degree(seed) == 0f, "restoring raised geometry restores full hiding without root/owner changes");

                    map.data.UnRegisterMap(owners[1].data);
                    update();
                    Require(Degree(roofs[2]) == 1f && ReadVisionDegree(((MapUnit)roofs[4]).ins, 0) == 1f,
                        "a missing Tile breaks connectivity rather than skipping to a distant roof");
                    map.data.RegisterMap(owners[1].data);
                    update();
                    Require(Degree(roofs[4]) == 0f, "restoring a missing owner Tile reconnects the component");

                    seed.data.prefabName = groundPrefab.name;
                    grounded.data.scale = new Vector3(1, 2, 1);
                    update();
                    Require(Degree(grounded) != 0f && Degree(roofs[1]) == 1f,
                        "tall Collider still crossing its owner layer cannot seed full hiding");
                    grounded.SetFirstPartCollisionBounds(new Rect(0, .6f, 1, .4f), 1f);
                    update();
                    Require(Degree(grounded) == 0f && Degree(roofs[1]) == 0f,
                        "texture-fitted Collider bottom above Tile plus one seeds roof hiding without moving its root");
                    grounded.SetFirstPartCollisionBounds(null, 1f);
                    grounded.data.scale = Vector3.one;
                    seed.data.prefabName = raisedPrefab.name;

                    DynamicGlobalSettings.playing = false;
                    update();
                    Require(Degree(seed) != 0f && ReadVisionDegree(((MapUnit)roofs[4]).ins, 0) != 0f,
                        "Mod high-layer preview does not inherit Play roof full hiding");
                    DynamicGlobalSettings.playing = true;
                    update();
                    int reverseKeys = ctrl.objectTileDic.GetDicT2().Count;
                    int ownerKeys = ctrl.objectTileDic.GetDicT1().Count;
                    for (int i = 0; i < 20; i++) update();
                    long before = GC.GetAllocatedBytesForCurrentThread();
                    for (int i = 0; i < 100; i++) update();
                    Require(GC.GetAllocatedBytesForCurrentThread() == before,
                        "owner-connected roof hiding is allocation-free after geometry/container warmup");
                    Require(ctrl.objectTileDic.GetDicT2().Count == reverseKeys && ctrl.objectTileDic.GetDicT1().Count == ownerKeys,
                        "roof BFS does not create missing owner/reverse-index keys");
                }
                ctrl.curCenterPos = map.utilCtrl.MapPos2RealPos(center + new Vector3Int(10, 0, 10));
                update();
                Require(ReadVisionDegree(((MapUnit)roofs[4]).ins, 0) == 1f && Degree(seed) == 1f,
                    "leaving the roof component restores full opacity for visible and omitted Objects");
                ctrl.End();
                foreach (var tile in tiles) map.data.UnRegisterMap(tile);
                tiles.Clear();
                units.Clear();
            }
        }
        finally
        {
            ctrl.End();
            foreach (var tile in tiles) map.data.UnRegisterMap(tile);
            map.data.mainData.viewSize = oldViewSize;
            DynamicGlobalSettings.cameraMode = oldMode;
            DynamicGlobalSettings.playing = oldPlaying;
        }
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
