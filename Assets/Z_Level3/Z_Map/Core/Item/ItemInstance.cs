using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Map
{
    public class ItemInstance : Instance
    {
        public ItemUnit unit
        {
            set { base.unit = value; }
            get { return (ItemUnit)base.unit; }
        }
    }
}