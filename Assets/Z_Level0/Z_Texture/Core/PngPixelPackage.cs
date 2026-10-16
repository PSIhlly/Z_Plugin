using System;
using System.Collections.Generic;
using System.IO;
using System.IO.Compression;
using System.Security.Cryptography;
using UnityEngine;
using CompressionLevel = System.IO.Compression.CompressionLevel;

namespace Z_Texture
{
    /// <summary>
    /// Stores exact package bytes in PNG RGB samples, not image metadata. A small
    /// cover is optional; the remaining pixels contain a GZip-compressed payload.
    /// Rows are streamed so a large package never becomes a Unity/GPU texture.
    /// </summary>
    public static class PngPixelPackage
    {
        public const int MaxDimension = 8192;
        public const int MaxPayloadBytes = 512 * 1024 * 1024;
        private const int HeaderBytes = 64;
        private const int CoverLimit = 256;
        private const int IdatChunkBytes = 128 * 1024;
        private const int MaxPackedBytes = MaxDimension * (MaxDimension - 1) * 3;
        private const int MaxPngBytes = MaxDimension * MaxDimension * 4 + 8 * 1024 * 1024;
        private static readonly byte[] Signature = { 137, 80, 78, 71, 13, 10, 26, 10 };
        private static readonly byte[] Magic = { 90, 80, 75, 71, 80, 78, 71, 49 }; // ZPKGPNG1
        private static readonly byte[] Ihdr = { 73, 72, 68, 82 };
        private static readonly byte[] Idat = { 73, 68, 65, 84 };
        private static readonly byte[] Iend = { 73, 69, 78, 68 };
        private static readonly uint[] CrcTable = CreateCrcTable();

        /// <summary>
        /// Immutable CPU-only cover pixels captured on the Unity main thread.
        /// A snapshot can be reused safely by background package encoders.
        /// </summary>
        public sealed class CoverSnapshot
        {
            private readonly byte[] rgb;
            public int Width { get; }
            public int Rows { get; }
            internal int RgbLength => rgb.Length;

            internal CoverSnapshot(byte[] rgb, int width, int rows)
            {
                this.rgb = (byte[])rgb.Clone();
                Width = width;
                Rows = rows;
            }

            internal void CopyRowTo(int row, byte[] destination, int offset)
            {
                Buffer.BlockCopy(rgb, row * Width * 3, destination, offset, Width * 3);
            }
        }

        /// <summary>
        /// Captures a small cover preview on the Unity main thread. The returned
        /// snapshot contains no Unity objects; null covers produce no cover rows.
        /// </summary>
        public static CoverSnapshot CaptureCover(Texture2D cover)
        {
            int width, rows;
            byte[] rgb = ReadCover(cover, out width, out rows);
            return new CoverSnapshot(rgb, width, rows);
        }

        /// <summary>Produces an ordinary RGB8, non-interlaced PNG with no tEXt chunk.</summary>
        public static byte[] Encode(Texture2D cover, byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (payload.Length > MaxPayloadBytes)
                throw Invalid("Package payload exceeds the 512 MiB safety limit.");
            return EncodeWithCover(CaptureCover(cover), payload);
        }

        /// <summary>
        /// Produces the same package PNG using only managed data, suitable for
        /// Task.Run. A null snapshot is equivalent to a null texture cover.
        /// </summary>
        public static byte[] EncodeWithCover(CoverSnapshot cover, byte[] payload)
        {
            if (payload == null) throw new ArgumentNullException(nameof(payload));
            if (payload.Length > MaxPayloadBytes)
                throw Invalid("Package payload exceeds the 512 MiB safety limit.");

            using (var packed = new MemoryStream())
            {
                using (var gzip = new GZipStream(packed, CompressionLevel.Optimal, true))
                    gzip.Write(payload, 0, payload.Length);
                // Some runtimes do not initialize GZip on a zero-byte Write.
                if (packed.Length == 0)
                {
                    byte[] emptyGzip = { 31, 139, 8, 0, 0, 0, 0, 0, 0, 255, 3, 0, 0, 0, 0, 0, 0, 0, 0, 0 };
                    packed.Write(emptyGzip, 0, emptyGzip.Length);
                }
                if (packed.Length > MaxPackedBytes)
                    throw Invalid("Compressed package is too large for an 8192 x 8192 PNG.");

                int coverWidth = cover?.Width ?? 0;
                int coverRows = cover?.Rows ?? 0;
                int packedLength = checked((int)packed.Length);
                int width = Math.Max(CoverLimit, (int)Math.Ceiling(Math.Sqrt(packedLength / 3.0)));
                width = Math.Min(width, MaxDimension);
                while (1L + coverRows + (packedLength + (long)width * 3 - 1) / (width * 3) > MaxDimension)
                {
                    if (width == MaxDimension)
                        throw Invalid("Compressed package and cover exceed the PNG dimension limit.");
                    width = Math.Min(MaxDimension, width + 64);
                }
                int rowBytes = width * 3;
                int dataRows = Math.Max(1, (packedLength + rowBytes - 1) / rowBytes);
                int height = 1 + coverRows + dataRows;
                var header = new byte[HeaderBytes];
                Buffer.BlockCopy(Magic, 0, header, 0, Magic.Length);
                WriteUInt32(header, 8, 1); // Protocol version.
                WriteUInt32(header, 12, 1); // GZip payload.
                WriteUInt32(header, 16, (uint)payload.Length);
                WriteUInt32(header, 20, (uint)packedLength);
                WriteUInt32(header, 24, (uint)coverRows);
                using (var sha = SHA256.Create())
                    Buffer.BlockCopy(sha.ComputeHash(payload), 0, header, 32, 32);

                // Reserve PNG/deflate overhead up front: going only a few KiB
                // past packedLength must not double a 100+ MiB output buffer.
                int outputCapacity = checked(packedLength + (cover?.RgbLength ?? 0) + packedLength / 1024 + 65536);
                using (var png = new MemoryStream(outputCapacity))
                {
                    png.Write(Signature, 0, Signature.Length);
                    var imageHeader = new byte[13];
                    WriteUInt32(imageHeader, 0, (uint)width);
                    WriteUInt32(imageHeader, 4, (uint)height);
                    imageHeader[8] = 8;
                    imageHeader[9] = 2; // RGB, no alpha or palette.
                    WriteChunk(png, Ihdr, imageHeader, 0, imageHeader.Length);
                    var adler = new Adler32();
                    var row = new byte[rowBytes + 1]; // Filter byte followed by RGB.
                    byte[] packedBytes = packed.GetBuffer();
                    using (var chunks = new IdatWriter(png))
                    {
                        chunks.WriteByte(0x78);
                        chunks.WriteByte(0x01); // zlib, deflate, no preset dictionary.
                        // Optimal can emit stored blocks for already-compressed
                        // bytes. Fastest on some runtimes adds ~5% unnecessary
                        // Huffman overhead to random GZip data.
                        using (var deflate = new DeflateStream(chunks, CompressionLevel.Optimal, true))
                        {
                            int dataOffset = 0;
                            for (int y = 0; y < height; y++)
                            {
                                Array.Clear(row, 0, row.Length);
                                if (y == 0)
                                    Buffer.BlockCopy(header, 0, row, 1, header.Length);
                                else if (y <= coverRows)
                                    cover.CopyRowTo(y - 1, row, 1 + (width - coverWidth) / 2 * 3);
                                else
                                {
                                    int count = Math.Min(rowBytes, packedLength - dataOffset);
                                    if (count > 0)
                                        Buffer.BlockCopy(packedBytes, dataOffset, row, 1, count);
                                    dataOffset += count;
                                }
                                adler.Update(row, 0, row.Length);
                                deflate.Write(row, 0, row.Length);
                            }
                        }
                        var checksum = new byte[4];
                        WriteUInt32(checksum, 0, adler.Value);
                        chunks.Write(checksum, 0, checksum.Length);
                    }
                    WriteChunk(png, Iend, Array.Empty<byte>(), 0, 0);
                    return png.ToArray();
                }
            }
        }

        /// <summary>
        /// Returns null for a PNG without the pixel-package magic. Recognized but
        /// damaged packages throw InvalidDataException before callers write files.
        /// RGB8/RGBA8 PNG filters 0..4 are supported for lossless re-encoding.
        /// </summary>
        public static byte[] Decode(byte[] png)
        {
            if (png == null) throw new ArgumentNullException(nameof(png));
            PngInfo info = ParsePng(png);
            if (info.BitDepth != 8 || (info.ColorType != 2 && info.ColorType != 6) || info.Interlace != 0)
                return null;
            int channels = info.ColorType == 2 ? 3 : 4;
            if (info.Width * 3 < HeaderBytes) return null;
            using (var zlib = new ChunkReader(png, info.Data))
            {
                if (zlib.Length < 6) throw Invalid("Truncated PNG zlib stream.");
                int cmf = zlib.ReadByte();
                int flg = zlib.ReadByte();
                if (cmf < 0 || flg < 0 || (cmf & 15) != 8 || (cmf >> 4) > 7 ||
                    ((cmf << 8) + flg) % 31 != 0 || (flg & 32) != 0)
                    throw Invalid("Unsupported or damaged PNG zlib header.");
                zlib.Position = zlib.Length - 4;
                uint expectedAdler = ReadUInt32(zlib);
                zlib.Position = 2;
                using (var compressed = new WindowReader(zlib, zlib.Length - 6))
                using (var inflate = new DeflateStream(compressed, CompressionMode.Decompress, true))
                {
                    var row = new byte[info.Width * channels];
                    var previous = new byte[row.Length];
                    var adler = new Adler32();
                    ReadRow(inflate, row, previous, channels, adler);
                    var header = new byte[HeaderBytes];
                    CopyRgb(row, channels, header, 0, HeaderBytes);
                    for (int i = 0; i < Magic.Length; i++)
                        if (header[i] != Magic[i]) return null;
                    if (ReadUInt32(header, 8) != 1 || ReadUInt32(header, 12) != 1 ||
                        ReadUInt32(header, 28) != 0)
                        throw Invalid("Unsupported pixel package header/version.");
                    uint rawLength = ReadUInt32(header, 16);
                    uint packedLength = ReadUInt32(header, 20);
                    uint coverRows = ReadUInt32(header, 24);
                    long capacity = (info.Height - 1L - coverRows) * info.Width * 3;
                    if (rawLength > MaxPayloadBytes || packedLength < 18 || packedLength > MaxPackedBytes ||
                        coverRows > CoverLimit || capacity < packedLength)
                        throw Invalid("Pixel package lengths exceed the image or safety limits.");
                    // GZip payloads produced by Encode are already compressed.
                    // Their RGB samples must not expand a tiny IDAT into a huge
                    // packed allocation; allow a 16 MiB floor for small, very
                    // compressible packages and 64x beyond that.
                    if (packedLength > Math.Max(16L * 1024 * 1024, png.Length * 64L))
                        throw Invalid("Pixel package compressed-image expansion exceeds the safety limit.");
                    var packed = new byte[(int)packedLength];
                    int copied = 0;
                    for (int y = 1; y < info.Height; y++)
                    {
                        ReadRow(inflate, row, previous, channels, adler);
                        if (y > coverRows && copied < packed.Length)
                        {
                            int count = Math.Min(info.Width * 3, packed.Length - copied);
                            CopyRgb(row, channels, packed, copied, count);
                            copied += count;
                        }
                    }
                    if (inflate.ReadByte() != -1) throw Invalid("PNG contains excess decompressed rows.");
                    if (adler.Value != expectedAdler) throw Invalid("PNG zlib checksum does not match.");
                    if (packed[0] != 0x1f || packed[1] != 0x8b || packed[2] != 8 || (packed[3] & 0xe0) != 0 ||
                        ReadLittleUInt32(packed, packed.Length - 4) != rawLength)
                        throw Invalid("Pixel package GZip length/header does not match.");
                    // Never allocate rawLength bytes based on an untrusted
                    // header/ISIZE alone. Validate bounded expansion and SHA in
                    // a small buffer first; malformed tiny images cannot force
                    // a 512 MiB allocation on Android.
                    byte[] actualHash;
                    using (var sha = SHA256.Create())
                    using (var bytes = new MemoryStream(packed, false))
                    using (var gzip = new GZipStream(bytes, CompressionMode.Decompress))
                    {
                        var buffer = new byte[65536];
                        long expanded = 0;
                        uint rawCrcState = 0xffffffffu;
                        int read;
                        while ((read = gzip.Read(buffer, 0, (int)Math.Min(buffer.Length, rawLength - expanded + 1))) > 0)
                        {
                            expanded += read;
                            if (expanded > rawLength) throw Invalid("Pixel package expands beyond its declared length.");
                            sha.TransformBlock(buffer, 0, read, buffer, 0);
                            rawCrcState = Crc(buffer, 0, read, rawCrcState) ^ 0xffffffffu;
                        }
                        if (expanded != rawLength) throw Invalid("Pixel package expands to a different length.");
                        if ((rawCrcState ^ 0xffffffffu) != ReadLittleUInt32(packed, packed.Length - 8))
                            throw Invalid("Pixel package GZip checksum does not match.");
                        sha.TransformFinalBlock(Array.Empty<byte>(), 0, 0);
                        actualHash = sha.Hash;
                    }
                    int mismatch = 0;
                    for (int i = 0; i < actualHash.Length; i++)
                        mismatch |= actualHash[i] ^ header[32 + i];
                    if (mismatch != 0) throw Invalid("Pixel package payload checksum does not match.");
                    var payload = new byte[(int)rawLength];
                    using (var bytes = new MemoryStream(packed, false))
                    using (var gzip = new GZipStream(bytes, CompressionMode.Decompress))
                        ReadExactly(gzip, payload, 0, payload.Length);
                    return payload;
                }
            }
        }

        private static byte[] ReadCover(Texture2D cover, out int width, out int height)
        {
            width = height = 0;
            if (cover == null || cover.width < 1 || cover.height < 1) return Array.Empty<byte>();
            float scale = Math.Min(1f, CoverLimit / (float)Math.Max(cover.width, cover.height));
            width = Math.Max(1, (int)Math.Round(cover.width * scale));
            height = Math.Max(1, (int)Math.Round(cover.height * scale));
            var rgb = new byte[width * height * 3];
            Texture2D readable = null;
            RenderTexture target = null;
            RenderTexture prior = RenderTexture.active;
            try
            {
                Texture2D source = cover;
                if (!cover.isReadable)
                {
                    target = RenderTexture.GetTemporary(width, height, 0, RenderTextureFormat.ARGB32,
                        RenderTextureReadWrite.sRGB);
                    Graphics.Blit(cover, target);
                    RenderTexture.active = target;
                    readable = new Texture2D(width, height, TextureFormat.RGBA32, false);
                    readable.ReadPixels(new Rect(0, 0, width, height), 0, 0);
                    readable.Apply();
                    source = readable;
                }
                for (int y = 0; y < height; y++)
                {
                    for (int x = 0; x < width; x++)
                    {
                        // Integer sampling keeps an already-small cover's pixels
                        // unchanged and never allocates the full source image.
                        int sourceX = Math.Min(source.width - 1, (int)((x + .5f) * source.width / width));
                        int sourceY = source.height - 1 - Math.Min(source.height - 1,
                            (int)((y + .5f) * source.height / height));
                        Color32 color = source.GetPixel(sourceX, sourceY);
                        int offset = (y * width + x) * 3;
                        // RGB PNG flattens the thumbnail against white, not black.
                        rgb[offset] = Flatten(color.r, color.a);
                        rgb[offset + 1] = Flatten(color.g, color.a);
                        rgb[offset + 2] = Flatten(color.b, color.a);
                    }
                }
                return rgb;
            }
            finally
            {
                RenderTexture.active = prior;
                if (target != null) RenderTexture.ReleaseTemporary(target);
                if (readable != null)
                {
                    if (Application.isPlaying) UnityEngine.Object.Destroy(readable);
                    else UnityEngine.Object.DestroyImmediate(readable);
                }
            }
        }

        private static byte Flatten(byte color, byte alpha)
        {
            return (byte)((color * alpha + 255 * (255 - alpha) + 127) / 255);
        }

        private static void CopyRgb(byte[] row, int channels, byte[] destination, int offset, int count)
        {
            if (channels == 3)
                Buffer.BlockCopy(row, 0, destination, offset, count);
            else
                for (int i = 0; i < count; i++)
                    destination[offset + i] = row[i / 3 * 4 + i % 3];
        }

        private static void ReadRow(Stream stream, byte[] row, byte[] previous, int channels, Adler32 adler)
        {
            int filter = stream.ReadByte();
            if (filter < 0) throw Invalid("Truncated PNG scanline.");
            adler.Update((byte)filter);
            ReadExactly(stream, row, 0, row.Length);
            adler.Update(row, 0, row.Length);
            if (filter > 4) throw Invalid("Unsupported PNG scanline filter.");
            if (filter == 0)
            {
                Buffer.BlockCopy(row, 0, previous, 0, row.Length);
                return;
            }
            for (int i = 0; i < row.Length; i++)
            {
                int left = i >= channels ? row[i - channels] : 0;
                int up = previous[i];
                int upperLeft = i >= channels ? previous[i - channels] : 0;
                int prediction;
                switch (filter)
                {
                    case 1: prediction = left; break;
                    case 2: prediction = up; break;
                    case 3: prediction = (left + up) / 2; break;
                    case 4: prediction = Paeth(left, up, upperLeft); break;
                    default: prediction = 0; break;
                }
                row[i] = unchecked((byte)(row[i] + prediction));
            }
            Buffer.BlockCopy(row, 0, previous, 0, row.Length);
        }

        private static int Paeth(int left, int up, int upperLeft)
        {
            int p = left + up - upperLeft;
            int a = Math.Abs(p - left), b = Math.Abs(p - up), c = Math.Abs(p - upperLeft);
            return a <= b && a <= c ? left : b <= c ? up : upperLeft;
        }

        private sealed class PngInfo
        {
            public int Width, Height, BitDepth, ColorType, Interlace;
            public readonly List<Segment> Data = new List<Segment>();
        }

        private struct Segment
        {
            public int Offset, Count;
            public Segment(int offset, int count) { Offset = offset; Count = count; }
        }

        private static PngInfo ParsePng(byte[] png)
        {
            if (png.Length < 45 || png.Length > MaxPngBytes) throw Invalid("PNG file size is invalid.");
            for (int i = 0; i < Signature.Length; i++)
                if (png[i] != Signature[i]) throw Invalid("The file is not a PNG image.");
            var info = new PngInfo();
            int offset = Signature.Length;
            bool header = false, data = false, dataEnded = false, paletteSeen = false;
            int chunkCount = 0;
            while (offset <= png.Length - 12)
            {
                if (++chunkCount > 100000) throw Invalid("PNG has too many chunks.");
                uint length = ReadUInt32(png, offset);
                if (length > int.MaxValue || length > png.Length - (long)offset - 12)
                    throw Invalid("Truncated or oversized PNG chunk.");
                int count = (int)length;
                int type = offset + 4, content = offset + 8;
                for (int i = 0; i < 4; i++)
                {
                    byte letter = png[type + i];
                    if ((letter < 'A' || letter > 'Z') && (letter < 'a' || letter > 'z'))
                        throw Invalid("PNG chunk type is invalid.");
                }
                if ((png[type + 2] & 32) != 0) throw Invalid("PNG chunk reserved bit is invalid.");
                uint crc = Crc(png, type, count + 4);
                if (crc != ReadUInt32(png, content + count)) throw Invalid("PNG chunk CRC does not match.");
                bool isHeader = IsType(png, type, Ihdr), isData = IsType(png, type, Idat);
                bool isEnd = IsType(png, type, Iend);
                if (!header && !isHeader) throw Invalid("PNG IHDR must be the first chunk.");
                if (isHeader)
                {
                    if (header || count != 13) throw Invalid("Invalid or duplicate PNG IHDR.");
                    uint width = ReadUInt32(png, content), height = ReadUInt32(png, content + 4);
                    if (width < 1 || height < 1 || width > MaxDimension || height > MaxDimension)
                        throw Invalid("PNG dimensions exceed the 8192-pixel safety limit.");
                    info.Width = (int)width;
                    info.Height = (int)height;
                    info.BitDepth = png[content + 8];
                    info.ColorType = png[content + 9];
                    info.Interlace = png[content + 12];
                    if (png[content + 10] != 0 || png[content + 11] != 0 || info.Interlace > 1)
                        throw Invalid("Unsupported PNG compression/filter method.");
                    header = true;
                }
                else if (isData)
                {
                    if (dataEnded) throw Invalid("PNG IDAT chunks must be consecutive.");
                    data = true;
                    info.Data.Add(new Segment(content, count));
                }
                else if (isEnd)
                {
                    if (!data || count != 0 || content + 4 != png.Length)
                        throw Invalid("Invalid, missing, or nonterminal PNG IEND.");
                    return info;
                }
                else
                {
                    if (data) dataEnded = true;
                    // PLTE is optional for true-color PNGs; all other unknown
                    // critical chunks must be rejected, never silently decoded.
                    bool palette = png[type] == 'P' && png[type + 1] == 'L' &&
                        png[type + 2] == 'T' && png[type + 3] == 'E';
                    if ((png[type] & 32) == 0 && !palette) throw Invalid("Unsupported critical PNG chunk.");
                    if (palette)
                    {
                        if (paletteSeen || data || count == 0 || count > 768 || count % 3 != 0)
                            throw Invalid("Invalid PNG palette chunk.");
                        paletteSeen = true;
                    }
                }
                offset = content + count + 4;
            }
            throw Invalid("PNG IEND is missing or truncated.");
        }

        private static bool IsType(byte[] bytes, int offset, byte[] type)
        {
            for (int i = 0; i < 4; i++) if (bytes[offset + i] != type[i]) return false;
            return true;
        }

        private static void ReadExactly(Stream input, byte[] bytes, int offset, int count)
        {
            while (count > 0)
            {
                int got = input.Read(bytes, offset, count);
                if (got <= 0) throw Invalid("Compressed package/image data is truncated.");
                offset += got;
                count -= got;
            }
        }

        private static uint ReadUInt32(byte[] bytes, int offset)
        {
            return (uint)bytes[offset] << 24 | (uint)bytes[offset + 1] << 16 |
                (uint)bytes[offset + 2] << 8 | bytes[offset + 3];
        }

        private static uint ReadUInt32(Stream input)
        {
            var bytes = new byte[4];
            ReadExactly(input, bytes, 0, bytes.Length);
            return ReadUInt32(bytes, 0);
        }

        private static uint ReadLittleUInt32(byte[] bytes, int offset)
        {
            return (uint)bytes[offset + 3] << 24 | (uint)bytes[offset + 2] << 16 |
                (uint)bytes[offset + 1] << 8 | bytes[offset];
        }

        private static void WriteUInt32(byte[] bytes, int offset, uint value)
        {
            bytes[offset] = (byte)(value >> 24);
            bytes[offset + 1] = (byte)(value >> 16);
            bytes[offset + 2] = (byte)(value >> 8);
            bytes[offset + 3] = (byte)value;
        }

        private static uint[] CreateCrcTable()
        {
            var table = new uint[256];
            for (uint i = 0; i < table.Length; i++)
            {
                uint value = i;
                for (int bit = 0; bit < 8; bit++) value = (value >> 1) ^ ((value & 1) != 0 ? 0xedb88320u : 0);
                table[i] = value;
            }
            return table;
        }

        private static uint Crc(byte[] bytes, int offset, int count, uint state = 0xffffffffu)
        {
            for (int end = offset + count; offset < end; offset++)
                state = (state >> 8) ^ CrcTable[(state ^ bytes[offset]) & 255];
            return state ^ 0xffffffffu;
        }

        private static void WriteChunk(Stream png, byte[] type, byte[] bytes, int offset, int count)
        {
            var length = new byte[4];
            WriteUInt32(length, 0, (uint)count);
            png.Write(length, 0, 4);
            png.Write(type, 0, 4);
            png.Write(bytes, offset, count);
            uint typeCrc = Crc(type, 0, 4);
            WriteUInt32(length, 0, Crc(bytes, offset, count, typeCrc ^ 0xffffffffu));
            png.Write(length, 0, 4);
        }

        private sealed class Adler32
        {
            private uint a = 1, b;
            public uint Value => (b << 16) | a;
            public void Update(byte value)
            {
                a = (a + value) % 65521;
                b = (b + a) % 65521;
            }
            public void Update(byte[] bytes, int offset, int count)
            {
                // 5552-byte batches avoid overflow and per-byte modulo work.
                while (count > 0)
                {
                    int end = offset + Math.Min(count, 5552);
                    count -= end - offset;
                    while (offset < end) { a += bytes[offset++]; b += a; }
                    a %= 65521;
                    b %= 65521;
                }
            }
        }

        private sealed class IdatWriter : Stream
        {
            private readonly Stream png;
            private readonly byte[] buffer = new byte[IdatChunkBytes];
            private int used;
            public IdatWriter(Stream png) { this.png = png; }
            public override bool CanRead => false;
            public override bool CanSeek => false;
            public override bool CanWrite => true;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override void Write(byte[] bytes, int offset, int count)
            {
                while (count > 0)
                {
                    int copy = Math.Min(buffer.Length - used, count);
                    Buffer.BlockCopy(bytes, offset, buffer, used, copy);
                    used += copy;
                    offset += copy;
                    count -= copy;
                    if (used == buffer.Length) Flush();
                }
            }
            public override void Flush()
            {
                if (used == 0) return;
                WriteChunk(png, Idat, buffer, 0, used);
                used = 0;
            }
            protected override void Dispose(bool disposing) { if (disposing) Flush(); base.Dispose(disposing); }
            public override int Read(byte[] bytes, int offset, int count) => throw new NotSupportedException();
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
        }

        // Reads concatenated IDAT contents directly from the original PNG bytes;
        // there is no second full compressed image allocation.
        private sealed class ChunkReader : Stream
        {
            private readonly byte[] bytes;
            private readonly List<Segment> segments;
            private readonly long length;
            private long position;
            private int segmentIndex, segmentOffset;
            public ChunkReader(byte[] bytes, List<Segment> segments)
            {
                this.bytes = bytes;
                this.segments = segments;
                foreach (var segment in segments) length += segment.Count;
            }
            public override bool CanRead => true;
            public override bool CanSeek => true;
            public override bool CanWrite => false;
            public override long Length => length;
            public override long Position
            {
                get => position;
                set
                {
                    if (value < 0 || value > length) throw new ArgumentOutOfRangeException(nameof(value));
                    position = value;
                    segmentIndex = 0;
                    long remaining = value;
                    while (segmentIndex < segments.Count && remaining >= segments[segmentIndex].Count)
                        remaining -= segments[segmentIndex++].Count;
                    segmentOffset = (int)remaining;
                }
            }
            public override int Read(byte[] buffer, int offset, int count)
            {
                int total = 0;
                while (count > 0 && segmentIndex < segments.Count)
                {
                    var segment = segments[segmentIndex];
                    int copy = Math.Min(count, segment.Count - segmentOffset);
                    Buffer.BlockCopy(bytes, segment.Offset + segmentOffset, buffer, offset, copy);
                    offset += copy;
                    count -= copy;
                    total += copy;
                    position += copy;
                    segmentOffset += copy;
                    if (segmentOffset == segment.Count) { segmentIndex++; segmentOffset = 0; }
                }
                return total;
            }
            public override long Seek(long offset, SeekOrigin origin)
            {
                Position = origin == SeekOrigin.Begin ? offset : origin == SeekOrigin.Current ? Position + offset : Length + offset;
                return Position;
            }
            public override void Flush() { }
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        private sealed class WindowReader : Stream
        {
            private readonly Stream input;
            private long remaining;
            public WindowReader(Stream input, long count) { this.input = input; remaining = count; }
            public override bool CanRead => true;
            public override bool CanSeek => false;
            public override bool CanWrite => false;
            public override long Length => throw new NotSupportedException();
            public override long Position { get => throw new NotSupportedException(); set => throw new NotSupportedException(); }
            public override int Read(byte[] buffer, int offset, int count)
            {
                int read = input.Read(buffer, offset, (int)Math.Min(remaining, count));
                remaining -= read;
                return read;
            }
            public override void Flush() { }
            public override long Seek(long offset, SeekOrigin origin) => throw new NotSupportedException();
            public override void SetLength(long value) => throw new NotSupportedException();
            public override void Write(byte[] buffer, int offset, int count) => throw new NotSupportedException();
        }

        private static InvalidDataException Invalid(string message) => new InvalidDataException(message);
    }
}
