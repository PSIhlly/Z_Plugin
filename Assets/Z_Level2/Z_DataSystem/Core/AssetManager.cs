using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DesignStyle;
using Z_Texture;

namespace Z_DataSystem
{
    public class AssetManager : Z_MonoManager<AssetManager>
    {
        private static readonly string[] SupportedImageExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
    };
        public class AssetsRes
        {
            public List<(string,Texture)> texs = new List<(string, Texture)>();
        }
        public AssetsRes GetAssetsByFolder(string path)
        {
            AssetsRes res = new AssetsRes();

            string[] allFiles = Directory.GetFiles(path);

            // 过滤出图片文件
            foreach (string file in allFiles)
            {
                string extension = Path.GetExtension(file).ToLower();
                if (Array.Exists(SupportedImageExtensions, ext => ext == extension))
                {
                    res.texs.Add((Path.GetFileNameWithoutExtension(file),TextureHelper.GetTextureByPath(file)));
                }
            }
            return res;
        }
    }
}
