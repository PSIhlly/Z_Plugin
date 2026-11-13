using System;
using System.Collections.Generic;
using System.Globalization;
using Form;
using Z_Map;
using Z_UnitSystem.Form;

public static partial class GlobalEventHelper
{
    public enum ObjectReferenceKind { Unit, Product }

    public static bool TryGetSceneObjectReference(string reference, out ObjectReferenceKind kind, out int id)
    {
        kind = ObjectReferenceKind.Product;
        id = 0;
        if (string.IsNullOrEmpty(reference) || !reference.StartsWith(SCENEOBJECT, StringComparison.Ordinal)
            || !reference.EndsWith(SCENEOBJECT, StringComparison.Ordinal) || reference.Length <= SCENEOBJECT.Length * 2)
            return false;
        string body = reference.Substring(SCENEOBJECT.Length, reference.Length - SCENEOBJECT.Length * 2);
        if (body.StartsWith("u:", StringComparison.Ordinal))
        {
            kind = ObjectReferenceKind.Unit;
            body = body.Substring(2);
        }
        return int.TryParse(body, NumberStyles.None, CultureInfo.InvariantCulture, out id) && id > 0;
    }

    public static string GetSceneObjectProductReference(int productUid) => GetName(SCENEOBJECT, productUid.ToString(CultureInfo.InvariantCulture));
    public static string GetSceneUnitReference(int unitUid) => GetName(SCENEOBJECT, "u:" + unitUid.ToString(CultureInfo.InvariantCulture));

    public static string GetSceneObjectReference(MapUnit unit) => unit is ObjectUnit obj && obj.product != null
        ? GetSceneObjectProductReference(obj.product.uid) : GetSceneUnitReference(unit.data.uid);

    public static MapUnit ResolveSceneObjectUnit(string reference)
    {
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return null;
        if (kind == ObjectReferenceKind.Product) return ObjectProductForm.GetSceneUnit(id)?.unit;
        return UnitForm.DataByUid.TryGetValue(id, out var data) && data.unit is MapUnit unit
            && ReferenceEquals(unit.data, data) ? unit : null;
    }

    public static List<MapUnit> ResolveSceneObjectUnits(string reference)
    {
        var result = new List<MapUnit>();
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return result;
        if (kind == ObjectReferenceKind.Product) ObjectProductForm.CollectSceneUnits(id, result);
        else if (UnitForm.DataByUid.TryGetValue(id, out var data) && data.unit is MapUnit unit
            && ReferenceEquals(unit.data, data)) result.Add(unit);
        return result;
    }

    public static ObjectProductForm.Data ResolveSceneObjectTemplate(string reference)
    {
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return null;
        if (kind == ObjectReferenceKind.Product)
            return ObjectProductForm.DataByUid.TryGetValue(id, out var product) ? product : null;
        return (ResolveSceneObjectUnit(reference) as ObjectUnit)?.product;
    }

    public static List<ObjectProductForm.Data> ResolveSceneObjectProducts(string reference)
    {
        if (!TryGetSceneObjectReference(reference, out var kind, out int id)) return new List<ObjectProductForm.Data>();
        if (kind == ObjectReferenceKind.Product) return ObjectProductForm.ResolveReferences(id);
        var product = (ResolveSceneObjectUnit(reference) as ObjectUnit)?.product;
        return product == null ? new List<ObjectProductForm.Data>() : new List<ObjectProductForm.Data> { product };
    }
}
