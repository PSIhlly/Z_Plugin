using System;
using System.Collections.Generic;
using Form;
using Newtonsoft.Json.Linq;
using Z_DataSystem.Form;

namespace Z_Map
{
    public partial class ObjectUnit
    {
        public ObjectProductForm.Data product => ObjectProductForm.DataByUid.TryGetValue(productInfo.Item1, out var row)
            ? row : null;

        public ObjectProductForm.Data BindProduct(ObjectProductForm.Data source = null)
        {
            source ??= product;
            if (source == null) return null;
            if (DynamicGlobalSettings.playing && source.protoUid == 0)
                source = CreateProductForPlacement(source, string.IsNullOrEmpty(data.extra) ? null : JObject.Parse(data.extra));
            productInfo = (source.uid, -1);
            data.prefabName = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(ObjectProductForm.GetTemplateUid(source));
            if (DynamicGlobalSettings.playing) BindObjectProductParameters(GetProductParameters());
            return source;
        }

        public static ObjectProductForm.Data CreateProductForPlacement(ObjectProductForm.Data source, JObject extra)
        {
            var copy = source.Copy(false);
            copy.ToProduct(ObjectProductForm.GetTemplateUid(source));
            if (extra?[paramKey] is JArray parameters)
                foreach (var entry in parameters)
                    if (entry["k"]?.Type == JTokenType.String && entry["v"] is JObject value)
                        copy.paramDic[(string)entry["k"]] = MapObjectParamForm.GetDataByJo(value);
            if (!ObjectProductForm.DataByUid.TryGetValue(copy.uid, out var registered) || !ReferenceEquals(registered, copy))
                throw new InvalidOperationException("Object Product ID pool is exhausted.");
            return copy;
        }

        internal Dictionary<string, GameParamForm.Data> GetProductParameters()
        {
            var owner = product;
            if (owner == null) return null;
            var result = new Dictionary<string, GameParamForm.Data>(owner.paramDic?.Count ?? 0);
            if (owner.paramDic != null)
                foreach (var pair in owner.paramDic)
                    if (pair.Value != null) result.Add(pair.Key, pair.Value);
            return result;
        }

        internal Dictionary<string, GameParamForm.Data> SetProductParameters(Dictionary<string, GameParamForm.Data> values)
        {
            var owner = product;
            if (owner == null) return values;
            var owned = new Dictionary<string, MapObjectParamForm.Data>(values?.Count ?? 0);
            if (values != null)
                foreach (var pair in values)
                {
                    if (pair.Value == null) continue;
                    var parameter = pair.Value as MapObjectParamForm.Data;
                    if (parameter == null)
                    {
                        parameter = new MapObjectParamForm.Data(pair.Value);
                        parameter.SetValue(pair.Value.GetValue());
                    }
                    owned.Add(pair.Key, parameter);
                }
            owner.paramDic = owned;
            return GetProductParameters();
        }
    }
}
