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

// Runs the authored Story 1 program through the real compiler/interpreter.
// Selection commands use deterministic probes; one real low-wall cast checks height semantics.
public static class StoryTargetSelectionRegression
{
    private const string Self = "$ch$1$ch$", Far = "$ch$2$ch$", Near = "$ch$3$ch$",
        Blocked = "$ch$4$ch$", Friend = "$ch$5$ch$";
    private static readonly Dictionary<string, Vector3> Positions = new Dictionary<string, Vector3>
    {
        [Self] = new Vector3(500, 2, 500), [Far] = new Vector3(506, 2, 500),
        [Near] = new Vector3(500, 2, 502), [Blocked] = new Vector3(501, 2, 500),
        [Friend] = new Vector3(500, 2, 500.5f)
    };
    private static string[] candidates;
    private static readonly HashSet<string> occluded = new HashSet<string>();
    private static string currentCandidate, lookTarget, selected;
    private static int checks, rotations;
    private static readonly int[] ChangedPrograms = { 5, 6, 7, 19, 20, 23, 28 };
    private static readonly Vector3 Forward = new Vector3(.6f, 0, .8f);
    private static readonly Dictionary<string, Vector3> ProjectilePositions = new Dictionary<string, Vector3>();
    private static readonly Dictionary<string, Dictionary<string, BoxDataForm.Data>> ProjectileParameters =
        new Dictionary<string, Dictionary<string, BoxDataForm.Data>>();
    private static readonly List<Vector3> WrittenDirections = new List<Vector3>();
    private static readonly List<Vector3> Moves = new List<Vector3>();
    private static bool remoteCase, homingCase;
    private static int projectileId;

    public static void Run()
    {
        try
        {
            foreach (var form in new[] { typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(EventProgramDataForm) })
                form.GetMethod("Register", BindingFlags.Static | BindingFlags.NonPublic).Invoke(null, null);
            // EditMode does not run Player initialization hooks automatically.
            // Register only command hooks; production Manager/Form lifetimes stay untouched.
            foreach (var type in typeof(CharacterCmd).Assembly.GetTypes().Where(type => type.IsSubclassOf(typeof(CmdBase))))
            foreach (var method in type.GetMethods(BindingFlags.Static | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly))
                if (method.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length > 0)
                    method.Invoke(null, null);
            Probe("GetPosition", p => Vector(Position(p[0].str)));
            Probe("GetCharacterPosition", p => Vector(Position(p[0].str)), "GetPosition");
            Probe("GetObjectPosition", p => Vector(Position(p[0].str)), "GetPosition");
            Probe("GetCharactersInArea", p =>
            {
                if (homingCase)
                    Check(p[1].num == 3, "homing query retains its three-tile radius");
                else
                {
                    Equal(Read(p[0]), Positions[Self], "query center keeps the Character's raw height");
                    Check(p[1].num == 10 || (remoteCase && p[1].num == 6), "query radius is the selector or remote attack range");
                }
                var list = CodeHelper.CreateBox();
                for (int i = 0; i < candidates.Length; i++) list.dic[i.ToString()] = Text(candidates[i]);
                return list;
            });
            Probe("Len", p => Number(p[0].dic.Count));
            Probe("GetCharacterParameter", p =>
            {
                if (p[1].str == "射程") return Number(6);
                Check(p[1].str == "Team", "authored hostility program reads the Team parameter");
                if (p[0].str != Self) currentCandidate = p[0].str;
                return Number(p[0].str == Self || p[0].str == Friend ? 0 : 1);
            });
            Probe("IsUnobstructed", p =>
            {
                bool remoteRay = remoteCase && Mathf.Abs(p[2].num - .3f) < .00001f;
                Equal(Read(p[remoteRay ? 1 : 0]), Positions[Self] + Vector3.up * .5f,
                    "caller explicitly raises the LOS origin");
                Equal(Read(p[remoteRay ? 0 : 1]), Positions[currentCandidate] + Vector3.up * .5f,
                    "caller explicitly raises the current candidate's LOS endpoint");
                Check(remoteRay || Mathf.Abs(p[2].num - .2f) < .00001f, "LOS preserves authored sweep radius");
                return Number(occluded.Contains(currentCandidate) ? 0 : 1);
            });
            Probe("GetVectorLength", p => Number(Read(p[0]).magnitude));
            Probe("GetNormalizedVector", p => Vector(Read(p[0]).normalized));
            Probe("GetCharacterForwardVector", p => Vector(Forward));
            Probe("GetCharacterRightVector", p => Vector(Vector3.right));
            Probe("GetCharacterRotation", p => Number(0));
            Probe("GetLookAtRotation", p =>
            {
                if (!homingCase) Equal(Read(p[0]), Positions[Self], "rotation starts from the current actor");
                lookTarget = Positions.Single(pair => pair.Value == Read(p[1])).Key;
                var delta = Read(p[1]) - Read(p[0]);
                return Number(Mathf.Repeat(Mathf.Atan2(delta.x, delta.z) * Mathf.Rad2Deg, 360));
            });
            Probe("SetRotation", p =>
            {
                Check(p[0].str == Self && p[2].num == 0, "only the actor is rotated, without transition");
                selected = lookTarget;
                rotations++;
                return null;
            });
            Probe("SetCharacterRotation", p =>
            {
                Check(remoteCase && p[0].str == Self && Mathf.Abs(p[2].num - .1f) < .00001f,
                    "enemy ranged attack retains its timed visual turn before the shot");
                return null;
            }, "SetRotation");
            Probe("CharacterPlayAnim", p => null);
            Probe("ShowEffectByCharacter", p => null);
            Probe("ShowTip", p => null);
            Probe("GenerateObject", p =>
            {
                string reference = "$so$p:" + (++projectileId + 990000) + "$so$";
                ProjectilePositions[reference] = Read(p[1]);
                ProjectileParameters[reference] = new Dictionary<string, BoxDataForm.Data>();
                return Text(reference);
            });
            Probe("SetSceneObjectParameter", p =>
            {
                ProjectileParameters[p[0].str][p[1].str] = p[2].DeepCopy();
                if (p[1].str == "移动方向") WrittenDirections.Add(Read(p[2]));
                return null;
            });
            Probe("GetSceneObjectParameter", p => ProjectileParameters[p[0].str][p[1].str].DeepCopy());
            Probe("SetObjectRotation", p => null, "SetRotation");
            Probe("MoveObjectRelative", p =>
            {
                var delta = Read(p[1]);
                Check(Mathf.Abs(p[2].num - (homingCase ? .3f : 10f)) < .00001f,
                    "projectile movement preserves the existing duration");
                Moves.Add(delta);
                ProjectilePositions[p[0].str] += delta;
                return null;
            }, "MoveRelative");
            Probe("DestroyObject", p => null);

            string projectRoot = Path.GetDirectoryName(Application.dataPath);
            var rows = JArray.Parse(File.ReadAllText(Path.Combine(projectRoot, "ExtraAssets/1/Core/ef")))
                .OfType<JObject>().ToArray();
            var programs = rows.Select(EventProgramDataForm.GetDataByJo).ToArray();
            programs.Single(data => data.uid == 28).returnValue = "vector";
            foreach (var data in programs) EventProgramDataForm.AddData(data);
            foreach (var data in programs.OrderBy(data => data.uid == 28 ? 0 : 1))
            {
                Check(data.TryApplyCode(data.code, out _, out var errors),
                    "authored Story 1 program compiles: " + data.name + ": " + string.Join("; ", errors));
                Check(data.zCode.Count > 0 && data.zCode.Count == data.zCodeMap.Count,
                    "compiled event and source map remain aligned: " + data.name);
            }
            // The image helper is unrelated to trajectory routing. Override it only
            // after compilation so stored custom-program call metadata stays real.
            Probe("弹图", p => null, "ShowTip");
            var program = programs.Single(data => data.uid == 28 && data.name == "朝向敌人");
            Check(program.returnValue == "vector", "target selector publishes vector return metadata before caller compilation");

            RunCase(program, new[] { Self, Far, Near }, Array.Empty<string>(), Near, "self-first unsorted candidates");
            RunCase(program, new[] { Friend, Blocked, Far, Near, Self }, new[] { Blocked }, Near,
                "nearest blocked enemy and closer friendly are skipped");
            RunCase(program, new[] { Self, Near, Far, Blocked, Friend }, new[] { Blocked }, Near,
                "candidate-order reversal does not change the nearest visible target");
            RunCase(program, new[] { Self, Near, Far }, new[] { Near }, Far,
                "blocked nearest enemy falls back to farther visible enemy");
            RunCase(program, new[] { Self, Near, Far, Friend }, new[] { Near, Far }, null,
                "all blocked enemies preserve the existing facing");
            RunCase(program, new[] { Self, Friend }, Array.Empty<string>(), null, "friendly-only query preserves facing");
            RunCase(program, Array.Empty<string>(), Array.Empty<string>(), null, "empty query preserves facing");
            var nearPosition = Positions[Near];
            Positions[Near] = nearPosition + Vector3.up * 3;
            RunCase(program, new[] { Self, Near }, Array.Empty<string>(), Near, "higher target returns an upward vector");
            CheckShotPrograms(programs, (Positions[Near] - Positions[Self]).normalized);
            Positions[Near] = nearPosition - Vector3.up * 2;
            RunCase(program, new[] { Self, Near }, Array.Empty<string>(), Near, "lower target returns a downward vector");
            CheckShotPrograms(programs, (Positions[Near] - Positions[Self]).normalized);
            Positions[Near] = nearPosition;
            CheckProjectileMovement(programs);
            CheckExactCoordinateRaycast();

            File.WriteAllText(Path.Combine(projectRoot, "compiled-facing-event.json"),
                EventProgramDataForm.GetJoByData(program).ToString());
            File.WriteAllText(Path.Combine(projectRoot, "compiled-los-events.json"),
                new JArray(programs.Where(data => new[] { 14, 20, 28 }.Contains(data.uid)).OrderBy(data => data.uid)
                    .Select(EventProgramDataForm.GetJoByData)).ToString());
            File.WriteAllText(Path.Combine(projectRoot, "compiled-bullet-events.json"),
                new JArray(programs.Where(data => ChangedPrograms.Contains(data.uid)).OrderBy(data => data.uid)
                    .Select(EventProgramDataForm.GetJoByData)).ToString());
            Debug.Log("STORY_TARGET_SELECTION_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception exception)
        {
            Debug.LogException(exception);
            EditorApplication.Exit(1);
        }
    }

    private static void CheckExactCoordinateRaycast()
    {
        var map = CreateManager<MapManager>();
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(5, 2, 3),
                Vector3Int.one, "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        map.utilCtrl = new MapUtilController(map);
        map.updateCtrl = new MapUpdateController(map);
        var pool = CreateManager<InstancePoolManager>();
        pool.Init();
        pool.defaultRoot = pool.transform;
        var prefab = new GameObject("target-selection-low-wall");
        var collider = prefab.AddComponent<BoxCollider>();
        collider.size = new Vector3(.5f, .4f, 1f);
        collider.center = new Vector3(0, .2f, 0);
        pool.AddPool(prefab);
        UnitForm.InitInternal();
        TileUnitForm.InitInternal();
        map.data.RegisterMap(new TileUnitForm.Data(990001, "", new Dictionary<int, int>(),
            new Vector3Int(2, 0, 0), prefab.name, new Vector3(2, 0, 0), Vector3.zero, Vector3.one,
            UpdateType.ShowOnly, new List<int>(), "", true, false, new List<int>()));
        var execute = typeof(IsUnobstructedCmd).GetMethod("ExecuteInternal", BindingFlags.Instance | BindingFlags.NonPublic);
        float Cast(float height)
        {
            var task = new InterpretAsyncTask(null);
            var from = GameManager.MapPosToPlayerPos(new Vector3(0, height, 0));
            var to = GameManager.MapPosToPlayerPos(new Vector3(4, height, 0));
            execute.Invoke(new IsUnobstructedCmd(), new object[]
                { new[] { Vector(from), Vector(to), Number(.05f) }, task });
            return task.res[0].num;
        }
        Check(Cast(.1f) == 0, "real IsUnobstructed uses exact low endpoints and hits the low wall");
        Check(Cast(.6f) == 1, "real IsUnobstructed passes the wall only when the caller raises endpoints");
    }

    private static void RunCase(ProgramDataForm.Data program, string[] query, string[] blocked, string expected, string context)
    {
        candidates = query;
        occluded.Clear();
        foreach (var reference in blocked) occluded.Add(reference);
        currentCandidate = lookTarget = selected = null;
        rotations = 0;
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            new Dictionary<string, BoxDataForm.Data> { ["param1"] = Text(Self) }, program,
            0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = data.Interpret();
        Check(result.complete && result.errors.Count == 0,
            context + " executes without interpreter errors: " + string.Join("; ", result.errors));
        Check(selected == expected && rotations == (expected == null ? 0 : 1),
            context + ": selected=" + selected + " expected=" + expected);
        Equal(Read(result.ret), expected == null ? Forward : (Positions[expected] - Positions[Self]).normalized,
            context + " returns normalized 3D direction or the unchanged forward fallback");
    }

    private static void CheckShotPrograms(EventProgramDataForm.Data[] programs, Vector3 expected)
    {
        candidates = new[] { Self, Near };
        occluded.Clear();
        foreach (int uid in new[] { 5, 6, 19, 23, 20 })
        {
            WrittenDirections.Clear();
            remoteCase = uid == 20;
            RunProgram(programs.Single(data => data.uid == uid), Self);
            Check(WrittenDirections.Count == (uid == 23 ? 28 : 1),
                "shot program " + uid + " retains its projectile count");
            foreach (var direction in WrittenDirections)
                Equal(direction, expected * (uid == 19 || uid == 20 ? 2 : 1),
                    "shot program " + uid + " passes the returned 3D vector, preserving Sword speed");
            remoteCase = false;
        }
        candidates = Array.Empty<string>();
        WrittenDirections.Clear();
        RunProgram(programs.Single(data => data.uid == 19), Self);
        Equal(WrittenDirections.Single(), Forward * 2, "Sword without an enemy retains its old forward direction and doubled speed");
    }

    private static void CheckProjectileMovement(EventProgramDataForm.Data[] programs)
    {
        const string projectile = "$so$p:989999$so$";
        var direction = new Vector3(.3f, .4f, .5f);
        ProjectilePositions[projectile] = Positions[Self] + Vector3.up * .2f;
        ProjectileParameters[projectile] = new Dictionary<string, BoxDataForm.Data>
        {
            ["移动方向"] = Vector(direction), ["所属人物"] = Text(Self)
        };
        Moves.Clear();
        RunProgram(programs.Single(data => data.uid == 9), projectile);
        Equal(Moves.Single(), direction * 80, "straight projectile movement preserves and scales all three axes");

        homingCase = true;
        candidates = new[] { Near };
        var original = Positions[Near];
        foreach (float heightDelta in new[] { 3f, -2f })
        {
            Positions[Near] = original + Vector3.up * heightDelta;
            ProjectilePositions[projectile] = Positions[Self] + Vector3.up * .2f;
            ProjectileParameters[projectile]["移动方向"] = Vector(Positions[Near] - Positions[Self]);
            Moves.Clear();
            RunProgram(programs.Single(data => data.uid == 7), projectile);
            Check(Moves.Count == 20, "homing projectile retains its original movement count");
            Check(Moves[0].y * heightDelta > 0 && Moves[1].y * heightDelta > 0,
                "homing initial and retargeted segments retain upward/downward components");
            var firstPosition = Positions[Self] + Vector3.up * .2f + Moves[0];
            Equal(Moves[1], (Positions[Near] + Vector3.up * .2f - firstPosition).normalized,
                "homing retarget aims at the target's launch-height point in 3D");
        }
        Positions[Near] = original;
        homingCase = false;
    }

    private static void RunProgram(ProgramDataForm.Data program, string actor)
    {
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
            new Dictionary<string, BoxDataForm.Data> { ["param1"] = Text(actor) }, program,
            0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        for (int polls = 0; polls < 100; polls++)
        {
            var result = data.Interpret();
            Check(result.errors.Count == 0, program.name + " executes without errors: " + string.Join("; ", result.errors));
            if (result.complete) return;
            // This routing fixture advances only the literal waits in the beam;
            // bounded-clock/timer behavior is covered by the timing regressions.
            Check(data.program.zCode[data.p] == ((int)Op.Wait).ToString() && data.stack.Count > 0,
                program.name + " suspends only at an authored literal wait");
            data.stack[data.stack.Count - 1].num = 0;
        }
        throw new InvalidOperationException(program.name + " did not finish its trajectory routing fixture");
    }

    private static Vector3 Position(string reference) => ProjectilePositions.TryGetValue(reference ?? "", out var bullet)
        ? bullet : Positions.TryGetValue(reference ?? "", out var actor) ? actor : Vector3.zero;

    private static void Probe(string name, Func<BoxDataForm.Data[], BoxDataForm.Data> execute, string metadataName = null) =>
        BaseData.cmdDic[name] = new ProbeCommand(metadataName ?? name, execute);
    private sealed class ProbeCommand : CmdBase
    {
        private readonly string name;
        private readonly Func<BoxDataForm.Data[], BoxDataForm.Data> execute;
        public ProbeCommand(string name, Func<BoxDataForm.Data[], BoxDataForm.Data> execute)
        { this.name = name; this.execute = execute; }
        public override string GetName() => name;
        public override CmdBase GetNew() => new ProbeCommand(name, execute);
        protected override bool ExecuteInternal(BoxDataForm.Data[] parameters, InterpretAsyncTask task)
        {
            var result = execute(parameters);
            task.res = result == null ? Array.Empty<BoxDataForm.Data>() : new[] { result };
            return true;
        }
    }
    private static BoxDataForm.Data Text(string value) => CodeHelper.CreateBoxByStr(value);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static BoxDataForm.Data Vector(Vector3 value)
    {
        var box = CodeHelper.CreateBox();
        box.dic["x"] = Number(value.x); box.dic["height"] = Number(value.y); box.dic["y"] = Number(value.z);
        return box;
    }
    private static Vector3 Read(BoxDataForm.Data value) =>
        new Vector3(value.dic["x"].num, value.dic["height"].num, value.dic["y"].num);
    private static void Equal(Vector3 actual, Vector3 expected, string message) =>
        Check(Vector3.Distance(actual, expected) < .00001f, message + ": " + actual + " != " + expected);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
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
}
