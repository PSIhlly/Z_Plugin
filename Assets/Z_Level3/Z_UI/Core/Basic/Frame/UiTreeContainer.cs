using System;
using System.Collections.Generic;
using UnityEngine;

namespace Z_Ui.Base
{
    public interface IUiTreePoolItem
    {
        // Release nested containers and references only on pool return, not on Hide.
        void OnTreePoolRelease();
    }

    /// <summary>
    /// A container whose leased nodes may be reparented beneath one another.
    /// Recycling flattens every node into an inactive staging root before reuse.
    /// Parameters are bound and the destination parent is restored before activation.
    /// </summary>
    public sealed class UiTreeContainer<T> where T : UiCtrl, new()
    {
        public readonly List<UiParam> paramLst = new List<UiParam>();
        private readonly UiCtrl owner;
        private readonly GameObject template;
        private readonly Transform defaultParent;
        private readonly Func<UiParam, Transform> selectParent;
        private readonly Queue<UiHolder> pool = new Queue<UiHolder>();
        private readonly List<UiHolder> leased = new List<UiHolder>();
        private readonly Dictionary<UiParam, UiHolder> byParam = new Dictionary<UiParam, UiHolder>();
        private readonly List<UiHolder> releasing = new List<UiHolder>();
        private readonly HashSet<UiHolder> releaseSet = new HashSet<UiHolder>();
        private Transform poolRoot;
        private int renderId, revision;
        private bool recycling;
        public int CreatedCount { get; private set; }
        public int ActiveCount => leased.Count;
        public int PooledCount => pool.Count;

        public UiTreeContainer(UiCtrl owner, GameObject template, Func<UiParam, Transform> selectParent = null)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.template = template ? template : throw new ArgumentNullException(nameof(template));
            this.selectParent = selectParent;
            defaultParent = template.transform.parent;
            template.SetActive(false);
        }

        private Transform PoolRoot
        {
            get
            {
                if (!poolRoot)
                {
                    var go = new GameObject("UiTreePool-" + typeof(T).Name, typeof(RectTransform));
                    go.SetActive(false);
                    go.transform.SetParent(owner.gameObject.transform, false);
                    poolRoot = go.transform;
                }
                return poolRoot;
            }
        }

        public void Add(UiParam param, int id = -1)
        {
            if (param == null) throw new ArgumentNullException(nameof(param));
            if (id < 0) paramLst.Add(param);
            else paramLst.Insert(id, param);
        }

        public int GetNowRenderId() => renderId;
        public UiHolder Get(UiParam param) => byParam[param];

        public void Clear()
        {
            if (recycling) return;
            revision++;
            releasing.Clear();
            releasing.AddRange(leased);
            Recycle();
            paramLst.Clear();
            byParam.Clear();
            renderId = 0;
        }

        public void DelReal(GameObject subtree)
        {
            if (!subtree || recycling) return;
            revision++;
            releasing.Clear();
            foreach (var node in leased)
                if (node && (node.gameObject == subtree || node.transform.IsChildOf(subtree.transform)))
                    releasing.Add(node);
            Recycle();
            for (int i = paramLst.Count - 1; i >= 0; i--)
                if (!byParam.ContainsKey(paramLst[i])) paramLst.RemoveAt(i);
        }

        private void Recycle()
        {
            recycling = true;
            try
            {
                releaseSet.UnionWith(releasing);
                // Hide first, then flatten the complete captured set. No node
                // can be rented while still attached to another pooled node.
                foreach (var node in releasing)
                    if (node) node.gameObject.SetActive(false);
                foreach (var node in releasing)
                    if (node) node.transform.SetParent(PoolRoot, false);
                foreach (var node in releasing)
                {
                    if (!node) continue;
                    if (node.ctrl is IUiTreePoolItem item) item.OnTreePoolRelease();
                    node.ctrl.SetParam(null);
                    node.parent = null;
                    pool.Enqueue(node);
                }
                leased.RemoveAll(IsReleasing);
                owner.uiHolder.subUiHolderLst.RemoveAll(IsReleasing);
                // DelReal can remove a subtree without clearing the remaining tree.
                for (int i = 0; i < paramLst.Count; i++)
                    if (byParam.TryGetValue(paramLst[i], out var node) && releaseSet.Contains(node))
                        byParam.Remove(paramLst[i]);
            }
            finally { releasing.Clear(); releaseSet.Clear(); recycling = false; }
        }

        private bool IsReleasing(UiHolder holder) => releaseSet.Contains(holder);

        public GameObject AddReal(UiParam param)
        {
            if (recycling) throw new InvalidOperationException("Cannot render while recycling a UI tree.");
            UiHolder holder = null;
            while (pool.Count > 0 && !holder) holder = pool.Dequeue();
            if (!holder)
            {
                var go = UnityEngine.Object.Instantiate(template, PoolRoot);
                go.SetActive(false);
                holder = go.GetComponent<UiHolder>();
                if (!holder) throw new InvalidOperationException("A UI tree template requires UiHolder.");
                holder.SubUiBind(new T());
                CreatedCount++;
            }
            Transform destination = selectParent?.Invoke(param) ?? defaultParent;
            if (!destination) throw new InvalidOperationException("A UI tree node requires a live destination parent.");
            holder.transform.SetParent(destination, false);
            if (holder.transform is RectTransform rect && template.transform is RectTransform source)
            {
                rect.anchorMin = source.anchorMin; rect.anchorMax = source.anchorMax;
                rect.pivot = source.pivot; rect.sizeDelta = source.sizeDelta;
                rect.anchoredPosition3D = source.anchoredPosition3D;
            }
            holder.transform.localScale = template.transform.localScale;
            holder.transform.localRotation = template.transform.localRotation;
            holder.transform.SetAsLastSibling();
            holder.parent = owner.uiHolder;
            owner.uiHolder.subUiHolderLst.Add(holder);
            holder.ctrl.SetParam(param);
            leased.Add(holder);
            byParam[param] = holder;
            holder.gameObject.SetActive(true);
            return holder.gameObject;
        }

        public void Refresh()
        {
            int current = ++revision;
            for (renderId = 0; renderId < paramLst.Count; renderId++)
            {
                var param = paramLst[renderId];
                if (byParam.TryGetValue(param, out var existing) && existing) continue;
                AddReal(param);
                if (current != revision) return;
            }
            renderId = 0;
        }
    }
}
