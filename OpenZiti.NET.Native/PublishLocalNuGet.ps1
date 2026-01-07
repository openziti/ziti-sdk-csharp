# Define paths
$root = $PSScriptRoot
Write-Host "Script Root: $PSScriptRoot"

$localNuGetPath = Join-Path $root "local-nuget"
$projectFile = Join-Path $root "src\OpenZiti.NET.Native\OpenZiti.NET.Native.csproj"

# Load version from version.props
$version = (Get-Content "$root\version.props" | Select-String -Pattern "<Version>(.*?)</Version>" | ForEach-Object { $_.Matches.Groups[1].Value })

# Update package path with the version
$packagePath = Join-Path $root "src\OpenZiti.NET.Native\bin\Release\OpenZiti.NET.Native.$version.nupkg"

# Check if project file exists
if (-not (Test-Path $projectFile)) {
    Write-Host "Project file does not exist at $projectFile"
    exit
}

# Create the local NuGet folder if it doesn't exist
if (-not (Test-Path $localNuGetPath)) {
    Write-Host "Creating local NuGet folder at $localNuGetPath"
    New-Item -ItemType Directory -Path $localNuGetPath
}

# Build the project
Write-Host "Building project..."
dotnet build $projectFile -c Release

# Pack the project into a .nupkg
Write-Host "Packing the project into a NuGet package..."
dotnet pack $projectFile -c Release -v detailed

# Check if package exists
if (-not (Test-Path $packagePath)) {
    Write-Host "NuGet package not found at $packagePath"
    exit
}

# Push the NuGet package to the local folder
Write-Host "Pushing NuGet package to $localNuGetPath"
dotnet nuget push $packagePath --source $localNuGetPath

Write-Host "Done!"
