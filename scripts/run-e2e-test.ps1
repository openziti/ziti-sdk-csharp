<#
.SYNOPSIS
    Run the native traffic e2e tests against a freshly packed OpenZiti.NET.native package, using a local
    `ziti edge quickstart` overlay.

.DESCRIPTION
    1. Requires the ziti CLI to already be on PATH (it does not install one). Install it however you like, e.g.
       https://get.openziti.io/quick/getZiti.ps1, or in CI via the openziti/ziti/setup-cli action.
    2. Starts `ziti edge quickstart` in the background, pinned to localhost, admin/admin.
    3. Waits for the controller to answer and the quickstart's edge router to come online. A quickstart that
       does not come up is retried once.
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

.PARAMETER RouterPort
    Port of the quickstart's integrated edge router. Defaults to 3022.

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
    [int]    $RouterPort = 3022,
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

function Stop-Quickstart([System.Diagnostics.Process] $Process) {
    if ($Process -and -not $Process.HasExited) {
        Write-Host "Stopping quickstart (PID $($Process.Id)) ..."
        if ($IsWindows) {
            taskkill /T /F /PID $Process.Id 2>$null | Out-Null
        }
        else {
            Stop-Process -Id $Process.Id -Force -ErrorAction SilentlyContinue
        }
        $Process.WaitForExit(15000) | Out-Null
    }
}

function Show-QuickstartLog([string] $Path) {
    foreach ($p in @($Path, "$Path.err")) {
        if (Test-Path -LiteralPath $p) {
            Write-Host "----- last 40 lines of $p -----"
            Get-Content -LiteralPath $p -Tail 40 | ForEach-Object { Write-Host $_ }
        }
    }
}

$quickstart = $null
try {
    # 2-3. Start the overlay in the background and wait for the controller to answer and authenticate
    #      admin/admin, and for the edge router to come online. A quickstart that does not come up is retried
    #      once with a fresh process.
    $versionUrl = "https://${CtrlAddress}:${CtrlPort}/edge/client/v1/version"
    $authUrl = "https://${CtrlAddress}:${CtrlPort}/edge/management/v1/authenticate?method=password"
    $routersUrl = "https://${CtrlAddress}:${CtrlPort}/edge/management/v1/edge-routers"
    $maxAttempts = 2
    $ready = $false
    for ($attempt = 1; $attempt -le $maxAttempts -and -not $ready; $attempt++) {
        Write-Host "Starting ziti edge quickstart on ${CtrlAddress}:${CtrlPort} (attempt $attempt/$maxAttempts) ..."
        $quickstart = Start-Process -FilePath ziti -PassThru -NoNewWindow `
            -RedirectStandardOutput $logFile -RedirectStandardError "$logFile.err" `
            -ArgumentList @(
                'edge', 'quickstart',
                '--ctrl-address', $CtrlAddress,
                '--router-address', $CtrlAddress,
                '--ctrl-port', $CtrlPort,
                '--router-port', $RouterPort,
                '--username', $AdminUser,
                '--password', $AdminPassword
            )

        $lastError = ''
        $deadline = (Get-Date).AddSeconds(150)
        while ((Get-Date) -lt $deadline) {
            if ($quickstart.HasExited) {
                Write-Warning "quickstart exited early (code $($quickstart.ExitCode))."
                break
            }
            try {
                Invoke-WebRequest -SkipCertificateCheck -TimeoutSec 5 -Uri $versionUrl | Out-Null
                $body = @{ username = $AdminUser; password = $AdminPassword } | ConvertTo-Json
                $auth = Invoke-RestMethod -SkipCertificateCheck -TimeoutSec 5 -Method Post -Uri $authUrl `
                    -ContentType 'application/json' -Body $body
                # The tests need the quickstart's router online, not just the controller.
                $routers = Invoke-RestMethod -SkipCertificateCheck -TimeoutSec 5 -Uri $routersUrl `
                    -Headers @{ 'zt-session' = $auth.data.token }
                if (-not ($routers.data | Where-Object { $_.isOnline })) { throw "no edge router online yet" }
                $ready = $true
                break
            }
            catch {
                $lastError = $_.Exception.Message.Split("`n")[0]
                Start-Sleep -Seconds 2
            }
        }
        if (-not $ready) {
            Write-Warning "Controller at $versionUrl not ready on attempt $attempt. Last error: $lastError"
            Show-QuickstartLog $logFile
            Stop-Quickstart $quickstart
        }
    }
    if (-not $ready) {
        throw "Controller at $versionUrl did not become ready (admin auth, router online) after $maxAttempts attempts."
    }
    Write-Host "Controller is up, admin authenticates, and an edge router is online."

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
    # 6. Always tear the overlay down.
    Stop-Quickstart $quickstart
}

exit $testExit
