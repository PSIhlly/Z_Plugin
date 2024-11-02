using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_DesignStyle
{
    public abstract class Z_MonoManager<T> : Z_MonoSingleton<T>, IZ_Manager where T:MonoBehaviour
    {
        public bool inited
        {
            get;protected set;
        }
        public virtual void Init()
        {
            if (inited)
                return;
            inited = true;
        }
        protected override void Awake()
        {
            base.Awake();
            Init();
        }
    }
}

