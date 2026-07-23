<#
.SYNOPSIS
    Create (and push) a lightweight git tag pointing at a commit. Idempotent. No GitHub Release.

.DESCRIPTION
    Marks a published build with a tag so the exact commit behind a nuget version is findable, without the
    ceremony of a GitHub Release. Kept as a standalone script so a maintainer can run the same step locally; the
    workflow only checks out and invokes this. A lightweight tag inherits the target commit's "Verified" state.

.PARAMETER Tag
    The tag name to create, e.g. `OpenZiti.NET.native/1.18.2.50`.

.PARAMETER Sha
    The commit to tag. Defaults to HEAD.

.EXAMPLE
    ./create-tag.ps1 -Tag 'OpenZiti.NET.native/1.18.2.50' -Sha $env:GITHUB_SHA
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Tag,
    [string] $Sha = 'HEAD'
)

$ErrorActionPreference = 'Stop'

# Idempotent: if the tag already exists on the remote, a re-run is a no-op rather than a failed push.
git ls-remote --exit-code origin "refs/tags/$Tag" *> $null
if ($LASTEXITCODE -eq 0) {
    Write-Host "tag $Tag already exists on origin, skipping"
    return
}

Write-Host "Tagging $Tag -> $Sha"
git tag $Tag $Sha
if ($LASTEXITCODE -ne 0) { throw "git tag $Tag failed ($LASTEXITCODE)" }
git push origin "refs/tags/$Tag"
if ($LASTEXITCODE -ne 0) { throw "git push of $Tag failed ($LASTEXITCODE)" }
