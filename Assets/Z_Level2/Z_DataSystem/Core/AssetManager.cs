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
            public virtual void ToProduct(int protoUid)
            {
                this.protoUid = protoUid;
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
    public interface IAssetController
    {

        public abstract string GetMark();
        public abstract string[] GetSupportedExtensions();

        public abstract string GetName(int id);

        public abstract bool IsAsset(string name);
    }

    public static class AssetFilePicker
    {
        public static void GetImages(Action<string[]> callback)
        {
#if UNITY_EDITOR_WIN
            callback?.Invoke(GetFiles(
                "Select image files",
                "Image files\0*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp\0All files\0*.*\0\0"));
#else
            NativeGallery.GetImagesFromGallery(paths => callback?.Invoke(paths));
#endif
        }

        public static void GetAudios(Action<string[]> callback)
        {
#if UNITY_EDITOR_WIN
            callback?.Invoke(GetFiles(
                "Select audio files",
                "Audio files\0*.mp3;*.aac;*.flac\0All files\0*.*\0\0"));
#else
            NativeGallery.GetAudiosFromGallery(paths => callback?.Invoke(paths));
#endif
        }

        public static void GetVideos(Action<string[]> callback)
        {
#if UNITY_EDITOR_WIN
            callback?.Invoke(GetFiles(
                "Select video files",
                "Video files\0*.mp4;*.mov;*.wav;*.avi\0All files\0*.*\0\0"));
#else
            NativeGallery.GetVideosFromGallery(paths => callback?.Invoke(paths));
#endif
        }

#if UNITY_EDITOR_WIN
        private const int MaxPathBuffer = 65536;
        private const int OfnAllowMultiSelect = 0x00000200;
        private const int OfnPathMustExist = 0x00000800;
        private const int OfnFileMustExist = 0x00001000;
        private const int OfnExplorer = 0x00080000;
        private const int OfnNoChangeDir = 0x00000008;

        [System.Runtime.InteropServices.StructLayout(
            System.Runtime.InteropServices.LayoutKind.Sequential,
            CharSet = System.Runtime.InteropServices.CharSet.Unicode)]
        private sealed class OpenFileName
        {
            public int structSize;
            public IntPtr dlgOwner;
            public IntPtr instance;
            public string filter;
            public string customFilter;
            public int maxCustFilter;
            public int filterIndex;
            public IntPtr file;
            public int maxFile;
            public IntPtr fileTitle;
            public int maxFileTitle;
            public string initialDir;
            public string title;
            public int flags;
            public short fileOffset;
            public short fileExtension;
            public string defExt;
            public IntPtr custData;
            public IntPtr hook;
            public string templateName;
            public IntPtr reservedPtr;
            public int reservedInt;
            public int flagsEx;
        }

        [System.Runtime.InteropServices.DllImport(
            "comdlg32.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Unicode,
            EntryPoint = "GetOpenFileNameW",
            SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetOpenFileName(
            [System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] OpenFileName openFileName);

        private static string[] GetFiles(string title, string filter)
        {
            var buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(MaxPathBuffer * sizeof(char));
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(new byte[MaxPathBuffer * sizeof(char)], 0, buffer, MaxPathBuffer * sizeof(char));
                var openFileName = new OpenFileName
                {
                    structSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(OpenFileName)),
                    dlgOwner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle,
                    filter = filter,
                    filterIndex = 1,
                    file = buffer,
                    maxFile = MaxPathBuffer,
                    title = title,
                    flags = OfnExplorer | OfnAllowMultiSelect | OfnPathMustExist | OfnFileMustExist | OfnNoChangeDir
                };
                if (!GetOpenFileName(openFileName))
                    return Array.Empty<string>();

                var chars = new char[MaxPathBuffer];
                System.Runtime.InteropServices.Marshal.Copy(buffer, chars, 0, chars.Length);
                var parts = new List<string>();
                var start = 0;
                for (var i = 0; i < chars.Length; i++)
                {
                    if (chars[i] != '\0')
                        continue;
                    if (i == start)
                        break;
                    parts.Add(new string(chars, start, i - start));
                    start = i + 1;
                }

                if (parts.Count <= 1)
                    return parts.ToArray();

                var directory = parts[0];
                var paths = new string[parts.Count - 1];
                for (var i = 1; i < parts.Count; i++)
                    paths[i - 1] = Path.Combine(directory, parts[i]);
                return paths;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
            }
        }
#endif
    }

    public abstract class SelectTask<T> where T : IAssetController
    {
        protected T ctrl;
        public virtual void Run(T ctrl)
        {
            this.ctrl = ctrl;
        }
        public abstract void OnImportComplete(byte[] data,string name);
    }
    public static class AssetDefines
    {
        public const string IMAGE_MARK = "$i$";
        public const string AUDIO_MARK = "$a$";
        public const string VIDEO_MARK = "$v$";
    }

    public class AssetManager : Z_MonoManager<AssetManager>
    {

        public static string externPatn{
            get
            {
                var path = "";
#if UNITY_EDITOR_WIN
                path = $"{Application.dataPath}/HlzyAssets/";
                if (!Directory.Exists(path))
                {
                    Directory.CreateDirectory(path);
                }
#elif UNITY_ANDROID
                var id = Application.persistentDataPath.IndexOf("Android");
                path = Application.persistentDataPath.Remove(id)+ "HlzyAssets";
#endif
                
                return path;
            }
            
        }
        public static string cachePath => Application.temporaryCachePath + "/DataSystem/";
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
                    res.texs.Add((tex.name, new TexAssetForm.Data(int.TryParse(tex.name, out var id) ? id : -1, tex.name, "", null, "", tex, 0)));
                }

                GameObject[] gos = Resources.LoadAll<GameObject>(path);
                foreach (var go in gos)
                {
                    res.gos.Add((go.name, new GameObjectAssetForm.Data(int.TryParse(go.name, out var id) ? id : -1, go.name, "", null, "", go, 0)));
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
                        res.texs.Add((Path.GetFileNameWithoutExtension(file), new TexAssetForm.Data(-1, Path.GetFileNameWithoutExtension(file), Path.GetFullPath(file), null, null, null, 0)));
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
