using System.Collections.Generic;
using UnityEngine;
using UnityEngine.UI;

namespace Z_Ui.Base
{
    /// <summary>One final sizing transaction for intrinsic-size UI trees.</summary>
    public static class UiTreeLayout
    {
        private sealed class Work
        {
            internal readonly List<RectTransform> rects = new List<RectTransform>();
            internal readonly List<Component> components = new List<Component>();
        }
        private static HashSet<RectTransform> pending = new HashSet<RectTransform>();
        private static HashSet<RectTransform> flushing = new HashSet<RectTransform>();
        private static readonly Stack<Work> workPool = new Stack<Work>();
        private static bool registered, isFlushing;

        public static void Request(RectTransform root)
        {
            if (!root) return;
            pending.Add(root);
            if (registered) return;
            registered = true;
            Canvas.preWillRenderCanvases += Flush;
        }

        public static void Cancel(RectTransform root)
        {
            pending.Remove(root);
            if (pending.Count != 0 || !registered) return;
            Canvas.preWillRenderCanvases -= Flush;
            registered = false;
        }

        private static void Flush()
        {
            if (isFlushing) return;
            isFlushing = true;
            Canvas.preWillRenderCanvases -= Flush;
            registered = false;
            var ready = pending; pending = flushing; flushing = ready;
            try
            {
                foreach (var root in ready)
                {
                    if (!root || !root.gameObject.activeInHierarchy) continue;
                    bool covered = false;
                    foreach (var other in ready)
                        if (other && other != root && other.gameObject.activeInHierarchy && root.IsChildOf(other))
                        { covered = true; break; }
                    if (!covered) Rebuild(root);
                }
            }
            finally { ready.Clear(); isFlushing = false; }
        }

        // These trees size themselves from their contents. Interleaving child
        // measurement and fitting bottom-up lets fixed-child-size parents see
        // final sizes in the same pass, without recursively rebuilding each Rect.
        public static void Rebuild(RectTransform root)
        {
            if (!root || !root.gameObject.activeInHierarchy) return;
            var work = workPool.Count > 0 ? workPool.Pop() : new Work();
            try
            {
                root.GetComponentsInChildren(false, work.rects);
                for (int axis = 0; axis < 2; axis++)
                    for (int i = work.rects.Count - 1; i >= 0; i--)
                    {
                        var rect = work.rects[i];
                        if (!rect || !rect.gameObject.activeInHierarchy) continue;
                        rect.GetComponents(work.components);
                        foreach (var component in work.components)
                            if (IsActive(component) && component is ILayoutElement element)
                            {
                                if (axis == 0) element.CalculateLayoutInputHorizontal();
                                else element.CalculateLayoutInputVertical();
                            }
                        // Self fitting must precede positioning its own children.
                        for (int self = 1; self >= 0; self--)
                            foreach (var component in work.components)
                                if (IsActive(component) && component is ILayoutController controller
                                    && (component is ILayoutSelfController) == (self == 1))
                                {
                                    if (axis == 0) controller.SetLayoutHorizontal();
                                    else controller.SetLayoutVertical();
                                }
                        work.components.Clear();
                    }
            }
            finally { work.rects.Clear(); work.components.Clear(); workPool.Push(work); }
        }

        private static bool IsActive(Component component) => component
            && (!(component is Behaviour behaviour) || behaviour.isActiveAndEnabled);

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void Reset()
        {
            Canvas.preWillRenderCanvases -= Flush;
            registered = isFlushing = false;
            pending.Clear(); flushing.Clear(); workPool.Clear();
        }
    }
}
