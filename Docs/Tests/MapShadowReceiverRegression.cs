using System;
using System.Collections.Generic;
using System.Linq;
using UnityEditor.Rendering;
using UnityEditor;
using UnityEngine;
using UnityEngine.Rendering;
using Object = UnityEngine.Object;

// Reproduce URP's empty-shadow fallback and preserve real shadows in both color passes.
public static class MapShadowReceiverRegression
{
    public static void Run()
    {
        try
        {
            ShaderUtil.allowAsyncCompilation = false;
            Shader.globalRenderPipeline = "UniversalPipeline";
            var material = new Material(AssetDatabase.LoadAssetAtPath<Shader>("Assets/DisFadeCode.shader"));
            material.enableInstancing = true;
            material.SetTexture("_Tex", Texture2D.whiteTexture);
            material.SetFloat("_Alpha", 1);
            material.SetFloat("_Show", 1);
            material.SetFloat("_FadeCenter", 0);
            material.SetFloat("_Cutoff", .2f);
            material.SetFloat("_LightSensitivity", 2);
            var primitive = GameObject.CreatePrimitive(PrimitiveType.Quad);
            Mesh mesh = primitive.GetComponent<MeshFilter>().sharedMesh;
            Object.DestroyImmediate(primitive);
            var camera = new GameObject("isolated-ground-coverage-camera").AddComponent<Camera>();
            camera.enabled = false;
            camera.orthographic = true;
            camera.orthographicSize = 3;
            camera.aspect = 1;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 1000;
            var emptyShadow = new RenderTexture(1, 1, 16, RenderTextureFormat.Shadowmap);
            emptyShadow.Create();
            int cases = 0, failures = 0;
            var unshadowedMeans = new Dictionary<string, double>();
            foreach (Vector3 center in new[] { Vector3.zero, new Vector3(496, 500, 519) })
            foreach (float yaw in new[] { 0f, 45f, 130f })
            foreach (int samples in new[] { 1, 2 })
            foreach (bool shadows in new[] { false, true })
            foreach (bool fading in new[] { false, true })
            foreach (bool transparent in new[] { false, true })
            {
                material.SetFloat("_Alpha", transparent ? .5f : 1f);
                camera.transform.rotation = Quaternion.Euler(45, yaw, 0);
                camera.transform.position = center - camera.transform.forward * 12;
                Matrix4x4 view = camera.worldToCameraMatrix;
                Matrix4x4 projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
                var descriptor = new RenderTextureDescriptor(256, 256, RenderTextureFormat.ARGB32, 24)
                    { msaaSamples = samples, sRGB = false };
                var target = RenderTexture.GetTemporary(descriptor);
                var resolved = RenderTexture.GetTemporary(256, 256, 0, RenderTextureFormat.ARGB32,
                    RenderTextureReadWrite.Linear);
                var commands = new CommandBuffer();
                commands.SetRenderTarget(emptyShadow);
                commands.ClearRenderTarget(true, false, Color.clear, 0f);
                commands.SetRenderTarget(target);
                commands.ClearRenderTarget(true, true, Color.magenta);
                commands.SetViewProjectionMatrices(view, projection);
                commands.SetGlobalMatrix("unity_MatrixV", view);
                commands.SetGlobalMatrix("unity_MatrixVP", projection * view);
                commands.SetGlobalVector("_WorldSpaceCameraPos", camera.transform.position);
                commands.SetGlobalVector("_MainLightPosition", new Vector4(0, .94f, -.342f, 0));
                commands.SetGlobalVector("_MainLightColor", new Vector4(.5f, .5f, .5f, 1));
                commands.SetGlobalTexture("_MainLightShadowmapTexture", emptyShadow);
                commands.SetGlobalVector("_MainLightShadowParams", new Vector4(1, 0, fading ? 1 : 0, 0));
                commands.SetGlobalVector("_MainLightShadowmapSize", new Vector4(1, 1, 1, 1));
                Matrix4x4 staleShadow = Matrix4x4.zero;
                staleShadow.m03 = .5f;
                staleShadow.m13 = .5f;
                staleShadow.m22 = 10f;
                staleShadow.m23 = -center.z * 10f + .5f;
                commands.SetGlobalMatrixArray("_MainLightWorldToShadow", new[] {
                    staleShadow, staleShadow, staleShadow, staleShadow, staleShadow });
                material.DisableKeyword("_MAIN_LIGHT_SHADOWS_CASCADE");
                if (shadows) material.EnableKeyword("_MAIN_LIGHT_SHADOWS");
                else material.DisableKeyword("_MAIN_LIGHT_SHADOWS");
                commands.SetGlobalVector("_CascadeShadowSplitSphereRadii", Vector4.zero);
                commands.SetGlobalFloat("_DisFadeSplitEnabled", 0f);
                var grounds = new Matrix4x4[81];
                var fronts = new Matrix4x4[81];
                int index = 0;
                for (int x = -4; x <= 4; x++)
                for (int z = -4; z <= 4; z++)
                {
                    Vector3 position = center + new Vector3(x, 0, z);
                    grounds[index] = Matrix4x4.TRS(position + new Vector3(0, -.01999998f, 0),
                        Quaternion.Euler(90, 0, 0), new Vector3(1, 1.001f, 1));
                    fronts[index] = Matrix4x4.TRS(position + new Vector3(0, -.53f, -.5f),
                        Quaternion.identity, new Vector3(1.01f, 1.01f, 1));
                    index++;
                }
                commands.DrawMeshInstanced(mesh, 0, material, transparent ? 0 : 2, grounds, grounds.Length);
                commands.DrawMeshInstanced(mesh, 0, material, transparent ? 0 : 2, fronts, fronts.Length);
                commands.Blit(target, resolved);
                Graphics.ExecuteCommandBuffer(commands);
                var previous = RenderTexture.active;
                RenderTexture.active = resolved;
                var pixels = new Texture2D(256, 256, TextureFormat.RGBA32, false, true);
                pixels.ReadPixels(new Rect(0, 0, 256, 256), 0, 0);
                pixels.Apply();
                var values = pixels.GetPixels32();
                int dark = 0, gap = 0;
                long greenSum = 0;
                for (int y = 80; y < 176; y++)
                for (int x = 80; x < 176; x++)
                {
                    var c = values[y * 256 + x];
                    greenSum += c.g;
                    if (c.g < 4) dark++;
                    if (c.r > 240 && c.g < 4 && c.b > 240) gap++;
                }
                double greenMean = (double)greenSum / (96 * 96);
                string key = center + "|" + yaw + "|" + samples + "|" + transparent;
                if (!shadows) unshadowedMeans[key] = greenMean;
                Debug.Log("SHADOW_RECEIVER center=" + center + " yaw=" + yaw + " MSAA=" + samples
                    + " shadowKeyword=" + shadows + " distanceFade=" + fading + " transparent=" + transparent
                    + " dark=" + dark + " gaps=" + gap + " meanGreen=" + greenMean
                    + " litMean=" + unshadowedMeans[key]);
                bool expectsRealShadow = shadows && !fading;
                // Transparent unshadowed front faces can blend over a shadowed
                // ground pixel: check reduced light, not a fully black pixel.
                if (expectsRealShadow ? greenMean >= unshadowedMeans[key] - .1
                    : dark != 0 || gap != 0 || Math.Abs(greenMean - unshadowedMeans[key]) > .1)
                    failures++;
                cases++;
                RenderTexture.active = previous;
                Object.DestroyImmediate(pixels);
                commands.Release();
                RenderTexture.ReleaseTemporary(target);
                RenderTexture.ReleaseTemporary(resolved);
            }
            if (ShaderUtil.GetShaderMessages(material.shader).Any(message => message.severity == ShaderCompilerMessageSeverity.Error))
                throw new Exception("Shadow receiver has GPU shader compilation errors.");
            if (failures != 0) throw new Exception("Shadow receiver failed " + failures + " of " + cases + " cases.");
            Debug.Log("MAP_SHADOW_RECEIVER_REGRESSION_PASS cases=" + cases);
            Object.DestroyImmediate(material);
            Object.DestroyImmediate(camera.gameObject);
            Object.DestroyImmediate(emptyShadow);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }
}
