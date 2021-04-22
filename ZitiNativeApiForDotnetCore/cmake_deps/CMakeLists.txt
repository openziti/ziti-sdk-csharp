include(FetchContent)

FetchContent_Declare(ziti-sdk-c
        GIT_REPOSITORY https://github.com/openziti/ziti-sdk-c.git
        GIT_TAG ${ZITI_SDK_C_BRANCH}
        )
set(ZITI_BUILD_TESTS off)
set(ZITI_BUILD_PROGRAMS off)
set(BUILDING_ZITI_SHARED 1)
FetchContent_MakeAvailable(ziti-sdk-c)

