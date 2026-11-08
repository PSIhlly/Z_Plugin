using System;
using System.Collections.Generic;
using System.Globalization;
using System.Linq;
using System.Text.RegularExpressions;
using Form;
using Newtonsoft.Json.Linq;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

public static partial class GlobalEventHelper
{
    // Numeric handles always name Products. Scene-only Units use an explicit u:.
    public enum ObjectReferenceKind { Unit, Product, Template }

    public static bool TryGetSceneObjectReference(string reference, out ObjectReferenceKind kind, out int id)
    {
        kind = ObjectReferenceKind.Product;
        id = 0;
        if (string.IsNullOrEmpty(reference)) return false;
        string marker;
        if (reference.StartsWith(SCENEOBJECTPROTO, StringComparison.Ordinal))
        {
            marker = SCENEOBJECTPROTO;
            kind = ObjectReferenceKind.Template;
        }
        else marker = SCENEOBJECT;
        if (!reference.StartsWith(marker, StringComparison.Ordinal) || !reference.EndsWith(marker, StringComparison.Ordinal)
            || reference.Length <= marker.Length * 2) return false;
        var body = reference.Substring(marker.Length, reference.Length - marker.Length * 2);
        if (marker == SCENEOBJECT && body.StartsWith("p:", StringComparison.Ordinal))
        {
            kind = ObjectReferenceKind.Product;
            body = body.Substring(2);
        }
        else if (marker == SCENEOBJECT && body.StartsWith("u:", StringComparison.Ordinal))
        {
            kind = ObjectReferenceKind.Unit;
            body = body.Substring(2);
        }
        else if (marker == SCENEOBJECT && body.StartsWith("t:", StringComparison.Ordinal))
        {
            kind = ObjectReferenceKind.Template;
            body = body.Substring(2);
        }
        return int.TryParse(body, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }

    public static string GetSceneObjectTemplateReference(int templateId) => GetName(SCENEOBJECT, "t:" + templateId);

    public static string GetSceneObjectProductReference(int productUid) => GetName(SCENEOBJECT, productUid.ToString(CultureInfo.InvariantCulture));

    public static string GetSceneUnitReference(int unitUid) => GetName(SCENEOBJECT, "u:" + unitUid.ToString(CultureInfo.InvariantCulture));

    public static string GetSceneObjectReference(MapUnit unit)
    {
        if (unit is ObjectUnit obj)
        {
            var product = ObjectProductForm.GetProduct(obj.data);
            if (product != null) return GetSceneObjectProductReference(product.uid);
        }
        return GetSceneUnitReference(unit.data.uid);
    }

    public const string SceneObjectReferenceVersionKey = "sorv";
    private const int SceneObjectReferenceVersion = 2;
    private static readonly Regex sceneObjectReferences = new Regex(
        @"\$so\$(?:(p:|u:|t:))?([0-9]+)\$so\$|\$sop\$([0-9]+)\$sop\$");

    // Each Form row / map payload owns its version. Never infer an old handle's
    // meaning by searching two registries whose UID pools can overlap.
    public static bool MigrateSceneObjectReferences(JToken token)
    {
        if (token is JArray rows)
        {
            bool changed = false;
            foreach (var row in rows.OfType<JObject>())
                changed |= RewriteSceneObjectReferences(row, true);
            return changed;
        }
        return token is JObject record && RewriteSceneObjectReferences(record, true);
    }

    public static T SaveSceneObjectReferences<T>(T token) where T : JToken
    {
        if (token is JArray rows)
        {
            foreach (var row in rows.OfType<JObject>())
                RewriteSceneObjectReferences(row, false);
        }
        else if (token is JObject record)
            RewriteSceneObjectReferences(record, false);
        return token;
    }

    private static bool RewriteSceneObjectReferences(JObject record, bool loading)
    {
        int version = (int?)record[SceneObjectReferenceVersionKey] ?? 1;
        bool legacyUnitNumbers = loading && version < SceneObjectReferenceVersion;
        bool changed = version < SceneObjectReferenceVersion;
        string originalCode = record["code"]?.Type == JTokenType.String ? (string)record["code"] : null;
        foreach (var value in record.Descendants().OfType<JValue>())
        {
            if (value.Type != JTokenType.String)
                continue;
            string original = (string)value;
            string migrated = sceneObjectReferences.Replace(original, match =>
            {
                bool oldTemplate = match.Groups[3].Success;
                string digits = oldTemplate ? match.Groups[3].Value : match.Groups[2].Value;
                if (!int.TryParse(digits, NumberStyles.None, CultureInfo.InvariantCulture, out int id) || id <= 0)
                    return match.Value;
                string prefix = match.Groups[1].Value;
                if (oldTemplate || prefix == "t:")
                {
                    var product = ObjectProductForm.ResolveLegacyTemplate(id);
                    return product == null ? match.Value : GetSceneObjectProductReference(product.uid);
                }
                if (prefix == "p:")
                    return GetSceneObjectProductReference(id);
                if (prefix.Length == 0 && legacyUnitNumbers)
                    return GetSceneUnitReference(id);
                return match.Value;
            });
            if (migrated == original)
                continue;
            value.Value = migrated;
            changed = true;
        }
        if (version < SceneObjectReferenceVersion)
            record[SceneObjectReferenceVersionKey] = SceneObjectReferenceVersion;
        if (originalCode != null && originalCode != (string)record["code"])
        {
            // LoadEvent recompiles after registering every program, preserving
            // source maps and cross-program calls after literal lengths change.
            record["zCode"] = new JArray();
            record["zCodeMap"] = new JArray();
        }
        return changed;
    }

    public static List<MapUnit> ResolveSceneObjectUnits(string reference)
    {
        var result = new List<MapUnit>();
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return result;
        if (kind == ObjectReferenceKind.Unit)
        {
            if (UnitForm.DataByUid.TryGetValue(id, out var data) && data.unit is MapUnit unit
                && ReferenceEquals(unit.data, data)) result.Add(unit);
            return result;
        }
        ObjectProductForm.Data referenceProduct;
        if (kind == ObjectReferenceKind.Product)
            ObjectProductForm.DataByUid.TryGetValue(id, out referenceProduct);
        else
            referenceProduct = ObjectProductForm.ResolveLegacyTemplate(id);
        if (referenceProduct == null) return result;
        bool exact = referenceProduct.protoUid != 0;
        int templateUid = ObjectProductForm.GetTemplateUid(referenceProduct);
        foreach (var data in ObjectUnitForm.DataByUid.Values)
        {
            if (!(data.unit is ObjectUnit unit) || !ReferenceEquals(unit.data, data)
                || !UnitForm.DataByUid.TryGetValue(data.uid, out var current) || !ReferenceEquals(data, current)) continue;
            var product = ObjectProductForm.GetProduct(data);
            if (product != null && (exact ? product.uid == referenceProduct.uid
                    : ObjectProductForm.GetTemplateUid(product) == templateUid)) result.Add(unit);
        }
        result.Sort((a, b) => a.data.uid.CompareTo(b.data.uid));
        return result;
    }

    public static MapUnit ResolveSceneObjectUnit(string reference)
    {
        var units = ResolveSceneObjectUnits(reference);
        return units.Count == 0 ? null : units[0];
    }

    public static ObjectProductForm.Data ResolveSceneObjectTemplate(string reference)
    {
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return null;
        if (kind == ObjectReferenceKind.Product)
        {
            return ObjectProductForm.DataByUid.TryGetValue(id, out var product) ? product : null;
        }
        else if (kind == ObjectReferenceKind.Unit)
        {
            if (!(ResolveSceneObjectUnit(reference) is ObjectUnit unit)) return null;
            return ObjectProductForm.GetProduct(unit.data);
        }
        return ObjectProductForm.ResolveLegacyTemplate(id);
    }

    public static List<ObjectProductForm.Data> ResolveSceneObjectProducts(string reference)
    {
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return new List<ObjectProductForm.Data>();
        if (kind == ObjectReferenceKind.Unit)
        {
            var unit = ResolveSceneObjectUnit(reference) as ObjectUnit;
            var product = unit == null ? null : ObjectProductForm.GetProduct(unit.data);
            return product == null ? new List<ObjectProductForm.Data>() : new List<ObjectProductForm.Data> { product };
        }
        if (kind == ObjectReferenceKind.Product)
        {
            return ObjectProductForm.ResolveReferences(id);
        }
        var template = ObjectProductForm.ResolveLegacyTemplate(id);
        return template == null ? new List<ObjectProductForm.Data>() : ObjectProductForm.ResolveReferences(template.uid);
    }
}
