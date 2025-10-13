using System.IO.Compression;
using System.IO;
using System.Text;
using System;
namespace Z_ByteSerialize
{
    public class Compress
    {
        static string compressLock = "compress";
        public static string CompressWithGZip(string input)
        {
            if (string.IsNullOrEmpty(input))
                return input;
            lock (compressLock)
            {
                // 将字符串转换为字节数组
                byte[] inputBytes = Encoding.UTF8.GetBytes(input);

                using (var outputStream = new MemoryStream())
                {
                    // 使用GZip压缩
                    using (var gzipStream = new GZipStream(outputStream, CompressionMode.Compress))
                    {
                        gzipStream.Write(inputBytes, 0, inputBytes.Length);
                    }

                    // 将压缩后的字节数组转换为Base64字符串（便于传输和存储）
                    return Convert.ToBase64String(outputStream.ToArray());
                }
            }
        }

        public static string DecompressWithGZip(string compressedInput)
        {
            if (string.IsNullOrEmpty(compressedInput))
                return compressedInput;
            lock (compressLock)
            {
                // 将Base64字符串转换为字节数组
                byte[] compressedBytes = Convert.FromBase64String(compressedInput);

                using (var inputStream = new MemoryStream(compressedBytes))
                using (var gzipStream = new GZipStream(inputStream, CompressionMode.Decompress))
                using (var outputStream = new MemoryStream())
                {
                    // 解压数据
                    gzipStream.CopyTo(outputStream);

                    // 将字节数组转换回字符串
                    return Encoding.UTF8.GetString(outputStream.ToArray());
                }
            }
        }
    }
}