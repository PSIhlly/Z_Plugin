using System;
using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_Code.Form;

namespace Z_Code
{
    public class LenCmd : SynchronousQueryCmd
    {
        [RuntimeInitializeOnLoadMethod(RuntimeInitializeLoadType.AfterAssembliesLoaded)]
        static void Init()
        {
            Register(new LenCmd());
        }
        public override string GetName() => "Len";
        protected override BoxDataForm.Data Query(ReadOnlySpan<BoxDataForm.Data> prm)
        {
            return CodeHelper.CreateBoxByNum(GetLength(prm[0].dic));
        }

        // This is scratch space, not a cache of a dictionary's length: same-count
        // key edits must be visible immediately. Commands execute on Unity's thread.
        private static uint[] prefixBits = Array.Empty<uint>();

        internal static int GetLength(Dictionary<string, BoxDataForm.Data> values)
        {
            int count = values?.Count ?? 0;
            if (count == 0 || !values.ContainsKey("0")) return 0;

            // Count distinct canonical indices within [0,count). If every key fits,
            // the dictionary is a dense list, without creating any index strings.
            bool dense = true;
            foreach (var pair in values)
            {
                if (!TryGetIndex(pair.Key, count, out _))
                {
                    dense = false;
                    break;
                }
            }
            if (dense) return count;

            // Named, sparse or noncanonical keys retain Len's first-gap semantics.
            int words = (count >> 5) + ((count & 31) == 0 ? 0 : 1);
            if (prefixBits.Length < words) prefixBits = new uint[words];
            else Array.Clear(prefixBits, 0, words);
            foreach (var pair in values)
                if (TryGetIndex(pair.Key, count, out int index))
                    prefixBits[index >> 5] |= 1u << (index & 31);
            for (int index = 0; index < count; index++)
                if ((prefixBits[index >> 5] & (1u << (index & 31))) == 0)
                    return index;
            return count;
        }

        private static bool TryGetIndex(string key, int count, out int index)
        {
            index = 0;
            if (string.IsNullOrEmpty(key) || key.Length > 10 || key.Length > 1 && key[0] == '0') return false;
            long value = 0;
            foreach (char character in key)
            {
                int digit = character - '0';
                if ((uint)digit > 9) return false;
                value = value * 10 + digit;
                if (value >= count) return false;
            }
            index = (int)value;
            return true;
        }
    }
}
