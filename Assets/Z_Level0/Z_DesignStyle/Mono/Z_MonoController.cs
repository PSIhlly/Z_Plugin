using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public class Z_MonoController<T>:MonoBehaviour, IZ_Controller
    {
        protected IZ_Manager _manager;
        public virtual void Init(IZ_Manager manager)
        {
            _manager = manager;
        }
    }
}
