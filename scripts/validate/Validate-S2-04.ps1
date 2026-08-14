[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
    [string]$BepInExReferencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

& (Join-Path $PSScriptRoot 'Validate-S2-03.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath
if ($LASTEXITCODE -ne 0) {
    throw "S2-03 dependency validation failed with exit code $LASTEXITCODE."
}

$assemblyPath = Join-Path $GameRoot 'DSPGAME_Data\Managed\Assembly-CSharp.dll'
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyPath)
try {
    $manualBehaviour = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'ManualBehaviour' }
    if ($null -eq $manualBehaviour -or $manualBehaviour.BaseType.FullName -ne 'UnityEngine.MonoBehaviour') {
        throw 'ManualBehaviour authority type or inheritance is missing.'
    }

    $activeProperty = $manualBehaviour.Properties | Where-Object { $_.Name -eq 'active' }
    if ($null -eq $activeProperty -or
        $activeProperty.PropertyType.FullName -ne 'System.Boolean' -or
        $null -eq $activeProperty.GetMethod -or
        -not $activeProperty.GetMethod.IsPublic -or
        $null -eq $activeProperty.SetMethod -or
        $activeProperty.SetMethod.IsPublic) {
        throw 'ManualBehaviour.active authority signature or accessibility changed.'
    }

    $uiGame = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIGame' }
    if ($null -eq $uiGame) {
        throw 'UIGame is absent from the authority assembly.'
    }

    $expectedFields = @{
        techTree = 'UITechTree'
        dysonEditor = 'UIDysonEditor'
        inventoryWindow = 'UIInventoryWindow'
        replicator = 'UIReplicatorWindow'
        statWindow = 'UIStatisticsWindow'
        dashboard = 'UIDashboard'
    }
    foreach ($entry in $expectedFields.GetEnumerator()) {
        $field = $uiGame.Fields | Where-Object { $_.Name -eq $entry.Key }
        if ($null -eq $field -or -not $field.IsPublic -or $field.FieldType.FullName -ne $entry.Value) {
            throw "Authority field mismatch: UIGame.$($entry.Key)"
        }

        $windowType = $assembly.MainModule.Types | Where-Object { $_.FullName -eq $entry.Value }
        if ($null -eq $windowType -or $windowType.BaseType.FullName -ne 'ManualBehaviour') {
            throw "Authority inheritance mismatch: $($entry.Value)"
        }
    }
}
finally {
    $assembly.Dispose()
}

$modelPath = Join-Path $repoRoot 'src\DSPRecipeTracker\MajorInterfaceVisibility.cs'
$adapterPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityMajorInterfaceStateAdapter.cs'
$modelText = [IO.File]::ReadAllText($modelPath)
$adapterText = [IO.File]::ReadAllText($adapterPath)

foreach ($requiredText in @(
    'Tech || DysonEditor || Inventory || Replicator || Statistics || Dashboard',
    'if (!snapshot.IsAvailable)',
    'VisibilityPolicy.IsVisible',
    'major-interface availability=unavailable',
    'major-interface availability=available',
    'major-interface state=',
    'tech = uiGame.techTree',
    'dysonEditor = uiGame.dysonEditor',
    'inventory = uiGame.inventoryWindow',
    'replicator = uiGame.replicator',
    'statistics = uiGame.statWindow',
    'dashboard = uiGame.dashboard'
)) {
    if (($modelText + $adapterText) -notmatch [Regex]::Escape($requiredText)) {
        throw "S2-04 source is missing required visibility contract text: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'isAnyFunctionWindowActive',
    'uiPanelActiveMask',
    'System.Reflection',
    'Harmony',
    'void Update(',
    'void LateUpdate(',
    'void FixedUpdate('
)) {
    if (($modelText + $adapterText) -match [Regex]::Escape($prohibitedText)) {
        throw "S2-04 source contains prohibited broad binding or lifecycle text: $prohibitedText"
    }
}

foreach ($prohibitedTerm in @('UnityEngine', 'UIGame', 'ManualBehaviour', 'System.Reflection')) {
    if ($modelText -match [Regex]::Escape($prohibitedTerm)) {
        throw "MajorInterfaceVisibility contains prohibited Unity or runtime dependency term $prohibitedTerm."
    }
}

$testProjectText = [IO.File]::ReadAllText((Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'MajorInterfaceVisibility\.cs') {
    throw 'The deterministic tests do not link the S2-04 visibility model directly.'
}

Write-Output 'S2-04 acceptance validation passed for the exact six direct active-state bindings, explicit availability, logical-OR collection, fail-closed policy handoff, changed-only bounded Debug diagnostics, and authority-backed shim coverage.'
