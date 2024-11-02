using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public class Z_MonoController<T>:MonoBehaviour, IZ_Controller<T>
    {
        protected T _manager;
        public virtual void Init(T manager)
        {
            _manager = manager;
        }
    }
}
