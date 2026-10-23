using System;
using System.Collections.Generic;
using System.Linq;
using System.Runtime.CompilerServices;
using Newtonsoft.Json.Linq;
using Z_DataSystem.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;

namespace Form
{
    public static partial class ObjectProductForm
    {
        public const string ProductVersionKey = "opv";
        public const int ProductVersion = 2;

        // Import aliases only. MapObject rows are disposable chooser entries.
        private static readonly Dictionary<int, int> legacyTemplates = new Dictionary<int, int>();
        private sealed class UnitBinding
        {
            public string extra;
            public (int, int) tuple;
            public int productUid;
        }
        private static ConditionalWeakTable<ObjectUnitForm.Data, UnitBinding> unitBindings =
            new ConditionalWeakTable<ObjectUnitForm.Data, UnitBinding>();

        public partial class Data
        {
            public override void ToProduct(int protoUid)
            {
                // Generated Copy() only clones outer containers. Own nested
                // model, event, animation and parameter data before registering.
                var owned = GetDataByJo(GetJoByData(this));
                model = owned.model;
                events = owned.events;
                animClip = owned.animClip;
                paramDic = owned.paramDic;
                base.ToProduct(protoUid);
            }
        }

        public static int GetTemplateUid(Data product) => product == null ? 0 :
            product.protoUid == 0 ? product.uid : product.protoUid;

        public static Data ResolveLegacyTemplate(int mapObjectId) =>
            legacyTemplates.TryGetValue(mapObjectId, out int uid) && DataByUid.TryGetValue(uid, out var product)
                ? product : null;

        public static Data GetTemplatePrototype(int mapObjectId) => ResolveLegacyTemplate(mapObjectId);

        public static List<Data> ResolveTemplateProducts(int mapObjectId)
        {
            var prototype = ResolveLegacyTemplate(mapObjectId);
            return prototype == null ? new List<Data>() : ResolveReferences(prototype.uid);
        }

        public static List<Data> ResolveReferences(int uid)
        {
            var result = new List<Data>();
            if (uid <= 0 || !DataByUid.TryGetValue(uid, out var source))
                return result;
            if (source.protoUid != 0)
                result.Add(source);
            else if (DatasByProtouid.TryGetValue(uid, out var copies))
                result.AddRange(copies.Where(copy => DataByUid.TryGetValue(copy.uid, out var current)
                    && ReferenceEquals(copy, current)));
            if (source.protoUid == 0 && !DynamicGlobalSettings.playing && result.Count == 0)
                result.Add(source);
            result.Sort((left, right) => left.uid.CompareTo(right.uid));
            return result;
        }

        public static Data ResolveReference(int uid)
        {
            if (!DataByUid.TryGetValue(uid, out var source))
                return null;
            if (source.protoUid != 0)
                return source;
            foreach (var unit in ObjectUnitForm.DataByUid.Values.OrderBy(unit => unit.uid))
            {
                var product = GetProduct(unit);
                if (product != null && GetTemplateUid(product) == uid)
                    return product;
            }
            var products = ResolveReferences(uid);
            return products.Count > 0 ? products[0] : source;
        }

        // Compatibility entry point for existing save orchestration. No map
        // list row contributes configuration or identity to a Product.
        public static void EnsurePrototypes()
        {
            foreach (var product in DataByUid.Values)
                product.labId = LabForm.GetOrCreateForBelong(product.labId, nameof(ObjectProductForm));
        }

        /// <summary>Import removed legacy fields before generated deserialization discards them.</summary>
        public static void LoadProducts(JArray products, JArray legacyObjects)
        {
            Clear();
            ResetReferences();
            var templates = new Dictionary<int, JObject>();
            if (legacyObjects != null)
                foreach (var item in legacyObjects.OfType<JObject>())
                    if ((int?)item["id"] > 0)
                        templates[(int)item["id"]] = item;

            var wrappers = new List<(Data product, int mapId)>();
            if (products != null)
                foreach (var item in products.OfType<JObject>())
                {
                    JObject payload = item;
                    int mapId = (int?)item["mapObjectId"] ?? 0;
                    if (mapId > 0)
                    {
                        payload = templates.TryGetValue(mapId, out var template)
                            ? (JObject)template.DeepClone() : new JObject();
                        payload.Remove("id");
                        foreach (string field in new[] { "uid", "name", "labId", "protoUid", "paramDic" })
                            if (item[field] != null)
                                payload[field] = item[field].DeepClone();
                    }
                    var product = GetDataByJo(payload);
                    if (product.uid <= 0)
                        continue;
                    if (ProductForm.DataByUid.ContainsKey(product.uid))
                        throw new InvalidOperationException("Duplicate Object Product UID: " + product.uid);
                    Normalize(product);
                    RegisterProduct(product);
                    if (mapId > 0)
                    {
                        wrappers.Add((product, mapId));
                        if (product.protoUid == 0)
                            legacyTemplates[mapId] = product.uid;
                    }
                }

            // Reserve all persisted shared Product UIDs before allocating new
            // prototypes for stories which predate the oprf table.
            foreach (var pair in templates)
                if (!legacyTemplates.ContainsKey(pair.Key))
                {
                    if (products != null && products.Count > 0 && wrappers.Count == 0)
                        continue; // A new-format oprf overrides any stale mof.
                    var payload = (JObject)pair.Value.DeepClone();
                    payload.Remove("id");
                    payload["uid"] = -1;
                    payload["protoUid"] = 0;
                    var prototype = GetDataByJo(payload);
                    Normalize(prototype);
                    RegisterProduct(prototype);
                    legacyTemplates[pair.Key] = prototype.uid;
                }
            foreach (var wrapper in wrappers)
                if (wrapper.product.protoUid != 0 && legacyTemplates.TryGetValue(wrapper.mapId, out int protoUid))
                    wrapper.product.protoUid = protoUid;
        }

        private static void Normalize(Data product)
        {
            product.labId = LabForm.GetOrCreateForBelong(product.labId, nameof(ObjectProductForm));
            product.EnsureDirectionData();
            product.SyncLegacyAnimClip(product.GetDefaultAnimDirection());
        }

        public static void ResetReferences()
        {
            legacyTemplates.Clear();
            unitBindings = new ConditionalWeakTable<ObjectUnitForm.Data, UnitBinding>();
        }

        public static void PreparePlayStory(string cacheFolder) => PrepareScenes(cacheFolder, true);
        public static void PrepareModStory(string cacheFolder) => PrepareScenes(cacheFolder, false);

        // Cache-only migration; never register/clear scene Units or rewrite Core/Save.
        private static void PrepareScenes(string cacheFolder, bool instantiate)
        {
            foreach (var scene in SceneForm.DataByUid.Values)
            {
                string path = cacheFolder + "/" + Main2StoryManager.GetSceneFileNameById(scene.uid);
                if (!SaveAndLoad.Exist(path))
                    continue;
                var map = JObject.Parse(SaveAndLoad.Load<string>(path));
                if (MigrateScene(map, instantiate))
                    SaveAndLoad.Save(path, map.ToString());
            }
        }

        public static string MigrateSceneJson(string json, bool instantiate)
        {
            var map = JObject.Parse(json);
            return MigrateScene(map, instantiate) ? map.ToString() : json;
        }

        private static bool MigrateScene(JObject map, bool instantiate)
        {
            var token = map["objectJa"];
            if (token == null || token.Type == JTokenType.Null)
                return false;
            var objects = token.Type == JTokenType.String ? JArray.Parse((string)token) : (JArray)token;
            bool changed = false;
            foreach (var obj in objects.OfType<JObject>())
            {
                var extra = ParseExtra(obj["extra"]);
                var product = GetProduct(extra);
                if (product == null)
                    continue;
                if (instantiate && product.protoUid == 0)
                    product = CreateProduct(product, ReadParameters(extra));
                bool current = (int?)extra[ProductVersionKey] == ProductVersion;
                var tuple = extra[MapUnit.productKey] as JArray;
                string prefabName = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(GetTemplateUid(product));
                if (!current || tuple == null || tuple.Count != 2 || (int)tuple[0] != product.uid || (int)tuple[1] != -1
                    || (string)obj["prefabName"] != prefabName)
                {
                    extra[ProductVersionKey] = ProductVersion;
                    extra[MapUnit.productKey] = new JArray(product.uid, -1);
                    obj["extra"] = extra.ToString();
                    obj["prefabName"] = prefabName;
                    changed = true;
                }
            }
            if (changed)
                map["objectJa"] = token.Type == JTokenType.String ? (JToken)objects.ToString() : objects;
            return changed;
        }

        public static Data GetProduct(ObjectUnitForm.Data unitData)
        {
            if (unitData == null)
                return null;
            var tuple = unitData.unit.productInfo;
            var binding = unitBindings.GetValue(unitData, _ => new UnitBinding());
            // Visibility, collision and WangTile query this hot path. Parse
            // legacy/version JSON once per changed Unit payload, not per read.
            if (!ReferenceEquals(binding.extra, unitData.extra) || binding.tuple != tuple)
            {
                var product = GetProduct(ParseExtra(unitData.extra));
                binding.extra = unitData.extra;
                binding.tuple = tuple;
                binding.productUid = product?.uid ?? 0;
            }
            return DataByUid.TryGetValue(binding.productUid, out var current) ? current : null;
        }

        private static Data GetProduct(JObject extra)
        {
            var tuple = extra[MapUnit.productKey] as JArray;
            if (tuple == null || tuple.Count == 0)
                return null;
            if ((int?)extra[ProductVersionKey] == ProductVersion)
                return DataByUid.TryGetValue((int)tuple[0], out var product) ? product : null;
            // The legacy first value is a Map ID: never guess by numeric equality.
            if (tuple.Count > 1 && (int)tuple[1] > 0 && DataByUid.TryGetValue((int)tuple[1], out var placed))
                return placed;
            return ResolveLegacyTemplate((int)tuple[0]);
        }

        public static void AssignProduct(ObjectUnitForm.Data unitData, Data product)
        {
            if (unitData == null || product == null)
                return;
            var extra = ParseExtra(unitData.extra);
            extra[ProductVersionKey] = ProductVersion;
            extra[MapUnit.productKey] = new JArray(product.uid, -1);
            unitData.extra = extra.ToString();
            unitData.unit.productInfo = (product.uid, -1);
            unitData.prefabName = GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(GetTemplateUid(product));
            if (DynamicGlobalSettings.playing)
                unitData.unit.paramInfo = AsUnitParameters(product.paramDic);
        }

        public static Data BindUnit(ObjectUnitForm.Data unitData)
        {
            var product = GetProduct(unitData);
            if (product == null)
                return null;
            if (DynamicGlobalSettings.playing && product.protoUid == 0)
                product = CreateProduct(product, ReadParameters(ParseExtra(unitData.extra)));
            AssignProduct(unitData, product);
            return product;
        }

        public static Data CreateForUnit(ObjectUnitForm.Data unitData) => BindUnit(unitData);

        private static Data CreateProduct(Data prototype, Dictionary<string, MapObjectParamForm.Data> overrides)
        {
            var product = prototype.Copy(false);
            product.ToProduct(GetTemplateUid(prototype));
            if (overrides != null)
                foreach (var pair in overrides)
                    product.paramDic[pair.Key] = pair.Value.Copy();
            return product;
        }

        public static void RefreshUnitParameters(ObjectUnitForm.Data unitData)
        {
            var product = GetProduct(unitData);
            if (product != null)
                unitData.unit.paramInfo = AsUnitParameters(product.paramDic);
        }

        public static Dictionary<string, GameParamForm.Data> GetUnitParameters(ObjectUnitForm.Data unitData)
        {
            var product = GetProduct(unitData);
            return product == null ? null : AsUnitParameters(product.paramDic);
        }

        public static Dictionary<string, GameParamForm.Data> SetUnitParameters(ObjectUnitForm.Data unitData,
            Dictionary<string, GameParamForm.Data> parameters)
        {
            var product = GetProduct(unitData);
            if (product == null)
                return parameters;
            var owned = new Dictionary<string, MapObjectParamForm.Data>();
            if (parameters != null)
                foreach (var pair in parameters)
                    if (pair.Value != null)
                        owned[pair.Key] = pair.Value as MapObjectParamForm.Data ?? CopyParameter(pair.Value);
            product.paramDic = owned;
            return AsUnitParameters(owned);
        }

        public static void RemoveForUnit(ObjectUnitForm.Data unitData)
        {
            var product = GetProduct(unitData);
            if (product != null && product.protoUid != 0)
                RemoveData(product.uid);
        }

        private static void RegisterProduct(Data product)
        {
            if (AddData(product) <= 0)
                throw new InvalidOperationException("Scene object Product ID pool is exhausted.");
        }

        private static JObject ParseExtra(JToken extra) => extra == null || extra.Type == JTokenType.Null
            || extra.Type == JTokenType.String && string.IsNullOrEmpty((string)extra)
                ? new JObject() : extra is JObject obj ? (JObject)obj.DeepClone() : JObject.Parse((string)extra);

        private static Dictionary<string, MapObjectParamForm.Data> ReadParameters(JObject extra)
        {
            var result = new Dictionary<string, MapObjectParamForm.Data>();
            if (extra[MapUnit.paramKey] is JArray parameters)
                foreach (var pair in parameters.OfType<JObject>())
                    if (pair["k"]?.Type == JTokenType.String && pair["v"] is JObject value)
                        result[(string)pair["k"]] = MapObjectParamForm.GetDataByJo(value);
            return result;
        }

        private static MapObjectParamForm.Data CopyParameter(GameParamForm.Data data) =>
            new MapObjectParamForm.Data(data.uid, data.name, data.valueType, data.min, data.v, data.max);

        private static Dictionary<string, GameParamForm.Data> AsUnitParameters(Dictionary<string, MapObjectParamForm.Data> source)
        {
            var result = new Dictionary<string, GameParamForm.Data>();
            if (source != null)
                foreach (var pair in source)
                    if (pair.Value != null)
                        result[pair.Key] = pair.Value;
            return result;
        }
    }
}
