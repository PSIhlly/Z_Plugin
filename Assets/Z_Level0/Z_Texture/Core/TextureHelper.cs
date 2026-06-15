using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
namespace Z_Texture
{
    public static class TextureHelper
    {


        private static Texture2D _transparentTexture;
        public static Texture2D transparentTexture
        {
            get
            {
                if (_transparentTexture == null)
                {
                    _transparentTexture = new Texture2D(5, 5);
                    for (int y = 0; y < 5; y++)
                        for (int x = 0; x < 5; x++)
                        {
                            _transparentTexture.SetPixel(x, y, Color.clear);
                        }
                    _transparentTexture.Apply();
                }
                return _transparentTexture;
            }
        }
        private static Sprite _transparentSprite;
        public static Sprite transparentSprite
        {
            get
            {
                if (_transparentSprite == null)
                {
                    _transparentSprite = GetSpriteByTexture(transparentTexture);
                }
                return _transparentSprite;
            }
        }

        public static Texture GetTextureByPath(string path)
        {
            Texture res;
            if (File.Exists(path))
            {
                res = InternalGetTextureByPath(path);
            }
            else
            {
                res = InternalGetTextureByPathWithoutExtension(path);
            }
            return res;
        }
        public static async Task<Texture> GetTextureByPathAsync(string path)//todo
        {
            Texture res=null;
            res = GetTextureByPath(path);
            return res;
        }
        public static Texture GetTextureByByte(byte[] data)
        {
            // 使用 RGBA32 格式，禁用 mipmap，避免边缘问题
            Texture2D texture = new Texture2D(2, 2, TextureFormat.RGBA32, false);
            texture.LoadImage(data);
            // 设置点采样模式，避免双线性过滤导致的边缘模糊
            texture.filterMode = FilterMode.Point;
            // 设置钳制模式，避免边缘采样时从另一侧取像素
            texture.wrapMode = TextureWrapMode.Clamp;
            return texture;
        }
        public static Sprite GetSpriteByPath(string path)
        {
            var tex = GetTextureByPath(path);
            var s = Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
            return s;
        }
        public static Sprite GetSpriteByTexture(Texture tex)
        {
            return Sprite.Create((Texture2D)tex, new Rect(0, 0, tex.width, tex.height), new Vector2(0.5f, 0.5f));
        }

        public static byte[] GetTextureByte(Texture2D tex)
        {
            return DeCompress(tex).EncodeToPNG();
        }

        public static Texture2D DeCompress(Texture2D source)
        {
            RenderTexture renderTex = RenderTexture.GetTemporary(
      source.width,
      source.height,
      0,
      RenderTextureFormat.ARGB32,  // 使用 ARGB32 格式
      RenderTextureReadWrite.sRGB);  // 使用 sRGB 空间

            // 进行 Blit 操作，确保图像没有颜色偏差
            Graphics.Blit(source, renderTex);

            RenderTexture previous = RenderTexture.active;
            RenderTexture.active = renderTex;

            // 创建一个高质量的 Texture2D
            Texture2D readableText = new Texture2D(source.width, source.height, TextureFormat.RGBA32, false);
            readableText.filterMode = FilterMode.Bilinear; // 使用双线性过滤
            readableText.wrapMode = TextureWrapMode.Repeat; // 设置纹理的环绕模式

            // 读取像素
            readableText.ReadPixels(new Rect(0, 0, renderTex.width, renderTex.height), 0, 0);
            readableText.Apply();

            RenderTexture.active = previous;
            RenderTexture.ReleaseTemporary(renderTex);

            return readableText;
        }

        public static void DeleteTexture(string path)
        {
            File.Delete(path);
        }

        public static void RenameTexture(string path, string oldFileName, string newFileName)
        {
            var oldPath = Path.GetFullPath(path + "/" + oldFileName);
            var newPath = Path.GetFullPath(path + "/" + newFileName);
            File.Move(oldPath, newPath);
        }

        public static List<GifFrameData> GetGifFramesByPath(string path)
        {
            if (!File.Exists(path))
            {
                Debug.LogError("GIF file not found: " + path);
                return null;
            }
            return GifDecoder.Decode(File.ReadAllBytes(path));
        }

        public static List<GifFrameData> GetGifFramesByByte(byte[] data)
        {
            return GifDecoder.Decode(data);
        }

        public static List<Sprite> GetGifSpritesByPath(string path)
        {
            var frames = GetGifFramesByPath(path);
            if (frames == null) return null;
            var sprites = new List<Sprite>(frames.Count);
            foreach (var frame in frames)
            {
                sprites.Add(GetSpriteByTexture(frame.texture));
            }
            return sprites;
        }

        #region tool
        /// <summary>
        /// 计算Texture2D的SHA1哈希值（基于像素数据+纹理信息）
        /// </summary>
        public static string GetSHA1Hash(this Texture2D texture)
        {
            if (texture == null)
                return string.Empty;

            // 创建一个内存流，包含纹理的关键信息（尺寸、格式、像素）
            using (System.IO.MemoryStream ms = new System.IO.MemoryStream())
            using (System.IO.BinaryWriter writer = new System.IO.BinaryWriter(ms))
            {
                // 写入纹理基本信息（影响哈希值的关键参数）
                writer.Write(texture.width);
                writer.Write(texture.height);
                writer.Write(texture.format.ToString());
                writer.Write(texture.mipmapCount);

                // 写入像素数据
                byte[] pixelData = ToByteArray(texture.GetPixels32());
                byte[] byteData = new byte[pixelData.Length * 4];
                System.Buffer.BlockCopy(pixelData, 0, byteData, 0, byteData.Length);
                writer.Write(byteData);

                // 计算SHA1哈希
                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hashBytes = sha1.ComputeHash(ms.ToArray());
                    return ByteArrayToHexString(hashBytes);
                }
            }
        }

        public static byte[] GetPNGWithExtraInfo(Texture2D tex,byte[] info)
{
    byte[] pngData = GetTextureByte(tex);
    return InsertTextChunk(pngData, "ZEXTRA", Convert.ToBase64String(info));
}
        public static byte[] GetExtraInfoByPNG(byte[] texByte)
{
    string infoBase64 = ExtractTextChunk(texByte, "ZEXTRA");
    if (string.IsNullOrEmpty(infoBase64))
        return null;
    return Convert.FromBase64String(infoBase64);
}

        private static byte[] InsertTextChunk(byte[] pngData, string keyword, string text)
{
    using (MemoryStream ms = new MemoryStream())
    using (BinaryWriter writer = new BinaryWriter(ms))
    {
        int pngSignatureLength = 8;
        writer.Write(pngData, 0, pngSignatureLength);
        
        byte[] textChunk = CreateTextChunk(keyword, text);
        writer.Write(textChunk);
        
        int idatStart = FindChunk(pngData, "IDAT");
        if (idatStart == -1)
        {
            writer.Write(pngData, pngSignatureLength, pngData.Length - pngSignatureLength);
        }
        else
        {
            writer.Write(pngData, pngSignatureLength, idatStart - pngSignatureLength);
            writer.Write(pngData, idatStart, pngData.Length - idatStart);
        }
        
        return ms.ToArray();
    }
        }

        private static string ExtractTextChunk(byte[] pngData, string keyword)
{
    int offset = 8;
    while (offset < pngData.Length)
    {
        int chunkLength = BitConverter.ToInt32(pngData, offset);
        if (BitConverter.IsLittleEndian)
            chunkLength = ReverseBytes(chunkLength);
        
        offset += 4;
        string chunkType = Encoding.ASCII.GetString(pngData, offset, 4);
        offset += 4;
        
        if (chunkType == "tEXt")
        {
            byte[] chunkData = new byte[chunkLength];
            Array.Copy(pngData, offset, chunkData, 0, chunkLength);
            
            int nullIndex = Array.IndexOf(chunkData, (byte)0);
            if (nullIndex != -1)
            {
                string chunkKeyword = Encoding.ASCII.GetString(chunkData, 0, nullIndex);
                if (chunkKeyword == keyword)
                {
                    return Encoding.ASCII.GetString(chunkData, nullIndex + 1, chunkLength - nullIndex - 1);
                }
            }
        }
        
        offset += chunkLength + 4;
    }
    return null;
        }

        private static byte[] CreateTextChunk(string keyword, string text)
{
    byte[] keywordBytes = Encoding.ASCII.GetBytes(keyword);
    byte[] textBytes = Encoding.ASCII.GetBytes(text);
    byte[] data = new byte[keywordBytes.Length + 1 + textBytes.Length];
    
    Array.Copy(keywordBytes, 0, data, 0, keywordBytes.Length);
    data[keywordBytes.Length] = 0;
    Array.Copy(textBytes, 0, data, keywordBytes.Length + 1, textBytes.Length);
    
    using (MemoryStream ms = new MemoryStream())
    using (BinaryWriter writer = new BinaryWriter(ms))
    {
        int length = data.Length;
        if (BitConverter.IsLittleEndian)
            length = ReverseBytes(length);
        writer.Write(length);
        writer.Write(Encoding.ASCII.GetBytes("tEXt"));
        writer.Write(data);
        
        using (CRC32 crc = new CRC32())
        {
            byte[] chunkHeader = Encoding.ASCII.GetBytes("tEXt");
            writer.Write(ReverseBytes(BitConverter.ToInt32(crc.ComputeHash(chunkHeader.Concat(data).ToArray()))));
        }
        
        return ms.ToArray();
    }
        }

        private static int FindChunk(byte[] pngData, string chunkType)
{
    int offset = 8;
    while (offset < pngData.Length)
    {
        int chunkLength = BitConverter.ToInt32(pngData, offset);
        if (BitConverter.IsLittleEndian)
            chunkLength = ReverseBytes(chunkLength);
        
        offset += 4;
        string currentChunkType = Encoding.ASCII.GetString(pngData, offset, 4);
        if (currentChunkType == chunkType)
            return offset - 4;
        offset += 4 + chunkLength + 4;
    }
    return -1;
        }

        private static int ReverseBytes(int value)
{
    byte[] bytes = BitConverter.GetBytes(value);
    Array.Reverse(bytes);
    return BitConverter.ToInt32(bytes, 0);
        }

        private class CRC32 : HashAlgorithm
{
    private const uint Polynomial = 0xEDB88320;
    private uint[] table;
    private uint hash;

    public CRC32()
    {
        table = new uint[256];
        for (uint i = 0; i < 256; i++)
        {
            uint crc = i;
            for (int j = 0; j < 8; j++)
            {
                crc = (crc >> 1) ^ ((crc & 1) == 1 ? Polynomial : 0);
            }
            table[i] = crc;
        }
        Initialize();
    }

    public override void Initialize()
    {
        hash = 0xFFFFFFFF;
    }

    protected override byte[] HashFinal()
    {
        byte[] bytes = BitConverter.GetBytes(~hash);
        Array.Reverse(bytes);
        return bytes;
    }

    protected override void HashCore(byte[] array, int ibStart, int cbSize)
    {
        for (int i = ibStart; i < ibStart + cbSize; i++)
        {
            hash = (hash >> 8) ^ table[(hash ^ array[i]) & 0xFF];
        }
    }
        }

        #endregion



        #region util

        private static Texture InternalGetTextureByPath(string path)
        {
            // 尝试获取文件的字节数组
            if (File.Exists(path))
            {
                return GetTextureByByte(File.ReadAllBytes(path));
            }
            Debug.LogError("文件未找到：" + path);
            return Texture2D.whiteTexture;
        }
        private static Texture InternalGetTextureByPathWithoutExtension(string path)
        {

            // 尝试获取文件的字节数组
            foreach (var extension in new[] { ".png", ".jpg", ".jpeg", ".bmp", ".gif" }) // 添加你需要的后缀
            {
                string fileWithExtension = path + extension;

                if (File.Exists(fileWithExtension))
                {
                    return InternalGetTextureByPath(fileWithExtension);
                }
            }

            Debug.LogError("文件未找到：" + path);
            return Texture2D.whiteTexture;
        }

        private static byte[] ToByteArray(Color32[] colors)
        {
            if (colors == null || colors.Length == 0)
                return Array.Empty<byte>();

            // 每个Color32包含4个字节（r, g, b, a）
            int byteCount = colors.Length * 4;
            byte[] bytes = new byte[byteCount];

            // 直接复制内存块（高效转换）
            Buffer.BlockCopy(colors, 0, bytes, 0, byteCount);

            return bytes;
        }
        private static string ByteArrayToHexString(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return string.Empty;

            StringBuilder sb = new StringBuilder(bytes.Length * 2);
            foreach (byte b in bytes)
            {
                // 将每个字节转换为两位十六进制字符（小写）
                sb.AppendFormat("{0:x2}", b);
            }
            return sb.ToString();
        }


        

        #endregion
    }


}