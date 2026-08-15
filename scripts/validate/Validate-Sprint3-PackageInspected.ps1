[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot,

    [Parameter(Mandatory = $true)]
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
    throw "Package-inspected must build the checked-out revision $SourceRevision; current HEAD is $headRevision."
}
$workingState = @(& git -c "safe.directory=$safeDirectory" -C $repoRoot status --short)
if ($LASTEXITCODE -ne 0 -or $workingState.Count -ne 0) {
    throw "Package-inspected requires a clean tracked source revision: $($workingState -join ', ')"
}

$buildScript = Join-Path $repoRoot 'scripts\build\Build-S1-03.ps1'
& $buildScript `
    -ReferenceMode Hosted `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "Hosted package inspection failed with exit code $LASTEXITCODE."
}

$semanticVersion = "0.1.$BuildNumber"
$packageRoot = Join-Path $repoRoot "artifacts\package\$semanticVersion"
Copy-Item -LiteralPath (Join-Path $packageRoot 'package-validation.json') `
    -Destination (Join-Path $packageRoot 'package-validation-hosted.json') -Force

& $buildScript `
    -ReferenceMode Local `
    -GameRoot $GameRoot `
    -BepInExReferencePath $BepInExReferencePath `
    -BuildNumber $BuildNumber `
    -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "Local package inspection failed with exit code $LASTEXITCODE."
}
Copy-Item -LiteralPath (Join-Path $packageRoot 'package-validation.json') `
    -Destination (Join-Path $packageRoot 'package-validation-local.json') -Force

$buildInfoPath = Join-Path $packageRoot 'build-info.json'
$dllPath = Join-Path $packageRoot 'DSPRecipeTracker.dll'
$zipPath = Join-Path $packageRoot "DSPRecipeTracker-$semanticVersion.zip"
foreach ($requiredArtifact in @($buildInfoPath, $dllPath, $zipPath)) {
    if (-not (Test-Path -LiteralPath $requiredArtifact)) {
        throw "Package-inspected output is missing: $requiredArtifact"
    }
}

$buildInfo = [IO.File]::ReadAllText($buildInfoPath) | ConvertFrom-Json
if ($buildInfo.semanticVersion -ne $semanticVersion -or
    $buildInfo.sourceRevision -ne $SourceRevision.ToLowerInvariant() -or
    $buildInfo.referenceMode -ne 'Local') {
    throw 'The retained owner-test artifact does not match the expected version, source revision, and Local reference mode.'
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
$archive = [IO.Compression.ZipFile]::OpenRead($zipPath)
try {
    $actualEntries = @($archive.Entries | ForEach-Object { $_.FullName })
    $expectedEntries = @(
        'manifest.json',
        'README.md',
        'icon.png',
        'BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll'
    )
    $difference = @(Compare-Object -ReferenceObject $expectedEntries -DifferenceObject $actualEntries)
    if ($difference.Count -ne 0) {
        throw "Package-inspected archive allowlist mismatch: $($difference | Out-String)"
    }
}
finally {
    $archive.Dispose()
}

foreach ($reportName in @('package-validation-hosted.json', 'package-validation-local.json')) {
    $report = [IO.File]::ReadAllText((Join-Path $packageRoot $reportName)) | ConvertFrom-Json
    if (-not $report.passed) {
        throw "Package inspection report did not pass: $reportName"
    }
}

Write-Output "Sprint 3 Package-inspected validation passed for source revision $SourceRevision."
Write-Output "Owner-test DLL: $dllPath"
Write-Output "Owner-test package: $zipPath"
