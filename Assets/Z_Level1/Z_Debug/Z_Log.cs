using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Debug
{
    public static class Z_Log
    {
        const int maxLength = 5000;
        public static void Log<T>(IEnumerable<T> enumerable)
        {
            int i = 0;
            string res = enumerable.GetType().Name+":\n";
            foreach(var o in enumerable)
            {
                ++i;
                res += i +" : " + o + "\n";
            }
            Log(res);
        }
        public static void Log(string str)
        {
            if(str.Length> maxLength)
            {
                Debug.Log((str.Length/ maxLength+1)+"parts log:");
            }
            for (int p=0;p<str.Length;)
            {
                Debug.Log(str.Substring(p,Mathf.Min(str.Length-p,maxLength)));
                p += maxLength;
            }
        }
        public static void LogErr(string str)
        {
            Debug.LogError(str);
        }
    }
}
