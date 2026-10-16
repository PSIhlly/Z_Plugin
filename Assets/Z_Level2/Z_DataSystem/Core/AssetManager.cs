using JetBrains.Annotations;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading;
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
        public static async Task<string> GetModPackageAsync(string title, CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
            var selection = new TaskCompletionSource<string>();
            using (cancellationToken.Register(() => selection.TrySetCanceled()))
            {
                GetModPackage(path => selection.TrySetResult(path),
                    error => selection.TrySetException(new IOException(error)), title);
                return await selection.Task;
            }
        }

        public static async Task<bool> ExportModPackageAsync(Func<Task<byte[]>> package, string name, string title,
            CancellationToken cancellationToken = default)
        {
            cancellationToken.ThrowIfCancellationRequested();
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
            var path = GetModPackageSavePath(name, title);
            if (string.IsNullOrEmpty(path))
                return false;
            cancellationToken.ThrowIfCancellationRequested();
            var content = await package();
            await Task.Run(() => File.WriteAllBytes(path, content), cancellationToken);
            return true;
#elif UNITY_ANDROID && !UNITY_EDITOR
            var content = await package();
            cancellationToken.ThrowIfCancellationRequested();
            var saved = new TaskCompletionSource<bool>();
            using (cancellationToken.Register(() => saved.TrySetCanceled()))
            {
                // NativeGallery/Android JNI must be called on the Unity thread.
                // Its byte[] API preserves the prepared lossless PNG unchanged.
                var permission = NativeGallery.SaveImageToGallery(content, Application.productName,
                    GetModPackageFileName(name), (success, path) =>
                    {
                        if (success)
                            saved.TrySetResult(true);
                        else
                            saved.TrySetException(new IOException("Could not save the Mod package to the gallery."));
                    });
                if (permission != NativeGallery.Permission.Granted)
                    saved.TrySetException(new IOException("Image save permission was not granted."));
                return await saved.Task;
            }
#else
            throw new PlatformNotSupportedException("Mod file export is only available on Windows and Android.");
#endif
        }

        public static void GetModPackage(Action<string> callback, Action<string> onError, string title)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR) || (UNITY_ANDROID && !UNITY_EDITOR)
            // Use the same image picker as texture import. Read its original
            // file bytes, never load/resize/re-encode the package image.
            GetImage(path =>
            {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
                if (!string.IsNullOrEmpty(path) && !Path.GetExtension(path).Equals(".png", StringComparison.OrdinalIgnoreCase))
                {
                    if (onError != null)
                        onError.Invoke("Please select a .png package.");
                    else
                        callback?.Invoke(null);
                    return;
                }
#endif
                // Gallery providers can use a .tmp cache name. ImportPackage
                // validates the PNG signature/protocol from the original bytes.
                callback?.Invoke(path);
            }, title, "image/png", onError);
#else
            throw new PlatformNotSupportedException("Mod file selection is only available on Windows and Android.");
#endif
        }

        public static void ExportModPackage(Func<byte[]> package, string name, string title,
            Action<bool> onComplete, Action<string> onError)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
            var path = GetModPackageSavePath(name, title);
            if (string.IsNullOrEmpty(path))
                onComplete?.Invoke(false);
            else
            {
                File.WriteAllBytes(path, package());
                onComplete?.Invoke(true);
            }
#elif UNITY_ANDROID && !UNITY_EDITOR
            // The byte[] overload copies the PNG unchanged into the gallery.
            var permission = NativeGallery.SaveImageToGallery(package(), Application.productName,
                GetModPackageFileName(name), (success, path) =>
                {
                    if (success)
                        onComplete?.Invoke(true);
                    else if (onError != null)
                        onError.Invoke("Could not save the Mod package to the gallery.");
                    else
                        onComplete?.Invoke(false);
                });
            if (permission != NativeGallery.Permission.Granted)
            {
                if (onError != null)
                    onError.Invoke("Image save permission was not granted.");
                else
                    onComplete?.Invoke(false);
            }
#else
            throw new PlatformNotSupportedException("Mod file export is only available on Windows and Android.");
#endif
        }

#if UNITY_EDITOR_WIN
        private const string AudioFileFilter = "Audio files\0*.mp3;*.aac;*.flac;*.wav\0All files\0*.*\0\0";
#endif

        public static void GetImage(Action<string> callback, string title = "Select image file",
            string mime = "image/*", Action<string> onError = null)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
            var files = GetFiles(
                title,
                mime == "image/png" ? "PNG image files\0*.png\0\0" :
                    "Image files\0*.png;*.jpg;*.jpeg;*.gif;*.bmp;*.webp\0All files\0*.*\0\0",
                false, onError != null);
            callback?.Invoke(files.Length > 0 ? files[0] : null);
#else
            if (NativeGallery.IsMediaPickerBusy())
            {
                Fail("An image picker is already open.");
                return;
            }
            var permission = NativeGallery.GetImageFromGallery(path => callback?.Invoke(path), title, mime);
            if (permission != NativeGallery.Permission.Granted)
                Fail("Image selection permission was not granted.");

            void Fail(string error)
            {
                if (onError != null)
                    onError.Invoke(error);
                else
                    callback?.Invoke(null);
            }
#endif
        }

        public static void GetImages(Action<string[]> callback)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
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
                AudioFileFilter));
#else
            NativeGallery.GetAudiosFromGallery(paths => callback?.Invoke(paths));
#endif
        }

        public static void GetAudio(Action<string> callback)
        {
#if UNITY_EDITOR_WIN
            var files = GetFiles(
                "Select audio file",
                AudioFileFilter,
                false);
            callback?.Invoke(files.Length > 0 ? files[0] : null);
#else
            NativeGallery.GetAudioFromGallery(path => callback?.Invoke(path));
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

        // Story names are user-authored, not paths. Keep the suggested package name
        // valid even when it contains separators or Windows reserved names.
        public static string GetModPackageFileName(string name)
        {
            var chars = (name ?? "").ToCharArray();
            for (var i = 0; i < chars.Length; i++)
                if (chars[i] < ' ' || "<>:\"/\\|?*".IndexOf(chars[i]) >= 0)
                    chars[i] = '_';
            name = new string(chars).Trim().TrimEnd('.');
            if (name.Length == 0)
                name = "Story";
            if (name.Length > 200)
                name = name.Substring(0, char.IsHighSurrogate(name[199]) ? 199 : 200);

            var stem = name.Split('.')[0].TrimEnd().ToUpperInvariant();
            if (stem == "CON" || stem == "PRN" || stem == "AUX" || stem == "NUL" ||
                stem == "CONIN$" || stem == "CONOUT$" ||
                (stem.Length == 4 && (stem.StartsWith("COM") || stem.StartsWith("LPT")) &&
                 "123456789¹²³".IndexOf(stem[3]) >= 0))
                name = "_" + name;
            return name + ".png";
        }

        // Returns null on cancel. This native dialog does not write the file.
        public static string GetModPackageSavePath(string name, string title)
        {
#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
            var buffer = System.Runtime.InteropServices.Marshal.AllocHGlobal(MaxPathBuffer * sizeof(char));
            try
            {
                System.Runtime.InteropServices.Marshal.Copy(new byte[MaxPathBuffer * sizeof(char)], 0, buffer, MaxPathBuffer * sizeof(char));
                var fileName = (GetModPackageFileName(name) + '\0').ToCharArray();
                System.Runtime.InteropServices.Marshal.Copy(fileName, 0, buffer, fileName.Length);
                var saveFileName = new OpenFileName
                {
                    structSize = System.Runtime.InteropServices.Marshal.SizeOf(typeof(OpenFileName)),
                    dlgOwner = System.Diagnostics.Process.GetCurrentProcess().MainWindowHandle,
                    filter = "Mod package (*.png)\0*.png\0\0",
                    filterIndex = 1,
                    file = buffer,
                    maxFile = MaxPathBuffer,
                    title = title,
                    defExt = "png",
                    flags = OfnExplorer | OfnPathMustExist | OfnNoChangeDir | OfnOverwritePrompt
                };
                if (!GetSaveFileName(saveFileName))
                {
                    var error = CommDlgExtendedError();
                    if (error != 0)
                        throw new InvalidOperationException("Mod save dialog failed (0x" + error.ToString("X") + ").");
                    return null;
                }

                var path = System.Runtime.InteropServices.Marshal.PtrToStringUni(buffer);
                // Do not change the destination after the native overwrite prompt.
                if (!string.Equals(Path.GetExtension(path), ".png", StringComparison.OrdinalIgnoreCase))
                    throw new ArgumentException("Please use a .png file name.");
                return path;
            }
            finally
            {
                System.Runtime.InteropServices.Marshal.FreeHGlobal(buffer);
            }
#else
            throw new PlatformNotSupportedException("The Mod save dialog is only available on Windows.");
#endif
        }

#if UNITY_EDITOR_WIN || (UNITY_STANDALONE_WIN && !UNITY_EDITOR)
        private const int MaxPathBuffer = 65536;
        private const int OfnOverwritePrompt = 0x00000002;
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

        [System.Runtime.InteropServices.DllImport(
            "comdlg32.dll",
            CharSet = System.Runtime.InteropServices.CharSet.Unicode,
            EntryPoint = "GetSaveFileNameW",
            SetLastError = true)]
        [return: System.Runtime.InteropServices.MarshalAs(System.Runtime.InteropServices.UnmanagedType.Bool)]
        private static extern bool GetSaveFileName(
            [System.Runtime.InteropServices.In, System.Runtime.InteropServices.Out] OpenFileName openFileName);

        [System.Runtime.InteropServices.DllImport("comdlg32.dll")]
        private static extern uint CommDlgExtendedError();

        private static string[] GetFiles(string title, string filter, bool allowMultiSelect = true, bool reportError = false)
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
                    flags = OfnExplorer | OfnPathMustExist | OfnFileMustExist | OfnNoChangeDir |
                            (allowMultiSelect ? OfnAllowMultiSelect : 0)
                };
                if (!GetOpenFileName(openFileName))
                {
                    if (reportError)
                    {
                        var error = CommDlgExtendedError();
                        if (error != 0)
                            throw new InvalidOperationException("Mod open dialog failed (0x" + error.ToString("X") + ").");
                    }
                    return Array.Empty<string>();
                }

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
