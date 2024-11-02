using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_Map
{
    public class MapInstancePool : Z_Pool<GameObject>
    {
        public readonly GameObject prefab;
        private Transform root;
        public MapInstancePool(GameObject prefab,Transform root)
        {
            this.prefab = prefab;
        }
        public override void Clear(GameObject obj)
        {
            obj.GetComponent<MapInstance>().unit = null;
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
