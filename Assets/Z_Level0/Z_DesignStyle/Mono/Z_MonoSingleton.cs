using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_DesignStyle
{
    public abstract class Z_MonoSingleton<T> : MonoBehaviour where T:MonoBehaviour
    {
        private static T _instance;
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    var listener = new GameObject("ClientCore");
                    _instance = (T)listener.AddComponent(typeof(T));
                }
                return _instance;
            }
        }

        // Start is called before the first frame update
        void Awake()
        {
            if (_instance != null&& _instance != (T)(object)this)
            {
                Debug.LogError(typeof(T) + "Singleton exist!");
                object obj = instance;
                if(obj is Object Obj)
                Destroy(Obj);
            }

            _instance = (T)(object)this;
        }


    }
}
