using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Runtime.Serialization;
using Form;
using TMPro;
using Ui.ModStoryEventEditWindow;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Text.Form;
using Z_Time;
using Z_Trick.BaseFunc;
using Z_Ui.Base;

// Real UiHolder lifecycle and UGUI layout, only in a disposable Play project.
public static class UiTreeContainerRegression
{
    private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags HiddenStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static TMP_FontAsset font;
    private static int checks;

    public static void Run()
    {
        if (!new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name.StartsWith("MapRuntimeRegression-"))
            throw new InvalidOperationException("isolated project guard");
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload | EnterPlayModeOptions.DisableSceneReload;
        EditorApplication.playModeStateChanged += Entered;
        EditorApplication.EnterPlaymode();
    }

    private static void Entered(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= Entered;
        try
        {
            foreach (var form in new[] { typeof(CmdDataForm), typeof(EventProgramDataForm), typeof(CommonTextForm), typeof(Z_DataSystem.Form.LabForm) }) Register(form);
            var settings = ScriptableObject.CreateInstance<TMP_Settings>();
            typeof(TMP_Settings).GetField("s_Instance", HiddenStatic).SetValue(null, settings);
            font = TMP_FontAsset.CreateFontAsset(AssetDatabase.LoadAssetAtPath<Font>("Assets/PlayAssetTextShaders/LiberationSans.ttf"));
            typeof(TMP_Settings).GetField("m_defaultFontAsset", Hidden).SetValue(settings, font);
            typeof(TMP_Settings).GetField("m_fallbackFontAssets", Hidden).SetValue(settings, new List<TMP_FontAsset>());
            var timerObject = new GameObject("tree-test-timer"); timerObject.SetActive(false);
            var timer = timerObject.AddComponent<TimeManager>();
            typeof(Z_MonoSingleton<TimeManager>).GetField("_instance", HiddenStatic).SetValue(null, timer);
            timerObject.SetActive(true);
            timer.StartCoroutine(Guard(Checks()));
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static IEnumerator Checks()
    {
        PoolChecks();
        EditorNodeChecks();
        CodeSwitchChecks();
        LayoutChecks();
        yield return null;
        Canvas.ForceUpdateCanvases();
    }

    private static void PoolChecks()
    {
        var root = Child("pool-owner"); root.SetActive(false);
        var owner = new UiCtrl(); owner.BindHolderRecursively(root.AddComponent<UiHolder>());
        var template = Child("tree-template", root.transform); template.SetActive(false);
        template.AddComponent<UiHolder>();
        var con = new UiTreeContainer<TreeProbeCtrl>(owner, template, p => ((TreeProbeParam)p).destination);
        root.SetActive(true);
        var rootParam = new TreeProbeParam { con = con, destination = root.transform, levels = 5 };
        con.Add(rootParam); con.Refresh();
        Check(con.ActiveCount == 63 && con.CreatedCount == 63, "dynamic sub-in-sub parameters render the entire binary tree");
        var first = con.paramLst.Select(p => con.Get(p)).ToArray();
        Check(first.All(h => ((TreeProbeCtrl)h.ctrl).shows == 1), "each node binds exactly once before activation");
        int parameters = con.paramLst.Count;
        root.SetActive(false); root.SetActive(true);
        Check(con.paramLst.Count == parameters && first.All(h => ((TreeProbeCtrl)h.ctrl).renders == 1),
            "ordinary visibility changes do not expand the tree again");
        con.DelReal(con.Get(con.paramLst[1]).gameObject);
        Check(con.ActiveCount == 32 && con.PooledCount == 31 && con.paramLst.Count == 32, "subtree return captures and removes every descendant");
        Check(first.Where(h => !h.gameObject.activeSelf).All(h => h.parent == null && ((TreeProbeCtrl)h.ctrl).param == null),
            "returned nodes forget logical parents and parameters");
        con.Clear(); con.Clear();
        Check(con.ActiveCount == 0 && con.PooledCount == 63 && owner.uiHolder.subUiHolderLst.Count == 0, "clear is idempotent and unlinks all logical children");
        Check(first.All(h => h.transform.parent == first[0].transform.parent && h.transform.childCount == 0),
            "all pooled dynamic nodes are flattened instead of retaining pooled descendants");
        for (int pass = 0; pass < 5; pass++)
        {
            con.Add(new TreeProbeParam { con = con, destination = root.transform, levels = pass % 3 }); con.Refresh();
            Check(con.CreatedCount == 63, "smaller and differently shaped trees reuse warmed nodes");
            Check(con.paramLst.All(p => ReferenceEquals(((TreeProbeCtrl)con.Get(p).ctrl).param, p)), "every reused node has its new parameter");
            con.Clear();
        }
        root.SetActive(false);
        con.Add(new TreeProbeParam { con = con, destination = root.transform, levels = 0 });
        root.SetActive(true); con.Refresh();
        UnityEngine.Object.DestroyImmediate(con.Get(con.paramLst[0]).gameObject);
        con.Clear();
        con.Add(new TreeProbeParam { con = con, destination = root.transform, levels = 0 }); con.Refresh();
        Check(con.ActiveCount == 1 && con.Get(con.paramLst[0]), "externally destroyed nodes are skipped on the next rent");
        con.Clear();
    }

    private static void EditorNodeChecks()
    {
        var root = Child("event-owner"); root.SetActive(false);
        var holder = root.AddComponent<UiHolder>();
        var editor = new TreeFixtureEditor { uiHolder = holder, model = new UiModStoryEventEditWindowModel() };
        holder.ctrl = editor;
        var rowsRoot = Child("rows", root.transform);
        var template = ItemTemplate(rowsRoot.transform);
        var rows = new UiTreeContainer<UiItemCtrl>(editor, template);
        var lineTemplate = Child("line-template", rowsRoot.transform); lineTemplate.SetActive(false);
        var lineHolder = lineTemplate.AddComponent<UiHolder>();
        var lineState = States(lineTemplate.transform, 3);
        var lineButton = Child("line-button", lineTemplate.transform).AddComponent<Btn>();
        lineHolder.elementTrsLst = new List<Transform> { lineTemplate.transform, lineState.transform, lineButton.transform };
        var lines = new UiTreeContainer<UiLineCtrl>(editor, lineTemplate);
        typeof(UiModStoryEventEditWindowCtrl).GetField("itemCon", Hidden).SetValue(editor, rows);
        typeof(UiModStoryEventEditWindowCtrl).GetField("lineCon", Hidden).SetValue(editor, lines);
        editor.view = (UiModStoryEventEditWindowView)FormatterServices.GetUninitializedObject(typeof(UiModStoryEventEditWindowView));
        editor.view.sta_item = States(root.transform, 2); editor.view.sta_unit = States(root.transform, 2);
        var metadata = CmdDataForm.defaultData;
        metadata.uid = 90120; metadata.name = "TreeDialog";
        metadata.desc = "a{0}b{1}c{2}d{3}e{4}f{5}g{6}h{7}i{8}j{9}";
        CmdDataForm.AddData(metadata);
        root.SetActive(true);
        for (int i = 0; i < 58; i++) rows.Add(new UiItemParam { con = rows, node = Dialog(i) });
        rows.Refresh();
        var items = rows.paramLst.Select(p => (UiItemCtrl)rows.Get(p).ctrl).ToArray();
        var unitField = typeof(UiItemCtrl).GetField("unitCon", Hidden);
        var unitPools = items.Select(item => (UiTreeContainer<UiUnitCtrl>)unitField.GetValue(item)).ToArray();
        Check(unitPools.All(pool => pool.ActiveCount == 21), "58 ten-argument command rows render 1218 actual Unit controllers");
        var oldUnits = unitPools.SelectMany(pool => pool.paramLst.Select(p => (UiUnitCtrl)pool.Get(p).ctrl)).ToArray();
        var before = oldUnits.Select(u => u.gameObject.GetInstanceID()).OrderBy(id => id).ToArray();
        rowsRoot.SetActive(false); rowsRoot.SetActive(true);
        Check(unitPools.All(pool => pool.ActiveCount == 21 && pool.paramLst.Count == 21), "showing the panel again adds no duplicate Unit parameters");
        rows.Clear();
        Check(oldUnits.All(u => u.param == null && u.model.node == null && u.model.con == null && u.model.parent == null),
            "returning Item rows releases the whole AST, child destinations and Unit parameters");
        for (int i = 0; i < 58; i++) rows.Add(new UiItemParam { con = rows, node = Dialog(100 + i) });
        rows.Refresh();
        var nextItems = rows.paramLst.Select(p => (UiItemCtrl)rows.Get(p).ctrl).ToArray();
        var nextUnits = nextItems.SelectMany(item => {
            var pool = (UiTreeContainer<UiUnitCtrl>)unitField.GetValue(item);
            return pool.paramLst.Select(p => (UiUnitCtrl)pool.Get(p).ctrl);
        }).ToArray();
        Check(rows.CreatedCount == 58 && unitPools.All(pool => pool.CreatedCount == 21), "a warm event refresh creates no additional rows or Units");
        Check(before.SequenceEqual(nextUnits.Select(u => u.gameObject.GetInstanceID()).OrderBy(id => id)), "all 1218 Unit GameObjects are reused");
        Check(nextUnits.Where(u => u.model.node?.desc.type == CodeType.Str).All(u => u.view.txt_.text.StartsWith("\"new-")), "pooled text displays only the newly bound strings");
        for (int pass = 0; pass < 3; pass++)
        {
            lines.Clear();
            for (int i = 0; i < nextItems.Length; i++) lines.Add(new UiLineParam { item = nextItems[i], node = nextItems[i].model.prm.node, index = i });
            lines.Refresh();
            var line = (UiLineCtrl)lines.Get(lines.paramLst[pass]).ctrl;
            line.view.btn_.onClick.Invoke();
            Check(ReferenceEquals(editor.model.selItem, nextItems[pass].model.prm.node) && line.view.sta_.state == 2,
                "reused Line click selects the current Item and refreshes its selection state");
        }
        Check(lines.CreatedCount == 58, "Line backgrounds also reuse their warmed holders");
        var row = nextItems[0];
        var rootNode = row.model.prm.node;
        row.RefreshUnits(); row.RefreshUnits();
        Check(((UiTreeContainer<UiUnitCtrl>)unitField.GetValue(row)).ActiveCount == 21, "replacing one Item's Units leaves exactly one rendered syntax tree");
        rootNode.subNodes[0] = new SyntaxNode(new Desc("+", CodeType.Operator), new List<SyntaxNode> {
            new SyntaxNode(new Desc("2", CodeType.Num)), new SyntaxNode(new Desc("1", CodeType.Num)) });
        row.RefreshUnits();
        var nestedPool = (UiTreeContainer<UiUnitCtrl>)unitField.GetValue(row);
        Check(nestedPool.ActiveCount > 21 && nestedPool.paramLst.Any(p => {
            var u = (UiUnitCtrl)nestedPool.Get(p).ctrl;
            return u.model.node?.desc.code == "1" && u.rect.parent.parent.GetComponent<UiHolder>()?.ctrl is UiUnitCtrl;
        }), "actual nested operator Units are placed inside their owning command's descendants");
        UiTreeLayout.Rebuild((RectTransform)rowsRoot.transform);
        Check(nextUnits.Where(u => u.model.node?.desc.type == CodeType.Str).All(u => u.rect.rect.width > 0), "intrinsic layout sizes the newly bound TMP strings");
        lines.Clear(); rows.Clear();
    }

    private static SyntaxNode Dialog(int seed)
    {
        var args = new List<SyntaxNode>();
        for (int i = 0; i < 10; i++) args.Add(new SyntaxNode(new Desc((seed >= 100 ? "new-" : "old-") + seed + ":" + i, CodeType.Str)));
        return new SyntaxNode(new Desc("TreeDialog", CodeType.FuncName), args);
    }

    private static void CodeSwitchChecks()
    {
        var root = Child("actual-event-window"); root.SetActive(false);
        var holder = root.AddComponent<UiHolder>();
        var view = (UiModStoryEventEditWindowView)FormatterServices.GetUninitializedObject(typeof(UiModStoryEventEditWindowView));
        var panelState = States(root.transform, 2);
        var panels = (GameObject[])typeof(State).GetField("stateGo", Hidden).GetValue(panelState);
        view.sta_switchModPanel = panelState; view.sta_switchMod = States(root.transform, 2);
        var content = Child("item-content", panels[0].transform); Intrinsic(content, true);
        view.go_item = ItemTemplate(content.transform);
        view.go_line = Child("line", content.transform); view.go_line.SetActive(false);
        var lineHolder = view.go_line.AddComponent<UiHolder>();
        var lineState = States(view.go_line.transform, 3);
        var button = Child("button", view.go_line.transform).AddComponent<Btn>();
        lineHolder.elementTrsLst = new List<Transform> { view.go_line.transform, lineState.transform, button.transform };
        view.scr_items = panels[0].AddComponent<ScrView>();
        view.scr_items.content = (RectTransform)content.transform;
        view.scr_items.viewport = (RectTransform)panels[0].transform;
        view.ipt_code = Input(panels[1].transform); view.ipt_name = Input(root.transform);
        view.txt_title = Text(root.transform, "title"); view.txt_lab = Text(root.transform, "lab");
        view.sta_item = States(root.transform, 2); view.sta_unit = States(root.transform, 2);
        view.btn_lab = Child("lab-button", root.transform).AddComponent<Btn>();
        view.btn_switchMod = Child("switch", root.transform).AddComponent<Btn>();
        view.btn_apply = Child("apply", root.transform).AddComponent<Btn>();
        view.btn_close = Child("close", root.transform).AddComponent<Btn>();
        view.btn_edit = Child("edit", root.transform).AddComponent<Btn>();
        view.btn_del = Child("delete", root.transform).AddComponent<Btn>();
        view.btn_insert = Child("insert", root.transform).AddComponent<Btn>();
        var data = EventProgramDataForm.defaultData; data.uid = 90200; data.name = "tree-switch-test"; data.code = "Print(\"start\");";
        int closed = 0;
        var editor = new UiModStoryEventEditWindowCtrl { uiHolder = holder, view = view,
            model = new UiModStoryEventEditWindowModel(), param = new UiModStoryEventEditWindowParam { data = data, onClose = () => closed++ } };
        holder.ctrl = editor;
        root.SetActive(true);
        var rows = (UiTreeContainer<UiItemCtrl>)typeof(UiModStoryEventEditWindowCtrl).GetField("itemCon", Hidden).GetValue(editor);
        Check(rows.ActiveCount == 2, "real editor OnShow renders a statement and insertion row");
        var firstParam = rows.paramLst[0];
        int created = rows.CreatedCount;
        view.btn_switchMod.onClick.Invoke();
        Check(editor.model.codeEditMode && !panels[0].activeSelf && ReferenceEquals(rows.paramLst[0], firstParam),
            "Item-to-Code applies without rebuilding the outgoing tree");
        view.ipt_code.Set("Print(\"changed\");");
        view.btn_switchMod.onClick.Invoke();
        Check(!editor.model.codeEditMode && panels[0].activeSelf && rows.ActiveCount == 2 && rows.CreatedCount == created,
            "Code-to-Item binds the updated syntax using the existing rows");
        Check(data.code.Contains("changed") && !ReferenceEquals(rows.paramLst[0], firstParam), "switching back publishes the current source and fresh AST bindings");
        var rendered = rows.paramLst[0];
        view.btn_close.onClick.Invoke();
        Check(closed == 1 && !root.activeSelf && ReferenceEquals(rendered, rows.paramLst[0]), "Return applies and closes without rebuilding a hidden list");
    }

    private static Ipt Input(Transform parent)
    {
        var go = Child("input", parent); go.SetActive(false);
        var input = go.AddComponent<Ipt>(); input.textViewport = (RectTransform)go.transform;
        input.textComponent = Text(go.transform, "input-text"); go.SetActive(true); return input;
    }

    private static GameObject ItemTemplate(Transform parent)
    {
        var go = Child("item-template", parent); go.SetActive(false);
        var holder = go.AddComponent<UiHolder>();
        var depth = Child("depth-template", go.transform); depth.SetActive(false);
        var depthHolder = depth.AddComponent<UiHolder>(); depthHolder.elementTrsLst.Add(depth.transform);
        new UiDeepthCtrl().BindHolderRecursively(depthHolder);
        var sta = States(go.transform, 2);
        var units = Child("unit-root", go.transform); Intrinsic(units, false);
        var text = Text(go.transform, "new-label");
        var unit = Child("unit-template", units.transform); unit.SetActive(false); Intrinsic(unit, false);
        var unitHolder = unit.AddComponent<UiHolder>();
        var button = Child("button", unit.transform).AddComponent<Btn>();
        var unitState = States(unit.transform, 2);
        var image = Child("image", unit.transform).AddComponent<Img>();
        var subRoot = Child("nested-content", unit.transform); Intrinsic(subRoot, false);
        var label = Text(unit.transform, "unit-label");
        unitHolder.elementTrsLst = new List<Transform> { unit.transform, unit.transform, button.transform,
            unitState.transform, image.transform, subRoot.transform, image.transform, label.transform };
        new UiUnitCtrl().BindHolderRecursively(unitHolder);
        holder.subUiHolderLst.Add(depthHolder); holder.subUiHolderLst.Add(unitHolder);
        depthHolder.parent = holder; unitHolder.parent = holder;
        holder.elementTrsLst = new List<Transform> { go.transform, depth.transform, depth.transform,
            sta.transform, units.transform, text.transform, unit.transform, unit.transform, unit.transform };
        new UiItemCtrl().BindHolderRecursively(holder);
        return go;
    }

    private static void LayoutChecks()
    {
        var canvas = Child("layout-canvas"); canvas.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var content = Child("intrinsic-content", canvas.transform); Intrinsic(content, true);
        var rows = new List<RectTransform>();
        for (int i = 0; i < 3; i++)
        {
            var row = Child("row", content.transform); Intrinsic(row, false); rows.Add((RectTransform)row.transform);
            var wrap = Child("nested-fit", row.transform); Intrinsic(wrap, true);
            for (int j = 0; j < 2; j++)
            {
                var leaf = Child("leaf", wrap.transform);
                var size = leaf.AddComponent<LayoutElement>(); size.preferredWidth = 40 + i * 10; size.preferredHeight = 15 + i * 5;
                var fit = leaf.AddComponent<ContentSizeFitter>(); fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
            }
        }
        var root = (RectTransform)content.transform;
        var probe = content.AddComponent<TreeLayoutProbe>();
        for (int i = 0; i < 10; i++) { UiTreeLayout.Request(root); UiTreeLayout.Request(rows[0]); }
        typeof(UiTreeLayout).GetMethod("Flush", HiddenStatic).Invoke(null, null);
        Check(probe.horizontal == 1 && probe.vertical == 1, "duplicate and ancestor/descendant requests form one final layout transaction");
        Check(Near(root.rect.width, 60) && Near(root.rect.height, 120), "one bottom-up transaction resolves nested preferred width and height");
        Check(Near(rows[0].rect.height, 30) && Near(rows[1].rect.height, 40) && Near(rows[2].rect.height, 50), "parent fitting sees completed child dimensions in that same pass");
        Check(rows[0].anchoredPosition.y != rows[1].anchoredPosition.y && rows[1].anchoredPosition.y != rows[2].anchoredPosition.y,
            "rows are positioned with their final heights");
        UiTreeLayout.Request(root); UiTreeLayout.Cancel(root);
        typeof(UiTreeLayout).GetMethod("Flush", HiddenStatic).Invoke(null, null);
        Check(probe.horizontal == 1, "cancelled layout requests do no work");
        content.SetActive(false); UiTreeLayout.Request(root);
        typeof(UiTreeLayout).GetMethod("Flush", HiddenStatic).Invoke(null, null);
        Check(probe.horizontal == 1, "hidden trees are skipped");
        content.SetActive(true); UiTreeLayout.Request(root); Canvas.ForceUpdateCanvases();
        Check(Near(root.rect.height, 120), "queued sizing also runs in the Canvas render lifecycle");
    }

    private static bool Near(float a, float b) => Mathf.Abs(a - b) < .1f;
    private static void Intrinsic(GameObject go, bool vertical)
    {
        var group = vertical ? (HorizontalOrVerticalLayoutGroup)go.AddComponent<VerticalLayoutGroup>() : go.AddComponent<HorizontalLayoutGroup>();
        group.childControlWidth = group.childControlHeight = false;
        group.childForceExpandWidth = group.childForceExpandHeight = false;
        var fit = go.AddComponent<ContentSizeFitter>(); fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
    }
    private static Sta States(Transform parent, int count)
    {
        var go = Child("state", parent); go.SetActive(false);
        var sta = go.AddComponent<Sta>();
        typeof(State).GetField("stateGo", Hidden).SetValue(sta, Enumerable.Range(0, count).Select(i => Child("state-" + i, go.transform)).ToArray());
        go.SetActive(true); return sta;
    }
    private static Txt Text(Transform parent, string name)
    {
        var go = Child(name, parent); go.SetActive(false);
        var text = go.AddComponent<Txt>(); text.font = font; text.languageTranslatable = false; text.fontSize = 20; text.enableWordWrapping = false;
        var fit = go.AddComponent<ContentSizeFitter>(); fit.horizontalFit = fit.verticalFit = ContentSizeFitter.FitMode.PreferredSize;
        go.SetActive(true); return text;
    }
    private static GameObject Child(string name, Transform parent = null)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false);
        ((RectTransform)go.transform).sizeDelta = Vector2.zero; return go;
    }
    private static void Register(Type type)
    {
        if (!forms.Add(type)) return;
        var parent = type.GetNestedType("Data")?.BaseType?.DeclaringType; if (parent != null) Register(parent);
        type.GetMethod("Register", HiddenStatic)?.Invoke(null, null);
        type.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
    }
    private static IEnumerator Guard(IEnumerator work)
    {
        while (true)
        {
            bool next;
            try { next = work.MoveNext(); }
            catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); yield break; }
            if (!next) break;
            yield return work.Current;
        }
        Debug.Log("UI TREE CONTAINER REGRESSION PASS checks=" + checks); EditorApplication.Exit(0);
    }
    internal static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message); checks++;
    }
}

public sealed class TreeFixtureEditor : UiModStoryEventEditWindowCtrl
{
    public override void OnCreate() { }
    public override void OnShow() { }
    public override void OnDisable() { }
}

public sealed class TreeProbeParam : UiParam
{
    public UiTreeContainer<TreeProbeCtrl> con;
    public Transform destination;
    public int levels;
}
public sealed class TreeProbeCtrl : UiCtrl, IUiTreePoolItem
{
    public TreeProbeParam param;
    private TreeProbeParam bound;
    public int shows, renders;
    public override void SetParam(UiParam value) { param = (TreeProbeParam)value; }
    public override void OnShow()
    {
        shows++;
        UiTreeContainerRegression.Check(param != null && transformParent() == param.destination, "destination and parameter precede activation");
        if (ReferenceEquals(bound, param)) return;
        bound = param; renders++;
        UiTreeContainerRegression.Check(uiHolder.transform.childCount == 0, "rent never activates stale pooled children");
        if (param.levels == 0) return;
        for (int i = 0; i < 2; i++) param.con.Add(new TreeProbeParam { con = param.con, destination = uiHolder.transform, levels = param.levels - 1 });
    }
    private Transform transformParent() => uiHolder.transform.parent;
    public void OnTreePoolRelease() { bound = null; }
}
public sealed class TreeLayoutProbe : MonoBehaviour, ILayoutElement
{
    public int horizontal, vertical;
    public void CalculateLayoutInputHorizontal() { horizontal++; }
    public void CalculateLayoutInputVertical() { vertical++; }
    public float minWidth => -1;
    public float preferredWidth => -1;
    public float flexibleWidth => -1;
    public float minHeight => -1;
    public float preferredHeight => -1;
    public float flexibleHeight => -1;
    public int layoutPriority => 0;
}
