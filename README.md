# ziti-sdk-csharp

An C#-based SDK to access Ziti 

## Overview of the Code

The Ziti .NET SDK is written in C# and around the [C SDK](https://github.com/openziti/ziti-sdk-c). The .NET SDK requires a "native" nuget package
to be built and published. Publishing the native package is handled by GitHub actions. If you are interested in learning how the native library is
built, see the [github action file](.github/workflows/native-nuget-publish.yml). Publishing the package is somewhat complex.  The package is
built using a [cmake](https://cmake.org/) project found at the root of this project in a folder named ZitiNativeApiForDotnetCore. You can look
through that project for more detailed infromation.

The ZitiNativeApiForDotnetCore project will 

and requires a native library for your target platform of choice.
Building this library for all platforms is complex. There is a [cmake](https://cmake.org/) project at the root of the checkout named:
ZitiNativeApiForDotnetCore. This project must be built in o

## Add Links to the Native Libraries

This project uses a native library for most of the work communicating to ziti. When you are consuming the nuget pagage - this is all packaged up
for you. However if you are trying to do development of the c# sdk itself you'll need to build these native libraries yourself. This can be done
easily if you have experience with cmake. The ZitiNativeApiForDotnetCore folder contains a CMakeLists.txt file which can be used to build these
native libraries as well as a bat file that makes it easier to build in the expected mannor. 

To prepare for building the nuget package - cd to ZitiNativeApiForDotnetCore and run `msvc-build.bat` from a Visual Studio 2019 command prompt.
After it completes you should see output similar to:

```
Build from cmake using:
    cmake --build c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build-win\x86 --config Debug
    cmake --build c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build-win\x86 --config Release

    cmake --build c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build-win\x64 --config Debug
    cmake --build c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build-win\x64 --config Release
```

You'll likely want to just build the Release libraries but you can use Debug if you like but you'll have to update any references to the .dlls.

Once built - the project will expect these libraries to be at:

* ZitiNativeApiForDotnetCore\build-win\x86\library\Release\ziti4dotnet.dll
* ZitiNativeApiForDotnetCore\build-win\x64\library\Release\ziti4dotnet.dll

If the C SDK changes and you need to export additional functions with ziti4dotnet.dll you will need to rerun defgen after building the C SDK and the
you'll want to rebuild the ziti4dotnet.dll libs for x86 and x64. You "should" only have to run defgen one time to generate the proper files for the
dll to be built correctly. The ZitiNativeApiForDotnetCore\library\CMakeLists.txt file will refer to ziti.def and is what allows the static functions
to be exported by the resultant dll.

```
cd ZitiNativeApiForDotnetCore
defgen 32 build-win\x86\_deps\ziti-sdk-c-build\library\Release\ziti.dll
```

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

