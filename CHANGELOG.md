# Changelog

All notable changes to the OpenZiti.NET SDK will be documented in this file.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/),
and this project adheres to [Semantic Versioning](https://semver.org/spec/v2.0.0.html).

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
