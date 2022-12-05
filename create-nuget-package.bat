@echo off

dotnet build ziti.nuget.sln --configuration Release /p:Platform=x86
dotnet build ziti.nuget.sln --configuration Release /p:Platform=x64

mkdir OpenZiti.NET\bin\Release\net6.0\

REM Thanks to https://msicc.net/how-to-create-a-multi-architecture-nuget-package-from-a-uwp-class-library/
REM for the corflags tip
echo "copying x86 dll and removing removing the 32Bit-flag to be AnyCPU compatible"
cp OpenZiti.NET\bin\x86\Release\net6.0\OpenZiti.NET.dll OpenZiti.NET\bin\Release\net6.0\OpenZiti.NET.dll
cp OpenZiti.NET\bin\x86\Release\net6.0\OpenZiti.NET.pdb OpenZiti.NET\bin\Release\net6.0\OpenZiti.NET.pdb
corflags /32bitreq- OpenZiti.NET\bin\Release\net6.0\OpenZiti.NET.dll

dotnet pack Ziti.NuGet.sln --configuration Release --output local-nuget-packages