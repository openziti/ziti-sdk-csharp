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

# Running an x86 test makes dotnet test spawn an x86 testhost, which needs the x86 .NET runtime. Hosted
# runners (and most dev boxes) only have x64, so the x86 testhost fails to load hostfxr (HRESULT 0x800700C1).
# Install the x86 runtime and point DOTNET_ROOT(x86) at it so the testhost resolves it. No-op if already there.
if ($Rid -eq 'win-x86') {
    $x86Root = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet-x86'
    if (-not (Test-Path (Join-Path $x86Root 'host\fxr'))) {
        Write-Host "Installing the x86 .NET runtime for the win-x86 testhost ..."
        $installer = Join-Path ([System.IO.Path]::GetTempPath()) 'dotnet-install.ps1'
        Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
        & $installer -Architecture x86 -Runtime dotnet -Channel 8.0 -InstallDir $x86Root
    }
    [Environment]::SetEnvironmentVariable('DOTNET_ROOT(x86)', $x86Root)
    Write-Host "DOTNET_ROOT(x86)=$x86Root"
}

Write-Host "Smoke test: RID=$Rid version=$PackageVersion source=$source"

dotnet test $project `
    -p:RuntimeIdentifier=$Rid `
    -p:ZitiNativeVersion=$PackageVersion `
    -p:RestoreAdditionalProjectSources=$source

exit $LASTEXITCODE
