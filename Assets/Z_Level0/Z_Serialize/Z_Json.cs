using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;
using System;

namespace Z_ByteSerialize
{
    public static class Z_Json
    {
        public static void Set<T>(this JObject jo, string key, T value)
        {
            if(value==null)
            {
                jo[key] = null;
            }
            else if (value is Vector3 v3)
            {
                jo[key] = v3.x + "|" + v3.y + "|" + v3.z;
            }
            else if(value is Vector3Int v3i)
            {
                jo[key] = v3i.x + "|" + v3i.y + "|" + v3i.z;
            }
            else if (value is List<Vector3> v3lst)
            {
                JArray ja = new JArray();
                jo[key] = JToken.FromObject(ja);
                foreach(var v in v3lst)
                {
                    ja.Add(v.x + "|" + v.y + "|" + v.z);
                }
            }
            else if (value is List<Vector3Int> v3ilst)
            {
                JArray ja = new JArray();
                jo[key] = JToken.FromObject(ja);
                foreach (var v in v3ilst)
                {
                    ja.Add(v.x + "|" + v.y + "|" + v.z);
                }
            }
            else
            {
                jo[key] = JToken.FromObject(value);
            }
        }
        public static T Get<T>(this JObject jo, string key)
        {
            var tp = typeof(T);
            try {
            if (tp == typeof(Vector3))
            {
                string[] str = jo.Get<string>(key).Split("|");
                return (T)(object)new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
            } 
            else if (tp == typeof(Vector3Int))
            {
                string[] str = jo.Get<string>(key).Split("|");
                return (T)(object)new Vector3Int(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]));
            }
            else if (tp == typeof(List<Vector3>))
            {
                JArray ja = (JArray)jo[key];
                List<Vector3> v3lst = new List<Vector3>();
                foreach (var v in ja)
                {
                    var str = ((string)v).Split("|");
                    v3lst.Add(new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2])));
                }
                return (T)(object)v3lst;
            }
            else if (tp == typeof(List<Vector3Int>))
            {
                JArray ja = (JArray)jo[key];
                List<Vector3Int> v3lst = new List<Vector3Int>();
                foreach (var v in ja)
                {
                    var str = ((string)v).Split("|");
                    v3lst.Add(new Vector3Int(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2])));
                }
                return (T)(object)v3lst;
            }
            else
            {
                return (T)(object)jo[key].ToObject<T>();
                }
            }
            catch(Exception e)
            {
                return (T)(object)null;
            }
        }
    }
}