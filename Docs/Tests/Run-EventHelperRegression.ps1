param([string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot '../..')).Path)
$ErrorActionPreference = 'Stop'
# Compile the actual foundational source; UnityEngine is an unused using here.
$source = (Get-Content -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level0/Z_DesignStyle/Core/Z_EventHelper.cs') -Raw).Replace('using UnityEngine;', '')
$fixture = Get-Content -LiteralPath (Join-Path $PSScriptRoot 'EventHelperRegression.cs') -Raw
Add-Type -TypeDefinition ($source + "`n" + $fixture)
$checks = [EventHelperRegression]::Run()
Write-Output "EVENT_HELPER_REGRESSION_PASS checks=$checks"
