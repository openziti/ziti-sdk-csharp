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
should be able to simply issue something like what is shown below. The output will go to `./build` and it seems vcpkg
is sensitive to trying to change the `binaryDir`. You'll also only be able to build one arch at a time.

```
cmake --preset win64 -S .
cmake --build build --config Debug
cmake --build build --config Release
```

When the build completes (shown here using the Windows x64 preset) you'll have two libraries compiled at:
```
%TARGETDIR%/library/Debug/ziti4dotnet.dll
%TARGETDIR%/library/Release/ziti4dotnet.dll
```

(Every arch is different. Linux produces "libziti4dotnet.so", macOS produces "libziti4dotnet.dylib".)

Inspect the [native-nuget-publish.yml](../.github/actions/native-nuget-publish.yml) action to see the exact set of steps
performed, but really you will probably (hopefully) never need to learn how to build this project.

### C SDK Version
The version of the C SDK is controlled in two ways. The first way is by setting an environment variable 
named ZITI_SDK_C_BRANCH. The cmake file will look for this env var and use it, if it's supplied. The second
is via the cmake file itself which often gets updated, but might not be _the latest_. It's a good idea to update the
CMakeLists.txt file with the latest C SDK every now and then. CI will use the environment variable, passed in when
the action is invoked (manually).

## Upgrading the C SDK library

### The Quick Punchlist

* change CMakeLists.txt: `set(ZITI_SDK_C_BRANCH_DEFAULT "0.35.0")`
* configure cmake: `cmake --preset win64 .`
* add the files: `git add CMakeLists.txt library/ziti.def`
* push to GitHub
* manually trigger [the GitHub Workflow](https://github.com/openziti/ziti-sdk-csharp/actions/workflows/native-nuget-publish.yml)
  with the new SDK version.

If you're updating the C SDK and it's not a major change, you probably can just update the the version and it'll
be fine. Sometimes new functions show up which are exported from the C SDK but when you try to use them the functions
will not be available inside the dotnet runtime. This is _usually_ because the function was not exported
correctly when compiled. This is where the [ziti.def](./library/ziti.def) file becomes important.

NOTE!
> When upgrading the C SDK Library, you really should verify ziti.def and ZitiStatus.cs are correct.

### ziti.def
If you explore the CMakeLists.txt file you will see there is a ziti.def file referenced. This file is **REQUIRED** for 
Windows library builds. It is also imperative that it is kept up to date. A 
[.bat file named defgen.bat](./defgen.bat) exists in this folder which _should_ create this def file properly. As of
October 2023, the process was added as a part of the cmake configuration step. It will use `FetchContent_Declare` 
and `URL`. See the CMakeLists.txt file for how it's done.

**IF YOU UPDATE THE LIBRARY, YOU MUST RUN CMAKE AND MAKE SURE THE .DEFGEN STEP**

To run the defgen step, when configuring cmake pass: -DZITI_RUN_DEFGEN. It will then invoke `defgen.bat` during the
configuration step. If the file changes, **make sure you commit the file**.

Example command to run defgen during the `cmake` configuration step:
```
cmake --preset win64 -S . -DZITI_RUN_DEFGEN=yes
```

After it runs, defgen leaves behind three extraneous files: ziti-exports.txt, ziti.dll, ziti.exp. It leaves these 
files behind in case you need to do deubgging on the process but these files should not be checked in
(they are .gitignore'ed).

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
As of October 2023, the `ZitiStatus.cs` file is generated by a powershell script invoked from cmake if the cmake
param `GENERATE_ZITI_STATUS` is set to 'yes'. See [generateDotnetStatus.ps1]() for how this is accomplished.
