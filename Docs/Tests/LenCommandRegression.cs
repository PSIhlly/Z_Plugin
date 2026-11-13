using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Reflection;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;

// Actual LenCmd and interpreter, in the runner's isolated native Mono project.
public static class LenCommandRegression
{
    private static int checks;
    private static Func<Dictionary<string, BoxDataForm.Data>, int> length;
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name.StartsWith("MapRuntimeRegression-"), "isolated project guard");
            foreach (var type in new[] { typeof(BoxDataForm), typeof(ProgramDataForm), typeof(CmdDataForm) })
            {
                type.GetMethod("Register", HiddenStatic)?.Invoke(null, null);
                type.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
            }
            typeof(LenCmd).GetMethod("Init", HiddenStatic).Invoke(null, null);
            length = (Func<Dictionary<string, BoxDataForm.Data>, int>)typeof(LenCmd)
                .GetMethod("GetLength", HiddenStatic).CreateDelegate(typeof(Func<Dictionary<string, BoxDataForm.Data>, int>));
            Check(length(null) == 0 && length(new Dictionary<string, BoxDataForm.Data>()) == 0, "null and empty containers have length zero");
            foreach (int count in new[] { 1, 2, 31, 32, 33, 128, 1024, 4096 })
            {
                var values = Dense(count);
                Check(length(values) == count, "dense list length is independent of insertion order: " + count);
                if (count > 1)
                {
                    values.Remove("1"); values["alias"] = null;
                    Check(length(values) == 1, "same-count edits cannot leave a stale cached dense length");
                    values.Remove("alias"); values["1"] = null;
                    Check(length(values) == count, "restoring a missing index restores the full length immediately");
                }
            }
            var weird = new Dictionary<string, BoxDataForm.Data>
            { ["0"] = null, ["01"] = null, ["+1"] = null, ["-1"] = null, [" 1"] = null,
                ["2147483647"] = null, ["999999999999999999999"] = null, ["name"] = null };
            Check(length(weird) == 1, "noncanonical, named, negative and overflowing keys are not list indices");
            weird["1"] = null; weird["2"] = null;
            Check(length(weird) == 3, "named keys do not change the first-gap result");
            weird.Remove("0");
            Check(length(weird) == 0, "a list without its zero index has no prefix");
            var random = new System.Random(8731);
            for (int test = 0; test < 1500; test++)
            {
                var values = new Dictionary<string, BoxDataForm.Data>();
                int count = random.Next(1, 250);
                for (int i = 0; i < count; i++)
                {
                    string key = random.Next(3) == 0 ? "name" + i : random.Next(320).ToString(CultureInfo.InvariantCulture);
                    values[key] = null;
                }
                if ((test & 1) == 0) values["0"] = null;
                Check(length(values) == LegacyLength(values), "mixed dictionaries retain legacy first-gap semantics");
            }
            InterpreterLoop();
            Allocations();
            Debug.Log("LEN_COMMAND_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void InterpreterLoop()
    {
        var values = Dense(1024);
        // Executable bodies, not probes: this reproduces repeated Len(list) in
        // a loop condition while preserving normal budget/suspension semantics.
        var program = ProgramDataForm.defaultData;
        program.name = "Len-loop-1024";
        Check(program.TryApplyCode("for(i=0;i<Len(list);i++){if(i>=199){break;}}Return Len(list);", out _, out var errors),
            "the loop program compiles: " + string.Join(";", errors));
        var box = CodeHelper.CreateBox(); box.dic = values;
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            new Dictionary<string, BoxDataForm.Data> { ["list"] = box }, program, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        for (int poll = 0; poll < 10; poll++)
        {
            var result = data.Interpret();
            Check(result.errors.Count == 0, "the real Len command executes without errors");
            if (!result.complete) continue;
            Check(result.ret.num == 1024, "repeated native Len calls return the correct result");
            return;
        }
        throw new InvalidOperationException("Len loop did not complete");
    }

    private static void Allocations()
    {
        var values = Dense(1024);
        for (int i = 0; i < 20; i++) length(values);
        long beforeProbe = GC.GetAllocatedBytesForCurrentThread();
        var probe = new byte[1024]; GC.KeepAlive(probe);
        bool available = GC.GetAllocatedBytesForCurrentThread() > beforeProbe;
        long before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) length(values);
        long optimized = GC.GetAllocatedBytesForCurrentThread() - before;
        before = GC.GetAllocatedBytesForCurrentThread();
        for (int i = 0; i < 1000; i++) LegacyLength(values);
        long legacy = GC.GetAllocatedBytesForCurrentThread() - before;
        if (available)
        {
            Check(optimized == 0, "warmed length computation creates no index strings or per-query buffers");
            Check(legacy > 1000000, "allocation counter detects the previous repeated ToString loop");
        }
        Debug.Log($"LEN_GC_NATIVE_COUNTER available={available} optimized={optimized} legacy={legacy}");
    }

    private static Dictionary<string, BoxDataForm.Data> Dense(int count)
    {
        var result = new Dictionary<string, BoxDataForm.Data>();
        for (int i = count - 1; i >= 0; i--) result[i.ToString(CultureInfo.InvariantCulture)] = null;
        return result;
    }
    private static int LegacyLength(Dictionary<string, BoxDataForm.Data> values)
    {
        int i = 0; for (; i < values.Count && values.ContainsKey(i.ToString()); i++) { } return i;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
