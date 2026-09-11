using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.Events;
using UnityEngine.UI;
using UnityEngine.Video;
using Z_ByteSerialize;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
namespace Z_DataSystem.Form
{
    public partial class VideoAssetForm
    {
        public partial class Data
        {
            public void Play(MediaPlayer player, Action onComplete)
            {
                player.OpenMedia(new MediaPath(path, MediaPathType.AbsolutePathOrURL), true);
                UnityAction<MediaPlayer, MediaPlayerEvent.EventType, ErrorCode> handler = null;
                handler = (mp, et, errorCode) =>
                {
                    if (et == MediaPlayerEvent.EventType.FinishedPlaying)
                    {
                        player.Events.RemoveListener(handler);
                        onComplete?.Invoke();
                    }
                };
                player.Events.AddListener(handler);
            }
        }
    }
}
namespace Z_DataSystem
{
    public class VideoController : Z_Controller<AssetManager>, IAssetController
    {
        public bool IsAsset(string name)
        {
            var parts = name.Split(GetMark());
            return parts.Length == 3 && string.IsNullOrEmpty(parts[0]) && string.IsNullOrEmpty(parts[2]);
        }
        public virtual string GetName(int id=-1)
        {
            return $"{GetMark()}{id}{GetMark()}";
        }
        public int GetId(string name = "")
        {
            if (name == null)
                return -1;
            var parts = name.Split(GetMark());
            if (parts.Length == 3 && int.TryParse(parts[1], out int id))
            {
                return id;
            }
            return -1;
        }
        public string GetMark() => AssetDefines.VIDEO_MARK;
        public string[] GetSupportedExtensions() => new[]
{
        ".mp3"
        };

        public VideoController(AssetManager super) : base(super)
        {
        }


        public class SelectVideoTask : SelectTask<VideoController>
        {
            public Action<VideoAssetForm.Data> callback;
            public override void Run(VideoController ctrl)
            {
                base.Run(ctrl);
                NativeGallery.GetVideoFromGallery((path) => OnImportComplete(string.IsNullOrEmpty(path) ? null : File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path)));
            }
            public override void OnImportComplete(byte[] data, string name)
            {
                var form = ctrl.CreateSelectedData(data, name);
                if (form != null)
                    callback?.Invoke(form);
            }
        }
        public List<string> GetVideoAssetsByFolder(string path, bool isRes)
        {
            List<string> res = new List<string>();
            if (isRes)
            {
                VideoClip[] clips = Resources.LoadAll<VideoClip>(path);
                foreach (var clip in clips)
                {
                    res.Add(clip.name);
                }

            }
            else
            {
                string[] allFiles = Directory.GetFiles(path);
                // ���˳�ͼƬ�ļ�
                foreach (string file in allFiles)
                {
                    string extension = Path.GetExtension(file).ToLower();
                    if (Array.Exists(GetSupportedExtensions(), ext => ext == extension))
                    {
                        res.Add(file);
                    }
                }
            }
            return res;
        }

        public void Select(Action<VideoAssetForm.Data> callback = null)
        {
            SelectVideoTask task = new SelectVideoTask();
            task.callback = callback;
            task.Run(this);
        }
        public void SelectMultiple(Action<IReadOnlyList<VideoAssetForm.Data>> callback = null)
        {
            AssetFilePicker.GetVideos(paths =>
            {
                var forms = new List<VideoAssetForm.Data>();
                if (paths != null)
                {
                    foreach (var path in paths)
                    {
                        if (string.IsNullOrEmpty(path))
                            continue;
                        var form = CreateSelectedData(File.ReadAllBytes(path), Path.GetFileNameWithoutExtension(path));
                        if (form != null)
                            forms.Add(form);
                    }
                }
                callback?.Invoke(forms);
            });
        }
        private VideoAssetForm.Data CreateSelectedData(byte[] data, string name)
        {
            if (data == null)
                return null;

            var hash = BytesSerialize.GetHash(data);
            var nm = GetMark() + hash + GetMark();
            if (!Directory.Exists(AssetManager.cachePath))
                Directory.CreateDirectory(AssetManager.cachePath);
            File.WriteAllBytes(AssetManager.cachePath + nm, data);
            var form = CreateDataByPath(AssetManager.cachePath + nm, name);
            form.hash = hash;
            Z_EventHelper.Invoke(new AssetEvent()
            {
                importAssetName = name
            });
            return form;
        }
        public VideoAssetForm.Data CreateDataByClip(VideoClip clip, string name)
        {
            return new VideoAssetForm.Data(-1, name, "", null, "", clip, 0);
        }
        public VideoAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new VideoAssetForm.Data(-1, name, "", data, BytesSerialize.GetHash(data), null, 0);
        }
        public VideoAssetForm.Data CreateDataByPath(string path, string name)
        {
            path = SaveAndLoad.GetRealPath(path);
            return new VideoAssetForm.Data(-1, name, path, null, "", null, 0);
        }

    }
}
