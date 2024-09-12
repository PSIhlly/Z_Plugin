using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_DesignStyle
{
    public abstract class Z_MonoManager<T> : Z_MonoSingleton<T>, IZ_Manager
    {
        public abstract void Init();
    }
}

