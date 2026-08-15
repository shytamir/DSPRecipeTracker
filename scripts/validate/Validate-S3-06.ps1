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

& (Join-Path $PSScriptRoot 'Validate-S3-05.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-05 dependency validation failed with exit code $LASTEXITCODE."
}

$pluginText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'src\DSPRecipeTracker\DSPRecipeTrackerPlugin.cs'))
$orchestratorText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerOrchestrator.cs'))
$testText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\Program.cs'))
$procedurePath = Join-Path $repoRoot 'docs\OWNER-VALIDATION.md'
if (-not (Test-Path -LiteralPath $procedurePath)) {
    throw 'The Sprint 3 owner validation procedure is missing.'
}
$procedureText = [IO.File]::ReadAllText($procedurePath)

foreach ($requiredText in @(
    'new PinnedRecipeState(diagnostics)',
    'new TrackerPanelUiBoundary(panelAdapter)',
    'new TrackerPanelDrag(',
    'new ReplicatorPinInput(',
    'new RecipeGridTreatment(',
    'new RecipePresentationInputSource(',
    'new RecipePresentationModel(diagnostics)',
    'new UnityRecipeRowUiAdapter(panelAdapter, nativeFont)',
    'new LiveRecipePresentation(',
    'new MajorInterfaceVisibilityInput(',
    'new TrackerVisibilityControls(',
    'new TrackerOrchestrator(',
    'orchestrator.TryInitialize(PanelGeometry.Create(24f, 84f))',
    'orchestrator?.Refresh()',
    'orchestrator?.Dispose()'
)) {
    if ($pluginText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-06 plugin composition is missing: $requiredText"
    }
}

$cleanupOrder = @(
    'controls.Dispose()',
    'panelDrag.Dispose()',
    'recipePresentation.Dispose()',
    'gridTreatment.Dispose()',
    'pinInput.Dispose()',
    'panel.Dispose()'
)
$previousIndex = -1
foreach ($cleanupCall in $cleanupOrder) {
    $index = $orchestratorText.IndexOf($cleanupCall, [StringComparison]::Ordinal)
    if ($index -le $previousIndex) {
        throw "S3-06 cleanup order is missing or invalid at $cleanupCall."
    }
    $previousIndex = $index
}

foreach ($feature in @('panel', 'drag', 'input', 'treatment', 'presentation', 'controls')) {
    if ($testText -notmatch [Regex]::Escape('unavailableFeature != "' + $feature + '"')) {
        throw "S3-06 deterministic isolation matrix is missing $feature."
    }
}
foreach ($requiredTestText in @(
    'CheckOrchestrationIsolation(isolatedFeature)',
    'releases panel once',
    'releases drag listeners once',
    'releases input listener once',
    'releases treatment once',
    'releases presentation once',
    'releases controls once',
    'leaves callbacks inert',
    'reports shutdown once'
)) {
    if ($testText -notmatch [Regex]::Escape($requiredTestText)) {
        throw "S3-06 lifecycle coverage is missing: $requiredTestText"
    }
}

foreach ($requiredProcedureText in @(
    'artifacts/package/0.1.306/DSPRecipeTracker.dll',
    'DSPRecipeTracker-0.1.306.zip',
    '3840 by 2160',
    'UI Layout Reference Height',
    'Native pinning and live recipe presentation',
    'Dragging, bounds, and contained input',
    'Automatic and manual visibility',
    'Lifecycle and cleanup symptoms',
    'Tech Tree',
    'Dyson Sphere Editor',
    'Inventory',
    'Replicator',
    'Statistics',
    'Dashboard',
    'PASS | FAIL | UNEXPECTED',
    'If every group passes, no screenshot or log is needed'
)) {
    if ($procedureText -notmatch [Regex]::Escape($requiredProcedureText)) {
        throw "S3-06 owner procedure is incomplete: $requiredProcedureText"
    }
}
foreach ($prohibitedProcedureText in @(
    'tracker navigation',
    '1080-by-1920',
    'is a supported release',
    'is publication-ready'
)) {
    if ($procedureText -match [Regex]::Escape($prohibitedProcedureText)) {
        throw "S3-06 owner procedure contains an excluded or unsupported claim: $prohibitedProcedureText"
    }
}

$diagnosticSources = @(
    'PinnedRecipeState.cs',
    'ReplicatorPinInput.cs',
    'RecipeGridTreatment.cs',
    'LiveRecipePresentation.cs',
    'RecipeRowPresentation.cs',
    'MajorInterfaceVisibility.cs',
    'TrackerPanelDrag.cs',
    'TrackerOrchestrator.cs'
)
$diagnosticText = ($diagnosticSources | ForEach-Object {
    [IO.File]::ReadAllText((Join-Path $repoRoot ('src\DSPRecipeTracker\' + $_)))
}) -join "`n"
foreach ($diagnosticTerm in @(
    'tracker-state action=',
    'replicator-pin-input action=',
    'recipe-grid-treatment action=',
    'live-recipe-refresh action=',
    'recipe-rows action=',
    'major-interface ',
    'tracker-drag action=',
    'tracker-orchestration action=',
    'tracker-orchestration visibility='
)) {
    if ($diagnosticText -notmatch [Regex]::Escape($diagnosticTerm)) {
        throw "S3-06 diagnostic coverage is missing: $diagnosticTerm"
    }
}

Write-Output 'S3-06 acceptance validation passed for complete plugin composition, per-feature initialization isolation, ordered one-time cleanup, inert callbacks, bounded diagnostic coverage, and the self-contained owner procedure. Runtime execution remains pending owner validation.'
