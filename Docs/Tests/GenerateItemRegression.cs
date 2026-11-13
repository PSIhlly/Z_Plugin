using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Production GameUtil, Map, appearance and inventory APIs in a disposable project.
public static class GenerateItemRegression
{
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const string ParamName = "PotionValue";
    private static readonly List<GameObject> roots = new List<GameObject>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static readonly Probe probe = new Probe();
    private static GameManager game;
    private static PlayManager play;
    private static MapManager map;
    private static InstancePoolManager pools;
    private static PlayInfoController info;
    private static ProgressForm.Data progress;
    private static TileUnit owner;
    private static ItemProductForm.Data source;
    private static Material material;
    private static Texture2D firstTexture, secondTexture;
    private static string sourceJson, scratch;
    private static int checks;
    private static readonly Vector3 cellSize = new Vector3(2, 3, 4);
    private static readonly Vector3 authorPosition = new Vector3(2.25f, .5f, .75f);
    private static Vector3 WorldPosition => authorPosition + Vector3.one * 500f;

    public static void Run()
    {
        bool oldPlaying = DynamicGlobalSettings.playing;
        var oldCamera = DynamicGlobalSettings.cameraMode;
        var oldClock = TimeManager.animationTimeGetter;
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name
                .StartsWith("MapRuntimeRegression-", StringComparison.Ordinal), "isolated project guard");
            SetUp();
            MetadataAndInvalidArguments();
            UnsupportedPlacementAndResources();
            ForeignPoolRollback();
            GenerateAppearanceAndCopies();
            InterpreterAndMapRoundTrip();
            Check(ItemProductForm.GetJoByData(source).ToString() == sourceJson,
                "all generation, inventory and save operations preserve the original authored Item Product");
            Debug.Log("GENERATE_ITEM_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            probe.Unregister<ItemEvent>();
            if (info != null)
            {
                info.Unregister<CollideEvent>();
                info.Unregister<MissionEvent>();
            }
            if (game?.mapCtrl != null)
            {
                game.mapCtrl.Unregister<ItemEvent>();
                game.mapCtrl.Unregister<ObjectEvent>();
                game.mapCtrl.Unregister<TileEvent>();
            }
            if (map?.data != null) map.End();
            if (pools?.pools != null) pools.pools.Clear();
            TimeManager.animationTimeGetter = oldClock;
            DynamicGlobalSettings.playing = oldPlaying;
            DynamicGlobalSettings.cameraMode = oldCamera;
            foreach (var root in roots.AsEnumerable().Reverse())
                if (root != null) Object.DestroyImmediate(root);
            if (material != null) Object.DestroyImmediate(material);
            if (firstTexture != null) Object.DestroyImmediate(firstTexture);
            if (secondTexture != null) Object.DestroyImmediate(secondTexture);
            if (!string.IsNullOrEmpty(scratch) && Directory.Exists(scratch)
                && Path.GetFileName(scratch).StartsWith("GenerateItemRegression-", StringComparison.Ordinal))
                Directory.Delete(scratch, true);
        }
    }

    private static void RegisterForm(Type form)
    {
        if (!forms.Add(form)) return;
        var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType;
        if (parent != null) RegisterForm(parent);
        form.GetMethod("Register", HiddenStatic | BindingFlags.DeclaredOnly)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = Go("generate-item-regression-" + typeof(T).Name);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, manager);
        return manager;
    }

    private static GameObject Go(string name)
    {
        var result = new GameObject(name);
        roots.Add(result);
        return result;
    }

    private static void SetUp()
    {
        foreach (var form in new[] { typeof(ItemProductForm), typeof(ItemParamForm), typeof(CharacterParamForm),
            typeof(MapModelForm), typeof(EventTriggerForm), typeof(ProgressForm), typeof(SceneForm),
            typeof(MapMainForm), typeof(TileUnitForm), typeof(ItemUnitForm), typeof(ObjectUnitForm), typeof(CharacterUnitForm),
            typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(CmdLabForm), typeof(ProgramDataForm),
            typeof(EventProgramDataForm), typeof(EventInterpretDataForm), typeof(BoxDataForm),
            typeof(GameObjectAssetForm), typeof(TexAssetForm), typeof(MapTextureForm), typeof(ModTextForm) })
            RegisterForm(form);
        GameParamForm.RegisterGet();
        foreach (var type in new[] { typeof(GenerateItemCmd), typeof(GetPositionCmd), typeof(DestroyCmd), typeof(NewVectorCmd) })
            type.GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly).Invoke(null, null);

        DynamicGlobalSettings.playing = true;
        DynamicGlobalSettings.cameraMode = CameraMode.Overhead;
        game = Manager<GameManager>();
        play = Manager<PlayManager>();
        pools = Manager<InstancePoolManager>();
        pools.defaultRoot = pools.transform;
        pools.Init();
        // Pool roots and the timer must be active, while ordinary managers remain unstarted.
        pools.gameObject.SetActive(true);
        Manager<TimeManager>().gameObject.SetActive(true);
        map = Manager<MapManager>();
        map.mainGo = Go("generate-item-map-root");
        map.Init();
        map.enable = true;
        game.utilCtrl = new GameUtilController(game);
        game.mapCtrl = new GameMapController(game);
        game.mapCtrl.Unregister<TileEvent>();
        game.mapCtrl.Unregister<ObjectEvent>();
        game.evtCtrl = new GameEventController(game);
        // Lifecycle scheduling is covered separately; retain real Item appearance/removal.
        game.evtCtrl.sceneTriggerCtrl.Unregister<ItemEvent>();
        var scene = SceneForm.defaultData;
        scene.uid = 1;
        SceneForm.AddData(scene);
        game.curScene = scene;
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.bag = new List<int>();
        progress.team = new List<int>();
        progress.teamActive = new List<int>();
        progress.characterUid = 0;
        progress.seconds = 1.25f;
        ProgressForm.AddData(progress);
        TimeManager.animationTimeGetter = () => progress.seconds;
        info = new PlayInfoController(play);
        play.infoCtrl = info;
        info.Begin();
        probe.Register<ItemEvent>();

        var tilePrefab = Go(MapInfo.GetPrefabName("mapfloor"));
        tilePrefab.SetActive(false);
        tilePrefab.AddComponent<TileInstance>();
        var ground = tilePrefab.AddComponent<BoxCollider>();
        ground.center = Vector3.down * .05f;
        ground.size = new Vector3(1, .1f, 1);
        pools.AddPool(tilePrefab);
        map.data = new GameMapData
        {
            mainData = new MapMainForm.Data(1, cellSize, new Vector3Int(1000, 1000, 1000),
                new Vector3Int(4, 4, 4), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        var logical = map.utilCtrl.RealPos2MapPosInt(WorldPosition);
        logical.y -= 2;
        var tile = new TileUnitForm.Data(710001, "generate item support", new Dictionary<int, int>(), logical,
            tilePrefab.name, Vector3.Scale(logical, cellSize), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", false, false, new List<int>());
        TileUnitForm.AddData(tile);
        map.data.RegisterMap(tile);
        owner = tile.unit;
        owner.Show();
        Check(map.utilCtrl.GetPlacementTile(WorldPosition) == owner, "fixture target has a nearest-lower support Tile");

        var shader = Shader.Find("DisFadeCode");
        Check(shader != null, "fixture imports the production DisFade shader");
        material = new Material(shader);
        var sourceContainer = Go("generate-item-source-prefabs");
        sourceContainer.SetActive(false);
        var modelPrefab = GameObject.CreatePrimitive(PrimitiveType.Cube);
        modelPrefab.name = "generate-item-model";
        modelPrefab.transform.SetParent(sourceContainer.transform, false);
        Object.DestroyImmediate(modelPrefab.GetComponent<BoxCollider>());
        var modelBody = new GameObject("col");
        modelBody.transform.SetParent(modelPrefab.transform, false);
        modelBody.AddComponent<BoxCollider>();
        modelPrefab.GetComponent<Renderer>().sharedMaterial = material;
        GameObjectAssetForm.DataById[710011] = new GameObjectAssetForm.Data(710011, modelPrefab.name, null, null, null, modelPrefab, 0);
        firstTexture = new Texture2D(2, 2);
        secondTexture = new Texture2D(2, 2);
        TexAssetForm.DataById[710021] = new TexAssetForm.Data(710021, "first generated frame", "", null, "", firstTexture, 0);
        TexAssetForm.DataById[710022] = new TexAssetForm.Data(710022, "second generated frame", "", null, "", secondTexture, 0);
        source = ItemProductForm.defaultData;
        source.uid = 710101;
        source.name = "generated potion";
        source.protoUid = 0;
        source.amount = 7;
        source.maxAmountPer = 99;
        source.minimapIcon = 710021;
        source.model = new MapModelForm.Data(-1, new List<int> { 710011 }, new List<Vector3> { Vector3.zero },
            new List<Vector3> { Vector3.one }, new List<List<int>> { new List<int> { 710021, 710022 } }, 1f, false, .4f);
        source.events = new Dictionary<string, EventTriggerForm.Data>
        {
            { "onInteractEvent", new EventTriggerForm.Data(-1, "onInteractEvent", new List<int> { 43210 }, default) }
        };
        source.paramDic = new Dictionary<string, ItemParamForm.Data>
        {
            { ParamName, new ItemParamForm.Data(-1, ParamName, ValType.Float, "", "", "", ParamShowType.Hide) }
        };
        source.paramDic[ParamName].SetValue(4f);
        ItemProductForm.AddData(source);
        sourceJson = ItemProductForm.GetJoByData(source).ToString();
    }

    private static void MetadataAndInvalidArguments()
    {
        Check(CmdBase.TryGetForm("GenerateItem", out var form) && form is GameCmdDataForm.Data typed
            && typed.uid == 100133 && typed.labId == 10014, "GenerateItem has one Item-category metadata row");
        Check(form.prmTypes.SequenceEqual(new[] { "item", "vector" }) && form.prmNames.Count == 2
            && form.retNames.Count == 1 && form.retTypes.SequenceEqual(new[] { "sceneObject" }),
            "GenerateItem accepts only item/position and returns a scene Unit handle");
        Check(BaseData.cmdDic["GenerateItem"] is GenerateItemCmd && new GenerateItemCmd().GetNew() is GenerateItemCmd,
            "runtime registration and command cloning resolve GenerateItem");
        Check(typeof(GenerateItemCmd).GetMethod("Init", HiddenStatic).GetCustomAttributes(
            typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1, "GenerateItem has explicit Player registration");
        Compile(form.defaultCode.TrimEnd(';') + ";");
        foreach (string handle in new[] { "", "plain", "$it$$it$", "$it$0$it$", "$it$-1$it$", "$it$bad$it$",
            "$it$99999999999$it$", "$it$999999$it$", "$ch$710101$ch$", "$so$710101$so$" })
            ExpectFailure(Text(handle), Vector(authorPosition), "invalid Item reference " + handle);
        ExpectFailure(Number(source.uid), Vector(authorPosition), "a raw UID is not an Item handle");
        ExpectFailure(Text(Handle(source.uid)), Number(0), "scalar/missing position");
        var missing = Vector(authorPosition);
        missing.dic.Remove("height");
        ExpectFailure(Text(Handle(source.uid)), missing, "position missing height");
        foreach (float invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            foreach (string coordinate in new[] { "x", "y", "height" })
            {
                var vector = Vector(authorPosition);
                vector.dic[coordinate].num = invalid;
                ExpectFailure(Text(Handle(source.uid)), vector, "nonfinite position " + coordinate);
            }
        var textCoordinate = Vector(authorPosition);
        textCoordinate.dic["x"] = Text("2.25");
        ExpectFailure(Text(Handle(source.uid)), textCoordinate, "text coordinates are not silently parsed");
        Check(Run("Return GenerateItem();").num == 0f, "omitted arguments compile, pad and safely return zero");
    }

    private static void UnsupportedPlacementAndResources()
    {
        ExpectFailure(Text(Handle(source.uid)), Vector(new Vector3(100, 10, 100)), "unsupported X/Z column");
        var savedMap = map.data;
        map.data = null;
        ExpectFailure(Text(Handle(source.uid)), Vector(authorPosition), "no active map data");
        map.data = savedMap;
        var model = source.model;
        source.model = null;
        ExpectFailure(Text(Handle(source.uid)), Vector(authorPosition), "missing Item model");
        source.model = model;
        var modelParts = source.model.subPrefabUnitName;
        source.model.subPrefabUnitName = new List<int>();
        ExpectFailure(Text(Handle(source.uid)), Vector(authorPosition), "empty model parts");
        source.model.subPrefabUnitName = modelParts;
        source.model.subPrefabUnitName[0] = 799999;
        ExpectFailure(Text(Handle(source.uid)), Vector(authorPosition), "missing base model asset");
        source.model.subPrefabUnitName[0] = 710011;
        Check(ItemProductForm.GetJoByData(source).ToString() == sourceJson, "resource failure checks restore the authored source");
    }

    private static void ForeignPoolRollback()
    {
        int candidate = ProductForm.uidChain.PeekId();
        var foreign = Go(GlobalDefaultHelper.GetRuntimeMapItemPrefabName(candidate));
        foreign.SetActive(false);
        foreign.AddComponent<ItemInstance>();
        pools.AddPool(foreign);
        var previous = pools.pools[foreign.name];
        ExpectFailure(Text(Handle(source.uid)), Vector(authorPosition), "a foreign non-scene-owned pool cannot be replaced");
        Check(pools.pools[foreign.name] == previous && !ItemProductForm.DataByUid.ContainsKey(candidate),
            "failed generation rolls back only its Product, retaining the original pool");
        pools.Unregister(previous);
        previous.Destroy();
    }

    private static void GenerateAppearanceAndCopies()
    {
        int bagCount = progress.bag.Count;
        var first = Generate(source);
        var firstProduct = ItemProductForm.DataByUid[first.unit.productInfo.Item1];
        var firstPool = pools.pools[first.prefabName];
        Check(first.pos == WorldPosition && first.unit.belongTile == owner
            && first.minimapIcon == source.minimapIcon, "world coordinates and nearest-lower owner are applied without cell scaling or height snapping");
        Check(firstProduct.uid != source.uid && firstProduct.protoUid == source.uid && firstProduct.amount == 7,
            "one scene drop owns a new runtime Product with the source's entire amount");
        Check(progress.bag.Count == bagCount && source.amount == 7, "generation neither consumes the source nor changes the backpack");
        Check(first.unit.ins != null && first.unit.isShowing
            && first.unit.ins.transform.position == WorldPosition && ShownTexture(first) == secondTexture,
            "a visible generated Item immediately uses its bound Product's animation phase");
        Check(probe.Created(first.uid) == 1, "appearance rebinding does not repeat Item Create");
        Check(JObject.Parse(first.extra)["pdt"] != null || first.extra.Contains(firstProduct.uid.ToString(CultureInfo.InvariantCulture)),
            "the scene Product binding is immediately serialized into extra");

        var second = Generate(source);
        var secondProduct = ItemProductForm.DataByUid[second.unit.productInfo.Item1];
        Check(secondProduct.uid != firstProduct.uid && second.uid != first.uid,
            "repeated generation produces independent scene and Product identities");
        Check(!ReferenceEquals(firstProduct.model, source.model)
            && !ReferenceEquals(firstProduct.model.subUnitTexsName[0], source.model.subUnitTexsName[0])
            && !ReferenceEquals(firstProduct.paramDic[ParamName], source.paramDic[ParamName])
            && !ReferenceEquals(firstProduct.events["onInteractEvent"], source.events["onInteractEvent"]),
            "generated model, nested texture lists, parameters and trigger data are deep copies");
        firstProduct.model.subPrefabUnitScale[0] = new Vector3(2, .5f, .75f);
        firstProduct.events["onInteractEvent"].evt.Add(555);
        firstProduct.paramDic[ParamName].SetValue(12f);
        Check(source.paramDic[ParamName].GetValue().num == 4f && secondProduct.paramDic[ParamName].GetValue().num == 4f
            && secondProduct.model.subPrefabUnitScale[0] == Vector3.one
            && secondProduct.events["onInteractEvent"].evt.SequenceEqual(new[] { 43210 })
            && ItemProductForm.GetJoByData(source).ToString() == sourceJson,
            "editing one generated drop cannot mutate the authored source or another generated drop");
        var read = Execute(new GetPositionCmd(), Text(GlobalEventHelper.GetSceneUnitReference(first.uid))).res[0];
        Check(ReadVector(read) == authorPosition, "the precise generated Unit handle works with GetPosition");
        Execute(new DestroyCmd(), Text(GlobalEventHelper.GetSceneUnitReference(second.uid)));
        Check(!ItemUnitForm.DataByUid.ContainsKey(second.uid) && ItemUnitForm.DataByUid.ContainsKey(first.uid)
            && ItemProductForm.DataByUid.ContainsKey(secondProduct.uid), "Destroy removes only the selected drop and retains Item Products");

        // Same production operations as pickup, with toast/message disabled to avoid UI.
        info.GainItem(firstProduct, false, false);
        map.RemoveItem(first);
        Check(!ItemUnitForm.DataByUid.ContainsKey(first.uid) && !ItemProductForm.DataByUid.ContainsKey(firstProduct.uid)
            && progress.bag.Sum(uid => ItemProductForm.DataByUid[uid].amount) == 7 && source.amount == 7,
            "pickup consumes only the generated Product and adds its quantity to the backpack");
        var bagProduct = ItemProductForm.DataByUid[progress.bag.Single()];
        bagProduct.model.subPrefabUnitScale[0] = new Vector3(3, .3f, .6f);
        int bagAmount = bagProduct.amount;
        // The allocator need not immediately reuse the just-freed UID. Model
        // that later reuse deterministically by renaming this fixture-owned,
        // inactive orphan pool to the next Product's canonical pool name.
        int nextProductUid = ProductForm.uidChain.PeekId();
        pools.Unregister(firstPool);
        firstPool.prefab.name = GlobalDefaultHelper.GetRuntimeMapItemPrefabName(nextProductUid);
        pools.Register(firstPool);
        var reused = Generate(bagProduct);
        var reusedProduct = ItemProductForm.DataByUid[reused.unit.productInfo.Item1];
        Check(reusedProduct.uid == nextProductUid && pools.pools[reused.prefabName] != firstPool
            && pools.pools[reused.prefabName].prefab.transform.GetChild(0).localScale == new Vector3(3, .3f, .6f),
            "a recycled Product UID rebuilds the unused pool instead of reusing old geometry");
        Check(reusedProduct.protoUid == source.uid && bagProduct.amount == bagAmount
            && progress.bag.SequenceEqual(new[] { bagProduct.uid }) && reusedProduct.amount == bagAmount,
            "a runtime/backpack source is copied without consuming it or nesting its prototype identity");
    }

    private static void InterpreterAndMapRoundTrip()
    {
        var result = Run("generated = GenerateItem(\"" + Handle(source.uid)
            + "\",NewVector(2.25,0.75,0.5)); Return GetPosition(generated);");
        Check(ReadVector(result) == authorPosition, "compiled GenerateItem composes with NewVector and GetPosition");
        var item = ItemUnitForm.DataByUid.Values.Last();
        int unitUid = item.uid, productUid = item.unit.productInfo.Item1;
        var productJson = ItemProductForm.GetJoByData(ItemProductForm.DataByUid[productUid]);
        scratch = Path.Combine(Path.GetTempPath(), "GenerateItemRegression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        string file = Path.Combine(scratch, "scene-map");
        var save = new GameSaveController(game);
        save.SaveSceneMap(file, map.data);
        map.End();
        UnitForm.Clear();
        ItemUnitForm.Clear();
        ItemProductForm.RemoveData(productUid);
        ItemProductForm.AddData(ItemProductForm.GetDataByJo(productJson));
        var loaded = save.LoadSceneMap(file);
        map.data = loaded;
        map.Init();
        map.enable = true;
        map.mainGo.SetActive(true);
        map.updateCtrl.Begin();
        owner = map.utilCtrl.GetPlacementTile(WorldPosition);
        owner.Show();
        foreach (var drop in ItemUnitForm.DataByUid.Values) map.updateCtrl.UpdateSingleOne(drop.unit);
        var restored = ItemUnitForm.DataByUid[unitUid];
        Check(!ReferenceEquals(restored, item) && restored.unit.productInfo.Item1 == productUid && restored.pos == WorldPosition
            && restored.unit.belongTile == owner && restored.unit.ins != null && ShownTexture(restored) == secondTexture,
            "actual scene SaveSceneMap/LoadSceneMap round-trip restores drop identity, owner and appearance");
        Check(ItemProductForm.DataByUid[productUid].amount == 7 && loaded.mainData.mapUnitSize == cellSize
            && progress.bag.Count == 1, "map reload retains runtime quantity, nonuniform cells and independent backpack state");
    }

    private static ItemUnitForm.Data Generate(ItemProductForm.Data item)
    {
        var result = Execute(new GenerateItemCmd(), Text(Handle(item.uid)), Vector(authorPosition)).res[0];
        Check(result.str != null && result.str.StartsWith("$so$u:", StringComparison.Ordinal), "successful generation returns a precise scene Unit reference");
        Check(GlobalEventHelper.TryGetSceneObjectReference(result.str, out var kind, out int uid)
            && kind == GlobalEventHelper.ObjectReferenceKind.Unit
            && ItemUnitForm.DataByUid.ContainsKey(uid), "returned reference resolves to the registered scene Item");
        return ItemUnitForm.DataByUid[uid];
    }

    private static void ExpectFailure(BoxDataForm.Data item, BoxDataForm.Data position, string reason)
    {
        int products = ItemProductForm.DataByUid.Count, units = ItemUnitForm.DataByUid.Count,
            poolCount = pools.pools.Count, bag = progress.bag.Count;
        var result = Execute(new GenerateItemCmd(), item, position).res[0];
        Check(result.str == null && result.num == 0f, reason + " returns numeric zero");
        Check(ItemProductForm.DataByUid.Count == products && ItemUnitForm.DataByUid.Count == units
            && pools.pools.Count == poolCount && progress.bag.Count == bag,
            reason + " leaves no Product, scene Unit or prefab-pool leak");
    }

    private static InterpretAsyncTask Execute(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments.Reverse().ToArray(), new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && task.error == null, command.GetName() + " completes without runtime error: " + task.error);
        return task;
    }
    private static ProgramDataForm.Data Compile(string code)
    {
        var program = ProgramDataForm.defaultData;
        program.name = "generate-item-regression";
        Check(program.TryApplyCode(code, out _, out var errors), "source compiles: " + string.Join(";", errors));
        return program;
    }
    private static BoxDataForm.Data Run(string code)
    {
        var program = Compile(code);
        var restored = ProgramDataForm.GetDataByJo(ProgramDataForm.GetJoByData(program));
        var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            restored, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = execution.Interpret();
        Check(result.complete && result.errors.Count == 0 && execution.stack.Count == 0,
            "compiled GenerateItem program executes with a balanced stack: " + string.Join(";", result.errors));
        return result.ret;
    }
    private static string Handle(int uid) => "$it$" + uid.ToString(CultureInfo.InvariantCulture) + "$it$";
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static BoxDataForm.Data Vector(Vector3 value)
    {
        var result = CodeHelper.CreateBox();
        result.dic["x"] = Number(value.x);
        result.dic["y"] = Number(value.z);
        result.dic["height"] = Number(value.y);
        return result;
    }
    private static Vector3 ReadVector(BoxDataForm.Data value) => new Vector3(value.dic["x"].num, value.dic["height"].num, value.dic["y"].num);
    private static Texture ShownTexture(ItemUnitForm.Data value) => value.unit.ins.GetComponentInChildren<Renderer>(true).sharedMaterial.GetTexture("_Tex");
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
    private sealed class Probe : IZ_Listener<ItemEvent>
    {
        private readonly Dictionary<int, int> created = new Dictionary<int, int>();
        public void OnEvent(ItemEvent evt)
        {
            if (evt.type == MapEventType.Create) created[evt.unit.data.uid] = Created(evt.unit.data.uid) + 1;
        }
        public int Created(int uid) => created.TryGetValue(uid, out int count) ? count : 0;
    }
}
