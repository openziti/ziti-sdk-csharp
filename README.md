# OpenZiti .NET SDK

An SDK targetting .NET 6.0, written in C#, deployed as a nuget package and designed to 
provide easy, secure connectivity over an OpenZiti overlay network to other .NET applications.

## Overview

This project provides two nuget packages. One nuget package is a wrapper around the OpenZiti
[C SDK](https://github.com/openziti/ziti-sdk-c) exposing the native SDK in a way easily consumed by .NET projects. 
The second package is a .NET native library which consumes the native C SDK package and exposes a more idomatic .NET API.

### Native NuGet Package

The NuGet package providing the native libraries is built and published by GitHub actions. Building the native library is somewhat
complex. If you are interested in learning how the native library is built, see the [github action file](.github/workflows/native-nuget-publish.yml).
The package is built using a [cmake](https://cmake.org/) project found in the repository, in a folder named ZitiNativeApiForDotnetCore.
You can look through that project and [read the readme](./ZitiNativeApiForDotnetCore/README.md) for more detailed information. 
Building a cross-platform .NET library, using a native C library is somewhat complicated.

### NuGet Package for .NET SDK

The project also provides [a solution file](./Ziti.NuGet.sln) which is designed to create the actual dependency you would add to your .NET
project in the form of another NuGet package. This package will provide idiomatic .NET functions for use in your own project.

### Samples

The project provides [a solution file](./Ziti.Samples.sln) which contains a suite of samples which you can inspect and draw inspiration
from.

## For Project Developers

If you're cloning this package with the intention to make a fix or to update the C SDK used, here's a quick punchlist of things you will
want to review and understand before really digging in. This will take you through just the bullet points of what you need to do to make
sure you can develop/test/debug. 

Things you should do/understand:

* Build the native project for x64
* **If** you're using Windows, also build the native project for x86
* Package the native dlls into a nuget package. This boils down to putting the dll for YOUR operating system into the proper location,
  edit [the nuspec](./native-package.nuspec) and hack out the lines you don't want (or better copy that nuspec to a different one you
  don't end up committing). then package it, and publish it to a **local** NuGet repo path and use it locally.
  * Once you're ready and you think the native project is "correct" - you should push just the relevant changes and let 
    [the GitHub Action](https://github.com/openziti/ziti-sdk-csharp/actions/workflows/native-nuget-publish.yml) publish the latest 
    native nuget package. 
  * Pull and use the latest native nuget package
* Assuming you have 'the latest' nuget package - adapt the C# SDK code and write **IDIOMATIC** C# for the API.
* Once the **IDIOMATIC** C# API exists, publish the Ziti.NET.Standard version to your **LOCAL** NuGet repo.
  * Run `dotnet build` to build and publish the project. Make sure you supply the variable named "NUGET_SOURCE". It is used to control
    where you push to. It can be either your **LOCAL** nuget repo or https://api.nuget.org/v3/index.json. This build will **ALSO** 
    build both x86 and x64 for you.
    ```
    dotnet build Ziti.NET.Standard\Ziti.NET.Standard.csproj /t:NugetPush /p:Configuration=Release;NUGET_SOURCE=c:/temp/nuget
    ```
* Open Ziti.Samples.sln and update the Ziti.NET.Standard version to use your latest version from local
* Develop one or more samples which illustrate the usage of the SDK. 
* Once happy with the samples, push back to GitHub, merge to a release branch/tag/main and let GitHub publish the package to NuGet central
* Once verified and published on NuGet, update the Ziti.Samples.sln with the **ACTUAL** deployed value for Ziti.NET.Standard
* Test on Windows x86, x64, linux, MacOS - or hopefully we write (wrote?) automated tests to do this

### Build and Package the NuGet Native Project

See [the readme](./ZitiNativeApiForDotnetCore/README.md) for details on how to build the native NuGet package. You need to be able
to deploy a local version of the native NuGet package if you want to verify your code will work before trying to push fixes.

### Package the .NET Project Locally

The [project](./Ziti.NET.Standard/) has a target within it which should make it trivial for you to build the dotnet NuGet package. To do so
simply issue
```
SET LOCAL_NUGET_PACKAGES=%CD%\local-nuget-packages
SET APP_KEY=_local_
mkdir %LOCAL_NUGET_PACKAGES%
dotnet build OpenZiti.NET\OpenZiti.NET.csproj /t:NugetPush /p:Configuration=Release;NUGET_SOURCE=%LOCAL_NUGET_PACKAGES%
```

This will subsequently issue `dotnet build` commands to build the project as x86, x64, as well as "Any CPU". It will then issue `nuget push`
and will push your freshly built .nupkg into the location specified by the properly `NUGET_SOURCE=`.

### TestProject

Another project is included in the [Ziti.NuGet.sln](./Ziti.NuGet.sln) is [TestProject](./TestProject). This project **should** contain
**linked** .cs files from the [Ziti.NET.Standard](./Ziti.NET.Standard) project. Any new .cs files should be part of the project that 
produces the nuget package and only **linked** in TestProject.  TestProject is then able to be a playground to verify your changes
are functioning as expected.

