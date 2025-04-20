using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Map
{
    public class CharacterInstance : MapInstance
    {
        public CharacterUnit unit
        {
            set { base.unit = value; }
            get { return (CharacterUnit)base.unit; }
        }
        public Vector3 step;
    }
}