using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public abstract class Z_Singleton<T> where T : Z_Singleton<T>, new()
    {

        private static T _instance;
        private bool inited;
        public static T instance
        {
            get
            {
                if (_instance == null)
                {
                    _instance = new T();
                }
                if(!_instance.inited)
                {
                    _instance.OnInit();
                }
                _instance.inited = true;
                return _instance;
            }

        }
        protected virtual void OnInit()
        {

        }

    }
}