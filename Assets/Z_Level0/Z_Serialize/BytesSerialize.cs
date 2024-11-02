using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_ByteSerialize
{
    public class BytesSerialize : MonoBehaviour
    {

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