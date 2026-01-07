$ErrorActionPreference = 'Stop'

Write-Host "=== OpenZiti native fetch starting ==="

# Set root to the directory of the script
$root = $PSScriptRoot

# Load version from version.props
$versionProps = Join-Path $root 'version.props'
if (-not (Test-Path $versionProps)) {
    Write-Error "version.props not found at $versionProps. Ensure this file exists in the root directory."
    exit 1
}

[xml]$xml = Get-Content $versionProps
$version = $xml.Project.PropertyGroup.Version

if (-not $version) {
    Write-Error "Version not found in version.props. Ensure <Version> is defined in the file."
    exit 1
}

Write-Host "Target ziti-sdk-c version: $version"

# Define directories
$incoming = Join-Path $root 'incoming'
$destRoot = Join-Path $root "third_party\ziti-sdk-c\$version"

Write-Host "Creating necessary directories..."
New-Item -ItemType Directory -Force -Path $incoming | Out-Null
New-Item -ItemType Directory -Force -Path $destRoot | Out-Null

# List of all asset URLs
$urls = @(
    "ziti-sdk-Darwin-arm64.zip",
    "ziti-sdk-Darwin-x86_64.zip",
    "ziti-sdk-Linux-arm.zip",
    "ziti-sdk-Linux-arm64.zip",
    "ziti-sdk-Linux-x86_64.zip",
    "ziti-sdk-Windows-x86.zip",
    "ziti-sdk-Windows-AMD64.zip",
    "ziti-sdk-Windows-ARM64.zip"
)

# Base URL for all assets
$baseUrl = "https://github.com/openziti/ziti-sdk-c/releases/download/$version/"

# Function to handle downloading and extraction
function DownloadAndExtract {
    param (
        [Parameter(Mandatory=$true)] [string]$fileName,
        [Parameter(Mandatory=$true)] [string]$incomingDir,
        [Parameter(Mandatory=$true)] [string]$destDir
    )

    $url = $baseUrl + $fileName
    $zipPath = Join-Path $incomingDir $fileName
    $nativePath = Join-Path $destDir "runtimes\$($fileName.Split('-')[2..3] -join '-')\native"

    # Create runtime and native directories before extraction
    $runtimePath = Join-Path $destDir "runtimes\$($fileName.Split('-')[2..3] -join '-')"
    if (-not (Test-Path $runtimePath)) {
        Write-Host "Creating runtime directory: $runtimePath"
        New-Item -ItemType Directory -Force -Path $runtimePath | Out-Null
    }
    if (-not (Test-Path $nativePath)) {
        Write-Host "Creating native directory: $nativePath"
        New-Item -ItemType Directory -Force -Path $nativePath | Out-Null
    }

    # Download zip if not already downloaded
    if (-not (Test-Path $zipPath)) {
        Write-Host "Downloading file: $fileName from $url..."
        try {
            Invoke-WebRequest -Uri $url -OutFile $zipPath
            Write-Host "  Successfully downloaded $fileName."
        } catch {
            Write-Error "Failed to download $fileName. Check URL and network connection."
            exit 1
        }
    } else {
        Write-Host "File already downloaded: $fileName"
    }

    # Extract zip if not already extracted
    if (-not (Test-Path $nativePath)) {
        Write-Host "Extracting file: $fileName to $destDir..."
        try {
            Expand-Archive -Force $zipPath -DestinationPath $destDir
            Write-Host "  Successfully extracted $fileName."
        } catch {
            Write-Error "Failed to extract $fileName. Check the archive and permissions."
            exit 1
        }
    } else {
        Write-Host "File already extracted: $fileName"
    }
}

# Download and extract all assets
foreach ($file in $urls) {
    DownloadAndExtract -fileName $file -incomingDir $incoming -destDir $destRoot
}

Write-Host "=== OpenZiti native fetch complete ==="
