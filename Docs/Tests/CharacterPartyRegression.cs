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
using Z_Map;
using Z_Ui.Base;
using Z_UnitSystem;

// Run in the isolated regression project only. No scene, save or authored story is loaded.
public static class CharacterPartyRegression
{
    private static int checks;
    private static readonly List<GameObject> managers = new List<GameObject>();
    private static readonly CmdBase[] commands =
    {
        new AddCharacterToTeamCmd(), new RemoveCharacterFromTeamCmd(), new DeployCharacterCmd(),
        new UndeployCharacterCmd(), new SetCurrentCharacterCmd()
    };
    private static ProgressForm.Data progress;
    private static PlayInfoController info;
    private static PlayManager play;
    private static EventProbe probe;

    public static void Run()
    {
        bool previousPlaying = DynamicGlobalSettings.playing;
        try
        {
            SetUp();
            CheckMetadata();
            CheckCommands();
            CheckTemplateTargets();
            CheckRecruitment();
            CheckExistingUiEntrypoints();
            CheckSavedCurrentRepair();
            CheckNullEventConsumers();
            CheckInvalidInputs();
            Debug.Log("CHARACTER_PARTY_REGRESSION_PASS checks=" + checks);
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
            DynamicGlobalSettings.playing = previousPlaying;
            foreach (var manager in managers)
                if (manager != null) UnityEngine.Object.DestroyImmediate(manager);
            managers.Clear();
        }
    }

    private static void SetUp()
    {
        foreach (var form in new[] { typeof(ProductForm), typeof(CharacterProductForm),
            typeof(SkillProductForm), typeof(GameParamForm), typeof(CharacterParamForm),
            typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(CmdLabForm),
            typeof(ProgramDataForm) })
            form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic)?.Invoke(null, null);
        ProductForm.InitInternal();
        CharacterProductForm.InitInternal();
        SkillProductForm.InitInternal();
        GameParamForm.InitInternal();
        CharacterParamForm.InitInternal();
        GameCmdDataForm.InitInternal();
        GameCmdTypeDataForm.InitInternal();
        CmdLabForm.InitInternal();
        ProgramDataForm.InitInternal();
        ProgressForm.InitInternal();

        CreateManager<GameManager>();
        play = CreateManager<PlayManager>();
        info = new PlayInfoController(play);
        play.infoCtrl = info;
        // State changes before BeginScene must not require a Map or Camera singleton.
        play.sceneCtrl = null;
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.bag = new List<int>();
        ProgressForm.AddData(progress);
        ResetParty();
        DynamicGlobalSettings.playing = true;
        probe = new EventProbe();
        Z_EventHelper.Register<StoryCharacterEvent>(probe);
    }

    private static void CheckMetadata()
    {
        foreach (var command in commands)
        {
            var init = command.GetType().GetMethod("Init", BindingFlags.Static | BindingFlags.NonPublic);
            Check(init != null && init.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1,
                command.GetName() + " has explicit Player-safe runtime registration");
            init.Invoke(null, null);
            Check(BaseData.cmdDic.TryGetValue(command.GetName(), out var registered)
                && registered.GetType() == command.GetType(), command.GetName() + " is registered by canonical name");
            Check(!ReferenceEquals(command.GetNew(), command) && command.GetNew().GetType() == command.GetType(),
                command.GetName() + " creates an independent invocation");
            Check(CmdBase.TryGetForm(command.GetName(), out var metadata)
                && metadata is GameCmdDataForm.Data, command.GetName() + " is visible in command metadata");
            var gameMetadata = (GameCmdDataForm.Data)metadata;
            Check(gameMetadata.prmNames.SequenceEqual(new[] { "character" })
                && gameMetadata.prmTypes.SequenceEqual(new[] { "character" })
                && gameMetadata.retTypes.SequenceEqual(new[] { "void" })
                && gameMetadata.labId == 10007 && gameMetadata.lowestEditorStyle == EditorStyle.Rpg,
                command.GetName() + " has one typed Character argument in the Character category");
            Check(gameMetadata.defaultCode == command.GetName() + "(param1);",
                command.GetName() + " Item insertion default is a complete statement");
            var program = Compile(gameMetadata.defaultCode);
            Check(program.paramCount == 1, command.GetName() + " default binds param1");
            var restored = ProgramDataForm.GetDataByJo(ProgramDataForm.GetJoByData(program));
            Check(restored.code == program.code && restored.zCode.SequenceEqual(program.zCode)
                && restored.zCodeMap.SequenceEqual(program.zCodeMap),
                command.GetName() + " compiled default survives program serialization");
        }
    }

    private static void CheckCommands()
    {
        var source = Product(99101, 0);
        var a = Product(99102, source.uid);
        var b = Product(99103, source.uid);
        ResetParty();
        RunCode("AddCharacterToTeam(\"" + Handle(a.uid) + "\");");
        CheckParty(new[] { a.uid }, Array.Empty<int>(), 0, "Add only joins the team");
        AssertNoEvents(() => info.AddCharacterToTeam(Handle(a.uid)), "Repeated Add is idempotent");
        RunCode("DeployCharacter(\"" + Handle(a.uid) + "\");");
        CheckParty(new[] { a.uid }, new[] { a.uid }, a.uid, "First deployment also becomes current");
        AssertNoEvents(() => info.DeployCharacter(Handle(a.uid)), "Repeated Deploy is idempotent");
        RunCode("SetCurrentCharacter(\"" + Handle(b.uid) + "\");");
        CheckParty(new[] { a.uid, b.uid }, new[] { a.uid, b.uid }, b.uid,
            "Switching a non-team/non-active character joins and deploys before selecting");
        AssertNoEvents(() => info.SetCurrentCharacter(Handle(b.uid)), "Repeated current selection is idempotent");
        RunCode("UndeployCharacter(\"" + Handle(b.uid) + "\");");
        CheckParty(new[] { a.uid, b.uid }, new[] { a.uid }, a.uid,
            "Undeploying current selects the first remaining active character and retains membership");
        AssertNoEvents(() => info.UndeployCharacter(Handle(b.uid)), "Repeated Undeploy is idempotent");
        RunCode("RemoveCharacterFromTeam(\"" + Handle(a.uid) + "\");");
        CheckParty(new[] { b.uid }, Array.Empty<int>(), 0,
            "Removing the last current/active character clears it without redeploying a reserve");
        AssertNoEvents(() => info.RemoveCharacterFromTeam(Handle(a.uid)), "Repeated Remove is idempotent");
        Check(CharacterProductForm.DataByUid.ContainsKey(a.uid) && CharacterProductForm.DataByUid.ContainsKey(b.uid),
            "Membership/deployment changes never destroy Character Products");

        info.DeployCharacter(Handle(a.uid));
        CheckParty(new[] { b.uid, a.uid }, new[] { a.uid }, a.uid,
            "Deploying a removed character automatically rejoins the team");
        info.SetCurrentCharacter(Handle(b.uid));
        info.RemoveCharacterFromTeam(Handle(a.uid));
        CheckParty(new[] { b.uid }, new[] { b.uid }, b.uid,
            "Removing a non-current active character preserves current selection");
        info.UndeployCharacter(Handle(b.uid));
        CheckParty(new[] { b.uid }, Array.Empty<int>(), 0,
            "Undeploying the final character permits a team with no active/current character");
        Check(probe.events.Any(evt => evt.type == StoryCharacterEventType.ChangeTeamer)
            && probe.events.Any(evt => evt.type == StoryCharacterEventType.ChangeCharacter),
            "Team and current changes publish their existing UI notifications");
        var roundTrip = ProgressForm.GetDataByJo(ProgressForm.GetJoByData(progress));
        Check(roundTrip.team.SequenceEqual(progress.team) && roundTrip.teamActive.SequenceEqual(progress.teamActive)
            && roundTrip.characterUid == progress.characterUid, "Existing progress serialization preserves party state");
    }

    private static void CheckTemplateTargets()
    {
        var source = CharacterProductForm.DataByUid[99101];
        var a = CharacterProductForm.DataByUid[99102];
        var b = CharacterProductForm.DataByUid[99103];
        var otherSource = Product(99104, 0);
        var other = Product(99105, otherSource.uid);
        ResetParty();
        info.AddCharacterToTeam(Handle(source.uid));
        CheckParty(new[] { a.uid, b.uid }, Array.Empty<int>(), 0,
            "A template Add expands all runtime Products, not the authored template");
        info.RemoveCharacterFromTeam(Handle(a.uid));
        CheckParty(new[] { b.uid }, Array.Empty<int>(), 0, "An exact runtime reference removes only that Product");
        info.DeployCharacter(Handle(source.uid));
        CheckParty(new[] { b.uid, a.uid }, new[] { a.uid, b.uid }, a.uid,
            "A template Deploy expands every matching Product without duplicate membership");
        info.SetCurrentCharacter(Handle(b.uid));
        AssertNoEvents(() => info.SetCurrentCharacter(Handle(source.uid)),
            "Template SetCurrent retains an already-current matching instance");
        Check(progress.characterUid == b.uid, "Template current selection cannot arbitrarily jump between copies");
        info.DeployCharacter(Handle(other.uid));
        info.UndeployCharacter(Handle(source.uid));
        CheckParty(new[] { b.uid, a.uid, other.uid }, new[] { other.uid }, other.uid,
            "Template Undeploy removes all matches and falls back to unrelated active membership");
        info.RemoveCharacterFromTeam(Handle(source.uid));
        CheckParty(new[] { other.uid }, new[] { other.uid }, other.uid,
            "Template Remove removes all matches but never unrelated members");
        Check(CharacterProductForm.DataByUid[source.uid] == source
            && CharacterProductForm.DataByUid[a.uid] == a && CharacterProductForm.DataByUid[b.uid] == b,
            "Template party operations leave every Product registered");

        ResetParty();
        info.SetCurrentCharacter(Handle(source.uid));
        CheckParty(new[] { a.uid }, new[] { a.uid }, a.uid,
            "A scalar template switch selects one deterministic stored representative");
        Check(!progress.team.Contains(source.uid), "Switching a non-unique template never places authored data in the team");
    }

    private static void CheckRecruitment()
    {
        var skill = SkillProductForm.defaultData;
        skill.uid = 99301;
        skill.name = "party-authored-skill";
        skill.protoUid = 0;
        SkillProductForm.AddData(skill);
        var source = Product(99201, 0);
        source.skill[SkillType.LightAttack] = skill.uid;
        source.paramDic["PartyHp"] = new CharacterParamForm.Data(-1, "PartyHp", ValType.Float,
            "0", "10", "100", ParamShowType.Hide);
        ResetParty();
        int count = CharacterProductForm.DataByUid.Count;
        info.AddCharacterToTeam(Handle(source.uid));
        var recruited = CharacterProductForm.DataByUid[progress.team.Single()];
        Check(recruited.uid != source.uid && recruited.protoUid == source.uid
            && CharacterProductForm.DataByUid.Count == count + 1,
            "Recruiting an unspawned non-unique template creates one runtime Product");
        Check(progress.teamActive.Count == 0 && progress.characterUid == 0,
            "Creating a Product for Add still does not deploy it");
        Check(recruited.skill[SkillType.LightAttack] != skill.uid
            && SkillProductForm.DataByUid[recruited.skill[SkillType.LightAttack]].characterUid == recruited.uid
            && source.skill[SkillType.LightAttack] == skill.uid,
            "Recruitment copies equipped skills with the new Character ownership");
        Check(!ReferenceEquals(source.paramDic["PartyHp"], recruited.paramDic["PartyHp"]),
            "Recruited Character parameters are independent of authored defaults");
        recruited.paramDic["PartyHp"].v = "42";
        Check(source.paramDic["PartyHp"].v == "10", "Runtime parameter edits cannot mutate the recruitment template");
        info.DeployCharacter(Handle(source.uid));
        info.SetCurrentCharacter(Handle(source.uid));
        Check(CharacterProductForm.DataByUid.Count == count + 1 && progress.characterUid == recruited.uid,
            "Deploy/Switch reuse the recruited Product instead of creating more copies");

        foreach (bool switchCurrent in new[] { false, true })
        {
            var unspawned = Product(switchCurrent ? 99203 : 99202, 0);
            ResetParty();
            count = CharacterProductForm.DataByUid.Count;
            if (switchCurrent) info.SetCurrentCharacter(Handle(unspawned.uid));
            else info.DeployCharacter(Handle(unspawned.uid));
            var target = CharacterProductForm.DataByUid[progress.characterUid];
            Check(target.uid != unspawned.uid && target.protoUid == unspawned.uid
                && CharacterProductForm.DataByUid.Count == count + 1
                && progress.team.Contains(target.uid) && progress.teamActive.Contains(target.uid),
                (switchCurrent ? "Switch" : "Deploy") + " independently recruits an unspawned template");
        }
        var untouched = Product(99204, 0);
        count = CharacterProductForm.DataByUid.Count;
        AssertNoEvents(() => info.RemoveCharacterFromTeam(Handle(untouched.uid)),
            "Removing a never-recruited template is a no-op");
        AssertNoEvents(() => info.UndeployCharacter(Handle(untouched.uid)),
            "Undeploying a never-recruited template is a no-op");
        Check(CharacterProductForm.DataByUid.Count == count,
            "Removal and Undeploy never instantiate missing runtime Products");

        var unique = Product(99205, 0, true);
        ResetParty();
        count = CharacterProductForm.DataByUid.Count;
        info.SetCurrentCharacter(Handle(unique.uid));
        CheckParty(new[] { unique.uid }, new[] { unique.uid }, unique.uid,
            "An authored unique Character retains singleton identity when selected");
        info.RemoveCharacterFromTeam(Handle(unique.uid));
        CheckParty(Array.Empty<int>(), Array.Empty<int>(), 0, "Unique removal clears membership and current");
        Check(CharacterProductForm.DataByUid.Count == count && CharacterProductForm.DataByUid[unique.uid] == unique,
            "Unique party changes neither clone nor destroy the authored singleton");
    }

    private static void CheckExistingUiEntrypoints()
    {
        int a = 99102, b = 99103, c = 99105;
        ResetParty();
        info.ChooseCurrentCharacter(a);
        CheckParty(new[] { a }, new[] { a }, a,
            "Existing current-character UI entrypoint enforces automatic team and active membership");
        AssertNoEvents(() => info.ChooseCurrentCharacter(a), "Existing current selection is idempotent");
        info.DeployCharacter(Handle(b));
        info.ReplaceActiveTeamer(a, b, 0);
        CheckParty(new[] { a, b }, new[] { b }, b,
            "Replacing current with an already-active character deduplicates and repairs selection");
        info.ReplaceActiveTeamer(0, c, int.MaxValue);
        CheckParty(new[] { a, b, c }, new[] { b, c }, b,
            "Adding through an old empty UI slot clamps its index and joins the new member");
        info.ReplaceActiveTeamer(0, c, -5);
        Check(progress.teamActive.Distinct().Count() == progress.teamActive.Count
            && progress.teamActive.Contains(c), "Existing active insertion cannot duplicate a member at a negative slot");

        progress.team = new List<int> { a, b, c };
        progress.teamActive = new List<int> { c, b };
        progress.characterUid = b;
        info.ReplaceTeamer(b, a);
        CheckParty(new[] { a, b, c }, new[] { c, a }, c,
            "Existing replacement keeps the chosen slot and selects first remaining active when current leaves");
        AssertNoEvents(() => info.ReplaceTeamer(a, a), "Existing self replacement is idempotent");

        progress.team = new List<int> { a, a, -1, int.MaxValue };
        progress.teamActive = new List<int> { b, b, int.MaxValue };
        progress.characterUid = int.MaxValue;
        info.AddCharacterToTeam(Handle(c));
        Check(progress.team.ToHashSet().SetEquals(new[] { a, b, c })
            && progress.team.Count == 3 && progress.teamActive.SequenceEqual(new[] { b })
            && progress.characterUid == b,
            "A valid operation repairs duplicate/stale IDs and active membership from old state");
        AssertInvariant("repaired old state");
        progress.team = null;
        progress.teamActive = null;
        progress.characterUid = a;
        info.AddCharacterToTeam(Handle(c));
        CheckParty(new[] { c }, Array.Empty<int>(), 0, "A valid operation initializes missing saved party lists safely");
    }

    private static void CheckInvalidInputs()
    {
        ResetParty();
        info.SetCurrentCharacter(Handle(99102));
        string[] malformed = { null, "", "character", "$ch$$ch$", "$ch$x$ch$", "$ch$0$ch$",
            "$ch$-1$ch$", "$ch$1$ch$extra", "$ch$99999999999999999$ch$", "$so$99102$so$", "$ch$999999$ch$" };
        foreach (var command in commands)
        {
            foreach (string reference in malformed)
                AssertNoEvents(() => Invoke(command, CodeHelper.CreateBoxByStr(reference)),
                    command.GetName() + " safely ignores invalid reference " + reference);
            AssertNoEvents(() => Invoke(command, Array.Empty<BoxDataForm.Data>()),
                command.GetName() + " safely handles an empty direct argument array");
            AssertNoEvents(() => RunCode(command.GetName() + "();"),
                command.GetName() + " omitted arguments compile, pad and no-op safely");
        }
        CheckParty(new[] { 99102 }, new[] { 99102 }, 99102, "Invalid commands preserve valid existing state");
        AssertNoEvents(() => info.ChooseCurrentCharacter(int.MaxValue), "Missing numeric UI selection is a no-op");
        AssertNoEvents(() => info.ReplaceActiveTeamer(99102, int.MaxValue, 0), "Missing replacement Product is a no-op");
        play.infoCtrl = null;
        foreach (var command in commands) Invoke(command, CodeHelper.CreateBoxByStr(Handle(99102)));
        play.infoCtrl = info;
        CheckParty(new[] { 99102 }, new[] { 99102 }, 99102, "Commands before controller readiness are safe");
        ProgressForm.RemoveData(progress.uid);
        foreach (var command in commands) Invoke(command, CodeHelper.CreateBoxByStr(Handle(99102)));
        ProgressForm.AddData(progress);
        CheckParty(new[] { 99102 }, new[] { 99102 }, 99102, "Commands with no loaded progress do not corrupt retained data");
    }

    private static void CheckSavedCurrentRepair()
    {
        // The constructor only captures its Manager; call the in-memory repair
        // directly, never LoadSaveStory/SaveProgress or any filesystem entrypoint.
        var save = new GameSaveController(GameManager.instance);
        var repair = typeof(GameSaveController).GetMethod("RepairSavedCurrentCharacter",
            BindingFlags.Instance | BindingFlags.NonPublic);
        Check(repair != null, "Saved-current repair is available for in-memory compatibility checks");
        int a = 99102, b = 99103;

        ResetParty();
        progress.team.AddRange(new[] { a, b });
        Check(!(bool)repair.Invoke(save, null),
            "An explicit empty active list with current zero does not request a save repair");
        CheckParty(new[] { a, b }, Array.Empty<int>(), 0,
            "Reload preserves the deliberate no-deployed-character state with reserve members");

        progress.team = new List<int> { a };
        progress.teamActive = new List<int> { b };
        progress.characterUid = 0;
        Check((bool)repair.Invoke(save, null), "Missing current with an active candidate still requests repair");
        CheckParty(new[] { a, b }, new[] { b }, b,
            "Saved-current repair prefers the active candidate and adds its missing team membership");

        progress.team = new List<int> { a };
        progress.teamActive = new List<int>();
        progress.characterUid = int.MaxValue;
        Check((bool)repair.Invoke(save, null), "A nonzero missing saved current keeps the legacy repair behavior");
        CheckParty(new[] { a }, new[] { a }, a,
            "A corrupted saved current falls back to a valid reserve and makes it active");

        progress.team = new List<int> { a };
        progress.teamActive = null;
        progress.characterUid = 0;
        Check((bool)repair.Invoke(save, null), "A missing legacy active-list field is distinct from explicit empty");
        CheckParty(new[] { a }, new[] { a }, a,
            "Legacy progress with null active list still restores current and active membership");
    }

    private static void CheckNullEventConsumers()
    {
        var triggers = new GameEventStoryTriggerController(null);
        try
        {
            var hud = new Ui.PlaySceneMain.UiPlaySceneMainCtrl();
            foreach (StoryCharacterEventType type in Enum.GetValues(typeof(StoryCharacterEventType)))
            {
                var notification = new StoryCharacterEvent { type = type, data = null };
                triggers.OnEvent(notification);
                hud.OnEvent(notification);
                Check(true, "Story trigger and hidden HUD ignore missing Character data for " + type);
            }
            // An enabled UI controller can receive a non-party notification with no
            // Product. It must not dereference data or attempt a full HUD refresh.
            var root = new GameObject("party-null-event-hud");
            root.SetActive(false);
            managers.Add(root);
            var holder = root.AddComponent<UiHolder>();
            holder.enabled = false;
            hud.uiHolder = holder;
            hud.inited = true;
            root.SetActive(true);
            Check(hud.active, "Null-data HUD consumer check exercises an active controller");
            hud.OnEvent(new StoryCharacterEvent { type = StoryCharacterEventType.ParamChange, data = null });
            hud.OnEvent(new StoryCharacterEvent { type = StoryCharacterEventType.SkillParamChange, data = null });
            Check(true, "Active HUD ignores null parameter notifications without refreshing unbound views");
        }
        finally
        {
            Z_EventHelper.Unregister<StoryLifeEvent>(triggers);
            Z_EventHelper.Unregister<StoryItemEvent>(triggers);
            Z_EventHelper.Unregister<StoryCharacterEvent>(triggers);
            Z_EventHelper.Unregister<CharacterSkillEvent>(triggers);
        }
    }

    private static CharacterProductForm.Data Product(int uid, int protoUid, bool unique = false)
    {
        var data = CharacterProductForm.defaultData;
        data.uid = uid;
        data.name = "party-character-" + uid;
        data.protoUid = protoUid;
        data.unique = unique;
        data.paramDic = new Dictionary<string, CharacterParamForm.Data>();
        data.skill = new Dictionary<SkillType, int>();
        CharacterProductForm.AddData(data);
        return data;
    }

    private static ProgramDataForm.Data Compile(string source)
    {
        var compiler = new Compiler();
        bool ok = compiler.TryCompile(source, out var code, out _, out int count, out string result,
            out var sourceMap, out var errors);
        Check(ok && code.Count > 0 && code.Count == sourceMap.Count,
            "Party source compiles with aligned source map: " + source + ": " + string.Join("; ", errors));
        return new ProgramDataForm.Data(-1, "party-regression", source, code, sourceMap, count, result);
    }

    private static void RunCode(string source)
    {
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            new Dictionary<string, BoxDataForm.Data>(), Compile(source), 0, -1, 0, null,
            new List<BoxDataForm.Data>(), 0);
        var result = data.Interpret();
        Check(result.complete && result.errors.Count == 0 && data.stack.Count == 0,
            "Compiled party command completes synchronously with a balanced stack: " + source
            + ": " + string.Join("; ", result.errors));
    }

    private static void Invoke(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments, new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && task.error == null, command.GetName() + " completes without runtime error: " + task.error);
    }

    private static void ResetParty()
    {
        progress.team = new List<int>();
        progress.teamActive = new List<int>();
        progress.characterUid = 0;
        probe?.events.Clear();
    }

    private static void CheckParty(int[] team, int[] active, int current, string context)
    {
        Check(progress.team.SequenceEqual(team) && progress.teamActive.SequenceEqual(active)
            && progress.characterUid == current, context + ": team=" + string.Join(",", progress.team)
            + " active=" + string.Join(",", progress.teamActive) + " current=" + progress.characterUid);
        AssertInvariant(context);
    }

    private static void AssertInvariant(string context)
    {
        Check(progress.team != null && progress.teamActive != null
            && progress.team.Distinct().Count() == progress.team.Count
            && progress.teamActive.Distinct().Count() == progress.teamActive.Count
            && progress.team.All(uid => uid > 0 && CharacterProductForm.DataByUid.ContainsKey(uid))
            && progress.teamActive.All(uid => progress.team.Contains(uid))
            && (progress.characterUid == 0 || progress.teamActive.Contains(progress.characterUid)),
            "Current is active, active is in team, and all IDs are valid/unique: " + context);
    }

    private static void AssertNoEvents(Action action, string message)
    {
        int count = probe.events.Count;
        string before = State();
        action();
        Check(probe.events.Count == count && State() == before, message);
    }

    private static string State() => string.Join(",", progress.team) + "|"
        + string.Join(",", progress.teamActive) + "|" + progress.characterUid;
    private static string Handle(int uid) => "$ch$" + uid + "$ch$";

    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
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

    private sealed class EventProbe : IZ_Listener<StoryCharacterEvent>
    {
        public readonly List<StoryCharacterEvent> events = new List<StoryCharacterEvent>();

        public void OnEvent(StoryCharacterEvent evt)
        {
            AssertInvariant("before publishing " + evt.type);
            events.Add(evt);
        }
    }
}
