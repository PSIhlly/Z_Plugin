param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Mod/Ui/Map/UiModStoryMapSceneUnitCtrl.cs') -Raw
function Get-BracedBody([string]$text, [string]$anchor) {
    $start = $text.IndexOf($anchor)
    if ($start -lt 0) { throw "Source anchor not found: $anchor" }
    $open = $text.IndexOf('{', $start)
    $depth = 1
    for ($i = $open + 1; $i -lt $text.Length; $i++) {
        if ($text[$i] -eq '{') { $depth++ }
        elseif ($text[$i] -eq '}') { $depth-- }
        if ($depth -eq 0) { return $text.Substring($open + 1, $i - $open - 1) }
    }
    throw "Unclosed source body: $anchor"
}
$callback = Get-BracedBody $source 'view.btn_edit.onClick.AddListener(async () =>'
$assetSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Mod/ModAssetCtrl.cs') -Raw
$create = [regex]::Match($assetSource, '(?s)public void CreateScene\(.*?(?=public void ImportSceneMiniMap)')
if (-not $create.Success -or -not $create.Value.Contains('SceneForm.AddData(') -or
    $create.Value -match 'SaveCoreStory|SaveSceneMap|File\.|\.Init\(') {
    throw 'CreateScene must only register the row, without generating a map file.'
}
$mainSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main2Scene/Main2StoryManager.cs') -Raw
if (-not $mainSource.Contains('public async Task StartLoadSceneUgc(int sceneId)')) {
    throw 'UGC scene entry must return an awaitable Task through BeginScene.'
}
$sharedLoad = Get-BracedBody $mainSource 'private async Task<bool> StartLoadScene('
if ($sharedLoad -notmatch '(?s)finally\s*\{\s*LoadingManager\.instance\.RemoveLoadItem\("scene"\);' -or
    $sharedLoad.IndexOf('onMapComplete?.Invoke();') -gt $sharedLoad.IndexOf('RemoveLoadItem("scene")')) {
    throw 'The shared scene load must retain its item through map setup and release it in finally.'
}
$loadingSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level3/Z_UI/Core/Module/Loading/LoadingManager.cs') -Raw
$loadingStart = $loadingSource.IndexOf('    public enum LoadingState')
if ($loadingStart -lt 0) { throw 'Loading manager source not found.' }
$loadingTypes = $loadingSource.Substring($loadingStart, $loadingSource.LastIndexOf('}') - $loadingStart)
# Execute the actual callback and LoadingManager with frame/save/load/UI spies.
# No authored story, persistence directory, global Form or Unity scene is changed.
$fixture = @'
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
public class SceneEntryRegression
{
    public static List<string> Trace = new List<string>();
    public static bool FailSave, FailLoad;
    public static TaskCompletionSource<bool> LoadGate;
    private static int checks;
    public SceneModel model = new SceneModel();
    public class SceneModel { public SceneData data = new SceneData(); public bool startingScene; }
    public class SceneData { public int uid = 2; }
    public async Task Click() { __ENTRY_CALLBACK__ }
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
    private static void Reset()
    {
        Trace.Clear(); FailSave = FailLoad = false; LoadGate = null; Debug.Errors = 0;
        UiManager.instance.LoadingVisible = false; UiManager.instance.StoryCloses = 0; UiManager.instance.Shows = 0;
        LoadingManager.instance.Init();
    }
    private static void Draw() { TimeManager.instance.EndFrame(); TimeManager.instance.NextUpdate(); }
    public static int Run()
    {
        Reset();
        var entry = new SceneEntryRegression();
        var task = entry.Click();
        Check(UiManager.instance.LoadingVisible && entry.model.startingScene && !task.IsCompleted,
            "Loading must show immediately before synchronous save/load work.");
        Check(string.Join(",", Trace) == "loading:show", "Save must not run before loading renders.");
        entry.Click().GetAwaiter().GetResult();
        Check(UiManager.instance.Shows == 1, "Repeated entry clicks must not start another operation.");
        GameManager.instance.curStory.id = 7; entry.model.data.uid = 8;
        TimeManager.instance.EndFrame();
        Check(string.Join(",", Trace) == "loading:show", "Frame end alone must not start blocking save work.");
        TimeManager.instance.NextUpdate(); task.GetAwaiter().GetResult();
        Check(string.Join(",", Trace) == "loading:show,save:1,load:2,loading:show,begin:2,story:close,loading:close",
            "Capture IDs at click and retain loading through save, load, BeginScene and Story closure.");
        Check(!entry.model.startingScene && !UiManager.instance.LoadingVisible, "Success releases the guard and loading item.");

        Reset(); task = entry.Click(); Draw(); task.GetAwaiter().GetResult();
        Check(Trace.Contains("save:7") && Trace.Contains("load:8") && Trace.Contains("begin:8"),
            "Subsequent entry uses current Story and Scene, not hardcoded IDs.");

        Reset(); FailSave = true; task = entry.Click(); Draw(); task.GetAwaiter().GetResult();
        Check(Trace.Contains("save:7") && !Trace.Contains("load:8") && UiManager.instance.StoryCloses == 0,
            "Save failure must not load a Scene or close ModStory.");
        Check(!entry.model.startingScene && !UiManager.instance.LoadingVisible && Debug.Errors == 1,
            "Save failure is observed and releases loading/entry guard.");

        Reset(); FailLoad = true; task = entry.Click(); Draw(); task.GetAwaiter().GetResult();
        Check(Trace.Contains("load:8") && !Trace.Contains("begin:8") && UiManager.instance.StoryCloses == 0,
            "Load failure keeps ModStory open without beginning an invalid scene.");
        Check(!UiManager.instance.LoadingVisible && !entry.model.startingScene && Debug.Errors == 1,
            "Nested load failure cannot strand loading or entry guard.");

        Reset(); LoadGate = new TaskCompletionSource<bool>(); task = entry.Click(); Draw();
        Check(UiManager.instance.LoadingVisible && entry.model.startingScene && !task.IsCompleted,
            "Loading stays visible for the entire asynchronous scene load.");
        Check(UiManager.instance.StoryCloses == 0 && !Trace.Contains("begin:8"), "Do not close ModStory before entry completes.");
        int shows = UiManager.instance.Shows; entry.Click().GetAwaiter().GetResult();
        Check(UiManager.instance.Shows == shows, "An in-flight load rejects duplicate clicks.");
        LoadGate.SetResult(true); task.GetAwaiter().GetResult();
        Check(!UiManager.instance.LoadingVisible && UiManager.instance.StoryCloses == 1 && !entry.model.startingScene,
            "Delayed success completes scene entry before removing the last loading item.");

        Reset(); LoadGate = new TaskCompletionSource<bool>(); task = entry.Click(); Draw();
        LoadGate.SetException(new InvalidOperationException("read failed")); task.GetAwaiter().GetResult();
        Check(!UiManager.instance.LoadingVisible && !entry.model.startingScene && UiManager.instance.StoryCloses == 0
            && Debug.Errors == 1, "Delayed exceptions are awaited, observed and cleaned up.");

        Reset(); LoadingManager.instance.AddLoadItem("external"); task = entry.Click(); Draw(); task.GetAwaiter().GetResult();
        Check(UiManager.instance.LoadingVisible, "Scene entry cannot close another operation's loading item.");
        LoadingManager.instance.RemoveLoadItem("external");
        Check(!UiManager.instance.LoadingVisible, "The final owner closes loading.");
        return checks;
    }
}
public class GameManager
{
    public static GameManager instance = new GameManager();
    public Story curStory = new Story(); public Saver saveCtrl = new Saver();
    public class Story { public int id = 1; }
    public class Saver { public void SaveCoreStory(int id) { SceneEntryRegression.Trace.Add("save:" + id); if (SceneEntryRegression.FailSave) throw new InvalidOperationException("save failed"); } }
}
public class Main2StoryManager
{
    public static Main2StoryManager instance = new Main2StoryManager();
    public async Task StartLoadSceneUgc(int id)
    {
        SceneEntryRegression.Trace.Add("load:" + id);
        LoadingManager.instance.AddLoadItem("scene");
        try
        {
            if (SceneEntryRegression.FailLoad) throw new InvalidOperationException("load failed");
            if (SceneEntryRegression.LoadGate != null) await SceneEntryRegression.LoadGate.Task;
            SceneEntryRegression.Trace.Add("begin:" + id);
        }
        finally { LoadingManager.instance.RemoveLoadItem("scene"); }
    }
}
public class UiModStoryCtrl { }
public class UiLoadingCtrl { }
public class UiManager
{
    public static UiManager instance = new UiManager();
    public bool LoadingVisible; public int StoryCloses, Shows;
    public void ShowUi<T>() { LoadingVisible = true; Shows++; SceneEntryRegression.Trace.Add("loading:show"); }
    public void CloseUi<T>()
    {
        if (typeof(T) == typeof(UiLoadingCtrl)) { LoadingVisible = false; SceneEntryRegression.Trace.Add("loading:close"); }
        else { StoryCloses++; SceneEntryRegression.Trace.Add("story:close"); }
    }
}
public static class Debug { public static int Errors; public static void LogException(Exception exception) { Errors++; } }
public class Z_Event { }
public class Z_Manager<T> where T : new() { public static T instance = new T(); public virtual void Init() { } }
public class TimeManager
{
    public static TimeManager instance = new TimeManager();
    private readonly Queue<Action> endFrame = new Queue<Action>(), nextUpdate = new Queue<Action>();
    public void AddCurFrameEndWithoutCheckAction(Action action) { endFrame.Enqueue(action); }
    public void AddNextUpdateWithoutCheckAction(Action action) { nextUpdate.Enqueue(action); }
    public void EndFrame() { int count = endFrame.Count; while (count-- > 0) endFrame.Dequeue()(); }
    public void NextUpdate() { int count = nextUpdate.Count; while (count-- > 0) nextUpdate.Dequeue()(); }
}
__LOADING_TYPES__
'@
Add-Type -TypeDefinition $fixture.Replace('__ENTRY_CALLBACK__', $callback).Replace('__LOADING_TYPES__', $loadingTypes).Replace(' = default)', ' = default(CancellationToken))')
try { $checks = 3 + [SceneEntryRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "MOD_STORY_SCENE_ENTRY_REGRESSION_PASS checks=$checks"
