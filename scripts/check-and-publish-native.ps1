<#
.SYNOPSIS
    Decide whether the newest openziti/ziti-sdk-c release still needs to be published to nuget.org
    as the OpenZiti.NET.native package.

.DESCRIPTION
    Decides whether a ziti-sdk-c version still needs to be published to nuget.org as OpenZiti.NET.native.
    Two modes:
      - Default (no -Version): resolves the latest ziti-sdk-c release tag and checks that one. Used by the
        nightly job.
      - -Version <tag>: checks that one specific tag. Used as the pre-flight guard inside the publish
        workflow so an already-published version just skips instead of double-publishing.

    Comparison is on the 3-part base version (the published nuget version is "<base>.<run_number>", e.g.
    1.16.0.213, so 1.16.0 counts as "1.16.0 published").

    Prints a human-readable summary. When running inside GitHub Actions ($env:GITHUB_OUTPUT is set), also
    writes 'shouldPublish', 'version', and 'baseVersion' outputs so a downstream job can gate on them.

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

if ($Version) {
    # Explicit mode: check exactly this tag (the publish workflow's pre-flight guard / manual backfill).
    $releaseTag = $Version
    $releaseBase = Get-BaseVersion $releaseTag
    if (-not $releaseBase) {
        throw "Could not parse a 3-part version from -Version '$releaseTag'."
    }
    Write-Host "Checking specific version: tag '$releaseTag' -> base version '$releaseBase'"
}
else {
    Write-Host "Resolving latest release of $CSdkRepo ..."

    $ghHeaders = @{
        'Accept'               = 'application/vnd.github+json'
        'X-GitHub-Api-Version' = '2022-11-28'
        'User-Agent'           = 'check-and-publish-native'
    }
    if ($GithubToken) { $ghHeaders['Authorization'] = "Bearer $GithubToken" }

    $latestRelease = Invoke-RestMethod -Method Get -Headers $ghHeaders `
        -Uri "https://api.github.com/repos/$CSdkRepo/releases/latest"

    $releaseTag = $latestRelease.tag_name
    $releaseBase = Get-BaseVersion $releaseTag
    if (-not $releaseBase) {
        throw "Could not parse a 3-part version from latest release tag '$releaseTag'."
    }
    Write-Host "Latest $CSdkRepo release: tag '$releaseTag' -> base version '$releaseBase'"
}

Write-Host "Resolving published versions of $NugetPackageId on nuget.org ..."

# nuget flatcontainer index is lowercase by convention.
$pkgLower = $NugetPackageId.ToLowerInvariant()
$publishedBases = @()
try {
    $index = Invoke-RestMethod -Method Get -Uri "https://api.nuget.org/v3-flatcontainer/$pkgLower/index.json"
    $publishedBases = @($index.versions | ForEach-Object { Get-BaseVersion $_ } | Where-Object { $_ } | Sort-Object -Unique)
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

Write-Host "Published base versions on nuget.org: $([string]::Join(', ', $publishedBases))"

$alreadyPublished = $publishedBases -contains $releaseBase
$shouldPublish = -not $alreadyPublished

Write-Host ''
if ($alreadyPublished) {
    Write-Host "Release '$releaseBase' is already published. Nothing to do."
}
else {
    Write-Host "Release '$releaseBase' is NOT yet published."
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

# Exit 0 always: "nothing to publish" is a normal, successful outcome.
exit 0
