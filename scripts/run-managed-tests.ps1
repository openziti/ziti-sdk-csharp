<#
.SYNOPSIS
    Build the managed solution and run the fast managed tests (no overlay needed).

.DESCRIPTION
    The CI-on-PR check. Builds Ziti.NuGet.sln (compile gate) and runs the managed tests that need only the
    OpenZiti.NET.native nuget package and no ziti overlay:
      - NativeCodeValueChecker.TestCSDKStructValues       (struct marshalling / accessor faithfulness)
      - NativeLayoutChecker.TestStructAlignmentsAgainstNativeLayout (live per-field layout vs z4d_layout_report)

    Deliberately EXCLUDED here (they need a live quickstart overlay, so they run in the publish e2e gate, not
    on every PR): DataTests.TestWeatherAsync and everything in the native/e2e project (CallbackTrafficTest,
    ProxyBridgeTest, IdiomaticTrafficTest).

    Runnable locally to reproduce CI: ./scripts/run-managed-tests.ps1

.PARAMETER Configuration
    Build configuration. Defaults to Release.
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release'
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

Write-Host "Building Ziti.NuGet.sln ($Configuration) ..."
dotnet build (Join-Path $repoRoot 'Ziti.NuGet.sln') -c $Configuration --nologo
if ($LASTEXITCODE -ne 0) { throw "build failed ($LASTEXITCODE)" }

Write-Host "Running managed ABI tests ..."
dotnet test (Join-Path $repoRoot 'OpenZiti.NET.Tests/OpenZiti.NET.Tests.csproj') `
    -c $Configuration --no-build --nologo `
    --filter "FullyQualifiedName~NativeCodeValueChecker|FullyQualifiedName~NativeLayoutChecker"
if ($LASTEXITCODE -ne 0) { throw "managed tests failed ($LASTEXITCODE)" }

Write-Host "Managed tests passed."
