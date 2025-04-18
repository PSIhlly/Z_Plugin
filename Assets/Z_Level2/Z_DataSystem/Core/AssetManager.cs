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
        Float = 0,
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
        static public T GetValue<T>(this Data data, ParamForm.Data prm)
        {
            data.TryInit(prm);
            (float, float, float) info = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                    return (T)(object)info.Item1;
                case Type.Bool:
                    return info.Item1 == 1 ? (T)(object)true : (T)(object)false;
                default:
                    return (T)(object)data.paramDic[prm.name];
            }
        }
        static public T GetValueMin<T>(this Data data, ParamForm.Data prm)
        {
            data.TryInit(prm);
            (float, float, float) info = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                    return (T)(object)info.Item2;
                case Type.Bool:
                    return info.Item2 == 1 ? (T)(object)true : (T)(object)false;
                default:
                    return (T)(object)data.paramDic[prm.name];
            }
        }
        static public T GetValueMax<T>(this Data data, ParamForm.Data prm)
        {
            data.TryInit(prm);
            (float, float, float) info = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                    return (T)(object)info.Item3;
                case Type.Bool:
                    return info.Item3 == 1 ? (T)(object)true : (T)(object)false;
                default:
                    return (T)(object)data.paramDic[prm.name];
            }
        }

        static public void SetValue(this Data data, ParamForm.Data prm, object v)
        {
            data.TryInit(prm);
            (float, float, float) info = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                    info.Item1 = (int)v;
                    break;
                case Type.Bool:
                    info.Item1 = ((bool)v) ? 1 : 0;
                    break;
                default:
                    break;
            }
            data.paramDic[prm.name] = info;
            data.CheckInt(prm);
        }
        static public void SetValueMin(this Data data, ParamForm.Data prm, object v)
        {
            data.TryInit(prm);

            (float, float, float) info = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                    info.Item2 = (int)v;
                    break;
                case Type.Bool:
                    info.Item2 = ((bool)v) ? 1 : 0;
                    break;
                default:
                    break;
            }
            data.paramDic[prm.name] = info;
            data.CheckInt(prm);
        }
        static public void SetValueMax(this Data data, ParamForm.Data prm, object v)
        {
            data.TryInit(prm);
            (float, float, float) info = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                    info.Item3 = (int)v;
                    break;
                case Type.Bool:
                    info.Item3 = ((bool)v) ? 1 : 0;
                    break;
                default:
                    break;
            }
            data.paramDic[prm.name] = info;
            data.CheckInt(prm);
        }
        static private void TryInit(this Data data, ParamForm.Data prm)
        {
            if (!data.paramDic.ContainsKey(prm.name))
            {
                switch (prm.GetValueType())
                {
                    case Type.Float:
                        data.paramDic[prm.name] = (100, 0, 100);
                        break;
                    case Type.Bool:
                        data.paramDic[prm.name] = (0, 0, 1);
                        break;
                }

            }
        }
        static private void CheckInt(this Data data, ParamForm.Data prm)
        {
            data.TryInit(prm);
            (float, float, float) v = data.paramDic[prm.name];
            switch (prm.GetValueType())
            {
                case Type.Float:
                case Type.Bool:

                    if (v.Item1 < v.Item2)
                        v.Item1 = v.Item2;
                    if (v.Item1 > v.Item3)
                        v.Item1 = v.Item3;

                    data.paramDic[prm.name] = v;
                    break;
            }
        }

    }


    public partial class TexAssetForm
    {
        public partial class Data
        {
            private Sprite _sprite;
            public Sprite sprite
            {
                get
                {
                    
                    if (_sprite == null)
                    {
                        _sprite = TextureHelper.GetSpriteByTexture(tex);
                    }
                    return _sprite;
                }
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

    public class AssetCacheCtroller : Z_Controller<AssetManager>
    {
        public AssetCacheCtroller(AssetManager super) : base(super)
        {
            Init(super);
        }
        private static Dictionary<string, Texture> textureCache = new Dictionary<string, Texture>();
        private static Dictionary<Texture, Sprite> spriteCache = new Dictionary<Texture, Sprite>();
    }


    public class AssetManager : Z_MonoManager<AssetManager>
    {
        AssetCacheCtroller cacheCtrl;
        public AssetManager()
        {
            cacheCtrl = new AssetCacheCtroller(this);
        }
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
                        var tex = TextureHelper.GetTextureByPath(file);
                            res.texs.Add((Path.GetFullPath(file), tex));
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
                TexAssetForm.AddData(new TexAssetForm.Data(isDefault ? ((++assetDefaultIdCnt) + AssetForm.autoIdCnt) : -1, Path.GetFileNameWithoutExtension(res.texs[i].Item1), res.texs[i].Item2));
            }

            for (int i = 0; i < res.gos.Count; i++)
            {
                GameObjectAssetForm.AddData(new GameObjectAssetForm.Data(isDefault ? ((++assetDefaultIdCnt) + AssetForm.autoIdCnt) : -1, Path.GetFileName(res.gos[i].Item1), res.gos[i].Item2));
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
            public List<(string, Texture)> texs = new List<(string, Texture)>();
            public List<(string, GameObject)> gos = new List<(string, GameObject)>();
        }
        public class SelectTexTask
        {
            public Action<Texture2D, string> callback;
            public Vector2Int forceSize;
            public void Run()
            {
                FileImporter.ImportImageBytes(OnImportImageBytesComplete);
            }
            public void OnImportImageBytesComplete(byte[] data)
            {
                if (data != null)
                {
                    var tex = (Texture2D)TextureHelper.GetTextureByByte(data);
                    if (forceSize != Vector2Int.zero)
                        tex = TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y);
                    var nm = tex.imageContentsHash.GetHashCode().ToString();
                    instance.LoadTex(tex, nm);
                    callback?.Invoke(tex, nm);
                    Z_EventHelper.Invoke(new AssetEvent()
                    {
                        importAssetName = nm
                    });
                }
            }
        }
        public List<string> GetTexAssetsByFolder(string path, bool isRes)
        {
            List<string> res = new List<string>();
            if (isRes)
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(path);
                foreach (var tex in textures)
                {
                    res.Add(tex.name);
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
                        res.Add(file);
                    }
                }
            }
            return res;
        }

        public void SelectTex(Vector2Int forceSize, Action<Texture2D,string> callback = null)
        {
            SelectTexTask task = new SelectTexTask();
            task.callback = callback;
            task.forceSize = forceSize;
            task.Run();
        }
        public void LoadTex(Texture2D tex, string name, Vector2Int forceSize)
        {
            LoadTexBytes(TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y).EncodeToPNG(), name);
        }
        public void LoadTex(Texture2D tex, string name)
        {
            LoadTexBytes(tex.EncodeToPNG(), name);
        }
        public void LoadTexBytes(byte[] data, string name)
        {
            var tex = TextureHelper.GetTextureByByte(data);
            TexAssetForm.AddData(new TexAssetForm.Data(-1, name, tex));
        }

        public void DeleteTexAsset(string path, string fileName)
        {
            DeleteTexAsset(path, fileName);
            if (!TexAssetForm.DataByName.ContainsKey(fileName))
                return;
            var data = TexAssetForm.DataByName[fileName];
            TexAssetForm.RemoveData(data.id);
        }
        public void RenameTargetAsset(string oldName, string newName)
        {
            TexAssetForm.DataByName[oldName].name = newName;
        }

        #endregion

        #region gameobject

        public GameObject GetGameObject(string name)
        {
            return GameObjectAssetForm.DataByName.ContainsKey(name) ? GameObjectAssetForm.DataByName[name].go : null;
        }

        #endregion


        public void UnloadAllAuto()
        {
            List<AssetForm.Data> texDatas = new List<AssetForm.Data>(AssetForm.DataById.Values);
            foreach (var data in texDatas)
            {
                if (data.id <= AssetForm.autoIdCnt)
                {
                    TexAssetForm.DataById.Remove(data.id);
                }
            }
        }

    }
}
