echo off
REM this bat file is a convinience script for local development. It is meant to build an entire
REM local native nuget package for use while testing/local dev. It's frequently necessary to add
REM functions around the C SDK for helping dotnet with something that's better done in C or for
REM testing the blitting of types when structs change. This script should build both x86 and x64
REM for windows, and produce a nuget package ONLY usable by windows for development.
REM
REM After executing, there should be a nuget package located at local-nuget-packages named after
REM the year.month.day.hourminute.nupkg and should look something like these:
REM 
REM    local-nuget-packages\OpenZiti.NET.native.2022.11.1.14.nupkg
REM    local-nuget-packages\OpenZiti.NET.native.2022.11.1.946.nupkg
REM
set CODE_ROOT=%~dp0
set NATIVE_ROOT=%CODE_ROOT%ZitiNativeApiForDotnetCore\
set LOCAL_NUGET_PACKAGES=%CODE_ROOT%local-nuget-packages
cd %NATIVE_ROOT%

del /s %NATIVE_ROOT%build-win\x64\library\Debug\ziti4dotnet.*
del /s %NATIVE_ROOT%build-win\x86\library\Debug\ziti4dotnet.*
echo REMOVED OLD DLLS

call msvc-build.bat

cd %CODE_ROOT%
copy /y %NATIVE_ROOT%build-win\x64\library\Debug\ziti4dotnet.* runtimes\win-x64\native
copy /y %NATIVE_ROOT%build-win\x86\library\Debug\ziti4dotnet.* runtimes\win-x86\native

if not exist %LOCAL_NUGET_PACKAGES% mkdir %LOCAL_NUGET_PACKAGES%

set yearstr=%date:~10,4%
set daystr=%date:~7,2%
set monthstr=%date:~4,2%

SET HOUR=%TIME:~0,2%
IF "%HOUR:~0,1%" == " " SET HOUR=0%HOUR:~1,1%

set timenow=%TIME: =0%
set minstr=%timenow:~3,2%

set datestr=%date:~10,4%-%date:~7,2%-%date:~4,2%
echo %datestr% %yearstr% %monthstr% %daystr% %HOUR% %minstr%
nuget pack -version %yearstr%.%monthstr%.%daystr%.%HOUR%%minstr% -OutputDirectory %LOCAL_NUGET_PACKAGES% native-package.nuspec

