@echo off
pushd .
set CSDK_HOME=%~dp0
cd /d %CSDK_HOME%

set cmake=C:\Program Files (x86)\Microsoft Visual Studio\2019\Community\Common7\IDE\CommonExtensions\Microsoft\CMake\CMake\bin\cmake.exe
set BUILDFOLDER=%CSDK_HOME%build

ECHO Building via cmake using:
ECHO     %cmake% --build %BUILDFOLDER%\x86 --config Debug
"%cmake%" --build %BUILDFOLDER%\x86 --config Debug
REM > "%BUILDFOLDER%\x86.Debug.txt"
ECHO     build result saved to: %BUILDFOLDER%\x86.Debug.txt
ECHO.

ECHO     %cmake% --build %BUILDFOLDER%\x86 --config Release
"%cmake%" --build %BUILDFOLDER%\x86 --config Release
REM > "%BUILDFOLDER%\x86.Release.txt"
ECHO     build result saved to: %BUILDFOLDER%\x86.Release.txt
ECHO.

ECHO     %cmake% --build %BUILDFOLDER%\x64 --config Debug
"%cmake%" --build %BUILDFOLDER%\x64 --config Debug
REM > "%BUILDFOLDER%\x64.Debug.txt"
ECHO     build result saved to: %BUILDFOLDER%\x64.Debug.txt
ECHO.

ECHO     %cmake% --build %BUILDFOLDER%\x64 --config Release
"%cmake%" --build %BUILDFOLDER%\x64 --config Release
REM > "%BUILDFOLDER%\x64.Release.txt"
ECHO     build result saved to: %BUILDFOLDER%\x64.Release.txt
ECHO.

popd

