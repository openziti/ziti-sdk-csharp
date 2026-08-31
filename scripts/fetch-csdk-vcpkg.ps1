<#
.SYNOPSIS
    Fetch the vcpkg inputs and cross-compile toolchains for a ziti-sdk-c version into the native project.

.DESCRIPTION
    We compile the C SDK from source, but vcpkg manifest mode only reads the ROOT manifest, which is ours. So
    the C SDK's own vcpkg.json, vcpkg-configuration.json, vcpkg-overlays/ and toolchains/ are ignored during our
    build and we have to supply them. We do not own any of them and have no reason to edit them, so none are
    checked in: this fetches them for the version being built.

    Fetched: vcpkg.json, vcpkg-configuration.json, vcpkg-overlays/, toolchains/

    Writes .csdk-version with the tag it fetched. CMakeLists compares that stamp to ZITI_SDK_C_BRANCH and
    refuses to configure when it is missing or stale, because stale inputs do not fail the build, they build
    against another release's dependency set and compiler flags.

    Cheap to re-run: a shallow, blobless, sparse clone of four paths.

.PARAMETER Version
    C SDK release tag to fetch from. Defaults to the ZITI_SDK_C_BRANCH env var, the same one CMakeLists reads.

.PARAMETER CSdkRepo
    owner/name of the C SDK repo. Defaults to openziti/ziti-sdk-c.

.PARAMETER NativeDir
    Project to fetch into. Defaults to native/ZitiNativeApiForDotnetCore under the repo root.

.PARAMETER Force
    Re-fetch even when the stamp already matches the requested version.

.EXAMPLE
    ./fetch-csdk-vcpkg.ps1 -Version 1.18.9

.EXAMPLE
    $env:ZITI_SDK_C_BRANCH = '1.18.9'; ./fetch-csdk-vcpkg.ps1
#>
[CmdletBinding()]
param(
    [string] $Version = $env:ZITI_SDK_C_BRANCH,
    [string] $CSdkRepo = 'openziti/ziti-sdk-c',
    [string] $NativeDir,
    [switch] $Force
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Version is required (pass -Version or set ZITI_SDK_C_BRANCH)."
}

$repoRoot = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($NativeDir)) {
    $NativeDir = Join-Path $repoRoot 'native/ZitiNativeApiForDotnetCore'
}
if (-not (Test-Path $NativeDir)) { throw "Native dir not found: $NativeDir" }

# Paths are fetched wholesale: a directory is replaced, a file is overwritten. Nothing local survives in them,
# which is the point.
$paths = @('vcpkg.json', 'vcpkg-configuration.json', 'vcpkg-overlays', 'toolchains')
$stamp = Join-Path $NativeDir '.csdk-version'

if ((Test-Path $stamp) -and -not $Force) {
    $have = (Get-Content $stamp -Raw).Trim()
    if ($have -eq $Version) {
        Write-Host "vcpkg inputs already at $CSdkRepo @ $Version (use -Force to re-fetch)"
        return
    }
    Write-Host "vcpkg inputs are at $have, want ${Version}: re-fetching"
}

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("csdk-vcpkg-" + [System.Guid]::NewGuid().ToString('N'))
$cloneUrl = "https://github.com/$CSdkRepo.git"

Write-Host "Fetching vcpkg inputs from $CSdkRepo @ $Version"
try {
    & git clone --quiet --depth 1 --branch $Version --filter=blob:none --sparse $cloneUrl $tmp
    if ($LASTEXITCODE -ne 0) { throw "git clone of $cloneUrl @ $Version failed ($LASTEXITCODE). Is the tag published?" }
    & git -C $tmp sparse-checkout set --no-cone @($paths | ForEach-Object { "/$_" })
    if ($LASTEXITCODE -ne 0) { throw "git sparse-checkout failed ($LASTEXITCODE)." }

    foreach ($p in $paths) {
        $src = Join-Path $tmp $p
        $dst = Join-Path $NativeDir $p
        if (-not (Test-Path $src)) {
            # vcpkg-configuration.json is the one that may legitimately be absent in an older release.
            if ($p -eq 'vcpkg-configuration.json') {
                if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
                Write-Host "  skipped $p (not in $Version)"
                continue
            }
            throw "$CSdkRepo @ $Version has no $p."
        }
        if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
        Copy-Item $src $dst -Recurse -Force
        Write-Host "  fetched $p"
    }

    $Version | Set-Content -Path $stamp -Encoding utf8 -NoNewline

    $baseline = (Get-Content (Join-Path $NativeDir 'vcpkg.json') -Raw | ConvertFrom-Json).'builtin-baseline'
    Write-Host "  vcpkg baseline: $baseline"
}
finally {
    if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue }
}
