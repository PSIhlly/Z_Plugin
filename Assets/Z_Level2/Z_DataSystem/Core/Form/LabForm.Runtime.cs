using System.Collections.Generic;

namespace Z_DataSystem.Form
{
    public static partial class LabForm
    {
        public const int NoneId = 0;

        public static void ClearRuntimeData()
        {
            var ids = new List<int>(DataById.Keys);
            foreach (var id in ids)
            {
                if (id > NoneId && id <= autoIdCnt)
                    RemoveData(id);
            }
        }

        public static bool TryGetData(int labId, out Data data)
        {
            data = null;
            return labId != NoneId && DataById.TryGetValue(labId, out data);
        }

        public static string GetDisplayName(int labId, string separator = "/")
        {
            if (!TryGetData(labId, out var data))
                return string.Empty;

            var names = new[]
            {
                data.lv1Lab ?? string.Empty,
                data.lv2Lab ?? string.Empty,
                data.lv3Lab ?? string.Empty
            };
            var lastLevel = names.Length - 1;
            while (lastLevel >= 0 && names[lastLevel].Length == 0)
                lastLevel--;
            return lastLevel < 0 ? string.Empty : string.Join(separator, names, 0, lastLevel + 1);
        }

        public static int GetOrCreate(string label, string belong)
        {
            return GetOrCreate(label, string.Empty, string.Empty, belong);
        }

        public static int GetOrCreateDisplayName(string displayName, string belong, string separator = "/")
        {
            return GetOrCreateDisplayName(displayName, belong, NoneId, separator);
        }

        public static int GetOrCreateDisplayName(string displayName, string belong, int currentLabId, string separator = "/")
        {
            if (string.IsNullOrWhiteSpace(displayName))
                return NoneId;

            separator = string.IsNullOrEmpty(separator) ? "/" : separator;
            if (TryGetData(currentLabId, out var current) &&
                current.belong == belong &&
                GetDisplayName(currentLabId, separator) == displayName)
            {
                return currentLabId;
            }

            Data displayMatch = null;
            foreach (var data in DataById.Values)
            {
                if (data.belong == belong &&
                    GetDisplayName(data.id, separator) == displayName &&
                    (displayMatch == null || data.id < displayMatch.id))
                {
                    displayMatch = data;
                }
            }
            if (displayMatch != null)
                return displayMatch.id;

            var parts = displayName.Split(new[] { separator }, System.StringSplitOptions.None);
            return GetOrCreate(
                parts[0],
                parts.Length > 1 ? parts[1] : string.Empty,
                parts.Length > 2 ? string.Join(separator, parts, 2, parts.Length - 2) : string.Empty,
                belong);
        }

        public static int GetOrCreateForBelong(int labId, string belong)
        {
            return TryGetData(labId, out var data)
                ? GetOrCreate(data.lv1Lab, data.lv2Lab, data.lv3Lab, belong)
                : NoneId;
        }

        public static int GetOrCreate(string lv1Lab, string lv2Lab, string lv3Lab, string belong)
        {
            lv1Lab ??= string.Empty;
            lv2Lab ??= string.Empty;
            lv3Lab ??= string.Empty;
            belong ??= string.Empty;

            if (lv1Lab.Length == 0 && lv2Lab.Length == 0 && lv3Lab.Length == 0)
                return NoneId;

            Data match = null;
            foreach (var data in DataById.Values)
            {
                if (data.lv1Lab == lv1Lab &&
                    data.lv2Lab == lv2Lab &&
                    data.lv3Lab == lv3Lab &&
                    data.belong == belong &&
                    (match == null || data.id < match.id))
                {
                    match = data;
                }
            }

            if (match != null)
                return match.id;

            var newData = new Data(-1, lv1Lab, lv2Lab, lv3Lab, belong);
            AddData(newData);
            return newData.id;
        }
    }
}
