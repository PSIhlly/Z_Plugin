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
        public void AddPool(GameObject go)
        {
            pools.Add(new InstancePool(go, defaultRoot));
        }
        public void Register(InstancePool pool)
        {
            pools.Add(pool);
        }
        public void Unregister(InstancePool pool)
        {
            pools.Remove(pool);
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
                if (tar.name == pool.prefab.name)
                {
                    var poolGo = pool.Get();
                    poolGo.name = tar.name;
                    return poolGo;
                }
            var go = GameObject.Instantiate(tar);
            go.name = tar.name;
            return go;
        }
        public void DeleteInstance(GameObject tar, GameObject proto)
        {
            foreach (var pool in pools)
                if (proto.name == pool.prefab.name)
                {
                    pool.Push(tar);
                    break;
                }
            tar.SetActive(false);
        }
        public void Clear()
        {
            foreach(var pool in pools)
            {
                pool.Destroy();
            }
            pools.Clear();
        }    
    }
}
