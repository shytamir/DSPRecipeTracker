param(
    [Parameter(Mandatory = $true)]
    [string]$GameRoot
)

$script:FailureCount = 0
$repoRoot = (Resolve-Path (Join-Path $PSScriptRoot '..\..')).Path
$repoSafePath = $repoRoot.Replace('\', '/')
$fixturePath = Join-Path $PSScriptRoot 'S1-01.BuildContract.proj'
$inventoryPath = Join-Path $repoRoot 'ci\compile-references\surface-inventory.json'

function Write-Pass([string]$Message) {
    Write-Output "PASS: $Message"
}

function Write-Failure([string]$Message) {
    $script:FailureCount += 1
    Write-Output "FAIL: $Message"
}

function Invoke-MsBuildCase {
    param(
        [string]$Name,
        [string[]]$Properties,
        [bool]$ExpectSuccess,
        [string]$ExpectedCode
    )

    $arguments = @(
        'msbuild',
        $fixturePath,
        '-nologo',
        '-verbosity:minimal',
        '-target:ValidateS101Contract'
    ) + $Properties

    $output = & dotnet @arguments 2>&1 | Out-String
    $exitCode = $LASTEXITCODE

    if ($ExpectSuccess) {
        if ($exitCode -eq 0) {
            Write-Pass $Name
        }
        else {
            Write-Failure "$Name returned exit code $exitCode. $output"
        }
        return
    }

    if ($exitCode -ne 0 -and $output -match [regex]::Escape($ExpectedCode)) {
        Write-Pass "$Name failed with $ExpectedCode"
    }
    else {
        Write-Failure "$Name did not fail with $ExpectedCode. Exit code: $exitCode. $output"
    }
}

$trackedFiles = & git -c "safe.directory=$repoSafePath" -C $repoRoot ls-files --cached --others --exclude-standard
if ($LASTEXITCODE -ne 0) {
    Write-Failure 'git ls-files failed.'
}
else {
    $prohibitedPattern = '(?i)(^|/)(bin|obj|artifacts|dist|packages|TestResults|coverage|screenshots|captures|diagnostics|saves)(/|$)|\.(dll|pdb|mdb|exe|zip|nupkg|snupkg|trx|coverage|log|tmp|temp|bak|dsv|sav)$'
    $prohibitedTrackedFiles = @($trackedFiles | Where-Object { $_ -match $prohibitedPattern })
    if ($prohibitedTrackedFiles.Count -eq 0) {
        Write-Pass 'No prohibited generated, binary, package, save, diagnostic, or runtime-evidence file is tracked.'
    }
    else {
        Write-Failure "Prohibited tracked files: $($prohibitedTrackedFiles -join ', ')"
    }
}

$ignoredCandidates = @(
    'src/example/bin/Release/example.dll',
    'src/example/obj/project.assets.json',
    'artifacts/runtime/Assembly-CSharp.dll',
    'dist/DSPRecipeTracker.zip',
    'packages/DSPRecipeTracker.zip',
    'TestResults/results.trx',
    'coverage/report.xml',
    'screenshots/panel.png',
    'captures/input.gif',
    'diagnostics/player.json',
    'saves/test.dsv',
    'logs/plugin.log'
)

foreach ($candidate in $ignoredCandidates) {
    & git -c "safe.directory=$repoSafePath" -C $repoRoot check-ignore --no-index --quiet -- $candidate
    if ($LASTEXITCODE -eq 0) {
        Write-Pass "Ignore rule covers $candidate"
    }
    else {
        Write-Failure "Ignore rule does not cover $candidate"
    }
}

try {
    $inventory = Get-Content -LiteralPath $inventoryPath -Raw | ConvertFrom-Json
    if ($inventory.schemaVersion -eq 1 -and
        @($inventory.assemblies).Count -ge 1 -and
        @($inventory.assemblies | Where-Object { [string]::IsNullOrWhiteSpace($_.name) }).Count -eq 0) {
        Write-Pass 'Hosted compile-reference inventory has the required versioned assembly shape.'
    }
    else {
        Write-Failure 'Hosted compile-reference inventory has an unexpected initial shape or state.'
    }
}
catch {
    Write-Failure "Hosted compile-reference inventory is not valid JSON. $($_.Exception.Message)"
}

$buildContractPaths = @(
    (Join-Path $repoRoot 'Directory.Build.props'),
    (Join-Path $repoRoot 'Directory.Build.targets')
)
$buildContractText = ($buildContractPaths | ForEach-Object {
    Get-Content -LiteralPath $_ -Raw
}) -join "`n"
$forbiddenBuildBehavior = '(?i)https?://|Invoke-WebRequest|Start-BitsTransfer|Download(File|String)|Get-ChildItem|Get-ItemProperty|Registry::|Program Files|<\s*(Copy|Exec|Delete|Move|DownloadFile)\b'
if ($buildContractText -match $forbiddenBuildBehavior) {
    Write-Failure 'Build configuration contains discovery, network, execution, or mutation behavior.'
}
else {
    Write-Pass 'Build configuration contains no discovery, network, execution, or mutation behavior.'
}

Invoke-MsBuildCase -Name 'Explicit local reference configuration' `
    -Properties @('-property:DSPReferenceMode=Local', "-property:GameRoot=$GameRoot") `
    -ExpectSuccess $true -ExpectedCode ''

Invoke-MsBuildCase -Name 'Explicit hosted reference configuration' `
    -Properties @(
        '-property:DSPReferenceMode=Hosted',
        "-property:BepInExReferencePath=$(Join-Path $GameRoot 'BepInEx\core\BepInEx.dll')"
    ) `
    -ExpectSuccess $true -ExpectedCode ''

Invoke-MsBuildCase -Name 'Missing hosted BepInEx reference' `
    -Properties @('-property:DSPReferenceMode=Hosted') `
    -ExpectSuccess $false -ExpectedCode 'DRT1006'

$outsideHostedRoot = Join-Path $repoRoot 'artifacts\external-compile-references'
Invoke-MsBuildCase -Name 'Redirected hosted reference root' `
    -Properties @(
        '-property:DSPReferenceMode=Hosted',
        "-property:BepInExReferencePath=$(Join-Path $GameRoot 'BepInEx\core\BepInEx.dll')",
        "-property:DSPRecipeTrackerCompileReferenceRoot=$outsideHostedRoot"
    ) `
    -ExpectSuccess $false -ExpectedCode 'DRT1005'

Invoke-MsBuildCase -Name 'Missing reference mode' `
    -Properties @() -ExpectSuccess $false -ExpectedCode 'DRT1001'

Invoke-MsBuildCase -Name 'Missing local GameRoot' `
    -Properties @('-property:DSPReferenceMode=Local') `
    -ExpectSuccess $false -ExpectedCode 'DRT1002'

$missingGameRoot = Join-Path $repoRoot 'artifacts\missing-game-root'
Invoke-MsBuildCase -Name 'Nonexistent local GameRoot' `
    -Properties @('-property:DSPReferenceMode=Local', "-property:GameRoot=$missingGameRoot") `
    -ExpectSuccess $false -ExpectedCode 'DRT1003'

$missingAssembly = Join-Path $repoRoot 'artifacts\missing-Assembly-CSharp.dll'
Invoke-MsBuildCase -Name 'Missing required local assembly' `
    -Properties @(
        '-property:DSPReferenceMode=Local',
        "-property:GameRoot=$GameRoot",
        "-property:DSPAssemblyCSharpReferencePath=$missingAssembly"
    ) `
    -ExpectSuccess $false -ExpectedCode 'DRT1004'

if ($script:FailureCount -gt 0) {
    Write-Output "S1-01 validation failed with $script:FailureCount failure(s)."
    exit 1
}

Write-Output 'S1-01 validation passed.'
$global:LASTEXITCODE = 0
