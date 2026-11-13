using System;
using System.Collections;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using Form;
using Ui.Notify;
using UnityEditor;
using UnityEngine;
using Z_Code;
using Z_Code.Form;
using Z_DesignStyle;
using Z_Ui;
using Z_Ui.Base;
using Z_Ui.Notify;
using Stopwatch = System.Diagnostics.Stopwatch;

// Uses the production clock/budget and scheduler in an isolated native project.
public static class InterpreterTimeoutRegression
{
    private const BindingFlags Hidden = BindingFlags.Instance | BindingFlags.NonPublic;
    private const BindingFlags StaticHidden = BindingFlags.Static | BindingFlags.NonPublic;
    private static readonly Queue<Func<InterpretAsyncTask, bool>> steps = new Queue<Func<InterpretAsyncTask, bool>>();
    private static readonly HashSet<Type> forms = new HashSet<Type>();
    private static int checks, sequence;
    private static GameEventController events;

    private sealed class Probe : CmdBase
    {
        public override string GetName() => "RegressionTimeoutProbe";
        public override CmdBase GetNew() => new Probe();
        protected override bool ExecuteInternal(BoxDataForm.Data[] p, InterpretAsyncTask task) => steps.Dequeue()(task);
    }

    // Capture the existing notification packet, without constructing prefab UI.
    private sealed class CaptureNotify : UiNotifyCtrl
    {
        public PopupInfo captured;
        public override void SetParam(UiParam value) { captured = ((UiNotifyParam)value).popupInfo; }
    }

    public static void Run()
    {
        try
        {
            Check(new DirectoryInfo(Path.GetDirectoryName(Application.dataPath)).Name.StartsWith("MapRuntimeRegression-"), "isolated project guard");
            foreach (var type in new[] { typeof(GameCmdDataForm), typeof(GameCmdTypeDataForm), typeof(EventProgramDataForm),
                typeof(EventInterpretDataForm), typeof(ProgressForm), typeof(SceneForm) }) Register(type);
            BaseData.cmdDic["RegressionTimeoutProbe"] = new Probe();
            CmdDataForm.AddData(new CmdDataForm.Data(98871, "RegressionTimeoutProbe", new List<string>(),
                new List<string>(), null, new List<string> { "void" }, "", "RegressionTimeoutProbe();"));
            typeof(PauseCmd).GetMethod("Init", StaticHidden).Invoke(null, null);
            Check(Interpreter.MaxExecutionSecondsPerFrame == 1d, "one-second production threshold");
            DirectExecution();
            SharedAndFrameBudgets();
            SchedulerErrorAndCleanup();
            Debug.Log("INTERPRETER_TIMEOUT_REGRESSION_PASS checks=" + checks);
            EditorApplication.Exit(0);
        }
        catch (Exception error) { Debug.LogException(error); EditorApplication.Exit(1); }
        finally { events?.Reset(); }
    }

    private static void DirectExecution()
    {
        var fast = Create("for(i=0;i<5;i++){value=i;}Return value;");
        var result = fast.Interpret();
        Check(result.complete && result.errors.Count == 0 && result.ret.num == 4, "fast events are unchanged");
        steps.Enqueue(task => { Thread.Sleep(1050); return true; });
        var slow = Create("before=1;\nRegressionTimeoutProbe();\nafter=1;");
        result = slow.Interpret();
        CheckTimeout(result, "a slow synchronous command stops the event");
        var error = result.errors[0];
        Check(error.ProgramName == slow.program.name && error.LineNumber == 2 && error.ColumnNumber > 0,
            "timeout has the original program and line/column");
        Check(error.OpCode == Op.CallDiscard && error.SourceLine == "RegressionTimeoutProbe();", "timeout points to the slow command, not its next statement");
        Check(!slow.heap.ContainsKey("after"), "statements after a slow command do not execute");
        Debug.Log("TIMEOUT_REAL_CLOCK " + error);

        steps.Enqueue(task => { Advance(task, 1.01); return true; });
        result = Create("RegressionTimeoutProbe();").Interpret();
        CheckTimeout(result, "a slow final instruction cannot escape through normal completion");
        steps.Enqueue(task => { Advance(task, 1.01); return false; });
        result = Create("RegressionTimeoutProbe();after=1;").Interpret();
        CheckTimeout(result, "an async-start command cannot escape through a yield");

        var reset = Create("RegressionTimeoutProbe();Return 7;");
        steps.Enqueue(task => { Advance(task, 1.01); return true; });
        CheckTimeout(reset.Interpret(), "reset fixture initially exceeds its budget");
        reset.Reset();
        steps.Enqueue(task => true);
        result = reset.Interpret();
        Check(result.complete && result.errors.Count == 0 && result.ret.num == 7, "Reset clears the previous execution time");
        result = Create("Return 1/0;").Interpret();
        Check(result.complete && result.errors.Count == 1 && !result.errors[0].Message.Contains("超时"), "ordinary runtime errors retain their existing diagnostics");
    }

    private static void SharedAndFrameBudgets()
    {
        var child = Program("RegressionTimeoutProbe();", "TimeoutChild"); ProgramDataForm.AddData(child);
        var parent = Create("TimeoutChild();\nTimeoutChild();\nafter=1;");
        steps.Enqueue(task => { Advance(task, .6); return true; });
        steps.Enqueue(task => { Advance(task, .6); return true; });
        var result = parent.Interpret();
        CheckTimeout(result, "sibling subprogram calls share the root event's time");
        Check(result.errors[0].ProgramName == child.name && !parent.heap.ContainsKey("after"), "nested timeouts retain the child's location and stop the caller");

        InterpretAsyncTask pending = null;
        var cumulative = Create("RegressionTimeoutProbe();RegressionTimeoutProbe();Return 1;");
        steps.Enqueue(task => { Advance(task, .6); pending = task; return false; });
        result = cumulative.Interpret();
        Check(!result.complete && result.errors.Count == 0, "under-budget suspended call stays pending");
        pending.Complete();
        steps.Enqueue(task => { Advance(task, .6); return true; });
        CheckTimeout(cumulative.Interpret(), "separate Interpret slices in the same frame accumulate");

        var nextFrame = Create("RegressionTimeoutProbe();RegressionTimeoutProbe();Return 2;");
        steps.Enqueue(task => { Advance(task, .7); pending = task; return false; });
        Check(!nextFrame.Interpret().complete, "first simulated frame yields below the limit");
        var interpreter = InterpreterOf(nextFrame);
        typeof(Interpreter).GetField("executionFrame", Hidden).SetValue(interpreter, Time.frameCount - 1);
        pending.Complete();
        steps.Enqueue(task => { Advance(task, .7); return true; });
        result = nextFrame.Interpret();
        Check(result.complete && result.errors.Count == 0 && result.ret.num == 2, "a new frame resets elapsed time even if event lifetime exceeds one second");

        var gap = Create("RegressionTimeoutProbe();Return 3;");
        steps.Enqueue(task => { pending = task; return false; });
        Check(!gap.Interpret().complete, "external-wait fixture suspends");
        Thread.Sleep(1050); pending.Complete();
        result = gap.Interpret();
        Check(result.complete && result.errors.Count == 0 && result.ret.num == 3, "time between Interpret calls is excluded, even within one native frame");
        for (int eventIndex = 0; eventIndex < 2; eventIndex++)
        {
            steps.Enqueue(task => { Advance(task, .7); return true; });
            result = Create("RegressionTimeoutProbe();Return 4;").Interpret();
            Check(result.complete && result.errors.Count == 0, "independent events never share execution time");
        }
        var large = Create("for(i=0;i<2000;i++){value=i;}Return value;");
        result = large.Interpret();
        Check(!result.complete && result.errors.Count == 0, "4096-instruction budget still yields instead of reporting a time error");
        Check(steps.Count == 0, "all probe commands executed exactly once");
    }

    private static void SchedulerErrorAndCleanup()
    {
        var game = Manager<GameManager>();
        game.curScene = SceneForm.defaultData;
        var progress = ProgressForm.defaultData; progress.uid = 1; ProgressForm.AddData(progress);
        events = new GameEventController(game); game.evtCtrl = events;
        var ui = Manager<UiManager>();
        var root = new GameObject("timeout-notify", typeof(RectTransform), typeof(UiHolder));
        var holder = root.GetComponent<UiHolder>();
        var capture = new CaptureNotify { uiHolder = holder }; holder.ctrl = capture;
        ui.uiCtrlName2Uis[typeof(UiNotifyCtrl).Name] = new List<UiHolder> { holder };
        int polls = 0, cancels = 0;
        steps.Enqueue(task =>
        {
            events.StartTask(task, () => { polls++; return false; }, () => cancels++);
            Advance(task, 1.01);
            return false;
        });
        var program = new EventProgramDataForm.Data(98872, "TimeoutScheduler", "", new List<string>(), new List<int>(), 0, "void", 0);
        Check(program.TryApplyCode("GamePause();\nRegressionTimeoutProbe();\nafter=1;", out _, out var compileErrors), "scheduler source compiles: " + string.Join(";", compileErrors));
        var data = events.Execute(program, 0, null);
        events.LateUpdate();
        Check(!EventInterpretDataForm.DataByUid.ContainsKey(data.uid) && !data.heap.ContainsKey("after"), "scheduler removes the timed-out event without running its continuation");
        Check(progress.blockProgramUid == 0 && cancels == 1 && polls == 1, "timeout releases Pause and cancels owned tasks before further polls");
        Check(((ICollection)typeof(GameEventController).GetField("tasks", Hidden).GetValue(events)).Count == 0, "no timeout-owned task survives cleanup");
        Check(capture.captured != null && capture.captured.title == "运行时错误" && capture.captured.content.Contains("TimeoutScheduler")
            && capture.captured.content.Contains("行2列") && capture.captured.content.Contains("RegressionTimeoutProbe();")
            && capture.captured.content.Contains("单帧执行超时"), "existing error-popup packet contains program, source location, source and elapsed-time error");
    }

    private static void Advance(InterpretAsyncTask task, double seconds)
    {
        // Deterministic boundary tests adjust only this real budget's accumulated
        // ticks; one separate test above exercises the actual monotonic clock.
        object budget = typeof(Interpreter).GetField("activeBudget", Hidden).GetValue(task.interpreter);
        var field = budget.GetType().GetField("priorExecutionTicks", Hidden);
        field.SetValue(budget, (long)field.GetValue(budget) + (long)(seconds * Stopwatch.Frequency));
    }
    private static Interpreter InterpreterOf(InterpretDataForm.Data data) => (Interpreter)typeof(InterpretDataForm.Data).GetField("_interpreter", Hidden).GetValue(data);
    private static ProgramDataForm.Data Program(string code, string name)
    {
        var program = new ProgramDataForm.Data(98900 + ++sequence, name, "", new List<string>(), new List<int>(), 0, "void");
        Check(program.TryApplyCode(code, out _, out var errors), "source compiles: " + string.Join(";", errors));
        return program;
    }
    private static InterpretDataForm.Data Create(string code) => new InterpretDataForm.Data(-1, new List<BoxDataForm.Data>(),
        new Dictionary<string, BoxDataForm.Data>(), Program(code, "TimeoutCase" + sequence), 0, -1, 0, null, new List<BoxDataForm.Data>(), 0);
    private static void CheckTimeout(InterpretDataForm.RetInfo result, string message) => Check(result.complete && result.errors.Count == 1 && result.errors[0].Message.Contains("单帧执行超时"), message);
    private static void Register(Type type)
    {
        if (!forms.Add(type)) return;
        var parent = type.GetNestedType("Data")?.BaseType?.DeclaringType; if (parent != null) Register(parent);
        type.GetMethod("Register", StaticHidden)?.Invoke(null, null);
        type.GetMethod("InitInternal", BindingFlags.Public | BindingFlags.Static)?.Invoke(null, null);
    }
    private static T Manager<T>() where T : Z_MonoSingleton<T>
    {
        var root = new GameObject("timeout-" + typeof(T).Name); root.SetActive(false);
        var manager = root.AddComponent<T>(); typeof(Z_MonoSingleton<T>).GetField("_instance", StaticHidden).SetValue(null, manager); return manager;
    }
    private static void Check(bool condition, string message) { if (!condition) throw new InvalidOperationException(message); checks++; }
}
