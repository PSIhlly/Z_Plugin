using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;

// Reuse URP's Render Objects pass, retaining the original material/MPB values.
// Only this shader's fully opaque pixels participate; no other shaders change.
public sealed class DisFadeOpaqueFeature : ScriptableRendererFeature
{
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
        public OpaquePass() : base("DisFade Opaque", RenderPassEvent.AfterRenderingSkybox,
            new[] { "DisFadeOpaque" }, RenderQueueType.Transparent, -1, new RenderObjects.CustomCameraSettings())
        {
        }

        public override void Execute(ScriptableRenderContext context, ref RenderingData renderingData)
        {
            var commands = CommandBufferPool.Get("DisFade split begin");
            try
            {
                commands.SetGlobalFloat(SplitEnabled, 1f);
                context.ExecuteCommandBuffer(commands);
            }
            finally { CommandBufferPool.Release(commands); }
            base.Execute(context, ref renderingData);
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
