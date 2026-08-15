[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
    [string]$BepInExReferencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

& (Join-Path $PSScriptRoot 'Validate-S2-02.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath
if ($LASTEXITCODE -ne 0) {
    throw "S2-02 dependency validation failed with exit code $LASTEXITCODE."
}

$managedRoot = Join-Path $GameRoot 'DSPGAME_Data\Managed'
$assemblyCSharpPath = Join-Path $managedRoot 'Assembly-CSharp.dll'
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyCSharpPath)
try {
    $window = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIReplicatorWindow' }
    if ($null -eq $window) {
        throw 'UIReplicatorWindow is absent from the authority assembly.'
    }

    $expectedFields = @(
        @{ Name = 'recipeBg'; Type = 'UnityEngine.UI.Image'; Public = $true },
        @{ Name = 'recipeProtoArray'; Type = 'RecipeProto[]'; Public = $false }
    )
    foreach ($expectedField in $expectedFields) {
        $field = $window.Fields | Where-Object { $_.Name -eq $expectedField.Name }
        if ($null -eq $field -or
            $field.FieldType.FullName -ne $expectedField.Type -or
            $field.IsPublic -ne $expectedField.Public) {
            throw "Authority field mismatch: UIReplicatorWindow.$($expectedField.Name)"
        }
    }

    $hitTestMethod = $window.Methods | Where-Object { $_.Name -eq 'TestMouseRecipeIndex' }
    $hitTestInstructions = @($hitTestMethod.Body.Instructions | ForEach-Object { $_.ToString() }) -join "`n"
    foreach ($requiredInstruction in @('ldc.r4 46', 'ldc.i4.s 14', 'ldc.i4.8', 'mul', 'add')) {
        if ($hitTestInstructions -notmatch [Regex]::Escape($requiredInstruction)) {
            throw "Native recipe-grid hit testing no longer contains $requiredInstruction."
        }
    }
}
finally {
    $assembly.Dispose()
}

$modelPath = Join-Path $repoRoot 'src\DSPRecipeTracker\RecipeGridTreatment.cs'
$adapterPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityRecipeGridTreatmentAdapter.cs'
$modelText = [IO.File]::ReadAllText($modelPath)
$adapterText = [IO.File]::ReadAllText($adapterPath)

foreach ($requiredText in @(
    'CellCount = 120',
    'PinnedMarkerState = 0x1',
    'GridColumns = 14',
    'GridRows = 8',
    'CellSize = 46f',
    'MarkerCapacity = PinnedRecipeState.Capacity',
    'CornerLength = 10f',
    'CornerThickness = 2f',
    'new Color(0.2f, 0.75f, 0.25f, 0.95f)',
    'raycastTarget = false',
    'SetParent(window.recipeBg.transform, false)',
    'markerObjects[markerIndex].SetActive(true)',
    'markerObjects[markerIndex].SetActive(false)'
)) {
    if (($modelText + $adapterText) -notmatch [Regex]::Escape($requiredText)) {
        throw "S2-03 source is missing required treatment contract text: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'recipeStateArray',
    'recipeStateBuffer',
    'recipeBgMat',
    'ComputeBuffer',
    'Material',
    '_StateBuffer',
    '_FilterColor',
    '_BansColor',
    'Object.Instantiate(window.recipeBg',
    'new Color(0.78f, 0.22f, 0.22f',
    '.SetValue(',
    'Harmony',
    'void Update(',
    'void LateUpdate(',
    'void FixedUpdate('
)) {
    if ($adapterText -match [Regex]::Escape($prohibitedText)) {
        throw "UnityRecipeGridTreatmentAdapter contains prohibited original-state or hot-loop text: $prohibitedText"
    }
}

foreach ($prohibitedTerm in @('UnityEngine', 'UIReplicatorWindow', 'RecipeProto', 'ComputeBuffer', 'Material', 'System.Reflection')) {
    if ($modelText -match [Regex]::Escape($prohibitedTerm)) {
        throw "RecipeGridTreatment contains prohibited Unity or runtime dependency term $prohibitedTerm."
    }
}

$testProjectText = [IO.File]::ReadAllText((Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'RecipeGridTreatment\.cs') {
    throw 'The deterministic tests do not link the S2-03 treatment model directly.'
}

Write-Output 'S2-03 acceptance validation passed for pinned-only green corner markers, neutral unpinned cells, native 14-by-8 geometry, independent state ownership, changed-only refresh, non-raycasting presentation, fail-soft isolation, one-time cleanup, and bounded Debug diagnostics.'
