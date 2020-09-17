#project root
cmake_minimum_required(VERSION 3.12)
include(git.cmake)
set(CMAKE_POSITION_INDEPENDENT_CODE ON)
set(CMAKE_C_STANDARD 99)

project(ziti4dotnet
        VERSION ${ver}
        LANGUAGES C CXX
)

add_subdirectory(cmake_deps) 

set(ZITI_4_DOTNET_HEADERS
        ${PROJECT_SOURCE_DIR}/includes/ziti4dotnet.h
        ${PROJECT_SOURCE_DIR}/ziti.def
)
set(ZITI_4_DOTNET_SOURCE
        ${PROJECT_SOURCE_DIR}/library/ziti4dotnet.c
)

add_library(ziti4dotnet SHARED ${ZITI_4_DOTNET_SOURCE} ${ZITI_4_DOTNET_HEADERS} )

get_cmake_property(_variableNames VARIABLES)
list (SORT _variableNames)
foreach (_variableName ${_variableNames})
    message(STATUS "CMAKE_ENV: ${_variableName}=${${_variableName}}")
endforeach()

target_include_directories(ziti4dotnet
        PUBLIC includes
        PUBLIC ${CMAKE_BINARY_DIR}/_deps/ziti-sdk-c-src/programs/ziti-prox-c/win32/include
        PUBLIC ${CMAKE_BINARY_DIR}/_deps/ziti-sdk-c-src/inc_internal
        PUBLIC ${CMAKE_BINARY_DIR}/_deps/subcommand-src
)

target_compile_definitions(ziti4dotnet PUBLIC
        BUILD_DATE=${BUILD_DATE}
        GIT_BRANCH=${GIT_BRANCH}
        GIT_COMMIT_HASH=${GIT_COMMIT_HASH}
        PRIVATE BUILDING_ZITI_SHAREDaa=1
)

set_property(TARGET ziti4dotnet PROPERTY C_STANDARD 11)

target_compile_definitions(ziti4dotnet
        PRIVATE GIT_VERSION=${GIT_VERSION}
        PRIVATE ZITI_LOG_PREFIX=${PROJECT_NAME}
)

target_link_libraries(ziti4dotnet
        PUBLIC ziti
)

execute_process(
    COMMAND cl.exe /C /EP /I ${CMAKE_BINARY_DIR}/_deps/ziti-sdk-c-src/includes  ${PROJECT_SOURCE_DIR}/library/sharp-errors.c
    COMMAND_ECHO STDOUT
    OUTPUT_FILE ${PROJECT_SOURCE_DIR}/../DotnetCoreConsole1/OpenZiti/ZitiStatus.cs
)

#C:\git\github\openziti\ziti-sdk-csharp\DotnetCoreConsole1\DotnetCoreConsole1.csproj
# cl.exe /C /EP /I build\x64\_deps\ziti-sdk-c-src\includes library\sharp-errors.c > ../DotnetCoreConsole1/OpenZiti/ZitiStatus.cs
