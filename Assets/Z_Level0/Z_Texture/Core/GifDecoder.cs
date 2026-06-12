using System;
using System.Collections.Generic;
using System.IO;
using UnityEngine;

namespace Z_Texture
{
    public class GifFrameData
    {
        public Texture2D texture;
        public float delaySeconds;
    }

    public static class GifDecoder
    {
        public static List<GifFrameData> Decode(byte[] data)
        {
            using (var ms = new MemoryStream(data))
            using (var reader = new BinaryReader(ms))
            {
                // GIF signature
                string signature = new string(reader.ReadChars(6));
                if (signature != "GIF87a" && signature != "GIF89a")
                {
                    Debug.LogError("Not a valid GIF file");
                    return null;
                }

                // Logical Screen Descriptor
                int width = reader.ReadUInt16();
                int height = reader.ReadUInt16();
                byte packed = reader.ReadByte();
                bool hasGct = (packed & 0x80) != 0;
                int gctSize = 2 << (packed & 0x07);
                byte bgColorIndex = reader.ReadByte();
                byte pixelAspectRatio = reader.ReadByte();

                // Global Color Table
                Color32[] gct = null;
                if (hasGct)
                {
                    gct = ReadColorTable(reader, gctSize);
                }

                var frames = new List<GifFrameData>();
                Color32[] prevFrame = null;
                int disposalMethod = 0;
                int transparentIndex = -1;

                while (true)
                {
                    byte blockType = reader.ReadByte();

                    if (blockType == 0x3B) // Trailer
                    {
                        break;
                    }
                    else if (blockType == 0x21) // Extension
                    {
                        byte label = reader.ReadByte();

                        if (label == 0xF9) // Graphics Control Extension
                        {
                            ReadGraphicsControlExtension(reader, out disposalMethod, out transparentIndex, out int delayCs);
                        }
                        else
                        {
                            // Skip other extensions
                            SkipSubBlocks(reader);
                        }
                    }
                    else if (blockType == 0x2C) // Image Descriptor
                    {
                        int left = reader.ReadUInt16();
                        int top = reader.ReadUInt16();
                        int imgWidth = reader.ReadUInt16();
                        int imgHeight = reader.ReadUInt16();
                        byte imgPacked = reader.ReadByte();
                        bool hasLct = (imgPacked & 0x80) != 0;
                        bool interlaced = (imgPacked & 0x40) != 0;
                        int lctSize = 2 << (imgPacked & 0x07);

                        Color32[] lct = null;
                        if (hasLct)
                        {
                            lct = ReadColorTable(reader, lctSize);
                        }

                        Color32[] activeTable = lct ?? gct;

                        // LZW Minimum Code Size
                        byte minCodeSize = reader.ReadByte();

                        // Image data sub-blocks
                        byte[] imageData = ReadSubBlocks(reader);

                        // Decode LZW
                        var pixels = DecodeLZW(imageData, minCodeSize, imgWidth * imgHeight);

                        // Build frame
                        var frameColors = new Color32[width * height];

                        // Copy previous frame based on disposal method
                        if (prevFrame != null && disposalMethod != 2)
                        {
                            Array.Copy(prevFrame, frameColors, prevFrame.Length);
                        }

                        // Write current frame pixels
                        int pixelIndex = 0;
                        for (int y = 0; y < imgHeight; y++)
                        {
                            for (int x = 0; x < imgWidth; x++)
                            {
                                int dstX = left + x;
                                int dstY = top + y;
                                if (dstX < width && dstY < height)
                                {
                                    int idx = pixels[pixelIndex];
                                    if (idx != transparentIndex && idx < activeTable.Length)
                                    {
                                        // GIF stores from top, Unity Texture2D from bottom
                                        frameColors[(height - 1 - dstY) * width + dstX] = activeTable[idx];
                                    }
                                }
                                pixelIndex++;
                            }
                        }

                        // Create texture
                        var tex = new Texture2D(width, height, TextureFormat.RGBA32, false);
                        tex.SetPixels32(frameColors);
                        tex.Apply();

                        // Get delay (re-read from the stored GCE data)
                        float delay = _lastDelaySeconds;
                        if (delay <= 0) delay = 0.1f; // default 100ms

                        frames.Add(new GifFrameData
                        {
                            texture = tex,
                            delaySeconds = delay
                        });

                        prevFrame = frameColors;
                        disposalMethod = 0;
                        transparentIndex = -1;
                    }
                    else
                    {
                        // Unknown block, try to skip
                        break;
                    }
                }

                return frames;
            }
        }

        private static float _lastDelaySeconds;

        static void ReadGraphicsControlExtension(BinaryReader reader, out int disposalMethod, out int transparentIndex, out int delayCs)
        {
            byte blockSize = reader.ReadByte(); // should be 4
            byte packed = reader.ReadByte();
            disposalMethod = (packed >> 2) & 0x07;
            bool hasTransparent = (packed & 0x01) != 0;
            delayCs = reader.ReadUInt16();
            byte transIdx = reader.ReadByte();
            transparentIndex = hasTransparent ? transIdx : -1;
            reader.ReadByte(); // block terminator

            _lastDelaySeconds = delayCs * 0.01f;
        }

        static Color32[] ReadColorTable(BinaryReader reader, int size)
        {
            var colors = new Color32[size];
            for (int i = 0; i < size; i++)
            {
                byte r = reader.ReadByte();
                byte g = reader.ReadByte();
                byte b = reader.ReadByte();
                colors[i] = new Color32(r, g, b, 255);
            }
            return colors;
        }

        static byte[] ReadSubBlocks(BinaryReader reader)
        {
            using (var ms = new MemoryStream())
            {
                while (true)
                {
                    byte blockSize = reader.ReadByte();
                    if (blockSize == 0) break;
                    ms.Write(reader.ReadBytes(blockSize), 0, blockSize);
                }
                return ms.ToArray();
            }
        }

        static void SkipSubBlocks(BinaryReader reader)
        {
            while (true)
            {
                byte blockSize = reader.ReadByte();
                if (blockSize == 0) break;
                reader.ReadBytes(blockSize);
            }
        }

        static int[] DecodeLZW(byte[] data, int minCodeSize, int pixelCount)
        {
            int clearCode = 1 << minCodeSize;
            int eoiCode = clearCode + 1;
            int codeSize = minCodeSize + 1;
            int nextCode = eoiCode + 1;
            int maxCodeSize = 1 << codeSize;

            // Initialize code table
            var codeTable = new List<byte[]>();
            for (int i = 0; i < clearCode; i++)
            {
                codeTable.Add(new byte[] { (byte)i });
            }
            codeTable.Add(null); // clear code
            codeTable.Add(null); // eoi code

            var output = new int[pixelCount];
            int outputIndex = 0;

            int bitBuffer = 0;
            int bitsInBuffer = 0;
            int dataPos = 0;

            int ReadCode()
            {
                while (bitsInBuffer < codeSize)
                {
                    if (dataPos >= data.Length) return eoiCode;
                    bitBuffer |= data[dataPos++] << bitsInBuffer;
                    bitsInBuffer += 8;
                }
                int code = bitBuffer & ((1 << codeSize) - 1);
                bitBuffer >>= codeSize;
                bitsInBuffer -= codeSize;
                return code;
            }

            // First code must be clear code
            int code = ReadCode();
            if (code != clearCode) return output;

            // Reset
            codeSize = minCodeSize + 1;
            nextCode = eoiCode + 1;
            codeTable.Clear();
            for (int i = 0; i < clearCode; i++)
            {
                codeTable.Add(new byte[] { (byte)i });
            }
            codeTable.Add(null); // clear
            codeTable.Add(null); // eoi

            int prevCode = -1;

            while (outputIndex < pixelCount)
            {
                code = ReadCode();

                if (code == clearCode)
                {
                    codeSize = minCodeSize + 1;
                    nextCode = eoiCode + 1;
                    codeTable.Clear();
                    for (int i = 0; i < clearCode; i++)
                    {
                        codeTable.Add(new byte[] { (byte)i });
                    }
                    codeTable.Add(null);
                    codeTable.Add(null);
                    prevCode = -1;
                    continue;
                }

                if (code == eoiCode) break;

                byte[] entry;
                if (code < codeTable.Count)
                {
                    entry = codeTable[code];
                }
                else if (code == nextCode && prevCode >= 0)
                {
                    // Special case: code not yet in table
                    byte[] prevEntry = codeTable[prevCode];
                    entry = new byte[prevEntry.Length + 1];
                    Array.Copy(prevEntry, entry, prevEntry.Length);
                    entry[prevEntry.Length] = prevEntry[0];
                }
                else
                {
                    Debug.LogError($"LZW decode error: code={code}, nextCode={nextCode}, tableSize={codeTable.Count}");
                    break;
                }

                // Output
                for (int i = 0; i < entry.Length && outputIndex < pixelCount; i++)
                {
                    output[outputIndex++] = entry[i];
                }

                // Add to table
                if (prevCode >= 0 && nextCode < 4096)
                {
                    byte[] prevEntry = codeTable[prevCode];
                    var newEntry = new byte[prevEntry.Length + 1];
                    Array.Copy(prevEntry, newEntry, prevEntry.Length);
                    newEntry[prevEntry.Length] = entry[0];
                    codeTable.Add(newEntry);
                    nextCode++;

                    if (nextCode >= (1 << codeSize) && codeSize < 12)
                    {
                        codeSize++;
                    }
                }

                prevCode = code;
            }

            return output;
        }
    }
}
