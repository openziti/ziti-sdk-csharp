cmake_minimum_required(VERSION 3.14)
include(git.cmake)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_C_STANDARD 99)
set( CMAKE_SUPPORT_WINDOWS_EXPORT_ALL_SYMBOLS 1 )
set(WINDOWS_EXPORT_ALL_SYMBOLS ON)
set(USING_ZITI_SHARED ON)
set(BUILDING_ZITI_SHARED ON)
set(ZITI_SDK_C_BRANCH_DEFAULT "0.34.1")

if ("$ENV{ZITI_SDK_C_BRANCH}" STREQUAL "")
    #allow using a different branch of the CSDK easily
    set(ZITI_SDK_C_BRANCH "${ZITI_SDK_C_BRANCH_DEFAULT}")
    message("using default ZITI_SDK_C_BRANCH: ${ZITI_SDK_C_BRANCH}")
else()
    set(ZITI_SDK_C_BRANCH "$ENV{ZITI_SDK_C_BRANCH}")
endif()

message("CSDK SELECTED: ${ZITI_SDK_C_BRANCH}")

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
#add_subdirectory(programs)

string(TOLOWER "${GENERATE_ZITI_STATUS}" generate_csharp_enum)
    FetchContent_MakeAvailable(ziti-sdk-c)
if("${generate_csharp_enum}" STREQUAL "yes")
    set(SourceErrorsFile "${ziti-sdk-c_SOURCE_DIR}/includes/ziti/errors.h")
    set(OUTPUT_FILE "${CMAKE_CURRENT_SOURCE_DIR}/../OpenZiti.NET/src/OpenZiti/ZitiStatus.cs")
    execute_process(
        COMMAND powershell -File "${CMAKE_CURRENT_SOURCE_DIR}/generateDotnetStatus.ps1" -SourceFile "${SourceErrorsFile}" -OutputPath ${OUTPUT_FILE}
    )
    message("sharp-errors output to: ${OUTPUT_FILE}")
endif()

include(FetchContent)
FetchContent_Declare(
        ziti-sdk-c-archive
        URL "https://github.com/openziti/ziti-sdk-c/releases/download/${ZITI_SDK_C_BRANCH}/ziti-sdk-${ZITI_SDK_C_BRANCH}-Windows-AMD64.zip"
        DOWNLOAD_NO_EXTRACT false
        DOWNLOAD_EXTRACT_TIMESTAMP true
)
FetchContent_MakeAvailable(ziti-sdk-c-archive)

message("Generating .def file from downloaded archive at: ${ziti-sdk-c-archive_SOURCE_DIR}/lib/ziti.dll")

# Specify the path to your .bat file
set(batch_file_path "defgen.bat")

execute_process(
        COMMAND "defgen.bat" "64" "${ziti-sdk-c-archive_SOURCE_DIR}/lib/ziti.dll"
        WORKING_DIRECTORY ${CMAKE_CURRENT_SOURCE_DIR}
        RESULT_VARIABLE result
)