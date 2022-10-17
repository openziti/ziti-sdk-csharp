@echo off

ECHO Building the solution for the Ziti.NET.standard.dll

SET Ziti_Net_HOME=%~dp0

ECHO msbuild %Ziti_Net_HOME%..\Ziti.NuGet.sln /property:Configuration=Release /property:Platform=x64

msbuild %Ziti_Net_HOME%..\Ziti.NuGet.sln /property:Configuration=Release /property:Platform=x64

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo Build of %Ziti_Net_HOME%..\Ziti.NuGet.sln for Platform=x64 failed
    echo.
    goto FAIL
) else (
    echo.
    echo result of msbuild for Platform=x64: %ACTUAL_ERR%
)

ECHO msbuild %Ziti_Net_HOME%..\Ziti.NuGet.sln /property:Configuration=Release /property:Platform=x86

msbuild %Ziti_Net_HOME%..\Ziti.NuGet.sln /property:Configuration=Release /property:Platform=x86

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo Build of %Ziti_Net_HOME%..\Ziti.NuGet.sln for Platform=x86 failed
    echo.
    goto FAIL
) else (
    echo.
    echo result of msbuild for Platform=x86: %ACTUAL_ERR%
)

ECHO msbuild %Ziti_Net_HOME%..\Ziti.NuGet.sln /property:Configuration=Release

msbuild %Ziti_Net_HOME%..\Ziti.NuGet.sln /property:Configuration=Release

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo Build of %Ziti_Net_HOME%..\Ziti.NuGet.sln failed
    echo.
    goto FAIL
) else (
    echo.
    echo result of msbuild: %ACTUAL_ERR%
)

SET NUGET_PATH=%Ziti_Net_HOME%..\NuGet

mkdir %NUGET_PATH%

ECHO dotnet pack %Ziti_Net_HOME%..\Ziti.NuGet.sln --configuration Release --output %Ziti_Net_HOME%

dotnet pack %Ziti_Net_HOME%..\Ziti.NuGet.sln --configuration Release --output %Ziti_Net_HOME%

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo nuget pack for %BUILD_VERSION% failed
    echo.
    goto FAIL
) else (
    echo.
    echo result of dotnet pack: %ACTUAL_ERR%
)

REM 'tail' has to be on your path to execute the below command
FOR /F "delims= " %%i IN ('ls -rt Ziti.NET.Standard.*.nupkg ^| tail -n 1') DO set NUPKG_FILE=%%i

ECHO nuget push -source %NUGET_PATH% %Ziti_Net_HOME%%NUPKG_FILE%

nuget push -source %NUGET_PATH% %Ziti_Net_HOME%%NUPKG_FILE%

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo nuget push for %NUPKG_FILE% failed
    echo.
    goto FAIL
) else (
    echo.
    echo result of nuget push: %ACTUAL_ERR%
)

GOTO END

:FAIL
ECHO.
ECHO ACTUAL_ERR: %ACTUAL_ERR%
EXIT /B %ACTUAL_ERR%

:END

ECHO.
ECHO.
ECHO =====================================================
ECHO	BUILD COMPLETE	:
ECHO 	Package file 	: %NUPKG_FILE%
ECHO =====================================================
