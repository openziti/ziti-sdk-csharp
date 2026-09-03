# Overlay of vcpkg's community arm64-ios triplet, adding an autotools host triple.
#
# vcpkg_make gives iOS the same configure triple as macOS (vcpkg_make.cmake: "${TARGET_ARCH}-apple-darwin" for
# VCPKG_TARGET_IS_IOS OR VCPKG_TARGET_IS_OSX). Building arm64-ios on an arm64 macOS runner then yields
# --host equal to --build, autoconf decides the build is native, and it runs an iOS test binary on the host:
#
#   configure: error: cannot run C compiled programs.
#
# Naming the host triple restores the mismatch that tells autoconf to cross-compile. arm-apple-darwin10 is the
# triple the C SDK's own iOS toolchain declares for libsodium.

set(VCPKG_TARGET_ARCHITECTURE arm64)
set(VCPKG_CRT_LINKAGE dynamic)
set(VCPKG_LIBRARY_LINKAGE static)
set(VCPKG_CMAKE_SYSTEM_NAME iOS)

set(VCPKG_MAKE_BUILD_TRIPLET "--host=arm-apple-darwin10")
