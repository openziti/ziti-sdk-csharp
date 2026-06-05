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

# dotnet test runs the test process in the SDK's own architecture. To probe the x86 native lib the test must
# itself be x86, but hosted runners ship only the x64 SDK. So for win-x86 install an x86 .NET SDK and use its
# dotnet.exe for the whole build+test. No-op if already present. Other RIDs use the runner's dotnet.
$dotnet = 'dotnet'
if ($Rid -eq 'win-x86') {
    $x86Root = Join-Path $env:LOCALAPPDATA 'Microsoft\dotnet-x86'
    $dotnet = Join-Path $x86Root 'dotnet.exe'
    if (-not (Test-Path $dotnet)) {
        Write-Host "Installing an x86 .NET SDK so the win-x86 test runs as x86 ..."
        $installer = Join-Path ([System.IO.Path]::GetTempPath()) 'dotnet-install.ps1'
        Invoke-WebRequest -Uri 'https://dot.net/v1/dotnet-install.ps1' -OutFile $installer
        & $installer -Architecture x86 -Channel 8.0 -InstallDir $x86Root
    }
}

Write-Host "Smoke test: RID=$Rid version=$PackageVersion source=$source (dotnet: $dotnet)"

& $dotnet test $project `
    -p:RuntimeIdentifier=$Rid `
    -p:ZitiNativeVersion=$PackageVersion `
    -p:RestoreAdditionalProjectSources=$source

exit $LASTEXITCODE
