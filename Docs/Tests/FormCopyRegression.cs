using System;
using System.Collections.Generic;
using System.Diagnostics;
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
using Z_UnitSystem.Form;
using Debug = UnityEngine.Debug;

public static class FormCopyRegression
{
    private static int checks;
    private static object retained;

    public static void Run()
    {
        try
        {
            foreach (var type in new[] { typeof(ProductForm), typeof(ParamForm), typeof(GameParamForm),
                typeof(MapObjectParamForm), typeof(ObjectProductForm), typeof(MapModelForm), typeof(EventTriggerForm),
                typeof(BoxDataForm), typeof(UnitForm), typeof(ObjectUnitForm) })
                type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
            GameParamForm.RegisterGet();
            NestedCopies();
            Cycles();
            Binding();
            Benchmark();
            Debug.Log("FORM_COPY_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
    }

    private static ObjectProductForm.Data Source()
    {
        var data = ObjectProductForm.defaultData;
        data.name = "copy-test";
        data.model.subUnitTexsName = new List<List<int>> { new List<int> { 1, 2, 3 }, null };
        data.events["test"] = new EventTriggerForm.Data(0, "test", new List<int> { 11, 12 }, default);
        data.animClip[AnimDirecton.Up] = new List<int> { 21, 22 };
        data.modelSizes[AnimDirecton.Up] = new Vector3(2, 3, 4);
        data.modelPositions[AnimDirecton.Up] = new Vector3(5, 6, 7);
        var box = CodeHelper.CreateBox();
        box.dic = new Dictionary<string, BoxDataForm.Data> { ["live"] = CodeHelper.CreateBoxByNum(31) };
        data.paramDic["value"] = new MapObjectParamForm.Data(0, "value", ValType.Float, "",
            BoxDataForm.GetJoByData(box).ToString(), "");
        return data;
    }

    private static void NestedCopies()
    {
        var source = Source();
        var parameter = source.paramDic["value"];
        string serialized = parameter.v;
        parameter.GetValue().dic["live"].num = 77;
        var copy = source.Copy();
        Check(copy.uid == source.uid && copy.protoUid == source.protoUid, "Copy retains identity by default");
        Check(!ReferenceEquals(copy.model, source.model)
            && !ReferenceEquals(copy.model.subUnitTexsName[0], source.model.subUnitTexsName[0]), "model and nested texture rows are owned");
        Check(copy.model.subUnitTexsName[1] == null, "nested null entries are preserved");
        Check(!ReferenceEquals(copy.events["test"], source.events["test"])
            && !ReferenceEquals(copy.events["test"].evt, source.events["test"].evt), "event rows and ID lists are owned");
        Check(!ReferenceEquals(copy.animClip[AnimDirecton.Up], source.animClip[AnimDirecton.Up]), "nested animation lists are owned");
        Check(!ReferenceEquals(copy.paramDic["value"], parameter)
            && !ReferenceEquals(copy.paramDic["value"].GetValue().dic["live"], parameter.GetValue().dic["live"]),
            "inherited parameter Copy hook owns the live Box graph");
        Check(copy.paramDic["value"].GetValue().dic["live"].num == 77 && parameter.v == serialized,
            "Copy sees script changes without serializing the source parameter");
        copy.paramDic["value"].GetValue().dic["live"].num = 88;
        copy.model.subUnitTexsName[0][0] = 99;
        copy.events["test"].evt[0] = 99;
        copy.animClip[AnimDirecton.Up][0] = 99;
        copy.modelSizes[AnimDirecton.Up] = Vector3.zero;
        copy.modelPositions[AnimDirecton.Up] = Vector3.zero;
        Check(parameter.GetValue().dic["live"].num == 77 && source.model.subUnitTexsName[0][0] == 1
            && source.events["test"].evt[0] == 11 && source.animClip[AnimDirecton.Up][0] == 21
            && source.modelSizes[AnimDirecton.Up] == new Vector3(2, 3, 4)
            && source.modelPositions[AnimDirecton.Up] == new Vector3(5, 6, 7), "mutations cannot leak into the source");
        copy = source.Copy();
        Check(JToken.DeepEquals(ObjectProductForm.GetJoByData(source), ObjectProductForm.GetJoByData(copy)),
            "generated deep copy preserves the complete saved payload");
        var comparer = new Dictionary<string, EventTriggerForm.Data>(StringComparer.OrdinalIgnoreCase) { ["Test"] = source.events["test"] };
        source.events = comparer;
        Check(source.Copy().events.ContainsKey("test"), "deep dictionary copying preserves its comparer");
    }

    private static void Cycles()
    {
        var a = CodeHelper.CreateBox(); var b = CodeHelper.CreateBox(); var c = CodeHelper.CreateBoxByNum(3);
        a.dic = new Dictionary<string, BoxDataForm.Data> { ["b"] = b };
        b.dic = new Dictionary<string, BoxDataForm.Data> { ["c"] = c };
        Check(a.Copy().dic["b"].dic["c"].num == 3, "different instances of the same type do not form a cycle");
        a.dic["other"] = b;
        var shared = a.Copy();
        Check(!ReferenceEquals(shared.dic["b"], shared.dic["other"]), "a shared acyclic object is independently copied for both fields");
        b.dic["a"] = a;
        ExpectCycle(() => a.Copy(), "mutual object references");
        b.dic.Remove("a");
        Check(a.Copy().dic["b"].dic["c"].num == 3, "a failed copy releases every active-path entry");
        c.dic = new Dictionary<string, BoxDataForm.Data> { ["self"] = c };
        ExpectCycle(() => c.Copy(), "a direct self reference");
        c.dic = null;
        var parameter = new MapObjectParamForm.Data(0, "cycle", ValType.Float, "", "", "");
        parameter.GetValue().dic = new Dictionary<string, BoxDataForm.Data>();
        parameter.GetValue().dic["self"] = parameter.GetValue();
        ExpectCycle(() => parameter.Copy(), "a cycle reached through the inherited live-value hook");
        parameter.GetValue().dic.Clear();
        Check(parameter.Copy().GetValue().dic.Count == 0, "later copies still work after a hook throws");
    }

    private static void ExpectCycle(Action copy, string description)
    {
        try { copy(); }
        catch (InvalidOperationException error)
        {
            Check(error.Message.Contains("Copy() circular object reference") && error.Message.Contains(" -> "), description);
            return;
        }
        throw new InvalidOperationException("Missing cycle error: " + description);
    }

    private static void Binding()
    {
        bool playing = DynamicGlobalSettings.playing;
        try
        {
            DynamicGlobalSettings.playing = true;
            var source = Source(); source.uid = -1; ObjectProductForm.AddData(source);
            var unit = new ObjectUnitForm.Data(-1, false, "", "", Vector3.zero, Vector3.zero, Vector3.one,
                0, new List<int>(), "", false, 0);
            int serializations = 0;
            Action<ObjectProductForm.Data> observer = _ => serializations++;
            ObjectProductForm.beforeGetAction += observer;
            ObjectProductForm.Data product;
            try { product = unit.unit.BindProduct(source); }
            finally { ObjectProductForm.beforeGetAction -= observer; }
            Check(product.uid != source.uid && product.protoUid == source.uid && serializations == 0,
                "binding a prototype creates one independently copied Product without Product JSON");
            Check(ReferenceEquals(unit.unit.product, product)
                && ReferenceEquals(unit.unit.paramInfo["value"], product.paramDic["value"]), "initial parameter binding directly uses owned Product values");
            var parameters = unit.unit.paramInfo;
            parameters["new"] = new GameParamForm.Data(0, "new", ValType.String, "", "", "");
            unit.unit.paramInfo = parameters;
            Check(product.paramDic.ContainsKey("new"), "the public parameter setter still publishes later key edits");
            var exact = product.Copy(false); exact.ToProduct(source.uid);
            unit.unit.BindProduct(exact);
            Check(unit.unit.product == exact, "rebinding replaces the cached concrete Product");
            ObjectProductForm.RemoveData(exact.uid); ObjectProductForm.RemoveData(product.uid); ObjectProductForm.RemoveData(source.uid);
        }
        finally { DynamicGlobalSettings.playing = playing; }
    }

    private static void Benchmark()
    {
        var source = Source();
        for (int i = 0; i < 16; i++)
            source.events["trigger" + i] = new EventTriggerForm.Data(0, "trigger" + i, new List<int> { 1, 2, 3, 4 }, default);
        // Warm both algorithms and force BeforeGet's lazy parameter decoding first.
        for (int i = 0; i < 4; i++)
        {
            retained = source.Copy();
            retained = ObjectProductForm.GetDataByJo(ObjectProductForm.GetJoByData(source));
        }
        const int count = 512;
        long calibration = GC.GetAllocatedBytesForCurrentThread(); retained = new byte[4096];
        bool allocationsAvailable = GC.GetAllocatedBytesForCurrentThread() > calibration;
        var timer = new Stopwatch();
        long before = GC.GetAllocatedBytesForCurrentThread(); timer.Start();
        for (int i = 0; i < count; i++) retained = source.Copy();
        timer.Stop(); double directMs = timer.Elapsed.TotalMilliseconds;
        long directBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread(); timer.Restart();
        for (int i = 0; i < count; i++) retained = ObjectProductForm.GetDataByJo(ObjectProductForm.GetJoByData(source));
        timer.Stop(); double jsonMs = timer.Elapsed.TotalMilliseconds;
        long jsonBytes = GC.GetAllocatedBytesForCurrentThread() - before;
        Debug.Log($"FORM_COPY_BENCHMARK count={count} directMs={directMs:F3} jsonMs={jsonMs:F3} "
            + (allocationsAvailable ? $"directBytes={directBytes} jsonBytes={jsonBytes}" : "allocation counter unavailable"));
        if (allocationsAvailable) Check(directBytes < jsonBytes, "direct copying allocates less than the equivalent JSON round trip");
    }

    private static void Check(bool success, string message)
    {
        if (!success) throw new InvalidOperationException(message);
        checks++;
    }
}
