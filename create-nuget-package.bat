@echo off

dotnet build ziti.nuget.sln --configuration Release /p:Platform=x86
dotnet build ziti.nuget.sln --configuration Release /p:Platform=x64

REM :point-down: does not create a nuget package which is compatible with "AnyCPU" somehow?
REM dotnet build ziti.nuget.sln --configuration Release

mkdir Ziti.NET.Standard\bin\Release\netstandard2.1\

REM Thanks to https://msicc.net/how-to-create-a-multi-architecture-nuget-package-from-a-uwp-class-library/
REM for the corflags tip
echo "copying x86 dll and removing removing the 32Bit-flag to be AnyCPU compatible"
cp Ziti.NET.Standard\bin\x86\Release\netstandard2.1\Ziti.NET.Standard.dll Ziti.NET.Standard\bin\Release\netstandard2.1\Ziti.NET.Standard.dll
cp Ziti.NET.Standard\bin\x86\Release\netstandard2.1\Ziti.NET.Standard.pdb Ziti.NET.Standard\bin\Release\netstandard2.1\Ziti.NET.Standard.pdb
corflags /32bitreq- Ziti.NET.Standard\bin\Release\netstandard2.1\Ziti.NET.Standard.dll

dotnet pack Ziti.NuGet.sln
