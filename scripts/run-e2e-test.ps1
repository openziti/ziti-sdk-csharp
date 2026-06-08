<#
.SYNOPSIS
    Run the native traffic e2e tests against a freshly packed OpenZiti.NET.native package, using a local
    `ziti edge quickstart` overlay.

.DESCRIPTION
    1. Requires the ziti CLI to already be on PATH (it does not install one). Install it however you like, e.g.
       https://get.openziti.io/quick/getZiti.ps1, or in CI via the openziti/ziti/setup-cli action.
    2. Starts `ziti edge quickstart` in the background, pinned to localhost, admin/admin.
    3. Waits for the controller to answer.
    4. Builds the e2e app (native/e2e-app, the ziti client+server) against the fresh package.
    5. Runs native/e2e/E2ETest.csproj (CallbackTrafficTest): provisions a service + identities, runs the app
       as the server, then as the client; the client must dial the server and get its greeting. Two separate
       processes, both through the fresh native lib.
    6. Always tears the overlay down.

    This proves the C SDK works in .NET via P/Invoke as a real client+server, on whatever OS runs the script,
    not just that the lib loads. Locally runnable end to end.

    Note on ziti version: both the v1.6.x and v2.0.x quickstarts bootstrap an admin/admin controller on
    localhost and are validated by this script. Whichever ziti is on PATH is used; CI runs a matrix over both
    lines (via setup-cli) before publishing.

    Dual mode:
      - Native publish gate: pass -PackageDir/-PackageVersion to test a freshly packed nupkg (local source).
      - Idiomatic SDK / PR gate: omit them to test against the already-PUBLISHED OpenZiti.NET.native version
        pinned in the csprojs (currently 1.16.0.245), restored from nuget.org. No native rebuild.

.PARAMETER PackageDir
    Optional. Folder containing a freshly packed OpenZiti.NET.native.<version>.nupkg (local nuget source).
    Omit to use the published package pinned in the csprojs.

.PARAMETER PackageVersion
    Optional. The nuget version of the freshly packed package (e.g. 1.16.0.213). Omit to use the csproj default.

.PARAMETER Rid
    The runtime identifier to test. The e2e is gated on win-x64. Defaults to win-x64.

.PARAMETER CtrlAddress
    Controller advertised address. Defaults to localhost so enrolled identities resolve locally.

.PARAMETER CtrlPort
    Controller port. Defaults to 1280.

.EXAMPLE
    ./run-e2e-test.ps1 -PackageDir ./artifacts -PackageVersion 1.16.0.213
#>
[CmdletBinding()]
param(
    [string] $PackageDir = '',
    [string] $PackageVersion = '',
    [string] $Rid = 'win-x64',
    [string] $CtrlAddress = 'localhost',
    [int]    $CtrlPort = 1280,
    [string] $AdminUser = 'admin',
    [string] $AdminPassword = 'admin'
)

$ErrorActionPreference = 'Stop'

$repoRoot = Split-Path $PSScriptRoot -Parent
$project = Join-Path $repoRoot 'native/e2e/E2ETest.csproj'
$logFile = Join-Path ([System.IO.Path]::GetTempPath()) "ziti-quickstart-$PID.log"

# Build the native-version props to pass to dotnet. With a PackageDir we add a local nuget source + pin the
# packed version (native publish gate); without one we leave them off and the csproj's published default
# (1.16.0.245) is restored from nuget.org (idiomatic / PR gate).
$nativeProps = @()
if ($PackageVersion) { $nativeProps += "-p:ZitiNativeVersion=$PackageVersion" }
if ($PackageDir) {
    $source = (Resolve-Path $PackageDir).Path
    $nativeProps += "-p:RestoreAdditionalProjectSources=$source"
    Write-Host "Testing freshly packed native $PackageVersion from $source"
} else {
    Write-Host "Testing the published native pinned in the csprojs (restored from nuget.org)"
}

# 1. Require the ziti CLI on PATH. This script does not install ziti: install it however you like (e.g.
#    https://get.openziti.io/quick/getZiti.ps1) or, in CI, via the openziti/ziti/setup-cli action.
$ziti = Get-Command ziti -ErrorAction SilentlyContinue
if (-not $ziti) {
    throw "ziti needs to be on your PATH. Install the ziti CLI (e.g. https://get.openziti.io/quick/getZiti.ps1) and re-run."
}
Write-Host "Using ziti from $($ziti.Source)"
& ziti version

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

    # 4. Build the e2e app (the ziti client+server) against the fresh package. It P/Invokes the fresh native
    #    lib; the test runs it as two processes (host + dial) via E2E_APP_DLL.
    $appOut = Join-Path ([System.IO.Path]::GetTempPath()) "e2e-app-$PID"
    $appProj = Join-Path $repoRoot 'native/e2e-app/e2e-app.csproj'
    Write-Host "Building e2e app $appProj for $Rid ..."
    dotnet publish $appProj -c Release -r $Rid --self-contained false @nativeProps -o $appOut
    if ($LASTEXITCODE -ne 0) { throw "failed to build e2e app $appProj" }
    $env:E2E_APP_DLL = Join-Path $appOut 'e2e-app.dll'

    # 5. Run the e2e test against the fresh package. It orchestrates the two programs above.
    $env:ZITI_BASEURL = "${CtrlAddress}:${CtrlPort}"
    $env:ZITI_USERNAME = $AdminUser
    $env:ZITI_PASSWORD = $AdminPassword

    # detailed console logger so the per-test step narration (OverlaySetup.Say / the tests) is shown, not just
    # the pass/fail summary.
    dotnet test $project `
        -p:RuntimeIdentifier=$Rid `
        @nativeProps `
        --filter TestCategory=e2e `
        --logger "console;verbosity=detailed"
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
