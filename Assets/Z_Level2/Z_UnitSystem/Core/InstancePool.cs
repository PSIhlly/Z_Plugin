using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_UnitSystem
{
    public class InstancePool : Z_Pool<GameObject>
    {
        public readonly GameObject prefab;
        private Transform root;
        public InstancePool(GameObject prefab,Transform root)
        {
            this.prefab = prefab;
            this.root = root;
        }
        public override void Clear(GameObject obj)
        {
            obj.GetComponent<Instance>().unit = null;
            obj.gameObject.SetActive(false);
        }
        public override void Destroy()
        {
            while(pool.Count>0)
            {
                GameObject.Destroy(pool.Dequeue());
            }
            pool.Clear();
        }

        public override void Fresh(GameObject obj)
        {

            obj.gameObject.SetActive(true);
        }

        public override GameObject New()
        {
            var go = GameObject.Instantiate(prefab);

            go.transform.SetParent(root);
            return go;
        }


    }
}
