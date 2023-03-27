
# Native NuGet Package

As mentioned in [the README](../README.md), this project is responsible for creating a nuget package which
not only exposes the [ziti-sdk-c](https://github.com/openziti/ziti-sdk-c) functions in an easy-to-consume
way, but it also layers on helper functions as needed. Often these additional functions will be to do things
which dotnet doesn't seem to support, or we haven't discovered how to support it yet. Generally things like 
iterating a pointer or var args usage.

This is a project based on [cmake](https://cmake.org/) which will compile the given C SDK into the native 
nuget package.

The package should be consumable on Windows AMD x86/x64, AMD MacOS x64 and AMD linux x64 architectures. It
does not currently have ARM versions. We currently **do not** support every RID. If your favorite dotnet 
arch is not covered by the ones mentioned, we'd love help it getting it working for your specific environment.

## Building
If you're considering building this project, you are almost certainly trying to develop the actual dotnet SDK,
or you're just wondering how all this project comes together. By far, the easiest way to learn is to go read
[the helper bat file](../build-native.bat). Assuming you have the project checked out and the needed dependencies 
on the path:
* C compiler (Visual Studio 2019/2022 currently)
* cmake
* dotnet
* nuget
* [incomplete list, others might be needed]
 
this bat file _should_ just work.

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

