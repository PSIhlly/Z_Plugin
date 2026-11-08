using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using Ui.EventChoose;
using Ui.ModSceneBehaviourUnit;
using Ui.ModStoryEventTriggerWindow;
using Ui.Notify;
using UnityEditor;
using UnityEngine;
using UnityEngine.EventSystems;
using UnityEngine.UI;
using Object = UnityEngine.Object;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Text;
using Z_Trick.BaseFunc;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Z_UnitSystem.Form;

// Real production controllers, event resolution, pooling and row-local history.
// The runner supplies an isolated Unity project; no authored scene/save is opened.
public static class SceneBehaviourTriggerRegression
{
    [ExecuteAlways]
    public sealed class ActiveHolder : UiHolder { }
    private static int checks;
    private static readonly BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly HashSet<Type> registered = new HashSet<Type>();
    private const string Key = "onObjectTouchEvent";

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name.StartsWith("MapRuntimeRegression-"), "isolated project");
            foreach (var form in new[] { typeof(ObjectUnitForm), typeof(ObjectProductForm), typeof(MapMainForm),
                typeof(EventTriggerForm), typeof(CommonTextForm), typeof(ModTextForm), typeof(EventProgramDataForm),
                typeof(CmdTypeDataForm), typeof(CharacterProductForm), typeof(ItemProductForm), typeof(LabForm) })
                Register(form);
            Register(typeof(EventGlobalVariableForm));
            EventGlobalVariableForm.AddData(new EventGlobalVariableForm.Data(776, "watched variable", Z_Code.CodeHelper.CreateBoxByNum(0)));
            EventGlobalVariableForm.AddData(new EventGlobalVariableForm.Data(777, "another variable", Z_Code.CodeHelper.CreateBoxByNum(0)));
            var ui = Manager<UiManager>();
            var map = Manager<MapManager>();
            map.data = new GameMapData { mainData = MapMainForm.GetDataByJo(new JObject()) };
            var mod = Manager<ModManager>();
            mod.assetCtrl = new ModAssetCtrl(mod);
            var scene = new ModSceneController(mod);
            mod.sceneCtrl = scene;
            var history = new ModSceneHistory(map);
            typeof(ModSceneController).GetField("enable", Hidden).SetValue(scene, true);
            typeof(ModSceneController).GetField("history", Hidden).SetValue(scene, history);
            var game = Manager<GameManager>();
            game.evtCtrl = new GameEventController(game);
            var notify = Capture<UiNotifyCtrl>(ui);
            var window = Window();
            ui.uiCtrlName2Uis[typeof(UiModStoryEventTriggerWindowCtrl).Name] = new List<UiHolder> { window.uiHolder };

            var prototype = ObjectProductForm.GetDataByJo(new JObject());
            prototype.uid = 92001;
            prototype.events = new Dictionary<string, EventTriggerForm.Data> {
                [Key] = new EventTriggerForm.Data(-1, Key, new List<int> { 101, 102 }, TriggerType.OnceDuring),
                ["onLeaveSceneEvent"] = new EventTriggerForm.Data(-1, "onLeaveSceneEvent", new List<int> { 103 }, default)
            };
            const string globalKey = "onGlobalVarChangeEvent$776";
            prototype.events[globalKey] = new EventTriggerForm.Data(-1, globalKey, new List<int> { 105 }, default);
            ObjectProductForm.AddData(prototype);
            var data = ObjectUnitForm.GetDataByJo(new JObject());
            data.uid = 92002; data.name = "scene instance";
            data.extra = new JObject { [ObjectProductForm.ProductVersionKey] = ObjectProductForm.ProductVersion,
                [MapUnit.productKey] = new JArray(prototype.uid, -1), ["custom"] = "preserved" }.ToString();
            ObjectUnitForm.AddData(data);
            var unit = data.unit;
            var panel = Panel();
            foreach (var holder in panel.gameObject.GetComponentsInChildren<UiHolder>(true)) holder.enabled = true;
            foreach (var holder in window.gameObject.GetComponentsInChildren<UiHolder>(true)) holder.enabled = true;
            panel.SetParam(new UiModSceneBehaviourUnitParam { data = data });
            panel.gameObject.SetActive(true); panel.uiHolder.Create(); panel.OnShow(); panel.showed = true;
            DriveRows(panel.uiHolder);
            var rows = TriggerRows(panel);
            Check(rows.Select(row => row.model.prm.key).SequenceEqual(UiSceneEventChooseList.TriggerKeys), "all twelve shared triggers in order");
            Check(rows.Last().model.prm.key == "onDestroyEvent", "destroy trigger is included in the shared Unit list");
            Check(rows.All(row => row.gameObject.GetComponentsInChildren<Txt>(true).Single().oriText == row.model.prm.key), "txt_ contains trigger name");
            string originalExtra = data.extra;
            int[] ids = rows.Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).ToArray();
            panel.Refresh(); DriveRows(panel.uiHolder);
            Check(TriggerRows(panel).Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).SequenceEqual(ids), "refresh reuses trigger instances");
            TriggerRows(panel).Single(row => row.model.prm.key == Key).gameObject.GetComponentInChildren<Btn>(true).onClick.Invoke();
            Drive(window);
            Check(window.model.prm.trigger.evt.SequenceEqual(new[] { 101, 102 }), "click opens prototype events");
            Check(data.extra == originalExtra && history.undoCount == 0, "opening and rendering do not mutate or record history");
            Check(!window.view.btn_delete.gameObject.activeSelf, "scene trigger delete is hidden");
            foreach (var row in EventRows(window).Where(row => row.model.prm.uid != -1))
            {
                Check(!row.view.btn_delete.gameObject.activeSelf && !row.view.btn_edit.gameObject.activeSelf, "existing scene rows are read-only");
                row.view.btn_delete.onClick.Invoke(); row.view.btn_edit.onClick.Invoke();
            }
            window.view.btn_delete.onClick.Invoke();
            Check(window.gameObject.activeSelf && prototype.events[Key].evt.SequenceEqual(new[] { 101, 102 }), "direct button calls cannot remove inherited events or trigger");

            var append = EventRows(window).Last();
            Check(append.model.prm.uid == -1, "only append row follows inherited events");
            append.view.btn_add.onClick.Invoke();
            notify.param.multipleChooseInfo.func(new EntryItem { id = 101 });
            DriveRows(window.uiHolder);
            Check(unit.evtDic[Key].evt.SequenceEqual(new[] { 101 }), "instance stores only the appended ID, including a duplicate inherited ID");
            Check(unit.GetEvt(Key).evt.SequenceEqual(new[] { 101, 102, 101 }) && unit.GetEvt(Key).type == TriggerType.OnceDuring, "runtime resolves prototype first and preserves its trigger type");
            Check(EventRows(window).Select(row => row.model.prm.uid).SequenceEqual(new[] { 101, 102, 101, -1 }), "window refresh keeps append at the end");
            Check(history.undoCount == 1 && history.lastCapturedRowCount == 2, "one append records exactly its Unit before and after");
            Check((string)JObject.Parse(data.extra)["custom"] == "preserved", "serialization preserves unrelated extra fields");
            var reloaded = ObjectUnitForm.GetDataByJo(ObjectUnitForm.GetJoByData(data));
            Check(reloaded.unit.GetEvt(Key).evt.SequenceEqual(new[] { 101, 102, 101 }), "Form round-trip preserves appended runtime events");
            prototype.events[Key].evt.Add(104);
            Check(unit.GetEvt(Key).evt.SequenceEqual(new[] { 101, 102, 104, 101 }), "later prototype edits remain visible before instance events");
            string appendedExtra = data.extra;
            data.extra = originalExtra;
            Check(unit.GetEvt(Key).evt.SequenceEqual(new[] { 101, 102, 104 }), "restoring extra invalidates the local event cache");
            data.extra = appendedExtra;
            Check(unit.GetEvt("onLeaveSceneEvent").evt.SequenceEqual(new[] { 103 }), "leave-scene resolution includes prototype events");

            TriggerRows(panel).Single(row => row.model.prm.key == Key).view.btn_.onClick.Invoke(); Drive(window);
            var localRow = EventRows(window)[3];
            Check(window.view.btn_delete.gameObject.activeSelf && localRow.view.btn_edit.gameObject.activeSelf
                && localRow.view.btn_delete.gameObject.activeSelf, "local suffix and binding retain normal editing buttons");
            localRow.view.btn_edit.onClick.Invoke(); notify.param.multipleChooseInfo.func(new EntryItem { id = 107 });
            DriveRows(window.uiHolder);
            Check(unit.evtDic[Key].evt.SequenceEqual(new[] { 107 })
                && prototype.events[Key].evt.SequenceEqual(new[] { 101, 102, 104 }), "editing the local index serializes extra without changing inherited rows");
            EventRows(window).Last().view.btn_add.onClick.Invoke(); notify.param.multipleChooseInfo.func(new EntryItem { id = 107 });
            DriveRows(window.uiHolder); EventRows(window)[4].view.btn_delete.onClick.Invoke(); DriveRows(window.uiHolder);
            Check(unit.evtDic[Key].evt.SequenceEqual(new[] { 107 }), "deleting a duplicate local ID addresses only the clicked suffix index");
            window.view.btn_delete.onClick.Invoke();
            Check(!unit.evtDic.ContainsKey(Key) && unit.GetEvt(Key).evt.SequenceEqual(new[] { 101, 102, 104 }),
                "deleting the local binding preserves the entire inherited trigger");
            TriggerRows(panel).Single(row => row.model.prm.key == Key).view.btn_.onClick.Invoke(); Drive(window);
            Check(!window.view.btn_delete.gameObject.activeSelf && EventRows(window).Take(3)
                .All(row => !row.view.btn_edit.gameObject.activeSelf && !row.view.btn_delete.gameObject.activeSelf),
                "after local deletion the window returns to inherited read-only rows");
            EventRows(window).Last().view.btn_add.onClick.Invoke(); notify.param.multipleChooseInfo.func(new EntryItem { id = 101 });
            DriveRows(window.uiHolder); appendedExtra = data.extra;

            EventRows(window).Last().view.btn_add.onClick.Invoke();
            var stale = notify.param.multipleChooseInfo;
            panel.OnHide();
            stale.func(new EntryItem { id = 999 });
            Check(data.extra == appendedExtra, "closing the unit panel rejects a pending append callback");
            panel.SetParam(new UiModSceneBehaviourUnitParam { data = data }); panel.OnShow(); DriveRows(panel.uiHolder);
            TriggerRows(panel).First().gameObject.GetComponentInChildren<Btn>(true).onClick.Invoke(); Drive(window);
            Check(window.model.prm.trigger.name == UiSceneEventChooseList.TriggerKeys[0], "reopening binds rows to their current trigger");

            // The shared Story editor retains normal edit/delete behavior. Duplicate IDs
            // must address the clicked index rather than the first matching value.
            var regular = GameEventController.CreateTrigger("onShowEvent"); regular.evt.AddRange(new[] { 101, 102, 101 });
            window.SetParam(new UiModStoryEventTriggerWindowParam { trigger = regular,
                dic = new Dictionary<string, EventTriggerForm.Data> { [regular.name] = regular } });
            window.OnShow(); DriveRows(window.uiHolder);
            var third = EventRows(window)[2]; third.view.btn_delete.onClick.Invoke(); DriveRows(window.uiHolder);
            Check(regular.evt.SequenceEqual(new[] { 101, 102 }), "normal editor deletes the clicked duplicate index");
            Check(window.view.btn_delete.gameObject.activeSelf && EventRows(window)[0].view.btn_edit.gameObject.activeSelf, "pool reuse restores normal Story editing buttons");
            DriveRows(panel.uiHolder);
            var globalRows = GlobalRows(panel);
            Check(globalRows.Select(row => row.model.prm.key).SequenceEqual(new[] { globalKey, null }),
                "the new Model renders inherited global bindings and a trailing New row");
            int globalModelId = globalRows[0].parent.gameObject.GetInstanceID();
            globalRows[0].view.btn_.onClick.Invoke(); Drive(window);
            Check(window.model.prm.getInheritedEventCount() == 1 && window.model.prm.trigger.evt.SequenceEqual(new[] { 105 }),
                "a global binding opens the shared window with an inherited read-only prefix");
            var variableRow = window.uiHolder.subUiHolderLst.Where(holder => holder.gameObject.activeSelf
                && holder.ctrl is UiPrmCtrl).Select(holder => (UiPrmCtrl)holder.ctrl).Single();
            Check(variableRow.view.txt_.text == "watched variable" && variableRow.view.txt_.gameObject.activeInHierarchy
                && !variableRow.view.btn_edit.interactable,
                "the specific global name remains visible inside the disabled read-only edit Button");
            EventRows(window).Last().view.btn_add.onClick.Invoke();
            notify.param.multipleChooseInfo.func(new EntryItem { id = 106 });
            DriveRows(panel.uiHolder); DriveRows(window.uiHolder);
            Check(unit.evtDic[globalKey].evt.SequenceEqual(new[] { 106 })
                && unit.GetEvt(globalKey).evt.SequenceEqual(new[] { 105, 106 }),
                "global appends save only local IDs and keep prototype-first execution");
            Check(window.view.btn_delete.gameObject.activeSelf && !EventRows(window)[0].view.btn_edit.gameObject.activeSelf
                && EventRows(window)[1].view.btn_edit.gameObject.activeSelf && EventRows(window)[1].view.btn_delete.gameObject.activeSelf,
                "global bindings share fixed triggers' inherited/local row permissions");
            panel.Refresh(); DriveRows(panel.uiHolder);
            Check(GlobalRows(panel)[0].parent.gameObject.GetInstanceID() == globalModelId,
                "the global Model and inner rows are pooled across panel refresh");
            GlobalRows(panel).Last().view.btn_.onClick.Invoke();
            var variableChoice = notify.param.chooseInfo;
            Check(variableChoice.items.subs.Values.Select(item => item.id).SequenceEqual(new[] { 0, 776, 777 }),
                "New offers Any and story globals in stable UID order");
            window.Close();
            panel.Refresh(); DriveRows(panel.uiHolder);
            variableChoice.func(variableChoice.items.subs.Values.Single(item => item.id == 0));
            Check(window.gameObject.activeInHierarchy && window.param?.trigger?.name == "onGlobalVarChangeEvent$",
                "confirming a global choice activates the event manager with a complete trigger parameter before manual lifecycle driving");
            Drive(window);
            Check(window.model.prm.trigger.name == "onGlobalVarChangeEvent$" && window.model.prm.getInheritedEventCount() == 0
                && unit.evtDic.ContainsKey("onGlobalVarChangeEvent$"),
                "selecting Any explicitly creates and opens an editable local global binding");
            Check(window.view.go_params.activeInHierarchy && window.model.prm.configs.Count == 1
                && window.model.prm.configs[0].prmChangeAct != null && EventRows(window).Last().view.btn_add.gameObject.activeInHierarchy
                && window.view.btn_delete.gameObject.activeSelf,
                "a local global binding exposes parameter edit, trigger delete and child-event Add");
            Check(window.uiHolder.subUiHolderLst.Where(holder => holder.gameObject.activeSelf && holder.ctrl is UiPrmCtrl)
                .All(holder => ((UiPrmCtrl)holder.ctrl).view.txt_.gameObject.activeInHierarchy
                    && ((UiPrmCtrl)holder.ctrl).view.btn_edit.interactable),
                "a local parameter keeps its visible label and editable Button");
            window.model.prm.configs[0].prmChangeAct(window.model.prm.trigger); var rename = notify.param.chooseInfo;
            Check(!rename.func(rename.items.subs.Values.Single(item => item.id == 776))
                && unit.evtDic.ContainsKey("onGlobalVarChangeEvent$"), "renaming cannot overwrite an inherited variable binding");
            rename.func(rename.items.subs.Values.Single(item => item.id == 777)); DriveRows(window.uiHolder);
            Check(window.model.prm.trigger.name == "onGlobalVarChangeEvent$777"
                && unit.evtDic.ContainsKey("onGlobalVarChangeEvent$777") && !unit.evtDic.ContainsKey("onGlobalVarChangeEvent$"),
                "shared Scene parameter editor renames and serializes the local key and trigger name together");
            EventRows(window).Last().view.btn_add.onClick.Invoke(); notify.param.multipleChooseInfo.func(new EntryItem { id = 108 });
            DriveRows(window.uiHolder); EventRows(window)[0].view.btn_edit.onClick.Invoke();
            notify.param.multipleChooseInfo.func(new EntryItem { id = 109 }); DriveRows(window.uiHolder);
            Check(unit.evtDic["onGlobalVarChangeEvent$777"].evt.SequenceEqual(new[] { 109 }),
                "local global events remain editable after renaming their variable");
            EventRows(window)[0].view.btn_delete.onClick.Invoke(); DriveRows(window.uiHolder);
            Check(unit.evtDic["onGlobalVarChangeEvent$777"].evt.Count == 0, "local global child-event deletion is persisted");
            EventRows(window).Last().view.btn_add.onClick.Invoke(); var deletedChoice = notify.param.multipleChooseInfo;
            window.view.btn_delete.onClick.Invoke(); deletedChoice.func(new EntryItem { id = 110 });
            Check(!unit.evtDic.ContainsKey("onGlobalVarChangeEvent$777") && prototype.events[globalKey].evt.SequenceEqual(new[] { 105 }),
                "deleting a local global binding rejects pending event choices and preserves prototype bindings");
            string beforeStaleChoice = data.extra;
            GlobalRows(panel).Last().view.btn_.onClick.Invoke(); variableChoice = notify.param.chooseInfo;
            panel.OnHide();
            variableChoice.func(variableChoice.items.subs.Values.Single(item => item.id == 776));
            Check(data.extra == beforeStaleChoice && !window.gameObject.activeSelf,
                "hiding releases global bindings, closes its window, and rejects an outstanding variable choice");
            ProductGlobalModel(ui, notify, window);
            TouchPassUiFiltering();
            Debug.Log("SCENE_BEHAVIOUR_TRIGGER_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static UiModSceneBehaviourUnitCtrl Panel()
    {
        var root = Holder("scene-behaviour");
        var global = GlobalModel(root);
        var row = Holder("go_trigger", root); row.uiType = UiType.Sub;
        var label = Text(row.gameObject, "txt_");
        row.elementTrsLst.AddRange(new[] { row.transform, Button(row.gameObject, "btn_").transform, label.transform });
        root.subUiHolderLst.AddRange(new[] { global, row });
        root.elementTrsLst.AddRange(new[] { Button(root.gameObject, "bbg").transform,
            Component<Img>(root.gameObject, "image").transform, Text(root.gameObject, "name").transform,
            Button(root.gameObject, "close").transform, Button(root.gameObject, "delete").transform,
            global.transform, global.transform, row.transform, row.transform });
        var ctrl = new UiModSceneBehaviourUnitCtrl(); ctrl.BindHolderRecursively(root); return ctrl;
    }

    private static UiModStoryEventTriggerWindowCtrl Window()
    {
        var root = Holder("trigger-window");
        var prm = Holder("go_prm", root); var evt = Holder("go_event", root);
        var edit = Button(prm.gameObject, "edit");
        prm.elementTrsLst.AddRange(new[] { prm.transform, edit.transform, Text(edit.gameObject, "text").transform });
        var sta = Component<Sta>(evt.gameObject, "state");
        var states = new[] { Child(sta.gameObject, "add"), Child(sta.gameObject, "existing") };
        typeof(State).GetField("stateGo", Hidden).SetValue(sta, states);
        evt.elementTrsLst.AddRange(new[] { evt.transform, sta.transform, Button(evt.gameObject, "add").transform,
            Text(evt.gameObject, "text").transform, Button(evt.gameObject, "delete").transform, Button(evt.gameObject, "edit").transform });
        root.subUiHolderLst.AddRange(new[] { prm, evt });
        root.elementTrsLst.AddRange(new[] { Button(root.gameObject, "bbg").transform, Child(root.gameObject, "params").transform,
            Text(root.gameObject, "name").transform, Button(root.gameObject, "delete").transform,
            Component<ScrView>(root.gameObject, "scroll").transform, Button(root.gameObject, "close").transform,
            prm.transform, prm.transform, evt.transform, evt.transform });
        var ctrl = new UiModStoryEventTriggerWindowCtrl(); ctrl.BindHolderRecursively(root); return ctrl;
    }

    private static void Drive(UiCtrl ctrl) { ctrl.uiHolder.Create(); ctrl.OnShow(); ctrl.showed = true; DriveRows(ctrl.uiHolder); }
    private static void DriveRows(UiHolder holder)
    {
        foreach (var child in holder.subUiHolderLst.Where(row => row.gameObject.activeSelf).ToArray())
        { child.Create(); child.ctrl.OnShow(); child.ctrl.showed = true; DriveRows(child); }
    }
    private static UiEventTriggerCtrl[] TriggerRows(UiModSceneBehaviourUnitCtrl panel) => panel.uiHolder.subUiHolderLst
        .Where(holder => holder.gameObject.activeSelf && holder.ctrl is UiEventTriggerCtrl).OrderBy(holder => holder.transform.GetSiblingIndex())
        .Select(holder => (UiEventTriggerCtrl)holder.ctrl).ToArray();
    private static Ui.TriggerEventChoose.UiTriggerCtrl[] GlobalRows(UiModSceneBehaviourUnitCtrl panel) => panel.uiHolder.subUiHolderLst
        .Where(holder => holder.gameObject.activeSelf && holder.ctrl is Ui.TriggerEventChoose.UiTriggerEventChooseCtrl)
        .SelectMany(holder => holder.subUiHolderLst).Where(holder => holder.gameObject.activeSelf)
        .OrderBy(holder => holder.transform.GetSiblingIndex()).Select(holder => (Ui.TriggerEventChoose.UiTriggerCtrl)holder.ctrl).ToArray();

    private static UiHolder GlobalModel(UiHolder parent)
    {
        var root = Holder("go_triggerEventChoose", parent); root.uiType = UiType.Model;
        var row = Holder("go_trigger", root); row.uiType = UiType.Sub;
        var sta = Component<Sta>(row.gameObject, "state");
        typeof(State).GetField("stateGo", Hidden).SetValue(sta, new[] { Child(sta.gameObject, "new"), Child(sta.gameObject, "bound") });
        row.elementTrsLst.AddRange(new[] { row.transform, Button(row.gameObject, "choose").transform,
            sta.transform, Text(row.gameObject, "name").transform });
        root.subUiHolderLst.Add(row); root.elementTrsLst.AddRange(new[] { row.transform, row.transform });
        return root;
    }
    private static void ProductGlobalModel(UiManager ui, UiNotifyCtrl notify, UiModStoryEventTriggerWindowCtrl window)
    {
        var root = Holder("product-global-owner"); var owner = new UiCtrl(); owner.BindHolderRecursively(root);
        var model = GlobalModel(root); new Ui.TriggerEventChoose.UiTriggerEventChooseCtrl().BindHolderRecursively(model);
        root.subUiHolderLst.Add(model);
        var list = new Ui.TriggerEventChoose.GlobalVariableTriggerList(owner, model.gameObject);
        root.gameObject.SetActive(true); root.Create();
        var first = new Dictionary<string, EventTriggerForm.Data>(); var second = new Dictionary<string, EventTriggerForm.Data>();
        void Render(Dictionary<string, EventTriggerForm.Data> events)
        { list.Render(events, () => true); DriveRows(root); }
        Ui.TriggerEventChoose.UiTriggerCtrl[] Rows() => root.subUiHolderLst
            .Where(holder => holder.gameObject.activeSelf).SelectMany(holder => holder.subUiHolderLst)
            .Where(holder => holder.gameObject.activeSelf).Select(holder => (Ui.TriggerEventChoose.UiTriggerCtrl)holder.ctrl).ToArray();
        Render(first); Check(first.Count == 0 && Rows().Length == 1, "rendering a Product never creates saved bindings");
        Rows()[0].view.btn_.onClick.Invoke(); var stale = notify.param.chooseInfo;
        Render(second); stale.func(stale.items.subs.Values.Single(item => item.id == 776));
        Check(first.Count == 0 && second.Count == 0, "a recycled Model rejects a variable chooser from the previous Product");
        Rows()[0].view.btn_.onClick.Invoke(); var choice = notify.param.chooseInfo;
        Render(second);
        choice.func(choice.items.subs.Values.Single(item => item.id == 776)); Drive(window); DriveRows(root);
        Check(second.ContainsKey("onGlobalVarChangeEvent$776") && window.model.prm.getInheritedEventCount == null
            && window.model.prm.configs.Count == 1 && window.uiHolder.subUiHolderLst
                .Where(holder => holder.gameObject.activeSelf && holder.ctrl is UiPrmCtrl)
                .All(holder => ((UiPrmCtrl)holder.ctrl).view.btn_edit.interactable),
            "Product global bindings restore editable parameter Buttons after reusing read-only instance rows");
        window.view.btn_delete.onClick.Invoke(); DriveRows(root);
        Check(second.Count == 0, "Product global bindings remain deletable through the common window");
        list.Clear(); list.Clear(); Check(!Rows().Any(), "clearing a global Model is idempotent");
    }
    private static UiEventCtrl[] EventRows(UiModStoryEventTriggerWindowCtrl window) => window.uiHolder.subUiHolderLst
        .Where(holder => holder.gameObject.activeSelf && holder.ctrl is UiEventCtrl).OrderBy(holder => holder.transform.GetSiblingIndex())
        .Select(holder => (UiEventCtrl)holder.ctrl).ToArray();

    private static void TouchPassUiFiltering()
    {
        var input = Manager<InputManager>();
        var systemGo = new GameObject("pass-through-event-system", typeof(EventSystem));
        var system = systemGo.GetComponent<EventSystem>();
        // The fixture executes in EditMode, so explicitly drive native UI
        // registration callbacks that normally run when entering Play.
        EventSystem.current = system;
        bool enableSystem = EventSystem.current != system;
        if (enableSystem) typeof(EventSystem).GetMethod("OnEnable", Hidden).Invoke(system, null);
        EventSystem.current = system;
        var cameraGo = new GameObject("pass-through-camera", typeof(Camera));
        var camera = cameraGo.GetComponent<Camera>(); camera.enabled = false;
        camera.transform.position = Vector3.back * 10; camera.orthographic = true; camera.orthographicSize = 5;
        camera.cullingMask = 1 << 5;
        var target = new RenderTexture(256, 256, 16); target.Create(); camera.targetTexture = target;
        var canvasGo = new GameObject("pass-through-canvas", typeof(RectTransform), typeof(Canvas), typeof(GraphicRaycaster));
        canvasGo.layer = 5;
        var canvas = canvasGo.GetComponent<Canvas>(); canvas.renderMode = RenderMode.ScreenSpaceCamera;
        canvas.worldCamera = camera; canvas.planeDistance = 1;
        var raycaster = canvasGo.GetComponent<GraphicRaycaster>();
        bool enableRaycaster = !RaycasterManager.GetRaycasters().Contains(raycaster);
        if (enableRaycaster) typeof(GraphicRaycaster).GetMethod("OnEnable", Hidden).Invoke(raycaster, null);
        Btn NativeButton(string name, Transform parent)
        {
            var button = Button(parent.gameObject, name); button.gameObject.layer = 5;
            button.gameObject.AddComponent<Image>();
            var rect = (RectTransform)button.transform;
            rect.anchorMin = Vector2.zero; rect.anchorMax = Vector2.one;
            rect.offsetMin = rect.offsetMax = Vector2.zero;
            return button;
        }
        var blocker = NativeButton("ordinary-ui-behind", canvasGo.transform);
        blocker.gameObject.SetActive(false);
        var passButton = NativeButton("background-close-button", canvasGo.transform);
        var pass = passButton.gameObject.AddComponent<TouchPass>();
        var graphicGo = new GameObject("background-graphic-child", typeof(RectTransform), typeof(Image));
        graphicGo.layer = 5; graphicGo.transform.SetParent(passButton.transform, false);
        var graphicRect = (RectTransform)graphicGo.transform;
        graphicRect.anchorMin = Vector2.zero; graphicRect.anchorMax = Vector2.one;
        graphicRect.offsetMin = graphicRect.offsetMax = Vector2.zero;
        var nestedButton = NativeButton("ordinary-button-inside-pass-through", passButton.transform);
        nestedButton.gameObject.SetActive(false);
        GameObject Hit()
        {
            Canvas.ForceUpdateCanvases();
            camera.Render();
            return (GameObject)typeof(InputManager).GetMethod("UICheck", Hidden).Invoke(input,
                new object[] { new Vector2(target.width * .5f, target.height * .5f) });
        }
        try
        {
            pass.enabled = false;
            Check(Hit() != null, "native GraphicRaycaster produces a real UI hit before pass-through filtering: system="
                + EventSystem.current + " raycasters=" + RaycasterManager.GetRaycasters().Count
                + " graphics=" + GraphicRegistry.GetGraphicsForCanvas(canvas).Count + " depth=" + graphicGo.GetComponent<Image>().depth);
            pass.enabled = true;
            Check(Hit() == null, "TouchPass on the receiving parent ignores its Graphic child for scene-input interception");
            int closes = 0; passButton.onClick.AddListener(() => closes++);
            ExecuteEvents.Execute(passButton.gameObject, new PointerEventData(EventSystem.current)
                { button = PointerEventData.InputButton.Left }, ExecuteEvents.pointerClickHandler);
            Check(closes == 1, "filtering InputManager interception retains the background Button's ordinary UGUI click");
            blocker.gameObject.SetActive(true);
            Check(Hit() == blocker.gameObject, "a blocking UI behind TouchPass still intercepts scene input");
            blocker.gameObject.SetActive(false); nestedButton.gameObject.SetActive(true);
            Check(Hit() == nestedButton.gameObject, "an ordinary nested Button handles its own input despite a TouchPass ancestor");
            nestedButton.gameObject.SetActive(false); pass.enabled = false;
            Check(ExecuteEvents.GetEventHandler<IPointerDownHandler>(Hit()) == passButton.gameObject,
                "a disabled TouchPass no longer opts its background out of scene interception");
        }
        finally
        {
            if (enableRaycaster) typeof(GraphicRaycaster).GetMethod("OnDisable", Hidden).Invoke(raycaster, null);
            if (enableSystem) typeof(EventSystem).GetMethod("OnDisable", Hidden).Invoke(system, null);
            Object.DestroyImmediate(canvasGo); Object.DestroyImmediate(systemGo); Object.DestroyImmediate(cameraGo);
            target.Release(); Object.DestroyImmediate(target);
        }
    }
    private static GameObject Child(GameObject parent, string name) { var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent.transform, false); return go; }
    private static T Component<T>(GameObject parent, string name) where T : Component => Child(parent, name).AddComponent<T>();
    private static Btn Button(GameObject parent, string name) => Component<Btn>(parent, name);
    private static Txt Text(GameObject parent, string name) { var txt = Component<Txt>(parent, name); txt.enabled = false; txt.languageTranslatable = false; return txt; }
    private static UiHolder Holder(string name, UiHolder parent = null)
    { var go = new GameObject(name, typeof(RectTransform)); go.SetActive(false); if (parent != null) go.transform.SetParent(parent.transform, false);
        var holder = go.AddComponent<ActiveHolder>(); holder.enabled = false; holder.parent = parent; return holder; }
    private static T Capture<T>(UiManager ui) where T : UiCtrl, new()
    { var holder = Holder(typeof(T).Name); var ctrl = new T { uiHolder = holder }; holder.ctrl = ctrl;
        ui.uiCtrlName2Uis[typeof(T).Name] = new List<UiHolder> { holder }; return ctrl; }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    { var go = new GameObject(typeof(T).Name); go.SetActive(false); var value = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value); return value; }
    private static void Register(Type form)
    { if (!registered.Add(form)) return; var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType; if (parent != null) Register(parent);
        form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null); }
    private static void Check(bool value, string message) { if (!value) throw new InvalidOperationException(message); checks++; }
}
