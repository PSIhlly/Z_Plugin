using System;
using System.Collections.Generic;
using System.Diagnostics;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Z_Map;
using Debug = UnityEngine.Debug;

public static class MapEdgeTransitionRegression
{
    private const BindingFlags Fields = BindingFlags.Instance | BindingFlags.NonPublic;
    private static int checks;
    private static T Get<T>(MapEdgeTransition edge, string name) => (T)typeof(MapEdgeTransition).GetField(name, Fields).GetValue(edge);
    private static void Build(MapEdgeTransition edge, MapInfo map) =>
        typeof(MapEdgeTransition).GetMethod("BuildMask", Fields).Invoke(edge, new object[] { map });
    private static void Check(bool value, string message)
    {
        if (!value) throw new Exception(message);
        checks++;
    }
    private static MapInfo Map(int min, int max, Func<int, int, bool> includes)
    {
        var map = new MapInfo { mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>(), mapVersion = 1 };
        for (int z = min; z <= max; z++)
            for (int x = min; x <= max; x++)
                if (includes(x, z)) map.mapXZ2Y.Add((x, z), new SortedSet<int> { 0 });
        return map;
    }
    private static MapEdgeTransition Create(float smoothing)
    {
        var edge = new GameObject("Edge regression").AddComponent<MapEdgeTransition>();
        edge.cornerSmoothing = smoothing;
        return edge;
    }
    private static void Dispose(MapEdgeTransition edge)
    {
        foreach (string name in new[] { "mask", "mesh", "material" })
        {
            var owned = Get<UnityEngine.Object>(edge, name);
            if (owned != null) UnityEngine.Object.DestroyImmediate(owned);
        }
        UnityEngine.Object.DestroyImmediate(edge.gameObject);
    }
    public static void Run()
    {
        var sharp = Create(0f);
        var smooth = Create(0.5f);
        try
        {
            TestEuclidean(sharp);
            TestCorners(sharp, smooth);
            TestProfiles(sharp, smooth);
            TestSymmetry(smooth);
            TestRefresh(smooth);
            TestBudgetAndReuse(smooth);
            SavePreview(sharp, smooth);
            Debug.Log("MAP_EDGE_TRANSITION_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally { Dispose(sharp); Dispose(smooth); }
    }
    private static void TestEuclidean(MapEdgeTransition edge)
    {
        var map = Map(-5, 5, (x, z) => x < 0 || z < 0);
        Build(edge, map);
        var mask = Get<Texture2D>(edge, "mask");
        var origin = Get<Vector2>(edge, "maskOrigin");
        int r = mask.width / (int)Get<Vector2>(edge, "maskCells").x;
        var empty = new List<Vector2Int>();
        for (int z = 0; z < mask.height; z++)
            for (int x = 0; x < mask.width; x++)
                if (!map.mapXZ2Y.ContainsKey(((int)origin.x + x / r, (int)origin.y + z / r)))
                    empty.Add(new Vector2Int(x, z));
        var distances = Get<float[]>(edge, "distance");
        float error = 0f;
        for (int z = 0; z < mask.height; z++)
            for (int x = 0; x < mask.width; x++)
            {
                float squared = Mathf.Pow(Mathf.Ceil(edge.fadeWidth * r) + 2f, 2f);
                foreach (var source in empty)
                {
                    int dx = source.x - x, dz = source.y - z;
                    squared = Mathf.Min(squared, dx * dx + dz * dz);
                }
                error = Mathf.Max(error, Mathf.Abs(distances[z * mask.width + x] - Mathf.Sqrt(squared)));
            }
        Check(error < 0.0001f, "Concave corners must match brute-force Euclidean distance, not octagonal chamfer distance: " + error);
    }
    private static void TestCorners(MapEdgeTransition sharp, MapEdgeTransition smooth)
    {
        var map = Map(0, 12, (x, z) => true);
        Build(sharp, map); Build(smooth, map);
        int width = Get<Texture2D>(sharp, "mask").width;
        // Four texels/cell, two-cell margin: sample the inward diagonal crease.
        int i = (8 + 2) * width + 8 + 2;
        var a = Get<float[]>(sharp, "distance");
        var b = Get<float[]>(smooth, "distance");
        float sharpBend = Mathf.Abs(a[i - width + 1] + a[i + width - 1] - 2f * a[i]);
        float softBend = Mathf.Abs(b[i - width + 1] + b[i + width - 1] - 2f * b[i]);
        Check(softBend < sharpBend * 0.65f, "Corner smoothing removes the abrupt diagonal gradient crease: " + sharpBend + " -> " + softBend);
        for (int n = 0; n < a.Length; n++)
            Check((a[n] >= 6f || b[n] <= a[n] + 0.0001f) && b[n] >= 0f,
                "Smoothing must never reveal missing cells or weaken an existing dark border");
    }
    private static float Alpha(MapEdgeTransition edge, float x, float z)
    {
        var origin = Get<Vector2>(edge, "maskOrigin");
        var cells = Get<Vector2>(edge, "maskCells");
        // Match the shader's texel-center convention explicitly. Unity's CPU
        // GetPixelBilinear uses a different half-texel convention in this Editor.
        var mask = Get<Texture2D>(edge, "mask");
        float tx = (x - origin.x + 0.5f) / cells.x * mask.width - 0.5f;
        float tz = (z - origin.y + 0.5f) / cells.y * mask.height - 0.5f;
        int ix = Mathf.FloorToInt(tx), iz = Mathf.FloorToInt(tz);
        int x0 = Mathf.Clamp(ix, 0, mask.width - 1), x1 = Mathf.Clamp(ix + 1, 0, mask.width - 1);
        int z0 = Mathf.Clamp(iz, 0, mask.height - 1), z1 = Mathf.Clamp(iz + 1, 0, mask.height - 1);
        float packed = Mathf.Lerp(Mathf.Lerp(mask.GetPixel(x0, z0).a, mask.GetPixel(x1, z0).a, tx - ix),
            Mathf.Lerp(mask.GetPixel(x0, z1).a, mask.GetPixel(x1, z1).a, tx - ix), tz - iz);
        float coverage = Mathf.Clamp01((packed - Get<float>(edge, "maskMin")) * Get<float>(edge, "maskScale"));
        float fade = Mathf.Max(0.0001f, edge.fadeWidth);
        float start = Mathf.Min(0.9995f, Mathf.Clamp01(Mathf.Clamp(edge.blackWidth, 0f, fade) / fade));
        float t = Mathf.Clamp01((coverage - start) / Mathf.Max(0.0005f, 1f - start));
        float v = 1f - t;
        return Mathf.Clamp01(v + v * (1f - v) * (-0.107868f + v * (2.339329f + v * (-1.058959f - 0.315335f * v))));
    }
    private static void TestProfiles(MapEdgeTransition sharp, MapEdgeTransition smooth)
    {
        var map = Map(0, 12, (x, z) => true);
        foreach (float fade in new[] { 0f, 0.25f, 1f, 2f })
        {
            sharp.fadeWidth = smooth.fadeWidth = fade;
            Build(sharp, map); Build(smooth, map);
            for (int step = 0; step <= 24; step++)
            {
                float x = -0.5f + step / 8f;
                Check(Mathf.Abs(Alpha(sharp, x, 6f) - Alpha(smooth, x, 6f)) < 0.0001f,
                    "Straight-edge profile/width remains unchanged at fade=" + fade);
            }
        }
        sharp.fadeWidth = smooth.fadeWidth = 1f;
        // blackWidth 默认 0（边界处就开始淡出），黑带用例显式给半格。
        sharp.blackWidth = smooth.blackWidth = 0.5f;
        Build(smooth, map);
        foreach (float d in new[] { 0f, 0.25f, 0.5f })
            Check(Alpha(smooth, -0.5f + d, 6f) > 0.985f, "Half-cell black band stays opaque at " + d + ": " + Alpha(smooth, -0.5f + d, 6f));
        Check(Mathf.Abs(Alpha(smooth, 0.25f, 6f) - 0.6894f) < 0.015f, "Preserve the authored UI gradient at 0.75 cell");
        Check(Alpha(smooth, 0.5f, 6f) < 0.015f, "Straight edge is clear at one cell");
        smooth.blackWidth = 0f;
        Check(Alpha(smooth, -0.5f, 6f) > 0.99f, "Zero blackWidth must not leak across the true border");
        Check(Mathf.Abs(Alpha(smooth, -0.25f, 6f) - 0.9221f) < 0.015f, "Zero blackWidth keeps the authored curve at 0.25 cell");
        Check(Mathf.Abs(Alpha(smooth, 0f, 6f) - 0.6894f) < 0.015f, "Zero blackWidth keeps the authored curve at 0.5 cell");
        Check(Alpha(smooth, 0.5f, 6f) < 0.015f, "Zero blackWidth is clear at one cell");
        smooth.blackWidth = 0.5f;
        Build(smooth, Map(-5, 5, (x, z) => x != 0 || z != 0));
        Check(Alpha(smooth, 0f, 0f) == 1f, "An interior hole stays fully black");
    }
    private static void TestSymmetry(MapEdgeTransition edge)
    {
        var map = Map(-5, 5, (x, z) => x < 0 || z < 0);
        Build(edge, map);
        var mask = Get<Texture2D>(edge, "mask");
        var original = (float[])Get<float[]>(edge, "distance").Clone();
        var reflected = Map(-5, 5, (x, z) => -x < 0 || z < 0);
        Build(edge, reflected);
        var result = Get<float[]>(edge, "distance");
        for (int z = 0; z < mask.height; z++)
            for (int x = 0; x < mask.width; x++)
                Check(Mathf.Abs(original[z * mask.width + x] - result[z * mask.width + mask.width - 1 - x]) < 0.0001f,
                    "Mirrored concave/convex corners must not depend on traversal direction");
    }
    private static void TestBudgetAndReuse(MapEdgeTransition edge)
    {
        var map = Map(0, 250, (x, z) => true);
        Build(edge, map);
        var mask = Get<Texture2D>(edge, "mask");
        Check((long)mask.width * mask.height <= MapEdgeTransition.MAX_MASK_TEXELS, "Keep the one-million-texel budget");
        var distances = Get<float[]>(edge, "distance");
        var smoothing = Get<float[]>(edge, "smoothBuffer");
        var lines = Get<float[]>(edge, "lineInput");
        var watch = Stopwatch.StartNew();
        Build(edge, map);
        Debug.Log("MAP_EDGE_REBUILD_MS=" + watch.Elapsed.TotalMilliseconds.ToString("F2") + " texels=" + mask.width * mask.height);
        Check(ReferenceEquals(mask, Get<Texture2D>(edge, "mask")) && ReferenceEquals(distances, Get<float[]>(edge, "distance"))
            && ReferenceEquals(smoothing, Get<float[]>(edge, "smoothBuffer")) && ReferenceEquals(lines, Get<float[]>(edge, "lineInput")),
            "Repeated equal-size rebuilds reuse the texture and every work buffer");
    }
    private static void TestRefresh(MapEdgeTransition edge)
    {
        var map = Map(0, 12, (x, z) => true);
        var manager = new MapManager { data = map };
        var refresh = typeof(MapEdgeTransition).GetMethod("RefreshMask", Fields);
        edge.rebuildInterval = 0f;
        refresh.Invoke(edge, new object[] { manager });
        var mask = Get<Texture2D>(edge, "mask");
        uint version = mask.updateCount;
        refresh.Invoke(edge, new object[] { manager });
        Check(mask.updateCount == version, "Unchanged frames do not rebuild the mask");
        edge.cornerSmoothing = 0f;
        refresh.Invoke(edge, new object[] { manager });
        Check(mask.updateCount > version && Get<float>(edge, "maskSmoothing") == 0f,
            "Changing corner smoothing invalidates the cached mask");
        edge.cornerSmoothing = 0.5f;
        map.mapXZ2Y.Remove((6, 6)); map.mapVersion++;
        refresh.Invoke(edge, new object[] { manager });
        Check(Alpha(edge, 6f, 6f) == 1f && Get<int>(edge, "maskVersion") == map.mapVersion,
            "Editing creates a correctly darkened hole on the next allowed rebuild");
    }
    private static void SavePreview(MapEdgeTransition sharp, MapEdgeTransition smooth)
    {
        var map = Map(0, 12, (x, z) => (x < 8 || z < 7) && !(x >= 3 && x <= 4 && z >= 3 && z <= 4));
        sharp.fadeWidth = smooth.fadeWidth = 1f;
        Build(sharp, map); Build(smooth, map);
        const int size = 384;
        var preview = new Texture2D(size * 2, size, TextureFormat.RGB24, false);
        try
        {
            var pixels = new Color32[size * size * 2];
            for (int z = 0; z < size; z++)
                for (int x = 0; x < size; x++)
                    for (int panel = 0; panel < 2; panel++)
                    {
                        float px = -1f + (x + 0.5f) * 14f / size, pz = -1f + (z + 0.5f) * 14f / size;
                        float alpha = Alpha(panel == 0 ? sharp : smooth, px, pz);
                        bool grid = Mathf.Repeat(px + 0.5f, 1f) < 0.025f || Mathf.Repeat(pz + 0.5f, 1f) < 0.025f;
                        byte value = (byte)((grid ? 185f : 235f) * (1f - alpha));
                        pixels[z * size * 2 + panel * size + x] = new Color32(value, value, value, 255);
                    }
            preview.SetPixels32(pixels); preview.Apply();
            string path = Path.GetFullPath(Path.Combine(Application.dataPath, "../edge-corners-preview.png"));
            File.WriteAllBytes(path, preview.EncodeToPNG());
            Debug.Log("MAP_EDGE_PREVIEW=" + path);
        }
        finally { UnityEngine.Object.DestroyImmediate(preview); }
    }
}

