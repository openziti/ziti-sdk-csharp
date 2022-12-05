# Building

## Local Development - Managed Code

The managed code portion of this project is what other .NET projects will consume.
It provides the idiomatic representations of the underlying C SDK functions, structs
and patterns.

The managed package requires a native package. The native package can either be built
from source or can be installed from nuget.org. Instructions for how to build from source
are below.

The project contains a solution called Ziti.NuGet.sln. This solution file will have the
project inside which is the actual library of .cs files comprising the idiomatic .NET 
classes intended to be used by downstream projects. 

If you decide to/need to build the native NuGet package, you will need to update the NuGet
package in the project.

```
dotnet nuget add source --name "local-nuget-packages-linux" "$PWD/local-nuget-packages"
##NOT: nuget sources Add -Name "local-nuget-packages" -Source "$PWD/local-nuget-packages/"
Package source with Name: local-nuget-packages added successfully.
```

## Local Development - Native Package

The native package that wraps the C SDK is called OpenZiti.NET.native and is published
to nuget.org via GitHub actions. When updating the C SDK it is necessary to be able
to deploy a NuGet package locally, for the managed code to use. Follow these steps to
build the native NuGet package and publish to a folder named local-nuget-packages.

### Requirements
* cmake
* gcc/mingw/msvc

If you want to make changes to the solution, here are the basic steps you 
need to perform. For every platform you don't have access to you will need
to make a stub/dummy file in order for the nuspec packaging to complete.

### Stubs

To generate a nuget package with stubs, follow these steps:
```
mkdir runtimes\win-x86\native
mkdir runtimes\win-x64\native
mkdir runtimes\linux-x64\native
mkdir runtimes\osx-x64\native

echo "dummy" > runtimes\win-x64\native\ziti4dotnet.dll
echo "dummy" > runtimes\win-x86\native\ziti4dotnet.dll
echo "dummy" > runtimes\linux-x64\native\libziti4dotnet.so
echo "dummy" > runtimes\osx-x64\native\libziti4dotnet.dylib
```

### Windows Only
```
## build the ZitiNativeApiForDotnetCore project
cd ZitiNativeApiForDotnetCore
msvc-build.bat

cd ..
copy /y ZitiNativeApiForDotnetCore\build-win\x64\library\Release\ziti4dotnet.dll runtimes\win-x64\native
copy /y ZitiNativeApiForDotnetCore\build-win\x86\library\Release\ziti4dotnet.dll runtimes\win-x86\native

mkdir local-packages

set yearstr=%date:~10,4%
set daystr=%date:~7,2%
set monthstr=%date:~4,2%

set timenow=%TIME: =0%

SET HOUR=%TIME:~0,2%
IF "%HOUR:~0,1%" == " " SET HOUR=0%HOUR:~1,1%

set minstr=%timenow:~3,2%
set datestr=%date:~10,4%-%date:~7,2%-%date:~4,2%
echo %datestr% %yearstr% %monthstr% %daystr% %HOUR% %minstr%
nuget pack -version %yearstr%.%monthstr%.%daystr%.%HOUR%%minstr% -OutputDirectory local-nuget-packages native-package.nuspec
```

### MacOS
```
cd ZitiNativeApiForDotnetCore
cmake -E make_directory build/macos
cmake -S . -B build/macos
cmake --build build/macos --config Release

#move/copy the resultant libziti4dotnet.dylib to 
#  runtimes\osx-x64\native\libziti4dotnet.dylib
```

### Linux
```
cd ZitiNativeApiForDotnetCore
cmake -E make_directory build/linux
cmake -S . -B build/linux
cmake --build build/linux --config Release

#move/copy the resultant libziti4dotnet.so to
#  runtimes\linux-x64\native\libziti4dotnet.so
```