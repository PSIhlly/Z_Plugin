param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
$controller = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/MyScripts/Mod/Ui/Scene/UiModSceneBehaviourUnitCtrl.cs') -Raw
$prefab = Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/GameSample/Prefabs/Ui/standard/Module/ModSceneBehaviourUnit.prefab') -Raw
if ($prefab -notmatch 'propertyPath: m_Name\r?\n\s+value: btn_delete\r?\n') { throw 'The authored prefab must contain btn_delete.' }
# Compile the actual partial controller against isolated UI/map/history spies.
# No production Forms, map, scene, prefab or save are loaded or mutated.
$controller = [regex]::Replace($controller, '(?m)^using [^\r\n]+;\r?\n', '')
$fixture = @'
using System;
using System.Collections.Generic;
using UnityEngine;
using Z_Ui.Base;
using Z_Map;
using Z_Map.Form;
using Z_UnitSystem.Form;

namespace UnityEngine
{
    public class GameObject
    {
        public Btn[] buttons;
        public bool includeInactive;
        public T[] GetComponentsInChildren<T>(bool include) { includeInactive = include; return (T[])(object)buttons; }
    }
    public static class Debug { public static int errors; public static void LogError(string text, object context) { errors++; } }
}
namespace Z_Ui.Base
{
    public class Btn
    {
        public string name;
        public ClickEvent onClick = new ClickEvent();
        public class ClickEvent
        {
            public List<Action> listeners = new List<Action>();
            public void AddListener(Action action) { listeners.Add(action); }
            public void Invoke() { foreach (var action in listeners) action(); }
        }
    }
    public class Txt { public string text; }
    public class UiCtrl
    {
        public GameObject gameObject;
        public int closeCount;
        public virtual void OnCreate() { }
        public virtual void OnShow() { }
        public void Close() { closeCount++; Probe.trace.Add("close"); }
    }
}
namespace Z_UnitSystem.Form { public class UnitForm { public class Data { public object unit; public string name; } } }
namespace Z_Map { public class MapUnit { } }
namespace Z_Map.Form
{
    public class ItemUnitForm { public class Data : UnitForm.Data { } }
    public class ObjectUnitForm { public class Data : UnitForm.Data { } }
    public class CharacterUnitForm { public class Data : UnitForm.Data { } }
    public class TileUnitForm { public class Data : UnitForm.Data { } }
}
public class ModManager
{
    public static ModManager instance = new ModManager();
    public SceneController sceneCtrl = new SceneController();
}
public class SceneController
{
    public UnitForm.Data tracked;
    public IDisposable BeginOperation() { Probe.trace.Add("begin"); return new Scope(); }
    public void Track(UnitForm.Data data) { tracked = data; Probe.trace.Add("track"); }
    private class Scope : IDisposable { public void Dispose() { Probe.trace.Add("commit"); } }
}
public class MapManager
{
    public static MapManager instance = new MapManager();
    public UnitForm.Data removed;
    private void Remove(UnitForm.Data data, string type)
    {
        Probe.Check(ReferenceEquals(data, ModManager.instance.sceneCtrl.tracked), "track exact row before removal");
        removed = data; Probe.trace.Add(type);
    }
    public void RemoveItem(ItemUnitForm.Data data) { Remove(data, "item"); }
    public void RemoveObject(ObjectUnitForm.Data data) { Remove(data, "object"); }
    public void RemoveCharacter(CharacterUnitForm.Data data) { Remove(data, "character"); }
    public void RemoveTile(TileUnitForm.Data data) { Remove(data, "tile"); }
}
namespace Ui.ModSceneBehaviourUnit
{
    public class UiModSceneBehaviourUnitView
    {
        public Btn btn_bbg = new Btn(), btn_close = new Btn(), btn_evt = new Btn();
        public Txt txt_name = new Txt();
    }
    public partial class UiModSceneBehaviourUnitCtrl : UiCtrl
    {
        public UiModSceneBehaviourUnitView view = new UiModSceneBehaviourUnitView();
        public UiModSceneBehaviourUnitModel model = new UiModSceneBehaviourUnitModel();
        public UiModSceneBehaviourUnitParam param;
    }
}
public static class Probe
{
    public static List<string> trace = new List<string>();
    public static int checks;
    public static void Check(bool result, string text) { checks++; if (!result) throw new Exception(text); }
    public static int Run()
    {
        var rows = new UnitForm.Data[] { new ItemUnitForm.Data(), new ObjectUnitForm.Data(), new CharacterUnitForm.Data(), new TileUnitForm.Data() };
        var types = new[] { "item", "object", "character", "tile" };
        for (int i = 0; i < rows.Length; i++)
        {
            trace.Clear();
            var button = new Btn { name = "btn_delete" };
            var unrelated = new Btn { name = "btn_deleteOther" };
            var ctrl = new Ui.ModSceneBehaviourUnit.UiModSceneBehaviourUnitCtrl { gameObject = new GameObject { buttons = new[] { unrelated, button } } };
            ctrl.OnCreate();
            Check(ctrl.gameObject.includeInactive, "bind inactive children too");
            Check(button.onClick.listeners.Count == 1 && unrelated.onClick.listeners.Count == 0, "bind only the exact delete button once");
            rows[i].unit = new MapUnit(); rows[i].name = types[i];
            ctrl.param = new Ui.ModSceneBehaviourUnit.UiModSceneBehaviourUnitParam { data = rows[i] };
            ctrl.OnShow(); ctrl.OnShow();
            Check(button.onClick.listeners.Count == 1 && ctrl.view.txt_name.text == types[i], "reopening refreshes without duplicate binding");
            button.onClick.Invoke();
            Check(ReferenceEquals(MapManager.instance.removed, rows[i]), "remove selected row only");
            Check(string.Join(",", trace) == "begin,track," + types[i] + ",commit,close", "single transaction, track before remove, close after commit");
            Check(ctrl.model.data == null, "clear removed selection");
            trace.Clear(); button.onClick.Invoke();
            Check(string.Join(",", trace) == "close", "repeated click cannot remove or track again");
            ctrl.param = null; ctrl.OnShow();
            ctrl.param = new Ui.ModSceneBehaviourUnit.UiModSceneBehaviourUnitParam { data = new UnitForm.Data() }; ctrl.OnShow();
            Check(ctrl.closeCount == 4, "null or invalid selection closes safely");
        }
        var missing = new Ui.ModSceneBehaviourUnit.UiModSceneBehaviourUnitCtrl { gameObject = new GameObject { buttons = new Btn[0] } };
        missing.OnCreate();
        Check(Debug.errors == 1, "missing button reports configuration error");
        missing.view.btn_close.onClick.Invoke();
        Check(missing.closeCount == 1, "existing close button still works");
        return checks;
    }
}
'@
Add-Type -TypeDefinition ($fixture + "`n" + $controller)
try { $checks = [Probe]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "MOD_SCENE_BEHAVIOUR_DELETE_REGRESSION_PASS checks=$checks"
