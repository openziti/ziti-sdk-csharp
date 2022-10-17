# Native Helper for dotnet

This is a [cmake](https://cmake.org/) project which does numerous activities, culminating in a native nuget package
which is consumable by windows x86/x64 dotnet projects as well as MacOS x64 and linux x64 architectures. We currently
**do not** support every RID. If your favorite dotnet arch is not covered by the ones mentioned, we'd love help it
getting it working for your specific environment.

## Overview of the Subproject

This project will build a native library for use from C#. There is functionality which C# doesn't seem to support, or
we haven't discovered how to support it yet. Generally this is things like iterating a pointer or var args usage.

The project depends on the [ziti-sdk-c](https://github.com/openziti/ziti-sdk-c). The precise version is controlled two
ways. The first way is by setting an environment variable named ZITI_SDK_C_BRANCH. The cmake file will look for this
env var and use it, if it's supplied. The [github action](../.github/workflows/native-nuget-publish.yml) will set this
value as of May-14-2022. This is done so the action knows what version of the sdk is built, and the version of the native
package will be ZITI_SDK_C_VERSION.BUILD_NUMBER. Such as 0.28.1.123.  As of Oct-17-2022 you need to also update the 
github action at `.github\workflows\native-nuget-publish.yml`

## Upgrading the ZITI SDK C library version

### ziti.def

If you explore the CMakeLists.txt file you will see there is a ziti.def file. This file is **REQUIRED** for Windows library
builds. It is also imperative that it is update this file. A .bat file named defgen.bat has been provided which does this.
This file allows the exported functions from ziti.dll to be re-exported in ziti4dotnet.

Example:
```
cd ZitiNativeApiForDotnetCore
msvc-build.bat
defgen 64 %BUILDFOLDER%\x64\_deps\ziti-sdk-c-build\library\Release\ziti.dll
```

You'll probably have to remove the three extraneous files: ziti-exports.txt, ziti.dll, ziti.exp. defgen leaves these files
behind in case you need to do deubgging on the process. 

#### "No function found"

If you are getting errors indicating a function is not found, it's likely due to the ziti.def not being updated properly or
it's not included in the cmake file. 

### ZitiStatus.cs

This project is also responsible for generating a small amount of C# which is used in the Ziti.NT.Standard project. When updating
the C SDK version, please remember to recreate the ZitiStatus.cs file.

Here's how you can regenerate the file. You can either clone/fetch the version of the ZITI C SDK yo are targetting - or just
generate this project's meta data (which I think is easier).
```text
SET TARGETDIR=_TEMP_
cmake -E make_directory %TARGETDIR%
cmake -S . -B %TARGETDIR% 
cl /C /EP /I %TARGETDIR%/_deps/ziti-sdk-c-src/includes /c library/sharp-errors.c > ../Ziti.NET.Standard/src/OpenZiti/ZitiStatus.cs
- OR if using gcc not developer command prompt -
gcc -nostdinc -E -CC -P -I%TARGETDIR%/_deps/ziti-sdk-c-src/includes library/sharp-errors.c > ../Ziti.NET.Standard/src/OpenZiti/ZitiStatus.cs
```
