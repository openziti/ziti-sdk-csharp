<#
.SYNOPSIS
    Regenerate native/ZitiNativeApiForDotnetCore/library/ziti.def from a C SDK release's actual exports.

.DESCRIPTION
    The Windows native build links ziti4dotnet.dll against an exports list (ziti.def). That list drifts every
    time the C SDK changes (symbols added/removed), and a stale entry breaks the Windows link
    (e.g. LNK2001 unresolved external). This script regenerates ziti.def for a given C SDK version so it is
    never hand-maintained or stale.

    Mechanics: it drives the existing CMake ZITI_RUN_DEFGEN path, which downloads that version's
    ziti-sdk-Windows-AMD64.zip and runs defgen.bat (dumpbin /exports -> ziti.def), then return()s before the
    real build. defgen.bat needs dumpbin/lib, so if they are not already on PATH this sets up the VS x64 dev
    environment for the current process only.

    Locally runnable: run it from a clone with a VS install. CI invokes the exact same script.

    Only meaningful on Windows (ziti.def is a Windows linker artifact and defgen.bat is a Windows batch file).

.PARAMETER Version
    The C SDK release tag whose exports define the .def (e.g. 1.16.0). Must be a published release (the zip
    asset has to exist). Defaults to the ZITI_SDK_C_BRANCH env var.

.PARAMETER BuildDir
    Throwaway cmake build dir for the defgen-only configure pass. Defaults to a temp dir.

.EXAMPLE
    ./regenerate-ziti-def.ps1 -Version 1.16.0
#>
[CmdletBinding()]
param(
    [string] $Version = $env:ZITI_SDK_C_BRANCH,
    [string] $BuildDir = (Join-Path ([System.IO.Path]::GetTempPath()) "ziti-defgen")
)

$ErrorActionPreference = 'Stop'

if ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Version is required (pass -Version or set ZITI_SDK_C_BRANCH)."
}

$repoRoot = Split-Path $PSScriptRoot -Parent
$base = Join-Path $repoRoot 'native/ZitiNativeApiForDotnetCore'

# CMake's ZITI_RUN_DEFGEN path reads the version from this env var.
$env:ZITI_SDK_C_BRANCH = $Version

# defgen.bat shells out to dumpbin/lib. If they are not on PATH (e.g. not in a VS dev prompt), set up the
# VS x64 dev environment for THIS process only so it does not leak to other build steps.
if (-not (Get-Command dumpbin -ErrorAction SilentlyContinue)) {
    $vswhere = Join-Path ${env:ProgramFiles(x86)} 'Microsoft Visual Studio\Installer\vswhere.exe'
    if (-not (Test-Path $vswhere)) {
        throw "Could not find vswhere at $vswhere, and dumpbin is not on PATH. Run from a VS developer prompt."
    }
    $vsPath = (& $vswhere -latest -property installationPath).Trim()
    Import-Module (Join-Path $vsPath 'Common7\Tools\Microsoft.VisualStudio.DevShell.dll')
    Enter-VsDevShell -VsInstallPath $vsPath -SkipAutomaticLocation -DevCmdArguments '-arch=x64' | Out-Null
}

Write-Host "Regenerating ziti.def for C SDK $Version ..."
cmake -S $base -B $BuildDir -DZITI_RUN_DEFGEN=yes
if ($LASTEXITCODE -ne 0) {
    throw "defgen cmake pass failed with exit code $LASTEXITCODE"
}
Write-Host "Regenerated $base/library/ziti.def"
