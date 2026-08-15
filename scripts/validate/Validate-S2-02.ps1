[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
    [string]$BepInExReferencePath
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path

& (Join-Path $PSScriptRoot 'Validate-S2-01.ps1')
if ($LASTEXITCODE -ne 0) {
    throw "S2-01 dependency validation failed with exit code $LASTEXITCODE."
}

$managedRoot = Join-Path $GameRoot 'DSPGAME_Data\Managed'
$assemblyCSharpPath = Join-Path $managedRoot 'Assembly-CSharp.dll'
if (-not (Test-Path -LiteralPath $assemblyCSharpPath)) {
    throw "Required inspection input is missing: $assemblyCSharpPath"
}

$cecilPath = Join-Path (Split-Path -Parent $BepInExReferencePath) 'Mono.Cecil.dll'
if (-not (Test-Path -LiteralPath $cecilPath)) {
    throw "Mono.Cecil authority reader is missing beside the BepInEx compile reference: $cecilPath"
}

Add-Type -Path $cecilPath
$assembly = [Mono.Cecil.AssemblyDefinition]::ReadAssembly($assemblyCSharpPath)
try {
    $window = $assembly.MainModule.Types | Where-Object { $_.FullName -eq 'UIReplicatorWindow' }
    if ($null -eq $window) {
        throw 'UIReplicatorWindow is absent from the authority assembly.'
    }

    $expectedFields = @{
        evtRecipe = 'UnityEngine.EventSystems.EventTrigger'
        recipeProtoArray = 'RecipeProto[]'
        mouseRecipeIndex = 'System.Int32'
    }
    foreach ($fieldEntry in $expectedFields.GetEnumerator()) {
        $field = $window.Fields | Where-Object { $_.Name -eq $fieldEntry.Key }
        if ($null -eq $field -or -not $field.IsPrivate -or $field.FieldType.FullName -ne $fieldEntry.Value) {
            throw "Authority field mismatch: UIReplicatorWindow.$($fieldEntry.Key)"
        }
    }

    $nativeHandler = $window.Methods | Where-Object {
        $_.Name -eq 'OnRecipeMouseDown' -and
        $_.ReturnType.FullName -eq 'System.Void' -and
        $_.Parameters.Count -eq 1 -and
        $_.Parameters[0].ParameterType.FullName -eq 'UnityEngine.EventSystems.BaseEventData'
    }
    if ($null -eq $nativeHandler) {
        throw 'Native OnRecipeMouseDown(BaseEventData) authority signature is missing.'
    }

    $handlerOperands = @($nativeHandler.Body.Instructions | ForEach-Object { if ($_.Operand) { $_.Operand.ToString() } }) -join "`n"
    foreach ($requiredOperand in @('mouseRecipeIndex', 'recipeProtoArray', 'SetSelectedRecipeIndex')) {
        if ($handlerOperands -notmatch [Regex]::Escape($requiredOperand)) {
            throw "Native recipe handler no longer consumes $requiredOperand."
        }
    }

    $createMethod = $window.Methods | Where-Object { $_.Name -eq '_OnCreate' }
    $createOperands = @($createMethod.Body.Instructions | ForEach-Object { if ($_.Operand) { $_.Operand.ToString() } }) -join "`n"
    foreach ($requiredOperand in @('OnRecipeMouseDown', 'UnityEvent`1<UnityEngine.EventSystems.BaseEventData>::AddListener', 'evtRecipe', 'EventTrigger::get_triggers')) {
        if ($createOperands -notmatch [Regex]::Escape($requiredOperand)) {
            throw "Native recipe PointerDown construction no longer contains $requiredOperand."
        }
    }
}
finally {
    $assembly.Dispose()
}

$inventoryPath = Join-Path $repoRoot 'ci\compile-references\surface-inventory.json'
$inventory = Get-Content -Raw -LiteralPath $inventoryPath | ConvertFrom-Json
$bindingNames = @($inventory.runtimeBindings | ForEach-Object { $_.name })
$adapterText = [IO.File]::ReadAllText((Join-Path $repoRoot 'src\DSPRecipeTracker\UnityReplicatorPinInputAdapter.cs'))
foreach ($bindingName in @('evtRecipe', 'recipeProtoArray', 'mouseRecipeIndex')) {
    if ($bindingNames -notcontains $bindingName -or $adapterText -notmatch ('"' + [Regex]::Escape($bindingName) + '"')) {
        throw "Reflection binding is not aligned across source and inventory: $bindingName"
    }
}

$boundaryText = [IO.File]::ReadAllText((Join-Path $repoRoot 'src\DSPRecipeTracker\ReplicatorPinInput.cs'))
foreach ($prohibitedTerm in @('UnityEngine', 'UIReplicatorWindow', 'RecipeProto', 'Harmony', 'Craft', 'Inventory')) {
    if ($boundaryText -match [Regex]::Escape($prohibitedTerm)) {
        throw "ReplicatorPinInput contains prohibited runtime or out-of-scope term $prohibitedTerm."
    }
}

$testProjectText = [IO.File]::ReadAllText((Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'))
if ($testProjectText -notmatch 'ReplicatorPinInput\.cs') {
    throw 'The deterministic tests do not link the S2-02 input boundary directly.'
}

Write-Output 'S2-02 acceptance validation passed for native-listener preservation, exact authority bindings, right-click filtering, deterministic state handoff, fail-soft behavior, one-time cleanup, and bounded Debug diagnostics.'
