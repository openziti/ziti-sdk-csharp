#!/bin/bash

CLEAN_BUILD=""
NATIVE_LIB_VERSION=""
STANDARD_LIB=""
SAMPLE_PROGRAMS=""
ERROR=""

SCRIPT_DIR=$PWD
NUGET_PATH="$SCRIPT_DIR/NuGet"
mkdir $NUGET_PATH

usage() {
    echo "Syntax: scriptTemplate [-c|n <version>|s|p]"
    echo "options:"
    echo "-c           ### generate clean build"
    echo "-n <version> ### generate native library Ziti.NET.standard.native using the given version"
    echo "-s           ### generate standard library Ziti.NET.standard"
    echo "-p           ### build samples programs"
    exit 0
}

if [[ "$#" -gt 0 ]]; then 
    while getopts ":cn:sph" option;
        do
        case $option in
        c)
            echo "clean build $OPTARG"
            CLEAN_BUILD="clean"
            ;;
        n)
            echo "native library will be rebuilt with the version specified in the environment variable - ZITI_SDK_C_BRANCH [$ZITI_SDK_C_BRANCH]."
            echo "If ZITI_SDK_C_BRANCH is not set, the latest c-sdk version will be used. The Ziti.NET.Standard.native build will be created with version $OPTARG"
            NATIVE_LIB_VERSION=$OPTARG
            ;;
        s)
            echo "Ziti.NET.Standard library will be rebuilt $OPTARG"
            STANDARD_LIB="build"
            ;;
        p)
            echo "Build sample programs"
            SAMPLE_PROGRAMS="build"
            ;;
        h)
            usage
            ;;
        :)
            ERROR="option -$OPTARG needs an argument"
            ;;
        *)
            ERROR="invalid option $OPTARG, use -h to find the usage"
            ;;
        esac
        done
fi

if [[ "$ERROR" != "" ]]; then
    echo $ERROR
    echo "Error in the arguemnts, Exiting ..."
    exit 1
fi

if [[ "$NATIVE_LIB_VERSION" != "" ]]; then
    echo "Building the Ziti.NET.standard.native library"

    if [[ "$ZITI_SDK_C_BRANCH" == "" ]]; then
        echo "ZITI_SDK_C_BRANCH is not set - ZITI_SDK_C_BRANCH_CMD will be empty"
        ZITI_SDK_C_BRANCH_CMD=" "
    else
        echo "SETTING ZITI_SDK_C_BRANCH_CMD to: -DZITI_SDK_C_BRANCH=$ZITI_SDK_C_BRANCH."
        ZITI_SDK_C_BRANCH_CMD="-DZITI_SDK_C_BRANCH=$ZITI_SDK_C_BRANCH"
    fi

    echo "================ZITI_SDK_C_BRANCH_CMD: $ZITI_SDK_C_BRANCH_CMD"

    NATIVE_CODE_BUILD_PATH="$SCRIPT_DIR/ZitiNativeApiForDotnetCore/build-win"

    if [[ "$CLEAN_BUILD" -eq "clean" ]]; then
        echo "clean up:  $NATIVE_CODE_BUILD_PATH"
        rm -rf $NATIVE_CODE_BUILD_PATH  
    fi

    mkdir $NATIVE_CODE_BUILD_PATH 2> /dev/null
    mkdir "$NATIVE_CODE_BUILD_PATH/osx-x64" 2> /dev/null

    cmake -S $SCRIPT_DIR/ZitiNativeApiForDotnetCore -B "$NATIVE_CODE_BUILD_PATH/osx-x64" -G "Ninja Multi-Config" -DCMAKE_INSTALL_INCLUDEDIR=include -DCMAKE_INSTALL_LIBDIR=lib $ZITI_SDK_C_BRANCH_CMD

    echo "Build from cmake using: "
    echo "    cmake --build $NATIVE_CODE_BUILD_PATH/osx-x64 --config Debug"
    # cmake --build "$NATIVE_CODE_BUILD_PATH/osx-x64" --config Debug
    echo "    cmake --build $NATIVE_CODE_BUILD_PATH/osx-x64 --config Release"
    cmake --build "$NATIVE_CODE_BUILD_PATH/osx-x64" --config Release
    echo " " 
    echo "Or open $NATIVE_CODE_BUILD_PATH/ziti-sdk.sln"

    echo "install nuget before executing the beloe commands"
    nuget pack $SCRIPT_DIR/native-package-local.nuspec -Version $NATIVE_LIB_VERSION -OutputDirectory $SCRIPT_DIR
    nuget push -source $NUGET_PATH $SCRIPT_DIR/Ziti.NET.Standard.native.$NATIVE_LIB_VERSION.nupkg

    echo "To generate new ziti.dll, run the below commands from microsoft developer command prompt and run the build.sh program with -n option again"
    echo "cd $SCRIPT_DIR\ZitiNativeApiForDotnetCore"
    echo "defgen 32 build-win\x86\_deps\ziti-sdk-c-build\library\Release\ziti.dll"
    echo "copy ziti.def library"
    echo "cl /C /EP /I build-win/x86/_deps/ziti-sdk-c-src/includes /c library/sharp-errors.c > library/ZitiStatus.cs"
    echo "copy library/ZitiStatus.cs ../Ziti.NET.Standard/src/OpenZiti"

fi

if [[ "$STANDARD_LIB" == "build" ]]; then
    echo "Building the solution for the Ziti.NET.standard library"

    if [[ "$CLEAN_BUILD" -eq "clean" ]]; then

        echo "clean up:  $SCRIPT_DIR/Ziti.NET.standard/bin"
        rm -rf $SCRIPT_DIR/Ziti.NET.standard/bin
        echo "clean up:  $SCRIPT_DIR/Ziti.NET.standard/out"
        rm -rf $SCRIPT_DIR/Ziti.NET.standard/out
        
    fi

    echo dotnet build $SCRIPT_DIR/Ziti.NuGet.sln /property:Configuration=Release /property:Platform=x64

    dotnet build $SCRIPT_DIR/Ziti.NuGet.sln /property:Configuration=Release /property:Platform=x64

    retval=$?
    if [[ $retval != 0 ]]; then
        echo " "
        echo "Build of $SCRIPT_DIR/Ziti.NuGet.sln for Platform=x64 failed"
        echo "Exiting..."
        exit $retval
    else
        echo " "
        echo "result of dotnet build for Platform=x64: $retval"
    fi

    echo "dotnet build $SCRIPT_DIR/Ziti.NuGet.sln /property:Configuration=Release"

    dotnet build $SCRIPT_DIR/Ziti.NuGet.sln /property:Configuration=Release

    retval=$?
    if [[ $retval != 0 ]]; then
        echo " "
        echo "Build of $SCRIPT_DIR/Ziti.NuGet.sln failed"
        echo "Exiting..."
        exit $retval
    else
        echo " "
        echo "result of msbuild: $retval"
    fi

    echo "dotnet pack $SCRIPT_DIR/Ziti.NuGet.sln --configuration Release --runtime osx-x64 -p:Platform=x64 --output $SCRIPT_DIR"

    dotnet pack $SCRIPT_DIR/Ziti.NuGet.sln --configuration Release --runtime osx-x64 -p:Platform=x64 --output $SCRIPT_DIR

    retval=$?
    if [[ $retval != 0 ]]; then
        echo " "
        echo "nuget pack for $BUILD_VERSION failed"
        echo "Exiting..."
        exit $retval
    else
        echo " "
        echo "result of dotnet pack: $retval"
    fi

    NUPKG_FILE=`ls -rt Ziti.NET.Standard.*.nupkg | tail -n 1`

    echo nuget push -source $NUGET_PATH $SCRIPT_DIR/$NUPKG_FILE

    nuget push -source $NUGET_PATH "$SCRIPT_DIR/$NUPKG_FILE"

    retval=$?
    if [[ $retval != 0 ]]; then
        echo " "
        echo "nuget push for $NUPKG_FILE failed"
        echo "Exiting..."
        exit $retval
    else
        echo " "
        echo "====================================================="
        echo "result of nuget push $NUPKG_FILE: $retval"
        echo "====================================================="
    fi
fi

if [[ "$SAMPLE_PROGRAMS" == "build" ]]; then
    echo "Building Samples solution"

    if [[ "$CLEAN_BUILD" -eq "clean" ]]; then

        echo "clean up:  $SCRIPT_DIR/Samples/bin"
        rm -rf $SCRIPT_DIR/Samples/bin
        echo "clean up:  $SCRIPT_DIR/Samples/out"
        rm -rf $SCRIPT_DIR/Samples/out

    fi

    echo "Build samples using dotnet build: "
    dotnet build $SCRIPT_DIR/Ziti.Samples.sln --configuration Release /p:Platform=x64
    dotnet build $SCRIPT_DIR/Ziti.Samples.sln --configuration Debug /p:Platform=x64
fi

echo " "
echo " "
echo "====================================================="
echo "	BUILD COMPLETE	:"
echo "====================================================="

echo "Normal Exit..."