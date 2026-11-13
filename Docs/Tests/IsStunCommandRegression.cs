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
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Run only in the runner's disposable Unity project; never open a user story/save.
public static class IsStunCommandRegression
{
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly List<GameObject> roots = new List<GameObject>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static ProgressForm.Data progress;
    private static PlayManager play;
    private static int checks;

    public static void Run()
    {
        bool oldPlaying = DynamicGlobalSettings.playing;
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name
                .StartsWith("MapRuntimeRegression-", StringComparison.Ordinal), "isolated project guard");
            SetUp();
            MetadataAndInterpreter();
            StunTimerAndExpiration();
            CharacterReferences();
            InvalidTargetsAndMissingProgress();
            Debug.Log("IS_STUN_COMMAND_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            DynamicGlobalSettings.playing = oldPlaying;
            foreach (var root in roots)
                if (root != null) Object.DestroyImmediate(root);
        }
    }

    private static void RegisterForm(Type form)
    {
        if (!forms.Add(form)) return;
        var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType;
        if (parent != null) RegisterForm(parent);
        form.GetMethod("Register", HiddenStatic | BindingFlags.DeclaredOnly)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject("is-stun-regression-" + typeof(T).Name);
        roots.Add(root);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, manager);
        return manager;
    }

    private static void SetUp()
    {
        foreach (var form in new[] { typeof(CharacterProductForm), typeof(CharacterUnitForm),
            typeof(ProgressForm), typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(CmdLabForm),
            typeof(ProgramDataForm), typeof(BoxDataForm), typeof(CharacterParamForm) })
            RegisterForm(form);
        typeof(IsStunCmd).GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly).Invoke(null, null);
        typeof(CharacterStunCmd).GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly).Invoke(null, null);
        Manager<GameManager>();
        play = Manager<PlayManager>();
        play.sceneCtrl = new PlaySceneController(play);
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.seconds = 100f;
        ProgressForm.AddData(progress);
        DynamicGlobalSettings.playing = true;
    }

    private static void MetadataAndInterpreter()
    {
        Check(CmdBase.TryGetForm("IsStun", out var form), "IsStun has command metadata");
        Check(form is GameCmdDataForm.Data typed && typed.labId == 10007,
            "IsStun belongs to the Character command category");
        Check(form.prmNames.SequenceEqual(new[] { "character" })
            && form.prmTypes.SequenceEqual(new[] { "character" }), "IsStun takes exactly one Character argument");
        Check(form.retNames.Count == 1 && form.retTypes.SequenceEqual(new[] { "num" }),
            "IsStun exposes exactly one numeric result");
        Check(GameCmdDataForm.DataByUid.Values.Count(row => row.name == "IsStun") == 1,
            "the chooser has only one IsStun entry");
        Check(BaseData.cmdDic["IsStun"] is IsStunCmd
            && BaseData.cmdDic["IsStun"].GetNew() is IsStunCmd,
            "command registration and cloning resolve the production implementation");
        var init = typeof(IsStunCmd).GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly);
        Check(init.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1,
            "IsStun has explicit Player runtime registration");

        var saved = Compile("Return " + form.defaultCode.TrimEnd(';') + ";");
        var restored = ProgramDataForm.GetDataByJo(ProgramDataForm.GetJoByData(saved));
        Check(saved.code == restored.code && saved.zCode.SequenceEqual(restored.zCode)
            && saved.zCodeMap.SequenceEqual(restored.zCodeMap), "default source and bytecode round-trip unchanged");
        Check(Run(restored, new Dictionary<string, BoxDataForm.Data> { { "param1", Text("") } }).num == 0f,
            "default code returns numeric zero with an empty Character reference");
        Check(Run(Compile("Return IsStun();")).num == 0f, "an omitted Character argument safely returns zero");
    }

    private static void StunTimerAndExpiration()
    {
        var character = Character(481001, 491001);
        progress.seconds = 0f;
        Expect(character, 0f, "default recoveryTime zero is not stunned at story start");
        progress.seconds = 100f;
        Execute(new CharacterStunCmd(), Text(Handle(character)), Number(2f));
        Check(character.recoveryTime == 102f, "the existing stun command sets the shared recovery timer");
        Expect(character, 1f, "CharacterStun is immediately visible to IsStun");
        progress.seconds = 101.999f;
        Expect(character, 1f, "a future recovery time remains stunned");
        progress.seconds = 102f;
        Expect(character, 0f, "exact timer expiration is no longer stunned");
        progress.seconds = 104f;
        Expect(character, 0f, "an expired recovery time remains not stunned");
        Execute(new CharacterStunCmd(), Text(Handle(character)), Number(10f));
        Execute(new CharacterStunCmd(), Text(Handle(character)), Number(1f));
        Check(character.recoveryTime == 114f, "a shorter stun does not truncate the existing timer");
        Expect(character, 1f, "an extended/reapplied stun is reported");
        float timer = character.recoveryTime;
        int count = CharacterProductForm.DataByUid.Count;
        var result = Execute(new IsStunCmd(), Text(Handle(character)));
        result.num = 0;
        Expect(character, 1f, "each query owns its result and later queries still read the Product timer");
        Check(character.recoveryTime == timer && progress.seconds == 104f
            && CharacterProductForm.DataByUid.Count == count, "queries do not modify Character, clock or registry");
        character.enableNav = false;
        character.recoveryTime = 0f;
        Expect(character, 0f, "disabled navigation alone is not stun");
    }

    private static void CharacterReferences()
    {
        progress.seconds = 100f;
        var prototype = Character(481010, 0);
        var first = Character(481011, prototype.uid);
        var second = Character(481012, prototype.uid);
        prototype.recoveryTime = 200f;
        first.recoveryTime = 0f;
        second.recoveryTime = 200f;
        Expect(prototype, 0f, "a prototype query uses the scalar stored-Product representative, not authored defaults or any-match");
        Expect(second, 1f, "a concrete runtime Product resolves exactly");
        Unit(481101, second.uid);
        Unit(481102, first.uid);
        Expect(prototype, 1f, "a prototype prefers the first live scene Unit by ascending Unit UID");
        Expect(first, 0f, "a concrete handle does not resolve a stunned sibling");
        var unique = Character(481020, 0, true);
        unique.recoveryTime = 200f;
        Expect(unique, 1f, "a unique Character preserves singleton Product identity");
        play.sceneCtrl = null;
        Expect(prototype, 0f, "without a scene a prototype falls back to the stable stored representative");
        play.sceneCtrl = new PlaySceneController(play);
    }

    private static void InvalidTargetsAndMissingProgress()
    {
        foreach (string handle in new[] { "", "plain-name", "$ch$$ch$", "$ch$0$ch$", "$ch$-1$ch$",
            "$ch$bad$ch$", "$ch$2147483648$ch$", "$ch$999999$ch$", "$so$481001$so$", "$it$481001$it$" })
            Check(Execute(new IsStunCmd(), Text(handle)).num == 0f, "invalid/missing/wrong-kind handle returns zero: " + handle);
        Check(Execute(new IsStunCmd(), Number(481001)).num == 0f, "a raw numeric UID is not a Character reference");
        var removed = Character(481030, 491030);
        removed.recoveryTime = 200f;
        CharacterProductForm.RemoveData(removed.uid);
        Expect(removed, 0f, "a removed Product is not resolved from stale object data");
        ProgressForm.RemoveData(progress.uid);
        Expect(CharacterProductForm.DataByUid[481001], 0f, "missing Progress safely returns zero");
        ProgressForm.AddData(progress);
    }

    private static CharacterProductForm.Data Character(int uid, int prototype, bool unique = false)
    {
        var data = CharacterProductForm.defaultData;
        data.uid = uid;
        data.name = "is-stun-character-" + uid;
        data.protoUid = prototype;
        data.unique = unique;
        data.recoveryTime = 0f;
        CharacterProductForm.AddData(data);
        return data;
    }

    private static void Unit(int uid, int productUid)
    {
        var data = new CharacterUnitForm.Data(uid, false, Vector3.zero, 1, 1, 1, false, "is-stun-unit",
            "unused-is-stun-prefab", Vector3.zero, Vector3.zero, Vector3.one, UpdateType.ShowOnly,
            new List<int>(), MapUnit.GetProductInfoString(new JObject(), (productUid, -1)), false, 0, new List<int>());
        CharacterUnitForm.AddData(data);
    }

    private static void Expect(CharacterProductForm.Data data, float expected, string message)
    {
        Check(Execute(new IsStunCmd(), Text(Handle(data))).num == expected, message);
        var heap = new Dictionary<string, BoxDataForm.Data> { { "param1", Text(Handle(data)) } };
        Check(Run(Compile("Return IsStun(param1);"), heap).num == expected, "interpreter: " + message);
    }

    private static BoxDataForm.Data Execute(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments.Reverse().ToArray(), new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && task.error == null, command.GetName() + " completes synchronously: " + task.error);
        if (!(command is IsStunCmd)) return null;
        Check(task.res != null && task.res.Length == 1 && task.res[0] != null
            && (task.res[0].num == 0f || task.res[0].num == 1f) && task.res[0].str == null
            && string.IsNullOrEmpty(task.res[0].valName)
            && (task.res[0].dic == null || task.res[0].dic.Count == 0), "IsStun returns one owned numeric zero/one Box");
        return task.res[0];
    }

    private static ProgramDataForm.Data Compile(string source)
    {
        var data = ProgramDataForm.defaultData;
        data.name = "is-stun-regression";
        Check(data.TryApplyCode(source, out _, out _), "source compiles: " + source);
        return data;
    }

    private static BoxDataForm.Data Run(ProgramDataForm.Data program, Dictionary<string, BoxDataForm.Data> heap = null)
    {
        var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), heap ?? new Dictionary<string, BoxDataForm.Data>(),
            program, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = execution.Interpret();
        Check(result.complete && result.errors.Count == 0 && execution.stack.Count == 0,
            "query finishes with a balanced interpreter stack: " + string.Join(";", result.errors));
        return result.ret;
    }

    private static string Handle(CharacterProductForm.Data data) => "$ch$" + data.uid + "$ch$";
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
