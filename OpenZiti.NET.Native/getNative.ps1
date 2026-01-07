$ErrorActionPreference = "Stop"

Write-Host "== OpenZiti.NET.Native :: Fetch native binaries =="

$root = (Resolve-Path (Join-Path $PSScriptRoot ".")).Path
$verProps = Join-Path $root "version.props"
if (-not (Test-Path -LiteralPath $verProps)) {
    throw "version.props not found at $verProps"
}

[xml]$vp = Get-Content -LiteralPath $verProps
$ver = $vp.Project.PropertyGroup.Version
if ([string]::IsNullOrWhiteSpace($ver)) {
    throw "Version not found in version.props"
}

Write-Host "Version: $ver"

$incoming = Join-Path $root "incoming/ziti-sdk-c/$ver"
$unz      = Join-Path $incoming "unz"
$stage    = Join-Path $root "third_party/ziti-sdk-c/$ver/runtimes"

New-Item -ItemType Directory -Force -Path $incoming 2>&1 | Out-Null
New-Item -ItemType Directory -Force -Path $stage    2>&1 | Out-Null

$base = "https://github.com/openziti/ziti-sdk-c/releases/download/$ver"

$assets = @(
  @{ rid="win-x86";   file="ziti-sdk-Windows-x86.zip" }
  @{ rid="win-x64";   file="ziti-sdk-Windows-AMD64.zip" }
  @{ rid="win-arm64"; file="ziti-sdk-Windows-ARM64.zip" }

  @{ rid="linux-x64";   file="ziti-sdk-Linux-x86_64.zip" }
  @{ rid="linux-arm";   file="ziti-sdk-Linux-arm.zip" }
  @{ rid="linux-arm64"; file="ziti-sdk-Linux-arm64.zip" }

  @{ rid="osx-x64";   file="ziti-sdk-Darwin-x86_64.zip" }
  @{ rid="osx-arm64"; file="ziti-sdk-Darwin-arm64.zip" }
)

foreach ($a in $assets) {
    $zip = Join-Path $incoming $a.file
    if (Test-Path -LiteralPath $zip) {
        Write-Host "SKIP download: $($a.file)"
        continue
    }

    $url = "$base/$($a.file)"
    Write-Host "DOWNLOAD $url"
    Invoke-WebRequest -Uri $url -OutFile $zip 2>&1 | Out-Null
}

if (Test-Path -LiteralPath $unz) {
    Write-Host "Cleaning temp unzip dir"
    Remove-Item -Recurse -Force -LiteralPath $unz 2>&1 | Out-Null
}
New-Item -ItemType Directory -Force -Path $unz 2>&1 | Out-Null

function Expand-OneZip([string]$ZipPath, [string]$OutDir) {
    Write-Host "Unzip $(Split-Path $ZipPath -Leaf) -> $OutDir"
    if (Test-Path -LiteralPath $OutDir) {
        Remove-Item -Recurse -Force -LiteralPath $OutDir 2>&1 | Out-Null
    }
    New-Item -ItemType Directory -Force -Path $OutDir 2>&1 | Out-Null
    Expand-Archive -LiteralPath $ZipPath -DestinationPath $OutDir -Force 2>&1 | Out-Null
}

function Stage-Rid([string]$Rid, [string]$SrcDir) {
    $dst = Join-Path $stage "$Rid/native"
    Write-Host "Stage $Rid -> $dst"

    if (Test-Path -LiteralPath $dst) {
        Remove-Item -Recurse -Force -LiteralPath $dst 2>&1 | Out-Null
    }
    New-Item -ItemType Directory -Force -Path $dst 2>&1 | Out-Null

    $files = Get-ChildItem -Recurse -File -LiteralPath $SrcDir | Where-Object {
         $_.Name.EndsWith(".dll",   [System.StringComparison]::OrdinalIgnoreCase) `
      -or $_.Name.EndsWith(".so",    [System.StringComparison]::OrdinalIgnoreCase) `
      -or $_.Name.EndsWith(".dylib", [System.StringComparison]::OrdinalIgnoreCase) `
      -or $_.Name.EndsWith(".pdb",   [System.StringComparison]::OrdinalIgnoreCase)
    }

    foreach ($fi in $files) {
        Copy-Item -Force -LiteralPath $fi.FullName -Destination (Join-Path $dst $fi.Name) 2>&1 | Out-Null
    }

    Write-Host "  -> $(($files | Measure-Object).Count) files"
}

foreach ($a in $assets) {
    $zip = Join-Path $incoming $a.file
    $out = Join-Path $unz $a.rid
    Expand-OneZip $zip $out
    Stage-Rid $a.rid $out
}

Write-Host "DONE: native binaries staged under:"
Write-Host "  $stage"
