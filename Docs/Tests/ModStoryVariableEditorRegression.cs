using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Ui.Notify;
using Ui.ModStory.ModStoryEvent;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text.Form;
using Z_Trick.BaseFunc;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem;
using CharacterArgs = Ui.ModStory.ModStoryParameter.ModStoryCharacterParameter;
using ItemArgs = Ui.ModStory.ModStoryParameter.ModStoryItemParameter;
using ObjectArgs = Ui.ModStory.ModStoryParameter.ModStorySceneObjectParameter;
using SkillArgs = Ui.ModStory.ModStoryParameter.ModStorySkillParameter;
using GlobalArgs = Ui.ModStory.ModStoryEvent.ModStoryGlobalVar;

// Only runs in an isolated MapRuntimeRegression-* project. No authored scene,
// prefab, or production save is loaded. Inactive real controls exercise binding
// and callbacks without TMP mesh/font resources or real popup interaction.
public static class ModStoryVariableEditorRegression
{
    private static readonly List<GameObject> owned = new List<GameObject>();
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static ModAssetCtrl assets;
    private static UiNotifyCtrl notify;
    private static int checks;
    private static string ownedFolder;

    public static void Run()
    {
        try
        {
            string projectName = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name;
            Check(projectName.StartsWith("MapRuntimeRegression-", StringComparison.Ordinal),
                "The fixture may run only in its isolated temporary Unity project");
            SetUp();
            GlobalVariableCrud();
            TypedChooserCallbacks();
            EventTabs();
            GlobalVariableListButtons();
            DefaultParameterRows();
            GlobalVariableRow();
            CoreAndSaveIsolation();
            Debug.Log("MOD_STORY_VARIABLE_EDITOR_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) UnityEngine.Object.DestroyImmediate(owned[i]);
            if (ownedFolder != null && Directory.Exists(ownedFolder))
            {
                string prefix = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar + "ZPluginVariableEditorRegression-";
                string target = Path.GetFullPath(ownedFolder);
                if (target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    Directory.Delete(target, true);
            }
        }
    }

    private static void SetUp()
    {
        foreach (Type form in new[] { typeof(BoxDataForm), typeof(EventGlobalVariableForm),
            typeof(GameParamForm), typeof(CharacterParamForm), typeof(ItemParamForm),
            typeof(MapObjectParamForm), typeof(SkillParamForm), typeof(CommonTextForm), typeof(ModTextForm) })
            form.GetMethod("Register", PrivateStatic)?.Invoke(null, null);
        GameParamForm.RegisterGet();
        EventGlobalVariableForm.Clear();
        CommonTextForm.Init(); ModTextForm.Init();
        var ui = Manager<UiManager>();
        var mod = Manager<ModManager>();
        assets = new ModAssetCtrl(mod);
        mod.assetCtrl = assets;

        // Keep this holder disabled and the controller uninitialized. NotifyManager
        // therefore uses ShowUi/SetParam; it never builds a real popup or input UI.
        var notifyHolder = Holder("notification-capture");
        notify = new UiNotifyCtrl { uiHolder = notifyHolder, inited = false };
        notifyHolder.ctrl = notify;
        ui.uiCtrlName2Uis[nameof(UiNotifyCtrl)] = new List<UiHolder> { notifyHolder };
        Check(!notify.active, "Notification capture cannot enter the real popup-rendering path");
    }

    private static void GlobalVariableCrud()
    {
        EventGlobalVariableForm.Clear();
        var first = assets.CreateGlobalVar();
        var second = assets.CreateGlobalVar();
        Check(first != null && second != null && first.uid != second.uid && first.name != second.name,
            "New creates independent named records with allocated UIDs");
        Check(first.data != null && first.data.str == null && first.data.num == 0
            && !ReferenceEquals(first.data, second.data), "Every new global owns a numeric-zero Box");
        int uid = first.uid;
        string oldName = first.name;
        Check(assets.RenameGlobalVar(uid, "Global initial value") && first.uid == uid
            && !EventGlobalVariableForm.DataByName.ContainsKey(oldName)
            && ReferenceEquals(EventGlobalVariableForm.DataByName[first.name], first),
            "Rename updates the name index while retaining row and UID identity");
        Check(assets.RenameGlobalVar(uid, first.name), "Keeping the current name is an idempotent success");
        foreach (string invalid in new[] { null, "", " ", "\t\n", second.name })
            Check(!assets.RenameGlobalVar(uid, invalid) && first.name == "Global initial value"
                && EventGlobalVariableForm.DataByUid.Count == 2,
                "Empty, whitespace and duplicate names are rejected without registry changes");
        Check(!assets.RenameGlobalVar(int.MaxValue, "missing"), "Renaming a missing UID is harmless");
        Check(assets.CreateGlobalVar(first.name) == null && EventGlobalVariableForm.DataByUid.Count == 2,
            "Explicit duplicate creation cannot replace an existing record");
        var upper = assets.CreateGlobalVar("Case");
        var lower = assets.CreateGlobalVar("case");
        Check(upper != null && lower != null && upper.uid != lower.uid,
            "Editor names preserve the runtime's case-sensitive global lookup");
        assets.DeleteGlobalVar(uid);
        assets.DeleteGlobalVar(uid);
        assets.DeleteGlobalVar(int.MaxValue);
        Check(!EventGlobalVariableForm.DataByUid.ContainsKey(uid)
            && !EventGlobalVariableForm.DataByName.ContainsKey(first.name)
            && EventGlobalVariableForm.DataByName.ContainsKey(second.name),
            "Delete removes only its target and remains safe when repeated or missing");
    }

    private static void TypedChooserCallbacks()
    {
        var box = CodeHelper.CreateBoxByStr("existing text");
        box.num = 42;
        box.valName = "old-reference";
        box.dic = new Dictionary<string, BoxDataForm.Data> { ["old"] = CodeHelper.CreateBoxByNum(7) };
        int completed = 0;
        var choice = Choose(box, () => completed++);
        Check(completed == 0 && box.str == "existing text" && box.dic.Count == 1,
            "Opening the chooser cannot commit or mutate the supplied value");
        var input = SelectType(choice, 2);
        Check(input.defaultText == "existing text", "Text input starts from its string, never the numeric field");
        Check(input.func("new text") && completed == 1 && box.str == "new text" && box.num == 0
            && box.valName == null && box.dic == null,
            "Text confirmation commits once, refreshes its caller and removes stale dictionary/reference state");
        var numberInput = SelectType(Choose(box, () => completed++), 1);
        Check(!numberInput.func("not a number") && completed == 1 && box.str == "new text",
            "Invalid numeric input keeps the input open without commit or completion");
        Check(numberInput.func(12.5f.ToString()) && completed == 2 && box.str == null && box.num == 12.5f
            && box.valName == null && box.dic == null,
            "Numeric confirmation replaces string type and invokes completion once");
        var emptyInput = SelectType(Choose(box, () => completed++), 2);
        Check(emptyInput.defaultText == "" && emptyInput.func("") && completed == 3 && box.str == "",
            "Switching a number to an empty text value preserves explicit empty-string semantics");
        var before = notify.param;
        assets.ChooseBasicCmd(null, () => completed++);
        Check(ReferenceEquals(before, notify.param) && completed == 3,
            "A null Box cannot open a chooser or manufacture completion");
    }

    private static void EventTabs()
    {
        var holder = Holder("event-tabs");
        var custom = Holder("custom-event-page");
        custom.ctrl = new Ui.ModStory.ModStoryEvent.ModStoryEventCustom.UiModStoryEventCustomCtrl { uiHolder = custom };
        var global = Holder("global-variable-page");
        global.ctrl = new GlobalArgs.UiModStoryGlobalVarCtrl { uiHolder = global };
        var customButton = Component<Btn>("custom-tab");
        var customState = State("custom-state");
        var globalButton = Component<Btn>("global-tab");
        var globalState = State("global-state");
        holder.elementTrsLst = new List<Transform> { Component<ScrView>("tabs-scroll").transform,
            custom.transform, global.transform, customButton.transform, customState.transform,
            globalButton.transform, globalState.transform };
        var ctrl = new UiModStoryEventCtrl { uiHolder = holder,
            view = new UiModStoryEventView(holder), model = new UiModStoryEventModel() };
        ctrl.OnCreate();
        ctrl.OnShow();
        Check(custom.gameObject.activeSelf && !global.gameObject.activeSelf
            && customState.state == 1 && globalState.state == 0, "Event page defaults to the custom-event tab");
        globalButton.onClick.Invoke();
        Check(ctrl.model.selPage == 1 && !custom.gameObject.activeSelf && global.gameObject.activeSelf
            && customState.state == 0 && globalState.state == 1,
            "The actual global-variable button switches pages and selection states together");
        customButton.onClick.Invoke();
        Check(ctrl.model.selPage == 0 && custom.gameObject.activeSelf && !global.gameObject.activeSelf,
            "Returning to custom events closes the global page");
        ctrl.SetParam(new UiModStoryEventParam { selPage = 1 });
        ctrl.OnShow();
        Check(global.gameObject.activeSelf && !custom.gameObject.activeSelf,
            "An explicit GlobalVar page parameter is respected on reopen");
        ctrl.SelPage(100);
        Check(ctrl.model.selPage == 0 && custom.gameObject.activeSelf && !global.gameObject.activeSelf,
            "Invalid page requests fall back to a valid mutually exclusive tab");
    }

    private static void DefaultParameterRows()
    {
        var character = new CharacterParamForm.Data(73101, "typed-character", default, "", "", "", default);
        CharacterParamForm.AddData(character);
        var item = new ItemParamForm.Data(73102, "typed-item", default, "", "", "", default);
        ItemParamForm.AddData(item);
        var obj = new MapObjectParamForm.Data(73103, "typed-object", default, "", "", "");
        MapObjectParamForm.AddData(obj);
        var skill = new SkillParamForm.Data(73104, "typed-skill", default, "", "", "", default);
        SkillParamForm.AddData(skill);
        CheckDefaultRow<CharacterArgs.UiArgCtrl>(character, new CharacterArgs.UiArgParam { data = character },
            new CharacterArgs.UiArgParam(), () => character.Copy().GetValue());
        CheckDefaultRow<ItemArgs.UiArgCtrl>(item, new ItemArgs.UiArgParam { data = item },
            new ItemArgs.UiArgParam(), () => item.Copy().GetValue());
        CheckDefaultRow<ObjectArgs.UiArgCtrl>(obj, new ObjectArgs.UiArgParam { data = obj },
            new ObjectArgs.UiArgParam(), () => obj.Copy().GetValue());
        CheckDefaultRow<SkillArgs.UiArgCtrl>(skill, new SkillArgs.UiArgParam { data = skill },
            new SkillArgs.UiArgParam(), () => skill.Copy().GetValue());
    }

    private sealed class CapturingGlobalList : UiScrViewContainer<GlobalArgs.UiArgCtrl>
    {
        public int renders;
        public CapturingGlobalList(UiCtrl owner, GameObject template, ScrView scroll)
            : base(owner, template, scroll) { }
        // Keep the production list-building/clear logic, isolating only virtualized
        // screen geometry. No real prefab is needed to inspect the planned rows.
        public override void Refresh(List<Vector3> offsets = null) { renders++; }
    }

    private static void GlobalVariableListButtons()
    {
        EventGlobalVariableForm.Clear();
        var parentHolder = Holder("global-variable-list");
        var page = new GlobalArgs.UiModStoryGlobalVarCtrl { uiHolder = parentHolder };
        parentHolder.ctrl = page;
        var list = new CapturingGlobalList(page, Go("global-list-template"), Component<ScrView>("global-list-scroll"));
        typeof(GlobalArgs.UiModStoryGlobalVarCtrl).GetField("argCon", BindingFlags.Instance | BindingFlags.NonPublic)
            .SetValue(page, list);
        page.Refresh();
        Check(list.paramLst.Count == 1 && ((GlobalArgs.UiArgParam)list.paramLst[0]).data == null,
            "An empty global table renders only one New row");
        foreach (int uid in new[] { 903, 901, 902 })
            EventGlobalVariableForm.AddData(new EventGlobalVariableForm.Data(uid, "list-" + uid, CodeHelper.CreateBoxByNum(uid)));
        page.Refresh();
        Check(list.paramLst.Take(3).Cast<GlobalArgs.UiArgParam>().Select(param => param.data.uid)
                .SequenceEqual(new[] { 901, 902, 903 })
            && ((GlobalArgs.UiArgParam)list.paramLst.Last()).data == null,
            "Global rows are sorted by UID with New last, not by insertion order");
        EventGlobalVariableForm.Clear(); page.Refresh();
        var rowHolder = ArgHolder("global-list-button-row", false);
        rowHolder.parent = parentHolder;
        var row = new GlobalArgs.UiArgCtrl();
        row.BindHolderRecursively(rowHolder); row.inited = true; row.OnCreate();
        rowHolder.gameObject.SetActive(true);
        row.SetParam(new GlobalArgs.UiArgParam()); row.OnShow();
        int renders = list.renders;
        rowHolder.elementTrsLst[2].GetComponent<Btn>().onClick.Invoke();
        Check(EventGlobalVariableForm.DataByUid.Count == 1 && list.paramLst.Count == 2
            && list.renders == renders + 1,
            "The actual New button creates one zero-valued global and refreshes its parent list once");
        var data = EventGlobalVariableForm.DataByUid.Values.Single();
        Check(((GlobalArgs.UiArgParam)list.paramLst[0]).data == data
            && ((GlobalArgs.UiArgParam)list.paramLst[1]).data == null && data.data.num == 0,
            "The created global is displayed before the retained New row");
        row.SetParam(new GlobalArgs.UiArgParam { data = data }); row.OnShow();
        renders = list.renders;
        rowHolder.elementTrsLst[3].GetComponent<Btn>().onClick.Invoke();
        Check(EventGlobalVariableForm.DataByUid.Count == 0 && list.paramLst.Count == 1
            && ((GlobalArgs.UiArgParam)list.paramLst[0]).data == null && list.renders == renders + 1,
            "The actual Delete button removes its row and returns the list to New-only");
        rowHolder.elementTrsLst[3].GetComponent<Btn>().onClick.Invoke();
        page.Refresh(); page.Refresh();
        Check(EventGlobalVariableForm.DataByUid.Count == 0 && list.paramLst.Count == 1,
            "Repeated deletion and refresh cannot duplicate globals or New rows");
    }

    private static void CheckDefaultRow<T>(GameParamForm.Data data, UiParam existing, UiParam newRow,
        Func<BoxDataForm.Data> copiedValue) where T : UiCtrl, new()
    {
        string label = typeof(T).Namespace;
        // Some shared prefab revisions retain an unused visibility dropdown on
        // Object rows. Follow the actual generated View rather than guessing it.
        var viewType = typeof(T).Assembly.GetType(label + ".UiArgView", true);
        bool dropdown = viewType.GetField("dp_") != null;
        UiHolder holder = ArgHolder(label, dropdown);
        var ctrl = new T();
        ctrl.BindHolderRecursively(holder);
        ctrl.inited = true;
        ctrl.OnCreate();
        holder.gameObject.SetActive(true);
        ctrl.SetParam(existing);
        ctrl.OnShow();
        var valueButton = holder.elementTrsLst[5].GetComponent<Btn>();
        var valueText = holder.elementTrsLst[dropdown ? 7 : 6].GetComponent<Txt>();
        var state = holder.elementTrsLst[1].GetComponent<Sta>();
        Check(state.state == 1, label + " displays an existing row rather than New");
        valueButton.onClick.Invoke();
        var input = SelectType(LastChoice(), 2);
        string text = "typed default: " + label;
        Check(input.func(text) && data.GetValue().str == text && valueText.text == text,
            label + " edits text through the actual button/type/input callback chain");
        Check(copiedValue().str == text && !string.IsNullOrEmpty(data.v),
            label + " synchronizes serialized v so an immediate unsaved Copy receives its typed default");
        ctrl.OnShow();
        int requests = NotifyManager.popupIdCnt;
        valueButton.onClick.Invoke();
        Check(NotifyManager.popupIdCnt == requests + 1,
            label + " repeated Show keeps exactly one value-button listener");
        Check(SelectType(LastChoice(), 1).func("23") && data.GetValue().str == null
            && data.GetValue().num == 23 && copiedValue().num == 23 && valueText.text == "23",
            label + " can switch back to numeric defaults after repeated Show without stale text");
        ctrl.SetParam(newRow); ctrl.OnShow();
        var before = notify.param;
        valueButton.onClick.Invoke();
        Check(state.state == 0 && ReferenceEquals(before, notify.param),
            label + " New rows have no value chooser or null-data callbacks");
        ctrl.SetParam(null); ctrl.OnShow();
        Check(!holder.gameObject.activeSelf, label + " parameterless prefab templates stay disabled");
    }

    private static void GlobalVariableRow()
    {
        EventGlobalVariableForm.Clear();
        var data = assets.CreateGlobalVar("editor-global");
        data.data = CodeHelper.CreateBoxByStr("initial");
        var holder = ArgHolder("global-variable-row", false);
        var row = new GlobalArgs.UiArgCtrl();
        row.BindHolderRecursively(holder); row.inited = true; row.OnCreate();
        holder.gameObject.SetActive(true);
        row.SetParam(new GlobalArgs.UiArgParam { data = data }); row.OnShow();
        var valueButton = holder.elementTrsLst[5].GetComponent<Btn>();
        var valueText = holder.elementTrsLst[6].GetComponent<Txt>();
        var nameInput = holder.elementTrsLst[4].GetComponent<Ipt>();
        valueButton.onClick.Invoke();
        var input = SelectType(LastChoice(), 2);
        Check(data.data.str == "initial" && input.defaultText == "initial",
            "Global-row selection edits a draft and preserves the initial value until confirmation");
        Check(input.func("authored text") && data.data.str == "authored text" && valueText.text == "authored text",
            "Global-row confirmation publishes and refreshes the edited Box");
        nameInput.onFinishInput("renamed-global");
        Check(data.name == "renamed-global" && nameInput.text == data.name,
            "The actual global-row name callback updates registry and rendered text");
        nameInput.onFinishInput("");
        Check(data.name == "renamed-global" && nameInput.text == data.name,
            "Rejected global rename restores the existing rendered name");

        valueButton.onClick.Invoke();
        var staleInput = SelectType(LastChoice(), 2);
        int oldUid = data.uid;
        assets.DeleteGlobalVar(oldUid);
        var replacement = new EventGlobalVariableForm.Data(oldUid, "replacement-global", CodeHelper.CreateBoxByNum(77));
        EventGlobalVariableForm.AddData(replacement);
        row.SetParam(new GlobalArgs.UiArgParam { data = replacement }); row.OnShow();
        staleInput.func("must not reach replacement");
        Check(replacement.data.str == null && replacement.data.num == 77 && valueText.text == "77",
            "A stale popup cannot overwrite or refresh a replacement row that reused the old UID");
        replacement.data = null;
        row.OnShow();
        Check(valueText.text == "0", "Legacy null global payloads display a safe zero without throwing");
        row.SetParam(new GlobalArgs.UiArgParam()); row.OnShow();
        var before = notify.param; valueButton.onClick.Invoke();
        Check(ReferenceEquals(before, notify.param), "The global New row cannot edit a nonexistent value");
        row.SetParam(null); row.OnShow();
        Check(!holder.gameObject.activeSelf, "The global prefab template remains disabled without a Param");
    }

    private static void CoreAndSaveIsolation()
    {
        EventGlobalVariableForm.Clear();
        var initial = assets.CreateGlobalVar("initial-value");
        initial.data = CodeHelper.CreateBoxByStr("authored");
        ownedFolder = Path.Combine(Path.GetTempPath(), "ZPluginVariableEditorRegression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ownedFolder);
        string core = Path.Combine(ownedFolder, "story-a", "Core");
        string save = Path.Combine(ownedFolder, "story-a", "Save");
        var persistence = new GameSaveController(null);
        persistence.SaveGlobalVariables(core);
        persistence.LoadGlobalVariables(core);
        Check(ReadGlobal("initial-value").str == "authored", "A new Play session starts from the authored Core value");
        SetGlobal("initial-value", CodeHelper.CreateBoxByNum(19));
        SetGlobal("runtime-only", CodeHelper.CreateBoxByStr("saved"));
        persistence.SaveGlobalVariables(save);
        persistence.LoadGlobalVariables(core);
        Check(ReadGlobal("initial-value").str == "authored" && ReadGlobal("runtime-only").num == 0
            && EventGlobalVariableForm.DataByUid.Count == 1,
            "Returning to Mod or starting a fresh test restores Core without leaking runtime-only values");
        persistence.LoadGlobalVariables(save);
        Check(ReadGlobal("initial-value").str == null && ReadGlobal("initial-value").num == 19
            && ReadGlobal("runtime-only").str == "saved",
            "Continuing a saved Play keeps player values instead of reapplying authored defaults");
        persistence.LoadGlobalVariables(Path.Combine(ownedFolder, "legacy-story"));
        Check(EventGlobalVariableForm.DataByUid.Count == 0, "A story without initial globals does not inherit previous story values");
        persistence.LoadGlobalVariables(null);
        Check(EventGlobalVariableForm.DataByName.Count == 0, "Story reset also clears the name registry");
    }

    private static ChooseInfo Choose(BoxDataForm.Data value, Action complete)
    {
        notify.SetParam(null);
        assets.ChooseBasicCmd(value, complete);
        return LastChoice();
    }

    private static ChooseInfo LastChoice()
    {
        var choice = notify.param?.chooseInfo;
        Check(choice != null && choice.items.subs.Count == 2
            && choice.items.subs.Values.Select(item => item.id).OrderBy(id => id).SequenceEqual(new[] { 1, 2 }),
            "The actual notification request offers exactly numeric and text choices");
        return choice;
    }

    private static InputAreaInfo SelectType(ChooseInfo choice, int id)
    {
        var option = choice.items.subs.Values.Single(item => item.id == id);
        notify.SetParam(null);
        Check(choice.func(option), "A type choice completes and opens its value input");
        var input = notify.param?.inputAreaInfo;
        Check(input != null && input.func != null, "The real value-input request carries its confirmation callback");
        return input;
    }

    private static void SetGlobal(string name, BoxDataForm.Data value)
    {
        var task = new InterpretAsyncTask(null);
        new SetGlobalVarCmd().Execute(new[] { value, CodeHelper.CreateBoxByStr(name) },
            new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && string.IsNullOrEmpty(task.error), "The actual runtime setter accepts the authored global");
    }

    private static BoxDataForm.Data ReadGlobal(string name)
    {
        var task = new InterpretAsyncTask(null);
        new GetGlobalVarCmd().Execute(new[] { CodeHelper.CreateBoxByStr(name) },
            new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && string.IsNullOrEmpty(task.error) && task.res?.Length == 1,
            "The actual runtime getter reads the selected Core/Save table");
        return task.res[0];
    }

    private static UiHolder ArgHolder(string name, bool dropdown)
    {
        var holder = Holder(name);
        var elements = new List<Transform> { holder.transform, State(name + "-state").transform,
            Component<Btn>(name + "-new").transform, Component<Btn>(name + "-delete").transform,
            Component<Ipt>(name + "-name").transform, Component<Btn>(name + "-value").transform };
        if (dropdown) elements.Add(Component<Dp>(name + "-visibility").transform);
        elements.Add(Component<Txt>(name + "-text").transform);
        holder.elementTrsLst = elements;
        return holder;
    }

    private static Sta State(string name)
    {
        var state = Component<Sta>(name);
        var states = new[] { Go(name + "-0"), Go(name + "-1") };
        foreach (var child in states) child.transform.SetParent(state.transform, false);
        typeof(State).GetField("stateGo", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(state, states);
        return state;
    }

    private static T Component<T>(string name) where T : Component
    {
        var go = Go(name);
        var component = go.AddComponent<T>();
        if (component is Behaviour behaviour) behaviour.enabled = false;
        return component;
    }

    private static UiHolder Holder(string name) => Component<UiHolder>(name);

    private static GameObject Go(string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.SetActive(false);
        owned.Add(go);
        return go;
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var manager = Component<T>(typeof(T).Name);
        typeof(Z_MonoSingleton<T>).GetField("_instance", PrivateStatic).SetValue(null, manager);
        return manager;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
