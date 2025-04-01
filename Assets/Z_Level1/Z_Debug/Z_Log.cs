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
        public static void Log(int str)
        {
            Log(str.ToString());
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
        public static void Log(Texture2D tex)
        {
            Debug.Log(tex.name+":");
            var t = "";
            for (int i = 0; i < Mathf.Min(100,tex.width); i++)
            {
                for (int j = 0; j < Mathf.Min(100, tex.height); j++)
                {
                    if(tex.GetPixel(i, j).r> tex.GetPixel(i, j).g&& tex.GetPixel(i, j).r > tex.GetPixel(i, j).b)
                        t += "<color=red>" + ((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("#0.0") + "</color> ";
                    else if (tex.GetPixel(i, j).g > tex.GetPixel(i, j).r && tex.GetPixel(i, j).g > tex.GetPixel(i, j).b)
                        t += "<color=green>" + ((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("#0.0") + "</color> ";
                    else if (tex.GetPixel(i, j).b > tex.GetPixel(i, j).r && tex.GetPixel(i, j).b > tex.GetPixel(i, j).g)
                        t += "<color=blue>"+((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("#0.0") + "</color> ";
                    else
                        t +=  ((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("#0.0") + " ";

                }
                t += "\n";
            }
                Debug.Log(t);
            
        }
        public static void LogErr(string str)
        {
            Debug.LogError(str);
        }
    }
}
