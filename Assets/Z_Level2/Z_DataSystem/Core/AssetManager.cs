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
    public enum ValType
    {
        Float = 0,
        Bool = 1,
        String = 2
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
    public partial class ProductForm
    {
        public partial class Data
        {
            public void ToProduct()
            {
                isProto = false;
                AddData(this);
            }
            public void DestroyProduct()
            {
                RemoveData(uid);
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
            public Action<TexAssetForm.Data> callback;
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
                    var form=instance.LoadTex(tex, nm);
                    callback?.Invoke(form);
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

        public void SelectTex(Vector2Int forceSize=default, Action<TexAssetForm.Data> callback = null)
        {
            SelectTexTask task = new SelectTexTask();
            task.callback = callback;
            task.forceSize = forceSize;
            task.Run();
        }
        public TexAssetForm.Data LoadTex(Texture2D tex, string name, Vector2Int forceSize)
        {
            return LoadTexBytes(TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y).EncodeToPNG(), name);
        }
        public TexAssetForm.Data LoadTex(Texture2D tex, string name)
        {
            return LoadTexBytes(tex.EncodeToPNG(), name);
        }
        public TexAssetForm.Data LoadTexBytes(byte[] data, string name)
        {
            var tex = TextureHelper.GetTextureByByte(data);
            return new TexAssetForm.Data(-1, name, tex);
        }
        public TexAssetForm.Data LoadTexPath(string path, string name)
        {
            var tex = TextureHelper.GetTextureByPath(path);
            return new TexAssetForm.Data(-1, name, tex);
        }


        #endregion

        #region gameobject

        public GameObject GetGameObject(string name)
        {
            return GameObjectAssetForm.DataByName.ContainsKey(name) ? GameObjectAssetForm.DataByName[name].go : null;
        }

        #endregion

       
        

    }
}
