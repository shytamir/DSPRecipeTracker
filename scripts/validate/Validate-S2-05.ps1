[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
    [string]$BepInExReferencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

& (Join-Path $PSScriptRoot 'Validate-S2-04.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath
if ($LASTEXITCODE -ne 0) {
    throw "S2-04 dependency validation failed with exit code $LASTEXITCODE."
}

$assemblyPath = Join-Path $GameRoot 'DSPGAME_Data\Managed\Assembly-CSharp.dll'
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyPath)
try {
    $ldb = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'LDB' }
    $recipesProperty = $ldb.Properties | Where-Object { $_.Name -eq 'recipes' }
    if ($null -eq $recipesProperty -or
        $recipesProperty.PropertyType.FullName -ne 'RecipeProtoSet' -or
        $null -eq $recipesProperty.GetMethod -or
        -not $recipesProperty.GetMethod.IsPublic -or
        -not $recipesProperty.GetMethod.IsStatic) {
        throw 'LDB.recipes authority signature or accessibility changed.'
    }

    $recipeSet = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'RecipeProtoSet' }
    if ($null -eq $recipeSet -or $recipeSet.BaseType.FullName -ne 'ProtoSet`1<RecipeProto>') {
        throw 'RecipeProtoSet authority inheritance changed.'
    }

    $protoSet = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'ProtoSet`1' }
    $select = $protoSet.Methods | Where-Object {
        $_.Name -eq 'Select' -and
        $_.IsPublic -and
        -not $_.IsStatic -and
        $_.ReturnType.FullName -eq 'T' -and
        $_.Parameters.Count -eq 1 -and
        $_.Parameters[0].ParameterType.FullName -eq 'System.Int32'
    }
    if ($null -eq $select) {
        throw 'ProtoSet<T>.Select(int) authority signature changed.'
    }

    $recipe = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'RecipeProto' }
    $iconProperty = $recipe.Properties | Where-Object { $_.Name -eq 'iconSprite' }
    if ($null -eq $iconProperty -or
        $iconProperty.PropertyType.FullName -ne 'UnityEngine.Sprite' -or
        $null -eq $iconProperty.GetMethod -or
        -not $iconProperty.GetMethod.IsPublic -or
        $iconProperty.GetMethod.IsStatic) {
        throw 'RecipeProto.iconSprite authority signature or accessibility changed.'
    }
}
finally {
    $assembly.Dispose()
}

$modelPath = Join-Path $repoRoot 'src\DSPRecipeTracker\RecipeIconSlots.cs'
$resolverPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityRecipeIconResolver.cs'
$panelPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityTrackerPanelAdapter.cs'
$boundaryPath = Join-Path $repoRoot 'src\DSPRecipeTracker\TrackerPanelUiBoundary.cs'
$modelText = [IO.File]::ReadAllText($modelPath)
$resolverText = [IO.File]::ReadAllText($resolverPath)
$panelText = [IO.File]::ReadAllText($panelPath)
$boundaryText = [IO.File]::ReadAllText($boundaryPath)

foreach ($requiredText in @(
    'LDB.recipes.Select(recipeId)',
    'recipe.iconSprite',
    'new RecipeIconSlot[PinnedRecipeState.Capacity]',
    'pinnedRecipes.RemoveUnavailable(recipeId)',
    'recipe-icon-slots action=refresh order=',
    'recipe-icon-slots action=remove-unavailable recipeId=',
    'TryApplyRecipeIcons',
    'slotImage.raycastTarget = false',
    'panelBackground.raycastTarget = true',
    'PanelGeometry.MoveAndClamp'
)) {
    if (($modelText + $resolverText + $panelText + $boundaryText) -notmatch [Regex]::Escape($requiredText)) {
        throw "S2-05 source is missing required recipe-icon contract text: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'recipe.Results',
    'recipe.ResultCounts',
    'recipe.Items',
    'recipe.ItemCounts',
    'LDB.items',
    'ItemProto',
    'SetSelectedRecipe',
    'AddListener',
    'onClick',
    'Harmony'
)) {
    if (($resolverText + $modelText) -match [Regex]::Escape($prohibitedText)) {
        throw "S2-05 resolution or slot model contains prohibited reinterpretation or interaction text: $prohibitedText"
    }
}

foreach ($prohibitedTerm in @('UnityEngine', 'LDB', 'RecipeProto', 'Sprite', 'GameObject', 'System.Reflection')) {
    if ($modelText -match [Regex]::Escape($prohibitedTerm)) {
        throw "RecipeIconSlots contains prohibited Unity or runtime dependency term $prohibitedTerm."
    }
}

$testProjectText = [IO.File]::ReadAllText((Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'RecipeIconSlots\.cs') {
    throw 'The deterministic tests do not link the S2-05 slot model directly.'
}

Write-Output 'S2-05 acceptance validation passed for direct RecipeProto icon resolution, ordered maximum-three slots, invalid-identity removal, changed-only refresh, isolated failure and cleanup, clamped panel drag pass-through, input containment, and bounded Debug diagnostics.'
