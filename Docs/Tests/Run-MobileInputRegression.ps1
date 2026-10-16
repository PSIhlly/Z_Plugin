param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path,
    [ValidateSet('Android', 'Desktop')][string]$Platform = 'Android'
)
$ErrorActionPreference = 'Stop'
function Read-Source([string]$relative) { Get-Content -LiteralPath (Join-Path $ProjectRoot $relative) -Raw }
function Source-Span([string]$source, [string]$startToken, [string]$endToken) {
    $start = $source.IndexOf($startToken)
    $end = $source.IndexOf($endToken, $start)
    if ($start -lt 0 -or $end -le $start) { throw "Source boundaries missing: $startToken / $endToken" }
    $source.Substring($start, $end - $start)
}
function Without-Usings([string]$source) { [regex]::Replace($source, '(?m)^using [^\r\n]+;\r?\n', '') }
$inputSource = Read-Source 'Assets/Z_Level2/Z_Input/Core/InputManager.cs'
$stickSource = Read-Source 'Assets/Z_Level3/Z_UI/Ui/Standard/UiStickCtrl.cs'
$playerSource = Read-Source 'Assets/GameSample/MyScripts/Play/Ui/Scene/UiPlayerTouchOptCtrl.cs'
$mainSource = Read-Source 'Assets/GameSample/MyScripts/Play/Ui/Scene/UiPlaySceneMainCtrl.cs'
$skillStart = $mainSource.IndexOf('            void Drag(Vector3 dir, SkillType type)')
$skillEnd = [regex]::Match($mainSource, '(?m)^#if UNITY_STANDALONE_WIN\r?\n[ \t]*view.page_PlayerTouchOpt.SetShow').Index
if ($skillStart -lt 0 -or $skillEnd -le $skillStart) { throw 'Q/E callback source boundaries missing.' }
$skillSetup = $mainSource.Substring($skillStart, $skillEnd - $skillStart)
$skillHelpers = Source-Span $mainSource '        private bool CanContinueSkill(SkillType type)' '        public void OnEvent(StoryCharacterEvent evt)'
$skillRefresh = Source-Span $mainSource '            RefreshSkillStick(view.model_eStick' '            actCon.Clear();'
$eventSource = Read-Source 'Assets/Z_Level0/Z_DesignStyle/Core/Z_EventHelper.cs'
$modSource = Read-Source 'Assets/GameSample/MyScripts/Mod/ModSceneController.cs'
$modSource = $modSource.Substring($modSource.IndexOf('public class ModSceneController'))
$modPointers = Source-Span $modSource '    public void OnEvent(InputMouseEvent evt)' '    public void OnEvent(InputMouseScrollEvent evt)'
$modCommit = Source-Span $modSource '    public void CommitPendingOperation()' '    public void Undo()'
$modBegin = Source-Span $modSource '    public void Begin(int id)' '    public void End()'
$modEnd = Source-Span $modSource '    public void End()' '    public T TryGetUnit<T>'
if (-not $modBegin.Contains('ResetPointer();') -or -not $modEnd.Contains('ResetPointer();')) { throw 'Mod Begin/End must clear captured pointers.' }
# Read the authored sibling geometry/Graphic flags, not encoded .meta GUIDs.
function Prefab-Block([string]$source, [string]$id) {
    $match = [regex]::Match($source, '(?ms)^--- !u!\d+ &' + [regex]::Escape($id) + '(?: stripped)?\r?\n.*?(?=^---|\z)')
    if (-not $match.Success) { throw "Prefab object not found: $id" }
    $match.Value
}
$stickPrefab = Read-Source 'Assets/Z_Level3/Z_UI/Prefab/Standard/go_stick.prefab'
$centerImage = Prefab-Block $stickPrefab '1903189635033183910'
if (-not $centerImage.Contains('m_RaycastTarget: 1')) { throw 'The actual stick-center Graphic must retain raycast targeting.' }
foreach ($rectId in @('4245068258628321112', '5634179870878149815')) {
    if (-not (Prefab-Block $stickPrefab $rectId).Contains('m_Father: {fileID: 3477006036245981806}')) { throw 'Stick center and area must remain siblings under their authored root.' }
}
$playPrefab = Read-Source 'Assets/GameSample/Prefabs/Ui/standard/Module/PlaySceneMain.prefab'
$instances = [regex]::Matches($playPrefab, '(?ms)^--- !u!1001 &[^\r\n]+\r?\n.*?(?=^---|\z)')
foreach ($name in @('go_eStick', 'go_qStick')) {
    $instance = @($instances | Where-Object { $_.Value.Contains('value: ' + $name) })
    if ($instance.Count -ne 1) { throw "Expected one authored skill-stick instance: $name" }
    $text = $instance[0].Value
    if (-not [regex]::IsMatch($text, '(?m)target: \{fileID: 7364165830919807275,[^\r\n]+\}\r?\n\s+propertyPath: m_Enabled\r?\n\s+value: 0')) { throw "$name should exercise the disabled area Graphic configuration." }
    if ([regex]::IsMatch($text, '(?m)target: \{fileID: 1903189635033183910,[^\r\n]+\}\r?\n\s+propertyPath: m_RaycastTarget\r?\n\s+value: 0')) { throw "$name must not disable center raycast targeting." }
}
$fixture = Read-Source 'Docs/Tests/MobileInputRegression.cs'
$fixture = $fixture.Replace('__MOD_POINTER_METHODS__', $modPointers).Replace('__MOD_COMMIT_METHOD__', $modCommit)
$fixture = $fixture.Replace('__SKILL_SETUP__', $skillSetup).Replace('__SKILL_HELPERS__', $skillHelpers).Replace('__SKILL_REFRESH_CALLS__', $skillRefresh)
$prefix = 'using System; using System.Collections.Generic; using System.Linq; using System.Reflection; using UnityEngine; using UnityEngine.UI; using UnityEngine.EventSystems; using Z_DesignStyle; using Z_Debug; using Z_Input; using Z_Math; using Z_Ui.Base; using Form; using Ui.Stick; using Z_Map;'
$code = $prefix + "`n" + (Without-Usings $eventSource) + "`n" + (Without-Usings $inputSource) + "`n" + (Without-Usings $stickSource) + "`n" + (Without-Usings $playerSource) + "`n" + $fixture
$symbols = if ($Platform -eq 'Android') { 'UNITY_ANDROID' } else { 'UNITY_EDITOR,UNITY_EDITOR_WIN,UNITY_STANDALONE_WIN' }
Add-Type -TypeDefinition $code -CompilerOptions "/define:$symbols" -IgnoreWarnings
try { $checks = [MobileInputRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "MOBILE_INPUT_REGRESSION_PASS platform=$Platform checks=$checks"

