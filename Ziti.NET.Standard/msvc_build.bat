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

set /p BUILD_VERSION=<%Ziti_Net_HOME%..\version

ECHO nuget pack Ziti.NET.Standard.nuspec -Version %BUILD_VERSION%

nuget pack Ziti.NET.Standard.nuspec -Version %BUILD_VERSION%

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo nuget pack for %BUILD_VERSION% failed
    echo.
    goto FAIL
) else (
    echo.
    echo result of nuget pack: %ACTUAL_ERR%
)

ECHO nuget push -source %NUGET_PATH% %Ziti_Net_HOME%..\Ziti.NET.Standard.%BUILD_VERSION%.nupkg

nuget push -source %NUGET_PATH% %Ziti_Net_HOME%..\Ziti.NET.Standard.%BUILD_VERSION%.nupkg

SET ACTUAL_ERR=%ERRORLEVEL%
if %ACTUAL_ERR% NEQ 0 (
    echo.
    echo nuget push for %BUILD_VERSION% failed
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
ECHO 	VERSION 		: %BUILD_VERSION%
ECHO =====================================================
