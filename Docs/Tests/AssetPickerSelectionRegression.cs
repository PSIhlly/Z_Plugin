using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.ExceptionServices;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Z_Code;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Ui;
using Z_Ui.Base;
using Picker = Ui.ModAssetSelectWindow;
using Object = UnityEngine.Object;

// Only run in the runner's temporary Unity project; no scene/save is loaded.
public static class AssetPickerSelectionRegression
{
    private const BindingFlags PrivateStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags PrivateInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly MethodInfo Initialize = typeof(Picker.UiModAssetSelectWindowCtrl)
        .GetMethod("InitializeSelection", PrivateInstance);
    private static readonly MethodInfo Resolve = typeof(Picker.UiModAssetSelectWindowCtrl)
        .GetMethod("ResolveInitialSelection", PrivateStatic);
    private static readonly List<GameObject> owned = new List<GameObject>();
    private static int checks, callbackCount;
    private static StoryTexAssetForm.Data textureA, textureB, unclassified;
    private static AudioAssetForm.Data audio;
    private static int labA, labB;
    private static readonly AssetEvents events = new AssetEvents();

    public static void Run()
    {
        try
        {
            RegisterForms();
            MakeAssets();
            Z_EventHelper.Register<AssetEvent>(events);
            CheckSelectionState();
            CheckActualCallers();
            foreach (var dir in new[] { ScrView.Direction.Vertical, ScrView.Direction.Horizon })
            foreach (bool scaled in new[] { false, true })
                CheckVirtualizedFocus(dir, scaled);
            Check(events.count == 0 && callbackCount == 0,
                "initial selection and focus never complete a choice or publish AssetEvent");
            Debug.Log("ASSET_PICKER_SELECTION_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            Z_EventHelper.Unregister<AssetEvent>(events);
            for (int i = owned.Count - 1; i >= 0; i--)
                if (owned[i] != null) Object.DestroyImmediate(owned[i]);
        }
    }

    private sealed class AssetEvents : IZ_Listener<AssetEvent>
    {
        public int count;
        public void OnEvent(AssetEvent value) { count++; }
    }

    private static void RegisterForms()
    {
        foreach (var type in new[] { typeof(AssetForm), typeof(TexAssetForm), typeof(StoryTexAssetForm),
            typeof(AudioAssetForm), typeof(StoryAudioAssetForm), typeof(VideoAssetForm), typeof(StoryVideoAssetForm),
            typeof(LabForm), typeof(ProductForm), typeof(ObjectProductForm), typeof(MapModelForm),
            typeof(EventTriggerForm), typeof(MapObjectParamForm) })
            type.GetMethod("Register", PrivateStatic)?.Invoke(null, null);
        AssetForm.InitInternal(); TexAssetForm.InitInternal(); StoryTexAssetForm.InitInternal();
        AudioAssetForm.InitInternal(); StoryAudioAssetForm.InitInternal();
        VideoAssetForm.InitInternal(); StoryVideoAssetForm.InitInternal(); LabForm.InitInternal();
        ProductForm.InitInternal(); ObjectProductForm.InitInternal(); MapModelForm.InitInternal();
    }

    private static void MakeAssets()
    {
        labA = LabForm.GetOrCreate("picker-regression-A", "", "", nameof(StoryTexAssetForm));
        labB = LabForm.GetOrCreate("picker-regression-B", "", "", nameof(StoryTexAssetForm));
        textureA = Texture("selected-A", labA);
        textureB = Texture("selected-B", labB);
        unclassified = Texture("selected-unclassified", 0);
        var audioData = new StoryAudioAssetForm.Data(-1, "wrong-kind", "", null, "", null, 0);
        StoryAudioAssetForm.AddData(audioData);
        audio = audioData;
    }

    private static StoryTexAssetForm.Data Texture(string name, int lab)
    {
        var value = new StoryTexAssetForm.Data(-1, name, "", null, "", null, lab);
        StoryTexAssetForm.AddData(value);
        return value;
    }

    private static Picker.UiModAssetSelectTexWindowParam Param(int? id = null) =>
        new Picker.UiModAssetSelectTexWindowParam { selectedAssetId = id, onComplete = _ => callbackCount++ };

    private static void CheckSelectionState()
    {
        Check(Initialize != null && Resolve != null, "production initial-selection helpers are available");
        var ctrl = new Picker.UiModAssetSelectWindowCtrl { model = new Picker.UiModAssetSelectWindowModel() };
        ctrl.model.prm = Param();
        ctrl.model.labTabInitialized = true;
        ctrl.model.curLabId = labA;
        Init(ctrl, Param(textureB.id));
        Check(ReferenceEquals(ctrl.model.sel, textureB) && ctrl.model.curLabId == labB,
            "current texture is selected directly in its own Label, never All");
        Check(ctrl.model.pendingFocusAssetId == textureB.id && ctrl.model.retryInitialFocus,
            "valid incoming selection requests one virtualized focus");
        Init(ctrl, Param());
        Check(ctrl.model.sel == null && ctrl.model.curLabId == labB && !ctrl.model.pendingFocusAssetId.HasValue,
            "opening an empty frame retains its prior scope Label but clears selection and focus");
        foreach (int id in new[] { 0, int.MaxValue, audio.id })
        {
            Init(ctrl, Param(id));
            Check(ctrl.model.sel == null && ctrl.model.curLabId == labB && !ctrl.model.retryInitialFocus,
                "placeholder/missing/wrong-type references retain the old Label without selection");
        }
        var hidden = new TexAssetForm.Data(-1, "not-a-story-texture", "", null, "", null, 0);
        TexAssetForm.AddData(hidden);
        Init(ctrl, Param(hidden.id));
        Check(ctrl.model.sel == null && ctrl.model.curLabId == labB,
            "a texture excluded from the picker cannot force an invisible selection");
        Init(ctrl, Param(unclassified.id));
        Check(ReferenceEquals(ctrl.model.sel, unclassified) && ctrl.model.curLabId == 0,
            "an unclassified current texture selects the concrete zero Label rather than All");
        int foreignLabId = LabForm.GetOrCreate("picker-regression-foreign", "", "", nameof(StoryVideoAssetForm));
        foreach (int inaccessibleLabId in new[] { int.MaxValue, foreignLabId })
        {
            var orphan = Texture("inaccessible-label-" + inaccessibleLabId, inaccessibleLabId);
            Init(ctrl, Param(orphan.id));
            Check(ReferenceEquals(ctrl.model.sel, orphan) && !ctrl.model.curLabId.HasValue,
                "a current texture with a missing or wrong-scope Label falls back to All without losing selection");
            Check(ctrl.model.pendingFocusAssetId == orphan.id && ctrl.model.retryInitialFocus,
                "All fallback keeps the selected image's initial-focus request for its visible row");
        }
        ctrl.OnHide();
        Check(!ctrl.model.pendingFocusAssetId.HasValue && !ctrl.model.retryInitialFocus,
            "Hide cancels an initial-focus request that has not rendered");
        var completeFocus = typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("CompleteInitialFocus", PrivateInstance);
        Check(completeFocus != null, "production focus completion helper is available");
        foreach (bool labsFocused in new[] { false, true })
        foreach (bool itemsFocused in new[] { false, true })
        {
            Init(ctrl, Param(textureA.id));
            completeFocus.Invoke(ctrl, new object[] { labsFocused, itemsFocused });
            bool complete = labsFocused && itemsFocused;
            Check(ReferenceEquals(ctrl.model.sel, textureA)
                && ctrl.model.pendingFocusAssetId == (complete ? (int?)null : textureA.id)
                && ctrl.model.retryInitialFocus == !complete,
                "initial focus clears only after both Label and image lists focused: " + labsFocused + "/" + itemsFocused);
        }
        Check(Calls(typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("Refresh"), completeFocus),
            "the real Refresh lifecycle uses the tested dual-list focus completion rule");
        Init(ctrl, Param(textureA.id));
        ctrl.ClearPendingInitialFocus();
        Check(ReferenceEquals(ctrl.model.sel, textureA) && !ctrl.model.pendingFocusAssetId.HasValue,
            "consuming focus retains the selected current asset");
        Init(ctrl, Param(textureB.id));
        Check(ReferenceEquals(ctrl.model.sel, textureB) && ctrl.model.pendingFocusAssetId == textureB.id,
            "a reopened picker consumes the new incoming ID instead of a stale prior selection");
        Init(ctrl, new Picker.UiModAssetSelectAudioWindowParam { selectedAssetId = textureA.id });
        Check(ctrl.model.sel == null && ctrl.model.curLabId == 0,
            "changing asset family normalizes the remembered Label and rejects another family's ID");
        Init(ctrl, new Picker.UiModAssetSelectAudioWindowParam { selectedAssetId = audio.id });
        Check(ReferenceEquals(ctrl.model.sel, audio), "audio preselection does not require or start a media player");
        Check(Calls(typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("OnShow"), Initialize),
            "the real OnShow lifecycle invokes the tested initial-selection helper");
        Check(Calls(typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("OnHide"),
                typeof(Picker.UiModAssetSelectWindowCtrl).GetMethod("ClearPendingInitialFocus")),
            "the real OnHide lifecycle cancels pending focus");
    }

    private static bool Calls(MethodInfo caller, MethodInfo callee)
    {
        byte[] il = caller.GetMethodBody().GetILAsByteArray();
        for (int i = 0; i + 4 < il.Length; i++)
            if ((il[i] == 0x28 || il[i] == 0x6f) && BitConverter.ToInt32(il, i + 1) == callee.MetadataToken)
                return true;
        return false;
    }

    private static void Init(Picker.UiModAssetSelectWindowCtrl ctrl, Picker.UiModAssetSelectWindowParam param)
    {
        try { Initialize.Invoke(ctrl, new object[] { param }); }
        catch (TargetInvocationException error) when (error.InnerException != null)
        { ExceptionDispatchInfo.Capture(error.InnerException).Throw(); }
    }

    private sealed class CapturingPicker : Picker.UiModAssetSelectWindowCtrl
    {
        public Picker.UiModAssetSelectTexWindowParam captured;
        public override void OnShow() { captured = param as Picker.UiModAssetSelectTexWindowParam; }
    }

    private static void CheckActualCallers()
    {
        var manager = CreateManager<UiManager>();
        var root = Go("capturing-picker");
        root.SetActive(false);
        var holder = root.AddComponent<UiHolder>();
        var picker = new CapturingPicker { inited = true, model = new Picker.UiModAssetSelectWindowModel(), uiHolder = holder };
        holder.ctrl = picker;
        root.SetActive(true);
        manager.uiCtrlName2Uis[typeof(Picker.UiModAssetSelectWindowCtrl).Name] = new List<UiHolder> { holder };
        var product = ObjectProductForm.GetDataByJo(new JObject());
        product.uid = -1; product.name = "current-picture-object";
        product.faceType = FaceType.FourDirection;
        product.EnsureDirectionData();
        product.GetAnimClip(AnimDirecton.Up).Add(textureA.id);
        product.GetAnimClip(AnimDirecton.Right).Add(textureB.id);
        product.minimapIcon = textureA.id;
        ObjectProductForm.AddData(product);
        var assets = new ModAssetCtrl(null);
        assets.ImportObjectUnitTex(product.uid, AnimDirecton.Right, 0);
        Check(picker.captured != null && picker.captured.selectedAssetId == textureB.id,
            "Object frame import passes the selected facing's actual current texture through UiManager");
        assets.ImportObjectUnitTex(product.uid, AnimDirecton.Up, 0);
        Check(picker.captured.selectedAssetId == textureA.id,
            "another facing passes its own texture rather than the legacy mirror or previous picker value");
        assets.ImportObjectUnitTex(product.uid, AnimDirecton.Right, -1);
        Check(!picker.captured.selectedAssetId.HasValue || picker.captured.selectedAssetId == 0,
            "the New frame has no accidental current image");
        assets.ImportObjectUnitTex(product.uid, AnimDirecton.Right, 1000);
        Check(!picker.captured.selectedAssetId.HasValue || picker.captured.selectedAssetId == 0,
            "an absent frame cannot index beyond its clip");
        assets.ImportObjectMinimap(product.uid);
        Check(picker.captured.selectedAssetId == textureA.id, "Object minimap import passes its stored current image");
        var mod = CreateManager<ModManager>();
        mod.assetCtrl = assets;
        var assetManager = CreateManager<AssetManager>();
        assetManager.texCtrl = new TexController(assetManager);
        var imageCommand = new ImageCmd();
        string generated = null;
        string textureMarker = assetManager.texCtrl.GetName(textureB.id);
        imageCommand.GetUnitChooseCode(value => generated = value,
            new SyntaxNode(new Desc(textureMarker, CodeType.Str)));
        Check(picker.captured.selectedAssetId == textureB.id,
            "Image literal supplies its current marked texture without evaluating an expression");
        picker.captured.onComplete(textureA);
        Check(generated == "\"" + assetManager.texCtrl.GetName(textureA.id) + "\"",
            "Image callback preserves quoted image markers");
        imageCommand.GetUnitChooseCode(_ => callbackCount++, null);
        Check(!picker.captured.selectedAssetId.HasValue,
            "an empty Image expression has no accidental current asset");
        imageCommand.GetUnitChooseCode(_ => callbackCount++,
            new SyntaxNode(new Desc("currentImage", CodeType.VarName)));
        Check(!picker.captured.selectedAssetId.HasValue,
            "an Image variable is not evaluated to guess a current asset");
    }

    private static void CheckVirtualizedFocus(ScrView.Direction direction, bool scaled)
    {
        const int count = 240, target = 187;
        var root = Rect("virtualized-picker", null, new Vector2(480, 240));
        if (scaled) root.localScale = new Vector3(1.7f, .7f, 1);
        var viewport = Rect("viewport", root, new Vector2(480, 240));
        var content = Rect("content", viewport, new Vector2(480, 240));
        content.anchorMin = content.anchorMax = new Vector2(0, 1);
        content.pivot = new Vector2(0, 1);
        content.anchoredPosition = Vector2.zero;
        if (scaled) content.localScale = new Vector3(.8f, 1.1f, 1);
        var cell = Rect("cell", content, new Vector2(80, 40));
        cell.gameObject.SetActive(false);
        var scroll = root.gameObject.AddComponent<ScrView>();
        scroll.viewport = viewport; scroll.content = content; scroll.cell = cell;
        scroll.horizontal = direction == ScrView.Direction.Horizon;
        scroll.vertical = direction == ScrView.Direction.Vertical;
        scroll.dir = direction;
        scroll.xSpacing = 7; scroll.ySpacing = 9; scroll.left = 11; scroll.top = 13;
        scroll.fiilType = ScrView.FillType.None;
        var items = new Dictionary<int, GameObject>();
        int created = 0;
        scroll.ContainerAdd = index =>
        {
            created++;
            var value = Rect("visible-" + index, content, cell.sizeDelta).gameObject;
            items[index] = value;
            return value;
        };
        scroll.ContainerDel = value =>
        {
            int key = -1;
            foreach (var pair in items) if (ReferenceEquals(pair.Value, value)) { key = pair.Key; break; }
            if (key >= 0) items.Remove(key);
            value.SetActive(false);
        };
        Check(!scroll.ScrollToIndex(target), "uninitialized lists reject focus safely");
        scroll.RefreshView(count, new List<Vector3>());
        Check(!items.ContainsKey(target), "test target begins offscreen in the virtualized list");
        scroll.velocity = new Vector2(15, 20);
        Check(scroll.ScrollToIndex(target), direction + " accepts a valid focus index");
        Check(items.TryGetValue(target, out var selected), direction + " target is materialized immediately after focus");
        Check(scroll.velocity == Vector2.zero, "initial focus stops stale scroll inertia");
        CheckMainAxisVisible(selected.GetComponent<RectTransform>(), viewport, direction);
        Check(created < count && items.Count < count / 2, "focus never instantiates the whole asset list");
        Vector2 position = content.anchoredPosition;
        int before = created;
        scroll.ScrollToIndex(target);
        Check((content.anchoredPosition - position).sqrMagnitude < .00001f && before == created,
            "an already visible current item keeps the manual scroll position and instances");
        foreach (int edge in new[] { 0, count - 1 })
        {
            Check(scroll.ScrollToIndex(edge) && items.TryGetValue(edge, out selected), "first and last assets can be focused");
            CheckMainAxisVisible(selected.GetComponent<RectTransform>(), viewport, direction);
        }
        position = content.anchoredPosition;
        Check(!scroll.ScrollToIndex(-1) && !scroll.ScrollToIndex(count)
            && (position - content.anchoredPosition).sqrMagnitude < .00001f,
            "invalid focus indices never move a list");
    }

    private static void CheckMainAxisVisible(RectTransform item, RectTransform viewport, ScrView.Direction dir)
    {
        var corners = new Vector3[4]; item.GetWorldCorners(corners);
        float min = float.PositiveInfinity, max = float.NegativeInfinity;
        foreach (var corner in corners)
        {
            Vector3 local = viewport.InverseTransformPoint(corner);
            float value = dir == ScrView.Direction.Vertical ? local.y : local.x;
            min = Mathf.Min(min, value); max = Mathf.Max(max, value);
        }
        float low = dir == ScrView.Direction.Vertical ? viewport.rect.yMin : viewport.rect.xMin;
        float high = dir == ScrView.Direction.Vertical ? viewport.rect.yMax : viewport.rect.xMax;
        Check(min >= low - .02f && max <= high + .02f, "focused asset's full main-axis bounds lie in the viewport");
    }

    private static RectTransform Rect(string name, Transform parent, Vector2 size)
    {
        var go = Go(name, true); var rect = go.GetComponent<RectTransform>();
        if (parent != null) rect.SetParent(parent, false);
        rect.sizeDelta = size;
        return rect;
    }
    private static GameObject Go(string name, bool rect = false)
    {
        var go = rect ? new GameObject(name, typeof(RectTransform)) : new GameObject(name);
        owned.Add(go); return go;
    }
    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = Go(typeof(T).Name); root.SetActive(false);
        var value = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", PrivateStatic).SetValue(null, value);
        return value;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
