using System;
using System.Collections.Generic;
using System.Linq;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Language;
using Z_Text;

// Run only in the isolated project created by Run-MapRuntimeRegression.ps1.
public static class MissionRuntimeRegression
{
    private static int checks;
    private static readonly string[] canonical = { "AddMission", "DoneMission", "IsMissionAdded", "HasMissionDone" };

    private sealed class Listener : IZ_Listener<MissionEvent>
    {
        public readonly List<MissionEvent> events = new List<MissionEvent>();
        public void OnEvent(MissionEvent evt) => events.Add(evt);
    }

    public static void Run()
    {
        try
        {
            Initialize();
            MetadataAndDefaults();
            ConstantSelection();
            TypedAndLegacyCommands();
            ReferencesAndInvalidInputs();
            Debug.Log("MISSION_RUNTIME_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception ex)
        {
            Debug.LogException(ex);
            EditorApplication.Exit(1);
        }
    }

    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }

    private static void Initialize()
    {
        typeof(GameCmdDataForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        typeof(GameCmdTypeDataForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        GameCmdDataForm.Init();
        GameCmdTypeDataForm.Init();
        foreach (var type in new[] { typeof(AddMissionCmd), typeof(CompleteMissionCmd), typeof(IsMissionAddedCmd),
                     typeof(IsMissionCompleteCmd), typeof(MissionCmd) })
        {
            var init = type.GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
            Check(init != null && init.IsDefined(typeof(RuntimeInitializeOnLoadMethodAttribute), false), type.Name + " registration hook");
            init.Invoke(null, null);
        }
        MissionForm.Clear();
    }

    private static void MetadataAndDefaults()
    {
        Check(CmdTypeDataForm.DataByName.ContainsKey("mission"), "mission is in the shared type registry");
        Check(GameCmdTypeDataForm.DataByName["mission"].uid == 10012, "mission type ID");
        foreach (string name in canonical)
        {
            Check(BaseData.cmdDic.ContainsKey(name), name + " registered");
            Check(BaseData.cmdDic[name].GetName() == name, name + " canonical name");
            var data = GameCmdDataForm.DataByName[name];
            Check(data.prmTypes.SequenceEqual(new[] { "mission" }), name + " parameter type");
            Check(data.prmNames.SequenceEqual(new[] { "mission" }), name + " parameter label");
            Check(ReferenceEquals(BaseData.cmdDic[name].GetForm(), data), name + " shared metadata");
            string source = data.retTypes[0] == "void" ? data.defaultCode : "Return " + data.defaultCode + ";";
            Execute(source);
            Execute(data.retTypes[0] == "void" ? name + "();" : "Return " + name + "();");
        }
        Check(GameCmdDataForm.DataByName["Mission"].retTypes.SequenceEqual(new[] { "mission" }), "constant return type");
        Check(GameCmdDataForm.DataByName["Mission"].retNames.SequenceEqual(new[] { "mission" }), "constant return slot");
        Check(GlobalEventHelper.GetGameRetType(new Desc("$m$1001$m$", CodeType.Str)) == "mission", "literal editor type");
        Check(GlobalEventHelper.GetGameRetType(new Desc("$m$$m$", CodeType.Str)) == "mission", "empty literal editor type");
        Check(new Desc("Mission", CodeType.FuncName).retType == "mission", "constant compiler type");
        Check(Execute("Return " + GameCmdDataForm.DataByName["Mission"].defaultCode + ";").ret.str == "$m$$m$", "constant table default");
        Check(Execute("Return Mission();").ret.str == "$m$$m$", "constant runtime result");
        foreach (var alias in new[] { ("MissionAdd", "AddMission"), ("MissionDone", "DoneMission"),
                     ("CompleteMission", "DoneMission"), ("IsMissionComplete", "HasMissionDone") })
        {
            Check(BaseData.cmdDic[alias.Item1].GetName() == alias.Item2, alias.Item1 + " canonical mapping");
            Check(new Desc(alias.Item1, CodeType.FuncName).retType == CmdDataForm.DataByName[alias.Item2].retTypes[0], alias.Item1 + " metadata return type");
        }
    }

    private static MissionForm.Data AddData(int id, string name)
    {
        var data = new MissionForm.Data(id, name, 0, "", false, false, false, false, 0, Vector3.zero, 0);
        MissionForm.AddData(data);
        return data;
    }

    private static void ConstantSelection()
    {
        typeof(LabForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        typeof(ModTextForm).GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
        LabForm.Init();
        ModTextForm.Init();
        var constant = GameCmdDataForm.DataByName["Mission"];
        Check(constant.labId == GameCmdDataForm.DataByName["Text"].labId && constant.labId == 10001,
            "Mission is grouped with constants rather than mission operations");
        foreach (string command in canonical)
            Check(GameCmdDataForm.DataByName[command].labId == 10015, command + " retains mission category");
        Check(GameCmdDataForm.DatasByLabid[10001].Contains(constant), "constant index contains Mission");
        Check(!GameCmdDataForm.DatasByLabid[10015].Contains(constant), "mission-operation index excludes constant");

        var go = new GameObject("mission-constant-selection");
        go.SetActive(false);
        var game = go.AddComponent<GameManager>();
        var singleton = typeof(Z_MonoSingleton<GameManager>).GetField("_instance", BindingFlags.Static | BindingFlags.NonPublic);
        singleton.SetValue(null, game);
        var progress = (ProgressForm.Data)System.Runtime.Serialization.FormatterServices.GetUninitializedObject(typeof(ProgressForm.Data));
        progress.uid = 1;
        progress.editorStyle = EditorStyle.Rpg;
        ProgressForm.AddData(progress);
        var language = LanguageManager.instance.language;
        try
        {
            var events = new GameEventController(game);
            foreach (var selectedLanguage in new[] { Language.En, Language.Cn })
            {
                LanguageManager.instance.SetLanguage(selectedLanguage);
                foreach (string returnType in new[] { "mission", "var" })
                {
                    var entries = events.GetCmdEntry(SceneEventType.All, returnType, out _);
                    string category = TextManager.instance.GetTxt(LabForm.DataById[10001].lv1Lab);
                    Check(entries.subs.TryGetValue(category, out var constants), returnType + " chooser has basic category");
                    var mission = constants.subs.Values.SingleOrDefault(entry => entry.id == constant.uid);
                    Check(mission != null, returnType + " chooser exposes Mission constant");
                    Check(mission.content == TextManager.instance.GetTxt("mission"), "localized Mission label");
                    Check(entries.subs.Values.SelectMany(group => group.subs.Values).Count(entry => entry.id == constant.uid) == 1,
                        "Mission appears exactly once");
                }
            }
            var data = AddData(1201, "constant-selection-target");
            string reference = GlobalEventHelper.GetName(GlobalEventHelper.MISSION, data.id.ToString());
            Check(reference == "$m$1201$m$", "MissionForm ID produces wrapped constant");
            Check(Execute("Return \"" + reference + "\";").ret.str == reference, "selected literal round trip");
            Check(GlobalEventHelper.TryGetMission(reference, out var selected) && ReferenceEquals(selected, data),
                "selected constant resolves MissionForm data");
        }
        finally
        {
            LanguageManager.instance.SetLanguage(language);
            ProgressForm.RemoveData(progress.uid);
            singleton.SetValue(null, null);
            UnityEngine.Object.DestroyImmediate(go);
        }
    }

    private static void TypedAndLegacyCommands()
    {
        var listener = new Listener();
        listener.Register();
        try
        {
            int id = 1001;
            foreach (var commands in new[] { ("AddMission", "DoneMission", "HasMissionDone"),
                         ("MissionAdd", "MissionDone", "IsMissionComplete"),
                         ("AddMission", "CompleteMission", "IsMissionComplete") })
            {
                foreach (bool legacyName in new[] { false, true })
                {
                    var data = AddData(id++, "mission-" + id);
                    var untouched = AddData(id++, "unaffected-" + id);
                    string reference = legacyName ? data.name : GlobalEventHelper.GetName(GlobalEventHelper.MISSION, data.id.ToString());
                    int start = listener.events.Count;
                    Check(Execute("Return IsMissionAdded(\"" + reference + "\");").ret.num == 0, "initial received state");
                    Check(Execute("Return " + commands.Item3 + "(\"" + reference + "\");").ret.num == 0, "initial done state");
                    Execute(commands.Item1 + "(\"" + reference + "\");");
                    Check(data.received && !data.done, "add affects referenced mission");
                    Check(Execute("Return IsMissionAdded(\"" + reference + "\");").ret.num == 1, "received query after add");
                    Execute(commands.Item1 + "(\"" + reference + "\");");
                    Execute(commands.Item2 + "(\"" + reference + "\");");
                    Check(data.done, "done affects referenced mission");
                    Check(Execute("Return " + commands.Item3 + "(\"" + reference + "\");").ret.num == 1, "done query after completion");
                    Execute(commands.Item2 + "(\"" + reference + "\");");
                    Check(!untouched.received && !untouched.done, "other mission unchanged");
                    Check(listener.events.Count == start + 2, "duplicate add/done emits no extra events");
                    Check(listener.events[start].type == MissionEventType.Add && ReferenceEquals(listener.events[start].data, data), "add event identity");
                    Check(listener.events[start + 1].type == MissionEventType.Done && ReferenceEquals(listener.events[start + 1].data, data), "done event identity");
                }
            }
        }
        finally { listener.Unregister(); }
    }

    private static void ReferencesAndInvalidInputs()
    {
        var data = AddData(1101, "before-rename");
        string reference = "$m$1101$m$";
        Check(GlobalEventHelper.TryGetMission(reference, out var resolved) && ReferenceEquals(data, resolved), "ID resolution");
        data.name = "after-rename";
        Check(GlobalEventHelper.TryGetMission(reference, out resolved) && ReferenceEquals(data, resolved), "ID survives rename");
        Check(!GlobalEventHelper.TryGetMission("before-rename", out _), "old name no longer resolves");
        Check(GlobalEventHelper.TryGetMission("after-rename", out resolved) && ReferenceEquals(data, resolved), "current legacy name resolves");
        Execute("AddMission(\"" + reference + "\"); DoneMission(\"" + reference + "\");");
        Check(data.received && data.done, "commands still target renamed mission");
        MissionForm.RemoveData(data.id);
        var untouched = AddData(1102, "invalid-reference-target");
        foreach (string invalid in new[] { null, "", "$m$$m$", "$m$0$m$", "$m$-1$m$", "$m$abc$m$", "$m$2147483648$m$",
                     "$m$999999$m$", reference, "$m$1102", "1102$m$", "$m$1102$m$extra", "missing-name" })
        {
            Check(!GlobalEventHelper.TryGetMission(invalid, out _), "invalid reference rejected: " + invalid);
            foreach (string command in canonical)
            {
                var result = new InterpretAsyncTask(null);
                BaseData.cmdDic[command].GetNew().Execute(new[] { CodeHelper.CreateBoxByStr(invalid) },
                    new Dictionary<string, BoxDataForm.Data>(), result);
                Check(result.IsComplete() && string.IsNullOrEmpty(result.error), command + " invalid reference does not throw");
                if (command == "IsMissionAdded" || command == "HasMissionDone")
                    Check(result.res.Length == 1 && result.res[0].num == 0, command + " invalid reference returns false");
            }
            Check(!untouched.received && !untouched.done, "invalid reference is a no-op");
        }
    }

    private static InterpretDataForm.RetInfo Execute(string source)
    {
        var compiler = new Compiler();
        Check(compiler.TryCompile(source, out var code, out _, out int paramCount, out string returnType,
            out var codeMap, out var errors), "compile failed: " + source + " / " + string.Join("; ", errors));
        Check(code.Count == codeMap.Count, "source mapping count");
        var program = new ProgramDataForm.Data(-1, "mission-test", source, code, codeMap, paramCount, returnType);
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            program, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = data.Interpret();
        Check(result.complete && result.errors.Count == 0, "execution failed: " + source + " / " + string.Join("; ", result.errors));
        return result;
    }
}
