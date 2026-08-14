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
$buildScript = Join-Path $repoRoot 'scripts\build\Build-S1-03.ps1'
$bepInExReference = Join-Path $GameRoot 'BepInEx\core\BepInEx.dll'

& $buildScript -ReferenceMode Local -GameRoot $GameRoot -BuildNumber $BuildNumber -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "Local S1-03 package validation failed with exit code $LASTEXITCODE."
}

& $buildScript -ReferenceMode Hosted -BepInExReferencePath $bepInExReference -BuildNumber $BuildNumber -SourceRevision $SourceRevision
if ($LASTEXITCODE -ne 0) {
    throw "Hosted S1-03 package validation failed with exit code $LASTEXITCODE."
}

$buildInfo = [IO.File]::ReadAllText((Join-Path $repoRoot 'artifacts\build\build-info.json')) | ConvertFrom-Json
$packageRoot = Join-Path $repoRoot "artifacts\package\$($buildInfo.semanticVersion)"
$zipPath = Join-Path $packageRoot "DSPRecipeTracker-$($buildInfo.semanticVersion).zip"
$pluginPath = Join-Path $repoRoot $buildInfo.pluginRelativePath
$validatorProject = Join-Path $repoRoot 'scripts\validate\PackageValidator\PackageValidator.csproj'
$caseRoot = Join-Path $repoRoot 'artifacts\package\validator-cases'
[IO.Directory]::CreateDirectory($caseRoot) | Out-Null

function Confirm-Rejected {
    param(
        [string]$Name,
        [string]$PackagePath,
        [string]$ExpectedVersion,
        [string]$ExpectedAssemblyVersion,
        [string]$ExpectedDiagnostic,
        [string]$ExpectedGuid,
        [string]$ExpectedDisplayName
    )
    $report = Join-Path $caseRoot ($Name + '.json')
    $previousErrorPreference = $ErrorActionPreference
    $ErrorActionPreference = 'Continue'
    $output = & dotnet run --project $validatorProject --configuration Release -- `
        $PackagePath $pluginPath $ExpectedVersion $ExpectedAssemblyVersion $ExpectedDiagnostic `
        $ExpectedGuid $ExpectedDisplayName $report 2>&1 | Out-String
    $validatorExitCode = $LASTEXITCODE
    $ErrorActionPreference = $previousErrorPreference
    if ($validatorExitCode -eq 0) {
        throw "Negative package case unexpectedly passed: $Name"
    }
    Write-Output "PASS: Package validator rejected $Name."
}

function Confirm-Accepted {
    param(
        [string]$Name,
        [string]$PackagePath
    )
    $report = Join-Path $caseRoot ($Name + '.json')
    & dotnet run --project $validatorProject --configuration Release -- `
        $PackagePath $pluginPath $buildInfo.semanticVersion $buildInfo.assemblyVersion $buildInfo.diagnosticLabel `
        'dsprecipetracker' 'DSP-Recipe-Tracker' $report
    if ($LASTEXITCODE -ne 0) {
        throw "Valid package variation was rejected: $Name"
    }
    Write-Output "PASS: Package validator accepted $Name."
}

function New-MutatedPackage {
    param(
        [string]$Name,
        [Alias('TargetEntry')]
        [string]$EntryName,
        [byte[]]$ReplacementBytes,
        [switch]$AddEntry
    )
    Add-Type -AssemblyName System.IO.Compression
    Add-Type -AssemblyName System.IO.Compression.FileSystem
    $destination = Join-Path $caseRoot ($Name + '.zip')
    if (Test-Path -LiteralPath $destination) {
        Remove-Item -LiteralPath $destination -Force
    }
    $sourceArchive = [System.IO.Compression.ZipFile]::OpenRead($zipPath)
    $targetArchive = [System.IO.Compression.ZipFile]::Open($destination, [System.IO.Compression.ZipArchiveMode]::Create)
    try {
        foreach ($sourceEntry in $sourceArchive.Entries) {
            $createdEntry = $targetArchive.CreateEntry($sourceEntry.FullName)
            $targetStream = $createdEntry.Open()
            try {
                if ($sourceEntry.FullName -eq $EntryName -and -not $AddEntry) {
                    $targetStream.Write($ReplacementBytes, 0, $ReplacementBytes.Length)
                }
                else {
                    $sourceStream = $sourceEntry.Open()
                    try { $sourceStream.CopyTo($targetStream) } finally { $sourceStream.Dispose() }
                }
            }
            finally { $targetStream.Dispose() }
        }
        if ($AddEntry) {
            $extra = $targetArchive.CreateEntry($EntryName)
            $extraStream = $extra.Open()
            try { $extraStream.Write($ReplacementBytes, 0, $ReplacementBytes.Length) } finally { $extraStream.Dispose() }
        }
    }
    finally {
        $targetArchive.Dispose()
        $sourceArchive.Dispose()
    }
    return $destination
}

$alternateReadmePackage = New-MutatedPackage -Name 'alternate-readme' -TargetEntry 'README.md' -ReplacementBytes ([Text.Encoding]::UTF8.GetBytes("# Alternate package copy`n"))
Confirm-Accepted -Name 'alternate README copy within package limits' -PackagePath $alternateReadmePackage

$alternateManifest = Get-Content -LiteralPath (Join-Path $repoRoot 'packaging\manifest.json') -Raw | ConvertFrom-Json
$alternateManifest.version_number = $buildInfo.semanticVersion
$alternateManifest.description = 'Alternate package description within the documented limit.'
$alternateManifest.website_url = ''
$alternateManifestBytes = [Text.Encoding]::UTF8.GetBytes(($alternateManifest | ConvertTo-Json -Depth 5))
$alternateManifestPackage = New-MutatedPackage -Name 'alternate-manifest-copy' -TargetEntry 'manifest.json' -ReplacementBytes $alternateManifestBytes
Confirm-Accepted -Name 'alternate manifest copy within package limits' -PackagePath $alternateManifestPackage

$overlongManifest = Get-Content -LiteralPath (Join-Path $repoRoot 'packaging\manifest.json') -Raw | ConvertFrom-Json
$overlongManifest.version_number = $buildInfo.semanticVersion
$overlongManifest.description = (('x' * 251) -join '')
$overlongManifestBytes = [Text.Encoding]::UTF8.GetBytes(($overlongManifest | ConvertTo-Json -Depth 5))
$overlongManifestPackage = New-MutatedPackage -Name 'overlong-description' -TargetEntry 'manifest.json' -ReplacementBytes $overlongManifestBytes
Confirm-Rejected -Name 'description over 250 characters' -PackagePath $overlongManifestPackage -ExpectedVersion $buildInfo.semanticVersion -ExpectedAssemblyVersion $buildInfo.assemblyVersion -ExpectedDiagnostic $buildInfo.diagnosticLabel -ExpectedGuid 'dsprecipetracker' -ExpectedDisplayName 'DSP-Recipe-Tracker'

$zeroDllPackage = New-MutatedPackage -Name 'zero-byte-dll' -TargetEntry 'BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll' -ReplacementBytes ([byte[]]@())
Confirm-Rejected -Name 'zero-byte DLL' -PackagePath $zeroDllPackage -ExpectedVersion $buildInfo.semanticVersion -ExpectedAssemblyVersion $buildInfo.assemblyVersion -ExpectedDiagnostic $buildInfo.diagnosticLabel -ExpectedGuid 'dsprecipetracker' -ExpectedDisplayName 'DSP-Recipe-Tracker'

$nativeDllPackage = New-MutatedPackage -Name 'non-managed-dll' -TargetEntry 'BepInEx/plugins/DSPRecipeTracker/DSPRecipeTracker.dll' -ReplacementBytes ([Text.Encoding]::UTF8.GetBytes('not a managed assembly'))
Confirm-Rejected -Name 'non-managed DLL' -PackagePath $nativeDllPackage -ExpectedVersion $buildInfo.semanticVersion -ExpectedAssemblyVersion $buildInfo.assemblyVersion -ExpectedDiagnostic $buildInfo.diagnosticLabel -ExpectedGuid 'dsprecipetracker' -ExpectedDisplayName 'DSP-Recipe-Tracker'

Confirm-Rejected -Name 'version mismatch' -PackagePath $zipPath -ExpectedVersion '9.9.9' -ExpectedAssemblyVersion '9.9.9.0' -ExpectedDiagnostic '9.9.9.invalid' -ExpectedGuid 'dsprecipetracker' -ExpectedDisplayName 'DSP-Recipe-Tracker'
Confirm-Rejected -Name 'plugin metadata mismatch' -PackagePath $zipPath -ExpectedVersion $buildInfo.semanticVersion -ExpectedAssemblyVersion $buildInfo.assemblyVersion -ExpectedDiagnostic $buildInfo.diagnosticLabel -ExpectedGuid 'invalid.guid' -ExpectedDisplayName 'DSP-Recipe-Tracker'

$shimPackage = New-MutatedPackage -Name 'shim-in-package' -TargetEntry 'BepInEx/plugins/DSPRecipeTracker/UnityEngine.dll' -ReplacementBytes ([byte[]](1, 2, 3)) -AddEntry
Confirm-Rejected -Name 'compile-reference shim in package' -PackagePath $shimPackage -ExpectedVersion $buildInfo.semanticVersion -ExpectedAssemblyVersion $buildInfo.assemblyVersion -ExpectedDiagnostic $buildInfo.diagnosticLabel -ExpectedGuid 'dsprecipetracker' -ExpectedDisplayName 'DSP-Recipe-Tracker'

$dependencyPackage = New-MutatedPackage -Name 'dependency-in-package' -TargetEntry 'BepInEx/plugins/DSPRecipeTracker/BepInEx.dll' -ReplacementBytes ([byte[]](1, 2, 3)) -AddEntry
Confirm-Rejected -Name 'dependency binary in package' -PackagePath $dependencyPackage -ExpectedVersion $buildInfo.semanticVersion -ExpectedAssemblyVersion $buildInfo.assemblyVersion -ExpectedDiagnostic $buildInfo.diagnosticLabel -ExpectedGuid 'dsprecipetracker' -ExpectedDisplayName 'DSP-Recipe-Tracker'

Write-Output 'S1-03 acceptance validation passed in Local and Hosted modes, including copy-flexibility and required rejection cases.'
