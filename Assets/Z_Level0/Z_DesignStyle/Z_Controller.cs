using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public interface IZ_Controller
    {
        public void Init(IZ_Manager manager);
    }
    public class Z_Controller<T>: IZ_Controller
    {
        protected IZ_Manager _manager;
        public void Init(IZ_Manager manager)
        {
            _manager = manager;
        }
    }
}
