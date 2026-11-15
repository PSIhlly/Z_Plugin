using System;
using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

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
        private readonly HashSet<UiParam> removedParameters = new HashSet<UiParam>();
        private Transform poolRoot;
        private int renderId, revision;
        private bool recycling;
        private readonly ScrView viewport;
        private readonly bool sizeContent;
        private bool viewportEnabled, updatingViewport, viewportDirty = true;
        private float crossExtent;
        private int firstVisible = -1, endVisible = -1;
        private Vector2 lastCell, lastContent, lastViewport, lastSpacing, lastPadding;
        private ScrView.Direction lastDirection;
        public bool IsVirtualized => viewport && viewport.cell;
        public int CreatedCount { get; private set; }
        public int ActiveCount => leased.Count;
        public int PooledCount => pool.Count;

        public UiTreeContainer(UiCtrl owner, GameObject template, Func<UiParam, Transform> selectParent = null,
            ScrView viewport = null, bool sizeContent = true)
        {
            this.owner = owner ?? throw new ArgumentNullException(nameof(owner));
            this.template = template ? template : throw new ArgumentNullException(nameof(template));
            this.selectParent = selectParent;
            this.viewport = viewport;
            this.sizeContent = sizeContent;
            defaultParent = template.transform.parent;
            template.SetActive(false);
            if (IsVirtualized)
            {
                // The virtual extent includes unmaterialized rows. A LayoutGroup
                // must not collapse it to just the currently visible children.
                foreach (var layout in defaultParent.GetComponents<LayoutGroup>()) layout.enabled = false;
                foreach (var fitter in defaultParent.GetComponents<ContentSizeFitter>()) fitter.enabled = false;
                if (!sizeContent && defaultParent is RectTransform background && defaultParent.parent == viewport.content)
                {
                    background.anchorMin = Vector2.zero; background.anchorMax = Vector2.one;
                    background.offsetMin = background.offsetMax = Vector2.zero;
                }
                SetViewportEnabled(true);
            }
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
            viewportDirty = true;
        }

        public int GetNowRenderId() => renderId;
        public UiHolder Get(UiParam param) => byParam[param];
        public bool TryGet(UiParam param, out UiHolder holder) => byParam.TryGetValue(param, out holder) && holder;

        public void SetViewportEnabled(bool enabled)
        {
            if (!IsVirtualized || viewportEnabled == enabled) return;
            viewportEnabled = enabled;
            if (enabled)
            {
                viewport.onValueChanged.AddListener(OnScroll);
                Canvas.preWillRenderCanvases += RefreshViewport;
            }
            else
            {
                viewport.onValueChanged.RemoveListener(OnScroll);
                Canvas.preWillRenderCanvases -= RefreshViewport;
            }
        }

        private void OnScroll(Vector2 position) => RefreshViewport();
        public void InvalidateViewport() { viewportDirty = true; RefreshViewport(); }

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
            firstVisible = endVisible = -1;
            crossExtent = 0;
            viewportDirty = true;
        }

        public void DelReal(GameObject subtree)
        {
            if (!subtree || recycling) return;
            revision++;
            releasing.Clear();
            foreach (var node in leased)
                if (node && (node.gameObject == subtree || node.transform.IsChildOf(subtree.transform)))
                    releasing.Add(node);
            removedParameters.Clear();
            foreach (var param in paramLst)
                if (byParam.TryGetValue(param, out var node) && releasing.Contains(node)) removedParameters.Add(param);
            Recycle();
            for (int i = paramLst.Count - 1; i >= 0; i--)
                if (removedParameters.Contains(paramLst[i])) paramLst.RemoveAt(i);
            removedParameters.Clear();
            viewportDirty = true;
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
            if (IsVirtualized) PrepareVirtualNode(holder);
            holder.gameObject.SetActive(true);
            return holder.gameObject;
        }

        public void Refresh()
        {
            if (IsVirtualized) { RefreshViewport(); return; }
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

        private void PrepareVirtualNode(UiHolder holder)
        {
            bool vertical = viewport.dir == ScrView.Direction.Vertical;
            var source = template.GetComponent<ContentSizeFitter>();
            foreach (var fitter in holder.GetComponents<ContentSizeFitter>())
            {
                fitter.verticalFit = vertical ? ContentSizeFitter.FitMode.Unconstrained
                    : source ? source.verticalFit : fitter.verticalFit;
                fitter.horizontalFit = !vertical ? ContentSizeFitter.FitMode.Unconstrained
                    : source ? source.horizontalFit : fitter.horizontalFit;
            }
        }

        public void RefreshViewport()
        {
            if (!IsVirtualized || !viewportEnabled || updatingViewport || !owner.active
                || !viewport.isActiveAndEnabled || !viewport.TryGetTreeViewport(out var visible, out var cell)) return;
            updatingViewport = true;
            try
            {
                bool vertical = viewport.dir == ScrView.Direction.Vertical;
                if (sizeContent)
                {
                    float length = vertical
                        ? viewport.top + paramLst.Count * (cell.y + viewport.ySpacing)
                        : viewport.left + paramLst.Count * (cell.x + viewport.xSpacing);
                    float previousLength = vertical ? viewport.content.rect.height : viewport.content.rect.width;
                    float nextLength = Mathf.Max(vertical ? visible.height : visible.width, length);
                    SetExtent(viewport.content, vertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, nextLength);
                    if (nextLength < previousLength - .001f)
                    {
                        // A structural edit can remove the rows under the old
                        // scroll position. Clamp immediately, before renting rows.
                        viewport.StopMovement();
                        if (vertical) viewport.verticalNormalizedPosition = Mathf.Clamp01(viewport.verticalNormalizedPosition);
                        else viewport.horizontalNormalizedPosition = Mathf.Clamp01(viewport.horizontalNormalizedPosition);
                    }
                    viewport.TryGetTreeViewport(out visible, out cell);
                }
                viewport.GetTreeRange(paramLst.Count, visible, cell, out int first, out int end);
                var contentSize = viewport.content.rect.size;
                var spacing = new Vector2(viewport.xSpacing, viewport.ySpacing);
                var padding = new Vector2(viewport.left, viewport.top);
                if (!viewportDirty && first == firstVisible && end == endVisible && lastCell == cell
                    && lastContent == contentSize && lastViewport == visible.size && lastSpacing == spacing
                    && lastPadding == padding && lastDirection == viewport.dir) return;
                viewportDirty = false;
                firstVisible = first; endVisible = end;
                bool resized = lastDirection != viewport.dir || lastCell != cell;
                if (lastDirection != viewport.dir) crossExtent = 0;
                // Recycle only leaving rows, retaining their logical Params. A
                // nested Unit container is released through the row's pool hook.
                releasing.Clear();
                for (int i = 0; i < paramLst.Count; i++)
                    if ((i < first || i >= end) && byParam.TryGetValue(paramLst[i], out var old) && old)
                        releasing.Add(old);
                if (releasing.Count > 0) Recycle();
                for (renderId = first; renderId < end; renderId++)
                {
                    var param = paramLst[renderId];
                    if (!TryGet(param, out var holder))
                    {
                        holder = AddReal(param).GetComponent<UiHolder>();
                        UiTreeLayout.Rebuild((RectTransform)holder.transform);
                    }
                    else if (resized)
                    {
                        PrepareVirtualNode(holder);
                        UiTreeLayout.Rebuild((RectTransform)holder.transform);
                    }
                    var rect = (RectTransform)holder.transform;
                    var measured = viewport.GetTreeSize(rect);
                    crossExtent = Mathf.Max(crossExtent, vertical ? measured.x : measured.y);
                }
                renderId = 0;
                if (sizeContent)
                    SetExtent(viewport.content, vertical ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical,
                        Mathf.Max(vertical ? visible.width : visible.height,
                            crossExtent + (vertical ? viewport.left : viewport.top)));
                for (int i = first; i < end; i++)
                    if (TryGet(paramLst[i], out var holder)) PositionVirtualNode(holder, i, cell, vertical);
                lastCell = cell; lastContent = viewport.content.rect.size; lastViewport = visible.size;
                lastSpacing = spacing; lastPadding = padding; lastDirection = viewport.dir;
            }
            finally { updatingViewport = false; }
        }

        private void PositionVirtualNode(UiHolder holder, int index, Vector2 cell, bool vertical)
        {
            var rect = (RectTransform)holder.transform;
            float length = vertical ? cell.y : cell.x;
            var measured = viewport.GetTreeSize(rect);
            float scaleX = Mathf.Max(.001f, viewport.content.InverseTransformVector(rect.TransformVector(Vector3.right)).magnitude);
            float scaleY = Mathf.Max(.001f, viewport.content.InverseTransformVector(rect.TransformVector(Vector3.up)).magnitude);
            SetExtent(rect, vertical ? RectTransform.Axis.Vertical : RectTransform.Axis.Horizontal, length / (vertical ? scaleY : scaleX));
            if (!sizeContent)
                SetExtent(rect, vertical ? RectTransform.Axis.Horizontal : RectTransform.Axis.Vertical,
                    vertical ? viewport.content.rect.width / scaleX : viewport.content.rect.height / scaleY);
            measured = viewport.GetTreeSize(rect);
            var position = new Vector3(viewport.content.rect.xMin + (vertical && !sizeContent ? 0 : viewport.left),
                viewport.content.rect.yMax - (!vertical && !sizeContent ? 0 : viewport.top));
            if (vertical)
            {
                position.x += measured.x * rect.pivot.x;
                position.y -= index * (cell.y + viewport.ySpacing) + length * (1 - rect.pivot.y);
            }
            else
            {
                position.x += index * (cell.x + viewport.xSpacing) + length * rect.pivot.x;
                position.y -= measured.y * (1 - rect.pivot.y);
            }
            rect.localPosition = defaultParent.InverseTransformPoint(viewport.content.TransformPoint(position));
        }

        private static void SetExtent(RectTransform rect, RectTransform.Axis axis, float value)
        {
            if (!Mathf.Approximately(axis == RectTransform.Axis.Horizontal ? rect.rect.width : rect.rect.height, value))
                rect.SetSizeWithCurrentAnchors(axis, value);
        }
    }
}
