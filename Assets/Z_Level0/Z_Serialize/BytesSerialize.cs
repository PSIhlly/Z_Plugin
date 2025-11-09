using System;
using System.Collections;
using System.Collections.Generic;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;

namespace Z_ByteSerialize
{
    public class BytesSerialize
    {
        public static string GetHash(byte[] bytes)
        {
            if (bytes == null || bytes.Length == 0)
                return "0";

            StringBuilder sb = new StringBuilder();
            // 使用 using 确保资源释放
            using (SHA1 sha1 = SHA1.Create()) // 推荐使用 SHA1.Create() 而非直接实例化具体类
            {
                foreach (byte b in sha1.ComputeHash(bytes))
                {
                    // 格式化为两位十六进制，不足两位补0
                    sb.Append(b.ToString("x2"));
                }
            }
            return sb.ToString();
        }

    public static byte[] SetInHeadBytes(object src,ref byte[] tar)
    {
        byte[] tarBytes;
        if(src is int srcInt)
        {
                var srcBytes = BitConverter.GetBytes(srcInt);
                tarBytes = new byte[srcBytes.Length + tar.Length];
                Buffer.BlockCopy(srcBytes, 0, tarBytes, 0, srcBytes.Length);
                Buffer.BlockCopy(tar, 0, tarBytes, srcBytes.Length, tar.Length);
                tar = tarBytes;
        }
        
            return tar;
    }
        public static object GetOutHeadBytes(ref byte[] src,ref object tar)
        {
            byte[] srcBytes = new byte[0];
            if (tar is int)
            {
                srcBytes = new byte[4];
                Buffer.BlockCopy(src, 0, srcBytes, 0, 4);
                byte[] newSrc = new byte[src.Length - 4];
                Buffer.BlockCopy(src, 4, newSrc, 0, src.Length - 4);
                src = newSrc;
                tar = BitConverter.ToInt32(srcBytes,0);
            }
            return tar;
        }
    }
}