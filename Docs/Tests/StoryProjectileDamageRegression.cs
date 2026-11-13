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

// Executes the migrated authored programs with deterministic scene command probes.
// Only the runner's copied ef files are loaded; no live story or save is changed.
public static class StoryProjectileDamageRegression
{
    private const string Shooter = "$ch$1$ch$", Enemy = "$ch$2$ch$", Ally = "$ch$3$ch$",
        Neutral = "$ch$4$ch$", Wall = "$so$999$so$";
    private static readonly Dictionary<string, Dictionary<string, BoxDataForm.Data>> objects =
        new Dictionary<string, Dictionary<string, BoxDataForm.Data>>();
    private static readonly Dictionary<string, float> teams = new Dictionary<string, float>();
    private static readonly List<string> destroyed = new List<string>();
    private static readonly List<string> rotated = new List<string>();
    private static int checks, generated, effects, audio, stuns, hpWrites;
    private static float attack = 12, defense = 3, hp = 100;
    private static bool shooterExists = true, collidable = true;

    public static void Run()
    {
        try
        {
            foreach (var form in new[] { typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(EventProgramDataForm) })
                form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            foreach (var type in typeof(CharacterCmd).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(CmdBase))))
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length > 0)
                    method.Invoke(null, null);
            InstallProbes();
            var files = Directory.GetFiles(Path.Combine(Path.GetDirectoryName(Application.dataPath), "ProjectileEvents"), "*.json");
            Check(files.Length > 0, "The fixture has explicit isolated event copies");
            foreach (var file in files) CheckStory(file);
            Debug.Log("STORY_PROJECTILE_DAMAGE_REGRESSION_PASS checks=" + checks + " stories=" + files.Length);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
    }

    private static void InstallProbes()
    {
        Probe("朝向敌人", p => null, "StopCharacterNavigate");
        Probe("弹图", p => null, "ShowTip");
        foreach (string name in new[] { "CharacterPlayAnim", "ShowEffectByCharacter", "CharacterChat" })
            Probe(name, p => null);
        Probe("GetCharacterParameter", p =>
        {
            Check(p[0].str != Shooter || shooterExists, "A projectile never dereferences its removed firing Character");
            switch (p[1].str)
            {
                case "Team": return Num(teams[p[0].str]);
                case "攻击": Check(p[0].str == Shooter, "Only firing reads the Character's attack"); return Num(attack);
                case "防御": return Num(defense);
                case "Hp": return Num(hp);
                default: throw new InvalidOperationException("Unexpected Character parameter " + p[1].str);
            }
        });
        Probe("SetCharacterParameter", p => { Check(p[1].str == "Hp", "Damage only changes HP"); hp = p[2].num; hpWrites++; return null; });
        Probe("GenerateObject", p =>
        {
            string id = "$so$" + (++generated + 900000) + "$so$";
            objects[id] = new Dictionary<string, BoxDataForm.Data>();
            return Text(id);
        });
        Probe("SetSceneObjectParameter", p => { objects[p[0].str][p[1].str] = p[2].DeepCopy(); return null; });
        Probe("GetSceneObjectParameter", p => objects[p[0].str][p[1].str].DeepCopy());
        foreach (string name in new[] { "GetCharacterPosition", "GetObjectPosition" })
            Probe(name, p => Vector(0, 0, 0), "GetPosition");
        foreach (string name in new[] { "GetCharacterForwardVector", "GetCharacterRightVector", "GetObjectForwardVector" })
            Probe(name, p => Vector(1, 0, 0));
        Probe("GetCharacterRotation", p => Num(0));
        Probe("GetLookAtRotation", p => Num(0));
        Probe("SetObjectRotation", p => { rotated.Add(p[0].str); return null; }, "SetRotation");
        Probe("MoveObjectRelative", p => null, "MoveRelative");
        Probe("GetNormalizedVector", p => p[0].DeepCopy());
        Probe("GetCharactersInArea", p =>
        {
            var value = CodeHelper.CreateBox(); value.dic["0"] = Text(Ally); value.dic["1"] = Text(Enemy); return value;
        });
        Probe("Len", p => Num(p[0].dic.Count));
        Probe("IsObjectEnableCollision", p => Num(collidable ? 1 : 0));
        Probe("ShowEffect", p => { effects++; return null; });
        Probe("PlayAudio", p => { audio++; return null; });
        Probe("CharacterStun", p => { stuns++; return null; });
        Probe("DestroyObject", p => { destroyed.Add(p[0].str); return null; }, "Destroy");
        Probe("景物毁坏", p => { destroyed.Add(p[0].str); return null; }, "Destroy");
    }

    private static void CheckStory(string file)
    {
        EventProgramDataForm.Clear();
        var rows = JArray.Parse(File.ReadAllText(file)).OfType<JObject>().ToArray();
        var programs = rows.Select(EventProgramDataForm.GetDataByJo).ToArray();
        foreach (var program in programs) EventProgramDataForm.AddData(program);
        var changed = programs.Where(program => program.code.Contains("\"所属Team\"")
            || (program.name == "子弹碰撞销毁" && program.code.Contains("\"伤害\""))).ToArray();
        Check(changed.Length >= 11, "All shot and behavior programs are migrated in " + Path.GetFileName(file));
        Check(programs.All(program => !program.code.Contains("\"所属人物\"")), "No authored event still depends on a firing Character reference");
        foreach (var program in changed)
        {
            Check(program.TryApplyCode(program.code, out _, out var errors),
                "The real compiler accepts " + program.name + ": " + string.Join(";", errors));
            Check(program.zCode.Count > 0 && program.zCode.Count == program.zCodeMap.Count,
                "Recompiled bytecode/source-map lengths match for " + program.name);
        }
        foreach (var program in changed.Where(program => program.code.Contains("SetSceneObjectParameter(newObject")))
        {
            Reset();
            Execute(program, Shooter);
            Check(objects.Count > 0, program.name + " still generates projectiles");
            foreach (var bullet in objects.Values)
                Check(!bullet.ContainsKey("所属人物") && bullet["所属Team"].str == null
                    && bullet["所属Team"].num == 1 && bullet["伤害"].num == 12,
                    program.name + " snapshots numeric Team and attack into every generated projectile");
        }

        var hit = changed.Single(program => program.name == "子弹打人");
        Reset(); string shot = Bullet(); attack = 1000; teams[Shooter] = 0; shooterExists = false;
        Execute(hit, shot, Enemy);
        Check(hp == 91 && hpWrites == 1 && destroyed.SequenceEqual(new[] { shot }) && effects == 1 && audio == 1,
            "Enemy damage uses captured 12 minus defense 3, even after shooter removal/attack/Team changes");

        foreach (string target in new[] { Shooter, Ally })
        {
            Reset(); shot = Bullet(); Execute(hit, shot, target);
            Check(hp == 100 && hpWrites == 0 && destroyed.Count == 0 && effects == 0 && audio == 0 && stuns == 0,
                "Same-Team contacts, including the firing Character, pass through without any hit side effect");
        }
        Reset(); shot = Bullet(); defense = 20; Execute(hit, shot, Enemy);
        Check(hp == 100 && destroyed.Contains(shot), "Defense above stored damage clamps damage to zero, without healing");
        Reset(); shot = Bullet(); Execute(hit, shot, Neutral);
        Check(hp == 100 && hpWrites == 0 && stuns == 0, "The existing neutral-Team rule is retained");

        var collision = changed.Single(program => program.name == "子弹碰撞销毁");
        foreach (float wallHp in new[] { 30f, 5f })
        {
            Reset(); shot = Bullet(); shooterExists = false;
            objects[Wall] = new Dictionary<string, BoxDataForm.Data> { ["可破坏"] = Num(wallHp) };
            Execute(collision, shot, Wall);
            Check(objects[Wall]["可破坏"].num == wallHp - 12 && destroyed.Contains(shot),
                "Destructible obstacles also use the projectile's captured damage");
            Check(destroyed.Contains(Wall) == (wallHp <= 12), "Obstacle destruction keeps its existing HP boundary");
        }
        Reset(); shot = Bullet(); collidable = false; Execute(collision, shot, Wall);
        Check(destroyed.Count == 0 && effects == 0, "A non-collidable object does not consume the projectile");
        Reset(); shot = Bullet(); shooterExists = false;
        Execute(changed.Single(program => program.name == "子弹追踪"), shot);
        Check(rotated.Count == 19 && destroyed.Contains(shot), "Homing uses captured Team without consulting the removed shooter");
    }

    private static void Reset()
    {
        objects.Clear(); destroyed.Clear(); rotated.Clear(); teams.Clear();
        teams[Shooter] = teams[Ally] = 1; teams[Enemy] = 0; teams[Neutral] = 2;
        attack = 12; defense = 3; hp = 100; generated = effects = audio = stuns = hpWrites = 0;
        shooterExists = collidable = true;
    }

    private static string Bullet()
    {
        const string id = "$so$900001$so$";
        objects[id] = new Dictionary<string, BoxDataForm.Data>
        { ["所属Team"] = Num(1), ["伤害"] = Num(12), ["移动方向"] = Vector(1, 0, 0) };
        return id;
    }

    private static void Execute(ProgramDataForm.Data program, string actor, string target = null)
    {
        var heap = new Dictionary<string, BoxDataForm.Data> { ["param1"] = Text(actor) };
        if (target != null) heap["param2"] = Text(target);
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), heap, program,
            0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        for (int poll = 0; poll < 100; poll++)
        {
            var result = data.Interpret();
            Check(result.errors.Count == 0, program.name + " executes without errors: " + string.Join(";", result.errors));
            if (result.complete) return;
            Check(data.program.zCode[data.p] == ((int)Op.Wait).ToString() && data.stack.Count > 0,
                "Only authored literal Wait pauses this deterministic fixture");
            data.stack[data.stack.Count - 1].num = 0;
        }
        throw new InvalidOperationException(program.name + " did not finish");
    }

    private static void Probe(string name, Func<BoxDataForm.Data[], BoxDataForm.Data> execute, string metadata = null) =>
        BaseData.cmdDic[name] = new ProbeCommand(metadata ?? name, execute);
    private sealed class ProbeCommand : CmdBase
    {
        private readonly string name;
        private readonly Func<BoxDataForm.Data[], BoxDataForm.Data> execute;
        public ProbeCommand(string name, Func<BoxDataForm.Data[], BoxDataForm.Data> execute) { this.name = name; this.execute = execute; }
        public override string GetName() => name;
        public override CmdBase GetNew() => new ProbeCommand(name, execute);
        protected override bool ExecuteInternal(BoxDataForm.Data[] parameters, InterpretAsyncTask task)
        {
            var value = execute(parameters); task.res = value == null ? Array.Empty<BoxDataForm.Data>() : new[] { value }; return true;
        }
    }
    private static BoxDataForm.Data Num(float value) => CodeHelper.CreateBoxByNum(value);
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Vector(float x, float y, float height)
    {
        var value = CodeHelper.CreateBox(); value.dic["x"] = Num(x); value.dic["y"] = Num(y); value.dic["height"] = Num(height); return value;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
