[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [string]$BepInExReferencePath,

    [Parameter(Mandatory = $true)]
    [ValidateRange(0, 65535)]
    [int]$BuildNumber,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F]{7,40}$')]
    [string]$SourceRevision
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$sourceTestGate = Join-Path $PSScriptRoot 'Validate-S1-05.ps1'
$buildScript = Join-Path $repoRoot 'scripts\build\Build-S1-02.ps1'
$bepInExReference = $BepInExReferencePath
if ([string]::IsNullOrWhiteSpace($bepInExReference)) {
    $bepInExReference = Join-Path $GameRoot 'BepInEx\core\BepInEx.dll'
}
$inventoryPath = Join-Path $repoRoot 'ci\compile-references\surface-inventory.json'
$pluginPath = Join-Path $repoRoot 'src\DSPRecipeTracker\bin\Release\net472\DSPRecipeTracker.dll'
$shimPaths = @(
    (Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine\obj\Release\netstandard2.0\ref\UnityEngine.dll'),
    (Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine.CoreModule\obj\Release\netstandard2.0\ref\UnityEngine.CoreModule.dll'),
    (Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine.TextRenderingModule\obj\Release\netstandard2.0\ref\UnityEngine.TextRenderingModule.dll'),
    (Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine.UIModule\obj\Release\netstandard2.0\ref\UnityEngine.UIModule.dll'),
    (Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine.UI\obj\Release\netstandard2.0\ref\UnityEngine.UI.dll'),
    (Join-Path $repoRoot 'ci\compile-references\DSPGame.Reference\obj\Release\netstandard2.0\ref\Assembly-CSharp.dll')
)

& $sourceTestGate
if ($LASTEXITCODE -ne 0) {
    throw "S1-06 deterministic tests failed with exit code $LASTEXITCODE."
}

& $buildScript -ReferenceMode Hosted -BepInExReferencePath $bepInExReference `
    -BuildNumber $BuildNumber -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S1-06 Hosted Release validation failed with exit code $LASTEXITCODE."
}

& $buildScript -ReferenceMode Local -GameRoot $GameRoot -BepInExReferencePath $bepInExReference `
    -BuildNumber $BuildNumber -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S1-06 Local Release validation failed with exit code $LASTEXITCODE."
}

$localCoverageReport = Join-Path $repoRoot 'artifacts\build\compile-reference-validation-local.json'
& dotnet run --project (Join-Path $repoRoot 'scripts\validate\CompileReferenceValidator\CompileReferenceValidator.csproj') `
    --configuration Release -- $pluginPath $inventoryPath $localCoverageReport @shimPaths
if ($LASTEXITCODE -ne 0) {
    throw "S1-06 Local compile-reference coverage validation failed with exit code $LASTEXITCODE."
}

$hostedCoverageReport = Join-Path $repoRoot 'artifacts\build\compile-reference-validation.json'
$hostedCoverage = Get-Content -LiteralPath $hostedCoverageReport -Raw | ConvertFrom-Json
$localCoverage = Get-Content -LiteralPath $localCoverageReport -Raw | ConvertFrom-Json
$coverageDifference = @(Compare-Object `
    -ReferenceObject @($hostedCoverage.consumedSurface) `
    -DifferenceObject @($localCoverage.consumedSurface))
if ($coverageDifference.Count -ne 0) {
    throw "Local and Hosted production builds consume different Unity surfaces: $($coverageDifference | Out-String)"
}

$uiSources = @(
    (Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerPanelUiBoundary.cs'),
    (Join-Path $repoRoot 'src\DSPRecipeTracker\UnityTrackerPanelAdapter.cs'),
    (Join-Path $repoRoot 'src\DSPRecipeTracker\UnityTrackerPanelDragAdapter.cs'),
    (Join-Path $repoRoot 'src\DSPRecipeTracker\UnityRecipeRowUiAdapter.cs')
)
$uiSourceText = ($uiSources | ForEach-Object { [IO.File]::ReadAllText($_) }) -join "`n"
foreach ($prohibitedSurface in @(
    'Assembly-CSharp',
    'System.IO',
    'System.Diagnostics',
    'System.Environment',
    'System.Reflection')) {
    if ($uiSourceText -match [Regex]::Escape($prohibitedSurface)) {
        throw "The compile-time UI boundary unexpectedly consumes $prohibitedSurface."
    }
}

Write-Output 'S1-06 acceptance validation passed for real and hosted DSP/Unity references with complete consumed-surface coverage.'
