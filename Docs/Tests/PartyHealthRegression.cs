using System;
using System.Collections.Generic;
using System.Globalization;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DataSystem.Form;
using Z_DesignStyle;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem;
using Object = UnityEngine.Object;

// Real command and party-controller execution, without a user scene or save.
public static class PartyHealthRegression
{
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const string HpName = "PartyHP";
    private static readonly List<GameObject> roots = new List<GameObject>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static readonly CmdBase[] queries = { new GetDeployedCharactersCmd(), new GetTeamCharactersCmd() };
    private static ProgressForm.Data progress;
    private static PlayInfoController info;
    private static PlayManager play;
    private static SceneSpy scene;
    private static EventProbe probe;
    private static CharacterProductForm.Data a, b, c, reserve;
    private static int checks;

    public static void Run()
    {
        bool oldPlaying = DynamicGlobalSettings.playing;
        bool oldPauseNav = DynamicGlobalSettings.pauseNav;
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name
                .StartsWith("MapRuntimeRegression-", StringComparison.Ordinal), "isolated project guard");
            SetUp();
            MetadataAndInterpreter();
            ParameterDeathAndAllDead();
            RejectDeadSwitches();
            FrameAndSceneEntryRepair();
            MembershipFallback();
            ListContentsAndOwnership();
            MissingProgressAndLists();
            Debug.Log("PARTY_HEALTH_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (probe != null) Z_EventHelper.Unregister<StoryCharacterEvent>(probe);
            if (info != null)
            {
                Z_EventHelper.Unregister<CollideEvent>(info);
                Z_EventHelper.Unregister<MissionEvent>(info);
            }
            DynamicGlobalSettings.playing = oldPlaying;
            DynamicGlobalSettings.pauseNav = oldPauseNav;
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
        var root = new GameObject("party-health-regression-" + typeof(T).Name);
        roots.Add(root);
        root.SetActive(false);
        var manager = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, manager);
        return manager;
    }

    private static void SetUp()
    {
        foreach (var form in new[] { typeof(CharacterProductForm), typeof(SkillProductForm),
            typeof(ProgressForm), typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(CmdLabForm),
            typeof(ProgramDataForm), typeof(BoxDataForm), typeof(CharacterParamForm) })
            RegisterForm(form);
        foreach (var command in queries.Concat(new CmdBase[] { new SetCurrentCharacterCmd(), new LenCmd() }))
            command.GetType().GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly).Invoke(null, null);
        Manager<GameManager>();
        play = Manager<PlayManager>();
        scene = new SceneSpy();
        play.sceneCtrl = scene;
        info = new PlayInfoController(play);
        play.infoCtrl = info;
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.bag = new List<int>();
        ProgressForm.AddData(progress);
        DynamicGlobalSettings.playing = true;
        a = Product(610001, 620001, 10f);
        b = Product(610002, 620002, 10f);
        c = Product(610003, 620003, 10f);
        reserve = Product(610004, 620004, 10f);
        probe = new EventProbe();
        Z_EventHelper.Register<StoryCharacterEvent>(probe);
        ResetParty();
    }

    private static void MetadataAndInterpreter()
    {
        for (int i = 0; i < queries.Length; i++)
        {
            var command = queries[i];
            Check(CmdBase.TryGetForm(command.GetName(), out var form)
                && form is GameCmdDataForm.Data typed && typed.uid == 100131 + i && typed.labId == 10007,
                command.GetName() + " is registered in the Character command category");
            Check((form.prmNames == null || form.prmNames.Count == 0)
                && (form.prmTypes == null || form.prmTypes.Count == 0)
                && form.retNames.SequenceEqual(new[] { "list" }) && form.retTypes.SequenceEqual(new[] { "var" }),
                command.GetName() + " takes no arguments and returns one list Box");
            Check(GameCmdDataForm.DataByUid.Values.Count(row => row.name == command.GetName()) == 1,
                command.GetName() + " appears exactly once in the chooser");
            Check(BaseData.cmdDic[command.GetName()].GetType() == command.GetType()
                && command.GetNew().GetType() == command.GetType() && !ReferenceEquals(command, command.GetNew()),
                command.GetName() + " has a distinct production invocation");
            var init = command.GetType().GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly);
            Check(init.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1,
                command.GetName() + " is explicitly registered for Player builds");
            var compiled = Compile("Return " + form.defaultCode.TrimEnd(';') + ";");
            var restored = ProgramDataForm.GetDataByJo(ProgramDataForm.GetJoByData(compiled));
            Check(restored.code == compiled.code && restored.zCode.SequenceEqual(compiled.zCode)
                && restored.zCodeMap.SequenceEqual(compiled.zCodeMap), command.GetName() + " survives program Form round-trip");
            CheckList(Run(restored), Array.Empty<int>(), command.GetName() + " default executes with an empty party");
        }
    }

    private static void ParameterDeathAndAllDead()
    {
        ResetParty();
        SetParty(new[] { a.uid, b.uid, c.uid, reserve.uid }, new[] { a.uid, b.uid, c.uid }, a.uid);
        Hp(b, 0f);
        info.Begin();
        probe.events.Clear();
        scene.Clear();
        info.ChangeCharacterParam(a.uid, HpName, 0f);
        Check(progress.characterUid == c.uid, "HP zero immediately switches to the first living deployed member");
        CheckLists(new[] { a.uid, b.uid, c.uid, reserve.uid }, new[] { a.uid, b.uid, c.uid },
            "death neither removes team membership nor undeploys the fallen member");
        Check(scene.refreshes.SequenceEqual(new[] { c.uid }) && Changes().Single().data == c,
            "auto-switch refreshes the player and publishes the final Character once");
        AssertNoNotifications(info.EnsureCurrentCharacterAlive, "a healthy current is not refreshed or notified repeatedly");
        info.ChangeCharacterParam(reserve.uid, HpName, 0f);
        Check(progress.characterUid == c.uid && scene.refreshes.Count == 1,
            "changing a reserve Character does not replace a living current");
        Hp(reserve, 50f);
        info.ChangeCharacterParam(c.uid, HpName, -1f);
        Check(progress.characterUid == 0, "all deployed HP zero clears current instead of selecting a living reserve");
        Check(scene.refreshes.SequenceEqual(new[] { c.uid, 0 }) && Changes().Count == 2
            && Changes()[1].data == null, "clearing current refreshes the scene and sends a null Character event once");
        CheckLists(new[] { a.uid, b.uid, c.uid, reserve.uid }, new[] { a.uid, b.uid, c.uid },
            "the all-dead state retains every deployed and reserve entry");
        AssertNoNotifications(info.EnsureCurrentCharacterAlive, "an all-dead empty current is idempotent");
        Hp(c, 5f);
        AssertNoNotifications(info.EnsureCurrentCharacterAlive, "a deliberately empty current is not seized on revival");
        info.ChooseCurrentCharacter(c.uid);
        Check(progress.characterUid == c.uid, "a revived deployed member can be chosen explicitly");
    }

    private static void RejectDeadSwitches()
    {
        ResetParty();
        SetParty(new[] { a.uid, b.uid }, new[] { a.uid, b.uid }, a.uid);
        Hp(b, 0f);
        Hp(reserve, 0f);
        AssertNoNotifications(() => info.SetCurrentCharacter(Handle(b.uid)), "an already-deployed dead Character cannot become current");
        AssertNoNotifications(() => info.ChooseCurrentCharacter(b.uid), "the existing UI/keyboard chooser also rejects a dead Character");
        AssertNoNotifications(() => Run("SetCurrentCharacter(\"" + Handle(b.uid) + "\");"),
            "the compiled SetCurrentCharacter command rejects a dead Character");
        AssertNoNotifications(() => info.SetCurrentCharacter(Handle(reserve.uid)), "rejecting a dead outsider does not implicitly join or deploy it");
        CheckLists(new[] { a.uid, b.uid }, new[] { a.uid, b.uid }, "rejected dead switches leave both membership lists unchanged");

        var deadProto = Product(611001, 0, 0f);
        int productCount = CharacterProductForm.DataByUid.Count;
        AssertNoNotifications(() => info.SetCurrentCharacter(Handle(deadProto.uid)), "an unspawned dead prototype is not recruited by a switch");
        Check(CharacterProductForm.DataByUid.Count == productCount, "a rejected prototype switch creates no runtime Product");
        var deadUnique = Product(611002, 0, 0f, true);
        AssertNoNotifications(() => info.SetCurrentCharacter(Handle(deadUnique.uid)), "a dead unique singleton is not selectable");

        var proto = Product(611010, 0, 0f);
        var deadCopy = Product(611011, proto.uid, 0f);
        var livingCopy = Product(611012, proto.uid, 5f);
        info.SetCurrentCharacter(Handle(proto.uid));
        Check(progress.characterUid == livingCopy.uid && !progress.team.Contains(deadCopy.uid)
            && !progress.team.Contains(proto.uid), "a prototype switch skips its first dead runtime representative");
        Check(PlayInfoController.CanBeCurrentCharacter(livingCopy) && !PlayInfoController.CanBeCurrentCharacter(deadCopy)
            && !PlayInfoController.CanBeCurrentCharacter(null), "the shared predicate distinguishes living, dead and missing Products");
        var unbound = Product(611020, 621020, 0f);
        unbound.hpParamName = "";
        Check(PlayInfoController.CanBeCurrentCharacter(unbound), "an unbound HP field preserves existing selectable behavior");
        unbound.hpParamName = "MissingHp";
        Check(PlayInfoController.CanBeCurrentCharacter(unbound), "a missing bound parameter is not invented as zero HP");
        unbound.paramDic = null;
        Check(PlayInfoController.CanBeCurrentCharacter(unbound), "a missing parameter dictionary is compatible");
    }

    private static void FrameAndSceneEntryRepair()
    {
        ResetParty();
        SetParty(new[] { a.uid, b.uid }, new[] { a.uid, b.uid }, a.uid);
        info.Begin();
        Hp(a, 0f); // Deliberately bypass the parameter controller/event path.
        progress.blockProgramUid = 123;
        info.Update();
        Check(progress.characterUid == b.uid && scene.refreshes.SequenceEqual(new[] { b.uid }),
            "frame health validation catches direct Box changes even during a blocked event");
        AssertNoNotifications(info.Update, "subsequent blocked frames do not re-notify the same current");
        info.End();
        Hp(a, 10f);
        progress.characterUid = a.uid;
        Hp(a, 0f);
        info.Begin();
        Check(progress.characterUid == b.uid, "scene info Begin repairs a dead current before active updates");

        progress.characterUid = a.uid;
        var restored = ProgressForm.GetDataByJo(ProgressForm.GetJoByData(progress));
        var oldTeam = restored.team;
        var oldActive = restored.teamActive;
        string before = State();
        int eventCount = probe.events.Count, refreshCount = scene.refreshes.Count;
        Check(PlayInfoController.RepairCurrentCharacterHealth(restored) && restored.characterUid == b.uid,
            "the scene-entry pure repair accepts saved party state with a dead current");
        Check(ReferenceEquals(oldTeam, restored.team) && ReferenceEquals(oldActive, restored.teamActive)
            && restored.team.SequenceEqual(progress.team) && restored.teamActive.SequenceEqual(progress.teamActive)
            && State() == before && probe.events.Count == eventCount && scene.refreshes.Count == refreshCount,
            "pre-player-creation repair changes only the supplied current UID, without events or list replacement");
        Check(!PlayInfoController.RepairCurrentCharacterHealth(restored), "the pure scene repair is idempotent");
        restored.teamActive = new List<int> { 0, -1, int.MaxValue, a.uid };
        restored.characterUid = a.uid;
        Check(PlayInfoController.RepairCurrentCharacterHealth(restored) && restored.characterUid == 0,
            "pure repair skips stale IDs and clears current if no active Character is living");
        Check(!PlayInfoController.RepairCurrentCharacterHealth(null), "pure repair safely accepts missing Progress");
        progress.blockProgramUid = 0;
    }

    private static void MembershipFallback()
    {
        ResetParty();
        Hp(b, 0f);
        SetParty(new[] { a.uid, b.uid, c.uid }, new[] { a.uid, b.uid, c.uid }, a.uid);
        info.UndeployCharacter(Handle(a.uid));
        Check(progress.characterUid == c.uid && progress.teamActive.SequenceEqual(new[] { b.uid, c.uid }),
            "undeploying current skips the first dead active member");
        Hp(reserve, 0f);
        info.ReplaceActiveTeamer(c.uid, reserve.uid, 1);
        Check(progress.characterUid == 0 && progress.teamActive.SequenceEqual(new[] { b.uid, reserve.uid }),
            "replacing the final living active member with a dead member leaves no current");
        ResetParty();
        Hp(b, 0f);
        SetParty(new[] { a.uid, b.uid, c.uid }, new[] { a.uid, b.uid, c.uid }, a.uid);
        info.RemoveCharacterFromTeam(Handle(a.uid));
        Check(progress.characterUid == c.uid && progress.team.SequenceEqual(new[] { b.uid, c.uid }),
            "removing current chooses the first living remaining active member");
    }

    private static void ListContentsAndOwnership()
    {
        ResetParty();
        Hp(b, 0f);
        SetParty(new[] { c.uid, a.uid, b.uid, c.uid, int.MaxValue, 0, -1 },
            new[] { b.uid, c.uid, b.uid, int.MaxValue }, c.uid);
        string before = State();
        int productCount = CharacterProductForm.DataByUid.Count;
        var deployed = Execute(queries[0]);
        var team = Execute(queries[1]);
        CheckList(deployed, new[] { b.uid, c.uid }, "deployed query retains order and dead members, filtering stale/duplicate IDs");
        CheckList(team, new[] { c.uid, a.uid, b.uid }, "team query includes both reserve and deployed members in team order");
        Check(Run("Return Len(GetDeployedCharacters());").num == 2f
            && Run("Return Len(GetTeamCharacters());").num == 3f,
            "existing Len reads contiguous numeric dictionary keys from both queries");
        Check(Run("members = GetTeamCharacters(); Return members[0];").str == Handle(c.uid),
            "script list indexing yields ordinary Character references");
        Check(State() == before && CharacterProductForm.DataByUid.Count == productCount && probe.events.Count == 0
            && scene.refreshes.Count == 0, "list queries do not repair or mutate Progress/Products or notify listeners");
        team.dic["0"].str = "changed";
        team.dic.Remove("1");
        CheckList(Execute(queries[1]), new[] { c.uid, a.uid, b.uid }, "caller edits cannot corrupt subsequent list queries");
        progress.teamActive.Clear();
        CheckList(deployed, new[] { b.uid, c.uid }, "returned lists are snapshots independent of later membership edits");
    }

    private static void MissingProgressAndLists()
    {
        ResetParty();
        progress.team = null;
        progress.teamActive = null;
        foreach (var query in queries) CheckList(Execute(query), Array.Empty<int>(), query.GetName() + " handles null saved lists");
        ProgressForm.RemoveData(progress.uid);
        foreach (var query in queries)
        {
            CheckList(Execute(query), Array.Empty<int>(), query.GetName() + " handles missing Progress");
            Check(Run("Return Len(" + query.GetName() + "());").num == 0f,
                query.GetName() + " remains script-compatible with no loaded Progress");
        }
        info.EnsureCurrentCharacterAlive();
        ProgressForm.AddData(progress);
        ResetParty();
    }

    private static CharacterProductForm.Data Product(int uid, int protoUid, float hp, bool unique = false)
    {
        var data = CharacterProductForm.defaultData;
        data.uid = uid;
        data.name = "party-health-character-" + uid;
        data.protoUid = protoUid;
        data.unique = unique;
        data.skill = new Dictionary<SkillType, int>();
        data.paramDic = new Dictionary<string, CharacterParamForm.Data>();
        data.hpParamName = HpName;
        data.paramDic[HpName] = new CharacterParamForm.Data(-1, HpName, ValType.Float, "", "", "", ParamShowType.Hide);
        Hp(data, hp);
        CharacterProductForm.AddData(data);
        return data;
    }

    private static void Hp(CharacterProductForm.Data character, float value) => character.paramDic[HpName].SetValue(value);
    private static void ResetParty()
    {
        info.End();
        foreach (var member in new[] { a, b, c, reserve }) Hp(member, 10f);
        progress.team = new List<int>();
        progress.teamActive = new List<int>();
        progress.characterUid = 0;
        progress.blockProgramUid = 0;
        probe.events.Clear();
        scene.Clear();
    }

    private static void SetParty(int[] team, int[] active, int current)
    {
        progress.team = team.ToList();
        progress.teamActive = active.ToList();
        progress.characterUid = current;
    }

    private static void CheckLists(int[] team, int[] active, string message) =>
        Check(progress.team.SequenceEqual(team) && progress.teamActive.SequenceEqual(active), message);
    private static List<StoryCharacterEvent> Changes() => probe.events
        .Where(evt => evt.type == StoryCharacterEventType.ChangeCharacter).ToList();
    private static void AssertNoNotifications(Action action, string message)
    {
        int events = probe.events.Count, refreshes = scene.refreshes.Count;
        string before = State();
        action();
        Check(State() == before && probe.events.Count == events && scene.refreshes.Count == refreshes, message);
    }
    private static string State() => string.Join(",", progress.team ?? new List<int>()) + "|"
        + string.Join(",", progress.teamActive ?? new List<int>()) + "|" + progress.characterUid;
    private static string Handle(int uid) => "$ch$" + uid.ToString(CultureInfo.InvariantCulture) + "$ch$";

    private static BoxDataForm.Data Execute(CmdBase command)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(Array.Empty<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && task.error == null && task.res != null && task.res.Length == 1 && task.res[0] != null,
            command.GetName() + " completes synchronously with one result: " + task.error);
        return task.res[0];
    }
    private static void CheckList(BoxDataForm.Data list, int[] expected, string message)
    {
        Check(list.dic != null && list.dic.Count == expected.Length
            && expected.Select((uid, index) => list.dic.TryGetValue(index.ToString(CultureInfo.InvariantCulture), out var value)
                && value != null && value.str == Handle(uid)).All(value => value), message);
    }
    private static ProgramDataForm.Data Compile(string source)
    {
        var program = ProgramDataForm.defaultData;
        program.name = "party-health-regression";
        Check(program.TryApplyCode(source, out _, out _), "source compiles: " + source);
        return program;
    }
    private static BoxDataForm.Data Run(string source) => Run(Compile(source));
    private static BoxDataForm.Data Run(ProgramDataForm.Data program)
    {
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            program, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = data.Interpret();
        Check(result.complete && result.errors.Count == 0 && data.stack.Count == 0,
            "interpreted query completes with a balanced stack: " + string.Join(";", result.errors));
        return result.ret;
    }
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
    private sealed class EventProbe : IZ_Listener<StoryCharacterEvent>
    {
        public readonly List<StoryCharacterEvent> events = new List<StoryCharacterEvent>();
        public void OnEvent(StoryCharacterEvent evt) => events.Add(evt);
    }
    private sealed class SceneSpy : ExternalPlaySceneController
    {
        public readonly List<int> refreshes = new List<int>();
        public CharacterUnitForm.Data playerM => null;
        public CharacterProductForm.Data playerG => CharacterProductForm.DataByUid.TryGetValue(progress.characterUid, out var value) ? value : null;
        public void Clear() => refreshes.Clear();
        public void RefreshCurrentCharacter() => refreshes.Add(progress.characterUid);
        public void SetCamera(float x, float y, float z) { }
        public Vector3 GetPlayerPos() => Vector3.zero;
        public void SetPlayerPos(Vector3 pos) { }
        public void SetPlayerMove(Vector3 dir) { }
        public void SetPlayerRotation(Vector3 dir, float speed) { }
        public void AddMessage(string content) { }
        public void ForceUpdate() { }
        public SkillType? GetCurOptSkill() => null;
        public void SetCurOptSkill(SkillType? type) { }
        public CharacterUnitForm.Data GetCharacterUnit(int productUid) => null;
        public List<CharacterUnitForm.Data> GetCharacterUnits(int productUid) => new List<CharacterUnitForm.Data>();
        public bool RemoveCharacter(CharacterUnitForm.Data data) => false;
    }
}
