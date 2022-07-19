@echo off

REM a stupid env var JUST to allow a space to be set into an environment variable using substring...
set ZITI_SPACES=:   :

if "%ZITI_SDK_C_BRANCH%"=="" (
    echo ZITI_SDK_C_BRANCH is not set - ZITI_SDK_C_BRANCH_CMD will be empty
    SET ZITI_SDK_C_BRANCH_CMD=%ZITI_SPACES:~2,1%
) else (
    echo SETTING ZITI_SDK_C_BRANCH_CMD to: -DZITI_SDK_C_BRANCH^=%ZITI_SDK_C_BRANCH%
    SET ZITI_SDK_C_BRANCH_CMD=-DZITI_SDK_C_BRANCH=%ZITI_SDK_C_BRANCH%
)
REM echo "================ %ZITI_SDK_C_BRANCH_CMD%"

set CSDK_HOME=%~dp0

set BUILDFOLDER=%CSDK_HOME%build-win

mkdir %BUILDFOLDER% 2> NUL
mkdir %BUILDFOLDER%\x86 2> NUL
mkdir %BUILDFOLDER%\x64 2> NUL

cmake -S %CSDK_HOME% -B %BUILDFOLDER%\x86 -G "Visual Studio 17 2022" -A Win32 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib %ZITI_SDK_C_BRANCH_CMD%
cmake -S %CSDK_HOME% -B %BUILDFOLDER%\x64 -G "Visual Studio 17 2022" -A x64 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib %ZITI_SDK_C_BRANCH_CMD%


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

