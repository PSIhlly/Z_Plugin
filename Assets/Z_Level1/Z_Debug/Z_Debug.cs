using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Debug
{
    public static class Z_Debug
    {
        public static void Log<T>(IEnumerable<T> enumerable)
        {
            int i = 0;
            foreach(var o in enumerable)
            {
                ++i;
                Log(i + " : " + o);
            }
        }
        public static void Log(string str)
        {
            Debug.Log(str);
        }
        public static void LogErr(string str)
        {
            Debug.LogError(str);
        }
    }
}
