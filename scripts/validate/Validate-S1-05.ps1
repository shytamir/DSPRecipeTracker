[CmdletBinding()]
param()

$ErrorActionPreference = 'Stop'
$geometryAndPolicyGate = Join-Path $PSScriptRoot 'Validate-S1-04.ps1'

& $geometryAndPolicyGate
if ($LASTEXITCODE -ne 0) {
    throw "S1-05 deterministic tests failed with exit code $LASTEXITCODE."
}

Write-Output 'S1-05 acceptance validation passed for the complete visibility truth table without runtime dependencies.'
