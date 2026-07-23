# Changelog — OpenZiti.NET.native

All notable changes to the `OpenZiti.NET.native` package are documented here. This package is the native
`ziti4dotnet` shim over [ziti-sdk-c](https://github.com/openziti/ziti-sdk-c), packaged per RID; the managed
`OpenZiti.NET` package P/Invokes into it.

Its version is `<ziti-sdk-c version>.<revision>`: the first three parts are the exact ziti-sdk-c release it wraps,
and the 4th is a manual build revision (bumped for a rebuild of the same ziti-sdk-c version, e.g. a shim change).
Sections here are keyed by the full package version.

The format is based on [Keep a Changelog](https://keepachangelog.com/en/1.1.0/).

## [Unreleased]

## [1.18.2.50]

Built from [ziti-sdk-c 1.18.2](https://github.com/openziti/ziti-sdk-c/releases/tag/1.18.2) — the native binaries
are identical to 1.18.2.49. It is versioned 1.18.2.50 only to exercise the new native release workflow end-to-end:
the manual "run workflow" publish button, the smoke + e2e gates, and the resulting git tag + GitHub Release with
these notes. It is not a new ziti-sdk-c release and has no functional change over 1.18.2.49.

## [1.18.2.49]

Wraps [ziti-sdk-c 1.18.2](https://github.com/openziti/ziti-sdk-c/releases/tag/1.18.2).

### Changed
- Upstream ziti-sdk-c 1.18.2: better auth diagnostics in `ziti_dump`, a fix preventing an incoming
  close/disconnect from racing a local close, and edge-router capabilities now set before the connected event
  fires.

### Known issues
- On linux and macOS the zitilib socket bridge leaves bridged fds non-blocking after `Ziti_bind`/`Ziti_connect`;
  callers that do blocking accept/read must force the fd blocking themselves. The managed `OpenZiti.NET` SDK works
  around this in its `Bind`/`Accept`/`Connect` path. Windows is unaffected.

## [1.16.0.245]

Wraps [ziti-sdk-c 1.16.0](https://github.com/openziti/ziti-sdk-c/releases/tag/1.16.0).

### Changed
- First native package built against the ziti-sdk-c 1.16 ABI. `ziti4dotnet` exports `z4d_layout_report()` so the
  managed alignment harness can verify struct layout against the C compiler at test time.
