using System;
using System.Collections;
using System.Collections.Generic;
using System.Text;
using UnityEngine;
namespace Z_String
{
    public static class StringHelper
    {
        public static string FirstToLower(this string s)
        {
            return char.ToLower(s[0]) + s.Substring(1);
        }
        public static float ToFloat(string str,float defaultV)
        {
            if (float.TryParse(str, out var f))
            {
                return f;
            }
            return defaultV;
        }
        public static int ToInt(string str, int defaultV)
        {
            if (int.TryParse(str, out var i))
            {
                return i;
            }
            return defaultV;
        }
        public static bool IsUniqueName(ICollection col, string name)
        {
            foreach (var c in col)
            {
                if ((string)c == name)
                {
                    return false;
                }
            }
            return true;
        }
        public static string GetUniqueName(ICollection col)
        {
            int cur=0;
            foreach (var c in col)
            {
                var nms = ((string)c).Split("New");
                if (nms.Length > 1)
                {
                    cur = ToInt(nms[1], 0) + 1;
                }
            }
            return "New"+cur;
        }
        public static string RemoveMultiLine(string str)
        {
            string[] strs = str.Split("\n");
            string res = "";
            for (int i = 0; i < strs.Length; i++)
            {
                bool unique = true;
                for (int j = i + 1; j < strs.Length; j++)
                {
                    if (strs[j] == strs[i])
                    {

                        unique = false; break;
                    }
                }
                if(unique)
                {
                    res += strs[i] + "\n";
                }
            }
            return res;
        }

        public static string Repeat(this string str,int times)
        {
            StringBuilder sb = new StringBuilder();
            for(int i= 0; i < times; i++)
            {
                sb.Append(str);
            }
            return sb.ToString();
        }

            }
}
