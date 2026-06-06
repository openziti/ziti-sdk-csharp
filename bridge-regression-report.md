# ziti-sdk-c bridge dial regression: report + matrix + C test

## Question

The .NET e2e `ProxyBridgeTest` (zitilib socket bridge: `Ziti_connect`) fails on linux but not win/mac. Is the
cause the **ziti controller/CLI version** (1.6 vs 2.0) or the **ziti-sdk-c native version** (1.10 vs 1.16)?

## Answer

The **ziti-sdk-c native version**. It is a regression introduced in **1.12.0** (commit `46c1b8e`,
"zitilib: non-blocking connect support with IPv6", 2026-03-24), still present in the shipping **1.16.0**. The
ziti controller version is irrelevant. The bug is linux-only and is a missing `listen()` in the bridge's
loopback acceptor.

## Root cause (confirmed)

`library/zitilib/zl_connect.c`:

- `mk_acceptor()` does `socket()` + `bind()` + `getsockname()`, sets the socket non-blocking, and returns. It
  does **not** call `listen()`.
- `Ziti_connect()` schedules the ziti dial on the loop **without awaiting it**, then immediately does
  `connect(app_fd, acceptor)` on the calling thread.
- `listen()` happens later, in `connect_work()` on a uv worker thread that only runs **after** the ziti dial
  resolves over the network (hundreds of ms later).

So the loopback `connect()` targets a bound-but-not-listening port. The caller socket is **non-blocking** (the
.NET side sets it so), so `connect()` returns EINPROGRESS/EWOULDBLOCK and the SYN is sent. What happens next is
OS-specific:

- **linux** RSTs a SYN to a bound-not-listening loopback port immediately, so `SO_ERROR` becomes
  `ECONNREFUSED (111)` before the deferred `listen()` runs. Dial fails.
- **macOS / windows** hold the pending SYN (no RST); when `listen()` lands a moment later the connection
  completes. Dial succeeds.

Pre-1.12.0 the bridge ran the whole connect on the loop and awaited it (`schedule_on_loop(..., true)`), with no
caller-thread connect racing a later `listen()`, so there was no bug.

The misleading `"configuration not found"` text is a red herring: `Ziti_connect` returns `-1` on failure, and
`ziti_errorstr(-1)` happens to be `ZITI_CONFIG_NOT_FOUND`. The real error is in `Ziti_last_error()`, which the
.NET helper did not print.

## The fix (one line, upstream in ziti-sdk-c)

In `mk_acceptor()`, `listen()` the acceptor before returning, so it is listening before the caller-thread
connect races it:

```c
    if (s < 0 ||
        bind(s, (const struct sockaddr *)addr, addr_len) != 0 ||
        getsockname(s, (struct sockaddr *)addr, len) != 0 ||
        listen(s, 1) != 0 ) {          /* ADD THIS */
        ...
    }
```

The later `listen()` in `connect_work()` becomes redundant. This is the same change as the build-time patch
archived at `C:\temp\csdk-debug\archive\mk_acceptor-listen-patch\`.

## Version matrix (which ziti-sdk-c releases are affected)

| release | date | bridge dial on linux | note |
| --- | --- | --- | --- |
| 1.10.4 | 2025-12-26 | works | loop-side connect, awaited |
| 1.11.7 | 2026-03-16 | works | last good (mainline) |
| 1.11.8 / 1.11.9 | 2026-04 | works | 1.11 maintenance line, not affected |
| **1.12.0** | 2026-03-25 | **broken** | first broken; commit `46c1b8e`, `mk_acceptor` no `listen()` |
| 1.13.0 .. 1.16.0 | 2026-03..04 | broken | 1.16.0 is what this package ships today |
| **1.17.0** | 2026-05-29 | **fixed** | acceptor now `bind()`+`listen()`s together before the connect |

Confirmed by reading `zl_connect.c` at tags 1.11.7 (loop-side awaited connect, no `mk_acceptor`), 1.12.0 and
1.16.0 (`mk_acceptor` binds without `listen()`), and 1.17.0 (acceptor `bind()`+`listen()` together). Confirmed
empirically too: the same `ziti-socket-dial.c`, dialing one hosted service on one linux box, linked against
1.11.7 PASSES, 1.12.0 FAILS, 1.17.0 PASSES (only the linked `libziti.so` differs, per `ldd`).

## Exact commits / PRs

- Bug introduced: commit `46c1b8e` ("zitilib: non-blocking connect support with IPv6", 2026-03-24), first
  released in 1.12.0.
- Fixed by: PR #1047 "zitilib: better async support" (commit `02fc5b6e71`, merged 2026-05-11), first released
  in 1.17.0. It removed `mk_acceptor()` (bind without listen) and added `setup_bridge_socket()` that does
  `bind()` + `listen(srv_fd, 1)` together, called synchronously before the connect, and removed the old
  deferred `listen(req->accept_fd, 1)`. Verified in the commit diff.
- Test added: the SAME PR #1047 (commit `5977490cb9` "add tests sync/async tests for Ziti_connect()"), later
  reorganized into `tests/integ/zl_connect_tests.cpp` on 2026-05-29 (commit `57c4c30fbf`).

## Why ziti-sdk-c's own tests did not catch it

The fix and its test shipped in the SAME PR (#1047), so the test was born testing already-fixed code and never
saw red. Their integration test for this exact path (`tests/integ/zl_connect_tests.cpp` -> `"zitilib: connect
service"`, `[zl-connect]`, dials a hosted service via `Ziti_connect` blocking + async) and the integration-test
CI workflow (added 2026-06-01) both postdate the entire 1.12.0..1.16.0 broken window. Their integ CI tests
HEAD's SDK (the matrix varies the controller version, not the SDK version), and there is no "test old SDK
releases" regression matrix, so the regression in shipped releases 1.12.0..1.16.0 never had a chance to fail a
test.

## Recommendation

The upstream fix is already in 1.17.0, so do not carry the downstream patch. Move the native build from 1.16.0
to 1.17.0 (the publish flow takes the ziti-sdk-c version as an input; regenerate `library/ziti.def` for 1.17.0
per the repo pitfalls and rebuild), then remove the `[Ignore]` on `ProxyBridgeTest`. Until then, ship 1.16.0
with the callback gate green and the bridge test `[Ignore]`d.

## Real e2e matrix (CI run 27052892794, native 1.16.0)

`ProxyBridgeTest` (socket bridge) and `CallbackTrafficTest` (ziti_dial callback app) under
`--filter TestCategory=e2e`, each OS x both ziti lines:

| native | OS | ziti v1.6.14 | ziti v2.0.0 |
| --- | --- | --- | --- |
| 1.16.0 | win-x64 | both pass | both pass |
| 1.16.0 | osx-arm64 | both pass | both pass |
| 1.16.0 | linux-x64 | **ProxyBridgeTest FAILS**, CallbackTrafficTest passes | same |

The ziti line (1.6 vs 2.0) makes no difference, confirming it is not the controller version. The callback path
(`ziti_dial`, not the socket bridge) passes everywhere, which is why it is the gate.

## Socket-mode + cross-OS isolation (faithful race probe)

`race-test.c` (in `C:\temp\csdk-debug\`) reproduces the exact sequence with no overlay: bind a loopback
acceptor, set non-blocking, non-blocking `connect()` from a second socket, `listen()` 200ms late (the
"broken" deferred-listen case) vs `listen()` before connect (the "fixed" case). Built and run natively on each
OS:

| sequence | linux-x64 | win-x64 | osx-arm64 |
| --- | --- | --- | --- |
| BROKEN (listen deferred 200ms) | FAILS, SO_ERROR=111 | connects | connects |
| FIXED (listen before connect) | connects | connects | connects |

connect() immediate errno on the broken case: linux EINPROGRESS(115) then RST to SO_ERROR=111; win
WSAEWOULDBLOCK(10035); mac EINPROGRESS(36). Only linux RSTs the pending SYN before the late listen.

Blocking vs non-blocking, both tested:

- **non-blocking** (what the real bridge / .NET helper uses): linux fails, win/mac pass (table above). This is
  the production behavior.
- **blocking**: a blocking `connect()` to a bound-not-listening loopback port is refused immediately on linux
  AND windows (ECONNREFUSED / WSAECONNREFUSED 10061) and times out on macOS. So forcing the caller socket
  blocking does NOT fix linux and would break windows. Socket mode is not the fix; the missing `listen()` is.

(An earlier source-only hypothesis blamed the non-blocking socket mode and proposed forcing blocking. Running
it disproved that: blocking still fails on linux. Always prove by running.)

## C regression test for ziti-sdk-c

`C:\temp\csdk-debug\race-test.c` is the faithful, deterministic, no-overlay reproduction: it shows the broken
deferred-listen sequence fails on linux and the listen-before-connect fix succeeds, on every OS. It is the
artifact to hand the C SDK team (drop into their suite; the core is portable C, wrap in Catch2 if desired).

(`C:\temp\csdk-debug\mk-acceptor-listen-test.c` is an earlier, simpler variant but it used a blocking connect
and mislabeled win/mac as "tolerant", which the data above disproves. Prefer `race-test.c`.)

The strongest test for their suite is also an integration test that calls the real `Ziti_bind` / `Ziti_listen`
/ `Ziti_connect` against a quickstart and asserts the dial succeeds: it fails on 1.12.0..1.16.0 on linux and
passes after the `listen()` fix. The .NET `ProxyBridgeTest` already is exactly that and reproduces it in CI.

## What we shipped in this repo

The e2e gates on `CallbackTrafficTest` (the `ziti_dial` callback app, green on all 6 legs).
`ProxyBridgeTest` (the socket bridge) is `[Ignore]`d with the CI evidence in its reason, kept as the bridge
sample + linux repro, until the ziti-sdk-c `listen()` fix ships.
