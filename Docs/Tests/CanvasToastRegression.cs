using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using TMPro;
using UnityEditor;
using UnityEngine;
using UnityEngine.UI;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_UnitSystem;
using Object = UnityEngine.Object;

// Native Canvas/TMP, frame timers and instance pools in a disposable Play project.
public static class CanvasToastRegression
{
    private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags HiddenStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static InstancePoolManager pools;
    private static PlaySceneEffectController effects;
    private static GameObject prefab;
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
        try { SetUp(); TimeManager.instance.StartCoroutine(Guard(Checks())); }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void SetUp()
    {
        foreach (var type in new[] { typeof(ProgressForm), typeof(CharacterParamForm), typeof(CharacterUnitForm) }) Register(type);
        Manager<GameManager>();
        var progress = ProgressForm.defaultData;
        progress.uid = 1; progress.cameraMode = CameraMode.Overhead; ProgressForm.AddData(progress);
        var play = Manager<PlayManager>();
        var time = Manager<TimeManager>(); time.gameObject.SetActive(true);
        Time.captureDeltaTime = .05f; Time.timeScale = 1;
        pools = Manager<InstancePoolManager>();
        pools.defaultRoot = new GameObject("toast-instances").transform; pools.Init();

        var settings = ScriptableObject.CreateInstance<TMP_Settings>();
        typeof(TMP_Settings).GetField("s_Instance", HiddenStatic).SetValue(null, settings);
        var font = AssetDatabase.LoadAssetAtPath<Font>("Assets/PlayAssetTextShaders/LiberationSans.ttf");
        var fontAsset = TMP_FontAsset.CreateFontAsset(font);
        Check(fontAsset != null, "fixture creates a native TMP font");
        typeof(TMP_Settings).GetField("m_defaultFontAsset", Hidden).SetValue(settings, fontAsset);
        typeof(TMP_Settings).GetField("m_fallbackFontAssets", Hidden).SetValue(settings, new List<TMP_FontAsset>());

        prefab = new GameObject(MapInfo.GetPrefabName("canvas"), typeof(RectTransform)); prefab.SetActive(false);
        prefab.AddComponent<Canvas>().renderMode = RenderMode.WorldSpace;
        var holder = prefab.AddComponent<CanvasHolder>();
        holder.textProto = holder.toastTextProto = Text("toast-template", prefab.transform, fontAsset);
        holder.chat = Child("chat", prefab.transform); holder.chat.SetActive(false);
        holder.dirTrs = Child("direction", prefab.transform).transform; holder.dirTrs.gameObject.SetActive(false);
        var slider = Child("slider-template", prefab.transform); slider.SetActive(false);
        holder.sliderProto = slider.AddComponent<Slider>(); Text("slider-text", slider.transform, fontAsset).gameObject.SetActive(true);
        pools.AddPool(prefab);
        effects = new PlaySceneEffectController(play); effects.Begin();
    }

    private static IEnumerator Checks()
    {
        var holder = pools.CreateInstance(prefab).GetComponent<CanvasHolder>();
        int completed = 0;
        holder.ToastText("old-a", 2, () => completed++);
        holder.ToastText("old-b", 2, () => completed++);
        holder.ShowText("parameter"); holder.ShowSlider(3, 10);
        holder.chat.SetActive(true); holder.SetDirection(Vector3.one, 30);
        holder.chatTimer = TimeManager.instance.StartFrameTimer(() => false, holder);
        var chatTimer = holder.chatTimer;
        for (int i = 0; i < 10; i++) yield return null;
        var oldToasts = Toasts(holder).Keys.Cast<GameObject>().ToArray();
        var oldTimers = Toasts(holder).Values.Cast<Timer>().ToArray();
        Check(oldToasts.Length == 2, "multiple toasts are tracked independently");
        holder.Reset(); holder.Reset();
        Check(Toasts(holder).Count == 0 && oldTimers.All(timer => timer.cancel), "Reset cancels and forgets all toast timers idempotently");
        Check(oldToasts.All(go => go == null || !go.activeSelf), "Reset hides toasts immediately before deferred Destroy");
        Check(chatTimer.cancel && holder.chatTimer == null && !holder.chat.activeSelf && !holder.dirTrs.gameObject.activeSelf,
            "Reset also clears chat and direction");
        Check(holder.GetComponentsInChildren<TextMeshProUGUI>().Length == 0, "parameter and Slider roots also hide before same-frame reuse");

        holder.ToastText("disabled", 2, () => completed++);
        var disabledToast = Toasts(holder).Keys.Cast<GameObject>().Single();
        var disabledTimer = Toasts(holder).Values.Cast<Timer>().Single();
        holder.gameObject.SetActive(false);
        Check(disabledTimer.cancel && Toasts(holder).Count == 0 && !disabledToast.activeSelf, "OnDisable cancels and hides unfinished toast");
        holder.gameObject.SetActive(true);
        holder.ToastText("new lease", 2, () => completed++);
        for (int i = 0; i < 5; i++) yield return null;
        Check(completed == 0 && Toasts(holder).Count == 1, "canceled old callbacks cannot complete or alter a new lease");
        Object.Destroy(Toasts(holder).Keys.Cast<GameObject>().Single());
        for (int i = 0; i < 3; i++) yield return null;
        Check(completed == 1 && Toasts(holder).Count == 0, "external child destruction retires the live toast once");
        pools.DeleteInstance(holder.gameObject, prefab);

        effects.FloatingText("slow-frame", Vector3.one);
        var slow = Floating().Keys.Cast<CanvasHolder>().Single();
        float start = Time.time;
        for (int i = 0; i < 62; i++) yield return null;
        Check(Time.time - start >= 3f && slow.gameObject.activeSelf && Toasts(slow).Count == 1,
            "20 FPS: no fixed-three-second recycle before bounded animation completes");
        for (int i = 0; i < 25; i++) yield return null;
        Check(!slow.gameObject.activeSelf && Floating().Count == 0 && Toasts(slow).Count == 0,
            "slow-frame toast completes and returns its clean Canvas");

        var character = CharacterUnitForm.defaultData; character.uid = 701;
        var attached = effects.GetCanvas(character);
        attached.ToastText("removed character", 2);
        var removedTimer = Toasts(attached).Values.Cast<Timer>().Single();
        effects.OnEvent(new CharacterEvent { type = MapEventType.Remove, unit = new CharacterUnit(character) });
        Check(!attached.gameObject.activeSelf && removedTimer.cancel && Toasts(attached).Count == 0,
            "Character Remove immediately clears its attached Canvas and unfinished toast");

        for (int i = 0; i < 3; i++) effects.FloatingText("scene-ending-" + i, Vector3.zero);
        var ending = Floating().Keys.Cast<CanvasHolder>().ToArray();
        var endingTimers = ending.SelectMany(canvas => Toasts(canvas).Values.Cast<Timer>()).ToArray();
        effects.End(); effects.End();
        Check(Floating().Count == 0 && ending.All(canvas => !canvas.gameObject.activeSelf && Toasts(canvas).Count == 0)
            && endingTimers.All(timer => timer.cancel), "scene End releases all standalone toast Canvases idempotently");
        effects.FloatingText("late call after End", Vector3.zero);
        Check(Floating().Count == 0, "late calls cannot spawn after scene End");
        effects.Begin(); effects.FloatingText("new scene", Vector3.zero);
        var next = Floating().Keys.Cast<CanvasHolder>().Single();
        for (int i = 0; i < 5; i++) yield return null;
        Check(next.gameObject.activeSelf && Toasts(next).Count == 1, "previous scene callbacks cannot reclaim the new scene Canvas");
        effects.End(); effects.Begin();

        effects.FloatingText("external recycle", Vector3.zero);
        var external = Floating().Keys.Cast<CanvasHolder>().Single();
        pools.DeleteInstance(external.gameObject, prefab);
        var rented = new List<CanvasHolder>();
        CanvasHolder externalLease = null;
        // The pool may have several available Canvases; rent through the queue.
        for (int i = 0; i < 10 && externalLease == null; i++)
        {
            var lease = pools.CreateInstance(prefab).GetComponent<CanvasHolder>(); rented.Add(lease);
            if (lease == external) externalLease = lease;
        }
        Check(externalLease != null, "fixture reuses the externally recycled Canvas");
        externalLease.ToastText("unrelated new owner", 100);
        effects.End();
        Check(externalLease.gameObject.activeSelf && Toasts(externalLease).Count == 1,
            "scene cleanup cannot reclaim a Canvas already reused by an unrelated owner");
        foreach (var lease in rented) pools.DeleteInstance(lease.gameObject, prefab);

        Time.captureDeltaTime = 1f / 60f;
        effects.Begin(); effects.FloatingText("normal-frame", Vector3.zero);
        var normal = Floating().Keys.Cast<CanvasHolder>().Single();
        for (int i = 0; i < 100; i++) yield return null;
        Check(!normal.gameObject.activeSelf && Toasts(normal).Count == 0 && Floating().Count == 0,
            "60 FPS retains ordinary animation completion and clean pool return");
        effects.End();
    }

    private static IDictionary Toasts(CanvasHolder holder) => (IDictionary)typeof(CanvasHolder).GetField("toastTimers", Hidden).GetValue(holder);
    private static IDictionary Floating() => (IDictionary)typeof(PlaySceneEffectController).GetField("floatingCanvases", Hidden).GetValue(effects);
    private static GameObject Child(string name, Transform parent)
    {
        var go = new GameObject(name, typeof(RectTransform)); go.transform.SetParent(parent, false); return go;
    }
    private static TextMeshProUGUI Text(string name, Transform parent, TMP_FontAsset font)
    {
        var go = Child(name, parent); go.SetActive(false);
        var text = go.AddComponent<TextMeshProUGUI>(); text.font = font; text.text = ""; return text;
    }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject("toast-regression-" + typeof(T).Name); root.SetActive(false);
        var value = root.AddComponent<T>(); typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, value); return value;
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
        Time.captureDeltaTime = 0;
        Debug.Log("CANVAS_TOAST_REGRESSION_PASS checks=" + checks); EditorApplication.Exit(0);
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message); checks++;
    }
}
