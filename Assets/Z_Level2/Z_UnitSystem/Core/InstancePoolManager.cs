using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_UnitSystem
{
    public class InstancePoolManager : Z_MonoManager<InstancePoolManager>
    {
        public List<InstancePool> pools;

        [SerializeField]
        private List<GameObject> prefabs;

        [SerializeField]
        private List<Material> materials;

        public Transform defaultRoot;

        public override void Init()
        {
            base.Init();
            pools = new List<InstancePool>(prefabs.Count);
            for (int i = 0; i < prefabs.Count; i++)
            {
                pools.Add(new InstancePool(prefabs[i], defaultRoot));
            }
        }

        public void Register(InstancePool pool)
        {
            pools.Add(pool);
        }
        public void Unregister(InstancePool pool)
        {
            pools.Remove(pool);
        }
        public Material GetMaterial(int id)
        {
            return materials[id];
        }
        public GameObject GetPrefab(string name)
        {
            foreach (var pool in pools)
                if (name == pool.prefab.name)
                {
                    return pool.prefab;
                }
            return null;
        }
        public GameObject CreateInstance(GameObject tar)
        {
            foreach (var pool in pools)
                if (tar == pool.prefab)
                {
                    return pool.Get();
                }
            var go = GameObject.Instantiate(tar);
            return go;
        }
        public void DeleteInstance(GameObject tar, GameObject proto)
        {
            foreach (var pool in pools)
                if (proto == pool.prefab)
                {
                    pool.Push(tar);
                    return;
                }

            tar.SetActive(false);
        }
    }
}
