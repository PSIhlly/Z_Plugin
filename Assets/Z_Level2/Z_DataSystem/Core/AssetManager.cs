using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
namespace Z_DataSystem.Form
{
    public enum ValType
    {
        Float = 0,
        Bool = 1,
        String = 2
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
    public interface IAssetController {

        public abstract string GetMark();
        public abstract string[] GetSupportedExtensions();

        public abstract string GetName(string name);

        public abstract bool IsAsset(string name);
    }

    public abstract class SelectTask<T>where T : IAssetController
    {
        protected T ctrl;
        public virtual void Run(T ctrl)
        {
            this.ctrl = ctrl;
        }
        public abstract void OnImportComplete(byte[] data);
    }
    public static class AssetDefines
    {
        public const string IMAGE_MARK = "$i$";
        public const string AUDIO_MARK = "$a$";
        public const string VIDEO_MARK = "$v$";
    }

    public class AssetManager : Z_MonoManager<AssetManager>
    {
        public static string cachePath=>Application.temporaryCachePath+"/DataSystem/";
        AssetCacheCtroller cacheCtrl;
        public TexController texCtrl;
        public AudioController audioCtrl;
        public VideoController videoCtrl;
        public GameObjectController goCtrl;
        public AssetManager()
        {
            cacheCtrl = new AssetCacheCtroller(this);
            texCtrl = new TexController(this);
            audioCtrl = new AudioController(this);
            videoCtrl = new VideoController(this);
            goCtrl = new GameObjectController(this);
        }
        #region all
        public class AssetsRes
        {
            public List<(string, TexAssetForm.Data)> texs = new List<(string, TexAssetForm.Data)>();
            public List<(string, AudioAssetForm.Data)> auds = new List<(string, AudioAssetForm.Data)>();
            public List<(string, VideoAssetForm.Data)> vids = new List<(string, VideoAssetForm.Data)>();
            public List<(string, GameObjectAssetForm.Data)> gos = new List<(string, GameObjectAssetForm.Data)>();
        }

        public AssetsRes LoadAssetsByFolder(string path, bool isRes)
        {
            AssetsRes res = new AssetsRes();
            if (isRes)
            {
                Texture2D[] textures = Resources.LoadAll<Texture2D>(path);
                foreach (var tex in textures)
                {
                    res.texs.Add((tex.name, new TexAssetForm.Data(-1,tex.name,"",null,"", tex)));
                }

                GameObject[] gos = Resources.LoadAll<GameObject>(path);
                foreach (var go in gos)
                {
                    res.gos.Add((go.name, new GameObjectAssetForm.Data(-1, go.name, "", null, "", go)));
                }

            }
            else
            {
                string[] allFiles = Directory.GetFiles(path);

                // 过滤出图片文件
                foreach (string file in allFiles)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (Array.Exists(texCtrl.GetSupportedExtensions(), ext => ext == extension))
                    {
                        res.texs.Add((Path.GetFileNameWithoutExtension(file), new TexAssetForm.Data(-1, Path.GetFileNameWithoutExtension(file), Path.GetFullPath(file), null, null, null) ));
                    }
                }
            }

            return res;
        }

        #endregion

        public override void Init()
        {
            base.Init();


        }


    }
}
