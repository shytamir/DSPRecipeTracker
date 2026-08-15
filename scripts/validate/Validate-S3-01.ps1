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

& (Join-Path $PSScriptRoot 'Validate-S1-06.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-01 Local/Hosted Release validation failed with exit code $LASTEXITCODE."
}

$modelPath = Join-Path $repoRoot 'src\DSPRecipeTracker\RecipePresentation.cs'
$modelText = [IO.File]::ReadAllText($modelPath)
foreach ($prohibitedTerm in @(
    'BepInEx',
    'UnityEngine',
    'GameMain',
    'LDB.',
    'RecipeProto',
    'ItemProto',
    'StorageComponent',
    'MonoBehaviour',
    'GameObject',
    'Sprite',
    'Harmony',
    'System.Reflection',
    'System.IO'
)) {
    if ($modelText -match [Regex]::Escape($prohibitedTerm)) {
        throw "RecipePresentation contains prohibited runtime or UI dependency term $prohibitedTerm."
    }
}

$testProjectPath = Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'
$testProjectText = [IO.File]::ReadAllText($testProjectPath)
if ($testProjectText -notmatch 'RecipePresentation\.cs') {
    throw 'The deterministic tests do not link RecipePresentation.cs directly.'
}

Write-Output 'S3-01 acceptance validation passed for ordered one-through-six direct ingredients, exact sufficiency arithmetic, machine-only warning values, explicit row failures, structural frame equality, opaque icon pass-through, changed-only bounded Debug diagnostics, and UI/runtime separation.'
