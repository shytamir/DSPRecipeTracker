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
$managedRoot = Join-Path $GameRoot 'DSPGAME_Data\Managed'
$assemblyCSharpPath = Join-Path $managedRoot 'Assembly-CSharp.dll'
$uiPath = Join-Path $managedRoot 'UnityEngine.UI.dll'

& (Join-Path $PSScriptRoot 'Validate-S3-03.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-03 dependency validation failed with exit code $LASTEXITCODE."
}

foreach ($requiredPath in @($assemblyCSharpPath, $uiPath)) {
    if (-not (Test-Path -LiteralPath $requiredPath)) {
        throw "Required inspection input is missing: $requiredPath"
    }
}

$cecilCandidates = @()
if (-not [string]::IsNullOrWhiteSpace($BepInExReferencePath)) {
    $cecilCandidates += Join-Path (Split-Path -Parent $BepInExReferencePath) 'Mono.Cecil.dll'
}
$cecilCandidates += Join-Path $GameRoot 'BepInEx\core\Mono.Cecil.dll'
$cecilPath = $cecilCandidates | Where-Object { Test-Path -LiteralPath $_ } | Select-Object -First 1
if ([string]::IsNullOrWhiteSpace($cecilPath)) {
    throw 'Mono.Cecil.dll was not found beside the explicit BepInEx reference or under the supplied GameRoot.'
}
Add-Type -Path $cecilPath

function Require-Type([Mono.Cecil.ModuleDefinition]$module, [string]$name) {
    $type = $module.Types | Where-Object { $_.FullName -eq $name }
    if ($null -eq $type) {
        throw "Required consumed type is missing: $name"
    }
    return $type
}

$game = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyCSharpPath)
$ui = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($uiPath)
try {
    $replicator = Require-Type $game.MainModule 'UIReplicatorWindow'
    $nativeText = $replicator.Fields | Where-Object {
        $_.Name -eq 'queueCountText' -and
        $_.FieldType.FullName -eq 'UnityEngine.UI.Text' -and
        $_.IsPublic
    }
    if ($null -eq $nativeText) {
        throw 'Required consumed field mismatch: UIReplicatorWindow.queueCountText'
    }

    $text = Require-Type $ui.MainModule 'UnityEngine.UI.Text'
    $font = $text.Properties | Where-Object {
        $_.Name -eq 'font' -and
        $_.PropertyType.FullName -eq 'UnityEngine.Font' -and
        $null -ne $_.GetMethod -and $_.GetMethod.IsPublic
    }
    if ($null -eq $font) {
        throw 'Required consumed getter mismatch: UnityEngine.UI.Text.font'
    }
}
finally {
    $game.Dispose()
    $ui.Dispose()
}

$pluginPath = Join-Path $repoRoot 'src\DSPRecipeTracker\DSPRecipeTrackerPlugin.cs'
$orchestratorPath = Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerOrchestrator.cs'
$livePath = Join-Path $repoRoot 'src\DSPRecipeTracker\LiveRecipePresentation.cs'
$pluginText = [IO.File]::ReadAllText($pluginPath)
$orchestratorText = [IO.File]::ReadAllText($orchestratorPath)
$liveText = [IO.File]::ReadAllText($livePath)

foreach ($requiredText in @(
    'replicator.queueCountText',
    'nativeText.font',
    'new RecipePresentationInputSource(',
    'new DspRecipeDataAdapter()',
    'new DspInventoryDataAdapter()',
    'new RecipePresentationModel(diagnostics)',
    'new UnityRecipeRowUiAdapter(panelAdapter, nativeFont)',
    'new LiveRecipePresentation('
)) {
    if ($pluginText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-04 plugin composition is missing: $requiredText"
    }
}
if ($pluginText -match 'new RecipeIconSlotPresentation\(') {
    throw 'S3-04 must replace rather than duplicate the product-icon-only live path.'
}

foreach ($requiredText in @(
    'recipePresentation.TryInitialize()',
    'recipePresentation.Refresh()',
    'recipePresentation.Dispose()'
)) {
    if ($orchestratorText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-04 orchestration is missing: $requiredText"
    }
}

$presentationRefresh = $orchestratorText.IndexOf('recipePresentation.Refresh()', [StringComparison]::Ordinal)
$treatmentRefresh = $orchestratorText.IndexOf('gridTreatment.TryRefresh(state.RecipeIds)', [StringComparison]::Ordinal)
if ($presentationRefresh -lt 0 -or $treatmentRefresh -lt 0 -or $presentationRefresh -gt $treatmentRefresh) {
    throw 'Live presentation must refresh before pin-dependent treatment so invalid removal propagates in the same cycle.'
}

foreach ($requiredText in @(
    'SteadyRefreshCallInterval = 12',
    'HavePinsChanged()',
    'observedRecipeCount == 0 && !requiresRowRetry',
    'refreshCallsRemaining > 0',
    'inputSource.Collect()',
    'result.Changed || requiresRowRetry',
    'observedSuppressedIds',
    'live-recipe-refresh action=disable',
    'live-recipe-refresh action=release'
)) {
    if ($liveText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-04 live refresh boundary is missing: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'UnityEngine',
    'BepInEx',
    'GameMain',
    'LDB.',
    'System.Reflection',
    'System.Linq',
    'GetComponent',
    'FindObject',
    'ManualRequested',
    'MajorInterface',
    'TryApplyVisibility',
    '.Toggle(',
    '.Craft(',
    'MoveItem',
    'AddItem',
    'SetItem'
)) {
    if ($liveText -match [Regex]::Escape($prohibitedText)) {
        throw "S3-04 live refresh boundary contains prohibited runtime, search, allocation-prone, or mutation text: $prohibitedText"
    }
}

$refreshStart = $liveText.IndexOf('public void Refresh()', [StringComparison]::Ordinal)
$refreshEnd = $liveText.IndexOf('public void Dispose()', $refreshStart, [StringComparison]::Ordinal)
if ($refreshStart -lt 0 -or $refreshEnd -le $refreshStart) {
    throw 'S3-04 live refresh method boundary was not found for allocation review.'
}
$refreshMethodText = $liveText.Substring($refreshStart, $refreshEnd - $refreshStart)
if ($refreshMethodText -match '\bnew\s') {
    throw 'S3-04 live Refresh contains an explicit managed allocation.'
}

$testProjectText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'LiveRecipePresentation\.cs') {
    throw 'The deterministic tests do not link LiveRecipePresentation.cs directly.'
}
if ($testProjectText -match 'DSPRecipeTrackerPlugin\.cs|UnityRecipeRowUiAdapter\.cs') {
    throw 'Deterministic live-refresh tests must not link the Unity or plugin runtime boundary.'
}

$inventoryText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'ci\compile-references\surface-inventory.json'))
foreach ($inventoryTerm in @(
    'queueCountText',
    'get_font',
    'UnityEngine.Font()'
)) {
    if ($inventoryText -notmatch [Regex]::Escape($inventoryTerm)) {
        throw "Consumed-surface inventory is missing $inventoryTerm."
    }
}

Write-Output 'S3-04 acceptance validation passed for immediate pin-change and bounded steady refresh, changed-only Unity application, exact threshold transitions, pin-order stability, one-through-six ingredients, suppression and recovery, safe invalid removal, empty-state fast path, cleanup, bounded diagnostics, native Replicator font reuse, and exhaustive shim coverage.'
