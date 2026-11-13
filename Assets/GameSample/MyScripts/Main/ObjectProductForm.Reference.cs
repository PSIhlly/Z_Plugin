using System.Collections.Generic;
using Z_Map;

namespace Form
{
    public static partial class ObjectProductForm
    {
        public static int GetTemplateUid(Data product) => product == null ? 0
            : product.protoUid == 0 ? product.uid : product.protoUid;

        public static List<Data> ResolveReferences(int uid)
        {
            var result = new List<Data>();
            if (!DataByUid.TryGetValue(uid, out var source)) return result;
            if (source.protoUid != 0) result.Add(source);
            else if (DatasByProtouid.TryGetValue(uid, out var copies))
                foreach (var copy in copies)
                    if (DataByUid.TryGetValue(copy.uid, out var current) && ReferenceEquals(copy, current)) result.Add(copy);
            if (source.protoUid == 0 && !DynamicGlobalSettings.playing && result.Count == 0) result.Add(source);
            result.Sort((left, right) => left.uid.CompareTo(right.uid));
            return result;
        }

        public static Data ResolveReference(int uid)
        {
            if (!DataByUid.TryGetValue(uid, out var source)) return null;
            if (source.protoUid != 0) return source;
            var unit = GetSceneUnit(uid);
            if (unit != null) return unit.unit.product;
            Data first = null;
            if (DatasByProtouid.TryGetValue(uid, out var copies))
                foreach (var copy in copies)
                    if (DataByUid.TryGetValue(copy.uid, out var current) && ReferenceEquals(copy, current)
                        && (first == null || copy.uid < first.uid)) first = copy;
            return first ?? source;
        }
    }
}
