using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
namespace Z_Video
{
    public static class VideoHelper
    {
        public static void PlayVideoByPath(this VideoPlayer player, string path)
        {
            if(path.EndsWith(".mp4"))
            {
                string url = "file://" + path;
                player.source = VideoSource.Url;
                player.url = url;
                player.playOnAwake = false;
                // 准备播放
                player.prepareCompleted += OnPrepareCompleted;
                player.errorReceived += OnErrorReceived;
                player.Prepare();
            }else
            {
                PlayVideoByByte(player, File.ReadAllBytes(path));
            }
            


        }
        public static void PlayVideoByByte(this VideoPlayer player, byte[] bytes)
        {
            var path = Application.temporaryCachePath + $"/{bytes.Length}.mp4";
            if (File.Exists(path))
            {
                File.Delete(path);
            }
            File.WriteAllBytes(path, bytes);
            // 准备播放
            player.PlayVideoByPath(path);
        }
        private static void OnPrepareCompleted(VideoPlayer vp)
        {
            vp.Play();
        }

        private static void OnErrorReceived(VideoPlayer vp, string error)
        {
            Debug.LogError("Err: " + error);
        }

    }


}