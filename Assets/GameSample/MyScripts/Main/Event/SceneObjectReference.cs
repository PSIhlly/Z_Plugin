using System;
using System.Collections.Generic;
using System.Globalization;
using Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

public static partial class GlobalEventHelper
{
    // Bare numeric $so$ stays a legacy Unit UID. Product/template pools can overlap it.
    public enum ObjectReferenceKind { Unit, Product, Template }

    public static bool TryGetSceneObjectReference(string reference, out ObjectReferenceKind kind, out int id)
    {
        kind = ObjectReferenceKind.Unit;
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
        else if (marker == SCENEOBJECT && body.StartsWith("t:", StringComparison.Ordinal))
        {
            kind = ObjectReferenceKind.Template;
            body = body.Substring(2);
        }
        return int.TryParse(body, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }

    public static string GetSceneObjectTemplateReference(int templateId) => GetName(SCENEOBJECT, "t:" + templateId);

    public static string GetSceneObjectProductReference(int productUid) => GetName(SCENEOBJECT, "p:" + productUid);

    public static string GetSceneObjectReference(MapUnit unit)
    {
        if (unit is ObjectUnit obj)
        {
            var product = ObjectProductForm.GetProduct(obj.data);
            if (product != null) return GetSceneObjectProductReference(product.uid);
        }
        return GetName(SCENEOBJECT, unit.data.uid.ToString(CultureInfo.InvariantCulture));
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
