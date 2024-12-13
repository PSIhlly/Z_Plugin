using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Fight
{
    public class WeaponInstance : Instance
    {
        public WeaponUnit unit
        {
            set { base.unit = value; }
            get { return (WeaponUnit)base.unit; }
        }
    }
}