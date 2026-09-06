param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path,
    [string]$UnityEditor = "D:/WorkSoftWare/Unity/2022.3.61t4/Editor/Tuanjie.exe"
)
$ErrorActionPreference = "Stop"
# Build the production assembly first. All tests run in a separate temporary
# Unity project; no original scene, singleton, Form, asset or save is modified.
& dotnet build (Join-Path $ProjectRoot "Assembly-CSharp.csproj") --no-restore --nologo -v:quiet
if ($LASTEXITCODE -ne 0) { throw "Production build failed." }
$testProject = Join-Path $ProjectRoot ("Temp/MapRuntimeRegression-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path "$testProject/Assets/Plugins", "$testProject/Assets/Editor", "$testProject/Packages", "$testProject/ProjectSettings" -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "MapRuntimeRegression.cs") -Destination "$testProject/Assets/Editor/MapRuntimeRegression.cs"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "map-regression-manifest.json") -Destination "$testProject/Packages/manifest.json"
Copy-Item -LiteralPath (Join-Path $ProjectRoot "ProjectSettings/ProjectVersion.txt") -Destination "$testProject/ProjectSettings/ProjectVersion.txt"

$visited = @{}
function Copy-ProjectDependencies([string]$projectFile) {
    if ($visited.ContainsKey($projectFile)) { return }
    $visited[$projectFile] = $true
    [xml]$xml = Get-Content -Raw -LiteralPath $projectFile
    $parent = Split-Path -Parent $projectFile
    foreach ($reference in $xml.SelectNodes("//*[local-name()='Reference']/*[local-name()='HintPath']")) {
        $path = $reference.InnerText
        if (![IO.Path]::IsPathRooted($path)) { $path = Join-Path $parent $path }
        if ((Test-Path -LiteralPath $path) -and $path -notmatch '[\\/]Editor[\\/]Data[\\/]' -and
            [IO.Path]::GetFileName($path) -notmatch '^Unity\.(Rider|VisualStudio)\.Editor\.dll$') {
            Copy-Item -LiteralPath $path -Destination "$testProject/Assets/Plugins" -Force
        }
    }
    foreach ($reference in $xml.SelectNodes("//*[local-name()='ProjectReference']")) {
        Copy-ProjectDependencies (Join-Path $parent $reference.Include)
    }
}
Copy-ProjectDependencies (Join-Path $ProjectRoot "Assembly-CSharp.csproj")
# These transitive precompiled dependencies are not listed in the root project.
Copy-Item -LiteralPath (Join-Path $ProjectRoot "Library/ScriptAssemblies/Unity.ShaderGraph.Utilities.dll") -Destination "$testProject/Assets/Plugins"
$burstPackage = Get-ChildItem -LiteralPath (Join-Path $ProjectRoot "Library/PackageCache") -Directory -Filter "com.unity.burst@*" | Select-Object -First 1
if (!$burstPackage) { throw "Unity Burst package cache is missing; open the project in Unity first." }
Copy-Item -LiteralPath (Join-Path $burstPackage.FullName "Unity.Burst.Unsafe.dll") -Destination "$testProject/Assets/Plugins"
Get-ChildItem -LiteralPath (Join-Path $ProjectRoot "Temp/bin/Debug") -Filter *.dll |
    Where-Object { $_.Name -notmatch '(Editor|Demos|Assembly-CSharp)' } |
    ForEach-Object { Copy-Item -LiteralPath $_.FullName -Destination "$testProject/Assets/Plugins" -Force }
# Unity reserves Assembly-CSharp for source scripts and cannot attach its types
# from a precompiled plugin. Rename only the isolated binary's assembly identity.
[Reflection.Assembly]::LoadFrom((Join-Path $burstPackage.FullName "Unity.Burst.CodeGen/Unity.Burst.Cecil.dll")) | Out-Null
$runtimeAssembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $ProjectRoot "Temp/bin/Debug/Assembly-CSharp.dll"))
try {
    $runtimeAssembly.Name.Name = "MapRegressionRuntime"
    $runtimeAssembly.MainModule.Name = "MapRegressionRuntime.dll"
    $runtimeAssembly.Write((Join-Path $testProject "Assets/Plugins/MapRegressionRuntime.dll"))
}
finally { $runtimeAssembly.Dispose() }
$logPath = Join-Path $testProject "regression.log"
$argumentLine = '-batchmode -nographics -projectPath "' + $testProject + '" -executeMethod MapRuntimeRegression.Run -logFile "' + $logPath + '"'
$process = Start-Process -FilePath $UnityEditor -ArgumentList $argumentLine -WindowStyle Hidden -PassThru
Write-Output "Regression process: $($process.Id)"
Write-Output "Regression log: $logPath"
$process.WaitForExit()
if ($process.ExitCode -ne 0) {
    Get-Content -LiteralPath $logPath -Tail 100
    throw "Unity regression process failed: $($process.ExitCode)"
}
$passed = Select-String -LiteralPath $logPath -Pattern 'MAP_RUNTIME_REGRESSION_PASS'
if (!$passed) { throw "Missing regression success marker: $logPath" }
$passed.Line
