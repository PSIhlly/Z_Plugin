
#if UNITY_ANDROID
using UnityEngine;
using System.Collections;
namespace Z_Os.DllImporter
{
    public class AndroidDllActivityListener : MonoBehaviour//todo: monoSingleton
    {
        private static AndroidDllActivityListener _instance;
        public static AndroidDllActivityListener Instance
        {
            get
            {
                if(_instance==null)
                {
                    var listener=new GameObject("AndroidDllActivityListener");
                    _instance=listener.AddComponent<AndroidDllActivityListener>();
                }
                return _instance;
            }
            private set
            {
                _instance = value;
            }
        }

        public void OnActivityResult(string data)
        {
            string[] res = data.Split("^");
            switch (int.Parse(res[0]))
            {
                case AndroidCallbackParams.LOADFILE_IMAGE:
                    DialogShow.OnActivityResult(res[1]);
                    break;
                case AndroidCallbackParams.LOADFILE_VIDEO:
                    DialogShow.OnActivityResult(res[1]);
                    break;
            }
        }
    }
}
#endif