#library
set(ZITI_4_DOTNET_HEADERS
        ${PROJECT_SOURCE_DIR}/includes/ziti4dotnet.h
)
set(ZITI_4_DOTNET_SOURCE
        ${PROJECT_SOURCE_DIR}/library/ziti4dotnet.c
)

add_library(ziti4dotnet SHARED ${ZITI_4_DOTNET_SOURCE} ${ZITI_4_DOTNET_HEADERS} ziti.def )

target_include_directories(ziti4dotnet
        PUBLIC ${CMAKE_HOME_DIRECTORY}/includes
)

target_compile_definitions(ziti4dotnet
        PUBLIC
                BUILD_DATE=${BUILD_DATE}
                GIT_BRANCH=${GIT_BRANCH}
                GIT_COMMIT_HASH=${GIT_COMMIT_HASH}
        PRIVATE
                BUILDING_ZITI_SHARED=1
                GIT_VERSION=${GIT_VERSION}
                ZITI_LOG_PREFIX=${PROJECT_NAME}
)

set_property(TARGET ziti4dotnet PROPERTY C_STANDARD 11)

find_package(protobuf-c CONFIG REQUIRED)

# ziti is a STATIC lib that exports its public API via ZITI_FUNC (default visibility). Because it is static, the
# linker keeps only the object files the shim directly references and drops the rest, taking real exports with
# them. On linux/mac that silently dropped zl_connect.o, so Ziti_connect/Ziti_connect_addr were missing from the
# .so/.dylib and any P/Invoke to them failed (Windows kept them via ziti.def). WHOLE_ARCHIVE forces every ziti
# object into ziti4dotnet so the full public API is exported on non-Windows too. Windows keeps using ziti.def.
if (WIN32)
    target_link_libraries(ziti4dotnet PUBLIC ziti)
else()
    target_link_libraries(ziti4dotnet PUBLIC "$<LINK_LIBRARY:WHOLE_ARCHIVE,ziti>")
endif()
# USE WHEN DEBUGGING - target_link_options(ziti4dotnet PRIVATE "/VERBOSE:LIB")

add_executable(zitistructalignment ${ZITI_4_DOTNET_HEADERS} ${PROJECT_SOURCE_DIR}/library/zitistructalignment.c)

target_include_directories(zitistructalignment
        PUBLIC ${CMAKE_HOME_DIRECTORY}/includes
        )

set_property(TARGET zitistructalignment PROPERTY C_STANDARD 11)

target_link_libraries(zitistructalignment
        PUBLIC ziti4dotnet
        )
# USE WHEN DEBUGGING - target_link_options(zitistructalignment PRIVATE "/VERBOSE:LIB")
