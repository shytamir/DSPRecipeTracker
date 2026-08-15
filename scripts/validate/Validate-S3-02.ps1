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
$assemblyPath = Join-Path $managedRoot 'Assembly-CSharp.dll'

& (Join-Path $PSScriptRoot 'Validate-S3-01.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-01 dependency validation failed with exit code $LASTEXITCODE."
}

if (-not (Test-Path -LiteralPath $assemblyPath)) {
    throw "Required inspection input is missing: $assemblyPath"
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
        throw "Authority type is missing: $name"
    }
    return $type
}

function Require-Field($type, [string]$name, [string]$signature) {
    $field = $type.Fields | Where-Object {
        $_.Name -eq $name -and $_.IsPublic -and -not $_.IsStatic -and
        $_.FieldType.FullName -eq $signature
    }
    if ($null -eq $field) {
        throw "Authority field mismatch: $($type.FullName).$name"
    }
}

function Require-Property($type, [string]$name, [string]$signature, [bool]$isStatic = $false) {
    $property = $type.Properties | Where-Object {
        $_.Name -eq $name -and $_.PropertyType.FullName -eq $signature -and
        $null -ne $_.GetMethod -and $_.GetMethod.IsPublic -and
        $_.GetMethod.IsStatic -eq $isStatic
    }
    if ($null -eq $property) {
        throw "Authority property mismatch: $($type.FullName).$name"
    }
}

function Require-Method($type, [string]$name, [string]$returnType, [string[]]$parameters) {
    $method = $type.Methods | Where-Object {
        $_.Name -eq $name -and $_.IsPublic -and -not $_.IsStatic -and
        $_.ReturnType.FullName -eq $returnType -and
        $_.Parameters.Count -eq $parameters.Count
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

$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyPath)
try {
    $proto = Require-Type $assembly.MainModule 'Proto'
    $recipe = Require-Type $assembly.MainModule 'RecipeProto'
    $item = Require-Type $assembly.MainModule 'ItemProto'
    $itemSet = Require-Type $assembly.MainModule 'ItemProtoSet'
    $protoSet = Require-Type $assembly.MainModule 'ProtoSet`1'
    $ldb = Require-Type $assembly.MainModule 'LDB'
    $gameMain = Require-Type $assembly.MainModule 'GameMain'
    $player = Require-Type $assembly.MainModule 'Player'
    $storage = Require-Type $assembly.MainModule 'StorageComponent'

    Require-Field $proto 'ID' 'System.Int32'
    Require-Field $recipe 'Handcraft' 'System.Boolean'
    Require-Field $recipe 'Items' 'System.Int32[]'
    Require-Field $recipe 'ItemCounts' 'System.Int32[]'
    Require-Property $recipe 'iconSprite' 'UnityEngine.Sprite'
    Require-Property $recipe 'madeFromString' 'System.String'
    Require-Property $item 'iconSprite' 'UnityEngine.Sprite'
    Require-Property $ldb 'recipes' 'RecipeProtoSet' $true
    Require-Property $ldb 'items' 'ItemProtoSet' $true
    Require-Property $gameMain 'mainPlayer' 'Player' $true
    Require-Property $player 'package' 'StorageComponent'
    Require-Method $protoSet 'Select' 'T' @('System.Int32')
    Require-Method $storage 'GetItemCount' 'System.Int32' @('System.Int32')

    if ($itemSet.BaseType.FullName -ne 'ProtoSet`1<ItemProto>') {
        throw 'ItemProtoSet authority inheritance changed.'
    }
}
finally {
    $assembly.Dispose()
}

$runtimePath = Join-Path $repoRoot 'src\DSPRecipeTracker\DspRecipeDataAdapters.cs'
$sourcePath = Join-Path $repoRoot 'src\DSPRecipeTracker\RecipeDataSource.cs'
$runtimeText = [IO.File]::ReadAllText($runtimePath)
$sourceText = [IO.File]::ReadAllText($sourcePath)

foreach ($requiredText in @(
    'recipes = LDB.recipes',
    'items = LDB.items',
    'recipes.Select(recipeId)',
    'recipe.ID',
    'recipe.Items',
    'recipe.ItemCounts',
    'recipe.Handcraft',
    'recipe.madeFromString',
    'recipe.iconSprite',
    'items.Select(itemId)',
    'item.ID',
    'item.iconSprite',
    'GameMain.mainPlayer',
    'player.package',
    'package.GetItemCount(itemId)'
)) {
    if ($runtimeText -notmatch [Regex]::Escape($requiredText)) {
        throw "S3-02 runtime adapter is missing required read-only access: $requiredText"
    }
}

foreach ($prohibitedText in @(
    'System.Reflection',
    'Harmony',
    'ItemProto.maincraft',
    'recipe.Results',
    'recipe.ResultCounts',
    'MoveItem',
    'TakeTailItems',
    'AddItem',
    'SetItem',
    '.Craft(',
    '.forge.',
    'GameSave.Save',
    'BinaryWriter',
    'BepInEx.Configuration'
)) {
    if (($runtimeText + $sourceText) -match [Regex]::Escape($prohibitedText)) {
        throw "S3-02 source contains prohibited mutation, output, persistence, reflection, or navigation text: $prohibitedText"
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
    if ($sourceText -match [Regex]::Escape($prohibitedTerm)) {
        throw "RecipeDataSource contains prohibited runtime dependency term $prohibitedTerm."
    }
}

$testProjectText = [IO.File]::ReadAllText(
    (Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'RecipeDataSource\.cs') {
    throw 'The deterministic tests do not link RecipeDataSource.cs directly.'
}
if ($testProjectText -match 'DspRecipeDataAdapters\.cs') {
    throw 'The deterministic test process must not link the runtime DSP adapters.'
}

Write-Output 'S3-02 acceptance validation passed for the exact consumed read-only recipe, item-icon, machine-category, and Icarus-inventory surface; invalid-pin removal; temporary suppression and recovery; inert release; bounded diagnostics; and exhaustive shim coverage.'
