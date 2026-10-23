param(
    [string]$ProjectRoot = (Resolve-Path (Join-Path $PSScriptRoot "../..")).Path,
    [string]$UnityEditor = "D:/WorkSoftWare/Unity/2022.3.61t4/Editor/Tuanjie.exe",
    [ValidateSet('MapRuntimeRegression', 'MissionRuntimeRegression', 'MinimapRuntimeRegression', 'MapTextureRuntimeRegression', 'TileFrontVisibilityRegression', 'DisFadeRoutingRegression', 'DisFadeBatchingRegression', 'InstancePoolNameRegression', 'ModSceneHistoryRuntimeRegression', 'MapFadeShaderRegression', 'MapShadowBiasRegression', 'MapShadowReceiverRegression', 'MapEdgeTransitionRegression', 'StoryPackageImportRegression', 'CodeInterpreterRuntimeRegression', 'CharacterReferenceRegression', 'CharacterPartyRegression', 'ObjectProductRuntimeRegression', 'SceneObjectReferenceRegression', 'SceneProductCommandRegression', 'ObjectProductEditorRegression')]
    [string]$Fixture = 'MapRuntimeRegression',
    [switch]$CharacterPlayLifecycle,
    [switch]$SceneCommands,
    [switch]$SkipBuild
)
$ErrorActionPreference = "Stop"
$shaderPath = Join-Path $ProjectRoot "Assets/Z_Level0/Z_Shader/Func/DisDark/DisFadeCode.shader"
$shaderSource = Get-Content -LiteralPath $shaderPath -Raw
$shadowStart = $shaderSource.IndexOf('Name "ShadowCaster"')
$shadowEnd = if ($shadowStart -ge 0) { $shaderSource.IndexOf("ENDHLSL", $shadowStart) } else { -1 }
if ($shadowStart -lt 0 -or $shadowEnd -lt 0) { throw "DisFadeCode ShadowCaster pass is missing." }
$shadowSource = $shaderSource.Substring($shadowStart, $shadowEnd - $shadowStart)
if ($shadowSource.Contains("_Show") -or $shadowSource.Contains("_FadeCenter") -or
    $shaderSource.Contains("_AlphaTex") -or
    !$shadowSource.Contains("clip(baseColor.a -") -or !$shadowSource.Contains("_Cutoff")) {
    throw "DisFadeCode ShadowCaster must ignore visibility and retain base-texture alpha cutouts without AlphaTex."
}
# Build the production assembly first. All tests run in a separate temporary
# Unity project; no original scene, singleton, Form, asset or save is modified.
$runtimeProjectPath = Join-Path $ProjectRoot "Assembly-CSharp.csproj"
$bindingRelative = 'Assets/Z_Level0/Z_Shader/Func/DisDark/DisFadeMaterialBinding.cs'
[xml]$runtimeProject = Get-Content -LiteralPath $runtimeProjectPath -Raw
$includedSources = @($runtimeProject.SelectNodes("//*[local-name()='Compile']") |
    ForEach-Object { $_.Include.Replace('\', '/') })
$missingSources = @(@($bindingRelative,
    'Assets/GameSample/Forms/ExcelCs/ObjectProductForm.cs',
    'Assets/GameSample/MyScripts/Main/ObjectProductForm.Runtime.cs',
    'Assets/GameSample/MyScripts/Main/CharacterProductForm.Reference.cs',
    'Assets/GameSample/MyScripts/Main/Event/Cmd/CharacterPartyCmd.cs',
    'Assets/GameSample/MyScripts/Play/PlayInfoController.Party.cs',
    'Assets/GameSample/MyScripts/Main/Event/SceneObjectReference.cs') |
    Where-Object { $_ -notin $includedSources })
$buildArguments = @('build', $runtimeProjectPath, '--no-restore', '--nologo', '-v:quiet', '-m:1',
    '-p:BuildInParallel=false', '-p:UseSharedCompilation=false')
if ($missingSources.Count -gt 0) {
    # Unity can briefly leave generated project files stale after new source is
    # added. Inject only that source into this validation build, never edit the
    # generated .csproj or add Assembly-CSharp source to lower project refs.
    $validationBuildRoot = Join-Path $ProjectRoot ('Temp/MapRegressionBuild-' + [guid]::NewGuid().ToString('N'))
    New-Item -ItemType Directory -Path $validationBuildRoot -Force | Out-Null
    $validationTargets = Join-Path $validationBuildRoot 'ValidationSources.targets'
    $targetSource = '<Project><ItemGroup Condition="''$(MSBuildProjectName)'' == ''Assembly-CSharp''">'
    foreach ($relative in $missingSources) {
        $absolute = [Security.SecurityElement]::Escape((Join-Path $ProjectRoot $relative))
        $targetSource += '<Compile Include="' + $absolute + '" />'
    }
    $targetSource += '</ItemGroup></Project>'
    [IO.File]::WriteAllText($validationTargets, $targetSource)
    $buildArguments += '-p:CustomAfterMicrosoftCommonTargets=' + $validationTargets
}
if (!$SkipBuild) {
    & dotnet @buildArguments
    if ($LASTEXITCODE -ne 0) { throw "Production build failed." }
}
if ($Fixture -eq 'CodeInterpreterRuntimeRegression') {
    & dotnet build (Join-Path $ProjectRoot 'Z_Code.Tests.Editor.csproj') --no-restore --nologo -v:quiet -m:1 -p:BuildInParallel=false -p:UseSharedCompilation=false
    if ($LASTEXITCODE -ne 0) { throw 'Interpreter test build failed.' }
}
$testProject = Join-Path $ProjectRoot ("Temp/MapRuntimeRegression-" + [guid]::NewGuid().ToString("N"))
New-Item -ItemType Directory -Path "$testProject/Assets/Plugins", "$testProject/Assets/Editor", "$testProject/Packages", "$testProject/ProjectSettings" -Force | Out-Null
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "$Fixture.cs") -Destination "$testProject/Assets/Editor/$Fixture.cs"
Copy-Item -LiteralPath (Join-Path $PSScriptRoot "map-regression-manifest.json") -Destination "$testProject/Packages/manifest.json"
Copy-Item -LiteralPath (Join-Path $ProjectRoot "ProjectSettings/ProjectVersion.txt") -Destination "$testProject/ProjectSettings/ProjectVersion.txt"
if ($Fixture -eq 'CodeInterpreterRuntimeRegression') {
    Copy-Item -LiteralPath (Join-Path $ProjectRoot 'Temp/bin/Debug/Z_Code.Tests.Editor.dll') -Destination "$testProject/Assets/Plugins"
    $nunitPackage = Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Library/PackageCache') -Directory -Filter 'com.unity.ext.nunit@*' | Select-Object -First 1
    if (!$nunitPackage) { throw 'The cached NUnit dependency is missing.' }
    Copy-Item -LiteralPath (Join-Path $nunitPackage.FullName 'net35/unity-custom/nunit.framework.dll') -Destination "$testProject/Assets/Plugins"
    New-Item -ItemType Directory -Path "$testProject/ExtraAssets/2/Core" -Force | Out-Null
    Copy-Item -LiteralPath (Join-Path $ProjectRoot 'ExtraAssets/2/Core/ef') -Destination "$testProject/ExtraAssets/2/Core/ef"
}
if ($Fixture -eq 'MapFadeShaderRegression') {
    $rendererPath = Join-Path $ProjectRoot 'Assets/Samples/Universal RP/13.1.9/URP Package Samples/SharedAssets/Settings/SamplesForwardRenderer.asset'
    $pipelinePath = Join-Path (Split-Path -Parent $rendererPath) 'SamplesPipelineAsset.asset'
    $rendererGuid = [regex]::Match((Get-Content -LiteralPath "$rendererPath.meta" -Raw), '(?m)^guid: (\S+)').Groups[1].Value
    $pipelineGuid = [regex]::Match((Get-Content -LiteralPath "$pipelinePath.meta" -Raw), '(?m)^guid: (\S+)').Groups[1].Value
    $pipelineSource = Get-Content -LiteralPath $pipelinePath -Raw
    $graphicsSource = Get-Content -LiteralPath (Join-Path $ProjectRoot 'ProjectSettings/GraphicsSettings.asset') -Raw
    if (!$graphicsSource.Contains("m_CustomRenderPipeline: {fileID: 11400000, guid: $pipelineGuid, type: 2}") -or
        $pipelineSource -notmatch "m_RendererDataList:\s+- \{fileID: 11400000, guid: $rendererGuid, type: 2\}" -or
        !$pipelineSource.Contains('m_DefaultRendererIndex: 0')) {
        throw 'DisFadeOpaqueFeature must be registered on the actual GraphicsSettings default Renderer.'
    }
    $rendererSource = Get-Content -LiteralPath $rendererPath -Raw
    if ($rendererSource -notmatch '(?s)guid: 2a463cd78c7149e5bd66d25bee865ba7.*?m_Name: DisFade Opaque.*?m_Active: 1' -or
        !$rendererSource.Contains('m_RendererFeatures:') -or
        !$rendererSource.Contains('- {fileID: 11400001}') -or
        !$rendererSource.Contains('m_RendererFeatureMap: 41f3ad0000000000')) {
        throw 'The active URP Renderer must draw DisFadeOpaque with no material override before transparents.'
    }
    Copy-Item -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level0/Z_Shader/Func/DisDark/DisFadeOpaqueFeature.cs') -Destination "$testProject/Assets/DisFadeOpaqueFeature.cs"
    Copy-Item -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level0/Z_Shader/Func/DisDark/DisFadeRenderRouting.cs') -Destination "$testProject/Assets/DisFadeRenderRouting.cs"
    Copy-Item -LiteralPath (Join-Path $ProjectRoot $bindingRelative) -Destination "$testProject/Assets/DisFadeMaterialBinding.cs"
}
if ($Fixture -in @('MapFadeShaderRegression', 'MapShadowBiasRegression', 'MapShadowReceiverRegression', 'MapTextureRuntimeRegression', 'TileFrontVisibilityRegression', 'DisFadeRoutingRegression', 'DisFadeBatchingRegression')) {
    Copy-Item -LiteralPath $shaderPath -Destination "$testProject/Assets/DisFadeCode.shader"
    # Copy only cached shader headers into isolated embedded packages. No package
    # downloads, production package edits or duplicate runtime C# assemblies.
    foreach ($packageName in @('com.unity.render-pipelines.core', 'com.unity.render-pipelines.universal', 'com.unity.render-pipelines.universal-config')) {
        $package = Get-ChildItem -LiteralPath (Join-Path $ProjectRoot 'Library/PackageCache') -Directory -Filter "$packageName@*" | Select-Object -First 1
        if (!$package) { throw "Required cached shader package missing: $packageName" }
        $headerRoot = Join-Path $testProject "Packages/$packageName"
        New-Item -ItemType Directory -Path $headerRoot -Force | Out-Null
        foreach ($header in (Get-ChildItem -LiteralPath $package.FullName -Filter '*.hlsl' -Recurse -File)) {
            $relative = $header.FullName.Substring($package.FullName.Length + 1)
            $destination = Join-Path $headerRoot $relative
            New-Item -ItemType Directory -Path (Split-Path -Parent $destination) -Force | Out-Null
            Copy-Item -LiteralPath $header.FullName -Destination $destination
        }
        $headerManifest = @{ name = $packageName; version = '1.0.0'; displayName = 'Isolated shader headers' } | ConvertTo-Json
        [IO.File]::WriteAllText((Join-Path $headerRoot 'package.json'), $headerManifest)
    }
}

# Also compile the new Renderer Feature against the real cached URP binaries in
# the shader fixture. Embedded packages supply headers only, not duplicate C#.
if ($Fixture -eq 'MapEdgeTransitionRegression') {
    # The edge mask only reads column keys and version/cell-size metadata. Test
    # the actual component source with a minimal map context, without importing
    # unrelated runtime/vendor assemblies into this focused Unity project.
    Copy-Item -LiteralPath (Join-Path $ProjectRoot 'Assets/Z_Level3/Z_Map/Core/EdgeTransition/MapEdgeTransition.cs') -Destination "$testProject/Assets/MapEdgeTransition.cs"
    Copy-Item -LiteralPath (Join-Path $PSScriptRoot 'MapEdgeTransitionTestContext.cs') -Destination "$testProject/Assets/MapEdgeTransitionTestContext.cs"
}
else { & {
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
    if ($Fixture -notin @('MapFadeShaderRegression', 'MapShadowBiasRegression', 'MapShadowReceiverRegression')) {
        [Reflection.Assembly]::LoadFrom((Join-Path $burstPackage.FullName "Unity.Burst.CodeGen/Unity.Burst.Cecil.dll")) | Out-Null
        $resolver = New-Object Mono.Cecil.DefaultAssemblyResolver
        $resolver.AddSearchDirectory((Join-Path (Split-Path -Parent $UnityEditor) 'Data/Managed/UnityEngine'))
        $resolver.AddSearchDirectory((Join-Path (Split-Path -Parent $UnityEditor) 'Data/Managed'))
        $resolver.AddSearchDirectory((Join-Path $ProjectRoot 'Library/ScriptAssemblies'))
        $resolver.AddSearchDirectory((Join-Path $ProjectRoot 'Temp/bin/Debug'))
        $resolver.AddSearchDirectory((Join-Path $testProject 'Assets/Plugins'))
        $readerParameters = New-Object Mono.Cecil.ReaderParameters
        $readerParameters.AssemblyResolver = $resolver
        $runtimeAssembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly(
            (Join-Path $ProjectRoot "Temp/bin/Debug/Assembly-CSharp.dll"), $readerParameters)
        try {
            $runtimeAssembly.Name.Name = "MapRegressionRuntime"
            $runtimeAssembly.MainModule.Name = "MapRegressionRuntime.dll"
            $runtimeAssembly.Write((Join-Path $testProject "Assets/Plugins/MapRegressionRuntime.dll"))
        }
        finally { $runtimeAssembly.Dispose(); $resolver.Dispose() }
    }
} }
$logPath = Join-Path $testProject "regression.log"
$graphicsArguments = if ($Fixture -in @('MapFadeShaderRegression', 'MapShadowBiasRegression', 'MapShadowReceiverRegression', 'DisFadeBatchingRegression')) { '-force-d3d11' } else { '-nographics' }
$entryPoint = if ($CharacterPlayLifecycle -and $Fixture -eq 'MapTextureRuntimeRegression') { 'RunCharacterPlayLifecycle' }
    elseif ($SceneCommands -and $Fixture -eq 'MapRuntimeRegression') { 'RunSceneCommands' } else { 'Run' }
$argumentLine = '-batchmode ' + $graphicsArguments + ' -projectPath "' + $testProject + '" -executeMethod ' + $Fixture + '.' + $entryPoint + ' -logFile "' + $logPath + '"'
$process = Start-Process -FilePath $UnityEditor -ArgumentList $argumentLine -WindowStyle Hidden -PassThru
Write-Output "Regression process: $($process.Id)"
Write-Output "Regression log: $logPath"
$process.WaitForExit()
if ($process.ExitCode -ne 0) {
    Get-Content -LiteralPath $logPath -Tail 100
    throw "Unity regression process failed: $($process.ExitCode)"
}
$successMarker = switch ($Fixture) {
    'CharacterReferenceRegression' { 'CHARACTER_REFERENCE_REGRESSION_PASS' }
    'CharacterPartyRegression' { 'CHARACTER_PARTY_REGRESSION_PASS' }
    'ObjectProductRuntimeRegression' { 'OBJECT_PRODUCT_RUNTIME_REGRESSION_PASS' }
    'SceneObjectReferenceRegression' { 'SCENE_OBJECT_REFERENCE_REGRESSION_PASS' }
    'SceneProductCommandRegression' { 'SCENE_PRODUCT_COMMAND_REGRESSION_PASS' }
    'ObjectProductEditorRegression' { 'OBJECT_PRODUCT_EDITOR_REGRESSION_PASS' }
    'CodeInterpreterRuntimeRegression' { 'CODE_INTERPRETER_RUNTIME_REGRESSION_PASS' }
    'MissionRuntimeRegression' { 'MISSION_RUNTIME_REGRESSION_PASS' }
    'MinimapRuntimeRegression' { 'MINIMAP_RUNTIME_REGRESSION_PASS' }
    'MapTextureRuntimeRegression' { 'MAP_TEXTURE_RUNTIME_REGRESSION_PASS' }
    'TileFrontVisibilityRegression' { 'TILE_FRONT_VISIBILITY_REGRESSION_PASS' }
    'DisFadeRoutingRegression' { 'DIS_FADE_ROUTING_REGRESSION_PASS' }
    'DisFadeBatchingRegression' { 'DIS_FADE_BATCHING_REGRESSION_PASS' }
    'InstancePoolNameRegression' { 'INSTANCE_POOL_NAME_REGRESSION_PASS' }
    'ModSceneHistoryRuntimeRegression' { 'MOD_SCENE_HISTORY_RUNTIME_REGRESSION_PASS' }
    'MapFadeShaderRegression' { 'MAP_FADE_SHADER_REGRESSION_PASS' }
    'MapShadowBiasRegression' { 'MAP_SHADOW_BIAS_REGRESSION_PASS' }
    'MapShadowReceiverRegression' { 'MAP_SHADOW_RECEIVER_REGRESSION_PASS' }
    'MapEdgeTransitionRegression' { 'MAP_EDGE_TRANSITION_REGRESSION_PASS' }
    'StoryPackageImportRegression' { 'STORY_PACKAGE_IMPORT_REGRESSION_PASS' }
    default { 'MAP_RUNTIME_REGRESSION_PASS' }
}
$passed = Select-String -LiteralPath $logPath -Pattern $successMarker
if ($CharacterPlayLifecycle -and $Fixture -eq 'MapTextureRuntimeRegression') {
    $passed = Select-String -LiteralPath $logPath -Pattern 'MAP_CHARACTER_PLAY_LIFECYCLE_PASS'
}
if ($SceneCommands -and $Fixture -eq 'MapRuntimeRegression') {
    $passed = Select-String -LiteralPath $logPath -Pattern 'MAP_SCENE_COMMANDS_REGRESSION_PASS'
}
if (!$passed) { throw "Missing regression success marker: $logPath" }
$passed.Line
