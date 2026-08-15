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
$gamePath = Join-Path $managedRoot 'Assembly-CSharp.dll'
$corePath = Join-Path $managedRoot 'UnityEngine.CoreModule.dll'
$uiModulePath = Join-Path $managedRoot 'UnityEngine.UIModule.dll'
$uiPath = Join-Path $managedRoot 'UnityEngine.UI.dll'

& (Join-Path $PSScriptRoot 'Validate-S3-04.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-04 dependency validation failed with exit code $LASTEXITCODE."
}

foreach ($requiredPath in @($gamePath, $corePath, $uiModulePath, $uiPath)) {
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

function Require-PublicGetter($type, [string]$name, [string]$returnType) {
    $property = $type.Properties | Where-Object {
        $_.Name -eq $name -and
        $_.PropertyType.FullName -eq $returnType -and
        $null -ne $_.GetMethod -and
        $_.GetMethod.IsPublic
    }
    if ($null -eq $property) {
        throw "Required consumed getter mismatch: $($type.FullName).$name"
    }
}

$game = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($gamePath)
$core = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($corePath)
$uiModule = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($uiModulePath)
$ui = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($uiPath)
try {
    $root = Require-Type $game.MainModule 'UIRoot'
    $overlayCanvas = $root.Fields | Where-Object {
        $_.Name -eq 'overlayCanvas' -and
        $_.FieldType.FullName -eq 'UnityEngine.Canvas' -and
        $_.IsPublic
    }
    if ($null -eq $overlayCanvas) {
        throw 'Required consumed field mismatch: UIRoot.overlayCanvas'
    }

    $layoutHandler = Require-Type $game.MainModule 'UICanvasScalerHandler'
    $layoutHeight = $layoutHandler.Methods | Where-Object {
        $_.Name -eq 'GetSuggestUILayoutHeight' -and
        $_.ReturnType.FullName -eq 'System.Int32' -and
        $_.Parameters.Count -eq 1 -and
        $_.Parameters[0].ParameterType.FullName -eq 'System.Int32' -and
        $_.IsPublic -and $_.IsStatic
    }
    if ($null -eq $layoutHeight) {
        throw 'Required inspected method mismatch: UICanvasScalerHandler.GetSuggestUILayoutHeight(int)'
    }

    $canvas = Require-Type $uiModule.MainModule 'UnityEngine.Canvas'
    Require-PublicGetter $canvas 'scaleFactor' 'System.Single'

    $rectTransform = Require-Type $core.MainModule 'UnityEngine.RectTransform'
    $rect = Require-Type $core.MainModule 'UnityEngine.Rect'
    Require-PublicGetter $rectTransform 'rect' 'UnityEngine.Rect'
    Require-PublicGetter $rect 'width' 'System.Single'
    Require-PublicGetter $rect 'height' 'System.Single'

    $pointer = Require-Type $ui.MainModule 'UnityEngine.EventSystems.PointerEventData'
    Require-PublicGetter $pointer 'delta' 'UnityEngine.Vector2'

    $eventType = Require-Type $ui.MainModule 'UnityEngine.EventSystems.EventTriggerType'
    foreach ($expectedEvent in @(@('Drag', 5), @('EndDrag', 14))) {
        $field = $eventType.Fields | Where-Object {
            $_.Name -eq $expectedEvent[0] -and $_.IsPublic -and $_.IsStatic
        }
        if ($null -eq $field -or [int]$field.Constant -ne [int]$expectedEvent[1]) {
            throw "Required event value mismatch: EventTriggerType.$($expectedEvent[0])"
        }
    }
}
finally {
    $game.Dispose()
    $core.Dispose()
    $uiModule.Dispose()
    $ui.Dispose()
}

$dragPath = Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerPanelDrag.cs'
$unityDragPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityTrackerPanelDragAdapter.cs'
$panelPath = Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerPanelUiBoundary.cs'
$pluginPath = Join-Path $repoRoot 'src\DSPRecipeTracker\DSPRecipeTrackerPlugin.cs'
$orchestratorPath = Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerOrchestrator.cs'
$dragText = [IO.File]::ReadAllText($dragPath)
$unityDragText = [IO.File]::ReadAllText($unityDragPath)
$panelText = [IO.File]::ReadAllText($panelPath)
$pluginText = [IO.File]::ReadAllText($pluginPath)
$orchestratorText = [IO.File]::ReadAllText($orchestratorPath)

foreach ($requiredText in @(
    'EventTriggerType.Drag',
    'EventTriggerType.EndDrag',
    'new EventTrigger.TriggerEvent()',
    'new EventTrigger.Entry',
    'pointerEvent.delta',
    'new DragDelta(delta.x, -delta.y)',
    'overlayCanvas.scaleFactor',
    'parent.rect',
    'rectangle.width',
    'rectangle.height',
    'callback.RemoveListener',
    'trigger.triggers.Remove',
    'UnityEngine.Object.Destroy(ownedTrigger)'
)) {
    if ($unityDragText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-05 Unity drag adapter is missing required wiring or cleanup: $requiredText"
    }
}

foreach ($requiredText in @(
    'screenDelta.Horizontal / scaleFactor',
    'screenDelta.Vertical / scaleFactor',
    'TryCreateParentBounds',
    'panel.TryApplyDrag(layoutDelta, bounds, out var clamped)',
    'panel.TryReclamp(nextBounds, out var corrected)',
    'tracker-drag action=initialize',
    'tracker-drag action=complete',
    'tracker-drag action=bounds',
    'tracker-drag action=clamp-correction',
    'tracker-drag action=disable',
    'tracker-drag action=release'
)) {
    if ($dragText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-05 neutral drag boundary is missing: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'UnityEngine',
    'BepInEx',
    'UIRoot',
    'Screen.',
    'UICanvasScalerHandler',
    'System.Reflection',
    'GameMain',
    'LDB.',
    '.Craft(',
    'MoveItem',
    'AddItem',
    'SetItem'
)) {
    if ($dragText -match [Regex]::Escape($prohibitedText)) {
        throw "S3-05 neutral drag boundary contains prohibited runtime, scaling-algorithm, or mutation text: $prohibitedText"
    }
}

if ($unityDragText -match 'tracker-drag action=') {
    throw 'Pointer-event adapter must not emit per-event drag diagnostics.'
}
if ($panelText -notmatch 'public bool TryReclamp\(' -or
    $panelText -notmatch 'public bool TryApplyDrag\(DragDelta delta, ParentBounds parent, out bool clamped\)') {
    throw 'S3-05 panel boundary is missing isolated drag and reclamp forwarding.'
}
if ($pluginText -notmatch 'new UnityTrackerPanelDragAdapter\(panelAdapter, root\.overlayCanvas\)' -or
    $pluginText -notmatch 'new TrackerPanelDrag\(') {
    throw 'S3-05 plugin composition is missing the live overlay-canvas drag boundary.'
}
foreach ($requiredText in @(
    'panelDrag.TryInitialize()',
    'panelDrag.RefreshBounds()',
    'panelDrag.Dispose()'
)) {
    if ($orchestratorText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-05 orchestration is missing: $requiredText"
    }
}

$testProjectText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'TrackerPanelDrag\.cs') {
    throw 'The deterministic tests do not link TrackerPanelDrag.cs directly.'
}
if ($testProjectText -match 'UnityTrackerPanelDragAdapter\.cs|DSPRecipeTrackerPlugin\.cs') {
    throw 'Deterministic drag tests must not link the Unity or plugin runtime boundary.'
}

$inventoryText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'ci\compile-references\surface-inventory.json'))
foreach ($inventoryTerm in @(
    'UnityEngine.UIModule',
    'get_scaleFactor',
    'get_rect',
    'get_width',
    'get_height',
    'get_delta',
    '"name": "Drag"',
    '"name": "EndDrag"',
    '"name": "overlayCanvas"'
)) {
    if ($inventoryText -notmatch [Regex]::Escape($inventoryTerm)) {
        throw "Consumed-surface inventory is missing $inventoryTerm."
    }
}

Write-Output 'S3-05 acceptance validation passed for real drag/end-drag wiring, live pixel-to-layout scaling, 1080p/1440p Auto bounds, 4k factor-two conversion, changed-bound reclamping, edge/corner and invalid-input behavior, contained noninteractive rows, feature-isolated failure, bounded diagnostics, cleanup, Local/Hosted Release builds, and exhaustive shim coverage.'
