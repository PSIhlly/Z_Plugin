param(
    [Parameter(Mandatory = $true)]
    [string]$StoryDirectory,
    [string]$BackupDirectory,
    [switch]$DryRun
)

$ErrorActionPreference = 'Stop'
$story = [System.IO.Path]::GetFullPath($StoryDirectory)
if (-not (Test-Path -LiteralPath (Join-Path $story 'Core') -PathType Container)) {
    throw "Not a story directory: $story"
}

$number = '[-+]?(?:\d+(?:\.\d*)?|\.\d+)(?:[eE][-+]?\d+)?'
$culture = [System.Globalization.CultureInfo]::InvariantCulture
$encoding = [System.Text.UTF8Encoding]::new($false, $true)
$staged = [System.Collections.Generic.List[object]]::new()

function Scale-VectorY {
    param([string]$Text, [string[]]$Keys)

    $keyPattern = ($Keys | ForEach-Object { [regex]::Escape($_) }) -join '|'
    $pattern = '(?<prefix>\\?"(?<key>' + $keyPattern + ')\\?"\s*:\s*\\?")' +
        '(?<x>' + $number + ')\|(?<y>' + $number + ')\|(?<z>' + $number + ')(?<suffix>\\?")'
    $count = [regex]::Matches($Text, $pattern).Count
    $changed = [regex]::Replace($Text, $pattern, [System.Text.RegularExpressions.MatchEvaluator] {
        param($match)
        $y = [decimal]::Parse($match.Groups['y'].Value, $culture)
        $scaled = [decimal]::Round($y / [decimal]1.5, 6, [System.MidpointRounding]::AwayFromZero)
        $newY = $scaled.ToString('0.######', $culture)
        return $match.Groups['prefix'].Value + $match.Groups['x'].Value + '|' + $newY + '|' +
            $match.Groups['z'].Value + $match.Groups['suffix'].Value
    })
    return @{ Text = $changed; Count = $count }
}

function Stage-File {
    param([System.IO.FileInfo]$File, [string[]]$Keys)

    $text = [System.IO.File]::ReadAllText($File.FullName, $encoding)
    $result = Scale-VectorY $text $Keys
    if ($result.Count -eq 0) {
        throw "Expected a position vector in $($File.FullName)"
    }
    if ($result.Text -ne $text) {
        $null = ConvertFrom-Json -InputObject $result.Text -AsHashtable -ErrorAction Stop
        $staged.Add([pscustomobject]@{
            Source = $File.FullName
            Relative = [System.IO.Path]::GetRelativePath($story, $File.FullName)
            Text = $result.Text
            Count = $result.Count
        })
    }
}

$sceneFiles = @(
    foreach ($folderName in @('Core', 'Cache', 'Save')) {
        $folder = Join-Path $story $folderName
        if (Test-Path -LiteralPath $folder -PathType Container) {
            Get-ChildItem -LiteralPath $folder -File | Where-Object { $_.Name -match '^\d+$' }
        }
    }
)
$oldScenes = @()
foreach ($file in $sceneFiles) {
    $text = [System.IO.File]::ReadAllText($file.FullName, $encoding)
    $map = ConvertFrom-Json -InputObject $text -AsHashtable -ErrorAction Stop
    if (-not $map.Contains('mapUnitSize')) {
        continue
    }
    $parts = $map['mapUnitSize'].Split('|')
    if ($parts.Count -ne 3) {
        throw "Invalid mapUnitSize in $($file.FullName)"
    }
    $height = [decimal]::Parse($parts[1], $culture)
    if ($height -eq [decimal]1.5) {
        $oldScenes += $file
    }
    elseif ($height -ne [decimal]1) {
        throw "Unexpected map layer height $height in $($file.FullName)"
    }
}

if ($oldScenes.Count -eq 0) {
    Write-Output "No 1.5-height scenes found in $story; no changes made."
    return
}
if ($oldScenes.Count -ne $sceneFiles.Count) {
    throw 'This story mixes old and new scene heights; inspect it before converting shared progress.'
}

foreach ($file in $oldScenes) {
    Stage-File $file @('mapUnitSize', 'pos', 'destination')
}
foreach ($folderName in @('Core', 'Save')) {
    $folder = Join-Path $story $folderName
    foreach ($name in @('pf', 'msf')) {
        $filePath = Join-Path $folder $name
        if (Test-Path -LiteralPath $filePath -PathType Leaf) {
            $key = if ($name -eq 'pf') { 'pos' } else { 'targetPos' }
            Stage-File (Get-Item -LiteralPath $filePath) @($key)
        }
    }
}

foreach ($entry in $staged) {
    Write-Output ("{0}: {1} height vectors" -f $entry.Relative, $entry.Count)
}
if ($DryRun) {
    Write-Output 'Dry run: no files changed.'
    return
}

if ([string]::IsNullOrWhiteSpace($BackupDirectory)) {
    $BackupDirectory = Join-Path $story ('Backups/tile-height-1-' + (Get-Date -Format 'yyyyMMdd-HHmmss'))
}
$backup = [System.IO.Path]::GetFullPath($BackupDirectory)
if (Test-Path -LiteralPath $backup) {
    throw "Backup directory already exists: $backup"
}
$backedUp = [System.Collections.Generic.List[object]]::new()
try {
    foreach ($entry in $staged) {
        $destination = Join-Path $backup $entry.Relative
        $null = New-Item -ItemType Directory -Path ([System.IO.Path]::GetDirectoryName($destination)) -Force
        Copy-Item -LiteralPath $entry.Source -Destination $destination
        $backedUp.Add($entry)
    }
    foreach ($entry in $staged) {
        [System.IO.File]::WriteAllText($entry.Source, $entry.Text, $encoding)
    }
}
catch {
    foreach ($entry in $backedUp) {
        $source = Join-Path $backup $entry.Relative
        Copy-Item -LiteralPath $source -Destination $entry.Source -Force
    }
    throw
}
Write-Output "Migrated $($staged.Count) files. Backup: $backup"
