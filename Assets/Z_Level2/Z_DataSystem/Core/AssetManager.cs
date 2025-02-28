using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Os.File;
using Z_Texture;

namespace Z_DataSystem
{
    public class AssetEvent : Z_Event
    {

    }

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
        public class SelectTexTask
        {
            public string tarPath;
            public string fileName;
            public Action<Texture2D> callback;
            public void Run()
            {
                FileImporter.ImportImageBytes(OnImportImageBytesComplete);
            }
            public void OnImportImageBytesComplete(byte[] data)
            {
                if (data != null)
                {
                    instance.LoadTexBytesAutoAdd(data, tarPath, fileName);
                    callback?.Invoke((Texture2D)TextureHelper.GetTextureByPath(tarPath + fileName));
                    Z_EventHelper.Invoke(new AssetEvent());
                }
            }
        }
        public void SelectTexToAutoAdd(string tarPath,string fileName,Action<Texture2D> callback=null)
        {
            SelectTexTask task = new SelectTexTask();
            task.tarPath = tarPath;
            task.fileName = fileName;
            task.callback = callback;
            task.Run();
        }
        public void LoadTexBytesAutoAdd(Texture2D tex, string path, string fileName)
        {
            TextureHelper.SaveTexture(tex.EncodeToPNG(), path, fileName);
        }
        public void LoadTexBytesAutoAdd(byte[] data,string path,string fileName)
        {
            TextureHelper.SaveTexture(data, path, fileName);
            var tex = TextureHelper.GetTextureByPath(path+fileName);
            TexAssetForm.AddData(new TexAssetForm.Data(-1, fileName, tex));
        }

        public Sprite GetSprite(string name)
        {
            if (!TexAssetForm.DataByName.ContainsKey(name))
                return null;
            return TextureHelper.GetSpriteByTexture(TexAssetForm.DataByName[name].tex);
        }

        public AssetsRes LoadAssetsByFolder(string path)
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
        public void LoadAssetsByFolderAutoAdd(string path)
        {
            var res=LoadAssetsByFolder(path);
            foreach (var tex in res.texs)
            {
                TexAssetForm.AddData(new TexAssetForm.Data(-1, tex.Item1, tex.Item2));
            }
        }

        public void DeleteTargetAsset(string path,string fileName)
        {
            TextureHelper.DeleteTexture(path + fileName);
        }
        public void DeleteTargetAssetAutoDel(string path, string fileName)
        {
            DeleteTargetAsset(path, fileName);
            var data = TexAssetForm.DataByName[fileName];
            TexAssetForm.RemoveData(data.id);
        }
    }
}
