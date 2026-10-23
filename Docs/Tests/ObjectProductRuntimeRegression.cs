using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using Newtonsoft.Json.Linq;
using UnityEditor;
using UnityEngine;
using Z_DataSystem.Form;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

// Run only in the isolated project made by Run-MapRuntimeRegression.ps1.
public static class ObjectProductRuntimeRegression
{
    private static int checks;

    public static void Run()
    {
        string previousPath = SaveAndLoad.perPath;
        bool previousPlaying = DynamicGlobalSettings.playing;
        string root = Path.GetFullPath(Path.Combine(Application.dataPath,
            "../ObjectProductTest-" + Guid.NewGuid().ToString("N")));
        try
        {
            RegisterForms();
            Directory.CreateDirectory(root);
            SaveAndLoad.perPath = root;
            DynamicGlobalSettings.playing = true;
            var character = CharacterProductForm.defaultData;
            character.uid = 1;
            character.name = "reserved-character";
            CharacterProductForm.AddData(character);
            const int mapId = 71;
            var legacy = LegacyObject(mapId);
            SaveAndLoad.Save("Core/mof", new JArray(legacy).ToString());
            AddScene(81);
            AddScene(82);
            WriteScene("Core", 81, LegacyUnit(17, mapId, -1, "7"), LegacyUnit(18, mapId, -1, null));
            WriteScene("Core", 82, LegacyUnit(17, mapId, -1, "4"));
            WriteScene("Save", 81, LegacyUnit(17, mapId, -1, "7"));
            WriteScene("Cache", 81, LegacyUnit(17, mapId, -1, "7"), LegacyUnit(18, mapId, -1, null));
            WriteScene("Cache", 82, LegacyUnit(17, mapId, -1, "4"));
            string coreBefore = SaveAndLoad.Load<string>(ScenePath("Core", 81));
            string saveBefore = SaveAndLoad.Load<string>(ScenePath("Save", 81));
            var sentinel = ObjectUnitForm.GetDataByJo(LegacyUnit(89111, mapId, -1, null));
            ObjectUnitForm.AddData(sentinel);
            int unitCount = UnitForm.DataByUid.Count;
            var controller = new GameSaveController(null);
            controller.LoadObject("Core/");
            controller.LoadObjectProducts("Core/");
            var prototype = ObjectProductForm.ResolveLegacyTemplate(mapId);
            Check(prototype != null && prototype.uid != character.uid && prototype.protoUid == 0,
                "Legacy templates allocate Product UIDs after existing shared IDs are reserved");
            Check(MapObjectForm.DataById.Count == 0, "Legacy mof is not deserialized into transient chooser rows");
            Check(LabForm.DataById[prototype.labId].belong == nameof(ObjectProductForm),
                "Authored object labels belong to ObjectProductForm");
            Check(prototype.icon == 321 && !prototype.collision && prototype.minimapIcon == 322
                && prototype.isWangTile && prototype.boundsCollision && !prototype.centerCollider
                && prototype.faceType == FaceType.FourDirection,
                "Every removed MapObject configuration field migrates into the Product");
            Check(typeof(ObjectProductForm.Data).GetProperty("mapObjectId") == null
                && typeof(ObjectProductForm.Data).GetProperty("sceneUid") == null
                && typeof(ObjectProductForm.Data).GetProperty("sourceUnitUid") == null,
                "Product contains neither reverse MapObject identity nor scene provenance fields");

            string modJson = ObjectProductForm.MigrateSceneJson(coreBefore, false);
            Check(ObjectProductForm.DataByUid.Count == 1 && ProductUid(FirstObject(modJson)) == prototype.uid,
                "Mod in-memory migration references the authored Product without runtime copies");
            ObjectProductForm.PreparePlayStory("Cache/");
            var products = ObjectProductForm.ResolveReferences(prototype.uid);
            Check(products.Count == 3 && products.Select(product => product.uid).Distinct().Count() == 3,
                "All loaded and unloaded scene placements have independent Product UIDs");
            Check(products.All(product => product.protoUid == prototype.uid), "Runtime copies link only their prototype Product");
            Check(UnitForm.DataByUid.Count == unitCount && ReferenceEquals(UnitForm.DataByUid[sentinel.uid], sentinel),
                "Raw preparation never registers or clears live scene Units");
            Check(SaveAndLoad.Load<string>(ScenePath("Core", 81)) == coreBefore
                && SaveAndLoad.Load<string>(ScenePath("Save", 81)) == saveBefore,
                "Loading migration leaves Core and Save scene files untouched");
            var objects = ReadObjects("Cache", 81).OfType<JObject>().ToList();
            var one = ObjectProductForm.DataByUid[ProductUid(objects[0])];
            var two = ObjectProductForm.DataByUid[ProductUid(objects[1])];
            var offscene = ObjectProductForm.DataByUid[ProductUid(ReadObjects("Cache", 82).OfType<JObject>().Single())];
            Check(one.paramDic["Hp"].v == "7" && two.paramDic["Hp"].v == "10" && offscene.paramDic["Hp"].v == "4",
                "Per-placement legacy parameters override independent prototype defaults");
            Check(Extra(objects[0])[ObjectProductForm.ProductVersionKey].Value<int>() == 2
                && (int)Extra(objects[0])[MapUnit.productKey][1] == -1,
                "Versioned tuple contains only Product UID and reserved second value");
            Check((string)objects[0]["prefabName"] == GlobalDefaultHelper.GetRuntimeMapObjectPrefabName(prototype.uid),
                "Legacy prefab keys are upgraded to prototype Product UID");
            Check(!ReferenceEquals(one.model, prototype.model)
                && !ReferenceEquals(one.model.subUnitTexsName[0], prototype.model.subUnitTexsName[0])
                && !ReferenceEquals(one.events["test"], prototype.events["test"])
                && !ReferenceEquals(one.events["test"].evt, prototype.events["test"].evt)
                && !ReferenceEquals(one.animClip[AnimDirecton.Up], prototype.animClip[AnimDirecton.Up])
                && !ReferenceEquals(one.paramDic["Hp"], two.paramDic["Hp"]),
                "ToProduct deep-copies every mutable model, event, animation and parameter container");
            one.model.subUnitTexsName[0][0] = 900;
            one.events["test"].evt[0] = 901;
            one.animClip[AnimDirecton.Up][0] = 902;
            Check(prototype.model.subUnitTexsName[0][0] != 900 && two.events["test"].evt[0] != 901
                && prototype.animClip[AnimDirecton.Up][0] != 902,
                "Runtime configuration edits cannot leak into another Product");

            one.paramDic["Hp"].v = "99";
            offscene.paramDic["Hp"].v = "44";
            string cacheBefore = SaveAndLoad.Load<string>(ScenePath("Cache", 81));
            ObjectProductForm.PreparePlayStory("Cache/");
            Check(ObjectProductForm.DataByUid.Count == 4 && SaveAndLoad.Load<string>(ScenePath("Cache", 81)) == cacheBefore,
                "Repeated preparation is stable without provenance indexes or redundant writes");
            Check(one.paramDic["Hp"].v == "99" && offscene.paramDic["Hp"].v == "44",
                "Stale scene prm never overwrites existing Product state");
            var bound = ObjectUnitForm.GetDataByJo(objects[0]);
            Check(ReferenceEquals(ObjectProductForm.BindUnit(bound), one) && bound.unit.productInfo == (one.uid, -1),
                "Unit binding reads the direct Product link and preserves exact identity");
            bound.unit.paramInfo["Hp"].v = "23";
            Check(one.paramDic["Hp"].v == "23" && prototype.paramDic["Hp"].v == "10",
                "Live parameters are Product-owned and independent from authored defaults");
            var parameters = bound.unit.paramInfo;
            parameters["New"] = new GameParamForm.Data(-1, "New", ValType.String, "", "value", "");
            bound.unit.paramInfo = parameters;
            Check(one.paramDic["New"].v == "value", "Base parameter commands publish additions into the owning Product");

            AddScene(83);
            WriteScene("Cache", 83, Enumerable.Range(0, 128).Select(index => LegacyUnit(2000 + index, mapId, -1, null)).ToArray());
            ObjectProductForm.PreparePlayStory("Cache/");
            Check(ObjectProductForm.ResolveReferences(prototype.uid).Count == 131,
                "Product allocation exceeds the historical 99-ID shared capacity");
            controller.SaveObjectProducts("Core", false);
            controller.SaveObjectProducts("Save", true);
            var authored = JArray.Parse(SaveAndLoad.Load<string>("Core/oprf"));
            Check(authored.Count == 1 && authored[0]["model"] != null && authored[0]["mapObjectId"] == null
                && authored[0]["sceneUid"] == null && authored[0]["sourceUnitUid"] == null,
                "Core stores complete authored Product configuration without provenance");
            Check(!SaveAndLoad.Exist("Core/mof") && ProductUid(ReadObjects("Core", 81).OfType<JObject>().First()) == prototype.uid,
                "Explicit save upgrades unvisited source scenes before retiring obsolete mof");
            int oneUid = one.uid;
            int protoUid = prototype.uid;
            int offsceneUid = offscene.uid;
            controller.LoadObject("Save/");
            controller.LoadObjectProducts("Save/");
            Check(ObjectProductForm.DataByUid.Count == 132 && ObjectProductForm.ResolveLegacyTemplate(mapId) == null,
                "New-format save loads completely without mof or transient MapObject identities");
            var restored = ObjectProductForm.BindUnit(bound);
            Check(restored.uid == oneUid && restored.paramDic["Hp"].v == "23" && restored.paramDic["New"].v == "value",
                "Exact Product UID and live parameters survive persistence round trip");
            Check(ObjectProductForm.DataByUid[offsceneUid].paramDic["Hp"].v == "44"
                && ReferenceEquals(ProductForm.DataByUid[character.uid], character),
                "Unloaded scene state and unrelated Product tables survive reload");
            ObjectProductForm.PreparePlayStory("Cache/");
            Check(ObjectProductForm.DataByUid.Count == 132, "Reload and scene reentry retain all written Product links");
            ObjectProductForm.RemoveForUnit(bound);
            Check(!ObjectProductForm.DataByUid.ContainsKey(oneUid)
                && ObjectProductForm.DataByUid.ContainsKey(protoUid), "Explicit removal releases only the runtime Product");

            // Also cover the previous wrapper-only oprf schema while retaining
            // exact prototype/runtime UIDs and old per-instance parameter state.
            SaveAndLoad.Save("Legacy/mof", new JArray(legacy).ToString());
            SaveAndLoad.Save("Legacy/oprf", new JArray(
                Wrapper(8001, 0, mapId, "10"), Wrapper(8002, 8001, mapId, "66")).ToString());
            controller.LoadObject("Legacy/");
            controller.LoadObjectProducts("Legacy/");
            Check(ObjectProductForm.ResolveLegacyTemplate(mapId).uid == 8001
                && ObjectProductForm.DataByUid[8002].protoUid == 8001
                && ObjectProductForm.DataByUid[8002].paramDic["Hp"].v == "66"
                && ObjectProductForm.DataByUid[8002].model.subUnitTexsName[0][0] == 123,
                "Previous wrapper schema preserves IDs and parameters while importing full model configuration");
            var oldBound = ObjectUnitForm.GetDataByJo(LegacyUnit(17, mapId, 8002, "stale"));
            Check(ObjectProductForm.BindUnit(oldBound).uid == 8002 && oldBound.unit.productInfo == (8002, -1),
                "Legacy second tuple Product UID migrates without recopying stale parameters");
            controller.LoadObjectProducts(null);
            Check(ObjectProductForm.DataByUid.Count == 0 && ObjectProductForm.ResolveLegacyTemplate(mapId) == null
                && ReferenceEquals(ProductForm.DataByUid[character.uid], character),
                "Consecutive-story reset clears Products and legacy aliases without touching other Product tables");
            Debug.Log("OBJECT_PRODUCT_RUNTIME_REGRESSION_PASS checks=" + checks);
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
            SaveAndLoad.perPath = previousPath;
            if (Directory.Exists(root)) Directory.Delete(root, true);
        }
    }

    private static void RegisterForms()
    {
        foreach (var type in new[] { typeof(ProductForm), typeof(CharacterProductForm), typeof(ObjectProductForm),
            typeof(ParamForm), typeof(GameParamForm), typeof(MapObjectParamForm), typeof(MapBaseForm),
            typeof(MapModelForm), typeof(MapObjectForm), typeof(SceneForm), typeof(UnitForm), typeof(ObjectUnitForm),
            typeof(EventTriggerForm) })
            type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        ProductForm.InitInternal();
        CharacterProductForm.InitInternal();
        ObjectProductForm.InitInternal();
        MapBaseForm.InitInternal();
        MapObjectForm.InitInternal();
        SceneForm.InitInternal();
        UnitForm.InitInternal();
        ObjectUnitForm.InitInternal();
    }

    private static JObject LegacyObject(int mapId)
    {
        var data = ObjectProductForm.GetDataByJo(ObjectProductForm.GetJoByData(ObjectProductForm.defaultData));
        data.name = "test-tree";
        data.labId = LabForm.GetOrCreate("object-products-test", nameof(MapObjectForm));
        data.icon = 321;
        data.collision = false;
        data.minimapIcon = 322;
        data.isWangTile = true;
        data.boundsCollision = true;
        data.centerCollider = false;
        data.faceType = FaceType.FourDirection;
        data.paramDic["Hp"] = Parameter("10");
        data.model.subUnitTexsName = new List<List<int>> { new List<int> { 123 } };
        data.events["test"] = new EventTriggerForm.Data(-1, "test", new List<int> { 456 }, 0);
        data.animClip[AnimDirecton.Up] = new List<int> { 123 };
        var jo = ObjectProductForm.GetJoByData(data);
        jo.Remove("uid");
        jo.Remove("protoUid");
        jo["id"] = mapId;
        return jo;
    }

    private static JObject Wrapper(int uid, int protoUid, int mapId, string hp) => new JObject
    {
        ["uid"] = uid, ["name"] = "test-tree", ["protoUid"] = protoUid,
        ["mapObjectId"] = mapId, ["sceneUid"] = protoUid == 0 ? 0 : 81,
        ["sourceUnitUid"] = protoUid == 0 ? 0 : 17,
        ["paramDic"] = new JArray(new JObject { ["k"] = "Hp", ["v"] = MapObjectParamForm.GetJoByData(Parameter(hp)) })
    };

    private static void AddScene(int uid)
    {
        var scene = SceneForm.defaultData;
        scene.uid = uid;
        scene.name = "object-scene-" + uid;
        SceneForm.AddData(scene);
    }

    private static MapObjectParamForm.Data Parameter(string value) =>
        new MapObjectParamForm.Data(-1, "Hp", ValType.Float, "", value, "");

    private static JObject LegacyUnit(int uid, int mapId, int productUid, string hp)
    {
        var extra = new JObject { [MapUnit.productKey] = new JArray(mapId, productUid) };
        if (hp != null)
            extra[MapUnit.paramKey] = new JArray(new JObject
            { ["k"] = "Hp", ["v"] = MapObjectParamForm.GetJoByData(Parameter(hp)) });
        return new JObject { ["uid"] = uid, ["extra"] = extra.ToString(), ["name"] = "placed-tree" };
    }

    private static JObject Extra(JObject unit) => JObject.Parse((string)unit["extra"]);
    private static int ProductUid(JObject unit) => (int)Extra(unit)[MapUnit.productKey][0];
    private static JObject FirstObject(string json) =>
        JArray.Parse((string)JObject.Parse(json)["objectJa"]).OfType<JObject>().First();
    private static string ScenePath(string folder, int uid) => folder + "/" + Main2StoryManager.GetSceneFileNameById(uid);
    private static void WriteScene(string folder, int uid, params JObject[] objects) =>
        SaveAndLoad.Save(ScenePath(folder, uid), new JObject
        { ["objectJa"] = new JArray(objects).ToString(), ["sentinel"] = "keep" }.ToString());
    private static JArray ReadObjects(string folder, int uid) =>
        JArray.Parse((string)JObject.Parse(SaveAndLoad.Load<string>(ScenePath(folder, uid)))["objectJa"]);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
