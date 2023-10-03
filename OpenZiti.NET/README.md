dotnet tool install -g Microsoft.dotnet-openapi


dotnet openapi remove --updateProject OpenZiti.Management.csproj mgmt.yml

dotnet openapi add file .\mgmt.yml
dotnet openapi add url https://get.openziti.io/spec/management.yml

dotnet openapi add url https://get.openziti.io/spec/management.yml --updateProject OpenZiti.Management.csproj --code-generator NSwagCSharp --output-file management.yml


good luck with refreshing the service. just generate it, open it and edit it.... sigh

