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
            internal Texture _tex;
            public Texture tex
            {
                get
                {
                    if(forceTex!=null)
                    {
                        return forceTex;
                    }
                    if (_tex == null)
                    {
                        _tex = TextureHelper.GetTextureByPath(path);
                    }
                    return _tex;
                }
                set
                {
                    _tex = value;
                }
            }
        }
    }
    public partial class AudioAssetForm
    {
        public partial class Data
        {
            
            private AudioClip _clip;
            public AudioClip clip
            {
                get
                {

                    if (_clip == null)
                    {
                        //_clip = TextureHelper.GetTextureByPath(path);
                    }
                    return _clip;
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
        public class AssetsRes
        {
            public List<(string, Texture)> texs = new List<(string, Texture)>();
            public List<(string, AudioClip)> auds = new List<(string, AudioClip)>();
            public List<(string, string)> vids = new List<(string, string)>();
            public List<(string, GameObject)> gos = new List<(string, GameObject)>();
        }

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

        public const string IMAGE_MARK = "$i$";
        private static readonly string[] SupportedImageExtensions = new[]
    {
        ".jpg", ".jpeg", ".png", ".bmp", ".gif", ".webp"
    };

        public class SelectTexTask
        {
            public Action<TexAssetForm.Data> callback;
            public Vector2Int forceSize;
            public void Run()
            {
                FileImporter.ImportImageBytes(OnImportBytesComplete);
            }
            public void OnImportBytesComplete(byte[] data)
            {
                if (data != null)
                {
                    var tex = (Texture2D)TextureHelper.GetTextureByByte(data);
                    if (forceSize != Vector2Int.zero)
                        tex = TextureTransform.GetTargetSize(tex, forceSize.x, forceSize.y);
                    var nm = tex.GetHashCode().ToString();
                    var form = instance.LoadTex(tex, nm);
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

        public void SelectTex(Vector2Int forceSize = default, Action<TexAssetForm.Data> callback = null)
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
            return LoadTex(tex, name);
        }
        public TexAssetForm.Data LoadTexPath(string path, string name)
        {
            var tex = TextureHelper.GetTextureByPath(path);
            return new TexAssetForm.Data(-1, name, path,null);
        }
        public TexAssetForm.Data LoadTex(Texture tex, string name)
        {
            var texData = new TexAssetForm.Data(-1, name, "", null);
            texData._tex = tex;
            
            return texData;
        }


        #endregion
        /*  #region audio

          public const string AUDIO_MARK = "$a$";
          private static readonly string[] SupportedAudioExtensions = new[]
      {
          ".mp3", ".wav"
      };

          public class SelectAudioTask
          {
              public Action<AudioAssetForm.Data> callback;
              public void Run()
              {
                  FileImporter.ImportAudioBytes(OnImportBytesComplete);
              }
              public void OnImportBytesComplete(byte[] data)
              {
                  if (data != null)
                  {
                      var tex = (Texture2D)TextureHelper.GetTextureByByte(data);
                      var nm = tex.GetHashCode().ToString();
                      var form = instance.LoadAudio(tex, nm);
                      callback?.Invoke(form);
                      Z_EventHelper.Invoke(new AssetEvent()
                      {
                          importAssetName = nm
                      });
                  }
              }
          }
          public List<string> GetAudioAssetsByFolder(string path, bool isRes)
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

          public void SelectTex(Vector2Int forceSize = default, Action<TexAssetForm.Data> callback = null)
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


          #endregion*/


        #region gameobject

        public GameObject GetGameObject(string name)
        {
            return GameObjectAssetForm.DataByName.ContainsKey(name) ? GameObjectAssetForm.DataByName[name].go : null;
        }

        #endregion


        public static string GetIdNameKey(int id, string name)
        {
            return id + "$￥$" + name;
        }
        public static int GetKeyId(string key)
        {
            if (int.TryParse(key.Split("$￥$")[0], out int id))
                return id;
            return 0;
        }
        public static string GetKeyName(string key)
        {
            var res = key.Split("$￥$");
            if (res.Length>1)
                return res[1];
            return "";
        }

    }
}
