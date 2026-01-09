using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_UnitSystem
{
    public class InstancePool : Z_Pool<GameObject>
    {
        public readonly GameObject prefab;
        public MaterialPropertyBlock[] blocks;
        private Transform root;
        public InstancePool(GameObject prefab,Transform root)
        {
            this.prefab = prefab;
            this.root = root;
            var renders = prefab.GetComponentsInChildren<Renderer>();
            blocks = new MaterialPropertyBlock[renders.Length];
            for (int i=0;i<renders.Length;i++)
            {
                blocks[i] = new MaterialPropertyBlock();
                renders[i].GetPropertyBlock(blocks[i]);
            }
        }
        public override void Clear(GameObject obj)
        {
            if(obj.TryGetComponent<Instance>(out var ins))
            {
                ins.unit = null;
            }
            obj.gameObject.SetActive(false);
        }
        public override void Destroy()
        {
            while(pool.Count>0)
            {
                GameObject.Destroy(pool.Dequeue());
            }
            foreach(var activeObj in activeObjs)
            {
                GameObject.Destroy(activeObj);
            }
            activeObjs.Clear();
            pool.Clear();
        }

        public override void Fresh(GameObject obj)
        {
            var renders = obj.GetComponentsInChildren<Renderer>();
            for(int i=0;i< renders.Length;i++)
            {
                renders[i].SetPropertyBlock(blocks[i]);
            }
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
