using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Time;
using Z_UnitSystem;

namespace Z_Map
{
    public class TileInstance : MapInstance
    {
        public TileUnit unit
        {
            set { base.unit = value; }
            get { return (TileUnit)base.unit; }
        }


    }
}
