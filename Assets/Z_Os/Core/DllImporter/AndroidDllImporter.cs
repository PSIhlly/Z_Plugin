#if UNITY_ANDROID
using System;
using System.Collections;
using System.Collections.Generic;
using System.Threading.Tasks;
using UnityEngine;

namespace Z_Os.DllImporter
{
    public class AndroidCallbackParams
    {
        public const int LOADFILE_IMAGE = 11;
        public const int LOADFILE_VIDEO = 12;
    }


    public class FileOpenDialog
    {
        public string type;
        public string title;
    }
    public class DialogShow
    {
        static Action<byte[]> onComplete;

        public static void GetOpenFileName(FileOpenDialog dialog,Action<byte[]> onComplete)
        {
            DialogShow.onComplete = onComplete;
            var listener = AndroidDllActivityListener.Instance;

            AndroidJavaClass jc = new AndroidJavaClass("com.unity3d.player.UnityPlayer");
            AndroidJavaObject current = jc.GetStatic<AndroidJavaObject>("currentActivity");
            AndroidJavaObject jo = new AndroidJavaObject("com.z.z_androidos.AndroidEntryFeature");
            switch (dialog.type)
            {
                case "image":
                    jo.Call("LoadImage", current, AndroidCallbackParams.LOADFILE_IMAGE);
                    break;
                case "video":
                    jo.Call("LoadVideo", current, AndroidCallbackParams.LOADFILE_VIDEO);
                    break;
            }
        }

       

        // 处理从Android设备返回的结果
        public static void OnActivityResult(string path)
        {
            if (System.IO.File.Exists(path))
                onComplete?.Invoke(System.IO.File.ReadAllBytes(path));
            else onComplete?.Invoke(null);

        }
    }
        
}
#endif