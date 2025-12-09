using System.Collections.Generic;
using System;
using UnityEngine;
namespace Z_DesignStyle
{

    public static class DictionaryExtensions
{
        public static bool ContainsParent(this GameObject go,GameObject tar)
        {
            if(go==null) 
                return false;
            var tr = go.transform;
            while(tr!=null)
            {
                if (tr.gameObject == tar)
                    return true;
                tr = tr.parent;
            }
            return false;
        }
        public static TValue GetDv<TValue>(
        this List<TValue> lst,
        int id,
        TValue defaultValue = default)
        {
            // 校验字典是否为null
            if (lst == null || lst.Count<=id)
                return defaultValue;

            // 存在键则返回对应值，否则返回默认值
            return lst[id];
        }

        public static TValue GetDv<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TValue defaultValue = default)
    {
            // 校验字典是否为null
            if (dict == null || key == null)
                return defaultValue;

        // 存在键则返回对应值，否则返回默认值
        return dict.TryGetValue(key, out TValue value) ? value : defaultValue;
    }

        public static TValue GetDk<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TKey defaultKey = default)
        {
            // 校验字典是否为null
            if (dict == null)
                return default;

            // 存在键则返回对应值，否则返回默认值
            return dict.TryGetValue(key, out TValue value) ? value : dict[defaultKey];
        }
    }
}