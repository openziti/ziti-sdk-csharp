# Bucket list

Deferred follow-ups for the native nuget publish work (issue #84, branch
issue-84-publish-native-nuget-more-frequently). None of these block the initial publish unless marked BLOCKER.

## Blockers for a real (non-dryRun) publish

- **`mkdir` on existing `native/`** - `create-nuget-package` in `native-nuget-publish.yml` (~line 275) runs
  `mkdir ${{github.workspace}}/native`, which throws in pwsh because `native/` already exists after checkout.
  Fails every real publish at the pack stage. One-liner: `mkdir -Force`. Pre-existing, independent of the e2e
  work.

## From this session (e2e + C# review)

- **Apply csharp-expert #5** - free the `AllocHGlobal` reply buffer when `ziti_write` returns non-zero (the
  write callback won't fire). `native/e2e-app/Program.cs` `SendUnmanaged`. Cheap, real leak (harmless in a
  one-shot proc).
- **Apply csharp-expert #9** - the `ziti_config` blob is a guessed 8192 bytes with no bounds check; bump it +
  comment the validated SDK version, or add a `z4d_sizeof_ziti_config()` shim export and allocate exactly.
- **Optional hardening** - `z4d_sizeof_ziti_options()` startup assert (#1); stop using `Environment.Exit` from
  native callbacks (#8); type `OverlaySetup.FindAsync` instead of `dynamic` (#11, pre-existing). See
  `review-csharp-expert.md`.
- **Two e2e tests run per CI job** - `ProxyBridgeTest` (socket bridge) + `CallbackTrafficTest` (callback app)
  both run under `--filter TestCategory=e2e`, so each ziti-line x OS leg does two overlay setups + traffic
  runs. Decide if both should gate or one moves to a manual/optional category.
- **ROOT-CAUSE CONFIRMED + FIX: bridge dial fails on linux with the 1.16.0 native.** Proven empirically with
  C probes on the linux box: ziti-sdk-c 1.16.0 `zl_connect.c` `Ziti_connect` binds the loopback acceptor
  (`mk_acceptor`) but does NOT `listen()` before the in-thread `connect()`; the `listen()` is deferred to a uv
  worker that runs after the dial resolves, so Linux RSTs the SYN -> ECONNREFUSED (errno 111). Win/mac tolerate
  the bound-not-listening window. Caller socket mode (blocking vs non-blocking) is NOT the cause; neither fixes
  it. ziti controller version is NOT the cause (it's the 1.14.x bridge rearchitecture). THE FIX: `listen()` the
  acceptor before the in-thread connect in ziti-sdk-c `zl_connect.c` -- exactly the build-time patch archived at
  `C:\temp\csdk-debug\archive\mk_acceptor-listen-patch\`. File it upstream against ziti-sdk-c. The e2e gates on
  the callback path (`CallbackTrafficTest`), which doesn't use the bridge, so it's green; `ProxyBridgeTest`
  stays `[Ignore]`d until the SDK fix ships.
- **C regression test for the ziti-sdk-c bridge bug** - once the blocking-fix proof confirms it, write a
  standalone C test (addable to ziti-sdk-c, likely a Catch2 integration test or a `programs/` repro) that
  dials a hosted service via the zitilib socket bridge (`Ziti_connect`) with a NON-BLOCKING OS socket and
  asserts success. It FAILS on current 1.16.x on linux (EINPROGRESS from the caller-thread loopback connect
  not handled) and PASSES after the SDK handles EINPROGRESS. Include a blocking-socket case that passes, for
  contrast. This is the artifact the user wants to hand the C SDK team.

- **vcpkg cache keyed by baseline** - the build-cache release/tarballs are keyed by the ziti-sdk-c version as a
  proxy. The true key is the vcpkg baseline (+ triplet): versions that share a baseline could share the cache,
  and a baseline change without a version bump should invalidate it. Consider keying tarballs (or the release)
  on the `builtin-baseline` from vcpkg.json instead of the version. (Open question, in progress.)

- **END GOAL: one shared vcpkg binary cache hosted on the ziti-sdk-c project**, consumed by ALL of:
  ziti-sdk-c, ziti-tunnel-sdk-c, and this repo (ziti-sdk-csharp), to speed up every build. AND let developers
  pull from it locally on mac / linux / windows (anonymous read), so a dev doesn't rebuild openssl/protobuf/etc
  from scratch. Implies: the cache lives in/near ziti-sdk-c (since the baseline + deps originate there), keyed
  by the vcpkg baseline (+ triplet) so all three repos and all devs hit the same entries, with a documented
  local `VCPKG_BINARY_SOURCES` recipe per OS. This is the real target the per-repo cache work is building
  toward; the current per-repo `native-build-cache` release is a stepping stone.

## Deferred infra / housekeeping

- **ziti bug** - `ziti edge quickstart` on localhost 401s its own admin login when stale `~/.ziti` state exists
  (cached certs, prior v2 HA run). Fresh quickstart should work regardless of leftover local state. CI runners
  are clean so it doesn't affect CI.
- **setup-cli in CI e2e** - put ziti on PATH via `openziti/ziti/setup-cli@<tag>` (`version: 1.6.*` / `2.0.*`)
  and have `run-e2e-test.ps1` use whatever `ziti` is on PATH, falling back to getZiti only if none. (The CI
  e2e matrix already pins `v1.6.14` + `v2.0.0` via setup-cli; the getZiti fallback path in the script was
  broken on linux/macOS.)
- **Node 20 -> 24 actions** - `actions/checkout@v4`, `actions/cache@v4`, `actions/upload-artifact@v4`,
  `lukka/get-cmake`, `microsoft/setup-msbuild` still on Node 20. GitHub forces Node 24 ~2026-06-16, removes
  Node 20 ~2026-09-16. Just warnings for now.
- **Extract the vcpkg binary cache into a reusable composite action** (`openziti/setup-vcpkg-binary-cache@v1`)
  other openziti repos can consume: composite `action.yml` running the relocated cache script, exporting
  `VCPKG_BINARY_SOURCES`. Consumer keeps job-level `permissions: packages: write`.
- **Clint style guide / agent** - capture the writing + working preferences (terse, no preamble/filler, no
  em-dash or double-hyphen or semicolons in prose, one step at a time, ask before mutating git, raw git
  commands, no python).

## Done

- Layered vcpkg binary cache (Actions `files` then GitHub Packages `nuget` feed), PR #86, 2026-06-05.
