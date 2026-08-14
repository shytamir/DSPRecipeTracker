[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$deterministicGate = Join-Path $PSScriptRoot 'Validate-S1-04.ps1'

& $deterministicGate
if ($LASTEXITCODE -ne 0) {
    throw "S2-01 deterministic tests failed with exit code $LASTEXITCODE."
}

$stateSource = Join-Path $repoRoot 'src\DSPRecipeTracker\PinnedRecipeState.cs'
$stateText = [IO.File]::ReadAllText($stateSource)
foreach ($prohibitedTerm in @(
    'BepInEx',
    'UnityEngine',
    'GameMain',
    'RecipeProto',
    'Inventory',
    'Craft',
    'Factory',
    'ConfigurationManager',
    'System.IO'
)) {
    if ($stateText -match [Regex]::Escape($prohibitedTerm)) {
        throw "PinnedRecipeState contains prohibited dependency term $prohibitedTerm."
    }
}

$testProject = Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'
$testProjectText = [IO.File]::ReadAllText($testProject)
if ($testProjectText -notmatch 'PinnedRecipeState\.cs') {
    throw 'The deterministic tests do not link the S2-01 state source directly.'
}

Write-Output 'S2-01 acceptance validation passed for transient pin ordering, capacity, removal, and bounded Debug diagnostics without runtime dependencies.'
