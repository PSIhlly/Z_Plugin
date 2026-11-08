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
using Z_UnitSystem;

// Isolated project only. No real scene or save is loaded or changed.
public static class GlobalVariableRegression
{
    private static int checks;
    private static string ownedFolder;
    private static readonly GetGlobalVarCmd get = new GetGlobalVarCmd();
    private static readonly SetGlobalVarCmd set = new SetGlobalVarCmd();
    private static readonly List<int> programs = new List<int>();

    public static void Run()
    {
        bool initialized = false;
        try
        {
            // Guard against accidentally executing this fixture in the user's project.
            string projectName = new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name;
            Check(projectName.StartsWith("MapRuntimeRegression-", StringComparison.Ordinal),
                "Fixture runs only inside an isolated MapRuntimeRegression project");
            SetUp();
            initialized = true;
            MetadataAndRegistration();
            ScalarValuesAndNames();
            DictionaryCopyIsolation();
            InterpreterAndProgramSharing();
            OmittedAndInvalidArguments();
            PersistenceAndStoryIsolation();
            Debug.Log("GLOBAL_VARIABLE_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (initialized)
            {
                foreach (int uid in programs) ProgramDataForm.RemoveData(uid);
                EventGlobalVariableForm.Clear();
            }
            // Delete only the exact directory this fixture created under the OS temp root.
            if (ownedFolder != null && Directory.Exists(ownedFolder))
            {
                string prefix = Path.GetFullPath(Path.GetTempPath()).TrimEnd(Path.DirectorySeparatorChar)
                    + Path.DirectorySeparatorChar + "ZPluginGlobalVarRegression-";
                string target = Path.GetFullPath(ownedFolder);
                if (target.StartsWith(prefix, StringComparison.OrdinalIgnoreCase))
                    Directory.Delete(target, true);
            }
        }
    }

    private static void SetUp()
    {
        foreach (Type type in new[] { typeof(BoxDataForm), typeof(EventGlobalVariableForm),
            typeof(ProgramDataForm), typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(CmdLabForm) })
            type.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        BoxDataForm.Init(); EventGlobalVariableForm.Init(); ProgramDataForm.Init();
        GameCmdDataForm.Init(); GameCmdTypeDataForm.Init(); CmdLabForm.Init();
        Check(EventGlobalVariableForm.DataByUid.Count == 0,
            "Isolated global-variable table starts empty before any mutation");
    }

    private static void MetadataAndRegistration()
    {
        foreach (CmdBase command in new CmdBase[] { get, set })
        {
            MethodInfo init = command.GetType().GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
            Check(init != null && init.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1,
                command.GetName() + " has explicit Player-safe registration");
            init.Invoke(null, null);
            Check(BaseData.cmdDic.TryGetValue(command.GetName(), out var registered)
                && registered.GetType() == command.GetType(), command.GetName() + " registers by canonical name");
            Check(command.GetNew().GetType() == command.GetType() && !ReferenceEquals(command.GetNew(), command),
                command.GetName() + " creates independent invocations");
            Check(CmdBase.TryGetForm(command.GetName(), out var metadata) && metadata is GameCmdDataForm.Data,
                command.GetName() + " is visible in the command chooser metadata");
            var form = (GameCmdDataForm.Data)metadata;
            bool getter = command == get;
            Check(form.uid == (getter ? 100121 : 100122) && form.labId == 10001,
                command.GetName() + " uses its stable UID in the basic CmdLab");
            Check(form.prmTypes.SequenceEqual(getter ? new[] { "string" } : new[] { "string", "var" })
                && form.prmNames.Count == (getter ? 1 : 2),
                command.GetName() + " exposes the correct typed arguments");
            Check(form.retTypes.SequenceEqual(new[] { getter ? "var" : "void" })
                && (form.retNames?.Count ?? 0) == (getter ? 1 : 0),
                command.GetName() + " declares the actual runtime return count");
            Check(!string.IsNullOrEmpty(form.defaultCode), command.GetName() + " has insertion code");
            string source = getter ? "Return " + form.defaultCode.TrimEnd(';') + ";" : form.defaultCode;
            if (!getter) Check(source.TrimEnd().EndsWith(";", StringComparison.Ordinal),
                "SetGlobalVar insertion code is a complete statement");
            Compile(source);
        }
    }

    private static void ScalarValuesAndNames()
    {
        EventGlobalVariableForm.Clear();
        int count = EventGlobalVariableForm.DataByUid.Count;
        BoxDataForm.Data missing = Get("not-created");
        Check(missing.str == null && missing.num == 0 && EventGlobalVariableForm.DataByUid.Count == count,
            "Missing names read as numeric zero without creating a record");
        foreach (float value in new[] { 0f, -4.5f, 987654f })
        {
            var input = CodeHelper.CreateBoxByNum(value);
            Set("number", input);
            BoxDataForm.Data result = Get("number");
            Check(result.str == null && result.num == value && !ReferenceEquals(input, result),
                "Numbers retain numeric Box type and exact value " + value);
            input.num = value + 1f;
            result.num = value + 2f;
            Check(Get("number").num == value, "Numeric source and read result cannot change stored value");
        }
        var row = EventGlobalVariableForm.DataByName["number"];
        int uid = row.uid;
        Set("number", CodeHelper.CreateBoxByStr("replacement"));
        Check(EventGlobalVariableForm.DataByName["number"].uid == uid
            && EventGlobalVariableForm.DataByUid.Count == 1 && Get("number").str == "replacement",
            "Replacing a value preserves the named record without duplicate UID entries");
        Set("number", CodeHelper.CreateBoxByNum(3f));
        Check(Get("number").str == null && Get("number").num == 3f,
            "Replacing a string with a number clears the old string type");
        foreach (string value in new[] { "", "数字 01", "line1\nline2 \"quoted\"", "$ch$123$ch$" })
        {
            Set("text", CodeHelper.CreateBoxByStr(value));
            BoxDataForm.Data result = Get("text");
            Check(result.str == value, "Strings, empty strings and reference markers remain strings");
            result.str = "local change";
            Check(Get("text").str == value, "Mutating a returned string Box does not affect storage");
        }
        Set("Name", CodeHelper.CreateBoxByNum(1));
        Set("name", CodeHelper.CreateBoxByNum(2));
        Set(" name ", CodeHelper.CreateBoxByNum(3));
        Set("中文变量", CodeHelper.CreateBoxByNum(4));
        Check(Get("Name").num == 1 && Get("name").num == 2 && Get(" name ").num == 3
            && Get("中文变量").num == 4, "Names are exact, case-sensitive and support Unicode");
    }

    private static BoxDataForm.Data NestedValue()
    {
        var root = CodeHelper.CreateBox();
        root.dic["x"] = CodeHelper.CreateBoxByNum(2f);
        root.dic["title"] = CodeHelper.CreateBoxByStr("原始");
        var nested = CodeHelper.CreateBox();
        nested.dic["height"] = CodeHelper.CreateBoxByNum(0.5f);
        nested.dic["reference"] = CodeHelper.CreateBoxByStr("$so$101$so$");
        root.dic["nested"] = nested;
        return root;
    }

    private static void DictionaryCopyIsolation()
    {
        EventGlobalVariableForm.Clear();
        var input = NestedValue();
        int boxes = BoxDataForm.DataByUid.Count;
        Set("nested", input);
        var stored = EventGlobalVariableForm.DataByName["nested"].data;
        var a = Get("nested"); var b = Get("nested");
        Check(!ReferenceEquals(input, stored) && !ReferenceEquals(input.dic, stored.dic)
            && !ReferenceEquals(input.dic["nested"], stored.dic["nested"])
            && !ReferenceEquals(a, stored) && !ReferenceEquals(a, b)
            && !ReferenceEquals(a.dic, b.dic) && !ReferenceEquals(a.dic["nested"].dic, b.dic["nested"].dic),
            "Writes and independent reads deeply own every mutable dictionary and nested Box");
        input.dic["x"].num = 99f;
        input.dic["nested"].dic["height"].num = 99f;
        input.dic["new"] = CodeHelper.CreateBoxByNum(99f);
        a.dic["title"].str = "changed";
        a.dic["nested"].dic.Clear();
        b.dic.Clear();
        var actual = Get("nested");
        Check(actual.dic.Count == 3 && actual.dic["x"].num == 2f && actual.dic["title"].str == "原始"
            && actual.dic["nested"].dic["height"].num == 0.5f
            && actual.dic["nested"].dic["reference"].str == "$so$101$so$",
            "Source mutation, nested mutation and returned-container clearing do not leak into global storage");
        Check(BoxDataForm.DataByUid.Count == boxes,
            "Inline global Boxes are not registered as unrelated global Box Form rows");
        Set("nested", CodeHelper.CreateBoxByNum(8f));
        Check(Get("nested").num == 8f && (Get("nested").dic?.Count ?? 0) == 0,
            "Replacing a dictionary with a scalar does not retain dictionary members");
    }

    private static void InterpreterAndProgramSharing()
    {
        EventGlobalVariableForm.Clear();
        Check(Execute("SetGlobalVar(\"counter\", 3); Return GetGlobalVar(\"counter\");").num == 3f,
            "Compiled setter binds name then value and getter returns a usable expression");
        Check(Execute("SetGlobalVar(\"counter\", GetGlobalVar(\"counter\") + 4); Return GetGlobalVar(\"counter\");").num == 7f,
            "A separate Interpreter reads and updates shared values");
        var helper = Compile("SetGlobalVar(\"counter\", GetGlobalVar(\"counter\") + param1); Return GetGlobalVar(\"counter\");");
        helper.name = "GlobalVariableRegressionAdd";
        ProgramDataForm.AddData(helper); programs.Add(helper.uid);
        Check(Execute("a = GlobalVariableRegressionAdd(5); Return a + GetGlobalVar(\"counter\");").num == 24f,
            "Custom subprograms and their caller share globals while retaining normal return semantics");
        var input = NestedValue();
        var initialHeap = new Dictionary<string, BoxDataForm.Data> { ["param1"] = input };
        var value = Execute("SetGlobalVar(\"saved\", param1); param1.x = 99; a = GetGlobalVar(\"saved\"); a.x = 77; Return GetGlobalVar(\"saved\");", initialHeap);
        Check(input.dic["x"].num == 99f && value.dic["x"].num == 2f,
            "Interpreter command inputs and local assignment copies never alias global storage");
        value.dic["nested"].dic["height"].num = 50f;
        Check(Execute("Return GetGlobalVar(\"saved\");").dic["nested"].dic["height"].num == 0.5f,
            "RetInfo remains an independent deep value after the invoking Interpreter completes");
        Check(Execute("GetGlobalVar(\"counter\"); GetGlobalVar(\"counter\"); Return 9;").num == 9f,
            "Discarded getter calls do not leave extra values on the runtime stack");
        Execute("SetGlobalVar(\"many\", 1); for(i = 0; i < 160; i++) { a = i * 2; } Return 0;");
        Check(Get("many").num == 1f, "Interpreter temporary Box recycling cannot overwrite a stored scalar");
    }

    private static void OmittedAndInvalidArguments()
    {
        EventGlobalVariableForm.Clear();
        Set("valid", CodeHelper.CreateBoxByNum(5f));
        int count = EventGlobalVariableForm.DataByUid.Count;
        foreach (string name in new string[] { null, "" })
        {
            BoxDataForm.Data result = Get(name);
            Set(name, NestedValue());
            Check(result.str == null && result.num == 0 && EventGlobalVariableForm.DataByUid.Count == count,
                "An empty/null variable name reads zero and never creates or overwrites a record");
        }
        Check(Execute("SetGlobalVar(); Return GetGlobalVar();").num == 0f
            && EventGlobalVariableForm.DataByUid.Count == count && Get("valid").num == 5f,
            "Compiler/interpreter omitted names preserve existing values");
        var numericName = Call(get, CodeHelper.CreateBoxByNum(123f));
        Call(set, CodeHelper.CreateBoxByNum(123f), CodeHelper.CreateBoxByNum(456f));
        Check(numericName.res?.Length == 1 && numericName.res[0].str == null && numericName.res[0].num == 0f
            && EventGlobalVariableForm.DataByUid.Count == count && Get("valid").num == 5f,
            "Non-string numeric names read zero and never create or overwrite a global variable");
        var nullRecord = new EventGlobalVariableForm.Data(-1, "null-box", null);
        EventGlobalVariableForm.AddData(nullRecord);
        var nullValue = Get("null-box");
        Check(nullValue.str == null && nullValue.num == 0f && nullRecord.data == null
            && EventGlobalVariableForm.DataByUid.Count == count + 1
            && ReferenceEquals(EventGlobalVariableForm.DataByName["null-box"], nullRecord),
            "An existing null payload reads zero without repairing or replacing its stored record");
        Check(Execute("SetGlobalVar(\"omitted-value\"); Return GetGlobalVar(\"omitted-value\");").str == "",
            "Omitted setter value retains the standard empty-string Box argument convention");
    }

    private static void PersistenceAndStoryIsolation()
    {
        EventGlobalVariableForm.Clear();
        ownedFolder = Path.Combine(Path.GetTempPath(), "ZPluginGlobalVarRegression-" + Guid.NewGuid().ToString("N"));
        Directory.CreateDirectory(ownedFolder);
        string aFolder = Path.Combine(ownedFolder, "story-a", "Save");
        string bFolder = Path.Combine(ownedFolder, "story-b", "Core");
        string missingFolder = Path.Combine(ownedFolder, "legacy-without-egvf");
        var save = new GameSaveController(null);
        Check(save.globalVariableFormFileName == "egvf", "Global-variable persistence has a stable short filename");
        Set("nested", NestedValue()); Set("number", CodeHelper.CreateBoxByNum(-12.5f));
        Set("empty", CodeHelper.CreateBoxByStr("")); Set("text", CodeHelper.CreateBoxByStr("存档文字"));
        save.SaveGlobalVariables(aFolder);
        string aFile = Path.Combine(aFolder, save.globalVariableFormFileName);
        Check(File.Exists(aFile) && JArray.Parse(File.ReadAllText(aFile)).Count == 4,
            "The actual save API writes one inline Box record per global variable to the owned folder");
        EventGlobalVariableForm.Clear();
        Set("foreign", CodeHelper.CreateBoxByNum(999));
        save.LoadGlobalVariables(aFolder);
        Check(EventGlobalVariableForm.DataByUid.Count == 4 && !EventGlobalVariableForm.DataByName.ContainsKey("foreign")
            && Get("number").num == -12.5f && Get("number").str == null
            && Get("empty").str == "" && Get("text").str == "存档文字",
            "Load replaces previous globals and retains numeric/string/empty-string Box types");
        var result = Get("nested");
        Check(result.dic["x"].num == 2f && result.dic["nested"].dic["height"].num == 0.5f
            && result.dic["nested"].dic["reference"].str == "$so$101$so$",
            "Nested dictionary values and reference strings survive the actual JSON/file round trip");
        result.dic["nested"].dic["height"].num = 9f;
        Check(Get("nested").dic["nested"].dic["height"].num == 0.5f,
            "Post-load getters still return deep copies");
        var maximumUid = new EventGlobalVariableForm.Data(EventGlobalVariableForm.autoUidCnt,
            "maximum-uid", CodeHelper.CreateBoxByNum(1f));
        EventGlobalVariableForm.AddData(maximumUid);
        Check(EventGlobalVariableForm.DataByUid.ContainsKey(EventGlobalVariableForm.autoUidCnt),
            "Reset boundary fixture includes a record at the exact automatic UID pool maximum");
        save.LoadGlobalVariables(null);
        Check(EventGlobalVariableForm.DataByUid.Count == 0 && EventGlobalVariableForm.DataByName.Count == 0,
            "Null-folder story reset clears both indexes, including the exact maximum UID record");
        Set("other-story", CodeHelper.CreateBoxByNum(42f));
        save.SaveGlobalVariables(bFolder);
        save.LoadGlobalVariables(aFolder);
        Check(Get("other-story").num == 0f && Get("number").num == -12.5f,
            "Switching back to Story A does not retain Story B globals");
        save.LoadGlobalVariables(bFolder);
        Check(EventGlobalVariableForm.DataByUid.Count == 1 && Get("other-story").num == 42f
            && Get("number").num == 0f, "A second consecutive story load completely replaces the first story state");
        save.LoadGlobalVariables(missingFolder);
        Check(EventGlobalVariableForm.DataByUid.Count == 0 && !Directory.Exists(missingFolder),
            "An old story with no egvf starts empty without creating or rewriting its folder");
        save.SaveGlobalVariables(aFolder);
        Set("transient", CodeHelper.CreateBoxByNum(7));
        save.LoadGlobalVariables(aFolder);
        Check(EventGlobalVariableForm.DataByUid.Count == 0,
            "An explicitly saved empty global-variable table also clears previous state on reload");
        Check(SaveAndLoad.GetRealPath(aFile) == Path.GetFullPath(aFile),
            "Fixture writes stayed in the explicit absolute temporary path, never the real save root");
    }

    private static BoxDataForm.Data Get(string name)
    {
        var result = Call(get, CodeHelper.CreateBoxByStr(name));
        Check(result.res != null && result.res.Length == 1 && result.res[0] != null,
            "GetGlobalVar returns exactly one non-null Box");
        return result.res[0];
    }

    private static void Set(string name, BoxDataForm.Data value)
    {
        var task = Call(set, CodeHelper.CreateBoxByStr(name), value);
        Check(task.res == null || task.res.Length == 0, "SetGlobalVar does not manufacture a return value");
    }

    private static InterpretAsyncTask Call(CmdBase command, params BoxDataForm.Data[] sourceOrderArguments)
    {
        var arguments = (BoxDataForm.Data[])sourceOrderArguments.Clone();
        // CmdBase receives the interpreter's stack-pop order and restores source order internally.
        Array.Reverse(arguments);
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments, new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && string.IsNullOrEmpty(task.error),
            command.GetName() + " completes synchronously without an error: " + task.error);
        return task;
    }

    private static ProgramDataForm.Data Compile(string source)
    {
        bool success = new Compiler().TryCompile(source, out var code, out _, out int count, out string result,
            out var map, out var errors);
        Check(success && code.Count > 0 && code.Count == map.Count,
            "Source compiles with a complete aligned source map: " + source + ": " + string.Join("; ", errors));
        return new ProgramDataForm.Data(-1, "global-variable-regression", source, code, map, count, result);
    }

    private static BoxDataForm.Data Execute(string source, Dictionary<string, BoxDataForm.Data> heap = null)
    {
        var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            heap ?? new Dictionary<string, BoxDataForm.Data>(), Compile(source), 0, -1, 0, null,
            new List<BoxDataForm.Data>(), 0);
        var result = execution.Interpret();
        Check(result.complete && result.errors.Count == 0 && execution.stack.Count == 0,
            "Actual Interpreter completes with a balanced stack: " + source + ": " + string.Join("; ", result.errors));
        return result.ret;
    }

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
