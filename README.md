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

* mklink /j ziti-sdk-c\x86 c:\git\github\ziti-sdk-c\build\x86\windows\dotnet_dll\Release
* mklink /j ziti-sdk-c\x64 c:\git\github\ziti-sdk-c\build\x64\windows\dotnet_dll\Release

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
** SET VERSION=0.5.16  REM Or whatever the version is built - see the version of the Ziti.NET.Standard project
* Make a local nuget repo: `mkdir %NUGET_PATH%`
* Push the package into the local repo: `nuget push -source %NUGET_PATH% Ziti.NET.Standard\bin\Release\Ziti.NET.Standard.%VERSION%.nupkg`

You should see output like:

    Pushing Ziti.NET.Standard.0.0.19.nupkg to 'C:\git\github\ziti-sdk-csharp\NuGet'...
    Your package was pushed.

## Using the C# SDK

You can choose to use the latest version of the C# SDK which NetFoundry has published on nuget.org or you can 
work with the C# SDK you built and deployed to your own NuGet local repository. Open the example 
solution: Ziti.Core.Example.sln. In there is one project - Ziti.Core.Console. 

This is a sample application that allows you to make an http request to a website (http://wttr.in) to return 
a weather forcast.  After getting the project to build you'll want to run it. If you have access to a Ziti network
this will be easy. If you are not familiar with Ziti and need to create this service. Check out the docs 
over at https://nf-dev.github.io/ziti-doc/samples/index.html?tabs=csharp
