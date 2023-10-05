# OpenZiti.Management

This project exists as a C# implementation of the OpenZiti management API spec. Open the .csproj to see how it works.
It basically runs a powershell command to pull down the spec from a predefined url, caching it. You can run that
msbuild target with something like `dotnet build /t:DownloadMgmtYaml` to refresh the spec.

