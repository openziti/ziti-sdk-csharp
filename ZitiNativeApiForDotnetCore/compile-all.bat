@echo off

set NATIVE_LIB_HOME=%~dp0
set BUILDFOLDER=%NATIVE_LIB_HOME%build-win
if [%1]==[] goto usage
if /i "%1"=="Release" goto ok
if /i "%1"=="Debug" goto ok
goto usage

:ok
echo we got :%RELEASE_OR_DEBUG%:

set RELEASE_OR_DEBUG=%1

call %NATIVE_LIB_HOME%msvc-build.bat

ECHO Building via cmake using:
ECHO     cmake --build %BUILDFOLDER%\x86 --config %RELEASE_OR_DEBUG%
cmake --build %BUILDFOLDER%\x86 --config %RELEASE_OR_DEBUG% > "%BUILDFOLDER%\x86.%RELEASE_OR_DEBUG%.txt"
ECHO     build result saved to: %BUILDFOLDER%\x86.%RELEASE_OR_DEBUG%.txt
ECHO.

ECHO     cmake --build %BUILDFOLDER%\x64 --config %RELEASE_OR_DEBUG%
cmake --build %BUILDFOLDER%\x64 --config %RELEASE_OR_DEBUG% > "%BUILDFOLDER%\x64.%RELEASE_OR_DEBUG%.txt"
ECHO     build result saved to: %BUILDFOLDER%\x64.%RELEASE_OR_DEBUG%.txt
ECHO.
goto end

:usage
echo.
echo   USAGE:
echo       compile-all.bat ^[Release^|Debug^]
echo.
:end
echo done