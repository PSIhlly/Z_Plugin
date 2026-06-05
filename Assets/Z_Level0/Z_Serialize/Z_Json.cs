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
        public static Dictionary<Type, (Func<object, JObject>, Func<JObject, object>)> extra = new Dictionary<Type, (Func<object, JObject>, Func<JObject, object>)>();
        public static void Set<T>(this JObject jo, string key, T value)
        {
            var tp = typeof(T);
            jo.Set(tp, key, value);
        }
        public static void Set(this JObject jo, Type tp, string key, object value)
        {
            if (extra.ContainsKey(tp))
            {
                jo[key] = extra[tp].Item1(value);
            }
            else if (value == null)
            {
                jo[key] = null;
            }
            else if (value is string str)
            {
                if (str == null)
                    str = "";
                jo[key] = JToken.FromObject(str);
            }
            else if (value is Vector2 v2)
            {
                jo[key] = v2.x + "|" + v2.y;
            }
            else if (value is Vector2Int v2i)
            {
                jo[key] = v2i.x + "|" + v2i.y;
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

                    subJo.Set(tp.GetGenericArguments()[0], "v", v);
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
            else if (value is Enum)
            {

                jo[key] = (int)value;
            }
            else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Tuple<,>))
            {
                var tuple = value;
                JObject subJo = new JObject();
                subJo.Set(tp.GetGenericArguments()[0], "item1", tp.GetProperty("Item1").GetValue(tuple));
                subJo.Set(tp.GetGenericArguments()[1], "item2", tp.GetProperty("Item2").GetValue(tuple));
                jo[key] = subJo;
            }
            else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
            {
                var tuple = value;
                JObject subJo = new JObject();
                subJo.Set(tp.GetGenericArguments()[0], "item1", tp.GetField("Item1").GetValue(tuple));
                subJo.Set(tp.GetGenericArguments()[1], "item2", tp.GetField("Item2").GetValue(tuple));
                jo[key] = subJo;
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
        public static object Get(this JObject jo, Type tp, string key)
        {

            try
            {
                if (key == null || jo[key].Type == JTokenType.Null)
                {
                    return null;
                }
                else if (extra.ContainsKey(tp))
                {
                    return extra[tp].Item2((JObject)jo[key]);
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
                else if (tp == typeof(Vector2))
                {
                    string[] str = jo.Get<string>(key).Split("|");
                    return new Vector2(float.Parse(str[0]), float.Parse(str[1]));
                }
                else if (tp == typeof(Vector2Int))
                {
                    string[] str = jo.Get<string>(key).Split("|");
                    return new Vector2Int(int.Parse(str[0]), int.Parse(str[1]));
                }
                else if (IsIList(tp))
                {
                    if (!(jo[key] is JArray))
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
                else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(Tuple<,>))
                {
                    JObject subJo = (JObject)jo[key];
                    var args = tp.GetGenericArguments();
                    var item1 = subJo.Get(args[0], "item1");
                    var item2 = subJo.Get(args[1], "item2");
                    return Activator.CreateInstance(tp, item1, item2);
                }
                else if (tp.IsGenericType && tp.GetGenericTypeDefinition() == typeof(ValueTuple<,>))
                {
                    JObject subJo = (JObject)jo[key];
                    var args = tp.GetGenericArguments();
                    var item1 = subJo.Get(args[0], "item1");
                    var item2 = subJo.Get(args[1], "item2");
                    return Activator.CreateInstance(tp, item1, item2);
                }
                else if (tp.IsEnum)
                {
                    var obj = jo.Get<int>(key);
                    return obj;
                }
                else
                {
                    return jo[key].ToObject(tp);
                }
            }
            catch (Exception e)
            {
                Debug.LogError(jo + " " + tp + " " + key + " " + e);
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