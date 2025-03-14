using System.Collections;
using System.Collections.Generic;
using UnityEngine;

namespace Z_DesignStyle
{
    public interface IZ_Controller<Super>
    {
        public void Init(Super super);
    }
    public class Z_Controller<Super> : IZ_Controller<Super>
    {
        protected Super _super;
        public Z_Controller(Super super)
        {
            Init(super);
        }

        public void Init(Super super)
        {

            _super = super;
        }
    }
}
