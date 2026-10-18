using System;
using System.Collections.Generic;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Map;
using Z_Map.Form;
using Z_Texture;
using Z_Time;

// Run through the isolated project harness; never load a production scene/save.
public static class DisFadeRoutingRegression
{
    private static int checks;
    private const string Forward = "UniversalForward";
    private const string Opaque = "DisFadeOpaque";

    public static void Run()
    {
        GameObject root = null;
        Material source = null;
        Texture2D first = null, second = null;
        try
        {
            var shader = Shader.Find("DisFadeCode");
            Check(shader != null, "production shader imported");
            source = new Material(shader) { name = "routing-source", renderQueue = 4100 };
            Check(source.FindPass("Universal Forward") >= 0 && source.FindPass(Forward) < 0,
                "FindPass uses the ShaderLab Name while pass switches use its LightMode");
            source.SetFloat("_Alpha", 1f);
            source.SetFloat("_Show", 1f);
            source.SetFloat("_FadeCenter", 0f);
            first = new Texture2D(2, 2);
            second = new Texture2D(2, 2);
            source.SetTexture("_Tex", first);

            root = new GameObject("routing-tile");
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject("layer" + i);
                child.transform.SetParent(root.transform, false);
                child.AddComponent<MeshRenderer>().sharedMaterial = source;
            }
            var tile = root.AddComponent<TileInstance>();
            var body = tile.renderers[0];
            var front = tile.renderers[3];
            var block = new MaterialPropertyBlock();

            tile.ApplyVision(1f, 1f);
            CheckRoute(body, "Opaque");
            var opaque = body.sharedMaterial;
            tile.ApplyVision(.25f, .25f);
            CheckRoute(body, "Transparent");
            Check(!body.sharedMaterial.GetShaderPassEnabled(Opaque), "transparent skips opaque pass");
            var transparent = body.sharedMaterial;
            tile.ApplyVision(.5f, .5f);
            CheckUniformTransparency(body, .5f);
            CheckUniformTransparency(front, .5f);
            Check(body.sharedMaterial == transparent && front.sharedMaterial == transparent,
                "half occlusion reuses the ordinary transparent material");

            // Explicit mixed routing is independent of map occlusion.
            body.GetPropertyBlock(block);
            block.SetFloat("_Show", 1f);
            block.SetFloat("_FadeCenter", 1f);
            DisFadeRenderRouting.Apply(body, block, DisFadeRenderRouting.Route.Mixed);
            CheckRoute(body, "Mixed");
            Check(body.sharedMaterial.GetShaderPassEnabled(Opaque)
                && body.sharedMaterial.GetShaderPassEnabled(Forward), "mixed retains both color passes");
            var mixed = body.sharedMaterial;
            tile.ApplyVision(1f, .5f);
            Check(body.sharedMaterial == opaque && front.sharedMaterial == transparent,
                "tile body and front choose independent routes and reuse cached variants");
            Check(tile.GetRendererRoute(0) == DisFadeRenderRouting.Route.Opaque
                && tile.GetRendererRoute(3) == DisFadeRenderRouting.Route.Transparent,
                "tile body and front retain independent cached routes");
            body.GetPropertyBlock(block);
            Check(block.GetFloat("_Show") == 1f && block.GetFloat("_FadeCenter") == 0f,
                "map visibility clears a previous mixed override when restoring opacity");
            CheckUniformTransparency(front, .5f);

            DisFadeRenderRouting.BeginSplitCamera();
            Check(!opaque.GetShaderPassEnabled(Forward), "split camera suppresses actual opaque forward pass");
            Check(transparent.GetShaderPassEnabled(Forward) && mixed.GetShaderPassEnabled(Forward),
                "split camera retains transparent/mixed forward passes");
            DisFadeRenderRouting.SetTexture(front, second, tile.GetRendererRoute(3));
            CheckUniformTransparency(front, .5f);
            Check(front.sharedMaterial.GetTexture("_Tex") == second, "half-occluded texture changed");
            Check(front.sharedMaterial != transparent && transparent.GetTexture("_Tex") == first,
                "changing texture replaces the material without mutating the old transparent material");
            var secondTransparent = front.sharedMaterial;
            DisFadeRenderRouting.SetTexture(body, second, tile.GetRendererRoute(0));
            CheckRoute(body, "Opaque");
            Check(body.sharedMaterial != opaque && opaque.GetTexture("_Tex") == first,
                "changing texture preserves the old opaque material's binding");
            var secondOpaque = body.sharedMaterial;
            DisFadeRenderRouting.EndSplitCamera();
            Check(body.sharedMaterial.GetShaderPassEnabled(Forward), "camera cleanup restores opaque fallback");

            tile.ApplyVision(.25f, .25f);
            DisFadeRenderRouting.SetTexture(body, first, tile.GetRendererRoute(0));
            CheckRoute(body, "Transparent");
            Check(body.sharedMaterial == transparent, "texture changes preserve route and original source cache");
            Check(opaque.GetTexture("_Tex") == first && transparent.GetTexture("_Tex") == first
                && mixed.GetTexture("_Tex") == first && secondTransparent.GetTexture("_Tex") == second
                && secondOpaque.GetTexture("_Tex") == second,
                "all previously created materials retain their texture across route and texture changes");
            tile.ApplyVision(1f, 1f);
            body.GetPropertyBlock(block);
            block.SetFloat("_Alpha", .4f);
            DisFadeRenderRouting.Apply(body, block, DisFadeRenderRouting.Route.Transparent);
            CheckRoute(body, "Transparent");
            Check(tile.GetRendererRoute(0) == DisFadeRenderRouting.Route.Opaque,
                "editing the property block does not reclassify the cached map route");
            DisFadeRenderRouting.SetTexture(body, first, tile.GetRendererRoute(0));
            CheckRoute(body, "Opaque"); // Cached caller route wins even though _Alpha is still .4.
            block.SetFloat("_Alpha", 1f);
            block.SetFloat("_FadeCenter", 1f);
            DisFadeRenderRouting.Apply(body, block, DisFadeRenderRouting.Route.Mixed);
            CheckRoute(body, "Mixed");
            Check(body.sharedMaterial.GetTexture("_Tex") == first, "scalar changes retain texture");

            var bodyEnabled = body.enabled;
            var bodyShadowMode = body.shadowCastingMode;
            tile.ApplyVision(0f, 0f);
            body.GetPropertyBlock(block);
            Check(block.GetFloat("_Show") == 0f, "hidden state reaches renderer");
            Check(tile.GetRendererRoute(0) == DisFadeRenderRouting.Route.Hidden
                && tile.GetRendererRoute(3) == DisFadeRenderRouting.Route.Hidden,
                "hidden state replaces both cached routes");
            CheckHidden(body);
            CheckHidden(front);
            var firstHidden = body.sharedMaterial;
            var secondHidden = front.sharedMaterial;
            Check(firstHidden != mixed && firstHidden != transparent && secondHidden != secondTransparent,
                "hidden and visible routes use distinct cached materials");
            Check(body.enabled == bodyEnabled && body.shadowCastingMode == bodyShadowMode
                && firstHidden.GetShaderPassEnabled("ShadowCaster"),
                "hidden preserves renderer enablement and authored shadow casting");
            DisFadeRenderRouting.BeginSplitCamera();
            CheckHidden(body);
            CheckHidden(front);
            DisFadeRenderRouting.EndSplitCamera();
            CheckHidden(body);
            DisFadeRenderRouting.SetTexture(body, second, tile.GetRendererRoute(0));
            Check(body.sharedMaterial == secondHidden && firstHidden.GetTexture("_Tex") == first,
                "hidden texture change reuses the shared cache without mutating the old binding");
            DisFadeRenderRouting.SetTexture(body, first, tile.GetRendererRoute(0));
            Check(body.sharedMaterial == firstHidden, "returning to a hidden texture reuses its material");
            tile.ApplyVision(.25f, .5f);
            CheckUniformTransparency(body, .25f);
            CheckUniformTransparency(front, .5f);
            Check(body.sharedMaterial == transparent && front.sharedMaterial == secondTransparent,
                "showing hidden renderers restores both ordinary and half-occlusion transparent materials");
            tile.ApplyVision(1f, 1f);
            CheckRoute(body, "Opaque");
            Check(body.sharedMaterial == opaque && opaque.GetShaderPassEnabled(Forward)
                && opaque.GetShaderPassEnabled(Opaque), "showing hidden renderers restores opaque fallback");
            tile.ApplyVision(0f, 0f);
            Check(body.sharedMaterial == firstHidden && front.sharedMaterial == secondHidden,
                "repeated hiding reuses cached hidden materials");
            tile.ApplyVision(1f, 1f);
            tile.displayLayer = 0;
            tile.ApplyVision(1f, 1f);
            tile.renderers[1].GetPropertyBlock(block);
            Check(block.GetFloat("_Show") == 0f, "display ceiling hides upper layers");
            Check(tile.GetRendererRoute(1) == DisFadeRenderRouting.Route.Hidden
                && tile.GetRendererRoute(4) == DisFadeRenderRouting.Route.Hidden,
                "display ceiling is included in cached routes");
            CheckHidden(tile.renderers[1]);
            HiddenAuthoredPasses(source, first);

            // Exercise production Tile Show selection, not only the routing helper.
            TexAssetForm.InitInternal();
            MapTextureForm.InitInternal();
            TexAssetForm.DataById[81001] = new TexAssetForm.Data(81001, "first", "", null, "", first, 0);
            TexAssetForm.DataById[81002] = new TexAssetForm.Data(81002, "second", "", null, "", second, 0);
            var data = new TileUnitForm.Data(82001, "", new Dictionary<int, int>(), Vector3Int.zero,
                "", Vector3.zero, Vector3.zero, Vector3.one, Z_UnitSystem.Form.UpdateType.ShowOnly,
                new List<int>(), "", false, false, new List<int>());
            data.unit.ins = tile;
            tile.unit = data.unit;
            tile.displayLayer = int.MaxValue;
            tile.ApplyVision(1f, .5f);
            var product = new MapTextureForm.Data(83001, "test", 81001, 0, new List<int> { 81001 }, 0,
                new Dictionary<string, EventTriggerForm.Data>(), false, new Dictionary<int, int>(), 0,
                true, new List<int> { 81002 }, false, new Dictionary<int, int>());
            MapTextureForm.DataById[product.id] = product;
            data.texDic[0] = product.id;
            var controller = new GameMapController(null);
            var evt = new TileEvent { type = MapEventType.Show, unit = data.unit };
            controller.OnEvent(evt);
            Check(body.sharedMaterial.GetTexture("_Tex") == first
                && front.sharedMaterial.GetTexture("_Tex") == second, "Tile Show assigns both initial textures");
            CheckRoute(body, "Opaque");
            CheckUniformTransparency(front, .5f);
            Check(!tile.renderers[1].enabled && !tile.renderers[2].enabled
                && !tile.renderers[4].enabled && !tile.renderers[5].enabled, "empty upper layers stay disabled");
            tile.ApplyVision(.25f, .25f);
            controller.ShowFinalMat(tile, 0, new List<int> { 81002 }, 0);
            CheckRoute(body, "Transparent");
            Check(body.sharedMaterial.GetTexture("_Tex") == second, "production texture replacement preserves transparency");
            product.frontPartTexs.Clear();
            controller.OnEvent(evt);
            Check(!front.enabled, "removing front textures disables the old front renderer");

            EmptyTileLayers(controller, tile, source);
            CharacterTemplateLifecycle(source, first, second);

            Debug.Log("DIS_FADE_ROUTING_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            DisFadeRenderRouting.EndSplitCamera();
            if (root != null) UnityEngine.Object.DestroyImmediate(root);
            if (source != null) UnityEngine.Object.DestroyImmediate(source);
            if (first != null) UnityEngine.Object.DestroyImmediate(first);
            if (second != null) UnityEngine.Object.DestroyImmediate(second);
        }
    }

    private static void EmptyTileLayers(GameMapController controller, TileInstance tile, Material source)
    {
        var peerRoot = new GameObject("empty-tile-peer");
        var timeObject = new GameObject("empty-tile-time");
        timeObject.SetActive(false);
        var time = timeObject.AddComponent<TimeManager>();
        var singleton = typeof(Z_DesignStyle.Z_MonoSingleton<TimeManager>).GetField("_instance",
            BindingFlags.Static | BindingFlags.NonPublic);
        var previousTime = singleton.GetValue(null);
        singleton.SetValue(null, time);
        var empty = new List<int>();
        try
        {
            for (int i = 0; i < 6; i++)
            {
                var child = new GameObject("empty-layer" + i);
                child.transform.SetParent(peerRoot.transform, false);
                child.AddComponent<MeshRenderer>().sharedMaterial = source;
            }
            var peer = peerRoot.AddComponent<TileInstance>();
            tile.displayLayer = int.MaxValue;
            tile.ApplyVision(1f, 1f);
            peer.ApplyVision(1f, 1f);
            // These are the same texture submission paths used by ordinary and
            // generated WangTile frames. Different old textures must be forgotten.
            for (int i = 0; i < 6; i++)
            {
                controller.ShowFinalMat(tile, i, new List<int> { i % 2 == 0 ? 81001 : 81002 }, 0);
                controller.ShowFinalMat(peer, i, new List<int> { i % 2 == 0 ? 81002 : 81001 }, 0);
            }
            var oldMaterial = tile.renderers[0].sharedMaterial;
            var oldTexture = oldMaterial.GetTexture("_Tex");
            var oldTimer = new Timer { bind = tile };
            tile.animTimer[0] = oldTimer;
            for (int i = 0; i < 6; i++)
            {
                controller.ShowFinalMat(tile, i, empty, 0);
                controller.ShowFinalMat(peer, i, empty, 0);
            }
            Check(oldTimer.cancel && tile.animTimer[0] == null, "clearing a Tile slot cancels its prior animation");
            var sharedEmpty = tile.renderers[0].sharedMaterial;
            Check(sharedEmpty.GetTexture("_Tex") == TextureHelper.transparentTexture,
                "empty Tile slots use the canonical transparent texture");
            foreach (var instance in new[] { tile, peer })
                foreach (var renderer in instance.renderers)
                {
                    Check(!renderer.enabled && renderer.sharedMaterial == sharedEmpty,
                        "empty base/front layers and separate Tiles share one disabled default material");
                    CheckHidden(renderer);
                }
            Check(oldMaterial.GetTexture("_Tex") == oldTexture, "empty binding never rewrites an existing visible material");
            foreach (float value in new[] { 0f, .25f, .5f, 1f })
            {
                tile.displayLayer = value == .25f ? 0 : int.MaxValue;
                tile.ApplyVision(value, 1f - value);
                foreach (var renderer in tile.renderers)
                    Check(!renderer.enabled && renderer.sharedMaterial == sharedEmpty,
                        "visibility/display changes keep empty Tile slots on the same default material");
            }
            DisFadeRenderRouting.BeginSplitCamera();
            CheckHidden(tile.renderers[0]);
            DisFadeRenderRouting.EndSplitCamera();
            Check(tile.renderers[0].sharedMaterial == sharedEmpty, "camera transitions retain the shared empty material");
            tile.ApplyVision(.5f, 1f);
            Check(tile.GetRendererRoute(0) == DisFadeRenderRouting.Route.Transparent
                && tile.GetRendererRoute(3) == DisFadeRenderRouting.Route.Opaque,
                "empty slots retain the visibility routes needed by later texture assignment");
            controller.ShowFinalMat(tile, 0, new List<int> { 81001 }, 0);
            controller.ShowFinalMat(tile, 3, new List<int> { 81002 }, 0);
            Check(tile.renderers[0].enabled && tile.renderers[3].enabled,
                "assigning textures to empty Tile slots enables the configured parts");
            CheckUniformTransparency(tile.renderers[0], .5f);
            CheckRoute(tile.renderers[3], "Opaque");
            tile.ApplyVision(0f, 0f);
            Check(tile.renderers[0].sharedMaterial.GetTexture("_Tex") == TexAssetForm.DataById[81001].GetTex()
                && tile.renderers[3].sharedMaterial.GetTexture("_Tex") == TexAssetForm.DataById[81002].GetTex(),
                "occlusion-hidden configured Tiles keep their original texture-alpha shadow silhouettes");
            tile.PrepareForHide();
            tile.PrepareForShow();
            tile.ApplyVision(1f, 1f);
            Check(tile.renderers[1].sharedMaterial == sharedEmpty && !tile.renderers[1].enabled,
                "pool show preparation keeps empty slots on the shared default material");
        }
        finally
        {
            DisFadeRenderRouting.EndSplitCamera();
            singleton.SetValue(null, previousTime);
            UnityEngine.Object.DestroyImmediate(timeObject);
            UnityEngine.Object.DestroyImmediate(peerRoot);
        }
    }

    private static void CharacterTemplateLifecycle(Material source, Texture first, Texture second)
    {
        CharacterProductForm.InitInternal();
        var template = new GameObject("runtime$character");
        template.SetActive(false);
        for (int i = 0; i < 2; i++)
        {
            var child = new GameObject("part" + i);
            child.transform.SetParent(template.transform, false);
            var render = child.AddComponent<MeshRenderer>();
            render.sharedMaterial = source;
            render.enabled = false; // GameUtil's runtime Character template baseline.
            var block = new MaterialPropertyBlock();
            block.SetFloat("_Show", 0f);
            render.SetPropertyBlock(block);
            DisFadeRenderRouting.SetTexture(render, Texture2D.whiteTexture, DisFadeRenderRouting.Route.Hidden);
        }
        template.AddComponent<CharacterInstance>();
        var holder = new GameObject("character-pool");
        var timeObject = new GameObject("character-test-time");
        timeObject.SetActive(false);
        var time = timeObject.AddComponent<TimeManager>();
        var singleton = typeof(Z_DesignStyle.Z_MonoSingleton<TimeManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        var previousTime = singleton.GetValue(null);
        singleton.SetValue(null, time);
        var pool = new Z_UnitSystem.InstancePool(template, holder.transform);
        GameObject live = null;
        var data = new CharacterUnitForm.Data(84001, false, Vector3.zero, 0, 0, 0, false,
            "", "", Vector3.zero, Vector3.zero, Vector3.one, Z_UnitSystem.Form.UpdateType.ShowOnly,
            new List<int>(), "", false, 0, null);
        var product = new CharacterProductForm.Data(84002, "routing-character", 0, 0, null, 0,
            new Dictionary<string, CharacterAnimForm.Data>(), null, FaceType.Fixed, "", "", null, null,
            "", 0, false, null, 0, false, 0, 1, null);
        CharacterProductForm.DataByUid[product.uid] = product;
        data.unit.productInfo = (product.uid, -1);
        var enabledParts = new Dictionary<BodyPartType, bool> {
            { BodyPartType.UpperPart, true }, { BodyPartType.LowerPart, true }
        };
        var idle = new CharacterAnimForm.Data(84003, "idle", new Dictionary<AnimDirecton, List<CharacterAnimClipForm.Data>> {
            { AnimDirecton.Fixed, new List<CharacterAnimClipForm.Data> {
                new CharacterAnimClipForm.Data(0, null, null, new Dictionary<BodyPartType, int> {
                    { BodyPartType.UpperPart, 81001 }, { BodyPartType.LowerPart, 81002 }
                })
            } }
        }, 0, 1, enabledParts);
        product.animDic["idle"] = idle;
        var animation = new GameCharacterController.AnimController { data = data, idleAnim = idle, moveAnim = idle };
        var readBlock = new MaterialPropertyBlock();
        try
        {
            live = pool.New();
            var instance = live.GetComponent<CharacterInstance>();
            for (int reuse = 0; reuse < 3; reuse++)
            {
                pool.Fresh(live);
                Check(instance.GetRendererRoute(0) == DisFadeRenderRouting.Route.Hidden,
                    "new or reused character keeps the hidden cached route until visibility is applied: " + reuse);
                data.unit.ins = instance;
                instance.unit = data.unit;
                instance.VisOn();
                enabledParts[BodyPartType.LowerPart] = true;
                animation.Reset();
                animation.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart);
                animation.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.LowerPart);
                animation.CloseHideRenderer();
                var upper = instance.renderers[0];
                var lower = instance.renderers[1];
                Check(upper.enabled && lower.enabled, "character first Show/reuse enables both configured parts: " + reuse);
                Check(upper.sharedMaterial.GetTexture("_Tex") == first && lower.sharedMaterial.GetTexture("_Tex") == second,
                    "character first Show/reuse assigns both part textures: " + reuse);
                CheckRoute(upper, "Opaque");
                instance.ApplyVision(.25f);
                animation.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart, true);
                CheckRoute(upper, "Transparent");
                instance.ApplyVision(.5f);
                animation.ChangeState(GameCharacterController.AnimController.State.Idle, BodyPartType.UpperPart, true);
                CheckUniformTransparency(upper, .5f);
                CheckUniformTransparency(lower, .5f);
                Check(instance.GetRendererRoute(0) == DisFadeRenderRouting.Route.Transparent
                    && upper.sharedMaterial.GetTexture("_Tex") == first,
                    "character half occlusion survives animation texture updates: " + reuse);
                instance.ApplyVision(0f);
                animation.CloseHideRenderer();
                upper.GetPropertyBlock(readBlock);
                Check(readBlock.GetFloat("_Show") == 0f, "part configuration does not undo map visibility: " + reuse);
                CheckHidden(upper);
                Check(upper.enabled && lower.enabled,
                    "map hiding leaves configured character parts enabled for authored shadows: " + reuse);
                instance.ApplyVision(1f);
                enabledParts[BodyPartType.LowerPart] = false;
                animation.CloseHideRenderer();
                Check(upper.enabled && !lower.enabled, "disabled animation part stays hidden: " + reuse);
                pool.Clear(live);
                // Non-ExecuteAlways callbacks do not run in this EditMode fixture.
                instance.PrepareForHide();
            }
        }
        finally
        {
            animation.CancelTimers();
            pool.Destroy();
            if (live != null) UnityEngine.Object.DestroyImmediate(live);
            UnityEngine.Object.DestroyImmediate(template);
            UnityEngine.Object.DestroyImmediate(holder);
            singleton.SetValue(null, previousTime);
            UnityEngine.Object.DestroyImmediate(timeObject);
            CharacterProductForm.DataByUid.Remove(product.uid);
        }
    }

    private static void HiddenAuthoredPasses(Material source, Texture texture)
    {
        var authored = new Material(source);
        var root = new GameObject("hidden-authored-passes");
        try
        {
            authored.SetShaderPassEnabled(Forward, false);
            authored.SetShaderPassEnabled(Opaque, false);
            authored.SetShaderPassEnabled("ShadowCaster", false);
            var renderer = root.AddComponent<MeshRenderer>();
            renderer.sharedMaterial = authored;
            renderer.enabled = false;
            foreach (UnityEngine.Rendering.ShadowCastingMode mode in
                Enum.GetValues(typeof(UnityEngine.Rendering.ShadowCastingMode)))
            {
                renderer.shadowCastingMode = mode;
                DisFadeRenderRouting.SetTexture(renderer, texture, DisFadeRenderRouting.Route.Hidden);
                CheckHidden(renderer);
                Check(!renderer.enabled && renderer.shadowCastingMode == mode
                    && !renderer.sharedMaterial.GetShaderPassEnabled("ShadowCaster"),
                    "hidden preserves disabled renderer, authored shadow pass and shadow mode " + mode);
                DisFadeRenderRouting.BeginSplitCamera();
                CheckHidden(renderer);
                DisFadeRenderRouting.EndSplitCamera();
                CheckHidden(renderer);
                DisFadeRenderRouting.SetTexture(renderer, texture, DisFadeRenderRouting.Route.Opaque);
                Check(!renderer.sharedMaterial.GetShaderPassEnabled(Forward)
                    && !renderer.sharedMaterial.GetShaderPassEnabled(Opaque)
                    && !renderer.sharedMaterial.GetShaderPassEnabled("ShadowCaster"),
                    "show/camera cleanup preserves authored disabled passes " + mode);
            }
        }
        finally
        {
            UnityEngine.Object.DestroyImmediate(root);
            UnityEngine.Object.DestroyImmediate(authored);
        }
    }

    private static void CheckUniformTransparency(Renderer renderer, float opacity)
    {
        CheckRoute(renderer, "Transparent");
        var block = new MaterialPropertyBlock();
        renderer.GetPropertyBlock(block);
        Check(block.GetFloat("_Show") == opacity && block.GetFloat("_FadeCenter") == 0f,
            "map transparency uses uniform opacity without center fading: " + opacity);
        Check(renderer.sharedMaterial.GetShaderPassEnabled(Forward)
            && !renderer.sharedMaterial.GetShaderPassEnabled(Opaque),
            "map transparency uses only the forward color pass");
    }

    private static void CheckHidden(Renderer renderer)
    {
        CheckRoute(renderer, "Hidden");
        Check(!renderer.sharedMaterial.GetShaderPassEnabled(Forward)
            && !renderer.sharedMaterial.GetShaderPassEnabled(Opaque), "hidden disables both color passes");
    }

    private static void CheckRoute(Renderer renderer, string route)
    {
        Check(renderer.sharedMaterial.name.EndsWith("(DisFade " + route + ")"), "expected " + route + ": " + renderer.sharedMaterial.name);
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
