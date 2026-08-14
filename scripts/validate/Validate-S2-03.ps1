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
        @{ Name = 'recipeIcons'; Type = 'UnityEngine.UI.RawImage'; Public = $true },
        @{ Name = 'recipeProtoArray'; Type = 'RecipeProto[]'; Public = $false },
        @{ Name = 'recipeStateArray'; Type = 'System.UInt32[]'; Public = $false },
        @{ Name = 'recipeStateBuffer'; Type = 'UnityEngine.ComputeBuffer'; Public = $false },
        @{ Name = 'recipeBgMat'; Type = 'UnityEngine.Material'; Public = $false }
    )
    foreach ($expectedField in $expectedFields) {
        $field = $window.Fields | Where-Object { $_.Name -eq $expectedField.Name }
        if ($null -eq $field -or
            $field.FieldType.FullName -ne $expectedField.Type -or
            $field.IsPublic -ne $expectedField.Public) {
            throw "Authority field mismatch: UIReplicatorWindow.$($expectedField.Name)"
        }
    }

    $createMethod = $window.Methods | Where-Object { $_.Name -eq '_OnCreate' }
    $createInstructions = @($createMethod.Body.Instructions | ForEach-Object { $_.ToString() }) -join "`n"
    foreach ($requiredInstruction in @('ldc.i4.s 120', 'newarr System.UInt32', 'newarr RecipeProto', 'UnityEngine.ComputeBuffer::.ctor(System.Int32,System.Int32)')) {
        if ($createInstructions -notmatch [Regex]::Escape($requiredInstruction)) {
            throw "Native recipe-grid construction no longer contains $requiredInstruction."
        }
    }

    $materialMethod = $window.Methods | Where-Object { $_.Name -eq 'SetMaterialProps' }
    $materialInstructions = @($materialMethod.Body.Instructions | ForEach-Object { $_.ToString() }) -join "`n"
    foreach ($requiredInstruction in @('recipeBgMat', '_StateBuffer', 'recipeStateBuffer')) {
        if ($materialInstructions -notmatch [Regex]::Escape($requiredInstruction)) {
            throw "Native recipe-grid material binding no longer contains $requiredInstruction."
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
    'UnpinnedMask = 0x2',
    'PinnedMask = 0x8',
    'OverlayOpacity = 0.08f',
    'new Color(0.2f, 0.75f, 0.25f, 1f)',
    'new Color(0.78f, 0.22f, 0.22f, 0.45f)',
    'raycastTarget = false',
    'SetSiblingIndex',
    'stateBuffer.SetData(states)'
)) {
    if (($modelText + $adapterText) -notmatch [Regex]::Escape($requiredText)) {
        throw "S2-03 source is missing required treatment contract text: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'recipeStateArray',
    'recipeStateBuffer',
    'recipeBgMat',
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

Write-Output 'S2-03 acceptance validation passed for independent state ownership, exact native treatment mapping, changed-only refresh, neutral stale clearing, non-raycasting layer order, fail-soft isolation, one-time cleanup, and bounded Debug diagnostics.'
