using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_UnitSystem
{
    public class ColliderEventSender : MonoBehaviour
    {
        Instance _superIns;
        HashSet<Instance> touchList = new HashSet<Instance>();
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
                if (tar!=null&& !touchList.Contains(tar))
                {
                    touchList.Add(tar);
                    superIns.OnInstanceEnter(tar);
                }
            }
        }

        public void OnTriggerExit(Collider other)
        {
            if (superIns != null)
            {
                var tar = other.GetComponentInParent<Instance>();
                if (tar != null&& touchList.Contains(tar))
                {
                    touchList.Remove(tar);
                    superIns.OnInstanceExit(tar);
                }
            }
        }
    }
}
