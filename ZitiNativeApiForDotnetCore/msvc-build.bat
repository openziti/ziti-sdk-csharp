@echo off

REM a stupid env var JUST to allow a space to be set into an environment variable using substring...
set ZITI_SPACES=:   :
set NATIVE_CODE_DIR=%~dp0
set BUILDFOLDER=%NATIVE_CODE_DIR%build-win

IF /i "%1"=="help" (
    goto print_help
)
IF /i "%1"=="defgen" (
    goto defgen
)

if "%ZITI_SDK_C_BRANCH%"=="" (
    echo ZITI_SDK_C_BRANCH is not set - ZITI_SDK_C_BRANCH_CMD will be empty
    SET ZITI_SDK_C_BRANCH_CMD=%ZITI_SPACES:~2,1%
) else (
    echo SETTING ZITI_SDK_C_BRANCH_CMD to: -DZITI_SDK_C_BRANCH^=%ZITI_SDK_C_BRANCH%
    SET ZITI_SDK_C_BRANCH_CMD=-DZITI_SDK_C_BRANCH=%ZITI_SDK_C_BRANCH%
)

IF "%ZITI_DEBUG%"=="" (
    REM clear out if debug was run in the past
    SET ZITI_DEBUG_CMAKE=
) else (
    SET ZITI_DEBUG_CMAKE=-DCMAKE_BUILD_TYPE=Debug
    echo ZITI_DEBUG detected. will run cmake with: %ZITI_DEBUG_CMAKE%
)

cmake -E make_directory %BUILDFOLDER%

echo.
echo.
echo "Building 32-bit"
cmake --preset win32 -S %NATIVE_CODE_DIR% -B %BUILDFOLDER%\win32 -A Win32
cmake --build %BUILDFOLDER%\win32
cmake --build %BUILDFOLDER%\win32 --config Release

echo.
echo.
echo "Building 64-bit"
cmake --preset win64 -S %NATIVE_CODE_DIR% -B %BUILDFOLDER%\win64
cmake --build %BUILDFOLDER%\win64
cmake --build %BUILDFOLDER%\win64 --config Release
goto end

:print_help
echo.
echo To build the project issue the following commands (or execute this script with no parameters: msvc-build.bat)
echo.
echo   :Build 32-bit:
echo   cmake --preset win32 -S %NATIVE_CODE_DIR% -B %BUILDFOLDER%\win32 -A Win32
echo   cmake --build %BUILDFOLDER%\win32
echo   cmake --build %BUILDFOLDER%\win32 --config Release
echo.
echo.
echo   :Build 64-bit:
echo   cmake --preset win64 -S %NATIVE_CODE_DIR% -B %BUILDFOLDER%\win64
echo   cmake --build %BUILDFOLDER%\win64 
echo   cmake --build %BUILDFOLDER%\win64 --config Release
echo.
echo.
goto end

:defgen
cmake --preset win64 -S %NATIVE_CODE_DIR% -B %BUILDFOLDER%\win64 -DGENERATE_ZITI_STATUS=yes
REM run the below commands from microsoft developer command prompt
REM uncomment to generate a new ziti.def
REM defgen 32 build-win\x86\_deps\ziti-sdk-c-build\library\Release\ziti.dll
REM copy ziti.def library
REM cl /C /EP /I build-win/x86/_deps/ziti-sdk-c-src/includes /c library/sharp-errors.c > library/ZitiStatus.cs
REM copy library/ZitiStatus.cs ../OpenZiti.NET/src/OpenZiti 
goto end

:abnormalend
echo TERMINATED UNEXPECTEDLY

:end

