# Native NuGet Package

As mentioned in [the base README](../README.md), this project is responsible for creating a nuget package which exposes 
the [ziti-sdk-c](https://github.com/openziti/ziti-sdk-c) functions in an easy-to-consume and cross-architecture way.

This project does nothing but build the C SDK libraries on the various platforms using [cmake](https://cmake.org/)
and then it produces a [NuGet package](https://www.nuget.org/packages/OpenZiti.NET.native) which is expected to be included as dependency in the main,
[idiomatic dotnet (C#) SDK](../OpenZiti.NET) in this project.

## Publishing the NuGet Package

Generally, this project is only built from GitHub via the [native-nuget-publish.yml](../.github/actions/native-nuget-publish.yml) action.

The action will only push to NuGet when it's run from the organization/project of `openziti/ziti-sdk-csharp` and does
not verify the branch is main.  It's designed to be runnable from any branch at this time. 

This project also layers on helper functions as needed. Often these additional functions will be to do things
which dotnet doesn't seem to support, or we haven't discovered how to support it yet. Generally things like 
iterating a pointer or var args usage.

This is a project based on [cmake](https://cmake.org/) which will compile the given C SDK into the native 
nuget package. It also now requires [vcpkg](https://github.com/microsoft/vcpkg) in order to build. We make use of CMakePresets.json so if you're
confused where the preset values come from, look in there.

The package should be consumable on Windows (32 and 64 bit), MacOS x86_64 and linux x86_64 architectures. It
has ARM/ARM64 binaries too but those are untested. If you can test those, please let us know if it works and if you
read this doc, maybe add a PR to update this section of doc. 

The project **does not** support every arch, ever RID. If your favorite dotnet arch is not covered by the ones 
mentioned, we'd love help it getting it working for your specific environment.

## Building
If you're considering building this project, you are almost certainly trying to develop the actual dotnet SDK
and you're trying to update the native NuGet package, or you're just wondering how all this project comes together.

After installing cmake, gcc/msvc, vcpkg and any other dependencies that are needed, you'll first need to build this
project. Since it uses cmake, and assuming your shell is located in the same directory as this readme, you 
should be able to simply issue something like:

```
SET TARGETDIR=%CD%\build

cmake -E make_directory %TARGETDIR%
cmake --preset ci-windows-x64 -S . -B %TARGETDIR%
cmake --build %TARGETDIR% --config Debug
cmake --build %TARGETDIR% --config Release
```

When the build completes (shown here using the Windows x64 preset) you'll have two libraries compiled at:
```
%TARGETDIR%/library/Debug/ziti4dotnet.dll
%TARGETDIR%/library/Release/ziti4dotnet.dll
```

Inspect the [native-nuget-publish.yml](../.github/actions/native-nuget-publish.yml) action to see the exact set of steps performed, but really you will 
probably (hopefully) never need to learn how to build this project.  

### C SDK Version
The version of the C SDK is controlled in two ways. The first way is by setting an environment variable 
named ZITI_SDK_C_BRANCH. The cmake file will look for this env var and use it, if it's supplied. The second
is via the cmake file itself which often gets updated, but might not be _the latest_. 

## Upgrading the C SDK library

If you're updating the C SDK and it's not a major change, you probably can just update the the version and it'll
be fine. Sometimes new functions show up which are exported from the C SDK but when you try to use them the functions
will not be available inside the dotnet runtime. This is _usually_ because the function was not exported
correctly when compiled. This is where the [ziti.def](./library/ziti.def) file becomes important.

NOTE!
> When upgrading the C SDK Library, you really should verify ziti.def and ZitiStatus.cs are correct.

### ziti.def
If you explore the CMakeLists.txt file you will see there is a ziti.def file. This file is **REQUIRED** for 
Windows library builds. It is also imperative that it is kept up to date. A 
[.bat file named defgen.bat](./defgen.bat) exists in this folder which _should_ create this def file properly.

To use [defgen.bat](./defgen.bat) first make sure you have run the build and have properly compiled the project.

Assuming you have done that, you then need to run defgen (requires dumpbin), which will output a ziti.def file.
This file allows the exported functions from ziti.dll to be re-exported in ziti4dotnet.

Example:
```
cd ZitiNativeApiForDotnetCore
msvc-build.bat
defgen 64 %BUILDFOLDER%\x64\_deps\ziti-sdk-c-build\library\Release\ziti.dll
```

After you run defgen, there will be three extraneous files: ziti-exports.txt, ziti.dll, ziti.exp left behind.
defgen leaves these files behind in case you need to do deubgging on the process but these files should not
be checked in (they are .gitignore'ed).

#### Seeing What Changed
You can see the delta between what was checked in and what defgen generated with this manual process/flow:

1. obviously, checkout the repo, build it and successfully run defgen.
1. copy the output file
   > copy library\ziti.def library\ziti.def.new
1. revert the new file
   > git checkout library/ziti.def
1. trim out everything after the first space - here i'm going to use `cut` but do it however you want
   > cut -d " " library/ziti.def -f1 > orig.txt
   > cut -d " " library/ziti.def.new -f1 > new.txt
1. diff the orig.txt and new.txt - here i'll use the `diff` tool but do it however you want
    diff orig.txt new.txt
    39a40,41
    > Ziti_check_socket
    > Ziti_close
    713a716
    > ziti_service_has_permission

#### "No function found"

If you are getting errors indicating a function is not found, it's likely due to the ziti.def not being updated properly or
it's not included in the cmake file. Follow the steps above.

### ZitiStatus.cs
The C SDK has a macro that generates error codes. These codes are not quite as nice to use as a C# enum so this 
project is also responsible for generating a small amount of C# which represents those status/error codes. When
the C SDK changes, remember to recreate and commit the ZitiStatus.cs file.

Here's how you can regenerate the file manually. You can either clone/fetch the version of the C SDK you are 
targetting - or just generate this project's meta data (which I think is easier).
```text
SET TARGETDIR=_TEMP_
cmake -E make_directory %TARGETDIR%
cmake -S . -B %TARGETDIR% 
cl /C /EP /I %TARGETDIR%/_deps/ziti-sdk-c-src/includes /c library/sharp-errors.c > ../OpenZiti.NET/src/OpenZiti/ZitiStatus.cs
- OR if using gcc not developer command prompt -
gcc -nostdinc -E -CC -P -I%TARGETDIR%/_deps/ziti-sdk-c-src/includes library/sharp-errors.c > ../OpenZiti.NET/src/OpenZiti/ZitiStatus.cs
```


























## For Project Contributors

If you're cloning this package with the intention to make a fix or to update the C SDK used, here's a
quick punchlist of things you will want to review and understand before really digging in. This will
take you through just the bullet points of what you need to do to make sure you can develop/test/debug.

Things you should do/understand:

* Build the native project for x64
* **If** you're using Windows, also build the native project for x86 (win32)
* Package the native dlls into a native nuget package. This boils down to putting the dll for YOUR operating
  system into the proper location, edit [the nuspec](./native-package.nuspec) and hack out the lines
  you don't want (or better copy that nuspec to a different one you don't end up committing). then package
  it, and publish it to a **local** NuGet repo path and use it locally. A convinience script exists at the
  checkout root named: `dev-build-native.bat`. It is designed for Windows/Visual Studio development.
* With the Native NuGet package built