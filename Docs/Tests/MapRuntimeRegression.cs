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
using ObjectConfig = Ui.ModStory.ModStoryMapObject.ModStoryMapObjectObject.ModStoryMapObjectObjectConfig;
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
            MovementTimeStep();
            BoundedFrameClock();
            DictionaryQueries();
            Geometry();
            CaptureCastWholeSegment();
            MapGroundCoverageCaching();
            Coverage();
            LegacyPassTypeSceneData();
            BoundsCollisionPrefabs();
            ObjectRearAnchoredCollider();
            WangTileBoundsCollision();
            TerrainHeightCaching();
            ObjectBottomAnchoredHeight();
            PassTypeMovement();
            IncrementalNavigation();
            NavigationEndpoints();
            WangTileAnimationSelection();
            ObjectWangTileNeighbourSelection();
            SharedAnimationClock();
            CanvasHolderResetAfterPoolTeardown();
            SparseViewRefreshAfterErase();
            ViewRefreshPoolReuse();
            FreshMapActiveTileHandoff();
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
            NavigationObstacleOptimization();
            NavigationRebuildScheduling();
            ObjectCollisionScalePassTypeCoverage();
            ObjectMarksAndMissionCoordinates();
            AreaMapSelfMarker();
            BoundedProgressClock();
            KeyboardMovementTimeStep();
            CommandMovementTimeStep();
            SceneCommandForms();
            InteractionParameters();
            DestroyCharacters();
            InterpreterSchedulingOptimization();
            EventOperatorRendering();
            FormListOrdering();
            ObjectEmptyTextureThumbnails();
            ObjectConfigCenterColliderWithoutMap();
            TimedNotifyComments();
            ReturnKeywordDiagnostics();
            CompilerMethodDiagnostics();
            Debug.Log("MAP_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunNavigationOptimization()
    {
        try
        {
            SetUp();
            UnitForm.InitInternal();
            TileUnitForm.InitInternal();
            ObjectUnitForm.InitInternal();
            foreach (var tile in map.data.maps.Values) TileUnitForm.AddData(tile);
            Geometry();
            map.navigationCtrl.Build();
            IncrementalNavigation();
            NavigationEndpoints();
            NavigationObstacleOptimization();
            WholeTileObjectNavigation();
            DirectionalObjectNavigation();
            NavigationRebuildScheduling();
            Debug.Log("MAP_NAV_OPTIMIZATION_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunInterpreterSchedulingOptimization()
    {
        try
        {
            SetUp();
            InterpreterSchedulingOptimization();
            Debug.Log("MAP_INTERPRETER_SCHEDULING_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunDestroyCharacter()
    {
        try
        {
            SetUp();
            ObjectMarksAndMissionCoordinates();
            DestroyCharacters();
            Debug.Log("MAP_CHARACTER_REMOVAL_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunMovementTimeStep()
    {
        try
        {
            SetUp();
            MovementTimeStep();
            BoundedFrameClock();
            ObjectMarksAndMissionCoordinates();
            BoundedProgressClock();
            KeyboardMovementTimeStep();
            CommandMovementTimeStep();
            Debug.Log("MAP_MOVEMENT_TIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunSceneCommands()
    {
        try
        {
            SetUp();
            ObjectMarksAndMissionCoordinates();
            CommandMovementTimeStep();
            SceneCommandForms();
            InteractionParameters();
            AreaMapSelfMarker();
            Debug.Log("MAP_SCENE_COMMANDS_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception e)
        {
            Debug.LogException(e);
            EditorApplication.Exit(1);
        }
    }

    public static void RunTerrainHeightCaching()
    {
        try
        {
            SetUp();
            UnitForm.InitInternal();
            TileUnitForm.InitInternal();
            ObjectUnitForm.InitInternal();
            foreach (var tile in map.data.maps.Values) TileUnitForm.AddData(tile);
            Geometry();
            TerrainHeightCaching();
            Debug.Log("MAP_TERRAIN_HEIGHT_REGRESSION_PASS checks=" + checks);
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
            SparseViewRefreshAfterErase();
            ViewRefreshPoolReuse();
            FreshMapActiveTileHandoff();
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

    public static void RunViewRefreshPlayLifecycle()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload
            | EnterPlayModeOptions.DisableSceneReload;
        EditorApplication.playModeStateChanged += OnViewRefreshPlayState;
        EditorApplication.EnterPlaymode();
    }

    private static void OnViewRefreshPlayState(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= OnViewRefreshPlayState;
        try
        {
            SetUp();
            SparseViewRefreshAfterErase();
            ViewRefreshPoolReuse();
            ViewRefreshExceptionalLifecycle();
            FreshMapActiveTileHandoff();
            IncrementalViewRefresh();
            HeightProjectedViewRefresh();
            Debug.Log("MAP_VIEW_PLAY_LIFECYCLE_PASS checks=" + checks);
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
        pool.Init();
        pool.pools = new Dictionary<string, InstancePool>(StringComparer.Ordinal);
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

    private static float LegacyTileHeight(TileUnit tile, Vector2 point)
    {
        if (tile.data.scale == Vector3.zero) return -999f;
        var collider = tile.prefab.GetComponentInChildren<BoxCollider>();
        if (collider == null) return tile.data.pos.y;
        var euler = collider.transform.eulerAngles;
        euler.y += tile.data.euler.y;
        var normal = Quaternion.Euler(euler) * Vector3.back;
        if (Mathf.Abs(normal.y) < 1e-6f) return tile.data.pos.y;
        float tan = Mathf.Tan(collider.transform.eulerAngles.x * Mathf.PI / 180f);
        if (Mathf.Abs(tan) < 1e-6f) return tile.data.pos.y;
        return tile.data.pos.y + Z_Math.Graph.CalculateYAtPointInPlane(
            new Vector3(0, 1f / tan / 2f, 0), normal, point.x, point.y);
    }

    private static void TerrainHeightCaching()
    {
        var tile = map.utilCtrl.GetTile(5, 0, 5);
        var originalName = tile.data.prefabName;
        var originalPos = tile.data.pos;
        var originalEuler = tile.data.euler;
        var originalScale = tile.data.scale;
        var slope = Go("height-cache-slope");
        var collider = slope.AddComponent<BoxCollider>();
        var slopePool = new InstancePool(slope, pool.defaultRoot);
        pool.Register(slopePool);
        var flat = Go("height-cache-without-collider");
        pool.AddPool(flat);
        var points = new[] { Vector2.zero, Vector2.right * .25f, Vector2.left * .25f,
            Vector2.up * .25f, Vector2.down * .25f, new Vector2(.13f, -.21f) };
        var nav = new Z_Map.Analysis.NavigationController(map);
        void Compare(TileUnit current)
        {
            foreach (var point in points)
                Require(Mathf.Abs(current.GetYByPoint(point) - LegacyTileHeight(current, point)) < .00001f,
                    "cached height retains original plane equation/fallback");
        }
        void CheckSamples()
        {
            var node = nav.navUnits[(5, 0, 5)];
            for (int i = 0; i < 4; i++)
                Require(Mathf.Abs(node.dirGroundY[i] - LegacyTileHeight(tile, points[i + 1])) < .00001f,
                    "Nav ground cache follows changed Tile/plane/empty state");
        }
        try
        {
            Compare(tile);
            tile.data.prefabName = slope.name;
            foreach (float pitch in new[] { 0f, 30f, 45f, 90f, 315f })
            foreach (float yaw in new[] { 0f, 90f, 137f, 180f, 270f })
            {
                collider.transform.eulerAngles = new Vector3(pitch, 11, 0);
                tile.data.euler = new Vector3(7, yaw, 13);
                tile.InvalidateCollisionGeometry();
                Compare(tile);
            }
            collider.transform.eulerAngles = new Vector3(45, 0, 0);
            tile.data.euler = Vector3.zero;
            tile.InvalidateCollisionGeometry();
            Compare(tile);
            var timer = new System.Diagnostics.Stopwatch();
            long before = GC.GetAllocatedBytesForCurrentThread();
            float total = 0;
            timer.Start();
            for (int i = 0; i < 10000; i++) total += tile.GetYByPoint(points[i % points.Length]);
            timer.Stop();
            double cachedMs = timer.Elapsed.TotalMilliseconds;
            long cachedBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Require(cachedBytes == 0 && !float.IsNaN(total), "10000 warmed height reads allocate zero bytes");
            timer.Reset();
            before = GC.GetAllocatedBytesForCurrentThread();
            timer.Start();
            for (int i = 0; i < 10000; i++) total += LegacyTileHeight(tile, points[i % points.Length]);
            timer.Stop();
            long legacyBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Debug.Log("TERRAIN_HEIGHT_CACHE samples=10000 legacyBytes=" + legacyBytes + " cachedBytes=" + cachedBytes
                + " legacyMs=" + timer.Elapsed.TotalMilliseconds + " cachedMs=" + cachedMs);

            tile.data.pos += Vector3.up * 2;
            Compare(tile);
            tile.data.scale = Vector3.zero;
            Compare(tile);
            tile.data.scale = new Vector3(2, 3, 4);
            Compare(tile);
            tile.data.prefabName = flat.name;
            Compare(tile);
            tile.data.prefabName = slope.name;
            tile.data.scale = Vector3.one;
            nav.Build();
            CheckSamples();
            var node = nav.navUnits[(5, 0, 5)];
            var ground = node.dirGroundY;
            var max = node.dirMaxY;
            var pass = node.passTypes;
            var farGround = nav.navUnits[(10, 0, 10)].dirGroundY;
            // A sentinel proves the warm build restores cached samples instead
            // of quietly recomputing the same values into the retained arrays.
            float oldHeight = ground[0];
            ground[0] = oldHeight + .03125f;
            tile.SetPassTypes(new[] { 7711, 7712 });
            nav.RebuildNow();
            Require(ground[0] == oldHeight + .03125f && max[0] == ground[0],
                "unchanged periodic build does not resample any height");
            Require(ReferenceEquals(ground, node.dirGroundY) && ReferenceEquals(max, node.dirMaxY)
                && ReferenceEquals(pass, node.passTypes) && pass.SetEquals(new[] { 7711, 7712 }),
                "periodic build reuses arrays/sets but still refreshes pass types");
            ground[0] = oldHeight;
            nav.RebuildNow();
            before = GC.GetAllocatedBytesForCurrentThread();
            nav.RebuildNow();
            long rebuildBytes = GC.GetAllocatedBytesForCurrentThread() - before;
            Require(rebuildBytes < map.data.maps.Count * 16L,
                "warm full rebuild has no per-Tile sample arrays/HashSets/prefab-name strings");
            Debug.Log("TERRAIN_NAV_CACHE tiles=" + map.data.maps.Count + " rebuildBytes=" + rebuildBytes);

            tile.data.pos += Vector3.up * .4f;
            nav.RebuildNow(); CheckSamples();
            tile.data.euler = Vector3.up * 90;
            nav.RebuildNow(); CheckSamples();
            tile.data.scale = Vector3.zero;
            nav.RebuildNow(); CheckSamples();
            tile.data.scale = Vector3.one;
            nav.RebuildNow(); CheckSamples();
            collider.transform.eulerAngles = new Vector3(30, 0, 0);
            tile.InvalidateCollisionGeometry();
            nav.RebuildNow(); CheckSamples();
            collider.transform.eulerAngles = new Vector3(60, 0, 0);
            nav.RefreshTerrainTiles(new[] { tile.data.mapPos }); CheckSamples();
            Require(ReferenceEquals(farGround, nav.navUnits[(10, 0, 10)].dirGroundY),
                "explicit terrain edits retain unrelated sample arrays");
            var replacement = Go(slope.name);
            replacement.AddComponent<BoxCollider>().transform.eulerAngles = new Vector3(35, 0, 0);
            pool.Unregister(slopePool);
            pool.AddPool(replacement);
            nav.RebuildNow(); CheckSamples();
            Require(ReferenceEquals(ground, node.dirGroundY) && ReferenceEquals(max, node.dirMaxY),
                "same-name prefab replacement updates values without replacing arrays");
            tile.data.prefabName = flat.name;
            nav.RebuildNow(); CheckSamples();
            tile.data.prefabName = slope.name;
            nav.RebuildNow(); CheckSamples();
        }
        finally
        {
            tile.SetPassTypes(null);
            tile.data.prefabName = originalName;
            tile.data.pos = originalPos;
            tile.data.euler = originalEuler;
            tile.data.scale = originalScale;
            tile.InvalidateCollisionGeometry();
        }
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

    private static void ObjectConfigCenterColliderWithoutMap()
    {
        // ModStory can retain scene rows with no active map. Use a null scene
        // renderer as a sentinel: Config must not touch it in this state.
        var game = GameManager.instance;
        var oldMapData = map.data;
        var oldMapController = game.mapCtrl;
        bool oldMinimap = game.curProgress.enableMinimap;
        int defaultTextureId = GlobalDefaultHelper.DefaultStoryTexId;
        TexAssetForm.DataById.TryGetValue(defaultTextureId, out var oldTexture);
        var root = Go("object-config-no-map");
        root.SetActive(false); // No font loading or automatic UiHolder lifecycle in EditMode.
        GameObject Child(string name)
        {
            var child = Go(name);
            child.transform.SetParent(root.transform, false);
            return child;
        }
        Z_Ui.Base.Txt Text(string name)
        {
            var text = Child(name).AddComponent<Z_Ui.Base.Txt>();
            text.languageTranslatable = false;
            return text;
        }
        object Empty(Type type) => System.Runtime.Serialization.FormatterServices.GetUninitializedObject(type);
        var view = (ObjectConfig.UiModStoryMapObjectObjectConfigView)Empty(typeof(ObjectConfig.UiModStoryMapObjectObjectConfigView));
        foreach (var field in view.GetType().GetFields(BindingFlags.Instance | BindingFlags.Public))
        {
            object value;
            if (field.FieldType == typeof(GameObject)) value = Child(field.Name);
            else if (field.FieldType == typeof(Z_Ui.Base.Txt)) value = Text(field.Name);
            else if (field.FieldType == typeof(Ui.EventChoose.UiEventChooseCtrl))
            {
                var eventView = (Ui.EventChoose.UiEventChooseView)Empty(typeof(Ui.EventChoose.UiEventChooseView));
                eventView.txt_name = Text(field.Name + "-name");
                eventView.txt_onEvent = Text(field.Name + "-event");
                eventView.txt_onEventTrigger = Text(field.Name + "-trigger");
                value = new Ui.EventChoose.UiEventChooseCtrl
                { model = new Ui.EventChoose.UiEventChooseModel(), view = eventView };
            }
            else if (typeof(Component).IsAssignableFrom(field.FieldType))
            {
                var component = Child(field.Name).AddComponent(field.FieldType);
                if (component is Z_Ui.Base.Sta state)
                    typeof(Z_Trick.BaseFunc.State).GetField("stateGo", BindingFlags.Instance | BindingFlags.NonPublic)
                        .SetValue(state, new[] { Child(field.Name + "-0"), Child(field.Name + "-1") });
                value = component;
            }
            else continue;
            field.SetValue(view, value);
        }
        var product = Form.MapObjectForm.GetDataByJo(new JObject());
        product.centerCollider = true;
        var ctrl = new ObjectConfig.UiModStoryMapObjectObjectConfigCtrl
        {
            uiHolder = root.AddComponent<Z_Ui.Base.UiHolder>(), inited = true, view = view,
            model = new ObjectConfig.UiModStoryMapObjectObjectConfigModel(),
            param = new ObjectConfig.UiModStoryMapObjectObjectConfigParam { data = product }
        };
        try
        {
            map.data = null;
            game.mapCtrl = null;
            game.curProgress.enableMinimap = false;
            if (oldTexture == null)
                TexAssetForm.DataById[defaultTextureId] = new TexAssetForm.Data(defaultTextureId,
                    "config-default", null, null, null, Texture2D.whiteTexture, 0);
            ctrl.OnCreate();
            for (int opening = 0; opening < 2; opening++)
            {
                ctrl.OnShow();
                Require(view.sta_centerCollider.state == 1, "reopening Config reflects the saved centered state");
                foreach (bool centered in new[] { false, true })
                {
                    view.btn_centerCollider.onClick.Invoke();
                    Require(product.centerCollider == centered
                        && view.sta_centerCollider.state == (centered ? 1 : 0),
                        "actual center-Collider button immediately refreshes UI without an active map");
                }
                ctrl.OnHide();
            }
        }
        finally
        {
            map.data = oldMapData;
            game.mapCtrl = oldMapController;
            game.curProgress.enableMinimap = oldMinimap;
            if (oldTexture == null) TexAssetForm.DataById.Remove(defaultTextureId);
            Object.DestroyImmediate(root);
        }
    }

    private static void MovementTimeStep()
    {
        Require(Z_Map.GlobalSettings.MAX_MOVEMENT_DELTA_TIME == .02f, "movement shares a 20 ms cap");
        foreach (float frame in new[] { -.1f, 0f, 1f / 240, 1f / 120, 1f / 60, .02f, 1f / 30, .1f, .5f, 2f })
        {
            float expected = Mathf.Clamp(frame, 0f, .02f);
            Require(MapUtilController.ClampMovementDeltaTime(frame) == expected,
                "scaled fast/paused/slow frame uses the bounded movement step: " + frame);
        }
        Require(MapUtilController.MovementDeltaTime == MapUtilController.ClampMovementDeltaTime(Time.deltaTime),
            "live movement clock reads scaled Unity delta, not fixed/unscaled time");

        float remaining = 1f;
        float position = 0f;
        for (int frame = 0; frame < 10; frame++)
        {
            float delta = MapUtilController.ClampMovementDeltaTime(.1f);
            position += Mathf.Min(1f, delta / remaining) * (1f - position);
            remaining -= delta;
        }
        Require(Mathf.Abs(position - .2f) < .00001f && Mathf.Abs(remaining - .8f) < .00001f,
            "one real second at 10 FPS advances displacement and duration by only 0.2 movement seconds");
        float recoveredDelta = MapUtilController.ClampMovementDeltaTime(1f / 60);
        position += Mathf.Min(1f, recoveredDelta / remaining) * (1f - position);
        remaining -= recoveredDelta;
        Require(Mathf.Abs(position - (.2f + recoveredDelta)) < .00001f,
            "recovery does not catch up the discarded stall time");
        int frames = 0;
        while (remaining > 0f && frames++ < 100)
        {
            float delta = MapUtilController.ClampMovementDeltaTime(.1f);
            position += Mathf.Min(1f, delta / Mathf.Max(.0001f, remaining)) * (1f - position);
            remaining -= delta;
        }
        Require(remaining <= 0f && Mathf.Abs(position - 1f) < .00001f,
            "bounded interpolation still reaches its target instead of expiring during the stall");
    }

    private static void BoundedFrameClock()
    {
        Require(TimeManager.MAX_DELTA_TIME == .02f
            && Z_Map.GlobalSettings.MAX_MOVEMENT_DELTA_TIME == TimeManager.MAX_DELTA_TIME,
            "all frame clocks share one 20 ms constant");
        foreach (float frame in new[] { -.1f, 0f, 1f / 240, 1f / 60, .02f, .1f, 2f,
            float.NegativeInfinity, float.PositiveInfinity, float.NaN })
        {
            float expected = float.IsNaN(frame) ? 0f : Mathf.Clamp(frame, 0f, .02f);
            Require(TimeManager.ClampDeltaTime(frame) == expected
                && MapUtilController.ClampMovementDeltaTime(frame) == expected,
                "shared clock and movement alias bound explicit delta: " + frame);
        }
        float delta = TimeManager.GetDeltaTime();
        float unscaledDelta = TimeManager.GetDeltaTime(unscaled: true);
        Require(delta == TimeManager.ClampDeltaTime(Time.deltaTime)
            && delta == MapUtilController.MovementDeltaTime,
            "default frame clock remains scaled and agrees with movement");
        Require(unscaledDelta == TimeManager.ClampDeltaTime(Time.unscaledDeltaTime),
            "pause-independent UI uses bounded unscaled delta");

        var scroll = (Func<float, float>)typeof(Z_Ui.Base.Ipt)
            .GetMethod("ScrollStep", BindingFlags.Static | BindingFlags.NonPublic)
            .CreateDelegate(typeof(Func<float, float>));
        foreach (float overflow in new[] { -1000f, -20f, 0f, 20f, 1000f })
            Require(Mathf.Abs(scroll(overflow) - Mathf.Sign(overflow)
                * Mathf.Min(Mathf.Abs(overflow) * 4f, 800f) * unscaledDelta) < .000001f,
                "actual input drag scroll uses the bounded unscaled clock: " + overflow);

        var wait = new Z_ObjectAnimator.Base.WaitSecondEvent(.1f);
        int maxTimes = (int)typeof(Z_ObjectAnimator.Base.Event)
            .GetField("_maxTimes", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(wait);
        float conversionDelta = delta > 0f ? delta : TimeManager.MAX_DELTA_TIME;
        Require(maxTimes == (int)(.1f / conversionDelta) + 1,
            "animation duration conversion uses bounded time, including a safe zero-delta fallback");
        foreach (int frames in new[] { -1, 0, 1, 3 })
        {
            var frameWait = new Z_ObjectAnimator.Base.WaitFramesEvent(frames);
            int calls = Mathf.Max(1, frames + 1);
            for (int call = 1; call <= calls; call++)
                Require(frameWait.Excute().type == (call == calls
                    ? Z_ObjectAnimator.Base.ReturnValue.Type.Next : Z_ObjectAnimator.Base.ReturnValue.Type.Continue),
                    "explicit frame waits retain frame counts without delta conversion: " + frames + "/" + call);
        }

        var program = new Z_Code.Form.ProgramDataForm.Data(-98910, "bounded-wait", "",
            new List<string> { ((int)Z_Code.Op.PushNum).ToString(), "1", ((int)Z_Code.Op.Wait).ToString() },
            new List<int> { 0, 0, 0 }, 0, "void");
        var data = new Z_Code.Form.InterpretDataForm.Data(-98911, new List<Z_Code.Form.BoxDataForm.Data>(),
            new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), program, 0, -1, 0, null,
            new List<Z_Code.Form.BoxDataForm.Data>(), 0);
        var interpreter = new Z_Code.Interpreter(data);
        for (int frame = 1; frame <= 3; frame++)
        {
            var ret = interpreter.Interpret();
            Require(!ret.complete && ret.errors.Count == 0 && data.p == 2 && data.top == 0
                && Mathf.Abs(data.stack[0].num - (1f - frame * delta)) < .000001f,
                "actual interpreter Wait consumes only bounded time and retains its stack: " + frame);
        }
    }

    private static void BoundedProgressClock()
    {
        // The mission fixture installed isolated Game/Play managers and progress.
        var progress = GameManager.instance.curProgress;
        float oldSeconds = progress.seconds;
        int oldBlock = progress.blockProgramUid;
        var oldTeam = progress.team;
        bool oldPauseNav = Z_Map.DynamicGlobalSettings.pauseNav;
        var ctrl = new PlayInfoController(PlayManager.instance);
        typeof(PlayInfoController).GetField("enable", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(ctrl, true);
        try
        {
            progress.seconds = 10.25f; // Avoid EverySecond while checking the actual per-frame increment.
            progress.team = new List<int>();
            progress.blockProgramUid = 0;
            float delta = TimeManager.GetDeltaTime();
            for (int frame = 1; frame <= 3; frame++)
            {
                ctrl.Update();
                Require(Mathf.Abs(progress.seconds - (10.25f + frame * delta)) < .00001f,
                    "actual Play progress consumes the same bounded clock as movement: " + frame);
            }
            float before = progress.seconds;
            progress.blockProgramUid = 42;
            ctrl.Update();
            Require(progress.seconds == before && Z_Map.DynamicGlobalSettings.pauseNav,
                "blocking still pauses progress and navigation");
            progress.blockProgramUid = 0;
            ctrl.End();
            ctrl.Update();
            Require(progress.seconds == before, "disabled Play info does not advance the shared clock");
        }
        finally
        {
            progress.seconds = oldSeconds;
            progress.blockProgramUid = oldBlock;
            progress.team = oldTeam;
            Z_Map.DynamicGlobalSettings.pauseNav = oldPauseNav;
            ctrl.Unregister<CollideEvent>();
            ctrl.Unregister<MissionEvent>();
        }
    }

    private static void KeyboardMovementTimeStep()
    {
        // Prior mission checks installed isolated Game/Play managers and progress.
        var ctrl = new PlaySceneController(PlayManager.instance);
        var player = Character();
        var instanceGo = Go("movement-keyboard-player");
        instanceGo.SetActive(false);
        var instance = instanceGo.AddComponent<CharacterInstance>();
        player.ins = instance;
        instance.unit = player;
        var product = (Form.CharacterProductForm.Data)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Form.CharacterProductForm.Data));
        product.uid = -98901;
        const string speedName = "__movement_regression_speed__";
        product.speedParamName = speedName;
        Form.CharacterParamForm.InitInternal();
        var speed = new Form.CharacterParamForm.Data(98901, speedName, Z_DataSystem.Form.ValType.Float,
            "", "", "", default);
        speed.GetValue().num = 4f;
        product.paramDic = new Dictionary<string, Form.CharacterParamForm.Data> { [speedName] = speed };
        var paramNames = Form.CharacterParamForm.DataByName;
        paramNames.TryGetValue(speedName, out var oldSpeed);
        paramNames[speedName] = speed;
        var controllerType = typeof(PlaySceneController);
        var enableField = controllerType.GetField("enable", BindingFlags.Instance | BindingFlags.NonPublic);
        var moveField = controllerType.GetField("setPlayerMove", BindingFlags.Instance | BindingFlags.NonPublic);
        controllerType.GetField("_playerM", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, player.data);
        controllerType.GetField("_playerG", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(ctrl, product);
        enableField.SetValue(ctrl, true);
        var progress = GameManager.instance.curProgress;
        int oldBlock = progress.blockProgramUid;
        void Input(Vector3 expectedDirection, params KeyCode[] keys)
        {
            moveField.SetValue(ctrl, Vector3.zero);
            ctrl.OnEvent(new Z_Input.InputKeyEvent { key = keys.ToList() });
            Vector3 expected = expectedDirection * MapUtilController.MovementDeltaTime * 4f;
            Require(Vector3.Distance((Vector3)moveField.GetValue(ctrl), expected) < .000001f,
                "actual keyboard event uses bounded time and speed: " + string.Join(",", keys));
        }
        try
        {
            progress.blockProgramUid = 0;
            Input(Vector3.forward, KeyCode.W);
            Input(Vector3.back, KeyCode.S);
            Input(Vector3.left, KeyCode.A);
            Input(Vector3.right, KeyCode.D);
            Input((Vector3.forward + Vector3.left).normalized, KeyCode.W, KeyCode.A);
            Input(Vector3.zero, KeyCode.W, KeyCode.S);
            Input(Vector3.zero, KeyCode.W, KeyCode.A, KeyCode.S, KeyCode.D);
            Input(Vector3.zero, KeyCode.E);
            progress.blockProgramUid = 42;
            Input(Vector3.zero, KeyCode.W);
            progress.blockProgramUid = 0;
            enableField.SetValue(ctrl, false);
            Input(Vector3.zero, KeyCode.W);
        }
        finally
        {
            progress.blockProgramUid = oldBlock;
            if (oldSpeed == null) paramNames.Remove(speedName);
            else paramNames[speedName] = oldSpeed;
        }
    }

    private static void AreaMapSelfMarker()
    {
        var play = PlayManager.instance;
        var oldScene = play.sceneCtrl;
        var oldMap = play.mapCtrl;
        var oldEnable = play.enable;
        var oldSize = map.data.mainData.mapUnitSize;
        var scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        var mapCtrl = new PlayMapController(play) { size = (8, 2, -3, 4) };
        play.mapCtrl = mapCtrl;
        var ready = typeof(PlayMapController).GetProperty("isReady");
        ready.SetValue(mapCtrl, true);
        var player = new CharacterUnitForm.Data(97201, false, Vector3.zero, 4, 4, 4, false, "",
            source.name, Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, 0, new List<int>());
        var playerField = typeof(PlaySceneController).GetField("_playerM", BindingFlags.Instance | BindingFlags.NonPublic);
        playerField.SetValue(scene, player);
        var owner = map.utilCtrl.GetTile(5, 0, 5);
        map.updateCtrl.characterTileDic.Add(player.unit, owner);
        var root = Go("area-map-self-marker");
        root.SetActive(false);
        var content = root.AddComponent<RectTransform>();
        content.sizeDelta = new Vector2(464, 456); // Inner map is 64 x 56, plus four 200-pixel margins.
        content.position = new Vector3(30, -40, 7);
        content.localScale = new Vector3(1.5f, 2, 1);
        content.localRotation = Quaternion.Euler(0, 0, 20);
        var image = Go("area-map-inner").AddComponent<Z_Ui.Base.RImg>();
        image.transform.SetParent(content, false);
        image.rectTransform.anchorMin = Vector2.zero;
        image.rectTransform.anchorMax = Vector2.one;
        image.rectTransform.offsetMin = Vector2.one * 200;
        image.rectTransform.offsetMax = Vector2.one * -200;
        var marker = Go("area-map-self").AddComponent<RectTransform>();
        marker.SetParent(content, false);
        var view = (Ui.PlayMap.UiPlayMapView)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Ui.PlayMap.UiPlayMapView));
        view.go_self = marker.gameObject;
        view.rtf_self = marker;
        view.rimg_unlock = image;
        view.rtf_area = content;
        var ctrl = new Ui.PlayMap.UiPlayMapCtrl
        {
            view = view, model = new Ui.PlayMap.UiPlayMapModel { isArea = true, y = 0 }, mapCtrl = mapCtrl
        };
        var refresh = (Action)typeof(Ui.PlayMap.UiPlayMapCtrl)
            .GetMethod("RefreshSelfMarker", BindingFlags.Instance | BindingFlags.NonPublic)
            .CreateDelegate(typeof(Action), ctrl);
        try
        {
            map.data.mainData.mapUnitSize = new Vector3(2, 3, 4);
            player.pos = new Vector3(.5f, 0, 18);
            refresh();
            Require(marker.gameObject.activeSelf && Vector3.Distance(marker.localPosition,
                new Vector3((3.75f / 8 - .5f) * 64, (3f / 7 - .5f) * 56, 0)) < .0001f,
                "area self marker uses the live player, inner map bounds and non-unit cells under scaled/rotated UI");
            player.pos = new Vector3(1, 0, 19);
            refresh();
            Require(Vector3.Distance(marker.localPosition, new Vector3(0, -2, 0)) < .0001f,
                "self marker follows player movement independently of game seconds and mark/mission throttling");
            ctrl.model.y = 1;
            refresh();
            Require(!marker.gameObject.activeSelf, "browsing another Tile height hides the self marker");
            ctrl.model.y = 0;
            ctrl.model.isArea = false;
            refresh();
            Require(!marker.gameObject.activeSelf, "world mode never displays the area self marker");
            ctrl.model.isArea = true;
            refresh();
            Require(marker.gameObject.activeSelf, "returning to the player's area height restores the marker");
            playerField.SetValue(scene, null);
            refresh();
            Require(!marker.gameObject.activeSelf, "missing player clears a pooled marker");
            playerField.SetValue(scene, player);
            map.updateCtrl.characterTileDic.Del(player.unit);
            refresh();
            Require(!marker.gameObject.activeSelf, "a player without a support Tile has no self marker");
            map.updateCtrl.characterTileDic.Add(player.unit, owner);
            ready.SetValue(mapCtrl, false);
            refresh();
            Require(!marker.gameObject.activeSelf, "unready map hides the marker instead of keeping its old position");
            ready.SetValue(mapCtrl, true);
            mapCtrl.size = (0, 0, 0, 0);
            player.pos = Vector3.zero;
            refresh();
            Require(marker.gameObject.activeSelf && marker.localPosition.sqrMagnitude < .0000001f,
                "a single-cell map has a finite centered self marker");
            ctrl.OnDisable();
            Require(!marker.gameObject.activeSelf, "disable clears the self marker");
            marker.gameObject.SetActive(true);
            ctrl.OnEnable();
            Require(!marker.gameObject.activeSelf && ctrl.model.isArea && ctrl.model.cur == 0,
                "reopen clears the prior marker while resetting the displayed scene");
            play.enable = false;
            marker.gameObject.SetActive(true);
            ctrl.OnShow();
            Require(!marker.gameObject.activeSelf, "first show while Play is disabled does not retain a stale marker");

            var scrollRoot = Go("area-map-scroll").AddComponent<RectTransform>();
            scrollRoot.sizeDelta = new Vector2(1200, 800);
            scrollRoot.position = new Vector3(-10, 25, 3);
            scrollRoot.localRotation = Quaternion.Euler(0, 0, -9);
            scrollRoot.localScale = new Vector3(.9f, 1.2f, 1);
            var viewport = Go("area-map-viewport").AddComponent<RectTransform>();
            viewport.SetParent(scrollRoot, false);
            viewport.pivot = new Vector2(.25f, .7f);
            viewport.sizeDelta = scrollRoot.rect.size;
            content.SetParent(viewport, false);
            content.anchorMin = content.anchorMax = new Vector2(0, 1);
            var scroll = scrollRoot.gameObject.AddComponent<UnityEngine.UI.ScrollRect>();
            scroll.viewport = viewport;
            scroll.content = content;
            var center = (Action)typeof(Ui.PlayMap.UiPlayMapCtrl)
                .GetMethod("CenterOnPlayerIfPending", BindingFlags.Instance | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Action), ctrl);
            var updateBounds = (Action)typeof(UnityEngine.UI.ScrollRect)
                .GetMethod("UpdateBounds", BindingFlags.Instance | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Action), scroll);
            var offset = (Func<Vector2, Vector2>)typeof(UnityEngine.UI.ScrollRect)
                .GetMethod("CalculateOffset", BindingFlags.Instance | BindingFlags.NonPublic)
                .CreateDelegate(typeof(Func<Vector2, Vector2>), scroll);
            Vector3 ViewCenter() => viewport.TransformPoint(viewport.rect.center);
            void Open()
            {
                // Exercise OnShow's pending request without the unrelated mark/mission UI fixture.
                play.enable = false;
                ctrl.OnShow();
                play.enable = true;
                refresh();
                center();
            }

            scroll.velocity = new Vector2(120, -80);
            play.enable = true;
            refresh();
            center(); // Consume the first-show request once the map/player becomes available.
            Require(Vector3.Distance(marker.position, ViewCenter()) < .001f,
                "opening centers the live player in a noncentral-pivot viewport under scaled/rotated UI");
            Require(scroll.velocity == Vector2.zero, "opening clears the previous ScrollRect inertia");
            Require(image.rectTransform.offsetMin.x >= 200 && image.rectTransform.offsetMin.y >= 200
                && image.rectTransform.rect.size == new Vector2(8, 8),
                "centering retains at least 200-pixel padding without changing the inner map dimensions");
            updateBounds();
            Require(offset(Vector2.zero).sqrMagnitude < .0001f,
                "a single-cell map remains centered without ScrollRect elastic correction");

            content.anchoredPosition += new Vector2(50, -30);
            var dragged = content.anchoredPosition;
            player.pos = new Vector3(1, 0, 1);
            refresh();
            center();
            Require(content.anchoredPosition == dragged,
                "subsequent player refreshes preserve the user's map pan");

            mapCtrl.size = (8, 2, -3, 4);
            foreach (var corner in new[] { new Vector3(-6, 0, 8), new Vector3(8, 0, 32) })
            {
                player.pos = corner;
                Open();
                Require(Vector3.Distance(marker.position, ViewCenter()) < .001f,
                    "reopening centers the latest player position even at a map corner");
                updateBounds();
                Require(offset(Vector2.zero).sqrMagnitude < .0001f,
                    "edge-player centering has enough padding to avoid elastic rebound");
            }

            play.enable = false;
            ctrl.OnShow();
            ctrl.OnDisable();
            player.pos += Vector3.one;
            play.enable = true;
            dragged = content.anchoredPosition;
            refresh();
            center();
            Require(content.anchoredPosition == dragged,
                "hiding cancels a pending center so it cannot later overwrite map dragging");
        }
        finally
        {
            ctrl.OnHide();
            map.updateCtrl.characterTileDic.Del(player.unit);
            play.sceneCtrl = oldScene;
            play.mapCtrl = oldMap;
            play.enable = oldEnable;
            map.data.mainData.mapUnitSize = oldSize;
        }
    }

    private static void CommandMovementTimeStep()
    {
        UnitForm.InitInternal();
        CharacterUnitForm.InitInternal();
        var game = GameManager.instance;
        var play = PlayManager.instance;
        var oldEvents = game.evtCtrl;
        var oldScene = play.sceneCtrl;
        var oldNavigation = map.navigationCtrl;
        // This fixture isolates command timing/displacement from Nav rebuilding.
        map.navigationCtrl = null;
        var events = new GameEventController(game);
        game.evtCtrl = events;
        var scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        var prefab = Go("movement-command-no-body");
        pool.AddPool(prefab);
        Vector3 start = new Vector3(5, 0, 5);
        var obj = new ObjectUnitForm.Data(98902, false, "", prefab.name, start, Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, 0).unit;
        var character = new CharacterUnitForm.Data(98903, false, Vector3.zero, 1, 1, 1, false, "",
            prefab.name, start, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, 0, new List<int>()).unit;
        var product = (Form.CharacterProductForm.Data)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Form.CharacterProductForm.Data));
        product.uid = 98904;
        Form.CharacterProductForm.InitInternal();
        var products = Form.CharacterProductForm.DataByUid;
        products.TryGetValue(product.uid, out var oldProduct);
        products[product.uid] = product;
        UnitForm.DataByUid.TryGetValue(obj.data.uid, out var oldObject);
        UnitForm.DataByUid[obj.data.uid] = obj.data;
        CharacterUnitForm.AddData(character.data);
        typeof(PlaySceneController).GetField("_characterDic", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(scene, new Dictionary<Form.CharacterProductForm.Data, CharacterUnitForm.Data> { [product] = character.data });
        var owner = map.utilCtrl.GetTile(5, 0, 5);
        map.updateCtrl.objectTileDic.Add(obj, owner);
        map.updateCtrl.characterTileDic.Add(character, owner);
        var taskField = typeof(GameEventController).GetField("tasks", BindingFlags.Instance | BindingFlags.NonPublic);
        var clearTasks = typeof(GameEventController).GetMethod("CancelAllTasks", BindingFlags.Instance | BindingFlags.NonPublic);
        try
        {
            Z_Code.Form.BoxDataForm.Data VectorBox(Vector3 v)
            {
                var value = Z_Code.CodeHelper.CreateBox();
                value.dic["x"] = Z_Code.CodeHelper.CreateBoxByNum(v.x);
                value.dic["height"] = Z_Code.CodeHelper.CreateBoxByNum(v.y);
                value.dic["y"] = Z_Code.CodeHelper.CreateBoxByNum(v.z);
                return value;
            }
            Z_Code.InterpretAsyncTask Invoke(Z_Code.CmdBase cmd, params Z_Code.Form.BoxDataForm.Data[] prm)
            {
                var data = new Z_Code.Form.InterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(),
                    new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), null, 0, -1, 0, null,
                    new List<Z_Code.Form.BoxDataForm.Data>(), 0);
                var task = new Z_Code.InterpretAsyncTask(new Z_Code.Interpreter(data));
                task.Run();
                if ((bool)cmd.GetType().GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(cmd, new object[] { prm, task })) task.Complete();
                return task;
            }
            Z_Code.Form.BoxDataForm.Data Handle(MapUnit unit, bool productHandle = false) =>
                Z_Code.CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(productHandle
                    ? GlobalEventHelper.CHARACTER : GlobalEventHelper.SCENEOBJECT,
                    (productHandle ? product.uid : unit.data.uid).ToString()));
            Vector3 Read(Z_Code.Form.BoxDataForm.Data value) => new Vector3(
                value.dic["x"].num, value.dic["height"].num, value.dic["y"].num);

            var navigationSpeed = new Form.CharacterParamForm.Data(-1, "RunSpeed", default, "",
                Z_Code.Form.BoxDataForm.GetJoByData(Z_Code.CodeHelper.CreateBoxByNum(3)).ToString(), "", default);
            var zeroSpeed = new Form.CharacterParamForm.Data(-1, "Stopped", default, "",
                Z_Code.Form.BoxDataForm.GetJoByData(Z_Code.CodeHelper.CreateBoxByNum(0)).ToString(), "", default);
            var navigationParams = new Dictionary<string, Form.CharacterParamForm.Data>
            {
                ["Speed"] = navigationSpeed, ["RunSpeed"] = navigationSpeed,
                ["Stopped"] = zeroSpeed, ["NullSpeed"] = null
            };
            var navigationSize = map.data.mainData.mapUnitSize;
            try
            {
                foreach (var cellSize in new[] { Vector3.one, new Vector3(2, 3, 4) })
                {
                    map.data.mainData.mapUnitSize = cellSize;
                    foreach (bool relative in new[] { false, true })
                        foreach (var binding in new[] {
                            (name: "", parameters: navigationParams, expected: 2.5f),
                            (name: (string)null, parameters: navigationParams, expected: 2.5f),
                            (name: "RemovedSpeed", parameters: navigationParams, expected: 2.5f),
                            (name: "RunSpeed", parameters: (Dictionary<string, Form.CharacterParamForm.Data>)null, expected: 2.5f),
                            (name: "NullSpeed", parameters: navigationParams, expected: 2.5f),
                            (name: "RunSpeed", parameters: navigationParams, expected: 3f),
                            (name: "Stopped", parameters: navigationParams, expected: 0f) })
                        {
                            product.speedParamName = binding.name;
                            product.paramDic = binding.parameters;
                            product.enableNav = false;
                            character.data.pos = start;
                            character.data.speed = 2.5f;
                            character.data.pathDis = 1;
                            character.data.alertDis = 1;
                            var input = relative
                                ? Invoke(new Z_Code.NewVectorCmd(), Z_Code.CodeHelper.CreateBoxByNum(0),
                                    Z_Code.CodeHelper.CreateBoxByNum(4), Z_Code.CodeHelper.CreateBoxByNum(0)).res[0]
                                : VectorBox(GameManager.MapPosToPlayerPos(new Vector3(6, 0, 6)));
                            var original = Read(input);
                            var task = Invoke(relative ? (Z_Code.CmdBase)new Z_Code.SetCharacterNavigateRelativeCmd()
                                : new Z_Code.SetCharacterNavigateCmd(), Handle(character, true), input);
                            var destination = relative ? start + Vector3.forward * 4 * cellSize.z
                                : map.utilCtrl.MapPos2RealPos(new Vector3(6, 0, 6));
                            Require(task.IsComplete() && string.IsNullOrEmpty(task.error)
                                && character.data.speed == binding.expected,
                                "both navigation commands preserve speed for missing bindings and honor valid/zero parameters");
                            Require(character.data.destination == destination && product.enableNav
                                && character.data.pathDis == 999 && character.data.alertDis == 999
                                && product.speedParamName == binding.name && ReferenceEquals(product.paramDic, binding.parameters)
                                && Read(input) == original,
                                "navigation still activates and converts destinations without changing authored bindings or vector inputs");
                        }
                }
            }
            finally { map.data.mainData.mapUnitSize = navigationSize; }

            character.Move(Vector3.right * .001f);
            Require(Vector3.Distance(character.data.pos, start + Vector3.right * .001f) < .00001f,
                "unobstructed high-FPS sub-centimetre Character steps are not discarded");
            foreach (var sample in new[] {
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveObjectCmd(), unit: (MapUnit)obj, relative: false, isCharacter: false, legacy: true),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveObjectRelativeCmd(), unit: (MapUnit)obj, relative: true, isCharacter: false, legacy: true),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveCharacterCmd(), unit: (MapUnit)character, relative: false, isCharacter: true, legacy: true),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveCharacterRelativeCmd(), unit: (MapUnit)character, relative: true, isCharacter: true, legacy: true),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveCmd(), unit: (MapUnit)obj, relative: false, isCharacter: false, legacy: false),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveRelativeCmd(), unit: (MapUnit)obj, relative: true, isCharacter: false, legacy: false),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveCmd(), unit: (MapUnit)character, relative: false, isCharacter: true, legacy: false),
                (cmd: (Z_Code.CmdBase)new Z_Code.MoveRelativeCmd(), unit: (MapUnit)character, relative: true, isCharacter: true, legacy: false) })
            {
                clearTasks.Invoke(events, null);
                sample.unit.data.pos = start;
                var input = sample.relative ? new Vector3(1, 0, 1) : GameManager.MapPosToPlayerPos(
                    map.utilCtrl.RealPos2MapPos(new Vector3(6, 0, 6)));
                var pos = VectorBox(input);
                string handle = GlobalEventHelper.GetName(sample.isCharacter ? GlobalEventHelper.CHARACTER
                    : GlobalEventHelper.SCENEOBJECT, (sample.isCharacter ? product.uid : obj.data.uid).ToString());
                var prm = new[] { Z_Code.CodeHelper.CreateBoxByStr(handle), pos,
                    Z_Code.CodeHelper.CreateBoxByNum(sample.legacy ? 1 : Mathf.Sqrt(2)) };
                var data = new Z_Code.Form.InterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(),
                    new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), null, 0, -1, 0, null,
                    new List<Z_Code.Form.BoxDataForm.Data>(), 0);
                var task = new Z_Code.InterpretAsyncTask(new Z_Code.Interpreter(data));
                task.Run();
                sample.cmd.GetType().GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic)
                    .Invoke(sample.cmd, new object[] { prm, task });
                float delta = MapUtilController.MovementDeltaTime;
                Vector3 expected = new Vector3(1, 0, 1) * delta;
                Require(Vector3.Distance(sample.unit.data.pos, start + expected) < .0001f,
                    sample.cmd.GetName() + " passes only the bounded displacement, not the nonzero old position");
                Require(data.heapTemp.Count == 0 && !task.IsComplete() && Read(pos) == input,
                    sample.cmd.GetName() + " captures its own state without modifying input vectors or shared heapTemp");
                Require(((System.Collections.IList)taskField.GetValue(events)).Count == 1,
                    sample.cmd.GetName() + " remains scheduled rather than completing after a lagged frame");
            }
            clearTasks.Invoke(events, null);
            var advance = typeof(Z_Code.MoveCmd).GetMethod("AdvanceMove", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var unit in new MapUnit[] { obj, character })
                foreach (float speed in new[] { .25f, 2f, 4f })
                    foreach (float ownSpeed in new[] { 1f, 100f })
                    {
                        character.data.speed = ownSpeed;
                        unit.data.pos = start;
                        var args = new object[] { unit, start + Vector3.right, speed, 1f / speed, .1f };
                        Require(!(bool)advance.Invoke(null, args)
                            && Vector3.Distance(unit.data.pos, start + Vector3.right * speed * .02f) < .00001f
                            && Mathf.Abs((float)args[3] - (1f / speed - .02f)) < .00001f,
                            "canonical movement uses supplied speed and the same capped clock, independent of Character Speed");
                    }
            obj.data.pos = start;
            var lastStep = new object[] { obj, start + Vector3.right * .005f, 10f, .0005f, .02f };
            Require((bool)advance.Invoke(null, lastStep)
                && Vector3.Distance(obj.data.pos, start + Vector3.right * .005f) < .000001f,
                "last speed-based step reaches target without overshoot");
            foreach (var unit in new MapUnit[] { obj, character })
                foreach (float speed in new[] { 0f, -1f, float.NaN, float.PositiveInfinity })
                {
                    unit.data.pos = start;
                    var task = Invoke(new Z_Code.MoveRelativeCmd(), Handle(unit), VectorBox(Vector3.right),
                        Z_Code.CodeHelper.CreateBoxByNum(speed));
                    Require(task.IsComplete() && unit.data.pos == start, "invalid/nonpositive new speed is a safe no-op");
                }
            var rotate = typeof(Z_Code.SetRotationCmd).GetMethod("AdvanceRotation", BindingFlags.Static | BindingFlags.NonPublic);
            foreach (var unit in new MapUnit[] { obj, character })
            {
                var oldInstance = unit.ins;
                var visible = Go("rotation-command-instance");
                visible.SetActive(false);
                unit.ins = visible.AddComponent<MapInstance>();
                unit.ins.unit = unit;
                try
                {
                    foreach (bool productHandle in unit == character ? new[] { false, true } : new[] { false })
                        foreach (var cmd in new Z_Code.CmdBase[] { new Z_Code.SetRotationCmd(),
                            new Z_Code.SetCharacterRotationCmd(), new Z_Code.SetObjectRotationCmd() })
                            foreach (float angle in new[] { -90f, 450f })
                            {
                                unit.data.pos = start;
                                unit.data.euler = new Vector3(17, 350, 23);
                                var task = Invoke(cmd, Handle(unit, productHandle), Z_Code.CodeHelper.CreateBoxByNum(angle),
                                    Z_Code.CodeHelper.CreateBoxByNum(0));
                                float target = Mathf.Repeat(angle, 360);
                                Require(task.IsComplete() && unit.data.euler == new Vector3(17, target, 23)
                                    && Mathf.Abs(Mathf.DeltaAngle(unit.ins.transform.eulerAngles.y, target)) < .001f,
                                    "instant rotation and aliases update data/instance for both scene/Product handles");
                                if (unit is CharacterUnit ch)
                                    Require(ch.forceEuler == unit.data.euler, "rotation synchronizes Character pending facing");
                            }
                    foreach (var turn in new[] { (start: 350f, target: 10f, middle: 0f),
                        (start: 10f, target: 350f, middle: 0f), (start: 0f, target: 90f, middle: 45f) })
                    {
                        unit.data.euler = new Vector3(17, turn.start, 23);
                        var args = new object[] { unit, turn.start, turn.target, .04f, 0f, 1f };
                        Require(!(bool)rotate.Invoke(null, args) && Mathf.Abs((float)args[4] - .02f) < .000001f
                            && Mathf.Abs(Mathf.DeltaAngle(unit.data.euler.y, turn.middle)) < .001f,
                            "timed rotation uses shortest constant-rate interpolation and the bounded frame clock");
                        Require((bool)rotate.Invoke(null, args) && Mathf.Abs((float)args[4] - .04f) < .000001f
                            && unit.data.euler == new Vector3(17, turn.target, 23)
                            && Mathf.Abs(Mathf.DeltaAngle(unit.ins.transform.eulerAngles.y, turn.target)) < .001f,
                            "timed rotation completes at the exact target with X/Z preserved");
                    }
                    foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
                        foreach (bool invalidDuration in new[] { false, true })
                        {
                            var originalEuler = unit.data.euler;
                            var task = Invoke(new Z_Code.SetRotationCmd(), Handle(unit),
                                Z_Code.CodeHelper.CreateBoxByNum(invalidDuration ? 90 : invalid),
                                Z_Code.CodeHelper.CreateBoxByNum(invalidDuration ? invalid : 1));
                            Require(task.IsComplete() && unit.data.euler == originalEuler,
                                "nonfinite rotation arguments do not poison data or leave a waiting task");
                        }
                    var duration = Z_Code.CodeHelper.CreateBoxByNum(1);
                    var rotating = Invoke(new Z_Code.SetRotationCmd(), Handle(unit),
                        Z_Code.CodeHelper.CreateBoxByNum(180), duration);
                    Require(!rotating.IsComplete() && duration.num == 1,
                        "a positive rotation duration blocks completion without mutating the caller's argument");
                    var pending = ((System.Collections.IEnumerable)taskField.GetValue(events)).Cast<object>().Last();
                    UnitForm.DataByUid.Remove(unit.data.uid);
                    var original = unit.data.euler;
                    pending.GetType().GetMethod("Update").Invoke(pending, null);
                    Require(rotating.IsComplete() && unit.data.euler == original,
                        "rotation safely stops when the captured live unit is removed");
                    UnitForm.DataByUid[unit.data.uid] = unit.data;
                }
                finally { unit.ins = oldInstance; }
            }
            foreach (string invalid in new[] { "", "$so$bad$so$", "$so$-1$so$", "$so$9999999$so$", "not-a-reference" })
            {
                var original = obj.data.euler;
                var task = Invoke(new Z_Code.SetRotationCmd(), Z_Code.CodeHelper.CreateBoxByStr(invalid),
                    Z_Code.CodeHelper.CreateBoxByNum(90), Z_Code.CodeHelper.CreateBoxByNum(0));
                Require(task.IsComplete() && obj.data.euler == original, "invalid rotation references are safe no-ops");
            }
            var oldSize = map.data.mainData.mapUnitSize;
            try
            {
                map.data.mainData.mapUnitSize = new Vector3(2, 3, 4);
                foreach (var unit in new MapUnit[] { obj, character })
                    foreach (bool productHandle in unit == character ? new[] { false, true } : new[] { false })
                    {
                        unit.data.pos = map.utilCtrl.MapPos2RealPos(new Vector3(5, 0, 5));
                        var handle = Handle(unit, productHandle);
                        var absolute = GameManager.MapPosToPlayerPos(new Vector3(6, 0, 6));
                        Invoke(new Z_Code.SetPositionCmd(), handle, VectorBox(absolute));
                        Require(Vector3.Distance(unit.data.pos, new Vector3(12, 0, 24)) < .00001f
                            && Read(Invoke(new Z_Code.GetPositionCmd(), handle).res[0]) == absolute,
                            "Get/Set absolute round trip for both references and nonuniform map dimensions");
                        Invoke(new Z_Code.SetPositionRelativeCmd(), handle, VectorBox(new Vector3(-1, 0, -1)));
                        Require(Vector3.Distance(unit.data.pos, new Vector3(10, 0, 20)) < .00001f,
                            "relative Set scales world-axis offsets without the absolute 500 shift");
                        var args = new object[] { unit, new Vector3(12, 0, 20), 2f, .5f, .1f };
                        advance.Invoke(null, args);
                        Require(Vector3.Distance(unit.data.pos, new Vector3(10.08f, 0, 20)) < .00001f,
                            "supplied speed counts logical tiles rather than world metres");
                    }
                obj.data.pos = new Vector3(10, 0, 20);
                character.data.pos = new Vector3(12, 3, 28);
                Require(Read(Invoke(new Z_Code.GetPositionRelativeCmd(), Handle(character, true), Handle(obj)).res[0])
                    == new Vector3(1, 1, 2), "relative Get returns target minus origin in map-unit coordinates");
                Require(Read(Invoke(new Z_Code.GetPositionRelativeCmd(), Handle(obj), Handle(character)).res[0])
                    == new Vector3(-1, -1, -2), "relative Get supports mixed object/character scene handles");
                Require(Read(Invoke(new Z_Code.GetPositionRelativeCmd(), Handle(obj), Handle(obj)).res[0]) == Vector3.zero,
                    "relative Get of self is zero");
            }
            finally { map.data.mainData.mapUnitSize = oldSize; }
            foreach (string invalid in new[] { "", "$so$bad$so$", "$so$-1$so$", "$so$9999999$so$", "not-a-reference" })
                Require(Read(Invoke(new Z_Code.GetPositionCmd(), Z_Code.CodeHelper.CreateBoxByStr(invalid)).res[0]) == Vector3.zero,
                    "missing/malformed scene references produce a zero vector without throwing");
            character.productInfo = (product.uid, -1);
            character.data.pos = new Vector3(445, 0, 489);
            var queryCenter = Invoke(new Z_Code.GetPositionCmd(), Handle(character)).res[0];
            var neighbours = Invoke(new Z_Code.GetCharactersInAreaCmd(), queryCenter,
                Z_Code.CodeHelper.CreateBoxByNum(10)).res[0];
            Require(neighbours.dic.Values.Any(v => v.str == Handle(character, true).str),
                "interaction param1 scene handle supplies the actual center to character-area searches");

            obj.data.pos = start;
            var moving = Invoke(new Z_Code.MoveRelativeCmd(), Handle(obj), VectorBox(Vector3.right),
                Z_Code.CodeHelper.CreateBoxByNum(1));
            UnitForm.DataByUid.Remove(obj.data.uid);
            foreach (var task in ((System.Collections.IEnumerable)taskField.GetValue(events)).Cast<object>().ToArray())
                task.GetType().GetMethod("Update").Invoke(task, null);
            Require(moving.IsComplete(), "movement ends safely when its captured scene instance is removed");
            UnitForm.DataByUid[obj.data.uid] = obj.data;
        }
        finally
        {
            clearTasks.Invoke(events, null);
            game.evtCtrl = oldEvents;
            play.sceneCtrl = oldScene;
            if (oldProduct == null) products.Remove(product.uid); else products[product.uid] = oldProduct;
            if (oldObject == null) UnitForm.DataByUid.Remove(obj.data.uid); else UnitForm.DataByUid[obj.data.uid] = oldObject;
            map.updateCtrl.RemoveObjectOverlap(obj);
            map.navigationCtrl = oldNavigation;
            map.updateCtrl.characterTileDic.Del(character);
            map.updateCtrl.characterOverlapTileDic.Del(character);
            CharacterUnitForm.RemoveData(character.data.uid);
            UnitForm.RemoveData(character.data.uid);
            events.sceneTriggerCtrl.Unregister<CollideEvent>();
            events.sceneTriggerCtrl.Unregister<TileEvent>();
            events.sceneTriggerCtrl.Unregister<ItemEvent>();
            events.sceneTriggerCtrl.Unregister<ObjectEvent>();
            events.sceneTriggerCtrl.Unregister<CharacterEvent>();
            events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryItemEvent>();
            events.storyTriggerCtrl.Unregister<StoryCharacterEvent>();
            events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
        }
    }

    private static void SceneCommandForms()
    {
        var oldCommands = new Dictionary<string, Z_Code.CmdBase>(Z_Code.BaseData.cmdDic);
        var oldLabs = LabForm.DataById.Values.Where(l => !(l is Form.CmdLabForm.Data)).ToArray();
        // RuntimeInitialize methods do not run in this EditMode fixture.
        typeof(Form.CmdLabForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Form.CmdLabForm.InitInternal();
        Form.GameCmdDataForm.InitInternal();
        typeof(Z_Code.GetPositionCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        typeof(Z_Code.MoveCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        typeof(Z_Code.SetRotationCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        Z_Code.BaseData.cmdDic["NewVector"] = new Z_Code.NewVectorCmd();
        var save = new GameSaveController(GameManager.instance);
        // SaveAndLoad recognizes absolute paths by the HlZy marker; stay in the isolated temp root.
        var folder = System.IO.Path.Combine(Application.temporaryCachePath, "HlZy-scene-command-" + Guid.NewGuid().ToString("N"));
        System.IO.Directory.CreateDirectory(folder);
        try
        {
            Require(Form.CmdLabForm.DataById.Count == 26 && Form.CmdLabForm.DataById[10026].lv1Lab == "scene",
                "CmdLab inherits Lab and preserves all 25 old IDs plus the scene category");
            foreach (var cmd in Form.GameCmdDataForm.DataByUid.Values)
                Require(Form.CmdLabForm.DataById.TryGetValue(cmd.labId, out var lab)
                    && ReferenceEquals(lab, LabForm.DataById[cmd.labId]), "all command categories resolve to the child Form identity");
            var names = new[] { "GetPosition", "GetPositionRelative", "SetPosition", "SetPositionRelative", "Move", "MoveRelative", "SetRotation" };
            var compiler = new Z_Code.Compiler();
            foreach (var name in names)
            {
                Require(Z_Code.CmdBase.TryGetForm(name, out var cmd) && ((Form.GameCmdDataForm.Data)cmd).labId == 10026,
                    "canonical command appears in scene: " + name);
                string code = cmd.defaultCode + (cmd.defaultCode.EndsWith(";") ? "" : ";");
                Require(compiler.TryCompile(code, out _, out _, out _, out _, out _, out var errors),
                    "canonical command default code compiles: " + name + " " + string.Join(";", errors.Select(e => e.Message)));
            }
            foreach (var pair in new[] {
                ("GetCharacterPosition", "GetPosition"), ("GetObjectPosition", "GetPosition"),
                ("SetCharacterPosition", "SetPosition"), ("SetObjectPosition", "SetPosition"),
                ("SetCharacterPositionRelative", "SetPositionRelative"),
                ("MoveCharacter", "Move"), ("MoveObject", "Move"),
                ("MoveCharacterRelative", "MoveRelative"), ("MoveObjectRelative", "MoveRelative"),
                ("SetCharacterRotation", "SetRotation"), ("SetObjectRotation", "SetRotation") })
            {
                bool hasForm = Z_Code.CmdBase.TryGetForm(pair.Item1, out var cmd);
                Require(!Z_Code.Form.CmdDataForm.DataByName.ContainsKey(pair.Item1)
                    && hasForm && cmd.name == pair.Item2,
                    "old names are editable runtime aliases without duplicate chooser rows: " + pair.Item1);
                var code = cmd.defaultCode.Replace(pair.Item2 + "(", pair.Item1 + "(");
                if (!code.EndsWith(";")) code += ";";
                Require(compiler.TryCompile(code, out _, out _, out _, out _, out _, out _),
                    "stored legacy command compiles through canonical metadata: " + pair.Item1);
            }
            var custom = new LabForm.Data(9701, "regression-story", "", "", nameof(Form.EventProgramDataForm));
            LabForm.AddData(custom);
            save.SaveLab(folder);
            var serialized = JArray.Parse(SaveAndLoad.Load<string>(folder + "/lf"));
            Require(serialized.Any(l => (int)l["id"] == custom.id)
                && serialized.All(l => (string)l["belong"] != nameof(Form.GameCmdDataForm)),
                "story lf persists custom labels without built-in CmdLab rows");
            serialized.Add(LabForm.GetJoByData(new LabForm.Data(10026, "obsolete-scene", "", "", nameof(Form.GameCmdDataForm))));
            SaveAndLoad.Save(folder + "/lf", serialized.ToString());
            save.LoadLab(folder + "/");
            Require(LabForm.DataById[9701].lv1Lab == "regression-story"
                && ReferenceEquals(LabForm.DataById[10026], Form.CmdLabForm.DataById[10026])
                && Form.CmdLabForm.DataById[10026].lv1Lab == "scene",
                "old lf rows cannot overwrite command categories when loading a story");
            save.LoadLab(null);
            Require(!LabForm.DataById.ContainsKey(9701) && Form.CmdLabForm.DataById.Count == 26,
                "switching stories clears authored labels without removing static CmdLab entries");
        }
        finally
        {
            foreach (var lab in oldLabs) LabForm.AddData(lab);
            Z_Code.BaseData.cmdDic.Clear();
            foreach (var pair in oldCommands) Z_Code.BaseData.cmdDic[pair.Key] = pair.Value;
        }
    }

    private static void InteractionParameters()
    {
        UnitForm.InitInternal();
        CharacterUnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
        Form.EventProgramDataForm.InitInternal();
        Form.EventInterpretDataForm.InitInternal();
        var game = GameManager.instance;
        var play = PlayManager.instance;
        var oldEvents = game.evtCtrl;
        var oldScene = play.sceneCtrl;
        var oldState = game.curProgress.eventState;
        var oldBlock = game.curProgress.blockProgramUid;
        var events = new GameEventController(game);
        game.evtCtrl = events;
        var scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        var player = (Form.CharacterProductForm.Data)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Form.CharacterProductForm.Data));
        player.uid = 97092;
        var playerField = typeof(PlaySceneController).GetField("_playerG", BindingFlags.Instance | BindingFlags.NonPublic);
        playerField.SetValue(scene, player);
        game.curProgress.eventState = EventState.Normal;
        game.curProgress.blockProgramUid = 0;
        var character = new CharacterUnitForm.Data(97191, false, Vector3.zero, 1, 1, 1, false, "",
            source.name, Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, 0, new List<int>());
        CharacterUnitForm.AddData(character);
        character.unit.productInfo = (97091, -1); // Product UID deliberately differs from scene UID.
        var obj = new ObjectUnitForm.Data(97193, false, "", source.name, Vector3.zero,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.AddData(obj);
        obj.unit.productInfo = (97093, -1);
        var program = new Form.EventProgramDataForm.Data(97097, "interaction-typed-arguments", "",
            new List<string>(), new List<int>(), 0, "void", 0);
        Require(program.TryApplyCode("receivedSelf = param1; receivedPlayer = param2;", out _, out _),
            "interaction argument probe compiles without command dependencies");
        Form.EventProgramDataForm.AddData(program);
        var root = Go("interaction-argument-button");
        root.SetActive(false);
        var rect = root.AddComponent<RectTransform>();
        var button = root.AddComponent<Z_Ui.Base.Btn>();
        var holder = root.AddComponent<Z_Ui.Base.UiHolder>();
        holder.elementTrsLst = Enumerable.Repeat<Transform>(rect, 4).ToList();
        var action = new Ui.PlaySceneMain.UiActionCtrl
        {
            view = new Ui.PlaySceneMain.UiActionView(holder),
            model = new Ui.PlaySceneMain.UiActionModel()
        };
        action.OnCreate();
        try
        {
            foreach (MapUnit unit in new MapUnit[] { character.unit, obj.unit })
            {
                unit.evtDic = new Dictionary<string, Form.EventTriggerForm.Data>
                {
                    ["onInteractEvent"] = new Form.EventTriggerForm.Data(-1, "onInteractEvent", new List<int> { program.uid }, default)
                };
                action.model.data = unit.data;
                button.onClick.Invoke();
                var invocation = Form.EventInterpretDataForm.DataByUid.Values.Single(data =>
                    data.releaseTrigger == "onInteractEvent" && ReferenceEquals(data.sceneOwner, unit.data));
                string expected = unit is CharacterUnit
                    ? GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, unit.productInfo.Item1.ToString())
                    : GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, unit.data.uid.ToString());
                Require(invocation.heap["param1"].str == expected,
                    "actual interaction click distinguishes Character Product and Object scene references");
                Require(invocation.heap["param2"].str == GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, player.uid.ToString()),
                    "interaction param2 remains the current player's Character Product reference");
                Require(invocation.user == unit.data.uid && ReferenceEquals(invocation.sceneOwner, unit.data),
                    "typed interaction arguments do not change scene event ownership");
                events.LateUpdate();
                Require(invocation.heap["receivedSelf"].str == expected
                    && invocation.heap["receivedPlayer"].str == invocation.heap["param2"].str,
                    "the interpreter receives both typed interaction arguments unchanged");
            }
            int count = Form.EventInterpretDataForm.DataByUid.Count;
            playerField.SetValue(scene, null);
            button.onClick.Invoke();
            Require(Form.EventInterpretDataForm.DataByUid.Count == count, "interaction without a player remains a no-op");
            playerField.SetValue(scene, player);
            action.model.data = null;
            button.onClick.Invoke();
            Require(Form.EventInterpretDataForm.DataByUid.Count == count, "empty pooled interaction data is a safe no-op");
        }
        finally
        {
            foreach (var data in Form.EventInterpretDataForm.DataByUid.Values.Where(data =>
                ReferenceEquals(data.sceneOwner, character) || ReferenceEquals(data.sceneOwner, obj)).ToArray())
                Form.EventInterpretDataForm.RemoveData(data.uid);
            Form.EventProgramDataForm.RemoveData(program.uid);
            CharacterUnitForm.RemoveData(character.uid);
            UnitForm.RemoveData(character.uid);
            ObjectUnitForm.RemoveData(obj.uid);
            UnitForm.RemoveData(obj.uid);
            game.evtCtrl = oldEvents;
            play.sceneCtrl = oldScene;
            game.curProgress.eventState = oldState;
            game.curProgress.blockProgramUid = oldBlock;
            events.sceneTriggerCtrl.Unregister<CollideEvent>();
            events.sceneTriggerCtrl.Unregister<TileEvent>();
            events.sceneTriggerCtrl.Unregister<ItemEvent>();
            events.sceneTriggerCtrl.Unregister<ObjectEvent>();
            events.sceneTriggerCtrl.Unregister<CharacterEvent>();
            events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryItemEvent>();
            events.storyTriggerCtrl.Unregister<StoryCharacterEvent>();
            events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
        }
    }

    private sealed class CharacterRemovalProbe : IZ_Listener<CharacterEvent>, IZ_Listener<SceneActionEvent>
    {
        public readonly List<int> removed = new List<int>();
        public readonly List<int> options = new List<int>();
        public void OnEvent(CharacterEvent evt)
        {
            if (evt.type != MapEventType.Remove) return;
            Require(!UnitForm.DataByUid.ContainsKey(evt.unit.data.uid) && evt.unit.ins == null,
                "Character Remove fires after deregistration and instance release");
            removed.Add(evt.unit.data.uid);
        }
        public void OnEvent(SceneActionEvent evt)
        {
            if (evt.type == SceneActionEventType.Remove) options.Add(evt.unitUid);
        }
    }

    private sealed class SchedulingEvent : Form.EventInterpretDataForm.Data
    {
        public Func<bool> step;
        public int calls;

        public SchedulingEvent(int uid = -1, UnitForm.Data owner = null)
            : base(uid, new List<Z_Code.Form.BoxDataForm.Data>(),
                new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), null, 0, -1,
                owner?.uid ?? 0, null, new List<Z_Code.Form.BoxDataForm.Data>(), 0,
                owner == null ? "" : "onPerSecondEvent", 0)
        {
            sceneOwner = owner;
        }

        public override Z_Code.Form.InterpretDataForm.RetInfo Interpret()
        {
            calls++;
            return new Z_Code.Form.InterpretDataForm.RetInfo { complete = step?.Invoke() ?? false };
        }
    }

    private static void InterpreterSchedulingOptimization()
    {
        Form.EventInterpretDataForm.InitInternal();
        Form.ProgressForm.InitInternal();
        UnitForm.InitInternal();
        var gameField = typeof(Z_MonoSingleton<GameManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        var oldGame = (GameManager)gameField.GetValue(null);
        var gameGo = Go("interpreter-scheduling-game");
        gameGo.SetActive(false);
        var game = gameGo.AddComponent<GameManager>();
        gameField.SetValue(null, game);
        var progress = (Form.ProgressForm.Data)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Form.ProgressForm.Data));
        progress.uid = 1;
        progress.eventState = EventState.Normal;
        Form.ProgressForm.DataByUid.TryGetValue(1, out var oldProgress);
        Form.ProgressForm.DataByUid[1] = progress;
        game.curScene = new Form.SceneForm.Data(98001, "scheduler", 0, Vector2.zero, true, false,
            new Dictionary<string, Form.EventTriggerForm.Data>(), new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>>(), false);
        var events = new GameEventController(game);
        game.evtCtrl = events;
        var tasksField = typeof(GameEventController).GetField("tasks", BindingFlags.Instance | BindingFlags.NonPublic);
        var snapshotField = typeof(GameEventController).GetField("eventSnapshot", BindingFlags.Instance | BindingFlags.NonPublic);
        var cancelEventTasks = typeof(GameEventController).GetMethod("CancelEventTasks", BindingFlags.Instance | BindingFlags.NonPublic);
        int TaskCount() => ((System.Collections.ICollection)tasksField.GetValue(events)).Count;
        Z_Code.InterpretAsyncTask Task(Z_Code.Form.InterpretDataForm.Data data, Func<bool> update)
        {
            data ??= new Z_Code.Form.InterpretDataForm.Data(-1,
                new List<Z_Code.Form.BoxDataForm.Data>(), new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(),
                null, 0, -1, 0, null, new List<Z_Code.Form.BoxDataForm.Data>(), 0);
            var task = new Z_Code.InterpretAsyncTask(new Z_Code.Interpreter(data));
            task.Run();
            events.StartTask(task, update);
            return task;
        }
        SchedulingEvent AddEvent(SchedulingEvent data)
        {
            Form.EventInterpretDataForm.AddData(data);
            return data;
        }
        void Clean()
        {
            events.Reset();
            progress.blockProgramUid = 0;
        }

        try
        {
            Clean();
            var child = new SchedulingEvent { step = () => true };
            var parent = new SchedulingEvent();
            parent.step = () =>
            {
                AddEvent(child);
                events.LateUpdate();
                return true;
            };
            AddEvent(parent);
            var snapshot = snapshotField.GetValue(events);
            events.LateUpdate();
            Require(parent.calls == 1 && child.calls == 0 && Form.EventInterpretDataForm.DataByUid.ContainsKey(child.uid),
                "scheduler snapshot defers newly added events, including reentrant LateUpdate calls");
            events.LateUpdate();
            Require(child.calls == 1 && Form.EventInterpretDataForm.DataByUid.Count == 0
                && ReferenceEquals(snapshot, snapshotField.GetValue(events))
                && ((System.Collections.ICollection)snapshot).Count == 0,
                "scheduler reuses and releases its frame snapshot while running the child on the next pass");
            for (int i = 0; i < 16; i++) events.LateUpdate();
            long probeBefore = GC.GetAllocatedBytesForCurrentThread();
            var allocationProbe = new byte[1024];
            bool allocationCounterAvailable = GC.GetAllocatedBytesForCurrentThread() > probeBefore;
            GC.KeepAlive(allocationProbe);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 1000; i++) events.LateUpdate();
            long bytes = GC.GetAllocatedBytesForCurrentThread() - before;
            if (allocationCounterAvailable)
                Require(bytes == 0, "one thousand warmed empty scheduler passes allocate zero bytes: " + bytes);
            else
                Debug.LogWarning("SCHEDULER_GC_COUNTER_UNAVAILABLE: explicit allocations also report zero; allocation assertion skipped.");

            // Replacing a later snapshot entry under the same UID must not run or
            // remove the replacement in the current pass.
            Clean();
            AddEvent(new SchedulingEvent(98009));
            AddEvent(new SchedulingEvent(98010));
            // Form.Clear removes rows individually, so Mono Dictionary can reuse
            // its free slots in reverse insertion order. Select roles from the
            // actual next snapshot order instead of relying on insertion order.
            var replacementOrder = Form.EventInterpretDataForm.DataByUid.Values.Cast<SchedulingEvent>().ToArray();
            var replacing = replacementOrder[0];
            var stale = replacementOrder[1];
            var replacement = new SchedulingEvent(stale.uid) { step = () => true };
            replacing.step = () =>
            {
                Form.EventInterpretDataForm.RemoveData(stale.uid);
                AddEvent(replacement);
                return true;
            };
            events.LateUpdate();
            Form.EventInterpretDataForm.DataByUid.TryGetValue(replacement.uid, out var registeredReplacement);
            Require(stale.calls == 0 && replacement.calls == 0
                && ReferenceEquals(registeredReplacement, replacement),
                "a stale frame snapshot cannot interpret or remove a replacement with a recycled UID: "
                + $"first={replacing.uid}/{replacing.calls}, stale={stale.uid}/{stale.calls}, replacementCalls={replacement.calls}, "
                + $"registeredIsReplacement={ReferenceEquals(registeredReplacement, replacement)}, remaining={Form.EventInterpretDataForm.DataByUid.Count}");
            events.LateUpdate();
            Require(replacement.calls == 1 && Form.EventInterpretDataForm.DataByUid.Count == 0,
                "the replacement event starts on the next scheduler pass");

            Clean();
            var order = new List<string>();
            bool polling = false;
            bool appended = false;
            int childPolls = 0;
            Task(null, () =>
            {
                if (!polling) return false;
                order.Add("A");
                if (!appended)
                {
                    appended = true;
                    Task(null, () => { order.Add("C"); return ++childPolls == 2; });
                }
                return true;
            });
            Task(null, () => { if (!polling) return false; order.Add("B"); return true; });
            polling = true;
            events.LateUpdate();
            Require(order.SequenceEqual(new[] { "A", "C", "B", "C" }) && TaskCount() == 0,
                "stable task compaction preserves immediate and same-pass appended-task polling order");

            Clean();
            polling = false;
            bool finishSurvivors = false;
            order.Clear();
            Task(null, () => { if (!polling) return false; order.Add("A"); return finishSurvivors; });
            Task(null, () => { if (!polling) return false; order.Add("B"); return true; });
            Task(null, () => { if (!polling) return false; order.Add("C"); return finishSurvivors; });
            polling = true;
            events.LateUpdate();
            Require(order.SequenceEqual(new[] { "A", "B", "C" }) && TaskCount() == 2,
                "task compaction visits every task once and keeps interleaved survivors");
            finishSurvivors = true;
            order.Clear();
            events.LateUpdate();
            Require(order.SequenceEqual(new[] { "A", "C" }) && TaskCount() == 0,
                "task survivor order stays stable on the next frame");

            Clean();
            polling = false;
            bool throwOnPoll = true;
            int completedPolls = 0;
            int throwingPolls = 0;
            int unpolledPolls = 0;
            var exceptionRoot = AddEvent(new SchedulingEvent(98019));
            Task(exceptionRoot, () => { completedPolls++; return polling; });
            Task(exceptionRoot, () =>
            {
                throwingPolls++;
                if (!polling) return false;
                if (throwOnPoll) throw new InvalidOperationException("expected-task-poll");
                return true;
            });
            Task(exceptionRoot, () => { unpolledPolls++; return polling; });
            polling = true;
            bool caught = false;
            try { events.LateUpdate(); }
            catch (InvalidOperationException e) { caught = e.Message == "expected-task-poll"; }
            Require(caught && completedPolls == 2 && throwingPolls == 2 && unpolledPolls == 1 && TaskCount() == 2,
                "a throwing task retains itself and unpolled work without reviving completed tasks");
            throwOnPoll = false;
            events.LateUpdate();
            Require(completedPolls == 2 && throwingPolls == 3 && unpolledPolls == 2 && TaskCount() == 0
                && ((System.Collections.IDictionary)typeof(GameEventController)
                    .GetField("tasksByEvent", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(events)).Count == 0,
                "scheduler exception cleanup releases the snapshot guard and removes every finished task index");

            foreach (bool reset in new[] { false, true })
            {
                Clean();
                polling = false;
                int secondPolls = 0;
                int freshPolls = 0;
                Z_Code.InterpretAsyncTask fresh = null;
                var first = Task(null, () =>
                {
                    if (!polling) return false;
                    if (reset) events.Reset(); else events.ClearScene();
                    fresh = Task(null, () => ++freshPolls >= 2);
                    return true;
                });
                var second = Task(null, () => { secondPolls++; return false; });
                polling = true;
                events.LateUpdate();
                Require(first.IsCancelled() && second.IsCancelled() && secondPolls == 1
                    && fresh != null && !fresh.IsCancelled() && freshPolls == 1 && TaskCount() == 1,
                    "clearing from a task callback keeps the new lifetime queue intact: reset=" + reset);
                events.LateUpdate();
                Require(freshPolls == 2 && TaskCount() == 0,
                    "a task created after callback-driven clearing survives until its next poll: reset=" + reset);
            }

            Clean();
            var root = AddEvent(new SchedulingEvent(98020) { step = () => true });
            var sub = new Z_Code.Form.InterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(),
                new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), null, 0, -1, 0, null,
                new List<Z_Code.Form.BoxDataForm.Data>(), root.uid);
            root.subInterpret = sub;
            int subPolls = 0;
            var subTask = Task(sub, () => { subPolls++; return false; });
            events.LateUpdate();
            Require(root.calls == 1 && subTask.IsCancelled() && subPolls == 1 && TaskCount() == 0,
                "normal root-event completion cancels its subprogram task before another poll");

            Clean();
            var oldOwner = Data(98030, Vector3.zero);
            var newOwner = Data(98030, Vector3.one);
            UnitForm.DataByUid.TryGetValue(oldOwner.uid, out var oldUnit);
            try
            {
                UnitForm.DataByUid[oldOwner.uid] = oldOwner;
                var oldRoot = AddEvent(new SchedulingEvent(98031, oldOwner));
                int oldPolls = 0;
                var oldTask = Task(oldRoot, () => { oldPolls++; return false; });
                Form.EventInterpretDataForm.RemoveData(oldRoot.uid);
                UnitForm.DataByUid[newOwner.uid] = newOwner;
                var newRoot = AddEvent(new SchedulingEvent(oldRoot.uid, newOwner));
                int newPolls = 0;
                var newTask = Task(newRoot, () => { newPolls++; return false; });
                cancelEventTasks.Invoke(events, new object[] { oldRoot });
                events.CompleteUnitEvents(oldOwner);
                Require(oldTask.IsCancelled() && !newTask.IsCancelled() && !newRoot.IsCancelled,
                    "root-event and scene-owner indexes use Data identity, not recycled UIDs");
                events.LateUpdate();
                Require(oldPolls == 1 && newPolls == 2 && newRoot.calls == 1 && TaskCount() == 1,
                    "canceling stale owner work leaves the new owner's event and task running");
                events.CompleteUnitEvents(newOwner);
                events.CompleteUnitEvents(newOwner);
                Require(newTask.IsCancelled() && newRoot.IsCancelled,
                    "removing the actual owner immediately marks its indexed event tasks canceled");
                events.LateUpdate();
                Require(TaskCount() == 0 && Form.EventInterpretDataForm.DataByUid.Count == 0,
                    "canceled owner work is released without polling its callback again");
            }
            finally
            {
                if (oldUnit == null) UnitForm.DataByUid.Remove(oldOwner.uid);
                else UnitForm.DataByUid[oldOwner.uid] = oldUnit;
            }

            Clean();
            var owner = Data(98040, Vector3.zero);
            UnitForm.DataByUid.TryGetValue(owner.uid, out var previousOwner);
            try
            {
                UnitForm.DataByUid[owner.uid] = owner;
                bool continued = false;
                AddEvent(new SchedulingEvent(98041, owner));
                AddEvent(new SchedulingEvent(98042, owner));
                var removalOrder = Form.EventInterpretDataForm.DataByUid.Values.Cast<SchedulingEvent>().ToArray();
                var removing = removalOrder[0];
                var sibling = removalOrder[1];
                int removingPolls = 0;
                int siblingPolls = 0;
                var removingTask = Task(removing, () => { removingPolls++; return false; });
                var siblingTask = Task(sibling, () => { siblingPolls++; return false; });
                removing.step = () =>
                {
                    UnitForm.DataByUid.Remove(owner.uid);
                    events.CompleteUnitEvents(owner);
                    continued = !removing.IsCancelled;
                    return false;
                };
                events.LateUpdate();
                Require(continued && removing.calls == 1 && removing.IsCancelled && sibling.calls == 0
                    && sibling.IsCancelled && removingTask.IsCancelled() && siblingTask.IsCancelled()
                    && removingPolls == 1 && siblingPolls == 1 && TaskCount() == 0,
                    "self-removal finishes only its current pass while all owned tasks and sibling continuations stop");
                Require(Form.EventInterpretDataForm.DataByUid.Count == 0,
                    "self-removal releases both snapshot entries without leaving events behind");
            }
            finally
            {
                if (previousOwner == null) UnitForm.DataByUid.Remove(owner.uid);
                else UnitForm.DataByUid[owner.uid] = previousOwner;
            }
        }
        finally
        {
            events.Reset();
            events.sceneTriggerCtrl.Unregister<CollideEvent>();
            events.sceneTriggerCtrl.Unregister<TileEvent>();
            events.sceneTriggerCtrl.Unregister<ItemEvent>();
            events.sceneTriggerCtrl.Unregister<ObjectEvent>();
            events.sceneTriggerCtrl.Unregister<CharacterEvent>();
            events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryItemEvent>();
            events.storyTriggerCtrl.Unregister<StoryCharacterEvent>();
            events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
            if (oldProgress == null) Form.ProgressForm.DataByUid.Remove(1);
            else Form.ProgressForm.DataByUid[1] = oldProgress;
            gameField.SetValue(null, oldGame);
        }
    }

    private static void DestroyCharacters()
    {
        var game = GameManager.instance;
        var play = PlayManager.instance;
        var oldEvents = game.evtCtrl;
        var oldScene = play.sceneCtrl;
        var events = new GameEventController(game);
        game.evtCtrl = events;
        var scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        var characters = new Dictionary<Form.CharacterProductForm.Data, CharacterUnitForm.Data>();
        typeof(PlaySceneController).GetField("_characterDic", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(scene, characters);
        UnitForm.InitInternal();
        CharacterUnitForm.InitInternal();
        Form.CharacterProductForm.InitInternal();
        var prefab = Go("character-removal-prefab");
        Form.EventInterpretDataForm.InitInternal();
        Form.GameCmdDataForm.InitInternal();
        ObjectUnitForm.InitInternal();
        prefab.SetActive(false);
        prefab.AddComponent<CharacterInstance>();
        prefab.AddComponent<MeshRenderer>();
        prefab.AddComponent<BoxCollider>();
        pool.AddPool(prefab);
        var probe = new CharacterRemovalProbe();
        probe.Register<CharacterEvent>();
        probe.Register<SceneActionEvent>();
        var products = new List<Form.CharacterProductForm.Data>();
        var units = new List<CharacterUnitForm.Data>();
        var oldCommands = new Dictionary<string, Z_Code.CmdBase>(Z_Code.BaseData.cmdDic);
        int oldBlock = game.curProgress.blockProgramUid;

        CharacterUnitForm.Data Create(int id, bool unique)
        {
            var product = Form.CharacterProductForm.defaultData;
            product.uid = id;
            product.name = "removal-" + id;
            product.unique = unique;
            Form.CharacterProductForm.AddData(product);
            products.Add(product);
            var unit = new CharacterUnitForm.Data(id + 100, false, Vector3.zero, 1, 1, 1, false, "",
                prefab.name, new Vector3(5, 0, 5), Vector3.zero, Vector3.one, UpdateType.ShowOnly,
                new List<int>(), MapUnit.GetProductInfoString(new JObject(), (product.uid, -1)),
                false, 0, new List<int>());
            CharacterUnitForm.AddData(unit);
            units.Add(unit);
            characters[product] = unit;
            map.updateCtrl.characterTileDic.Add(unit.unit, map.utilCtrl.GetTile(5, 0, 5));
            map.updateCtrl.RefreshCharacterOverlap(unit.unit);
            map.updateCtrl.curCharacterLst.Add(unit);
            unit.unit.Show();
            return unit;
        }

        void Destroy(string handle)
        {
            var data = new Z_Code.Form.InterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(),
                new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), null, 0, -1, 0, null,
                new List<Z_Code.Form.BoxDataForm.Data>(), 0);
            var task = new Z_Code.InterpretAsyncTask(new Z_Code.Interpreter(data));
            new Z_Code.DestroyCharacterCmd().Execute(new[] { Z_Code.CodeHelper.CreateBoxByStr(handle) }, data.heap, task);
            Require(task.IsComplete() && string.IsNullOrEmpty(task.error), "DestroyCharacter completes synchronously without errors");
        }

        try
        {
            foreach (bool sceneReference in new[] { true, false })
            foreach (bool unique in new[] { false, true })
            {
                var unit = Create(97001 + products.Count, unique);
                var product = products.Last();
                var instance = unit.unit.ins;
                string handle = GlobalEventHelper.GetName(sceneReference ? GlobalEventHelper.SCENEOBJECT : GlobalEventHelper.CHARACTER,
                    (sceneReference ? unit.uid : product.uid).ToString());
                Require(scene.GetCharacterUnit(product.uid) == unit && unit.unit.isVising, "live character is resolvable and visible");
                if (sceneReference && unique)
                {
                    typeof(PlaySceneController).GetField("_playerM", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scene, unit);
                    typeof(PlaySceneController).GetField("_playerG", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(scene, product);
                }
                Destroy(handle);
                Require(!instance.gameObject.activeSelf && !instance.vising && unit.unit.ins == null,
                    "both reference forms immediately hide and release the visible character");
                Require(!CharacterUnitForm.DataByUid.ContainsKey(unit.uid) && !UnitForm.DataByUid.ContainsKey(unit.uid)
                    && !map.updateCtrl.curCharacterLst.Contains(unit)
                    && !map.updateCtrl.characterTileDic.TryGet(unit.unit, out _)
                    && !map.updateCtrl.characterOverlapTileDic.TryGet(unit.unit, out _), "removal clears forms and spatial/view indexes");
                Require(Form.CharacterProductForm.DataByUid.ContainsKey(product.uid) == unique,
                    "unique Product survives; non-unique Product is removed");
                Require(scene.GetCharacterUnit(product.uid) == null && !characters.ContainsKey(product), "scene lookup cache is cleared");
                Require(probe.removed.Count(id => id == unit.uid) == 1 && probe.options.Count(id => id == unit.uid) == 1,
                    "one removal immediately clears the interaction option");
                Destroy(handle);
                Require(probe.removed.Count(id => id == unit.uid) == 1, "repeated destruction is a no-op");
            }
            Require(scene.playerM == null && scene.playerG == null, "destroying the current player clears live control references");

            var stale = Create(97020, true);
            var staleProduct = products.Last();
            map.RemoveCharacter(stale);
            var replacement = new CharacterUnitForm.Data(stale.uid, false, Vector3.zero, 1, 1, 1, false, "",
                prefab.name, Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0, new List<int>());
            CharacterUnitForm.AddData(replacement);
            units.Add(replacement);
            Require(scene.GetCharacterUnit(staleProduct.uid) == null, "a recycled UID cannot revive cached Character Data");
            Require(!scene.RemoveCharacter(stale), "scene removal rejects stale Data identity");
            Destroy(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, staleProduct.uid.ToString()));
            Require(ReferenceEquals(CharacterUnitForm.DataByUid[replacement.uid], replacement), "stale references cannot delete a replacement");

            var orphan = Create(97025, false);
            var orphanInstance = orphan.unit.ins;
            Form.CharacterProductForm.RemoveData(products.Last().uid);
            Destroy(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, orphan.uid.ToString()));
            Require(!orphanInstance.gameObject.activeSelf && !CharacterUnitForm.DataByUid.ContainsKey(orphan.uid),
                "a missing Product does not prevent removing its live scene instance");

            var self = Create(97030, false);
            var selfInstance = self.unit.ins;
            Z_Code.BaseData.cmdDic["DestroyCharacter"] = new Z_Code.DestroyCharacterCmd();
            Z_Code.BaseData.cmdDic["Continue"] = new Z_Code.ContinueCmd();
            var program = new Form.EventProgramDataForm.Data(97050, "removal-continue", "", new List<string>(), new List<int>(), 0, "void", 0);
            Require(program.TryApplyCode("DestroyCharacter(param1); Continue(); finished = 1;", out _, out var errors),
                "interaction removal/Continue program compiles: " + string.Join("; ", errors));
            var heap = new Dictionary<string, Z_Code.Form.BoxDataForm.Data>
            {
                ["param1"] = Z_Code.CodeHelper.CreateBoxByStr(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, self.uid.ToString()))
            };
            var sibling = new Form.EventInterpretDataForm.Data(-1, new List<Z_Code.Form.BoxDataForm.Data>(),
                new Dictionary<string, Z_Code.Form.BoxDataForm.Data>(), null, 0, -1, self.uid, null,
                new List<Z_Code.Form.BoxDataForm.Data>(), 0, "onPerSecondEvent", 0) { sceneOwner = self };
            Form.EventInterpretDataForm.AddData(sibling);
            var siblingTask = new Z_Code.InterpretAsyncTask(new Z_Code.Interpreter(sibling));
            siblingTask.Run();
            int polls = 0;
            events.StartTask(siblingTask, () => { polls++; return false; });
            Require(polls == 1, "owned task is initially polled once");
            events.Execute(program, self.uid, heap, "onInteractEvent");
            var executing = Form.EventInterpretDataForm.DataByUid.Values.Single(data => data.releaseTrigger == "onInteractEvent"
                && ReferenceEquals(data.sceneOwner, self));
            Require(executing.user == self.uid && ReferenceEquals(executing.sceneOwner, self),
                "interaction event is owned by the live Character Data");
            game.curProgress.blockProgramUid = executing.uid;
            events.LateUpdate();
            heap.TryGetValue("finished", out var finished);
            Require(!selfInstance.gameObject.activeSelf && finished?.num == 1 && game.curProgress.blockProgramUid == 0,
                $"self-destruction/Continue: active={selfInstance.gameObject.activeSelf}, finished={finished?.num}, block={game.curProgress.blockProgramUid}, event={executing.uid}, pc={executing.p}");
            Require(!Form.EventInterpretDataForm.DataByUid.ContainsKey(executing.uid), "removed Character event completes without rescheduling");
            Require(sibling.IsCancelled && siblingTask.IsCancelled() && polls == 1,
                "other Character events/tasks cancel immediately without another poll");
            events.LateUpdate();
            Require(!Form.EventInterpretDataForm.DataByUid.ContainsKey(sibling.uid), "cancelled sibling event is released on the next scheduler pass");
            var removedCheck = typeof(GameEventSceneTriggerController).GetMethod("IsRemovedSceneUnit", BindingFlags.Static | BindingFlags.NonPublic);
            Require((bool)removedCheck.Invoke(null, new object[] { self.unit }), "deferred collision work rejects removed Characters");

            var nonCharacter = new ObjectUnitForm.Data(Data(97060, Vector3.zero));
            ObjectUnitForm.AddData(nonCharacter);
            try
            {
                Destroy(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, nonCharacter.uid.ToString()));
                Require(ObjectUnitForm.DataByUid.ContainsKey(nonCharacter.uid), "DestroyCharacter ignores Object scene references");
            }
            finally { ObjectUnitForm.RemoveData(nonCharacter.uid); UnitForm.RemoveData(nonCharacter.uid); }
            Destroy("");
            Destroy(GlobalEventHelper.GetName(GlobalEventHelper.SCENEOBJECT, "999999"));
            Destroy(GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, "999999"));
        }
        finally
        {
            foreach (var unit in units)
                if (CharacterUnitForm.DataByUid.TryGetValue(unit.uid, out var registered) && ReferenceEquals(unit, registered))
                    scene.RemoveCharacter(unit);
            foreach (var product in products) Form.CharacterProductForm.RemoveData(product.uid);
            events.Reset();
            game.curProgress.blockProgramUid = oldBlock;
            game.evtCtrl = oldEvents;
            play.sceneCtrl = oldScene;
            Z_Code.BaseData.cmdDic.Clear();
            foreach (var pair in oldCommands) Z_Code.BaseData.cmdDic.Add(pair.Key, pair.Value);
            probe.Unregister<CharacterEvent>(); probe.Unregister<SceneActionEvent>();
            events.sceneTriggerCtrl.Unregister<CollideEvent>();
            events.sceneTriggerCtrl.Unregister<TileEvent>();
            events.sceneTriggerCtrl.Unregister<ItemEvent>();
            events.sceneTriggerCtrl.Unregister<ObjectEvent>();
            events.sceneTriggerCtrl.Unregister<CharacterEvent>();
            events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryLifeEvent>();
            events.storyTriggerCtrl.Unregister<StoryItemEvent>();
            events.storyTriggerCtrl.Unregister<StoryCharacterEvent>();
            events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
        }
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
        var bfs = new ControlledBfs(navigation);
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

    // EditMode's Time.frameCount does not advance while this synchronous fixture
    // runs. Legacy semantic assertions explicitly wait for a completed query;
    // budget assertions use Step/Advance so no production frame guard is bypassed.
    private sealed class ControlledBfs : Z_Map.Analysis.Bfs
    {
        private int frame;
        private readonly Z_Map.Analysis.NavigationEndpointState legacyState =
            new Z_Map.Analysis.NavigationEndpointState();
        protected override int SearchFrame => frame;
        public Z_Map.Analysis.NavigationEndpointState LastEndpointState { get; private set; }

        public ControlledBfs(Z_Map.Analysis.NavigationController navigation) : base(navigation) { }
        public void Advance() { frame++; }

        public Vector3 Step(Vector3 current, Vector3 target, int maxStep, float radius,
            IReadOnlyCollection<int> passTypes, Z_Map.Analysis.NavigationEndpointState state = null)
        {
            LastEndpointState = state;
            return base.GetNextDir(current, target, maxStep, radius, passTypes, state);
        }

        public new Vector3 GetNextDir(Vector3 current, Vector3 target, int maxStep, float radius,
            IReadOnlyCollection<int> passTypes, Z_Map.Analysis.NavigationEndpointState state = null)
        {
            state = state ?? legacyState;
            LastEndpointState = state;
            int previousCompletions = state.completedSearchCount;
            for (int i = 0; i < 20000; i++)
            {
                Advance();
                Vector3 direction = base.GetNextDir(current, target, maxStep, radius, passTypes, state);
                Require(state.searchedCellsThisFrame <= Z_Map.Analysis.Bfs.CellsPerFrame,
                    "every legacy navigation stage respects the ten-cell frame budget");
                if (!state.isSearching || state.completedSearchCount != previousCompletions)
                    return direction;
            }
            throw new InvalidOperationException("incremental navigation failed to complete within 20,000 logical frames");
        }
    }

    private static void IncrementalNavigation()
    {
        var navigation = new Z_Map.Analysis.NavigationController(map)
        {
            navUnits = new Dictionary<(int, int, int), Z_Map.Analysis.NavUnit>(),
            step = 0.5f
        };
        for (int x = 0; x <= 30; x++)
        for (int z = 0; z <= 30; z++)
            navigation.navUnits[(x, 0, z)] = NavUnit(x, z);
        foreach (var node in navigation.navUnits.Values)
        {
            if (navigation.navUnits.TryGetValue((node.pos.x + 1, 0, node.pos.z), out var right)) node.links.Add(right);
            if (navigation.navUnits.TryGetValue((node.pos.x - 1, 0, node.pos.z), out var left)) node.links.Add(left);
            if (navigation.navUnits.TryGetValue((node.pos.x, 0, node.pos.z + 1), out var front)) node.links.Add(front);
            if (navigation.navUnits.TryGetValue((node.pos.x, 0, node.pos.z - 1), out var back)) node.links.Add(back);
        }
        var bfs = new ControlledBfs(navigation);
        var first = new Z_Map.Analysis.NavigationEndpointState();
        var second = new Z_Map.Analysis.NavigationEndpointState();
        Vector3 current = new Vector3(2, 0, 2);
        Vector3 target = new Vector3(28, 0, 28);
        bfs.Advance();
        Vector3 direction = bfs.Step(current, target, 999, .4f, Array.Empty<int>(), first);
        Require(first.isSearching && !first.hasResult && direction == Vector3.zero && first.moveTarget == current,
            "first incomplete search waits at the current position without fabricating a direction");
        Require(first.searchedCellsThisFrame > 0 && first.searchedCellsThisFrame <= 10,
            "large-map search processes at most ten cells in its first frame");
        int firstFrameWork = first.searchedCellsThisFrame;
        for (int i = 0; i < 30; i++) bfs.Step(current, target, 999, .4f, Array.Empty<int>(), first);
        Require(first.searchedCellsThisFrame == firstFrameWork && first.completedSearchCount == 0,
            "repeated calls in one frame cannot consume another ten-cell budget");
        first.Reset();
        direction = bfs.Step(current, target, 999, .4f, Array.Empty<int>(), first);
        Require(first.searchedCellsThisFrame == firstFrameWork && first.searchedCellsThisFrame <= 10
            && first.isSearching && !first.hasResult && direction == Vector3.zero,
            "Reset followed by another query in the same frame cannot replenish its ten-cell budget");
        bfs.Step(target, current, 999, .4f, Array.Empty<int>(), second);
        Require(second.isSearching && !second.hasResult && second.moveTarget == target
            && second.searchedCellsThisFrame > 0 && second.searchedCellsThisFrame <= 10
            && first.searchedCellsThisFrame == firstFrameWork,
            "two characters own independent queues, results and frame budgets in the shared BFS");
        int elapsedFrames = 0;
        while (first.completedSearchCount == 0 && elapsedFrames++ < 20000)
        {
            bfs.Advance();
            direction = bfs.Step(current, target, 999, .4f, Array.Empty<int>(), first);
            bfs.Step(target, current, 999, .4f, Array.Empty<int>(), second);
            Require(first.searchedCellsThisFrame <= 10 && second.searchedCellsThisFrame <= 10,
                "BFS, path backtracking and smoothing never escape the per-character frame budget");
        }
        Require(elapsedFrames > 1 && first.completedSearchCount == 1 && first.hasResult
            && direction != Vector3.zero && !first.isSearching,
            "a multi-frame search publishes its first result only after finishing all phases");
        Vector3 previousWaypoint = first.moveTarget;
        int completed = first.completedSearchCount;
        bfs.Advance();
        Vector3 movedCurrent = current + Vector3.right;
        direction = bfs.Step(movedCurrent, target, 999, .4f, Array.Empty<int>(), first);
        Require(first.isSearching && first.hasResult && first.completedSearchCount == completed
            && first.moveTarget == previousWaypoint
            && Vector3.Dot(direction, (previousWaypoint - movedCurrent).normalized) > .999f,
            "the next unfinished refresh reuses the last completed waypoint relative to the latest position");
        elapsedFrames = 0;
        while (first.completedSearchCount == completed && elapsedFrames++ < 20000)
        {
            bfs.Advance();
            movedCurrent = current + Vector3.right * (elapsedFrames % 2);
            bfs.Step(movedCurrent, target, 999, .4f, Array.Empty<int>(), first);
            Require(first.hasResult && first.searchedCellsThisFrame <= 10,
                "moving across start cells keeps the current search and previous completed result alive");
        }
        Require(first.completedSearchCount == completed + 1,
            "continuous start-cell movement cannot restart and starve the in-progress refresh");

        // The optional-state overload must also retain its own search between
        // frames; resetting a scratch state on every call would never finish.
        bfs.Advance();
        Require(bfs.Step(current, target, 999, .4f, Array.Empty<int>()) == Vector3.zero,
            "state-less callers wait for their first incremental result too");
        int scratchFrames = 0;
        do
        {
            bfs.Advance();
            direction = bfs.Step(current, target, 999, .4f, Array.Empty<int>());
        } while (direction == Vector3.zero && ++scratchFrames < 20000);
        Require(scratchFrames > 1 && direction != Vector3.zero,
            "the optional-state overload retains an independent scratch search across frames");
        bfs.Advance();
        Require(bfs.Step(current, target, 999, .4f, Array.Empty<int>()) != Vector3.zero,
            "state-less callers keep their completed result during the next refresh");

        bfs.Advance();
        completed = first.completedSearchCount;
        bfs.Step(current, target, 999, .4f, Array.Empty<int>(), first);
        previousWaypoint = first.moveTarget;
        Vector3 changedTarget = new Vector3(27, 0, 28);
        bfs.Advance();
        direction = bfs.Step(current, changedTarget, 999, .4f, Array.Empty<int>(), first);
        Require(first.isSearching && first.hasResult && first.completedSearchCount == completed
            && first.moveTarget == previousWaypoint
            && Vector3.Dot(direction, (previousWaypoint - current).normalized) > .999f,
            "a changed ordinary target cell preserves the in-flight query and its previous completed waypoint");
        elapsedFrames = 0;
        while (first.completedSearchCount == completed && elapsedFrames++ < 20000)
        {
            bfs.Advance();
            Vector3 movingTarget = elapsedFrames % 2 == 0 ? target : changedTarget;
            bfs.Step(current, movingTarget, 999, .4f, Array.Empty<int>(), first);
            Require(first.hasResult && first.searchedCellsThisFrame <= 10,
                "a target moving between cells every frame retains the last result and bounded search progress");
        }
        Require(first.completedSearchCount == completed + 1 && first.moveTarget == target,
            "a moving target cannot starve the current round, which finishes using its captured target");
        bfs.GetNextDir(current, changedTarget, 999, .4f, Array.Empty<int>(), first);
        Require(first.moveTarget == changedTarget,
            "the next completed round replaces the old route with the latest target-cell result");
        bfs.Advance();
        direction = bfs.Step(current, changedTarget, 999, 1.2f, Array.Empty<int>(), first);
        Require(first.isSearching && !first.hasResult && direction == Vector3.zero,
            "a physical-radius change invalidates the previous clearance result");
        bfs.GetNextDir(current, changedTarget, 999, 1.2f, Array.Empty<int>(), first);
        bfs.Advance();
        direction = bfs.Step(current, changedTarget, 999, 1.2f, new[] { 7401 }, first);
        Require(first.isSearching && !first.hasResult && direction == Vector3.zero,
            "a changed pass capability invalidates the previous result");
        first.Reset();
        Require(!first.isSearching && !first.hasResult && !first.directEndpoint,
            "stop, teleport and lifecycle reset discard pending and completed navigation state");

        // A blocked start used to run a synchronous whole-graph nearest-cell scan.
        var blocked = navigation.navUnits[(2, 0, 2)];
        blocked.objectBlocked = true;
        bfs.Advance();
        direction = bfs.Step(current, target, 999, .4f, Array.Empty<int>(), first);
        Require(first.isSearching && !first.hasResult && direction == Vector3.zero
            && first.searchedCellsThisFrame <= 10,
            "blocked-start nearest-cell resolution also waits and is capped at ten cells");
        bfs.GetNextDir(current, target, 999, .4f, Array.Empty<int>(), first);
        Require(first.directEndpoint && first.moveTarget == new Vector3(1, 0, 2),
            "incremental escape retains deterministic nearest-anchor selection");
        blocked.objectBlocked = false;

        // Public CanPass must retain arbitrary-call validation, while the BFS's
        // already-adjacent inner path removes redundant checks and enumerator boxing.
        var offsets = navigation.GetClearanceOffsets(1.2f);
        var from = navigation.navUnits[(5, 0, 5)];
        var to = navigation.navUnits[(6, 0, 5)];
        first.Reset();
        for (int i = 0; i < 100; i++) bfs.CanPass(from, to, offsets, Array.Empty<int>());
        long allocated = GC.GetAllocatedBytesForCurrentThread();
        bool allPassed = true;
        for (int i = 0; i < 10000; i++) allPassed &= bfs.CanPass(from, to, offsets, Array.Empty<int>());
        long bytes = GC.GetAllocatedBytesForCurrentThread() - allocated;
        Require(allPassed && bytes == 0,
            "ten thousand warmed nonzero-footprint CanPass checks allocate no enumerator boxes: " + bytes);
        Require(!bfs.CanPass(from, navigation.navUnits[(8, 0, 5)], offsets, Array.Empty<int>()),
            "public CanPass still rejects unrelated cells without a directional edge");
        to.passTypes.Add(7402);
        Require(!bfs.CanPass(from, to, offsets, Array.Empty<int>())
            && bfs.CanPass(from, to, offsets, new[] { 7402 }),
            "optimized CanPass preserves center-only pass-capability checks");
        to.passTypes.Clear();

        var character = Character();
        var characterState = (Z_Map.Analysis.NavigationEndpointState)typeof(CharacterUnit)
            .GetField("navigationEndpointState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(character);
        character.SetPassTypes(new[] { 7401, 7402 });
        bfs.GetNextDir(current, target, 999, .4f, character.passTypes, characterState);
        int characterCompletions = characterState.completedSearchCount;
        character.SetPassTypes(new[] { 7402, 0, 7401, 7401 });
        Require(characterState.hasResult && characterState.completedSearchCount == characterCompletions,
            "reapplying equal unordered pass types with duplicates/zero preserves an active character route");
        character.SetPassTypes(new[] { 7402 });
        Require(!characterState.hasResult && !characterState.isSearching,
            "an actual Character pass-type change cancels cached and pending searches");
        PendingCharacterNavigation(navigation, bfs);
        Debug.Log("MAP_INCREMENTAL_NAVIGATION_PASS frames=" + elapsedFrames + " canPassBytes=" + bytes);
    }

    private static void PendingCharacterNavigation(Z_Map.Analysis.NavigationController navigation, ControlledBfs bfs)
    {
        var floorPrefab = Go("pending-nav-ground");
        var floorCollider = floorPrefab.AddComponent<BoxCollider>();
        floorCollider.center = Vector3.down * .5f;
        floorCollider.size = Vector3.one;
        pool.AddPool(floorPrefab);
        var characterPrefab = Go("pending-nav-character");
        var sphere = characterPrefab.AddComponent<SphereCollider>();
        sphere.center = Vector3.up * .4f;
        sphere.radius = .4f;
        pool.AddPool(characterPrefab);
        var data = new CharacterUnitForm.Data(98521, true, new Vector3(28, 0, 28),
            1, 100, 999, false, "", characterPrefab.name, new Vector3(5, 0, 5),
            Vector3.zero, Vector3.one, UpdateType.Always, new List<int>(), "", false, 0, new List<int>());
        var character = data.unit;
        var instanceGo = Go("pending-nav-character-instance");
        var instance = instanceGo.AddComponent<CharacterInstance>();
        instance.unit = character;
        instance.vising = true;
        character.ins = instance;
        var owner = map.utilCtrl.GetTile(5, 0, 5);
        string oldGroundPrefab = owner.data.prefabName;
        var oldNavigation = map.navigationCtrl;
        bool oldPause = DynamicGlobalSettings.pauseNav;
        var lastFrame = typeof(Unit).GetField("lastUpdateFrame", BindingFlags.Instance | BindingFlags.NonPublic);
        var lastMoveTime = typeof(CharacterUnit).GetField("lastNavMoveTime", BindingFlags.Instance | BindingFlags.NonPublic);
        var avoidPosition = typeof(CharacterUnit).GetField("avoidPos", BindingFlags.Instance | BindingFlags.NonPublic);
        var endpoint = (Z_Map.Analysis.NavigationEndpointState)typeof(CharacterUnit)
            .GetField("navigationEndpointState", BindingFlags.Instance | BindingFlags.NonPublic).GetValue(character);
        try
        {
            map.navigationCtrl = navigation;
            typeof(Z_Map.Analysis.NavigationController).GetField("bfs", BindingFlags.Instance | BindingFlags.NonPublic)
                .SetValue(navigation, bfs);
            DynamicGlobalSettings.pauseNav = false;
            owner.data.prefabName = floorPrefab.name;
            owner.InvalidateCollisionGeometry();
            map.updateCtrl.characterTileDic.Add(character, owner);
            Require((bool)typeof(CharacterUnit).GetMethod("HasGroundContact", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(character, null), "waiting-navigation fixture is grounded independently of gravity or frame delta");
            Vector3 position = data.pos;
            for (int frame = 0; frame < 3; frame++)
            {
                bfs.Advance();
                lastFrame.SetValue(character, Time.frameCount - 1);
                lastMoveTime.SetValue(character, Mathf.Max(0f, Time.time - 2f));
                avoidPosition.SetValue(character, frame == 0 ? 1f : -1f);
                character.UpdateInfo();
                Require(endpoint.isSearching && !endpoint.hasResult && endpoint.moveTarget == position
                    && data.pos == position && data.euler == Vector3.zero,
                    "a real Character update remains stationary while its first search has no direction");
                Require((float)avoidPosition.GetValue(character) == -1f
                    && Mathf.Abs((float)lastMoveTime.GetValue(character) - Time.time) < .0001f,
                    "waiting for an incremental result cancels active sidestep escape and refreshes its stall timer");
            }
        }
        finally
        {
            map.navigationCtrl = oldNavigation;
            DynamicGlobalSettings.pauseNav = oldPause;
            owner.data.prefabName = oldGroundPrefab;
            owner.InvalidateCollisionGeometry();
            map.updateCtrl.characterTileDic.Del(character);
            character.ins = null;
            Object.DestroyImmediate(instanceGo);
        }
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
        var bfs = new ControlledBfs(navigation);
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
        var bfs = new ControlledBfs(map.navigationCtrl);
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
        var bfs = new ControlledBfs(map.navigationCtrl);
        bool ReachedTarget(Z_Map.Analysis.NavUnit target)
        {
            object search = typeof(Z_Map.Analysis.NavigationEndpointState)
                .GetField("searchState", BindingFlags.Instance | BindingFlags.NonPublic)
                .GetValue(bfs.LastEndpointState);
            var predecessors = (Dictionary<Z_Map.Analysis.NavUnit, Z_Map.Analysis.NavUnit>)search.GetType()
                .GetField("pre", BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic).GetValue(search);
            return predecessors.ContainsKey(target);
        }
        void RequireRoute(Vector3 from, Vector3 to, string name)
        {
            Vector3 expected = (to - from).normalized;
            Require(Vector3.Dot(bfs.GetNextDir(from, to, 100, 0.4f, Array.Empty<int>()), expected) > 0.99f,
                name + " navigates from dry bank through bridge to the opposite bank");
            var target = map.utilCtrl.RealPos2MapPosInt(to);
            Require(ReachedTarget(map.navigationCtrl.navUnits[(target.x, target.y, target.z)]),
                name + " actually reaches the opposite bank, not just the closest reachable cell");
            Require(Vector3.Dot(bfs.GetNextDir(to, from, 100, 0.4f, Array.Empty<int>()), -expected) > 0.99f,
                name + " also navigates in the opposite direction");
            target = map.utilCtrl.RealPos2MapPosInt(from);
            Require(ReachedTarget(map.navigationCtrl.navUnits[(target.x, target.y, target.z)]),
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

    private static void NavigationRebuildScheduling()
    {
        var controller = map.navigationCtrl;
        var update = (System.Collections.IEnumerator)typeof(Z_Map.Analysis.NavigationController)
            .GetMethod("UpdateInternal", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(controller, new object[] { 5 });
        var active = typeof(Z_Map.Analysis.NavigationController)
            .GetField("activeFullRebuilds", BindingFlags.Instance | BindingFlags.NonPublic);
        int errors = 0;
        Application.LogCallback countErrors = (message, stack, type) =>
        {
            if (type == LogType.Error || type == LogType.Exception) errors++;
        };
        Application.logMessageReceived += countErrors;
        try
        {
            Require(update.MoveNext() && (int)active.GetValue(controller) == 1,
                "small-batch navigation rebuild starts once and yields");
            controller.UpdateMap(5);
            controller.UpdateMap(int.MaxValue);
            Require((int)active.GetValue(controller) == 1 && errors == 0,
                "periodic updates skip active rebuild rather than starting a concurrent coroutine");
            int frames = 1;
            while (update.MoveNext()) Require(++frames < 10000, "small batches eventually complete");
            Require(frames > 100 && (int)active.GetValue(controller) == 0,
                "small-batch rebuild spans periodic intervals and releases active state on completion");
            RequireLocalNavMatchesFullBuild("small-batch navigation rebuild");
        }
        finally
        {
            (update as IDisposable)?.Dispose();
            Application.logMessageReceived -= countErrors;
        }
    }

    private static void NavigationObstacleOptimization()
    {
        var type = typeof(Z_Map.Analysis.NavigationController);
        var controller = map.navigationCtrl;
        var center = (Func<MeshInfo, Z_Map.Analysis.NavUnit, bool>)Delegate.CreateDelegate(
            typeof(Func<MeshInfo, Z_Map.Analysis.NavUnit, bool>), controller,
            type.GetMethod("IsObjectMeshBlocking", BindingFlags.Instance | BindingFlags.NonPublic));
        var directions = (Func<MeshInfo, Z_Map.Analysis.NavUnit, int>)Delegate.CreateDelegate(
            typeof(Func<MeshInfo, Z_Map.Analysis.NavUnit, int>), controller,
            type.GetMethod("GetObjectBlockingDirections", BindingFlags.Instance | BindingFlags.NonPublic));
        var clip = (Func<Vector3[], int, Vector3[], bool, float, bool, int>)Delegate.CreateDelegate(
            typeof(Func<Vector3[], int, Vector3[], bool, float, bool, int>),
            type.GetMethod("ClipObjectFace", BindingFlags.Static | BindingFlags.NonPublic));
        var a = new Vector3[16]; var b = new Vector3[16];
        int[,] faces = { { 0, 1, 3, 2 }, { 4, 6, 7, 5 }, { 0, 2, 6, 4 },
            { 1, 5, 7, 3 }, { 0, 4, 5, 1 }, { 2, 3, 7, 6 } };
        // Original Box path, with no prepared context or axis-aligned shortcut.
        bool LegacyRect(MeshInfo mesh, Z_Map.Analysis.NavUnit node,
            float minX, float maxX, float minZ, float maxZ)
        {
            var vertices = mesh.positions;
            if ((vertices[1] - vertices[0]).sqrMagnitude == 0f
                || (vertices[4] - vertices[0]).sqrMagnitude == 0f) return false;
            var size = map.data.mainData.mapUnitSize;
            float ground = (node.dirGroundY[0] + node.dirGroundY[1]) * .5f;
            float sx = (node.dirGroundY[0] - node.dirGroundY[1]) / Mathf.Max(Mathf.Abs(size.x) * .5f, .0001f);
            float sz = (node.dirGroundY[2] - node.dirGroundY[3]) / Mathf.Max(Mathf.Abs(size.z) * .5f, .0001f);
            float rise = float.NegativeInfinity;
            float lowX = float.PositiveInfinity, highX = float.NegativeInfinity;
            float lowZ = float.PositiveInfinity, highZ = float.NegativeInfinity;
            foreach (var point in vertices)
            {
                float floor = ground + sx * (point.x - node.realPos.x) + sz * (point.z - node.realPos.z);
                rise = Mathf.Max(rise, point.y - floor);
                lowX = Mathf.Min(lowX, point.x); highX = Mathf.Max(highX, point.x);
                lowZ = Mathf.Min(lowZ, point.z); highZ = Mathf.Max(highZ, point.z);
            }
            if (rise <= .3001f || highX <= minX || lowX >= maxX || highZ <= minZ || lowZ >= maxZ)
                return false;
            for (int face = 0; face < 6; face++)
            {
                for (int i = 0; i < 4; i++) a[i] = vertices[faces[face, i]];
                int count = clip(a, 4, b, true, minX, true);
                count = clip(b, count, a, true, maxX, false);
                count = clip(a, count, b, false, minZ, true);
                count = clip(b, count, a, false, maxZ, false);
                if (count < 3) continue;
                float area = 0f;
                for (int i = 0; i < count; i++)
                {
                    var p = a[i] - node.realPos; var q = a[(i + 1) % count] - node.realPos;
                    area += p.x * q.z - q.x * p.z;
                }
                if (Mathf.Abs(area) <= .0001f * .0001f) continue;
                for (int i = 0; i < count; i++)
                    if (a[i].y - (ground + sx * (a[i].x - node.realPos.x)
                        + sz * (a[i].z - node.realPos.z)) > .3001f) return true;
            }
            return false;
        }
        int LegacyDirections(MeshInfo mesh, Z_Map.Analysis.NavUnit node)
        {
            var size = map.data.mainData.mapUnitSize;
            float hx = Mathf.Abs(size.x) * .5f, hz = Mathf.Abs(size.z) * .5f;
            float x = node.realPos.x, z = node.realPos.z;
            return (LegacyRect(mesh, node, x + hx - .4f, x + hx, z - hz, z + hz) ? 1 : 0)
                | (LegacyRect(mesh, node, x - hx, x - hx + .4f, z - hz, z + hz) ? 2 : 0)
                | (LegacyRect(mesh, node, x - hx, x + hx, z + hz - .4f, z + hz) ? 4 : 0)
                | (LegacyRect(mesh, node, x - hx, x + hx, z - hz, z - hz + .4f) ? 8 : 0);
        }
        var random = new System.Random(1012);
        float Next(float low, float high) => low + (float)random.NextDouble() * (high - low);
        var node = new Z_Map.Analysis.NavUnit { dirGroundY = new float[4] };
        var originalSize = map.data.mainData.mapUnitSize;
        try
        {
            foreach (var cellSize in new[] { Vector3.one, new Vector3(.5f, 1, .5f), new Vector3(2, 1, 3) })
            {
                map.data.mainData.mapUnitSize = cellSize;
                for (int i = 0; i < 1200; i++)
                {
                    node.realPos = i % 2 == 0 ? Vector3.zero : new Vector3(640, 750, 640);
                    for (int d = 0; d < 4; d++) node.dirGroundY[d] = node.realPos.y;
                    if (i % 3 == 2) { node.dirGroundY[0] += .17f; node.dirGroundY[1] -= .17f; }
                    var rotation = i % 3 == 0 ? Vector3.zero : new Vector3(Next(-70, 70), Next(-180, 180), Next(-70, 70));
                    var size = new Vector3(Next(.001f, 2), i % 11 == 0 ? 0f : Next(.001f, 2), Next(.001f, 2));
                    var mesh = Z_Mesh.Mesh.GetMesh(node.realPos + new Vector3(Next(-1.5f, 1.5f), Next(-.5f, 1), Next(-1.5f, 1.5f)), rotation, size);
                    float x = node.realPos.x, z = node.realPos.z;
                    Require(center(mesh, node) == LegacyRect(mesh, node, x - .4f, x + .4f, z - .4f, z + .4f),
                        "optimized center matches original clipping at case " + i + " / " + cellSize);
                    Require(directions(mesh, node) == LegacyDirections(mesh, node),
                        "optimized edges match original clipping at case " + i + " / " + cellSize);
                }
            }
            map.data.mainData.mapUnitSize = Vector3.one;
            node.realPos = Vector3.zero; Array.Clear(node.dirGroundY, 0, 4);
            var benchmark = Z_Mesh.Mesh.GetMesh(new Vector3(.4f, .5f, .4f), Vector3.zero, new Vector3(.1f, 1, .1f));
            for (int i = 0; i < 50; i++) { center(benchmark, node); directions(benchmark, node); LegacyDirections(benchmark, node); }
            int oldSum = 0, newSum = 0;
            var timer = System.Diagnostics.Stopwatch.StartNew();
            for (int i = 0; i < 10000; i++) oldSum += (LegacyRect(benchmark, node, -.4f, .4f, -.4f, .4f) ? 16 : 0) + LegacyDirections(benchmark, node);
            double oldMs = timer.Elapsed.TotalMilliseconds;
            timer.Restart();
            for (int i = 0; i < 10000; i++) newSum += (center(benchmark, node) ? 16 : 0) + directions(benchmark, node);
            double newMs = timer.Elapsed.TotalMilliseconds;
            Require(oldSum == newSum, "benchmark retains identical center/edge flags");
            Debug.Log($"MAP_NAV_OBSTACLE_BENCHMARK legacy_ms={oldMs:F2} optimized_ms={newMs:F2} iterations=10000");
        }
        finally { map.data.mainData.mapUnitSize = originalSize; }
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
            var bfs = new ControlledBfs(controller);
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
            var bfs = new ControlledBfs(controller);
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
        var bfs = new ControlledBfs(map.navigationCtrl);
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
        var originalBindings = ins.viewRefreshBindings;
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
            Require(root.transform.childCount == 2 && ReferenceEquals(ins.viewRefreshBindings, originalBindings)
                && ins.viewRefreshBindings.Count == 1 && objectPrefab.transform.childCount == 1,
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
        MissionMapMarkers(mission);
    }

    // Render marker visibility without loading authored prefabs/fonts. The
    // production controllers still perform their real filtering, Clear/Add and
    // pending-refresh paths; ordinary map-mark containers are deliberately absent.
    public sealed class MissionMarkerContainer<T> : Z_Ui.Base.UiContainer<T>
        where T : Z_Ui.Base.UiCtrl, new()
    {
        private readonly GameObject marker;
        public int refreshCount;
        public MissionMarkerContainer(Z_Ui.Base.UiCtrl parent, GameObject marker) : base(parent, marker)
        { this.marker = marker; }
        public override void Refresh(List<Vector3> offsets = null)
        {
            refreshCount++;
            marker.SetActive(paramLst.Count != 0);
        }
    }

    private static void MissionMapMarkers(Form.MissionForm.Data mission)
    {
        var play = PlayManager.instance;
        var progress = GameManager.instance.curProgress;
        var oldMap = play.mapCtrl;
        var oldEnable = play.enable;
        var oldMissionId = progress.curMissionId;
        var oldSeconds = progress.seconds;
        var oldEnableMission = progress.enableMission;
        var oldShow = mission.show;
        var oldReceived = mission.received;
        var oldDone = mission.done;
        var oldFail = mission.fail;
        var oldTargetScene = mission.targetSceneId;
        var mapCtrl = new PlayMapController(play);
        play.mapCtrl = mapCtrl;
        progress.seconds = 0;
        progress.enableMission = true;
        mission.show = mission.received = true;
        mission.done = mission.fail = false;
        mission.targetSceneId = GameManager.instance.curScene.uid;
        progress.curMissionId = mission.id;

        var areaRoot = Go("mission-area-owner");
        var areaHolder = areaRoot.AddComponent<Z_Ui.Base.UiHolder>();
        var areaView = (Ui.PlayMap.UiPlayMapView)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Ui.PlayMap.UiPlayMapView));
        areaView.go_self = Go("mission-area-self");
        var area = new Ui.PlayMap.UiPlayMapCtrl
        {
            uiHolder = areaHolder, inited = true, view = areaView, mapCtrl = mapCtrl,
            model = new Ui.PlayMap.UiPlayMapModel { isArea = true,
                y = map.utilCtrl.RealPos2MapPosInt(MissionGuide.GetTargetWorldPosition(mission)).y }
        };
        var areaMarker = Go("mission-area-marker");
        var areaCon = new MissionMarkerContainer<Ui.PlayMap.UiMissionCtrl>(area, areaMarker);
        typeof(Ui.PlayMap.UiPlayMapCtrl).GetField("missionCon", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(area, areaCon);
        var areaRefresh = (Action<bool>)typeof(Ui.PlayMap.UiPlayMapCtrl)
            .GetMethod("RefreshMissionIfNeeded", BindingFlags.Instance | BindingFlags.NonPublic)
            .CreateDelegate(typeof(Action<bool>), area);

        var miniRoot = Go("mission-minimap-owner");
        var miniHolder = miniRoot.AddComponent<Z_Ui.Base.UiHolder>();
        var miniView = (Ui.PlaySceneMain.PlaySceneMinimap.UiPlaySceneMinimapView)System.Runtime.Serialization.FormatterServices
            .GetUninitializedObject(typeof(Ui.PlaySceneMain.PlaySceneMinimap.UiPlaySceneMinimapView));
        miniView.rtf_guide = Go("mission-minimap-guide").AddComponent<RectTransform>();
        var mini = new Ui.PlaySceneMain.PlaySceneMinimap.UiPlaySceneMinimapCtrl
        { uiHolder = miniHolder, inited = true, view = miniView, mapCtrl = mapCtrl };
        var miniMarker = Go("mission-minimap-marker");
        var miniCon = new MissionMarkerContainer<Ui.PlaySceneMain.PlaySceneMinimap.UiMissionCtrl>(mini, miniMarker);
        typeof(Ui.PlaySceneMain.PlaySceneMinimap.UiPlaySceneMinimapCtrl)
            .GetField("missionCon", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(mini, miniCon);
        var miniRefresh = (Action<bool>)typeof(Ui.PlaySceneMain.PlaySceneMinimap.UiPlaySceneMinimapCtrl)
            .GetMethod("RefreshMissionIfNeeded", BindingFlags.Instance | BindingFlags.NonPublic)
            .CreateDelegate(typeof(Action<bool>), mini);

        // Register UI first to verify pending refresh sees the final selection,
        // even when PlayInfo is later in the same synchronous event dispatch.
        play.enable = false;
        area.OnShow();
        mini.OnShow();
        var info = new PlayInfoController(play);
        typeof(PlayInfoController).GetField("enable", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(info, true);
        var replacement = new Form.MissionForm.Data(29004, "marker replacement", 0, "", true, true, false, false,
            mission.targetSceneId, mission.targetPos, 0);
        void RefreshPending() { areaRefresh(false); miniRefresh(false); }
        void RequireHidden(string reason)
        {
            Require(!areaMarker.activeSelf && !miniMarker.activeSelf
                && areaCon.paramLst.Count == 0 && miniCon.paramLst.Count == 0, reason);
            Require(!miniView.rtf_guide.gameObject.activeSelf, reason + " clears guide arrow");
        }
        void Complete()
        {
            typeof(Z_Code.CompleteMissionCmd).GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic)
                .Invoke(new Z_Code.CompleteMissionCmd(), new object[] {
                    new[] { Z_Code.CodeHelper.CreateBoxByStr(mission.name) }, null });
        }
        try
        {
            play.enable = true;
            RefreshPending();
            Require(areaMarker.activeSelf && miniMarker.activeSelf, "active received mission renders on both maps");
            miniView.rtf_guide.gameObject.SetActive(true);
            Complete();
            Require(mission.done && progress.curMissionId == 0, "DoneMission clears the final current mission ID");
            RefreshPending();
            RequireHidden("DoneMission removes both markers without advancing game time");
            Require(progress.seconds == 0, "mission event refresh does not advance game seconds");
            int areaCount = areaCon.refreshCount, miniCount = miniCon.refreshCount;
            RefreshPending();
            Require(areaCon.refreshCount == areaCount && miniCon.refreshCount == miniCount,
                "unchanged paused UI frames do not repeatedly rebuild mission markers");

            // A stale ID loaded from an old save cannot resurrect a done marker.
            progress.curMissionId = mission.id;
            areaRefresh(true); miniRefresh(true);
            RequireHidden("stale completed current ID is filtered on periodic/reopen refresh");

            mission.done = false;
            progress.curMissionId = mission.id;
            Form.MissionForm.AddData(replacement);
            Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Select, data = mission });
            RefreshPending();
            Complete();
            Require(progress.curMissionId == replacement.id, "completed selection changes to another unfinished received mission");
            RefreshPending();
            Require(((Ui.PlayMap.UiMissionParam)areaCon.paramLst.Single()).data == replacement
                && ((Ui.PlaySceneMain.PlaySceneMinimap.UiMissionParam)miniCon.paramLst.Single()).data == replacement,
                "both maps render the replacement after all event listeners have completed");
            Form.MissionForm.RemoveData(replacement.id);
            progress.curMissionId = mission.id;
            mission.done = false;
            mission.fail = true;
            miniView.rtf_guide.gameObject.SetActive(true);
            Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Fail, data = mission });
            RefreshPending();
            Require(progress.curMissionId == 0, "failed final current mission also clears its selection");
            RequireHidden("failed missions leave no marker or guide");

            mission.fail = false;
            progress.curMissionId = mission.id;
            foreach (var state in new[] { "hidden", "unreceived", "disabled", "other scene" })
            {
                mission.show = state != "hidden";
                mission.received = state != "unreceived";
                progress.enableMission = state != "disabled";
                mission.targetSceneId = state == "other scene" ? 999 : oldTargetScene;
                areaRefresh(true); miniRefresh(true);
                RequireHidden(state + " mission is not rendered");
            }
            mission.show = mission.received = progress.enableMission = true;
            mission.targetSceneId = oldTargetScene;
            area.model.y++;
            areaRefresh(true); miniRefresh(true);
            Require(!areaMarker.activeSelf && miniMarker.activeSelf, "area map still filters mission height independently");

            area.OnHide(); mini.OnHide();
            RefreshPending();
            areaCount = areaCon.refreshCount; miniCount = miniCon.refreshCount;
            Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Select, data = mission });
            RefreshPending();
            Require(areaCon.refreshCount == areaCount && miniCon.refreshCount == miniCount,
                "hidden map owners unregister mission events");
            play.enable = false;
            area.OnShow(); mini.OnShow();
            RefreshPending();
            areaCount = areaCon.refreshCount; miniCount = miniCon.refreshCount;
            Z_EventHelper.Invoke(new MissionEvent { type = MissionEventType.Select, data = mission });
            RefreshPending();
            Require(areaCon.refreshCount == areaCount + 1 && miniCon.refreshCount == miniCount + 1,
                "reopening restores exactly one mission event refresh per owner");
        }
        finally
        {
            area.OnHide(); mini.OnHide();
            info.Unregister<MissionEvent>(); info.Unregister<CollideEvent>();
            Form.MissionForm.RemoveData(replacement.id);
            play.mapCtrl = oldMap; play.enable = oldEnable;
            progress.curMissionId = oldMissionId; progress.seconds = oldSeconds; progress.enableMission = oldEnableMission;
            mission.show = oldShow; mission.received = oldReceived; mission.done = oldDone;
            mission.fail = oldFail; mission.targetSceneId = oldTargetScene;
        }
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

    private static void ReturnKeywordDiagnostics()
    {
        const string commandName = "GetCharacterParameter";
        bool hadPrevious = Z_Code.BaseData.cmdDic.TryGetValue(commandName, out var previous);
        Z_Code.BaseData.cmdDic[commandName] = new Z_Code.GetCharacterParameterCmd();
        try
        {
            const string source = "if (GetCharacterParameter(param1, \"Team\") == 1&&GetCharacterParameter(param2, \"Team\") == 0)\n"
                + "{\nreturn 1;\n}\nelse\n{\n"
                + "if (GetCharacterParameter(param1, \"Team\") == 0&&GetCharacterParameter(param2, \"Team\") == 1)\n"
                + "{\nreturn 1;\n}\n}\nreturn 0;";
            var compiler = new Z_Code.Compiler();
            Require(!compiler.TryCompile(source, out var invalidCode, out _, out _, out _, out _, out var errors)
                && invalidCode.Count == 0 && errors.Count == 3,
                "lowercase return remains a variable and produces three diagnostics without executable code");
            Require(errors[0].LineNumber == 3 && errors[0].ColumnNumber == 8
                && errors[1].LineNumber == 9 && errors[2].LineNumber == 12,
                "all three semicolon diagnostics point at the number after lowercase return");
            var corrected = source.Replace("return", "Return");
            Require(compiler.TryCompile(corrected, out var code, out _, out var parameterCount,
                    out var returnType, out var codeMap, out errors) && errors.Count == 0,
                "capitalizing only Return compiles the original nested if/else and compact && condition");
            Require(parameterCount == 2 && returnType == "num" && code.Count == codeMap.Count,
                "corrected Team condition preserves its parameter, return and source-map contracts");
        }
        finally
        {
            if (hadPrevious) Z_Code.BaseData.cmdDic[commandName] = previous;
            else Z_Code.BaseData.cmdDic.Remove(commandName);
        }
    }

    private static void CompilerMethodDiagnostics()
    {
        const string alias = "RegressionLenAlias";
        const string name = "RegressionCustomMethod";
        var commands = Z_Code.BaseData.cmdDic;
        var programs = Z_Code.Form.ProgramDataForm.DataByName;
        bool hadCommand = commands.TryGetValue(alias, out var previousCommand);
        bool hadProgram = programs.TryGetValue(name, out var previousProgram);
        var program = new Z_Code.Form.ProgramDataForm.Data(-1, name, "Return param1;",
            new List<string>(), new List<int>(), 1, "var");
        commands[alias] = new Z_Code.LenCmd();
        programs[name] = program;
        try
        {
            var compiler = new Z_Code.Compiler();
            Require(program.TryApplyCode($"Return {name}(param1);", out _, out var errors),
                "registered custom method accepts a self/forward call");
            Require(compiler.TryCompile($"{alias}();Return {name}(1);", out _, out _, out _,
                out _, out _, out errors), "registered Cmd alias and custom method compile");
            const string source = "if(0){\r\n  MissingOuter(MissingInner());\r\n}\r\nReturn 0&&MissingRight();";
            Require(!compiler.TryCompile(source, out var code, out _, out _, out _, out var codeMap, out errors)
                && code.Count == 0 && codeMap.Count == 0 && errors.Count == 3,
                "all nested/unreachable missing calls fail without executable code");
            var names = new[] { "MissingOuter", "MissingInner", "MissingRight" };
            for (int i = 0; i < names.Length; i++)
                Require(errors[i].Stage == "方法检查" && errors[i].Message.Contains(names[i])
                    && errors[i].Message.Contains("不存在")
                    && errors[i].StartIndex == source.IndexOf(names[i], StringComparison.Ordinal)
                    && errors[i].EndIndex == errors[i].StartIndex + names[i].Length - 1,
                    "method diagnostics name the exact identifier in source order");
            Require(errors[0].LineNumber == 2 && errors[0].ColumnNumber == 3
                && errors[2].LineNumber == 4, "method diagnostics retain CRLF line/column locations");
            var oldCode = program.code;
            var oldZCode = program.zCode;
            var oldMap = program.zCodeMap;
            Require(!program.TryApplyCode("MissingApply();", out _, out errors)
                && program.code == oldCode && ReferenceEquals(program.zCode, oldZCode)
                && ReferenceEquals(program.zCodeMap, oldMap) && program.paramCount == 1,
                "missing method does not overwrite the last valid program or signature");
            Require(compiler.TryCompile("owner.Missing=1;value=\"Missing()\";Return owner.Missing;",
                out _, out _, out _, out _, out _, out errors), "strings/member names are not method calls");
            Require(!compiler.TryCompile($"{alias.ToLowerInvariant()}();", out _, out _, out _,
                out _, out _, out errors) && errors.Count == 1 && errors[0].Message.Contains("不存在"),
                "method lookup remains case-sensitive");
        }
        finally
        {
            if (hadCommand) commands[alias] = previousCommand;
            else commands.Remove(alias);
            if (hadProgram) programs[name] = previousProgram;
            else programs.Remove(name);
        }
    }

    private static void TimedNotifyComments()
    {
        var remove = typeof(Ui.Notify.UiNotifyCtrl).GetMethod("RemoveExpiredComments",
            BindingFlags.Static | BindingFlags.NonPublic);
        var permanent = new Z_Ui.Notify.CommentInfo { id = 1 };
        var first = new Z_Ui.Notify.CommentInfo { id = 2, expiresAt = 105f };
        var second = new Z_Ui.Notify.CommentInfo { id = 3, expiresAt = 108f };
        var comments = new List<Z_Ui.Notify.CommentInfo> { permanent, first, second };
        Require(!(bool)remove.Invoke(null, new object[] { comments, 104.99f }) && comments.Count == 3,
            "five-second diagnostic remains until its actual expiry");
        Require((bool)remove.Invoke(null, new object[] { comments, 105f })
            && comments.Count == 2 && comments.Contains(permanent) && comments.Contains(second),
            "expiry removes only its diagnostic at exactly five seconds");
        Require(!(bool)remove.Invoke(null, new object[] { comments, 105f }),
            "expired diagnostic cannot repeatedly refresh or close a reused Comment");
        comments.Remove(second); // Same removal as manually closing before its deadline.
        Require(!(bool)remove.Invoke(null, new object[] { comments, 108f }) && comments[0] == permanent,
            "manual dismissal cancels expiry naturally and leaves ordinary Comments persistent");
        comments.Add(new Z_Ui.Notify.CommentInfo { id = 4, expiresAt = 110f });
        comments.Add(new Z_Ui.Notify.CommentInfo { id = 5, expiresAt = 110f });
        Require((bool)remove.Invoke(null, new object[] { comments, 111f }) && comments.Count == 1,
            "multiple expired diagnostics are removed safely without modifying a foreach enumeration");
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
            ElevatedCoveredObjectOcclusion();
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
        for (int i = 0; i < instance.viewRefreshBindings.Count; i++)
        {
            block.Clear();
            block.SetFloat("_RegressionTextureProperty", i + 10);
            block.SetFloat("_LightSensitivity", i + 2);
            instance.viewRefreshBindings[i].Target.SetPropertyBlock(block);
        }

        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        map.updateCtrl.curCenterPos = new Vector3(5, 0, 5);
        map.updateCtrl.curTileLst.Add(tile.data);
        var update = typeof(MapUpdateController).GetMethod("UpdateVision", BindingFlags.Instance | BindingFlags.NonPublic);
        update.Invoke(map.updateCtrl, null);
        Require(ReadVisionDegree(instance, 0) == 0f, "high Tile occlusion is fully transparent");
        int reads = instance.bindingReads;
        update.Invoke(map.updateCtrl, null);
        Require(instance.bindingReads == reads, "unchanged frame skips binding/renderer submissions");

        instance.displayLayer = 0;
        update.Invoke(map.updateCtrl, null);
        for (int i = 0; i < 6; i++)
        {
            instance.viewRefreshBindings[i].Target.GetPropertyBlock(block);
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
            instance.viewRefreshBindings[i].Target.GetPropertyBlock(block);
            bool displayed = i == 0 || i == 3;
            Require(block.GetFloat("_Show") == (displayed ? .5f : 0f)
                && block.GetFloat("_FadeCenter") == 0f,
                "semi-transparent slots submit uniform opacity with center fading disabled");
            Require(block.GetFloat("_RegressionTextureProperty") == i + 10,
                "opacity submission preserves unrelated renderer properties");
        }
        reads = instance.bindingReads;
        instance.ApplyVision(0.5f);
        Require(instance.bindingReads == reads, "unchanged opacity skips renderer submissions");
        instance.ApplyVision(0.5f, 1f);
        Require(ReadVisionDegree(instance, 0) == .5f && ReadVisionDegree(instance, 3) == 1f,
            "kept-visible front restores only its own opacity");
        instance.ApplyVision(1f, 0.5f);
        Require(ReadVisionDegree(instance, 0) == 1f && ReadVisionDegree(instance, 3) == .5f,
            "normal and front submit independent uniform opacities");
        instance.ApplyVision(1f);
        Require(ReadVisionDegree(instance, 0) == 1f && ReadVisionDegree(instance, 3) == 1f,
            "ordinary visibility restores both opacities");
        instance.ApplyVision(0.5f);
        go.SetActive(false);
        instance.ClearRendererProperties();
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
        instance.viewRefreshBindings[index].Target.GetPropertyBlock(block);
        float show = block.GetFloat("_Show");
        Require(block.GetFloat("_FadeCenter") == 0f, "map visibility always disables center fading");
        return show;
    }

    private static float ReadLightSensitivity(MapInstance instance, int index)
    {
        var block = new MaterialPropertyBlock();
        instance.viewRefreshBindings[index].Target.GetPropertyBlock(block);
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

        int objectReads = objIns.bindingReads, itemReads = itemIns.bindingReads;
        for (int i = 0; i < 10; i++) update();
        long bytesBefore = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 100; i++) update();
        long allocated = GC.GetAllocatedBytesForCurrentThread() - bytesBefore;
        Require(allocated == 0, "warm tile-first visibility allocates no managed memory: " + allocated);
        Require(objIns.bindingReads == objectReads && itemIns.bindingReads == itemReads,
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

        visible.Clear();
        showAndAdd.Invoke(ctrl, new object[] { visible, 14, 15, 1, 2, 14, 15 });
        Require(visible.Count == 0, "single-layer exact lookup skips a sparse middle gap");
        showAndAdd.Invoke(ctrl, new object[] { visible, 14, 15, 2, 3, 14, 15 });
        Require(visible.SetEquals(new[] { column[2] }), "single-layer exact lookup selects only the requested height");
        var related = (Action<TileUnit>)Delegate.CreateDelegate(typeof(Action<TileUnit>), ctrl,
            typeof(MapUpdateController).GetMethod("UpdateRelatedUnit", BindingFlags.Instance | BindingFlags.NonPublic));
        int objectKeys = ctrl.objectTileDic.GetDicT2().Count;
        int itemKeys = ctrl.itemTileDic.GetDicT2().Count;
        int characterKeys = ctrl.characterTileDic.GetDicT2().Count;
        for (int i = 0; i < 10; i++)
        {
            related(column[0].unit);
            related(column[2].unit);
        }
        Require(ctrl.objectTileDic.GetDicT2().Count == objectKeys
            && ctrl.itemTileDic.GetDicT2().Count == itemKeys
            && ctrl.characterTileDic.GetDicT2().Count == characterKeys,
            "related-unit visibility reads never add empty reverse-index keys");

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

    private static void ViewRefreshPoolReuse()
    {
        // The probe explicitly opts in to the lower-level Instance contract.
        // Real TileInstance whitelist/appearance coverage lives in the texture
        // fixture; adding this probe to a Tile would correctly reject reuse.
        var parent = Go("view-reuse-active-parent");
        var template = Go("view-reuse-probe-prefab");
        template.SetActive(false);
        template.AddComponent<ViewRefreshReuseProbe>();
        template.AddComponent<BoxCollider>();
        var otherTemplate = Go("view-reuse-other-prefab");
        otherTemplate.SetActive(false);
        otherTemplate.AddComponent<ViewRefreshReuseProbe>();
        var instancePool = new InstancePool(template, parent.transform);
        var otherPool = new InstancePool(otherTemplate, parent.transform);
        pool.Register(instancePool);
        pool.Register(otherPool);
        try
        {
            var oldInactive = pool.CreateInstance(template);
            var live = pool.CreateInstance(template);
            var other = pool.CreateInstance(otherTemplate);
            var probe = live.GetComponent<ViewRefreshReuseProbe>();
            var body = live.GetComponent<BoxCollider>();
            pool.DeleteInstance(oldInactive, template);
            Require(!oldInactive.activeSelf, "ordinary pool returns still deactivate immediately");
            int enables = probe.enables, disables = probe.disables;
            var oldOwner = new Unit(Data(680001, Vector3.zero));
            var newOwner = new Unit(Data(680002, Vector3.right));
            probe.unit = oldOwner;
            oldOwner.ins = probe;
            using (pool.BeginViewRefreshReuse())
            {
                pool.DeleteInstance(live, template);
                pool.DeleteInstance(live, template);
                Require(live.activeInHierarchy && probe.unit == null && oldOwner.ins == null && probe.prepares == 1,
                    "view return detaches the owner and prepares while the hierarchy remains active");
                Require(!body.enabled, "pending ownerless hierarchy cannot participate in physics queries");
                var reused = pool.CreateInstance(template);
                Require(ReferenceEquals(reused, live) && !oldInactive.activeSelf,
                    "same-prefab active pending instance wins over an older inactive FIFO entry");
                Require(body.enabled, "active reuse restores the original Collider enabled state");
                probe.unit = newOwner;
                Require(probe.enables == enables && probe.disables == disables,
                    "same-refresh handoff invokes neither OnDisable nor OnEnable");
                for (int i = 0; i < 4; i++)
                {
                    pool.DeleteInstance(live, template);
                    Require(ReferenceEquals(pool.CreateInstance(template), live), "repeated active handoff retains instance identity");
                    probe.unit = newOwner;
                }
                Require(probe.enables == enables && probe.disables == disables && probe.prepares == 5,
                    "each handoff prepares once without accumulating native activation callbacks");
            }
            Require(live.activeInHierarchy && probe.unit == newOwner,
                "scope completion does not disable a checked-out replacement owner");

            using (pool.BeginViewRefreshReuse())
            {
                pool.DeleteInstance(live, template);
                pool.DeleteInstance(other, otherTemplate);
                Require(ReferenceEquals(pool.CreateInstance(otherTemplate), other),
                    "pending reuse never crosses prefab pools");
                using (pool.BeginViewRefreshReuse())
                    Require(live.activeInHierarchy, "nested scope keeps an outer pending return active");
                Require(live.activeInHierarchy, "disposing an inner scope does not flush the outer transaction");
            }
            Require(!live.activeSelf && other.activeSelf && probe.disables == disables + 1,
                "outer scope deactivates only unused returns and does so once");
            Require(body.enabled, "unused return restores its Collider baseline only after deactivation");

            live = pool.CreateInstance(template);
            try
            {
                using (pool.BeginViewRefreshReuse())
                {
                    pool.DeleteInstance(live, template);
                    throw new ViewRefreshExpectedException();
                }
            }
            catch (ViewRefreshExpectedException) { }
            Require(!live.activeSelf, "exceptional view exit still flushes pending active instances");

            var fallback = pool.CreateInstance(template);
            fallback.GetComponent<ViewRefreshReuseProbe>().supportsReuse = false;
            using (pool.BeginViewRefreshReuse())
            {
                pool.DeleteInstance(fallback, template);
                Require(!fallback.activeSelf, "non-opted-in custom instances retain immediate disable callbacks");
            }

            var active = pool.CreateInstance(otherTemplate);
            using (pool.BeginViewRefreshReuse())
            {
                pool.DeleteInstance(active, otherTemplate);
                otherPool.Clear();
                Require(!active.activeSelf, "pool Clear flushes pending returns inside an open scope");
            }
            var again = pool.CreateInstance(otherTemplate);
            Require(again.activeSelf, "pool remains usable after clearing a pending view transaction");
            using (pool.BeginViewRefreshReuse())
            {
                pool.DeleteInstance(again, otherTemplate);
                otherPool.Destroy();
                Require(!again.activeSelf, "pool Destroy deactivates pending returns before deferred destruction");
            }
        }
        finally
        {
            instancePool.Clear();
            otherPool.Clear();
            pool.Unregister(instancePool);
            pool.Unregister(otherPool);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(template);
            Object.DestroyImmediate(otherTemplate);
        }
    }

    private sealed class ViewRefreshExpectedException : Exception { }

    private sealed class ViewRefreshThrowingPool : InstancePool
    {
        public bool throwOnFresh;
        public ViewRefreshThrowingPool(GameObject prefab, Transform parent) : base(prefab, parent) { }
        public override void Fresh(GameObject obj)
        {
            if (throwOnFresh) throw new ViewRefreshExpectedException();
            base.Fresh(obj);
        }
    }

    private static void ViewRefreshExceptionalLifecycle()
    {
        // This subset runs in native Play mode because Destroy is intentionally
        // deferred in production, never changed to DestroyImmediate for a test.
        var localGo = Go("view-reuse-exceptional-manager");
        localGo.SetActive(false);
        var manager = localGo.AddComponent<InstancePoolManager>();
        manager.Init();
        var parent = Go("view-reuse-exceptional-parent");
        var template = Go("view-reuse-exceptional-template");
        template.SetActive(false);
        template.AddComponent<ViewRefreshReuseProbe>();
        template.AddComponent<BoxCollider>();
        var instancePool = new ViewRefreshThrowingPool(template, parent.transform);
        manager.Register(instancePool);
        try
        {
            var live = manager.CreateInstance(template);
            using (manager.BeginViewRefreshReuse())
            {
                manager.DeleteInstance(live, template);
                instancePool.throwOnFresh = true;
                bool threw = false;
                try { manager.CreateInstance(template); }
                catch (ViewRefreshExpectedException) { threw = true; }
                Require(threw && !live.activeSelf && live.GetComponent<BoxCollider>().enabled,
                    "failed pending Fresh closes the root and restores its Collider baseline");
                instancePool.throwOnFresh = false;
                Require(ReferenceEquals(manager.CreateInstance(template), live),
                    "failed pending Fresh returns exactly once to the ordinary pool");
            }

            var oldScope = manager.BeginViewRefreshReuse();
            manager.DeleteInstance(live, template);
            manager.Clear();
            Require(!live.activeSelf && manager.pools.Count == 0,
                "manager Clear flushes and retires pending instances even while a scope is outstanding");
            var replacementPool = new InstancePool(template, parent.transform);
            manager.Register(replacementPool);
            var newScope = manager.BeginViewRefreshReuse();
            var newLive = manager.CreateInstance(template);
            manager.DeleteInstance(newLive, template);
            oldScope.Dispose();
            Require(newLive.activeInHierarchy,
                "disposing a pre-Clear scope cannot flush a newer scene transaction");
            var duplicate = newScope;
            newScope.Dispose();
            duplicate.Dispose();
            Require(!newLive.activeSelf, "copied scope disposal flushes once and is idempotent");
            manager.Clear();
        }
        finally
        {
            manager.Clear();
            Object.DestroyImmediate(localGo);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(template);
            typeof(InstancePoolManager).GetField("initializedManager", BindingFlags.Static | BindingFlags.NonPublic)
                .SetValue(null, pool);
        }
    }

    private static void FreshMapActiveTileHandoff()
    {
        var ctrl = map.updateCtrl;
        ctrl.End();
        var oldSize = map.data.mainData.viewSize;
        map.data.mainData.viewSize = new Vector3Int(2, 1, 1);
        var parent = Go("fresh-map-active-tile-parent");
        var template = Go("fresh-map-active-tile-template");
        template.SetActive(false);
        template.AddComponent<MeshRenderer>();
        template.AddComponent<BoxCollider>();
        template.AddComponent<TileInstance>();
        var instancePool = new InstancePool(template, parent.transform);
        pool.Register(instancePool);
        var tiles = new List<TileUnitForm.Data>();
        var center = typeof(MapUpdateController).GetField("viewCenter", BindingFlags.Instance | BindingFlags.NonPublic);
        var fresh = (Action<bool>)Delegate.CreateDelegate(typeof(Action<bool>), ctrl,
            typeof(MapUpdateController).GetMethod("RefreshMap", BindingFlags.Instance | BindingFlags.NonPublic));
        try
        {
            for (int x = 80; x <= 84; x++)
            {
                var data = new TileUnitForm.Data(681000 + x, "", new Dictionary<int, int>(), new Vector3Int(x, 0, 80),
                    template.name, new Vector3(x, 0, 80), Vector3.zero, Vector3.one,
                    UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
                tiles.Add(data);
                map.data.RegisterMap(data);
            }
            center.SetValue(ctrl, new Vector3Int(82, 0, 80));
            fresh(true);
            var departing = tiles[0].unit.ins;
            Require(departing != null && departing.SupportsViewRefreshReuse && departing.gameObject.activeInHierarchy,
                "actual FreshMap begins with an audited active Tile hierarchy");
            center.SetValue(ctrl, new Vector3Int(83, 0, 80));
            fresh(false);
            Require(tiles[0].unit.ins == null && tiles[4].unit.ins == departing
                && departing.unit == tiles[4].unit && departing.gameObject.activeInHierarchy,
                "FreshMap scope hands the leaving Tile hierarchy directly to an entering Tile");
            center.SetValue(ctrl, new Vector3Int(90, 0, 80));
            fresh(false);
            Require(!departing.gameObject.activeSelf && ctrl.curTileLst.Count == 0,
                "view with no replacements flushes all unused active returns before FreshMap exits");
        }
        finally
        {
            foreach (var tile in tiles)
            {
                tile.unit.Hide();
                map.data.UnRegisterMap(tile);
            }
            ctrl.End();
            instancePool.Clear();
            pool.Unregister(instancePool);
            Object.DestroyImmediate(parent);
            Object.DestroyImmediate(template);
            map.data.mainData.viewSize = oldSize;
        }
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
            "RefreshMap",
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
            typeof(MapUpdateController).GetMethod("RefreshMap", BindingFlags.Instance | BindingFlags.NonPublic));
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

    private static void ElevatedCoveredObjectOcclusion()
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
        var coveringPrefab = Go("offset-covering-roof");
        coveringPrefab.AddComponent<ObjectInstance>();
        var coveringModel = Go("offset-covering-roof-model");
        coveringModel.transform.SetParent(coveringPrefab.transform, false);
        coveringModel.transform.localPosition = new Vector3(9, 2, 0);
        coveringModel.transform.localScale = new Vector3(3, .4f, 1);
        coveringModel.AddComponent<BoxCollider>();
        pool.AddPool(coveringPrefab);
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
                var coverageBridge = Tile(179, layer, 181);
                var outsideOwner = Tile(171, layer, 180);
                var covering = Member(outsideOwner, emptyPrefab);
                ctrl.curObjectLst.Remove(covering.data);
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
                            "elevated same-layer coverage component fully hides beyond half radius, including out-of-list Objects: " + mode);
                    Require(Degree(sphere) == 0f && Degree(triggerOnly) == 0f,
                        "four-neighbour roof connectivity supports rotated physical Spheres and Trigger-only prefabs");
                    Require(Degree(grounded) == 1f && Degree(withinLayer) == 1f && Degree(mixed) == 1f && Degree(missing) == 1f,
                        "grounded/within-layer/multipart/missing physical bounds are not above the layer's upper boundary");
                    Require(Degree(owners[0]) == 1f && ReadVisionDegree(tileInstance, 0) == 1f
                        && Degree(character) == 1f && Degree(item) == 1f,
                        "roof hiding leaves current-layer Tile, front, Character and Item visibility unchanged");
                    Require(Degree(roofs[6]) == 1f && Degree(diagonal) == 1f,
                        "ground-only coverage and diagonal adjacency cannot connect roof components");

                    ctrl.objectTileDic.Add(diagonal, coverageBridge);
                    ctrl.objectTileDic.Add(roofs[6], owners[5]);
                    update();
                    Require(Degree(diagonal) == 0f && Degree(roofs[6]) == 0f && Degree(roofs[7]) == 0f,
                        "foreign elevated coverage bridges empty and ground-only Tiles regardless of belongTile");
                    Require(ctrl.objectTileDic.TryGetFirst(diagonal, out var diagonalOwner)
                        && diagonalOwner != coverageBridge
                        && ctrl.objectTileDic.TryGetFirst(roofs[6], out var farOwner) && farOwner == owners[6]
                        && Degree(roofs[5]) == 1f && Degree(owners[5]) == 1f && Degree(roofs[8]) == 1f,
                        "coverage hiding preserves ownership, ground Objects/Tiles and the view boundary");
                    ctrl.objectTileDic.Del(diagonal, coverageBridge);
                    ctrl.objectTileDic.Del(roofs[6], owners[5]);
                    update();
                    Require(Degree(diagonal) == 1f && Degree(roofs[6]) == 1f && Degree(roofs[7]) == 1f,
                        "removing foreign bridge coverage restores disconnected roofs next frame");

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
                    Require(Degree(seed) == 0f && Degree(roofs[1]) == 0f,
                        "rebinding belongTile without changing player-Tile coverage preserves full hiding");
                    ctrl.objectTileDic.Del(seed, owners[0]);
                    update();
                    Require(Degree(seed) == 1f && Degree(roofs[1]) == 1f,
                        "removing the player's elevated coverage breaks connectivity even when a neighbouring owner remains");
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
                        "no elevated coverage at the player's exact Tile prevents full hiding and restores omitted instances");
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

                    // Exercise real multi-Tile visual coverage refresh, not just
                    // hand-authored reverse links. The owner is outside the view.
                    seed.data.prefabName = groundPrefab.name;
                    covering.data.prefabName = coveringPrefab.name;
                    ctrl.RefreshObjectOverlap(covering);
                    Require(ctrl.objectTileDic.TryGetFirst(covering, out var outside)
                        && outside == outsideOwner
                        && ctrl.objectTileDic.TryGet(covering, out var coveredTiles) && coveredTiles.Contains(owners[0]),
                        "offset/scaled roof refresh covers the player Tile while preserving its out-of-view owner");
                    update();
                    Require(Degree(covering) == 0f && Degree(roofs[1]) == 0f
                        && ReadVisionDegree(((MapUnit)covering).ins, 0) == 0f,
                        "real foreign coverage alone seeds full hiding even outside the visible Object list");
                    Vector3 coveringPos = covering.data.pos;
                    covering.data.pos += Vector3.right * 20;
                    ctrl.RefreshObjectOverlap(covering);
                    update();
                    Require(ReadVisionDegree(((MapUnit)covering).ins, 0) == 1f && Degree(roofs[1]) == 1f,
                        "moving foreign coverage away restores its omitted instance and detaches the roof component");
                    covering.data.pos = coveringPos;
                    covering.data.euler = Vector3.up * 180;
                    ctrl.RefreshObjectOverlap(covering);
                    update();
                    Require(Degree(roofs[1]) == 1f,
                        "rotating the offset roof away removes its seed coverage without moving its owner");
                    covering.data.euler = Vector3.zero;
                    covering.data.scale = new Vector3(.5f, 1, 1);
                    ctrl.RefreshObjectOverlap(covering);
                    update();
                    Require(Degree(roofs[1]) == 1f,
                        "scaling the offset roof away removes its seed coverage without moving its owner");
                    covering.data.scale = Vector3.one;
                    ctrl.RefreshObjectOverlap(covering);
                    update();
                    Require(Degree(covering) == 0f && Degree(roofs[1]) == 0f,
                        "restoring the foreign visual footprint reconnects full hiding next frame");
                    covering.data.prefabName = emptyPrefab.name;
                    ctrl.RefreshObjectOverlap(covering);
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
                        "coverage-connected roof hiding is allocation-free after geometry/container warmup");
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
            if (type == MapEventType.Remove) continue; // Covered with a full runtime by DestroyCharacters.
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
    public int bindingReads;
    public override IReadOnlyList<DisFadeMaterialBinding> viewRefreshBindings
    {
        get { bindingReads++; return base.viewRefreshBindings; }
    }
}

[ExecuteAlways]
public class ViewRefreshReuseProbe : Instance
{
    public bool supportsReuse = true;
    public int enables;
    public int disables;
    public int prepares;
    public override bool SupportsViewRefreshReuse => supportsReuse;
    public override void PrepareForViewRefreshReuse()
    {
        base.PrepareForViewRefreshReuse();
        prepares++;
    }
    private void OnEnable() { enables++; }
    private void OnDisable() { disables++; }
}
