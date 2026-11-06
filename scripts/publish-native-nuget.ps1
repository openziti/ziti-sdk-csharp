<#
.SYNOPSIS
    Push a packed OpenZiti.NET.native nupkg to a nuget feed.

.DESCRIPTION
    CI passes the short-lived key minted by NuGet/login (trusted publishing). Run locally with a regular nuget.org
    API key.

.PARAMETER Package
    Path to the OpenZiti.NET.native .nupkg to push.

.PARAMETER ApiKey
    The nuget API key to push with.

.PARAMETER NugetSource
    The nuget feed to push to. Defaults to nuget.org.

.EXAMPLE
    ./publish-native-nuget.ps1 -Package ./artifacts/OpenZiti.NET.native.1.21.1.370.nupkg -ApiKey $env:NUGET_API_KEY
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory)] [string] $Package,
    [Parameter(Mandatory)] [string] $ApiKey,
    [string] $NugetSource = 'https://api.nuget.org/v3/index.json'
)

$ErrorActionPreference = 'Stop'

if (-not (Test-Path $Package)) { throw "package not found: $Package" }

Write-Host "Pushing $Package to $NugetSource"
dotnet nuget push $Package --api-key $ApiKey --source $NugetSource
if ($LASTEXITCODE -ne 0) { throw "dotnet nuget push failed ($LASTEXITCODE)" }
