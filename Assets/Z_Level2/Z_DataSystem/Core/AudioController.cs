using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Runtime.Remoting.Messaging;
using System.Threading.Tasks;
using System.Xml.Linq;
using UnityEngine;
using UnityEngine.UI;
using Z_Audio;
using Z_ByteSerialize;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Os.File;
using Z_Texture;
using Z_Time;
using Z_UnitSystem;
namespace Z_DataSystem.Form
{
    public partial class AudioAssetForm
    {
        public partial class Data
        {
            private AudioClip _clip => (AudioClip)asset;

            Z_MultiTask<AudioClip> clipTask = new Z_MultiTask<AudioClip>();
            public void GetClipAsync(Action<AudioClip> onLoaded)
            {
                clipTask.Run(_clip, GetClip, onLoaded);
            }
            public AudioClip GetClip()
            {
                if (_clip == null)
                {
                    asset = AudioHelper.GetAudioByPath(path).Result;
                }
                return _clip;
            }
        }
    }
    public class AudioController : Z_Controller<AssetManager>, IAssetController
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
                FileImporter.ImportAudioBytes(OnImportComplete);
            }
            public override void OnImportComplete(byte[] data)
            {
                if (data != null)
                {
                    var nm = ctrl.GetMark() + BytesSerialize.GetHash(data) + ctrl.GetMark();
                    var form = ctrl.CreateDataByBytes(data, nm);
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

        public void Select(Vector2Int forceSize = default, Action<AudioAssetForm.Data> callback = null)
        {
            SelectAudioTask task = new SelectAudioTask();
            task.callback = callback;
            task.Run(this);
        }
        public AudioAssetForm.Data CreateDataByClip(AudioClip clip, string name)
        {
            return new AudioAssetForm.Data(-1, name, "", null, "", clip);
        }
        public AudioAssetForm.Data CreateDataByBytes(byte[] data, string name)
        {
            return new AudioAssetForm.Data(-1, name, "", data, "",null);
        }
        public TexAssetForm.Data CreateDataByPath(string path, string name)
        {
            return new TexAssetForm.Data(-1, name, path, null, "", null);
        }

    }
}
