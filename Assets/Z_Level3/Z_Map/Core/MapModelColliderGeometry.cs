using System;
using System.Collections.Generic;
using UnityEngine;

namespace Z_Map
{
    // Immutable primitive recipes survive Sphere-to-Box fitting and pool cloning.
    // They are not facing state: callers supply the current model scale ratio.
    public sealed class MapModelColliderGeometry : MonoBehaviour
    {
        [Serializable]
        private sealed class Entry
        {
            public Collider collider;
            public bool sphere;
            public bool trigger;
            public Vector3 bodySize;
            public Vector3 size;
            public Vector3 center;
            public Vector3 originalScale;
            public Matrix4x4 colliderToPart;
            public Quaternion partRotation;
        }
        [SerializeField] private List<Entry> entries = new List<Entry>();

        public void Add(Collider body, Collider trigger, Vector3 originalScale, Transform part)
        {
            var size = body is BoxCollider box ? box.size : Vector3.one * (((SphereCollider)body).radius * 2f);
            var center = body is BoxCollider bodyBox ? bodyBox.center : ((SphereCollider)body).center;
            var colliderToPart = Matrix4x4.identity;
            for (var current = body.transform; current != part && current != null; current = current.parent)
                colliderToPart = Matrix4x4.TRS(current.localPosition, current.localRotation, current.localScale) * colliderToPart;
            entries.Add(new Entry { collider = body, sphere = body is SphereCollider, bodySize = size,
                size = size, center = center, originalScale = originalScale,
                colliderToPart = colliderToPart, partRotation = part.localRotation });
            entries.Add(new Entry { collider = trigger, sphere = body is SphereCollider, trigger = true,
                bodySize = size, size = trigger is BoxCollider triggerBox ? triggerBox.size
                    : Vector3.one * (((SphereCollider)trigger).radius * 2f),
                center = center, originalScale = originalScale,
                colliderToPart = colliderToPart, partRotation = part.localRotation });
        }

        public void Replace(Collider source, Collider replacement)
        {
            foreach (var entry in entries)
                if (entry.collider == source) entry.collider = replacement;
        }

        public bool TryGet(Collider collider, Vector3 ratio, out Vector3 bodySize, out Vector3 size, out Vector3 center,
            Vector3? modelScale = null)
        {
            foreach (var entry in entries)
            {
                if (entry.collider != collider) continue;
                bodySize = entry.bodySize;
                size = entry.size;
                center = entry.center;
                if (entry.trigger)
                {
                    // Computing the unshrunk primitive scale directly also handles
                    // a pool template with a flat (zero) default-facing axis.
                    Vector3 scaled = modelScale.HasValue
                        ? (Matrix4x4.TRS(Vector3.zero, entry.partRotation, modelScale.Value) * entry.colliderToPart).lossyScale
                        : Vector3.Scale(entry.originalScale, ratio);
                    if (entry.sphere)
                    {
                        size = bodySize + Vector3.one * (.2f * Mathf.Max(Mathf.Abs(scaled.x),
                            Mathf.Abs(scaled.y), Mathf.Abs(scaled.z)));
                    }
                    else if (modelScale.HasValue)
                        size = bodySize + new Vector3(Padding(scaled.x), Padding(scaled.y), Padding(scaled.z));
                    else
                        size = bodySize + new Vector3(Adjust(entry.size.x - bodySize.x, ratio.x),
                            Adjust(entry.size.y - bodySize.y, ratio.y), Adjust(entry.size.z - bodySize.z, ratio.z));
                }
                return true;
            }
            bodySize = size = center = default;
            return false;
        }

        private static float Adjust(float padding, float ratio) => Mathf.Abs(ratio) > .000001f
            ? padding / Mathf.Abs(ratio) : padding;

        internal static float Padding(float scale) => scale != 0f ? Mathf.Min(.1f / Mathf.Abs(scale), float.MaxValue) : 0f;
    }
}
