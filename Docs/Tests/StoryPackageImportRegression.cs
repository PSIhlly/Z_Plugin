using System;
using System.IO;
using System.Linq;
using System.Reflection;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_Texture;
using Z_UnitSystem;

// Run only in the isolated project created by Run-MapRuntimeRegression.ps1.
public static class StoryPackageImportRegression
{
    private static int checks;
    private static readonly byte[] png = Convert.FromBase64String(
        "iVBORw0KGgoAAAANSUhEUgAAAAEAAACACAYAAADK+QP0AAAABGdBTUEAALGPC/xhBQAAAAZiS0dEAAAAAAAA+UO7fwAAADBJREFUKM9j+P//PxMTAwPDSCMYmRgYGJhRWUPbR8xYPIhfCbaAwK8Ymzb8YvgJBgDyBAT7NxexAgAAAABJRU5ErkJggg==");

    public static async void Run()
    {
        var previousPath = SaveAndLoad.perPath;
        var root = Path.GetFullPath(Path.Combine(Application.dataPath, "../ModImportTest-" + Guid.NewGuid().ToString("N")));
        var initialRows = StoryForm.DataById.Values.ToArray();
        try
        {
            Directory.CreateDirectory(root);
            SaveAndLoad.perPath = root;
            TestPixelContainer();
            var ctrl = new GameSaveController(null);
            var original = new StoryForm.Data(1, "Existing story", "keep", "", "original-seed");
            StoryForm.AddData(original);
            SaveAndLoad.Save("1/Core/sf", StoryForm.GetJoByData(original).ToString());
            SaveAndLoad.Save("2/Save/keep", "orphan save must survive");
            var before = File.ReadAllBytes(Path.Combine(root, "1/Core/sf"));

            var author = new StoryForm.Data(81, "测试导入", "description", "", "authored-seed");
            var files = new JObject { ["sf"] = Encoding.UTF8.GetBytes(StoryForm.GetJoByData(author).ToString()),
                ["ast\\nested\\42"] = new byte[] { 0, 255, 10, 13, 128 },
                ["/ast"] = new JObject(), ["/ast/nested"] = new JObject(),
                ["saf"] = Encoding.UTF8.GetBytes("[]") };
            var bytes = Pack(files);
            Check(Encoding.UTF8.GetString(TextureHelper.GetExtraInfoByPNG(bytes)) == files.ToString(),
                "Production pixel package preserves the complete serialized filesystem JSON");
            var imported = ctrl.ImportPackage(bytes);
            Check(imported.id == 3, "Skip registered ID and orphan Save root");
            Check(imported.name == author.name && imported.desc == author.desc && imported.randomSeed == author.randomSeed,
                "Preserve imported metadata and random seed");
            Check(ReferenceEquals(StoryForm.DataById[1], original), "Preserve unrelated row identity");
            Check(File.ReadAllBytes(Path.Combine(root, "1/Core/sf")).SequenceEqual(before), "Existing Core unchanged");
            Check(File.ReadAllText(Path.Combine(root, "2/Save/keep")) == "orphan save must survive", "Existing Save unchanged");
            Check(File.ReadAllBytes(Path.Combine(root, "3/Core/ast/nested/42")).SequenceEqual(new byte[] { 0, 255, 10, 13, 128 }),
                "Normalize separators and preserve binary assets");
            Check(JObject.Parse(File.ReadAllText(Path.Combine(root, "3/Core/sf"))).Value<int>("id") == imported.id,
                "Persist the new local overview ID");
            var absoluteOverview = Path.Combine(root, "3/Core/sf");
            Check(SaveAndLoad.GetRealPath(absoluteOverview) == Path.GetFullPath(absoluteOverview), "Absolute paths are resolved once");
            Check(SaveAndLoad.Exist(absoluteOverview) && JObject.Parse(SaveAndLoad.Load<string>(absoluteOverview)).Value<int>("id") == 3,
                "Reload can read an absolute overview path outside a HlZy-named install");
            Check(SaveAndLoad.GetRealPath("ast/HlZy-test") == Path.GetFullPath(Path.Combine(root, "ast/HlZy-test")),
                "A relative filename containing HlZy is still relative");
            var again = ctrl.ImportPackage(bytes);
            Check(again.id == 4 && !ReferenceEquals(again, imported), "Duplicate import creates an independent story");
            Check(!Directory.Exists(Path.Combine(root, "3/Save")) && !Directory.Exists(Path.Combine(root, "3/Cache")),
                "Do not import/create player Save or Cache");

            // Round-trip the production filesystem packer (not a test encoder).
            var repackaged = Pack(JObject.Parse(SaveAndLoad.Package("3/Core/")));
            var roundtrip = ctrl.ImportPackage(repackaged);
            Check(roundtrip.id == 5 && roundtrip.name == author.name, "Production Package can be imported again");
            Check(File.ReadAllBytes(Path.Combine(root, "5/Core/ast/nested/42")).SequenceEqual(new byte[] { 0, 255, 10, 13, 128 }),
                "Production packer preserves nested asset bytes");

            // Read compatibility only: newly written packages never emit this chunk.
            var legacyBytes = LegacyPack(files);
            Check(Encoding.UTF8.GetString(TextureHelper.GetExtraInfoByPNG(legacyBytes)) == files.ToString(),
                "Legacy ZEXTRA text packages remain readable");
            var legacyImported = ctrl.ImportPackage(legacyBytes);
            Check(legacyImported.id == 6 && legacyImported.name == author.name,
                "Import a legacy text-chunk package through the same transaction");
            Check(File.ReadAllBytes(Path.Combine(root, "6/Core/ast/nested/42")).SequenceEqual(new byte[] { 0, 255, 10, 13, 128 }),
                "Legacy package binary assets remain byte-for-byte compatible");

            Reject(ctrl, null, "Null package");
            Reject(ctrl, Encoding.UTF8.GetBytes("not a package"), "Arbitrary document");
            Reject(ctrl, png, "Plain PNG has no embedded story");
            Reject(ctrl, bytes.Take(bytes.Length - 1).ToArray(), "Truncated package");
            Reject(ctrl, ResizePackage(bytes), "A resized image is rejected without writing a story");
            Reject(ctrl, MutateRgbHeader(bytes, 19), "Incorrect declared payload length");
            Reject(ctrl, MutateRgbHeader(bytes, 16, 255), "Payload allocation limit is enforced before decompression");
            Reject(ctrl, MutateRgbHeader(bytes, 20, 255), "Packed payload exceeds available image pixels");
            Reject(ctrl, MutateRgbHeader(bytes, 32), "Incorrect payload SHA-256 rejects the package");
            Reject(ctrl, MutateRgbHeader(bytes, 11, 2), "Unknown package header version");
            Reject(ctrl, MutateRgbHeader(bytes, 15, 2), "Unknown package compression codec");
            Reject(ctrl, MutateRgbHeader(bytes, 28, 1), "Reserved header flags must be zero");
            var badCrc = (byte[])bytes.Clone();
            badCrc[29] ^= 1;
            Reject(ctrl, badCrc, "PNG CRC corruption is rejected");
            var huge = (byte[])bytes.Clone();
            huge[8] = 255; huge[9] = 255; huge[10] = 255; huge[11] = 255;
            Reject(ctrl, huge, "Oversized PNG chunk rejected before allocation");
            Reject(ctrl, Pack(new JObject()), "Missing overview");
            Reject(ctrl, Pack(new JObject { ["sf"] = Encoding.UTF8.GetBytes("{}")} ), "Invalid overview");
            Reject(ctrl, Pack(new JObject { ["sf"] = Encoding.UTF8.GetBytes("invalid JSON")} ), "Malformed overview");
            var legacy = StoryForm.GetJoByData(author);
            legacy["icon"] = new JArray(new JObject { ["v"] = 255 });
            Reject(ctrl, Pack(new JObject { ["sf"] = Encoding.UTF8.GetBytes(legacy.ToString()) }), "Legacy cover schema not silently lost");
            var badBase64 = (JObject)files.DeepClone();
            badBase64["ast/invalid"] = "@@@";
            Reject(ctrl, Pack(badBase64), "Invalid asset Base64");
            foreach (var path in new[] { "../outside", "ast/../../outside", "ast\\..\\..\\outside", "C:/outside",
                "\\outside", "/outside", "ast/./sf", "ast//sf", "ast/bad.", "ast/bad " })
            {
                var malicious = (JObject)files.DeepClone();
                malicious[path] = new byte[] { 1 };
                Reject(ctrl, Pack(malicious), "Unsafe file key: " + path);
            }
            var collision = (JObject)files.DeepClone();
            collision["ast/nested/42"] = new byte[] { 1 };
            Reject(ctrl, Pack(collision), "Normalized duplicate file path");
            var nested = (JObject)files.DeepClone();
            nested["/directory"] = new JObject { ["../outside"] = new byte[] { 1 } };
            Reject(ctrl, Pack(nested), "Unsafe nested file path");
            var fileDirectoryConflict = (JObject)files.DeepClone();
            fileDirectoryConflict["ast"] = new byte[] { 1 };
            Reject(ctrl, Pack(fileDirectoryConflict), "Write failure rolls back staged import");
            var previousAdd = StoryForm.addAction;
            try
            {
                StoryForm.addAction += row => { throw new InvalidOperationException("Simulated registration failure"); };
                Reject(ctrl, bytes, "Registration failure rolls back the published directory and row");
            }
            finally { StoryForm.addAction = previousAdd; }
            var occupied = new System.Collections.Generic.List<int>();
            try
            {
                for (int id = 1; id < StoryForm.autoIdCnt; id++)
                {
                    if (StoryForm.DataById.ContainsKey(id)) continue;
                    StoryForm.AddData(new StoryForm.Data(id, "occupied", "", "", ""));
                    occupied.Add(id);
                }
                Reject(ctrl, bytes, "Exhausted IDs do not write or overwrite a story");
            }
            finally { foreach (var id in occupied) StoryForm.RemoveData(id); }
            Check(!Directory.EnumerateDirectories(root, ".mod-import-*").Any(), "No failed staging directory remains");
            Check(!File.Exists(Path.Combine(root, "outside")), "No traversal write");
            Check(StoryForm.DataById.Count == 5, "Only successful imports register rows");
            ctrl.LoadOverview();
            Check(StoryForm.DataById.Keys.OrderBy(id => id).SequenceEqual(new[] { 1, 3, 4, 5, 6 }), "Startup overview reload discovers every imported story");
            Check(StoryForm.DataById[3].name == author.name && StoryForm.DataById[3].randomSeed == author.randomSeed,
                "Imported metadata survives startup overview reload");
            await TestAsyncPackageWorkflow(ctrl, root, bytes);
            Debug.Log("STORY_PACKAGE_IMPORT_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            foreach (var row in StoryForm.DataById.Values.ToArray())
                if (!initialRows.Contains(row)) StoryForm.RemoveData(row.id);
            SaveAndLoad.perPath = previousPath;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static async Task TestAsyncPackageWorkflow(GameSaveController ctrl, string root, byte[] bytes)
    {
        var mainThread = Thread.CurrentThread.ManagedThreadId;
        var payload = Encoding.UTF8.GetBytes("worker snapshot payload");
        var cover = CreateCover();
        var snapshot = PngPixelPackage.CaptureCover(cover);
        UnityEngine.Object.DestroyImmediate(cover);
        var encoded = await Task.Run(() => PngPixelPackage.EncodeWithCover(snapshot, payload));
        Check(TextureHelper.GetExtraInfoByPNG(encoded).SequenceEqual(payload),
            "A worker encodes a cover snapshot after its Unity texture was destroyed");
        Check(Thread.CurrentThread.ManagedThreadId == mainThread, "Async continuation returns to Unity's thread");

        var selected = Path.Combine(root, "pickedMedia.tmp");
        File.WriteAllBytes(selected, bytes);
        var previousAdd = StoryForm.addAction;
        try
        {
            StoryForm.addAction += row => Check(Thread.CurrentThread.ManagedThreadId == mainThread,
                "Async import registers Forms only on the Unity thread");
            var story = await ctrl.ImportPackageAsync(selected);
            Check(story.id == 7 && StoryForm.DataById[7] == story, "Async import publishes a new overview");
            Check(File.ReadAllBytes(Path.Combine(root, "7/Core/ast/nested/42")).SequenceEqual(new byte[] { 0, 255, 10, 13, 128 }),
                "Async import preserves nested binary assets and gallery cache paths");
            Check(JObject.Parse(File.ReadAllText(Path.Combine(root, "7/Core/sf"))).Value<int>("id") == 7,
                "Async import writes the allocated ID before publishing Core");

            var expected = SaveAndLoad.Package("7/Core/");
            var exported = await ctrl.PackageAsync(7);
            Check(Encoding.UTF8.GetString(TextureHelper.GetExtraInfoByPNG(exported)) == expected,
                "Async export preserves the production filesystem payload");
            var textureCount = Resources.FindObjectsOfTypeAll<Texture2D>().Length;
            var capture = typeof(GameSaveController).GetMethod("CapturePackageCover", BindingFlags.NonPublic | BindingFlags.Static);
            capture.Invoke(null, new object[] { 7 });
            Check(Resources.FindObjectsOfTypeAll<Texture2D>().Length == textureCount,
                "Capturing an export cover releases its private decoded texture");
            Check(Texture2D.whiteTexture != null && Texture2D.whiteTexture.width > 0,
                "Export cover cleanup preserves shared fallback textures");

            using (var canceled = new CancellationTokenSource())
            {
                canceled.Cancel();
                try { await ctrl.ImportPackageAsync(selected, canceled.Token); Check(false, "Canceled import must stop"); }
                catch (OperationCanceledException) { Check(true, "Canceled import is not published"); }
            }
            Check(StoryForm.DataById.Count == 6 && !Directory.Exists(Path.Combine(root, "8")),
                "Cancellation leaves no overview or target directory");

            var malformed = Path.Combine(root, "malformed.png");
            File.WriteAllBytes(malformed, bytes.Take(bytes.Length - 1).ToArray());
            try { await ctrl.ImportPackageAsync(malformed); Check(false, "Async malformed package must fail"); }
            catch (InvalidDataException) { Check(true, "Async decoder rejects malformed packages"); }

            StoryForm.addAction += row => { throw new InvalidOperationException("Async registration failure"); };
            try { await ctrl.ImportPackageAsync(selected); Check(false, "Async registration failure must roll back"); }
            catch (InvalidOperationException) { Check(true, "Async registration failure propagates"); }
            Check(StoryForm.DataById.Count == 6 && !Directory.Exists(Path.Combine(root, "8")),
                "Async registration failure rolls back only its own published root");
        }
        finally { StoryForm.addAction = previousAdd; }

        var count = StoryForm.DataById.Count;
        var requests = new[] { ctrl.ImportPackageAsync(selected), ctrl.ImportPackageAsync(selected) };
        var imported = new System.Collections.Generic.List<StoryForm.Data>();
        foreach (var request in requests)
        {
            try { imported.Add(await request); }
            catch (IOException) { Check(true, "A raced directory is rejected without overwrite"); }
            catch (InvalidOperationException) { Check(true, "A raced Form ID is rejected without overwrite"); }
        }
        Check(imported.Count >= 1 && imported.Select(row => row.id).Distinct().Count() == imported.Count,
            "Concurrent requests never publish the same story ID twice");
        Check(StoryForm.DataById.Count == count + imported.Count, "Only committed async requests register rows");
        foreach (var story in imported)
            Check(JObject.Parse(File.ReadAllText(Path.Combine(root, story.id.ToString(), "Core/sf"))).Value<int>("id") == story.id,
                "Concurrent import persisted overview matches its registered row");
        Check(!Directory.EnumerateDirectories(root, ".mod-import-*").Any(), "Async success/failure/cancel leave no staging directory");
        Check(File.ReadAllText(Path.Combine(root, "2/Save/keep")) == "orphan save must survive",
            "Async operations leave unrelated Save roots unchanged");
    }

    private static byte[] Pack(JObject files)
    {
        var cover = CreateCover();
        try
        {
            return TextureHelper.GetPNGWithExtraInfo(cover, Encoding.UTF8.GetBytes(files.ToString()));
        }
        finally { UnityEngine.Object.DestroyImmediate(cover); }
    }

    private static byte[] LegacyPack(JObject files)
    {
        var insert = typeof(TextureHelper).GetMethod("InsertTextChunk", BindingFlags.NonPublic | BindingFlags.Static);
        return (byte[])insert.Invoke(null, new object[] { png, "ZEXTRA", Convert.ToBase64String(Encoding.UTF8.GetBytes(files.ToString())) });
    }

    private static Texture2D CreateCover()
    {
        var cover = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        cover.SetPixels32(new[] { new Color32(20, 30, 40, 255), new Color32(50, 60, 70, 255),
            new Color32(80, 90, 100, 255), new Color32(110, 120, 130, 255) });
        cover.Apply();
        return cover;
    }

    private static void TestPixelContainer()
    {
        var cover = CreateCover();
        try
        {
            var binary = Enumerable.Range(0, 256).Select(value => (byte)value).ToArray();
            var large = new byte[128 * 1024 + 7];
            new System.Random(1717).NextBytes(large);
            foreach (var payload in new[] { new byte[0], new byte[] { 0 }, binary, large })
            {
                var encoded = TextureHelper.GetPNGWithExtraInfo(cover, payload);
                Check(TextureHelper.GetExtraInfoByPNG(encoded).SequenceEqual(payload),
                    "Pixel payload exact round trip: " + payload.Length + " bytes");
                Check(encoded[24] == 8 && encoded[25] == 2 && encoded[28] == 0,
                    "Export is a standard 8-bit RGB non-interlaced PNG");
                var chunkTypes = GetChunkTypes(encoded);
                Check(chunkTypes.First() == "IHDR" && chunkTypes.Last() == "IEND", "Standard PNG chunk ordering");
                Check(chunkTypes.Contains("IDAT") && !chunkTypes.Any(type => type == "tEXt" || type == "zTXt" || type == "iTXt"),
                    "PNG payload is in image pixels, never a text metadata chunk");
                var image = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
                try
                {
                    Check(image.LoadImage(encoded), "Unity can decode exported package as an ordinary image");
                    Check(image.width >= cover.width && image.height >= cover.height, "Package includes its cover area");
                    var header = ReadRgbHeader(image);
                    Check(Encoding.ASCII.GetString(header, 0, 8) == "ZPKGPNG1", "Versioned RGB header is in the top image row");
                    Check(ReadUInt32(header, 16) == payload.Length, "RGB header preserves uncompressed payload length");
                    using (var sha = System.Security.Cryptography.SHA256.Create())
                        Check(header.Skip(32).Take(32).SequenceEqual(sha.ComputeHash(payload)), "RGB header stores the exact raw payload digest");
                    var coverRows = (int)ReadUInt32(header, 24);
                    Check(coverRows > 0 && coverRows <= 256, "Small source cover has a bounded preview area");
                    var imagePixels = image.GetPixels32();
                    var coverPixels = cover.GetPixels32();
                    var coverLeft = (image.width - cover.width) / 2;
                    Check(coverRows == cover.height &&
                        imagePixels[(image.height - 2) * image.width + coverLeft].Equals(coverPixels[2]) &&
                        imagePixels[(image.height - 2) * image.width + coverLeft + 1].Equals(coverPixels[3]) &&
                        imagePixels[(image.height - 3) * image.width + coverLeft].Equals(coverPixels[0]) &&
                        imagePixels[(image.height - 3) * image.width + coverLeft + 1].Equals(coverPixels[1]),
                        "Original small cover pixels and orientation survive exactly in the preview rows");
                    Check(image.height > coverRows + 1, "Compressed payload occupies rows after the header and cover");
                    var rewritten = image.EncodeToPNG();
                    Check(TextureHelper.GetExtraInfoByPNG(rewritten).SequenceEqual(payload),
                        "Lossless Unity PNG re-encoding preserves RGB payload and handles encoder filters");
                    if (payload.Length == large.Length)
                        Check(image.height > cover.height + 1, "Large incompressible payload spans multiple image rows");
                }
                finally { UnityEngine.Object.DestroyImmediate(image); }
            }
            Check(cover.GetPixels32().SequenceEqual(new[] { new Color32(20, 30, 40, 255), new Color32(50, 60, 70, 255),
                new Color32(80, 90, 100, 255), new Color32(110, 120, 130, 255) }), "Packaging leaves source cover pixels unchanged");
            TestLargePixelContainer();
        }
        finally { UnityEngine.Object.DestroyImmediate(cover); }
    }

    private static void TestLargePixelContainer()
    {
        // No Unity texture, GPU or private sample data is involved in these cases.
        var compressed = new byte[2 * 1024 * 1024];
        var compactPng = PngPixelPackage.Encode(null, compressed);
        Check(PngPixelPackage.Decode(compactPng).SequenceEqual(compressed), "Large compressible payload round trip without a cover");
        Check(compactPng.Length < compressed.Length / 64, "GZip removes Base64/JSON-style repetition before pixels are stored");
        var random = new byte[2 * 1024 * 1024 + 13];
        new System.Random(715).NextBytes(random);
        var randomPng = PngPixelPackage.Encode(null, random);
        Check(PngPixelPackage.Decode(randomPng).SequenceEqual(random), "Multi-megabyte incompressible payload round trip without a cover");
        Check(randomPng.Length < random.Length * 1.04 + 4096, "PNG image carrier adds only bounded overhead for incompressible package bytes");
    }

    private static byte[] ResizePackage(byte[] bytes)
    {
        var image = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        Texture2D resized = null;
        try
        {
            Check(image.LoadImage(bytes), "Load package for resized-image corruption test");
            resized = new Texture2D(Math.Max(1, image.width / 2), Math.Max(1, image.height / 2), TextureFormat.RGBA32, false, true);
            var source = image.GetPixels32();
            var pixels = new Color32[resized.width * resized.height];
            for (int y = 0; y < resized.height; y++)
                for (int x = 0; x < resized.width; x++)
                    pixels[y * resized.width + x] = source[(y * image.height / resized.height) * image.width + x * image.width / resized.width];
            resized.SetPixels32(pixels);
            resized.Apply();
            return resized.EncodeToPNG();
        }
        finally
        {
            if (resized != null) UnityEngine.Object.DestroyImmediate(resized);
            UnityEngine.Object.DestroyImmediate(image);
        }
    }

    // Re-encoding creates valid PNG chunk CRCs, so these tests specifically
    // exercise the in-pixel package header rather than PNG transport errors.
    private static byte[] MutateRgbHeader(byte[] bytes, int byteOffset, byte? replacement = null)
    {
        var image = new Texture2D(2, 2, TextureFormat.RGBA32, false, true);
        try
        {
            Check(image.LoadImage(bytes), "Load image for header corruption test");
            var pixels = image.GetPixels32();
            var index = (image.height - 1) * image.width + byteOffset / 3;
            var color = pixels[index];
            if (byteOffset % 3 == 0) color.r = replacement ?? (byte)(color.r ^ 1);
            else if (byteOffset % 3 == 1) color.g = replacement ?? (byte)(color.g ^ 1);
            else color.b = replacement ?? (byte)(color.b ^ 1);
            pixels[index] = color;
            image.SetPixels32(pixels);
            image.Apply();
            return image.EncodeToPNG();
        }
        finally { UnityEngine.Object.DestroyImmediate(image); }
    }

    private static byte[] ReadRgbHeader(Texture2D image)
    {
        var pixels = image.GetPixels32();
        var header = new byte[64];
        for (int i = 0; i < header.Length; i++)
        {
            var color = pixels[(image.height - 1) * image.width + i / 3];
            header[i] = i % 3 == 0 ? color.r : i % 3 == 1 ? color.g : color.b;
        }
        return header;
    }

    private static uint ReadUInt32(byte[] bytes, int offset)
    {
        return (uint)bytes[offset] << 24 | (uint)bytes[offset + 1] << 16 | (uint)bytes[offset + 2] << 8 | bytes[offset + 3];
    }

    private static System.Collections.Generic.List<string> GetChunkTypes(byte[] bytes)
    {
        var chunks = new System.Collections.Generic.List<string>();
        Check(bytes.Length >= 8 && bytes.Take(8).SequenceEqual(new byte[] { 137, 80, 78, 71, 13, 10, 26, 10 }), "PNG signature");
        var offset = 8;
        while (offset < bytes.Length)
        {
            Check(bytes.Length - offset >= 12, "Complete PNG chunk header and CRC");
            var size = (long)bytes[offset] << 24 | (long)bytes[offset + 1] << 16 | (long)bytes[offset + 2] << 8 | bytes[offset + 3];
            Check(size <= bytes.Length - offset - 12, "PNG chunk bounds");
            chunks.Add(Encoding.ASCII.GetString(bytes, offset + 4, 4));
            offset += (int)size + 12;
        }
        Check(offset == bytes.Length, "No package data appended outside PNG chunks");
        return chunks;
    }

    private static void Reject(GameSaveController ctrl, byte[] bytes, string message)
    {
        var rows = StoryForm.DataById.Count;
        var roots = Directory.GetDirectories(SaveAndLoad.perPath).OrderBy(p => p).ToArray();
        var rejected = false;
        try { ctrl.ImportPackage(bytes); }
        catch (Exception) { rejected = true; }
        Check(rejected, message);
        Check(StoryForm.DataById.Count == rows, message + ": no registry mutation");
        Check(roots.SequenceEqual(Directory.GetDirectories(SaveAndLoad.perPath).OrderBy(p => p)), message + ": no published/staged root");
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new Exception("Import regression: " + message);
        ++checks;
    }
}
