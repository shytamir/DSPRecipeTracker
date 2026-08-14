[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$generatedDirectory = Join-Path $repoRoot 'artifacts\generated\s1-04'
$generatedSource = Join-Path $generatedDirectory 'BuildIdentity.g.cs'
[IO.Directory]::CreateDirectory($generatedDirectory) | Out-Null

$source = @"
namespace DSPRecipeTracker
{
    internal static class BuildIdentity
    {
        public const int Major = 0;
        public const int Minor = 1;
        public const int Build = 0;
        public const string SemanticVersion = "0.1.0";
        public const string AssemblyVersion = "0.1.0.0";
        public const string DiagnosticLabel = "0.1.0.s104";
    }
}
"@
[IO.File]::WriteAllText($generatedSource, $source, [Text.UTF8Encoding]::new($false))

$testProject = Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\DSPRecipeTracker.Tests.csproj'
& dotnet run --project $testProject --configuration Release `
    --property:GeneratedVersionSource=$generatedSource `
    --property:NuGetAudit=false
if ($LASTEXITCODE -ne 0) {
    throw "S1-04 deterministic tests failed with exit code $LASTEXITCODE."
}

$testProjectText = [IO.File]::ReadAllText($testProject)
if ($testProjectText -match '<ProjectReference|<Reference\s|<PackageReference') {
    throw 'The deterministic test project must not reference product, game, Unity, BepInEx, or package assemblies.'
}

$dependencyFile = Join-Path $repoRoot 'tests\DSPRecipeTracker.Tests\bin\Release\net8.0\DSPRecipeTracker.Tests.deps.json'
$dependencyText = [IO.File]::ReadAllText($dependencyFile)
foreach ($prohibitedDependency in @('BepInEx', 'UnityEngine', 'Assembly-CSharp')) {
    if ($dependencyText -match [Regex]::Escape($prohibitedDependency)) {
        throw "The deterministic test process includes prohibited dependency $prohibitedDependency."
    }
}

Write-Output 'S1-04 acceptance validation passed without loading Unity, DSP, BepInEx, or installed assemblies.'
