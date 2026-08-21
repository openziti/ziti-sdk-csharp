<#
.SYNOPSIS
    Decide whether the newest openziti/ziti-sdk-c release still needs to be published to nuget.org
    as the OpenZiti.NET.native package.

.DESCRIPTION
    Decides whether a ziti-sdk-c version still needs to be published to nuget.org as OpenZiti.NET.native.
    Two modes:
      - Default (no -Version): resolves the newest ziti-sdk-c release, PRERELEASES INCLUDED, and checks that
        one. Used by the nightly job. A csdk prerelease ships as a nuget prerelease ("-preview").
      - -Version <tag>: checks that one specific tag. Used as the pre-flight guard inside the publish
        workflow so an already-published version just skips instead of double-publishing. A "-" label in
        the version (e.g. 1.18.7.50-preview) marks it as the preview channel.

    Comparison is on the 3-part base version PLUS the channel (stable or preview). The published nuget
    version is "<base>.<run_number>" with an optional "-preview" suffix, e.g. 1.16.0.213 or
    1.16.0.213-preview, so the run number never affects the decision. Carrying the channel in the key is
    what lets a csdk release publish once as a preview and then again, correctly, as stable when csdk
    promotes it.

    Prints a human-readable summary. When running inside GitHub Actions ($env:GITHUB_OUTPUT is set), also
    writes 'shouldPublish', 'version', 'baseVersion', and 'channel' outputs so a downstream job can gate
    on them.

    No hardcoded secrets or run-specific values: every external input is a parameter, so this script can be
    run by hand for an ad-hoc check or backfill.

.EXAMPLE
    ./check-and-publish-native.ps1 -GithubToken $env:GH_PAT -DryRun

.EXAMPLE
    ./check-and-publish-native.ps1 -Version 1.16.0
#>
[CmdletBinding()]
param(
    [string] $CSdkRepo = 'openziti/ziti-sdk-c',
    [string] $NugetPackageId = 'OpenZiti.NET.native',
    [string] $GithubToken = '',
    # When set, check THIS tag instead of resolving the latest release.
    [string] $Version = '',
    # Force publishing even if this version's base is already on nuget.org (shim/build changed, version did not).
    [switch] $Force,
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'
Set-StrictMode -Version Latest

function Write-GhOutput([string] $Name, [string] $Value) {
    if ($env:GITHUB_OUTPUT) {
        "$Name=$Value" | Out-File -FilePath $env:GITHUB_OUTPUT -Append -Encoding utf8
    }
}

# Normalize a version string to its first three numeric components, e.g. "v1.16.0" -> "1.16.0",
# "1.16.0.213" -> "1.16.0". Returns $null if no version-like prefix is found.
function Get-BaseVersion([string] $Raw) {
    if ([string]::IsNullOrWhiteSpace($Raw)) { return $null }
    $trimmed = $Raw.Trim().TrimStart('v', 'V')
    $m = [regex]::Match($trimmed, '^(\d+)\.(\d+)\.(\d+)')
    if (-not $m.Success) { return $null }
    return "$($m.Groups[1].Value).$($m.Groups[2].Value).$($m.Groups[3].Value)"
}

# The semver prerelease label of a version, e.g. "1.18.7.50-preview" -> "preview", "1.18.7.50" -> $null.
# Anything after the first '-' counts, so build metadata ("+abc") is left alone.
function Get-PrereleaseLabel([string] $Raw) {
    if ([string]::IsNullOrWhiteSpace($Raw)) { return $null }
    $m = [regex]::Match($Raw.Trim(), '-(?<label>[^+]+)')
    if (-not $m.Success) { return $null }
    return $m.Groups['label'].Value
}

if ($Version) {
    # Explicit mode: check exactly this tag (the publish workflow's pre-flight guard / manual backfill).
    $releaseTag = $Version
    $releaseBase = Get-BaseVersion $releaseTag
    if (-not $releaseBase) {
        throw "Could not parse a 3-part version from -Version '$releaseTag'."
    }
    # There is no release to inspect in this mode, so the channel comes from the version string itself.
    $channel = if (Get-PrereleaseLabel $releaseTag) { 'preview' } else { 'stable' }
    Write-Host "Checking specific version: tag '$releaseTag' -> base version '$releaseBase' ($channel)"
}
else {
    Write-Host "Resolving newest release of $CSdkRepo (prereleases included) ..."

    $ghHeaders = @{
        'Accept'               = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent'           = 'check-and-publish-native'
    }
    if ($GithubToken) { $ghHeaders['Authorization'] = "Bearer $GithubToken" }

    # NOT /releases/latest: that endpoint excludes prereleases by definition, and we want to build and ship
    # every csdk prerelease. Take the whole (newest-first) list, drop drafts, and pick the highest version.
    # No @() around the call: Invoke-RestMethod hands back the JSON array as a single object, and wrapping it
    # would make a one-element array of an array. Piping it enumerates the releases properly.
    $releases = Invoke-RestMethod -Method Get -Headers $ghHeaders `
        -Uri "https://api.github.com/repos/$CSdkRepo/releases?per_page=50"

    # Non-version tags (csdk has a rolling 'nightly' release) fall out here, since Get-BaseVersion returns null.
    $candidates = @($releases |
        Where-Object { -not $_.draft } |
        ForEach-Object {
            $b = Get-BaseVersion $_.tag_name
            if ($b) { [pscustomobject]@{ Tag = $_.tag_name; Base = $b; Prerelease = [bool]$_.prerelease } }
        })
    if ($candidates.Count -eq 0) {
        throw "No $CSdkRepo release has a version-like tag. Nothing to resolve."
    }

    # Highest base version wins. On a tie (csdk cut both '1.19.0-rc1' and '1.19.0'), the stable one wins,
    # so a promoted release is seen as stable rather than sticking on its own prerelease.
    $latest = $candidates |
        Sort-Object -Property @{ Expression = { [version]$_.Base }; Descending = $true },
                              @{ Expression = { $_.Prerelease }; Descending = $false } |
        Select-Object -First 1

    $releaseTag = $latest.Tag
    $releaseBase = $latest.Base
    $channel = if ($latest.Prerelease) { 'preview' } else { 'stable' }
    Write-Host "Newest $CSdkRepo release: tag '$releaseTag' -> base version '$releaseBase' ($channel)"
}

Write-Host "Resolving published versions of $NugetPackageId on nuget.org ..."

# nuget flatcontainer index is lowercase by convention.
$pkgLower = $NugetPackageId.ToLowerInvariant()
# Keys are "<base>|<channel>", e.g. "1.18.2|stable" or "1.18.7|preview". The run number (4th part) is
# deliberately dropped: it identifies a build of a csdk version, not a different csdk version.
$publishedKeys = @()
try {
    $index = Invoke-RestMethod -Method Get -Uri "https://api.nuget.org/v3-flatcontainer/$pkgLower/index.json"
    $publishedKeys = @($index.versions |
        ForEach-Object {
            $b = Get-BaseVersion $_
            if ($b) {
                $c = if (Get-PrereleaseLabel $_) { 'preview' } else { 'stable' }
                "$b|$c"
            }
        } | Sort-Object -Unique)
}
catch {
    # A 404 means the package has never been published; treat as "nothing published yet".
    if ($_.Exception.Response -and $_.Exception.Response.StatusCode.value__ -eq 404) {
        Write-Host "Package $NugetPackageId not found on nuget.org (nothing published yet)."
    }
    else {
        throw
    }
}

Write-Host "Published base versions on nuget.org: $([string]::Join(', ', $publishedKeys))"

$releaseKey = "$releaseBase|$channel"
$alreadyPublished = $publishedKeys -contains $releaseKey
$shouldPublish = -not $alreadyPublished

Write-Host ''
if ($alreadyPublished -and $Force) {
    Write-Host "Release '$releaseBase' ($channel) is already published, but -Force was set: publishing anyway (new run number)."
    $shouldPublish = $true
}
elseif ($alreadyPublished) {
    Write-Host "Release '$releaseBase' ($channel) is already published. Nothing to do."
}
else {
    Write-Host "Release '$releaseBase' ($channel) is NOT yet published."
    if ($DryRun) {
        Write-Host "[DryRun] Would publish version '$releaseTag' (passing tag to the build workflow)."
    }
    else {
        Write-Host "Will publish version '$releaseTag'."
    }
}

Write-GhOutput 'shouldPublish' ($shouldPublish.ToString().ToLowerInvariant())
Write-GhOutput 'version' $releaseTag
Write-GhOutput 'baseVersion' $releaseBase
Write-GhOutput 'channel' $channel

# Exit 0 always: "nothing to publish" is a normal, successful outcome.
exit 0
