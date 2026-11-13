param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
function Read-Source([string]$relative) { Get-Content -LiteralPath (Join-Path $ProjectRoot $relative) -Raw }
function Without-Usings([string]$source) { [regex]::Replace($source, '(?m)^using [^\r\n]+;\r?\n', '') }
$command = Read-Source 'Assets/GameSample/MyScripts/Main/Event/Cmd/Ui/ShowTeamerSelectCmd.cs'
$selection = Read-Source 'Assets/GameSample/MyScripts/Main/Event/Cmd/Ui/SelectOneTeamerCmd.cs'
$ui = Read-Source 'Assets/GameSample/MyScripts/Play/Ui/UiTeamSelectWindowCtrl.cs'
$metadata = Read-Source 'Assets/GameSample/Forms/ExcelCs/GameCmdDataForm.cs'
if (-not [regex]::IsMatch($metadata, 'new Data\(100104,"ShowTeamerSelect",null,null,null,new List<string>\(\)\{"void",\}')) {
    throw 'ShowTeamerSelect must be a zero-argument void party-management command.'
}
if (-not [regex]::IsMatch($metadata, 'new Data\(100135,"SelectOneTeamer",null,null,new List<string>\(\)\{"teamer",\},new List<string>\(\)\{"character",\}') -or
    -not $metadata.Contains('teamer = SelectOneTeamer();')) {
    throw 'SelectOneTeamer must expose a zero-argument character return and complete default statement.'
}
$prefix = 'using System; using System.Collections.Generic; using System.Globalization; using System.Linq; using UnityEngine; using Form; using Z_Code; using Z_Code.Form; using Z_DataSystem.Form; using Z_Text; using Z_Ui; using Z_Ui.Base; using Z_Ui.Notify; using Ui.Notify; using Ui.TeamSelectWindow;'
$source = $prefix + "`n" + (Without-Usings $command) + "`n" + (Without-Usings $selection) + "`n" + (Without-Usings $ui) + "`n" + (Read-Source 'Docs/Tests/TeamerSelectRegression.cs')
Add-Type -TypeDefinition $source -IgnoreWarnings
try { $checks = [TeamerSelectRegression]::Run() }
catch { throw $_.Exception.GetBaseException() }
Write-Output "TEAMER_SELECT_REGRESSION_PASS checks=$checks"
