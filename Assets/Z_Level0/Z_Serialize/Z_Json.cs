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
                JObject subJo = new JObject();
                jo[key] = subJo;
                subJo["x"] = v3.x;
                subJo["y"] = v3.y;
                subJo["z"] = v3.z;
            }
            else if(value is Vector3Int v3i)
            {
                JObject subJo = new JObject();
                jo[key] = subJo;
                subJo["x"] = v3i.x;
                subJo["y"] = v3i.y;
                subJo["z"] = v3i.z;
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
                JObject subJo = jo.Get<JObject>(key);
                float x = subJo.Value<float>("x");
                float y = subJo.Value<float>("y");
                float z = subJo.Value<float>("z");
                return (T)(object)new Vector3(x, y, z);
            } else
            if (tp == typeof(Vector3Int))
            {
                JObject subJo = jo.Get<JObject>(key);
                int x = subJo.Value<int>("x");
                int y = subJo.Value<int>("y");
                int z = subJo.Value<int>("z");
                return (T)(object)new Vector3Int(x, y, z);
            }
            else
            {
                return (T)(object)jo[key].ToObject<T>();
            }
        }
    }
}