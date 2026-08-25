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

    Two modes, matching run-e2e-test.ps1:
      - No arguments: test against the native pinned in OpenZiti.NET.csproj, restored from nuget.org. This is
        the PR check.
      - -PackageDir/-PackageVersion: test against a freshly packed nupkg. This is the native publish gate, and
        it is what catches a ziti-sdk-c struct change BEFORE the native reaches nuget.org. A csdk change that
        still compiles can move a field, which the smoke and traffic gates would not notice.

    Runnable locally to reproduce CI: ./scripts/run-managed-tests.ps1

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER PackageDir
    Folder holding a freshly packed OpenZiti.NET.native nupkg, added as a nuget source.

.PARAMETER PackageVersion
    Version of that packed package. Pins OpenZiti.NET.Tests to it, overriding the transitive pinned native.

.EXAMPLE
    ./run-managed-tests.ps1

.EXAMPLE
    ./run-managed-tests.ps1 -PackageDir ./artifacts -PackageVersion 1.18.7.289-preview
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [string] $PackageDir = '',
    [string] $PackageVersion = ''
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

if ([string]::IsNullOrWhiteSpace($PackageDir) -ne [string]::IsNullOrWhiteSpace($PackageVersion)) {
    throw "-PackageDir and -PackageVersion go together: pass both or neither."
}

# Sort key for an OpenZiti.NET.native version: the 4-part numeric prefix, then unlabelled above -preview.
function Get-NativeVersionKey([string] $Raw) {
    $m = [regex]::Match($Raw, '^(?<num>\d+(\.\d+){0,3})(-(?<label>.+))?$')
    if (-not $m.Success) { throw "cannot parse native version '$Raw'." }
    return [pscustomobject]@{
        Number     = [version] $m.Groups['num'].Value
        Prerelease = $m.Groups['label'].Success
    }
}

if ($PackageVersion) {
    # OpenZiti.NET pins the native the SDK ships against; the tests reference the packed one directly to
    # override it. NuGet treats a lower direct version as a downgrade (NU1605) and fails the restore, so
    # catch it here with an explanation rather than letting it surface as a wall of restore errors.
    $sdkProj = Join-Path $repoRoot 'OpenZiti.NET/OpenZiti.NET.csproj'
    $pinMatch = [regex]::Match((Get-Content $sdkProj -Raw),
        '<PackageReference\s+Include="OpenZiti\.NET\.native"\s+Version="(?<v>[^"]+)"')
    if ($pinMatch.Success) {
        $pinned = $pinMatch.Groups['v'].Value
        $a = Get-NativeVersionKey $PackageVersion
        $b = Get-NativeVersionKey $pinned
        $older = ($a.Number -lt $b.Number) -or
                 ($a.Number -eq $b.Number -and $a.Prerelease -and -not $b.Prerelease)
        if ($older) {
            throw ("cannot gate native ${PackageVersion}: OpenZiti.NET pins ${pinned}, and nuget rejects the " +
                   "downgrade (NU1605). Gate a version at or above the pin, or move the pin first.")
        }
    }
}

# With a PackageDir, add the local source and pin the packed version; the test project's conditional
# PackageReference then takes precedence over the native it would otherwise inherit from OpenZiti.NET.
$nativeProps = @()
if ($PackageVersion) { $nativeProps += "-p:ZitiNativeVersion=$PackageVersion" }
if ($PackageDir) {
    $source = (Resolve-Path $PackageDir).Path
    $nativeProps += "-p:RestoreAdditionalProjectSources=$source"
    Write-Host "Testing freshly packed native $PackageVersion from $source"
} else {
    Write-Host "Testing the published native pinned in the csprojs (restored from nuget.org)"
}

Write-Host "Building Ziti.NuGet.sln ($Configuration) ..."
dotnet build (Join-Path $repoRoot 'Ziti.NuGet.sln') -c $Configuration --nologo @nativeProps
if ($LASTEXITCODE -ne 0) { throw "build failed ($LASTEXITCODE)" }

Write-Host "Running managed ABI tests ..."
dotnet test (Join-Path $repoRoot 'OpenZiti.NET.Tests/OpenZiti.NET.Tests.csproj') `
    -c $Configuration --no-build --nologo @nativeProps `
    --filter "FullyQualifiedName~NativeCodeValueChecker|FullyQualifiedName~NativeLayoutChecker"
if ($LASTEXITCODE -ne 0) { throw "managed tests failed ($LASTEXITCODE)" }

Write-Host "Managed tests passed."
