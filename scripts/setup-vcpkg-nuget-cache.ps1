<#
.SYNOPSIS
    Point vcpkg at a GitHub Packages NuGet feed as its binary cache (build once, push, pull).

.DESCRIPTION
    Escapes the 10 GB GitHub Actions cache (and its eviction/outages): vcpkg pushes and pulls prebuilt binary
    packages to a NuGet feed hosted on GitHub Packages, keyed by ABI and shared across runs and C SDK
    versions. Once a dependency is built and pushed, every later run on any branch pulls it instead of
    recompiling.

    All logic is here so it runs locally too. Cross-platform: on Windows the fetched nuget.exe is used
    directly; on linux/mac vcpkg drives nuget through mono, which this installs if missing.

    Note for macOS: run-vcpkg sets VCPKG_FORCE_SYSTEM_BINARIES=1 (required for arm64), under which
    `vcpkg fetch nuget` returns the bare `nuget` command (mono's own wrapper, not a CIL assembly) instead of
    a real NuGet.exe, so `mono nuget` fails. We clear that flag only for the fetch so vcpkg downloads a real
    NuGet.exe, then restore it. Invoke-Nuget also falls back to calling the command directly if a bare name
    still comes back.

    Sets VCPKG_BINARY_SOURCES for the rest of the job (writes to GITHUB_ENV in CI) and for the current process
    (local runs). Run AFTER vcpkg is bootstrapped (VCPKG_ROOT must be set).

.PARAMETER Owner
    GitHub owner whose Packages feed to use (e.g. openziti). Defaults to the GITHUB_REPOSITORY_OWNER env var.

.PARAMETER Token
    A token with packages:write (in CI, secrets.GITHUB_TOKEN). A read-only token still works, it just cannot
    push, so the cache degrades to pull-only (e.g. on fork PRs).

.PARAMETER Mode
    'readwrite' (default) or 'read'.

.EXAMPLE
    ./setup-vcpkg-nuget-cache.ps1 -Owner openziti -Token $env:GH_TOKEN
#>
[CmdletBinding()]
param(
    [string] $Owner = $env:GITHUB_REPOSITORY_OWNER,
    [string] $Token,
    [ValidateSet('read', 'readwrite')] [string] $Mode = 'readwrite'
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Owner)) { throw "Owner is required (-Owner or GITHUB_REPOSITORY_OWNER)." }
if ([string]::IsNullOrWhiteSpace($Token)) { throw "Token is required (-Token)." }
if ([string]::IsNullOrWhiteSpace($env:VCPKG_ROOT)) { throw "VCPKG_ROOT is not set; run this after vcpkg is bootstrapped." }

$feed = "https://nuget.pkg.github.com/$Owner/index.json"
$vcpkg = Join-Path $env:VCPKG_ROOT ($IsWindows ? 'vcpkg.exe' : 'vcpkg')

# On non-Windows, vcpkg invokes nuget through mono. Make sure it exists.
if (-not $IsWindows -and -not (Get-Command mono -ErrorAction SilentlyContinue)) {
    Write-Host "Installing mono (needed to run nuget for vcpkg's binary cache) ..."
    if ($IsLinux) {
        sudo apt-get update -y
        sudo apt-get install -y mono-complete
    }
    elseif ($IsMacOS) {
        brew install mono
    }
}

# Fetch the NuGet.exe vcpkg manages. On macOS, VCPKG_FORCE_SYSTEM_BINARIES=1 makes fetch hand back the bare
# `nuget` (mono's wrapper, not a real assembly), which breaks `mono nuget`. Clear it just for the fetch so a
# real NuGet.exe is downloaded, then restore the original value (the build step has its own process anyway).
$savedForceSystemBinaries = $env:VCPKG_FORCE_SYSTEM_BINARIES
if (-not $IsWindows) { $env:VCPKG_FORCE_SYSTEM_BINARIES = $null }
try {
    $nuget = (& $vcpkg fetch nuget | Select-Object -Last 1).Trim()
}
finally {
    if ($null -ne $savedForceSystemBinaries) { $env:VCPKG_FORCE_SYSTEM_BINARIES = $savedForceSystemBinaries }
}
if ([string]::IsNullOrWhiteSpace($nuget)) { throw "vcpkg fetch nuget returned nothing." }
Write-Host "Using NuGet at $nuget"

function Invoke-Nuget {
    param([Parameter(ValueFromRemainingArguments = $true)] [string[]] $NugetArgs)
    if ($IsWindows) { & $nuget @NugetArgs }
    elseif (Test-Path $nuget) { & mono $nuget @NugetArgs }  # a real NuGet.exe on disk
    else { & $nuget @NugetArgs }                            # a 'nuget' command on PATH that drives mono itself
}

# Register the feed with credentials so vcpkg's nuget can authenticate. Re-running is harmless.
Write-Host "Configuring NuGet source $feed ..."
Invoke-Nuget sources add -Source $feed -Name github -UserName $Owner -Password $Token -StorePasswordInCleartext
Invoke-Nuget setapikey $Token -Source $feed

# Append the nuget feed to whatever is already configured (in CI the job env sets a files source first, so
# restore order ends up files-then-nuget). Locally, with nothing preset, fall back to a clean nuget-only chain.
$existing = $env:VCPKG_BINARY_SOURCES
$sources = if ([string]::IsNullOrWhiteSpace($existing)) { "clear;nuget,$feed,$Mode" }
           else { "$existing;nuget,$feed,$Mode" }
$env:VCPKG_BINARY_SOURCES = $sources
if ($env:GITHUB_ENV) {
    "VCPKG_BINARY_SOURCES=$sources" | Out-File -FilePath $env:GITHUB_ENV -Append -Encoding utf8
}
Write-Host "VCPKG_BINARY_SOURCES=$sources"
