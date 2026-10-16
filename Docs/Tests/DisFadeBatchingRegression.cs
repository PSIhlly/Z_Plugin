using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
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
        public PerObjectData perObjectData;
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
                    DisFadeRenderRouting.BeginSplitCamera();
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
                    commands.SetGlobalFloat("_DisFadeSplitEnabled", 1f);
                    commands.SetGlobalVector("unity_OrthoParams", new Vector4(6.4f, 6.4f, 0, 1));
                    context.ExecuteCommandBuffer(commands);
                    commands.Clear();
                    var sorting = new SortingSettings(viewCamera) { criteria = asset.sorting, distanceMetric = DistanceMetric.Orthographic };
                    var drawing = new DrawingSettings(new ShaderTagId(asset.transparent ? "UniversalForward" : "DisFadeOpaque"), sorting)
                    {
                        enableInstancing = asset.instancing, enableDynamicBatching = false,
                        perObjectData = asset.perObjectData
                    };
                    var filtering = new FilteringSettings(RenderQueueRange.transparent, viewCamera.cullingMask);
                    context.DrawRenderers(culling, ref drawing, ref filtering);
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

    private static void Probe(ProbeAsset asset, List<GameObject> objects, Texture2D pixels,
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
