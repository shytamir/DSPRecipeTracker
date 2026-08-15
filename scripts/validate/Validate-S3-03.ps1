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
$uiPath = Join-Path $managedRoot 'UnityEngine.UI.dll'
$textRenderingPath = Join-Path $managedRoot 'UnityEngine.TextRenderingModule.dll'

& (Join-Path $PSScriptRoot 'Validate-S3-02.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-02 dependency validation failed with exit code $LASTEXITCODE."
}

foreach ($requiredPath in @($uiPath, $textRenderingPath)) {
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

function Require-Setter($type, [string]$name, [string]$valueType) {
    $property = $type.Properties | Where-Object {
        $_.Name -eq $name -and $_.PropertyType.FullName -eq $valueType -and
        $null -ne $_.SetMethod -and $_.SetMethod.IsPublic
    }
    if ($null -eq $property) {
        throw "Required consumed setter mismatch: $($type.FullName).$name"
    }
}

$ui = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($uiPath)
$textRendering = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($textRenderingPath)
try {
    $text = Require-Type $ui.MainModule 'UnityEngine.UI.Text'
    $font = Require-Type $textRendering.MainModule 'UnityEngine.Font'
    if ($text.BaseType.FullName -ne 'UnityEngine.UI.MaskableGraphic') {
        throw 'UnityEngine.UI.Text consumed inheritance changed.'
    }
    if ($font.BaseType.FullName -ne 'UnityEngine.Object') {
        throw 'UnityEngine.Font consumed inheritance changed.'
    }
    Require-Setter $text 'font' 'UnityEngine.Font'
    Require-Setter $text 'text' 'System.String'
    Require-Setter $text 'fontSize' 'System.Int32'
    Require-Setter $text 'alignment' 'UnityEngine.TextAnchor'
}
finally {
    $ui.Dispose()
    $textRendering.Dispose()
}

$presentationPath = Join-Path $repoRoot 'src\DSPRecipeTracker\RecipeRowPresentation.cs'
$unityPath = Join-Path $repoRoot 'src\DSPRecipeTracker\UnityRecipeRowUiAdapter.cs'
$presentationText = [IO.File]::ReadAllText($presentationPath)
$unityText = [IO.File]::ReadAllText($unityPath)

foreach ($requiredText in @(
    'RecipeRowLayout.RowTop(rowIndex)',
    'RecipeRowLayout.IngredientLeft(ingredientIndex)',
    'new GameObject("Recipe Row "',
    'AddComponent(typeof(Image))',
    'AddComponent(typeof(Text))',
    'text.font = nativeFont',
    'text.raycastTarget = false',
    'productImage.raycastTarget = false',
    'row.Ingredients[ingredientIndex].Icon.Value is Sprite',
    'IngredientValueTreatment.Sufficient',
    '"TARGET"',
    '"INGREDIENTS"',
    'TextAnchor.MiddleCenter',
    'TextAnchor.MiddleLeft',
    '"Machine Facility Footer"',
    'warningText.text = row.MachineWarning',
    'warningText.gameObject.SetActive(hasWarning)',
    'RecipeRowLayout.ContentHeight',
    'RecipeRowLayout.SeparatorLeft',
    'new Color(0.95f, 0.72f, 0.3f, 1f)',
    'Object.Destroy(ownedRow)'
)) {
    if ($unityText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-03 Unity row adapter is missing required composition or cleanup: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'Button',
    'EventTrigger',
    'onClick',
    'LDB.',
    'GameMain',
    'System.Reflection',
    'Harmony',
    '.Craft(',
    'GetItemCount',
    'MoveItem',
    'AddItem',
    'SetItem'
)) {
    if ($unityText -match [Regex]::Escape($prohibitedText)) {
        throw "S3-03 Unity row adapter contains prohibited interaction, runtime collection, or mutation text: $prohibitedText"
    }
}

foreach ($requiredPresentationText in @(
    'FormatMachineWarning(row.MachineWarning)',
    'value.Split((char[])null, StringSplitOptions.RemoveEmptyEntries)',
    'public const float RowHeight = 90f',
    'public const float RowSpacing = 90f',
    'public const float ContentHeight = 60f',
    'public const float ProductLabelLeft = 12f',
    'public const float ProductLabelTop = 60f',
    'public const float ProductLabelWidth = 336f',
    'public const float ProductLabelHeight = 30f',
    'string.Join(" ", words)'
)) {
    if ($presentationText -notmatch [Regex]::Escape($requiredPresentationText)) {
        throw "S3-03 presentation model is missing deterministic machine-label formatting: $requiredPresentationText"
    }
}

foreach ($prohibitedTerm in @(
    'BepInEx',
    'UnityEngine',
    'GameMain',
    'LDB.',
    'RecipeProto',
    'ItemProto',
    'StorageComponent',
    'System.Reflection',
    'Harmony'
)) {
    if ($presentationText -match [Regex]::Escape($prohibitedTerm)) {
        throw "RecipeRowPresentation contains prohibited runtime dependency term $prohibitedTerm."
    }
}

$testProjectText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'RecipeRowPresentation\.cs') {
    throw 'The deterministic tests do not link RecipeRowPresentation.cs directly.'
}
if ($testProjectText -match 'UnityRecipeRowUiAdapter\.cs') {
    throw 'The deterministic test process must not link the Unity row adapter.'
}

$inventoryText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'ci\compile-references\surface-inventory.json'))
foreach ($inventoryTerm in @(
    'UnityEngine.TextRenderingModule',
    'UnityEngine.Font',
    'UnityEngine.UI.Text',
    'set_font',
    'set_text',
    'set_fontSize',
    'set_alignment',
    'UnityEngine.TextAnchor'
)) {
    if ($inventoryText -notmatch [Regex]::Escape($inventoryTerm)) {
        throw "Consumed-surface inventory is missing $inventoryTerm."
    }
}

Write-Output 'S3-03 acceptance validation passed for semantic target/ingredient headings, centered readable quantities, a dedicated single-line machine footer, target/ingredient separation, complete ordered one-through-six ingredient rows, fixed-panel containment, non-interactive native resource reuse, failure isolation, cleanup, bounded diagnostics, exact consumed Text/font members, and exhaustive shim coverage.'
