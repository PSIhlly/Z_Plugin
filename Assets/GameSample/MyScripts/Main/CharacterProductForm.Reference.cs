using System;
using System.Collections.Generic;
using System.Globalization;
using Z_Map;

namespace Form
{
    public static partial class CharacterProductForm
    {
        public static bool TryGetReferenceUid(string reference, out int uid)
        {
            uid = 0;
            const string mark = "$ch$";
            return !string.IsNullOrEmpty(reference) && reference.Length > mark.Length * 2
                && reference.StartsWith(mark, StringComparison.Ordinal)
                && reference.EndsWith(mark, StringComparison.Ordinal)
                && int.TryParse(reference.Substring(mark.Length, reference.Length - mark.Length * 2),
                    NumberStyles.None, CultureInfo.InvariantCulture, out uid) && uid > 0;
        }

        public static List<Data> ResolveReferences(string reference) =>
            TryGetReferenceUid(reference, out int uid) ? ResolveReferences(uid) : new List<Data>();

        // A concrete Product remains a single target. A template addresses its
        // runtime copies; never accidentally mutate a non-unique Play template.
        public static List<Data> ResolveReferences(int uid)
        {
            var result = new List<Data>();
            if (uid <= 0 || !DataByUid.TryGetValue(uid, out var source) || source == null)
                return result;
            if (source.protoUid != 0)
            {
                result.Add(source);
                return result;
            }
            if (DatasByProtouid.TryGetValue(uid, out var copies))
                foreach (var copy in copies)
                    if (copy != null && DataByUid.TryGetValue(copy.uid, out var registered)
                        && ReferenceEquals(copy, registered))
                        result.Add(copy);
            if (source.unique || (!DynamicGlobalSettings.playing && result.Count == 0))
                result.Add(source);
            result.Sort((left, right) => left.uid.CompareTo(right.uid));
            return result;
        }

        public static Data ResolveReference(string reference) =>
            TryGetReferenceUid(reference, out int uid) ? ResolveReference(uid) : null;

        // Scalar getters retain their existing return shape. Prefer a live scene
        // representative, then a stable Product, and only then the authored data.
        public static Data ResolveReference(int uid)
        {
            if (uid <= 0 || !DataByUid.TryGetValue(uid, out var source) || source == null)
                return null;
            if (source.protoUid != 0)
                return source;
            var scene = PlayManager.instance.sceneCtrl;
            var units = scene?.GetCharacterUnits(uid);
            if (units != null && units.Count > 0
                && DataByUid.TryGetValue(units[0].unit.productInfo.Item1, out var live))
                return live;
            var copies = ResolveReferences(uid);
            return copies.Count > 0 ? copies[0] : source;
        }
    }
}
