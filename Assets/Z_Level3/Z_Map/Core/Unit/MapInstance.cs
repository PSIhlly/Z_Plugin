using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Map
{
    public class MapInstance : Instance
    {
        public MapUnit unit
        {
            set { base.unit = value; }
            get { return (MapUnit)base.unit; }
        }

        

    }
}
