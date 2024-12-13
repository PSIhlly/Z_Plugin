using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Fight
{
    public class FightInstance : Instance
    {
        public FightUnit unit
        {
            set { base.unit = value; }
            get { return (FightUnit)base.unit; }
        }
    }
}