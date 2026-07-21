# Changelog

All notable changes to the OpenZiti.NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

## [Unreleased]

### Changed
- Updated `OpenZiti.NET.native` dependency from 1.10.4.213 to 1.16.0.243 (ziti-sdk-c 1.16.0).
- **BREAKING (low-level interop only)**: re-laid the `OpenZiti.Native` model structs and event types to match the
  ziti-sdk-c 1.16 ABI. `model_number`-backed fields are now `long`, the event model is restructured (no leading
  event-type field on the union sub-structs; `ZitiMfaAuthEvent`→`ZitiAuthEvent`, `ZitiAPIEvent`→`ZitiConfigEvent`),
  and several structs gained/renamed fields. Apps using the modern `ZitiContext`/`ZitiSocket` surface are unaffected.
  See [MIGRATION.md](MIGRATION.md) for the full list.

### Added
- `API.AcceptAsync`, `API.ConnectAsync` and `API.ConnectByAddressAsync` (awaitable, `CancellationToken`-aware),
  plus an `Accept(socket, CancellationToken, out caller)` overload. Cancelling closes the listening socket to
  unblock the accept (like `TcpListener.Stop()`). New `ZitiSocket.Caller` exposes the dialing identity on
  accepted sockets. The bind/dial surface now supports both synchronous and `async`/`await` usage.

### Fixed
- Socket-bridge host/dial only worked on Windows: ziti-sdk-c leaves bridge fds non-blocking after
  `Ziti_bind`/`Ziti_connect`, so blocking `Accept` and `NetworkStream` I/O failed on linux/macOS. The SDK now
  forces the sockets it hands out to blocking, so sync and async accept/read/write work across platforms.
- `ZitifiedNetworkStream` now keys connect success/failure off the `Ziti_connect` return code instead of the
  process-global last error.
- `TestCSDKStructAlignments` now validates the full 1.16 struct layout on both x64 and x86.

## [1.0.0] - 2026-01-20

### Changed
- **BREAKING**: Upgraded target framework from .NET 6 to .NET 8
- Updated OpenZiti.NET.native dependency to 1.10.4.213 (stable release)
- Updated Microsoft.Extensions.Logging to 10.0.2
- Updated System.Text.Json to 10.0.2
- Updated NLog to 6.0.7

### Fixed
- Fixed nuspec file paths to match .NET 8 build output directories

## [0.9.x] - Previous Releases

### Added
- Kestrel Server Zitified sample (contributed by @natashell666)
- Appetizer reflect demo sample
- Support for loading identity from JSON (not just file)

### Changed
- Updated C SDK bindings to 1.10.x series
- Consolidated and cleaned up samples
- Updated native library build process with improved vcpkg integration

### Fixed
- Various sample fixes for compatibility with C SDK 1.x
- Fixed appetizer sample issues

## [0.8.x and Earlier]

Initial development releases providing:
- Core OpenZiti .NET SDK functionality
- P/Invoke bindings to the OpenZiti C SDK
- Cross-platform support (Windows x64/x86, macOS x64/arm64, Linux x64/arm64/arm)
- Sample applications demonstrating SDK usage
- Integration with ASP.NET Core and Kestrel
