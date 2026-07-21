<#
.SYNOPSIS
    Build, pack, and push the managed OpenZiti.NET nuget package, and optionally tag the release.

.DESCRIPTION
    Wraps the `dotnet build /t:NugetPush` invocation that publishes the idiomatic managed SDK
    (OpenZiti.NET) to a nuget feed. Kept as a standalone script so a maintainer can run the exact same
    publish locally; the workflow only checks out and invokes this.

    The managed version is date-based and computed inside msbuild (see OpenZiti.NET.csproj <Version>).
    We evaluate it ONCE here and pin the pack to it, so the pushed package and the git tag share one
    exact string instead of two msbuild invocations each stamping their own UtcNow.

    GitHub-specific values (the API key, the feed URL) are parameters so they can be supplied by hand.

.PARAMETER ApiKey
    The nuget API key to push with. Required to push; omit (with -DryRun) to build/pack only.

.PARAMETER NugetSource
    The nuget feed to push to. Defaults to nuget.org.

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER DryRun
    Build and pack but do not push (no API key needed). Never tags.

.PARAMETER Tag
    After a successful push, create the git tag `OpenZiti.NET/<version>` on HEAD and push it to origin.
    Idempotent: a re-run of an already-tagged version is a no-op, not a failure. Off by default so a
    local publish never mints a tag unless asked. The tag name does not start with `v`, so it does not
    re-trigger this repo's `v*`-tag publish workflow.

.EXAMPLE
    ./publish-managed-nuget.ps1 -ApiKey $env:NUGET_API_KEY -Tag

.EXAMPLE
    ./publish-managed-nuget.ps1 -DryRun
#>
[CmdletBinding()]
param(
    [string] $ApiKey,
    [string] $NugetSource = 'https://api.nuget.org/v3/index.json',
    [string] $Configuration = 'Release',
    [switch] $DryRun,
    [switch] $Tag
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $repoRoot 'OpenZiti.NET/OpenZiti.NET.csproj'

# Resolve the date-based version once, then pin every downstream build to it so the pushed package and
# the tag agree exactly (each msbuild invocation would otherwise recompute UtcNow and drift by seconds).
$verLines = dotnet msbuild $project --getProperty:Version
if ($LASTEXITCODE -ne 0) { throw "failed to read Version from $project ($LASTEXITCODE)" }
$version = ($verLines | Where-Object { $_ -match '\S' } | Select-Object -Last 1).Trim()
if ([string]::IsNullOrWhiteSpace($version)) { throw "resolved an empty managed version" }
Write-Host "Resolved managed version: $version"

if ($DryRun) {
    Write-Host "DRY RUN: building and packing $project ($Configuration) as $version, no push"
    dotnet build $project /t:NugetPack /p:Configuration=$Configuration /p:Version=$version /p:PackageVersion=$version
    if ($LASTEXITCODE -ne 0) { throw "dotnet NugetPack failed ($LASTEXITCODE)" }
    return
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "ApiKey is required to push. Pass -ApiKey, or use -DryRun to build/pack only."
}

Write-Host "Publishing $project ($Configuration) $version to $NugetSource"
dotnet build $project /t:NugetPush /p:Configuration=$Configuration /p:NUGET_SOURCE=$NugetSource /p:API_KEY=$ApiKey /p:Version=$version /p:PackageVersion=$version
if ($LASTEXITCODE -ne 0) { throw "dotnet NugetPush failed ($LASTEXITCODE)" }

if ($Tag) {
    $tagName = "OpenZiti.NET/$version"
    # Idempotent: an already-tagged version must not fail an otherwise-successful publish.
    if (git tag --list $tagName) {
        Write-Host "Tag $tagName already exists, skipping"
    } else {
        Write-Host "Tagging $tagName on HEAD and pushing to origin"
        git tag $tagName HEAD
        if ($LASTEXITCODE -ne 0) { throw "git tag $tagName failed ($LASTEXITCODE)" }
        git push origin $tagName
        if ($LASTEXITCODE -ne 0) { throw "git push of $tagName failed ($LASTEXITCODE)" }
    }
}
