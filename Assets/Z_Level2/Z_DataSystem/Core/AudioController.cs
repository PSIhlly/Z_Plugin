using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Z_ByteSerialize;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
namespace Z_DataSystem.Form
{
    public partial class AudioAssetForm
    {
        public partial class Data
        {
            public void Play(MediaPlayer player, bool loop = false)
            {
                player.OpenMedia(new MediaPath(path, MediaPathType.AbsolutePathOrURL), true);
                player.Loop = loop;
            }
        }
    }
}
namespace Z_DataSystem
{
    public class AudioController : Z_Controller<AssetManager>, IAssetController
    {
        public bool IsAsset(string name)
        {
            var parts = name.Split(GetMark());
            return parts.Length == 3 && string.IsNullOrEmpty(parts[0]) && string.IsNullOrEmpty(parts[2]);
        }
        public virtual string GetName(string name="")
        {
            return $"{GetMark()}{name}{GetMark()}";
        }
        public string GetMark() => AssetDefines.AUDIO_MARK;
        public string[] GetSupportedExtensions() => new[]
{
        ".mp3"
        };

        public AudioController(AssetManager super) : base(super)
        {
        }


        public class SelectAudioTask : SelectTask<AudioController>
        {
            public Action<AudioAssetForm.Data> callback;
            public override void Run(AudioController ctrl)
            {
                base.Run(ctrl);
                NativeGallery.GetAudioFromGallery((path) => OnImportComplete(string.IsNullOrEmpty(path) ? null : File.ReadAllBytes(path)));
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
        public List<string> GetAudioAssetsByFolder(string path, bool isRes)
        {
            List<string> res = new List<string>();
            if (isRes)
            {
                AudioClip[] clips = Resources.LoadAll<AudioClip>(path);
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

        public void Select(Action<AudioAssetForm.Data> callback = null)
        {
            SelectAudioTask task = new SelectAudioTask();
            task.callback = callback;
            task.Run(this);
        }
        public AudioAssetForm.Data CreateDataByClip(AudioClip clip, string name)
        {
            return new AudioAssetForm.Data(-1, name, "", null, "", clip,"");
        }
        public AudioAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new AudioAssetForm.Data(-1, name, "", data, "", null, "");
        }
        public AudioAssetForm.Data CreateDataByPath(string path, string name)
        {
            path = SaveAndLoad.GetRealPath(path);
            return new AudioAssetForm.Data(-1, name, path, null, "", null, "");
        }

    }
}
