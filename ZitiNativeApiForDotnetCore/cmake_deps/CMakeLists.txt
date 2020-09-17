#cmake_deps
project(ziti-deps)

include(FetchContent)

FetchContent_Declare(ziti-sdk-c
        GIT_REPOSITORY https://github.com/openziti/ziti-sdk-c.git
        GIT_TAG 0.15.13
        )
FetchContent_MakeAvailable(ziti-sdk-c)

add_library(ziti-sdk-c INTERFACE)
target_include_directories(ziti-sdk-c INTERFACE ${ziti-sdk-c_SOURCE_DIR})
