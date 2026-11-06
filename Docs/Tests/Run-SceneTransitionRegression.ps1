param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
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
$main = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main2Scene/Main2StoryManager.cs') -Raw
$play = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Play/PlaySceneController.cs') -Raw
if ((Get-BracedBody $play 'public void End()').Contains('SaveSceneMap(')) {
    throw 'PlaySceneController.End must not duplicate the save owned by UnloadScenePlay.'
}
$reset = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Main/GameSaveController.cs') -Raw
$resetBody = Get-BracedBody $reset 'public void ResetStory()'
if ($resetBody.IndexOf('DestroyWangTileTextures();') -lt 0 -or
    $resetBody.IndexOf('DestroyWangTileTextures();') -gt $resetBody.IndexOf('LoadObjectProducts(null);')) {
    throw 'Story reset must release generated WangTile assets before clearing their source Forms.'
}
$loading = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level3/Z_UI/Core/Module/Loading/LoadingManager.cs') -Raw
$loadingStart = $loading.IndexOf('    public enum LoadingState')
$loadingTypes = $loading.Substring($loadingStart, $loading.LastIndexOf('}') - $loadingStart)
# Run the actual transition/unload methods and LoadingManager with in-memory
# save/map/UI/frame spies. No production scene, save file or singleton is touched.
$fixture = @'
using System;
using System.Collections.Generic;
using System.Threading;
using System.Threading.Tasks;
public class Main2StoryManager
{
    private bool changingScene;
    public Task Change(int id) { return ChangeSceneAsync(id, null); }
    private async Task ChangeSceneAsync(int sceneId, Action oncomplete) { __CHANGE__ }
    private async Task LoadScenePlayAsync(int sceneId, Action onMapComplete) { __LOAD_PLAY__ }
    public void UnloadScenePlay() { __UNLOAD_PLAY__ }
    public void UnloadScene() { __UNLOAD__ }
    private async Task StartLoadScene(string folder, int id, Action callback)
    {
        Regression.Trace.Add("load:" + id);
        if (Regression.FailLoad) throw new InvalidOperationException("load failed");
        if (Regression.LoadGate != null) await Regression.LoadGate.Task;
        GameManager.instance.curScene = new Scene { uid = id };
        if (callback != null) callback();
    }
}
public static class Regression
{
    public static List<string> Trace = new List<string>();
    public static bool FailSave, FailLoad;
    public static TaskCompletionSource<bool> LoadGate;
    private static int checks;
    private static void Check(bool value, string message) { if (!value) throw new Exception(message); checks++; }
    private static void Reset()
    {
        Trace.Clear(); FailSave = FailLoad = false; LoadGate = null;
        LoadingManager.instance.Init(); PlayManager.instance.enable = true;
        GameManager.instance.curScene = new Scene { uid = 1 }; UiManager.instance.Visible = false;
    }
    private static void Render() { TimeManager.instance.EndFrame(); TimeManager.instance.NextUpdate(); }
    private static void ExpectFailure(Task task)
    {
        try { task.GetAwaiter().GetResult(); throw new Exception("expected failure"); }
        catch (InvalidOperationException) { checks++; }
    }
    public static int Run()
    {
        Reset();
        var manager = new Main2StoryManager();
        var task = manager.Change(2);
        Check(!task.IsCompleted && UiManager.instance.Visible && !PlayManager.instance.enable,
            "Loading is shown and Play updates stop before the first render wait");
        Check(Trace.Count == 2 && Trace[0] == "show" && Trace[1] == "show", "no save, teardown or load before rendering");
        Check(manager.Change(3).IsCompleted && Trace.Count == 2, "a pending transition cannot be started twice");
        TimeManager.instance.EndFrame();
        Check(Trace.Count == 2, "frame-end alone must not begin synchronous save work");
        TimeManager.instance.NextUpdate(); task.GetAwaiter().GetResult();
        Check(string.Join(",", Trace) == "show,show,save,map-reset,character-reset,end,load:2,begin:2,show",
            "save runs once before reset/end/load, and Begin retains a presentation loading item");
        Check(UiManager.instance.Visible && PlayManager.instance.enable, "loading survives until entry-event presentation completes");
        PlayManager.instance.Present();
        Check(!UiManager.instance.Visible, "presentation completion releases the last item");

        Reset(); LoadGate = new TaskCompletionSource<bool>(); task = manager.Change(4); Render();
        Check(!task.IsCompleted && UiManager.instance.Visible && !PlayManager.instance.enable,
            "Loading and the transition guard survive asynchronous map loading");
        Check(manager.Change(5).IsCompleted && !Trace.Contains("load:5"), "no reentry while waiting for map data");
        LoadGate.SetResult(true); task.GetAwaiter().GetResult(); PlayManager.instance.Present();
        Check(Trace.Contains("begin:4") && !UiManager.instance.Visible, "async load completes under the same loading ownership");

        Reset(); FailSave = true; task = manager.Change(6); Render(); ExpectFailure(task);
        Check(!UiManager.instance.Visible && !Trace.Contains("end") && !Trace.Contains("load:6"),
            "save failure releases Loading without resetting or loading another map");
        Reset(); FailLoad = true; task = manager.Change(7); Render(); ExpectFailure(task);
        Check(!UiManager.instance.Visible && !Trace.Contains("begin:7"), "map-load failure releases Loading without BeginScene");
        Reset(); task = manager.Change(8); Render(); task.GetAwaiter().GetResult();
        Check(Trace.Contains("begin:8"), "failure releases the transition guard for another request");
        PlayManager.instance.Present();
        return checks;
    }
}
public class Scene { public int uid; }
public class GameManager
{
    public static GameManager instance = new GameManager();
    public Scene curScene; public Save saveCtrl = new Save();
    public Map mapCtrl = new Map(); public Character characterCtrl = new Character();
}
public class Save
{
    public void SaveSceneMap(string path)
    {
        Regression.Trace.Add("save");
        if (Regression.FailSave) throw new InvalidOperationException("save failed");
    }
}
public class Map { public void Reset() { Regression.Trace.Add("map-reset"); } }
public class Character { public void Reset() { Regression.Trace.Add("character-reset"); } }
public class PlayManager
{
    public static PlayManager instance = new PlayManager(); public bool enable;
    public string GetStoryCacheFolder() { return "memory"; }
    public string GetSceneCacheFileName() { return "old"; }
    public void EndScene() { Regression.Trace.Add("end"); }
    public void BeginScene(int id)
    {
        Regression.Trace.Add("begin:" + id); enable = true; LoadingManager.instance.AddLoadItem("presentation");
    }
    public void Present() { LoadingManager.instance.RemoveLoadItem("presentation"); }
}
public static class DynamicGlobalSettings { public static bool playing; }
public class UiLoadingCtrl { }
public class UiManager
{
    public static UiManager instance = new UiManager(); public bool Visible;
    public void ShowUi<T>() { Visible = true; Regression.Trace.Add("show"); }
    public void CloseUi<T>() { Visible = false; Regression.Trace.Add("close"); }
}
public class Z_Event { }
public static class Mathf { public static float Clamp01(float value) { return Math.Max(0, Math.Min(1, value)); } }
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
$fixture = $fixture.Replace('__CHANGE__', (Get-BracedBody $main 'private async Task ChangeSceneAsync(')).
    Replace('__LOAD_PLAY__', (Get-BracedBody $main 'internal async Task LoadScenePlayAsync(')).
    Replace('__UNLOAD_PLAY__', (Get-BracedBody $main 'public void UnloadScenePlay()')).
    Replace('__UNLOAD__', (Get-BracedBody $main 'public void UnloadScene()')).
    Replace('__LOADING_TYPES__', $loadingTypes)
Add-Type -TypeDefinition $fixture
try { $checks = 2 + [Regression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "SCENE_TRANSITION_REGRESSION_PASS checks=$checks"
