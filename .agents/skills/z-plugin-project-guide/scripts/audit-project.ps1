[CmdletBinding()]
param(
    [Parameter(Position = 0)]
    [string]$ProjectRoot = (Get-Location).Path,

    [switch]$Json
)

Set-StrictMode -Version Latest
$ErrorActionPreference = "Stop"

function Resolve-ZPluginRoot {
    param([string]$StartPath)

    $resolved = (Resolve-Path -LiteralPath $StartPath).Path
    if (Test-Path -LiteralPath $resolved -PathType Leaf) {
        $resolved = Split-Path -LiteralPath $resolved -Parent
    }

    while ($true) {
        $versionFile = Join-Path $resolved "ProjectSettings\ProjectVersion.txt"
        $gameSample = Join-Path $resolved "Assets\GameSample"
        if ((Test-Path -LiteralPath $versionFile -PathType Leaf) -and
            (Test-Path -LiteralPath $gameSample -PathType Container)) {
            return $resolved
        }

        $parent = Split-Path -LiteralPath $resolved -Parent
        if ([string]::IsNullOrEmpty($parent) -or $parent -eq $resolved) {
            throw "Could not locate a Z_Plugin root above '$StartPath'."
        }
        $resolved = $parent
    }
}

function Get-RegexValue {
    param(
        [string]$Path,
        [string]$Pattern
    )

    $match = Select-String -LiteralPath $Path -Pattern $Pattern | Select-Object -First 1
    if ($null -eq $match) {
        return $null
    }
    return $match.Matches[0].Groups[1].Value.Trim()
}

function Write-List {
    param(
        [string]$Title,
        [object[]]$Values
    )

    Write-Output ""
    Write-Output $Title
    if ($Values.Count -eq 0) {
        Write-Output "  (none)"
        return
    }
    foreach ($value in $Values) {
        Write-Output "  $value"
    }
}

$projectRootResolved = Resolve-ZPluginRoot -StartPath $ProjectRoot
$gitCommand = Get-Command git -ErrorAction SilentlyContinue
$rgCommand = Get-Command rg -ErrorAction SilentlyContinue
if ($null -eq $gitCommand) {
    throw "git is required for this audit."
}
if ($null -eq $rgCommand) {
    throw "rg is required for this audit."
}

$versionPath = Join-Path $projectRootResolved "ProjectSettings\ProjectVersion.txt"
$projectSettingsPath = Join-Path $projectRootResolved "ProjectSettings\ProjectSettings.asset"
$buildSettingsPath = Join-Path $projectRootResolved "ProjectSettings\EditorBuildSettings.asset"
$manifestPath = Join-Path $projectRootResolved "Packages\manifest.json"

$editorVersion = Get-RegexValue -Path $versionPath -Pattern '^m_EditorVersion:\s*(.+)$'
$manifest = Get-Content -LiteralPath $manifestPath -Raw | ConvertFrom-Json
$urpVersion = $manifest.dependencies.'com.unity.render-pipelines.universal'
$activeInputHandler = Get-RegexValue -Path $projectSettingsPath -Pattern '^\s*activeInputHandler:\s*(\d+)\s*$'
$enabledSceneCount = @(
    Select-String -LiteralPath $buildSettingsPath -Pattern '^\s*-\s+enabled:\s+1\s*$'
).Count

$gitStatus = @(& git -C $projectRootResolved status --porcelain=v1)
if ($LASTEXITCODE -ne 0) {
    throw "git status failed for '$projectRootResolved'."
}
$gitStatus = @($gitStatus | Where-Object { -not [string]::IsNullOrWhiteSpace($_) })
$untracked = @($gitStatus | Where-Object { $_ -match '^\?\?' })
$generatedChanges = @(
    $gitStatus | Where-Object { $_ -match 'Assets[\\/].*(ExcelCs|UiBase)[\\/]' }
)
$excelChanges = @($gitStatus | Where-Object { $_ -match '\.xlsx?($|\")' })

$previousConsoleEncoding = [Console]::OutputEncoding
try {
    [Console]::OutputEncoding = [System.Text.UTF8Encoding]::new($false)
    $trackedAssets = @(
        & git -C $projectRootResolved -c core.quotepath=false ls-files -- Assets
    )
    if ($LASTEXITCODE -ne 0) {
        throw "git ls-files failed for '$projectRootResolved'."
    }
}
finally {
    [Console]::OutputEncoding = $previousConsoleEncoding
}

$missingMeta = [System.Collections.Generic.List[string]]::new()
$orphanMeta = [System.Collections.Generic.List[string]]::new()
foreach ($relativePath in $trackedAssets) {
    if ([string]::IsNullOrWhiteSpace($relativePath)) {
        continue
    }
    if ([IO.Path]::GetFileName($relativePath).StartsWith('.')) {
        continue
    }
    $nativeRelative = $relativePath.Replace('/', [IO.Path]::DirectorySeparatorChar)
    $absolutePath = Join-Path $projectRootResolved $nativeRelative
    if ($relativePath.EndsWith('.meta', [StringComparison]::OrdinalIgnoreCase)) {
        $assetPath = $absolutePath.Substring(0, $absolutePath.Length - 5)
        if (-not (Test-Path -LiteralPath $assetPath)) {
            $orphanMeta.Add($relativePath)
        }
    }
    elseif (-not (Test-Path -LiteralPath "$absolutePath.meta")) {
        $missingMeta.Add($relativePath)
    }
}

$excelLocks = @(
    Get-ChildItem -LiteralPath (Join-Path $projectRootResolved "Assets") `
        -Recurse -File -Filter '~$*.xls' -ErrorAction SilentlyContinue |
        ForEach-Object { $_.FullName.Substring($projectRootResolved.Length + 1) }
)

$runtimeEditorMentionsRaw = @(
    & rg -l --glob '*.cs' '(using\s+UnityEditor|UnityEditor\.)' Assets 2>$null
)
if ($LASTEXITCODE -notin @(0, 1)) {
    throw "rg failed while scanning UnityEditor references."
}
$runtimeEditorMentions = @(
    $runtimeEditorMentionsRaw |
        Where-Object { $_ -notmatch '(^|[\\/])Editor([\\/]|$)' } |
        Sort-Object -Unique
)

$testLikeFiles = @(
    & rg --files Assets -g '*Test*.cs' -g '*Tests*.cs' 2>$null | Sort-Object -Unique
)
if ($LASTEXITCODE -notin @(0, 1)) {
    throw "rg failed while scanning test-like files."
}

$sourceFormFiles = @(& rg --files Assets -g '*.xls' -g '*.xlsx')
if ($LASTEXITCODE -notin @(0, 1)) {
    throw "rg failed while scanning Form sources."
}
$expectedGenerated = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::OrdinalIgnoreCase
)
$managedFormRoots = [System.Collections.Generic.HashSet[string]]::new(
    [StringComparer]::OrdinalIgnoreCase
)
$managedSourceFormFiles = [System.Collections.Generic.List[string]]::new()
foreach ($sourceForm in $sourceFormFiles) {
    $normalized = $sourceForm.Replace('/', '\')
    $marker = '\Excels\'
    $markerIndex = $normalized.IndexOf($marker, [StringComparison]::OrdinalIgnoreCase)
    if ($markerIndex -lt 0) {
        continue
    }
    $formRoot = $normalized.Substring(0, $markerIndex)
    $runBat = Join-Path $projectRootResolved "$formRoot\run.bat"
    if (-not (Test-Path -LiteralPath $runBat -PathType Leaf)) {
        continue
    }
    [void]$managedFormRoots.Add($formRoot)
    $managedSourceFormFiles.Add($sourceForm)
    $formName = [IO.Path]::GetFileNameWithoutExtension($normalized).Split('_')[0]
    $generatedRelative = "$formRoot\ExcelCs\${formName}Form.cs"
    [void]$expectedGenerated.Add($generatedRelative)
}

$allGeneratedFormFiles = @(
    & rg --files Assets -g '*Form.cs' 2>$null |
        ForEach-Object { $_.Replace('/', '\') } |
        Where-Object { $_ -match '\\ExcelCs\\' }
)
if ($LASTEXITCODE -notin @(0, 1)) {
    throw "rg failed while scanning generated Forms."
}
$managedFormRootList = @($managedFormRoots)
$generatedFormFiles = @(
    $allGeneratedFormFiles | Where-Object {
        $candidate = $_
        $isManagedOutput = $false
        foreach ($formRoot in $managedFormRootList) {
            if ($candidate.StartsWith("$formRoot\ExcelCs\", [StringComparison]::OrdinalIgnoreCase)) {
                $isManagedOutput = $true
                break
            }
        }
        $isManagedOutput
    }
)
$orphanGeneratedForms = @(
    $generatedFormFiles |
        Where-Object { -not $expectedGenerated.Contains($_) } |
        Sort-Object -Unique
)
$missingGeneratedForms = @(
    $expectedGenerated |
        Where-Object {
            $candidate = Join-Path $projectRootResolved $_
            -not (Test-Path -LiteralPath $candidate -PathType Leaf)
        } |
        Sort-Object -Unique
)

$asmdefs = @(& rg --files Assets -g '*.asmdef' | Sort-Object -Unique)
if ($LASTEXITCODE -notin @(0, 1)) {
    throw "rg failed while scanning asmdefs."
}

$projectFiles = @(
    Get-ChildItem -LiteralPath $projectRootResolved -File -Filter '*.csproj' |
        Select-Object -ExpandProperty Name |
        Sort-Object
)

$report = [ordered]@{
    ProjectRoot = $projectRootResolved
    EditorVersion = $editorVersion
    UrpVersion = $urpVersion
    ActiveInputHandler = $activeInputHandler
    EnabledBuildScenes = $enabledSceneCount
    Git = [ordered]@{
        ChangedEntries = $gitStatus.Count
        UntrackedEntries = $untracked.Count
        GeneratedChanges = $generatedChanges.Count
        ExcelChanges = $excelChanges.Count
    }
    UnityAssets = [ordered]@{
        MissingMeta = @($missingMeta)
        OrphanMeta = @($orphanMeta)
        ExcelLockFiles = $excelLocks
    }
    Forms = [ordered]@{
        SourceWorkbookCount = $managedSourceFormFiles.Count
        UnmanagedWorkbookCount = $sourceFormFiles.Count - $managedSourceFormFiles.Count
        GeneratedFormCount = $generatedFormFiles.Count
        MissingGeneratedForms = $missingGeneratedForms
        OrphanGeneratedForms = $orphanGeneratedForms
    }
    RuntimeEditorMentions = $runtimeEditorMentions
    TestLikeFiles = $testLikeFiles
    Asmdefs = $asmdefs
    GeneratedProjectFiles = $projectFiles
}

if ($Json) {
    $report | ConvertTo-Json -Depth 8
    exit 0
}

Write-Output "Z_Plugin project audit"
Write-Output "Project root: $projectRootResolved"
Write-Output "Editor: $editorVersion"
Write-Output "URP: $urpVersion"
Write-Output "Legacy input selector: $activeInputHandler"
Write-Output "Enabled Build Settings scenes: $enabledSceneCount"
Write-Output "Git changes: $($gitStatus.Count) total, $($untracked.Count) untracked"
Write-Output "Generated-area changes: $($generatedChanges.Count)"
Write-Output "Excel changes: $($excelChanges.Count)"
Write-Output "Managed Form sources/generated: $($managedSourceFormFiles.Count)/$($generatedFormFiles.Count)"
Write-Output "Unmanaged/legacy workbook files: $($sourceFormFiles.Count - $managedSourceFormFiles.Count)"
Write-Output "Tracked assets missing meta: $($missingMeta.Count)"
Write-Output "Orphan tracked meta files: $($orphanMeta.Count)"

Write-List -Title "Tracked assets missing meta" -Values @($missingMeta)
Write-List -Title "Orphan tracked meta files" -Values @($orphanMeta)
Write-List -Title "Missing generated Forms" -Values $missingGeneratedForms
Write-List -Title "Potential orphan generated Forms" -Values $orphanGeneratedForms
Write-List -Title "Excel lock files" -Values $excelLocks
Write-List -Title "Runtime-path files mentioning UnityEditor (manual review)" -Values $runtimeEditorMentions
Write-List -Title "Test-like C# files (not proof of a test suite)" -Values $testLikeFiles

Write-Output ""
Write-Output "Generated project files found: $($projectFiles -join ', ')"
Write-Output "Run Unity reimport before treating their build results as authoritative."
