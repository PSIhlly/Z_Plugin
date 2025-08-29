using System.Collections.Generic;
using System;
namespace Z_DesignStyle
{

    public static class DictionaryExtensions
{
    public static TValue Get<TKey, TValue>(
        this Dictionary<TKey, TValue> dict,
        TKey key,
        TValue defaultValue = default)
    {
        // 校验字典是否为null
        if (dict == null)
            throw new ArgumentNullException(nameof(dict));

        // 存在键则返回对应值，否则返回默认值
        return dict.TryGetValue(key, out TValue value) ? value : defaultValue;
    }
}
}