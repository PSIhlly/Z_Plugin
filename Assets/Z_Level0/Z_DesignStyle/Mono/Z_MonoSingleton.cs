using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_DesignStyle
{
    public abstract class Z_MonoSingleton<T> : MonoBehaviour
    {
        public static T instance;
        // Start is called before the first frame update
        void Awake()
        {
            if (instance != null)
            {
                Debug.LogError(typeof(T) + "Singleton exist!");
                object obj = instance;
                if(obj is Object Obj)
                Destroy(Obj);
            }

            instance = (T)(object)this;
        }


    }
}
