<#
.SYNOPSIS
    Run the native load-probe smoke test against a freshly packed OpenZiti.NET.native package.

.DESCRIPTION
    Builds and runs native/smoke-test/SmokeTest.csproj with the package version and a local nuget source
    pointing at the folder that holds the freshly packed .nupkg. The test P/Invokes z4d_all_config_types()
    to prove the native lib for the target RID loads and is callable. No overlay or network needed.

    Locally runnable: a maintainer can point this at any packed nupkg dir and version to reproduce CI.

.PARAMETER PackageDir
    Folder containing the freshly packed OpenZiti.NET.native.<version>.nupkg (used as a local nuget source).

.PARAMETER PackageVersion
    The nuget version of the freshly packed package (e.g. 1.16.0.213).

.PARAMETER Rid
    The runtime identifier to test (e.g. win-x64, linux-x64, osx-arm64). Defaults to win-x64.

.EXAMPLE
    ./run-smoke-test.ps1 -PackageDir ./artifacts -PackageVersion 1.16.0.213 -Rid win-x64
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $PackageDir,
    [Parameter(Mandatory = $true)] [string] $PackageVersion,
    [string] $Rid = 'win-x64'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repoRoot 'native/smoke-test/SmokeTest.csproj'
$source = (Resolve-Path $PackageDir).Path

Write-Host "Smoke test: RID=$Rid version=$PackageVersion source=$source"

dotnet test $project `
    -p:RuntimeIdentifier=$Rid `
    -p:ZitiNativeVersion=$PackageVersion `
    -p:RestoreAdditionalProjectSources=$source

exit $LASTEXITCODE
