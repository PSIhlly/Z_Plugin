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
            string url = "file://" + path;
            player.source = VideoSource.Url;
            player.url = url;
            player.playOnAwake = false;

            // 准备播放
            player.Prepare();
            player.prepareCompleted += OnPrepareCompleted;
            player.errorReceived += OnErrorReceived;

        }
        private static void OnPrepareCompleted(VideoPlayer vp)
        {
            Debug.Log("Play!");
            vp.Play();
        }

        private static void OnErrorReceived(VideoPlayer vp, string error)
        {
            Debug.LogError("Err: " + error);
        }

    }


}