using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Security.Cryptography;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using UnityEditor.PackageManager.Requests;
using UnityEngine;
using UnityEngine.Networking;
namespace Z_Audio
{
    public static class AudioHelper
    {
        static int handleId;
        public static async Task<AudioClip> GetAudioByByte(byte[] data)
        {
            string tempPath = Path.Combine(Application.temporaryCachePath, $"temp{handleId++}.mp3");
            File.WriteAllBytes(tempPath, data);

            AudioClip clip = await GetAudioByPathAsync(tempPath);
            await Task.Delay(100);
            try
            {
                File.Delete(tempPath);
            }
            catch (Exception ex)
            {
            }
            return clip;
        }
        public static AudioClip GetAudioByPath(string path)
        {
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(
                "file://" + path, // 本地文件需加 file:// 前缀
                AudioType.MPEG // 指定为 MP3 格式
            );
            request.SendWebRequest();
            var time = Time.time;
            while(!request.isDone)
            {
                // 可选：添加超时逻辑（避免无限阻塞）
                if (Time.time > time+5) // 超时5秒
                {
                    request.Abort();
                    throw new TimeoutException("请求超时（5秒）");
                }
            }

            var res = DownloadHandlerAudioClip.GetContent(request);
            return res;
        }

        public static async Task<AudioClip> GetAudioByPathAsync(string path)
        {
            var tcs = new TaskCompletionSource<AudioClip>();
            UnityWebRequest request = UnityWebRequestMultimedia.GetAudioClip(
                "file://" + path, // 本地文件需加 file:// 前缀
                AudioType.MPEG // 指定为 MP3 格式
            );
            AudioClip res = null;
            request.SendWebRequest().completed += _ =>
            {
                if (request.result == UnityWebRequest.Result.Success)
                {
                    tcs.SetResult(DownloadHandlerAudioClip.GetContent(request));
                }
                else
                {
                    Debug.LogError("MP3 load faild：" + request.error);
                    tcs.SetResult(null);
                }
                request.Dispose();
            };
            return await tcs.Task;
        }


        public static string GetSHA1Hash(this AudioClip clip, int sampleStep = 20, bool useSingleChannel = true)
        {
            if (clip == null)
            {
                Debug.LogError("AudioClip 为空");
                return string.Empty;
            }

            // 采样间隔最小为 1（避免无效值）
            sampleStep = Mathf.Max(1, sampleStep);

            try
            {
                int totalSamples = clip.samples; // 单声道总样本数
                int channels = clip.channels;    // 声道数

                // 1. 计算需要采样的样本数（间隔采样）
                int sampledCount = (totalSamples + sampleStep - 1) / sampleStep; // 向上取整
                if (useSingleChannel)
                {
                    // 只取单声道（如左声道），数据量再次减半
                    sampledCount = (sampledCount + sampleStep - 1) / sampleStep;
                }

                // 2. 提取采样数据
                float[] sampledData = new float[sampledCount];
                float[] fullPcmData = new float[totalSamples * channels];
                clip.GetData(fullPcmData, 0); // 先获取完整数据

                int index = 0;
                for (int i = 0; i < totalSamples; i += sampleStep)
                {
                    if (index >= sampledCount) break;

                    // 取指定声道的样本（0 = 左声道，1 = 右声道）
                    int channelIndex = useSingleChannel ? 0 : (i % channels);
                    int dataIndex = i * channels + channelIndex;

                    if (dataIndex < fullPcmData.Length)
                    {
                        sampledData[index] = fullPcmData[dataIndex];
                        index++;
                    }
                }

                // 3. 转换为字节数组并计算哈希
                byte[] byteData = new byte[sampledData.Length * 4];
                Buffer.BlockCopy(sampledData, 0, byteData, 0, byteData.Length);

                using (SHA1 sha1 = SHA1.Create())
                {
                    byte[] hashBytes = sha1.ComputeHash(byteData);
                    StringBuilder sb = new StringBuilder();
                    foreach (byte b in hashBytes)
                    {
                        sb.Append(b.ToString("x2"));
                    }
                    return sb.ToString();
                }
            }
            catch (Exception e)
            {
                Debug.LogError("sha1 hash faild：" + e.Message);
                return string.Empty;
            }
        }
    }


}