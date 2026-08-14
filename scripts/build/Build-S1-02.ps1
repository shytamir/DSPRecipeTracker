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
$versionPath = Join-Path $repoRoot 'VERSION'
$versionValues = @{}

foreach ($line in [IO.File]::ReadAllLines($versionPath)) {
    if ($line -notmatch '^(MAJOR|MINOR)=([0-9]+)$') {
        throw "VERSION contains an invalid line: $line"
    }

    $versionValues[$Matches[1]] = [int]$Matches[2]
}

if ($versionValues.Count -ne 2 -or -not $versionValues.ContainsKey('MAJOR') -or -not $versionValues.ContainsKey('MINOR')) {
    throw 'VERSION must contain exactly one MAJOR and one MINOR value.'
}

$semanticVersion = "{0}.{1}.{2}" -f $versionValues.MAJOR, $versionValues.MINOR, $BuildNumber
$assemblyVersion = "$semanticVersion.0"
$shortRevision = $SourceRevision.Substring(0, [Math]::Min(12, $SourceRevision.Length)).ToLowerInvariant()
$diagnosticLabel = "$semanticVersion.$shortRevision"
$generatedDirectory = Join-Path $repoRoot 'artifacts\generated'
$generatedSource = Join-Path $generatedDirectory 'BuildIdentity.g.cs'
[IO.Directory]::CreateDirectory($generatedDirectory) | Out-Null

$source = @"
namespace DSPRecipeTracker
{
    internal static class BuildIdentity
    {
        public const int Major = $($versionValues.MAJOR);
        public const int Minor = $($versionValues.MINOR);
        public const int Build = $BuildNumber;
        public const string SemanticVersion = "$semanticVersion";
        public const string AssemblyVersion = "$assemblyVersion";
        public const string DiagnosticLabel = "$diagnosticLabel";
    }
}
"@
[IO.File]::WriteAllText($generatedSource, $source, [Text.UTF8Encoding]::new($false))

if ($ReferenceMode -eq 'Local' -and [string]::IsNullOrWhiteSpace($GameRoot)) {
    throw 'Local builds require -GameRoot.'
}

if ($ReferenceMode -eq 'Hosted' -and [string]::IsNullOrWhiteSpace($BepInExReferencePath)) {
    throw 'Hosted builds require -BepInExReferencePath.'
}

if ($ReferenceMode -eq 'Hosted') {
    & dotnet build (Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine\UnityEngine.Reference.csproj') --configuration Release --no-incremental
    if ($LASTEXITCODE -ne 0) {
        throw "Hosted compile-reference build failed with exit code $LASTEXITCODE."
    }
}

$buildArguments = @(
    'build', (Join-Path $repoRoot 'DSPRecipeTracker.sln'),
    '--configuration', 'Release',
    '--no-incremental',
    "-p:DSPReferenceMode=$ReferenceMode",
    "-p:GeneratedVersionSource=$generatedSource",
    "-p:Version=$semanticVersion",
    "-p:AssemblyVersion=$assemblyVersion",
    "-p:FileVersion=$assemblyVersion",
    "-p:InformationalVersion=$diagnosticLabel"
)

if ($ReferenceMode -eq 'Local') {
    $buildArguments += "-p:GameRoot=$GameRoot"
}
else {
    $buildArguments += "-p:BepInExReferencePath=$BepInExReferencePath"
}

& dotnet @buildArguments
if ($LASTEXITCODE -ne 0) {
    throw "Release build failed with exit code $LASTEXITCODE."
}

& dotnet run --project (Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj') --configuration Release --no-build --property:GeneratedVersionSource=$generatedSource
if ($LASTEXITCODE -ne 0) {
    throw "Deterministic tests failed with exit code $LASTEXITCODE."
}

$pluginPath = Join-Path $repoRoot 'src\DSPRecipeTracker\bin\Release\net472\DSPRecipeTracker.dll'
$assemblyName = [Reflection.AssemblyName]::GetAssemblyName($pluginPath)
$fileInfo = [Diagnostics.FileVersionInfo]::GetVersionInfo($pluginPath)
if ($assemblyName.Version.ToString() -ne $assemblyVersion) {
    throw "Assembly version is $($assemblyName.Version), expected $assemblyVersion."
}
if ($fileInfo.FileVersion -ne $assemblyVersion) {
    throw "File version is $($fileInfo.FileVersion), expected $assemblyVersion."
}
if ($fileInfo.ProductVersion -ne $diagnosticLabel) {
    throw "Diagnostic product version is $($fileInfo.ProductVersion), expected $diagnosticLabel."
}

if ($ReferenceMode -eq 'Hosted') {
    $shimPath = Join-Path $repoRoot 'ci\compile-references\Unity.Reference\UnityEngine\obj\Release\netstandard2.0\ref\UnityEngine.dll'
    $compileReferenceReport = Join-Path $repoRoot 'artifacts\build\compile-reference-validation.json'
    & dotnet run --project (Join-Path $repoRoot 'scripts\validate\CompileReferenceValidator\CompileReferenceValidator.csproj') --configuration Release -- $pluginPath $shimPath (Join-Path $repoRoot 'ci\compile-references\surface-inventory.json') $compileReferenceReport
    if ($LASTEXITCODE -ne 0) {
        throw "Compile-reference validation failed with exit code $LASTEXITCODE."
    }
}

$buildInfoDirectory = Join-Path $repoRoot 'artifacts\build'
$buildInfoPath = Join-Path $buildInfoDirectory 'build-info.json'
[IO.Directory]::CreateDirectory($buildInfoDirectory) | Out-Null
$buildInfo = [ordered]@{
    schemaVersion = 1
    referenceMode = $ReferenceMode
    semanticVersion = $semanticVersion
    assemblyVersion = $assemblyVersion
    diagnosticLabel = $diagnosticLabel
    buildNumber = $BuildNumber
    sourceRevision = $SourceRevision.ToLowerInvariant()
    pluginRelativePath = 'src/DSPRecipeTracker/bin/Release/net472/DSPRecipeTracker.dll'
}
[IO.File]::WriteAllText(
    $buildInfoPath,
    ($buildInfo | ConvertTo-Json -Depth 3),
    [Text.UTF8Encoding]::new($false))

Write-Output "S1-02 $ReferenceMode Release validation passed for $diagnosticLabel."
