using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_UnitSystem;

namespace Z_Fight
{
    public class BulletInstance : Instance
    {
        public BulletUnit unit
        {
            set { base.unit = value; }
            get { return (BulletUnit)base.unit; }
        }
        public void OnTriggerEnter(Collider other)
        {
            if(other.attachedRigidbody)
            {
                unit.TryBurst(other.attachedRigidbody.transform);
            }else
            {
                unit.TryBurst(other.transform);
            }
        }
    }
}
