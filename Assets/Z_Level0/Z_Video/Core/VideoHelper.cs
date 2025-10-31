using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using UnityEngine;
using UnityEngine.Video;
namespace Z_Audio
{
    public static class VideoHelper
    {
        /*public static VideoClip  GetVideoByByte(byte[] data)
        {
            using (MemoryStream stream = new MemoryStream(data))
        {
            // 2. 加载视频数据到 VideoClip（Unity 2020+ 支持）
            VideoClip clip = VideoClip.CreateFromStream(stream, "InMemoryMP4", (int)stream.Length);

            // 3. 配置 VideoPlayer
            videoPlayer.source = VideoSource.VideoClip;
            videoPlayer.clip = clip;

            // 4. 关联渲染目标
            if (videoScreen != null)
            {
                videoPlayer.targetTexture = new RenderTexture(
                    Screen.width, Screen.height, 24
                );
                videoScreen.texture = videoPlayer.targetTexture;
            }

            // 5. 准备并播放
            videoPlayer.Prepare();
            while (!videoPlayer.isPrepared) yield return null;
            videoPlayer.Play();
        }

        }*/


    }


}