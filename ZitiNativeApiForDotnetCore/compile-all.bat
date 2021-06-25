@echo off

set NATIVE_LIB_HOME=%~dp0
set BUILDFOLDER=%NATIVE_LIB_HOME%build-win
if [%1]==[] goto usage
if /i "%1"=="Release" goto ok
if /i "%1"=="Debug" goto ok
goto usage

:ok
set RELEASE_OR_DEBUG=%1
REM echo RELEASE_OR_DEBUG=%RELEASE_OR_DEBUG%

call %NATIVE_LIB_HOME%msvc-build.bat

echo Building via cmake using:
echo     cmake --build %BUILDFOLDER%\x86 --config %RELEASE_OR_DEBUG%
cmake --build %BUILDFOLDER%\x86 --config %RELEASE_OR_DEBUG% > "%BUILDFOLDER%\x86.%RELEASE_OR_DEBUG%.txt"
echo     build result saved to: %BUILDFOLDER%\x86.%RELEASE_OR_DEBUG%.txt
echo.

echo     cmake --build %BUILDFOLDER%\x64 --config %RELEASE_OR_DEBUG%
cmake --build %BUILDFOLDER%\x64 --config %RELEASE_OR_DEBUG% > "%BUILDFOLDER%\x64.%RELEASE_OR_DEBUG%.txt"
echo     build result saved to: %BUILDFOLDER%\x64.%RELEASE_OR_DEBUG%.txt
echo.
goto end

:usage
echo.
echo   USAGE:
echo       compile-all.bat ^[Release^|Debug^]
echo.
:end
echo done