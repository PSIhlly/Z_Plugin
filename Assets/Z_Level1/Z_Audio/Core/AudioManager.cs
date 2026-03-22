using RenderHeads.Media.AVProVideo;
using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Security.Cryptography;
using System.Text;
using System.Threading.Tasks;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Audio
{
    public class AudioManager : Z_MonoManager<AudioManager>
    {
        MediaPlayer bgmPlayer;

        public class MPPool : Z_Pool<MediaPlayer>
        {
            public override void Clear(MediaPlayer obj)
            {
                obj.gameObject.SetActive(false);
            }

            public override void Destroy()
            {
            }

            public override void Fresh(MediaPlayer obj)
            {
                obj.gameObject.SetActive(true);
            }

            public override MediaPlayer New()
            {
                var go = new GameObject("mp");
                go.transform.parent = instance.transform;
                return go.AddComponent<MediaPlayer>();

            }
        }
        private MPPool pool;
        public override void Init()
        {
            pool = new MPPool();
            var go = new GameObject("bgmMp");
            go.transform.parent = instance.transform;
            bgmPlayer = go.AddComponent<MediaPlayer>();
            bgmPlayer.Loop = true;
        }
        public void Clear()
        {
            pool.Clear();
        }
        public void Play(string path)
        {
            var mp = pool.Get();
            mp.Loop = false;
            mp.OpenMedia(new MediaPath(path, MediaPathType.AbsolutePathOrURL), true);
        }
        public void PlayStreaming(string name)
        {
            var mp = pool.Get();
            mp.Loop = false;
            mp.OpenMedia(new MediaPath(name, MediaPathType.RelativeToStreamingAssetsFolder), true);
        }
        public void Bgm(string path)
        {
            if(string.IsNullOrEmpty(path))
            {
                bgmPlayer.Stop();
            }
            else
            {
                bgmPlayer.Play();
                if (bgmPlayer.MediaPath.Path != path)
                    bgmPlayer.OpenMedia(new MediaPath(path, MediaPathType.AbsolutePathOrURL), true);
            }
        }
        public void BgmPause()
        {
            bgmPlayer.Pause();
        }
        public void BgmContinue()
        {
            bgmPlayer.Play();
        }
        public void BgmStreaming(string name)
        {
            if (bgmPlayer.MediaPath.Path != name)
                bgmPlayer.OpenMedia(new MediaPath(name, MediaPathType.RelativeToStreamingAssetsFolder), true);
        }
    }


}