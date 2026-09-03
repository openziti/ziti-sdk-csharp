# build-iphoneos-arm64
#
# Overrides the C SDK's toolchains/iOS-arm64.cmake, which is otherwise fetched. Deliberately does NOT export
# CFLAGS/LDFLAGS: set(ENV{...}) mutates the CMake process environment, the vcpkg child inherits it, and the
# try-compile that detects the HOST compiler (arm64-osx on a macOS runner) then links a macOS object against
# the iPhoneOS sysroot and dies before any port builds:
#
#   ld: building for 'iOS', but linking in object file (...) built for 'macOS'

set(CMAKE_SYSTEM_NAME iOS)
set(CMAKE_SYSTEM_PROCESSOR arm64)

SET(CMAKE_C_FLAGS "${CMAKE_C_FLAGS} -arch arm64")
SET(CMAKE_CXX_FLAGS "${CMAKE_CXX_FLAGS} -arch arm64")

# for libsodium
set(triple arm-apple-darwin10)
execute_process(COMMAND /usr/bin/xcrun -sdk iphoneos --show-sdk-path
                OUTPUT_VARIABLE CMAKE_OSX_SYSROOT
                OUTPUT_STRIP_TRAILING_WHITESPACE)

set(CMAKE_FIND_ROOT_PATH_MODE_PROGRAM NEVER)
set(CMAKE_FIND_ROOT_PATH_MODE_LIBRARY ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_INCLUDE ONLY)
set(CMAKE_FIND_ROOT_PATH_MODE_PACKAGE ONLY)
