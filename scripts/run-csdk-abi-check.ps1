<#
.SYNOPSIS
    Build the win-x64 native shim against a ziti-sdk-c ref and run the managed ABI tests against that build.

.DESCRIPTION
    The managed interop structs have to match the native they run against. The PR check normally tests them
    against the native pinned in OpenZiti.NET.csproj, which only covers C SDK releases that already have a
    published OpenZiti.NET.native. This builds ziti4dotnet.dll from source instead, so the managed structs can be
    checked against a C SDK that has no published native yet:
      - the newest C SDK release (default), so a PR that adapts to a new release can pass before the native for
        that release is on nuget.org
      - a C SDK branch such as main, so a struct change shows up before the C SDK releases it

    ziti.def is generated from a C SDK release zip, and a branch has none, so a branch build uses the newest
    release's exports. A branch that removes an exported function then fails the link, which is also a break
    worth hearing about. ziti.def is a tracked file: a local run leaves it modified.

    Windows x64 only: the managed tests verify the full struct catalog on Windows (see ci.yml).

.PARAMETER CsdkRef
    The ziti-sdk-c tag or branch to build. Blank resolves the newest release, prereleases included, the same
    way the native publish does.

.PARAMETER VcpkgRoot
    An existing vcpkg checkout. Defaults to VCPKG_ROOT; when that is unset too, vcpkg is cloned and
    bootstrapped under -WorkDir at the baseline the C SDK's vcpkg.json names.

.PARAMETER WorkDir
    Scratch space for the vcpkg clone, the binary cache, and the build tree. Defaults to a temp dir.

.PARAMETER Preset
    A configure preset, usually one from a local CMakeUserPresets.json. The preset then owns the generator, the
    vcpkg root, the binary cache and the installed dir; -VcpkgRoot and the release cache restore are
    skipped. The build goes to build/<Preset> under the native dir. Enters a VS developer shell when cl is not on
    PATH. Use this for local runs, so the build reuses the local vcpkg binary cache.

.PARAMETER Repo
    owner/name whose native-build-cache release holds the vcpkg binary cache. Defaults to GITHUB_REPOSITORY,
    then openziti/ziti-sdk-csharp.

.EXAMPLE
    ./run-csdk-abi-check.ps1

.EXAMPLE
    ./run-csdk-abi-check.ps1 -CsdkRef 1.20.0 -Preset win64-local
#>
[CmdletBinding()]
param(
    [string] $CsdkRef = '',
    [string] $VcpkgRoot = $env:VCPKG_ROOT,
    [string] $WorkDir = (Join-Path ([System.IO.Path]::GetTempPath()) 'z4d-csdk-abi'),
    [string] $Preset = '',
    [string] $Repo = $(if ($env:GITHUB_REPOSITORY) { $env:GITHUB_REPOSITORY } else { 'openziti/ziti-sdk-csharp' }),
    [string] $CSdkRepo = 'openziti/ziti-sdk-c',
    [string] $GithubToken = $env:GH_TOKEN
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

if (-not $IsWindows) { throw "the C SDK ABI check builds and tests win-x64; run it on Windows." }

$repoRoot = Split-Path $PSScriptRoot -Parent
$nativeDir = Join-Path $repoRoot 'native/ZitiNativeApiForDotnetCore'
New-Item -ItemType Directory -Force $WorkDir | Out-Null

# Newest C SDK release, prereleases included; same rule as check-and-publish-native.ps1.
function Get-NewestCsdkRelease {
    $headers = @{
        'Accept'               = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent'           = 'run-csdk-abi-check'
    }
    if ($GithubToken) { $headers['Authorization'] = "Bearer $GithubToken" }
    $releases = Invoke-RestMethod -Method Get -Headers $headers `
        -Uri "https://api.github.com/repos/$CSdkRepo/releases?per_page=50"
    $newest = $releases |
        Where-Object { -not $_.draft -and $_.tag_name -match '^\d+\.\d+\.\d+' } |
        Sort-Object -Property @{ Expression = { [version]($_.tag_name -replace '^(\d+\.\d+\.\d+).*$', '$1') }; Descending = $true },
                              @{ Expression = { [bool]$_.prerelease }; Descending = $false } |
        Select-Object -First 1
    if (-not $newest) { throw "no $CSdkRepo release has a version-like tag." }
    return $newest.tag_name
}

$newestRelease = Get-NewestCsdkRelease
if ([string]::IsNullOrWhiteSpace($CsdkRef)) { $CsdkRef = $newestRelease }
$isRelease = $CsdkRef -match '^\d+\.\d+\.\d+'
$defVersion = if ($isRelease) { $CsdkRef } else { $newestRelease }
Write-Host "C SDK ref: $CsdkRef (ziti.def from release $defVersion)"

# CMakeLists reads the C SDK ref from this env var.
$env:ZITI_SDK_C_BRANCH = $CsdkRef

# A branch moves, so its vcpkg inputs are re-fetched every run; a release tag's stamp can be trusted.
& (Join-Path $PSScriptRoot 'fetch-csdk-vcpkg.ps1') -Version $CsdkRef -Force:(-not $isRelease)

$baseline = (Get-Content (Join-Path $nativeDir 'vcpkg.json') -Raw | ConvertFrom-Json).'builtin-baseline'
if ($Preset) {
    Write-Host "vcpkg root, binary cache and installed dir come from preset $Preset (baseline $baseline)"
} elseif ([string]::IsNullOrWhiteSpace($VcpkgRoot)) {
    # Manifest mode resolves the baseline from the registry's git history, so this has to be a full clone.
    $VcpkgRoot = Join-Path $WorkDir 'vcpkg'
    if (-not (Test-Path (Join-Path $VcpkgRoot '.git'))) {
        git clone --quiet https://github.com/microsoft/vcpkg.git $VcpkgRoot
        if ($LASTEXITCODE -ne 0) { throw "git clone of vcpkg failed ($LASTEXITCODE)" }
    }
    git -C $VcpkgRoot fetch --quiet origin
    if ($LASTEXITCODE -ne 0) { throw "git fetch of vcpkg failed ($LASTEXITCODE)" }
    git -C $VcpkgRoot checkout --quiet $baseline
    if ($LASTEXITCODE -ne 0) { throw "vcpkg checkout of baseline $baseline failed ($LASTEXITCODE)" }
    & (Join-Path $VcpkgRoot 'bootstrap-vcpkg.bat') -disableMetrics
    if ($LASTEXITCODE -ne 0) { throw "vcpkg bootstrap failed ($LASTEXITCODE)" }
} else {
    # An older checkout fails late, mid-install, with "no version database entry".
    git -C $VcpkgRoot merge-base --is-ancestor $baseline HEAD 2>$null
    if ($LASTEXITCODE -ne 0) { throw "vcpkg at $VcpkgRoot predates baseline $baseline; update it or unset VCPKG_ROOT" }
}
if (-not $Preset) {
    Write-Host "vcpkg: $VcpkgRoot (baseline $baseline)"

    # The release cache is built on windows-2022 runners, so it only hits for that same compiler.
    $cacheDir = Join-Path $WorkDir 'vcpkg-bincache'
    $env:VCPKG_BINARY_SOURCES = "clear;files,$cacheDir,readwrite"
    & (Join-Path $PSScriptRoot 'sync-vcpkg-cache.ps1') -Action restore -Repo $Repo -Rid win-x64 -CacheDir $cacheDir
}

& (Join-Path $PSScriptRoot 'regenerate-ziti-def.ps1') -Version $defVersion

$buildDir = if ($Preset) { Join-Path $nativeDir "build/$Preset" } else { Join-Path $WorkDir 'build-win64' }
Push-Location $nativeDir
try {
    if ($Preset -and -not (Get-Command cl -ErrorAction SilentlyContinue)) {
        $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
        $vsPath = (& $vswhere -latest -property installationPath).Trim()
        Import-Module (Join-Path $vsPath 'Common7\Tools\Microsoft.VisualStudio.DevShell.dll')
        Enter-VsDevShell -VsInstallPath $vsPath -SkipAutomaticLocation `
            -DevCmdArguments '-arch=x64 -host_arch=x64' | Out-Null
    }
    if ($Preset) {
        cmake --preset $Preset -B $buildDir
    } else {
        # regenerate-ziti-def.ps1's VS dev shell points VCPKG_ROOT at the vcpkg bundled with VS.
        $env:VCPKG_ROOT = $VcpkgRoot
        cmake --preset win64 -B $buildDir
    }
    if ($LASTEXITCODE -ne 0) { throw "cmake configure failed ($LASTEXITCODE)" }
    cmake --build $buildDir --config Release --target ziti4dotnet
    if ($LASTEXITCODE -ne 0) { throw "native build failed ($LASTEXITCODE)" }
} finally {
    Pop-Location
}

$dll = Get-ChildItem $buildDir -Recurse -Filter ziti4dotnet.dll | Select-Object -First 1
if (-not $dll) { throw "ziti4dotnet.dll not found under $buildDir" }
Write-Host "built $($dll.FullName) against ziti-sdk-c $CsdkRef"

& (Join-Path $PSScriptRoot 'run-managed-tests.ps1') -NativeLib $dll.FullName
