using System;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;

namespace Z_UnitSystem
{
    public class InstancePoolManager : Z_MonoManager<InstancePoolManager>
    {
        public Dictionary<string, InstancePool> pools;

        [SerializeField]
        private List<GameObject> prefabs;

        public Transform defaultRoot;

        private static InstancePoolManager initializedManager;
        private readonly List<long> viewRefreshScopes = new List<long>();
        private long nextViewRefreshScope;
        private long viewRefreshGeneration;

        public readonly struct ViewRefreshReuseScope : IDisposable
        {
            private readonly InstancePoolManager manager;
            private readonly long generation;
            private readonly long token;

            internal ViewRefreshReuseScope(InstancePoolManager manager, long generation, long token)
            {
                this.manager = manager;
                this.generation = generation;
                this.token = token;
            }

            public void Dispose()
            {
                if (manager != null)
                    manager.EndViewRefreshReuse(generation, token);
            }
        }

        // Unlike the singleton getter, a visibility-only query must not create
        // a manager in an empty scene or during map teardown.
        public static ViewRefreshReuseScope BeginExistingViewRefreshReuse()
        {
            return initializedManager != null
                ? initializedManager.BeginViewRefreshReuse() : default;
        }

        public ViewRefreshReuseScope BeginViewRefreshReuse()
        {
            if (viewRefreshScopes.Count == 0)
                foreach (var pool in pools.Values)
                    pool.BeginViewRefreshReuse();
            long token = ++nextViewRefreshScope;
            viewRefreshScopes.Add(token);
            return new ViewRefreshReuseScope(this, viewRefreshGeneration, token);
        }

        private void EndViewRefreshReuse(long generation, long token)
        {
            if (generation != viewRefreshGeneration || !viewRefreshScopes.Remove(token))
                return;
            if (viewRefreshScopes.Count == 0)
                foreach (var pool in pools.Values)
                    pool.EndViewRefreshReuse();
        }

        public override void Init()
        {
            if (inited)
                return;
            base.Init();
            initializedManager = this;
            int count = prefabs != null ? prefabs.Count : 0;
            pools = new Dictionary<string, InstancePool>(count, StringComparer.Ordinal);
            for (int i = 0; i < count; i++)
            {
                Register(new InstancePool(prefabs[i], defaultRoot));
            }
        }
        public void AddPool(GameObject go)
        {
            Register(new InstancePool(go, defaultRoot));
        }
        public void Register(InstancePool pool)
        {
            pools.Add(pool.prefab.name, pool);
            if (viewRefreshScopes.Count > 0)
                pool.BeginViewRefreshReuse();
        }
        public void Unregister(InstancePool pool)
        {
            pool.EndViewRefreshReuse();
            if (pools.TryGetValue(pool.prefab.name, out var registered) && ReferenceEquals(registered, pool))
                pools.Remove(pool.prefab.name);
        }
        public GameObject GetPrefab(string name)
        {
            return name != null && pools.TryGetValue(name, out var pool) ? pool.prefab : null;
        }
        public GameObject CreateInstance(GameObject tar)
        {
            if (pools.TryGetValue(tar.name, out var pool))
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
            if (pools.TryGetValue(proto.name, out var pool))
            {
                pool.Push(tar);
                return;
            }
            tar.SetActive(false);
        }
        public void Clear()
        {
            // Invalidate outstanding scopes before destroying their pools. An
            // old Dispose cannot end a new refresh started after scene reload.
            viewRefreshGeneration++;
            viewRefreshScopes.Clear();
            if (pools == null)
                return;
            var retiringPools = new List<InstancePool>(pools.Values);
            pools.Clear();
            foreach(var pool in retiringPools)
            {
                pool.Destroy();
            }
        }

        private void OnDestroy()
        {
            Clear();
            if (initializedManager == this)
                initializedManager = null;
        }
    }
}
