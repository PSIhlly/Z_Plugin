using System;
using System.Collections;
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
using Z_Time;
using Z_UnitSystem;
using Z_UnitSystem.Form;

// Real scheduler, Pause/Continue, interpreter Wait, Progress and Object movement.
// Runs only in the runner's disposable project; no scene/save is loaded.
public static class PauseTasksRegression
{
    private const BindingFlags Hidden = BindingFlags.NonPublic | BindingFlags.Instance;
    private const BindingFlags StaticHidden = BindingFlags.NonPublic | BindingFlags.Static;
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static readonly Dictionary<int, int> polls = new Dictionary<int, int>();
    private static readonly HashSet<int> finish = new HashSet<int>();
    private static readonly Dictionary<int, int> cancelled = new Dictionary<int, int>();
    private static readonly MethodInfo advanceMove = typeof(MoveCmd).GetMethod("AdvanceMove", StaticHidden);
    private static GameManager game;
    private static GameEventController events;
    private static PlayInfoController info;
    private static ProgressForm.Data progress;
    private static ObjectUnit moving;
    private static float remaining;
    private static int checks, sequence;

    private sealed class TaskCmd : CmdBase
    {
        public override string GetName() => "RegressionPauseTask";
        public override CmdBase GetNew() => new TaskCmd();
        protected override bool ExecuteInternal(BoxDataForm.Data[] prm, InterpretAsyncTask task)
        {
            int tag = (int)prm[0].num;
            events.StartTask(task, () =>
            {
                polls[tag] = Count(polls, tag) + 1;
                if (tag == 1)
                {
                    var args = new object[] { moving, new Vector3(9, 0, 5), 1f, remaining, .02f };
                    advanceMove.Invoke(null, args);
                    remaining = (float)args[3];
                }
                if (!finish.Contains(tag)) return false;
                task.Complete();
                return true;
            }, () => cancelled[tag] = Count(cancelled, tag) + 1);
            return false;
        }
    }

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name.StartsWith("MapRuntimeRegression-"), "isolated project guard");
            SetUp();
            WaitAndProgress();
            ObjectMovementAndResume();
            InitialPollAndChildOwnership();
            CancellationWhilePaused();
            Debug.Log("PAUSE_TASKS_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        finally { events?.Reset(); }
    }

    private static void Register(Type form)
    {
        if (!forms.Add(form)) return;
        var parent = form.GetNestedType("Data")?.BaseType?.DeclaringType;
        if (parent != null) Register(parent);
        form.GetMethod("Register", StaticHidden)?.Invoke(null, null);
        form.GetMethod("InitInternal", BindingFlags.Static | BindingFlags.Public)?.Invoke(null, null);
    }

    private static void SetUp()
    {
        foreach (var form in new[] { typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(EventProgramDataForm),
            typeof(EventInterpretDataForm), typeof(ProgressForm), typeof(SceneForm), typeof(ObjectUnitForm),
            typeof(TileUnitForm), typeof(ObjectProductForm), typeof(CharacterProductForm) }) Register(form);
        foreach (var type in new[] { typeof(PauseCmd), typeof(ContinueCmd) }) type.GetMethod("Init", StaticHidden).Invoke(null, null);
        BaseData.cmdDic["RegressionPauseTask"] = new TaskCmd();
        CmdDataForm.AddData(new CmdDataForm.Data(98911, "RegressionPauseTask", new List<string> { "tag" },
            new List<string> { "num" }, null, new List<string> { "void" }, "", "RegressionPauseTask(1);"));
        game = Manager<GameManager>();
        game.curScene = SceneForm.defaultData;
        progress = ProgressForm.defaultData; progress.uid = 1; progress.characterUid = 0;
        progress.team = new List<int>(); progress.teamActive = new List<int>();
        ProgressForm.AddData(progress);
        events = new GameEventController(game); game.evtCtrl = events;
        info = new PlayInfoController(Manager<PlayManager>());
        typeof(PlayInfoController).GetField("enable", Hidden).SetValue(info, true);
        var map = Manager<MapManager>();
        map.data = new MapInfo
        {
            mainData = new MapMainForm.Data(1, Vector3.one, new Vector3Int(20, 2, 20), new Vector3Int(10, 1, 10), "", "", "", ""),
            maps = new Dictionary<(int, int, int), TileUnitForm.Data>(), mapXZ2Y = new Dictionary<(int, int), SortedSet<int>>()
        };
        map.utilCtrl = new MapUtilController(map); map.updateCtrl = new MapUpdateController(map);
        var pool = Manager<InstancePoolManager>(); pool.Init(); pool.defaultRoot = pool.transform;
        var prefab = new GameObject("pause-task-empty"); prefab.SetActive(false); pool.AddPool(prefab);
        for (int x = 4; x <= 10; x++)
            map.data.RegisterMap(new TileUnitForm.Data(97000 + x, "", new Dictionary<int, int>(), new Vector3Int(x, 0, 5),
                prefab.name, new Vector3(x, 0, 5), Vector3.zero, Vector3.one, UpdateType.ShowOnly, new List<int>(), "", true, false, new List<int>()));
        var product = ObjectProductForm.defaultData; product.uid = 97011; ObjectProductForm.AddData(product);
        var data = new ObjectUnitForm.Data(97012, true, "pause-move", prefab.name, new Vector3(5, 0, 5), Vector3.zero,
            Vector3.one, UpdateType.ShowOnly, new List<int>(), MapUnit.GetProductInfoString(new JObject
            {  }, (product.uid, -1)), false, 0);
        ObjectUnitForm.AddData(data); UnitForm.DataByUid[data.uid] = data;
        moving = data.unit; map.updateCtrl.objectTileDic.Add(moving, map.utilCtrl.GetTile(5, 0, 5));
    }

    private static EventInterpretDataForm.Data Queue(string code)
    {
        var program = new EventProgramDataForm.Data(-1, "pause-task-" + ++sequence, "", new List<string>(), new List<int>(), 0, "void", 0);
        Check(program.TryApplyCode(code, out _, out var errors), "real compiler: " + string.Join(";", errors));
        return events.Execute(program, 0, null);
    }

    private static void Clean()
    {
        events.Reset(); progress.blockProgramUid = 0; progress.seconds = 10.25f;
        polls.Clear(); finish.Clear(); cancelled.Clear(); remaining = 4;
    }
    private static void Tick() { info.Update(); events.LateUpdate(); }
    private static float WaitLeft(EventInterpretDataForm.Data data) => data.stack.Last().num;
    private static bool Has(EventInterpretDataForm.Data data, string key) => data.heap.TryGetValue(key, out var value) && value.num == 1;

    private static void WaitAndProgress()
    {
        Clean();
        var background = Queue("Wait(5); afterWait = 1;"); Tick();
        var owner = Queue("GamePause(); Wait(5); GameContinue(); afterPause = 1;"); Tick();
        Check(progress.blockProgramUid == owner.uid, "Pause selects its root event");
        float backgroundWait = WaitLeft(background), ownerWait = WaitLeft(owner), seconds = progress.seconds;
        float delta = TimeManager.GetDeltaTime();
        for (int frame = 1; frame <= 30; frame++)
        {
            Tick();
            Check(WaitLeft(background) == backgroundWait && !Has(background, "afterWait"), "Other-event Wait keeps its exact remaining duration throughout Pause");
            Check(Mathf.Abs(WaitLeft(owner) - (ownerWait - frame * delta)) < .0001f,
                "Pause owner's Wait consumes its ordinary bounded delta, independently of frozen Progress.seconds");
            Check(progress.seconds == seconds && DynamicGlobalSettings.pauseNav, "Pause freezes Progress.seconds and navigation");
        }
        // Advance only the owner's authored Wait to its boundary; the other
        // event's countdown must not have elapsed during those paused frames.
        owner.stack.Last().num = 0;
        Tick();
        Check(progress.blockProgramUid == 0 && Has(owner, "afterPause"), "Owner Wait can finish and run Continue without deadlock");
        Check(WaitLeft(background) == backgroundWait && !Has(background, "afterWait"), "Continue does not immediately skip another event's Wait");
        Tick();
        Check(Mathf.Abs(WaitLeft(background) - (backgroundWait - delta)) < .00001f && !Has(background, "afterWait"),
            "Resumed Wait consumes only the new frame, not wall time spent paused");
        Check(Mathf.Abs(progress.seconds - (seconds + delta)) < .00001f, "Progress resumes without a paused-time catch-up");
    }

    private static void ObjectMovementAndResume()
    {
        Clean();
        Queue("RegressionPauseTask(1);"); Tick();
        var owner = Queue("GamePause(); RegressionPauseTask(2); GameContinue();"); Tick();
        float beforeX = moving.data.pos.x, beforeTime = remaining;
        int beforePolls = Count(polls, 1), ownerPolls = Count(polls, 2);
        for (int frame = 0; frame < 20; frame++) Tick();
        Check(moving.data.pos.x == beforeX && remaining == beforeTime && Count(polls, 1) == beforePolls,
            "Already-started Object movement and its allotted duration both freeze under another event's Pause");
        Check(Count(polls, 2) == ownerPolls + 20 && progress.blockProgramUid == owner.uid, "Pause owner's own async task keeps polling");
        finish.Add(2); Tick(); Tick();
        Check(progress.blockProgramUid == 0 && Count(polls, 1) == beforePolls + 1,
            "Background tasks resume after the owner's task completes and Continue runs");
        Check(Mathf.Abs(moving.data.pos.x - beforeX - .02f) < .0001f && Mathf.Abs(remaining - beforeTime + .02f) < .0001f,
            "The real Object Move step resumes at normal speed without jumping or expiring");
    }

    private static void InitialPollAndChildOwnership()
    {
        Clean();
        var owner = Queue("GamePause(); RegressionPauseTask(2); GameContinue();"); Tick();
        var other = Queue("RegressionPauseTask(6);");
        var child = Queue("RegressionPauseTask(3);");
        typeof(EventInterpretDataForm.Data).GetField("blockingParent", Hidden).SetValue(child, owner);
        var grandchild = Queue("RegressionPauseTask(4);");
        typeof(EventInterpretDataForm.Data).GetField("blockingParent", Hidden).SetValue(grandchild, child);
        var standaloneData = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            null, 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
        var standaloneTask = new InterpretAsyncTask(new Interpreter(standaloneData)); standaloneTask.Run();
        int standalonePolls = 0;
        events.StartTask(standaloneTask, () => { standalonePolls++; return false; });
        int injectedPolls = 0;
        var task = new InterpretAsyncTask(new Interpreter(other)); task.Run();
        events.StartTask(task, () => { injectedPolls++; return false; });
        Check(standalonePolls == 0 && injectedPolls == 0, "Even StartTask's immediate first poll respects Pause for non-owner and standalone tasks");
        Tick();
        Check(Count(polls, 6) == 0 && injectedPolls == 0 && standalonePolls == 0, "Unrelated event bodies and all their tasks stay stopped");
        Check(Count(polls, 3) >= 2 && Count(polls, 4) >= 2, "Required child/grandchild hook tasks inherit the active Pause owner's permission");
        var sub = new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(), new Dictionary<string, BoxDataForm.Data>(),
            null, 0, -1, 0, null, new List<BoxDataForm.Data>(), owner.uid);
        var subTask = new InterpretAsyncTask(new Interpreter(sub)); subTask.Run();
        int subPolls = 0; events.StartTask(subTask, () => { subPolls++; return false; });
        Check(subPolls == 1, "A task created inside the Pause owner's subprogram resolves the root event and remains runnable");
    }

    private static void CancellationWhilePaused()
    {
        Clean();
        var other = Queue("RegressionPauseTask(7);"); Tick();
        Queue("GamePause(); RegressionPauseTask(2); GameContinue();"); Tick();
        int before = Count(polls, 7); other.Cancel(); Tick(); Tick();
        Check(Count(polls, 7) == before && Count(cancelled, 7) == 1 && !EventInterpretDataForm.DataByUid.ContainsKey(other.uid),
            "Paused tasks still cancel/release exactly once instead of executing or leaking");
        events.Reset();
        Check(((ICollection)typeof(GameEventController).GetField("tasks", Hidden).GetValue(events)).Count == 0,
            "Scene/reset cleanup releases paused and active tasks alike");
    }

    private static int Count(Dictionary<int, int> values, int key) => values.TryGetValue(key, out int value) ? value : 0;
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject("pause-regression-" + typeof(T).Name); root.SetActive(false);
        var manager = root.AddComponent<T>(); typeof(Z_MonoSingleton<T>).GetField("_instance", StaticHidden).SetValue(null, manager); return manager;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
