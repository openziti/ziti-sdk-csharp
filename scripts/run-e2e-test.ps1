<#
.SYNOPSIS
    Run the native traffic e2e tests against a freshly packed OpenZiti.NET.native package, using a local
    `ziti edge quickstart` overlay.

.DESCRIPTION
    1. Fetches the official getZiti helper from get.openziti.io and uses it to put the ziti CLI on PATH.
    2. Starts `ziti edge quickstart` in the background, pinned to localhost, admin/admin.
    3. Waits for the controller to answer.
    4. Runs native/e2e/E2ETest.csproj (the HostedEcho + prox-c ProxyBridge tests) against the fresh package.
    5. Always tears the overlay down.

    The e2e drives traffic through the managed SDK, which loads the fresh native lib, so it proves the
    package actually works, not just that it loads. Locally runnable end to end.

    Note on ziti version: both the v1.6.x and v2.0.x quickstarts bootstrap an admin/admin controller on
    localhost and are validated by this script. ZitiVersion selects which CLI release to download; CI runs a
    matrix over both lines before publishing.

.PARAMETER PackageDir
    Folder containing the freshly packed OpenZiti.NET.native.<version>.nupkg (used as a local nuget source).

.PARAMETER PackageVersion
    The nuget version of the freshly packed package (e.g. 1.16.0.213). Named PackageVersion (not Version) on
    purpose: getZiti.ps1 is dot-sourced and uses $Version internally, and PowerShell variable names are
    case-insensitive, so a $Version here would be clobbered.

.PARAMETER Rid
    The runtime identifier to test. The e2e is gated on win-x64. Defaults to win-x64.

.PARAMETER ZitiVersion
    The ziti CLI release tag to download for the overlay (e.g. v1.6.14). Defaults to a known-good v1.x.

.PARAMETER CtrlAddress
    Controller advertised address. Defaults to localhost so enrolled identities resolve locally.

.PARAMETER CtrlPort
    Controller port. Defaults to 1280.

.EXAMPLE
    ./run-e2e-test.ps1 -PackageDir ./artifacts -PackageVersion 1.16.0.213
#>
[CmdletBinding()]
param(
    [Parameter(Mandatory = $true)] [string] $PackageDir,
    [Parameter(Mandatory = $true)] [string] $PackageVersion,
    [string] $Rid = 'win-x64',
    [string] $ZitiVersion = 'v1.6.14',
    [string] $GetZitiUrl = 'https://get.openziti.io/quick/getZiti.ps1',
    [string] $CtrlAddress = 'localhost',
    [int]    $CtrlPort = 1280,
    [string] $AdminUser = 'admin',
    [string] $AdminPassword = 'admin'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repoRoot 'native/e2e/E2ETest.csproj'
$source = (Resolve-Path $PackageDir).Path
$logFile = Join-Path ([System.IO.Path]::GetTempPath()) "ziti-quickstart-$PID.log"

# 1. Fetch the official getZiti helper and use it to put the ziti CLI on PATH. Downloaded fresh each run
#    (no committed dependency) and dot-sourced so its PATH change reaches this scope.
$getZiti = Join-Path ([System.IO.Path]::GetTempPath()) "getZiti-$PID.ps1"
Write-Host "Fetching getZiti helper from $GetZitiUrl ..."
Invoke-WebRequest -Uri $GetZitiUrl -OutFile $getZiti
Write-Host "Installing ziti $ZitiVersion ..."
. $getZiti -Version $ZitiVersion -NonInteractive
if (-not (Get-Command ziti -ErrorAction SilentlyContinue)) {
    throw "ziti CLI is not on PATH after running getZiti."
}

$quickstart = $null
try {
    # 2. Start the overlay in the background.
    Write-Host "Starting ziti edge quickstart on ${CtrlAddress}:${CtrlPort} ..."
    $quickstart = Start-Process -FilePath ziti -PassThru -NoNewWindow `
        -RedirectStandardOutput $logFile -RedirectStandardError "$logFile.err" `
        -ArgumentList @(
            'edge', 'quickstart',
            '--ctrl-address', $CtrlAddress,
            '--router-address', $CtrlAddress,
            '--username', $AdminUser,
            '--password', $AdminPassword
        )

    # 3. Wait for the controller to answer (and to actually authenticate admin/admin).
    $versionUrl = "https://${CtrlAddress}:${CtrlPort}/edge/client/v1/version"
    $authUrl = "https://${CtrlAddress}:${CtrlPort}/edge/management/v1/authenticate?method=password"
    $deadline = (Get-Date).AddSeconds(120)
    $ready = $false
    while ((Get-Date) -lt $deadline) {
        if ($quickstart.HasExited) {
            throw "quickstart exited early (code $($quickstart.ExitCode)). See $logFile.err"
        }
        try {
            Invoke-WebRequest -SkipCertificateCheck -TimeoutSec 5 -Uri $versionUrl | Out-Null
            $body = @{ username = $AdminUser; password = $AdminPassword } | ConvertTo-Json
            Invoke-WebRequest -SkipCertificateCheck -TimeoutSec 5 -Method Post -Uri $authUrl `
                -ContentType 'application/json' -Body $body | Out-Null
            $ready = $true
            break
        }
        catch {
            Start-Sleep -Seconds 2
        }
    }
    if (-not $ready) {
        throw "Controller at $versionUrl did not become ready (admin auth) within the timeout."
    }
    Write-Host "Controller is up and admin authenticates."

    # 4. Run the e2e tests against the fresh package.
    $env:ZITI_BASEURL = "${CtrlAddress}:${CtrlPort}"
    $env:ZITI_USERNAME = $AdminUser
    $env:ZITI_PASSWORD = $AdminPassword

    dotnet test $project `
        -p:RuntimeIdentifier=$Rid `
        -p:ZitiNativeVersion=$PackageVersion `
        -p:RestoreAdditionalProjectSources=$source `
        --filter TestCategory=e2e
    $testExit = $LASTEXITCODE
}
finally {
    # 5. Always tear the overlay down.
    if ($quickstart -and -not $quickstart.HasExited) {
        Write-Host "Stopping quickstart (PID $($quickstart.Id)) ..."
        if ($IsWindows) {
            taskkill /T /F /PID $quickstart.Id 2>$null | Out-Null
        }
        else {
            Stop-Process -Id $quickstart.Id -Force -ErrorAction SilentlyContinue
        }
    }
}

exit $testExit
