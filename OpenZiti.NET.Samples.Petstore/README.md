# OpenZiti.NET.Samples.Petstore

This project exists as a C# implementation of the Swagger Petstore application for demonstration purposes. It is
currently used the [petstore](../OpenZiti.NET.Samples/src/PetStore/README.md) sample. You can inspect the .csproj 
file to see how the spec is translated into c# compatible code. It basically runs a powershell command to pull 
down the spec from a predefined url, caching it and then processes the file. You can run that
msbuild target with something like `dotnet build /t:DownloadPetstoreV2Json` to refresh the spec.

