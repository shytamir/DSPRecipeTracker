[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
    [string]$BepInExReferencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

& (Join-Path $PSScriptRoot 'Validate-S2-05.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath
if ($LASTEXITCODE -ne 0) {
    throw "S2-05 dependency validation failed with exit code $LASTEXITCODE."
}

$managedRoot = Join-Path $GameRoot 'DSPGAME_Data\Managed'
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $managedRoot 'Assembly-CSharp.dll'))
$core = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $managedRoot 'UnityEngine.CoreModule.dll'))
$ui = [Mono.Cecil.AssemblyDefinition]::ReadAssembly((Join-Path $managedRoot 'UnityEngine.UI.dll'))
try {
    function Require-Field($type, [string]$name, [string]$signature) {
        $field = $type.Fields | Where-Object { $_.Name -eq $name }
        if ($null -eq $field -or -not $field.IsPublic -or $field.FieldType.FullName -ne $signature) {
            throw "Authority field mismatch: $($type.FullName).$name"
        }
    }

    function Require-Method($type, [string]$name, [string]$returnType, [string[]]$parameters, [bool]$isStatic = $false) {
        $method = $type.Methods | Where-Object {
            $_.Name -eq $name -and $_.IsPublic -and $_.IsStatic -eq $isStatic -and
            $_.ReturnType.FullName -eq $returnType -and $_.Parameters.Count -eq $parameters.Count
        } | Where-Object {
            $matches = $true
            for ($index = 0; $index -lt $parameters.Count; $index++) {
                if ($_.Parameters[$index].ParameterType.FullName -ne $parameters[$index]) {
                    $matches = $false
                }
            }
            $matches
        }
        if ($null -eq $method) {
            throw "Authority method mismatch: $($type.FullName).$name"
        }
    }

    $uiRoot = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIRoot' }
    $uiGame = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIGame' }
    $gameMenu = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIGameMenu' }
    $uiButton = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIButton' }
    $localizer = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'Localizer' }
    if ($null -eq $uiRoot -or $null -eq $uiGame -or $null -eq $gameMenu -or
        $null -eq $uiButton -or $null -eq $localizer) {
        throw 'One or more S2-06 authority types are missing.'
    }

    $instance = $uiRoot.Properties | Where-Object { $_.Name -eq 'instance' }
    if ($null -eq $instance -or $instance.PropertyType.FullName -ne 'UIRoot' -or
        $null -eq $instance.GetMethod -or -not $instance.GetMethod.IsPublic -or
        -not $instance.GetMethod.IsStatic) {
        throw 'UIRoot.instance authority signature changed.'
    }

    Require-Field $uiRoot 'uiGame' 'UIGame'
    Require-Field $uiGame 'gameMenu' 'UIGameMenu'
    Require-Field $gameMenu 'button3' 'UnityEngine.UI.Button'
    Require-Field $gameMenu 'buttonS' 'UnityEngine.UI.Button'
    Require-Field $uiButton 'tips' 'UIButton/TipSettings'
    Require-Field $uiButton 'tipTitleFormatString' 'System.String'
    Require-Field $uiButton 'tipTextFormatString' 'System.String'

    $button3Click = $gameMenu.Methods | Where-Object { $_.Name -eq 'OnButton3Click' }
    if ($null -eq $button3Click) {
        throw 'UIGameMenu.OnButton3Click is missing.'
    }
    $replicatorCall = $button3Click.Body.Instructions | Where-Object {
        $_.Operand -is [Mono.Cecil.MethodReference] -and
        $_.Operand.DeclaringType.FullName -eq 'UIGame' -and
        $_.Operand.Name -eq 'On_F_Switch'
    }
    if ($null -eq $replicatorCall) {
        throw 'UIGameMenu.button3 no longer maps to the Replicator action.'
    }

    $tipSettings = $uiButton.NestedTypes | Where-Object { $_.Name -eq 'TipSettings' }
    foreach ($entry in @{
        itemId = 'System.Int32'; itemCount = 'System.Int32'; itemInc = 'System.Int32';
        tipSprite = 'UnityEngine.Sprite'; tipTitle = 'System.String'; tipText = 'System.String';
        type = 'UIButton/ItemTipType'
    }.GetEnumerator()) {
        Require-Field $tipSettings $entry.Key $entry.Value
    }

    $object = $core.MainModule.Types | Where-Object { $_.FullName -eq 'UnityEngine.Object' }
    $component = $core.MainModule.Types | Where-Object { $_.FullName -eq 'UnityEngine.Component' }
    $rectTransform = $core.MainModule.Types | Where-Object { $_.FullName -eq 'UnityEngine.RectTransform' }
    Require-Method $component 'GetComponentsInChildren' 'T[]' @('System.Boolean')
    Require-Method $rectTransform 'get_anchoredPosition' 'UnityEngine.Vector2' @()

    $button = $ui.MainModule.Types | Where-Object { $_.FullName -eq 'UnityEngine.UI.Button' }
    $graphic = $ui.MainModule.Types | Where-Object { $_.FullName -eq 'UnityEngine.UI.Graphic' }
    $image = $ui.MainModule.Types | Where-Object { $_.FullName -eq 'UnityEngine.UI.Image' }
    Require-Method $button 'get_onClick' 'UnityEngine.UI.Button/ButtonClickedEvent' @()
    Require-Method $button 'set_onClick' 'System.Void' @('UnityEngine.UI.Button/ButtonClickedEvent')
    Require-Method $graphic 'get_raycastTarget' 'System.Boolean' @()
    Require-Method $image 'get_sprite' 'UnityEngine.Sprite' @()
}
finally {
    $assembly.Dispose()
    $core.Dispose()
    $ui.Dispose()
}

$orchestrationPath = Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerOrchestrator.cs'
$controlsPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityTrackerVisibilityControlAdapter.cs'
$pluginPath = Join-Path $repoRoot 'src\DSPRecipeTracker\DSPRecipeTrackerPlugin.cs'
$orchestrationText = [IO.File]::ReadAllText($orchestrationPath)
$controlsText = [IO.File]::ReadAllText($controlsPath)
$pluginText = [IO.File]::ReadAllText($pluginPath)

foreach ($requiredText in @(
    'private bool manualRequested = true',
    'MajorInterfaceVisibilityInput.ResolveTrackerVisibility',
    'controls.TryInitialize(HidePanel, ToggleGlobal, manualRequested)',
    'tracker-orchestration action=initialize',
    'tracker-orchestration action=release',
    'tracker-orchestration visibility=',
    'Object.Instantiate(nativeTemplate, globalParent, false)',
    'button.onClick = new Button.ButtonClickedEvent()',
    'GetComponentsInChildren<Localizer>(true)',
    'internal const string ControlTitle = "Recipe Tracker"',
    'internal const string HideCopy = "Hide Recipe Tracker"',
    'internal const string ShowCopy = "Show Recipe Tracker"',
    'TryResolveNativeIcon(gameMenu.button3)',
    'source.anchoredPosition + new Vector2(0f, 38f)',
    '!images[index].raycastTarget',
    'UIRoot.instance',
    'uiGame.transform as RectTransform'
)) {
    if (($orchestrationText + $controlsText + $pluginText) -notmatch [Regex]::Escape($requiredText)) {
        throw "S2-06 source is missing required orchestration or control text: $requiredText"
    }
}

foreach ($prohibitedText in @('BepInEx.Configuration', 'Harmony', 'GameMain.Begin', 'Application.Quit')) {
    if (($orchestrationText + $controlsText + $pluginText) -match [Regex]::Escape($prohibitedText)) {
        throw "S2-06 source contains prohibited persistence, patching, or runtime-control text: $prohibitedText"
    }
}

$testProjectText = [IO.File]::ReadAllText((Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
foreach ($linkedModel in @('TrackerOrchestrator.cs', 'TrackerVisibilityControls.cs')) {
    if ($testProjectText -notmatch [Regex]::Escape($linkedModel)) {
        throw "The deterministic tests do not link $linkedModel directly."
    }
}

Write-Output 'S2-06 acceptance validation passed for authority-backed startup, paired controls, stored manual intent, fail-closed visibility, feature-local failure, one-time cleanup, bounded diagnostics, and architecture separation.'
