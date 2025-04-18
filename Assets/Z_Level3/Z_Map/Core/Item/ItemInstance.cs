using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Map
{
    
    public class ItemInstance : Instance
    {
        public ObjectUnit unit
        {
            set { base.unit = value; }
            get { return (ObjectUnit)base.unit; }
        }

    }
}