using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;

// Real effect controller, GIF decoder, frame timer, material routing and pool.
// Runs in an isolated Play-mode project; never loads a production scene/save.
public static class EffectAnimationRegression
{
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static InstancePoolManager pools;
    private static PlaySceneEffectController effects;
    private static TexAssetForm.Data animated;
    private static Texture2D staticTexture;
    private static GameObject template;
    private static int checks, nextUid = 98000;
    private static readonly MaterialPropertyBlock block = new MaterialPropertyBlock();

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
            SetUp();
            TimeManager.instance.StartCoroutine(Guard(Checks()));
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void SetUp()
    {
        foreach (var type in new[] { typeof(EffectForm), typeof(EffectClipForm), typeof(ProgressForm), typeof(TexAssetForm) }) Register(type);
        var game = Manager<GameManager>();
        var progress = ProgressForm.defaultData; progress.uid = 1; progress.cameraMode = CameraMode.Overhead; ProgressForm.AddData(progress);
        var play = Manager<PlayManager>();
        var time = Manager<TimeManager>(); time.gameObject.SetActive(true);
        Time.captureDeltaTime = .02f;
        pools = Manager<InstancePoolManager>(); pools.defaultRoot = new GameObject("effect-instances").transform; pools.Init();
        string path = Path.Combine(Application.dataPath, "../EffectGif.gif");
        animated = new TexAssetForm.Data(8531, "sleep-effect-gif", "", null, "", null, 0);
        if (File.Exists(path)) animated.bytes = File.ReadAllBytes(path);
        else animated.SetRuntimeAnimationFrames(new[]
        {
            new AnimatedFrameData { texture = new Texture2D(2, 2), delaySeconds = .08f },
            new AnimatedFrameData { texture = new Texture2D(2, 2), delaySeconds = .16f },
            new AnimatedFrameData { texture = new Texture2D(2, 2), delaySeconds = .12f }
        });
        TexAssetForm.AddData(animated);
        var frames = animated.GetAnimationFrames();
        Check(frames != null && frames.Count > 1, "effect asset decodes multiple internal frames");
        Debug.Log("EFFECT_GIF_SOURCE frames=" + frames.Count + " duration=" + animated.GetAnimationDuration() + " actualGif=" + animated.isGif);
        staticTexture = new Texture2D(2, 2);
        TexAssetForm.AddData(new TexAssetForm.Data(8532, "static-effect", "", null, "", staticTexture, 0));
        template = GameObject.CreatePrimitive(PrimitiveType.Quad); template.name = MapInfo.GetPrefabName("img"); template.SetActive(false);
        var material = new Material(Shader.Find("DisFadeCode"));
        material.SetFloat("_Alpha", 1); material.SetFloat("_Show", 1); material.SetFloat("_FadeCenter", 0);
        template.GetComponent<Renderer>().sharedMaterial = material;
        DisFadeRenderRouting.SetTemplateTexture(template.GetComponent<Renderer>(), frames[0].texture);
        template.AddComponent<ImageHolder>(); pools.AddPool(template);
        effects = new PlaySceneEffectController(play);
    }

    private static IEnumerator Checks()
    {
        int sleep = AddEffect(new EffectClipForm.Data(-1, 8531, 7f, new Vector3(0, .3f, 0), 0, Vector3.one, 1, false));
        var image = Spawn(sleep);
        var first = animated.GetAnimationTexture(0);
        Check(TextureOf(image) == first, "frame zero is displayed synchronously");
        Check(image.animTimer.bind == image, "the variable-frame timer is owned by the pooled image");
        Check(image.trs.position == new Vector3(2, 3.3f, 4) && image.trs.localScale == Vector3.one, "animated effects preserve authored position/scale");
        var seen = new HashSet<Texture>(); bool looped = false, leftFirst = false;
        float start = Time.time;
        while (Time.time - start < 7.2f)
        {
            if (image.gameObject.activeSelf)
            {
                var texture = TextureOf(image); seen.Add(texture);
                leftFirst |= texture != first;
                looped |= leftFirst && texture == first;
                if (Time.time - start > 1f && Time.time - start < 6.9f)
                    Check(image.gameObject.activeSelf, "the GIF does not shorten its authored seven-second clip");
            }
            yield return null;
        }
        Check(seen.Count > 1 && looped, "sleep animates and repeats its internal cycle during the seven-second effect");
        Check(!image.gameObject.activeSelf, "the effect still returns to its pool after its authored duration");

        int transition = AddEffect(new EffectClipForm.Data(-1, 8531, .5f, Vector3.zero, 0, Vector3.one, 1, true),
            new EffectClipForm.Data(-1, 8532, .35f, new Vector3(1, 0, 0), 0, Vector3.one * 2, .2f, false));
        image = Spawn(transition); start = Time.time;
        while (Time.time - start < .25f) yield return null;
        image.render.GetPropertyBlock(block);
        Check(block.GetFloat("_Alpha") > .3f && block.GetFloat("_Alpha") < .9f, "GIF frames coexist with opacity interpolation");
        while (Time.time - start < .65f) yield return null;
        Check(image.gameObject.activeSelf && TextureOf(image) == staticTexture, "switching clips replaces the GIF with the next static texture");
        Check(image.trs.position == new Vector3(3, 3, 4) && image.trs.localScale == Vector3.one * 2, "the next clip retains its authored transforms");
        while (Time.time - start < 1f) yield return null;
        Check(!image.gameObject.activeSelf, "the static ending clip completes normally");

        int plain = AddEffect(new EffectClipForm.Data(-1, 8532, .4f, Vector3.zero, 0, Vector3.one, 1, false));
        image = Spawn(sleep); var oldTimer = image.animTimer;
        pools.DeleteInstance(image.gameObject, template);
        var reused = Spawn(plain);
        Check(reused == image && oldTimer.cancel, "pool reuse cancels the previous effect timer");
        start = Time.time;
        while (Time.time - start < .2f) yield return null;
        Check(TextureOf(reused) == staticTexture && reused.gameObject.activeSelf, "an old GIF callback cannot overwrite a reused static effect");
        while (Time.time - start < .6f) yield return null;
        Check(!reused.gameObject.activeSelf, "reused static effects expire correctly");
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
        Debug.Log("EFFECT_ANIMATION_REGRESSION_PASS checks=" + checks);
        EditorApplication.Exit(0);
    }
    private static int AddEffect(params EffectClipForm.Data[] clips)
    {
        int uid = nextUid++;
        EffectForm.AddData(new EffectForm.Data(uid, "effect-animation-" + uid, 0, new List<List<EffectClipForm.Data>> { new List<EffectClipForm.Data>(clips) }, false));
        return uid;
    }
    private static ImageHolder Spawn(int uid)
    {
        ImageHolder result = null;
        effects.CreatEffect(uid, new Vector3(2, 3, 4), 0, image => result = image);
        Check(result != null, "actual controller creates and binds an effect immediately");
        return result;
    }
    private static Texture TextureOf(ImageHolder image) => image.render.sharedMaterial.GetTexture("_Tex");
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject("effect-regression-" + typeof(T).Name); root.SetActive(false);
        var manager = root.AddComponent<T>(); typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, manager); return manager;
    }
    private static void Register(Type type)
    {
        if (!forms.Add(type)) return;
        var parent = type.GetNestedType("Data")?.BaseType?.DeclaringType; if (parent != null) Register(parent);
        type.GetMethod("Register", HiddenStatic)?.Invoke(null, null);
        type.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
