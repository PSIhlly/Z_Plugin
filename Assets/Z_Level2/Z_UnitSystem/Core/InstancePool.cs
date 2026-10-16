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
        private readonly bool[] rendererActiveStates;
        private Transform root;
        private bool viewRefreshReuse;
        private bool destroying;
        private readonly List<GameObject> pendingActive = new List<GameObject>();
        private readonly HashSet<GameObject> returnedObjects = new HashSet<GameObject>();
        private readonly Dictionary<GameObject, ReuseState> reuseStates = new Dictionary<GameObject, ReuseState>();

        private sealed class ReuseState
        {
            public readonly List<Collider> colliders = new List<Collider>();
            public readonly List<bool> colliderEnabled = new List<bool>();
            public readonly List<Renderer> renderers = new List<Renderer>();

            public void SuspendColliders(GameObject obj)
            {
                obj.GetComponentsInChildren(true, colliders);
                colliderEnabled.Clear();
                foreach (var collider in colliders)
                {
                    colliderEnabled.Add(collider.enabled);
                    collider.enabled = false;
                }
            }

            public void RestoreColliders()
            {
                for (int i = 0; i < colliderEnabled.Count; i++)
                    if (colliders[i] != null)
                        colliders[i].enabled = colliderEnabled[i];
                colliderEnabled.Clear();
            }
        }

        internal void BeginViewRefreshReuse()
        {
            viewRefreshReuse = true;
        }

        internal void EndViewRefreshReuse()
        {
            viewRefreshReuse = false;
            while (pendingActive.Count > 0)
            {
                int index = pendingActive.Count - 1;
                var obj = pendingActive[index];
                pendingActive.RemoveAt(index);
                if (obj == null)
                {
                    returnedObjects.Remove(obj);
                    reuseStates.Remove(obj);
                    continue;
                }
                Deactivate(obj);
                pool.Enqueue(obj);
            }
        }

        public override GameObject Get()
        {
            if (destroying)
                throw new InvalidOperationException("Cannot acquire an instance from a destroyed pool.");
            // Use terrain leaving this same view refresh before waking an
            // already-inactive instance from the ordinary FIFO pool.
            while (pendingActive.Count > 0)
            {
                int index = pendingActive.Count - 1;
                var obj = pendingActive[index];
                pendingActive.RemoveAt(index);
                returnedObjects.Remove(obj);
                if (obj == null)
                {
                    reuseStates.Remove(obj);
                    continue;
                }
                activeObjs.Add(obj);
                try
                {
                    // A Show callback may have attached a new script after
                    // return. Give it the ordinary lifecycle before reuse.
                    if (!obj.TryGetComponent<Instance>(out var ins) || !ins.SupportsViewRefreshReuse)
                        Deactivate(obj);
                    reuseStates[obj].RestoreColliders();
                    Fresh(obj);
                    return obj;
                }
                catch
                {
                    activeObjs.Remove(obj);
                    returnedObjects.Add(obj);
                    Deactivate(obj);
                    pool.Enqueue(obj);
                    throw;
                }
            }
            var instance = base.Get();
            returnedObjects.Remove(instance);
            return instance;
        }

        public override void Push(GameObject obj)
        {
            if (obj == null || !returnedObjects.Add(obj))
                return;
            activeObjs.Remove(obj);
            if (destroying)
            {
                Deactivate(obj);
                GameObject.Destroy(obj);
                return;
            }
            try
            {
                if (obj.TryGetComponent<Instance>(out var ins))
                {
                    ins.PrepareForHide();
                    DetachOwner(ins);
                    if (viewRefreshReuse && obj.activeInHierarchy && ins.SupportsViewRefreshReuse)
                    {
                        // No physics query may hit the ownerless instance while
                        // synchronous Show callbacks are assembling the new view.
                        if (!reuseStates.TryGetValue(obj, out var state))
                        {
                            state = new ReuseState();
                            reuseStates.Add(obj, state);
                        }
                        state.SuspendColliders(obj);
                        ins.PrepareForViewRefreshReuse();
                        pendingActive.Add(obj);
                        return;
                    }
                }
                Clear(obj);
            }
            catch
            {
                Deactivate(obj);
                pool.Enqueue(obj);
                throw;
            }
            pool.Enqueue(obj);
        }

        private static void DetachOwner(Instance ins)
        {
            var owner = ins.unit;
            if (owner != null && ReferenceEquals(owner.ins, ins))
                owner.ins = null;
            ins.unit = null;
        }
        public InstancePool(GameObject prefab,Transform root)
        {
            this.prefab = prefab;
            this.root = root;
            var renders = prefab.GetComponentsInChildren<Renderer>(true);
            blocks = new MaterialPropertyBlock[renders.Length];
            rendererActiveStates = new bool[renders.Length];
            for (int i=0;i<renders.Length;i++)
            {
                blocks[i] = new MaterialPropertyBlock();
                renders[i].GetPropertyBlock(blocks[i]);
                rendererActiveStates[i] = renders[i].gameObject.activeSelf;
            }
        }
        public override void Clear(GameObject obj)
        {
            pendingActive.Remove(obj);
            Deactivate(obj);
        }

        private void Deactivate(GameObject obj)
        {
            if (obj == null)
                return;
            if(obj.TryGetComponent<Instance>(out var ins))
            {
                DetachOwner(ins);
            }
            obj.gameObject.SetActive(false);
            if (reuseStates.TryGetValue(obj, out var state))
                state.RestoreColliders();
        }

        public override void Clear()
        {
            bool resumeReuse = viewRefreshReuse;
            EndViewRefreshReuse();
            base.Clear();
            viewRefreshReuse = resumeReuse;
        }
        public override void Destroy()
        {
            destroying = true;
            EndViewRefreshReuse();
            while(pool.Count>0)
            {
                GameObject.Destroy(pool.Dequeue());
            }
            // Deactivation can invoke custom callbacks. Detach the collection
            // before those callbacks can attempt another return to this pool.
            var remaining = new List<GameObject>(activeObjs);
            activeObjs.Clear();
            foreach(var activeObj in remaining)
            {
                returnedObjects.Add(activeObj);
                Deactivate(activeObj);
                GameObject.Destroy(activeObj);
            }
            pool.Clear();
            returnedObjects.Clear();
            reuseStates.Clear();
        }

        public override void Fresh(GameObject obj)
        {
            if (obj.TryGetComponent<Instance>(out var ins))
                ins.PrepareForShow();
            if (reuseStates.TryGetValue(obj, out var state))
            {
                obj.GetComponentsInChildren(true, state.renderers);
                RestoreRendererSlots(state.renderers);
                obj.SetActive(true);
                return;
            }
            var renders = obj.GetComponentsInChildren<Renderer>(true);
            // Runtime-owned decorations may be appended after prefab renderers.
            // They have no prefab material/active-state slot to restore.
            for(int i=0;i< blocks.Length && i< renders.Length;i++)
            {
                renders[i].gameObject.SetActive(rendererActiveStates[i]);
                renders[i].SetPropertyBlock(blocks[i]);
            }
            obj.gameObject.SetActive(true);
        }

        private void RestoreRendererSlots(List<Renderer> renders)
        {
            for (int i = 0; i < blocks.Length && i < renders.Count; i++)
            {
                renders[i].gameObject.SetActive(rendererActiveStates[i]);
                renders[i].SetPropertyBlock(blocks[i]);
            }
        }

        public override GameObject New()
        {
            var go = GameObject.Instantiate(prefab);

            go.transform.SetParent(root);
            return go;
        }


    }
}
