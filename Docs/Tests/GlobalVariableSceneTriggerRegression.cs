using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using Ui.EventCustomTrigger;
using Ui.Notify;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_DataSystem.Form;
using Z_UnitSystem.Form;
using Z_Text.Form;
using Z_Ui;
using Z_Ui.Base;
using Z_UnitSystem;
using SceneUi = Ui.ModStory.ModStoryMap.ModStoryMapScene.ModStoryMapSceneUnit;

// Only runs in the regression runner's isolated project. No authored scene,
// production save, prefab or live editor singleton is loaded or modified.
public static class GlobalVariableSceneTriggerRegression
{
    private const string Prefix = "onGlobalVarChangeEvent$";
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static readonly List<Observation> captures = new List<Observation>();
    private static readonly ChangedProbe changes = new ChangedProbe();
    private static GameManager game;
    private static GameEventController events;
    private static ProgressForm.Data progress;
    private static SceneForm.Data scene;
    private static UiNotifyCtrl notify;
    private static string ownedFolder;
    private static int checks, sequence;

    private sealed class Observation
    {
        public int tag;
        public string name;
        public string owner;
        public BoxDataForm.Data oldValue, newValue;
    }

    private sealed class ChangedProbe : IZ_Listener<GlobalVariableChangedEvent>
    {
        public readonly List<GlobalVariableChangedEvent> payloads = new List<GlobalVariableChangedEvent>();
        public void OnEvent(GlobalVariableChangedEvent value)
        {
            Check(EventGlobalVariableForm.DataByUid.TryGetValue(value.uid, out var current)
                && current.name == value.name && Equal(current.data, value.newValue),
                "The change notification is published only after the new global value is committed");
            Check(!ReferenceEquals(current.data, value.newValue),
                "The published new value is an independent Box snapshot, not writable storage");
            payloads.Add(value);
        }
    }

    private sealed class CaptureCmd : CmdBase
    {
        public override string GetName() => "GlobalSceneTriggerProbe";
        public override CmdBase GetNew() => new CaptureCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask task)
        {
            captures.Add(new Observation { tag = (int)prm[0].num, name = prm[1].str,
                oldValue = prm[2].DeepCopy(), newValue = prm[3].DeepCopy() });
            return true;
        }
    }

    private sealed class UnitCaptureCmd : CmdBase
    {
        public override string GetName() => "GlobalUnitTriggerProbe";
        public override CmdBase GetNew() => new UnitCaptureCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask task)
        {
            captures.Add(new Observation { tag = (int)prm[0].num, owner = prm[1].str, name = prm[2].str,
                oldValue = prm[3].DeepCopy(), newValue = prm[4].DeepCopy() });
            return true;
        }
    }

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            string project = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name;
            Check(project.StartsWith("MapRuntimeRegression-", StringComparison.Ordinal),
                "Fixture execution requires an isolated MapRuntimeRegression project");
            SetUp();
            SceneEditorSelection();
            ActualCommandRoutingAndRename();
            EqualityAndTypedValues();
            HeapAndPayloadIsolation();
            SchedulerAndNoFeedbackLoop();
            TriggerModes();
            GuardsAndAuthoring();
            PersistenceAndUidRetirement();
            UnitBindingsAndRetirement();
            Debug.Log("GLOBAL_VARIABLE_SCENE_TRIGGER_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            Z_EventHelper.Unregister<GlobalVariableChangedEvent>(changes);
            if (events != null)
            {
                events.Reset();
                events.storyTriggerCtrl.Unregister<GlobalVariableChangedEvent>();
                events.storyTriggerCtrl.Unregister<StoryLifeEvent>();
                events.storyTriggerCtrl.Unregister<StoryItemEvent>();
                events.storyTriggerCtrl.Unregister<StoryCharacterEvent>();
                events.storyTriggerCtrl.Unregister<CharacterSkillEvent>();
                events.sceneTriggerCtrl.Unregister<CollideEvent>();
                events.sceneTriggerCtrl.Unregister<TileEvent>();
                events.sceneTriggerCtrl.Unregister<ItemEvent>();
                events.sceneTriggerCtrl.Unregister<ObjectEvent>();
                events.sceneTriggerCtrl.Unregister<CharacterEvent>();
                events.sceneTriggerCtrl.Unregister<StoryLifeEvent>();
            }
            DynamicGlobalSettings.playing = previousPlaying;
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            if (ownedFolder != null && Directory.Exists(ownedFolder))
            {
                string prefix = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar + "ZPluginGlobalSceneTrigger-";
                string target = Path.GetFullPath(ownedFolder);
                if (target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    Directory.Delete(target, true);
            }
        }
    }

    private static void SetUp()
    {
        foreach (Type type in new[] { typeof(BoxDataForm), typeof(EventGlobalVariableForm),
            typeof(ProgramDataForm), typeof(EventProgramDataForm), typeof(InterpretDataForm),
            typeof(EventInterpretDataForm), typeof(EventTriggerForm), typeof(SceneForm),
            typeof(ProgressForm), typeof(CmdDataForm), typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm),
            typeof(CommonTextForm), typeof(ModTextForm) })
            type.GetMethod("Register", HiddenStatic)?.Invoke(null, null);
        SceneForm.Init(); ProgressForm.Init(); EventProgramDataForm.Init(); EventInterpretDataForm.Init();
        GameCmdDataForm.Init(); GameCmdTypeDataForm.Init();
        foreach (Type type in new[] { typeof(GetGlobalVarCmd), typeof(SetGlobalVarCmd) })
            type.GetMethod("Init", HiddenStatic).Invoke(null, null);
        var capture = new CaptureCmd();
        BaseData.cmdDic[capture.GetName()] = capture;
        CmdDataForm.AddData(new CmdDataForm.Data(98101, capture.GetName(),
            new List<string> { "tag", "name", "old", "new" },
            new List<string> { "num", "var", "var", "var" }, null, new List<string> { "void" },
            "", "GlobalSceneTriggerProbe(0,param1,param2,param3);"));
        var unitCapture = new UnitCaptureCmd();
        BaseData.cmdDic[unitCapture.GetName()] = unitCapture;
        CmdDataForm.AddData(new CmdDataForm.Data(98102, unitCapture.GetName(),
            new List<string> { "tag", "owner", "name", "old", "new" },
            new List<string> { "num", "var", "var", "var", "var" }, null, new List<string> { "void" },
            "", "GlobalUnitTriggerProbe(0,param1,param2,param3,param4);"));
        game = Manager<GameManager>();
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        ProgressForm.AddData(progress);
        events = new GameEventController(game);
        game.evtCtrl = events;
        var ui = Manager<UiManager>();
        var notifyObject = new GameObject("global-trigger-notification-capture");
        notifyObject.SetActive(false); owned.Add(notifyObject);
        var holder = notifyObject.AddComponent<UiHolder>(); holder.enabled = false;
        notify = new UiNotifyCtrl { uiHolder = holder, inited = false };
        holder.ctrl = notify;
        ui.uiCtrlName2Uis[nameof(UiNotifyCtrl)] = new List<UiHolder> { holder };
        Z_EventHelper.Register<GlobalVariableChangedEvent>(changes);
        FreshCase();
    }

    private static void SceneEditorSelection()
    {
        FreshCase();
        var globals = new[]
        {
            new EventGlobalVariableForm.Data(778, "last", Num(0)),
            new EventGlobalVariableForm.Data(776, "Any", Num(0)),
            new EventGlobalVariableForm.Data(777, "with$dollar", Num(0))
        };
        foreach (var variable in globals) EventGlobalVariableForm.AddData(variable);
        var ctrl = new SceneUi.UiModStoryMapSceneUnitCtrl
        { model = new SceneUi.UiModStoryMapSceneUnitModel { data = scene } };
        var helper = typeof(SceneUi.UiModStoryMapSceneUnitCtrl).GetMethod("CreateGlobalVarTriggerParam",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Check(helper != null, "The scene editor exposes its real global-trigger configuration builder");
        var prm = (UiEventCustomTriggerParam)helper.Invoke(ctrl, null);
        Check(ReferenceEquals(prm.dic, scene.events) && prm.defaultKey == Prefix && prm.configs.Count == 1,
            "The generated GlobalVar widget is configured against this scene's actual persisted event table");
        Check(prm.newKeyFunc() == Prefix, "New initially offers the unbound Any trigger");
        var any = Bind(scene, Prefix, 91);
        Check(prm.newKeyFunc() == Prefix + 776, "With Any present, New uses the first unbound global by ascending UID");
        var exact = Bind(scene, Prefix + 776, 92);
        Bind(scene, Prefix + 777, 93); Bind(scene, Prefix + 778, 94);
        Check(prm.newKeyFunc() == null, "New declines cleanly when all global trigger choices already exist");
        var config = prm.configs[0];
        config.prmChangeAct(any);
        var choice = notify.param?.chooseInfo;
        Check(choice != null && choice.items.subs.Values.Select(item => item.id)
                .SequenceEqual(new[] { 0, 776, 777, 778 }),
            "The actual picker offers Any plus UID-sorted globals, including a global named Any");
        var chosen = choice.items.subs.Values.Single(item => item.id == 776);
        Check(!choice.func(chosen) && ReferenceEquals(scene.events[Prefix], any)
            && ReferenceEquals(scene.events[Prefix + 776], exact),
            "Choosing an already configured global cannot overwrite its other event binding");
        scene.events.Remove(Prefix + 776);
        Check(choice.func(chosen) && any.name == Prefix + 776 && !scene.events.ContainsKey(Prefix)
            && ReferenceEquals(scene.events[Prefix + 776], any),
            "The picker moves both trigger.name and dictionary key to the selected stable UID");
        Check(config.GetNameFunc(Prefix + 776) == "Any" && prm.newKeyFunc() == Prefix,
            "A selected global named Any stays distinct from the available Any/wildcard binding");
        new ModAssetCtrl(null).RenameGlobalVar(776, "renamed$choice");
        Check(config.GetNameFunc(Prefix + 776) == "renamed$choice"
            && config.GetNameFunc(Prefix + int.MaxValue) == int.MaxValue.ToString()
            && config.GetNameFunc(Prefix + "bad$uid") == "bad$uid" && config.GetNameFunc(null) == "",
            "Labels follow current names and handle missing/malformed identities without parsing exceptions");
        config.prmChangeAct(any);
        var staleSceneChoice = notify.param.chooseInfo;
        ctrl.model.data = NewScene(2);
        Check(staleSceneChoice.func(staleSceneChoice.items.subs.Values.Single(item => item.id == 777))
            && scene.events.ContainsKey(Prefix + 776) && any.name == Prefix + 776 && prm.newKeyFunc() == null,
            "A picker captured before the controller changes scenes cannot modify its previous scene");
        ctrl.model.data = scene;
        config.prmChangeAct(any);
        var staleGlobalChoice = notify.param.chooseInfo;
        new ModAssetCtrl(null).DeleteGlobalVar(777);
        EventGlobalVariableForm.AddData(new EventGlobalVariableForm.Data(777, "replacement-choice", Num(0)));
        Check(staleGlobalChoice.func(staleGlobalChoice.items.subs.Values.Single(item => item.id == 777))
            && scene.events.ContainsKey(Prefix + 776) && any.name == Prefix + 776,
            "A stale variable choice is rejected when its deleted UID now identifies a different global record");
    }

    private static void FreshCase()
    {
        events.Reset();
        EventGlobalVariableForm.Clear();
        SceneForm.Clear();
        scene = NewScene(1);
        game.curScene = scene;
        progress.blockProgramUid = 0;
        progress.eventState = EventState.Normal;
        DynamicGlobalSettings.playing = true;
        captures.Clear(); changes.payloads.Clear();
    }

    private static SceneForm.Data NewScene(int uid)
    {
        var data = new SceneForm.Data(uid, "scene-" + uid, 0, Vector2.zero, false, false,
            new Dictionary<string, EventTriggerForm.Data>(), new Dictionary<int, List<string>>(),
            new Dictionary<int, List<string>>(), false);
        SceneForm.AddData(data);
        return data;
    }

    private static EventGlobalVariableForm.Data Global(string name, BoxDataForm.Data initial)
    {
        var data = new EventGlobalVariableForm.Data(-1, name, initial);
        EventGlobalVariableForm.AddData(data);
        return data;
    }

    private static EventProgramDataForm.Data Program(string source)
    {
        var data = new EventProgramDataForm.Data(-1, "global-scene-trigger-" + ++sequence,
            "", new List<string>(), new List<int>(), 0, "void", 0);
        Check(data.TryApplyCode(source, out _, out var errors),
            "The actual compiler accepts the trigger program: " + string.Join("; ", errors));
        EventProgramDataForm.AddData(data);
        return data;
    }

    private static string Capture(int tag) => "GlobalSceneTriggerProbe(" + tag + ",param1,param2,param3);";

    private static void UnitBindingsAndRetirement()
    {
        FreshCase();
        var registered = new HashSet<Type>();
        void Register(Type form)
        {
            if (!registered.Add(form)) return;
            var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType;
            if (parent != null) Register(parent);
            form.GetMethod("Register", HiddenStatic)?.Invoke(null, null);
            form.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
        }
        foreach (var form in new[] { typeof(ObjectUnitForm), typeof(CharacterUnitForm), typeof(ItemUnitForm),
            typeof(TileUnitForm), typeof(ObjectProductForm), typeof(CharacterProductForm), typeof(ItemProductForm), typeof(MapTextureForm) }) Register(form);
        var global = Global("格挡1", Num(0)); string key = Prefix + global.uid;
        string UnitCapture(int tag) => "GlobalUnitTriggerProbe(" + tag + ",param1,param2,param3,param4);";
        EventTriggerForm.Data Trigger(int tag) => new EventTriggerForm.Data(-1, key,
            new List<int> { Program(UnitCapture(tag)).uid }, TriggerType.NoLimit);
        var product = ObjectProductForm.GetDataByJo(new JObject()); product.uid = 93001;
        product.events = new Dictionary<string, EventTriggerForm.Data> { [key] = Trigger(31) };
        ObjectProductForm.AddData(product);
        var data = ObjectUnitForm.GetDataByJo(new JObject()); data.uid = 93002;
        data.extra = new JObject { [ObjectProductForm.ProductVersionKey] = ObjectProductForm.ProductVersion,
            [MapUnit.productKey] = new JArray(product.uid, -1) }.ToString(); ObjectUnitForm.AddData(data);
        var local = new Dictionary<string, EventTriggerForm.Data> { [key] = Trigger(32),
            [Prefix] = new EventTriggerForm.Data(-1, Prefix, new List<int> { Program(UnitCapture(33)).uid }, default) };
        data.unit.evtDic = local;
        var character = CharacterProductForm.GetDataByJo(new JObject()); character.uid = 93003;
        character.events = new Dictionary<string, EventTriggerForm.Data> { [key] = Trigger(34) }; CharacterProductForm.AddData(character);
        var ch = CharacterUnitForm.GetDataByJo(new JObject()); ch.uid = 93004;
        ch.extra = new JObject { [MapUnit.productKey] = new JArray(character.uid, -1) }.ToString(); CharacterUnitForm.AddData(ch);
        var item = ItemProductForm.GetDataByJo(new JObject()); item.uid = 93005;
        item.events = new Dictionary<string, EventTriggerForm.Data> { [key] = Trigger(35) }; ItemProductForm.AddData(item);
        var it = ItemUnitForm.GetDataByJo(new JObject()); it.uid = 93006;
        it.extra = new JObject { [MapUnit.productKey] = new JArray(item.uid, -1) }.ToString(); ItemUnitForm.AddData(it);
        var texture = MapTextureForm.GetDataByJo(new JObject()); texture.id = 93007;
        texture.events = new Dictionary<string, EventTriggerForm.Data> { [key] = Trigger(36) }; MapTextureForm.AddData(texture);
        var tile = TileUnitForm.GetDataByJo(new JObject()); tile.uid = 93008;
        tile.texDic = new Dictionary<int, int> { [0] = texture.id }; TileUnitForm.AddData(tile);
        Set(global.name, Num(2));
        Check(EventInterpretDataForm.DataByUid.Values.All(evt => evt.sceneOwner != null),
            "global Unit handlers capture their real scene-owner Data for cancellation and UID-reuse guards");
        Tick();
        Check(captures.Select(value => value.tag).OrderBy(value => value).SequenceEqual(new[] { 31, 32, 33, 34, 35, 36 }),
            "Object, Character, Item and Tile instances execute their inherited and appended global bindings");
        Check(captures.FindIndex(value => value.tag == 31) < captures.FindIndex(value => value.tag == 32)
            && captures.All(value => value.name == global.name && value.oldValue.num == 0 && value.newValue.num == 2),
            "prototype handlers run before instance appends with independent global-change arguments: "
            + string.Join(";", captures.Select(value => value.tag + ":" + value.name + ":" + value.oldValue.num + "/" + value.newValue.num)));
        foreach (var capture in captures)
        {
            int ownerUid = capture.tag <= 33 ? data.uid : capture.tag == 34 ? ch.uid : capture.tag == 35 ? it.uid : tile.uid;
            Check(capture.owner == GlobalEventHelper.GetSceneUnitReference(ownerUid)
                && ReferenceEquals(GlobalEventHelper.ResolveSceneObjectUnit(capture.owner)?.data, UnitForm.DataByUid[ownerUid]),
                "Unit param1 resolves the exact owner for position/destruction commands, independently of the global name");
        }
        captures.Clear(); Set(global.name, Num(3)); ObjectUnitForm.RemoveData(data.uid); Tick();
        Check(captures.All(value => value.tag != 31 && value.tag != 32 && value.tag != 33),
            "removing a Unit suppresses its queued variable handlers");
        ObjectUnitForm.AddData(data);
        string originalExtra = data.extra;
        var mapJson = new JObject { ["characterJa"] = new JArray(new JObject { ["extra"] = originalExtra,
            ["untouched"] = key }).ToString(), ["other"] = "preserved" }.ToString();
        Check(GlobalVariableTriggerBindings.RemoveFromJson(mapJson, key, out var cleaned),
            "variable retirement traverses serialized map arrays and nested Unit extra JSON");
        var cleanedMap = JObject.Parse(cleaned); var cleanedUnit = JArray.Parse((string)cleanedMap["characterJa"])[0];
        var cleanedExtra = JObject.Parse((string)cleanedUnit["extra"]);
        Check(((JArray)cleanedExtra["events"]).Count == 1 && (string)cleanedExtra["events"][0]["k"] == Prefix
            && (string)cleanedUnit["untouched"] == key && (string)cleanedMap["other"] == "preserved",
            "retiring an unloaded-map binding preserves Any and unrelated keys or values");
        new ModAssetCtrl(null).DeleteGlobalVar(global.uid);
        Check(!product.events.ContainsKey(key) && !character.events.ContainsKey(key) && !item.events.ContainsKey(key)
            && !texture.events.ContainsKey(key) && !data.unit.evtDic.ContainsKey(key) && data.unit.evtDic.ContainsKey(Prefix),
            "deleting a global retires Product, texture and live instance bindings before UID reuse");
        ObjectUnitForm.Clear(); CharacterUnitForm.Clear(); ItemUnitForm.Clear(); TileUnitForm.Clear();
    }

    private static EventTriggerForm.Data Bind(SceneForm.Data owner, string key, int tag,
        TriggerType type = TriggerType.NoLimit) => Bind(owner, key, type, Program(Capture(tag)));

    private static EventTriggerForm.Data Bind(SceneForm.Data owner, string key, TriggerType type,
        params EventProgramDataForm.Data[] programs)
    {
        var data = new EventTriggerForm.Data(-1, key, programs.Select(item => item.uid).ToList(), type);
        owner.events[key] = data;
        return data;
    }

    private static void ActualCommandRoutingAndRename()
    {
        FreshCase();
        var global = Global("watched$variable", Num(0));
        Bind(scene, Prefix, 1);
        Bind(scene, Prefix + global.uid, 2);
        Bind(scene, Prefix + (global.uid - 1), 3);
        Set(global.name, Num(5));
        Check(captures.Count == 0 && EventInterpretDataForm.DataByUid.Count == 2,
            "SetGlobalVar queues Any and exact UID handlers without executing either synchronously");
        Check(changes.payloads.Count == 1 && changes.payloads[0].uid == global.uid,
            "One committed write publishes one change with the stable global UID");
        Tick();
        Check(captures.Select(item => item.tag).OrderBy(tag => tag).SequenceEqual(new[] { 1, 2 }),
            "The real scheduler executes Any and exact UID handlers, not an unrelated global binding");
        Check(captures.All(item => item.name == "watched$variable" && item.oldValue.num == 0
            && item.newValue.num == 5), "Handlers receive name, old value and new value in param1..3");
        Check(new ModAssetCtrl(null).RenameGlobalVar(global.uid, "renamed$global"),
            "The actual authoring rename succeeds without replacing the global UID");
        captures.Clear();
        Set(global.name, Num(7)); Tick();
        Check(captures.Count == 2 && captures.All(item => item.name == "renamed$global"
            && item.oldValue.num == 5 && item.newValue.num == 7),
            "Renaming a variable, including a dollar sign in its name, preserves the UID trigger binding");
        events.ClearScene();
        var next = NewScene(2);
        Bind(next, Prefix, 4); Bind(next, Prefix + global.uid, 5);
        game.curScene = next;
        captures.Clear(); Set(global.name, Num(9)); Tick();
        Check(captures.Select(item => item.tag).OrderBy(tag => tag).SequenceEqual(new[] { 4, 5 }),
            "Only the current scene's selected handlers run after a logical scene switch");
    }

    private static void EqualityAndTypedValues()
    {
        CheckChange(null, Num(0), false, "Missing/null values have the same effective numeric zero");
        CheckChange(new BoxDataForm.Data(42, null, "irrelevant", 0,
            new Dictionary<string, BoxDataForm.Data>()), Num(0), false,
            "Stored scalar metadata and empty/lazy dictionary do not change a value");
        CheckChange(Num(1), Num(2), true, "Numeric changes trigger");
        CheckChange(Num(1), Text("1"), true, "Numeric and text types are distinct");
        CheckChange(Text(""), Text(""), false, "Explicit empty strings compare equal");
        CheckChange(new BoxDataForm.Data(2, "same", "old", 99, null),
            new BoxDataForm.Data(3, "same", null, -2, new Dictionary<string, BoxDataForm.Data>()), false,
            "Unobservable numeric/reference metadata on equal strings does not trigger");
        CheckChange(Text("old"), Text("new"), true, "Text changes trigger");
        var left = Nested(1, 0.5f);
        var right = CodeHelper.CreateBoxByStr("unobservable root scalar");
        right.dic = new Dictionary<string, BoxDataForm.Data>
        {
            ["nested"] = left.dic["nested"].DeepCopy(), ["x"] = Num(1)
        };
        CheckChange(left, right, false, "Deep equality ignores dictionary insertion order and root scalar metadata");
        CheckChange(Nested(1, 0.5f), Nested(1, 0.6f), true, "Nested dictionary changes trigger");
        var extra = Nested(1, 0.5f); extra.dic["extra"] = Num(0);
        CheckChange(Nested(1, 0.5f), extra, true, "Adding a zero-valued dictionary key still changes the value");
        CheckChange(extra, Nested(1, 0.5f), true, "Removing a dictionary key changes the value");
        CheckChange(Num(0), Nested(0, 0), true, "A dictionary does not compare equal to a scalar");

        FreshCase(); Bind(scene, Prefix, 9);
        Set("not-created-before", Num(0));
        Check(EventGlobalVariableForm.DataByName.ContainsKey("not-created-before")
            && changes.payloads.Count == 0 && EventInterpretDataForm.DataByUid.Count == 0,
            "Creating a named zero retains setter storage behavior without a false zero-to-zero change");
    }

    private static void CheckChange(BoxDataForm.Data before, BoxDataForm.Data after, bool changed, string message)
    {
        FreshCase();
        Global("compare", before);
        Bind(scene, Prefix, 9);
        Set("compare", after);
        Check(changes.payloads.Count == (changed ? 1 : 0)
            && EventInterpretDataForm.DataByUid.Count == (changed ? 1 : 0), message);
        Check(Equal(EventGlobalVariableForm.DataByName["compare"].data, after),
            "The setter still commits an independent effective value in equality cases");
        if (!changed) return;
        Tick();
        Check(captures.Count == 1 && Equal(captures[0].oldValue, before) && Equal(captures[0].newValue, after),
            "Typed snapshots preserve both effective values through the actual interpreter");
    }

    private static void HeapAndPayloadIsolation()
    {
        FreshCase();
        var old = Nested(1, 0.5f);
        var global = Global("dictionary", old);
        Bind(scene, Prefix, TriggerType.NoLimit,
            Program("param2.x = -100; param3.x = -200; " + Capture(11)), Program(Capture(12)));
        Bind(scene, Prefix + global.uid, 13);
        var input = Nested(2, 0.8f);
        Set(global.name, input);
        var pending = EventInterpretDataForm.DataByUid.Values.ToArray();
        Check(pending.Length == 3 && !ReferenceEquals(pending[0].heap, pending[1].heap)
            && !ReferenceEquals(pending[0].heap["param3"], pending[1].heap["param3"])
            && !ReferenceEquals(pending[0].heap["param3"].dic["nested"], pending[2].heap["param3"].dic["nested"]),
            "Every program, including siblings in one trigger, receives an independent deep argument heap");
        input.dic["x"].num = 200;
        old.dic["x"].num = 100;
        changes.payloads[0].newValue.dic["x"].num = 300;
        Check(pending.All(item => item.heap["param2"].dic["x"].num == 1
            && item.heap["param3"].dic["x"].num == 2),
            "Queued arguments are isolated from source, previous storage and published-event mutations");
        Tick();
        Check(captures.Single(item => item.tag == 11).oldValue.dic["x"].num == -100
            && captures.Single(item => item.tag == 11).newValue.dic["x"].num == -200,
            "The mutation program really writes its own trigger parameters");
        Check(captures.Where(item => item.tag != 11).All(item => item.oldValue.dic["x"].num == 1
            && item.newValue.dic["x"].num == 2) && global.data.dic["x"].num == 2,
            "A handler cannot corrupt sibling/exact handlers or the committed global value");
    }

    private static void SchedulerAndNoFeedbackLoop()
    {
        FreshCase();
        var global = Global("scheduler", Num(0));
        Bind(scene, Prefix + global.uid, TriggerType.NoLimit,
            Program("SetGlobalVar(param1,param3); " + Capture(21)));
        events.Execute(Program("SetGlobalVar(\"scheduler\",1);"), 0, null);
        Tick();
        Check(captures.Count == 0 && EventInterpretDataForm.DataByUid.Count == 1,
            "A setter inside an event queues its change handler for the next normal scheduler pass");
        Tick();
        Check(captures.Count == 1 && captures[0].tag == 21 && changes.payloads.Count == 1
            && EventInterpretDataForm.DataByUid.Count == 0,
            "Setting the same value from its own handler does not create an event feedback loop");
        Tick();
        Check(captures.Count == 1 && EventInterpretDataForm.DataByUid.Count == 0,
            "No work reappears on the following frame after the stable write");

        FreshCase();
        var source = Global("source", Num(0)); var destination = Global("destination", Num(0));
        Bind(scene, Prefix + source.uid, TriggerType.NoLimit,
            Program("SetGlobalVar(\"destination\",7); " + Capture(22)));
        Bind(scene, Prefix + destination.uid, 23);
        Set(source.name, Num(1)); Tick();
        Check(captures.Count == 1 && captures[0].tag == 22 && EventInterpretDataForm.DataByUid.Count == 1,
            "A different global changed by a handler also defers its nested handler to the next pass");
        Tick();
        Check(captures.Count == 2 && captures[1].tag == 23 && captures[1].newValue.num == 7,
            "Deferred nested global changes still run through the ordinary event pipeline");
    }

    private static void TriggerModes()
    {
        FreshCase();
        var global = Global("once", Num(0));
        Bind(scene, Prefix + global.uid, 31, TriggerType.Once);
        Set(global.name, Num(1)); Set(global.name, Num(2));
        Check(EventInterpretDataForm.DataByUid.Count == 1, "Once suppresses repeated changes after the first queued trigger");
        Tick(); Set(global.name, Num(3)); Tick();
        Check(captures.Count == 1 && captures[0].newValue.num == 1, "Once remains suppressed after its first completion");

        FreshCase();
        global = Global("once-during", Num(0));
        Bind(scene, Prefix + global.uid, 32, TriggerType.OnceDuring);
        Set(global.name, Num(1)); Set(global.name, Num(2));
        Check(EventInterpretDataForm.DataByUid.Count == 1, "OnceDuring suppresses a concurrent second change");
        Tick(); Set(global.name, Num(3)); Tick();
        Check(captures.Count == 2 && captures[1].oldValue.num == 2 && captures[1].newValue.num == 3,
            "OnceDuring releases after completion and runs a later value change");
    }

    private static void GuardsAndAuthoring()
    {
        FreshCase();
        var global = Global("guarded", Num(0)); Bind(scene, Prefix, 40);
        DynamicGlobalSettings.playing = false;
        Set(global.name, Num(1));
        Check(EventInterpretDataForm.DataByUid.Count == 0, "Mod mode never queues scene gameplay handlers");
        int published = changes.payloads.Count;
        global.data = Text("authored initial value");
        new ModAssetCtrl(null).CreateGlobalVar("authored-only");
        Check(changes.payloads.Count == published, "Authoring initial values does not publish runtime change events");
        DynamicGlobalSettings.playing = true;
        game.curScene = null; Set(global.name, Num(2));
        Check(EventInterpretDataForm.DataByUid.Count == 0, "Missing current scene is safe");
        game.curScene = scene; progress.eventState = EventState.Leave; Set(global.name, Num(3));
        Check(EventInterpretDataForm.DataByUid.Count == 0, "Leaving a scene cannot start a new global handler");
        progress.eventState = EventState.Normal;
        ProgressForm.RemoveData(progress.uid); Set(global.name, Num(4));
        Check(EventInterpretDataForm.DataByUid.Count == 0, "Missing progress is safe");
        ProgressForm.AddData(progress);
        var bindings = scene.events; scene.events = null; Set(global.name, Num(5));
        Check(EventInterpretDataForm.DataByUid.Count == 0, "A null legacy scene event table is safe");
        scene.events = bindings;
        published = changes.payloads.Count;
        Set("", Num(1)); Set(null, Num(1));
        Check(changes.payloads.Count == published && EventInterpretDataForm.DataByUid.Count == 0,
            "Empty and omitted names do not notify or queue a handler");
        events.storyTriggerCtrl.OnEvent((GlobalVariableChangedEvent)null);
        Check(EventInterpretDataForm.DataByUid.Count == 0, "A null change notification is ignored");
    }

    private static void PersistenceAndUidRetirement()
    {
        FreshCase();
        var global = Global("persisted", Num(0));
        var any = Bind(scene, Prefix, 51);
        var specific = Bind(scene, Prefix + global.uid, 52, TriggerType.OnceDuring);
        var otherScene = NewScene(2);
        Bind(otherScene, Prefix, 53); Bind(otherScene, Prefix + global.uid, 54);
        ownedFolder = Path.Combine(Path.GetTempPath(), "ZPluginGlobalSceneTrigger-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ownedFolder);
        string folder = Path.Combine(ownedFolder, "Core") + Path.DirectorySeparatorChar;
        var save = new GameSaveController(game);
        save.SaveScene(folder);
        string path = Path.Combine(folder, save.sceneFormFileName);
        Check(File.Exists(path), "The actual scene save API writes only into the fixture-owned directory");
        SceneForm.Clear(); save.LoadScene(folder);
        scene = SceneForm.DataByUid[1]; game.curScene = scene;
        Check(scene.events.ContainsKey(Prefix) && scene.events.ContainsKey(Prefix + global.uid)
            && scene.events[Prefix + global.uid].name == Prefix + global.uid
            && scene.events[Prefix + global.uid].type == specific.type
            && scene.events[Prefix + global.uid].evt.SequenceEqual(specific.evt),
            "Scene save/load preserves selected global UIDs, event lists and trigger modes without a schema field");
        Set(global.name, Num(1)); Tick();
        Check(captures.Select(item => item.tag).OrderBy(tag => tag).SequenceEqual(new[] { 51, 52 }),
            "The restored scene bindings actually dispatch their saved programs");
        var legacyJson = SceneForm.GetJoByData(scene); legacyJson.Remove("events");
        var legacy = SceneForm.GetDataByJo(legacyJson);
        Check(legacy.events != null && legacy.events.Count == 0, "Old scene JSON without event data defaults safely to no bindings");
        game.curScene = legacy; captures.Clear(); Set(global.name, Num(2));
        Check(EventInterpretDataForm.DataByUid.Count == 0, "An old scene with no global bindings does not run another scene's handlers");
        game.curScene = scene;
        int[] programs = scene.events.Values.SelectMany(item => item.evt).ToArray();
        new ModAssetCtrl(null).DeleteGlobalVar(global.uid);
        Check(SceneForm.DataByUid.Values.All(item => !item.events.ContainsKey(Prefix + global.uid)
            && item.events.ContainsKey(Prefix)) && programs.All(id => EventProgramDataForm.DataByUid.ContainsKey(id)),
            "Deleting a global retires exact bindings in all scenes but preserves Any bindings and event programs");
        var replacement = new EventGlobalVariableForm.Data(global.uid, "replacement", Num(0));
        EventGlobalVariableForm.AddData(replacement);
        Set(replacement.name, Num(1)); Tick();
        Check(captures.Count == 1 && captures[0].tag == 51,
            "A reused global UID cannot activate the deleted variable's previous exact trigger");
        Check(SaveAndLoad.GetRealPath(path) == Path.GetFullPath(path), "No scene persistence path escaped the explicit owned directory");
    }

    private static BoxDataForm.Data Num(float value) => CodeHelper.CreateBoxByNum(value);
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Nested(float x, float height)
    {
        var value = CodeHelper.CreateBox();
        value.dic["x"] = Num(x);
        var nested = CodeHelper.CreateBox(); nested.dic["height"] = Num(height);
        value.dic["nested"] = nested;
        return value;
    }

    private static void Set(string name, BoxDataForm.Data value)
    {
        var task = new InterpretAsyncTask(null);
        new SetGlobalVarCmd().Execute(new[] { value, Text(name) }, new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && string.IsNullOrEmpty(task.error), "Actual SetGlobalVar completes: " + task.error);
    }

    private static void Tick()
    {
        events.LateUpdate();
        Check(progress.blockProgramUid == 0, "Global handlers leave the ordinary event scheduler unblocked");
    }

    private static bool Equal(BoxDataForm.Data a, BoxDataForm.Data b)
    {
        if (ReferenceEquals(a, b)) return true;
        int count = a?.dic?.Count ?? 0;
        if (count != (b?.dic?.Count ?? 0)) return false;
        if (count == 0) return a?.str == b?.str && (a?.str != null || (a?.num ?? 0).Equals(b?.num ?? 0));
        foreach (var pair in a.dic)
            if (!b.dic.TryGetValue(pair.Key, out var other) || !Equal(pair.Value, other)) return false;
        return true;
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject(typeof(T).Name); go.SetActive(false); owned.Add(go);
        var manager = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, manager);
        return manager;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
