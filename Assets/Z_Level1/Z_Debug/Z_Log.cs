using System.Collections;
using System.Collections.Generic;
using System.Reflection;
using UnityEngine;
using Z_DesignStyle;
namespace Z_Debug
{
    public static class Z_Log
    {
        const int maxLength = 5000;
        public static void Log(GameObject go)
        {
            string res = "";
            if (go!= null)
            {
                var trs = go.transform;
                while (trs != null)
                {
                    res =  trs.name + $"({trs.GetSiblingIndex()})/" + res;
                    trs = trs.parent;
                }
                
            }
            
            Log(res);
        }
        public static void Log(ICollection col,string[] fields=null)
        {
            int i = 0;
            var type = col.GetType();
            string res = type.Name+":\n";
            foreach(var o in col)
            {
                ++i;
                res += i + " : \n"+ Get(o, fields)+"\n";
            }
            Log(res);
        }
        public static string Get(object obj, string[] fields=null)
        {
            string res = "";
            if (fields != null)
            {
                for (int j = 0; j < fields.Length; j++)
                {
                    string[] addresses = fields[j].Split(".");
                    var tmp = obj;
                    foreach (var ad in addresses)
                    {
                        FieldInfo field = tmp.GetType().GetField(ad);
                        object value = field.GetValue(tmp);  // 获取值
                        tmp = value;
                    }
                   
                    res += fields[j] + ":" + tmp.ToString() + " ;";
                }
            }
            else
            {
                res += obj;
            }
            return res;
        }
        public static void LogTree(object obj,string subName,string[] fields = null)
        {
            var type = obj.GetType();
            string res = type.Name + ":\n" + TreeDfs(obj,subName,fields,0);
            Log(res);
        }
        private static string TreeDfs(object obj, string subName, string[] fields, int depth)
        {
            string res = new string(' ',depth)+ Get(obj,fields) + "\n";
            FieldInfo field = obj.GetType().GetField(subName);
            object value = field.GetValue(obj);  // 获取值
            if(value is ICollection col)
            {
                foreach(var o in col)
                {
                    res+=TreeDfs(o, subName, fields, depth + 1);
                }
            }
            return res;
        }

        public static void Log(float flt)
        {
            Log(flt.ToString());
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
                Debug.Log(Time.frameCount+":"+str.Substring(p,Mathf.Min(str.Length-p,maxLength)));
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
                        t += "<color=red>" + ((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("0.##") + "</color> ";
                    else if (tex.GetPixel(i, j).g > tex.GetPixel(i, j).r && tex.GetPixel(i, j).g > tex.GetPixel(i, j).b)
                        t += "<color=green>" + ((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("0.##") + "</color> ";
                    else if (tex.GetPixel(i, j).b > tex.GetPixel(i, j).r && tex.GetPixel(i, j).b > tex.GetPixel(i, j).g)
                        t += "<color=blue>"+((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("0.##") + "</color> ";
                    else
                        t +=  ((tex.GetPixel(i, j).r + tex.GetPixel(i, j).g + tex.GetPixel(i, j).b) / 3f * 10).ToString("0.##") + " ";

                }
                t += "\n";
            }
                Log(t);
            
        }
        public static void LogErr(string str)
        {
            Debug.LogError(str);
        }
    }
}
