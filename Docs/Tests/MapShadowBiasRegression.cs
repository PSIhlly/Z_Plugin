using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

// Draw the production ShadowCaster with real URP globals, never game/save data.
public static class MapShadowBiasRegression
{
    private const int Resolution = 256;
    private static readonly Vector3 Center = new Vector3(496f, 500f, 519f);
    private static int checks;
    private static Camera camera;
    private static Material material;
    private static Mesh quad;
    private static int shadowPass;

    public static void Run()
    {
        try
        {
            ShaderUtil.allowAsyncCompilation = false;
            Shader.globalRenderPipeline = "UniversalPipeline";
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/DisFadeCode.shader");
            Check(shader != null && shader.isSupported, "production shader imports on the GPU");
            material = new Material(shader) { enableInstancing = true };
            shadowPass = material.FindPass("ShadowCaster");
            Check(shadowPass == 1 && material.FindPass("DisFadeOpaque") == 2,
                "shadow and color pass indices are unchanged");
            material.SetTexture("_Tex", Texture2D.whiteTexture);
            material.SetFloat("_Cutoff", .5f);
            // Visibility must not affect authored shadow silhouettes.
            material.SetFloat("_Alpha", 0f);
            material.SetFloat("_Show", 0f);
            material.SetFloat("_FadeCenter", 1f);
            quad = new Mesh
            {
                vertices = new[] { new Vector3(-.5f, -.5f, 0), new Vector3(.5f, -.5f, 0),
                    new Vector3(.5f, .5f, 0), new Vector3(-.5f, .5f, 0) },
                uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up },
                normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back },
                triangles = new[] { 0, 2, 1, 0, 3, 2 }
            };
            camera = new GameObject("isolated-shadow-bias-camera").AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 2f;
            camera.aspect = 1f;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 100f;

            // Check both small and the reported map coordinates.
            foreach (Vector3 center in new[] { Vector3.zero, Center })
            {
                camera.transform.SetPositionAndRotation(center - Vector3.forward * 10f, Quaternion.identity);
                foreach (bool instanced in new[] { false, true })
                {
                    if (instanced) Check(SystemInfo.supportsInstancing, "GPU supports actual instanced shadow draws");
                    Matrix4x4 transform = Matrix4x4.TRS(center, Quaternion.identity, Vector3.one);
                    float baseline = Draw(transform, Vector4.zero, false, instanced);
                    Check(!float.IsNaN(baseline), "hidden/fading units retain the shadow silhouette");
                    float shifted = Draw(transform, new Vector4(.5f, 0, 0, 0), false, instanced);
                    Check(Mathf.Abs((shifted - baseline) - 32f) < 2f,
                        "directional depth bias moves the real silhouette at " + center + ", instanced=" + instanced);

                    // The punctual variant must use _LightPosition, not _LightDirection.
                    shifted = Draw(transform, new Vector4(.5f, 0, 0, 0), true, instanced);
                    Check(Mathf.Abs((shifted - baseline) - 32f) < 2f,
                        "punctual depth bias uses the per-vertex light direction");

                    Quaternion rotation = Quaternion.Euler(0, 45f, 0);
                    transform = Matrix4x4.TRS(center, rotation, Vector3.one);
                    baseline = Draw(transform, Vector4.zero, false, instanced);
                    shifted = Draw(transform, new Vector4(0, .5f, 0, 0), false, instanced);
                    float expected = (rotation * Vector3.back).x * .5f * Resolution / 4f;
                    Check(Mathf.Abs((shifted - baseline) - expected) < 2f,
                        "normal bias uses the transformed surface normal");
                }
            }

            camera.transform.SetPositionAndRotation(Center - Vector3.forward * 10f, Quaternion.identity);
            Matrix4x4 nearTransform = Matrix4x4.TRS(camera.transform.position + Vector3.forward * .05f,
                Quaternion.identity, Vector3.one);
            Check(!float.IsNaN(Draw(nearTransform, Vector4.zero, false, false)),
                "geometry across the shadow near plane is clamped, not lost");
            var clear = new Texture2D(1, 1, TextureFormat.RGBA32, false);
            clear.SetPixel(0, 0, Color.clear);
            clear.Apply();
            material.SetTexture("_Tex", clear);
            Check(float.IsNaN(Draw(Matrix4x4.TRS(Center, Quaternion.identity, Vector3.one),
                new Vector4(.5f, .5f, 0, 0), false, false)), "texture alpha still removes shadow pixels");
            Object.DestroyImmediate(clear);
            Check(!ShaderUtil.GetShaderMessages(shader).Any(message => message.severity == ShaderCompilerMessageSeverity.Error),
                "compiled ordinary/instanced directional/punctual variants have no shader errors");
            Debug.Log("MAP_SHADOW_BIAS_REGRESSION_PASS checks=" + checks + " device=" + SystemInfo.graphicsDeviceName);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    // Return the dark silhouette's pixel-X centroid, NaN when alpha clips it all.
    private static float Draw(Matrix4x4 transform, Vector4 bias, bool punctual, bool instanced)
    {
        RenderTexture target = RenderTexture.GetTemporary(Resolution, Resolution, 24,
            RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        var commands = new CommandBuffer { name = "Production ShadowCaster bias regression" };
        Texture2D pixels = null;
        RenderTexture previous = RenderTexture.active;
        try
        {
            if (punctual) material.EnableKeyword("_CASTING_PUNCTUAL_LIGHT_SHADOW");
            else material.DisableKeyword("_CASTING_PUNCTUAL_LIGHT_SHADOW");
            Matrix4x4 view = camera.worldToCameraMatrix;
            Matrix4x4 projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
            commands.SetRenderTarget(target);
            commands.ClearRenderTarget(true, true, Color.white);
            commands.SetViewProjectionMatrices(view, projection);
            commands.SetGlobalMatrix("unity_MatrixV", view);
            commands.SetGlobalMatrix("unity_MatrixVP", projection * view);
            commands.SetGlobalVector("_LightDirection", punctual ? Vector3.left : Vector3.right);
            commands.SetGlobalVector("_LightPosition", (Vector3)transform.GetColumn(3) + Vector3.right * 1000f);
            commands.SetGlobalVector("_ShadowBias", bias);
            if (instanced)
                commands.DrawMeshInstanced(quad, 0, material, shadowPass, new[] { transform,
                    Matrix4x4.Translate(Vector3.up * 1.5f) * transform }, 2);
            else commands.DrawMesh(quad, transform, material, 0, shadowPass);
            commands.SetGlobalVector("_ShadowBias", Vector4.zero);
            Graphics.ExecuteCommandBuffer(commands);
            pixels = new Texture2D(Resolution, Resolution, TextureFormat.RGBA32, false, true);
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, Resolution, Resolution), 0, 0);
            pixels.Apply();
            Color32[] values = pixels.GetPixels32();
            long sumX = 0;
            int count = 0;
            for (int i = 0; i < values.Length; i++)
                if (values[i].r < 32)
                {
                    sumX += i % Resolution;
                    count++;
                }
            return count == 0 ? float.NaN : (float)sumX / count;
        }
        finally
        {
            RenderTexture.active = previous;
            if (pixels != null) Object.DestroyImmediate(pixels);
            commands.Release();
            RenderTexture.ReleaseTemporary(target);
        }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception("Shadow bias regression failed: " + message);
        checks++;
    }
}
