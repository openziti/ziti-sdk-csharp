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

### Building one RID the way CI does

CI builds each RID from the presets in `native/ZitiNativeApiForDotnetCore/CMakePresets.json`. `ZITI_SDK_C_BRANCH`
selects which ziti-sdk-c release to fetch and compile against, so this is how you check a C SDK bump before it
reaches the build matrix:

```
$env:ZITI_SDK_C_BRANCH = "1.18.4"
cmake --preset win64 -S native/ZitiNativeApiForDotnetCore
cmake --build native/ZitiNativeApiForDotnetCore/build --config Release
```

The first configure is slow: vcpkg compiles openssl, protobuf and the rest of the dependency set from source.

#### `MSB8020`: the v143 toolset is missing

The `common-windows` preset pins `"generator": "Visual Studio 17 2022"`, which requires the v143 toolset. On a
machine with only a newer Visual Studio installed, MSBuild fails during configure:

```
error MSB8020: The build tools for Visual Studio 2022 (Platform Toolset = 'v143') cannot be found.
```

vcpkg surfaces the same failure indirectly, as a dependency that "failed to build" while it was really asking
MSBuild for `VCTargetsPath`. Either install the VS2022 build tools, or configure without the preset and let Ninja
use whichever toolset you do have. This expands `win64` by hand, generator swapped, with `CMAKE_BUILD_TYPE` set
because Ninja is single-config:

```
$env:ZITI_SDK_C_BRANCH = "1.18.4"
$env:VCPKG_ROOT = "C:\Program Files\Microsoft Visual Studio\18\Community\VC\vcpkg"

cmake -S native/ZitiNativeApiForDotnetCore -B native/ZitiNativeApiForDotnetCore/build `
  -G Ninja `
  -D CMAKE_BUILD_TYPE=Release `
  -D CMAKE_TOOLCHAIN_FILE="$env:VCPKG_ROOT\scripts\buildsystems\vcpkg.cmake" `
  -D VCPKG_INSTALLED_DIR=D:/vi `
  -D VCPKG_TARGET_TRIPLET=x64-windows-static-md `
  -D VCPKG_HOST_TRIPLET=x64-windows `
  -D CMAKE_C_FLAGS="/utf-8 /W4 /permissive- /volatile:iso /Zc:preprocessor" `
  -D CMAKE_CXX_FLAGS="/utf-8 /W4 /permissive- /volatile:iso /Zc:preprocessor /Zc:__cplusplus /Zc:externConstexpr /Zc:throwingNew /EHsc"

cmake --build native/ZitiNativeApiForDotnetCore/build
```

Switching generators needs an empty build directory; delete `native/ZitiNativeApiForDotnetCore/build` first or
CMake refuses to reuse the cache.

#### `LNK1104: cannot open file ... intermediate.manifest`

`VCPKG_INSTALLED_DIR=D:/vi` above is not cosmetic. By default vcpkg builds its dependencies under
`<build>/vcpkg_installed/vcpkg/blds/<port>/<triplet>/CMakeFiles/CMakeScratch/TryCompile-xxxxxx/`, and under a
deep checkout path that blows the 250-character limit on an object file path. CMake warns first, then the link
step fails because it cannot create its manifest:

```
The object file directory ... has 242 characters. The maximum full path to an object file is 250 characters
LINK : fatal error LNK1104: cannot open file 'CMakeFiles\cmTC_89549.dir/intermediate.manifest'
```

Pointing `VCPKG_INSTALLED_DIR` at a short root fixes it. The `LongPathsEnabled` registry setting does not:
`link.exe` and several other MSVC tools are not long-path aware. A shallow checkout path leaves enough headroom to
avoid this entirely.

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
## build the native/ZitiNativeApiForDotnetCore project
cd native/ZitiNativeApiForDotnetCore
msvc-build.bat

cd ..
copy /y native/ZitiNativeApiForDotnetCore\build-win\x64\library\Release\ziti4dotnet.dll runtimes\win-x64\native
copy /y native/ZitiNativeApiForDotnetCore\build-win\x86\library\Release\ziti4dotnet.dll runtimes\win-x86\native

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
cd native/ZitiNativeApiForDotnetCore
cmake -E make_directory build/macos
cmake -S . -B build/macos
cmake --build build/macos --config Release

#move/copy the resultant libziti4dotnet.dylib to 
#  runtimes\osx-x64\native\libziti4dotnet.dylib
```

### Linux
```
cd native/ZitiNativeApiForDotnetCore
cmake -E make_directory build/linux
cmake -S . -B build/linux
cmake --build build/linux --config Release

#move/copy the resultant libziti4dotnet.so to
#  runtimes\linux-x64\native\libziti4dotnet.so
```