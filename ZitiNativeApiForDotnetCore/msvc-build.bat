@echo off & setlocal
pushd .
set CSDK_HOME=%~dp0
cd /d %CSDK_HOME%

set BUILDFOLDER=%CSDK_HOME%build

mkdir %BUILDFOLDER% 2> NUL
mkdir %BUILDFOLDER%\x86 2> NUL
mkdir %BUILDFOLDER%\x64 2> NUL
pushd %BUILDFOLDER%

pushd %BUILDFOLDER%\x86
REM cmake -S %BUILDFOLDER%\x86 -G "Visual Studio 16 2019" -A Win32 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib
cmake -S %CSDK_HOME% -B %BUILDFOLDER%\x86 -G "Visual Studio 16 2019" -A Win32 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib
popd

pushd %BUILDFOLDER%\x64
REM cmake ..\.. -G "Visual Studio 16 2019" -A x64 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib
cmake -S %CSDK_HOME% -B %BUILDFOLDER%\x64 -G "Visual Studio 16 2019" -A Win32 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib
popd

ECHO Build from cmake using: 
ECHO     cmake --build %BUILDFOLDER%\x86 --config Debug
ECHO     cmake --build %BUILDFOLDER%\x86 --config Release
ECHO. 
ECHO     cmake --build %BUILDFOLDER%\x64 --config Debug
ECHO     cmake --build %BUILDFOLDER%\x64 --config Release
ECHO. 
ECHO Or open %BUILDFOLDER%\ziti-sdk.sln

goto end

:abnormalend
echo TERMINATED UNEXPECTEDLY

:end
popd

