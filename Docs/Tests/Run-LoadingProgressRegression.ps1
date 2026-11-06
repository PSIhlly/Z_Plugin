param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level3/Z_UI/Core/Module/Loading/LoadingManager.cs') -Raw
$start = $source.IndexOf('    public enum LoadingState')
$types = $source.Substring($start, $source.LastIndexOf('}') - $start)
$ui = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main/Ui/UiLoadingCtrl.cs') -Raw
$ui = ($ui -split "`n" | Where-Object { $_ -notmatch '^using ' }) -join "`n"
$language = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level2/Z_Text/Core/LanguageManager.cs') -Raw
$language = ($language -split "`n" | Where-Object { $_ -notmatch '^using ' }) -join "`n"
$texts = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/Forms/ExcelCs/CommonTextForm.cs') -Raw
$languageEntries = ([regex]::Matches($texts, '\{\d+,new Data\(\d+,("(?:\\.|[^"\\])*"),("(?:\\.|[^"\\])*"),("(?:\\.|[^"\\])*")\)\}') | ForEach-Object {
    '{' + $_.Groups[1].Value + ',new TextEntry {contentEn=' + $_.Groups[2].Value + ',contentCn=' + $_.Groups[3].Value + '}},'
}) -join "`n"
$play = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Play/PlayManager.cs') -Raw
$playStart = $play.IndexOf('public async void BeginStory(')
$playOpen = $play.IndexOf('{', $playStart)
$depth = 1
for ($i = $playOpen + 1; $i -lt $play.Length; $i++) {
    if ($play[$i] -eq '{') { $depth++ }
    elseif ($play[$i] -eq '}') { $depth-- }
    if ($depth -eq 0) { $storyBody = $play.Substring($playOpen + 1, $i - $playOpen - 1); break }
}
# Execute the production progress accounting, frame gate and UI text
# controller with only their platform dependencies replaced. No game data is used.
$fixture = @'
using System;
using System.Collections.Generic;
using System.IO;
using System.Reflection;
using System.Threading;
using System.Threading.Tasks;
using Ui.Loading;
using Z_Ui.Loading;
using Z_Language;
using Z_Text.Form;
public static class LoadingProgressRegression
{
    private static int checks;
    private static LoadingManager manager;
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
    private static void Reset()
    {
        manager = new LoadingManager(); LoadingManager.instance = manager; manager.Init();
        UiManager.instance = new UiManager(); TimeManager.instance = new TimeManager();
    }
    private static void Draw() { TimeManager.instance.EndFrame(); TimeManager.instance.NextUpdate(); }
    private static void Near(float value, float expected, string message) { Check(Math.Abs(value - expected) < .0001, message); }
    public static int Run()
    {
            Reset(); manager.AddLoadItem("guard");
            Check(!manager.hasProgress && UiManager.instance.ctrl.view.txt_progress.text == "…", "visibility-only work is indeterminate");
            manager.AddLoadItem("child", 4); manager.Advance("child");
            Near(manager.progress, .25f, "guard does not dilute completed/total work");
            UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_progress.text == "25%", "UI derives its percentage from completed units");
            manager.AddLoadItem("child", 100);
            Near(manager.progress, .25f, "duplicate Add is idempotent and preserves the existing plan");
            manager.Advance("child", 3); manager.RemoveLoadItem("child");
            Near(manager.progress, 1f, "removing a completed child retains completed work under a guard");
            Check(UiManager.instance.visible, "completed work does not close its owner's loading UI");
            manager.AddLoadItem("next", 4);
            Near(manager.progress, .5f, "new child work extends the same session denominator");
            UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_progress.text == "50%", "unpainted completion does not force a jump to 99%");
            manager.AddWork("next", 8);
            Near(manager.progress, .25f, "discovered work changes the exact raw ratio");
            UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_progress.text == "50%", "display does not move backward when more work is discovered");
            manager.Advance("next", 12); UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_progress.text == "99%", "active lifecycle ownership cannot display completed loading");
            manager.RemoveLoadItem("next"); manager.RemoveLoadItem("guard");
            Check(!UiManager.instance.visible && UiManager.instance.closes == 1, "only the last owner closes the UI");
            int oldSession = manager.sessionId;
            manager.AddLoadItem("new", 2);
            Check(manager.sessionId == oldSession + 1 && UiManager.instance.ctrl.view.txt_progress.text == "0%", "new session resets counts and the display");
            manager.Advance("child", 100);
            Near(manager.progress, 0f, "stale reports from a removed child cannot change another session");
            manager.Advance("new", 100);
            Near(manager.progress, 1f, "over-reporting cannot exceed the declared work");
            manager.RemoveLoadItem("new"); manager.RemoveLoadItem("new");
            Check(UiManager.instance.closes == 2, "duplicate Remove cannot close twice");

            Reset(); manager.AddLoadItem("steps", 3);
            bool called = false;
            manager.RunStep("steps", "sync", () => called = true);
            Near(manager.progress, 1f / 3f, "a successful synchronous stage completes one unit");
            Check(called, "RunStep executes the real stage");
            var gate = new TaskCompletionSource<int>();
            var pending = manager.RunStepAsync("steps", "async", () => gate.Task);
            Near(manager.progress, 1f / 3f, "an unfinished worker contributes no completion");
            gate.SetResult(7);
            Check(pending.GetAwaiter().GetResult() == 7, "asynchronous stage preserves its result");
            Near(manager.progress, 2f / 3f, "asynchronous completion advances exactly once");
            try { manager.RunStep("steps", "failure", () => { throw new IOException("expected stage failure"); }); }
            catch (IOException) { checks++; }
            Near(manager.progress, 2f / 3f, "failure does not fabricate completed work");
            manager.RemoveLoadItem("steps");

            Reset(); manager.AddLoadItem("detailed", 2);
            Action<string, long, long> reportStep = null;
            var detailGate = new TaskCompletionSource<int>();
            var detailed = manager.RunStepAsync("detailed", "package", report => { reportStep = report; return detailGate.Task; });
            reportStep("files", 0, 4); reportStep("rows", 0, 10);
            reportStep("files", 2, 4);
            Near(manager.progress, .125f, "real substage counts fill a fraction of one declared unit");
            reportStep("files", 4, 4); reportStep("rows", 5, 10);
            Near(manager.progress, .375f, "bytes/rows are normalized per declared substage without mixing their units");
            detailGate.SetResult(9);
            Check(detailed.GetAwaiter().GetResult() == 9, "detailed async stages preserve results");
            Near(manager.progress, .5f, "completion does not count a detailed unit twice");
            reportStep("rows", 10, 10);
            Near(manager.progress, .5f, "late detailed callbacks cannot mutate a completed phase");
            manager.RemoveLoadItem("detailed");
            manager.AddLoadItem("detailed", 2); reportStep("rows", 10, 10);
            Near(manager.progress, 0f, "late callbacks cannot contaminate a new session reusing the same item name");
            manager.RemoveLoadItem("detailed");

            Reset(); manager.AddLoadItem("chunks", 1); int actions = 0;
            var chunked = manager.RunStepsAsync("chunks", "resources", new Action[] {
                () => { actions++; Thread.Sleep(12); }, () => { actions++; Thread.Sleep(12); }, () => actions++ });
            Check(actions == 0 && !chunked.IsCompleted, "resource batches render before starting heavy work");
            Draw(); Check(actions == 1 && !chunked.IsCompleted, "resource budget yields after the first expensive resource");
            Near(manager.progress, 1f / 3f, "resource count advances before the next render");
            Draw(); Check(actions == 2 && !chunked.IsCompleted, "resource budget permits another render between expensive resources");
            Draw(); chunked.GetAwaiter().GetResult();
            Check(actions == 3, "chunking preserves every resource and its order");
            Near(manager.progress, 1f, "chunked stage completes exactly one declared unit");
            manager.RemoveLoadItem("chunks");

            Reset(); manager.AddLoadItem("title", 2);
            LanguageManager.instance.SetLanguage(Language.Cn);
            manager.SetPhase("title", "Prepare prefab pool"); UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_title.text == "正在准备场景资源…", "title uses the generated Chinese language entry");
            manager.SetPhase("title", "Prepare WangTile cache"); UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_title.text == "正在生成拼接贴图…", "title changes even when progress percentage is unchanged");
            LanguageManager.instance.SetLanguage(Language.En); UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_title.text == "Generating tile textures…", "title responds to a language change without changing phase");
            manager.SetPhase("title", "Scene id=2 count=5", "Preparing scene"); UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_title.text == "Preparing scene…", "diagnostic IDs/counts do not leak into the title");
            manager.AddLoadItem("reserved", 5);
            Check(manager.currentPhase == "Preparing scene", "reserved future work does not replace an active title");
            manager.RemoveLoadItem("reserved");
            var titleGate = new TaskCompletionSource<int>();
            var titleTask = manager.RunStepAsync("title", "Build package", report => {
                report("Package source bytes", 0, 0); report("Encode PNG rows", 0, 0);
                Check(manager.currentPhase == "Build package", "declared future substages do not become current titles");
                report("Package source bytes", 0, 200);
                Check(manager.currentPhase == "Package source bytes", "an actual worker substage becomes the current title");
                return titleGate.Task;
            });
            UiManager.instance.ctrl.OnUpdate();
            Check(UiManager.instance.ctrl.view.txt_title.text == "Compressing story files…", "worker details are localized on the UI thread");
            titleGate.SetResult(1); titleTask.GetAwaiter().GetResult(); manager.RemoveLoadItem("title");

            Reset(); manager.AddLoadItem("worker", 1000);
            Parallel.For(0, 1000, _ => manager.Advance("worker"));
            Near(manager.progress, 1f, "parallel worker increments are not lost");
            manager.RemoveLoadItem("worker");

            Reset(); called = false;
            var runGate = new TaskCompletionSource<bool>();
            Task operation = manager.RunAsync("operation", () => { called = true; return runGate.Task; });
            Check(!called && !operation.IsCompleted && UiManager.instance.visible, "Loading precedes blocking work");
            TimeManager.instance.EndFrame();
            Check(!called, "frame-end alone does not start work before rendering finishes");
            TimeManager.instance.NextUpdate();
            Check(called && !operation.IsCompleted, "work starts after the next Update");
            runGate.SetResult(true); operation.GetAwaiter().GetResult();
            Check(!UiManager.instance.visible, "async action retains ownership until completion");
            Reset(); called = false;
            using (var cancel = new CancellationTokenSource())
            {
                operation = manager.RunAsync("cancel", () => { called = true; return Task.CompletedTask; }, cancel.Token);
                cancel.Cancel();
                try { operation.GetAwaiter().GetResult(); throw new Exception("expected cancellation"); }
                catch (OperationCanceledException) { checks++; }
                Draw(); Check(!called && !UiManager.instance.visible, "cancel before rendering releases ownership without running work");
            }
            StoryEntry();
            return checks;
    }
    private static void StoryEntry()
    {
        foreach (int mode in new[] { 0, 1, 2 })
        {
            Reset(); PlayStoryFixture.trace.Clear(); SaveAndLoad.hasSave = mode == 1;
            GameManager.instance = new GameManager(); Main2StoryManager.sceneGate = new TaskCompletionSource<bool>();
            var story = new PlayStoryFixture();
            var task = story.BeginStory(7, mode == 2);
            Check(!task.IsCompleted && UiManager.instance.visible && !story.enable && PlayStoryFixture.trace.Count == 0,
                "all Story entry branches paint Loading before reading or initializing Story data");
            Draw();
            string expected = mode == 0 ? "core,init,objects,save,load,objects,assets,scene"
                : mode == 1 ? "load,objects,assets,scene" : "core,init,objects,assets,scene";
            Check(string.Join(",", PlayStoryFixture.trace) == expected && !task.IsCompleted && UiManager.instance.visible,
                "new/saved/test Story branches preserve initialization order and retain loading through the map wait");
            Main2StoryManager.sceneGate.SetResult(true); task.GetAwaiter().GetResult();
            Check(UiManager.instance.visible && GameManager.instance.curProgress.pos == 107,
                "position is applied before completion and the presentation item retains Loading");
            manager.Advance("playSceneStart", 6); manager.RemoveLoadItem("playSceneStart");
            Check(!UiManager.instance.visible && UiManager.instance.closes == 1, "Story-to-scene handoff does not flicker closed");
        }
        Reset(); PlayStoryFixture.trace.Clear(); GameManager.instance = new GameManager();
        GameManager.instance.saveCtrl.fail = true;
        Task failure = new PlayStoryFixture().BeginStory(7, true); Draw();
        try { failure.GetAwaiter().GetResult(); throw new Exception("expected Story failure"); }
        catch (IOException) { checks++; }
        Check(!UiManager.instance.visible && PlayStoryFixture.trace.Count == 0,
            "failed Story initialization releases Loading without beginning scene/player setup");
    }
}
public class PlayStoryFixture
{
    private const string ScenePresentationLoadItem = "playSceneStart";
    private static readonly string[] ScenePresentationPhases = new string[6];
    public static List<string> trace = new List<string>();
    public string _folderName; public bool boxPlay, enable = true;
    public Assets _assetCtrl = new Assets();
    public class Assets { public void Begin() { trace.Add("assets"); } }
    public string GetStorySaveFolder() { return "Save"; }
    public string GetStoryCacheFolder() { return "Cache"; }
    public static void FirstPlayInit() { trace.Add("init"); }
    public async Task BeginStory(int id, bool boxPlay) { __BEGIN_STORY__ }
}
public static class SaveAndLoad { public static bool hasSave; public static bool Exist(string path) { return hasSave; } }
public static class ObjectProductForm { public static void PreparePlayStory(string path) { PlayStoryFixture.trace.Add("objects"); } }
public class GameManager
{
    public static GameManager instance = new GameManager(); public Save saveCtrl = new Save(); public Progress curProgress = new Progress();
    public static int PlayerPosToMapPos(int value) { return value; }
    public class Progress { public int sceneId, pos; public (int, int) targetScene = (2, 7); }
    public class Save
    {
        public bool fail;
        public void LoadCoreStory(int id) { if (fail) throw new IOException("failed Story"); PlayStoryFixture.trace.Add("core"); }
        public void LoadSaveStory(int id) { PlayStoryFixture.trace.Add("load"); }
        public void SaveSaveStory(int id) { PlayStoryFixture.trace.Add("save"); }
    }
}
public class Main2StoryManager
{
    public const int SceneLoadingWorkCount = 5;
    public static Main2StoryManager instance = new Main2StoryManager(); public static TaskCompletionSource<bool> sceneGate;
    public static string GetStoryFolderNameById(int id) { return id.ToString(); }
    public async Task LoadScenePlayAsync(int id, Action callback)
    {
        PlayStoryFixture.trace.Add("scene"); await sceneGate.Task; callback();
        LoadingManager.instance.Advance("scene", 5); LoadingManager.instance.RemoveLoadItem("scene");
    }
}
public class MapManager
{
    public static MapManager instance = new MapManager(); public Util utilCtrl = new Util();
    public class Util { public int MapPos2RealPos(int value) { return value + 100; } }
}
public class Z_Event { }
public class Z_Manager<T> where T : new() { public static T instance = new T(); public virtual void Init() { } }
public static class Mathf
{
    public static int RoundToInt(float value) { return (int)Math.Round(value); }
    public static int Min(int a, int b) { return Math.Min(a, b); }
    public static int Max(int a, int b) { return Math.Max(a, b); }
}
public class TimeManager
{
    public static TimeManager instance = new TimeManager();
    private readonly Queue<Action> ends = new Queue<Action>(), updates = new Queue<Action>();
    public void AddCurFrameEndWithoutCheckAction(Action action) { ends.Enqueue(action); }
    public void AddNextUpdateWithoutCheckAction(Action action) { updates.Enqueue(action); }
    public void EndFrame() { int count = ends.Count; while (count-- > 0) ends.Dequeue()(); }
    public void NextUpdate() { int count = updates.Count; while (count-- > 0) updates.Dequeue()(); }
    public void StartTimer(float duration, int repeat, Func<bool> callback) { }
}
public class UiManager
{
    public static UiManager instance = new UiManager();
    public UiLoadingCtrl ctrl = new UiLoadingCtrl(); public bool visible; public int closes;
    public void ShowUi<T>() { visible = true; ctrl.OnShow(); }
    public void CloseUi<T>() { visible = false; closes++; ctrl.Close(); }
}
public class UiCtrl
{
    public virtual void OnCreate() { }
    public virtual void OnShow() { }
    public virtual void OnUpdate() { }
    public virtual void OnHide() { }
    public virtual void Close() { OnHide(); }
}
namespace Ui.Loading
{
    public partial class UiLoadingParam { }
    public partial class UiLoadingModel { }
    public class Text {
        public string text; public bool languageTranslatable = true;
        public string oriText { set { text = LanguageManager.instance.GetTxt(value); } }
    }
    public class View { public Text txt_progress = new Text(), txt_title = new Text(); }
    public partial class UiLoadingCtrl : UiCtrl
    {
        public UiLoadingParam param;
        public UiLoadingModel model = new UiLoadingModel();
        public View view = new View();
    }
}
namespace Z_Ui.Loading { __LOADING_TYPES__ }
namespace Z_Text.Form {
    public class TextEntry { public string contentEn, contentCn; }
    public static class TextBaseForm {
        public static Dictionary<string,TextEntry> DataByKey = new Dictionary<string,TextEntry> { __LANGUAGE_ENTRIES__ };
    }
}
public static class Debug { public static void LogError(string error) { throw new Exception(error); } }
__LANGUAGE_TYPES__
__UI_CONTROLLER__
'@
Add-Type -TypeDefinition $fixture.Replace('__LOADING_TYPES__', $types).Replace('__UI_CONTROLLER__', $ui).Replace('__BEGIN_STORY__', $storyBody).Replace('__LANGUAGE_TYPES__', $language).Replace('__LANGUAGE_ENTRIES__', $languageEntries)
try { $checks = [LoadingProgressRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "LOADING_PROGRESS_REGRESSION_PASS checks=$checks"
