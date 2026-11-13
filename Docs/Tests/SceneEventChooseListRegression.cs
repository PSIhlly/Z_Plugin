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
using UnityEngine.UI;
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
            PendingActivationLayout();
            GridBehindPlainWrapper();
            ImmediateGridLayout(template);
            Debug.Log("SCENE_EVENT_CHOOSE_LIST_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static void PendingActivationLayout()
    {
        var root = new GameObject("activation-layout", typeof(RectTransform));
        root.SetActive(false);
        var canvas = root.AddComponent<Canvas>();
        canvas.renderMode = RenderMode.WorldSpace;
        ((RectTransform)root.transform).sizeDelta = new Vector2(1359.753f, 750.361f);
        var activation = root.AddComponent<SceneEventChooseLayoutActivationProbe>();
        var contentObject = new GameObject("content", typeof(RectTransform), typeof(VerticalLayoutGroup), typeof(ContentSizeFitter));
        var content = (RectTransform)contentObject.transform;
        content.SetParent(root.transform, false);
        content.sizeDelta = new Vector2(1359.753f, 1725.141f);
        var vertical = contentObject.GetComponent<VerticalLayoutGroup>();
        vertical.childControlWidth = false; vertical.childForceExpandWidth = false;
        vertical.childControlHeight = false; vertical.childForceExpandHeight = false;
        var fitter = contentObject.GetComponent<ContentSizeFitter>();
        fitter.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        foreach (float height in new[] { 200f, 515.141f })
        {
            var header = new GameObject("header", typeof(RectTransform));
            header.transform.SetParent(content, false);
            ((RectTransform)header.transform).sizeDelta = new Vector2(100, height);
        }
        var gridObject = new GameObject("mapObjectEvents", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        var gridRect = (RectTransform)gridObject.transform;
        gridRect.SetParent(content, false);
        gridRect.sizeDelta = new Vector2(1309.136f, 1010);
        var grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(340, 140); grid.spacing = new Vector2(100, 100); grid.padding.bottom = 150;
        gridObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        for (int i = 0; i < 13; i++)
        {
            var row = new GameObject("event-row-" + i, typeof(RectTransform));
            row.transform.SetParent(gridRect, false);
            ((RectTransform)row.transform).sizeDelta = new Vector2(340, 140);
        }
        bool pending = false;
        activation.onEnable = () =>
        {
            pending = grid.enabled && !grid.isActiveAndEnabled && vertical.enabled && !vertical.isActiveAndEnabled;
            UiManager.Rebuild(root, true);
        };
        try
        {
            root.SetActive(true);
            if (activation.error != null) throw activation.error;
            Check(pending, "the early OnEnable callback runs before enabled layout components join the active manager");
            Canvas.ForceUpdateCanvases();
            Check(Mathf.Abs(gridRect.rect.height - 1250) < .1f,
                "the first Canvas submission positions all thirteen newly activated event cells");
            Check(Mathf.Abs(content.rect.height - 1965.141f) < .1f,
                "the first Canvas submission includes the new Grid height in its fixed-height parent: " + content.rect.height);
            Check(Mathf.Abs(((RectTransform)gridRect.GetChild(0)).anchoredPosition.y
                - ((RectTransform)gridRect.GetChild(3)).anchoredPosition.y - 240) < .1f,
                "newly activated rows are no longer stacked at the template position");
            Canvas.ForceUpdateCanvases();
            Check(Mathf.Abs(content.rect.height - 1965.141f) < .1f,
                "later Canvas submissions keep the activation layout stable");
            root.SetActive(false);
            root.SetActive(true);
            root.SetActive(false);
            Canvas.ForceUpdateCanvases();
            Check(!root.activeSelf, "a pending activation rebuild cannot reopen a hidden page");
            grid.enabled = false; vertical.enabled = false; fitter.enabled = false;
            gridObject.GetComponent<ContentSizeFitter>().enabled = false;
            root.SetActive(true);
            Canvas.ForceUpdateCanvases();
            Check(!grid.enabled && !vertical.enabled && !fitter.enabled,
                "explicitly disabled layout components stay disabled");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static void GridBehindPlainWrapper()
    {
        var root = new GameObject("grid-layout-root", typeof(RectTransform), typeof(VerticalLayoutGroup));
        var rootRect = (RectTransform)root.transform;
        rootRect.sizeDelta = new Vector2(1230, 400);
        var vertical = root.GetComponent<VerticalLayoutGroup>();
        vertical.childControlWidth = true; vertical.childForceExpandWidth = true;
        vertical.childControlHeight = false; vertical.childForceExpandHeight = false;
        var wrapper = new GameObject("plain-wrapper", typeof(RectTransform));
        wrapper.transform.SetParent(root.transform, false);
        ((RectTransform)wrapper.transform).sizeDelta = new Vector2(200, 100);
        var gridObject = new GameObject("wrapped-grid", typeof(RectTransform), typeof(GridLayoutGroup), typeof(ContentSizeFitter));
        var gridRect = (RectTransform)gridObject.transform;
        gridRect.SetParent(wrapper.transform, false);
        gridRect.anchorMin = new Vector2(0, 1); gridRect.anchorMax = Vector2.one; gridRect.sizeDelta = Vector2.zero;
        var grid = gridObject.GetComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(340, 140); grid.spacing = new Vector2(100, 100); grid.padding.bottom = 150;
        gridObject.GetComponent<ContentSizeFitter>().verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        for (int i = 0; i < 7; i++)
            new GameObject("cell-" + i, typeof(RectTransform)).transform.SetParent(gridRect, false);
        try
        {
            UiManager.Rebuild(root, true);
            Check(Mathf.Abs(gridRect.rect.width - 1230) < .1f && Mathf.Abs(gridRect.rect.height - 770) < .1f,
                "a Grid behind a plain wrapper uses the parent's final width immediately: " + gridRect.rect);
            rootRect.sizeDelta = new Vector2(700, 400);
            UiManager.Rebuild(root, true);
            Check(Mathf.Abs(gridRect.rect.width - 700) < .1f && Mathf.Abs(gridRect.rect.height - 1730) < .1f,
                "the wrapped Grid synchronously reflows after narrowing the parent");
            rootRect.sizeDelta = new Vector2(1230, 400);
            UiManager.Rebuild(root, true);
            Check(Mathf.Abs(gridRect.rect.height - 770) < .1f,
                "the wrapped Grid synchronously reflows after widening the parent");
        }
        finally { UnityEngine.Object.DestroyImmediate(root); }
    }

    private static void ImmediateGridLayout(GameObject rowTemplate)
    {
        var canvasObject = new GameObject("event-layout-canvas", typeof(RectTransform), typeof(Canvas));
        canvasObject.GetComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        ((RectTransform)canvasObject.transform).sizeDelta = new Vector2(1600, 900);
        var page = new GameObject("event-layout-page", typeof(RectTransform));
        page.transform.SetParent(canvasObject.transform, false);
        var pageRect = (RectTransform)page.transform;
        pageRect.sizeDelta = new Vector2(700, 400);
        var ownerHolder = page.AddComponent<UiHolder>();
        ownerHolder.enabled = false;
        var owner = new UiCtrl { inited = true };
        owner.BindHolderRecursively(ownerHolder);

        RectTransform Rect(string name, Transform parent)
        {
            var go = new GameObject(name, typeof(RectTransform));
            go.transform.SetParent(parent, false);
            return (RectTransform)go.transform;
        }
        var scrollRoot = Rect("scroll", page.transform);
        scrollRoot.anchorMin = Vector2.zero; scrollRoot.anchorMax = Vector2.one; scrollRoot.sizeDelta = Vector2.zero;
        var viewport = Rect("viewport", scrollRoot);
        viewport.anchorMin = Vector2.zero; viewport.anchorMax = Vector2.one; viewport.sizeDelta = Vector2.zero;
        var content = Rect("content", viewport);
        content.anchorMin = new Vector2(0, 1); content.anchorMax = Vector2.one;
        content.pivot = new Vector2(.5f, 1); content.sizeDelta = Vector2.zero;
        var vertical = content.gameObject.AddComponent<VerticalLayoutGroup>();
        vertical.childControlWidth = true; vertical.childForceExpandWidth = true;
        vertical.childControlHeight = true; vertical.childForceExpandHeight = false;
        var contentFit = content.gameObject.AddComponent<ContentSizeFitter>();
        contentFit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var scrollbarRect = Rect("scrollbar", scrollRoot);
        scrollbarRect.anchorMin = new Vector2(1, 0); scrollbarRect.anchorMax = Vector2.one;
        scrollbarRect.pivot = Vector2.one; scrollbarRect.sizeDelta = new Vector2(20, 0);
        var scrollbar = scrollbarRect.gameObject.AddComponent<Scrollbar>();
        scrollbar.direction = Scrollbar.Direction.BottomToTop;
        var scroll = scrollRoot.gameObject.AddComponent<ScrollRect>();
        scroll.viewport = viewport; scroll.content = content; scroll.horizontal = false;
        scroll.verticalScrollbar = scrollbar;
        scroll.verticalScrollbarVisibility = ScrollRect.ScrollbarVisibility.AutoHideAndExpandViewport;

        var gridRect = Rect("mapObjectEvents", content);
        gridRect.sizeDelta = new Vector2(1291.8f, 0);
        var grid = gridRect.gameObject.AddComponent<GridLayoutGroup>();
        grid.cellSize = new Vector2(340, 140); grid.spacing = new Vector2(100, 100);
        grid.padding.bottom = 150;
        var fit = gridRect.gameObject.AddComponent<ContentSizeFitter>();
        fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        var template = UnityEngine.Object.Instantiate(rowTemplate, gridRect);
        var templateHolder = template.GetComponent<UiHolder>();
        templateHolder.ctrl = null; templateHolder.parent = ownerHolder;
        new UiEventChooseCtrl().BindHolderRecursively(templateHolder);
        ownerHolder.subUiHolderLst.Add(templateHolder);
        var list = new UiSceneEventChooseList(owner, template);
        var events = new Dictionary<string, EventTriggerForm.Data>();
        try
        {
            void Flush() { UiManager.Rebuild(page, true); }
            void Height(float expected, string reason)
            {
                Check(Mathf.Abs(gridRect.rect.height - expected) < .1f, reason + " grid=" + gridRect.rect.height);
                Check(Mathf.Abs(content.rect.height - expected) < .1f, reason + " content=" + content.rect.height);
            }
            Render(list, ownerHolder, events, true);
            // The caller supplies its current width and requires the layout
            // to be correct when Rebuild returns, without deferred callbacks.
            pageRect.sizeDelta = new Vector2(1230, 400);
            Flush();
            Check(Mathf.Abs(viewport.rect.width - 1210) < .1f,
                "the expanded ScrollRect settles its final width during the synchronous rebuild");
            Height(1490, "twelve event rows form six rows at the final two-column width");
            var originalIds = Rows(ownerHolder).Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).ToArray();
            var originalPositions = Rows(ownerHolder).Select(row => row.rect.anchoredPosition).ToArray();
            for (int i = 2; i < originalPositions.Length; i++)
                Check(Mathf.Abs(originalPositions[i].y - originalPositions[i - 2].y + 240) < .1f,
                    "event rows use the final grid spacing without overlap");
            Canvas.ForceUpdateCanvases();
            Height(1490, "a later ordinary Canvas pass does not change the settled event height");

            pageRect.sizeDelta = new Vector2(700, 400);
            Z_EventHelper.Invoke(new EventModifyEvent());
            Flush();
            Height(2930, "a narrower page reflows all twelve rows into one column");
            Check(Rows(ownerHolder).Select(row => row.gameObject.GetInstanceID()).OrderBy(id => id).SequenceEqual(originalIds),
                "layout changes reuse the same event-row pool");
            Render(list, ownerHolder, null, true);
            Flush();
            // UGUI deliberately reserves scrollbar space outside Play mode;
            // verify the content's real shrink without asserting Play visibility.
            Check(Rows(ownerHolder).Length == 0, "clearing the event list removes all pooled rows");
            Height(50, "an empty grid retains only its authored padding/spacing");

            pageRect.sizeDelta = new Vector2(1230, 400);
            Render(list, ownerHolder, events, true);
            Render(list, ownerHolder, events, false);
            Flush();
            Check(Rows(ownerHolder).Length == 11, "consecutive renders use the final minimap filter");
            Height(1490, "the last same-frame binding determines the final layout");
            Render(list, ownerHolder, events, true);
            list.Clear(); page.SetActive(false); Flush();
            Check(Rows(ownerHolder).Length == 0, "rebuilding a hidden page does not reopen cleared rows");
            page.SetActive(true); Render(list, ownerHolder, events, true); Flush();
            Height(1490, "reopening immediately rebuilds the current event rows");
        }
        finally
        {
            list.Clear();
            UnityEngine.Object.DestroyImmediate(canvasObject);
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

[ExecuteAlways, DefaultExecutionOrder(-100)]
public sealed class SceneEventChooseLayoutActivationProbe : MonoBehaviour
{
    public Action onEnable;
    public Exception error;
    private void OnEnable()
    {
        try { onEnable?.Invoke(); }
        catch (Exception exception) { error = exception; }
    }
}
