cmake_minimum_required(VERSION 3.14)
include(git.cmake)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_C_STANDARD 99)

if(NOT ZITI_SDK_C_BRANCH)
    #allow using a different branch of the CSDK easily
    set(ZITI_SDK_C_BRANCH "0.22.0")
endif()

file (STRINGS "../version" BUILD_NUMBER)

project(ziti4dotnet VERSION ${BUILD_NUMBER} LANGUAGES C CXX)

message("project version: ${PROJECT_VERSION} (BUILD_NUMBER ${BUILD_NUMBER})")
message("cross-compiling: ${CMAKE_CROSSCOMPILING}")

#uncomment to see cmake variables
#get_cmake_property(_variableNames VARIABLES)
#list (SORT _variableNames)
#foreach (_variableName ${_variableNames})
#    message(STATUS "CMAKE_ENV: ${_variableName}=${${_variableName}}")
#endforeach()

add_subdirectory(cmake_deps)
add_subdirectory(library)

#generate the .cs file that represents the c sdk errors using the preprocessor
set(OUTPUT_FILE ${CMAKE_HOME_DIRECTORY}/../Ziti.NET.Standard/src/OpenZiti/ZitiStatus.cs)
set(ZITI_INCLUDES ${CMAKE_HOME_DIRECTORY}/build/x64/_deps/ziti-sdk-c-src/includes)
set(ZITI_ERR_FILE ${CMAKE_HOME_DIRECTORY}/library/sharp-errors.c)
if (CMAKE_CXX_COMPILER_ID STREQUAL "MSVC")
    execute_process(
            COMMAND cl.exe /C /EP /I ${ZITI_INCLUDES} ${ZITI_ERR_FILE}
            COMMAND_ECHO STDOUT
            OUTPUT_FILE ${OUTPUT_FILE}
    )
else()
    execute_process(
            COMMAND gcc -nostdinc -E -CC -P -I${ZITI_INCLUDES} ${ZITI_ERR_FILE}
            COMMAND_ECHO STDOUT
            OUTPUT_FILE ${OUTPUT_FILE}
    )
endif()
