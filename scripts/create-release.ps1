<#
.SYNOPSIS
    Create a GitHub Release (and its tag) for a published package, with notes pulled from a changelog.

.DESCRIPTION
    Cuts a GitHub Release via `gh release create`, which also creates the tag as a lightweight tag on the given
    commit. The release body is the matching section extracted from a Keep a Changelog-style file. Kept as a
    standalone script so a maintainer can run the exact same step locally; the workflow only checks out and invokes
    this.

    Verified-tag note: `gh release create` makes a lightweight tag, so the tag's "Verified" badge is inherited from
    the target commit. Pass a commit that GitHub itself signed (a Squash/Merge-button commit) to get a verified
    tag with no signing keys.

.PARAMETER Tag
    The tag/release name to create, e.g. `OpenZiti.NET.native/1.18.2.49`.

.PARAMETER Title
    The release title.

.PARAMETER TargetSha
    The commit the tag points at (the release's target commitish).

.PARAMETER ChangelogFile
    Path to the changelog (absolute, or relative to the repo root).

.PARAMETER Section
    The version key of the changelog section to extract, e.g. `1.18.2`. Matched against a `## [<Section>]` heading.
    A missing or empty section is a hard error, so a release is never cut with no notes.

.PARAMETER GitHubToken
    Token for `gh`. Optional; if omitted, `gh` uses the ambient GH_TOKEN/auth.

.PARAMETER Prerelease
    Mark the release as a prerelease.

.PARAMETER AllowGeneratedNotes
    If the changelog section is missing/empty, fall back to GitHub-generated notes instead of failing. Used by the
    nightly auto-publish, which can hit a brand-new ziti-sdk-c version before anyone has written its section.
    Omit it for deliberate releases so a missing section is a hard error.

.EXAMPLE
    ./create-release.ps1 -Tag 'OpenZiti.NET.native/1.18.2.49' -Title 'OpenZiti.NET.native 1.18.2.49' `
        -TargetSha $env:GITHUB_SHA -ChangelogFile CHANGELOG.native.md -Section 1.18.2
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Tag,
    [Parameter(Mandatory)] [string] $Title,
    [Parameter(Mandatory)] [string] $TargetSha,
    [Parameter(Mandatory)] [string] $ChangelogFile,
    [Parameter(Mandatory)] [string] $Section,
    [string] $GitHubToken,
    [switch] $Prerelease,
    [switch] $AllowGeneratedNotes
)

$ErrorActionPreference = 'Stop'

if ($GitHubToken) { $env:GH_TOKEN = $GitHubToken }

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$changelogPath = if ([System.IO.Path]::IsPathRooted($ChangelogFile)) { $ChangelogFile } else { Join-Path $repoRoot $ChangelogFile }
if (-not (Test-Path $changelogPath)) { throw "changelog not found: $changelogPath" }

# Pull the "## [<Section>]" block: everything from that heading up to the next "## [" heading.
$notes = New-Object System.Collections.Generic.List[string]
$inSection = $false
foreach ($line in (Get-Content -LiteralPath $changelogPath)) {
    if ($line -match '^##\s+\[') {
        if ($inSection) { break }
        if ($line -match "^##\s+\[$([regex]::Escape($Section))\]") { $inSection = $true }
        continue
    }
    if ($inSection) { $notes.Add($line) }
}
$body = ($notes -join "`n").Trim()
$useGenerated = $false
if ([string]::IsNullOrWhiteSpace($body)) {
    if ($AllowGeneratedNotes) {
        Write-Warning "no changelog section '[$Section]' in $ChangelogFile; falling back to GitHub-generated notes"
        $useGenerated = $true
    } else {
        throw "no changelog section '[$Section]' with content found in $ChangelogFile"
    }
}

# Idempotent: a re-run of an already-released tag is a no-op, not a failure.
gh release view $Tag *> $null
if ($LASTEXITCODE -eq 0) {
    Write-Host "release $Tag already exists, skipping"
    return
}

$ghArgs = @('release', 'create', $Tag, '--target', $TargetSha, '--title', $Title)
if ($useGenerated) {
    $ghArgs += '--generate-notes'
    Write-Host "Creating release $Tag on $TargetSha (GitHub-generated notes)"
} else {
    $notesFile = New-TemporaryFile
    Set-Content -LiteralPath $notesFile.FullName -Value $body -Encoding utf8
    $ghArgs += @('--notes-file', $notesFile.FullName)
    Write-Host "Creating release $Tag on $TargetSha (notes from [$Section] of $ChangelogFile)"
}
if ($Prerelease) { $ghArgs += '--prerelease' }

gh @ghArgs
if ($LASTEXITCODE -ne 0) { throw "gh release create failed ($LASTEXITCODE)" }
