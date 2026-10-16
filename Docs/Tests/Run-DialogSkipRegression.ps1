param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
function Read-Source([string]$relative) { Get-Content -LiteralPath (Join-Path $ProjectRoot $relative) -Raw }
function Without-Usings([string]$source) { [regex]::Replace($source, '(?m)^using [^\r\n]+;\r?\n', '') }
$manager = Read-Source 'Assets/Z_Level3/Z_UI/Core/Module/Dialog/DialogManager.cs'
$start = $manager.IndexOf('    public class Settings')
$end = $manager.IndexOf('    public enum ShowType', $start)
if ($start -lt 0 -or $end -le $start) { throw 'Dialog settings source boundaries missing.' }
$fixture = (Read-Source 'Docs/Tests/DialogSkipRegression.cs').Replace('__DIALOG_SETTINGS__', $manager.Substring($start, $end - $start))
$code = $fixture + "`n" + (Without-Usings (Read-Source 'Assets/Z_Level0/Z_DesignStyle/Core/Z_EventHelper.cs'))
foreach ($name in @('MainText', 'Options')) {
    $code += "`n" + (Without-Usings (Read-Source "Assets/Z_Level3/Z_UI/Ui/Dialog/UiDialogMainCtrl.$name.cs"))
}
# Actual partial controller/settings/event-bus source, isolated timer/UI spies.
# Does not open or modify any Unity scene, Form, asset or save.
Add-Type -TypeDefinition $code
try { $checks = [DialogSkipRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "DIALOG_SKIP_REGRESSION_PASS checks=$checks"
