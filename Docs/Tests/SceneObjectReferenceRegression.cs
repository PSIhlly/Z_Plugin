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
using Z_DataSystem;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;

// Isolated helper/command regression; never load a production scene or save.
public static class SceneObjectReferenceRegression
{
    private static int checks;
    private const string ParameterName = "reference-value";

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            // RuntimeInitialize hooks do not run automatically in this EditMode fixture.
            foreach (var form in new[] { typeof(ObjectProductForm), typeof(MapModelForm), typeof(EventTriggerForm),
                typeof(GameParamForm), typeof(MapObjectParamForm), typeof(EventInterpretDataForm),
                typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm) })
                form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            typeof(SceneObjectCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            typeof(IsSameObjectCmd).GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            Check(CmdBase.TryGetForm("SceneObject", out var canonical) && canonical.name == "SceneObject",
                "Object constant resolves canonical metadata");
            Check(GameCmdDataForm.DataByName["GenerateObject"].prmTypes[0] == "sceneObject"
                && !GameCmdTypeDataForm.DataByName.ContainsKey("sceneObjectProto")
                && GameCmdTypeDataForm.defaultData.name == "void",
                "unified Object argument metadata retains the void default and removes the old separate type");
            Check(CmdBase.TryGetForm("IsSameObject", out var sameForm)
                && sameForm is GameCmdDataForm.Data gameSameForm && gameSameForm.labId == 10016
                && sameForm.prmTypes.SequenceEqual(new[] { "sceneObject", "sceneObject" })
                && sameForm.retNames.Count == 1 && sameForm.retTypes.SequenceEqual(new[] { "num" }),
                "IsSameObject is registered with two Object arguments and one numeric result");
            var game = CreateManager<GameManager>();
            game.evtCtrl = new GameEventController(game);
            var map = CreateManager<MapManager>();
            map.data = new MapInfo
            {
                mainData = new MapMainForm.Data(1, Vector3.one, Vector3Int.one, Vector3Int.one, "", "", "", ""),
                maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
                mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
            };
            map.utilCtrl = new MapUtilController(map);
            map.updateCtrl = new MapUpdateController(map);
            var pool = CreateManager<InstancePoolManager>();
            pool.Init();
            game.mapCtrl = new GameMapController(game);
            GameParamForm.RegisterGet();
            MapObjectForm.InitInternal();
            ObjectProductForm.InitInternal();
            ObjectUnitForm.InitInternal();
            UnitForm.InitInternal();
            EventInterpretDataForm.InitInternal();
            MapObjectParamForm.InitInternal();
            DynamicGlobalSettings.playing = true;

            const int collisionId = 98101;
            var template = Template(collisionId, 7);
            var prototype = Product(98110, 0, template.id, 0, 7);
            // These IDs intentionally coincide across three independent pools.
            var productA = Product(collisionId, prototype.uid, template.id, collisionId, 11);
            var productB = Product(98102, prototype.uid, template.id, 98103, 22);
            var unitA = Unit(collisionId, template.id, productA.uid);
            var unitB = Unit(98103, template.id, productB.uid);
            var otherTemplate = Template(98105, 55);
            var otherPrototype = Product(98112, 0, otherTemplate.id, 0, 55);
            var otherProduct = Product(98113, otherPrototype.uid, otherTemplate.id, 98106, 66);
            var otherUnit = Unit(98106, otherTemplate.id, otherProduct.uid);
            string numeric = GlobalEventHelper.GetSceneUnitReference(collisionId);
            string authored = GlobalEventHelper.GetSceneObjectProductReference(prototype.uid);
            string exact = GlobalEventHelper.GetSceneObjectReference(unitA.unit);
            string prototypeReference = GlobalEventHelper.GetSceneObjectProductReference(prototype.uid);
            string siblingReference = GlobalEventHelper.GetSceneObjectProductReference(productB.uid);
            string otherReference = GlobalEventHelper.GetSceneObjectProductReference(otherProduct.uid);

            foreach (string first in new[] { exact, numeric })
            {
                foreach (string second in new[] { authored, prototypeReference, siblingReference,
                    GlobalEventHelper.GetSceneUnitReference(unitB.uid) })
                    Check(Same(first, second) == 1f,
                        "Object prototype comparison accepts prototype and sibling references: " + first + "/" + second);
                Check(Same(first, otherReference) == 0f, "different Object prototypes do not match");
            }
            Check(Same(exact, exact) == 1f, "the same runtime Object shares its own prototype");
            Check(Same(prototypeReference, prototypeReference) == 0f
                && Same(prototypeReference, GlobalEventHelper.GetSceneObjectProductReference(otherPrototype.uid)) == 0f,
                "missing source prototype does not match itself or another zero prototype");
            Check(prototype.protoUid == 0 && productA.protoUid == prototype.uid && productB.protoUid == prototype.uid,
                "Object prototype comparison does not mutate Products");
            var sameProgram = ProgramDataForm.defaultData;
            sameProgram.name = "IsSameObjectRegression";
            Check(sameProgram.TryApplyCode("Return IsSameObject(\"" + exact + "\",\"" + prototypeReference + "\");",
                    out _, out _), "the registered command compiles in an event expression");
            var sameExecution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
                new Dictionary<string, BoxDataForm.Data>(), sameProgram, 0, -1, 0, null,
                new List<BoxDataForm.Data>(), 0);
            var sameResult = sameExecution.Interpret();
            Check(sameResult.complete && sameResult.errors.Count == 0 && sameResult.ret.num == 1f
                && sameExecution.stack.Count == 0, "interpreter preserves argument order and returns the comparison");

            CheckKind(numeric, GlobalEventHelper.ObjectReferenceKind.Unit, collisionId);
            CheckKind("$so$" + collisionId + "$so$", GlobalEventHelper.ObjectReferenceKind.Product, collisionId);
            CheckKind(authored, GlobalEventHelper.ObjectReferenceKind.Product, prototype.uid);
            CheckKind(exact, GlobalEventHelper.ObjectReferenceKind.Product, productA.uid);
            Check(exact == "$so$" + productA.uid + "$so$", "new Object references contain only the Product UID");
            Check(GlobalEventHelper.ResolveSceneObjectUnits(exact)
                .SequenceEqual(new[] { unitA.unit }), "canonical Product handles resolve exactly");
            Check(GlobalEventHelper.ResolveSceneObjectUnits(numeric).SequenceEqual(new[] { unitA.unit }),
                "explicit Unit reference stays exact despite matching template/Product IDs");
            foreach (string templateReference in new[] { authored, prototypeReference })
            {
                Check(GlobalEventHelper.ResolveSceneObjectUnits(templateReference)
                    .SequenceEqual(new MapUnit[] { unitA.unit, unitB.unit }),
                    "prototype references expand every matching live placement");
                Check(GlobalEventHelper.ResolveSceneObjectProducts(templateReference)
                    .SequenceEqual(new[] { productA, productB }),
                    "template Product expansion excludes authored prototype and unrelated Products");
                Check(GlobalEventHelper.ResolveSceneObjectTemplate(templateReference) == prototype,
                    "generation resolves template aliases and Product prototypes");
            }
            Check(GlobalEventHelper.ResolveSceneObjectUnits(exact).SequenceEqual(new[] { unitA.unit })
                && GlobalEventHelper.ResolveSceneObjectProducts(exact).SequenceEqual(new[] { productA }),
                "exact runtime Product reference never expands to sibling placements");
            Check(GlobalEventHelper.ResolveSceneObjectTemplate(exact) == productA
                && GlobalEventHelper.ResolveSceneObjectTemplate(numeric) == productA,
                "GenerateObject obtains actual configuration from precise Product and explicit Unit handles");
            IndexedSceneLookup(prototype, productA, productB, unitA, unitB, otherUnit);

            foreach (string malformed in new[] { null, "", "$so$$so$", "$sop$$sop$", "$so$p:x$so$",
                "$so$t:0$so$", "$so$p:98101$so$", "$sop$1$sop$", "$so$p:-1$so$", "$so$98101$so$tail", "$so$99999999999999999999$so$", "$ch$98101$ch$" })
            {
                Check(!GlobalEventHelper.TryGetSceneObjectReference(malformed, out _, out _)
                    && GlobalEventHelper.ResolveSceneObjectUnits(malformed).Count == 0
                    && GlobalEventHelper.ResolveSceneObjectProducts(malformed).Count == 0
                    && GlobalEventHelper.ResolveSceneObjectTemplate(malformed) == null,
                    "malformed scene-object reference is safely rejected: " + malformed);
                Invoke(new SetSceneObjectParameterCmd(), malformed, 99);
                Invoke(new DestroyObjectCmd(), malformed);
                Check(Invoke(new IsObjectActiveCmd(), malformed).res[0].num == 0,
                    "malformed scene-object activity query safely returns zero");
                Check(Same(malformed, exact) == 0f && Same(exact, malformed) == 0f,
                    "malformed Object comparison argument safely returns zero");
            }

            Check(Get(authored) == 11, "parameter getter chooses the first current live Object Product");
            Set(authored, 30);
            Check(Value(productA) == 30 && Value(productB) == 30 && Value(prototype) == 7
                && Value(otherProduct) == 66,
                "template parameter mutation fans out stored Products without modifying templates or unrelated Objects");
            Check(Get(numeric) == 30 && Get(exact) == 30,
                "parameter refresh makes legacy Unit and canonical Product reads agree");
            Set(exact, 40);
            Check(Value(productA) == 40 && Value(productB) == 30
                && !ReferenceEquals(productA.paramDic[ParameterName], productB.paramDic[ParameterName]),
                "exact Product mutation updates just its independently owned parameters");
            var savedProduct = ObjectProductForm.GetDataByJo(ObjectProductForm.GetJoByData(productA));
            Check(Value(savedProduct) == 40 && !ReferenceEquals(savedProduct.paramDic[ParameterName], productA.paramDic[ParameterName]),
                "cached parameter mutations serialize and deserialize as independent Product data");

            // The authoring identity must work before a scene's Unit rows exist.
            ObjectUnitForm.RemoveData(unitA.uid);
            ObjectUnitForm.RemoveData(unitB.uid);
            Check(Same(exact, prototypeReference) == 1f && Same(exact, siblingReference) == 1f
                && Same(numeric, prototypeReference) == 0f,
                "persistent Product comparison works without loaded Units while removed Unit handles fail");
            Check(GlobalEventHelper.ResolveSceneObjectUnits(authored).Count == 0 && Get(authored) == 40,
                "parameter getter falls back to stored runtime Product before scene load");
            Set(authored, 50);
            Check(Value(productA) == 50 && Value(productB) == 50,
                "template parameter updates work for stored Products in an unloaded scene");
            ObjectUnitForm.AddData(unitA);
            ObjectUnitForm.AddData(unitB);
            Check(Get(authored) == 50 && Invoke(new IsObjectActiveCmd(), authored).res[0].num == 1,
                "reloaded Unit rows immediately resolve their persistent Products");

            var emptyTemplate = Template(98114, 81);
            var emptyPrototype = Product(98115, 0, emptyTemplate.id, 0, 81);
            Check(Get(GlobalEventHelper.GetSceneObjectProductReference(emptyPrototype.uid)) == 81,
                "read-only template parameter is available when no runtime copy exists");
            Set(GlobalEventHelper.GetSceneObjectProductReference(emptyPrototype.uid), 100);
            Check(Value(emptyPrototype) == 81,
                "mutating an uninstantiated template is a no-op");

            var snapshot = GlobalEventHelper.ResolveSceneObjectUnits(authored);
            UnitForm.DataByUid[unitA.uid] = otherUnit;
            Check(GlobalEventHelper.ResolveSceneObjectUnits(authored).SequenceEqual(new[] { unitB.unit })
                && snapshot.Count == 2 && ReferenceEquals(snapshot[0], unitA.unit),
                "new resolution rejects stale root Data identity while a captured snapshot stays stable");
            UnitForm.DataByUid[unitA.uid] = unitA;
            ObjectUnitForm.RemoveData(unitA.uid);
            var reused = Unit(unitA.uid, otherTemplate.id, otherProduct.uid);
            Check(Same(numeric, prototypeReference) == 0f && Same(exact, prototypeReference) == 1f,
                "overlapping/reused Unit and Product UIDs keep their distinct comparison meanings");
            Check(GlobalEventHelper.ResolveSceneObjectUnits(exact).Count == 0
                && GlobalEventHelper.ResolveSceneObjectUnit(numeric) == reused.unit,
                "reused Unit UID cannot revive the old Product reference; explicit Unit handle retains scene semantics");
            ObjectUnitForm.RemoveData(reused.uid);
            ObjectUnitForm.AddData(unitA);

            Invoke(new DestroyObjectCmd(), authored);
            Check(!ObjectUnitForm.DataByUid.ContainsKey(unitA.uid) && !ObjectUnitForm.DataByUid.ContainsKey(unitB.uid)
                && !ObjectProductForm.DataByUid.ContainsKey(productA.uid) && !ObjectProductForm.DataByUid.ContainsKey(productB.uid),
                "template destruction removes all live placements and their runtime Products");
            Check(MapObjectForm.DataById.ContainsKey(template.id) && ObjectProductForm.DataByUid.ContainsKey(prototype.uid)
                && ObjectUnitForm.DataByUid.ContainsKey(otherUnit.uid) && ObjectProductForm.DataByUid.ContainsKey(otherProduct.uid),
                "template destruction preserves authored data/prototype and unrelated placements");
            Invoke(new DestroyObjectCmd(), authored);
            Check(Same(exact, prototypeReference) == 0f, "removed Product references no longer match a prototype");
            Check(Invoke(new IsObjectActiveCmd(), authored).res[0].num == 0,
                "repeated destruction is a harmless no-op");

            Debug.Log("SCENE_OBJECT_REFERENCE_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally { DynamicGlobalSettings.playing = previousPlaying; }
    }

    private static void IndexedSceneLookup(ObjectProductForm.Data prototype, ObjectProductForm.Data productA,
        ObjectProductForm.Data productB, ObjectUnitForm.Data unitA, ObjectUnitForm.Data unitB, ObjectUnitForm.Data unrelated)
    {
        const BindingFlags hidden = BindingFlags.Static | BindingFlags.NonPublic;
        var index = (Dictionary<int, List<ObjectUnitForm.Data>>)typeof(ObjectProductForm)
            .GetField("sceneUnitsByProduct", hidden).GetValue(null);
        ObjectProductForm.GetSceneUnit(productA.uid);
        var group = index[productA.uid];
        string exact = GlobalEventHelper.GetSceneObjectProductReference(productA.uid);
        string authored = GlobalEventHelper.GetSceneObjectProductReference(prototype.uid);
        bool correct = true;
        for (int i = 0; i < 2000; i++)
            correct &= GlobalEventHelper.ResolveSceneObjectUnit(exact) == unitA.unit
                && GlobalEventHelper.ResolveSceneObjectUnit(authored) == unitA.unit;
        Check(correct && ReferenceEquals(group, index[productA.uid]),
            "warm exact/prototype lookups reuse Product-to-Unit groups");

        var duplicate = Unit(98099, 0, productA.uid);
        var snapshot = GlobalEventHelper.ResolveSceneObjectUnits(exact);
        Check(snapshot.SequenceEqual(new MapUnit[] { duplicate.unit, unitA.unit })
            && GlobalEventHelper.ResolveSceneObjectUnit(exact) == duplicate.unit,
            "incremental addition preserves multiple placements and minimum-UID scalar selection");
        ObjectUnitForm.RemoveData(duplicate.uid);
        Check(ObjectProductForm.GetSceneUnit(productA.uid) == unitA && snapshot.Count == 2,
            "removal updates the index without mutating earlier snapshots");

        unitA.unit.productInfo = (productB.uid, -1);
        Check(ObjectProductForm.GetSceneUnit(productA.uid) == null
            && GlobalEventHelper.ResolveSceneObjectUnits(GlobalEventHelper.GetSceneObjectProductReference(productB.uid))
                .SequenceEqual(new MapUnit[] { unitA.unit, unitB.unit }), "explicit productInfo rebinding updates both Product groups");
        unitA.unit.BindProduct(productA);
        var extra = JObject.Parse(unitA.extra);
        extra[MapUnit.productKey] = new JArray(productB.uid, -1);
        unitA.extra = extra.ToString();
        Check(ObjectProductForm.GetSceneUnit(productA.uid) == null && ObjectProductForm.GetSceneUnit(productB.uid) == unitA,
            "raw extra replacement also refreshes just the changed Unit's binding");
        unitA.unit.BindProduct(productA);
        Check(ObjectProductForm.GetSceneUnit(productA.uid) == unitA && ObjectProductForm.GetSceneUnit(productB.uid) == unitB,
            "normal assignment restores both indexed groups");

        var lateUnit = Unit(98201, 0, 98200);
        ObjectProductForm.GetSceneUnit(productA.uid); // Index the not-yet-registered canonical Product UID.
        var lateProduct = Product(98200, prototype.uid, 0, lateUnit.uid, 12);
        Check(ObjectProductForm.GetSceneUnit(lateProduct.uid) == lateUnit && lateUnit.unit.product == lateProduct,
            "a canonical binding loaded before its Product becomes resolvable without rebuilding every Unit");
        ObjectProductForm.RemoveData(lateProduct.uid);
        var replacement = Product(98200, prototype.uid, 0, lateUnit.uid, 13);
        Check(ObjectProductForm.GetSceneUnit(replacement.uid) == lateUnit,
            "same-UID Product replacement uses the current Product table row");
        ObjectUnitForm.RemoveData(lateUnit.uid);
        ObjectProductForm.RemoveData(replacement.uid);
        ObjectProductForm.ResetSceneUnits();
        Check(ObjectProductForm.GetSceneUnit(productA.uid) == unitA, "story/reference reset rebuilds scene groups from current bindings");

    }

    private static MapObjectForm.Data Template(int id, float value)
    {
        var data = MapObjectForm.defaultData;
        data.id = id;
        data.name = "reference-template-" + id;
        MapObjectForm.AddData(data);
        return data;
    }

    private static ObjectProductForm.Data Product(int uid, int protoUid, int templateId, int unitUid, float value)
    {
        var data = ObjectProductForm.defaultData;
        data.uid = uid;
        data.name = "reference-product-" + uid;
        data.protoUid = protoUid;
        data.paramDic = Parameters(value);
        ObjectProductForm.AddData(data);
        if (protoUid == 0)
        {
            MapObjectForm.DataById[templateId].objectUid = uid;
        }
        return data;
    }

    private static Dictionary<string, MapObjectParamForm.Data> Parameters(float value) =>
        new Dictionary<string, MapObjectParamForm.Data>
        {
            [ParameterName] = new MapObjectParamForm.Data(-1, ParameterName, 0, "",
                BoxDataForm.GetJoByData(CodeHelper.CreateBoxByNum(value)).ToString(), "")
        };

    private static ObjectUnitForm.Data Unit(int uid, int templateId, int productUid)
    {
        var data = new ObjectUnitForm.Data(uid, true, "reference-object", "unused-reference-object", Vector3.zero,
            Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(),
            MapUnit.GetProductInfoString(new JObject(),
                (productUid, -1)), false, 0);
        ObjectUnitForm.AddData(data);
        return data;
    }

    private static float Value(ObjectProductForm.Data data) => data.paramDic[ParameterName].GetValue().num;
    private static float Same(string first, string second)
    {
        var task = new InterpretAsyncTask(null);
        // CmdBase accepts the interpreter's reverse-stack argument order.
        new IsSameObjectCmd().Execute(new[] { CodeHelper.CreateBoxByStr(second), CodeHelper.CreateBoxByStr(first) },
            new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && string.IsNullOrEmpty(task.error) && task.res?.Length == 1,
            "Object comparison returns one value synchronously");
        return task.res[0].num;
    }
    private static float Get(string reference) => Invoke(new GetSceneObjectParameterCmd(), reference).res[0].num;
    private static void Set(string reference, float value) => Invoke(new SetSceneObjectParameterCmd(), reference, value);

    private static InterpretAsyncTask Invoke(CmdBase command, string reference, float value = 0)
    {
        var task = new InterpretAsyncTask(null);
        var arguments = new[] { CodeHelper.CreateBoxByStr(reference), CodeHelper.CreateBoxByStr(ParameterName),
            CodeHelper.CreateBoxByNum(value) };
        command.GetType().GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic)
            .Invoke(command, new object[] { arguments, task });
        checks++;
        return task;
    }

    private static void CheckKind(string reference, GlobalEventHelper.ObjectReferenceKind expected, int id)
    {
        Check(GlobalEventHelper.TryGetSceneObjectReference(reference, out var kind, out int parsed)
            && kind == expected && parsed == id, "parse discriminated reference " + reference);
    }

    private static T CreateManager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject(typeof(T).Name);
        root.SetActive(false);
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
