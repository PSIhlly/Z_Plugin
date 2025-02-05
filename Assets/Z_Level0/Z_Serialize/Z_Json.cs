using System.Collections;
using System.Collections.Generic;
using UnityEngine;
using Newtonsoft.Json;
using Newtonsoft.Json.Linq;

namespace Z_ByteSerialize
{
    public static class Z_Json
    {
        public static void Set<T>(this JObject jo, string key, T value)
        {
            
            if (value is Vector3 v3)
            {
                jo[key] = v3.x + "|" + v3.y + "|" + v3.z;
            }
            else if(value is Vector3Int v3i)
            {
                jo[key] = v3i.x + "|" + v3i.y + "|" + v3i.z;
            }
            else
            {
                jo[key] = JToken.FromObject(value);
            }
        }
        public static T Get<T>(this JObject jo, string key)
        {
            var tp = typeof(T);
            if (tp == typeof(Vector3))
            {
                string[] str = jo.Get<string>(key).Split("|");
                return (T)(object)new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
            } else
            if (tp == typeof(Vector3Int))
            {
                string[] str = jo.Get<string>(key).Split("|");
                return (T)(object)new Vector3Int(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]));
            }
            else
            {
                return (T)(object)jo[key].ToObject<T>();
            }
        }
    }
}