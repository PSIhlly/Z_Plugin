using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using UnityEngine;
using unity.libwebp;
using unity.libwebp.Interop;

namespace Z_Texture
{
    /// <summary>Decodes complete, composited WebP canvases in display order.</summary>
    public static class WebPDecoder
    {
        private const long MaxDecodedBytes = 256L * 1024 * 1024;

        public static Texture2D DecodeFirstFrame(byte[] data)
        {
            var frames = DecodeInternal(data, true);
            return frames != null && frames.Count > 0 ? frames[0].texture : null;
        }

        public static List<AnimatedFrameData> Decode(byte[] data)
        {
            return DecodeInternal(data, false);
        }

        private static unsafe List<AnimatedFrameData> DecodeInternal(byte[] data, bool firstFrameOnly)
        {
            if (!TextureHelper.IsWebP(data))
                return null;

            var frames = new List<AnimatedFrameData>();
            try
            {
                fixed (byte* source = data)
                {
                    var webpData = new WebPData { bytes = source, size = (UIntPtr)data.Length };
                    var options = new WebPAnimDecoderOptions();
                    if (NativeLibwebpdemux.WebPAnimDecoderOptionsInit(&options) == 0)
                        throw new InvalidOperationException("WebP animation decoder ABI is incompatible.");
                    options.color_mode = WEBP_CSP_MODE.MODE_RGBA;
                    options.use_threads = 1;

                    WebPAnimDecoder* decoder = NativeLibwebpdemux.WebPAnimDecoderNew(&webpData, &options);
                    if (decoder == null)
                        throw new InvalidOperationException("Unable to open WebP image.");

                    try
                    {
                        var info = new WebPAnimInfo();
                        if (NativeLibwebpdemux.WebPAnimDecoderGetInfo(decoder, &info) == 0 ||
                            info.canvas_width == 0 || info.canvas_height == 0 || info.frame_count == 0)
                            throw new InvalidOperationException("Invalid WebP canvas or frame count.");

                        int width = checked((int)info.canvas_width);
                        int height = checked((int)info.canvas_height);
                        int frameCount = firstFrameOnly ? 1 : checked((int)info.frame_count);
                        long frameBytes = (long)width * height * 4;
                        if (frameBytes * frameCount > MaxDecodedBytes)
                            throw new InvalidOperationException("WebP animation exceeds the decoded image size limit.");

                        int rowBytes = checked(width * 4);
                        int byteCount = checked(rowBytes * height);
                        int previousTimestamp = 0;
                        for (int i = 0; i < frameCount; i++)
                        {
                            byte* canvas = null;
                            int timestamp = 0;
                            if (NativeLibwebpdemux.WebPAnimDecoderGetNext(decoder, &canvas, &timestamp) == 0 || canvas == null)
                                throw new InvalidOperationException($"Unable to decode WebP frame {i}.");

                            var topDown = new byte[byteCount];
                            var bottomUp = new byte[byteCount];
                            Marshal.Copy((IntPtr)canvas, topDown, 0, byteCount);
                            for (int y = 0; y < height; y++)
                                Buffer.BlockCopy(topDown, y * rowBytes, bottomUp, (height - 1 - y) * rowBytes, rowBytes);

                            Texture2D texture = null;
                            try
                            {
                                texture = new Texture2D(width, height, TextureFormat.RGBA32, false)
                                {
                                    filterMode = FilterMode.Point,
                                    wrapMode = TextureWrapMode.Clamp
                                };
                                texture.LoadRawTextureData(bottomUp);
                                texture.Apply(false, false);

                                float delay = (timestamp - previousTimestamp) * 0.001f;
                                frames.Add(new AnimatedFrameData
                                {
                                    texture = texture,
                                    delaySeconds = delay > 0 ? delay : 0.1f
                                });
                            }
                            catch
                            {
                                if (texture != null)
                                    UnityEngine.Object.Destroy(texture);
                                throw;
                            }
                            previousTimestamp = timestamp;
                        }
                    }
                    finally
                    {
                        NativeLibwebpdemux.WebPAnimDecoderDelete(decoder);
                    }
                }
                return frames;
            }
            catch (Exception exception)
            {
                foreach (var frame in frames)
                    if (frame.texture != null)
                        UnityEngine.Object.Destroy(frame.texture);
                Debug.LogError($"WebP decode failed: {exception.Message}");
                return null;
            }
        }
    }
}
