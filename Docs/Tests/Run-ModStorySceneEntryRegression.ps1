param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$source = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Mod/Ui/Map/UiModStoryMapSceneUnitCtrl.cs') -Raw
$callback = [regex]::Match($source, '(?s)view\.btn_edit\.onClick\.AddListener\(\(\) =>\s*\{(?<body>.*?)\}\);')
if (-not $callback.Success) { throw 'Scene entry callback not found.' }
$assetSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Mod/ModAssetCtrl.cs') -Raw
$create = [regex]::Match($assetSource, '(?s)public void CreateScene\(.*?(?=public void ImportSceneMiniMap)')
if (-not $create.Success -or -not $create.Value.Contains('SceneForm.AddData(') -or
    $create.Value -match 'SaveCoreStory|SaveSceneMap|File\.|\.Init\(') {
    throw 'CreateScene must only register the row, without generating a map file.'
}
# Execute the actual button callback against save/load/UI spies only.
# No authored story, persistence directory, global Form or Unity scene is changed.
$fixture = @'
using System;
using System.Collections.Generic;
public class SceneEntryRegression
{
    public static List<string> Trace = new List<string>();
    public static bool FailSave;
    public SceneModel model = new SceneModel();
    public class SceneModel { public SceneData data = new SceneData(); }
    public class SceneData { public int uid = 2; }
    public void Click() { __ENTRY_CALLBACK__ }
    public static int Run()
    {
        var entry = new SceneEntryRegression();
        entry.Click();
        if (string.Join(",", Trace) != "save:1,load:2,close") throw new Exception("Save must finish before loading the selected Scene and closing ModStory.");
        Trace.Clear(); GameManager.instance.curStory.id = 7; entry.model.data.uid = 8;
        entry.Click();
        if (string.Join(",", Trace) != "save:7,load:8,close") throw new Exception("Save current Story and load selected Scene, not hardcoded IDs.");
        Trace.Clear(); FailSave = true;
        try { entry.Click(); throw new Exception("Expected save failure."); }
        catch (InvalidOperationException) { }
        if (string.Join(",", Trace) != "save:7") throw new Exception("A failed save must not start Scene load or close ModStory.");
        return 3;
    }
}
public class GameManager
{
    public static GameManager instance = new GameManager();
    public Story curStory = new Story(); public Saver saveCtrl = new Saver();
    public class Story { public int id = 1; }
    public class Saver { public void SaveCoreStory(int id) { SceneEntryRegression.Trace.Add("save:" + id); if (SceneEntryRegression.FailSave) throw new InvalidOperationException("save failed"); } }
}
public class Main2StoryManager { public static Main2StoryManager instance = new Main2StoryManager(); public void StartLoadSceneUgc(int id) { SceneEntryRegression.Trace.Add("load:" + id); } }
public class UiModStoryCtrl { }
public class UiManager { public static UiManager instance = new UiManager(); public void CloseUi<T>() { SceneEntryRegression.Trace.Add("close"); } }
'@
Add-Type -TypeDefinition $fixture.Replace('__ENTRY_CALLBACK__', $callback.Groups['body'].Value)
try { $checks = 1 + [SceneEntryRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "MOD_STORY_SCENE_ENTRY_REGRESSION_PASS checks=$checks"
