using System;
using System.Linq;
using UnityEditor;
using UnityEditor.Rendering;
using UnityEngine;
using UnityEngine.Rendering;
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
            Check(material.HasProperty("_FadeCenter") && material.GetFloat("_FadeCenter") == 0f,
                "fadeCenter exists and defaults off");
            material.SetTexture("_Tex", Texture2D.whiteTexture);
            material.SetTexture("_AlphaTex", Texture2D.whiteTexture);
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
            material.SetTexture("_AlphaTex", Texture2D.blackTexture);
            Render(0, false);
            Check(Coverage(Vector3.zero) == 0f, "forward mask cutouts are retained");
            material.SetTexture("_AlphaTex", Texture2D.whiteTexture);
            material.SetFloat("_Show", 0f);
            material.SetFloat("_FadeCenter", 1f);
            Render(1, true);
            Check(Coverage(Vector3.zero, true) == 1f, "hidden/fading units still cast a full shadow silhouette");
            material.SetTexture("_AlphaTex", Texture2D.blackTexture);
            Render(1, true);
            Check(Coverage(Vector3.zero, true) == 0f, "shadow mask still cuts out pixels");
            material.SetTexture("_AlphaTex", Texture2D.whiteTexture);
            // Use an explicitly transparent base to check the alpha cutout.
            var transparent = new Texture2D(1, 1);
            transparent.SetPixel(0, 0, Color.clear);
            transparent.Apply();
            material.SetTexture("_Tex", transparent);
            Render(1, true);
            Check(Coverage(Vector3.zero, true) == 0f, "shadow base-texture alpha still cuts out pixels");
            Object.DestroyImmediate(transparent);
            Check(!ShaderUtil.GetShaderMessages(shader).Any(message => message.severity == ShaderCompilerMessageSeverity.Error),
                "GPU-compiled forward and shadow variants have no compiler errors");
            Debug.Log("MAP_FADE_SHADER_REGRESSION_PASS checks=" + checks + " device=" + SystemInfo.graphicsDeviceName);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void Render(int pass, bool shadow, float? secondDepthOffset = null)
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
        commands.SetGlobalVector("unity_OrthoParams", new Vector4(camera.orthographicSize * camera.aspect * 2f,
            camera.orthographicSize * 2f, 0f, camera.orthographic ? 1f : 0f));
        var matrix = Matrix4x4.TRS(center, camera.transform.rotation, new Vector3(30f, 30f, 1f));
        commands.DrawMesh(quad, matrix, material, 0, pass);
        if (secondDepthOffset.HasValue)
        {
            // Draw farther geometry second on purpose: transparent surfaces must
            // not write depth and prevent another transparent surface rendering.
            var second = Matrix4x4.TRS(center + camera.transform.forward * secondDepthOffset.Value,
                camera.transform.rotation, new Vector3(30f, 30f, 1f));
            commands.DrawMesh(quad, second, material, 0, pass);
        }
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
