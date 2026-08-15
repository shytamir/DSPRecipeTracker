[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)]
    [ValidateSet('Local', 'Hosted')]
    [string]$ReferenceMode,

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
$sourceBuild = Join-Path $repoRoot 'scripts\build\Build-S1-02.ps1'
if ($ReferenceMode -eq 'Hosted') {
    if ([string]::IsNullOrWhiteSpace($BepInExReferencePath) -or -not (Test-Path -LiteralPath $BepInExReferencePath)) {
        throw 'Hosted package builds require an existing -BepInExReferencePath.'
    }
    $bepInExVersion = [Reflection.AssemblyName]::GetAssemblyName($BepInExReferencePath).Version.ToString()
    if ($bepInExVersion -ne '5.4.17.0') {
        throw "Hosted BepInEx reference version is $bepInExVersion, expected 5.4.17.0."
    }
}
$sourceArguments = @{
    ReferenceMode = $ReferenceMode
    BuildNumber = $BuildNumber
    SourceRevision = $SourceRevision
}
if ($ReferenceMode -eq 'Local') {
    $sourceArguments.GameRoot = $GameRoot
    if (-not [string]::IsNullOrWhiteSpace($BepInExReferencePath)) {
        $sourceArguments.BepInExReferencePath = $BepInExReferencePath
    }
}
else {
    $sourceArguments.BepInExReferencePath = $BepInExReferencePath
}

& $sourceBuild @sourceArguments
if ($LASTEXITCODE -ne 0) {
    throw "S1-02 source build failed with exit code $LASTEXITCODE."
}

$buildInfoPath = Join-Path $repoRoot 'artifacts\build\build-info.json'
$buildInfo = [IO.File]::ReadAllText($buildInfoPath) | ConvertFrom-Json
$semanticVersion = $buildInfo.semanticVersion
$packageRoot = Join-Path $repoRoot "artifacts\package\$semanticVersion"
$stagingRoot = Join-Path $repoRoot 'artifacts\package\work'
$pluginDirectory = Join-Path $stagingRoot 'BepInEx\plugins\DSPRecipeTracker'
$pluginSource = Join-Path $repoRoot $buildInfo.pluginRelativePath
$zipPath = Join-Path $packageRoot "DSPRecipeTracker-$semanticVersion.zip"
$retainedDll = Join-Path $packageRoot 'DSPRecipeTracker.dll'
$reportPath = Join-Path $packageRoot 'package-validation.json'

if (Test-Path -LiteralPath $stagingRoot) {
    $resolvedWorkRoot = [IO.Path]::GetFullPath($stagingRoot)
    $expectedWorkRoot = [IO.Path]::GetFullPath((Join-Path $repoRoot 'artifacts\package\work'))
    if ($resolvedWorkRoot -ne $expectedWorkRoot) {
        throw "Refusing to remove unexpected staging path: $resolvedWorkRoot"
    }
    Remove-Item -LiteralPath $resolvedWorkRoot -Recurse -Force
}
[IO.Directory]::CreateDirectory($pluginDirectory) | Out-Null
[IO.Directory]::CreateDirectory($packageRoot) | Out-Null

$manifestTemplate = [IO.File]::ReadAllText((Join-Path $repoRoot 'packaging\manifest.json'))
if ([regex]::Matches($manifestTemplate, '__VERSION__').Count -ne 1) {
    throw 'packaging/manifest.json must contain exactly one __VERSION__ token.'
}
[IO.File]::WriteAllText(
    (Join-Path $stagingRoot 'manifest.json'),
    $manifestTemplate.Replace('__VERSION__', $semanticVersion),
    [Text.UTF8Encoding]::new($false))
Copy-Item -LiteralPath (Join-Path $repoRoot 'packaging\README.md') -Destination (Join-Path $stagingRoot 'README.md')
Copy-Item -LiteralPath (Join-Path $repoRoot 'packaging\icon.png') -Destination (Join-Path $stagingRoot 'icon.png')
Copy-Item -LiteralPath $pluginSource -Destination (Join-Path $pluginDirectory 'DSPRecipeTracker.dll')
Copy-Item -LiteralPath $pluginSource -Destination $retainedDll -Force
Copy-Item -LiteralPath $buildInfoPath -Destination (Join-Path $packageRoot 'build-info.json') -Force
if ($ReferenceMode -eq 'Hosted') {
    Copy-Item -LiteralPath (Join-Path $repoRoot 'artifacts\build\compile-reference-validation.json') -Destination (Join-Path $packageRoot 'compile-reference-validation.json') -Force
}

Add-Type -AssemblyName System.IO.Compression
Add-Type -AssemblyName System.IO.Compression.FileSystem
if (Test-Path -LiteralPath $zipPath) {
    Remove-Item -LiteralPath $zipPath -Force
}
$archive = [System.IO.Compression.ZipFile]::Open($zipPath, [System.IO.Compression.ZipArchiveMode]::Create)
try {
    $entries = [ordered]@{
        'manifest.json' = (Join-Path $stagingRoot 'manifest.json')
        'README.md' = (Join-Path $stagingRoot 'README.md')
        'icon.png' = (Join-Path $stagingRoot 'icon.png')
        'BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll' = (Join-Path $pluginDirectory 'DSPRecipeTracker.dll')
    }
    foreach ($entryName in $entries.Keys) {
        $entry = $archive.CreateEntry($entryName, [IO.Compression.CompressionLevel]::Optimal)
        $entry.LastWriteTime = [DateTimeOffset]::new(1980, 1, 1, 0, 0, 0, [TimeSpan]::Zero)
        $input = [IO.File]::OpenRead($entries[$entryName])
        $output = $entry.Open()
        try {
            $input.CopyTo($output)
        }
        finally {
            $output.Dispose()
            $input.Dispose()
        }
    }
}
finally {
    $archive.Dispose()
}

& dotnet run --project (Join-Path $repoRoot 'scripts\validate\PackageValidator\PackageValidator.csproj') --configuration Release -- `
    $zipPath $pluginSource $buildInfo.semanticVersion $buildInfo.assemblyVersion $buildInfo.diagnosticLabel `
    'dsprecipetracker' 'DSP-Recipe-Tracker' $reportPath
if ($LASTEXITCODE -ne 0) {
    throw "Package validation failed with exit code $LASTEXITCODE."
}

Write-Output "S1-03 $ReferenceMode package inspection passed: $zipPath"
