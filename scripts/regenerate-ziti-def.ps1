<#
.SYNOPSIS
    Regenerate native/ZitiNativeApiForDotnetCore/library/ziti.def from the C SDK's actual exports.

.DESCRIPTION
    The Windows native build links ziti4dotnet.dll against an exports list (ziti.def). That list drifts every
    time the C SDK changes (symbols added/removed), and a stale entry breaks the Windows link
    (e.g. LNK2001 unresolved external). This script regenerates ziti.def so it is never hand-maintained or stale.

    Two sources:
      - -Version: a C SDK release. Drives the CMake ZITI_RUN_DEFGEN path, which downloads that release's
        ziti-sdk-Windows-AMD64.zip and runs defgen.bat on its ziti.dll, then return()s before the real build.
      - -Dll: a ziti.dll already on disk, such as one built from a C SDK branch. Runs defgen.bat on it directly.

    defgen.bat (dumpbin /exports -> ziti.def) needs dumpbin/lib, so if they are not already on PATH this sets up
    the VS x64 dev environment for the current process only.

    Locally runnable: run it from a clone with a VS install. CI invokes the exact same script.

    Only meaningful on Windows (ziti.def is a Windows linker artifact and defgen.bat is a Windows batch file).

.PARAMETER Version
    The C SDK release tag whose exports define the .def (e.g. 1.16.0). Must be a published release (the zip
    asset has to exist). Defaults to the ZITI_SDK_C_BRANCH env var. Ignored when -Dll is given.

.PARAMETER Dll
    A C SDK ziti.dll whose exports define the .def.

.PARAMETER BuildDir
    Throwaway cmake build dir for the defgen-only configure pass. Defaults to a temp dir.

.EXAMPLE
    ./regenerate-ziti-def.ps1 -Version 1.16.0

.EXAMPLE
    ./regenerate-ziti-def.ps1 -Dll build/win64/_deps/ziti-sdk-c-build/library/Release/ziti.dll
#>
[CmdletBinding()]
param(
    [string] $Version = $env:ZITI_SDK_C_BRANCH,
    [string] $Dll = '',
    [string] $BuildDir = (Join-Path ([System.IO.Path]::GetTempPath()) "ziti-defgen")
)

$ErrorActionPreference = 'Stop'

if ($Dll) {
    if (-not (Test-Path $Dll)) { throw "ziti.dll not found at $Dll" }
    $Dll = (Resolve-Path $Dll).Path
} elseif ([string]::IsNullOrWhiteSpace($Version)) {
    throw "Version is required (pass -Version, -Dll, or set ZITI_SDK_C_BRANCH)."
}

$repoRoot = Split-Path $PSScriptRoot -Parent
$base = Join-Path $repoRoot 'native/ZitiNativeApiForDotnetCore'
$defFile = Join-Path $base 'library/ziti.def'

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

if ($Dll) {
    Write-Host "Regenerating ziti.def from $Dll ..."
    # defgen.bat writes library\ziti.def relative to the working directory.
    Push-Location $base
    try {
        & cmd /c (Join-Path $base 'defgen.bat') 64 $Dll
        if ($LASTEXITCODE -ne 0) { throw "defgen.bat failed with exit code $LASTEXITCODE" }
    } finally {
        Pop-Location
    }
} else {
    # CMake's ZITI_RUN_DEFGEN path reads the version from this env var.
    $env:ZITI_SDK_C_BRANCH = $Version
    Write-Host "Regenerating ziti.def for C SDK $Version ..."
    cmake -S $base -B $BuildDir -DZITI_RUN_DEFGEN=yes
    if ($LASTEXITCODE -ne 0) {
        throw "defgen cmake pass failed with exit code $LASTEXITCODE"
    }
}

# defgen.bat writes the header lines even when dumpbin finds nothing, so an empty export list links but
# exports nothing.
$exports = @(Get-Content $defFile | Select-Object -Skip 2 | Where-Object { $_.Trim() })
if ($exports.Count -eq 0) { throw "$defFile has no exports" }
Write-Host "Regenerated $defFile ($($exports.Count) exports)"
