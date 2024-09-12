using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public interface IZ_Manager
    {
        public void Init();
    }


    public abstract class Z_Manager<T> : Z_Singleton<T>, IZ_Manager where T : class, new()
    {
        // Start is called before the first frame update
        public abstract void Init();
    }
}