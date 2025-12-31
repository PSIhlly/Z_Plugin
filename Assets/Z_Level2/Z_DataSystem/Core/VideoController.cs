using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
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
            public void Play(MediaPlayer player)
            {
                player.OpenMedia(new MediaPath(path, MediaPathType.AbsolutePathOrURL), true);
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
        public virtual string GetName(string name)
        {
            return $"{GetMark()}{name}{GetMark()}";
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
                NativeGallery.GetVideoFromGallery((path) => OnImportComplete(string.IsNullOrEmpty(path) ? null : File.ReadAllBytes(path)));
            }
            public override void OnImportComplete(byte[] data)
            {
                if (data != null)
                {
                    var nm = ctrl.GetMark() + BytesSerialize.GetHash(data) + ctrl.GetMark();
                    File.WriteAllBytes(AssetManager.cachePath + nm, data);
                    var form = ctrl.CreateDataByPath(AssetManager.cachePath + nm, nm);
                    callback?.Invoke(form);
                    Z_EventHelper.Invoke(new AssetEvent()
                    {
                        importAssetName = nm
                    });
                }
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
                // 过滤出图片文件
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

        public void Select(Vector2Int forceSize = default, Action<VideoAssetForm.Data> callback = null)
        {
            SelectVideoTask task = new SelectVideoTask();
            task.callback = callback;
            task.Run(this);
        }
        public VideoAssetForm.Data CreateDataByClip(VideoClip clip, string name)
        {
            return new VideoAssetForm.Data(-1, name, "", null, "", clip);
        }
        public VideoAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new VideoAssetForm.Data(-1, name, "", data, "", null);
        }
        public VideoAssetForm.Data CreateDataByPath(string path, string name)
        {
            path = SaveAndLoad.GetRealPath(path);
            return new VideoAssetForm.Data(-1, name, path, null, "", null);
        }

    }
}
