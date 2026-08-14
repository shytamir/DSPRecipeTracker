[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
    [ValidateRange(0, 65535)]
    [int]$BuildNumber,

    [Parameter(Mandatory = $true)]
    [ValidatePattern('^[0-9a-fA-F]{7,40}$')]
    [string]$SourceRevision
)

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$buildScript = Join-Path $repoRoot 'scripts\build\Build-S1-02.ps1'
$bepInExReference = Join-Path $GameRoot 'BepInEx\core\BepInEx.dll'

& $buildScript -ReferenceMode Local -GameRoot $GameRoot -BuildNumber $BuildNumber -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "Local S1-02 validation failed with exit code $LASTEXITCODE."
}

& $buildScript -ReferenceMode Hosted -BepInExReferencePath $bepInExReference -BuildNumber $BuildNumber -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "Hosted S1-02 validation failed with exit code $LASTEXITCODE."
}

Write-Output 'S1-02 acceptance validation passed in Local and Hosted modes.'
