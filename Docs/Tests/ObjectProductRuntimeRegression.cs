using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

// Current-schema persistence and extra-cache tests in an isolated Unity project.
public static class ObjectProductRuntimeRegression
{
    private static int checks;
    public static void Run()
    {
        string previousPath = SaveAndLoad.perPath;
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            foreach (var type in new[] { typeof(ProductForm), typeof(ObjectProductForm), typeof(MapModelForm),
                typeof(ParamForm), typeof(GameParamForm), typeof(MapObjectParamForm), typeof(EventTriggerForm),
                typeof(MapBaseForm), typeof(SceneForm), typeof(UnitForm), typeof(ObjectUnitForm) })
                type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
            ProductForm.InitInternal(); ObjectProductForm.InitInternal(); SceneForm.InitInternal();
            UnitForm.InitInternal(); ObjectUnitForm.InitInternal(); GameParamForm.RegisterGet();
            SaveAndLoad.perPath = Path.GetFullPath(Path.Combine(Application.dataPath, "../CurrentObjectProducts-" + Guid.NewGuid().ToString("N")));
            Directory.CreateDirectory(SaveAndLoad.perPath);
            Persistence();
            ExtraCaches();
            Debug.Log("OBJECT_PRODUCT_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        finally { SaveAndLoad.perPath = previousPath; DynamicGlobalSettings.playing = previousPlaying; }
    }

    private static ObjectProductForm.Data Product(int uid, float hp)
    {
        var row = ObjectProductForm.defaultData;
        row.uid = uid; row.name = "product-" + uid;
        row.paramDic["Hp"] = Parameter(hp);
        row.model.subUnitTexsName = new List<List<int>> { new List<int> { 123 } };
        row.animClip[AnimDirecton.Up] = new List<int> { 123 };
        row.events["test"] = new EventTriggerForm.Data(-1, "test", new List<int> { 11 }, default);
        ObjectProductForm.AddData(row);
        return row;
    }
    private static MapObjectParamForm.Data Parameter(float hp) => new MapObjectParamForm.Data(-1, "Hp", ValType.Float,
        "", BoxDataForm.GetJoByData(CodeHelper.CreateBoxByNum(hp)).ToString(), "");
    private static JObject Extra(int uid, float? hp = null)
    {
        var extra = new JObject { [MapUnit.productKey] = new JArray(uid, -1) };
        if (hp.HasValue) extra[MapUnit.paramKey] = new JArray(new JObject { ["k"] = "Hp", ["v"] = MapObjectParamForm.GetJoByData(Parameter(hp.Value)) });
        return extra;
    }
    private static ObjectUnitForm.Data Unit(int uid, string extra, bool register = true)
    {
        var row = new ObjectUnitForm.Data(uid, false, "unit-" + uid, "", Vector3.zero, Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), extra, false, 0);
        if (register) ObjectUnitForm.AddData(row);
        return row;
    }
    private static void WriteScene(string folder, int sceneUid, params JObject[] rows) => SaveAndLoad.Save(
        folder + "/" + Main2StoryManager.GetSceneFileNameById(sceneUid), new JObject { ["objectJa"] = new JArray(rows).ToString(), ["sentinel"] = "keep" }.ToString());
    private static JArray Rows(string folder, int sceneUid) => JArray.Parse((string)JObject.Parse(SaveAndLoad.Load<string>(
        folder + "/" + Main2StoryManager.GetSceneFileNameById(sceneUid)))["objectJa"]);
    private static int ProductUid(JToken row) => (int)JObject.Parse((string)row["extra"])[MapUnit.productKey][0];
    private static float Hp(ObjectProductForm.Data row) => row.paramDic["Hp"].GetValue().num;

    private static void Persistence()
    {
        var source = Product(85001, 10);
        foreach (int uid in new[] { 81, 82 }) { var scene = SceneForm.defaultData; scene.uid = uid; SceneForm.AddData(scene); }
        var one = ObjectUnitForm.GetJoByData(Unit(86001, Extra(source.uid, 7).ToString(), false));
        var two = ObjectUnitForm.GetJoByData(Unit(86002, Extra(source.uid).ToString(), false));
        WriteScene("Core", 81, one, two); WriteScene("Core", 82, one);
        WriteScene("Cache", 81, one, two); WriteScene("Cache", 82, one);
        string core = SaveAndLoad.Load<string>("Core/81");
        var controller = new GameSaveController(null);
        controller.SaveObjectProducts("Core", false);
        int liveUnits = UnitForm.DataByUid.Count;
        DynamicGlobalSettings.playing = true;
        controller.PrepareObjectProducts("Cache");
        var placements = Rows("Cache", 81);
        var a = ObjectProductForm.DataByUid[ProductUid(placements[0])];
        var b = ObjectProductForm.DataByUid[ProductUid(placements[1])];
        var offscene = ObjectProductForm.DataByUid[ProductUid(Rows("Cache", 82)[0])];
        Check(a.uid != b.uid && a.uid != offscene.uid && new[] { a, b, offscene }.All(x => x.protoUid == source.uid), "every current placement owns a separately registered Product");
        Check(Hp(a) == 7 && Hp(b) == 10 && Hp(offscene) == 7 && Hp(source) == 10, "placement parameter overrides merge only into the newly created Product");
        Check(UnitForm.DataByUid.Count == liveUnits && SaveAndLoad.Load<string>("Core/81") == core, "raw Cache preparation neither registers live Units nor rewrites Core");
        Check(!ReferenceEquals(a.model, source.model) && !ReferenceEquals(a.model.subUnitTexsName[0], source.model.subUnitTexsName[0])
            && !ReferenceEquals(a.events["test"], b.events["test"]) && !ReferenceEquals(a.animClip[AnimDirecton.Up], b.animClip[AnimDirecton.Up]), "generated Copy owns all nested placement configuration");
        a.paramDic["Hp"].SetValue(99); offscene.paramDic["Hp"].SetValue(44);
        string cache = SaveAndLoad.Load<string>("Cache/81"); int count = ObjectProductForm.DataByUid.Count;
        controller.PrepareObjectProducts("Cache");
        Check(ObjectProductForm.DataByUid.Count == count && SaveAndLoad.Load<string>("Cache/81") == cache && Hp(a) == 99, "preparation is idempotent and does not reapply stale placement values");
        var bound = ObjectUnitForm.GetDataByJo((JObject)placements[0]);
        Check(bound.unit.product == a && bound.unit.BindProduct() == a, "a Unit resolves its Product directly through productInfo and the Form dictionary");
        var parameters = bound.unit.paramInfo;
        parameters["new"] = new GameParamForm.Data(-1, "new", ValType.String, "", "", "");
        bound.unit.paramInfo = parameters;
        Check(a.paramDic.ContainsKey("new"), "the parameter setter publishes new keys to the owning Product");
        controller.SaveObjectProducts("Save", true);
        Check(JArray.Parse(SaveAndLoad.Load<string>("Core/oprf")).Count == 1 && JArray.Parse(SaveAndLoad.Load<string>("Save/oprf")).Count == count,
            "Core saves authored Products and Save retains all runtime Products");
        int aUid = a.uid, offsceneUid = offscene.uid;
        controller.LoadObjectProducts("Save/");
        Check(bound.unit.product.uid == aUid && Hp(bound.unit.product) == 99 && Hp(ObjectProductForm.DataByUid[offsceneUid]) == 44,
            "save/load retains Product identity and current parameter values, including unloaded scenes");
        controller.PrepareObjectProducts("Cache");
        Check(ObjectProductForm.DataByUid.Count == count, "scene preparation after reloading reuses existing runtime bindings");
        controller.LoadObjectProducts(null);
        Check(ObjectProductForm.DataByUid.Count == 0 && bound.unit.product == null, "reset clears Products and direct lookup immediately stops resolving them");
    }

    private static void ExtraCaches()
    {
        DynamicGlobalSettings.playing = false;
        var a = Product(85101, 10); var b = Product(85102, 20);
        var extra = Extra(a.uid, 5);
        extra[MapUnit.evtKey] = EventEntries(22);
        var row = Unit(86101, extra.ToString()); var unit = row.unit;
        var oldParameters = unit.paramInfo;
        Check(unit.product == a && unit.GetEvt("test").evt.SequenceEqual(new[] { 11, 22 }), "prime Product, parameter and combined event caches");
        var fields = BindingFlags.Instance | BindingFlags.NonPublic;
        extra[MapUnit.productKey] = new JArray(b.uid, -1);
        extra[MapUnit.paramKey] = new JArray(new JObject { ["k"] = "Hp", ["v"] = MapObjectParamForm.GetJoByData(Parameter(77)) });
        extra[MapUnit.evtKey] = EventEntries(33);
        row.extra = extra.ToString();
        Check(!(bool)typeof(MapUnit).GetField("productInfoLoaded", fields).GetValue(unit)
            && typeof(MapUnit).GetField("_paramInfo", fields).GetValue(unit) == null
            && typeof(MapUnit).GetField("_evtSet", fields).GetValue(unit) == null
            && typeof(MapUnit).GetField("combinedEvents", fields).GetValue(unit) == null,
            "Form changeExtraAction synchronously clears every derived Unit cache");
        Check(unit.product == b && unit.productInfo == (b.uid, -1) && !ReferenceEquals(oldParameters, unit.paramInfo)
            && unit.paramInfo["Hp"].GetValue().num == 77 && unit.GetEvt("test").evt.SequenceEqual(new[] { 11, 33 }),
            "all wrappers rebuild from the replaced extra rather than stale values");
        Check(Hp(a) == 10 && Hp(b) == 20, "clearing caches does not clear or mutate real Product parameters");
        DynamicGlobalSettings.playing = true;
        var runtime = unit.BindProduct();
        Check(runtime.protoUid == b.uid && Hp(runtime) == 77 && Hp(b) == 20, "Play binding creates an independent Product with current placement overrides");
        runtime.paramDic["Hp"].SetValue(123);
        var projection = unit.paramInfo;
        extra = JObject.Parse(row.extra); extra["note"] = "changed"; row.extra = extra.ToString();
        Check(unit.product == runtime && Hp(runtime) == 123 && !ReferenceEquals(projection, unit.paramInfo)
            && ReferenceEquals(unit.paramInfo["Hp"], runtime.paramDic["Hp"]), "extra invalidation discards only projections while live Product values survive");
        unit.productInfo = (a.uid, -1);
        Check(unit.product == a && (int)JObject.Parse(row.extra)[MapUnit.productKey][0] == a.uid, "productInfo assignment writes its canonical binding back to extra");
        ObjectUnitForm.RemoveData(row.uid);
    }
    private static JArray EventEntries(int uid) => new JArray(new JObject { ["k"] = "test",
        ["v"] = EventTriggerForm.GetJoByData(new EventTriggerForm.Data(-1, "test", new List<int> { uid }, default)) });
    private static void Check(bool success, string message) { if (!success) throw new InvalidOperationException(message); checks++; }
}
