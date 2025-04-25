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
        public static Dictionary<Type,(Func<object,JObject>, Func<JObject, object>)> extra=new Dictionary<Type, (Func<object, JObject>, Func<JObject, object>)>();
        public static void Set<T>(this JObject jo, string key, T value)
        {
            var tp = typeof(T);
            jo.Set(tp, key, value);
        }
        public static void Set(this JObject jo, Type tp,string key, object value)
        {
            if (extra.ContainsKey(tp))
            {
                jo[key] = extra[tp].Item1(value);
            }
            else if (value == null)
            {
                jo[key] = null;
            }
            else if (value is Vector3 v3)
            {
                jo[key] = v3.x + "|" + v3.y + "|" + v3.z;
            }
            else if (value is Vector3Int v3i)
            {
                jo[key] = v3i.x + "|" + v3i.y + "|" + v3i.z;
            }
            else if (value is IList lst)
            {
                JArray ja = new JArray();
                foreach (var v in lst)
                {
                    JObject subJo = new JObject();

                    subJo.Set(tp.GetGenericArguments()[0],"v", v);
                    ja.Add(subJo);
                }
                jo[key] = ja;
            }
            else if (value is IDictionary dic)
            {
                JArray ja = new JArray();
                foreach (var k in dic.Keys)
                {
                    JObject subJo = new JObject();
                    subJo.Set(tp.GetGenericArguments()[0], "k", k);
                    subJo.Set(tp.GetGenericArguments()[1], "v", dic[k]);
                    ja.Add(subJo);
                }
                jo[key] = ja;
            }
            else
            {
                jo[key] = JToken.FromObject(value);
            }

        }

        public static T Get<T>(this JObject jo, string key)
        {
            var tp = typeof(T);
            return (T)jo.Get(tp, key);
        }
        public static object Get(this JObject jo,Type tp, string key)
        {

            try
            {
                if (extra.ContainsKey(tp))
                {
                    return extra[tp].Item2((JObject)jo[key]);
                }
                else if (jo[key] == null||jo[key]==null)
                {
                    return null;
                }
                else if (tp == typeof(Vector3))
                {
                    string[] str = jo.Get<string>(key).Split("|");
                    return new Vector3(float.Parse(str[0]), float.Parse(str[1]), float.Parse(str[2]));
                }
                else if (tp == typeof(Vector3Int))
                {
                    string[] str = jo.Get<string>(key).Split("|");
                    return new Vector3Int(int.Parse(str[0]), int.Parse(str[1]), int.Parse(str[2]));
                }
                else if (IsIList(tp))
                {
                    if(!(jo[key] is JArray))
                    {
                        return null;
                    }
                    JArray ja = (JArray)jo[key];
                    var obj = Activator.CreateInstance(tp);
                    if (obj is IList lst)
                    {
                        foreach (JObject sub in ja)
                        {
                            lst.Add(sub.Get(tp.GetGenericArguments()[0], "v"));
                        }
                    }
                    return obj;
                }
                else if (IsIDic(tp))
                {
                    if (!(jo[key] is JArray))
                    {
                        return null;
                    }
                    JArray ja = (JArray)jo[key];
                    var obj = Activator.CreateInstance(tp);
                    if (obj is IDictionary dic)
                    {
                        foreach (JObject sub in ja)
                        {
                            dic[sub.Get(tp.GetGenericArguments()[0], "k")] = sub.Get(tp.GetGenericArguments()[1], "v");
                        }
                    }
                    return obj;
                }
                else
                {
                    return jo[key].ToObject(tp);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(jo+" "+tp+" "+key+" "+e);
                return null;
            }
        }


        static bool IsIList(Type type)
        {
            return typeof(IList).IsAssignableFrom(type);
        }
        static bool IsIDic(Type type)
        {
            return typeof(IDictionary).IsAssignableFrom(type);
        }
    }
}