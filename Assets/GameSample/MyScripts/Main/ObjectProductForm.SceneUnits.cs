using System;
using System.Collections.Generic;
using UnityEngine;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

namespace Form
{
    public static partial class ObjectProductForm
    {
        // Concrete Product UID -> live scene placements, ordered by Unit UID.
        private static readonly Dictionary<int, List<ObjectUnitForm.Data>> sceneUnitsByProduct =
            new Dictionary<int, List<ObjectUnitForm.Data>>();
        private static readonly Dictionary<ObjectUnitForm.Data, int> indexedSceneProducts =
            new Dictionary<ObjectUnitForm.Data, int>();
        private static readonly Dictionary<ObjectUnitForm.Data, string> pendingSceneUnits =
            new Dictionary<ObjectUnitForm.Data, string>();
        private static readonly List<ObjectUnitForm.Data> completedSceneUnits = new List<ObjectUnitForm.Data>();
        private static readonly IComparer<ObjectUnitForm.Data> sceneUnitOrder =
            Comparer<ObjectUnitForm.Data>.Create((left, right) => left.uid.CompareTo(right.uid));
        private static Dictionary<int, ObjectUnitForm.Data> indexedSceneRows;
        private static int indexedSceneRowCount;
        private static bool sceneUnitIndexReady;
        private static bool sceneUnitIndexTracking;

        private static void TrackSceneUnitChanges()
        {
            if (sceneUnitIndexTracking) return;
            ObjectUnitForm.addAction += NotifySceneUnitBindingChanged;
            ObjectUnitForm.removeAction += RemoveIndexedSceneUnit;
            UnitForm.changeExtraAction += OnSceneUnitExtraChanged;
            sceneUnitIndexTracking = true;
        }

        private static void OnSceneUnitExtraChanged(UnitForm.Data row, string oldValue, string newValue)
        {
            if (oldValue != newValue && sceneUnitIndexReady && row is ObjectUnitForm.Data unit)
                pendingSceneUnits[unit] = newValue;
        }

        internal static void NotifySceneUnitBindingChanged(ObjectUnitForm.Data unit)
        {
            if (!sceneUnitIndexReady) return;
            pendingSceneUnits[unit] = unit.extra;
            indexedSceneRowCount = ObjectUnitForm.DataByUid.Count;
        }

        private static bool IsLiveSceneObject(ObjectUnitForm.Data unit) => unit != null
            && ObjectUnitForm.DataByUid.TryGetValue(unit.uid, out var current) && ReferenceEquals(current, unit)
            && UnitForm.DataByUid.TryGetValue(unit.uid, out var registered) && ReferenceEquals(registered, unit)
            && unit.unit != null && ReferenceEquals(unit.unit.data, unit);

        private static void RemoveIndexedSceneUnit(ObjectUnitForm.Data unit)
        {
            pendingSceneUnits.Remove(unit);
            if (indexedSceneProducts.TryGetValue(unit, out int uid))
            {
                indexedSceneProducts.Remove(unit);
                if (sceneUnitsByProduct.TryGetValue(uid, out var group))
                {
                    group.Remove(unit);
                    if (group.Count == 0) sceneUnitsByProduct.Remove(uid);
                }
            }
            indexedSceneRowCount = ObjectUnitForm.DataByUid.Count;
        }

        private static void RefreshIndexedSceneUnit(ObjectUnitForm.Data unit)
        {
            pendingSceneUnits.Remove(unit);
            if (!IsLiveSceneObject(unit))
            {
                RemoveIndexedSceneUnit(unit);
                return;
            }
            int uid = unit.unit.productInfo.Item1;
            if (uid > 0 && indexedSceneProducts.TryGetValue(unit, out int previous) && previous == uid) return;
            RemoveIndexedSceneUnit(unit);
            if (uid <= 0) return;
            if (!sceneUnitsByProduct.TryGetValue(uid, out var group))
                sceneUnitsByProduct.Add(uid, group = new List<ObjectUnitForm.Data>());
            int index = group.BinarySearch(unit, sceneUnitOrder);
            group.Insert(index < 0 ? ~index : index, unit);
            indexedSceneProducts[unit] = uid;
        }

        private static void EnsureSceneUnitIndex()
        {
            TrackSceneUnitChanges();
            var rows = ObjectUnitForm.DataByUid;
            if (!sceneUnitIndexReady || !ReferenceEquals(rows, indexedSceneRows) || indexedSceneRowCount != rows.Count)
            {
                ResetSceneUnits();
                foreach (var unit in rows.Values) RefreshIndexedSceneUnit(unit);
                indexedSceneRows = rows;
                indexedSceneRowCount = rows.Count;
                sceneUnitIndexReady = true;
                return;
            }
            // Extra's generated notification runs before its backing field is set.
            // Defer that one Unit until the announced string is actually installed.
            completedSceneUnits.Clear();
            foreach (var pair in pendingSceneUnits)
                if (!IsLiveSceneObject(pair.Key) || ReferenceEquals(pair.Key.extra, pair.Value))
                    completedSceneUnits.Add(pair.Key);
            foreach (var unit in completedSceneUnits) RefreshIndexedSceneUnit(unit);
            completedSceneUnits.Clear();
        }

        public static void ResetSceneUnits()
        {
            sceneUnitsByProduct.Clear();
            indexedSceneProducts.Clear();
            pendingSceneUnits.Clear();
            completedSceneUnits.Clear();
            indexedSceneRows = null;
            indexedSceneRowCount = 0;
            sceneUnitIndexReady = false;
        }

        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.SubsystemRegistration)]
        private static void ResetSceneUnitIndexTracking()
        {
            ObjectUnitForm.addAction -= NotifySceneUnitBindingChanged;
            ObjectUnitForm.removeAction -= RemoveIndexedSceneUnit;
            UnitForm.changeExtraAction -= OnSceneUnitExtraChanged;
            sceneUnitIndexTracking = false;
            ResetSceneUnits();
        }

        private static ObjectUnitForm.Data FirstIndexedSceneUnit(int productUid)
        {
            if (sceneUnitsByProduct.TryGetValue(productUid, out var group))
                foreach (var unit in group)
                    if (IsLiveSceneObject(unit)) return unit;
            return null;
        }

        public static ObjectUnitForm.Data GetSceneUnit(int productUid)
        {
            if (!DataByUid.TryGetValue(productUid, out var source)) return null;
            EnsureSceneUnitIndex();
            var result = FirstIndexedSceneUnit(productUid);
            if (source.protoUid == 0 && DatasByProtouid.TryGetValue(productUid, out var copies))
                foreach (var copy in copies)
                {
                    if (!DataByUid.TryGetValue(copy.uid, out var current) || !ReferenceEquals(copy, current)) continue;
                    var unit = FirstIndexedSceneUnit(copy.uid);
                    if (unit != null && (result == null || unit.uid < result.uid)) result = unit;
                }
            return result;
        }

        private static void AddIndexedSceneUnits(int productUid, List<MapUnit> result)
        {
            if (sceneUnitsByProduct.TryGetValue(productUid, out var group))
                foreach (var unit in group)
                    if (IsLiveSceneObject(unit)) result.Add(unit.unit);
        }

        public static void CollectSceneUnits(int productUid, List<MapUnit> result)
        {
            if (!DataByUid.TryGetValue(productUid, out var source)) return;
            EnsureSceneUnitIndex();
            AddIndexedSceneUnits(productUid, result);
            if (source.protoUid == 0 && DatasByProtouid.TryGetValue(productUid, out var copies))
                foreach (var copy in copies)
                    if (DataByUid.TryGetValue(copy.uid, out var current) && ReferenceEquals(copy, current))
                        AddIndexedSceneUnits(copy.uid, result);
            result.Sort((left, right) => left.data.uid.CompareTo(right.data.uid));
        }
    }
}
