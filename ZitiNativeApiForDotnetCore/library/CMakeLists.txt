#library
set(ZITI_4_DOTNET_HEADERS
        ${PROJECT_SOURCE_DIR}/includes/ziti4dotnet.h
)
set(ZITI_4_DOTNET_SOURCE
        ${PROJECT_SOURCE_DIR}/library/ziti4dotnet.c
)

add_library(ziti4dotnet SHARED ${ZITI_4_DOTNET_SOURCE} ${ZITI_4_DOTNET_HEADERS} )

target_include_directories(ziti4dotnet
        PUBLIC ${CMAKE_HOME_DIRECTORY}/includes
)

target_compile_definitions(ziti4dotnet PUBLIC
        BUILD_DATE=${BUILD_DATE}
        GIT_BRANCH=${GIT_BRANCH}
        GIT_COMMIT_HASH=${GIT_COMMIT_HASH}
        PRIVATE BUILDING_ZITI_SHARED=1
)

set_property(TARGET ziti4dotnet PROPERTY C_STANDARD 11)

target_compile_definitions(ziti4dotnet
        PRIVATE GIT_VERSION=${GIT_VERSION}
        PRIVATE ZITI_LOG_PREFIX=${PROJECT_NAME}
        )

target_link_libraries(ziti4dotnet
        PUBLIC ziti
        )