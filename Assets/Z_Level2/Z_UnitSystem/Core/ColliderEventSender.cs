using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_UnitSystem
{
    public class ColliderEventSender : MonoBehaviour
    {
        Instance _superIns;
        HashSet<Unit> touchList = new HashSet<Unit>();
        public Instance superIns
        {
            get
            {
                if (_superIns == null)
                {
                    _superIns = transform.GetComponentInParent<Instance>();
                }
                return _superIns;
            }
        }
        public void OnTriggerEnter(Collider other)
        {
            if (superIns != null)
            {
                var tar = other.GetComponentInParent<Instance>();
                if (tar!=null&& !touchList.Contains(tar.unit))
                {
                    touchList.Add(tar.unit);
                    superIns.unit.OnEnter(tar.unit);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (superIns != null)
            {
                var tar = other.GetComponentInParent<Instance>();
                if (tar != null&& touchList.Contains(tar.unit))
                {
                    touchList.Remove(tar.unit);
                    superIns.unit.OnExit(tar.unit);
                }
            }
        }
    }
}
