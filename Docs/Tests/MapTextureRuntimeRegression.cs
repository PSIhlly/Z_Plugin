using System;
using System.Collections;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_Time;
using Z_Texture;
using Z_UnitSystem.Form;

// Isolated Editor fixture; never load the real game, story, or saves.
public static class MapTextureRuntimeRegression
{
    private const BindingFlags PrivateStatic = BindingFlags.NonPublic | BindingFlags.Static;
    private const BindingFlags PrivateInstance = BindingFlags.NonPublic | BindingFlags.Instance;
    private static int checks;
    private static readonly MaterialPropertyBlock readBlock = new MaterialPropertyBlock();
    private static readonly int TextureId = Shader.PropertyToID("_Tex");
    private static readonly int LegacyMaskId = Shader.PropertyToID("_AlphaTex");
    private static Material textureTemplate;

    public static void RunMaterialBindingLifecycle()
    {
        var a = new Texture2D(1, 1);
        var b = new Texture2D(1, 1);
        try
        {
            textureTemplate = new Material(Shader.Find("DisFadeCode"));
            textureTemplate.SetFloat("_Alpha", 1f);
            textureTemplate.SetFloat("_Show", 1f);
            textureTemplate.SetTexture(TextureId, a);
            RepeatedActiveBindingRebind(a, b);
            SourceMaterialSnapshots(a, b);
            TemplateTexturePoolLifecycle(a, b);
            ExternalCachedMaterialChanges(a, b);
            Debug.Log("MAP_MATERIAL_BINDING_LIFECYCLE_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
        finally
        {
            DisFadeRenderRouting.Clear();
            if (textureTemplate != null) UnityEngine.Object.DestroyImmediate(textureTemplate);
            UnityEngine.Object.DestroyImmediate(a);
            UnityEngine.Object.DestroyImmediate(b);
        }
    }

    public static void RunDeferredVisibilityPlayLifecycle()
    {
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload
            | EnterPlayModeOptions.DisableSceneReload;
        EditorApplication.playModeStateChanged += OnDeferredVisibilityPlayMode;
        EditorApplication.EnterPlaymode();
    }

    private static void OnDeferredVisibilityPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= OnDeferredVisibilityPlayMode;
        var a = new Texture2D(1, 1);
        var b = new Texture2D(1, 1);
        try
        {
            textureTemplate = new Material(Shader.Find("DisFadeCode"));
            textureTemplate.SetFloat("_Alpha", 1f);
            textureTemplate.SetFloat("_Show", 1f);
            textureTemplate.SetTexture(TextureId, a);
            DeferredVisibilityAndPoolReuse(a, b);
            Debug.Log("MAP_DEFERRED_VISIBILITY_LIFECYCLE_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    public static void RunCharacterPlayLifecycle()
    {
        // This entry point runs only in the isolated temporary project. Exercise
        // native MonoBehaviour callbacks, not the EditMode reflection surrogate.
        EditorSettings.enterPlayModeOptionsEnabled = true;
        EditorSettings.enterPlayModeOptions = EnterPlayModeOptions.DisableDomainReload
            | EnterPlayModeOptions.DisableSceneReload;
        EditorApplication.playModeStateChanged += OnCharacterPlayMode;
        EditorApplication.EnterPlaymode();
    }

    private static void OnCharacterPlayMode(PlayModeStateChange state)
    {
        if (state != PlayModeStateChange.EnteredPlayMode) return;
        EditorApplication.playModeStateChanged -= OnCharacterPlayMode;
        try
        {
            TexAssetForm.InitInternal();
            CharacterProductForm.InitInternal();
            CharacterAnimForm.InitInternal();
            CharacterAnimClipForm.InitInternal();
            CharacterUnitForm.InitInternal();
            CreateManager<TimeManager>();
            AddTexture(81001);
            AddTexture(81002);
            textureTemplate = new Material(Shader.Find("DisFadeCode"));
            textureTemplate.SetFloat("_Alpha", 1);
            textureTemplate.SetFloat("_Show", 1);
            CharacterTemplateFirstShow();
            Debug.Log("MAP_CHARACTER_PLAY_LIFECYCLE_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    public static void Run()
    {
        try
        {
            PerspectiveImageScaleAfterRotation();
            // RuntimeInitializeOnLoadMethod registration does not run in EditMode.
            TexAssetForm.InitInternal();
            MapTextureForm.InitInternal();
            MapMaskForm.InitInternal();
            var map = CreateManager<MapManager>();
            map.data = new MapInfo { maps = new Dictionary<(int, int, int), TileUnitForm.Data>() };
            map.utilCtrl = new MapUtilController(map);
            var time = CreateManager<TimeManager>();
            time.gameObject.SetActive(true);
            var ctrl = new GameMapController(null);
            ctrl.Reset();
            var a = AddTexture(81001);
            var b = AddTexture(81002);
            var mask = AddTexture(81003);
            var shader = Shader.Find("DisFadeCode");
            Check(shader != null, "texture fixture imports the real DisFadeCode shader");
            textureTemplate = new Material(shader) { name = "texture-template" };
            textureTemplate.SetFloat("_Alpha", 1f);
            textureTemplate.SetFloat("_Show", 1f);
            textureTemplate.SetTexture(TextureId, a);
            DisFadeRouting(ctrl, a, b, mask);
            SourceMaterialSnapshots(a, b);
            TemplateTexturePoolLifecycle(a, b);
            RepeatedActiveBindingRebind(a, b);
            UnifiedModelBindings();
            TileViewRefreshReuse(ctrl, a, b);
            ExternalCachedMaterialChanges(a, b);
            var replacement = new Texture2D(2, 2);
            var tile = NewTile(82001);
            var root = new GameObject("texture-regression");
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject("renderer-" + i);
                child.transform.SetParent(root.transform, false);
                child.AddComponent<MeshRenderer>().sharedMaterial = textureTemplate;
            }
            var ins = root.AddComponent<TileInstance>();
            tile.unit.ins = ins;
            ins.unit = tile.unit;
            var renderer = ins.viewRefreshBindings[0].Target;
            readBlock.SetFloat("_Show", 1f);
            readBlock.SetFloat("_FadeCenter", 1f);
            readBlock.SetFloat("_LightSensitivity", .3f);
            renderer.SetPropertyBlock(readBlock);

            var baseFrames = new List<int> { 81001 };
            var frontFrames = new List<int> { 81002 };
            var product = new MapTextureForm.Data(83001, "test", 81001, 0, baseFrames, 0,
                new Dictionary<string, EventTriggerForm.Data>(), false, new Dictionary<int, int>(), 0,
                true, frontFrames, false, new Dictionary<int, int>());
            MapTextureForm.DataById[product.id] = product;
            var maskProduct = new MapMaskForm.Data(83002, "mask", 81003, new List<int> { 81003 }, 0);
            MapMaskForm.DataById[maskProduct.id] = maskProduct;
            tile.texDic[0] = product.id;
            tile.texDic[3] = maskProduct.id;
            var evt = new TileEvent { type = MapEventType.Show, unit = tile.unit };
            ctrl.OnEvent(evt);
            Check(Texture(renderer, "_Tex") == a && Texture(renderer, "_AlphaTex") == null,
                "base texture uses a cached shared material and legacy masks are ignored");
            Check(Texture(ins.viewRefreshBindings[3].Target, "_Tex") == b, "front has its independent texture");
            Check(!ins.viewRefreshBindings[1].Target.enabled && !ins.viewRefreshBindings[4].Target.enabled, "empty base/front disabled");
            renderer.GetPropertyBlock(readBlock);
            Check(readBlock.GetFloat("_Show") == 1f && readBlock.GetFloat("_FadeCenter") == 1f
                && readBlock.GetFloat("_LightSensitivity") == .3f,
                "texture writes preserve camera-center fade, visibility and lighting");
            var blockField = typeof(DisFadeRenderRouting).GetField("propertyScratch", PrivateStatic);
            var scratch = blockField.GetValue(null);
            ctrl.OnEvent(evt);
            Check(ReferenceEquals(scratch, blockField.GetValue(null)), "scalar migration scratch block reused across all layers");

            tile.texDic.Remove(0);
            ctrl.OnEvent(evt);
            Check(!renderer.enabled && !ins.viewRefreshBindings[3].Target.enabled, "retained mask cannot enable removed layer");
            tile.texDic[0] = product.id;
            tile.texDic.Remove(3);
            ctrl.OnEvent(evt);
            Check(renderer.enabled && Texture(renderer, "_AlphaTex") == null,
                "removing an ignored mask does not change the enabled base layer");
            product.enableFrontPart = false;
            ctrl.OnEvent(evt);
            Check(!ins.viewRefreshBindings[3].Target.enabled, "disabled front clears previous active state");
            product.enableFrontPart = true;
            ins.viewRefreshBindings[3].Target.gameObject.SetActive(false);
            ctrl.OnEvent(evt);
            Check(ins.viewRefreshBindings[3].Target.gameObject.activeSelf && ins.viewRefreshBindings[3].Target.enabled, "inactive authored front activates");

            var mixed = new List<int> { 89999, 81001, 81002 };
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == a, "missing frames skipped in source order");
            mixed[1] = 81002;
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == b, "same-length in-place edits invalidate cached frames");
            var removed = TexAssetForm.DataById[81002];
            TexAssetForm.DataById.Remove(81002);
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(renderer.enabled && Texture(renderer, "_Tex") != b, "all missing frames use default, not stale texture");
            TexAssetForm.DataById[81002] = removed;
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == b, "restored IDs invalidate filtered frame cache");
            removed.asset = replacement;
            ctrl.ShowFinalMat(ins, 0, mixed, 0);
            Check(Texture(renderer, "_Tex") == replacement, "same-ID asset replacement is not texture-cached");
            removed.asset = b;

            product.isWangTile = true;
            product.frontIsWangTile = true;
            product.WangTileDic[0] = 81002;
            product.FrontWangTileDic[0] = 81001;
            ctrl.OnEvent(evt);
            Check(Texture(renderer, "_Tex") == b && Texture(ins.viewRefreshBindings[3].Target, "_Tex") == a,
                "single Wang variants use independent inline IDs without lists");
            var register = typeof(GameMapController).GetMethod("RegisterWangTileAnimationTexture", PrivateStatic);
            register.Invoke(null, new object[] { product.id, false, 0, 81001 });
            register.Invoke(null, new object[] { product.id, false, 0, 81002 });
            ctrl.OnEvent(evt);
            Check(Texture(renderer, "_Tex") == a, "registered Wang sequence selected before single variant");

            // Warm the complete static Tile Show route and filtered sequence route.
            for (int i = 0; i < 20; i++) { ctrl.OnEvent(evt); ctrl.ShowFinalMat(ins, 0, mixed, 0); }
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int i = 0; i < 100; i++) { ctrl.OnEvent(evt); ctrl.ShowFinalMat(ins, 0, mixed, 0); }
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Check(allocated == 0, "warmed static/Wang/missing-frame display paths allocate zero managed bytes: " + allocated);

            // Drive the exact callback helper directly in EditMode; real scheduling
            // still requires Play-mode smoke testing.
            var framesType = typeof(GameMapController).GetNestedType("TextureFrames", BindingFlags.NonPublic);
            var animation = new List<int> { 81001, 81002 };
            var frames = Activator.CreateInstance(framesType, new object[] { animation });
            var tick = typeof(GameMapController).GetMethod("UpdateTextureAnimation", PrivateInstance);
            var frameIndices = new Dictionary<int, int>();
            TimeManager.animationTimeGetter = () => 1.25f;
            tile.texDic[3] = maskProduct.id;
            product.animTimeInterval = .5f;
            ctrl.OnEvent(evt);
            Check(ins.animTimer[0] != null && !ins.animTimer[0].cancel
                && Texture(renderer, "_Tex") == a && Texture(renderer, "_AlphaTex") == null,
                "Tile Show retains shared-phase animation without a legacy mask MPB");
            ctrl.ShowFinalMat(ins, 0, animation, .5f);
            var timer = ins.animTimer[0];
            ctrl.ShowFinalMat(ins, 0, maskProduct.texsName, 0, true);
            Check(timer != null && !timer.cancel && ReferenceEquals(timer, ins.animTimer[0]),
                "mask-only update keeps the running base animation");
            TimeManager.animationTimeGetter = () => 1.75f;
            tick.Invoke(ctrl, new object[] { ins, tile.unit, renderer, 0, frames, .5f, frameIndices });
            Check(Texture(renderer, "_Tex") == b && Texture(renderer, "_AlphaTex") == null,
                "animation selects the shared-clock material frame without a mask MPB");
            renderer.GetPropertyBlock(readBlock);
            Check(readBlock.GetFloat("_Show") == 1f && readBlock.GetFloat("_FadeCenter") == 1f
                && ReferenceEquals(scratch, blockField.GetValue(null)),
                "animation reuses block and preserves camera-center fade");
            animation[1] = 81001;
            tick.Invoke(ctrl, new object[] { ins, tile.unit, renderer, 0, frames, .5f, frameIndices });
            Check(Texture(renderer, "_Tex") == a, "animation observes edits even at the same clock frame");

            AnimatedAssets(ctrl, ins, tick, framesType, frameIndices);
            CharacterAnimatedAssets(time);
            CharacterTemplateFirstShow();
            AnimatedWangTileAssets();

            var newTile = NewTile(82002);
            ins.unit = newTile.unit;
            renderer.SetPropertyBlock(null);
            Check((bool)tick.Invoke(ctrl, new object[] { ins, tile.unit, renderer, 0, frames, .5f, frameIndices }),
                "old-owner animation terminates after pool rebind");
            ctrl.ShowFinalMat(ins, 0, frontFrames, 0);
            Check(timer.cancel && ins.animTimer[0] == null && Texture(renderer, "_Tex") == b,
                "pool rebind restores texture and cancels old animation");
            ctrl.ShowFinalMat(ins, 0, null, 0);
            ctrl.ShowFinalMat(ins, 0, maskProduct.texsName, 0, true);
            Check(!renderer.enabled && Texture(renderer, "_AlphaTex") == null,
                "ignored mask-only path keeps empty pooled layer disabled");
            var cache = (IDictionary)typeof(GameMapController).GetField("validatedTextureFrames", PrivateStatic).GetValue(null);
            Check(cache.Count > 0, "multi-frame validation cache populated");
            ctrl.Reset();
            Check(cache.Count == 0 && ctrl.animCurCache.Count == 0, "reset releases frame caches between stories");
            TimeManager.animationTimeGetter = null;
            Debug.Log("MAP_TEXTURE_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static TexAssetForm.Data AddGif(int id)
    {
        // Real two-frame 1x1 GIF89a: red for 100ms, blue for 200ms.
        var bytes = new List<byte> { 71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 128, 0, 0,
            255, 0, 0, 0, 0, 255 };
        foreach (int frame in new[] { 0, 1 })
        {
            bytes.AddRange(new byte[] { 33, 249, 4, 0, (byte)(frame == 0 ? 10 : 20), 0, 0, 0,
                44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 2, 2, (byte)(frame == 0 ? 68 : 76), 1, 0 });
        }
        bytes.Add(59);
        var data = new TexAssetForm.Data(id, "two-frame-gif", "", bytes.ToArray(), "", null, 0);
        TexAssetForm.DataById[id] = data;
        return data;
    }

    private static void AnimatedAssets(GameMapController ctrl, TileInstance ins, MethodInfo tick,
        Type framesType, Dictionary<int, int> indices)
    {
        var gif = AddGif(81004);
        var decoded = gif.GetAnimationFrames();
        Check(decoded.Count == 2 && decoded[0].texture.GetPixel(0, 0).r > .9f
            && decoded[1].texture.GetPixel(0, 0).b > .9f, "real GIF decodes distinct composited frames");
        double cycle = gif.GetAnimationDuration();
        var ids = new List<int> { 81004, 81002 };
        Check(TextureAnimation.GetSample(ids, .5f, .05).texture == decoded[0].texture, "GIF first-frame delay retained");
        Check(TextureAnimation.GetSample(ids, .5f, .15).texture == decoded[1].texture, "GIF second-frame delay retained");
        Check(TextureAnimation.GetSample(ids, .5f, cycle + .49).texture == decoded[1].texture,
            "after the GIF cycle, interval holds the last frame rather than replaying it");
        Check(TextureAnimation.GetSample(ids, .5f, cycle + .51).texture == TexAssetForm.DataById[81002].GetTex(),
            "next tex starts only after GIF duration plus interval");
        Check(TextureAnimation.GetSample(ids, .5f, cycle + 1.01).texture == decoded[0].texture,
            "mixed GIF/static clip loops using variable entry durations");
        Check(TextureAnimation.GetSample(81004, 0, cycle + .05).texture == decoded[0].texture,
            "single GIF with zero interval still animates and loops");
        Check(TextureAnimation.GetSample(81004, .5f, cycle + .49).texture == decoded[1].texture,
            "single GIF also waits interval between cycles");
        Check(TextureAnimation.GetSample(ids, 0, cycle + .05).texture == decoded[0].texture,
            "zero-duration static entries cannot stall a mixed clip");
        Check(TextureAnimation.GetSample(new[] { 81001, 81002 }, 0, 100).texture == TexAssetForm.DataById[81001].GetTex(),
            "static-only zero interval retains original first-tex behavior");

        var frames = Activator.CreateInstance(framesType, new object[] { ids });
        var renderer = ins.viewRefreshBindings[0].Target;
        TimeManager.animationTimeGetter = () => .15f;
        ctrl.ShowFinalMat(ins, 0, ids, .5f);
        Check(ins.animTimer[0] != null && Texture(renderer, "_Tex") == decoded[1].texture,
            "new map instance joins the internal GIF's shared phase immediately");
        TimeManager.animationTimeGetter = () => (float)(cycle + .49);
        tick.Invoke(ctrl, new object[] { ins, ins.unit, renderer, 0, frames, .5f, indices });
        Check(Texture(renderer, "_Tex") == decoded[1].texture, "map tick holds GIF last frame through interval");
        TimeManager.animationTimeGetter = () => (float)(cycle + .51);
        tick.Invoke(ctrl, new object[] { ins, ins.unit, renderer, 0, frames, .5f, indices });
        Check(Texture(renderer, "_Tex") == TexAssetForm.DataById[81002].GetTex()
            && Texture(renderer, "_AlphaTex") == null, "map GIF transition keeps legacy masks out of MPB");
        var beforeTexture = Texture(renderer, "_Tex");
        tick.Invoke(ctrl, new object[] { ins, ins.unit, renderer, 0, frames, .5f, indices });
        Check(Texture(renderer, "_Tex") == beforeTexture, "paused game clock freezes nested GIF playback");

        var replacement = AddGif(81004);
        var replacementFrames = replacement.GetAnimationFrames();
        TimeManager.animationTimeGetter = () => .15f;
        tick.Invoke(ctrl, new object[] { ins, ins.unit, renderer, 0, frames, .5f, indices });
        Check(Texture(renderer, "_Tex") == replacementFrames[1].texture, "same-ID GIF replacement uses new decoded frames");
        ids[0] = 81001;
        tick.Invoke(ctrl, new object[] { ins, ins.unit, renderer, 0, frames, .5f, indices });
        Check(Texture(renderer, "_Tex") == TexAssetForm.DataById[81001].GetTex(), "GIF clip observes in-place static replacement");
        ids[0] = 81004;
        for (int i = 0; i < 20; i++) tick.Invoke(ctrl, new object[] { ins, ins.unit, renderer, 0, frames, .5f, indices });
        // Measure the sampler itself, not Reflection's boxing/call allocations.
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) TextureAnimation.GetSample(ids, .5f, .15);
        Check(GC.GetAllocatedBytesForCurrentThread() == before, "warmed GIF timeline sampling allocates zero bytes");

        replacement.ClearRuntimeCache();
        Check(!ReferenceEquals(replacement.GetAnimationFrames(), replacementFrames), "asset cache reset re-decodes GIF frames");
        ctrl.ShowFinalMat(ins, 0, new List<int> { 81004 }, 0);
        Check(ins.animTimer[0] != null, "a sole GIF installs animation even without interval or other tex entries");
        ctrl.ShowFinalMat(ins, 0, new List<int> { 81001 }, 0);
    }

    private static void CharacterAnimatedAssets(TimeManager time)
    {
        CharacterProductForm.InitInternal();
        CharacterAnimForm.InitInternal();
        CharacterAnimClipForm.InitInternal();
        CharacterUnitForm.InitInternal();
        var product = new CharacterProductForm.Data(84001, "gif-character", 0, 0, null, 0,
            new Dictionary<string, CharacterAnimForm.Data>(), null, FaceType.Fixed, "", "", null, null,
            "", 0, false, null, 0, false, 0, 1, null);
        CharacterProductForm.DataByUid[product.uid] = product;
        var data = new CharacterUnitForm.Data(84002, false, Vector3.zero, 0, 0, 0, false,
            "", "", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0, null);
        data.unit.productInfo = (product.uid, -1);
        var root = new GameObject("gif-character");
        for (int i = 0; i < 2; i++)
        {
            var child = new GameObject("body-" + i);
            child.transform.SetParent(root.transform, false);
            child.AddComponent<MeshRenderer>().sharedMaterial = textureTemplate;
        }
        var ins = root.AddComponent<CharacterInstance>();
        data.unit.ins = ins;
        ins.unit = data.unit;
        var parts = new Dictionary<BodyPartType, bool> { { BodyPartType.UpperPart, true }, { BodyPartType.LowerPart, true } };
        var clip = new List<CharacterAnimClipForm.Data>();
        foreach (int id in new[] { 81004, 81002 })
            clip.Add(new CharacterAnimClipForm.Data(0, null, null,
                new Dictionary<BodyPartType, int> { { BodyPartType.UpperPart, id }, { BodyPartType.LowerPart, id } }));
        var idle = new CharacterAnimForm.Data(84003, "idle", new Dictionary<AnimDirecton, List<CharacterAnimClipForm.Data>>
            { { AnimDirecton.Fixed, clip } }, .5f, 1, parts);
        var ctrl = new GameCharacterController.AnimController { data = data, idleAnim = idle, moveAnim = idle };
        ctrl.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart);
        var renderer = ins.viewRefreshBindings[0].Target;
        var gif = TexAssetForm.DataById[81004];
        Check(Texture(renderer, "_Tex") == gif.GetAnimationFrames()[0].texture && ctrl.animTimer[BodyPartType.UpperPart] != null,
            "character starts GIF at its local first frame, not map global phase");
        var advance = ctrl.GetType().GetMethod("AdvanceTextureAnimation", PrivateInstance);
        var block = new MaterialPropertyBlock();
        object[] args = { BodyPartType.UpperPart, renderer, new[] { 81004, 81002 }, .5f, block, 0d, .15f };
        advance.Invoke(ctrl, args);
        Check(Texture(renderer, "_Tex") == gif.GetAnimationFrames()[1].texture, "character advances GIF internal frames");
        args[6] = .6f;
        advance.Invoke(ctrl, args);
        Check(Texture(renderer, "_Tex") == gif.GetAnimationFrames()[1].texture, "character waits interval after GIF completion");
        args[6] = .1f;
        advance.Invoke(ctrl, args);
        Check(Texture(renderer, "_Tex") == TexAssetForm.DataById[81002].GetTex(), "character then advances to the next tex");
        ctrl.stateCur[BodyPartType.UpperPart] = GameCharacterController.AnimController.State.Die;
        args[5] = 0d; args[6] = .75f;
        Check(!(bool)advance.Invoke(ctrl, args), "Die cannot finish while the GIF post-cycle interval is still running");
        args[6] = .6f;
        Check((bool)advance.Invoke(ctrl, args) && Texture(renderer, "_Tex") == TexAssetForm.DataById[81002].GetTex(),
            "Die completes once and keeps the final entry");
        product.animDic["special"] = new CharacterAnimForm.Data(84004, "special", idle.animClip, .5f, 1, parts);
        ctrl.ChangeState(GameCharacterController.AnimController.State.Special, BodyPartType.UpperPart, true, "special");
        args[5] = 0d; args[6] = .75f;
        Check(!(bool)advance.Invoke(ctrl, args), "Special also waits for GIF and interval");
        args[6] = .6f;
        Check((bool)advance.Invoke(ctrl, args) && ctrl.stateCur[BodyPartType.UpperPart] == GameCharacterController.AnimController.State.Idle
            && Texture(renderer, "_Tex") == gif.GetAnimationFrames()[0].texture, "Special returns to Idle at full variable-duration completion");
        var timer = ctrl.animTimer[BodyPartType.UpperPart];
        ctrl.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart, true);
        Check(timer.cancel && !ReferenceEquals(timer, ctrl.animTimer[BodyPartType.UpperPart]), "forced replay cancels old GIF timer");
        product.faceType = FaceType.FourDirection;
        idle.animClip[AnimDirecton.Up] = clip;
        idle.animClip[AnimDirecton.Right] = new List<CharacterAnimClipForm.Data> { clip[1] };
        root.transform.eulerAngles = Vector3.zero;
        ctrl.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart);
        ctrl.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.LowerPart);
        root.transform.eulerAngles = new Vector3(0, 90, 0);
        ctrl.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart);
        ctrl.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.LowerPart);
        Check(Texture(ins.viewRefreshBindings[0].Target, "_Tex") == TexAssetForm.DataById[81002].GetTex()
            && Texture(ins.viewRefreshBindings[1].Target, "_Tex") == TexAssetForm.DataById[81002].GetTex(), "both character parts rebind on direction changes");
        product.animDic["idle"] = idle;
        renderer.shadowCastingMode = ShadowCastingMode.Off;
        DisFadeRenderRouting.Refresh(renderer);
        ins.ApplyVision(0);
        ctrl.CloseHideRenderer();
        Check(!renderer.enabled && DisFadeRenderRouting.IsTextureConfigured(renderer),
            "enabling a character animation part cannot revive a fully hidden non-caster renderer");
        idle.partEnable[BodyPartType.UpperPart] = false;
        ctrl.CloseHideRenderer();
        DisFadeRenderRouting.SetTexture(renderer, TexAssetForm.DataById[81001].GetTex());
        ins.ApplyVision(1);
        Check(!renderer.enabled && !DisFadeRenderRouting.IsTextureConfigured(renderer)
            && Texture(renderer, "_Tex") == TexAssetForm.DataById[81001].GetTex(),
            "changing a disabled character part's texture keeps its configuration disabled after vision restoration");
        idle.partEnable[BodyPartType.UpperPart] = true;
        ctrl.CloseHideRenderer();
        Check(renderer.enabled && renderer.shadowCastingMode == ShadowCastingMode.Off,
            "explicitly re-enabling a visible animation part restores color without inventing shadows");
        foreach (var timerEntry in ctrl.animTimer.Values) time.CancelTimer(timerEntry);
        CharacterAnimationLifecycle(data, ins, product, idle);
        UnityEngine.Object.DestroyImmediate(root);
    }

    private static void CharacterAnimationLifecycle(CharacterUnitForm.Data data, CharacterInstance ins,
        CharacterProductForm.Data product, CharacterAnimForm.Data idle)
    {
        var game = CreateManager<GameManager>();
        var play = CreateManager<PlayManager>();
        var animation = new GameCharacterController(game);
        game.characterCtrl = animation;
        var scene = new PlaySceneController(play);
        play.sceneCtrl = scene;
        var characters = new Dictionary<CharacterProductForm.Data, CharacterUnitForm.Data> { { product, data } };
        typeof(PlaySceneController).GetField("_characterDic", PrivateInstance).SetValue(scene, characters);
        CharacterUnitForm.DataByUid[data.uid] = data;
        product.animDic["idle"] = idle;
        product.defaultAnimName = new Dictionary<string, string> { { "idle", "idle" }, { "move", "idle" } };
        var command = new CharacterPlayAnimCmd();
        var heap = new Dictionary<string, BoxDataForm.Data>();
        Action invoke = () =>
        {
            var task = new InterpretAsyncTask(null);
            // CmdBase receives arguments in stack order, then reverses them.
            command.Execute(new[] { CodeHelper.CreateBoxByStr("special"), CodeHelper.CreateBoxByStr(
                GlobalEventHelper.GetName(GlobalEventHelper.CHARACTER, product.uid.ToString())) }, heap, task);
            Check(task.IsComplete() && task.error == null, "CharacterPlayAnim completes without a runtime diagnostic");
        };
        try
        {
            // Reproduce the screenshot: a live scene row has never acquired a rendered instance/controller.
            data.unit.ins = null;
            ins.unit = null;
            invoke();
            animation.RegisterAnim(data);
            Check(animation.animControllerDic.Count == 0, "never-shown Character skips visual animation and registration");
            data.unit.ins = ins;
            ins.unit = data.unit;
            invoke();
            var status = animation.animControllerDic[data];
            Check(status.stateCur[BodyPartType.UpperPart] == GameCharacterController.AnimController.State.Special
                && status.stateCur[BodyPartType.LowerPart] == GameCharacterController.AnimController.State.Special,
                "visible Character lazily registers and plays Special on both parts");
            var upper = status.animTimer[BodyPartType.UpperPart];
            var lower = status.animTimer[BodyPartType.LowerPart];
            invoke();
            Check(upper.cancel && lower.cancel, "replaying Special cancels both previous timers");

            upper = status.animTimer[BodyPartType.UpperPart];
            lower = status.animTimer[BodyPartType.LowerPart];
            animation.LoadModel(ins);
            Check(upper.cancel && lower.cancel && status.stateCur.Count == 0
                && status.animTimer[BodyPartType.UpperPart] == null,
                "pooled model reset cancels timers before dropping references and clears Special state");
            animation.CheckAnim(ins);
            Check(status.stateCur[BodyPartType.UpperPart] == GameCharacterController.AnimController.State.Idle,
                "model rebind can resume Idle instead of retaining stale Special state");

            upper = status.animTimer[BodyPartType.UpperPart];
            lower = status.animTimer[BodyPartType.LowerPart];
            animation.UnregisterAnim(data);
            Check(upper.cancel && lower.cancel && animation.animControllerDic.Count == 0,
                "unregister cancels both timers and removes only this Character controller");
            animation.UnregisterAnim(data);
            invoke();
            Check(!ReferenceEquals(status, animation.animControllerDic[data]), "visible Character can register again after unregister");
            status = animation.animControllerDic[data];

            ins.gameObject.SetActive(false);
            var hiddenTimer = status.animTimer[BodyPartType.UpperPart];
            invoke();
            Check(ReferenceEquals(hiddenTimer, status.animTimer[BodyPartType.UpperPart]) && !hiddenTimer.cancel,
                "offscreen animation command does not start or replay a timer on a hidden instance");
            ins.gameObject.SetActive(true);

            var other = new CharacterUnitForm.Data(data.uid, false, Vector3.zero, 0, 0, 0, false,
                "", "", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0, null);
            CharacterUnitForm.DataByUid[data.uid] = other;
            animation.PlaySpecialAnim(data, "special");
            animation.RegisterAnim(data);
            Check(ReferenceEquals(hiddenTimer, status.animTimer[BodyPartType.UpperPart]) && !hiddenTimer.cancel,
                "same UID with replacement Data cannot replay or register the removed Character");
            CharacterUnitForm.DataByUid[data.uid] = data;

            CharacterProductForm.DataByUid.Remove(product.uid);
            animation.PlaySpecialAnim(data, "special");
            Check(!hiddenTimer.cancel, "removed Product cannot animate a retained scene row");
            CharacterProductForm.DataByUid[product.uid] = product;
            ins.unit = null;
            animation.PlaySpecialAnim(data, "special");
            animation.CheckAnim(ins);
            Check(!hiddenTimer.cancel, "unbound pooled instance is ignored");
            ins.unit = data.unit;

            lower = status.animTimer[BodyPartType.LowerPart];
            animation.Reset();
            Check(hiddenTimer.cancel && lower.cancel && animation.animControllerDic.Count == 0,
                "scene reset cancels all Character timers before clearing controllers");
            invoke();
            status = animation.animControllerDic[data];
            upper = status.animTimer[BodyPartType.UpperPart];
            animation.OnEvent(new CharacterEvent { type = MapEventType.Hide, unit = data.unit });
            CharacterUnitForm.DataByUid.Remove(data.uid);
            animation.PlaySpecialAnim(data, "special");
            invoke();
            Check(upper.cancel && animation.animControllerDic.Count == 0,
                "removed Character's delayed animation is a no-op and cannot recreate its controller");
        }
        finally
        {
            animation.Reset();
            animation.Unregister<CharacterEvent>();
            var sizeChanged = typeof(GameCharacterController).GetMethod("OnCharacterSizeChanged", PrivateInstance);
            CharacterProductForm.changeSizeAction -= (Action<CharacterProductForm.Data, int, int>)
                Delegate.CreateDelegate(typeof(Action<CharacterProductForm.Data, int, int>), animation, sizeChanged);
            var passChanged = typeof(GameCharacterController).GetMethod("OnCharacterPassTypeChanged", PrivateInstance);
            CharacterProductForm.changePasstypeAction -= (Action<CharacterProductForm.Data, List<int>, List<int>>)
                Delegate.CreateDelegate(typeof(Action<CharacterProductForm.Data, List<int>, List<int>>), animation, passChanged);
            CharacterUnitForm.DataByUid.Remove(data.uid);
            data.unit.ins = ins;
            ins.unit = data.unit;
            ins.gameObject.SetActive(true);
            UnityEngine.Object.DestroyImmediate(play.gameObject);
            UnityEngine.Object.DestroyImmediate(game.gameObject);
        }
    }

    private static void AnimatedWangTileAssets()
    {
        var red = new Texture2D(4, 6);
        var blue = new Texture2D(4, 6);
        var redPixels = new Color[24]; var bluePixels = new Color[24];
        for (int i = 0; i < 24; i++) { redPixels[i] = Color.red; bluePixels[i] = Color.blue; }
        red.SetPixels(redPixels); red.Apply(); blue.SetPixels(bluePixels); blue.Apply();
        var frames = new List<AnimatedFrameData> {
            new AnimatedFrameData { texture = red, delaySeconds = .1f },
            new AnimatedFrameData { texture = blue, delaySeconds = .2f } };
        var make = typeof(Main2StoryManager).GetMethod("CreateAnimatedWangTileFrames", PrivateStatic);
        var variants = (Dictionary<int, List<AnimatedFrameData>>)make.Invoke(null, new object[] { frames });
        Check(variants.Count == 256, "animated WangTile expands all neighbour masks");
        var generated = new TexAssetForm.Data(81005, "animated-wang", "", null, "", null, 0);
        generated.SetRuntimeAnimationFrames(variants[0]);
        TexAssetForm.DataById[generated.id] = generated;
        Check(generated.GetAnimationFrames().Count == 2 && Math.Abs(generated.GetAnimationDuration() - .3) < .0001,
            "generated WangTile asset retains source GIF frame delays");
        Check(((Texture2D)TextureAnimation.GetSample(81005, .5f, .15).texture).GetPixel(0, 0).b > .9f
            && ((Texture2D)TextureAnimation.GetSample(81005, .5f, .75).texture).GetPixel(0, 0).b > .9f,
            "generated WangTile plays internal frames then holds its last frame");
        var textures = new HashSet<Texture>();
        foreach (var variant in variants.Values)
        {
            Check(variant.Count == 2 && variant[0].texture.GetPixel(0, 0).r > .9f
                && variant[1].texture.GetPixel(0, 0).b > .9f, "each WangTile mask preserves both GIF frames");
            foreach (var frame in variant) textures.Add(frame.texture);
        }
        Check(textures.Count == 94, "equivalent animated WangTile masks share 47 textures per GIF frame");
        foreach (var texture in textures) UnityEngine.Object.DestroyImmediate(texture);
        UnityEngine.Object.DestroyImmediate(red); UnityEngine.Object.DestroyImmediate(blue);
        TexAssetForm.DataById.Remove(generated.id);
    }

    private static void DisFadeRouting(GameMapController ctrl, Texture a, Texture b, Texture mask)
    {
        var shader = Shader.Find("DisFadeCode");
        Check(shader != null, "routing fixture imports the real DisFadeCode shader");
        var source = new Material(shader) { name = "routing-source", renderQueue = 4100 };
        source.SetFloat("_Alpha", 1f);
        source.SetFloat("_Show", 1f);
        source.SetFloat("_FadeCenter", 0f);
        source.SetFloat("_LightSensitivity", .8f);
        source.SetFloat("_Cutoff", .3f);
        source.SetTexture(TextureId, a);
        Check(!source.HasProperty(LegacyMaskId), "DisFade no longer exposes the unused AlphaTex property");
        var prefab = new GameObject("routing-prefab");
        prefab.SetActive(false);
        var holder = new GameObject("routing-pool");
        var modes = new[] { ShadowCastingMode.On, ShadowCastingMode.Off, ShadowCastingMode.TwoSided,
            ShadowCastingMode.ShadowsOnly, ShadowCastingMode.On, ShadowCastingMode.Off };
        var block = new MaterialPropertyBlock();
        GameObject live = null;
        try
        {
            for (int i = 0; i < modes.Length; i++)
            {
                var child = new GameObject("routing-renderer-" + i);
                child.transform.SetParent(prefab.transform, false);
                var renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = source;
                renderer.shadowCastingMode = modes[i];
                renderer.enabled = i != 5;
                block.Clear();
                block.SetFloat("_LightSensitivity", .8f);
                renderer.SetPropertyBlock(block);
            }
            prefab.AddComponent<RegressionRoutingTileInstance>();
            var pool = new Z_UnitSystem.InstancePool(prefab, holder.transform);
            live = pool.New();
            pool.Fresh(live);
            var ins = live.GetComponent<TileInstance>();
            var tile = NewTile(82501);
            tile.unit.ins = ins;
            ins.unit = tile.unit;

            // A reused instance may still carry blocks produced before texture
            // binding moved to materials. Strip both old texture entries while
            // retaining all supported numeric instance overrides.
            ins.viewRefreshBindings[0].Target.GetPropertyBlock(block);
            block.SetTexture(TextureId, b);
            block.SetTexture(LegacyMaskId, mask);
            block.SetFloat("_Show", 1f);
            block.SetFloat("_FadeCenter", 0f);
            ins.viewRefreshBindings[0].Target.SetPropertyBlock(block);
            DisFadeRenderRouting.SetTexture(ins.viewRefreshBindings[0].Target, a);
            ins.viewRefreshBindings[0].Target.GetPropertyBlock(block);
            Check(!block.HasTexture(TextureId) && !block.HasTexture(LegacyMaskId)
                && block.GetFloat("_LightSensitivity") == .8f && block.GetFloat("_Show") == 1f,
                "texture migration strips legacy texture/mask MPBs and preserves numeric overrides");

            Check(DisFadeRenderRouting.Classify(1, 0, 0) == DisFadeRenderRouting.Route.Hidden
                && DisFadeRenderRouting.Classify(1, 1, 0) == DisFadeRenderRouting.Route.Opaque
                && DisFadeRenderRouting.Classify(1, .25f, 0) == DisFadeRenderRouting.Route.Transparent
                && DisFadeRenderRouting.Classify(1, .5f, 0) == DisFadeRenderRouting.Route.Transparent
                && DisFadeRenderRouting.Classify(1, 1, 1) == DisFadeRenderRouting.Route.Mixed
                && DisFadeRenderRouting.Classify(.8f, 1, 1) == DisFadeRenderRouting.Route.Transparent,
                "routes use final alpha/show and only fully opaque center fades require two passes");
            Check(DisFadeRenderRouting.Classify(1, .9999f, 0) == DisFadeRenderRouting.Route.Transparent
                && DisFadeRenderRouting.Classify(1, .00001f, 0) == DisFadeRenderRouting.Route.Hidden,
                "partial opacity never enters the opaque pass and the near-zero cutoff stays hidden");

            ins.ApplyVision(0f);
            var hiddenMaterial = ins.viewRefreshBindings[0].Target.sharedMaterial;
            Check(ins.viewRefreshBindings[2].Target.sharedMaterial == hiddenMaterial
                && hiddenMaterial.GetShaderPassEnabled("UniversalForward")
                && hiddenMaterial.GetShaderPassEnabled("DisFadeOpaque"),
                "hidden renderers share the existing Mixed material without a fourth variant");
            for (int i = 0; i < modes.Length; i++)
            {
                var renderer = ins.viewRefreshBindings[i].Target;
                Check(renderer.enabled == (i != 5 && modes[i] != ShadowCastingMode.Off),
                    "fully hidden routing keeps only authored casters enabled at slot " + i);
                Check(renderer.shadowCastingMode == (modes[i] == ShadowCastingMode.Off
                        ? modes[i] : ShadowCastingMode.ShadowsOnly),
                    "hidden renderers suppress color with ShadowsOnly or disabled state at slot " + i);
                CheckRoutingProperties(renderer, 0, 0, a);
            }
            ins.ApplyVision(1f);
            var opaque = ins.viewRefreshBindings[0].Target.sharedMaterial;
            Check(opaque != source && opaque.enableInstancing
                && opaque.GetShaderPassEnabled("DisFadeOpaque") && opaque.GetShaderPassEnabled("UniversalForward"),
                "opaque texture material enables instancing and retains feature-free forward fallback");
            for (int i = 0; i < modes.Length; i++)
            {
                Check(ins.viewRefreshBindings[i].Target.enabled == (i != 5) && ins.viewRefreshBindings[i].Target.shadowCastingMode == modes[i],
                    "visibility restoration preserves enabled and shadow baselines");
                Check(ins.viewRefreshBindings[i].Target.sharedMaterial == opaque, "same source/texture/route shares one material across six slots");
                CheckRoutingProperties(ins.viewRefreshBindings[i].Target, 1, 0, a);
            }
            DisFadeRenderRouting.BeginSplitCamera();
            Check(!opaque.GetShaderPassEnabled("UniversalForward") && opaque.GetShaderPassEnabled("DisFadeOpaque"),
                "split camera suppresses only the redundant opaque forward draw");
            DisFadeRenderRouting.EndSplitCamera();
            Check(opaque.GetShaderPassEnabled("UniversalForward"), "feature-free camera restores opaque fallback");

            ins.ApplyVision(.25f);
            var transparent = ins.viewRefreshBindings[0].Target.sharedMaterial;
            Check(transparent != source && transparent != opaque && transparent.enableInstancing
                && !transparent.GetShaderPassEnabled("DisFadeOpaque")
                && transparent.GetShaderPassEnabled("UniversalForward"),
                "constant partial material suppresses only its empty opaque pass");
            for (int i = 0; i < modes.Length; i++)
            {
                Check(ins.viewRefreshBindings[i].Target.sharedMaterial == transparent, "same texture shares a constant-transparent material");
                CheckRoutingProperties(ins.viewRefreshBindings[i].Target, .25f, 0, a);
            }
            ins.ApplyVision(.5f);
            var mixed = ins.viewRefreshBindings[0].Target.sharedMaterial;
            Check(mixed.enableInstancing && mixed.GetShaderPassEnabled("UniversalForward")
                && mixed.GetShaderPassEnabled("DisFadeOpaque"),
                "mixed-center material retains both passes and instancing");
            for (int i = 0; i < modes.Length; i++)
            {
                Check(ins.viewRefreshBindings[i].Target.sharedMaterial == mixed, "same texture shares a mixed-center material");
                CheckRoutingProperties(ins.viewRefreshBindings[i].Target, 1, 1, a);
            }
            ins.ApplyVision(.25f, .5f);
            for (int i = 0; i < modes.Length; i++)
                Check(ins.viewRefreshBindings[i].Target.sharedMaterial == (i < 3 ? transparent : mixed),
                    "body/front independently select constant-transparent or mixed routes");

            ins.ApplyVision(1f);
            for (int display = 0; display < 3; display++)
            {
                ins.displayLayer = display;
                ins.ApplyVision(.5f);
                for (int i = 0; i < modes.Length; i++)
                {
                    bool shown = i % 3 <= display;
                    var renderer = ins.viewRefreshBindings[i].Target;
                    CheckRoutingProperties(renderer, shown ? 1 : 0, shown ? 1 : 0, a);
                    Check(renderer.enabled == (i != 5 && (shown || modes[i] != ShadowCastingMode.Off))
                        && renderer.shadowCastingMode == (shown || modes[i] == ShadowCastingMode.Off
                            ? modes[i] : ShadowCastingMode.ShadowsOnly),
                        "display layer routes each normal/front pair without changing authored shadows");
                }
            }
            ins.displayLayer = int.MaxValue;
            ins.ApplyVision(1f);

            DisFadeRenderRouting.SetTexture(ins.viewRefreshBindings[0].Target, b);
            var opaqueB = ins.viewRefreshBindings[0].Target.sharedMaterial;
            Check(opaqueB != opaque && opaqueB.GetTexture(TextureId) == b
                && opaque.GetTexture(TextureId) == a && source.GetTexture(TextureId) == a,
                "a different texture creates a separate cached material without mutating other renderers/template");
            DisFadeRenderRouting.SetTexture(ins.viewRefreshBindings[1].Target, b);
            Check(ins.viewRefreshBindings[1].Target.sharedMaterial == opaqueB, "same texture and source reuse the cached material");
            DisFadeRenderRouting.SetTexture(ins.viewRefreshBindings[0].Target, a);
            Check(ins.viewRefreshBindings[0].Target.sharedMaterial == opaque, "switching texture back reuses the original cache entry");
            DisFadeRenderRouting.SetTexture(ins.viewRefreshBindings[1].Target, a);
            Check(DisFadeRenderRouting.GetTexture(ins.viewRefreshBindings[1].Target) == a, "texture API reports the current material texture");
            var otherSource = new Material(source) { name = "other-source" };
            try
            {
                ins.viewRefreshBindings[4].Target.sharedMaterial = otherSource;
                DisFadeRenderRouting.SetTexture(ins.viewRefreshBindings[4].Target, b);
                Check(ins.viewRefreshBindings[4].Target.sharedMaterial != opaqueB
                    && ins.viewRefreshBindings[4].Target.sharedMaterial.GetTexture(TextureId) == b,
                    "equal textures with different templates never incorrectly share source properties");
                DisFadeRenderRouting.Release(ins.viewRefreshBindings[4].Target);
                Check(ins.viewRefreshBindings[4].Target.GetComponent<DisFadeMaterialBinding>().Source == otherSource
                    && ins.viewRefreshBindings[4].Target.sharedMaterial.GetTexture(TextureId) == a,
                    "external source replacement releases to the new source's static texture, not the previous template");
                ins.viewRefreshBindings[4].Target.sharedMaterial = source;
                DisFadeRenderRouting.Refresh(ins.viewRefreshBindings[4].Target);
            }
            finally { UnityEngine.Object.DestroyImmediate(otherSource); }

            ins.ApplyVision(0f);
            ctrl.ShowFinalMat(ins, 0, null, 0);
            var emptyMaterial = ins.viewRefreshBindings[0].Target.sharedMaterial;
            ctrl.ShowFinalMat(ins, 0, new List<int> { 81003 }, 0, true);
            Check(!ins.viewRefreshBindings[0].Target.enabled && !DisFadeRenderRouting.IsTextureConfigured(ins.viewRefreshBindings[0].Target)
                && ins.viewRefreshBindings[0].Target.sharedMaterial == emptyMaterial,
                "ignored mask never re-enables an empty layer or changes its cached material");
            ins.ApplyVision(1f);
            Check(!ins.viewRefreshBindings[0].Target.enabled, "vision cannot revive an intentionally empty texture layer");
            ins.ApplyVision(0f);
            ctrl.ShowFinalMat(ins, 0, new List<int> { 81001 }, 0);
            Check(ins.viewRefreshBindings[0].Target.enabled && ins.viewRefreshBindings[0].Target.shadowCastingMode == ShadowCastingMode.ShadowsOnly,
                "adding a hidden texture restores only its original shadow silhouette");
            CheckRoutingProperties(ins.viewRefreshBindings[0].Target, 0, 0, a);
            var animation = new List<int> { 81001, 81002 };
            TimeManager.animationTimeGetter = () => 1.25f;
            ctrl.ShowFinalMat(ins, 1, animation, .5f);
            var framesType = typeof(GameMapController).GetNestedType("TextureFrames", BindingFlags.NonPublic);
            var frames = Activator.CreateInstance(framesType, new object[] { animation });
            var tick = typeof(GameMapController).GetMethod("UpdateTextureAnimation", PrivateInstance);
            var indices = new Dictionary<int, int>();
            var timer = ins.animTimer[1];
            var animationMaterial = ins.viewRefreshBindings[1].Target.sharedMaterial;
            ctrl.ShowFinalMat(ins, 1, new List<int> { 81003 }, 0, true);
            Check(ReferenceEquals(timer, ins.animTimer[1]) && !timer.cancel
                && ins.viewRefreshBindings[1].Target.sharedMaterial == animationMaterial,
                "ignored mask does not cancel animation or replace its texture material");
            TimeManager.animationTimeGetter = () => 1.75f;
            tick.Invoke(ctrl, new object[] { ins, tile.unit, ins.viewRefreshBindings[1].Target, 1, frames, .5f, indices });
            Check(!ins.viewRefreshBindings[1].Target.enabled && ins.viewRefreshBindings[1].Target.shadowCastingMode == ShadowCastingMode.Off
                && Texture(ins.viewRefreshBindings[1].Target, "_Tex") == b,
                "hidden animation changes material frames without reopening color submission");
            CheckRoutingProperties(ins.viewRefreshBindings[1].Target, 0, 0, b);
            TimeManager.instance.CancelTimer(ins.animTimer[1]);
            ins.animTimer[1] = null;
            TimeManager.animationTimeGetter = null;

            ins.viewRefreshBindings[2].Target.GetPropertyBlock(block);
            block.SetFloat("_Alpha", .7f);
            block.SetFloat("_Cutoff", .42f);
            ins.viewRefreshBindings[2].Target.SetPropertyBlock(block);
            ins.ApplyVision(.5f);
            ins.viewRefreshBindings[2].Target.GetPropertyBlock(block);
            Check(block.GetFloat("_Alpha") == .7f && block.GetFloat("_Cutoff") == .42f
                && ins.viewRefreshBindings[2].Target.sharedMaterial == transparent,
                "numeric instance overrides survive vision and select the correct route");
            CheckRoutingProperties(ins.viewRefreshBindings[2].Target, 1, 1, a);
            source.SetFloat("_Alpha", .4f);
            source.SetFloat("_Cutoff", .35f);
            source.renderQueue = 4200;
            DisFadeRenderRouting.Refresh(ins.viewRefreshBindings[0].Target);
            Check(ins.viewRefreshBindings[0].Target.sharedMaterial == transparent && transparent.GetFloat("_Alpha") == .4f
                && transparent.GetFloat("_Cutoff") == .35f && transparent.renderQueue == 4200,
                "cached materials synchronize changed source values and queues");
            source.SetFloat("_Alpha", 1f);
            source.SetFloat("_Cutoff", .3f);
            source.renderQueue = 4100;
            DisFadeRenderRouting.RefreshSource(source);

            pool.Clear(live);
            for (int i = 0; i < modes.Length; i++)
                Check(ins.viewRefreshBindings[i].Target.GetComponent<DisFadeMaterialBinding>().Source == source
                    && ins.viewRefreshBindings[i].Target.sharedMaterial.GetTexture(TextureId) == a
                    && ins.viewRefreshBindings[i].Target.shadowCastingMode == modes[i] && ins.viewRefreshBindings[i].Target.enabled == (i != 5)
                    && source.GetTexture(TextureId) == a,
                    "pool disable restores static template, texture, enabled and casting baselines");
            pool.Fresh(live);
            var next = NewTile(82502);
            next.unit.ins = ins;
            ins.unit = next.unit;
            ins.ApplyVision(1f);
            for (int i = 0; i < modes.Length; i++)
            {
                Check(ins.viewRefreshBindings[i].Target.sharedMaterial == opaque && ins.viewRefreshBindings[i].Target.shadowCastingMode == modes[i]
                    && ins.viewRefreshBindings[i].Target.enabled == (i != 5),
                    "pool reuse shares the original material without stale frame/fade state");
                CheckRoutingProperties(ins.viewRefreshBindings[i].Target, 1, 0, a);
            }

            // Reflection is intentionally absent from the allocation probe.
            // Warm every route and both animation-frame cache entries first.
            for (int repeat = 0; repeat < 20; repeat++)
                WarmTextureRouting(ins, a, b);
            long before = GC.GetAllocatedBytesForCurrentThread();
            for (int repeat = 0; repeat < 100; repeat++)
                WarmTextureRouting(ins, a, b);
            long allocated = GC.GetAllocatedBytesForCurrentThread() - before;
            Check(allocated == 0, "warmed visibility, texture-frame changes and camera scopes allocate zero bytes: " + allocated);
            Check(source.GetShaderPassEnabled("UniversalForward") && source.GetShaderPassEnabled("DisFadeOpaque")
                && source.GetFloat("_Alpha") == 1 && source.GetFloat("_Show") == 1
                && source.GetFloat("_FadeCenter") == 0 && source.GetTexture(TextureId) == a && source.renderQueue == 4100,
                "routing never mutates authored template properties or pass flags");
            ins.ApplyVision(1);
            ctrl.ResetSceneUnitCaches();
            Check(opaque == null && opaqueB == null && transparent == null && mixed == null && hiddenMaterial == null,
                "scene reset destroys all shared texture/route variants");
            for (int i = 0; i < modes.Length; i++)
            {
                Check(ins.viewRefreshBindings[i].Target.sharedMaterial != null && ins.viewRefreshBindings[i].Target.shadowCastingMode == modes[i]
                    && ins.viewRefreshBindings[i].Target.enabled == (i != 5),
                    "scene reset safely rebinds every live renderer after releasing old cached materials");
                CheckRoutingProperties(ins.viewRefreshBindings[i].Target, 1, 0, a);
            }

            UnsupportedMaterialBoundaries(source, a, b);
            NeverUsedInactiveRendererCache(source);
        }
        finally
        {
            TimeManager.animationTimeGetter = null;
            DisFadeRenderRouting.Clear();
            if (live != null) UnityEngine.Object.DestroyImmediate(live);
            UnityEngine.Object.DestroyImmediate(prefab);
            UnityEngine.Object.DestroyImmediate(holder);
            UnityEngine.Object.DestroyImmediate(source);
        }
    }

    private static void SourceMaterialSnapshots(Texture a, Texture b)
    {
        var root = new GameObject("source-snapshot-probe");
        var source = new Material(textureTemplate.shader) { renderQueue = 3000 };
        source.SetTexture(TextureId, a);
        source.SetFloat("_Alpha", 1f);
        source.SetFloat("_Show", 1f);
        source.SetFloat("_FadeCenter", 0f);
        source.SetFloat("_Cutoff", .3f);
        source.SetFloat("_LightSensitivity", .8f);
        source.SetShaderPassEnabled("UniversalForward", true);
        source.SetShaderPassEnabled("DisFadeOpaque", true);
        var extraTexture = new Texture2D(1, 1);
        var block = new MaterialPropertyBlock();
        try
        {
            var first = root.AddComponent<MeshRenderer>();
            var child = new GameObject("second-source-user");
            child.transform.SetParent(root.transform, false);
            var second = child.AddComponent<MeshRenderer>();
            first.sharedMaterial = second.sharedMaterial = source;
            DisFadeRenderRouting.SetTemplateTexture(first, a);
            DisFadeRenderRouting.SetTemplateTexture(second, b);
            var records = (System.Collections.IDictionary)typeof(DisFadeRenderRouting)
                .GetField("sources", PrivateStatic).GetValue(null);
            var record = records[source];
            var recordType = record.GetType();
            var versionProperty = recordType.GetProperty("version");
            int version = (int)versionProperty.GetValue(record);
            var textures = (System.Collections.IDictionary)recordType.GetField("variants").GetValue(record);
            Check(version == 1 && textures.Count == 2,
                "two textures share one original-material snapshot captured once");

            source.SetFloat("_Alpha", .25f);
            source.SetFloat("_Cutoff", .6f);
            source.SetFloat("_LightSensitivity", .2f);
            source.renderQueue = 3100;
            source.SetShaderPassEnabled("UniversalForward", false);
            DisFadeRenderRouting.SetTexture(first, extraTexture);
            var extraVariant = first.sharedMaterial;
            Check(extraVariant.GetFloat("_Alpha") == 1f && extraVariant.GetFloat("_Cutoff") == .3f
                && extraVariant.GetFloat("_LightSensitivity") == .8f && extraVariant.renderQueue == 3000
                && extraVariant.GetShaderPassEnabled("UniversalForward"),
                "new texture variants use cached source values rather than unannounced edits");
            for (int i = 0; i < 20; i++)
            {
                block.SetFloat("_Show", i % 2);
                DisFadeRenderRouting.Apply(first, block);
                DisFadeRenderRouting.SetTextureConfigured(first, i % 2 == 0);
                DisFadeRenderRouting.Release(first);
                DisFadeRenderRouting.SetTexture(first, i % 2 == 0 ? a : b);
                DisFadeRenderRouting.BeginSplitCamera();
                DisFadeRenderRouting.EndSplitCamera();
            }
            Check(ReferenceEquals(records[source], record) && (int)versionProperty.GetValue(record) == version,
                "visibility/texture/configured/release/camera paths never recapture source properties");

            DisFadeRenderRouting.RefreshSource(source);
            Check((int)versionProperty.GetValue(record) == version + 1
                && extraVariant.GetFloat("_Alpha") == .25f && extraVariant.GetFloat("_Cutoff") == .6f
                && extraVariant.GetFloat("_LightSensitivity") == .2f && extraVariant.renderQueue == 3100,
                "explicit source refresh captures once and synchronizes all texture variants");
            DisFadeRenderRouting.BeginSplitCamera();
            DisFadeRenderRouting.EndSplitCamera();
            Check(!extraVariant.GetShaderPassEnabled("UniversalForward"),
                "camera cleanup respects the cached authored forward-pass switch");

            source.SetFloat("_Alpha", .5f);
            source.SetShaderPassEnabled("UniversalForward", true);
            block.Clear();
            first.SetPropertyBlock(block);
            DisFadeRenderRouting.Refresh(first);
            Check((int)versionProperty.GetValue(record) == version + 2
                && first.sharedMaterial.GetFloat("_Alpha") == .5f
                && !first.sharedMaterial.GetShaderPassEnabled("DisFadeOpaque"),
                "renderer Refresh explicitly updates the shared snapshot and reevaluates its route");
            DisFadeRenderRouting.Clear();
            var nextRecords = (System.Collections.IDictionary)typeof(DisFadeRenderRouting)
                .GetField("sources", PrivateStatic).GetValue(null);
            Check(!ReferenceEquals(record, nextRecords[source]) && extraVariant == null
                && first.sharedMaterial.GetFloat("_Alpha") == .5f,
                "scene cache clear retires source snapshots/variants and recaptures surviving owners");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            DisFadeRenderRouting.Clear();
            UnityEngine.Object.DestroyImmediate(extraTexture);
            UnityEngine.Object.DestroyImmediate(source);
        }
    }

    private static void WarmTextureRouting(TileInstance ins, Texture a, Texture b)
    {
        ins.ApplyVision(0); ins.ApplyVision(1); ins.ApplyVision(.25f); ins.ApplyVision(.5f);
        var renderer = ins.viewRefreshBindings[0].Target;
        DisFadeRenderRouting.SetTexture(renderer, b);
        DisFadeRenderRouting.SetTexture(renderer, a);
        DisFadeRenderRouting.Refresh(renderer);
        DisFadeRenderRouting.BeginSplitCamera(); DisFadeRenderRouting.EndSplitCamera();
    }

    private static void CheckRoutingProperties(Renderer renderer, float show, float fade, Texture texture)
    {
        renderer.GetPropertyBlock(readBlock);
        Check(readBlock.GetFloat("_Show") == show && readBlock.GetFloat("_FadeCenter") == fade
            && !readBlock.HasTexture(TextureId) && !readBlock.HasTexture(LegacyMaskId)
            && renderer.sharedMaterial.GetTexture(TextureId) == texture,
            "routing preserves numeric instance values and material-only texture binding");
    }

    private static void UnsupportedMaterialBoundaries(Material source, Texture a, Texture b)
    {
        var root = new GameObject("unsupported-route-probe");
        var renderer = root.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = source;
        renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
        var block = new MaterialPropertyBlock();
        block.SetFloat("_Show", .25f);
        block.SetFloat("_FadeCenter", 1f);
        renderer.SetPropertyBlock(block);
        try
        {
            foreach (int queue in new[] { 2500, 5001 })
            {
                source.renderQueue = queue;
                DisFadeRenderRouting.RefreshSource(source);
                DisFadeRenderRouting.SetTexture(renderer, b);
                Check(renderer.enabled && renderer.shadowCastingMode == ShadowCastingMode.TwoSided
                    && renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                    "out-of-range queue keeps its conservative forward and authored shadow path");
                Check(renderer.sharedMaterial.GetTexture(TextureId) == b && source.GetTexture(TextureId) == a,
                    "out-of-range queue texture override remains independent of its shared template");
                DisFadeRenderRouting.Release(renderer);
                Check(renderer.GetComponent<DisFadeMaterialBinding>().Source == source
                    && renderer.sharedMaterial.GetTexture(TextureId) == a,
                    "unsupported queue release restores its source's static texture");
            }
            source.renderQueue = 3000;
            DisFadeRenderRouting.RefreshSource(source);
            var destroyed = new Texture2D(1, 1);
            DisFadeRenderRouting.SetTexture(renderer, destroyed);
            var deadMaterial = renderer.sharedMaterial;
            UnityEngine.Object.DestroyImmediate(destroyed);
            DisFadeRenderRouting.SetTexture(renderer, b);
            Check(renderer.sharedMaterial.GetTexture(TextureId) == b && source.GetTexture(TextureId) == a,
                "destroyed texture followed by replacement cannot poison the source or texture lookup");
            DisFadeRenderRouting.SetTexture(renderer, a);
            var liveMaterial = renderer.sharedMaterial;
            DisFadeRenderRouting.Clear();
            Check(deadMaterial == null && liveMaterial == null && renderer.sharedMaterial != null
                && renderer.sharedMaterial.GetTexture(TextureId) == a,
                "clear releases old destroyed-texture entries and safely rebinds the live current texture");
            CheckRoutingProperties(renderer, .25f, 1, a);

            renderer.sharedMaterial = null;
            InvokeBindingCallback(renderer, "OnEnable");
            Check(renderer.sharedMaterial == null,
                "clearing the shared material is a safe external replacement and cannot revive an old binding");
            renderer.sharedMaterial = source;
            DisFadeRenderRouting.SetTexture(renderer, a);

            // Shader type is guaranteed by callers. Multi-material Renderers
            // still keep the existing MPB path rather than losing extra slots.
            DisFadeRenderRouting.Release(renderer);
            renderer.sharedMaterials = new[] { source, source };
            DisFadeRenderRouting.SetTexture(renderer, b);
            renderer.GetPropertyBlock(block);
            Check(renderer.sharedMaterials.Length == 2 && renderer.sharedMaterial == source
                && block.GetTexture(TextureId) == b && renderer.shadowCastingMode == ShadowCastingMode.TwoSided,
                "DisFade-only routing retains multi-material slots and the texture-MPB fallback");
            root.SetActive(false);
            InvokeBindingCallback(renderer, "OnDisable");
            root.SetActive(true);
            InvokeBindingCallback(renderer, "OnEnable");
            DisFadeRenderRouting.Clear();
            renderer.GetPropertyBlock(block);
            Check(renderer.sharedMaterials.Length == 2 && renderer.sharedMaterial == source
                && block.GetTexture(TextureId) == b,
                "multi-material fallback survives native lifecycle and cache retirement");
        }
        finally
        {
            source.renderQueue = 4100;
            DisFadeRenderRouting.RefreshSource(source);
            DisFadeRenderRouting.Release(renderer);
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static void TemplateTexturePoolLifecycle(Texture a, Texture b)
    {
        var source = new Material(textureTemplate.shader) { name = "serialized-texture-template" };
        source.SetFloat("_Alpha", 1f);
        source.SetFloat("_Show", 1f);
        source.SetTexture(TextureId, Texture2D.whiteTexture);
        var template = new GameObject("texture-binding-template");
        template.SetActive(false);
        var renderer = template.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = source;
        var holder = new GameObject("texture-binding-pool");
        GameObject live = null;
        GameObject clone = null;
        try
        {
            DisFadeRenderRouting.SetTemplateTexture(renderer, a);
            Check(renderer.sharedMaterial.GetTexture(TextureId) == a
                && source.GetTexture(TextureId) == Texture2D.whiteTexture,
                "template texture is serialized separately without mutating the authored source");
            var pool = new Z_UnitSystem.InstancePool(template, holder.transform);
            live = pool.New();
            pool.Fresh(live);
            var liveRenderer = live.GetComponent<MeshRenderer>();
            InvokeBindingCallback(liveRenderer, "OnEnable");
            Check(Texture(liveRenderer, "_Tex") == a,
                "a cloned pool instance rebinds the serialized static template texture");
            var block = new MaterialPropertyBlock();
            block.SetFloat("_Show", .25f);
            block.SetFloat("_FadeCenter", 1f);
            block.SetFloat("_Alpha", .8f);
            block.SetFloat("_LightSensitivity", .43f);
            liveRenderer.SetPropertyBlock(block);
            DisFadeRenderRouting.SetTexture(liveRenderer, b);
            var cached = liveRenderer.sharedMaterial;
            var templateCached = renderer.sharedMaterial;
            DisFadeRenderRouting.Clear();
            Check(cached == null && templateCached == null && Texture(liveRenderer, "_Tex") == b
                && Texture(renderer, "_Tex") == a,
                "clearing the cache releases old materials but preserves live and inactive-template textures");
            liveRenderer.GetPropertyBlock(block);
            Check(block.GetFloat("_Alpha") == .8f && block.GetFloat("_Show") == .25f
                && block.GetFloat("_FadeCenter") == 1f && block.GetFloat("_LightSensitivity") == .43f,
                "cache rebind never clears live numeric instance overrides");
            clone = UnityEngine.Object.Instantiate(template);
            clone.SetActive(true);
            InvokeBindingCallback(clone.GetComponent<MeshRenderer>(), "OnEnable");
            Check(Texture(clone.GetComponent<MeshRenderer>(), "_Tex") == a,
                "cloning an inactive template after cache destruction cannot inherit a missing runtime material");
            pool.Clear(live);
            InvokeBindingCallback(liveRenderer, "OnDisable");
            Check(DisFadeRenderRouting.GetTexture(liveRenderer) == b,
                "pool disable retains the animation frame until the next acquisition");
            pool.Fresh(live);
            InvokeBindingCallback(liveRenderer, "OnEnable");
            Check(Texture(liveRenderer, "_Tex") == a,
                "pool reuse starts from the static template texture without stale animated material state");
        }
        finally
        {
            if (clone != null) UnityEngine.Object.DestroyImmediate(clone);
            if (live != null) UnityEngine.Object.DestroyImmediate(live);
            UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(holder);
            DisFadeRenderRouting.Clear();
            UnityEngine.Object.DestroyImmediate(source);
        }
    }

    private static void RepeatedActiveBindingRebind(Texture a, Texture b)
    {
        var source = new Material(textureTemplate) { name = "repeated-active-rebind-source" };
        var root = new GameObject("repeated-active-rebind");
        var renderer = root.AddComponent<MeshRenderer>();
        renderer.sharedMaterial = source;
        renderer.shadowCastingMode = ShadowCastingMode.On;
        var block = new MaterialPropertyBlock();
        try
        {
            DisFadeRenderRouting.SetTemplateTexture(renderer, a);
            block.SetFloat("_Show", .25f);
            DisFadeRenderRouting.Apply(renderer, block);
            DisFadeRenderRouting.SetTexture(renderer, b);
            var transparent = renderer.sharedMaterial;
            var binding = renderer.GetComponent<DisFadeMaterialBinding>();
            var records = (IDictionary)typeof(DisFadeRenderRouting).GetField("states", PrivateStatic).GetValue(null);
            var originalState = records[renderer];
            Check(originalState.GetType().GetField("assigned") == null,
                "RendererState no longer duplicates the binding's assigned material");
            for (int i = 0; i < 3; i++)
            {
                InvokeBindingCallback(renderer, "OnEnable");
                Check(ReferenceEquals(records[renderer], originalState)
                    && renderer.sharedMaterial == transparent && binding.LastAssigned == transparent
                    && binding.Source == source && Texture(renderer, "_Tex") == b,
                    "active rebind retains state/source/current texture and the shared transparent route");
            }
            block.SetFloat("_Show", 0f);
            DisFadeRenderRouting.Apply(renderer, block);
            InvokeBindingCallback(renderer, "OnEnable");
            Check(renderer.enabled && renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly,
                "active hidden rebind retains the existing shadow-only state");
            DisFadeRenderRouting.Release(renderer);
            Check(renderer.enabled && renderer.shadowCastingMode == ShadowCastingMode.On
                && Texture(renderer, "_Tex") == a && binding.LastAssigned == renderer.sharedMaterial,
                "release after active rebind restores the original template and authored shadow baseline");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            DisFadeRenderRouting.Clear();
            UnityEngine.Object.DestroyImmediate(source);
        }
    }

    private static void DeferredVisibilityAndPoolReuse(Texture a, Texture b)
    {
        var manager = CreateManager<Z_UnitSystem.InstancePoolManager>();
        manager.Init();
        var template = new GameObject("deferred-visibility-template");
        template.SetActive(false);
        var source = new Material(textureTemplate);
        var replacement = new Material(textureTemplate);
        replacement.SetTexture(TextureId, b);
        var holder = new GameObject("deferred-visibility-pool");
        var block = new MaterialPropertyBlock();
        var collider = template.AddComponent<BoxCollider>();
        for (int i = 0; i < 6; i++)
        {
            var child = new GameObject("deferred-renderer-" + i);
            child.transform.SetParent(template.transform, false);
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = source;
            renderer.shadowCastingMode = i == 1 ? ShadowCastingMode.Off : ShadowCastingMode.On;
            block.Clear();
            block.SetFloat("_LightSensitivity", .73f);
            renderer.SetPropertyBlock(block);
            DisFadeRenderRouting.SetTemplateTexture(renderer, a);
        }
        template.AddComponent<TileInstance>();
        var pool = new Z_UnitSystem.InstancePool(template, holder.transform);
        manager.Register(pool);
        try
        {
            var first = NewTile(85901);
            first.prefabName = template.name;
            first.unit.Show();
            var ins = first.unit.ins;
            var root = ins.gameObject;
            var renderer = ins.viewRefreshBindings[0].Target;
            var disabledShadow = ins.viewRefreshBindings[1].Target;
            var front = ins.viewRefreshBindings[3].Target;
            DisFadeRenderRouting.SetTexture(renderer, b);
            ins.ApplyVision(.5f, .25f);
            var material = renderer.sharedMaterial;
            var records = (IDictionary)typeof(DisFadeRenderRouting).GetField("states", PrivateStatic).GetValue(null);
            var owners = (IDictionary)typeof(DisFadeRenderRouting).GetField("materialOwners", PrivateStatic).GetValue(null);
            var originalState = records[renderer];
            int materialCount = owners.Count;
            ins.VisOff();
            renderer.GetPropertyBlock(block);
            Check(renderer.sharedMaterial == material && ReferenceEquals(records[renderer], originalState)
                && owners.Count == materialCount && block.GetFloat("_Show") == 1f
                && block.GetFloat("_FadeCenter") == 1f && renderer.enabled
                && renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly && !disabledShadow.enabled,
                "VisOff only suppresses color: MPB/material/texture/state remain and authored shadows survive");
            front.GetPropertyBlock(block);
            Check(block.GetFloat("_Show") == .25f && block.GetFloat("_FadeCenter") == 0f,
                "VisOff also retains independent Tile-front MPBs");
            ins.VisOff();
            Check(owners.Count == materialCount && ReferenceEquals(records[renderer], originalState),
                "repeated VisOff does not create materials or replace routing state");
            DisFadeRenderRouting.SetTexture(renderer, a);
            Check(renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly && !ins.vising,
                "an animation texture update cannot reveal a VisOff renderer");
            DisFadeRenderRouting.SetTexture(renderer, b);
            DisFadeRenderRouting.Clear();
            Check(renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly && !disabledShadow.enabled,
                "cache retirement preserves visibility-off despite retaining visible numeric MPBs");
            ins.VisOn();
            renderer.GetPropertyBlock(block);
            Check(renderer.enabled && renderer.shadowCastingMode == ShadowCastingMode.On
                && block.GetFloat("_Show") == 1f && block.GetFloat("_FadeCenter") == 0f
                && renderer.sharedMaterial.GetTexture(TextureId) == b,
                "VisOn submits current visible values and restores authored shadow mode without resetting animation");
            DisFadeRenderRouting.SetTextureConfigured(disabledShadow, false);
            ins.VisOff();
            ins.VisOn();
            Check(!disabledShadow.enabled && !DisFadeRenderRouting.IsTextureConfigured(disabledShadow),
                "VisOn does not reveal an intentionally empty texture slot");

            ins.ApplyVision(.5f, .25f);
            material = renderer.sharedMaterial;
            originalState = records[renderer];
            var timer = new Timer { bind = ins };
            ins.animTimer[0] = timer;
            materialCount = owners.Count;
            first.unit.Hide();
            renderer.GetPropertyBlock(block);
            Check(!root.activeSelf && first.unit.ins == null && ins.unit == null && !timer.cancel
                && ins.animTimer[0] == timer && renderer.sharedMaterial == material
                && renderer.sharedMaterial.GetTexture(TextureId) == b && owners.Count == materialCount
                && ReferenceEquals(records[renderer], originalState)
                && block.GetFloat("_Show") == 1f && block.GetFloat("_FadeCenter") == 1f,
                "Hide detaches but retains timer handles, MPBs, material, texture and routing state until Show");
            var timerManager = CreateManager<TimeManager>();
            int timerCalls = 0;
            var timerWork = (IEnumerator)typeof(TimeManager).GetMethod("WorkFrameTimer", PrivateInstance)
                .Invoke(timerManager, new object[] { timer, new Func<bool>(() => { timerCalls++; return false; }) });
            Check(timerWork.MoveNext() && !timerWork.MoveNext() && timerCalls == 0 && !timer.cancel,
                "the production timer bind check stops inactive work without explicit Hide cancellation");
            UnityEngine.Object.DestroyImmediate(timerManager.gameObject);
            var second = NewTile(85902);
            second.prefabName = template.name;
            second.unit.Show();
            renderer.GetPropertyBlock(block);
            Check(second.unit.ins == ins && root.activeSelf && renderer.sharedMaterial.GetTexture(TextureId) == a
                && renderer.shadowCastingMode == ShadowCastingMode.On && block.GetFloat("_Show") == 1f
                && block.GetFloat("_FadeCenter") == 0f && Mathf.Abs(block.GetFloat("_LightSensitivity") - .73f) < .0001f,
                "ordinary Show restores template texture, prefab MPBs and visible state for the next owner");
            DisFadeRenderRouting.SetTextureConfigured(disabledShadow, true);
            using (manager.BeginViewRefreshReuse())
            {
                DisFadeRenderRouting.SetTexture(renderer, b);
                ins.ApplyVision(.5f, .25f);
                material = renderer.sharedMaterial;
                second.unit.Hide();
                Check(root.activeInHierarchy && !root.GetComponent<BoxCollider>().enabled
                    && renderer.sharedMaterial == material && renderer.sharedMaterial.GetTexture(TextureId) == b,
                    "same-refresh return retains appearance while suspending ownerless physical colliders");
                var third = NewTile(85903);
                third.prefabName = template.name;
                third.unit.Show();
                renderer.GetPropertyBlock(block);
                Check(third.unit.ins == ins && root.GetComponent<BoxCollider>().enabled
                    && renderer.sharedMaterial.GetTexture(TextureId) == a
                    && renderer.shadowCastingMode == ShadowCastingMode.On && block.GetFloat("_FadeCenter") == 0f,
                    "same-refresh acquisition restores deferred state before handing the live hierarchy to its next owner");
                var callback = typeof(GameMapController).GetMethod("UpdateTextureAnimation", PrivateInstance);
                var frameType = callback.GetParameters()[4].ParameterType;
                bool oldOwnerFinished = (bool)callback.Invoke(new GameMapController(null),
                    new object[] { ins, second.unit, renderer, 0, Activator.CreateInstance(frameType), .5f, null });
                Check(oldOwnerFinished && !timer.cancel,
                    "an active handoff rejects the old owner's texture callback without cancelling its timer");
                third.unit.Hide();
            }
            Check(!root.activeSelf, "pending-active leftovers still deactivate at the synchronous refresh boundary");
            renderer.sharedMaterial = replacement;
            var fourth = NewTile(85904);
            fourth.prefabName = template.name;
            fourth.unit.Show();
            Check(ins.viewRefreshBindings[0].Source == replacement && renderer.sharedMaterial.GetTexture(TextureId) == b,
                "a source replaced while pooled is captured on Show instead of reviving the old recipe");
            fourth.unit.Hide();
            renderer.sharedMaterial = null;
            var fifth = NewTile(85905);
            fifth.prefabName = template.name;
            fifth.unit.Show();
            Check(renderer.sharedMaterial == null,
                "a material cleared while pooled remains null on the next Show");
            fifth.unit.Hide();
        }
        finally
        {
            manager.Unregister(pool);
            pool.Destroy();
            UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(holder);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
            DisFadeRenderRouting.Clear();
            UnityEngine.Object.DestroyImmediate(source);
            UnityEngine.Object.DestroyImmediate(replacement);
        }
    }

    private static void UnifiedModelBindings()
    {
        var root = new GameObject("unified-model-bindings");
        root.SetActive(false);
        var targets = new List<Renderer>();
        var alternateSource = new Material(textureTemplate);
        try
        {
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject("binding-slot-" + i);
                child.transform.SetParent(root.transform, false);
                child.SetActive(i != 1 && i != 4);
                var target = child.AddComponent<MeshRenderer>();
                target.sharedMaterial = i == 0 ? null : i == 1 ? alternateSource : textureTemplate;
                targets.Add(target);
            }
            var ins = root.AddComponent<TileInstance>();
            var bindings = ins.viewRefreshBindings;
            Check(bindings.Count == 6, "one binding slot exists for every model Renderer before routing");
            for (int i = 0; i < targets.Count; i++)
                Check(bindings[i] != null && bindings[i].Target == targets[i],
                    "binding slots preserve inactive/null/alternate DisFade material order " + i);
            Check(targets[0].sharedMaterial == null && targets[1].sharedMaterial == alternateSource,
                "obtaining bindings alone never changes null/alternate DisFade materials");
            ins.ApplyVision(0.5f, 1f);
            for (int i = 0; i < 6; i++)
            {
                targets[i].GetPropertyBlock(readBlock);
                Check(readBlock.GetFloat("_Show") == 1f
                    && readBlock.GetFloat("_FadeCenter") == (i < 3 ? 1f : 0f),
                    "binding-only visibility preserves body/front slots " + i);
            }
            ins.ClearRendererProperties();
            for (int i = 0; i < 6; i++)
            {
                targets[i].GetPropertyBlock(readBlock);
                Check(readBlock.isEmpty, "binding-only Hide clears every model property block " + i);
            }
            Check(ReferenceEquals(bindings, ins.viewRefreshBindings),
                "visibility/cleanup reuse the same single binding list");
            var appended = new GameObject("runtime-extra-renderer");
            appended.transform.SetParent(root.transform, false);
            var extraRenderer = appended.AddComponent<MeshRenderer>();
            Check(!ins.SupportsViewRefreshReuse && bindings.Count == 6,
                "Renderer added after slot caching uses native lifecycle without shifting model slots");
            extraRenderer.sharedMaterial = textureTemplate;
            DisFadeRenderRouting.SetTexture(extraRenderer, textureTemplate.GetTexture(TextureId));
            Check(!ins.SupportsViewRefreshReuse,
                "an additional runtime binding cannot bypass the fixed model-slot safety check");
            UnityEngine.Object.DestroyImmediate(appended);
            Check(ins.SupportsViewRefreshReuse, "removing the appended renderer restores audited reuse");
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(alternateSource);
        }
    }

    private static void TileViewRefreshReuse(GameMapController ctrl, Texture a, Texture b)
    {
        var manager = CreateManager<Z_UnitSystem.InstancePoolManager>();
        manager.Init();
        var holder = new GameObject("tile-view-reuse-pool");
        var template = new GameObject("tile-view-reuse-template");
        template.SetActive(false);
        var source = new Material(textureTemplate.shader) { name = "tile-view-reuse-source" };
        source.SetFloat("_Alpha", 1f);
        source.SetFloat("_Show", 1f);
        source.SetFloat("_LightSensitivity", .8f);
        var block = new MaterialPropertyBlock();
        for (int i = 0; i < 6; i++)
        {
            var child = new GameObject("tile-layer-" + i);
            child.transform.SetParent(template.transform, false);
            var renderer = child.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = source;
            renderer.shadowCastingMode = ShadowCastingMode.On;
            block.Clear();
            block.SetFloat("_LightSensitivity", .8f);
            renderer.SetPropertyBlock(block);
            DisFadeRenderRouting.SetTemplateTexture(renderer, a);
        }
        template.AddComponent<BoxCollider>();
        var templateInstance = template.AddComponent<TileInstance>();
        var instancePool = new Z_UnitSystem.InstancePool(template, holder.transform);
        manager.Register(instancePool);
        TileUnit previous = null;
        GameObject root = null;
        try
        {
            Check(templateInstance.SupportsViewRefreshReuse, "plain audited TileInstance opts in without a test probe component");
            var inactivePart = new GameObject("inactive-reuse-part");
            inactivePart.transform.SetParent(template.transform, false);
            inactivePart.SetActive(false);
            inactivePart.AddComponent<MeshFilter>();
            var keeper = inactivePart.AddComponent<PerspectiveKeeper>();
            Check(templateInstance.SupportsViewRefreshReuse,
                "typed reuse collection includes inactive audited children");
            var bindingsField = typeof(MapInstance).GetField("_viewRefreshBindings", PrivateInstance);
            var keepersField = typeof(MapInstance).GetField("viewRefreshKeepers", PrivateInstance);
            var bindings = (List<DisFadeMaterialBinding>)bindingsField.GetValue(templateInstance);
            var keepers = (List<PerspectiveKeeper>)keepersField.GetValue(templateInstance);
            Check(bindings.Count == 6 && keepers.Count == 1 && keepers[0] == keeper,
                "material bindings and perspective keepers have separate strongly typed lists");
            Check(templateInstance.SupportsViewRefreshReuse
                && ReferenceEquals(bindings, bindingsField.GetValue(templateInstance))
                && ReferenceEquals(keepers, keepersField.GetValue(templateInstance))
                && bindings.Count == 6 && keepers.Count == 1,
                "repeated audits reuse typed lists without accumulating entries");
            var unknownNative = inactivePart.AddComponent<Light>();
            Check(!templateInstance.SupportsViewRefreshReuse,
                "runtime native component on an inactive child still rejects active reuse");
            UnityEngine.Object.DestroyImmediate(unknownNative);
            var nestedInstance = inactivePart.AddComponent<TileInstance>();
            Check(!templateInstance.SupportsViewRefreshReuse,
                "typed collection rejects another TileInstance nested under its owner");
            UnityEngine.Object.DestroyImmediate(nestedInstance);
            Check(templateInstance.SupportsViewRefreshReuse,
                "removing unaudited components restores eligibility on the next audit");
            var first = NewTile(84901);
            first.prefabName = template.name;
            first.unit.Show();
            previous = first.unit;
            root = previous.ins.gameObject;
            var ins = previous.ins;
            Check(ins.SupportsViewRefreshReuse && root.activeInHierarchy, "real pooled Tile remains eligible for active view reuse");
            ctrl.ShowFinalMat(ins, 0, new List<int> { 81001, 81002 }, .5f);
            var oldTimer = ins.animTimer[0];
            Check(oldTimer != null, "old Tile owns a real animated texture timer");
            ins.ApplyVision(0f);
            Check(ins.viewRefreshBindings[0].Target.shadowCastingMode == ShadowCastingMode.ShadowsOnly,
                "old hidden Tile exercises the routed shadow-only state");
            using (manager.BeginViewRefreshReuse())
            {
                previous.Hide();
                Check(root.activeInHierarchy && previous.ins == null && ins.unit == null,
                    "real Tile Hide detaches immediately without deactivating the pending root");
                Check(oldTimer.cancel && ins.animTimer[0] == null,
                    "handoff explicitly cancels old owner animation without OnDisable");
                Check(DisFadeRenderRouting.GetTexture(ins.viewRefreshBindings[0].Target) == a
                    && ins.viewRefreshBindings[0].Target.shadowCastingMode == ShadowCastingMode.On,
                    "handoff releases animated material and restores the authored shadow baseline");
                for (int reuse = 0; reuse < 4; reuse++)
                {
                    var next = NewTile(84902 + reuse);
                    next.prefabName = template.name;
                    next.pos = Vector3.right * (reuse + 1);
                    next.unit.Show();
                    Check(next.unit.ins == ins && ins.unit == next.unit && root.activeInHierarchy,
                        "new Tile owner reuses the same live hierarchy " + reuse);
                    ins.displayLayer = int.MaxValue;
                    ins.ApplyVision(1f);
                    ctrl.ShowFinalMat(ins, 0, reuse % 2 == 0 ? null : new List<int> { 81002 }, 0f);
                    Check(ins.viewRefreshBindings[0].Target.enabled == (reuse % 2 != 0),
                        "an empty next-owner texture never retains the previous owner's image " + reuse);
                    if (reuse % 2 != 0)
                    {
                        ins.viewRefreshBindings[0].Target.GetPropertyBlock(block);
                        Check(Texture(ins.viewRefreshBindings[0].Target, "_Tex") == b
                            && ins.viewRefreshBindings[0].Target.shadowCastingMode == ShadowCastingMode.On
                            && block.GetFloat("_Show") == 1f && block.GetFloat("_FadeCenter") == 0f
                            && Mathf.Abs(block.GetFloat("_LightSensitivity") - .8f) < .0001f,
                            "visible replacement restores texture, fade, lighting and non-shadow-only rendering " + reuse);
                    }
                    previous = next.unit;
                    previous.Hide();
                    Check(root.activeInHierarchy && ins.animTimer.All(timer => timer == null),
                        "successive active handoffs leave no owner-bound animation timer " + reuse);
                }
            }
            Check(!root.activeSelf, "unused real Tile root is disabled at the transaction boundary");

            var custom = manager.CreateInstance(template);
            custom.AddComponent<RegressionUnknownTileBehaviour>();
            Check(!custom.GetComponent<TileInstance>().SupportsViewRefreshReuse,
                "adding an unaudited MonoBehaviour after first use disables the fast lifecycle path");
            using (manager.BeginViewRefreshReuse())
            {
                manager.DeleteInstance(custom, template);
                Check(!custom.activeSelf, "unknown Tile behaviour falls back to native disable immediately");
            }
        }
        finally
        {
            if (previous != null && previous.ins != null) previous.Hide();
            instancePool.Clear();
            manager.Unregister(instancePool);
            UnityEngine.Object.DestroyImmediate(holder);
            UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(manager.gameObject);
            DisFadeRenderRouting.Clear();
            UnityEngine.Object.DestroyImmediate(source);
        }
    }

    private static void CharacterTemplateFirstShow()
    {
        // Match GameSaveController's initially empty, disabled character template,
        // then use the actual GameUtil, pool, visibility and animation entry points.
        GameObjectAssetForm.InitInternal();
        var game = CreateManager<GameManager>();
        var sphere = new GameObject("MapPrefab$sphere");
        var rotate = new GameObject("rotateHolder");
        rotate.transform.SetParent(sphere.transform, false);
        var image = new GameObject("img");
        image.transform.SetParent(rotate.transform, false);
        var originalRenderer = image.AddComponent<MeshRenderer>();
        originalRenderer.sharedMaterial = textureTemplate;
        originalRenderer.shadowCastingMode = ShadowCastingMode.On;
        image.AddComponent<PerspectiveKeeper>().rotateHolder = rotate.transform;
        var collider = new GameObject("Collider");
        collider.transform.SetParent(sphere.transform, false);
        collider.AddComponent<SphereCollider>().radius = .5f;
        var asset = new GameObjectAssetForm.Data(84701, sphere.name, null, null, null, sphere, 0);
        GameObjectAssetForm.DataById[asset.id] = asset;
        game.innerAssetDic = new Dictionary<string, AssetForm.Data> { { "sphere", asset } };
        var util = new GameUtilController(game);
        GameObject template = null, live = null;
        var poolRoot = new GameObject("character-first-show-pool");
        var data = new CharacterUnitForm.Data(84702, false, Vector3.zero, 0, 0, 0, false,
            "", "", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", false, 0, null);
        var product = new CharacterProductForm.Data(84703, "first-show-character", 0, 0, null, 0,
            new Dictionary<string, CharacterAnimForm.Data>(), null, FaceType.Fixed, "", "", null, null,
            "", 0, false, null, 0, false, 0, 1, null);
        CharacterProductForm.DataByUid[product.uid] = product;
        data.unit.productInfo = (product.uid, -1);
        var parts = new Dictionary<BodyPartType, bool> { { BodyPartType.UpperPart, true }, { BodyPartType.LowerPart, true } };
        var idle = new CharacterAnimForm.Data(84704, "idle", new Dictionary<AnimDirecton, List<CharacterAnimClipForm.Data>> {
            { AnimDirecton.Fixed, new List<CharacterAnimClipForm.Data> {
                new CharacterAnimClipForm.Data(0, null, null, new Dictionary<BodyPartType, int> {
                    { BodyPartType.UpperPart, 81001 }, { BodyPartType.LowerPart, 81002 } }) } } }, 0, 1, parts);
        product.animDic["idle"] = idle;
        var anim = new GameCharacterController.AnimController { data = data, idleAnim = idle, moveAnim = idle };
        try
        {
            template = util.CombineNewCharacterByPrefabs("runtime$character", new List<int> {
                GlobalDefaultHelper.DefaultTexId, GlobalDefaultHelper.DefaultTexId }, true);
            // Non-ExecuteAlways production callbacks do not run in this EditMode fixture.
            foreach (var render in template.GetComponentsInChildren<Renderer>(true))
                InvokeBindingCallback(render, "OnDisable");
            // Map/story cache retirement must not bake a temporary ShadowsOnly
            // route into the inactive template inherited by the next clone.
            DisFadeRenderRouting.Clear();
            DisFadeRenderRouting.Clear();
            var routingStates = (IDictionary)typeof(DisFadeRenderRouting).GetField("states", PrivateStatic).GetValue(null);
            foreach (var render in template.GetComponentsInChildren<Renderer>(true))
                Check(!render.enabled && render.shadowCastingMode == ShadowCastingMode.On && !routingStates.Contains(render),
                    "repeated cache retirement leaves an inactive empty character template unrouted and preserves its authored baseline");
            var pool = new Z_UnitSystem.InstancePool(template, poolRoot.transform);
            live = pool.New();
            var ins = live.GetComponent<CharacterInstance>();
            for (int reuse = 0; reuse < 3; reuse++)
            {
                pool.Fresh(live);
                foreach (var render in live.GetComponentsInChildren<Renderer>(true))
                    InvokeBindingCallback(render, "OnEnable");
                data.unit.ins = ins;
                ins.unit = data.unit;
                if (!Application.isPlaying)
                    typeof(MapInstance).GetMethod("OnEnable", PrivateInstance).Invoke(ins, null);
                ins.VisOn();
                anim.Reset();
                anim.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart);
                anim.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.LowerPart);
                anim.CloseHideRenderer();
                for (int i = 0; i < 2; i++)
                {
                    var render = ins.viewRefreshBindings[i].Target;
                    render.GetPropertyBlock(readBlock);
                    Check(render.enabled && render.shadowCastingMode != ShadowCastingMode.ShadowsOnly
                        && readBlock.GetFloat("_Show") == 1 && Texture(render, "_Tex") == TexAssetForm.DataById[81001 + i].GetTex(),
                        "real empty character template displays both animated parts on first Show and reuse " + reuse + "/" + i
                        + " enabled=" + render.enabled + " caster=" + render.shadowCastingMode + " show=" + readBlock.GetFloat("_Show"));
                }
                ins.VisOff();
                ins.ClearRendererProperties();
                pool.Clear(live);
                foreach (var render in live.GetComponentsInChildren<Renderer>(true))
                    InvokeBindingCallback(render, "OnDisable");
                if (!Application.isPlaying)
                    typeof(MapInstance).GetMethod("OnDisable", PrivateInstance).Invoke(ins, null);
            }
        }
        finally
        {
            anim.CancelTimers();
            if (live != null) UnityEngine.Object.DestroyImmediate(live);
            if (template != null) UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(poolRoot);
            UnityEngine.Object.DestroyImmediate(sphere);
            UnityEngine.Object.DestroyImmediate(game.gameObject);
            CharacterProductForm.DataByUid.Remove(product.uid);
            GameObjectAssetForm.DataById.Remove(asset.id);
            DisFadeRenderRouting.Clear();
        }
    }

    private static void InvokeBindingCallback(Renderer renderer, string callback)
    {
        // The fixture is EditMode. Exercise the actual production callback,
        // which a normal non-ExecuteAlways component receives in Play only.
        if (Application.isPlaying) return;
        var binding = renderer.GetComponent<DisFadeMaterialBinding>();
        if (binding == null || binding.GetType().IsDefined(typeof(ExecuteAlways), true)) return;
        var method = binding.GetType().GetMethod(callback, PrivateInstance);
        Check(method != null, "binding lifecycle callback exists: " + callback);
        method.Invoke(binding, null);
    }

    private static void ExternalCachedMaterialChanges(Texture a, Texture b)
    {
        var sourceA = new Material(textureTemplate.shader) { name = "external-cache-source-a" };
        var sourceB = new Material(textureTemplate.shader) { name = "external-cache-source-b" };
        var external = new Material(textureTemplate.shader) { name = "external-uncached-source" };
        foreach (var source in new[] { sourceA, sourceB, external })
        {
            source.SetFloat("_Alpha", 1f);
            source.SetFloat("_Show", 1f);
        }
        sourceA.SetTexture(TextureId, a);
        sourceB.SetTexture(TextureId, a);
        sourceB.SetFloat("_LightSensitivity", .37f);
        external.SetTexture(TextureId, b);
        var rootA = new GameObject("external-material-copy-a");
        var rootB = new GameObject("external-material-copy-b");
        var rendererA = rootA.AddComponent<MeshRenderer>();
        var rendererB = rootB.AddComponent<MeshRenderer>();
        rendererA.sharedMaterial = sourceA;
        rendererB.sharedMaterial = sourceB;
        rendererA.shadowCastingMode = ShadowCastingMode.Off;
        rendererB.shadowCastingMode = ShadowCastingMode.TwoSided;
        try
        {
            DisFadeRenderRouting.SetTexture(rendererA, a);
            DisFadeRenderRouting.SetTexture(rendererB, b);
            var copiedMaterial = rendererB.sharedMaterial;
            rendererA.sharedMaterial = copiedMaterial;
            // Deliberately skip Apply/Refresh: clear must recognize the runtime
            // material's true source/texture before retiring its generation.
            DisFadeRenderRouting.Clear();
            Check(copiedMaterial == null, "cache clear releases the externally copied material's old generation");
            Check(Texture(rendererA, "_Tex") == b,
                "cache clear retains the externally copied texture rather than the destination's old texture");
            Check(rendererA.GetComponent<DisFadeMaterialBinding>().Source == sourceB,
                "cache clear rebases the externally copied material to its true source");
            Check(rendererA.sharedMaterial == rendererB.sharedMaterial,
                "cache clear continues sharing the same source/texture/route after an external material copy: "
                + rendererA.sharedMaterial.name + " / " + rendererB.sharedMaterial.name);
            Check(rendererA.shadowCastingMode == ShadowCastingMode.Off
                && rendererB.shadowCastingMode == ShadowCastingMode.TwoSided
                && rendererA.sharedMaterial.GetFloat("_LightSensitivity") == .37f,
                "copying a cached material shares appearance without changing each renderer's authored shadow mode");
            rootA.SetActive(false);
            InvokeBindingCallback(rendererA, "OnDisable");
            rootA.SetActive(true);
            InvokeBindingCallback(rendererA, "OnEnable");
            Check(Texture(rendererA, "_Tex") == b
                && rendererA.GetComponent<DisFadeMaterialBinding>().Source == sourceB,
                "copied cached texture remains the new static recipe across pool disable/re-enable");

            rendererA.sharedMaterial = external;
            DisFadeRenderRouting.Clear();
            rootA.SetActive(false);
            InvokeBindingCallback(rendererA, "OnDisable");
            rootA.SetActive(true);
            InvokeBindingCallback(rendererA, "OnEnable");
            Check(rendererA.sharedMaterial == external && Texture(rendererA, "_Tex") == b,
                "uncached external source replacement survives Clear and re-enable without reviving its old source");

            // Changing shader types is outside the DisFade-only contract;
            // destruction/null sources remain supported lifecycle boundaries.
            foreach (bool releaseOnly in new[] { false, true })
            {
                var destroyedSource = new Material(textureTemplate);
                rendererA.sharedMaterial = destroyedSource;
                DisFadeRenderRouting.SetTexture(rendererA, b);
                UnityEngine.Object.DestroyImmediate(destroyedSource);
                if (releaseOnly) DisFadeRenderRouting.Release(rendererA);
                else DisFadeRenderRouting.Clear();
                InvokeBindingCallback(rendererA, "OnEnable");
                Check(rendererA.sharedMaterial == null,
                    "source destroyed before " + (releaseOnly ? "Release" : "Clear")
                    + " is not queried or revived on re-enable");
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(rootA);
            UnityEngine.Object.DestroyImmediate(rootB);
            DisFadeRenderRouting.Clear();
            UnityEngine.Object.DestroyImmediate(sourceA);
            UnityEngine.Object.DestroyImmediate(sourceB);
            UnityEngine.Object.DestroyImmediate(external);
        }
    }

    private static void NeverUsedInactiveRendererCache(Material source)
    {
        // Never create model binding components from disable/destroy callbacks.
        // The first real access must still include initially inactive parts.
        foreach (string owner in new[] { "character", "item" })
        {
            var root = new GameObject("never-used-" + owner);
            root.SetActive(false);
            try
            {
                var child = new GameObject("inactive-image");
                child.transform.SetParent(root.transform, false);
                var renderer = child.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = source;
                child.SetActive(false);
                var ins = root.AddComponent<RegressionUnvisitedMapInstance>();
                root.SetActive(true);
                root.SetActive(false);
                Check(typeof(MapInstance).GetField("_viewRefreshBindings", PrivateInstance).GetValue(ins) == null
                    && child.GetComponent<DisFadeMaterialBinding>() == null,
                    "never-routed " + owner + " disable does not create/cache bindings");
                child.SetActive(true);
                root.SetActive(true);
                Check(ins.viewRefreshBindings.Count == 1 && ins.viewRefreshBindings[0].Target == renderer,
                    "previously inactive " + owner + " image is discovered when its first appearance is ready");
            }
            finally { UnityEngine.Object.DestroyImmediate(root); }
        }
    }

    private static Texture2D AddTexture(int id)
    {
        var texture = new Texture2D(2, 2);
        TexAssetForm.DataById[id] = new TexAssetForm.Data(id, "test", "", null, "", texture, 0);
        return texture;
    }

    private static TileUnitForm.Data NewTile(int uid)
    {
        return new TileUnitForm.Data(uid, "", new Dictionary<int, int>(), Vector3Int.zero,
            "", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), "", false, false, new List<int>());
    }

    private static void PerspectiveImageScaleAfterRotation()
    {
        var previousMode = DynamicGlobalSettings.cameraMode;
        var root = new GameObject("perspective-object-instance");
        try
        {
            var primitive = new GameObject("MapPrefab$cube");
            primitive.transform.SetParent(root.transform, false);
            var body = new GameObject("Collider");
            body.transform.SetParent(primitive.transform, false);
            var collider = body.AddComponent<BoxCollider>();
            var holder = new GameObject("rotateHolder");
            holder.transform.SetParent(primitive.transform, false);
            var image = new GameObject("img");
            image.transform.SetParent(holder.transform, false);
            var authoredScale = new Vector3(.8f, 1.2f, 1f);
            image.transform.localScale = authoredScale;
            var keeper = image.AddComponent<PerspectiveKeeper>();
            keeper.rotateHolder = holder.transform;
            keeper.enableFixedXRotationDefault = true;
            keeper.deepth = .07f;

            foreach (var rootScale in new[] { Vector3.one, new Vector3(2, 3, .5f) })
            foreach (var modelScale in new[] { Vector3.one, new Vector3(1, .1f, 1), new Vector3(1, 10, 1), new Vector3(2, 1, 8), new Vector3(3, .25f, .1f) })
            foreach (float yaw in new[] { 0f, 37f, 90f, 180f, 270f })
            foreach (bool fixedPitch in new[] { false, true })
            foreach (var mode in new[] { CameraMode.Isometric, CameraMode.Overhead, CameraMode.Isometric })
            {
                root.transform.localScale = rootScale;
                root.transform.rotation = Quaternion.Euler(8, yaw, 7);
                primitive.transform.localScale = modelScale;
                keeper.enableFixedXRotationDefault = fixedPitch;
                DynamicGlobalSettings.cameraMode = mode;
                Vector3 size = primitive.transform.lossyScale;
                float pitch = mode == CameraMode.Isometric
                    ? Mathf.Atan2(Mathf.Abs(size.z), Mathf.Abs(size.y)) * Mathf.Rad2Deg : 90f;
                var rotation = Quaternion.Euler(pitch, 0, 0)
                    * Quaternion.AngleAxis(-primitive.transform.eulerAngles.y, Vector3.forward);
                float imageHeight = mode == CameraMode.Isometric
                    ? Mathf.Sqrt(size.y * size.y + size.z * size.z) : Mathf.Abs(size.z);
                for (int repeat = 0; repeat < 2; repeat++)
                {
                    keeper.RefreshNow();
                    Check(image.transform.localRotation == Quaternion.identity && holder.transform.localScale == Vector3.one,
                        "outer rotateHolder owns rotation, inner img owns scale");
                    Check(holder.transform.parent.name == "imageScaleCompensator" && primitive.transform.childCount == 2,
                        "one owned scale compensator is reused without changing model/Collider hierarchy");
                    var expectedPosition = mode == CameraMode.Isometric
                        ? primitive.transform.position + new Vector3(0, 1, -1) * keeper.deepth
                        : primitive.transform.position + Vector3.up * primitive.transform.localScale.y / 2 + Vector3.down * keeper.deepth;
                    Check((holder.transform.position - expectedPosition).sqrMagnitude < .000001f,
                        "scale compensation never rescales the authored world depth/height offset");
                    Vector3 actualRight = image.transform.TransformVector(Vector3.right);
                    Vector3 actualUp = image.transform.TransformVector(Vector3.up);
                    Check(Vector3.Angle(actualRight, rotation * Vector3.right) < .05f,
                        "model/holder scale must not change the actual image right direction");
                    Check(Vector3.Angle(actualUp, rotation * Vector3.up) < .05f,
                        "model/holder scale must not change the actual image up direction or pitch");
                    Check(Mathf.Abs(Vector3.Dot(actualRight.normalized, actualUp.normalized)) < .0001f,
                        "the rendered image plane remains rectangular rather than sheared");
                    Check(Mathf.Abs(actualRight.magnitude - Mathf.Abs(size.x) * authoredScale.x) < .0001f
                        && Mathf.Abs(actualUp.magnitude - imageHeight * authoredScale.y) < .0001f,
                        "scale applies along rotated image axes and preserves width/diagonal height without accumulation");
                    Check(body.transform.localPosition == Vector3.zero && body.transform.localScale == Vector3.one
                        && collider.center == Vector3.zero && collider.size == Vector3.one,
                        "image transform correction never modifies the physical Collider");
                }
            }
            // Independently verify the reported 1 x 0.1 x 1 bridge against
            // its diagonal vector, not merely the same angle formula.
            root.transform.localScale = Vector3.one;
            root.transform.rotation = Quaternion.identity;
            primitive.transform.localScale = new Vector3(1, .1f, 1);
            DynamicGlobalSettings.cameraMode = CameraMode.Isometric;
            keeper.RefreshNow();
            Check(Quaternion.Angle(holder.transform.rotation, Quaternion.Euler(84.2894f, 0, 0)) < .001f,
                "thin bridge uses about 84.29 degrees rather than the square's 45-degree pitch");
            Check((image.transform.TransformVector(Vector3.up)
                - new Vector3(0, .1f, 1) * authoredScale.y).sqrMagnitude < .000001f,
                "bridge image up axis spans its actual height/depth diagonal");
            primitive.transform.localScale = new Vector3(1, 1, .1f);
            keeper.LateUpdate();
            Check((image.transform.TransformVector(Vector3.up)
                - new Vector3(0, 1, .1f) * authoredScale.y).sqrMagnitude < .000001f,
                "LateUpdate immediately recomputes pitch when model aspect ratio changes");
            var compensator = holder.transform.parent;
            root.SetActive(false);
            root.SetActive(true);
            keeper.LateUpdate();
            Check(holder.transform.parent == compensator && primitive.transform.childCount == 2,
                "pool re-enable reuses the same neutral carrier");
            var refreshedScale = image.transform.localScale;
            var clone = UnityEngine.Object.Instantiate(root);
            try
            {
                var clonedKeeper = clone.GetComponentInChildren<PerspectiveKeeper>();
                clonedKeeper.RefreshNow();
                Check((clonedKeeper.transform.localScale - refreshedScale).sqrMagnitude < .000001f
                    && clonedKeeper.rotateHolder.parent.parent.childCount == 2,
                    "cloning a refreshed hierarchy retains the authored image scale and existing carrier");
            }
            finally { UnityEngine.Object.DestroyImmediate(clone); }

            // Characters share this helper through the sphere prefab. Their
            // diameter-sized square and direction must remain unchanged.
            var sphereRoot = new GameObject("MapPrefab$sphere");
            sphereRoot.transform.SetParent(root.transform, false);
            root.transform.localScale = Vector3.one * 2;
            root.transform.rotation = Quaternion.Euler(0, 37, 0);
            sphereRoot.transform.localScale = new Vector3(1, 2, 1);
            var sphereCollider = sphereRoot.AddComponent<SphereCollider>();
            sphereCollider.radius = .4f;
            var sphereHolder = new GameObject("rotateHolder");
            sphereHolder.transform.SetParent(sphereRoot.transform, false);
            var sphereImage = new GameObject("img");
            sphereImage.transform.SetParent(sphereHolder.transform, false);
            var sphereKeeper = sphereImage.AddComponent<PerspectiveKeeper>();
            sphereKeeper.rotateHolder = sphereHolder.transform;
            sphereKeeper.RefreshNow();
            Vector3 sphereRight = sphereImage.transform.TransformVector(Vector3.right);
            Vector3 sphereUp = sphereImage.transform.TransformVector(Vector3.up);
            var sphereRotation = Quaternion.Euler(45, 0, 0) * Quaternion.AngleAxis(-37, Vector3.forward);
            Check(Vector3.Angle(sphereRight, sphereRotation * Vector3.right) < .05f
                && Vector3.Angle(sphereUp, sphereRotation * Vector3.up) < .05f
                && Mathf.Abs(sphereRight.magnitude - 4) < .0001f && Mathf.Abs(sphereUp.magnitude - 4) < .0001f,
                "sphere/character keeps its original diameter square and image-plane direction");
            Check(sphereCollider.radius == .4f && sphereRoot.transform.localScale == new Vector3(1, 2, 1),
                "sphere visual correction never resizes the gameplay body or authored model");
        }
        finally
        {
            DynamicGlobalSettings.cameraMode = previousMode;
            UnityEngine.Object.DestroyImmediate(root);
        }
    }

    private static Texture Texture(Renderer renderer, string property)
    {
        renderer.GetPropertyBlock(readBlock);
        if (renderer.sharedMaterial != null && renderer.sharedMaterial.shader == textureTemplate.shader)
        {
            Check(!readBlock.HasTexture(TextureId) && !readBlock.HasTexture(LegacyMaskId),
                "DisFade texture bindings live only on shared materials, never on the instancing MPB");
            return property == "_Tex" ? renderer.sharedMaterial.GetTexture(TextureId) : null;
        }
        return readBlock.GetTexture(property);
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var go = new GameObject(typeof(T).Name);
        go.SetActive(false);
        var manager = go.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic).SetValue(null, manager);
        return manager;
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }
}

[ExecuteAlways]
public class RegressionRoutingTileInstance : TileInstance
{
}

[ExecuteAlways]
public class RegressionUnvisitedMapInstance : MapInstance
{
}

public class RegressionUnknownTileBehaviour : MonoBehaviour
{
}
