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

    Modes:
      - No arguments: test against the native pinned in OpenZiti.NET.csproj, restored from nuget.org.
      - -PackageDir/-PackageVersion: test against a freshly packed nupkg. This is the native publish gate, and
        it is what catches a ziti-sdk-c struct change BEFORE the native reaches nuget.org. A csdk change that
        still compiles can move a field, which the smoke and traffic gates would not notice.
      - -NativeLib: build against the pinned native, then swap the given win-x64 ziti4dotnet.dll into the test
        output before testing. run-csdk-abi-check.ps1 uses this for a native built from source.
      - -BaseRef: the PR check. When the change since BaseRef touches the native shim or the managed interop
        structs, hand off to run-csdk-abi-check.ps1, which tests them against the newest C SDK release built
        from source. The pinned native cannot pass that change when it adapts to a C SDK release that has no
        published native yet. Any other change tests against the pinned native.

    Runnable locally to reproduce CI: ./scripts/run-managed-tests.ps1

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER PackageDir
    Folder holding a freshly packed OpenZiti.NET.native nupkg, added as a nuget source.

.PARAMETER PackageVersion
    Version of that packed package. Pins OpenZiti.NET.Tests to it, overriding the transitive pinned native.

.PARAMETER NativeLib
    A win-x64 ziti4dotnet.dll to test against in place of the pinned native's.

.PARAMETER BaseRef
    The commit the change is compared against (the PR base, or the previous main commit on a push).

.EXAMPLE
    ./run-managed-tests.ps1

.EXAMPLE
    ./run-managed-tests.ps1 -PackageDir ./artifacts -PackageVersion 1.18.7.289-preview

.EXAMPLE
    ./run-managed-tests.ps1 -BaseRef origin/main
#>
[CmdletBinding()]
param(
    [string] $Configuration = 'Release',
    [string] $PackageDir = '',
    [string] $PackageVersion = '',
    [string] $NativeLib = '',
    [string] $BaseRef = ''
)

$ErrorActionPreference = 'Stop'
$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')

if ([string]::IsNullOrWhiteSpace($PackageDir) -ne [string]::IsNullOrWhiteSpace($PackageVersion)) {
    throw "-PackageDir and -PackageVersion go together: pass both or neither."
}
if ($NativeLib -and $PackageDir) { throw "-NativeLib and -PackageDir are separate modes: pass one." }

# Files whose changes have to be tested against the C SDK they adapt to, not only the pinned native.
$abiPaths = @(
    'native/ZitiNativeApiForDotnetCore/library/',
    'native/ZitiNativeApiForDotnetCore/includes/',
    'OpenZiti.NET/src/OpenZiti/Native/',
    'OpenZiti.NET.Tests/TestNative.cs',
    'OpenZiti.NET.Tests/NativeLayout.cs'
)
if ($BaseRef) {
    $changed = @(git -C $repoRoot diff --name-only "$BaseRef...HEAD")
    if ($LASTEXITCODE -ne 0) { throw "git diff against $BaseRef failed ($LASTEXITCODE); is it fetched?" }
    $abiChanges = @($changed | Where-Object { $f = $_; $abiPaths | Where-Object { $f.StartsWith($_) } })
    if ($abiChanges.Count -gt 0) {
        Write-Host "ABI files changed since ${BaseRef}:"
        $abiChanges | ForEach-Object { Write-Host "  $_" }
        Write-Host "Testing against the newest C SDK release built from source."
        & (Join-Path $PSScriptRoot 'run-csdk-abi-check.ps1')
        return
    }
    Write-Host "No ABI files changed since $BaseRef."
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

if ($NativeLib) {
    $testBin = Join-Path $repoRoot "OpenZiti.NET.Tests/bin/$Configuration"
    $targets = @(Get-ChildItem $testBin -Recurse -Filter ziti4dotnet.dll |
        Where-Object { $_.FullName -match '[\\/]runtimes[\\/]win-x64[\\/]native[\\/]' })
    if ($targets.Count -eq 0) { throw "no runtimes/win-x64/native/ziti4dotnet.dll under $testBin to replace" }
    foreach ($t in $targets) { Copy-Item $NativeLib $t.FullName -Force }
    Write-Host "Testing native $NativeLib"
}

Write-Host "Running managed ABI tests ..."
dotnet test (Join-Path $repoRoot 'OpenZiti.NET.Tests/OpenZiti.NET.Tests.csproj') `
    -c $Configuration --no-build --nologo @nativeProps `
    --filter "FullyQualifiedName~NativeCodeValueChecker|FullyQualifiedName~NativeLayoutChecker"
if ($LASTEXITCODE -ne 0) { throw "managed tests failed ($LASTEXITCODE)" }

Write-Host "Managed tests passed."
