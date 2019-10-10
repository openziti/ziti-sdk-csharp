# ziti-sdk-csharp

An C#-based SDK to access Ziti 

## Build the Ziti C SDK

The Ziti C# SDK is based on the [C SDK](https://github.com/nf-dev/ziti-sdk-c) and requires a native library for your target platform of choice.

## Add Links to the Native Libraries

This project expects both an x86 and x64 library to be built and to exist at the root of this project in the following folders:

* ziti-sdk-c/x86/ziti_dll.dll
* ziti-sdk-c/x64/ziti_dll.dll

If you follow the build instructions for the [C SDK](https://github.com/nf-dev/ziti-sdk-c) you can then create 
links to the built artifacts using `mklink`.

* mklink /j x86 c:\git\github\ziti-sdk-c\build\x86\windows\dotnet_dll\Release
* mklink /j x64 c:\git\github\ziti-sdk-c\build\x64\windows\dotnet_dll\Release

## Build the Ziti.NuGet.sln Project

Open Ziti.NuGet.sln or use msbuild (`msbuild Ziti.NuGet.sln`) to build the project. The output from within visual studio looks like this:

    1>------ Build started: Project: Ziti.NET.Standard, Configuration: Release Any CPU ------
    1>Ziti.NET.Standard -> C:\git\github\ziti-sdk-csharp\Ziti.NET.Standard\bin\Release\netcoreapp2.0\Ziti.NET.Standard.dll
    1>Ziti.NET.Standard -> C:\git\github\ziti-sdk-csharp\Ziti.NET.Standard\bin\Release\net472\Ziti.NET.Standard.dll
    1>Ziti.NET.Standard -> C:\git\github\ziti-sdk-csharp\Ziti.NET.Standard\bin\Release\netstandard2.0\Ziti.NET.Standard.dll
    1>Successfully created package 'C:\git\github\ziti-sdk-csharp\Ziti.NET.Standard\bin\Release\Ziti.NET.Standard.0.0.19.nupkg'.
    ========== Build: 1 succeeded, 0 failed, 0 up-to-date, 0 skipped ==========

## Build the NuGet Package

Consuming native artifacts from C# is sometimes tedious. As you may have noted above the Ziti.NET.Standard project will build a nupkg when it builds.
If you add that package to a local NuGet repository it makes consuming the C# SDK much easier as the native libraries will be added to the proejct
correctly and should "just work"

Here's how you would make your own local NuGet repository on your developer machine:

* open a command prompt
* set some environment variables:
** SET NUGET_PATH=C:\git\github\ziti-sdk-csharp\NuGet
** SET VERSION=0.0.19  REM Or whatever the version is built - see the version of the Ziti.NET.Standard project
* Make a local nuget repo: `mkdir %NUGET_PATH%`
* Push the package into the local repo: `nuget push -source %NUGET_PATH% Ziti.NET.Standard\bin\Release\Ziti.NET.Standard.0.0.19.nupkg`

You should see output like:

    Pushing Ziti.NET.Standard.0.0.19.nupkg to 'C:\git\github\ziti-sdk-csharp\NuGet'...
    Your package was pushed.

## Using the C# SDK


C:\git\github\ziti-sdk-csharp\Ziti.NET.Standard\bin\Debug\Ziti.NET.Standard.0.0.19.nupkg


first need to acquire built library

build xc64

need proper version of .net core

build project:

git clone
cd Ziti.NET.Standard
msbuild -p:Configuration=Release 

(or do it with visual studio)

will build a nuget package
add nuget package to local source

set ziti_csharp_root=V:\work\git\ziti-sdk-c\windows\Ziti.NET.Standard
set zitiver=17
set nuget_repo=C:/temp/ziti/local-nuget/
set remote_nuget_repo=https://netfoundry.jfrog.io/netfoundry/api/nuget/nuget-local
nuget add -source %nuget_repo% %ziti_csharp_root%\bin\Release\Ziti.NET.Standard.0.0.%zitiver%.nupkg


toggle prefer 32bit





