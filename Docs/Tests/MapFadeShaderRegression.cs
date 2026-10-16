using System;
using System.Linq;
using System.Reflection;
using UnityEditor;
using UnityEditor.Rendering;
using Unity.Profiling;
using UnityEngine;
using UnityEngine.Rendering;
using UnityEngine.Rendering.Universal;
using UnityEngine.Experimental.Rendering.Universal;
using Object = UnityEngine.Object;

// GPU-only isolated Editor fixture. Draw the actual forward/shadow passes, not
// a C# reimplementation of their math; never load a game scene or saved story.
public static class MapFadeShaderRegression
{
    private static int checks;
    private static Material material;
    private static Mesh quad;
    private static Camera camera;
    private static Texture2D pixels;
    private static Vector3 center;
    private static int opaquePass;

    public static void Run()
    {
        try
        {
            Shader shader = AssetDatabase.LoadAssetAtPath<Shader>("Assets/DisFadeCode.shader");
            Check(shader != null, "production shader imported");
            ShaderUtil.allowAsyncCompilation = false;
            Check(!ShaderUtil.GetShaderMessages(shader).Any(message => message.severity == ShaderCompilerMessageSeverity.Error),
                "import has no shader compiler errors");
            Shader.globalRenderPipeline = "UniversalPipeline";
            material = new Material(shader);
            material.enableInstancing = true;
            Check(!material.HasProperty("_AlphaTex"), "unused AlphaTex is removed from the shader contract");
            Check(SystemInfo.supportsInstancing, "the GPU supports actual multi-instance draws");
            opaquePass = material.FindPass("DisFadeOpaque");
            Check(opaquePass >= 0 && material.FindPass("Universal Forward") == 0
                && material.FindPass("ShadowCaster") == 1, "opaque pass exists without moving forward/shadow indices");
            FeatureLifetime();
            Check(material.HasProperty("_FadeCenter") && material.GetFloat("_FadeCenter") == 0f,
                "fadeCenter exists and defaults off");
            material.SetTexture("_Tex", Texture2D.whiteTexture);
            material.SetFloat("_Alpha", 1f);
            material.SetFloat("_Show", 1f);
            material.SetFloat("_FadeCenter", 1f);
            quad = new Mesh
            {
                vertices = new[] { new Vector3(-1, -1, 0), new Vector3(1, -1, 0), new Vector3(1, 1, 0), new Vector3(-1, 1, 0) },
                uv = new[] { Vector2.zero, Vector2.right, Vector2.one, Vector2.up },
                normals = new[] { Vector3.back, Vector3.back, Vector3.back, Vector3.back },
                triangles = new[] { 0, 2, 1, 0, 3, 2 }
            };
            camera = new GameObject("fade-test-camera").AddComponent<Camera>();
            camera.enabled = false;
            camera.nearClipPlane = .1f;
            camera.farClipPlane = 100f;
            foreach (var setup in new[] {
                (pitch: 90f, yaw: 0f, zoom: 6f, aspect: 1f, ortho: true, cells: new Vector2(1, 1)),
                (pitch: 45f, yaw: 0f, zoom: 6f, aspect: 1.6f, ortho: true, cells: new Vector2(1, 1)),
                (pitch: 45f, yaw: 35f, zoom: 8f, aspect: 1.2f, ortho: true, cells: new Vector2(2, 1.5f)),
                (pitch: 45f, yaw: 0f, zoom: 4f, aspect: 1.2f, ortho: false, cells: new Vector2(1, 1)) })
            {
                center = new Vector3(12.25f, 3.5f, -6.75f);
                camera.transform.rotation = Quaternion.Euler(setup.pitch, setup.yaw, 0);
                camera.transform.position = center - camera.transform.forward * 12f;
                camera.orthographic = setup.ortho;
                camera.orthographicSize = setup.zoom;
                camera.fieldOfView = 60f;
                camera.aspect = setup.aspect;
                Vector2 searchRange = setup.cells * 3f;
                Shader.SetGlobalVector("_MapFadeCenterRange", new Vector4(searchRange.x, searchRange.y, 0, 0));
                Shader.SetGlobalVector("_MapFadeCenterPosition", center);
                Render(0, false);
                Check(Mathf.Abs(Coverage(Vector3.zero) - .2f) < .005f, "camera center retains twenty-percent opacity: " + setup);
                CheckSmoothRegion(Vector3.zero, .2f, .005f);
                Check(Mathf.Abs(Coverage(new Vector3(1.25f, 0, 0)) - .2f) < .005f
                    && Mathf.Abs(Coverage(new Vector3(0, 0, 1.25f)) - .2f) < .005f,
                    "the inner 1.5-world-unit disk retains twenty-percent opacity along X/Z: " + setup);
                Check(Mathf.Abs(Coverage(new Vector3(1.5f, 0, 0)) - .2f) < .015f,
                    "the fade begins at 1.5 rather than at the center: " + setup);
                foreach (float distance in new[] { 2f, 2.5f, 3f })
                {
                    float visibility = (distance - 1.5f) / (3.5f - 1.5f);
                    float expected = .2f + .8f * visibility * visibility;
                    Check(Mathf.Abs(Coverage(new Vector3(distance, 0, 0)) - expected) < .05f,
                        "the 1.5..3.5 ring follows squared distance at " + distance + ": " + setup);
                    CheckFadeRingRegion(new Vector3(distance, 0, 0));
                }
                Check(Coverage(new Vector3(3.5f, 0, 0)) > .9f, "X outer boundary is opaque: " + setup);
                Check(Coverage(new Vector3(0, 0, 3.5f)) > .9f, "Z outer boundary accounts for tilt: " + setup);
                Check(Coverage(new Vector3(2.1f, 0, 2.8f)) > .9f,
                    "circular world-distance boundary is unchanged by cell sizes or yaw: " + setup);
                Check(Coverage(new Vector3(3.75f, 0, 0)) == 1f && Coverage(new Vector3(0, 0, 3.75f)) == 1f,
                    "beyond 3.5 world units is fully opaque: " + setup);
            }
            Shader.SetGlobalVector("_MapFadeCenterRange", Vector4.zero);
            Render(0, false);
            Check(Mathf.Abs(Coverage(new Vector3(1.25f, 0, 0)) - .2f) < .005f
                && Mathf.Abs(Coverage(new Vector3(2.5f, 0, 0)) - .4f) < .05f
                && Coverage(new Vector3(3.75f, 0, 0)) == 1f,
                "standalone materials retain the fixed 1.5..3.5 world-unit fade ring");
            Shader.SetGlobalVector("_MapFadeCenterRange", new Vector4(.75f, .75f, 0, 0));
            Render(0, false);
            Check(Mathf.Abs(Coverage(new Vector3(1.25f, 0, 0)) - .2f) < .005f
                && Mathf.Abs(Coverage(new Vector3(2.5f, 0, 0)) - .4f) < .05f
                && Coverage(new Vector3(3.75f, 0, 0)) == 1f,
                "sub-unit map cells do not shrink or scale the world-unit fade ring");
            material.SetFloat("_FadeCenter", 0f);
            Render(0, false);
            Check(Coverage(Vector3.zero) == 1f, "ordinary Show=1 clears center fade");
            material.SetFloat("_Show", .25f);
            Render(0, false);
            Check(Mathf.Abs(Coverage(Vector3.zero) - .25f) < .05f, "explicit non-occlusion degree is retained");
            CheckSmoothRegion(Vector3.zero, .25f, .01f);
            Render(0, false, 2f);
            Check(Mathf.Abs(Coverage(Vector3.zero) - .4375f) < .01f,
                "two quarter-opacity layers blend continuously without foreground depth blocking");
            material.SetFloat("_Show", 0f);
            Render(0, false);
            Check(Coverage(Vector3.zero) == 0f, "Show=0 remains fully hidden");
            material.SetFloat("_Show", 1f);
            material.SetFloat("_Show", 0f);
            material.SetFloat("_FadeCenter", 1f);
            Render(1, true);
            Check(Coverage(Vector3.zero, true) == 1f, "hidden/fading units still cast a full shadow silhouette");
            // Use an explicitly transparent base to check the alpha cutout.
            var transparent = new Texture2D(1, 1);
            transparent.SetPixel(0, 0, Color.clear);
            transparent.Apply();
            material.SetTexture("_Tex", transparent);
            material.SetFloat("_Show", 1f);
            material.SetFloat("_FadeCenter", 0f);
            Render(0, false);
            Check(Coverage(Vector3.zero) == 0f, "forward base-texture alpha still cuts out pixels");
            Render(1, true);
            Check(Coverage(Vector3.zero, true) == 0f, "shadow base-texture alpha still cuts out pixels");
            Object.DestroyImmediate(transparent);
            OpaqueDepthAndTransparency();
            InstancedPasses();
            NativeRendererRouting();
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void Finish()
    {
        Check(!ShaderUtil.GetShaderMessages(material.shader).Any(message => message.severity == ShaderCompilerMessageSeverity.Error),
            "GPU-compiled ordinary and instanced variants have no compiler errors");
        Debug.Log("MAP_FADE_SHADER_REGRESSION_PASS checks=" + checks + " device=" + SystemInfo.graphicsDeviceName);
        EditorApplication.Exit(0);
    }

    private static void Render(int pass, bool shadow, float? secondDepthOffset = null)
    {
        var first = new DrawLayer(Matrix4x4.TRS(center, camera.transform.rotation, new Vector3(30f, 30f, 1f)), null);
        var layers = secondDepthOffset.HasValue
            ? new[] { first, new DrawLayer(Matrix4x4.TRS(center + camera.transform.forward * secondDepthOffset.Value,
                camera.transform.rotation, new Vector3(30f, 30f, 1f)), null) }
            : new[] { first };
        RenderDraws(pass, shadow, layers);
    }

    private sealed class DrawLayer
    {
        public readonly Matrix4x4 transform;
        public readonly MaterialPropertyBlock properties;

        public DrawLayer(Matrix4x4 transform, MaterialPropertyBlock properties)
        {
            this.transform = transform;
            this.properties = properties;
        }
    }

    private static DrawLayer Layer(Texture texture, float opacity, float depth = 0f, bool fade = false,
        float yaw = 0f)
    {
        var properties = new MaterialPropertyBlock();
        properties.SetTexture("_Tex", texture);
        properties.SetFloat("_Alpha", 1f);
        properties.SetFloat("_Show", opacity);
        properties.SetFloat("_FadeCenter", fade ? 1f : 0f);
        return new DrawLayer(Matrix4x4.TRS(center + camera.transform.forward * depth,
            camera.transform.rotation * Quaternion.Euler(0, yaw, 0), new Vector3(30f, 30f, 1f)), properties);
    }

    private static void RenderDraws(int pass, bool shadow, DrawLayer[] layers, bool opaqueOnly = false,
        bool transparentOnly = false, bool splitEnabled = true)
    {
        int height = 640;
        int width = Mathf.RoundToInt(height * camera.aspect);
        var target = RenderTexture.GetTemporary(width, height, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        var commands = new CommandBuffer { name = "Actual DisFadeCode pixel regression" };
        var view = camera.worldToCameraMatrix;
        var projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
        commands.SetRenderTarget(target);
        commands.ClearRenderTarget(true, true, shadow ? Color.white : Color.clear);
        commands.SetViewProjectionMatrices(view, projection);
        commands.SetGlobalMatrix("unity_MatrixV", view);
        commands.SetGlobalMatrix("unity_MatrixVP", projection * view);
        commands.SetGlobalVector("_WorldSpaceCameraPos", camera.transform.position);
        commands.SetGlobalVector("_MainLightPosition", -camera.transform.forward);
        commands.SetGlobalVector("_MainLightColor", Vector4.one);
        commands.SetGlobalFloat("_DisFadeSplitEnabled", !shadow && pass == 0 && splitEnabled ? 1f : 0f);
        commands.SetGlobalVector("unity_OrthoParams", new Vector4(camera.orthographicSize * camera.aspect * 2f,
            camera.orthographicSize * 2f, 0f, camera.orthographic ? 1f : 0f));
        // Match the configured URP feature: finish ALL opaque color/depth first,
        // then draw the remaining transparent pixels. Never interleave per object.
        if (!shadow && pass == 0 && splitEnabled && !transparentOnly)
            foreach (var layer in layers)
                commands.DrawMesh(quad, layer.transform, material, 0, opaquePass, layer.properties);
        if (!opaqueOnly)
            foreach (var layer in layers)
                commands.DrawMesh(quad, layer.transform, material, 0, pass, layer.properties);
        commands.SetGlobalFloat("_DisFadeSplitEnabled", 0f);
        Graphics.ExecuteCommandBuffer(commands);
        commands.Release();
        if (pixels != null) Object.DestroyImmediate(pixels);
        pixels = new Texture2D(width, height, TextureFormat.RGBA32, false, true);
        var previous = RenderTexture.active;
        RenderTexture.active = target;
        pixels.ReadPixels(new Rect(0, 0, width, height), 0, 0);
        pixels.Apply();
        RenderTexture.active = previous;
        RenderTexture.ReleaseTemporary(target);
    }

    private static void OpaqueDepthAndTransparency()
    {
        center = Vector3.zero;
        camera.transform.rotation = Quaternion.Euler(45, 0, 0);
        camera.transform.position = center - camera.transform.forward * 12f;
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.aspect = 1f;
        Shader.SetGlobalVector("_MapFadeCenterPosition", center);
        var red = SolidTexture(Color.red);
        var green = SolidTexture(Color.green);
        var clear = SolidTexture(Color.clear);
        try
        {
            var near = Layer(red, 1f);
            var far = Layer(green, 1f, 2f);
            RenderDraws(0, false, new[] { near, far });
            Dominant(Vector3.zero, true, "far opaque draw second cannot cover the nearer surface");
            RenderDraws(0, false, new[] { far, near });
            Dominant(Vector3.zero, true, "opaque depth is independent of renderer draw order");
            RenderDraws(0, false, new[] { near }, transparentOnly: true);
            Check(Coverage(Vector3.zero) == 0f, "opaque pixels cannot be drawn a second time by the transparent pass");
            RenderDraws(0, false, new[] { near }, transparentOnly: true, splitEnabled: false);
            Dominant(Vector3.zero, true, "previews/Renderers without the feature retain ordinary opaque visibility");
            Check(Shader.GetGlobalFloat("_DisFadeSplitEnabled") == 0f, "the split marker resets between cameras");

            var slopeRed = Layer(red, 1f, yaw: 25f);
            var slopeGreen = Layer(green, 1f, yaw: -25f);
            foreach (var layers in new[] { new[] { slopeRed, slopeGreen }, new[] { slopeGreen, slopeRed } })
            {
                RenderDraws(0, false, layers);
                foreach (float x in new[] { -2f, 2f })
                {
                    var offset = Vector3.right * x;
                    var point = SamplePoint(offset);
                    var ray = camera.ViewportPointToRay(new Vector3((point.x + .5f) / pixels.width,
                        (point.y + .5f) / pixels.height, 0));
                    var redPlane = new Plane(slopeRed.transform.MultiplyVector(Vector3.forward), center);
                    var greenPlane = new Plane(slopeGreen.transform.MultiplyVector(Vector3.forward), center);
                    bool redHit = redPlane.Raycast(ray, out float redDistance);
                    bool greenHit = greenPlane.Raycast(ray, out float greenDistance);
                    Check(redHit && greenHit, "sample intersects both crossing image planes");
                    Dominant(offset, redDistance < greenDistance,
                        "crossing tilted planes use per-pixel depth instead of their equal centers");
                }
            }

            RenderDraws(0, false, new[] { near });
            float fullRed = Pixel(Vector3.zero).r;
            RenderDraws(0, false, new[] { far });
            float fullGreen = Pixel(Vector3.zero).g;
            foreach (float opacity in new[] { .2f, .25f, .9999f })
            {
                var partial = Layer(red, opacity);
                RenderDraws(0, false, new[] { partial }, opaqueOnly: true);
                Check(Coverage(Vector3.zero) == 0f, "partial opacity cannot write opaque color/depth: " + opacity);
                // The opaque background is deliberately later in the supplied
                // object order; it must still precede the translucent foreground.
                RenderDraws(0, false, new[] { partial, far });
                var pixel = Pixel(Vector3.zero);
                Check(Mathf.Abs(pixel.r - fullRed * opacity) < .015f
                    && Mathf.Abs(pixel.g - fullGreen * (1f - opacity)) < .015f
                    && Mathf.Abs(pixel.a - 1f) < .005f,
                    "transparent foreground blends over opaque background without being overwritten: " + opacity);
                RenderDraws(0, false, new[] { partial, far }, opaqueOnly: true);
                Dominant(Vector3.zero, false, "even near-one opacity leaves the background's depth intact");
            }

            var fading = Layer(red, 1f, fade: true);
            RenderDraws(0, false, new[] { fading }, opaqueOnly: true);
            Check(Coverage(Vector3.zero) == 0f && Coverage(Vector3.right * 2.5f) == 0f,
                "fade disk/ring pixels skip opaque depth on the same renderer");
            Check(Coverage(Vector3.right * 3.75f) == 1f, "fully opaque fade exterior writes color/depth");
            RenderDraws(0, false, new[] { fading, far });
            var centerPixel = Pixel(Vector3.zero);
            Check(Mathf.Abs(centerPixel.r - fullRed * .2f) < .015f
                && Mathf.Abs(centerPixel.g - fullGreen * .8f) < .015f,
                "camera fade center reveals the background rather than occluding its depth");
            Dominant(Vector3.right * 3.75f, true, "the same renderer's opaque exterior hides farther pixels");

            foreach (var hole in new[] { Layer(red, 0f), Layer(clear, 1f) })
            {
                RenderDraws(0, false, new[] { hole, far });
                Dominant(Vector3.zero, false, "hidden and texture-alpha holes write no depth");
            }
        }
        finally
        {
            Object.DestroyImmediate(red);
            Object.DestroyImmediate(green);
            Object.DestroyImmediate(clear);
        }
    }

    // These are one native multi-instance command per pass, not a loop of
    // ordinary DrawMesh calls. Different transforms and numeric arrays expose
    // a missing vertex/fragment instance ID or accidentally uniform property.
    private static void InstancedPasses()
    {
        center = Vector3.zero;
        camera.transform.rotation = Quaternion.Euler(90f, 0f, 0f);
        camera.transform.position = center - camera.transform.forward * 12f;
        camera.orthographic = true;
        camera.orthographicSize = 6f;
        camera.aspect = 1f;
        Shader.SetGlobalVector("_MapFadeCenterPosition", center);
        var texture = SolidTexture(new Color(1f, 1f, 1f, .5f));
        var offsets = new[]
        {
            new Vector3(-4, 0, -3), new Vector3(-2, 0, -3), new Vector3(0, 0, -3),
            new Vector3(2, 0, -3), Vector3.zero, new Vector3(4, 0, 0),
            new Vector3(-4, 0, 3), new Vector3(-2, 0, 3), new Vector3(2.5f, 0, 0)
        };
        var transforms = offsets.Select(offset => Matrix4x4.TRS(center + offset,
            camera.transform.rotation, new Vector3(.55f, .55f, 1f))).ToArray();
        var show = new[] { 1f, .5f, 1f, 0f, 1f, 1f, 1f, 1f, 1f };
        var alpha = new[] { 1f, 1f, .25f, 1f, 1f, 1f, 1f, 1f, 1f };
        var fade = new[] { 0f, 0f, 0f, 0f, 1f, 1f, 0f, 0f, 1f };
        var cutoff = new[] { .25f, .25f, .25f, .25f, .25f, .25f, .75f, .25f, .25f };
        var light = new[] { .5f, 1f, 2f, 4f, .25f, 2f, 1f, 4f, 1f };
        var opacity = new[] { 1f, .5f, .25f, 0f, .2f, 1f, 0f, 1f, .4f };
        var properties = new MaterialPropertyBlock();
        properties.SetFloatArray("_Show", show);
        properties.SetFloatArray("_Alpha", alpha);
        properties.SetFloatArray("_FadeCenter", fade);
        properties.SetFloatArray("_Cutoff", cutoff);
        properties.SetFloatArray("_LightSensitivity", light);
        try
        {
            material.SetTexture("_Tex", texture);
            material.SetFloat("_Show", 0f);
            material.SetFloat("_Alpha", 0f);
            material.SetFloat("_FadeCenter", 0f);
            material.SetFloat("_Cutoff", .9f);
            material.SetFloat("_LightSensitivity", 9f);
            foreach (int pass in new[] { 0, opaquePass, 1 })
            {
                RenderInstanced(pass, transforms, properties, split: pass != 0);
                for (int i = 0; i < offsets.Length; i++)
                {
                    float expected = pass == 1 ? (i == 6 ? 0f : 1f)
                        : pass == opaquePass ? (opacity[i] == 1f ? 1f : 0f) : opacity[i];
                    var pixel = Pixel(offsets[i]);
                    float measured = pass == 1 ? 1f - pixel.a : pixel.a;
                    Check(Mathf.Abs(measured - expected) < .025f,
                        "one instanced pass preserves transform and each instance's Show/Alpha/Fade/Cutoff: "
                        + pass + "/" + i + " alpha=" + measured + "/" + expected);
                    if (pass == 1 || expected == 0f) continue;
                    float dark = 1f - (camera.transform.position.y - offsets[i].y - 8f) / 10f;
                    float expectedColor = Mathf.Pow(.5f, 1f / light[i]) * dark * expected;
                    Check(Mathf.Abs(pixel.r - expectedColor) < .025f,
                        "instanced lighting is isolated in each color pass: " + pass + "/" + i
                        + " color=" + pixel.r + "/" + expectedColor);
                }
            }
            RenderInstanced(0, transforms, properties, split: true);
            for (int i = 0; i < offsets.Length; i++)
            {
                float expected = opacity[i] == 1f ? 0f : opacity[i];
                Check(Mathf.Abs(Pixel(offsets[i]).a - expected) < .025f,
                    "instanced Forward split retains partial alpha only, without opaque duplicates: " + i);
            }
            RenderInstanced(0, transforms, properties, split: true, bothColorPasses: true);
            for (int i = 0; i < offsets.Length; i++)
                Check(Mathf.Abs(Pixel(offsets[i]).a - opacity[i]) < .025f,
                    "all instanced opaque depth precedes mixed/constant transparent blending: " + i);
            Check(material.enableInstancing, "all three real GPU pass checks use an instancing-enabled material");
            Debug.Log("MAP_FADE_INSTANCED_PASSES_PASS instances=" + transforms.Length + " draws=6");
        }
        finally
        {
            material.SetTexture("_Tex", Texture2D.whiteTexture);
            material.SetFloat("_Show", 1f);
            material.SetFloat("_Alpha", 1f);
            material.SetFloat("_Cutoff", .5f);
            material.SetFloat("_LightSensitivity", 1f);
            Object.DestroyImmediate(texture);
        }
    }

    private static void RenderInstanced(int pass, Matrix4x4[] transforms, MaterialPropertyBlock properties, bool split,
        bool bothColorPasses = false)
    {
        var target = RenderTexture.GetTemporary(640, 640, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        var commands = new CommandBuffer { name = "Real DisFade multi-instance pass regression" };
        try
        {
            var view = camera.worldToCameraMatrix;
            var projection = GL.GetGPUProjectionMatrix(camera.projectionMatrix, true);
            commands.SetRenderTarget(target);
            commands.ClearRenderTarget(true, true, pass == 1 ? Color.white : Color.clear);
            commands.SetViewProjectionMatrices(view, projection);
            commands.SetGlobalMatrix("unity_MatrixV", view);
            commands.SetGlobalMatrix("unity_MatrixVP", projection * view);
            commands.SetGlobalVector("_WorldSpaceCameraPos", camera.transform.position);
            commands.SetGlobalVector("_MainLightPosition",
                -camera.transform.forward * .5f + camera.transform.right * Mathf.Sqrt(.75f));
            commands.SetGlobalVector("_MainLightColor", Vector4.one);
            commands.SetGlobalVector("unity_OrthoParams", new Vector4(12f, 12f, 0f, 1f));
            commands.SetGlobalFloat("_DisFadeSplitEnabled", split ? 1f : 0f);
            if (bothColorPasses)
                commands.DrawMeshInstanced(quad, 0, material, opaquePass, transforms, transforms.Length, properties);
            commands.DrawMeshInstanced(quad, 0, material, pass, transforms, transforms.Length, properties);
            commands.SetGlobalFloat("_DisFadeSplitEnabled", 0f);
            Graphics.ExecuteCommandBuffer(commands);
            if (pixels != null) Object.DestroyImmediate(pixels);
            pixels = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false, true);
            var previous = RenderTexture.active;
            RenderTexture.active = target;
            pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
            pixels.Apply();
            RenderTexture.active = previous;
        }
        finally
        {
            commands.Release();
            RenderTexture.ReleaseTemporary(target);
        }
    }

    private static void FeatureLifetime()
    {
        Shader.SetGlobalFloat("_DisFadeSplitEnabled", 1f);
        var feature = ScriptableObject.CreateInstance<DisFadeOpaqueFeature>();
        var rendererObject = new GameObject("feature-lifetime-routing-probe");
        var renderer = rendererObject.AddComponent<MeshRenderer>();
        var depthTarget = RenderTexture.GetTemporary(8, 8, 24, RenderTextureFormat.Depth);
        try
        {
            Check(Shader.GetGlobalFloat("_DisFadeSplitEnabled") == 0f, "feature creation clears stale camera state");
            var pass = (RenderObjectsPass)typeof(DisFadeOpaqueFeature).GetField("pass",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(feature);
            Check(pass.renderPassEvent == RenderPassEvent.AfterRenderingSkybox
                && pass.renderPassEvent < RenderPassEvent.BeforeRenderingTransparents,
                "real URP pass is scheduled before every transparent draw");
            var filter = (FilteringSettings)typeof(RenderObjectsPass).GetField("m_FilteringSettings",
                BindingFlags.Instance | BindingFlags.NonPublic).GetValue(pass);
            Check(filter.layerMask == -1 && filter.renderQueueRange.lowerBound == RenderQueueRange.transparent.lowerBound
                && filter.renderQueueRange.upperBound == RenderQueueRange.transparent.upperBound,
                "opaque split covers every layer and the existing 2900/3000/4000 material queues");
            Check(pass.overrideMaterial == null && pass.overrideShader == null,
                "the real pass retains each renderer's textures, opacity and property block");
            renderer.sharedMaterial = material;
            var block = new MaterialPropertyBlock();
            block.SetFloat("_Alpha", 1f);
            block.SetFloat("_Show", 1f);
            block.SetFloat("_FadeCenter", 0f);
            renderer.SetPropertyBlock(block);
            DisFadeRenderRouting.Refresh(renderer);
            DisFadeRenderRouting.BeginSplitCamera();
            Check(!renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                "lifetime probe enters an opaque split camera");
            feature.Create();
            Check(renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                "feature recreation restores the previous camera's disabled Forward pass");
            var preview = new CameraData { cameraType = CameraType.Preview };
            feature.OnCameraPreCull(null, in preview);
            Check(renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                "Preview pre-cull keeps the ordinary Forward fallback enabled");
            var depthOnly = new CameraData { cameraType = CameraType.Game, targetTexture = depthTarget };
            feature.OnCameraPreCull(null, in depthOnly);
            Check(renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                "offscreen depth-only pre-cull does not enable color-pass routing");
            var gameCamera = new CameraData { cameraType = CameraType.Game };
            feature.OnCameraPreCull(null, in gameCamera);
            Check(!renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                "ordinary camera routing begins in the actual Feature pre-cull hook");
            var commands = new CommandBuffer();
            commands.SetGlobalFloat("_DisFadeSplitEnabled", 1f);
            pass.OnCameraCleanup(commands);
            Graphics.ExecuteCommandBuffer(commands);
            commands.Release();
            Check(Shader.GetGlobalFloat("_DisFadeSplitEnabled") == 0f, "real pass cleanup clears the marker after each camera");
            Check(renderer.sharedMaterial.GetShaderPassEnabled("UniversalForward"),
                "real pass cleanup restores the following camera's Forward fallback");
            Shader.SetGlobalFloat("_DisFadeSplitEnabled", 1f);
            feature.Dispose();
            Check(Shader.GetGlobalFloat("_DisFadeSplitEnabled") == 0f, "feature disposal restores the fallback");
        }
        finally
        {
            DisFadeRenderRouting.Release(renderer);
            DisFadeRenderRouting.Clear();
            RenderTexture.ReleaseTemporary(depthTarget);
            Object.DestroyImmediate(rendererObject);
            Object.DestroyImmediate(feature);
        }
    }

    // DrawMesh selects an explicit pass index and does not prove that SRP honors
    // SetShaderPassEnabled during Renderer submission. This minimal real SRP
    // probes DrawRenderers with the production LightMode tags and routing helper.
    // It runs only in the runner's isolated project; no product scene is loaded.
    public sealed class RendererProbeAsset : RenderPipelineAsset
    {
        public bool drawOpaque, drawForward, split, shaderSplit;
        public bool instancing = true;
        public bool countCalls;
        public ProfilerRecorder drawCalls;
        public long callsBefore, callsAfter;
        public DisFadeOpaqueFeature feature;
        public RenderObjectsPass cleanupPass;
        public int renders;
        public Exception error;
        public override string renderPipelineShaderTag => "UniversalPipeline";
        protected override RenderPipeline CreatePipeline() => new RendererProbePipeline(this);
    }

    private sealed class RendererProbePipeline : RenderPipeline
    {
        private readonly RendererProbeAsset asset;
        public RendererProbePipeline(RendererProbeAsset asset) { this.asset = asset; }

        protected override void Render(ScriptableRenderContext context, Camera[] cameras)
        {
            foreach (var viewCamera in cameras)
            {
                if (viewCamera != camera) continue;
                var commands = new CommandBuffer { name = "Native DisFade Renderer routing probe" };
                try
                {
                    Check(viewCamera.TryGetCullingParameters(out var parameters), "native probe has culling parameters");
                    // Use the actual production pre-cull hook. Native culling
                    // captures shader-pass eligibility, so an AddRenderPasses
                    // implementation is too late even though getters change.
                    if (asset.split)
                    {
                        var cameraData = new CameraData
                        {
                            camera = viewCamera, cameraType = CameraType.Game,
                            targetTexture = viewCamera.targetTexture
                        };
                        asset.feature.OnCameraPreCull(null, in cameraData);
                    }
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
                    commands.SetGlobalVector("_MainLightPosition", -viewCamera.transform.forward);
                    commands.SetGlobalVector("_MainLightColor", Vector4.one);
                    commands.SetGlobalFloat("_DisFadeSplitEnabled", asset.shaderSplit ? 1f : 0f);
                    commands.SetGlobalVector("unity_OrthoParams", new Vector4(
                        viewCamera.orthographicSize * viewCamera.aspect * 2f,
                        viewCamera.orthographicSize * 2f, 0f, 1f));
                    context.ExecuteCommandBuffer(commands);
                    commands.Clear();
                    var sorting = new SortingSettings(viewCamera) { criteria = SortingCriteria.CommonTransparent };
                    sorting.distanceMetric = DistanceMetric.Orthographic;
                    var filtering = new FilteringSettings(RenderQueueRange.transparent, viewCamera.cullingMask);
                    if (asset.countCalls) asset.callsBefore = asset.drawCalls.CurrentValue;
                    if (asset.drawOpaque)
                    {
                        var drawing = new DrawingSettings(new ShaderTagId("DisFadeOpaque"), sorting)
                        {
                            enableDynamicBatching = false, enableInstancing = asset.instancing, perObjectData = PerObjectData.None
                        };
                        context.DrawRenderers(culling, ref drawing, ref filtering);
                    }
                    if (asset.drawForward)
                    {
                        var drawing = new DrawingSettings(new ShaderTagId("UniversalForward"), sorting)
                        {
                            enableDynamicBatching = false, enableInstancing = asset.instancing, perObjectData = PerObjectData.None
                        };
                        context.DrawRenderers(culling, ref drawing, ref filtering);
                    }
                    if (asset.split) asset.cleanupPass.OnCameraCleanup(commands);
                    else commands.SetGlobalFloat("_DisFadeSplitEnabled", 0f);
                    context.ExecuteCommandBuffer(commands);
                    // Real URP cleanup restores pass flags before final Submit.
                    // Recorded DrawRenderers must still retain the chosen routing.
                    DisFadeRenderRouting.EndSplitCamera();
                    context.Submit();
                    if (asset.countCalls) asset.callsAfter = asset.drawCalls.CurrentValue;
                    asset.renders++;
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

    private static void NativeRendererRouting()
    {
        var previousGraphics = GraphicsSettings.renderPipelineAsset;
        var previousQuality = QualitySettings.renderPipeline;
        var previousTarget = camera.targetTexture;
        var previousActive = RenderTexture.active;
        int previousMask = camera.cullingMask;
        float previousAspect = camera.aspect;
        string previousPipelineTag = Shader.globalRenderPipeline;
        var asset = ScriptableObject.CreateInstance<RendererProbeAsset>();
        var feature = ScriptableObject.CreateInstance<DisFadeOpaqueFeature>();
        asset.feature = feature;
        asset.cleanupPass = (RenderObjectsPass)typeof(DisFadeOpaqueFeature).GetField("pass",
            BindingFlags.Instance | BindingFlags.NonPublic).GetValue(feature);
        var meshObject = new GameObject("native-routing-renderer");
        var renderer = meshObject.AddComponent<MeshRenderer>();
        meshObject.AddComponent<MeshFilter>().sharedMesh = quad;
        meshObject.transform.SetPositionAndRotation(center, camera.transform.rotation);
        meshObject.transform.localScale = new Vector3(30f, 30f, 1f);
        var source = new Material(material.shader);
        var red = SolidTexture(Color.red);
        var target = RenderTexture.GetTemporary(640, 640, 24, RenderTextureFormat.ARGB32, RenderTextureReadWrite.Linear);
        var block = new MaterialPropertyBlock();
        var batchObjects = new System.Collections.Generic.List<GameObject>();
        bool deferredCleanup = false;
        bool cleaned = false;
        Action cleanup = () =>
        {
            if (cleaned) return;
            cleaned = true;
            if (asset.drawCalls.Valid) asset.drawCalls.Dispose();
            foreach (var instance in batchObjects)
            {
                DisFadeRenderRouting.Release(instance.GetComponent<MeshRenderer>());
                Object.DestroyImmediate(instance);
            }
            DisFadeRenderRouting.EndSplitCamera();
            DisFadeRenderRouting.Release(renderer);
            DisFadeRenderRouting.Clear();
            camera.targetTexture = previousTarget;
            camera.cullingMask = previousMask;
            camera.aspect = previousAspect;
            RenderTexture.active = previousActive;
            QualitySettings.renderPipeline = previousQuality;
            GraphicsSettings.renderPipelineAsset = previousGraphics;
            Shader.globalRenderPipeline = previousPipelineTag;
            Shader.SetGlobalFloat("_DisFadeSplitEnabled", 0f);
            Object.DestroyImmediate(meshObject);
            Object.DestroyImmediate(source);
            Object.DestroyImmediate(red);
            Object.DestroyImmediate(feature);
            Object.DestroyImmediate(asset);
            RenderTexture.ReleaseTemporary(target);
        };
        try
        {
            source.SetTexture("_Tex", red);
            source.enableInstancing = true;
            source.SetFloat("_Alpha", 1f);
            source.SetFloat("_Show", 1f);
            source.SetFloat("_FadeCenter", 0f);
            source.SetFloat("_Cutoff", .5f);
            source.SetFloat("_LightSensitivity", 1f);
            source.renderQueue = 3000;
            renderer.sharedMaterial = source;
            renderer.shadowCastingMode = ShadowCastingMode.Off;
            camera.targetTexture = target;
            camera.cullingMask = 1 << meshObject.layer;
            camera.aspect = 1f;
            QualitySettings.renderPipeline = null;
            GraphicsSettings.renderPipelineAsset = asset;
            Shader.SetGlobalVector("_MapFadeCenterPosition", center);

            Action<float, bool> route = (show, fade) =>
            {
                block.Clear();
                block.SetFloat("_Alpha", 1f);
                block.SetFloat("_Show", show);
                block.SetFloat("_FadeCenter", fade ? 1f : 0f);
                renderer.SetPropertyBlock(block);
                DisFadeRenderRouting.Refresh(renderer);
            };
            Action<bool, bool, bool, bool> draw = (opaque, forward, split, shaderSplit) =>
            {
                asset.drawOpaque = opaque;
                asset.drawForward = forward;
                asset.split = split;
                asset.shaderSplit = shaderSplit;
                int previousRenders = asset.renders;
                camera.Render();
                if (asset.error != null) throw asset.error;
                Check(asset.renders == previousRenders + 1, "Camera.Render entered the actual native SRP context");
                if (pixels != null) Object.DestroyImmediate(pixels);
                pixels = new Texture2D(target.width, target.height, TextureFormat.RGBA32, false, true);
                RenderTexture.active = target;
                pixels.ReadPixels(new Rect(0, 0, target.width, target.height), 0, 0);
                pixels.Apply();
                RenderTexture.active = previousActive;
                if (asset.countCalls) asset.callsAfter = asset.drawCalls.CurrentValue;
            };

            // Real character visibility writes only these two properties. The
            // other instanced scalars must retain their source-material values,
            // not the shader defaults (_Alpha defaults to zero).
            block.Clear();
            block.SetFloat("_Show", 1f);
            block.SetFloat("_FadeCenter", 0f);
            Check(!block.HasFloat("_Alpha") && !block.HasFloat("_Cutoff")
                && !block.HasFloat("_LightSensitivity"),
                "character-like sparse input MPB leaves material defaults implicit");
            DisFadeRenderRouting.Apply(renderer, block);
            foreach (bool instancing in new[] { false, true })
            {
                asset.instancing = instancing;
                draw(false, true, false, false);
                Debug.Log("MAP_FADE_SPARSE_SINGLE instancing=" + instancing + " pixel=" + Pixel(Vector3.zero)
                    + " materialAlpha=" + renderer.sharedMaterial.GetFloat("_Alpha"));
                Dominant(Vector3.zero, true,
                    "native sparse character MPB inherits source-material Alpha/Cutoff/light: instancing=" + instancing);
                draw(true, false, true, true);
                Dominant(Vector3.zero, true,
                    "native sparse character MPB remains visible in the opaque pass: instancing=" + instancing);
            }
            asset.instancing = true;
            route(1f, false);
            draw(false, true, false, false);
            Dominant(Vector3.zero, true, "native fallback opaque Forward is visibly submitted");
            // Disable the shader's opaque-fragment discard: if Forward were
            // submitted despite its disabled flag, this would render red.
            draw(false, true, true, false);
            Check(Coverage(Vector3.zero) == 0f,
                "native split suppresses opaque Forward submission, not just its fragments");
            draw(true, false, true, true);
            Dominant(Vector3.zero, true, "native opaque route is submitted in DisFadeOpaque");
            draw(false, true, false, false);
            Dominant(Vector3.zero, true, "native next fallback camera restores opaque Forward");

            route(.25f, false);
            draw(true, false, true, true);
            Check(Coverage(Vector3.zero) == 0f, "native constant-translucent route has no opaque output");
            draw(false, true, true, true);
            Check(Mathf.Abs(Coverage(Vector3.zero) - .25f) < .01f,
                "native constant-translucent route retains its Forward blend");
            // Clear only the draw-time MPB without reclassifying: the cloned
            // source is fully opaque, so an erroneously enabled opaque pass
            // would now produce pixels instead of failing its alpha test.
            renderer.SetPropertyBlock(null);
            draw(true, false, true, true);
            Check(Coverage(Vector3.zero) == 0f,
                "native translucent variant actually disables opaque Renderer submission");
            draw(false, true, false, false);
            Dominant(Vector3.zero, true, "native translucent-variant control shader can produce opaque pixels");

            route(1f, true);
            draw(true, true, true, true);
            Check(Mathf.Abs(Coverage(Vector3.zero) - .2f) < .01f,
                "native Mixed route retains the continuously translucent center");
            Check(Coverage(Vector3.right * 3.75f) == 1f,
                "native Mixed route retains its fully opaque exterior");
            draw(true, false, true, true);
            Check(Coverage(Vector3.zero) == 0f && Coverage(Vector3.right * 3.75f) == 1f,
                "native Mixed opaque pass draws only the exterior");
            draw(false, true, true, true);
            Check(Mathf.Abs(Coverage(Vector3.zero) - .2f) < .01f
                && Coverage(Vector3.right * 3.75f) == 0f,
                "native Mixed Forward draws only the translucent center/ring");

            renderer.shadowCastingMode = ShadowCastingMode.TwoSided;
            route(0f, false);
            Check(renderer.enabled && renderer.shadowCastingMode == ShadowCastingMode.ShadowsOnly
                && renderer.sharedMaterial.GetShaderPassEnabled("ShadowCaster"),
                "hidden route keeps the Renderer and shadow pass enabled through ShadowsOnly");
            renderer.SetPropertyBlock(null);
            draw(true, false, true, false);
            Check(Coverage(Vector3.zero) == 0f,
                "native hidden ShadowsOnly suppresses opaque submission even with opaque draw-time alpha");
            draw(false, true, true, false);
            Check(Coverage(Vector3.zero) == 0f,
                "native hidden ShadowsOnly suppresses Forward submission even with opaque draw-time alpha");
            DisFadeRenderRouting.Release(renderer);
            draw(false, true, false, false);
            Dominant(Vector3.zero, true, "releasing hidden ShadowsOnly restores the original visible material");
            Check(Shader.GetGlobalFloat("_DisFadeSplitEnabled") == 0f,
                "native probe cleanup leaves no stale camera split marker");

            // Exercise the normal Renderer submission path too. Every object
            // shares the actual cached material and mesh, has a distinct
            // transform and numeric-only MPB, and is rendered at equal depth.
            // The uninstanced control must cost many more real GPU draw calls.
            meshObject.SetActive(false);
            const int batchCount = 32;
            for (int i = 0; i < batchCount; i++)
            {
                var instance = new GameObject("native-instancing-renderer-" + i);
                batchObjects.Add(instance);
                instance.transform.SetPositionAndRotation(center + camera.transform.right * (-3.5f + i % 8)
                    + camera.transform.up * (-1.5f + i / 8), camera.transform.rotation);
                instance.transform.localScale = new Vector3(.3f, .3f, 1f);
                instance.AddComponent<MeshFilter>().sharedMesh = quad;
                var batchRenderer = instance.AddComponent<MeshRenderer>();
                batchRenderer.shadowCastingMode = ShadowCastingMode.Off;
                batchRenderer.sharedMaterial = source;
                var numeric = new MaterialPropertyBlock();
                numeric.SetFloat("_Show", 1f);
                numeric.SetFloat("_FadeCenter", 0f);
                batchRenderer.SetPropertyBlock(numeric);
                DisFadeRenderRouting.Refresh(batchRenderer);
                Check(batchRenderer.sharedMaterial.enableInstancing,
                    "the cached production material enables instancing: " + i);
                if (i > 0) Check(batchRenderer.sharedMaterial == batchObjects[0].GetComponent<MeshRenderer>().sharedMaterial,
                    "same source, texture and opacity route reuse one material: " + i);
            }
            asset.instancing = true;
            draw(true, false, true, true);
            Debug.Log("MAP_FADE_SPARSE_BATCH first=" + Pixel(batchObjects[0].transform.position - center)
                + " last=" + Pixel(batchObjects[batchCount - 1].transform.position - center));
            for (int i = 0; i < batchCount; i++)
            {
                var offset = batchObjects[i].transform.position - center;
                Check(Mathf.Abs(Pixel(offset).a - 1f) < .005f && Pixel(offset).r > .05f,
                    "native sparse automatic instance inherits source-material Alpha/Cutoff/light: " + i);
            }
            draw(false, true, false, false);
            for (int i = 0; i < batchCount; i++)
                Check(Mathf.Abs(Pixel(batchObjects[i].transform.position - center).a - 1f) < .005f,
                    "native sparse automatic instance remains visible in fallback Forward: " + i);
            // Now exercise independent non-default values in the same batch.
            for (int i = 0; i < batchCount; i++)
            {
                var batchRenderer = batchObjects[i].GetComponent<MeshRenderer>();
                batchRenderer.GetPropertyBlock(block);
                block.SetFloat("_Alpha", 1f);
                block.SetFloat("_LightSensitivity", .5f + i * .1f);
                block.SetFloat("_Cutoff", .2f + (i % 3) * .1f);
                DisFadeRenderRouting.Apply(batchRenderer, block);
            }
            draw(true, false, true, true);
            for (int i = 0; i < batchCount; i++)
            {
                var offset = batchObjects[i].transform.position - center;
                Check(Mathf.Abs(Pixel(offset).a - 1f) < .005f && Pixel(offset).r > .05f,
                    "native automatic instance keeps its distinct transform and numeric MPB: " + i);
            }
            for (int i = 0; i < batchCount; i++)
            {
                var batchRenderer = batchObjects[i].GetComponent<MeshRenderer>();
                batchRenderer.GetPropertyBlock(block);
                block.SetFloat("_Alpha", i % 2 == 0 ? .25f : .5f);
                batchRenderer.SetPropertyBlock(block);
                DisFadeRenderRouting.Refresh(batchRenderer);
                if (i > 0) Check(batchRenderer.sharedMaterial == batchObjects[0].GetComponent<MeshRenderer>().sharedMaterial,
                    "different constant alpha uses the same transparent cached material: " + i);
            }
            draw(false, true, true, true);
            for (int i = 0; i < batchCount; i++)
            {
                float expected = i % 2 == 0 ? .25f : .5f;
                Check(Mathf.Abs(Pixel(batchObjects[i].transform.position - center).a - expected) < .01f,
                    "native automatic transparent instances retain their own alpha: " + i);
            }
            // Rendering counters are published at the end of an Editor frame,
            // not synchronously by Camera.Render/Submit. Keep the isolated
            // context alive and compare warmed frame medians below.
            StartNativeBatchCounters(asset, batchObjects, draw, cleanup);
            deferredCleanup = true;
        }
        finally
        {
            if (!deferredCleanup) cleanup();
        }
    }

    private static void StartNativeBatchCounters(RendererProbeAsset asset,
        System.Collections.Generic.List<GameObject> objects, Action<bool, bool, bool, bool> draw, Action cleanup)
    {
        bool previousProfiler = UnityEngine.Profiling.Profiler.enabled;
        bool previousRenderingArea = UnityEngine.Profiling.Profiler.GetAreaEnabled(UnityEngine.Profiling.ProfilerArea.Rendering);
        UnityEngine.Profiling.Profiler.SetAreaEnabled(UnityEngine.Profiling.ProfilerArea.Rendering, true);
        UnityEngine.Profiling.Profiler.enabled = true;
        // 2022.3's batchmode can omit Rendering counter publication even when
        // Camera.Render has produced verified GPU pixels. The Editor's native
        // FrameDebugger event stream identifies real InstancedMesh submissions
        // directly; use it as stronger evidence than a zero statistic.
        var debugger = typeof(EditorApplication).Assembly.GetType("UnityEditorInternal.FrameDebuggerInternal.FrameDebuggerUtility");
        var enableDebugger = debugger?.GetMethod("SetEnabled", BindingFlags.Public | BindingFlags.Static);
        var eventsMethod = debugger?.GetMethod("GetFrameEvents", BindingFlags.Public | BindingFlags.Static);
        var locallySupported = debugger?.GetProperty("locallySupported", BindingFlags.Public | BindingFlags.Static);
        bool debuggerSupported = enableDebugger != null && eventsMethod != null && locallySupported != null
            && enableDebugger.GetParameters().Length == 2 && (bool)locallySupported.GetValue(null);
        var drawEvents = new int[4];
        var instanceEvents = new int[4];
        Action release = () =>
        {
            if (debuggerSupported) enableDebugger.Invoke(null, new object[] { false, 0 });
            UnityEngine.Profiling.Profiler.SetAreaEnabled(UnityEngine.Profiling.ProfilerArea.Rendering, previousRenderingArea);
            UnityEngine.Profiling.Profiler.enabled = previousProfiler;
            cleanup();
        };
        asset.drawCalls = ProfilerRecorder.StartNew(ProfilerCategory.Render, "Draw Calls Count", 64);
        Check(asset.drawCalls.Valid, "native automatic-instancing probe has a real draw-call counter");
        var frameCalls = new System.Collections.Generic.List<long>[4];
        for (int i = 0; i < frameCalls.Length; i++) frameCalls[i] = new System.Collections.Generic.List<long>();
        int phase = 0, frames = 0;
        var numeric = new MaterialPropertyBlock();
        Action configure = () =>
        {
            for (int i = 0; i < objects.Count; i++)
            {
                objects[i].SetActive(phase != 0);
                var targetRenderer = objects[i].GetComponent<MeshRenderer>();
                targetRenderer.GetPropertyBlock(numeric);
                numeric.SetFloat("_Alpha", phase == 3 ? (i % 2 == 0 ? .25f : .5f) : 1f);
                targetRenderer.SetPropertyBlock(numeric);
                DisFadeRenderRouting.Refresh(targetRenderer);
            }
            asset.instancing = phase >= 2;
            if (debuggerSupported)
            {
                enableDebugger.Invoke(null, new object[] { false, 0 });
                enableDebugger.Invoke(null, new object[] { true, 0 });
            }
        };
        configure();
        double deadline = EditorApplication.timeSinceStartup + 30;
        EditorApplication.CallbackFunction tick = null;
        tick = () =>
        {
            try
            {
                Check(EditorApplication.timeSinceStartup < deadline, "native frame counter probe finishes within 30 seconds");
                // Two warm-up updates exclude the previous phase. Counters are
                // frame totals, so baseline rendering/Editor overhead is measured
                // in phase zero and subtracted from each later median.
                if (frames >= 2) frameCalls[phase].Add(asset.drawCalls.LastValue);
                if (++frames >= 8)
                {
                    if (debuggerSupported)
                    {
                        var events = (Array)eventsMethod.Invoke(null, null);
                        foreach (var item in events)
                        {
                            string eventType = item.GetType().GetField("m_Type").GetValue(item).ToString();
                            if (eventType == "Mesh" || eventType == "InstancedMesh") drawEvents[phase]++;
                            if (eventType == "InstancedMesh") instanceEvents[phase]++;
                        }
                        Debug.Log("MAP_FADE_NATIVE_FRAME_EVENTS phase=" + phase + " all=" + events.Length
                            + " draws=" + drawEvents[phase] + " instances=" + instanceEvents[phase]);
                    }
                    phase++;
                    frames = 0;
                    if (phase == frameCalls.Length)
                    {
                        EditorApplication.update -= tick;
                        var medians = frameCalls.Select(values => values.OrderBy(value => value).ElementAt(values.Count / 2)).ToArray();
                        long ordinary = medians[1] - medians[0], opaque = medians[2] - medians[0], transparent = medians[3] - medians[0];
                        Debug.Log("MAP_FADE_NATIVE_INSTANCING_COUNTS baseline=" + medians[0] + " ordinary=" + ordinary
                            + " opaque=" + opaque + " transparent=" + transparent + " renderers=" + objects.Count);
                        bool frameProof = drawEvents[1] >= objects.Count && instanceEvents[2] > 0 && instanceEvents[3] > 0
                            && drawEvents[2] < drawEvents[1] && drawEvents[3] < drawEvents[1];
                        bool counterProof = ordinary >= objects.Count && opaque > 0 && opaque < ordinary && transparent > 0 && transparent < ordinary;
                        bool idProof = false;
                        if (!frameProof && !counterProof)
                        {
                            if (debuggerSupported) enableDebugger.Invoke(null, new object[] { false, 0 });
                            idProof = NativeInstanceIds(asset, objects, draw);
                        }
                        Check(frameProof || counterProof || idProof,
                            "native DrawRenderers performs actual opaque/transparent automatic GPU instancing; counter="
                            + ordinary + "/" + opaque + "/" + transparent + " native events="
                            + drawEvents[1] + "/" + drawEvents[2] + "/" + drawEvents[3]);
                        release();
                        Finish();
                        return;
                    }
                    configure();
                }
                draw(phase == 1 || phase == 2, phase == 3, true, true);
                EditorApplication.QueuePlayerLoopUpdate();
            }
            catch (Exception exception)
            {
                EditorApplication.update -= tick;
                release();
                Debug.LogException(exception);
                EditorApplication.Exit(1);
            }
        };
        EditorApplication.update += tick;
        EditorApplication.QueuePlayerLoopUpdate();
    }

    private static bool NativeInstanceIds(RendererProbeAsset asset,
        System.Collections.Generic.List<GameObject> objects, Action<bool, bool, bool, bool> draw)
    {
        // This asset exists only inside the runner's isolated project. Preserve
        // production instance buffers, vertex/fragment IDs and alpha/depth math;
        // replace the final RGB output solely to observe the hardware instance
        // index. Subtract Unity's base offset rather than mistaking an upload
        // buffer offset for actual GPU instance progression.
        string source = System.IO.File.ReadAllText(System.IO.Path.Combine(Application.dataPath, "DisFadeCode.shader"));
        source = source.Replace("Shader \"DisFadeCode\"", "Shader \"DisFadeInstancingIdProbe\"");
        int includeEnd = source.IndexOf("ENDHLSL", StringComparison.Ordinal);
        Check(includeEnd >= 0, "instance-ID probe retains the shared production shader include");
        const string probeFunction = "\nfloat4 DisFadeInstanceProbeColor()\n{\n"
            + "#if defined(UNITY_INSTANCING_ENABLED)\n"
            + "    uint hardwareId = unity_InstanceID - unity_BaseInstanceID;\n"
            + "#else\n    uint hardwareId = 0;\n#endif\n"
            + "    return float4((hardwareId + 1.0) / 255.0, 0.0, 0.0, 1.0);\n}\n";
        source = source.Insert(includeEnd, probeFunction);
        source = source.Replace("return half4(diffuse.rgb * finalAlpha, finalAlpha);", "return DisFadeInstanceProbeColor();")
            .Replace("return half4(diffuse, 1.0h);", "return DisFadeInstanceProbeColor();");
        Check(source.Split(new[] { "return DisFadeInstanceProbeColor();" }, StringSplitOptions.None).Length == 3,
            "instance-ID probe instruments both color passes without changing their instance setup/clip");
        const string path = "Assets/DisFadeInstancingIdProbe.shader";
        System.IO.File.WriteAllText(System.IO.Path.Combine(Application.dataPath, "DisFadeInstancingIdProbe.shader"), source);
        AssetDatabase.ImportAsset(path, ImportAssetOptions.ForceSynchronousImport);
        var shader = AssetDatabase.LoadAssetAtPath<Shader>(path);
        Check(shader != null, "isolated instance-ID shader imports");
        var original = objects[0].GetComponent<DisFadeMaterialBinding>().Source;
        var probe = new Material(original) { shader = shader, enableInstancing = true };
        var block = new MaterialPropertyBlock();
        try
        {
            foreach (var instance in objects)
            {
                var target = instance.GetComponent<MeshRenderer>();
                DisFadeRenderRouting.Release(target);
                target.sharedMaterial = probe;
                target.GetPropertyBlock(block);
                block.SetFloat("_Alpha", 1f);
                target.SetPropertyBlock(block);
            }
            asset.instancing = false;
            draw(true, false, false, true);
            foreach (var instance in objects)
            {
                var pixel = Pixel(instance.transform.position - center);
                Check(pixel.a > .99f && Mathf.RoundToInt(pixel.r * 255f) == 1,
                    "ordinary native Renderer control has hardware instance ID zero: " + pixel);
            }
            asset.instancing = true;
            draw(true, false, false, true);
            var ids = objects.Select(instance => Mathf.RoundToInt(Pixel(instance.transform.position - center).r * 255f) - 1).ToArray();
            Check(ids.Min() == 0 && ids.Max() == objects.Count - 1 && ids.Distinct().Count() == objects.Count,
                "automatic native DrawRenderers has consecutive distinct hardware GPU instance IDs, not separate ID-zero draws: "
                + string.Join(",", ids));
            foreach (var instance in objects)
            {
                var target = instance.GetComponent<MeshRenderer>();
                target.GetPropertyBlock(block);
                block.SetFloat("_Alpha", .25f);
                target.SetPropertyBlock(block);
            }
            asset.instancing = false;
            draw(false, true, false, true);
            foreach (var instance in objects)
                Check(Mathf.RoundToInt(Pixel(instance.transform.position - center).r * 255f) == 1,
                    "ordinary transparent Forward control also has hardware instance ID zero");
            asset.instancing = true;
            draw(false, true, false, true);
            var forwardIds = objects.Select(instance =>
                Mathf.RoundToInt(Pixel(instance.transform.position - center).r * 255f) - 1).ToArray();
            Check(forwardIds.Min() == 0 && forwardIds.Max() == objects.Count - 1
                && forwardIds.Distinct().Count() == objects.Count,
                "automatic transparent Forward also has consecutive distinct hardware GPU instance IDs: "
                + string.Join(",", forwardIds));
            Check(!ShaderUtil.GetShaderMessages(shader).Any(message => message.severity == ShaderCompilerMessageSeverity.Error),
                "the instrumented production-layout instancing variants have no compiler errors");
            Debug.Log("MAP_FADE_NATIVE_INSTANCE_IDS_PASS rendererCount=" + objects.Count
                + " opaqueIds=" + string.Join(",", ids) + " forwardIds=" + string.Join(",", forwardIds));
            return true;
        }
        finally
        {
            foreach (var instance in objects) instance.GetComponent<MeshRenderer>().sharedMaterial = original;
            Object.DestroyImmediate(probe);
        }
    }

    private static Texture2D SolidTexture(Color color)
    {
        var texture = new Texture2D(1, 1, TextureFormat.RGBA32, false, true);
        texture.SetPixel(0, 0, color);
        texture.Apply();
        return texture;
    }

    private static Color Pixel(Vector3 offset)
    {
        var point = SamplePoint(offset);
        return pixels.GetPixel(point.x, point.y);
    }

    private static void Dominant(Vector3 offset, bool red, string message)
    {
        var pixel = Pixel(offset);
        Check((red ? pixel.r > .05f && pixel.g < .005f : pixel.g > .05f && pixel.r < .005f)
            && Mathf.Abs(pixel.a - 1f) < .005f, message + ": " + pixel);
    }

    private static float Coverage(Vector3 groundOffset, bool shadow = false)
    {
        // Sample on the center-depth image plane, matching the projected ground
        // circle rather than letting the test point's own depth change its UV.
        Vector2Int point = SamplePoint(groundOffset);
        float covered = 0f;
        for (int dy = -5; dy <= 5; dy++)
        for (int dx = -5; dx <= 5; dx++)
        {
            Check(point.x + dx >= 0 && point.x + dx < pixels.width && point.y + dy >= 0 && point.y + dy < pixels.height,
                "sample region is on screen");
            float alpha = pixels.GetPixel(point.x + dx, point.y + dy).a;
            covered += shadow ? 1f - alpha : alpha;
        }
        return covered / 121f;
    }

    private static Vector2Int SamplePoint(Vector3 groundOffset)
    {
        Vector3 sample = center + camera.transform.right * Vector3.Dot(camera.transform.right, groundOffset)
            + camera.transform.up * Vector3.Dot(camera.transform.up, groundOffset);
        Vector3 viewport = camera.WorldToViewportPoint(sample);
        return new Vector2Int(Mathf.RoundToInt(viewport.x * pixels.width), Mathf.RoundToInt(viewport.y * pixels.height));
    }

    private static void CheckSmoothRegion(Vector3 groundOffset, float expected, float tolerance)
    {
        Vector2Int point = SamplePoint(groundOffset);
        for (int dy = -5; dy <= 5; dy++)
        for (int dx = -5; dx <= 5; dx++)
        {
            float alpha = pixels.GetPixel(point.x + dx, point.y + dy).a;
            Check(Mathf.Abs(alpha - expected) < tolerance,
                "each pixel is continuously translucent, never a remaining opaque dither speck: " + alpha);
        }
    }

    private static void CheckFadeRingRegion(Vector3 groundOffset)
    {
        Vector2Int point = SamplePoint(groundOffset);
        var right = camera.transform.right;
        var up = camera.transform.up;
        float depth = Vector3.Dot(center - camera.transform.position, camera.transform.forward);
        float determinant = right.x * up.z - right.z * up.x;
        for (int dy = -5; dy <= 5; dy++)
        for (int dx = -5; dx <= 5; dx++)
        {
            int x = point.x + dx, y = point.y + dy;
            // A narrow fade ring has appreciable variation within the sample
            // patch, especially with camera yaw/zoom. Verify each pixel's own
            // world distance rather than comparing all pixels to the patch center.
            var pixelWorld = camera.ViewportToWorldPoint(new Vector3(
                (x + .5f) / pixels.width, (y + .5f) / pixels.height, depth));
            var offset = pixelWorld - center;
            float viewX = Vector3.Dot(right, offset), viewY = Vector3.Dot(up, offset);
            var ground = new Vector2((viewX * up.z - viewY * right.z) / determinant,
                (viewY * right.x - viewX * up.x) / determinant);
            float visibility = Mathf.Clamp01((ground.magnitude - 1.5f) / (3.5f - 1.5f));
            float expected = .2f + .8f * visibility * visibility;
            float alpha = pixels.GetPixel(x, y).a;
            Check(Mathf.Abs(alpha - expected) < .02f,
                "fade-ring pixel matches its own world-distance opacity, without dither: " + alpha + "/" + expected);
        }
    }

    private static void Check(bool success, string message)
    {
        checks++;
        if (!success) throw new Exception("FAIL: " + message);
    }
}
