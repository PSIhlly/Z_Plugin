using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;

// Run only in the isolated regression project. No scene or save files are read/written.
public static class ObjectProductEditorRegression
{
    private static int checks;
    private static readonly List<GameObject> managers = new List<GameObject>();

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            RegisterForms();
            CreateManager<GameManager>();
            var mod = CreateManager<ModManager>();
            var assets = new ModAssetCtrl(mod);
            mod.assetCtrl = assets;
            DynamicGlobalSettings.playing = false;
            ObjectProductForm.Clear();
            MapObjectForm.Clear();
            MapObjectParamForm.Clear();

            var parameter = new MapObjectParamForm.Data(-1, "EditorHp", ValType.Float, "0", "12", "100");
            MapObjectParamForm.AddData(parameter);
            int authoredLab = LabForm.GetOrCreate("editor-regression", "objects", "initial", nameof(ObjectProductForm));
            assets.CreateObject(authoredLab, "editor-tree");
            var product = ObjectProductForm.DataByNameProtouid[("editor-tree", 0)];
            Check(product.uid > 0 && product.protoUid == 0, "New creates an authored Object Product");
            Check(ReferenceEquals(ProductForm.DataByUid[product.uid], product), "Object participates in the shared Product registry");
            Check(product.icon == GlobalDefaultHelper.DefaultTexId && product.minimapIcon == GlobalDefaultHelper.DefaultTexId
                && !product.collision && !product.isWangTile && !product.boundsCollision && product.centerCollider
                && product.faceType == FaceType.Fixed, "New Product owns complete object defaults");
            Check(product.model != null && product.model.subPrefabUnitName.Count > 0
                && product.model.subPrefabUnitPos.Count > 0 && product.model.subPrefabUnitScale.Count > 0
                && product.events != null && product.animClip != null && product.GetAnimClip(AnimDirecton.Fixed) != null,
                "New Product initializes model, animation and event configuration");
            Check(product.paramDic[parameter.name].v == "12" && !ReferenceEquals(product.paramDic[parameter.name], parameter),
                "Object parameters copy defaults rather than sharing the parameter declaration");
            product.paramDic[parameter.name].v = "31";
            Check(parameter.v == "12", "Editing Product parameters cannot mutate shared declarations");

            Check(typeof(MapObjectForm.Data).GetProperties(BindingFlags.Public | BindingFlags.Instance)
                    .Select(property => property.Name).OrderBy(name => name)
                    .SequenceEqual(new[] { "icon", "id", "labId", "name", "objectUid" }),
                "MapObject exposes only its five list-entry fields");
            var row = MapObjectForm.DataById.Values.Single();
            var originalRow = row;
            int originalRowId = row.id;
            Check(row.objectUid == product.uid && row.name == product.name && row.icon == product.icon,
                "List entry links and displays its owning Product");
            Check(LabForm.DataById[product.labId].belong == nameof(ObjectProductForm)
                && LabForm.DataById[row.labId].belong == nameof(MapObjectForm)
                && row.labId != product.labId && LabForm.GetDisplayName(row.labId) == LabForm.GetDisplayName(product.labId),
                "Product and transient list row have distinct concrete-Form labels with matching paths");

            MapObjectForm.AddData(new MapObjectForm.Data(-1, "duplicate-list-row", 0, 0, product.uid));
            MapObjectForm.AddData(new MapObjectForm.Data(-1, "missing-product-row", 0, 0, int.MaxValue));
            assets.RefreshObjectList();
            Check(MapObjectForm.DataById.Count == 1
                && ReferenceEquals(MapObjectForm.DataById.Values.Single(), originalRow),
                "Refresh removes invalid/duplicate entries while preserving the original valid row");

            var runtime = product.Copy(false);
            runtime.ToProduct(product.uid);
            assets.RefreshObjectList();
            Check(ObjectProductForm.DataByUid.Count == 2 && MapObjectForm.DataById.Count == 1
                && MapObjectForm.DataById.Values.Single().objectUid == product.uid,
                "List rebuilding includes prototypes only, never runtime copies");

            product.name = "editor-bridge";
            product.icon = 987654;
            product.labId = LabForm.GetOrCreate("editor-regression", "objects", "updated", nameof(ObjectProductForm));
            product.collision = true;
            product.boundsCollision = true;
            product.centerCollider = false;
            product.model.subPrefabUnitScale[0] = new Vector3(2, 0.1f, 3);
            product.animClip[AnimDirecton.Fixed] = new List<int> { 876543 };
            assets.RefreshObjectList();
            row = MapObjectForm.DataById.Values.Single();
            Check(ReferenceEquals(row, originalRow) && row.id == originalRowId,
                "Refreshing a Product preserves the list-row reference and ID held by selection/brush state");
            Check(row.name == "editor-bridge" && row.icon == 987654 && row.objectUid == product.uid,
                "Rebuilding reflects changed Product display values without changing its identity");
            Check(LabForm.GetDisplayName(row.labId) == "editor-regression/objects/updated"
                && LabForm.DataById[row.labId].belong == nameof(MapObjectForm),
                "Changed Product labels are remapped to the list-row Form");
            Check(product.collision && product.boundsCollision && !product.centerCollider
                && product.model.subPrefabUnitScale[0] == new Vector3(2, 0.1f, 3)
                && product.animClip[AnimDirecton.Fixed].Single() == 876543,
                "Rebuilding list entries leaves Product-only configuration untouched");

            row.name = "display-only";
            row.icon = 456789;
            row.labId = 0;
            Check(product.name == "editor-bridge" && product.icon == 987654 && product.labId != 0,
                "Editing disposable row display fields cannot change Product data");
            assets.RefreshObjectList();
            Check(MapObjectForm.DataById.Values.Single().name == "editor-bridge",
                "Rebuilding discards row-only edits");

            assets.DeleteObject(product.name);
            Check(!ObjectProductForm.DataByUid.ContainsKey(product.uid) && MapObjectForm.DataById.Count == 0,
                "Deleting an authored Product removes its chooser row despite a remaining runtime copy");

            ObjectProductForm.Clear();
            MapObjectForm.Clear();
            assets.CreateObject(0, "second-story-object");
            var next = ObjectProductForm.DataByNameProtouid[("second-story-object", 0)];
            assets.RefreshObjectList();
            Check(ObjectProductForm.DataByUid.Count == 1 && MapObjectForm.DataById.Count == 1
                && MapObjectForm.DataById.Values.Single().objectUid == next.uid
                && MapObjectForm.DataById.Values.Single().name == "second-story-object",
                "Consecutive story clear/rebuild cannot retain previous Products or rows");
            Check(next.labId == 0 && MapObjectForm.DataById.Values.Single().labId == 0,
                "Unclassified labels remain zero on both Product and list row");
            ObjectProductForm.Clear();
            assets.RefreshObjectList();
            Check(MapObjectForm.DataById.Count == 0, "Rebuilding an empty story clears every old row");

            Debug.Log("OBJECT_PRODUCT_EDITOR_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            DynamicGlobalSettings.playing = previousPlaying;
            foreach (var manager in managers)
                if (manager != null) UnityEngine.Object.DestroyImmediate(manager);
            managers.Clear();
        }
    }

    private static void RegisterForms()
    {
        foreach (var type in new[] { typeof(ProductForm), typeof(ObjectProductForm), typeof(ParamForm),
            typeof(GameParamForm), typeof(MapObjectParamForm), typeof(MapBaseForm), typeof(MapObjectForm),
            typeof(MapModelForm), typeof(LabForm) })
            type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        ProductForm.InitInternal();
        ObjectProductForm.InitInternal();
        ParamForm.InitInternal();
        GameParamForm.InitInternal();
        MapObjectParamForm.InitInternal();
        MapBaseForm.InitInternal();
        MapObjectForm.InitInternal();
        MapModelForm.InitInternal();
        LabForm.InitInternal();
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject(typeof(T).Name);
        root.SetActive(false);
        managers.Add(root);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic)
            .SetValue(null, manager);
        return manager;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
