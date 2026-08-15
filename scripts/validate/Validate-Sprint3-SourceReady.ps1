[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [string]$BepInExReferencePath,

    [Parameter(Mandatory = $true)]
    [ValidateRange(0, 65535)]
    [int]$BuildNumber,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F]{40}$')]
    [string]$SourceRevision
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$safeDirectory = $repoRoot.Replace('\', '/')
$headRevision = (& git -c "safe.directory=$safeDirectory" -C $repoRoot rev-parse HEAD).Trim()
if ($LASTEXITCODE -ne 0 -or $headRevision -ne $SourceRevision.ToLowerInvariant()) {
    throw "Source-ready must validate the checked-out revision $SourceRevision; current HEAD is $headRevision."
}
$workingState = @(& git -c "safe.directory=$safeDirectory" -C $repoRoot status --short)
if ($LASTEXITCODE -ne 0 -or $workingState.Count -ne 0) {
    throw "Source-ready requires a clean tracked source revision: $($workingState -join ', ')"
}

& (Join-Path $PSScriptRoot 'Validate-S3-06.ps1') `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "S3-06 validation failed with exit code $LASTEXITCODE."
}

$productionText = (Get-ChildItem -LiteralPath (Join-Path $repoRoot 'src\DSPRecipeTracker') -Filter '*.cs' |
    ForEach-Object { [IO.File]::ReadAllText($_.FullName) }) -join "`n"
foreach ($prohibitedMutation in @(
    '.Craft(',
    'MoveItem',
    'AddItem',
    'SetItem',
    'SaveData',
    'System.IO.File',
    'System.IO.Directory'
)) {
    if ($productionText -match [Regex]::Escape($prohibitedMutation)) {
        throw "Source-ready review found a prohibited game-state, persistence, or file mutation surface: $prohibitedMutation"
    }
}

$trackedOutputs = @(& git -c "safe.directory=$safeDirectory" -C $repoRoot ls-files -- `
    '*.dll' '*.pdb' 'artifacts/**' '**/bin/**' '**/obj/**')
if ($LASTEXITCODE -ne 0 -or $trackedOutputs.Count -ne 0) {
    throw "Source-ready found tracked binary or generated output: $($trackedOutputs -join ', ')"
}

$roadmapText = [IO.File]::ReadAllText((Join-Path $repoRoot 'docs\ROADMAP.md'))
if ($roadmapText -notmatch [Regex]::Escape('(OWNER-VALIDATION.md)')) {
    throw 'The active roadmap does not link the owner validation procedure.'
}

Write-Output "Sprint 3 Source-ready validation passed for source revision $SourceRevision."
