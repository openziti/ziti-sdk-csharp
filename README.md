![Ziggy using the sdk-csharp](https://raw.githubusercontent.com/openziti/branding/main/images/banners/CSharp.jpg)

# OpenZiti.NET SDK

The OpenZiti.NET SDK is a project that enables developers to create clients and applications leveraging an OpenZiti 
overlay network. OpenZiti is a modern, programmable overlay network with associated edge components, for 
application-embedded, zero trust network connectivity, written by developers for developers. The SDK harnesses that 
power via APIs that allow developers to imagine and develop solutions beyond what OpenZiti handles by default.

Using an OpenZiti SDK for your application's communication needs makes your application secure-by-default by 
incorporating zero trust principles **directly into** the application itself. All communication over an OpenZiti 
overlay network is performed over a [mutual TLS (mtls)](https://en.wikipedia.org/wiki/Mutual_authentication) secured connection. OpenZiti also implements truly
[end-to-end encryption](https://openziti.io/docs/learn/introduction/features/#e2e-encryption) (via [libsodium](https://libsodium.gitbook.io/)) by default. The developer does not need to do more than incorporate
the OpenZiti SDK into an application to gain these immediate benefits. The OpenZiti overlay network provides numerous
other security features that combine to make a compelling security solution for any application.

If you're new to OpenZiti or overlay networks it might be useful to check out 
[the official documentation](https://openziti.io) or inspect [https://github.com/openziti/ziti](the main repo) and 
learn a more about OpenZiti and zero trust in general.

## Getting Started

To get started you'll need to have an OpenZiti overlay network. Deploy one by running 
[one of the OpenZiti network quickstarts](https://openziti.io/docs/category/network).

With access to an OpenZiti overlay network, there are a few important concepts that are helpful to understand, but
are not required for executing the samples:

* What is an [Identity](https://openziti.io/docs/learn/core-concepts/identities/overview/)
* How are identities created
* What is [enrollment](https://openziti.io/docs/learn/core-concepts/security/enrollment/) and how are identities enrolled
* Using and loading an [Identity](https://openziti.io/docs/learn/core-concepts/identities/overview/) to create a [ZitiContext](OpenZiti.NET/src/OpenZiti/ZitiContext.cs)
* Dialing and Binding

## Running the Samples

The samples included in this repo are designed to allow you to explore this SDK and explore OpenZiti without fully
understanding these concepts. You will of course need to learn them to make the most of the SDK, but to get started
you won't need to.

Running the samples 








---
## OLDER STUFFS
This project is dedicated to creating an idiomatic dotnet SDK for OpenZiti to add secure
communications to your application, quickly and easily.

The project is written in C# and exposes the native OpenZiti 
[C SDK](https://github.com/openziti/ziti-sdk-c) in a way that is meant to be portable and
easy to consume by by other dotnet applications.

You can read more about OpenZiti on the official docs site: [https://openziti.io](https://openziti.io).




 

## Overview

This project provides two nuget packages. 
* Native NuGet package - [available on nuget.org](https://www.nuget.org/packages/OpenZiti.NET.native/)
* Idiomatic dotnet SDK - [available on nuget.org](https://www.nuget.org/packages/OpenZiti.NET/)

### Native NuGet Package

The Native NuGet package is built and published by GitHub actions. It currently supports the following
architectures:
* Windows - x64 (64bit)
* Windows - x86 (32bit)
* MacOS - x86_64
* MacOS - arm64
* linux - x64
* linux - arm64
* linux - arm

Building the native library is somewhat complex if you're unfamiliar with cmake. The native libraries 
are built built using a [cmake](https://cmake.org/) project found in the ZitiNativeApiForDotnetCore directory.

This project 



If you are interested in learning
how the native library is built, see the [github action file](.github/workflows/native-nuget-publish.yml) or
look at the file [msvc-build.bat](./ZitiNativeApiForDotnetCore/msvc-build.bat).  You can look through
that project and [read the readme](./ZitiNativeApiForDotnetCore/README.md) for more detailed information.
Building a cross-platform .NET library, using a native C library is somewhat complicated.

The Native library is then wrapped by the .NET nuget package for idiomatic consumption.










One NuGet package is simply a packaging of the 
OpenZiti [C SDK](https://github.com/openziti/ziti-sdk-c) to expose the native C SDK in a
way easily consumed by .NET projects. The second package is a dotnet library that wraps
the native NuGet package and exposes a more idomatic dotnet API. The second package



### NuGet Package for .NET SDK

The project provides [a solution file](./Ziti.NuGet.sln) which is used to create the actual dependency 
most people will use add as a dependency to their projects. This is the idiomatic .NET wrapper around the
native nuget package for use in other .NET projects.

### Samples

The project provides [a solution file](./Ziti.Samples.sln) which contains a suite of samples which can
inspected and draw inspiration from. This project will consume the idiomatic, NuGet package for .NET.


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


1
























  * Once you're ready and you think the native project is "correct" - you should push just the relevant 
    changes and let [the GitHub Action](https://github.com/openziti/ziti-sdk-csharp/actions/workflows/native-nuget-publish.yml) 
    publish the latest native nuget package. 
  * Pull and use the latest native nuget package
* Assuming you have 'the latest' nuget package - adapt the C# SDK code and write **IDIOMATIC** C# for the API.
* Once the **IDIOMATIC** C# API exists, publish the OpenZiti.NET version to your **LOCAL** NuGet repo.
  * Run `dotnet build` to build and publish the project. Make sure you supply the variable named "NUGET_SOURCE". It is used to control
    where you push to. It can be either your **LOCAL** nuget repo or https://api.nuget.org/v3/index.json. This build will **ALSO** 
    build both x86 and x64 for you.
    ```
    SET LOCAL_NUGET_PACKAGES=%CD%\local-nuget-packages
    SET APP_KEY=_local_
    mkdir %LOCAL_NUGET_PACKAGES%
    dotnet build OpenZiti.NET\OpenZiti.NET.csproj /t:NugetPush /p:Configuration=Release;NUGET_SOURCE=%LOCAL_NUGET_PACKAGES%
    ```
* Open Ziti.Samples.sln and update the OpenZiti.NET version to use your latest version from local
* Develop one or more samples which illustrate the usage of the SDK. 
* Once happy with the samples, push back to GitHub, merge to a release branch/tag/main and let GitHub publish the package to NuGet central
* Once verified and published on NuGet, update the Ziti.Samples.sln with the **ACTUAL** deployed value for OpenZiti.NET
* Test on Windows x86, x64, linux, MacOS - or hopefully we write (wrote?) automated tests to do this

### Build and Package the NuGet Native Project

See [the readme](./ZitiNativeApiForDotnetCore/README.md) for details on how to build the native NuGet package. You need to be able
to deploy a local version of the native NuGet package if you want to verify your code will work before trying to push fixes.

### Package the .NET Project Locally

The [project](./OpenZiti.NET/) has a target within it which should make it trivial for you to build the dotnet NuGet package. To do so
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
**linked** .cs files from the [OpenZiti.NET](./OpenZiti.NET) project. Any new .cs files should be part of the project that 
produces the nuget package and only **linked** in TestProject.  TestProject is then able to be a playground to verify your changes
are functioning as expected.
