# ziti-sdk-csharp

An C#-based SDK to access Ziti 

## Build the Ziti C SDK

The Ziti C# SDK is based on the [C SDK](https://github.com/nf-dev/ziti-sdk-c) and requires a native library for your target platform of choice.

This project expects both an x86 and x64 library to be built and to exist at the root of this project in the following folders:

* ziti-sdk-c/x86/ziti_dll.dll
* ziti-sdk-c/x64/ziti_dll.dll

If you follow the build instructions for the [C SDK](https://github.com/nf-dev/ziti-sdk-c) you can then create 
links to the built artifacts using `mklink`.




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





