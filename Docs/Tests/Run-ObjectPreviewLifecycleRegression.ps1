#Requires -Version 7.0
param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path
)

$ErrorActionPreference = 'Stop'
function Read-Source([string]$relative) {
    Get-Content -LiteralPath (Join-Path $ProjectRoot $relative) -Raw
}
function Source-Span([string]$source, [string]$startToken, [string]$endToken) {
    $start = $source.IndexOf($startToken)
    if ($start -lt 0) { throw "Source start missing: $startToken" }
    $end = $source.IndexOf($endToken, $start)
    if ($end -le $start) { throw "Source end missing: $endToken" }
    $source.Substring($start, $end - $start)
}

$managerSource = Read-Source 'Assets/GameSample/MyScripts/Util/DisplayCameraAreaManager.cs'
$managerSource = [regex]::Replace($managerSource, '(?m)^using [^\r\n]+;\r?\n', '')
$controllerSource = Read-Source 'Assets/GameSample/MyScripts/Mod/Ui/MapObject/UiModStoryMapObjectObjectAppearanceCtrl.cs'
$controllerMethods = Source-Span $controllerSource '        public override void OnShow()' '        public void Refresh()'
if (-not $controllerMethods.Contains('public override void OnHide()')) {
    throw 'Object appearance must release the preview in OnHide.'
}
$holderSource = Read-Source 'Assets/Z_Level3/Z_UI/Core/Basic/Frame/UiHolder.cs'
$holderDisable = Source-Span $holderSource '        protected void OnDisable()' '        protected void OnApplicationQuit()'
if (-not $holderDisable.Contains('ctrl.OnHide();')) {
    throw 'Hiding the UI holder must dispatch the controller OnHide lifecycle.'
}

# Compile the production manager and production Object OnShow/OnHide methods.
# Unity stubs model activation and deferred Destroy; no Unity scene or user data
# is opened. The fixture's Refresh only creates the preview object's stand-in.
$fixture = @'
using System;
using System.Collections.Generic;
using UnityEngine;
using Z_DesignStyle;
using Z_Map;

namespace UnityEngine {
    public class Object {
        private static readonly List<GameObject> pending = new List<GameObject>();
        public static void Destroy(GameObject value) {
            if (value == null || value.destroyed || value.destroyQueued) return;
            value.destroyQueued = true;
            pending.Add(value);
        }
        public static void EndFrame() {
            foreach (var value in pending) value.destroyed = true;
            pending.Clear();
        }
    }
    public struct Vector3 {
        public float x, y, z;
        public Vector3(float x, float y, float z) { this.x = x; this.y = y; this.z = z; }
        public static Vector3 zero { get { return new Vector3(); } }
        public static Vector3 operator +(Vector3 a, Vector3 b) {
            return new Vector3(a.x + b.x, a.y + b.y, a.z + b.z);
        }
    }
    public class Transform {
        public GameObject gameObject;
        public Transform parent;
        public Vector3 position, localPosition, eulerAngles;
    }
    public class GameObject : Object {
        public bool activeSelf = true, destroyQueued, destroyed;
        public readonly Transform transform;
        public GameObject() { transform = new Transform { gameObject = this }; }
        public bool activeInHierarchy {
            get { return !destroyed && activeSelf &&
                (transform.parent == null || transform.parent.gameObject.activeInHierarchy); }
        }
        public void SetActive(bool value) { activeSelf = value; }
    }
    public class MonoBehaviour : Object {
        public readonly GameObject gameObject = new GameObject();
        public Transform transform { get { return gameObject.transform; } }
    }
    public class Camera : MonoBehaviour {
        public float orthographicSize = 1.5f;
        public bool enabled = true;
        public bool rendering { get { return enabled && gameObject.activeInHierarchy; } }
    }
}
namespace Z_DesignStyle {
    public class Z_MonoManager<T> : MonoBehaviour where T : MonoBehaviour {
        public static T instance;
        public virtual void Init() { }
    }
}
namespace Z_Map { public enum CameraMode { Overhead, Isometric } }
public static class DynamicGlobalSettings { public static CameraMode cameraMode; }
public class UiCtrlFixture {
    public virtual void OnShow() { }
    public virtual void OnHide() { }
}
public class AppearanceDataFixture {
    public int ensureCalls;
    public void EnsureDirectionData() { ensureCalls++; }
    public int GetDefaultAnimDirection() { return 3; }
}
public class AppearanceParamFixture { public AppearanceDataFixture data; }
public class AppearanceModelFixture { public AppearanceDataFixture data; public int dir, id; }
public class ObjectAppearanceFixture : UiCtrlFixture {
    public AppearanceParamFixture param;
    public AppearanceModelFixture model = new AppearanceModelFixture();
    public GameObject latest;
    public int refreshCalls;
    public void Refresh() {
        refreshCalls++;
        latest = new GameObject();
        DisplayCameraAreaManager.instance.Add(latest, new Vector3(1, 0, 2));
    }
__CONTROLLER_METHODS__
}
public static class ObjectPreviewLifecycleRegression {
    private static int checks;
    private static void Check(bool value, string label) {
        checks++;
        if (!value) throw new Exception(label);
    }
    public static int Run() {
        checks = 0;
        var manager = new DisplayCameraAreaManager();
        DisplayCameraAreaManager.instance = manager;
        manager.displayCamera = new Camera();
        manager.displayCamera.transform.parent = manager.transform;
        manager.transform.position = new Vector3(-500, 0, -500);
        manager.Init();
        Check(!manager.gameObject.activeSelf, "Init hides the preview system");
        Check(!manager.displayCamera.rendering, "Init prevents preview rendering");
        Check(manager.shows.Count == 0, "Init has no retained preview objects");

        var data = new AppearanceDataFixture();
        var ctrl = new ObjectAppearanceFixture { param = new AppearanceParamFixture { data = data } };
        for (int cycle = 0; cycle < 32; cycle++) {
            DynamicGlobalSettings.cameraMode = cycle % 2 == 0 ? CameraMode.Overhead : CameraMode.Isometric;
            ctrl.OnShow();
            Check(manager.displayCamera.rendering, "Opening Object Appearance enables preview");
            Check(manager.shows.Count == 1 && manager.shows[0] == ctrl.latest,
                "Opening has exactly its new preview");
            Check(ctrl.model.data == data && ctrl.model.dir == 3 && ctrl.model.id == -1,
                "Show retains the authored data and normal selection/facing initialization");
            Check(ctrl.latest.transform.position.x == -499 && ctrl.latest.transform.position.z == -498,
                "Preview retains the manager-relative placement");
            Check(manager.normalized2scene == 3, "Preview scale remains unchanged");
            Check(manager.displayCamera.transform.eulerAngles.x == (cycle % 2 == 0 ? 90 : 45),
                "Both camera modes retain their existing preview orientation");

            var previous = ctrl.latest;
            ctrl.OnShow();
            Check(previous.destroyQueued && !previous.destroyed,
                "Repeated Show schedules old preview destruction");
            Check(manager.shows.Count == 1 && manager.shows[0] != previous,
                "Repeated Show cannot accumulate retained preview objects");
            UnityEngine.Object.EndFrame();
            Check(previous.destroyed, "Repeated Show frees old preview by frame end");

            var closed = ctrl.latest;
            ctrl.OnHide();
            Check(!manager.gameObject.activeSelf && !manager.displayCamera.rendering,
                "Switching away/closing Object Appearance stops preview camera");
            Check(manager.shows.Count == 0 && closed.destroyQueued,
                "Hide clears retained preview objects and schedules destruction");
            Check(ctrl.model.data == data && ctrl.model.dir == 3 && ctrl.model.id == -1,
                "Hide changes no authored/selection state");
            ctrl.OnHide();
            Check(manager.shows.Count == 0 && !manager.displayCamera.rendering,
                "Repeated Hide is harmless");
            UnityEngine.Object.EndFrame();
            Check(closed.destroyed, "Hide frees the last preview by frame end");
        }

        // Like pooled SetShow without incoming Param, reopening reuses data.
        ctrl.param = null;
        ctrl.OnShow();
        Check(ctrl.model.data == data && manager.displayCamera.rendering,
            "Null-param reopen preserves data and re-enables camera");
        var cleared = ctrl.latest;
        manager.Clear();
        Check(manager.shows.Count == 0 && manager.displayCamera.rendering && cleared.destroyQueued,
            "Clear replaces content without closing the active preview camera");
        UnityEngine.Object.EndFrame();
        Check(cleared.destroyed, "Clear destroys its old content by frame end");
        ctrl.OnHide();
        Check(!manager.displayCamera.rendering, "Hide after Clear stops preview");
        Check(ctrl.refreshCalls == 65 && data.ensureCalls == 65,
            "Lifecycle calls never introduce extra refreshes or skip initialization");
        return checks;
    }
}
__MANAGER_SOURCE__
'@
$code = $fixture.Replace('__CONTROLLER_METHODS__', $controllerMethods).Replace('__MANAGER_SOURCE__', $managerSource)
Add-Type -TypeDefinition $code -IgnoreWarnings
try { $checks = [ObjectPreviewLifecycleRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "OBJECT_PREVIEW_LIFECYCLE_REGRESSION_PASS checks=$checks"
