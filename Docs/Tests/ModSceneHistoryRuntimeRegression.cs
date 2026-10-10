using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using Ui.ModSceneBehaviourUnit;
using Ui.ModSceneUnit;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Language;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Ui;
using Z_Ui.Base;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Run only in the isolated project created by Run-MapRuntimeRegression.ps1.
// Real production Forms, geometry, ownership, navigation and erase operations are used.
public static class ModSceneHistoryRuntimeRegression
{
    private const int ObjectUid = 61000;
    private const int ForeignObjectUid = 61001;
    private const int CharacterUid = 61002;
    private const int ItemUid = 61003;
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static int checks;
    private static MapManager map;
    private static GameManager game;
    private static InstancePoolManager pool;
    private static int sceneUid;
    private static int otherSceneUid;
    private static int textureId;
    private static int passTypeId;
    private static int characterProductUid;
    private static int itemProductUid;
    private static GameMapData mapData;
    private static ModSceneHistory history;
    private static readonly HashSet<Type> registered = new HashSet<Type>();

    public static void Run()
    {
        try
        {
            Initialize();
            CapacityAndRedoBranch();
            NestedTransactionsAndNoOps();
            ControllerPointerTransactions();
            DeepMutableDataAndSceneScope();
            OwnedEraseAndEntityRecovery();
            EraseIgnoresRetainedPlacementOffsets();
            MovementAndNonDataState();
            AddedAndDeletedRows();
            TerrainFootprintAndLayerRecovery();
            TextureEraseUpdatesPassTypesImmediately();
            PlacementRotationAndImmediateAppearance();
            RaisedObjectPlacementUsesLowerOwner();
            ObjectSelectionRespectsColliderHeight();
            ObjectWangTileHistoryNeighbours();
            LargeMapIncrementalHistory();
            ClearAndSceneReplacement();
            TranslatedEmptyHistoryTip();
            Debug.Log("MOD_SCENE_HISTORY_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }

    private sealed class CharacterUpdateProbe : IZ_Listener<CharacterEvent>
    {
        public CharacterUnit unit;
        public int count;

        public void OnEvent(CharacterEvent evt)
        {
            if (evt.unit == unit && evt.type == MapEventType.AfterUpdate)
                count++;
        }
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject("history-test-" + typeof(T).Name);
        go.SetActive(false);
        var result = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", PrivateStatic).SetValue(null, result);
        return result;
    }

    private static void RegisterForm(Type form)
    {
        if (!registered.Add(form)) return;
        var data = form.GetNestedType("Data", BindingFlags.Public);
        var parentForm = data?.BaseType?.DeclaringType;
        if (parentForm != null) RegisterForm(parentForm);
        // RuntimeInitializeOnLoadMethod hooks do not run in this EditMode fixture.
        form.GetMethod("Register", PrivateStatic)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
    }

    private static GameObject Prefab<T>(string name) where T : Component
    {
        var result = new GameObject(name);
        result.SetActive(false);
        result.AddComponent<T>();
        return result;
    }

    private static void Initialize()
    {
        foreach (var form in new[] { typeof(MapMainForm), typeof(TileUnitForm), typeof(ObjectUnitForm),
                     typeof(CharacterUnitForm), typeof(ItemUnitForm), typeof(SceneForm), typeof(EventTriggerForm),
                     typeof(GameParamForm), typeof(CharacterProductForm), typeof(ItemProductForm),
                     typeof(MapObjectForm), typeof(MapItemForm), typeof(MapCharacterForm),
                     typeof(MapTextureForm), typeof(PassTypeForm), typeof(MapEraseForm),
                     typeof(ModTextForm), typeof(TexAssetForm), typeof(MapModelForm) })
            RegisterForm(form);

        UnitForm.Clear();
        var characterProduct = CharacterProductForm.GetDataByJo(new JObject());
        characterProduct.uid = -1;
        characterProduct.name = "history character";
        characterProduct.size = 2;
        characterProduct.paramDic = new Dictionary<string, CharacterParamForm.Data>();
        characterProduct.passType = new List<int>();
        characterProductUid = CharacterProductForm.AddData(characterProduct);
        var itemProduct = ItemProductForm.GetDataByJo(new JObject());
        itemProduct.uid = -1;
        itemProduct.name = "history item";
        itemProduct.paramDic = new Dictionary<string, ItemParamForm.Data>();
        itemProductUid = ItemProductForm.AddData(itemProduct);

        passTypeId = PassTypeForm.AddData(new PassTypeForm.Data(-1, "history restricted ground"));
        characterProduct.passType.Add(passTypeId);
        textureId = MapTextureForm.AddData(new MapTextureForm.Data(-1, "history terrain", 0, 0,
            new List<int>(), 0, new Dictionary<string, EventTriggerForm.Data>(), false,
            new Dictionary<int, int>(), passTypeId, false, new List<int>(), false, new Dictionary<int, int>()));
        sceneUid = SceneForm.AddData(new SceneForm.Data(-1, "history scene", 0, Vector2.zero, false, false,
            new Dictionary<string, EventTriggerForm.Data>
            {
                ["onEnterEvent"] = new EventTriggerForm.Data(-1, "onEnterEvent", new List<int> { 101, 102 }, default)
            }, new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>> { [99] = new List<string> { "before" } }, false));
        otherSceneUid = SceneForm.AddData(new SceneForm.Data(-1, "other scene", 0, Vector2.zero, false, false,
            new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>>(), false));

        map = CreateManager<MapManager>();
        map.mainGo = new GameObject("history-map-root");
        map.Init();
        pool = CreateManager<InstancePoolManager>();
        pool.pools = new List<InstancePool>();
        pool.defaultRoot = pool.transform;
        // No real scene or UI prefab is loaded; empty UI dictionaries also allow
        // production restoration to close stale entity panels normally.
        CreateManager<UiManager>();
        game = CreateManager<GameManager>();
        game.curScene = SceneForm.DataByUid[sceneUid];
        game.mapCtrl = new GameMapController(game);

        var terrain = Prefab<TileInstance>(MapInfo.GetPrefabName("mapfloor"));
        var groundBody = terrain.AddComponent<BoxCollider>();
        groundBody.center = Vector3.down * .05f;
        groundBody.size = new Vector3(1, .1f, 1);
        pool.AddPool(terrain);
        var generatedTerrain = Prefab<TileInstance>(MapInfo.GetPrefabName("mapground"));
        var generatedGroundBody = generatedTerrain.AddComponent<BoxCollider>();
        generatedGroundBody.center = groundBody.center;
        generatedGroundBody.size = groundBody.size;
        pool.AddPool(generatedTerrain);
        var slopeTerrain = Prefab<TileInstance>(MapInfo.GetPrefabName("mapslope"));
        var slopeBody = new GameObject("slope-body");
        slopeBody.transform.SetParent(slopeTerrain.transform, false);
        slopeBody.transform.localEulerAngles = new Vector3(45, 0, 0);
        slopeBody.AddComponent<BoxCollider>().size = new Vector3(1, .1f, 1);
        pool.AddPool(slopeTerrain);
        var obj = Prefab<ObjectInstance>("history-object");
        AddBody(obj, new Vector3(0, .4f, 0), new Vector3(.7f, .8f, .7f));
        pool.AddPool(obj);
        var foreign = Prefab<ObjectInstance>("history-foreign-object");
        AddBody(foreign, new Vector3(0, .4f, 0), new Vector3(.7f, .8f, .7f));
        var wideVisual = new GameObject("wide-model-footprint");
        wideVisual.transform.SetParent(foreign.transform, false);
        wideVisual.transform.localPosition = new Vector3(-1.2f, .5f, 0);
        wideVisual.transform.localScale = new Vector3(3, 1, 1);
        pool.AddPool(foreign);
        var character = Prefab<CharacterInstance>("history-character");
        var sphere = character.AddComponent<SphereCollider>();
        sphere.center = Vector3.up * .5f;
        sphere.radius = .5f;
        pool.AddPool(character);
        var item = Prefab<ItemInstance>("history-item");
        AddBody(item, Vector3.up * .1f, Vector3.one * .2f);
        pool.AddPool(item);

        mapData = new GameMapData
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(30, 3, 30),
                new Vector3Int(3, 1, 3), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        for (int x = 10; x <= 14; x++)
        for (int z = 10; z <= 14; z++)
        {
            var tile = new TileUnitForm.Data(60000 + (x - 10) * 5 + z - 10, "tile", new Dictionary<int, int>
                { [0] = textureId, [3] = 0, [4] = 0 }, new Vector3Int(x, 0, z), terrain.name,
                new Vector3(x, 0, z), Vector3.zero, Vector3.one, UpdateType.ShowOnly,
                new List<int>(), "", false, false, new List<int>());
            TileUnitForm.AddData(tile);
            mapData.RegisterMap(tile);
            GameMapData.ApplyTilePassTypes(tile);
        }
        var ownedObject = new ObjectUnitForm.Data(ObjectUid, true, "owned object", obj.name,
            new Vector3(12, 0, 12), Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0);
        ObjectUnitForm.AddData(ownedObject);
        ObjectUnitForm.AddData(new ObjectUnitForm.Data(ForeignObjectUid, true, "foreign object", foreign.name,
            new Vector3(14, 0, 12), Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0));
        ownedObject.unit.paramInfo = new Dictionary<string, GameParamForm.Data>
            { ["attack"] = new GameParamForm.Data(-1, "attack", default, "0", "10", "100") };
        ownedObject.unit.productInfo = (0, -1);
        ownedObject.unit.evtDic = new Dictionary<string, EventTriggerForm.Data>
            { ["onInteractEvent"] = new EventTriggerForm.Data(-1, "onInteractEvent", new List<int> { 201 }, default) };
        var characterData = new CharacterUnitForm.Data(CharacterUid, false, Vector3.zero, 0, 0, 100, false,
            characterProduct.name, character.name, new Vector3(12, 0, 12), Vector3.zero, Vector3.one * 2,
            UpdateType.ShowOnly, new List<int>(), MapUnit.GetProductInfoString(new JObject(), (characterProductUid, -1)),
            false, 0, new List<int> { passTypeId });
        CharacterUnitForm.AddData(characterData);
        GameMapData.ApplyCharacterProductPassTypes(characterData);
        ItemUnitForm.AddData(new ItemUnitForm.Data(ItemUid, itemProduct.name, item.name,
            new Vector3(12, 0, 12), Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(),
            MapUnit.GetProductInfoString(new JObject(), (itemProductUid, -1)), false, 0));

        DynamicGlobalSettings.playing = false;
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        map.Begin(mapData);
        map.SetPos(new Vector3(12, 0, 12));
        // Synchronize save-time caches before any history baseline is taken.
        mapData.GetJsonData();
        history = new ModSceneHistory(map, sceneUid);
        CheckRegisteredEntities();
    }

    private static void AddBody(GameObject root, Vector3 position, Vector3 scale)
    {
        var child = new GameObject("physical-body");
        child.transform.SetParent(root.transform, false);
        child.transform.localPosition = position;
        child.transform.localScale = scale;
        child.AddComponent<BoxCollider>().size = Vector3.one;
    }

    private sealed class SelectionProbe : UiCtrl
    {
        public UnitForm.Data selected;
        public override void SetParam(UiParam param)
        {
            selected = param is UiModSceneUnitParam unit ? unit.data
                : ((UiModSceneBehaviourUnitParam)param).data;
        }
    }

    private static void ObjectSelectionRespectsColliderHeight()
    {
        var mod = ModManager.instance;
        var previousController = mod.sceneCtrl;
        var controller = new ModSceneController(mod);
        mod.sceneCtrl = controller;
        typeof(ModSceneController).GetField("enable", PrivateInstance).SetValue(controller, true);
        var camera = CameraInstance.instance;
        var cam = camera.cam;
        Vector3 previousCameraPosition = cam.transform.position;
        Quaternion previousCameraRotation = cam.transform.rotation;
        Vector3 previousTarget = camera.tarTrs.position;
        var ui = UiManager.instance;
        var probes = new Dictionary<DesignType, SelectionProbe>();
        foreach (var mode in new[] { DesignType.MapObject, DesignType.Event })
        {
            string key = mode == DesignType.MapObject ? nameof(UiModSceneUnitCtrl) : nameof(UiModSceneBehaviourUnitCtrl);
            var go = new GameObject("selection-probe-" + key);
            go.SetActive(false);
            var holder = go.AddComponent<UiHolder>();
            var probe = new SelectionProbe { uiHolder = holder, inited = true };
            holder.ctrl = probe;
            probes.Add(mode, probe);
            ui.uiCtrlName2Uis.Add(key, new List<UiHolder> { holder });
        }
        var elevatedPrefab = Prefab<ObjectInstance>("selection-elevated-object");
        AddBody(elevatedPrefab, Vector3.up * 2, Vector3.one);
        var pick = elevatedPrefab.AddComponent<BoxCollider>();
        pick.isTrigger = true;
        pick.center = Vector3.up;
        pick.size = new Vector3(.8f, 4, .8f);
        pool.AddPool(elevatedPrefab);
        var lowerPrefab = Prefab<ObjectInstance>("selection-lower-object");
        AddBody(lowerPrefab, Vector3.up * .5f, Vector3.one);
        var lowerPick = lowerPrefab.AddComponent<BoxCollider>();
        lowerPick.isTrigger = true;
        lowerPick.center = Vector3.up * .5f;
        lowerPick.size = new Vector3(.8f, 1, .8f);
        pool.AddPool(lowerPrefab);
        var lower = map.AddObject("selectable lower object", new Vector3(10, 0, 10), lowerPrefab.name);
        var elevated = map.AddObject("raised collider, lower owner", new Vector3(10, 0, 10), elevatedPrefab.name);
        lower.unit.Show();
        elevated.unit.Show();
        // Fixture managers are intentionally inactive. Expose only these live
        // instances so this test really exercises Physics.RaycastAll.
        lower.unit.ins.transform.SetParent(map.mainGo.transform, true);
        elevated.unit.ins.transform.SetParent(map.mainGo.transform, true);
        // The root is the picking Trigger; the physical body is its child.
        var body = elevated.unit.ins.transform.GetChild(0).GetComponent<BoxCollider>();
        var livePick = elevated.unit.ins.GetComponent<BoxCollider>();
        GameObject secondBody = null;
        SphereCollider sphere = null;
        TileUnitForm.Data upper = null;
        ObjectUnitForm.Data higher = null;
        BoxCollider tilePick = null;
        UnitForm.Data Click(float height, DesignType mode, float pitch = 90)
        {
            controller.designType = mode;
            camera.tarTrs.position = new Vector3(10, height, 10);
            cam.transform.eulerAngles = Vector3.right * pitch;
            cam.transform.position = camera.tarTrs.position - cam.transform.forward * 10;
            Physics.SyncTransforms();
            Vector3 screen = cam.WorldToScreenPoint(new Vector3(10, height, 10));
            probes[mode].selected = null;
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = screen });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = screen });
            return probes[mode].selected;
        }
        string HitDetails()
        {
            Vector3 screen = cam.WorldToScreenPoint(new Vector3(10, camera.tarTrs.position.y, 10));
            return string.Join("; ", Physics.RaycastAll(cam.ScreenPointToRay(screen), 100,
                Physics.DefaultRaycastLayers, QueryTriggerInteraction.Collide)
                .Select(hit => hit.collider.name + ":" + hit.collider.isTrigger + ":" + hit.collider.bounds.min.y))
                + "; lower active=" + lower.unit.ins.gameObject.activeInHierarchy
                + "; body min=" + body.bounds.min.y + "; selected=" + probes[controller.designType].selected?.name;
        }
        try
        {
            Check(elevated.unit.belongTile == Tile(10, 10).unit && elevated.pos.y == 0,
                "selection fixture has a current-layer root/owner but an elevated physical body");
            foreach (var mode in new[] { DesignType.MapObject, DesignType.Event })
            {
                Check(Click(0, mode) == lower, "raised physical body is skipped despite padded Trigger reaching current layer: " + mode + "; " + HitDetails());
                Check(Click(1.5f, mode) == elevated, "Collider bottom touching current editing plane remains selectable: " + mode);
                Check(Click(2, mode) == elevated, "raising editing plane permits an Object still owned by a lower Tile: " + mode);
                body.center = Vector3.down * 1.5f;
                Check(Click(0, mode) == elevated, "actual Collider center offset reaching current layer remains selectable: " + mode);
                body.center = Vector3.zero;
                body.transform.localPosition = Vector3.up * .5f;
                Check(Click(0, mode) == elevated, "lowering child model without changing root/owner updates selection immediately: " + mode);
                body.transform.localPosition = Vector3.up * 2;
                body.transform.localScale = new Vector3(1, 3, 1);
                Check(Click(0, mode) == lower, "scaled physical bottom above plane is still excluded: " + mode);
                body.transform.localScale = new Vector3(1, 1, 4);
                body.transform.localEulerAngles = Vector3.right * 90;
                Check(Click(0, mode) == elevated, "rotation brings actual body bottom to plane: " + mode);
                body.transform.localEulerAngles = Vector3.zero;
                body.transform.localScale = Vector3.one;
            }

            higher = map.AddObject("highest fallback object", new Vector3(10, 3, 10), elevatedPrefab.name);
            higher.unit.Show();
            higher.unit.ins.transform.SetParent(map.mainGo.transform, true);
            var highestPick = higher.unit.ins.GetComponent<BoxCollider>();
            var liveLowerPick = lower.unit.ins.GetComponent<BoxCollider>();
            // Broad picking Triggers keep all three Objects on the same ray in
            // both overhead and side view; physical heights still decide priority.
            livePick.size = new Vector3(.8f, 4, 14);
            highestPick.size = new Vector3(.8f, 4, 14);
            liveLowerPick.size = new Vector3(.8f, 1, 14);
            foreach (float pitch in new[] { 90f, 45f })
                foreach (var mode in new[] { DesignType.MapObject, DesignType.Event })
                {
                    Check(Click(0, mode, pitch) == lower, "current-height target wins over multiple higher hits: " + mode + "/" + pitch);
                    lower.unit.ins.gameObject.SetActive(false);
                    Check(Click(0, mode, pitch) == higher, "no current-height target falls back to highest ray hit: " + mode + "/" + pitch);
                    higher.unit.ins.gameObject.SetActive(false);
                    Check(Click(0, mode, pitch) == elevated, "next lower elevated hit remains selectable as fallback: " + mode + "/" + pitch);
                    elevated.unit.ins.gameObject.SetActive(false);
                    Check(Click(0, mode, pitch) == null, "empty click stays empty rather than reusing previous selection: " + mode + "/" + pitch);
                    elevated.unit.ins.gameObject.SetActive(true);
                    higher.unit.ins.gameObject.SetActive(true);
                    lower.unit.ins.gameObject.SetActive(true);
                }

            // Event mode still permits Tile selection: a valid Tile means the
            // click is not empty and must not trigger the higher-Object fallback.
            var ground = Tile(10, 10).unit;
            bool groundWasShowing = ground.isVising;
            ground.Show();
            var groundParent = ground.ins.transform.parent;
            ground.ins.transform.SetParent(map.mainGo.transform, true);
            tilePick = ground.ins.gameObject.AddComponent<BoxCollider>();
            tilePick.isTrigger = true;
            tilePick.center = Vector3.up * .05f;
            tilePick.size = new Vector3(.9f, .1f, .9f);
            lower.unit.ins.gameObject.SetActive(false);
            Check(Click(0, DesignType.Event) == ground.data, "Event Tile selection prevents higher-Object fallback");
            Check(Click(0, DesignType.MapObject) == higher, "normal mode ignores Tiles before selecting the high fallback");
            Object.DestroyImmediate(tilePick);
            tilePick = null;
            ground.ins.transform.SetParent(groundParent, true);
            if (!groundWasShowing) ground.Hide();
            lower.unit.ins.gameObject.SetActive(true);
            map.RemoveObject(higher);
            higher = null;
            livePick.size = new Vector3(.8f, 4, .8f);
            liveLowerPick.size = new Vector3(.8f, 1, .8f);

            secondBody = new GameObject("second-selection-body");
            secondBody.transform.SetParent(elevated.unit.ins.transform, false);
            secondBody.transform.localPosition = Vector3.up * .25f;
            var second = secondBody.AddComponent<BoxCollider>();
            second.size = Vector3.one * .5f;
            Check(Click(0, DesignType.MapObject) == elevated, "one lower physical body keeps a multipart Object selectable");
            second.enabled = false;
            Check(Click(0, DesignType.MapObject) == lower, "disabled lower body cannot bypass elevated-body filtering");
            second.enabled = true;
            secondBody.SetActive(false);
            Check(Click(0, DesignType.Event) == lower, "inactive lower body cannot bypass elevated-body filtering");
            secondBody.SetActive(true);
            second.isTrigger = true;
            Check(Click(0, DesignType.Event) == lower, "additional lower Trigger cannot bypass a physical body above the plane");
            secondBody.SetActive(false);

            body.enabled = false;
            livePick.center = Vector3.up * 2;
            livePick.size = new Vector3(.8f, 1, .8f);
            Check(Click(0, DesignType.MapObject) == lower, "Trigger-only Object above editing plane is excluded");
            lower.unit.ins.gameObject.SetActive(false);
            Check(Click(0, DesignType.MapObject) == elevated, "Trigger-only Object is a fallback when nothing lower is selected");
            lower.unit.ins.gameObject.SetActive(true);
            livePick.center = Vector3.up;
            livePick.size = new Vector3(.8f, 4, .8f);
            Check(Click(0, DesignType.Event) == elevated, "Trigger-only prefab touching editing plane remains selectable");
            sphere = body.gameObject.AddComponent<SphereCollider>();
            sphere.radius = 1;
            body.transform.localEulerAngles = new Vector3(35, 20, 15);
            Check(Click(0, DesignType.MapObject) == lower, "rotated Sphere actual lower bound above plane is excluded");
            body.transform.localScale = Vector3.one * 2;
            Check(Click(0, DesignType.Event) == elevated, "scaled Sphere touches plane and remains selectable");
            sphere.enabled = false;
            body.enabled = true;
            body.transform.localScale = Vector3.one;
            body.transform.localEulerAngles = Vector3.zero;

            upper = map.AddTile(new Vector3Int(10, 2, 10));
            map.updateCtrl.ApplyMove(elevated.unit, new Vector3(10, 2, 10), Vector3.zero, true);
            body.transform.localPosition = Vector3.down * 1.5f;
            Check(elevated.unit.belongTile == upper.unit, "upper-owner fixture actually belongs to a higher Tile");
            Check(Click(0, DesignType.MapObject) == lower, "higher owner is excluded even when child Collider extends down");
            lower.unit.ins.gameObject.SetActive(false);
            Check(Click(0, DesignType.MapObject) == elevated, "higher owner remains selectable as an empty-click fallback");
            lower.unit.ins.gameObject.SetActive(true);
            Check(Click(2, DesignType.Event) == elevated, "higher Object is selectable after switching editing height");
            Check(history.undoCount == 0 && history.redoCount == 0, "selection does not create history operations");
        }
        finally
        {
            controller.CommitPendingOperation();
            controller.Unregister<InputKeyEvent>();
            controller.Unregister<InputMouseEvent>();
            controller.Unregister<InputMouseDownEvent>();
            controller.Unregister<InputMouseUpEvent>();
            controller.Unregister<InputMouseScrollEvent>();
            controller.Unregister<TileEvent>();
            controller.Unregister<ObjectEvent>();
            if (secondBody != null) Object.DestroyImmediate(secondBody);
            if (sphere != null) Object.DestroyImmediate(sphere);
            if (tilePick != null) Object.DestroyImmediate(tilePick);
            if (higher != null) map.RemoveObject(higher);
            map.RemoveObject(elevated);
            map.RemoveObject(lower);
            if (upper != null) map.RemoveTile(upper);
            foreach (var pair in probes)
            {
                string key = pair.Key == DesignType.MapObject ? nameof(UiModSceneUnitCtrl) : nameof(UiModSceneBehaviourUnitCtrl);
                ui.uiCtrlName2Uis.Remove(key);
                Object.DestroyImmediate(pair.Value.gameObject);
            }
            camera.tarTrs.position = previousTarget;
            cam.transform.SetPositionAndRotation(previousCameraPosition, previousCameraRotation);
            mod.sceneCtrl = previousController;
        }
    }

    private static TileUnitForm.Data Tile(int x = 12, int z = 12) => map.data.maps[(x, 0, z)];
    private static ObjectUnitForm.Data OwnedObject() => ObjectUnitForm.DataByUid[ObjectUid];
    private static SceneForm.Data Scene() => SceneForm.DataByUid[sceneUid];

    private static void SetTexture(TileUnitForm.Data tile, int slot, int value)
    {
        history.Track(tile);
        tile.texDic[slot] = value;
    }

    private static void CapacityAndRedoBranch()
    {
        for (int i = 1; i <= 12; i++)
            using (history.BeginOperation()) SetTexture(Tile(), 3, i);
        Check(history.undoCount == 10 && history.redoCount == 0, "only the ten latest effective operations are retained");
        for (int expected = 11; expected >= 2; expected--)
        {
            Check(history.TryUndo() && Tile().texDic[3] == expected, "undo restores exact preceding value " + expected);
            Check(ReferenceEquals(map.data, mapData), "undo preserves the original MapInfo owner");
        }
        Check(!history.TryUndo() && Tile().texDic[3] == 2 && history.redoCount == 10,
            "eleventh undo is unavailable without changing data");
        for (int expected = 3; expected <= 12; expected++)
            Check(history.TryRedo() && Tile().texDic[3] == expected, "redo restores exact recorded value " + expected);
        Check(!history.TryRedo() && history.undoCount == 10, "redo has a bounded empty terminal state");
        Check(history.TryUndo() && Tile().texDic[3] == 11, "branch starts after one undo");
        using (history.BeginOperation()) SetTexture(Tile(), 3, 11);
        Check(history.redoCount == 1, "no-op does not discard the existing redo branch");
        using (history.BeginOperation()) SetTexture(Tile(), 3, 30);
        Check(history.redoCount == 0 && !history.TryRedo(), "effective editing discards the old redo branch");
        Check(history.TryUndo() && Tile().texDic[3] == 11 && history.TryRedo() && Tile().texDic[3] == 30,
            "new branch has its own reversible operation");
        history.Clear();
    }

    private static void NestedTransactionsAndNoOps()
    {
        int initial = Tile().texDic[3];
        int initialOther = Tile().texDic[4];
        using (history.BeginOperation())
        {
            SetTexture(Tile(), 3, 40);
            using (history.BeginOperation()) SetTexture(Tile(), 4, 50);
            using (history.BeginOperation()) SetTexture(Tile(), 3, 60);
            Check(history.undoCount == 0 && !history.TryUndo(), "a held brush transaction is not committed per frame");
        }
        Check(history.undoCount == 1, "nested multi-frame brush edits commit as exactly one operation");
        Check(history.TryUndo() && Tile().texDic[3] == initial && Tile().texDic[4] == initialOther,
            "one undo restores every row change in the brush stroke");
        Check(history.TryRedo() && Tile().texDic[3] == 60 && Tile().texDic[4] == 50,
            "one redo restores the completed stroke");
        history.Clear();
        using (history.BeginOperation()) { }
        using (history.BeginOperation())
        {
            history.Track(Tile());
            int value = Tile().texDic[3];
            Tile().texDic[3] = value + 1;
            Tile().texDic[3] = value;
            Tile().texDic.Remove(3);
            Tile().texDic.Add(3, value);
            mapData.GetJsonData();
        }
        Check(history.undoCount == 0, "empty/net-zero/dictionary-order/save-cache changes are no-ops");
        using (history.BeginOperation())
        {
            history.Track(Tile());
            Tile().enteredScene = !Tile().enteredScene;
            Tile().collidingUnitUid.Add(999999);
        }
        Check(history.undoCount == 0, "runtime entry/collision bookkeeping does not create authored history");
        Tile().enteredScene = true;
        Tile().collidingUnitUid.Clear();
        using (history.BeginOperation()) SetTexture(Tile(), 3, 61);
        Tile(10, 10).texDic[3] = 77;
        Check(history.TryUndo() && Tile(10, 10).texDic[3] == 77,
            "undo applies only recorded rows and preserves unrelated current Form data");
        history.Clear();
    }

    private static void DeepMutableDataAndSceneScope()
    {
        var staleSceneTrigger = Scene().events["onEnterEvent"];
        var staleUnitTrigger = OwnedObject().unit.evtDic["onInteractEvent"];
        var staleParam = OwnedObject().unit.paramInfo["attack"];
        var originalSceneIdentity = game.curScene;
        using (history.BeginOperation())
        {
            history.TrackScene();
            history.Track(OwnedObject());
            staleSceneTrigger.evt.Add(103);
            Scene().triggeredOnceEvts[99].Add("after");
            staleUnitTrigger.evt.Add(202);
            OwnedObject().unit.evtDic = OwnedObject().unit.evtDic;
            staleParam.v = "30";
        }
        Check(history.undoCount == 1, "in-place dictionaries/nested Form/list edits are recorded");
        Check(history.TryUndo(), "nested data undo succeeds");
        Check(Scene().events["onEnterEvent"].evt.SequenceEqual(new[] { 101, 102 })
            && Scene().triggeredOnceEvts[99].SequenceEqual(new[] { "before" }), "scene event and nested list state is restored deeply");
        Check(OwnedObject().unit.evtDic["onInteractEvent"].evt.SequenceEqual(new[] { 201 })
            && OwnedObject().unit.paramInfo["attack"].v == "10", "restored runtime entity getters read the restored event/parameter state");
        Check(ReferenceEquals(game.curScene, originalSceneIdentity), "scene restore keeps the current scene identity usable by UI owners");
        staleSceneTrigger.evt.Add(999);
        staleUnitTrigger.evt.Add(999);
        staleParam.v = "999";
        Check(history.TryRedo(), "nested data redo succeeds after old containers were modified");
        Check(Scene().events["onEnterEvent"].evt.SequenceEqual(new[] { 101, 102, 103 })
            && OwnedObject().unit.evtDic["onInteractEvent"].evt.SequenceEqual(new[] { 201, 202 })
            && OwnedObject().unit.paramInfo["attack"].v == "30", "history snapshots never share old mutable containers");
        history.Clear();
        using (history.BeginOperation()) SceneForm.DataByUid[otherSceneUid].name = "other scene edited";
        Check(history.undoCount == 0, "a different logical scene is outside this scene's history");
        string originalName = Scene().name;
        using (history.BeginOperation())
        {
            history.TrackScene();
            Scene().name = "changed current scene";
        }
        SceneForm.DataByUid[otherSceneUid].name = "unrelated current value";
        Check(history.TryUndo() && Scene().name == originalName
            && SceneForm.DataByUid[otherSceneUid].name == "unrelated current value", "current-scene metadata undo leaves other scenes untouched");
        history.Clear();
    }

    private static void ControllerPointerTransactions()
    {
        // Only the raycast paint action is suppressed; Down/Up, scope lifetime,
        // Undo/Redo and the display refresh use the production Controller methods.
        var controller = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
        typeof(ModSceneController).GetField("enable", PrivateInstance).SetValue(controller, true);
        typeof(ModSceneController).GetField("waitForActive", PrivateInstance).SetValue(controller, true);
        typeof(ModSceneController).GetField("history", PrivateInstance).SetValue(controller, history);
        var camera = CreateManager<CameraInstance>();
        var cameraGo = new GameObject("history-camera");
        cameraGo.transform.SetParent(camera.transform, false);
        var cameraComponent = cameraGo.AddComponent<Camera>();
        cameraComponent.orthographic = true;
        cameraComponent.orthographicSize = 5.2f;
        camera.tarTrs.position = new Vector3(12.25f, 0, 12.2f);
        Vector3 cameraPosition = camera.tarTrs.position;
        controller.curData = MapEraseForm.DataByName["remain terrain"];
        controller.layer = 1;
        controller.designType = DesignType.Event;
        controller.cntX = 7;
        controller.angle = 180;
        var releaseUi = new GameObject("history-release-ui");
        var pointerField = typeof(ModSceneController).GetField("pointerDown", PrivateInstance);
        var brushField = typeof(ModSceneController).GetField("brushOperation", PrivateInstance);
        var revisionField = typeof(ModSceneController).GetField("brushRevision", PrivateInstance);
        try
        {
            history.Clear();
            controller.OnEvent(new InputMouseDownEvent { id = 1, pos = Vector3.zero });
            controller.OnEvent(new InputMouseUpEvent { id = 1, pos = Vector3.zero });
            Check(history.undoCount == 0 && brushField.GetValue(controller) == null,
                "right-button events do not start a brush transaction");
            int initialA = Tile().texDic[3];
            int initialB = Tile(11, 12).texDic[4];
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = Vector3.zero });
            Check((bool)pointerField.GetValue(controller) && brushField.GetValue(controller) != null,
                "screen origin (0,0) is a valid held left-button brush stroke");
            for (int frame = 1; frame <= 3; frame++)
                using (controller.BeginOperation())
                {
                    SetTexture(Tile(), 3, initialA + frame);
                    SetTexture(Tile(11, 12), 4, initialB + frame);
                }
            // Actual paint/erase increments this revision after mutating Forms.
            // The raycast action is blocked above, so simulate that one signal.
            revisionField.SetValue(controller, (int)revisionField.GetValue(controller) + 1);
            Check(history.undoCount == 0, "real Controller Down keeps multiple per-frame operations pending");
            controller.OnEvent(new InputMouseDownEvent { id = 1, pos = new Vector3(20, 20, 0) });
            controller.OnEvent(new InputMouseUpEvent { id = 1, pos = new Vector3(20, 20, 0) });
            Check(history.undoCount == 0 && (bool)pointerField.GetValue(controller)
                && brushField.GetValue(controller) != null, "right-button events cannot finish an active left-button stroke");
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = new Vector3(100, 100, 0), ui = releaseUi });
            Check(history.undoCount == 1 && !(bool)pointerField.GetValue(controller)
                && brushField.GetValue(controller) == null, "release over UI commits exactly one completed stroke");
            controller.Undo();
            Check(history.undoCount == 0 && history.redoCount == 1
                && Tile().texDic[3] == initialA && Tile(11, 12).texDic[4] == initialB,
                "real Controller Undo restores every touched Tile in the stroke");
            Check(controller.curData == MapEraseForm.DataByName["remain terrain"]
                && controller.layer == 1 && controller.designType == DesignType.Event
                && controller.cntX == 7 && controller.angle == 180
                && camera.tarTrs.position == cameraPosition && cameraComponent.orthographicSize == 5.2f,
                "Controller Undo preserves mode, brush, layer, brush dimensions and camera view");
            controller.Redo();
            Check(history.undoCount == 1 && history.redoCount == 0
                && Tile().texDic[3] == initialA + 3 && Tile(11, 12).texDic[4] == initialB + 3,
                "real Controller Redo restores the whole completed stroke");
            history.Clear();
            typeof(ModSceneController).GetField("waitForActive", PrivateInstance).SetValue(controller, true);
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = new Vector3(8, 8, 0), ui = releaseUi });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = new Vector3(8, 8, 0), ui = releaseUi });
            Check(history.undoCount == 0 && brushField.GetValue(controller) == null,
                "a pointer press originating on UI cannot start painting history");
            controller.curData = null;
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = new Vector3(8, 8, 0) });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = new Vector3(8, 8, 0) });
            Check(history.undoCount == 0 && brushField.GetValue(controller) == null,
                "a click with no brush creates no scene history");
            controller.curData = MapEraseForm.DataByName["remain terrain"];
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = new Vector3(8, 8, 0) });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = new Vector3(8, 8, 0) });
            Check(history.undoCount == 0, "an empty brush stroke is a no-op");
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = new Vector3(8, 8, 0) });
            using (controller.BeginOperation()) SetTexture(Tile(), 3, Tile().texDic[3] + 1);
            revisionField.SetValue(controller, (int)revisionField.GetValue(controller) + 1);
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = new Vector3(8, 8, 0) });
            Check(history.undoCount == 1, "a single-click brush edit is one operation");
            controller.Undo();
            Check(history.undoCount == 0 && history.redoCount == 1,
                "the single-click brush operation can be undone through the Controller");
            typeof(ModSceneController).GetField("waitForActive", PrivateInstance).SetValue(controller, true);
            Vector3 originalCharacterPosition = CharacterUnitForm.DataByUid[CharacterUid].pos;
            Vector3 runtimeCharacterPosition = originalCharacterPosition + Vector3.forward * .1f;
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = new Vector3(8, 8, 0) });
            map.updateCtrl.ApplyMove(CharacterUnitForm.DataByUid[CharacterUid].unit,
                runtimeCharacterPosition, CharacterUnitForm.DataByUid[CharacterUid].euler, true);
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = new Vector3(8, 8, 0), ui = releaseUi });
            Check(history.undoCount == 0 && history.redoCount == 1
                && CharacterUnitForm.DataByUid[CharacterUid].pos == runtimeCharacterPosition,
                "empty painting ignores runtime Character movement and preserves redo");
            controller.Redo();
            Check(history.undoCount == 1 && history.redoCount == 0
                && CharacterUnitForm.DataByUid[CharacterUid].pos == runtimeCharacterPosition,
                "redo remains usable after an empty stroke and keeps unrelated runtime position");
            map.updateCtrl.ApplyMove(CharacterUnitForm.DataByUid[CharacterUid].unit,
                originalCharacterPosition, CharacterUnitForm.DataByUid[CharacterUid].euler, true);
            history.Clear();
        }
        finally
        {
            controller.CommitPendingOperation();
            Object.DestroyImmediate(releaseUi);
        }
    }

    private static void CheckRegisteredEntities()
    {
        var owner = Tile().unit;
        var obj = OwnedObject().unit;
        var character = CharacterUnitForm.DataByUid[CharacterUid].unit;
        var item = ItemUnitForm.DataByUid[ItemUid].unit;
        var foreign = ObjectUnitForm.DataByUid[ForeignObjectUid].unit;
        Check(obj.belongTile == owner && character.belongTile == owner && item.belongTile == owner,
            "all three entity types have the correct restored owner Tile");
        Check(map.updateCtrl.objectTileDic.TryGet(owner, out var objects) && objects.Contains(obj)
            && objects.Contains(foreign) && foreign.belongTile == Tile(14, 12).unit,
            "bidirectional Object coverage includes a neighbour-owned visual overlap");
        Check(map.updateCtrl.characterTileDic.TryGet(owner, out var characters) && characters.Contains(character)
            && map.updateCtrl.characterOverlapTileDic.TryGet(character, out var covered) && covered.Count > 1,
            "Character owner and scaled physical coverage are rebuilt together");
        Check(map.updateCtrl.itemTileDic.TryGet(owner, out var items) && items.Contains(item), "Item reverse owner index is restored");
        var passIndex = (Z_DoubleDictionary.DoubleDictionary<ObjectUnit, TileUnit>)typeof(MapUpdateController)
            .GetField("objectPassTypeTileDic", PrivateInstance).GetValue(map.updateCtrl);
        var navIndex = (Z_DoubleDictionary.DoubleDictionary<ObjectUnit, TileUnit>)typeof(MapUpdateController)
            .GetField("objectNavigationTileDic", PrivateInstance).GetValue(map.updateCtrl);
        Check(passIndex.TryGet(obj, out var passTiles) && passTiles.Contains(owner)
            && navIndex.TryGet(obj, out var navTiles) && navTiles.Contains(owner), "Object physical pass/navigation coverage indexes are restored");
        Check(owner.passTypes.Count == 0 && map.navigationCtrl.navUnits[(12, 0, 12)].objectBlocked,
            "physical Object restores effective Tile pass types and blocked navigation immediately");
        Check(CharacterUnitForm.DataByUid[CharacterUid].scale == Vector3.one * 2
            && character.passTypes.Contains(passTypeId), "Character Product size and pass capabilities survive restoration");
        Check(ReferenceEquals(UnitForm.DataByUid[ObjectUid], ObjectUnitForm.DataByUid[ObjectUid])
            && ReferenceEquals(UnitForm.DataByUid[CharacterUid], CharacterUnitForm.DataByUid[CharacterUid])
            && ReferenceEquals(UnitForm.DataByUid[ItemUid], ItemUnitForm.DataByUid[ItemUid]), "concrete and root Unit Form registries share restored identities");
        Check(map.data.mapXZ2Y[(12, 12)].SetEquals(new[] { 0 }), "sparse height index agrees with restored terrain");
    }

    private static void OwnedEraseAndEntityRecovery()
    {
        var erase = typeof(ModSceneController).GetMethod("EraseTile", PrivateInstance);
        Check(erase != null, "real production erase operation exists");
        var controller = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
        typeof(ModSceneController).GetField("enable", PrivateInstance).SetValue(controller, true);
        typeof(ModSceneController).GetField("history", PrivateInstance).SetValue(controller, history);
        foreach (string brushName in new[] { "remain terrain", "all erase" })
        {
            history.Clear();
            var originalTerrain = new Dictionary<int, int>(Tile().texDic);
            int tileUid = Tile().uid;
            using (history.BeginOperation()) erase.Invoke(controller, new object[] { Tile(), MapEraseForm.DataByName[brushName] });
            Check(history.undoCount == 1 && !ObjectUnitForm.DataByUid.ContainsKey(ObjectUid)
                && !CharacterUnitForm.DataByUid.ContainsKey(CharacterUid) && !ItemUnitForm.DataByUid.ContainsKey(ItemUid),
                brushName + " removes all owned entities as one operation");
            Check(ObjectUnitForm.DataByUid.ContainsKey(ForeignObjectUid), brushName + " keeps neighbouring owner's overlapping Object");
            bool keepsTerrain = brushName == "remain terrain";
            Check(map.data.maps.ContainsKey((12, 0, 12)) == keepsTerrain
                && TileUnitForm.DataByUid.ContainsKey(tileUid) == keepsTerrain, brushName + " removes/preserves terrain correctly");
            if (keepsTerrain)
                Check(Tile().unit.passTypes.Contains(passTypeId) && !map.navigationCtrl.navUnits[(12, 0, 12)].objectBlocked,
                    "remain terrain immediately restores terrain pass requirements and removes the obstacle");
            else
                Check(!map.data.mapXZ2Y.ContainsKey((12, 12)), "all erase clears the final sparse height entry");
            Check(history.TryUndo(), brushName + " undo succeeds");
            Check(Tile().uid == tileUid && originalTerrain.All(pair => Tile().texDic.TryGetValue(pair.Key, out int value) && value == pair.Value),
                "undo restores the original terrain UID, textures and masks");
            CheckRegisteredEntities();
            Check(history.TryRedo() && !ItemUnitForm.DataByUid.ContainsKey(ItemUid)
                && !ObjectUnitForm.DataByUid.ContainsKey(ObjectUid) && !CharacterUnitForm.DataByUid.ContainsKey(CharacterUid),
                "redo does not leave deleted entities in concrete registries");
            Check(history.TryUndo(), "second undo restores erased entities again");
            CheckRegisteredEntities();
        }
        history.Clear();
    }

    private static void EraseIgnoresRetainedPlacementOffsets()
    {
        var mod = CreateManager<ModManager>();
        var controller = new ModSceneController(mod);
        mod.sceneCtrl = controller;
        var input = CreateManager<InputManager>();
        input.screenSize = new Vector2(640, 480);
        var camera = CameraInstance.instance;
        var cam = camera.GetComponentInChildren<Camera>(true);
        typeof(CameraInstance).GetField("_cam", PrivateInstance).SetValue(camera, cam);
        cam.transform.position = new Vector3(12, 10, 12);
        cam.transform.eulerAngles = Vector3.right * 90;
        camera.tarTrs.position = new Vector3(12, 0, 12);
        // Reposition the child Camera after moving its target parent.
        cam.transform.position = new Vector3(12, 10, 12);
        var enableField = typeof(ModSceneController).GetField("enable", PrivateInstance);
        var waitField = typeof(ModSceneController).GetField("waitForActive", PrivateInstance);
        var historyField = typeof(ModSceneController).GetField("history", PrivateInstance);
        var screenPosition = cam.WorldToScreenPoint(new Vector3(12, 0, 12));
        var probeRay = cam.ScreenPointToRay(screenPosition);
        var probePlane = new Plane(Vector3.up, -camera.tarTrs.position.y);
        Check(probePlane.Raycast(probeRay, out float probeEnter)
            && (probeRay.GetPoint(probeEnter) - new Vector3(12, 0, 12)).sqrMagnitude < .0001f,
            "actual erase-click ray resolves the intended Tile center");
        var farTile = Tile(14, 14);
        try
        {
            // These offsets are deliberately retained by the scene Controller
            // across entry, just as after placing a raised/freely positioned model.
            controller.posX = 1.2f;
            controller.posY = .9f;
            controller.posZ = -1.3f;
            controller.cntX = controller.cntY = 1;
            TileUnitForm.Data placementTarget = null;
            Action<TileUnitForm.Data, Vector3> collectOldTarget = (tile, _) => placementTarget = tile;
            typeof(ModSceneController).GetMethod("ForeachPos", PrivateInstance).Invoke(controller,
                new object[] { new Vector3Int(12, 0, 12), new Vector3(12, 0, 12), collectOldTarget });
            Check(placementTarget != null && placementTarget != Tile(),
                "model-placement targeting is offset from the clicked erase Tile with retained offsets");
            foreach (string brushName in new[] { "remain terrain", "all erase", "remain terrain" })
            {
                history.Clear();
                enableField.SetValue(controller, false);
                controller.curData = null;
                enableField.SetValue(controller, true);
                waitField.SetValue(controller, false);
                historyField.SetValue(controller, history);
                controller.curData = MapEraseForm.DataByName[brushName];
                controller.OnEvent(new InputMouseDownEvent { id = 0, pos = screenPosition });
                controller.OnEvent(new InputMouseUpEvent { id = 0, pos = screenPosition });
                Check(!ObjectUnitForm.DataByUid.ContainsKey(ObjectUid)
                    && !CharacterUnitForm.DataByUid.ContainsKey(CharacterUid)
                    && !ItemUnitForm.DataByUid.ContainsKey(ItemUid),
                    "actual erase click still removes clicked owners after reentry with retained placement offsets: " + brushName
                    + "; pointer=" + typeof(ModSceneController).GetField("pointerDown", PrivateInstance).GetValue(controller)
                    + "; revision=" + typeof(ModSceneController).GetField("brushRevision", PrivateInstance).GetValue(controller)
                    + "; wait=" + waitField.GetValue(controller));
                Check(map.data.maps.ContainsKey((12, 0, 12)) == (brushName == "remain terrain"),
                    "actual erase click targets the clicked Tile layer, not the retained Y placement offset");
                Check(ReferenceEquals(farTile, Tile(14, 14)) && ObjectUnitForm.DataByUid.ContainsKey(ForeignObjectUid),
                    "actual erase click leaves neighbouring/distant owners and terrain unchanged");
                Check(history.undoCount == 1 && history.TryUndo(), "actual click erase remains one undoable stroke");
                CheckRegisteredEntities();
                Check(controller.posX == 1.2f && controller.posY == .9f && controller.posZ == -1.3f,
                    "erase does not reset authored placement controls to work around the bad target");
            }
            history.Clear();
        }
        finally
        {
            controller.CommitPendingOperation();
            controller.Unregister<InputKeyEvent>();
            controller.Unregister<InputMouseEvent>();
            controller.Unregister<InputMouseDownEvent>();
            controller.Unregister<InputMouseUpEvent>();
            controller.Unregister<InputMouseScrollEvent>();
            controller.Unregister<TileEvent>();
            controller.Unregister<ObjectEvent>();
        }
    }

    private static void MovementAndNonDataState()
    {
        var editor = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
        editor.curData = MapEraseForm.DataByName["remain terrain"];
        editor.layer = 1;
        editor.designType = DesignType.Event;
        editor.cntX = 5;
        editor.cntY = 3;
        editor.angle = 90;
        editor.posX = 2;
        editor.posY = 1;
        editor.posZ = -2;
        Vector3 oldCenter = new Vector3(11, 0, 11);
        map.SetPos(oldCenter);
        Vector3 before = OwnedObject().pos;
        using (history.BeginOperation())
        {
            history.Track(OwnedObject());
            map.updateCtrl.ApplyMove(OwnedObject().unit, new Vector3(11, 0, 11), Vector3.up * 45, true);
        }
        Check(!map.navigationCtrl.navUnits[(12, 0, 12)].objectBlocked
            && map.navigationCtrl.navUnits[(11, 0, 11)].objectBlocked, "actual movement updates old/new obstacle cells");
        Check(history.TryUndo() && OwnedObject().pos == before && OwnedObject().euler == Vector3.zero,
            "movement undo restores position and rotation with the original UID");
        Check(editor.curData == MapEraseForm.DataByName["remain terrain"] && editor.designType == DesignType.Event
            && editor.layer == 1 && editor.cntX == 5 && editor.cntY == 3 && editor.angle == 90
            && editor.posX == 2 && editor.posY == 1 && editor.posZ == -2, "undo does not restore tool/mode/layer/brush settings");
        Check(map.updateCtrl.curCenterPos == oldCenter, "undo preserves the current camera/view center");
        CheckRegisteredEntities();
        Check(history.TryRedo() && OwnedObject().pos == new Vector3(11, 0, 11)
            && OwnedObject().euler == Vector3.up * 45 && !map.navigationCtrl.navUnits[(12, 0, 12)].objectBlocked,
            "movement redo rebuilds geometry and navigation at the new position");
        Check(history.TryUndo(), "movement fixture resets to its original position");
        history.Clear();
        Vector3Int originalView = map.data.mainData.viewSize;
        using (history.BeginOperation())
        {
            history.TrackMain();
            map.data.mainData.viewSize = originalView + Vector3Int.one;
        }
        Check(history.TryUndo() && map.data.mainData.viewSize == originalView, "MapMain authored configuration is reversible");
        history.Clear();
    }

    private static void ClearAndSceneReplacement()
    {
        using (history.BeginOperation()) SetTexture(Tile(), 3, Tile().texDic[3] + 1);
        Check(history.TryUndo() && history.redoCount == 1, "clear fixture has an available redo");
        var pending = history.BeginOperation();
        history.Track(Tile());
        Tile().texDic[4]++;
        history.Clear();
        pending.Dispose();
        pending.Dispose();
        Check(history.undoCount == 0 && history.redoCount == 0 && !history.TryUndo() && !history.TryRedo(),
            "scene exit clears both branches and ignores old/double-disposed operation scopes");
        var reentered = new ModSceneHistory(map, sceneUid);
        Check(!reentered.TryUndo() && !reentered.TryRedo(), "new scene entry has no inherited history");
        using (reentered.BeginOperation())
        {
            reentered.Track(Tile());
            Tile().texDic[3]++;
        }
        var originalMap = map.data;
        map.data = new MapInfo();
        try
        {
            Check(!reentered.TryUndo() && !reentered.TryRedo(), "old history cannot act on another scene MapInfo");
            using (reentered.BeginOperation()) { }
        }
        finally { map.data = originalMap; }
        reentered.Clear();
    }

    private static void AddedAndDeletedRows()
    {
        history.Clear();
        TileUnitForm.Data newTile;
        ObjectUnitForm.Data newObject;
        CharacterUnitForm.Data newCharacter;
        ItemUnitForm.Data newItem;
        using (history.BeginOperation())
        {
            newTile = map.AddTile(new Vector3Int(15, 0, 12));
            history.TrackAdded(newTile);
            newObject = map.AddObject("added object", new Vector3(13, 0, 11), "history-object");
            newObject.isObstacle = true;
            map.updateCtrl.RefreshHistoryUnit(newObject.unit);
            history.TrackAdded(newObject);
            newCharacter = map.AddCharacter("history character", new Vector3(13, 0, 11), "history-character",
                false, MapUnit.GetProductInfoString(new JObject(), (characterProductUid, -1)), 2);
            history.TrackAdded(newCharacter);
            newItem = map.AddItem("history item", new Vector3(13, 0, 11), "history-item");
            newItem.unit.productInfo = (itemProductUid, -1);
            history.TrackAdded(newItem);
        }
        var ids = new[] { newTile.uid, newObject.uid, newCharacter.uid, newItem.uid };
        Check(history.undoCount == 1 && history.lastCapturedRowCount == 4,
            "four added entity rows capture only their completed after values");
        Check(history.TryUndo() && ids.All(id => !UnitForm.DataByUid.ContainsKey(id))
            && !map.data.maps.ContainsKey((15, 0, 12)), "undo of creation removes all four newly allocated rows");
        Check(history.TryRedo() && ids.All(UnitForm.DataByUid.ContainsKey)
            && map.data.maps[(15, 0, 12)].uid == newTile.uid, "redo of creation restores every original UID");
        var owner = Tile(13, 11).unit;
        Check(ObjectUnitForm.DataByUid[newObject.uid].unit.belongTile == owner
            && CharacterUnitForm.DataByUid[newCharacter.uid].unit.belongTile == owner
            && ItemUnitForm.DataByUid[newItem.uid].unit.belongTile == owner,
            "created entity redo rebuilds Object/Character/Item ownership");
        Check(map.navigationCtrl.navUnits.ContainsKey((15, 0, 12))
            && map.navigationCtrl.navUnits[(13, 0, 11)].objectBlocked,
            "created terrain and obstacle redo update their local navigation cells");
        history.Clear();
        using (history.BeginOperation())
        {
            history.Track(ItemUnitForm.DataByUid[newItem.uid]);
            map.RemoveItem(ItemUnitForm.DataByUid[newItem.uid]);
            history.Track(CharacterUnitForm.DataByUid[newCharacter.uid]);
            map.RemoveCharacter(CharacterUnitForm.DataByUid[newCharacter.uid]);
            history.Track(ObjectUnitForm.DataByUid[newObject.uid]);
            map.RemoveObject(ObjectUnitForm.DataByUid[newObject.uid]);
            history.Track(TileUnitForm.DataByUid[newTile.uid]);
            map.RemoveTile(TileUnitForm.DataByUid[newTile.uid]);
        }
        Check(history.undoCount == 1 && history.lastCapturedRowCount == 4,
            "four deleted rows capture only their original before values");
        Check(history.TryUndo() && ids.All(UnitForm.DataByUid.ContainsKey), "undo of explicit deletion restores all four concrete rows");
        Check(history.TryRedo() && ids.All(id => !UnitForm.DataByUid.ContainsKey(id))
            && !map.data.maps.ContainsKey((15, 0, 12)), "redo of deletion cleans concrete/root registries and terrain");
        CheckRegisteredEntities();
        history.Clear();
    }

    private static void TerrainFootprintAndLayerRecovery()
    {
        history.Clear();
        var spanning = map.AddObject("empty-column obstacle", new Vector3(15, 0, 11), "history-object");
        spanning.isObstacle = true;
        spanning.scale = new Vector3(3, 1, 1);
        spanning.unit.InvalidateCollisionGeometry();
        map.updateCtrl.RefreshHistoryUnit(spanning.unit);
        var spanningUnit = spanning.unit;
        TileUnitForm.Data newTile;
        using (history.BeginOperation())
        {
            newTile = map.AddTile(new Vector3Int(16, 0, 11));
            newTile.texDic[0] = textureId;
            GameMapData.ApplyTilePassTypes(newTile);
            history.TrackAdded(newTile);
        }
        int newTileUid = newTile.uid;
        Check(history.TryUndo() && !map.data.maps.ContainsKey((16, 0, 11)),
            "undo removes terrain from a previously empty Object footprint column");
        Check(history.TryRedo() && TileUnitForm.DataByUid.ContainsKey(newTileUid)
            && map.navigationCtrl.navUnits[(16, 0, 11)].objectBlocked,
            "redo terrain refresh finds a pre-existing Object that crosses a formerly empty column");
        Check(map.updateCtrl.objectTileDic.Get(spanningUnit).Contains(Tile(16, 11).unit)
            && ReferenceEquals(ObjectUnitForm.DataByUid[spanning.uid].unit, spanningUnit),
            "new local terrain restores Object coverage without replacing the unaffected Object");
        history.Clear();
        map.RemoveObject(spanning);
        map.RemoveTile(Tile(16, 11));

        var above = map.AddCharacter("history character", new Vector3(11, 1, 13), "history-character", false,
            MapUnit.GetProductInfoString(new JObject(), (characterProductUid, -1)), 2);
        var aboveUnit = above.unit;
        map.updateCtrl.RefreshHistoryUnit(aboveUnit);
        Vector3 abovePos = above.pos;
        Check(aboveUnit.belongTile == Tile(11, 13).unit, "floating Character is initially owned by the lower Tile");
        TileUnitForm.Data upper;
        using (history.BeginOperation())
        {
            upper = map.AddTile(new Vector3Int(11, 1, 13));
            history.TrackAdded(upper);
        }
        int upperUid = upper.uid;
        Check(history.TryUndo() && aboveUnit.belongTile == Tile(11, 13).unit && above.pos == abovePos,
            "undo of upper terrain retains Character position and lower-layer ownership");
        Check(history.TryRedo() && aboveUnit.belongTile == TileUnitForm.DataByUid[upperUid].unit
            && ReferenceEquals(CharacterUnitForm.DataByUid[above.uid].unit, aboveUnit) && above.pos == abovePos,
            "redo of upper terrain rebinds the unchanged Character to its nearest lower/exact layer");
        history.Clear();
        using (history.BeginOperation())
        {
            history.Track(TileUnitForm.DataByUid[upperUid]);
            map.RemoveTile(TileUnitForm.DataByUid[upperUid]);
        }
        Check(aboveUnit.belongTile == Tile(11, 13).unit, "ordinary upper Tile deletion rebinds an existing Character downwards");
        Check(history.TryUndo() && aboveUnit.belongTile == TileUnitForm.DataByUid[upperUid].unit,
            "undo of terrain deletion restores Character owner and reverse index");
        Check(history.TryRedo() && aboveUnit.belongTile == Tile(11, 13).unit
            && map.updateCtrl.characterTileDic.Get(Tile(11, 13).unit).Contains(aboveUnit),
            "redo of terrain deletion restores lower Character ownership in both directions");
        history.Clear();
        map.RemoveCharacter(above);

        // The upper solid body's bottom occupies the lower floor. Replacing a
        // slope with mapground must update both this layer and its lower links.
        var slope = map.AddTile(new Vector3Int(11, 1, 11));
        slope.prefabName = MapInfo.GetPrefabName("mapslope");
        slope.unit.InvalidateCollisionGeometry();
        var neighbour = map.AddTile(new Vector3Int(11, 1, 10));
        map.navigationCtrl.RefreshTerrainTiles(new[] { slope.mapPos, neighbour.mapPos });
        var slopeKey = (11, 1, 11);
        var lowerKey = (11, 0, 11);
        var oldSamples = (float[])map.navigationCtrl.navUnits[slopeKey].dirGroundY.Clone();
        var oldLinks = map.navigationCtrl.navUnits[slopeKey].links.Select(link => link.pos).ToHashSet();
        var lowerLinks = map.navigationCtrl.navUnits[lowerKey].links.Select(link => link.pos).ToHashSet();
        Check(oldSamples.Distinct().Count() > 1 && lowerLinks.Count > 0,
            "slope fixture has directional samples and a walkable lower floor");
        using (history.BeginOperation())
        {
            history.Track(slope);
            slope.prefabName = MapInfo.GetPrefabName("mapground");
            slope.unit.InvalidateCollisionGeometry();
            map.navigationCtrl.RefreshTerrainTiles(new[] { slope.mapPos });
        }
        Check(map.navigationCtrl.navUnits[slopeKey].dirGroundY.All(y => Mathf.Abs(y - 1) < .001f)
            && map.navigationCtrl.navUnits[lowerKey].links.Count == 0,
            "mapground replacement flattens local samples and blocks the covered lower floor");
        Check(history.TryUndo() && map.navigationCtrl.navUnits[slopeKey].dirGroundY.SequenceEqual(oldSamples)
            && map.navigationCtrl.navUnits[slopeKey].links.Select(link => link.pos).ToHashSet().SetEquals(oldLinks)
            && map.navigationCtrl.navUnits[lowerKey].links.Select(link => link.pos).ToHashSet().SetEquals(lowerLinks),
            "local slope undo restores directional samples, edges and lower-floor passability");
        Check(history.TryRedo() && map.navigationCtrl.navUnits[lowerKey].links.Count == 0
            && map.navigationCtrl.navUnits[slopeKey].dirGroundY.All(y => Mathf.Abs(y - 1) < .001f),
            "local mapground redo restores upper samples and downward solid obstruction");
        history.Clear();
        map.RemoveTile(TileUnitForm.DataByUid[slope.uid]);
        map.RemoveTile(neighbour);
        CheckRegisteredEntities();
    }

    private static void TextureEraseUpdatesPassTypesImmediately()
    {
        history.Clear();
        var controller = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
        typeof(ModSceneController).GetField("enable", PrivateInstance).SetValue(controller, true);
        typeof(ModSceneController).GetField("history", PrivateInstance).SetValue(controller, history);
        controller.layer = 0;
        var erase = typeof(ModSceneController).GetMethod("EraseTile", PrivateInstance);
        var target = Tile(10, 14);
        GameMapData.ApplyTilePassTypes(target);
        map.navigationCtrl.RefreshObjectTiles(new[] { target.unit });
        Check(target.unit.passTypes.Contains(passTypeId)
            && map.navigationCtrl.navUnits[(10, 0, 14)].passTypes.Contains(passTypeId),
            "texture erase fixture begins with an authored terrain pass requirement");
        using (controller.BeginOperation())
            erase.Invoke(controller, new object[] { target, MapEraseForm.DataByName["texture only"] });
        Check(history.undoCount == 1 && history.lastCapturedRowCount == 2,
            "real texture-only erase tracks only its changed Tile row");
        Check(!Tile(10, 14).texDic.ContainsKey(0) && Tile(10, 14).unit.passTypes.Count == 0
            && map.navigationCtrl.navUnits[(10, 0, 14)].passTypes.Count == 0,
            "texture-only erase clears Tile and Nav pass requirements immediately without a full view update");
        Check(history.TryUndo() && Tile(10, 14).texDic[0] == textureId
            && Tile(10, 14).unit.passTypes.Contains(passTypeId)
            && map.navigationCtrl.navUnits[(10, 0, 14)].passTypes.Contains(passTypeId),
            "texture-only erase undo restores both authored texture and runtime Tile/Nav requirements");
        Check(history.TryRedo() && !Tile(10, 14).texDic.ContainsKey(0) && Tile(10, 14).unit.passTypes.Count == 0
            && map.navigationCtrl.navUnits[(10, 0, 14)].passTypes.Count == 0,
            "texture-only erase redo clears Tile/Nav requirements in the same operation");
        Check(history.TryUndo(), "texture erase fixture restores its initial Tile");
        history.Clear();
    }

    private static MapModelForm.Data StaticModel(int texture)
    {
        return new MapModelForm.Data(-1, new List<int>(), new List<Vector3>(), new List<Vector3>(),
            new List<List<int>> { new List<int> { texture } }, 0, false, 1);
    }

    private static Texture2D AddStaticTexture(int id)
    {
        var texture = new Texture2D(2, 2);
        TexAssetForm.DataById[id] = new TexAssetForm.Data(id, "history texture", "", null, "", texture, 0);
        return texture;
    }

    private static Texture ShownTexture(MapInstance instance)
    {
        var block = new MaterialPropertyBlock();
        instance.renderers[0].GetPropertyBlock(block);
        return block.GetTexture("_Tex");
    }

    private static void PlacementRotationAndImmediateAppearance()
    {
        history.Clear();
        var controller = (ModSceneController)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ModSceneController));
        typeof(ModSceneController).GetField("enable", PrivateInstance).SetValue(controller, true);
        typeof(ModSceneController).GetField("history", PrivateInstance).SetValue(controller, history);
        var mod = CreateManager<ModManager>();
        mod.sceneCtrl = controller;
        var asset = new ModAssetCtrl(mod);
        const int itemTextureId = 98001;
        var itemTexture = AddStaticTexture(itemTextureId);
        var product = ItemProductForm.GetDataByJo(new JObject());
        product.uid = -1;
        product.name = "real ModAsset placement item";
        product.paramDic = new Dictionary<string, ItemParamForm.Data>();
        product.model = StaticModel(itemTextureId);
        ItemProductForm.AddData(product);
        var itemPrefab = Prefab<ItemInstance>(GlobalDefaultHelper.GetRuntimeMapItemPrefabName(product.uid));
        AddBody(itemPrefab, Vector3.up * .1f, Vector3.one * .2f);
        itemPrefab.transform.GetChild(0).gameObject.AddComponent<MeshRenderer>();
        pool.AddPool(itemPrefab);
        var characterPrefab = Prefab<CharacterInstance>(GlobalDefaultHelper.GetRuntimePrefabName("character"));
        var collider = characterPrefab.AddComponent<SphereCollider>();
        collider.center = Vector3.up * .5f;
        collider.radius = .4f;
        pool.AddPool(characterPrefab);
        map.SetPos(new Vector3(12, 0, 12));
        // EditMode has no Update loop; establish the production visibility
        // rectangle before measuring individual placement operations.
        map.updateCtrl.RefreshView();
        // Stay away from the pre-existing large Character, whose side contact
        // would hide the ground gap introduced by the .4 physical radius.
        Vector3 position = new Vector3(10, 0, 10);
        Check(Tile(10, 10).unit.isShowing, "real placement fixture uses a Tile inside the current visible rectangle");
        var addedItem = asset.AddItem(product, position, 35);
        Check(addedItem != null && history.undoCount == 1 && history.lastCapturedRowCount == 1,
            "real ModAsset AddItem records only its completed row");
        Check(addedItem.euler.y == 35 && addedItem.unit.ins != null
            && Mathf.Abs(Mathf.DeltaAngle(addedItem.unit.ins.transform.eulerAngles.y, 35)) < .001f,
            "ModAsset Item placement immediately applies nonzero rotation to data and pooled instance");
        Check(addedItem.unit.productInfo.Item1 == product.uid && ShownTexture(addedItem.unit.ins) == itemTexture,
            "ModAsset Item placement refreshes only the new item's appearance after product metadata assignment");
        Check(asset.AddItem(product, position, 35) == null && history.undoCount == 1,
            "duplicate ModAsset Item placement is a no-op");
        int itemUid = addedItem.uid;
        Check(history.TryUndo() && !ItemUnitForm.DataByUid.ContainsKey(itemUid), "real Item placement undo removes its row");
        Check(history.TryRedo() && ItemUnitForm.DataByUid[itemUid].euler.y == 35
            && ShownTexture(ItemUnitForm.DataByUid[itemUid].unit.ins) == itemTexture,
            "real Item placement redo restores its original UID, angle and texture in the same operation");
        Check(history.TryUndo(), "Item placement fixture cleans up through history");
        history.Clear();
        var characterProduct = CharacterProductForm.DataByUid[characterProductUid];
        var addedCharacter = asset.AddCharacter(characterProduct, position, 137);
        Check(addedCharacter != null && history.undoCount == 1 && history.lastCapturedRowCount == 1,
            "real ModAsset AddCharacter records only its completed row");
        Check(addedCharacter.euler.y == 137 && addedCharacter.unit.ins != null
            && Mathf.Abs(Mathf.DeltaAngle(addedCharacter.unit.ins.transform.eulerAngles.y, 137)) < .001f,
            "ModAsset Character placement immediately applies nonzero rotation without a full-view refresh");
        Check(addedCharacter.scale == Vector3.one * 2 && addedCharacter.unit.passTypes.Contains(passTypeId)
            && addedCharacter.unit.belongTile == Tile(10, 10).unit,
            "real Character placement registers product size, capabilities and owner immediately");
        Check(!(bool)typeof(CharacterUnit).GetMethod("HasGroundContact", PrivateInstance).Invoke(addedCharacter.unit, null),
            "radius .4 Character has a ground gap at its authored position, reproducing the gravity drift condition");
        Vector3 pendingRotation = new Vector3(0, 221, 0);
        addedCharacter.unit.forceEuler = pendingRotation;
        var updateFrame = typeof(Unit).GetField("lastUpdateFrame", PrivateInstance);
        var updateProbe = new CharacterUpdateProbe { unit = addedCharacter.unit };
        updateProbe.Register();
        try
        {
            for (int frame = 0; frame < 3; frame++)
            {
                // The synchronous EditMode fixture cannot advance Time.frameCount.
                updateFrame.SetValue(addedCharacter.unit, -1);
                controller.Update();
            }
        }
        finally { updateProbe.Unregister(); }
        Check(addedCharacter.pos == position && addedCharacter.euler.y == 137
            && addedCharacter.unit.forceEuler == pendingRotation,
            "ModScene frame refresh leaves authored position and pending gameplay rotation unchanged");
        Check(updateProbe.count == 3, "ModScene frames preserve Character AfterUpdate appearance callbacks");
        updateFrame.SetValue(addedCharacter.unit, -1);
        addedCharacter.unit.UpdateInfo();
        Check(addedCharacter.unit.forceEuler == null && addedCharacter.euler.y == 221,
            "normal runtime Character update still consumes gameplay rotation after an editor refresh");
        map.updateCtrl.ApplyMove(addedCharacter.unit, position, new Vector3(0, 137, 0), true);
        Check(asset.AddCharacter(characterProduct, position, 137) == null && history.undoCount == 1,
            "repeated Character placement after editor frames creates no duplicate or history operation");
        Check(asset.AddCharacter(characterProduct, position, 221) == null && history.undoCount == 1,
            "same product at the same position is a duplicate regardless of placement angle");
        string originalCharacterName = addedCharacter.name;
        addedCharacter.name = "renamed scene instance";
        Check(asset.AddCharacter(characterProduct, position, 137) == null,
            "Character placement deduplicates by product identity rather than an editable instance name");
        addedCharacter.name = originalCharacterName;
        int characterUid = addedCharacter.uid;
        Check(history.TryUndo() && !CharacterUnitForm.DataByUid.ContainsKey(characterUid), "real Character placement undo removes its row");
        Check(history.TryRedo() && CharacterUnitForm.DataByUid[characterUid].euler.y == 137
            && CharacterUnitForm.DataByUid[characterUid].unit.ins != null
            && Mathf.Abs(Mathf.DeltaAngle(CharacterUnitForm.DataByUid[characterUid].unit.ins.transform.eulerAngles.y, 137)) < .001f,
            "real Character placement redo restores its angle and pooled instance immediately");
        Check(history.TryUndo(), "Character placement fixture cleans up through history");
        history.Clear();
    }

    private static void RaisedObjectPlacementUsesLowerOwner()
    {
        history.Clear();
        var mod = ModManager.instance;
        var previousController = mod.sceneCtrl;
        var previousAsset = mod.assetCtrl;
        var controller = new ModSceneController(mod);
        mod.sceneCtrl = controller;
        mod.assetCtrl = new ModAssetCtrl(mod);
        typeof(ModSceneController).GetField("enable", PrivateInstance).SetValue(controller, true);
        typeof(ModSceneController).GetField("history", PrivateInstance).SetValue(controller, history);
        var waitField = typeof(ModSceneController).GetField("waitForActive", PrivateInstance);
        var product = new MapObjectForm.Data(-1, "raised placement object", 0, StaticModel(98001), 0, false,
            new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<string, MapObjectParamForm.Data>(),
            0, FaceType.Fixed, new Dictionary<AnimDirecton, List<int>>
                { [AnimDirecton.Fixed] = new List<int> { 98001 } }, false, false, true);
        MapObjectForm.AddData(product);
        var prefab = Prefab<ObjectInstance>(GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(product.id));
        AddBody(prefab, Vector3.up * .5f, Vector3.one);
        prefab.transform.GetChild(0).gameObject.AddComponent<MeshRenderer>();
        pool.AddPool(prefab);
        Vector3Int previousViewSize = map.data.mainData.viewSize;
        // Include both candidate owner layers in the fixture's visible range;
        // ordinary pooled entities correctly stay hidden for offscreen owners.
        map.data.mainData.viewSize = new Vector3Int(previousViewSize.x, 3, previousViewSize.z);
        map.SetPos(new Vector3(12, 0, 12));
        map.updateCtrl.RefreshView();
        var upper = map.AddTile(new Vector3Int(10, 2, 10));
        var aboveOnly = map.AddTile(new Vector3Int(16, 2, 10));
        Check(upper.unit.isShowing && Tile(10, 10).unit.isShowing,
            "raised-placement fixture includes both exact/lower owners in the visible range");
        var camera = CameraInstance.instance;
        var cam = camera.cam;
        Vector3 previousCameraPosition = cam.transform.position;
        Quaternion previousCameraRotation = cam.transform.rotation;
        Vector3 previousTarget = camera.tarTrs.position;
        Vector3 previousCellSize = map.data.mainData.mapUnitSize;
        try
        {
            controller.curData = product;
            controller.cntX = controller.cntY = 1;
            controller.angle = 73;
            var tool = new Ui.ModSceneMain.ModTool.UiModToolModel();
            tool.posY = "6.25";
            Check(controller.posY == 6.25f && tool.posY == "6.25",
                "Object height input no longer clamps at .9 or the highest map layer");
            foreach (float height in new[] { 6.25f, 3.25f, 2f, 1.75f, 1f, .9f, 0f })
            {
                Vector3 position = new Vector3(10, height, 10);
                TileUnit expectedOwner = map.utilCtrl.RealPos2MapPosInt(position).y >= 2 ? upper.unit : Tile(10, 10).unit;
                var added = mod.assetCtrl.AddObject(product, position, 73);
                Check(added != null && added.pos == position && added.euler.y == 73,
                    "raised Object preserves requested position and rotation at height " + height);
                Check(added.unit.belongTile == expectedOwner
                    && map.updateCtrl.objectTileDic.Get(added.unit)[0] == expectedOwner
                    && map.updateCtrl.objectTileDic.Get(expectedOwner).Contains(added.unit),
                    "Object uses nearest lower owner first in both index directions at height " + height);
                Check(added.unit.ins != null && added.unit.ins.transform.position == position,
                    "raised Object is shown through its lower owner without snapping downward");
                Check(history.undoCount == 1 && mod.assetCtrl.AddObject(product, position, 73) == null
                    && history.undoCount == 1,
                    "raised placement remains one operation and rejects duplicates");
                int uid = added.uid;
                Check(history.TryUndo() && !ObjectUnitForm.DataByUid.ContainsKey(uid),
                    "raised Object placement undo removes its original row");
                Check(history.TryRedo() && ObjectUnitForm.DataByUid[uid].pos == position
                    && ObjectUnitForm.DataByUid[uid].unit.belongTile == expectedOwner,
                    "raised Object placement redo preserves height and reconstructs lower owner");
                var restored = ObjectUnitForm.DataByUid[uid];
                map.updateCtrl.RefreshHistoryUnit(restored.unit);
                Check(restored.pos == position && restored.unit.belongTile == expectedOwner,
                    "entity rebind keeps the same requested height and lower owner");
                var editor = new UiModSceneUnitModel { data = restored };
                editor.posY = GameManager.MapPosToPlayerPos(8.25f).ToString();
                Check(restored.pos.y == 8.25f && restored.unit.belongTile == upper.unit,
                    "Object coordinate editor also allows height above its owner and the highest Tile");
                Check(history.TryUndo() && ObjectUnitForm.DataByUid[uid].pos == position
                    && ObjectUnitForm.DataByUid[uid].unit.belongTile == expectedOwner,
                    "Object height edit undo restores requested position and lower owner");
                Check(history.TryUndo(), "raised Object fixture removes the completed placement");
                int rowCount = ObjectUnitForm.DataByUid.Count;
                int redoCount = history.redoCount;
                Check(mod.assetCtrl.AddObject(product, new Vector3(16, 1, 10)) == null
                    && mod.assetCtrl.AddObject(product, new Vector3(17, 6, 10)) == null,
                    "Object placement rejects an above-only column or a completely empty column");
                Check(ObjectUnitForm.DataByUid.Count == rowCount && history.undoCount == 0
                    && history.redoCount == redoCount,
                    "unsupported placements create no row/history and preserve the existing redo branch");
                history.Clear();
            }

            var itemProduct = ItemProductForm.DataByUid.Values.Single(row => row.name == "real ModAsset placement item");
            var itemBrush = new MapItemForm.Data(-1, "raised Item brush", 0, itemProduct.model, 0, itemProduct.uid);
            MapItemForm.AddData(itemBrush);
            var characterProduct = CharacterProductForm.DataByUid[characterProductUid];
            var characterBrush = new MapCharacterForm.Data(-1, "raised Character brush", 0, 0, characterProductUid);
            MapCharacterForm.AddData(characterBrush);
            void VerifyOtherPlacement(MapBaseForm.Data brush, Func<Vector3, UnitForm.Data> place)
            {
                controller.curData = brush;
                tool.posY = "6.25";
                Check(controller.posY == 6.25f, brush.name + " removes the same height input limit");
                foreach (float height in new[] { 6.25f, 3.25f, 2f, 1f, .9f, 0f })
                {
                    Vector3 position = new Vector3(10, height, 10);
                    TileUnit expectedOwner = map.utilCtrl.RealPos2MapPosInt(position).y >= 2 ? upper.unit : Tile(10, 10).unit;
                    UnitForm.Data added = place(position);
                    Check(added != null && added.pos == position && added.euler.y == 73
                        && ((MapUnit)added.unit).belongTile == expectedOwner,
                        brush.name + " keeps requested height/rotation and nearest lower owner: " + height);
                    Check(added.unit.ins != null && added.unit.ins.transform.position == position,
                        brush.name + " shows immediately through its lower owner: height=" + height
                        + "; ownerShown=" + expectedOwner.isShowing + "; instance=" + added.unit.ins
                        + "; instancePos=" + (added.unit.ins == null ? "null" : added.unit.ins.transform.position.ToString()));
                    Check(place(position) == null && history.undoCount == 1,
                        brush.name + " repeated placement is a history-free no-op");
                    int uid = added.uid;
                    Check(history.TryUndo() && !UnitForm.DataByUid.ContainsKey(uid),
                        brush.name + " raised placement undo removes its row");
                    Check(history.TryRedo() && UnitForm.DataByUid[uid].pos == position
                        && ((MapUnit)UnitForm.DataByUid[uid].unit).belongTile == expectedOwner,
                        brush.name + " redo reconstructs requested position and lower owner");
                    var editor = new UiModSceneUnitModel { data = UnitForm.DataByUid[uid] };
                    editor.posY = GameManager.MapPosToPlayerPos(8.25f).ToString();
                    Check(UnitForm.DataByUid[uid].pos.y == 8.25f
                        && ((MapUnit)UnitForm.DataByUid[uid].unit).belongTile == upper.unit,
                        brush.name + " coordinate editor also removes its owner-relative upper height cap");
                    Check(history.TryUndo() && UnitForm.DataByUid[uid].pos == position,
                        brush.name + " height edit remains independently undoable");
                    Check(history.TryUndo(), brush.name + " fixture removes its completed placement");
                    int rows = UnitForm.DataByUid.Count;
                    int redo = history.redoCount;
                    Check(place(new Vector3(16, 1, 10)) == null && place(new Vector3(17, 6, 10)) == null
                        && UnitForm.DataByUid.Count == rows && history.undoCount == 0 && history.redoCount == redo,
                        brush.name + " rejects above-only/empty columns without a row or history mutation");
                    history.Clear();
                }
                camera.tarTrs.position = new Vector3(10, 1, 10);
                cam.transform.position = new Vector3(10, 10, 10);
                cam.transform.eulerAngles = Vector3.right * 90;
                controller.posY = 0;
                Vector3 screen = cam.WorldToScreenPoint(new Vector3(10, 1, 10));
                waitField.SetValue(controller, false);
                controller.OnEvent(new InputMouseDownEvent { id = 0, pos = screen });
                controller.OnEvent(new InputMouseUpEvent { id = 0, pos = screen });
                Check(history.undoCount == 1 && history.TryUndo(),
                    brush.name + " actual click accepts and undoes an empty camera layer with lower support");
                history.Clear();
            }
            VerifyOtherPlacement(itemBrush, position => mod.assetCtrl.AddItem(itemProduct, position, 73));
            VerifyOtherPlacement(characterBrush, position => mod.assetCtrl.AddCharacter(characterProduct, position, 73));
            controller.curData = product;

            // The camera's current layer has a hole, while ground exists below.
            // Exercise actual clicks, not just the direct AddScene/asset API.
            camera.tarTrs.position = new Vector3(10, 1, 10);
            cam.transform.position = new Vector3(10, 10, 10);
            cam.transform.eulerAngles = Vector3.right * 90;
            controller.posY = 0;
            Vector3 screenPosition = cam.WorldToScreenPoint(new Vector3(10, 1, 10));
            int initialCount = ObjectUnitForm.DataByUid.Count;
            waitField.SetValue(controller, false);
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = screenPosition });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = screenPosition });
            Check(ObjectUnitForm.DataByUid.Count == initialCount + 1 && history.undoCount == 1,
                "actual Object click in an empty camera layer accepts a lower Tile");
            var clicked = ObjectUnitForm.DataByUid.Values.Single(row => row.unit.productInfo.Item1 == product.id);
            Check(Mathf.Abs(clicked.pos.y - 1) < .001f && clicked.unit.belongTile == Tile(10, 10).unit,
                "camera-layer hole placement retains camera height rather than lower owner height");
            Check(history.TryUndo(), "camera-layer hole click is undoable");
            history.Clear();
            controller.posY = 4.25f;
            waitField.SetValue(controller, false);
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = screenPosition });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = screenPosition });
            clicked = ObjectUnitForm.DataByUid.Values.Single(row => row.unit.productInfo.Item1 == product.id);
            Check(Mathf.Abs(clicked.pos.y - 5.25f) < .001f && clicked.unit.belongTile == upper.unit,
                "actual raised click finds the nearest lower Tile across empty layers");
            map.updateCtrl.ApplyMove(clicked.unit, new Vector3(10, .9f, 10), clicked.euler, true);
            Check(clicked.unit.belongTile == Tile(10, 10).unit && clicked.pos.y == .9f,
                "Object movement recomputes nearest lower owner without changing requested height");
            Check(history.TryUndo(), "raised-click fixture removes its row");
            history.Clear();
            camera.tarTrs.position = new Vector3(16, 1, 10);
            cam.transform.position = new Vector3(16, 10, 10);
            controller.posY = 0;
            screenPosition = cam.WorldToScreenPoint(new Vector3(16, 1, 10));
            waitField.SetValue(controller, false);
            controller.OnEvent(new InputMouseDownEvent { id = 0, pos = screenPosition });
            controller.OnEvent(new InputMouseUpEvent { id = 0, pos = screenPosition });
            Check(ObjectUnitForm.DataByUid.Count == initialCount && history.undoCount == 0,
                "actual click rejects a hole without lower support even when a Tile exists above");

            map.data.mainData.mapUnitSize = new Vector3(2, 1.5f, 3);
            tool.posY = "2.5";
            Check(controller.posY == 3.75f && tool.posY == "2.5",
                "unbounded Object height input still converts logical layers to world height");
            Check(map.utilCtrl.GetPlacementTile(new Vector3(20, 1.5f, 30)) == Tile(10, 10).unit
                && map.utilCtrl.GetPlacementTile(new Vector3(20, 4.875f, 30)) == upper.unit,
                "nearest-lower Object ownership uses the existing map cell coordinate conversion");
        }
        finally
        {
            map.data.mainData.mapUnitSize = previousCellSize;
            controller.CommitPendingOperation();
            controller.Unregister<InputKeyEvent>();
            controller.Unregister<InputMouseEvent>();
            controller.Unregister<InputMouseDownEvent>();
            controller.Unregister<InputMouseUpEvent>();
            controller.Unregister<InputMouseScrollEvent>();
            controller.Unregister<TileEvent>();
            controller.Unregister<ObjectEvent>();
            foreach (var row in ObjectUnitForm.DataByUid.Values.Where(row => row.unit.productInfo.Item1 == product.id).ToArray())
                map.RemoveObject(row);
            map.RemoveTile(upper);
            map.RemoveTile(aboveOnly);
            history.Clear();
            map.data.mainData.viewSize = previousViewSize;
            map.updateCtrl.RefreshView();
            camera.tarTrs.position = previousTarget;
            cam.transform.SetPositionAndRotation(previousCameraPosition, previousCameraRotation);
            mod.sceneCtrl = previousController;
            mod.assetCtrl = previousAsset;
        }
    }

    private static void ObjectWangTileHistoryNeighbours()
    {
        history.Clear();
        const int disconnectedId = 98002;
        const int connectedId = 98003;
        var disconnected = AddStaticTexture(disconnectedId);
        var connected = AddStaticTexture(connectedId);
        var product = new MapObjectForm.Data(-1, "history Wang Object", 0, StaticModel(disconnectedId), 0, false,
            new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<string, MapObjectParamForm.Data>(),
            0, FaceType.Fixed, new Dictionary<AnimDirecton, List<int>> { [AnimDirecton.Fixed] = new List<int> { disconnectedId } }, true, false, true);
        MapObjectForm.AddData(product);
        var prefab = Prefab<ObjectInstance>("history-Wang-object");
        AddBody(prefab, Vector3.up * .5f, Vector3.one);
        prefab.transform.GetChild(0).gameObject.AddComponent<MeshRenderer>();
        pool.AddPool(prefab);
        var register = typeof(GameMapController).GetMethod("RegisterObjectWangTileAnimationTexture", PrivateStatic);
        register.Invoke(null, new object[] { product.id, AnimDirecton.Fixed, 0, disconnectedId });
        register.Invoke(null, new object[] { product.id, AnimDirecton.Fixed, 1 << 4, connectedId });
        register.Invoke(null, new object[] { product.id, AnimDirecton.Fixed, 1 << 3, connectedId });
        map.SetPos(new Vector3(12, 0, 12));
        map.updateCtrl.RefreshView();
        var left = map.AddObject(product.name, new Vector3(11, 0, 11), prefab.name);
        game.mapCtrl.RegisterObject(left, product);
        map.updateCtrl.UpdateSingleOne(left.unit);
        var moving = map.AddObject(product.name, new Vector3(12, 0, 11), prefab.name);
        game.mapCtrl.RegisterObject(moving, product);
        map.updateCtrl.UpdateSingleOne(moving.unit);
        Check(left.unit.ins != null && moving.unit.ins != null
            && ShownTexture(left.unit.ins) == connected && ShownTexture(moving.unit.ins) == connected,
            "Wang Object fixture has touching same-product visible neighbours");
        var leftUnit = left.unit;
        var leftInstance = leftUnit.ins;
        int movingUid = moving.uid;
        // Exercise the actual coordinate editor's local refresh contract. A
        // bare geometry ApplyMove does not own Object appearance events.
        typeof(ModSceneController).GetField("waitForActive", PrivateInstance)
            .SetValue(ModManager.instance.sceneCtrl, true);
        var model = new UiModSceneUnitModel { data = moving };
        model.posX = GameManager.MapPosToPlayerPos(13).ToString();
        Check(ShownTexture(leftUnit.ins) == disconnected && history.lastCapturedRowCount == 2,
            "ordinary Object movement refreshes its old Wang neighbour and captures only the moved row");
        Check(history.TryUndo() && ShownTexture(leftUnit.ins) == connected
            && ShownTexture(ObjectUnitForm.DataByUid[movingUid].unit.ins) == connected,
            "local movement undo restores both Wang adjacency masks");
        Check(history.TryRedo() && ShownTexture(leftUnit.ins) == disconnected
            && ShownTexture(ObjectUnitForm.DataByUid[movingUid].unit.ins) == disconnected,
            "local movement redo consumes cached old bounds before cache eviction and refreshes departed neighbours");
        Check(ReferenceEquals(ObjectUnitForm.DataByUid[left.uid], left)
            && ReferenceEquals(left.unit, leftUnit) && ReferenceEquals(leftUnit.ins, leftInstance),
            "Wang neighbour appearance refresh preserves its untouched row, unit and instance");
        Check(history.TryUndo(), "Wang movement fixture returns to the connected position");
        history.Clear();
        using (history.BeginOperation())
        {
            history.Track(ObjectUnitForm.DataByUid[movingUid]);
            map.RemoveObject(ObjectUnitForm.DataByUid[movingUid]);
        }
        Check(ShownTexture(leftUnit.ins) == disconnected, "normal Wang Object deletion refreshes its old neighbour");
        Check(history.TryUndo() && ShownTexture(leftUnit.ins) == connected,
            "local Wang Object deletion undo restores neighbour appearance");
        Check(history.TryRedo() && ShownTexture(leftUnit.ins) == disconnected,
            "local Wang Object deletion redo refreshes neighbour appearance without a global Object scan");
        history.Clear();
        map.RemoveObject(left);
    }

    private static void LargeMapIncrementalHistory()
    {
        // Preparation is deliberately outside every measured transaction. These
        // remote cells cannot be reached by a local neighbour/footprint refresh.
        for (int x = 100; x < 164; x++)
        for (int z = 100; z < 164; z++)
        {
            var tile = new TileUnitForm.Data(80000 + (x - 100) * 64 + z - 100, "remote tile",
                new Dictionary<int, int> { [0] = textureId, [3] = 0 }, new Vector3Int(x, 0, z),
                MapInfo.GetPrefabName("mapfloor"), new Vector3(x, 0, z), Vector3.zero, Vector3.one,
                UpdateType.ShowOnly, new List<int>(), "", true, false, new List<int>());
            TileUnitForm.AddData(tile);
            map.data.RegisterMap(tile);
            GameMapData.ApplyTilePassTypes(tile);
        }
        map.navigationCtrl.Build();
        history.Clear();
        Check(TileUnitForm.DataByUid.Count >= 4000, "performance fixture contains at least four thousand registered Tiles");
        var target = Tile(10, 12);
        var remote = Tile(160, 160);
        var remoteUnit = remote.unit;
        remoteUnit.Show();
        var remoteInstance = remoteUnit.ins;
        var originalNavController = map.navigationCtrl;
        var originalNavDictionary = map.navigationCtrl.navUnits;
        var remoteNav = map.navigationCtrl.navUnits[(160, 0, 160)];
        var remoteNavLinks = remoteNav.links;
        var remoteNavGround = remoteNav.dirGroundY;
        var remoteNavHeights = remoteNav.dirMaxY;
        var remoteNavPassTypes = remoteNav.passTypes;
        var originalUpdateController = map.updateCtrl;
        var untouchedObject = OwnedObject();
        var untouchedObjectUnit = untouchedObject.unit;
        var untouchedCharacter = CharacterUnitForm.DataByUid[CharacterUid];
        var untouchedCharacterUnit = untouchedCharacter.unit;
        var untouchedItem = ItemUnitForm.DataByUid[ItemUid];
        var untouchedItemUnit = untouchedItem.unit;
        TileUnitForm.GetJoByData(target); // Warm just the touched row's lazy fields.
        int serializedUnits = 0;
        int serializedMain = 0;
        int serializedScene = 0;
        var serializedUids = new HashSet<int>();
        Action<UnitForm.Data> onUnitSerialize = data => { serializedUnits++; serializedUids.Add(data.uid); };
        Action<MapMainForm.Data> onMainSerialize = _ => serializedMain++;
        Action<SceneForm.Data> onSceneSerialize = _ => serializedScene++;
        UnitForm.beforeGetAction += onUnitSerialize;
        MapMainForm.beforeGetAction += onMainSerialize;
        SceneForm.beforeGetAction += onSceneSerialize;
        try
        {
            // Some Unity Mono versions expose this API but always return zero.
            // Do not mistake that stub for proof of zero allocations. The fallback
            // is a retained heap delta, not an exact cumulative allocation count.
            long beforeProbe = GC.GetAllocatedBytesForCurrentThread();
            var allocationProbe = new byte[4096];
            bool allocationCounterSupported = GC.GetAllocatedBytesForCurrentThread() - beforeProbe >= allocationProbe.Length;
            GC.KeepAlive(allocationProbe);
            GC.Collect();
            long beforeEmpty = ReadMemory();
            using (history.BeginOperation()) { }
            long emptyBytes = ReadMemory() - beforeEmpty;
            Check(serializedUnits == 0 && serializedMain == 0 && serializedScene == 0
                && history.lastCapturedRowCount == 0, "an empty Begin/End never serializes any Form row on a large map");
            Check(emptyBytes < 16 * 1024, "an empty transaction has bounded small allocation: " + emptyBytes);
            int initial = target.texDic[3];
            GC.Collect();
            long beforeEdit = ReadMemory();
            using (history.BeginOperation())
            {
                history.Track(target);
                target.texDic[3] = initial + 1;
                using (history.BeginOperation())
                {
                    history.Track(target);
                    target.texDic[3] = initial + 2;
                }
                history.Track(target);
                target.texDic[3] = initial + 3;
                Check(history.pendingRowCount == 1, "repeated/nested tracking holds exactly one changed row");
            }
            long editBytes = ReadMemory() - beforeEdit;
            Check(history.lastCapturedRowCount == 2 && serializedUnits == 2
                && serializedUids.SetEquals(new[] { target.uid }) && serializedMain == 0 && serializedScene == 0,
                "single-Tile history serializes exactly that row once before and once after");
            Check(editBytes < 256 * 1024, "single-row recording allocation is independent of four-thousand-Tile map: " + editBytes);
            serializedUnits = serializedMain = serializedScene = 0;
            serializedUids.Clear();
            Check(history.TryUndo() && Tile(10, 12).texDic[3] == initial, "large-map local undo restores the touched row");
            Check(serializedUnits == 0 && serializedMain == 0 && serializedScene == 0,
                "undo consumes recorded JSON without serializing current map Forms");
            CheckUntouchedIdentity();
            Check(history.TryRedo() && Tile(10, 12).texDic[3] == initial + 3,
                "large-map local redo restores the touched row");
            Check(serializedUnits == 0 && serializedMain == 0 && serializedScene == 0,
                "redo does not serialize untouched rows or full-map state");
            CheckUntouchedIdentity();
            history.Clear();

            Vector3Int oldViewSize = map.data.mainData.viewSize;
            using (history.BeginOperation())
            {
                history.TrackMain();
                map.data.mainData.viewSize = oldViewSize + Vector3Int.one;
            }
            Check(serializedUnits == 0 && serializedMain == 2 && serializedScene == 0,
                "view-size changes serialize only their MapMain row");
            Check(history.TryUndo() && map.data.mainData.viewSize == oldViewSize,
                "large-map view-size undo succeeds locally");
            CheckUntouchedIdentity();
            history.Clear();
            Debug.Log("MOD_SCENE_HISTORY_INCREMENTAL_METRICS tiles=" + TileUnitForm.DataByUid.Count
                + " metric=" + (allocationCounterSupported ? "allocatedBytes" : "monoUsedHeapDelta")
                + " emptyBytes=" + emptyBytes + " editBytes=" + editBytes + " capturedRows=2");

            long ReadMemory() => allocationCounterSupported
                ? GC.GetAllocatedBytesForCurrentThread()
                : UnityEngine.Profiling.Profiler.GetMonoUsedSizeLong();
        }
        finally
        {
            UnitForm.beforeGetAction -= onUnitSerialize;
            MapMainForm.beforeGetAction -= onMainSerialize;
            SceneForm.beforeGetAction -= onSceneSerialize;
            remoteUnit.Hide();
        }

        void CheckUntouchedIdentity()
        {
            Check(ReferenceEquals(Tile(160, 160), remote) && ReferenceEquals(Tile(160, 160).unit, remoteUnit)
                && ReferenceEquals(remoteUnit.ins, remoteInstance) && remoteUnit.isShowing,
                "local history preserves an untouched Tile Data, Unit and pooled instance");
            Check(ReferenceEquals(map.navigationCtrl, originalNavController)
                && ReferenceEquals(map.navigationCtrl.navUnits, originalNavDictionary)
                && ReferenceEquals(map.navigationCtrl.navUnits[(160, 0, 160)], remoteNav)
                && ReferenceEquals(remoteNav.links, remoteNavLinks)
                && ReferenceEquals(remoteNav.dirGroundY, remoteNavGround)
                && ReferenceEquals(remoteNav.dirMaxY, remoteNavHeights)
                && ReferenceEquals(remoteNav.passTypes, remoteNavPassTypes),
                "local history never rebuilds the controller, whole graph, or remote navigation state");
            Check(ReferenceEquals(map.updateCtrl, originalUpdateController)
                && ReferenceEquals(OwnedObject(), untouchedObject) && ReferenceEquals(OwnedObject().unit, untouchedObjectUnit)
                && ReferenceEquals(CharacterUnitForm.DataByUid[CharacterUid], untouchedCharacter)
                && ReferenceEquals(CharacterUnitForm.DataByUid[CharacterUid].unit, untouchedCharacterUnit)
                && ReferenceEquals(ItemUnitForm.DataByUid[ItemUid], untouchedItem)
                && ReferenceEquals(ItemUnitForm.DataByUid[ItemUid].unit, untouchedItemUnit),
                "local history preserves untouched entity Data/Units and all update index owners");
        }
    }

    private static void TranslatedEmptyHistoryTip()
    {
        var original = LanguageManager.instance.language;
        try
        {
            LanguageManager.instance.SetLanguage(Language.En);
            Check(TextManager.instance.GetTxt("noAvailableOperations") == "No available operations", "empty history tip has the English translation");
            LanguageManager.instance.SetLanguage(Language.Cn);
            Check(TextManager.instance.GetTxt("noAvailableOperations") == "没有可用的操作", "empty history tip has the Chinese translation");
        }
        finally { LanguageManager.instance.SetLanguage(original); }
    }
}
