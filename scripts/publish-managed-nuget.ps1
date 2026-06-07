<#
.SYNOPSIS
    Build, pack, and push the managed OpenZiti.NET nuget package.

.DESCRIPTION
    Wraps the `dotnet build /t:NugetPush` invocation that publishes the idiomatic managed SDK
    (OpenZiti.NET) to a nuget feed. Kept as a standalone script so a maintainer can run the exact same
    publish locally; the workflow only checks out and invokes this.

    GitHub-specific values (the API key, the feed URL) are parameters so they can be supplied by hand.

.PARAMETER ApiKey
    The nuget API key to push with. Required to push; omit (with -DryRun) to build/pack only.

.PARAMETER NugetSource
    The nuget feed to push to. Defaults to nuget.org.

.PARAMETER Configuration
    Build configuration. Defaults to Release.

.PARAMETER DryRun
    Build and pack but do not push (no API key needed).

.EXAMPLE
    ./publish-managed-nuget.ps1 -ApiKey $env:NUGET_API_KEY

.EXAMPLE
    ./publish-managed-nuget.ps1 -DryRun
#>
[CmdletBinding()]
param(
    [string] $ApiKey,
    [string] $NugetSource = 'https://api.nuget.org/v3/index.json',
    [string] $Configuration = 'Release',
    [switch] $DryRun
)

$ErrorActionPreference = 'Stop'

$repoRoot = Resolve-Path (Join-Path $PSScriptRoot '..')
$project = Join-Path $repoRoot 'OpenZiti.NET/OpenZiti.NET.csproj'

if ($DryRun) {
    Write-Host "DRY RUN: building and packing $project ($Configuration), no push"
    dotnet build $project /t:NugetPack /p:Configuration=$Configuration
    if ($LASTEXITCODE -ne 0) { throw "dotnet NugetPack failed ($LASTEXITCODE)" }
    return
}

if ([string]::IsNullOrWhiteSpace($ApiKey)) {
    throw "ApiKey is required to push. Pass -ApiKey, or use -DryRun to build/pack only."
}

Write-Host "Publishing $project ($Configuration) to $NugetSource"
dotnet build $project /t:NugetPush /p:Configuration=$Configuration /p:NUGET_SOURCE=$NugetSource /p:API_KEY=$ApiKey
if ($LASTEXITCODE -ne 0) { throw "dotnet NugetPush failed ($LASTEXITCODE)" }
