using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using Z_Map;
using Object = UnityEngine.Object;

// Native DrawRenderers/instance-ID probe in an isolated project, never the live story.
public static class DisFadeBatchingRegression
{
    private const int Count = 32;
    private static Camera camera;
    private static int checks;

    public sealed class ProbeAsset : RenderPipelineAsset
    {
        public SortingCriteria sorting;
        public bool instancing = true;
        public bool transparent;
        public bool split = true;
        public bool shaderSplit = true;
        public PerObjectData perObjectData;
        public RenderQueueRange queueRange = RenderQueueRange.transparent;
        public ScriptableRenderPass productionPass;
        public Exception error;
        public override string renderPipelineShaderTag => "UniversalPipeline";
        protected override RenderPipeline CreatePipeline() => new ProbePipeline(this);
    }

    private sealed class ProbePipeline : RenderPipeline
    {
        private readonly ProbeAsset asset;
        public ProbePipeline(ProbeAsset asset) { this.asset = asset; }
        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var viewCamera in cameras)
            {
                if (viewCamera != camera) continue;
                var commands = new CommandBuffer { name = "DisFade batch diagnosis" };
                try
                {
                    if (asset.split) DisFadeRenderRouting.BeginSplitCamera();
                    else DisFadeRenderRouting.EndSplitCamera();
                    Check(viewCamera.TryGetCullingParameters(out var parameters), "camera supports native culling");
                    parameters.cullingOptions &= ~CullingOptions.OcclusionCull;
                    var culling = context.Cull(ref parameters);
                    context.SetupCameraProperties(viewCamera);
                    var view = viewCamera.worldToCameraMatrix;
                    var projection = GL.GetGPUProjectionMatrix(viewCamera.projectionMatrix, true);
                    commands.SetRenderTarget(viewCamera.targetTexture);
                    commands.ClearRenderTarget(true, true, Color.clear);
                    commands.SetViewProjectionMatrices(view, projection);
                    commands.SetGlobalMatrix("unity_MatrixV", view);
                    commands.SetGlobalMatrix("unity_MatrixVP", projection * view);
                    commands.SetGlobalVector("_WorldSpaceCameraPos", viewCamera.transform.position);
                    commands.SetGlobalVector("_MainLightPosition", new Vector4(0, 0, -1, 0));
                    commands.SetGlobalVector("_MainLightColor", Vector4.one);
                    commands.SetGlobalFloat("_DisFadeSplitEnabled", asset.shaderSplit ? 1f : 0f);
                    commands.SetGlobalVector("unity_OrthoParams", new Vector4(6.4f, 6.4f, 0, 1));
                    context.ExecuteCommandBuffer(commands);
                    commands.Clear();
                    if (asset.productionPass != null)
                    {
                        var renderingData = new RenderingData
                        {
                            cullResults = culling,
                            cameraData = new CameraData { camera = viewCamera },
                            perObjectData = asset.perObjectData,
                            lightData = new LightData { mainLightIndex = -1 }
                        };
                        asset.productionPass.Execute(context, ref renderingData);
                        asset.productionPass.OnCameraCleanup(commands);
                        context.ExecuteCommandBuffer(commands);
                        commands.Clear();
                    }
                    else
                    {
                        var sorting = new SortingSettings(viewCamera) { criteria = asset.sorting, distanceMetric = DistanceMetric.Orthographic };
                        var drawing = new DrawingSettings(new ShaderTagId(asset.transparent ? "UniversalForward" : "DisFadeOpaque"), sorting)
                        {
                            enableInstancing = asset.instancing, enableDynamicBatching = false,
                            perObjectData = asset.perObjectData
                        };
                        var filtering = new FilteringSettings(asset.queueRange, viewCamera.cullingMask);
                        context.DrawRenderers(culling, ref drawing, ref filtering);
                    }
                    context.Submit();
                }
                catch (Exception exception) { asset.error = exception; }
                finally
                {
                    DisFadeRenderRouting.EndSplitCamera();
                    commands.Release();
                }
            }
        }
    }

    public static void Run()
    {
        var previousGraphics = GraphicsSettings.renderPipelineAsset;
        var previousQuality = QualitySettings.renderPipeline;
        var previousBatcher = GraphicsSettings.useScriptableRenderPipelineBatching;
        var previousTag = Shader.globalRenderPipeline;
        var previousTarget = RenderTexture.active;
        var objects = new List<GameObject>();
        ProbeAsset asset = null;
        Material source = null;
        Texture2D pixels = null;
        RenderTexture target = null;
        GameObject cameraObject = null, primitive = null;
        try
        {
            var shader = CreateInstanceIdShader();
            source = new Material(shader) { name = "shared-tile-batch-source", enableInstancing = true, renderQueue = 3000 };
            source.SetTexture("_Tex", Texture2D.whiteTexture);
            source.SetFloat("_Alpha", 1f);
            source.SetFloat("_Show", 1f);
            source.SetFloat("_FadeCenter", 0f);
            source.SetFloat("_Cutoff", .5f);
            source.SetFloat("_LightSensitivity", 1f);
            primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            var mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            primitive.SetActive(false);
            for (int i = 0; i < Count; i++)
            {
                var tile = new GameObject("ordinary-tile-" + i) { layer = 30 };
                tile.transform.localScale = Vector3.one * .4f;
                tile.AddComponent<MeshFilter>().sharedMesh = mesh;
                var renderer = tile.AddComponent<MeshRenderer>();
                renderer.sharedMaterial = source;
                renderer.shadowCastingMode = ShadowCastingMode.Off;
                tile.AddComponent<TileInstance>().ApplyVision(1f);
                DisFadeRenderRouting.SetTexture(renderer, Texture2D.whiteTexture, DisFadeRenderRouting.Route.Opaque);
                objects.Add(tile);
            }
            Check(objects.Select(o => o.GetComponent<MeshRenderer>().sharedMaterial).Distinct().Count() == 1,
                "all ordinary Tiles share one cached material");
            Check(objects.Select(o => o.GetComponent<MeshFilter>().sharedMesh).Distinct().Count() == 1,
                "all ordinary Tiles share one mesh");
            cameraObject = new GameObject("tile-batch-camera");
            camera = cameraObject.AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 3.2f;
            camera.aspect = 1f;
            camera.transform.position = new Vector3(0, 0, -10);
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 30f;
            camera.cullingMask = 1 << 30;
            target = new RenderTexture(512, 512, 24, RenderTextureFormat.ARGBFloat, RenderTextureReadWrite.Linear);
            target.Create();
            camera.targetTexture = target;
            pixels = new Texture2D(512, 512, TextureFormat.RGBAFloat, false, true);
            asset = ScriptableObject.CreateInstance<ProbeAsset>();
            GraphicsSettings.useScriptableRenderPipelineBatching = true;
            QualitySettings.renderPipeline = null;
            GraphicsSettings.renderPipelineAsset = asset;

            Probe(asset, objects, pixels, false, false, false, SortingCriteria.CommonTransparent, "control-no-instancing");
            Probe(asset, objects, pixels, true, false, false, SortingCriteria.CommonTransparent, "same-depth-opaque-transparent-sort");
            Probe(asset, objects, pixels, true, true, false, SortingCriteria.CommonTransparent, "varied-depth-opaque-transparent-sort");
            Probe(asset, objects, pixels, true, true, false, SortingCriteria.CommonOpaque, "varied-depth-opaque-opaque-sort");
            Probe(asset, objects, pixels, true, true, false, SortingCriteria.OptimizeStateChanges, "varied-depth-opaque-state-sort");
            Probe(asset, objects, pixels, true, true, true, SortingCriteria.CommonTransparent, "varied-depth-transparent-transparent-sort");
            asset.perObjectData = PerObjectData.LightProbe | PerObjectData.ReflectionProbes | PerObjectData.LightData
                | PerObjectData.LightIndices | PerObjectData.OcclusionProbe | PerObjectData.ShadowMask;
            Probe(asset, objects, pixels, true, true, false, SortingCriteria.CommonTransparent, "varied-depth-opaque-probe-data");
            ProbeForwardEligibility(asset, objects, pixels);
            ProbeWangTileVariants(asset, objects, pixels);
            Debug.Log("DIS_FADE_BATCHING_REGRESSION_PASS checks=" + checks);
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
            QualitySettings.renderPipeline = previousQuality;
            GraphicsSettings.renderPipelineAsset = previousGraphics;
            GraphicsSettings.useScriptableRenderPipelineBatching = previousBatcher;
            Shader.globalRenderPipeline = previousTag;
            RenderTexture.active = previousTarget;
            foreach (var tile in objects) Object.DestroyImmediate(tile);
            if (primitive != null) Object.DestroyImmediate(primitive);
            if (cameraObject != null) Object.DestroyImmediate(cameraObject);
            if (source != null) Object.DestroyImmediate(source);
            if (asset != null) Object.DestroyImmediate(asset);
            if (pixels != null) Object.DestroyImmediate(pixels);
            if (target != null) { target.Release(); Object.DestroyImmediate(target); }
        }
    }

    private static List<int> Probe(ProbeAsset asset, List<GameObject> objects, Texture2D pixels,
        bool instancing, bool variedDepth, bool transparent, SortingCriteria sorting, string label)
    {
        asset.instancing = instancing;
        asset.transparent = transparent;
        asset.sorting = sorting;
        for (int i = 0; i < objects.Count; i++)
        {
            objects[i].transform.position = new Vector3(-2.45f + i % 8 * .7f, -1.05f + i / 8 * .7f,
                variedDepth ? i * .05f : 0f);
            objects[i].GetComponent<TileInstance>().ApplyVision(transparent ? .5f : 1f);
        }
        camera.Render();
        if (asset.error != null) throw asset.error;
        RenderTexture.active = camera.targetTexture;
        pixels.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        pixels.Apply();
        var ids = new List<int>();
        foreach (var tile in objects)
        {
            var viewport = camera.WorldToViewportPoint(tile.transform.position);
            var pixel = pixels.GetPixel((int)(viewport.x * 512), (int)(viewport.y * 512));
            Check(pixel.a > .99f, "native Tile pixel is visible: " + label + "/" + tile.name + "=" + pixel);
            ids.Add(Mathf.RoundToInt(pixel.r * 255f) - 1);
        }
        if (!instancing) Check(ids.All(id => id == 0), "uninstanced control uses hardware ID zero");
        Debug.Log("DIS_FADE_BATCH_PROBE label=" + label + " renderers=" + objects.Count
            + " distinctIds=" + ids.Distinct().Count() + " maxId=" + ids.Max()
            + " ids=" + string.Join(",", ids));
        return ids;
    }

    private static void ProbeWangTileVariants(ProbeAsset asset, List<GameObject> objects, Texture2D pixels)
    {
        Texture2D source = null;
        DisFadeOpaqueFeature feature = null;
        Dictionary<int, Sprite> sprites = null;
        var generated = new HashSet<Texture2D>();
        try
        {
            source = new Texture2D(4, 6, TextureFormat.RGBA32, false) { name = "white-wangtile-sheet" };
            source.SetPixels(Enumerable.Repeat(Color.white, 24).ToArray());
            source.Apply();
            sprites = TileHelper.GetAutoTileSprites(source);
            foreach (var sprite in sprites.Values) generated.Add(sprite.texture);
            Check(sprites.Count == 256, "real WangTile generation resolves every neighbour mask");
            Check(generated.Count == 47, "real WangTile generation creates 47 shared variant textures");
            Check(sprites[0] == sprites[1], "equivalent WangTile masks share their generated Sprite");
            var first = sprites[0].texture;
            var second = sprites[255].texture;
            Check(first != second && first.GetPixels().SequenceEqual(second.GetPixels()),
                "different WangTile variants are separate textures even when their pixels match");

            const BindingFlags fields = BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic;
            var sortingField = typeof(DisFadeOpaqueFeature).GetField("OpaqueSorting", fields);
            var queueField = typeof(DisFadeOpaqueFeature).GetField("OpaqueQueueRange", fields);
            Check(sortingField != null && queueField != null, "production split pass exposes its shared drawing configuration");
            var sorting = (SortingCriteria)sortingField.GetValue(null);
            var queueRange = (RenderQueueRange)queueField.GetValue(null);
            Check(sorting == SortingCriteria.CommonOpaque, "production split pass groups opaque pixels by render state");
            Check(queueRange.lowerBound == RenderQueueRange.transparent.lowerBound
                && queueRange.upperBound == RenderQueueRange.transparent.upperBound,
                "production split pass still selects the original transparent-queue materials");
            asset.queueRange = queueRange;
            feature = ScriptableObject.CreateInstance<DisFadeOpaqueFeature>();
            feature.Create();
            var pass = (ScriptableRenderPass)typeof(DisFadeOpaqueFeature).GetField("pass",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(feature);
            Check(pass != null && pass.renderPassEvent == RenderPassEvent.AfterRenderingSkybox
                && pass.renderPassEvent < RenderPassEvent.BeforeRenderingTransparents,
                "production WangTile opaque stage runs before all transparent rendering");
            var filterField = pass.GetType().GetFields(BindingFlags.Instance | BindingFlags.NonPublic)
                .Single(field => field.FieldType == typeof(FilteringSettings));
            var filter = (FilteringSettings)filterField.GetValue(pass);
            Check(filter.layerMask == -1 && filter.renderQueueRange.lowerBound == queueRange.lowerBound
                && filter.renderQueueRange.upperBound == queueRange.upperBound,
                "real production Execute selects every layer in the shared transparent queue range");

            foreach (var tile in objects)
                DisFadeRenderRouting.SetTexture(tile.GetComponent<MeshRenderer>(), first, DisFadeRenderRouting.Route.Opaque);
            Check(objects.Select(o => o.GetComponent<MeshRenderer>().sharedMaterial).Distinct().Count() == 1,
                "same WangTile variant shares one material across all Tile instances");
            asset.productionPass = pass;
            var sameVariant = Probe(asset, objects, pixels, true, true, false, sorting, "wangtile-same-variant-production-sort");
            Check(sameVariant.Distinct().Count() == Count && sameVariant.Min() == 0 && sameVariant.Max() == Count - 1,
                "32 same-variant Tiles use hardware instance IDs 0..31 in one draw");

            for (int i = 0; i < objects.Count; i++)
                DisFadeRenderRouting.SetTexture(objects[i].GetComponent<MeshRenderer>(), i % 2 == 0 ? first : second,
                    DisFadeRenderRouting.Route.Opaque);
            var materials = objects.Select(o => o.GetComponent<MeshRenderer>().sharedMaterial).Distinct().ToList();
            Check(materials.Count == 2 && materials[0].name == materials[1].name,
                "two WangTile textures produce two cached materials with the same display name");
            asset.productionPass = null;
            var transparentOrder = Probe(asset, objects, pixels, true, true, false, SortingCriteria.CommonTransparent,
                "wangtile-interleaved-transparent-sort");
            Check(transparentOrder.All(id => id == 0),
                "back-to-front interleaved WangTile variants require 32 single-instance draws");
            asset.productionPass = pass;
            var opaqueOrder = Probe(asset, objects, pixels, true, true, false, sorting,
                "wangtile-interleaved-production-sort");
            // Each native instanced draw restarts at zero. All 32 disjoint Tiles
            // are sampled, so two occurrences of every ID prove two 16-instance draws.
            Check(opaqueOrder.Count(id => id == 0) == 2 && opaqueOrder.Min() == 0 && opaqueOrder.Max() == Count / 2 - 1
                && opaqueOrder.GroupBy(id => id).All(group => group.Count() == 2),
                "production opaque ordering combines interleaved WangTile variants into two native draws");
            ProbeWangTileDepth(asset, objects, pixels, first, second);
            Check(Shader.GetGlobalFloat("_DisFadeSplitEnabled") == 0f,
                "real production camera cleanup resets the split marker after native drawing");
            Check(materials.All(material => material.GetShaderPassEnabled("UniversalForward")),
                "real production camera cleanup restores opaque materials for a following fallback camera");
        }
        finally
        {
            asset.productionPass = null;
            if (feature != null) { feature.Dispose(); Object.DestroyImmediate(feature); }
            foreach (var tile in objects)
                DisFadeRenderRouting.SetTexture(tile.GetComponent<MeshRenderer>(), Texture2D.whiteTexture,
                    DisFadeRenderRouting.Route.Opaque);
            if (sprites != null)
                foreach (var sprite in new HashSet<Sprite>(sprites.Values)) Object.DestroyImmediate(sprite);
            foreach (var texture in generated) Object.DestroyImmediate(texture);
            if (source != null) Object.DestroyImmediate(source);
        }
    }

    private static void ProbeWangTileDepth(ProbeAsset asset, List<GameObject> objects, Texture2D pixels,
        Texture2D firstTexture, Texture2D secondTexture)
    {
        for (int i = 0; i < objects.Count; i++) objects[i].SetActive(i < 2);
        var first = objects[0].GetComponent<MeshRenderer>();
        var second = objects[1].GetComponent<MeshRenderer>();
        DisFadeRenderRouting.SetTexture(first, firstTexture, DisFadeRenderRouting.Route.Opaque);
        DisFadeRenderRouting.SetTexture(second, secondTexture, DisFadeRenderRouting.Route.Opaque);
        for (int i = 0; i < 2; i++)
        {
            objects[i].GetComponent<TileInstance>().ApplyVision(1f);
            var renderer = objects[i].GetComponent<MeshRenderer>();
            var block = new MaterialPropertyBlock();
            renderer.GetPropertyBlock(block);
            block.SetFloat("_LightSensitivity", i == 0 ? 2f : 6f);
            renderer.SetPropertyBlock(block);
        }
        objects[0].transform.position = Vector3.zero;
        objects[1].transform.position = Vector3.forward;
        CheckCenterLight(asset, pixels, .2f, "opaque regrouping preserves the nearer first WangTile material");
        objects[0].transform.position = Vector3.forward;
        objects[1].transform.position = Vector3.zero;
        CheckCenterLight(asset, pixels, .6f, "opaque regrouping preserves the nearer second WangTile material");
        DisFadeRenderRouting.SetTexture(first, secondTexture, DisFadeRenderRouting.Route.Opaque);
        DisFadeRenderRouting.SetTexture(second, firstTexture, DisFadeRenderRouting.Route.Opaque);
        CheckCenterLight(asset, pixels, .6f, "swapping WangTile material groups cannot override the nearer surface");
        objects[1].GetComponent<TileInstance>().ApplyVision(0f);
        CheckCenterLight(asset, pixels, .2f, "a hidden near WangTile cannot occlude the visible farther surface");
        foreach (var tile in objects) tile.SetActive(true);
    }

    private static void CheckCenterLight(ProbeAsset asset, Texture2D pixels, float expected, string message)
    {
        camera.Render();
        if (asset.error != null) throw asset.error;
        RenderTexture.active = camera.targetTexture;
        pixels.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
        pixels.Apply();
        var pixel = pixels.GetPixel(256, 256);
        Check(pixel.a > .99f && Mathf.Abs(pixel.b - expected) < .001f, message + ": " + pixel);
    }

    private static void ProbeForwardEligibility(ProbeAsset asset, List<GameObject> objects, Texture2D pixels)
    {
        asset.productionPass = null;
        asset.transparent = true;
        asset.sorting = SortingCriteria.CommonTransparent;
        // Disable the fragment-level opaque discard: only native pass eligibility
        // can keep an Opaque Renderer out of this UniversalForward-only draw.
        asset.shaderSplit = false;
        var block = new MaterialPropertyBlock();
        try
        {
            foreach (var tile in objects)
            {
                tile.GetComponent<TileInstance>().ApplyVision(1f);
                DisFadeRenderRouting.SetTexture(tile.GetComponent<Renderer>(), Texture2D.whiteTexture,
                    DisFadeRenderRouting.Route.Opaque);
            }
            asset.split = true;
            CheckForwardPixels(false, "Opaque skipped by native UniversalForward selection");
            asset.split = false;
            CheckForwardPixels(true, "following fallback camera draws Opaque through UniversalForward");
            asset.split = true;
            CheckForwardPixels(false, "following split camera suppresses Opaque again");

            foreach (var tile in objects)
                tile.GetComponent<TileInstance>().ApplyVision(.5f);
            CheckForwardPixels(true, "Transparent still reaches UniversalForward");
            foreach (var tile in objects)
                tile.GetComponent<TileInstance>().ApplyVision(0f);
            CheckForwardPixels(false, "Hidden skips UniversalForward in split camera");
            asset.split = false;
            CheckForwardPixels(false, "Hidden skips UniversalForward in fallback camera");

            foreach (var tile in objects)
            {
                var renderer = tile.GetComponent<Renderer>();
                renderer.GetPropertyBlock(block);
                block.SetFloat("_Show", 1f);
                block.SetFloat("_FadeCenter", 0f);
                DisFadeRenderRouting.Apply(renderer, block, DisFadeRenderRouting.Route.Mixed);
                var material = renderer.sharedMaterial;
                Check(material.FindPass("Universal Forward") >= 0,
                    "FindPass resolves the authored Name with its space");
                material.SetShaderPassEnabled("Universal Forward", false);
                Check(!material.GetShaderPassEnabled("Universal Forward")
                    && material.GetShaderPassEnabled("UniversalForward"),
                    "disabling Name leaves the actual LightMode enabled");
            }
            CheckForwardPixels(true, "disabled Name still submits native UniversalForward draw");
            foreach (var tile in objects)
            {
                var material = tile.GetComponent<Renderer>().sharedMaterial;
                material.SetShaderPassEnabled("UniversalForward", false);
            }
            CheckForwardPixels(false, "disabled LightMode removes native UniversalForward draw");
            Debug.Log("DIS_FADE_FORWARD_ELIGIBILITY_PASS name-disabled=drawn lightmode-disabled=skipped opaque-split=skipped fallback=drawn");
        }
        finally
        {
            asset.split = true;
            asset.shaderSplit = true;
            asset.transparent = false;
            foreach (var tile in objects)
            {
                var renderer = tile.GetComponent<Renderer>();
                renderer.sharedMaterial.SetShaderPassEnabled("UniversalForward", true);
                renderer.sharedMaterial.SetShaderPassEnabled("Universal Forward", true);
                tile.GetComponent<TileInstance>().ApplyVision(1f);
                DisFadeRenderRouting.SetTexture(renderer, Texture2D.whiteTexture, DisFadeRenderRouting.Route.Opaque);
            }
        }

        void CheckForwardPixels(bool expected, string message)
        {
            camera.Render();
            if (asset.error != null) throw asset.error;
            RenderTexture.active = camera.targetTexture;
            pixels.ReadPixels(new Rect(0, 0, 512, 512), 0, 0);
            pixels.Apply();
            foreach (var tile in objects)
            {
                var viewport = camera.WorldToViewportPoint(tile.transform.position);
                var pixel = pixels.GetPixel((int)(viewport.x * 512), (int)(viewport.y * 512));
                Check(expected ? pixel.a > .99f : pixel.a < .01f, message + ": " + pixel);
            }
        }
    }

    private static Shader CreateInstanceIdShader()
    {
        var source = File.ReadAllText(Path.Combine(Application.dataPath, "DisFadeCode.shader"));
        source = source.Replace("Shader \"DisFadeCode\"", "Shader \"DisFadeBatchIdProbe\"");
        int includeEnd = source.IndexOf("ENDHLSL", StringComparison.Ordinal);
        Check(includeEnd >= 0, "production shader has a shared include");
        const string probe = "\nfloat4 DisFadeBatchIdColor()\n{\n#if defined(UNITY_INSTANCING_ENABLED)\n"
            + " uint id = unity_InstanceID - unity_BaseInstanceID;\n#else\n uint id = 0;\n#endif\n"
            + " return float4((id + 1.0) / 255.0, UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _Show),"
            + " UNITY_ACCESS_INSTANCED_PROP(UnityPerMaterial, _LightSensitivity) / 10.0, 1.0);\n}\n";
        source = source.Insert(includeEnd, probe);
        source = source.Replace("return half4(diffuse.rgb * finalAlpha, finalAlpha);", "return DisFadeBatchIdColor();")
            .Replace("return half4(diffuse, 1.0h);", "return DisFadeBatchIdColor();");
        Check(source.Split(new[] { "return DisFadeBatchIdColor();" }, StringSplitOptions.None).Length == 3,
            "probe only changes both color outputs while preserving native instance setup");
        const string path = "Assets/DisFadeBatchIdProbe.shader";
        File.WriteAllText(path, source);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        Check(shader != null && shader.isSupported, "instance-ID shader imports on the actual graphics device");
        return shader;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
