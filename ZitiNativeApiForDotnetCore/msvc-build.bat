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

IF "%ZITI_DEBUG%"=="" (
    REM clear out if debug was run in the past
    SET ZITI_DEBUG_CMAKE=
) else (
    SET ZITI_DEBUG_CMAKE=-DCMAKE_BUILD_TYPE=Debug
    echo ZITI_DEBUG detected. will run cmake with: %ZITI_DEBUG_CMAKE%
)

set CSDK_HOME=%~dp0

set BUILDFOLDER=%CSDK_HOME%build-win

mkdir %BUILDFOLDER% 2> NUL
mkdir %BUILDFOLDER%\x86 2> NUL
mkdir %BUILDFOLDER%\x64 2> NUL

cmake -S %CSDK_HOME% -B %BUILDFOLDER%\x86 -G "Visual Studio 16 2019" -A Win32 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib %ZITI_SDK_C_BRANCH_CMD% %ZITI_DEBUG_CMAKE%
cmake -S %CSDK_HOME% -B %BUILDFOLDER%\x64 -G "Visual Studio 16 2019" -A x64 -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib %ZITI_SDK_C_BRANCH_CMD% %ZITI_DEBUG_CMAKE%

REM run the below commands from microsoft developer command prompt
REM uncomment to generate a new ziti.def
REM defgen 32 build-win\x86\_deps\ziti-sdk-c-build\library\Release\ziti.dll
REM copy ziti.def library
REM cl /C /EP /I build-win/x86/_deps/ziti-sdk-c-src/includes /c library/sharp-errors.c > library/ZitiStatus.cs
REM copy library/ZitiStatus.cs ../OpenZiti.NET/src/OpenZiti 

ECHO Build from cmake using: 
ECHO     cmake --build %BUILDFOLDER%\x86 --config Debug
ECHO     cmake --build %BUILDFOLDER%\x86 --config Release
cmake --build %BUILDFOLDER%\x86 --config Debug
ECHO. 
ECHO     cmake --build %BUILDFOLDER%\x64 --config Debug
ECHO     cmake --build %BUILDFOLDER%\x64 --config Release
cmake --build %BUILDFOLDER%\x64 --config Debug
ECHO. 
ECHO Or open %BUILDFOLDER%\ziti-sdk.sln

goto end

:abnormalend
echo TERMINATED UNEXPECTEDLY

:end

