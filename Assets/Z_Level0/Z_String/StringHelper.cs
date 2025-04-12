using System.Collections;
using System.Collections.Generic;
using UnityEngine;
namespace Z_String
{
    public static class StringHelper
    {
        public static string FirstToLower(this string s)
        {
            return char.ToLower(s[0]) + s.Substring(1);
        }
    }
}
