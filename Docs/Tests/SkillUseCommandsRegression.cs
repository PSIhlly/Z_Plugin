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
using Z_UnitSystem;
using Object = UnityEngine.Object;

// Production command/controller execution in the runner's disposable project.
public static class SkillUseCommandsRegression
{
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly List<GameObject> roots = new List<GameObject>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static readonly CmdBase[] slotCommands =
    {
        new UseCharacterLightAttackCmd(), new UseCharacterHeavyAttackCmd(),
        new UseCharacterESkillCmd(), new UseCharacterQSkillCmd()
    };
    private static readonly SkillType[] slots = { SkillType.LightAttack, SkillType.HeavyAttack, SkillType.E, SkillType.Q };
    private static readonly int[] commandUids = { 100126, 100127, 100128, 100129 };
    private static PlayManager play;
    private static PlayInfoController info;
    private static ProgressForm.Data progress;
    private static Probe probe;
    private static int checks;
    private static int nextUid = 480100;

    public static void Run()
    {
        bool oldPlaying = DynamicGlobalSettings.playing;
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name
                .StartsWith("MapRuntimeRegression-", StringComparison.Ordinal), "isolated project guard");
            SetUp();
            MetadataAndDefaults();
            EquippedSlotsAndCooldown();
            SpecificSkillIgnoresCooldown();
            PrototypeTargetsAndReplacement();
            InvalidTargetsAndReadiness();
            LegacySignatureMigration();
            Debug.Log("SKILL_USE_COMMANDS_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            if (probe != null) Z_EventHelper.Unregister<CharacterSkillEvent>(probe);
            if (info != null)
            {
                Z_EventHelper.Unregister<CollideEvent>(info);
                Z_EventHelper.Unregister<MissionEvent>(info);
            }
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
        var root = new GameObject("skill-command-regression-" + typeof(T).Name);
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
            typeof(ProgramDataForm), typeof(EventProgramDataForm), typeof(BoxDataForm), typeof(CharacterParamForm), typeof(SkillParamForm) })
            RegisterForm(form);
        foreach (var command in slotCommands.Concat(new[] { new UseSkillCmd() }))
            command.GetType().GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly).Invoke(null, null);
        Manager<GameManager>();
        play = Manager<PlayManager>();
        play.sceneCtrl = new PlaySceneController(play);
        info = new PlayInfoController(play);
        play.infoCtrl = info;
        progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.enableSkill = true;
        progress.seconds = 100f;
        progress.blockProgramUid = 0;
        ProgressForm.AddData(progress);
        DynamicGlobalSettings.playing = true;
        probe = new Probe();
        Z_EventHelper.Register<CharacterSkillEvent>(probe);
    }

    private static void MetadataAndDefaults()
    {
        var all = slotCommands.Concat(new[] { new UseSkillCmd() }).ToArray();
        for (int i = 0; i < all.Length; i++)
        {
            var command = all[i];
            Check(CmdBase.TryGetForm(command.GetName(), out var form), command.GetName() + " has executable metadata");
            var typed = (GameCmdDataForm.Data)form;
            Check(typed.uid == (i < slotCommands.Length ? commandUids[i] : 100081) && typed.labId == 10007,
                command.GetName() + " retains a unique Character CmdLab identity");
            Check(form.prmNames.Count == 2 && form.prmTypes.SequenceEqual(new[] { "character", i < slotCommands.Length ? "num" : "skill" }),
                command.GetName() + " exposes only its intended two parameters");
            Check(form.retTypes.SequenceEqual(new[] { "void" }) && form.defaultCode.EndsWith(";", StringComparison.Ordinal),
                command.GetName() + " is an independently callable void statement");
            Check(BaseData.cmdDic[command.GetName()].GetNew().GetName() == command.GetName(),
                command.GetName() + " creates the correct concrete command");
            var init = command.GetType().GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly);
            Check(init.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1,
                command.GetName() + " is explicitly registered for Player builds");
            var saved = Compile(form.defaultCode);
            var restored = ProgramDataForm.GetDataByJo(ProgramDataForm.GetJoByData(saved));
            Check(restored.code == saved.code && restored.zCode.SequenceEqual(saved.zCode)
                && restored.zCodeMap.SequenceEqual(saved.zCodeMap), command.GetName() + " default survives Form round-trip");
        }
        var old = Compile("UseSkill(\"$ch$480002$ch$\",\"$sk$$sk$\",0);");
        var retired = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            old, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0).Interpret();
        Check(retired.complete && retired.errors.Any(error => error.ToString().Contains("最多接受 2") && error.ToString().Contains("实际为 3")),
            "unmigrated three-argument calls are rejected by existing interpreter arity validation");
    }

    private static void EquippedSlotsAndCooldown()
    {
        var character = Character(480001, 490001);
        var skills = slots.Select(slot => Equip(character, slot)).ToArray();
        for (int i = 0; i < slotCommands.Length; i++)
        {
            var command = slotCommands[i];
            var skill = skills[i];
            Reset();
            skill.lastUseTime = 0;
            Expect(character, skill, () => Execute(command, Text(Handle(character)), Number(0)),
                command.GetName() + " reads the currently equipped slot on first use");
            Check(skill.lastUseTime == 100f, "successful slot use records current progress time");
            NoUse(() => Execute(command, Text(Handle(character)), Number(0)), "CD-enabled repeat is blocked");
            Expect(character, skill, () => Execute(command, Text(Handle(character)), Number(1)),
                "ignoreCd=1 bypasses the cooldown");
            NoUse(() => Execute(command, Text(Handle(character)), Number(2)), "only numeric one bypasses cooldown");
            skill.lastUseTime = 90f;
            NoUse(() => Execute(command, Text(Handle(character)), Number(0)), "exact cooldown expiry keeps existing strict comparison");
            progress.seconds = 100.01f;
            Expect(character, skill, () => Execute(command, Text(Handle(character)), Number(0)),
                "elapsed cooldown permits use and emits the owning Character/Skill pair");
            Check(skill.lastUseTime == progress.seconds, "cooldown success writes the actual progress clock");

            var replacement = Equip(character, slots[i]);
            replacement.lastUseTime = 0;
            float previousUse = skill.lastUseTime;
            Expect(character, replacement, () => Execute(command, Text(Handle(character)), Number(0)),
                "equipping a different skill changes the command target without editing event source");
            Check(skill.lastUseTime == previousUse, "replaced skill cooldown is not modified");
            replacement.lastUseTime = 100;
            NoUse(() => Run(command.GetName() + "(\"" + Handle(character) + "\");"),
                "omitted cooldown parameter defaults to checking CD");
            progress.enableSkill = false;
            NoUse(() => Execute(command, Text(Handle(character)), Number(1)),
                "ignoreCd never bypasses the global skill-enable switch");
            progress.enableSkill = true;

            replacement.lastUseTime = 0;
            progress.blockProgramUid = 123;
            play.sceneCtrl.SetCurOptSkill(SkillType.E);
            Check(!info.CanUseSkill(character.uid, slots[i]), "UI CanUseSkill has additional input/block guards");
            Expect(character, replacement, () => Execute(command, Text(Handle(character)), Number(0)),
                "scripted slot use keeps CD-only semantics without applying UI capture/block guards");
            Reset();
        }
    }

    private static void SpecificSkillIgnoresCooldown()
    {
        Reset();
        var character = Character(480002, 490002);
        var equipped = Equip(character, SkillType.LightAttack);
        var specified = Skill(character.uid);
        specified.lastUseTime = 100;
        equipped.lastUseTime = 88;
        Expect(character, specified, () => Execute(new UseSkillCmd(), Text(Handle(character)), Text(SkillHandle(specified))),
            "two-argument UseSkill uses the specified skill even during its cooldown");
        Check(equipped.lastUseTime == 88, "specific skill use does not switch to an equipped/current slot");
        Expect(character, specified, () => Run("UseSkill(\"" + Handle(character) + "\",\"" + SkillHandle(specified) + "\");"),
            "compiled two-argument UseSkill also bypasses cooldown");
        var skillTemplate = Skill(0);
        var equippedCopy = Equip(character, SkillType.E);
        equippedCopy.protoUid = skillTemplate.uid;
        equippedCopy.lastUseTime = 100;
        Expect(character, equippedCopy, () => Execute(new UseSkillCmd(), Text(Handle(character)), Text(SkillHandle(skillTemplate))),
            "a specified Skill prototype resolves to this Character's currently equipped runtime copy");
        Check(skillTemplate.lastUseTime == 0, "specified prototype defaults do not acquire runtime cooldown state");
        var legacyHeavy = Equip(character, SkillType.HeavyAttack);
        legacyHeavy.lastUseTime = 100;
        Expect(character, legacyHeavy, () => Execute(new UseSkillCmd(), Text(Handle(character)), Text("$sk$-1$sk$")),
            "legacy negative HeavyAttack slot references remain supported without a cooldown argument");
        progress.enableSkill = false;
        NoUse(() => Execute(new UseSkillCmd(), Text(Handle(character)), Text(SkillHandle(specified))),
            "specific skill use still respects global skill enablement");
        Reset();
    }

    private static void PrototypeTargetsAndReplacement()
    {
        Reset();
        var prototype = Character(480010, 0);
        var a = Character(480011, prototype.uid);
        var b = Character(480012, prototype.uid);
        var templateSkill = Equip(prototype, SkillType.LightAttack);
        var aSkill = Equip(a, SkillType.LightAttack);
        var bSkill = Equip(b, SkillType.LightAttack);
        probe.events.Clear();
        Execute(new UseCharacterLightAttackCmd(), Text(Handle(prototype)), Number(0));
        Check(probe.events.Count == 2 && probe.events[0] == (a.uid, aSkill.uid) && probe.events[1] == (b.uid, bSkill.uid),
            "prototype command fans out runtime copies, each using its independent equipped slot");
        Check(templateSkill.lastUseTime == 0 && aSkill.lastUseTime == 100 && bSkill.lastUseTime == 100,
            "prototype defaults are not mutated and runtime cooldowns remain independent");
        var explicitSkill = Skill(a.uid);
        probe.events.Clear();
        Execute(new UseSkillCmd(), Text(Handle(prototype)), Text(SkillHandle(explicitSkill)));
        Check(probe.events.SequenceEqual(new[] { (a.uid, explicitSkill.uid), (b.uid, explicitSkill.uid) }),
            "specific skill fan-out is not blocked by the first copy setting its shared cooldown");
        var singleton = Character(480013, 0, true);
        var singletonSkill = Equip(singleton, SkillType.Q);
        Expect(singleton, singletonSkill, () => Execute(new UseCharacterQSkillCmd(), Text(Handle(singleton)), Number(0)),
            "authored unique Character references retain singleton targeting");
        var unusedPrototype = Character(480014, 0);
        Equip(unusedPrototype, SkillType.LightAttack);
        NoUse(() => Execute(new UseCharacterLightAttackCmd(), Text(Handle(unusedPrototype)), Number(1)),
            "a non-unique authored prototype with no runtime copy is not implicitly instantiated or modified");

        probe.events.Clear();
        probe.onUse = evt =>
        {
            if (evt.data != a) return;
            CharacterProductForm.RemoveData(b.uid);
            var replacement = Character(b.uid, prototype.uid);
            Equip(replacement, SkillType.LightAttack);
        };
        try { Execute(new UseCharacterLightAttackCmd(), Text(Handle(prototype)), Number(1)); }
        finally { probe.onUse = null; }
        Check(probe.events.Count == 1 && probe.events[0] == (a.uid, aSkill.uid),
            "reentrant Character UID reuse cannot redirect a captured fan-out target to replacement data");
    }

    private static void InvalidTargetsAndReadiness()
    {
        Reset();
        var character = Character(480020, 490020);
        var skill = Skill(character.uid);
        foreach (var command in slotCommands)
        {
            NoUse(() => Execute(command, Text(Handle(character)), Number(1)), "an empty equipped slot is a no-op");
            var slot = slots[Array.IndexOf(slotCommands, command)];
            character.skill[slot] = 999999;
            NoUse(() => Execute(command, Text(Handle(character)), Number(1)), "a deleted equipped skill is a no-op");
            character.skill[slot] = character.uid;
            NoUse(() => Execute(command, Text(Handle(character)), Number(1)), "a non-Skill Product UID cannot be used as a skill");
            character.skill = null;
            NoUse(() => Execute(command, Text(Handle(character)), Number(1)), "a missing slot dictionary is safe");
            character.skill = new Dictionary<SkillType, int>();
            foreach (var reference in new[] { "", "character", "$ch$$ch$", "$ch$x$ch$", "$ch$0$ch$", "$ch$999999$ch$", "$so$480020$so$" })
                NoUse(() => Execute(command, Text(reference), Number(1)), "invalid Character reference is a no-op: " + reference);
            NoUse(() => Run(command.GetName() + "();"), "fully omitted slot-command arguments no-op safely");
        }
        character.skill[SkillType.E] = skill.uid;
        foreach (var invalid in new[] { "", "skill", "$sk$$sk$", "$sk$0$sk$", "$sk$x$sk$", "$sk$999999$sk$" })
            NoUse(() => Execute(new UseSkillCmd(), Text(Handle(character)), Text(invalid)), "invalid explicit skill is a no-op: " + invalid);
        play.infoCtrl = null;
        NoUse(() => Execute(new UseCharacterESkillCmd(), Text(Handle(character)), Number(1)), "missing Play controller is safe");
        NoUse(() => Execute(new UseSkillCmd(), Text(Handle(character)), Text(SkillHandle(skill))), "specific use with missing controller is safe");
        play.infoCtrl = info;
        ProgressForm.RemoveData(progress.uid);
        NoUse(() => Execute(new UseCharacterESkillCmd(), Text(Handle(character)), Number(1)), "unloaded progress does not use retained skills");
        NoUse(() => Execute(new UseSkillCmd(), Text(Handle(character)), Text(SkillHandle(skill))), "specific use without progress is safe");
        ProgressForm.AddData(progress);
    }

    private static void LegacySignatureMigration()
    {
        string source = "UseSkill(ch,GetSkill(1,2),CheckCd());";
        string expected = "UseSkill(ch,GetSkill(1,2));";
        Check(SkillCommandMigration.MigrateCode(source) == expected,
            "legacy third parameter is removed without confusing nested argument commas");
        Check(SkillCommandMigration.MigrateCode(expected) == expected,
            "already migrated two-argument source is idempotent");
        source = "Return \"UseSkill(ch,skill,1);\";";
        Check(SkillCommandMigration.MigrateCode(source) == source,
            "quoted source-like text is not treated as a command call");
        source = "UseSkill(a,b);UseSkill(a,b,1);NotUseSkill(a,b,1);";
        Check(SkillCommandMigration.MigrateCode(source) == "UseSkill(a,b);UseSkill(a,b);NotUseSkill(a,b,1);",
            "only actual three-argument calls of the retired exact command name change");
        source = "UseSkill(a,b,c,d);useskill(a,b,c);";
        Check(SkillCommandMigration.MigrateCode(source) == source,
            "invalid different-arity and differently cased methods remain unchanged");
        foreach (var pair in new[]
        {
            ("UseSkill(\"(\",\"[,)]\",0);", "UseSkill(\"(\",\"[,)]\");"),
            ("UseSkill(a,b,);", "UseSkill(a,b);"),
            ("UseSkill(a,b,   );", "UseSkill(a,b);"),
            ("UseSkill(a,b,(1));", "UseSkill(a,b);"),
            ("UseSkill(a,b,UseSkill(c,d,e));", "UseSkill(a,b);"),
            ("UseSkill(UseSkill(a,b,c),s,d);", "UseSkill(UseSkill(a,b),s);"),
            ("UseSkill(a,UseSkill(b,c,d),e);", "UseSkill(a,UseSkill(b,c));"),
            ("UseSkill(a,GetSkill(Map[1]),(CheckCd(3,4)));", "UseSkill(a,GetSkill(Map[1]));")
        })
            Check(SkillCommandMigration.MigrateCode(pair.Item1) == pair.Item2,
                "migration preserves grouping/quoted punctuation/empty slots/nested calls: " + pair.Item1);

        // Write only under the already-checked disposable project, never under
        // Application.persistentDataPath or any real story's Core/Save directory.
        Reset();
        var character = Character(480040, 490040);
        var skill = Skill(character.uid);
        skill.lastUseTime = 100;
        string oldCode = "UseSkill(\"" + Handle(character) + "\",\"" + SkillHandle(skill) + "\",0);Return 7;";
        string newCode = SkillCommandMigration.MigrateCode(oldCode);
        Check(newCode != oldCode && Compile(newCode).code == newCode, "migrated saved source compiles with the new signature");
        var oldProgram = new EventProgramDataForm.Data(495001, "old-use-skill-signature", oldCode,
            new List<string> { "deliberately obsolete bytecode" }, new List<int> { 0 }, 0, "num", 0);
        var row = EventProgramDataForm.GetJoByData(oldProgram);
        row["sorv"] = 2;
        string folder = Path.Combine(Path.GetDirectoryName(Application.dataPath), "SkillSignatureMigrationTest") + "/";
        var save = new GameSaveController(GameManager.instance);
        string path = folder + save.eventFormFileName;
        SaveAndLoad.Save(path, new JArray(row).ToString());
        string savedBytes = File.ReadAllText(path);
        save.LoadEvent(folder);
        var loaded = EventProgramDataForm.DataByUid[oldProgram.uid];
        Check(loaded.code == newCode && loaded.zCode.Count > 1
            && !loaded.zCode.Contains("deliberately obsolete bytecode") && loaded.zCode.Count == loaded.zCodeMap.Count,
            "actual LoadEvent migrates old calls despite cached bytecode and recompiles aligned source maps");
        Check(File.ReadAllText(path) == savedBytes, "loading migration does not rewrite the saved event source file");
        Expect(character, skill, () =>
        {
            var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
                loaded, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
            var result = execution.Interpret();
            Check(result.complete && result.errors.Count == 0 && result.ret.num == 7,
                "migrated program executes the explicit skill and continues after it");
        }, "migrated UseSkill source bypasses CD consistently with newly authored two-argument calls");
    }

    private static CharacterProductForm.Data Character(int uid, int prototype, bool unique = false)
    {
        var data = CharacterProductForm.defaultData;
        data.uid = uid; data.name = "skill-command-character-" + uid; data.protoUid = prototype; data.unique = unique;
        data.paramDic = new Dictionary<string, CharacterParamForm.Data>();
        data.skill = new Dictionary<SkillType, int>();
        CharacterProductForm.AddData(data);
        return data;
    }
    private static SkillProductForm.Data Skill(int character)
    {
        int uid = nextUid++;
        var data = new SkillProductForm.Data(uid, "command-skill-" + uid, "", 0, 0, 0,
            new Dictionary<string, SkillParamForm.Data>(), new List<SkillType>(), character, 10f, 0f,
            new Dictionary<string, EventTriggerForm.Data>(), 0);
        SkillProductForm.AddData(data);
        return data;
    }
    private static SkillProductForm.Data Equip(CharacterProductForm.Data character, SkillType slot)
    {
        var skill = Skill(character.uid);
        character.skill[slot] = skill.uid;
        return skill;
    }
    private static void Reset()
    {
        progress.seconds = 100f; progress.enableSkill = true; progress.blockProgramUid = 0;
        play.sceneCtrl.SetCurOptSkill(null);
        probe.events.Clear(); probe.onUse = null;
    }
    private static void Expect(CharacterProductForm.Data character, SkillProductForm.Data skill, Action action, string message)
    {
        int before = probe.events.Count;
        action();
        Check(probe.events.Count == before + 1 && probe.events[before] == (character.uid, skill.uid), message);
    }
    private static void NoUse(Action action, string message)
    {
        int before = probe.events.Count;
        var cooldowns = SkillProductForm.DataByUid.Values.ToDictionary(row => row.uid, row => row.lastUseTime);
        action();
        Check(probe.events.Count == before && cooldowns.All(row => SkillProductForm.DataByUid[row.Key].lastUseTime == row.Value), message);
    }
    private static void Execute(CmdBase command, params BoxDataForm.Data[] arguments)
    {
        var task = new InterpretAsyncTask(null);
        command.Execute(arguments.Reverse().ToArray(), new Dictionary<string, BoxDataForm.Data>(), task);
        Check(task.IsComplete() && task.error == null, command.GetName() + " completes synchronously without error: " + task.error);
    }
    private static ProgramDataForm.Data Compile(string source)
    {
        var data = ProgramDataForm.defaultData;
        data.name = "skill-command-regression";
        Check(data.TryApplyCode(source, out _, out _), "source compiles: " + source);
        return data;
    }
    private static void Run(string source)
    {
        var execution = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            Compile(source), 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = execution.Interpret();
        Check(result.complete && result.errors.Count == 0 && execution.stack.Count == 0,
            "compiled skill command keeps the interpreter stack balanced: " + string.Join(";", result.errors));
    }
    private static string Handle(CharacterProductForm.Data data) => "$ch$" + data.uid + "$ch$";
    private static string SkillHandle(SkillProductForm.Data data) => "$sk$" + data.uid + "$sk$";
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static void Check(bool value, string message)
    {
        if (!value) throw new InvalidOperationException(message);
        checks++;
    }
    private sealed class Probe : IZ_Listener<CharacterSkillEvent>
    {
        public readonly List<(int character, int skill)> events = new List<(int, int)>();
        public Action<CharacterSkillEvent> onUse;
        public void OnEvent(CharacterSkillEvent evt)
        {
            if (evt.type != CharacterSkillEventType.Use) return;
            events.Add((evt.data.uid, evt.skillUid));
            onUse?.Invoke(evt);
        }
    }
}
