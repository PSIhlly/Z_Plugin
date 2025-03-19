using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Os.File;
using Z_Texture;
namespace Z_DataSystem.Form
{
    public enum Type
    {
        Int = 0,
        Bool = 1,
    }

    public partial class ParamForm
    {
        static public Type GetValueType(this Data data)
        {
            return (Type)data.valueType;
        }
    }
    public partial class ProductForm
    {
        static public object GetValue(this Data data, ParamForm.Data prm)
        {
            switch (prm.GetValueType())
            {
                case Type.Int:
                    (int, int, int) info = (((int, int, int))data.paramDic[prm.name]);
                    return info.Item1;
                case Type.Bool:
                    return (bool)data.paramDic[prm.name];
                default:
                    return data.paramDic[prm.name];
            }
        }
        static public object GetValueMin(this Data data, ParamForm.Data prm)
        {
            switch (prm.GetValueType())
            {
                case Type.Int:
                    (int, int, int) info = (((int, int, int))data.paramDic[prm.name]);
                    return info.Item2;
                case Type.Bool:
                    return false;
                default:
                    return data.paramDic[prm.name];
            }
        }
        static public object GetValueMax(this Data data, ParamForm.Data prm)
        {
            switch (prm.GetValueType())
            {
                case Type.Int:
                    (int, int, int) info = (((int, int, int))data.paramDic[prm.name]);
                    return info.Item3;
                case Type.Bool:
                    return true;
                default:
                    return data.paramDic[prm.name];
            }
        }

    }

}
namespace Z_DataSystem
{
    public class AssetEvent : Z_Event
    {
        public string importAssetName;
    }

    public class AssetManager : Z_MonoManager<AssetManager>
    {
        public int assetDefaultIdCnt;
        #region all
        public AssetsRes LoadAssetsByFolder(string path, bool isRes)
        {
            AssetsRes res = new AssetsRes();
            if (isRes)
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(path);
                foreach (var tex in textures)
                {
                    res.texs.Add((tex.name, tex));
                }

                GameObject[] gos = Resources.LoadAll<GameObject>(path);
                foreach (var go in gos)
                {
                    res.gos.Add((go.name, go));
                }

            }
            else
            {
                string[] allFiles = Directory.GetFiles(path);

                // 过滤出图片文件
                foreach (string file in allFiles)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (Array.Exists(SupportedImageExtensions, ext => ext == extension))
                    {
                        res.texs.Add((Path.GetFileNameWithoutExtension(file), TextureHelper.GetTextureByPath(file)));
                    }
                }
            }

            return res;
        }
        public void LoadAssetsByFolderAutoAdd(string path, bool isRes, bool isDefault)
        {
            var res = LoadAssetsByFolder(path, isRes);
            for (int i = 0; i < res.texs.Count; i++)
            {
                TexAssetForm.AddData(new TexAssetForm.Data(isDefault ? ((++assetDefaultIdCnt) + AssetForm.autoIdCnt) : -1, res.texs[i].Item1, res.texs[i].Item2));
            }

            for (int i = 0; i < res.gos.Count; i++)
            {
                GameObjectAssetForm.AddData(new GameObjectAssetForm.Data(isDefault ? ((++assetDefaultIdCnt) + AssetForm.autoIdCnt) : -1, res.gos[i].Item1, res.gos[i].Item2));
            }
        }

        #endregion


        #region texture
        private static readonly string[] SupportedImageExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
    };
        public class AssetsRes
        {
            public List<(string,Texture)> texs = new List<(string, Texture)>();
            public List<(string,GameObject)> gos = new List<(string, GameObject)>();
        }
        public class SelectTexTask
        {
            public string tarPath;
            public string fileName;
            public Action<Texture2D> callback;
            public Vector2Int forceSize;
            public void Run()
            {
                FileImporter.ImportImageBytes(OnImportImageBytesComplete);
            }
            public void OnImportImageBytesComplete(byte[] data)
            {
                if (data != null)
                {
                    var tex=(Texture2D)TextureHelper.GetTextureByByte(data);
                    if (forceSize != Vector2Int.zero)
                        tex = TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y);

                    instance.LoadTexBytesAutoAdd(tex, tarPath, fileName);
                    callback?.Invoke((Texture2D)TextureHelper.GetTextureByPath(tarPath + fileName));
                    Z_EventHelper.Invoke(new AssetEvent()
                    {
                        importAssetName = fileName
                    }) ;
                }
            }
        }
        public void SelectTexToAutoAdd(string tarPath,string fileName, Vector2Int forceSize, Action<Texture2D> callback=null)
        {
            SelectTexTask task = new SelectTexTask();
            task.tarPath = tarPath;
            task.fileName = fileName;
            task.callback = callback;
            task.forceSize = forceSize;
            task.Run();
        }
        public void LoadTexBytesAutoAdd(Texture2D tex, string path, string fileName, Vector2Int forceSize)
        {
            LoadTexBytesAutoAdd(TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y).EncodeToPNG(), path, fileName);
        }
        public void LoadTexBytesAutoAdd(Texture2D tex, string path, string fileName)
        {
            LoadTexBytesAutoAdd(tex.EncodeToPNG(), path, fileName);
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

      
        public void DeleteTexAsset(string path,string fileName)
        {
            TextureHelper.DeleteTexture(path + fileName);
        }
        public void DeleteTexAssetAutoDel(string path, string fileName)
        {
            DeleteTexAsset(path, fileName);
            if (!TexAssetForm.DataByName.ContainsKey(fileName))
                return;
            var data = TexAssetForm.DataByName[fileName];
            TexAssetForm.RemoveData(data.id);
        }
        public void RenameTargetAssetAuto(string path,string oldName,string newName)
        {
            TextureHelper.RenameTexture(path, oldName, newName);
            TexAssetForm.DataByName[oldName].name = newName;
        }

        #endregion

        #region gameobject

        public GameObject GetGameObject(string name)
        {
            return GameObjectAssetForm.DataByName.ContainsKey(name) ?GameObjectAssetForm.DataByName[name].go:null;
        }

        #endregion


        public void UnloadAllAuto()
        {
            List<AssetForm.Data> texDatas = new List<AssetForm.Data>(AssetForm.DataById.Values);
            foreach(var data in texDatas)
            {
                if(data.id<= AssetForm.autoIdCnt)
                {
                    TexAssetForm.DataById.Remove(data.id);
                }
            }
        }

    }
}
