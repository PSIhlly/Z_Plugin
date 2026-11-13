using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text.RegularExpressions;
using Form;
using Newtonsoft.Json.Linq;
using TMPro;
using Ui.PlayAsset;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Math;
using Z_Time;
using Z_Ui;
using Z_Ui.Base;
using Object = UnityEngine.Object;

// Native generated views, TMP/UGUI and real commands in a disposable project.
public static class PlayAssetTextRegression
{
    private const BindingFlags Hidden = BindingFlags.Static | BindingFlags.NonPublic;
    private const int TextureId = 730011;
    private static readonly List<GameObject> roots = new List<GameObject>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static GameManager game;
    private static PlayManager play;
    private static PlayAssetController assets;
    private static UiPlayAssetCtrl panel;
    private static TMP_FontAsset fontAsset;
    private static TMP_Settings settings, previousSettings;
    private static Font font;
    private static Texture2D texture;
    private static string scratch;
    private static int checks;

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name
                .StartsWith("MapRuntimeRegression-", StringComparison.Ordinal), "isolated project guard");
            SavedPrefabBindings();
            SetUp();
            MetadataAndSource();
            RawTextSizingAndReuse();
            MixedControlsAndTiming();
            ImageToTextClearsBindings();
            PersistenceAndLifetime();
            LegacyOpacityCompatibility();
            Debug.Log("PLAY_ASSET_TEXT_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (panel != null) panel.Unregister<PlayAssetEvent>();
            foreach (var root in roots.AsEnumerable().Reverse())
                if (root != null) Object.DestroyImmediate(root);
            if (texture != null) Object.DestroyImmediate(texture);
            if (fontAsset != null) Object.DestroyImmediate(fontAsset);
            typeof(TMP_Settings).GetField("s_Instance", Hidden).SetValue(null, previousSettings);
            if (settings != null) Object.DestroyImmediate(settings);
            if (font != null && !EditorUtility.IsPersistent(font)) Object.DestroyImmediate(font);
            if (!string.IsNullOrEmpty(scratch) && Directory.Exists(scratch)
                && Path.GetFileName(scratch).StartsWith("PlayAssetTextRegression-", StringComparison.Ordinal))
                Directory.Delete(scratch, true);
        }
    }

    private static void SavedPrefabBindings()
    {
        var source = File.ReadAllText(Path.Combine(Path.GetDirectoryName(Application.dataPath), "PlayAsset.prefab.txt"));
        var holder = Regex.Match(source,
            @"(?ms)^  uiName: Image\r?\n.*?^  elementTrsLst:\r?\n(?<entries>(?:  - \{fileID: -?\d+\}\r?\n)+)");
        Check(holder.Success, "The actual saved PlayAsset prefab contains its Image UiHolder bindings");
        var entries = Regex.Matches(holder.Groups["entries"].Value, @"fileID: (-?\d+)")
            .Cast<Match>().Select(match => match.Groups[1].Value).ToArray();
        Check(entries.Length == 3 && entries.All(id => id != "0") && entries.Distinct().Count() == 3,
            "Saved Image bindings contain go_image, img_ and txt_, matching the generated three-index view");
        Check(Regex.IsMatch(source, @"(?m)^--- !u!224 &" + Regex.Escape(entries[2]) + @"(?: stripped)?\r?$")
            && source.Contains("value: txt_"), "The third saved binding resolves to the nested text prefab's RectTransform");
    }

    private static void RegisterForm(Type form)
    {
        if (!forms.Add(form)) return;
        var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType;
        if (parent != null) RegisterForm(parent);
        form.GetMethod("Register", Hidden | BindingFlags.DeclaredOnly)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
    }
    private static GameObject Go(string name, Transform parent = null)
    {
        var root = new GameObject(name, typeof(RectTransform));
        root.SetActive(false);
        root.transform.SetParent(parent, false);
        roots.Add(root);
        return root;
    }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = Go("play-asset-text-" + typeof(T).Name);
        var value = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", Hidden).SetValue(null, value);
        return value;
    }
    private static void SetUp()
    {
        foreach (var form in new[] { typeof(ImageUiItemForm), typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm),
            typeof(CmdLabForm), typeof(ProgramDataForm), typeof(BoxDataForm), typeof(TexAssetForm), typeof(ModTextForm) })
            RegisterForm(form);
        foreach (var type in new[] { typeof(ShowTextCmd), typeof(ShowImageCmd), typeof(SetImagePositionCmd),
            typeof(SetImageAngleCmd), typeof(SetImageOpacityCmd), typeof(DeleteImageCmd) })
            type.GetMethod("Init", Hidden | BindingFlags.DeclaredOnly).Invoke(null, null);
        game = Manager<GameManager>();
        play = Manager<PlayManager>();
        var assetManager = Manager<AssetManager>();
        assetManager.texCtrl = new TexController(assetManager);
        assets = new PlayAssetController(play);
        play.assetCtrl = assets;
        var ui = Manager<UiManager>();
        texture = new Texture2D(10, 20);
        TexAssetForm.DataById[TextureId] = new TexAssetForm.Data(TextureId, "native test image", "", null, "", texture, 0);
        font = AssetDatabase.LoadAssetAtPath<Font>("Assets/PlayAssetTextShaders/LiberationSans.ttf");
        Check(font != null, "native fixture imports a font with embedded face data");
        var settingsField = typeof(TMP_Settings).GetField("s_Instance", Hidden);
        previousSettings = settingsField.GetValue(null) as TMP_Settings;
        settings = ScriptableObject.CreateInstance<TMP_Settings>();
        settingsField.SetValue(null, settings);
        fontAsset = TMP_FontAsset.CreateFontAsset(font);
        Check(fontAsset != null, "native fixture creates a real dynamic TMP font");
        typeof(TMP_Settings).GetField("m_defaultFontAsset", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(settings, fontAsset);
        typeof(TMP_Settings).GetField("m_fallbackFontAssets", BindingFlags.Instance | BindingFlags.NonPublic).SetValue(settings, new List<TMP_FontAsset>());

        var canvas = Go("play-asset-native-canvas");
        canvas.AddComponent<Canvas>().renderMode = RenderMode.ScreenSpaceOverlay;
        var root = Go("PlayAsset", canvas.transform);
        var rootHolder = root.AddComponent<RegressionPlayAssetTextHolder>();
        rootHolder.uiType = UiType.Panel;
        rootHolder.uiName = "PlayAsset";
        root.GetComponent<RectTransform>().sizeDelta = new Vector2(800, 600);
        var template = Go("go_image", root.transform);
        var templateHolder = template.AddComponent<RegressionPlayAssetTextHolder>();
        templateHolder.uiType = UiType.Sub;
        templateHolder.uiName = "Image";
        templateHolder.parent = rootHolder;
        var imageWrapper = Go("go_image", template.transform);
        var imageRoot = Go("img_", imageWrapper.transform);
        var image = imageRoot.AddComponent<Img>();
        var textRoot = Go("txt_", template.transform);
        var text = textRoot.AddComponent<Txt>();
        text.font = fontAsset;
        text.fontSize = 60;
        text.enableAutoSizing = false;
        text.enableWordWrapping = false;
        // Runtime must explicitly select raw text, even with a translatable prefab.
        text.languageTranslatable = true;
        text.color = Color.white;
        image.color = Color.white;
        templateHolder.elementTrsLst = new List<Transform> { imageWrapper.transform, imageRoot.transform, textRoot.transform };
        rootHolder.elementTrsLst = new List<Transform> { template.transform, template.transform };
        rootHolder.subUiHolderLst.Add(templateHolder);
        ui.uiCtrlName2Uis[nameof(UiPlayAssetCtrl)] = new List<UiHolder>();
        rootHolder.InstanceInit<UiPlayAssetCtrl>();
        panel = (UiPlayAssetCtrl)rootHolder.ctrl;
        imageRoot.SetActive(true);
        imageWrapper.SetActive(true);
        textRoot.SetActive(true);
        canvas.SetActive(true);
        assets.Begin();
        Check(panel.active && panel.inited, "real generated PlayAsset panel initializes through its UiHolder");
    }

    private static void MetadataAndSource()
    {
        Check(CmdBase.TryGetForm("ShowText", out var form) && form is GameCmdDataForm.Data typed && typed.uid == 100134,
            "ShowText has executable metadata");
        Check(form.prmNames.SequenceEqual(new[] { "text", "scale", "showTime" })
            && form.prmTypes.SequenceEqual(new[] { "string", "num", "num" })
            && form.retTypes.SequenceEqual(new[] { "uiImg" }), "ShowText exposes text/scale/duration and the shared overlay handle type");
        Check(BaseData.cmdDic["ShowText"] is ShowTextCmd && new ShowTextCmd().GetNew() is ShowTextCmd,
            "ShowText is explicitly registered and clones the production command");
        Check(typeof(ShowTextCmd).GetMethod("Init", Hidden).GetCustomAttributes(
            typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1, "ShowText is registered for Player builds");
        const string preset = "text = ShowText(\"empty\", 1, 1);";
        Check(form.defaultCode == preset, "ShowText preserves the requested complete default statement");
        var statement = typeof(Ui.ModStoryEventEditWindow.UiModStoryEventEditWindowCtrl)
            .GetMethod("BuildCommandStatementCode", Hidden);
        string expression = null;
        new ShowTextCmd().GetUnitChooseCode(code => expression = code, null);
        Check(expression == "ShowText(\"empty\", 1, 1)", "argument selection uses only the call expression");
        Check((string)statement.Invoke(null, new object[] { form, form.defaultCode }) == preset
            && (string)statement.Invoke(null, new object[] { form, expression }) == preset,
            "New, Insert and void replacement do not duplicate assignment prefixes or terminators");
        Compile(preset);
        Compile("Return " + expression + ";");
        var imageDefault = GameCmdDataForm.DataByName["ShowImage"];
        var imageStatement = (string)statement.Invoke(null, new object[] { imageDefault, imageDefault.defaultCode });
        Check(imageStatement == imageDefault.allowAsVoid + imageDefault.defaultCode + ";",
            "ordinary image defaults keep their previous statement wrapping");
        Compile(imageStatement);
        var compiled = Compile("shown = ShowText(\"gain\",1,3); Return shown;");
        var restored = ProgramDataForm.GetDataByJo(ProgramDataForm.GetJoByData(compiled));
        Check(compiled.zCode.SequenceEqual(restored.zCode) && compiled.zCodeMap.SequenceEqual(restored.zCodeMap),
            "ShowText source and bytecode survive a program Form round-trip");
        var result = Run(restored);
        var data = Data(result);
        Check(data.isText && data.text == "gain" && data.ctrl != null && data.ctrl.view.txt_.text == "gain",
            "compiled ShowText returns a handle for literal unlocalized text");
        Clear();
    }

    private static void RawTextSizingAndReuse()
    {
        var data = Data(Execute(new ShowTextCmd(), Text("gain\nRaw text"), Number(2), Number(30)).res[0]);
        var ctrl = data.ctrl;
        Check(data.textScale == 2 && ctrl.view.txt_.fontSize == 120 && ctrl.view.txt_.text == data.text
            && ctrl.view.txt_.enabled && !ctrl.view.img_.enabled,
            "text doubles the authored 60pt font and hides the sibling image");
        Check(ctrl.view.txt_.text == data.text && ctrl.rect.sizeDelta.x > 0 && ctrl.rect.sizeDelta.y > 0
            && !float.IsNaN(ctrl.rect.sizeDelta.x), "native TMP preferred size follows multiline literal content");
        float fontSize = ctrl.view.txt_.fontSize;
        Vector2 dimensions = ctrl.rect.sizeDelta;
        for (int i = 0; i < 3; i++) panel.Refresh();
        Check(data.ctrl.view.txt_.fontSize == fontSize && data.ctrl.rect.sizeDelta == dimensions,
            "refresh/pooling does not compound text scale or dimensions");
        int pooledId = data.ctrl.gameObject.GetInstanceID();
        assets.Remove(data.uid);
        Check(data.ctrl == null, "explicit deletion releases the old Data's controller reference");
        var normal = Data(Execute(new ShowTextCmd(), Text("short"), Number(1), Number(30)).res[0]);
        Check(normal.ctrl.gameObject.GetInstanceID() == pooledId && normal.ctrl.view.txt_.fontSize == 60
            && normal.ctrl.view.txt_.text == "short", "a recycled text row returns to the original font size");
        Clear();
        var empty = Data(Execute(new ShowTextCmd(), Text(""), Number(1), Number(30)).res[0]);
        Check(empty.ctrl.view.txt_.text == "" && empty.ctrl.rect.sizeDelta == Vector2.zero
            && !empty.ctrl.view.img_.enabled && empty.ctrl.view.img_.sprite == null,
            "empty text has zero measured size without a white image placeholder");
        Clear();
    }

    private static void MixedControlsAndTiming()
    {
        var textHandle = Execute(new ShowTextCmd(), Text("controls"), Number(1), Number(20)).res[0];
        var text = Data(textHandle);
        var imageHandle = Execute(new ShowImageCmd(), Text("$i$" + TextureId + "$i$"), Number(2), Number(20)).res[0];
        var image = Data(imageHandle);
        Check(text.uid != image.uid && text.isText && !image.isText && text.ctrl != null && image.ctrl != null,
            "image and text have independent handles in the same Form and panel");
        Check(image.size == new Vector2(20, 40) && image.ctrl.view.img_.enabled
            && !image.ctrl.view.txt_.enabled && image.ctrl.view.img_.sprite != null,
            "ordinary images retain their existing pixel scaling and hide the text sibling");
        foreach (var entry in new[] { text, image })
        {
            var handle = Text("$ui$" + entry.uid + "$ui$");
            Check(Mathf.Abs(entry.ctrl.CurrentOpacity - 1) < .001f, "new overlay starts fully opaque");
            Execute(new SetImagePositionCmd(), handle, Number(.25f), Number(.75f), Number(0));
            Check(Vector3.Distance(entry.ctrl.rect.position, Graph.GetRealPos(new Vector2(.25f, .75f), panel.rect)) < .001f,
                "shared position command moves either graphic kind");
            Execute(new SetImageAngleCmd(), handle, Number(90), Number(0));
            Check(Mathf.Abs(Mathf.DeltaAngle(entry.ctrl.rect.eulerAngles.z, 90)) < .001f,
                "shared angle command rotates either graphic kind");
            Execute(new SetImageOpacityCmd(), handle, Number(.25f), Number(0));
            Check(Mathf.Abs(entry.ctrl.CurrentOpacity - .25f) < .001f
                && Mathf.Abs((entry.isText ? entry.ctrl.view.txt_.color.a : entry.ctrl.view.img_.color.a) - .25f) < .001f,
                "shared opacity changes the active Graphic's actual alpha");
        }
        Execute(new SetImageOpacityCmd(), textHandle, Number(1), Number(0));
        Execute(new SetImageOpacityCmd(), textHandle, Number(.2f), Number(2));
        Execute(new SetImagePositionCmd(), textHandle, Number(.75f), Number(.25f), Number(2));
        Execute(new SetImageAngleCmd(), textHandle, Number(180), Number(2));
        text.posProgress = text.eulerProgress = text.opacityProgress = 1f;
        text.ctrl.Refresh();
        Check(Mathf.Abs(text.ctrl.CurrentOpacity - .6f) < .001f
            && Mathf.Abs(Mathf.DeltaAngle(text.ctrl.rect.eulerAngles.z, 135)) < .001f
            && Vector3.Distance(text.ctrl.rect.position, Vector2.Lerp(text.oldPos, text.tarPos, .5f)) < .001f,
            "text shares timed position, angle and opacity interpolation");
        float removal = text.removeTime, position = text.posProgress, rotation = text.eulerProgress, opacity = text.opacityProgress;
        for (int i = 0; i < 5; i++) { text.ctrl.Refresh(); panel.Refresh(); }
        Check(text.removeTime == removal && text.posProgress == position && text.eulerProgress == rotation
            && text.opacityProgress == opacity, "Refresh/rebind never advances any transition or expiry timer");
        float delta = TimeManager.GetDeltaTime();
        text.ctrl.OnUpdate();
        Check(Mathf.Abs(text.posProgress - position - delta) < .0001f
            && Mathf.Abs(text.eulerProgress - rotation - delta) < .0001f
            && Mathf.Abs(text.opacityProgress - opacity - delta) < .0001f
            && Mathf.Abs(text.removeTime - removal + delta) < .0001f, "one OnUpdate consumes exactly one bounded frame step");
        Execute(new DeleteImageCmd(), imageHandle, Number(0));
        if (ImageUiItemForm.DataByUid.ContainsKey(image.uid)) image.ctrl.OnUpdate();
        Check(!ImageUiItemForm.DataByUid.ContainsKey(image.uid) && image.ctrl == null
            && ImageUiItemForm.DataByUid.ContainsKey(text.uid), "DeleteImage removes only its selected image and releases stale binding");
        text.removeTime = -.01f;
        text.ctrl.OnUpdate();
        Check(!ImageUiItemForm.DataByUid.ContainsKey(text.uid) && text.ctrl == null,
            "text duration expiry uses the same row-removal path");
    }

    private static void ImageToTextClearsBindings()
    {
        Clear();
        var image = Data(Execute(new ShowImageCmd(), Text("$i$" + TextureId + "$i$"), Number(1), Number(20)).res[0]);
        var ctrl = image.ctrl;
        ctrl.view.img_.overrideSprite = ctrl.view.img_.sprite;
        assets.Remove(image.uid);
        Check(ctrl.view.img_.sprite == null && ctrl.view.img_.overrideSprite == null && !ctrl.view.img_.enabled,
            "returning an image row to the FIFO pool clears its old graphic binding");
        var text = Data(Execute(new ShowTextCmd(), Text("after image"), Number(1), Number(20)).res[0]);
        Check(text.ctrl.view.img_.sprite == null
            && text.ctrl.view.img_.overrideSprite == null && !text.ctrl.view.img_.enabled,
            "image-to-text pool reuse clears Sprite and override instead of leaving a white/old image");
        Check(typeof(Img).GetField("_texData", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(text.ctrl.view.img_) == null
            && typeof(Img).GetField("_animationCoroutine", BindingFlags.NonPublic | BindingFlags.Instance).GetValue(text.ctrl.view.img_) == null,
            "a text row retains no texture binding or image animation coroutine");
        assets.Remove(text.uid);
        var missing = assets.Add(799999, 1);
        var missingData = ImageUiItemForm.DataByUid[missing];
        Check(missingData.ctrl.view.img_.sprite == null && !missingData.ctrl.view.img_.enabled
            && !missingData.ctrl.view.txt_.enabled, "a missing image stays blank without stale text or a white Image");
        Clear();
    }

    private static void PersistenceAndLifetime()
    {
        var old = ImageUiItemForm.GetJoByData(ImageUiItemForm.defaultData);
        old.Remove("isText"); old.Remove("text"); old.Remove("textScale");
        var legacy = ImageUiItemForm.GetDataByJo(old);
        Check(!legacy.isText && legacy.text == "" && legacy.textScale == 1,
            "old image JSON defaults to image-kind with empty text and unit scale");
        var text = Data(Execute(new ShowTextCmd(), Text("saved\nraw text"), Number(2), Number(15)).res[0]);
        var image = Data(Execute(new ShowImageCmd(), Text("$i$" + TextureId + "$i$"), Number(1), Number(15)).res[0]);
        var json = ImageUiItemForm.GetJoByData(text);
        var copied = ImageUiItemForm.GetDataByJo(json);
        Check(copied.isText && copied.text == text.text && copied.textScale == 2 && copied.ctrl == null && json["ctrl"] == null,
            "new text fields round-trip while the runtime controller stays unsaved");
        int textUid = text.uid, imageUid = image.uid;
        assets.End();
        Check(text.ctrl == null && image.ctrl == null && !panel.active,
            "scene End hides the overlay and detaches every runtime row binding");
        scratch = Path.Combine(Path.GetTempPath(), "PlayAssetTextRegression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(scratch);
        var save = new GameSaveController(game);
        save.SaveUiItem(scratch);
        save.LoadUiItem(scratch + Path.DirectorySeparatorChar);
        var loadedText = ImageUiItemForm.DataByUid[textUid];
        var loadedImage = ImageUiItemForm.DataByUid[imageUid];
        Check(!ReferenceEquals(loadedText, text) && loadedText.isText && loadedText.text == "saved\nraw text"
            && loadedText.textScale == 2 && !loadedImage.isText && loadedText.ctrl == null,
            "actual temporary SaveUiItem/LoadUiItem preserves mixed kinds and has no stale controllers");
        assets.Begin();
        Check(loadedText.ctrl != null && loadedImage.ctrl != null && loadedText.ctrl.view.txt_.fontSize == 120
            && loadedText.ctrl.view.txt_.text == loadedText.text && loadedText.ctrl.CurrentOpacity == 1,
            "reopening restores saved text with original-relative scale, content and opacity");
        Clear();
    }

    private static void LegacyOpacityCompatibility()
    {
        assets.End();
        var rows = new JArray();
        for (int i = 0; i < 5; i++)
        {
            var row = ImageUiItemForm.GetJoByData(ImageUiItemForm.defaultData);
            row["uid"] = 4001 + i;
            row["tex"] = TextureId;
            row["oldOpacity"] = i == 1 ? 1f : 0f;
            row["tarOpacity"] = 0f;
            row["opacityTime"] = i == 2 ? 2f : 0f;
            if (i < 3)
            {
                row.Remove("isText"); row.Remove("text"); row.Remove("textScale");
            }
            else
            {
                row["isText"] = i == 4;
                row["text"] = "transparent text";
            }
            rows.Add(row);
        }
        var save = new GameSaveController(game);
        string path = Path.Combine(scratch, save.imageUiItemFormFileName);
        string original = rows.ToString();
        Z_UnitSystem.SaveAndLoad.Save(path, original);
        save.LoadUiItem(scratch + Path.DirectorySeparatorChar);
        Check(ImageUiItemForm.DataByUid[4001].oldOpacity == 1 && ImageUiItemForm.DataByUid[4001].tarOpacity == 1,
            "legacy untouched zero-opacity fields retain the original opaque appearance");
        foreach (int id in new[] { 4002, 4003, 4004, 4005 })
            Check(ImageUiItemForm.DataByUid[id].tarOpacity == 0,
                "legacy explicit/fading and new-schema transparent records are never made opaque");
        Check(File.ReadAllText(path) == original, "legacy compatibility loading never rewrites the source file");
        assets.Begin();
        Check(ImageUiItemForm.DataByUid[4001].ctrl.CurrentOpacity == 1
            && ImageUiItemForm.DataByUid[4002].ctrl.CurrentOpacity == 0
            && ImageUiItemForm.DataByUid[4005].ctrl.CurrentOpacity == 0,
            "native image/text graphics honor migrated and deliberate opacity values");
        assets.End();
        save.SaveUiItem(scratch);
        save.LoadUiItem(scratch + Path.DirectorySeparatorChar);
        Check(ImageUiItemForm.DataByUid[4001].tarOpacity == 1 && ImageUiItemForm.DataByUid[4004].tarOpacity == 0,
            "normal save/load is idempotent after legacy opacity normalization");
        Clear();
    }

    private static void Clear()
    {
        foreach (int uid in ImageUiItemForm.DataByUid.Keys.ToArray()) assets.Remove(uid);
    }
    private static ImageUiItemForm.Data Data(BoxDataForm.Data handle)
    {
        Check(handle?.str != null && handle.str.StartsWith(GlobalEventHelper.UIIMAGE, StringComparison.Ordinal),
            "Show command returns the shared UI overlay handle");
        int uid = GlobalEventHelper.GetId(handle.str, GlobalEventHelper.UIIMAGE);
        Check(ImageUiItemForm.DataByUid.TryGetValue(uid, out var data) && data.ctrl != null,
            "returned handle resolves to a registered, natively rendered row");
        return data;
    }
    private static InterpretAsyncTask Execute(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments.Reverse().ToArray(), new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && task.error == null, command.GetName() + " completes synchronously: " + task.error);
        return task;
    }
    private static ProgramDataForm.Data Compile(string source)
    {
        var program = ProgramDataForm.defaultData;
        program.name = "play-asset-text-regression";
        Check(program.TryApplyCode(source, out _, out var errors), "source compiles: " + string.Join(";", errors));
        return program;
    }
    private static BoxDataForm.Data Run(ProgramDataForm.Data program)
    {
        var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            program, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = execution.Interpret();
        Check(result.complete && result.errors.Count == 0 && execution.stack.Count == 0,
            "ShowText executes with a balanced interpreter stack: " + string.Join(";", result.errors));
        return result.ret;
    }
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}

[ExecuteAlways]
public class RegressionPlayAssetTextHolder : UiHolder { }
