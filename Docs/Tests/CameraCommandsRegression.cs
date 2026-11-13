using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Reflection;
using Form;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Input;
using Z_Map;
using Z_Map.Form;
using Z_Ui;
using Z_Ui.Base;
using Z_UnitSystem.Form;
using Object = UnityEngine.Object;

// Real command/camera/task code, only in the runner's disposable Unity project.
public static class CameraCommandsRegression
{
    private const BindingFlags HiddenStatic = BindingFlags.Static | BindingFlags.NonPublic;
    private const BindingFlags HiddenInstance = BindingFlags.Instance | BindingFlags.NonPublic;
    private static readonly List<GameObject> roots = new List<GameObject>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static readonly FieldInfo Tasks = typeof(GameEventController).GetField("tasks", HiddenInstance);
    private static readonly MethodInfo ClearTasks = typeof(GameEventController).GetMethod("CancelAllTasks", HiddenInstance);
    private static readonly MethodInfo Advance = typeof(MoveCameraCmd).GetMethod("AdvanceMove", HiddenStatic);
    private static CameraInstance camera;
    private static GameManager game;
    private static MapManager map;
    private static GameEventController events;
    private static PlaySceneController play;
    private static int checks;

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name
                .StartsWith("MapRuntimeRegression-", StringComparison.Ordinal), "isolated project guard");
            SetUp();
            MetadataAndCompilation();
            CoordinatesAndInstantMoves();
            BoundedLinearMovement();
            FollowAndRestore();
            AsyncOwnershipAndReplacement();
            LifecycleAndChildPose();
            Debug.Log("CAMERA_COMMANDS_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error)
        {
            Debug.LogException(error);
            EditorApplication.Exit(1);
        }
        finally
        {
            for (int i = roots.Count - 1; i >= 0; i--)
                if (roots[i] != null) Object.DestroyImmediate(roots[i]);
        }
    }

    private static void RegisterForm(Type form)
    {
        if (!forms.Add(form)) return;
        var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType;
        if (parent != null) RegisterForm(parent);
        form.GetMethod("Register", HiddenStatic)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
    }

    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject("camera-regression-" + typeof(T).Name);
        roots.Add(root);
        root.SetActive(false);
        var value = root.AddComponent<T>();
        typeof(Z_MonoSingleton<T>).GetField("_instance", HiddenStatic).SetValue(null, value);
        return value;
    }

    private static void SetUp()
    {
        foreach (var form in new[] { typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(CmdLabForm),
            typeof(BoxDataForm), typeof(ProgramDataForm), typeof(SceneForm), typeof(CharacterUnitForm),
            typeof(ObjectUnitForm), typeof(ItemUnitForm), typeof(TileUnitForm), typeof(EventInterpretDataForm),
            typeof(ProgressForm), typeof(CommonTextForm), typeof(ModTextForm) })
            RegisterForm(form);
        foreach (var type in new[] { typeof(MoveCameraCmd), typeof(MoveCameraToCmd), typeof(RestoreCameraCmd), typeof(NewVectorCmd) })
            type.GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly).Invoke(null, null);
        game = Manager<GameManager>();
        game.curScene = SceneForm.defaultData;
        var progress = ProgressForm.defaultData;
        progress.uid = 1;
        progress.editorStyle = EditorStyle.Avg;
        ProgressForm.AddData(progress);
        map = Manager<MapManager>();
        map.Init();
        map.mainGo = new GameObject("camera-regression-empty-map");
        roots.Add(map.mainGo);
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, new Vector3(2, 3, 4), new Vector3Int(2000, 2000, 2000),
                new Vector3Int(10, 2, 10), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(),
            mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        map.enable = true;
        events = new GameEventController(game);
        game.evtCtrl = events;
        var owner = Manager<PlayManager>();
        play = new PlaySceneController(owner);
        owner.sceneCtrl = play;
        var ui = Manager<UiManager>();
        ui.layer2Id = new Dictionary<UiLayer, int>();
        Manager<InputManager>();

        var root = new GameObject("camera-regression-root");
        roots.Add(root);
        var child = new GameObject("camera-child");
        child.transform.SetParent(root.transform, false);
        child.transform.localPosition = new Vector3(0, 8, -8);
        child.transform.localEulerAngles = new Vector3(45, 0, 0);
        child.AddComponent<Camera>().orthographic = true;
        camera = root.AddComponent<CameraInstance>();
        typeof(Z_MonoSingleton<CameraInstance>).GetField("_instance", HiddenStatic).SetValue(null, camera);
        camera.Register(Vector3.zero, Vector3.one * 10000, 4, 15, 5);
    }

    private static void MetadataAndCompilation()
    {
        var names = new[] { "MoveCamera", "RestoreCamera", "MoveCameraTo" };
        var ids = new[] { 100123, 100124, 100125 };
        for (int i = 0; i < names.Length; i++)
        {
            Check(CmdBase.TryGetForm(names[i], out var form), names[i] + " has registered metadata");
            var typed = (GameCmdDataForm.Data)form;
            Check(typed.uid == ids[i] && typed.labId == 10026, names[i] + " has a stable Scene CmdLab identity");
            Check(typed.lowestEditorStyle == EditorStyle.Avg, names[i] + " is available in the basic AVG editor mode");
            Check(form.retTypes.SequenceEqual(new[] { "void" }), names[i] + " returns void");
            if (names[i] == "RestoreCamera")
                Check(form.prmTypes == null || form.prmTypes.Count == 0, "RestoreCamera has no parameters");
            else
                Check(form.prmTypes.SequenceEqual(new[] { "vector", "num" }), names[i] + " uses vector then seconds");
            Check(form.defaultCode.EndsWith(";", StringComparison.Ordinal), names[i] + " default is an independent statement");
            var program = ProgramDataForm.defaultData;
            program.name = "camera-command-default-" + i;
            Check(program.TryApplyCode(form.defaultCode, out _, out _), names[i] + " editor default compiles");
            Check(BaseData.cmdDic[names[i]].GetNew().GetName() == names[i], names[i] + " creates the right concrete command");
            var init = BaseData.cmdDic[names[i]].GetType().GetMethod("Init", HiddenStatic | BindingFlags.DeclaredOnly);
            Check(init.GetCustomAttributes(typeof(RuntimeInitializeOnLoadMethodAttribute), false).Length == 1,
                names[i] + " has explicit Player-safe registration");
        }
        var chooser = events.GetCmdEntry(default, "void", out _);
        foreach (int uid in ids)
            Check(chooser.subs.Values.SelectMany(category => category.subs.Values).Count(item => item.id == uid) == 1,
                "AVG void-command chooser exposes camera command " + uid + " exactly once");
        var execution = ProgramDataForm.defaultData;
        execution.name = "camera-compiled-zero-duration";
        Check(execution.TryApplyCode("MoveCameraTo(NewVector(2,3,4),0);MoveCamera(NewVector(1,-2,3),0);Return 42;", out _, out _),
            "both camera movement signatures compile together");
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            execution, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var result = data.Interpret();
        Check(result.complete && result.errors.Count == 0 && result.ret.num == 42, "instant movement continues in the same interpreter pass");
        Equal(camera.tarTrs.position, new Vector3(503, 507, 501), "source NewVector maps x/y/height with absolute +500 only once");
    }

    private static void CoordinatesAndInstantMoves()
    {
        Reset(new Vector3(100, 200, 300));
        var task = Execute(new MoveCameraCmd(), Vector(new Vector3(2, 4, 3)), Number(0));
        Check(task.IsComplete() && task.error == null && !camera.followsPlayer, "zero duration takes manual control synchronously");
        Equal(camera.tarTrs.position, new Vector3(102, 204, 303), "relative vector uses world units without a +500 offset");
        Execute(new MoveCameraCmd(), Vector(new Vector3(-1, -2, -3)), Number(-1));
        Equal(camera.tarTrs.position, new Vector3(101, 202, 300), "negative duration positions immediately relative to the current camera");
        Execute(new MoveCameraToCmd(), Vector(new Vector3(2, 4, 3)), Number(0));
        Equal(camera.tarTrs.position, new Vector3(502, 504, 503), "absolute coordinates retain +500 without applying nonuniform cell scaling");
        camera.tarTrs.position = new Vector3(7, 8, 9);
        Execute(new MoveCameraToCmd(), Vector(new Vector3(2, 4, 3)), Number(0));
        Equal(camera.tarTrs.position, new Vector3(502, 504, 503), "absolute positioning does not add the current camera origin");
        Execute(new MoveCameraCmd(), Vector(Vector3.one * 10000), Number(0));
        Equal(camera.tarTrs.position, Vector3.one * 10000, "manual target respects registered camera limits");

        foreach (CmdBase command in new CmdBase[] { new MoveCameraCmd(), new MoveCameraToCmd() })
        {
            foreach (var invalid in new[] { float.NaN, float.PositiveInfinity, float.NegativeInfinity })
            {
                Reset(new Vector3(12, 13, 14));
                var duration = Execute(command, Vector(Vector3.one), Number(invalid));
                Check(duration.IsComplete() && duration.error == null && camera.followsPlayer,
                    command.GetName() + " invalid duration does not seize camera control");
                var vector = Execute(command, Vector(new Vector3(invalid, 0, 0)), Number(0));
                Check(vector.IsComplete() && vector.error == null && camera.followsPlayer,
                    command.GetName() + " invalid vector does not seize camera control");
                Equal(camera.tarTrs.position, new Vector3(12, 13, 14), "invalid values retain the camera position");
            }
            Reset(new Vector3(12, 13, 14));
            var malformed = Execute(command, Number(3), Number(1));
            Check(malformed.IsComplete() && malformed.error != null && camera.followsPlayer,
                command.GetName() + " malformed vectors report an error without taking control");
        }
        var previous = map.data;
        map.data = null;
        Check(Execute(new MoveCameraCmd(), Vector(Vector3.one), Number(1)).IsComplete(), "movement without a map returns immediately");
        map.data = previous;
    }

    private static void BoundedLinearMovement()
    {
        Reset(new Vector3(100, 200, 300));
        var start = camera.tarTrs.position;
        var target = start + new Vector3(10, 20, 30);
        uint version = camera.BeginScriptedMove();
        var state = new object[] { camera, version, start, target, 1f, 0f, 1f };
        Check(!(bool)Advance.Invoke(null, state) && Mathf.Abs((float)state[5] - .02f) < .00001f,
            "one stalled frame consumes only the bounded .02-second step");
        Equal(camera.tarTrs.position, Vector3.Lerp(start, target, .02f), "stalled frame has no movement catch-up");
        state[5] = .48f; state[6] = .02f;
        Check(!(bool)Advance.Invoke(null, state), "halfway still waits");
        Equal(camera.tarTrs.position, Vector3.Lerp(start, target, .5f), "movement is linear at the halfway point");
        state[5] = .99f;
        Check((bool)Advance.Invoke(null, state) && (float)state[5] == 1f, "last partial step reaches exact completion time");
        Equal(camera.tarTrs.position, target, "completed movement reaches the exact target");
        Check(!camera.followsPlayer, "completion keeps the endpoint in manual mode");
        play.SetCamera(1, 2, 3);
        Equal(camera.tarTrs.position, target, "normal player-follow writes cannot pull a completed pan back");
        state[5] = 0f; state[6] = 0f;
        Advance.Invoke(null, state);
        Equal(camera.tarTrs.position, start, "zero scaled delta consumes no movement time");
        state[6] = float.NaN;
        Advance.Invoke(null, state);
        Check((float)state[5] == 0f, "NaN explicit delta is clamped to zero");
    }

    private static void FollowAndRestore()
    {
        Reset(Vector3.one * 50);
        play.SetCamera(10, 20, 30);
        Equal(camera.tarTrs.position, new Vector3(10, 20, 30), "ordinary Play camera positions still follow while enabled");
        var first = Character(97001, new Vector3(20, 30, 40));
        var second = Character(97002, new Vector3(60, 70, 80));
        SetPlayer(first);
        uint version = camera.BeginScriptedMove();
        camera.SetScriptedPosition(version, new Vector3(100, 200, 300));
        SetPlayer(second);
        var restore = Execute(new RestoreCameraCmd());
        Check(restore.IsComplete() && restore.error == null && camera.followsPlayer, "RestoreCamera completes synchronously");
        Equal(camera.tarTrs.position, second.pos, "RestoreCamera selects the current player rather than the pan-start player");
        Check(!camera.HasScriptedControl(version), "restore invalidates the old motion version");
        second.pos += Vector3.one;
        play.SetCamera(second.pos.x, second.pos.y, second.pos.z);
        Equal(camera.tarTrs.position, second.pos, "following continues with later player movement");
        camera.BeginScriptedMove();
        SetPlayer(null);
        var held = camera.tarTrs.position;
        Execute(new RestoreCameraCmd());
        Check(camera.followsPlayer, "restoration without a player still enables future follow");
        Equal(camera.tarTrs.position, held, "restoration without a player retains camera position");
        SetPlayer(first);
        UnitForm.DataByUid[first.uid] = second;
        Execute(new RestoreCameraCmd());
        Equal(camera.tarTrs.position, held, "a recycled player UID cannot redirect restoration");
        UnitForm.DataByUid[first.uid] = first;
        SetPlayer(null);
    }

    private static void AsyncOwnershipAndReplacement()
    {
        Reset(new Vector3(100, 200, 300));
        var input = Vector(new Vector3(3, 4, 5));
        var task = Execute(new MoveCameraCmd(), input, Number(1));
        var pending = Pending();
        Check(!task.IsComplete() && !camera.followsPlayer, "positive duration waits through the real event task queue");
        var firstPoll = camera.tarTrs.position;
        input.dic["x"].num = 999;
        Poll(pending);
        Equal(camera.tarTrs.position, firstPoll, "same-frame task polls do not consume delta twice or reread mutated inputs");
        Execute(new MoveCameraToCmd(), Vector(new Vector3(1, 2, 3)), Number(0));
        var replaced = camera.tarTrs.position;
        Check(Poll(pending) && task.IsComplete(), "a newer movement completes the old pending command");
        Equal(camera.tarTrs.position, replaced, "old relative movement cannot overwrite a new absolute target");

        Reset(new Vector3(100, 200, 300));
        task = Execute(new MoveCameraToCmd(), Vector(Vector3.one), Number(1));
        pending = Pending();
        Execute(new RestoreCameraCmd());
        var restored = camera.tarTrs.position;
        Check(Poll(pending) && task.IsComplete(), "restore interrupts a waiting movement immediately even in its first frame");
        Equal(camera.tarTrs.position, restored, "interrupted movement cannot undo restoration");

        Reset(new Vector3(100, 200, 300));
        task = Execute(new MoveCameraCmd(), Vector(Vector3.one), Number(1));
        pending = Pending();
        var scene = game.curScene;
        game.curScene = SceneForm.defaultData;
        var atSwitch = camera.tarTrs.position;
        Check(Poll(pending) && task.IsComplete(), "scene identity changes finish old movement without resuming it");
        Equal(camera.tarTrs.position, atSwitch, "scene changes do not apply an old movement step");
        game.curScene = scene;

        Reset(new Vector3(100, 200, 300));
        task = Execute(new MoveCameraCmd(), Vector(Vector3.one), Number(1));
        pending = Pending();
        var original = map.data;
        map.data = new MapInfo();
        Check(Poll(pending) && task.IsComplete(), "map replacement ends a waiting movement");
        map.data = original;
        Reset(new Vector3(100, 200, 300));
        task = Execute(new MoveCameraCmd(), Vector(Vector3.one), Number(1));
        pending = Pending();
        var cancelled = camera.tarTrs.position;
        task.Cancel();
        Check(Poll(pending), "event task cancellation stops polling movement");
        Equal(camera.tarTrs.position, cancelled, "cancelled movement retains its current manual position");
        Check(!camera.followsPlayer, "event cancellation does not implicitly restore follow");
    }

    private static void LifecycleAndChildPose()
    {
        Reset(new Vector3(100, 200, 300));
        Vector3 localPosition = camera.cam.transform.localPosition;
        Quaternion localRotation = camera.cam.transform.localRotation;
        float zoom = camera.cam.orthographicSize;
        uint version = camera.BeginScriptedMove();
        camera.SetScriptedPosition(version, new Vector3(500, 600, 700));
        Check(camera.cam.transform.localPosition == localPosition && camera.cam.transform.localRotation == localRotation
            && camera.cam.orthographicSize == zoom, "root movement preserves child pose and zoom for both camera modes/shake");
        var shake = camera.gameObject.AddComponent<SceneShakeRunner>();
        shake.Begin(.1f);
        var rootBeforeShake = camera.tarTrs.position;
        typeof(SceneShakeRunner).GetMethod("Update", HiddenInstance).Invoke(shake, null);
        Equal(camera.tarTrs.position, rootBeforeShake, "the actual shake runner offsets only the child, never the camera root");
        var shakenChild = camera.cam.transform.localPosition;
        camera.SetScriptedPosition(version, new Vector3(800, 900, 1000));
        Equal(camera.cam.transform.localPosition, shakenChild, "scripted root movement preserves an in-progress shake offset");
        typeof(SceneShakeRunner).GetMethod("Finish", HiddenInstance).Invoke(shake, null);
        Equal(camera.cam.transform.localPosition, localPosition, "finishing a concurrent shake restores the original child pose");
        camera.Register(Vector3.zero, Vector3.one * 10000, 4, 15, 5);
        Check(camera.followsPlayer && !camera.HasScriptedControl(version), "scene registration resets scripted ownership");
        version = camera.BeginScriptedMove();
        camera.enabled = false;
        Check(!camera.HasScriptedControl(version), "a disabled camera rejects pending movement even before lifecycle cleanup");
        // EditMode does not dispatch callbacks on this ordinary runtime Mono.
        typeof(CameraInstance).GetMethod("OnDisable", HiddenInstance).Invoke(camera, null);
        Check(camera.followsPlayer && !camera.HasScriptedControl(version), "camera disable invalidates scripted movement");
        camera.enabled = true;
        version = camera.BeginScriptedMove();
        events.Reset();
        play.End();
        Check(camera.followsPlayer && !camera.HasScriptedControl(version) && map.data == null,
            "real Play End resets camera ownership and unloads the old map");
    }

    private static void Reset(Vector3 position)
    {
        ClearTasks.Invoke(events, null);
        camera.RestorePlayerFollow();
        camera.tarTrs.position = position;
    }
    private static CharacterUnitForm.Data Character(int uid, Vector3 position)
    {
        var data = new CharacterUnitForm.Data(uid, false, Vector3.zero, 1, 1, 1, false, "camera-player", "",
            position, Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", true, 0, new List<int>());
        CharacterUnitForm.AddData(data);
        UnitForm.DataByUid[uid] = data;
        return data;
    }
    private static void SetPlayer(CharacterUnitForm.Data player) => typeof(PlaySceneController)
        .GetField("_playerM", HiddenInstance).SetValue(play, player);
    private static InterpretAsyncTask Execute(CmdBase command, params BoxDataForm.Data[] parameters)
    {
        var data = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            null, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var task = new InterpretAsyncTask(new Interpreter(data));
        command.Execute(parameters.Reverse().ToArray(), new Dictionary<string, BoxDataForm.Data>(), task);
        return task;
    }
    private static object Pending() => ((IEnumerable)Tasks.GetValue(events)).Cast<object>().Last();
    private static bool Poll(object pending) => (bool)pending.GetType().GetMethod("Update").Invoke(pending, null);
    private static BoxDataForm.Data Number(float value) => CodeHelper.CreateBoxByNum(value);
    private static BoxDataForm.Data Vector(Vector3 value)
    {
        var box = CodeHelper.CreateBox();
        box.dic["x"] = Number(value.x); box.dic["height"] = Number(value.y); box.dic["y"] = Number(value.z);
        return box;
    }
    private static void Equal(Vector3 value, Vector3 expected, string message) =>
        Check(Vector3.Distance(value, expected) < .001f, message + " actual=" + value + " expected=" + expected);
    private static void Check(bool condition, string message)
    {
        if (!condition) throw new InvalidOperationException(message);
        checks++;
    }
}
