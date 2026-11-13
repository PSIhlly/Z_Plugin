using System;
using System.Collections.Generic;
using System.IO;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Texture;

// Isolated native Texture2D tests; no real story or asset registry is modified.
public static class GifDecoderRegression
{
    private static int checks;

    public static void Run()
    {
        try
        {
            ReferenceImages();
            // Short/odd heights exercise empty passes and the final partial pass.
            foreach (int height in new[] { 1, 2, 3, 4, 5, 7, 8, 9, 15, 16, 17 })
                foreach (bool interlaced in new[] { false, true })
                    foreach (bool offset in new[] { false, true })
                        Scanlines(height, interlaced, offset);
            AnimatedFrames();
            Debug.Log("GIF_DECODER_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static void ReferenceImages()
    {
        var directory = Path.Combine(Application.dataPath, "../GifReferences");
        if (!Directory.Exists(directory)) return;
        foreach (var path in Directory.GetFiles(directory, "*.gif"))
        {
            var bytes = File.ReadAllBytes(path);
            var asset = new TexAssetForm.Data(-1, "imported-gif", "", bytes, "", null, 0);
            var frames = asset.GetGifFrames();
            try
            {
                Check(frames.Count == 1, Path.GetFileName(path) + " is a single-frame GIF");
                Check(asset.GetTex() == frames[0].texture && asset.GetAnimationFrames() == frames,
                    "imported asset preview and animation share the correctly decoded image");
                var pixels = frames[0].texture.GetPixels32();
                var expected = File.ReadAllBytes(Path.ChangeExtension(path, ".rgba"));
                Check(expected.Length == pixels.Length * 4, "reference pixel count");
                for (int i = 0; i < pixels.Length; i++)
                {
                    var pixel = pixels[i];
                    if (pixel.r != expected[i * 4] || pixel.g != expected[i * 4 + 1]
                        || pixel.b != expected[i * 4 + 2] || pixel.a != expected[i * 4 + 3])
                        throw new InvalidOperationException(Path.GetFileName(path) + " differs from the reference at pixel " + i);
                }
                Check(true, Path.GetFileName(path) + " matches independent reference pixels");
                File.WriteAllBytes(Path.ChangeExtension(path, ".decoded.png"), frames[0].texture.EncodeToPNG());
            }
            finally { Release(frames); }
        }
    }

    private static void Scanlines(int height, bool interlaced, bool offset)
    {
        int left = offset ? 2 : 0, top = offset ? 3 : 0;
        int width = 3 + left, canvasHeight = height + top;
        var frames = GifDecoder.Decode(BuildGif(height, interlaced, left, top));
        try
        {
            Check(frames.Count == 1, "one image descriptor produces one frame");
            var texture = frames[0].texture;
            Check(texture.width == width && texture.height == canvasHeight, "logical canvas size is preserved");
            for (int y = 0; y < canvasHeight; y++)
                for (int x = 0; x < width; x++)
                {
                    var actual = (Color32)texture.GetPixel(x, canvasHeight - 1 - y);
                    var expected = x < left || y < top ? new Color32(0, 0, 0, 0)
                        : Palette((y - top + x - left) % 16);
                    if (!actual.Equals(expected))
                        throw new InvalidOperationException($"height={height}, interlaced={interlaced}, offset={offset}: pixel ({x},{y})");
                }
            Check(true, "scanlines, orientation and descriptor offsets match the authored image");
        }
        finally { Release(frames); }
    }

    private static byte[] BuildGif(int height, bool interlaced, int left, int top)
    {
        using (var stream = new MemoryStream())
        using (var writer = new BinaryWriter(stream))
        {
            writer.Write(System.Text.Encoding.ASCII.GetBytes("GIF89a"));
            writer.Write((ushort)(3 + left)); writer.Write((ushort)(height + top));
            writer.Write((byte)0x83); writer.Write((byte)0); writer.Write((byte)0);
            for (int i = 0; i < 16; i++)
            {
                var color = Palette(i);
                writer.Write(color.r); writer.Write(color.g); writer.Write(color.b);
            }
            writer.Write((byte)0x2c); writer.Write((ushort)left); writer.Write((ushort)top);
            writer.Write((ushort)3); writer.Write((ushort)height); writer.Write((byte)(interlaced ? 0x40 : 0));
            writer.Write((byte)4);
            var codes = new List<int>();
            var starts = interlaced ? new[] { 0, 4, 2, 1 } : new[] { 0 };
            var steps = interlaced ? new[] { 8, 8, 4, 2 } : new[] { 1 };
            for (int pass = 0; pass < starts.Length; pass++)
                for (int y = starts[pass]; y < height; y += steps[pass])
                    for (int x = 0; x < 3; x++)
                    {
                        codes.Add(16); // Clear before every pixel: fixed five-bit literal codes.
                        codes.Add((y + x) % 16);
                    }
            codes.Add(17);
            var packed = new byte[(codes.Count * 5 + 7) / 8];
            for (int i = 0; i < codes.Count; i++)
                for (int bit = 0; bit < 5; bit++)
                    if ((codes[i] & (1 << bit)) != 0)
                        packed[(i * 5 + bit) / 8] |= (byte)(1 << ((i * 5 + bit) % 8));
            writer.Write((byte)packed.Length); writer.Write(packed);
            writer.Write((byte)0); writer.Write((byte)0x3b);
            return stream.ToArray();
        }
    }

    private static Color32 Palette(int i) => new Color32((byte)(i * 13), (byte)(255 - i * 11), (byte)(i * 7), 255);

    private static void AnimatedFrames()
    {
        var bytes = new List<byte> { 71, 73, 70, 56, 57, 97, 1, 0, 1, 0, 128, 0, 0, 255, 0, 0, 0, 0, 255 };
        foreach (int frame in new[] { 0, 1 })
            bytes.AddRange(new byte[] { 33, 249, 4, 0, (byte)(frame == 0 ? 10 : 20), 0, 0, 0,
                44, 0, 0, 0, 0, 1, 0, 1, 0, 0, 2, 2, (byte)(frame == 0 ? 68 : 76), 1, 0 });
        bytes.Add(59);
        var asset = new TexAssetForm.Data(-1, "animated-gif", "", bytes.ToArray(), "", null, 0);
        var frames = asset.GetGifFrames();
        try
        {
            Check(frames.Count == 2, "ordinary animated GIF retains both frames");
            Check(frames[0].texture.GetPixel(0, 0).r > .9f && frames[1].texture.GetPixel(0, 0).b > .9f,
                "ordinary animation frame colors remain correct");
            Check(Mathf.Approximately(frames[0].delaySeconds, .1f) && Mathf.Approximately(frames[1].delaySeconds, .2f),
                "authored frame delays are preserved");
            Check(asset.GetTex() == frames[0].texture, "static consumers use the first decoded frame");
            Check(asset.GetAnimationFrames() == frames, "animation consumers share the decoded frames");
        }
        finally { Release(frames); }
    }

    private static void Release(List<GifFrameData> frames)
    {
        if (frames == null) return;
        foreach (var frame in frames) UnityEngine.Object.DestroyImmediate(frame.texture);
    }

    private static void Check(bool success, string message)
    {
        if (!success) throw new InvalidOperationException(message);
        checks++;
    }
}
