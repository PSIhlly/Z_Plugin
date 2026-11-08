using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Ui.EventChoose;
using Ui.ModStoryEventTriggerWindow;
using Ui.Notify;
using UnityEditor;
using UnityEngine;
using Z_DesignStyle;
using Z_Language;
using Z_Text;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;

// Real Model controllers and UiContainer pooling in an isolated Unity project.
public static class SceneEventChooseListRegression
{
    private static int checks;
    private static readonly string[] Expected = {
        "onCharacterTouchEvent", "onCharacterLeaveEvent", "onObjectTouchEvent",
        "onObjectLeaveEvent", "onTileTouchEvent", "onShowEvent", "onPerSecondEvent",
        "onBoundaryTouchEvent", "onInteractEvent", "onClickMinimapEvent", "onLeaveSceneEvent", "onDestroyEvent"
    };

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name.StartsWith("MapRuntimeRegression-"),
                "fixture is isolated from the production scene and saves");
            foreach (var form in new[] { typeof(CommonTextForm), typeof(ModTextForm), typeof(EventProgramDataForm) })
                form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            CommonTextForm.Init(); ModTextForm.Init();
            var ui = Manager<UiManager>();
            var mod = Manager<ModManager>();
            mod.assetCtrl = new ModAssetCtrl(mod);
            var notify = Capture<UiNotifyCtrl>(ui);
            var window = Capture<UiModStoryEventTriggerWindowCtrl>(ui);

            var root = new GameObject("event-list-owner", typeof(RectTransform));
            root.SetActive(false);
            var ownerHolder = root.AddComponent<UiHolder>();
            var owner = new UiCtrl();
            owner.BindHolderRecursively(ownerHolder);
            var template = new GameObject("go_eventChoose", typeof(RectTransform));
            template.transform.SetParent(root.transform, false);
            var holder = template.AddComponent<UiHolder>();
            holder.uiType = UiType.Model;
            holder.parent = ownerHolder;
            holder.elementTrsLst = new List<Transform> {
                Text(template, "name").transform, Button(template, "event").transform,
                Button(template, "type").transform, Text(template, "event-name").transform,
                Text(template, "trigger-type").transform
            };
            new UiEventChooseCtrl().BindHolderRecursively(holder);
            ownerHolder.subUiHolderLst.Add(holder);
            var list = new UiSceneEventChooseList(owner, template);
            root.SetActive(true);
            var first = new Dictionary<string, EventTriggerForm.Data>();
            var second = new Dictionary<string, EventTriggerForm.Data>();
            Render(list, ownerHolder, first, true);
            var originalRows = Rows(ownerHolder);
            Check(originalRows.Select(row => row.model.prm.key).SequenceEqual(Expected), "twelve triggers follow the authored order");
            Check(!template.activeSelf && first.Count == 0, "rendering hides the template without creating saved triggers");
            var embeddedRow = originalRows[0];
            embeddedRow.Set(null);
            embeddedRow.OnShow();
            Check(embeddedRow.gameObject.activeSelf, "an embedded Model shown before Set remains usable");
            embeddedRow.Set(new UiEventChooseParam { dic = first, key = Expected[0] });
            var originalIds = originalRows.Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).ToArray();
            for (int i = 0; i < 3; i++)
            {
                Render(list, ownerHolder, first, true);
                Check(Rows(ownerHolder).Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).SequenceEqual(originalIds),
                    "refresh reuses the existing Model instances");
            }
            var touch = Rows(ownerHolder).Single(row => row.model.prm.key == Expected[0]);
            int popupBefore = NotifyManager.popupIdCnt;
            touch.view.btn_onEventTrigger.onClick.Invoke();
            Check(NotifyManager.popupIdCnt == popupBefore + 1, "pooled button has only one click listener");
            var pending = notify.param.multipleChooseInfo;
            var oldTrigger = first[Expected[0]];
            var initialType = oldTrigger.type;
            var changedType = Enum.GetValues(typeof(TriggerType)).Cast<TriggerType>().First(type => type != initialType);
            Render(list, ownerHolder, second, true);
            pending.func(new EntryItem { id = (int)changedType });
            Check(oldTrigger.type == initialType && second.Count == 0,
                "a pending choice cannot write to a recycled row or its replacement dictionary");
            touch = Rows(ownerHolder).Single(row => row.model.prm.key == Expected[0]);
            touch.view.btn_onEventTrigger.onClick.Invoke();
            notify.param.multipleChooseInfo.func(new EntryItem { id = (int)changedType });
            Check(second[Expected[0]].type == changedType && oldTrigger.type == initialType,
                "fresh type selection edits only the currently bound Product");
            touch.view.btn_onEvent.onClick.Invoke();
            Check(ReferenceEquals(window.param.dic, second) && ReferenceEquals(window.param.trigger, second[Expected[0]]),
                "event selection opens the existing trigger editor with the current dictionary");
            var mini = GameEventController.CreateTrigger("onClickMinimapEvent");
            second[mini.name] = mini;
            Render(list, ownerHolder, second, false);
            Check(Rows(ownerHolder).Select(row => row.model.prm.key).SequenceEqual(Expected.Where(key => key != mini.name)),
                "disabling the minimap hides only its click row");
            Check(ReferenceEquals(second[mini.name], mini), "hiding a row does not remove its saved binding");
            Render(list, ownerHolder, second, true);
            Check(Rows(ownerHolder).Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).SequenceEqual(originalIds),
                "toggling the minimap reuses the same twelve instances");
            var lastRows = Rows(ownerHolder);
            list.Clear(); list.Clear();
            Check(Rows(ownerHolder).Length == 0 && ownerHolder.subUiHolderLst.Count == 1,
                "clear detaches all runtime rows and is idempotent");
            Check(lastRows.All(row => row.param == null && row.model.prm == null && !row.gameObject.activeSelf),
                "pooled rows release the old dictionary on hide");
            Render(list, ownerHolder, first, true);
            Check(Rows(ownerHolder).All(row => ReferenceEquals(row.model.prm.dic, first)),
                "reopening rebinds every row to the incoming dictionary");
            Render(list, ownerHolder, null, true);
            Check(Rows(ownerHolder).Length == 0, "null data returns every row to the pool");
            Debug.Log("SCENE_EVENT_CHOOSE_LIST_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static void Render(UiSceneEventChooseList list, UiHolder owner,
        Dictionary<string, EventTriggerForm.Data> events, bool minimap)
    {
        list.Render(events, minimap);
        // Non-ExecuteAlways MonoBehaviours do not receive Play activation in EditMode.
        // Drive the same controller lifecycle explicitly while retaining real cloning/pooling.
        foreach (var row in Rows(owner))
        {
            row.uiHolder.Create();
            row.OnShow();
            row.showed = true;
        }
    }

    private static UiEventChooseCtrl[] Rows(UiHolder owner) => owner.subUiHolderLst
        .Where(holder => holder.gameObject.activeSelf && holder.ctrl is UiEventChooseCtrl)
        .OrderBy(holder => holder.transform.GetSiblingIndex()).Select(holder => (UiEventChooseCtrl)holder.ctrl).ToArray();

    private static Txt Text(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        var txt = go.AddComponent<Txt>();
        txt.enabled = false;
        txt.languageTranslatable = false;
        return txt;
    }
    private static Btn Button(GameObject parent, string name)
    {
        var go = new GameObject(name, typeof(RectTransform));
        go.transform.SetParent(parent.transform, false);
        return go.AddComponent<Btn>();
    }
    private static T Capture<T>(UiManager ui) where T : UiCtrl, new()
    {
        var go = new GameObject(typeof(T).Name, typeof(RectTransform));
        go.SetActive(false);
        var holder = go.AddComponent<UiHolder>();
        holder.enabled = false;
        var ctrl = new T { uiHolder = holder };
        holder.ctrl = ctrl;
        ui.uiCtrlName2Uis[typeof(T).Name] = new List<UiHolder> { holder };
        return ctrl;
    }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject(typeof(T).Name);
        go.SetActive(false);
        var value = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, value);
        return value;
    }
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }
}
