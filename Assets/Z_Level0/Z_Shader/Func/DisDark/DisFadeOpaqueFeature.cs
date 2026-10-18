using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;

// Retain URP's Render Objects integration and the original material/MPB values.
// Only this shader's fully opaque pixels participate; no other shaders change.
public sealed class DisFadeOpaqueFeature : ScriptableRendererFeature
{
    internal const SortingCriteria OpaqueSorting = SortingCriteria.CommonOpaque;
    internal static readonly RenderQueueRange OpaqueQueueRange = RenderQueueRange.transparent;
    private static readonly int SplitEnabled = Shader.PropertyToID("_DisFadeSplitEnabled");
    private OpaquePass pass;

    public override void Create()
    {
        DisFadeRenderRouting.EndSplitCamera();
        pass = new OpaquePass();
        Shader.SetGlobalFloat(SplitEnabled, 0f);
    }

    public override void OnCameraPreCull(ScriptableRenderer renderer, in CameraData cameraData)
    {
        if (cameraData.cameraType == CameraType.Preview
            || UniversalRenderer.IsOffscreenDepthTexture(in cameraData))
            return;
        // Unity captures material pass flags during Cull. AddRenderPasses runs
        // after Cull and is too late to remove the unused forward submission.
        DisFadeRenderRouting.BeginSplitCamera();
    }

    public override void AddRenderPasses(ScriptableRenderer renderer, ref RenderingData renderingData)
    {
        if (renderingData.cameraData.cameraType == CameraType.Preview
            || UniversalRenderer.IsOffscreenDepthTexture(in renderingData.cameraData))
            return;
        renderer.EnqueuePass(pass);
    }

    protected override void Dispose(bool disposing)
    {
        Shader.SetGlobalFloat(SplitEnabled, 0f);
        DisFadeRenderRouting.EndSplitCamera();
    }

    private sealed class OpaquePass : RenderObjectsPass
    {
        private static readonly ShaderTagId OpaqueTag = new ShaderTagId("DisFadeOpaque");
        private readonly ProfilingSampler sampler = new ProfilingSampler("DisFade Opaque");
        private FilteringSettings filtering = new FilteringSettings(OpaqueQueueRange, -1);

        public OpaquePass() : base("DisFade Opaque", RenderPassEvent.AfterRenderingSkybox,
            new[] { "DisFadeOpaque" }, RenderQueueType.Transparent, -1, new RenderObjects.CustomCameraSettings())
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            // The materials stay in the transparent queue, but these pixels write
            // opaque color/depth. Back-to-front sorting would interleave WangTile
            // variants and split otherwise shared materials into one draw per Tile.
            // Keep the base class's URP/RenderGraph integration, with independent
            // filtering and opaque sorting for the actual submission.
            var drawing = CreateDrawingSettings(OpaqueTag, ref renderingData, OpaqueSorting);
            var commands = CommandBufferPool.Get();
            try
            {
#if UNITY_EDITOR || DEVELOPMENT_BUILD
                // URP's debugger owns wireframe/overdraw render-state overrides.
                // Keep that path when debugging; ordinary rendering uses the
                // independent opaque sorting below.
                if (UniversalRenderPipelineDebugDisplaySettings.Instance.AreAnySettingsActive)
                {
                    commands.SetGlobalFloat(SplitEnabled, 1f);
                    context.ExecuteCommandBuffer(commands);
                    commands.Clear();
                    base.Execute(context, ref renderingData);
                    return;
                }
#endif
                using (new ProfilingScope(commands, sampler))
                {
                    commands.SetGlobalFloat(SplitEnabled, 1f);
                    context.ExecuteCommandBuffer(commands);
                    commands.Clear();
                    context.DrawRenderers(renderingData.cullResults, ref drawing, ref filtering);
                }
                context.ExecuteCommandBuffer(commands);
            }
            finally { CommandBufferPool.Release(commands); }
        }

        public override void OnCameraCleanup(CommandBuffer commands)
        {
            // Previews and cameras with another Renderer keep the original
            // single-pass fallback instead of losing their opaque pixels.
            commands.SetGlobalFloat(SplitEnabled, 0f);
            DisFadeRenderRouting.EndSplitCamera();
        }
    }
}
