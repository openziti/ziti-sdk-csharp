<#
.SYNOPSIS
    Sync our native vcpkg setup (manifest, overlays, configuration, toolchains) from a ziti-sdk-c release tag.

.DESCRIPTION
    Our native build (native/ZitiNativeApiForDotnetCore) FetchContent's the C SDK as a subproject, but vcpkg
    manifest mode only reads the ROOT manifest (ours), so the C SDK's own vcpkg.json / vcpkg-overlays /
    vcpkg-configuration.json / toolchains are IGNORED during our build. That means we must carry our own copies,
    and they drift from upstream every release. That drift is what breaks the build (e.g. the Windows legs failing
    "pkg-config tool not found" because upstream declared a pkgconf host dependency that our manifest never copied).

    This script removes the drift: for a given C SDK tag it pulls upstream's
        vcpkg.json, vcpkg-configuration.json, vcpkg-overlays/, toolchains/
    and writes them into our native project so our vcpkg inputs match what the C SDK was built and tested against.

    Baseline policy: we DO sync builtin-baseline to match the SDK. The vcpkg binary cache is keyed by that
    baseline, so a baseline change naturally lands in new cache assets (the old ones are simply not reused). Expect
    the first build after a baseline change to be cold (openssl/protobuf/etc. rebuild) before the cache refills.

    Toolchains: upstream files overwrite ours; files we carry that upstream does not (e.g. git.cmake,
    Windows-win32.cmake) are kept, not deleted. Overlays and vcpkg-configuration.json are mirrored exactly
    (including upstream deletions) so stale overlay ports do not linger.

    Locally runnable: needs git and the gh-cloneable C SDK repo. CI invokes the exact same script. Review the
    result with `git diff` before committing; this script never commits.

.PARAMETER Version
    The C SDK release tag to sync from (e.g. 1.17.0). Defaults to the ZITI_SDK_C_BRANCH env var.

.PARAMETER CSdkRepo
    owner/name of the C SDK repo. Defaults to openziti/ziti-sdk-c.

.PARAMETER NativeDir
    The native project to sync into. Defaults to native/ZitiNativeApiForDotnetCore under the repo root.

.PARAMETER DryRun
    Report what would change (file-level diffs and the baseline move) without modifying the working tree.

.EXAMPLE
    ./sync-csdk-vcpkg.ps1 -Version 1.17.0

.EXAMPLE
    ./sync-csdk-vcpkg.ps1 -Version 1.17.0 -DryRun
#>
[CmdletBinding()]
param(
    [string] $Version = $env:ZITI_SDK_C_BRANCH,
    [string] $CSdkRepo = 'openziti/ziti-sdk-c',
    [string] $NativeDir,
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Version is required (pass -Version or set ZITI_SDK_C_BRANCH)."
}

$repoRoot = Split-Path $PSScriptRoot -Parent
if ([string]::IsNullOrWhiteSpace($NativeDir)) {
    $NativeDir = Join-Path $repoRoot 'native/ZitiNativeApiForDotnetCore'
}
if (-not (Test-Path $NativeDir)) {
    throw "Native dir not found: $NativeDir"
}

# Paths we sync. 'mirror' = match upstream exactly (propagate deletions); 'merge' = overwrite from upstream but
# keep local-only files; 'file-mirror' = copy if upstream has it, delete ours if upstream dropped it.
$syncPlan = @(
    @{ Path = 'vcpkg.json';               Mode = 'file' }
    @{ Path = 'vcpkg-configuration.json'; Mode = 'file-mirror' }
    @{ Path = 'vcpkg-overlays';           Mode = 'mirror' }
    @{ Path = 'toolchains';               Mode = 'merge' }
)

function Get-Baseline([string] $manifestPath) {
    if (-not (Test-Path $manifestPath)) { return '<none>' }
    try { return (Get-Content $manifestPath -Raw | ConvertFrom-Json).'builtin-baseline' }
    catch { return '<unparseable>' }
}

# Print a file diff for dry-run, tolerating either side being absent (added/removed).
function Show-Diff([string] $current, [string] $incoming, [string] $label) {
    $hasCur = Test-Path $current
    $hasInc = Test-Path $incoming
    if ($hasCur -and $hasInc) { & git --no-pager diff --no-index -- $current $incoming }
    elseif ($hasInc)          { Write-Host "  would ADD    $label" }
    elseif ($hasCur)          { Write-Host "  would DELETE $label" }
}

$tmp = Join-Path ([System.IO.Path]::GetTempPath()) ("csdk-vcpkg-sync-" + [System.Guid]::NewGuid().ToString('N'))
$cloneUrl = "https://github.com/$CSdkRepo.git"

Write-Host "Syncing vcpkg inputs from $CSdkRepo @ $Version"
Write-Host "  into: $NativeDir"
if ($DryRun) { Write-Host "  (dry run, no files will change)" }

try {
    # Shallow, blobless, sparse clone: fetch only the trees we need at exactly this tag.
    & git clone --quiet --depth 1 --branch $Version --filter=blob:none --sparse $cloneUrl $tmp
    if ($LASTEXITCODE -ne 0) { throw "git clone of $cloneUrl @ $Version failed (exit $LASTEXITCODE). Is the tag published?" }
    & git -C $tmp sparse-checkout set --no-cone @($syncPlan.Path)
    if ($LASTEXITCODE -ne 0) { throw "git sparse-checkout failed (exit $LASTEXITCODE)." }

    $oldBaseline = Get-Baseline (Join-Path $NativeDir 'vcpkg.json')
    $newBaseline = Get-Baseline (Join-Path $tmp 'vcpkg.json')

    foreach ($item in $syncPlan) {
        $name = $item.Path
        $src  = Join-Path $tmp $name
        $dst  = Join-Path $NativeDir $name
        $srcExists = Test-Path $src

        switch ($item.Mode) {
            'file' {
                if (-not $srcExists) { throw "Expected $name in the C SDK @ $Version but it is missing." }
                if ($DryRun) { Show-Diff $dst $src $name; continue }
                Copy-Item $src $dst -Force
                Write-Host "  synced  $name"
            }
            'file-mirror' {
                if ($DryRun) { Show-Diff $dst $src $name; continue }
                if ($srcExists) {
                    Copy-Item $src $dst -Force
                    Write-Host "  synced  $name"
                } elseif (Test-Path $dst) {
                    Remove-Item $dst -Force
                    Write-Host "  deleted $name (not in upstream)"
                }
            }
            'mirror' {
                if ($DryRun) {
                    $curFiles = @()
                    $incFiles = @()
                    if (Test-Path $dst) { $curFiles = Get-ChildItem $dst -Recurse -File }
                    if ($srcExists)     { $incFiles = Get-ChildItem $src -Recurse -File }
                    foreach ($f in $incFiles) {
                        Show-Diff (Join-Path $dst $f.FullName.Substring($src.Length).TrimStart('\','/')) $f.FullName "$name/$($f.FullName.Substring($src.Length).TrimStart('\','/'))"
                    }
                    foreach ($f in $curFiles) {
                        $rel = $f.FullName.Substring($dst.Length).TrimStart('\','/')
                        if (-not (Test-Path (Join-Path $src $rel))) { Write-Host "  would DELETE $name/$rel" }
                    }
                    continue
                }
                if (Test-Path $dst) { Remove-Item $dst -Recurse -Force }
                if ($srcExists) {
                    Copy-Item $src $dst -Recurse -Force
                    $subdirs = (Get-ChildItem $dst -Directory -ErrorAction SilentlyContinue | Select-Object -ExpandProperty Name) -join ', '
                    Write-Host "  mirrored $name/ (now: $subdirs)"
                } else {
                    Write-Host "  removed $name/ (not in upstream)"
                }
            }
            'merge' {
                if (-not $srcExists) { Write-Host "  skip    $name (not in upstream)"; continue }
                Get-ChildItem $src -File | ForEach-Object {
                    $target = Join-Path $dst $_.Name
                    if ($DryRun) { Show-Diff $target $_.FullName "$name/$($_.Name)" }
                    else { Copy-Item $_.FullName $target -Force }
                }
                if (-not $DryRun) {
                    $kept = @()
                    if (Test-Path $dst) {
                        $upstreamNames = (Get-ChildItem $src -File).Name
                        $kept = (Get-ChildItem $dst -File).Name | Where-Object { $upstreamNames -notcontains $_ }
                    }
                    Write-Host "  synced  $name/ (kept local-only: $([string]::Join(', ', $kept)))"
                }
            }
        }
    }

    Write-Host ""
    if ($oldBaseline -ne $newBaseline) {
        Write-Host "baseline: $oldBaseline -> $newBaseline"
        Write-Host "  NOTE: baseline changed. The vcpkg binary cache is keyed by baseline, so the first build will be"
        Write-Host "        cold (deps rebuild) until the new cache assets are produced."
    } else {
        Write-Host "baseline: unchanged ($oldBaseline)"
    }
    Write-Host ""
    if ($DryRun) { Write-Host "Dry run complete. Re-run without -DryRun to apply." }
    else { Write-Host "Done. Review with 'git diff' before committing." }
}
finally {
    if (Test-Path $tmp) { Remove-Item $tmp -Recurse -Force -ErrorAction SilentlyContinue }
}
