# csharp-expert review: native/e2e-app + e2e harness

Captured from the csharp-expert agent. Scope: the raw P/Invoke e2e (native/e2e-app/Program.cs,
native/e2e/ZitiProgram.cs, CallbackTrafficTest.cs, OverlaySetup.cs). My triage notes are in the last section.

---

## CRITICAL

**1. `Program.cs:ZitiOptions` struct layout is almost certainly wrong on Windows**

`ZitiOptions` has `byte disabled` as the first field. The real `ziti_options` almost certainly has `bool`
(`int`) or has padding/alignment before the first pointer-width field. A single `byte` followed by `IntPtr
config_types` will have 7 bytes of padding on 64-bit (both platforms), making the managed struct match the C
layout only if the C struct opens with a single `uint8_t` and the compiler adds that same padding. That is
likely fine. The bigger issue: you are not verifying field count or overall size against the actual header. If
`ziti_options` in the C SDK gains or reorders fields between versions, this struct silently misaligns and
`event_cb` points at garbage -- a callback-into-garbage crash, hard to diagnose.

Fix: add a size assertion at startup (a `z4d_sizeof_ziti_options()` shim export asserted at startup), or at
minimum document the expected `sizeof(ziti_options)`.

**2. `Program.cs:246` -- `Marshal.Copy(data, buf, 0, (int)n)` truncates on large payloads**

`n` is `ssize_t` (`IntPtr`-sized). On 64-bit `n` can exceed `int.MaxValue` before the cast; `(int)n` wraps
negative and `Marshal.Copy` throws, or silently truncates. Use checked arithmetic or cap with an assertion.

**3. `Program.cs:43` -- `ClientCb` `IntPtr clientCtx` vs C `ziti_client_ctx *`**

Correct IF the callback receives the pointer by value and you never dereference it (you don't). Document the
assumption.

**4. `Program.cs:167-168` -- `event_cb` stored as raw `IntPtr`; delegate lifetime**

Done correctly (`_eventCb` static). But `opts` is a stack local. If `ziti_context_set_options` keeps `&opts`
rather than copying, it dangles. Verify; if uncertain, promote `opts` to a static.

---

## SHOULD-FIX

**5. `OnServerData`/`SendUnmanaged` -- `AllocHGlobal` not freed on the `ziti_write` error path**

If `ziti_write` returns non-zero, `OnWrite` never fires and the buffer leaks. Fix:
```csharp
int rc = ziti_write(conn, buf, (IntPtr)bytes.Length, _onWrite, buf);
if (rc != ZITI_OK) Marshal.FreeHGlobal(buf);
```

**6. `OnWrite` -- negative `ssize_t` status via `ToInt64()` loses sign on 32-bit**

`status.ToInt64()` zero-extends on 32-bit IntPtr. Use `(long)(nint)status` (sign-extends on both widths).

**7. `OnDialData` -- `ziti_shutdown` called inside a native data callback**

Not documented as re-entrant-safe; sample-dial.c uses `uv_stop`. Calling it from the loop thread could
deadlock/corrupt loop state. Mirror sample-dial.c exactly (uv_async_send or uv_stop).

**8. `Environment.Exit` inside `Die()` from native callbacks**

Tears down the CLR (frees pinned handles) while the native loop thread may still call back -- use-after-free /
non-deterministic signal exit on Linux that CI may read as a hang. Prefer flag + drain + non-zero `Main`
return.

**9. `_cfgPin` 8192-byte blob may be insufficient**

`ziti_load_config` writes into the pinned buffer with no bounds check; if `sizeof(ziti_config)` exceeds 8192
it corrupts the GC heap. Expose `z4d_sizeof_ziti_config()` and allocate that, or document the validated size +
SDK version.

**10. `ZitiProgram.RunAsync` -- stdout/stderr pipe**

`BeginOutputReadLine`/`BeginErrorReadLine` before `WaitForExitAsync` is the correct no-deadlock pattern. The
`sb` can be appended after the method returns (post-`Kill`); benign now, latent race if extended.

**11. `OverlaySetup.FindAsync` -- `dynamic` dispatch**

Runtime duck-typing against NSwag-generated types; fails at runtime with no compile signal if the shape
changes. Fragile for a publish gate. Type it properly.

---

## NIT

12. `DataCb` returns `IntPtr` (ssize_t) -- returning 0 on EOF/error is fine since the C side checks the
    return only when `n > 0`. Comment it.
13. `ZITI_LOG` env parse fallback is silent (`int.TryParse`); warn on garbage.
14. `_onConnect` shared across server/client roles via `_server`; safe (set before any callback) but fragile;
    consider two statics.
15. `e2e-app.csproj` has `Nullable disable`; missed safety net for new code.
16. `OverlaySetup.cs:147` uses `Encoding.ASCII` for the JWT; ASCII-only today, `Encoding.UTF8` is safer.

---

## What actually matters for CI correctness (expert's summary)

- Item 7 (`ziti_shutdown` from a data callback): most likely to hang/crash on Linux/mac.
- Item 8 (`Environment.Exit` from a native callback): most likely to produce a signal exit on Linux that CI
  reports as a crash rather than a clean failure.

---

## Triage (mine, against the actual headers + the ported sample)

- **#7 is WRONG.** `sample-dial.c`'s `on_data` calls `ziti_close` + `ziti_shutdown` exactly like ours; it's
  the sanctioned pattern and it passed on win + linux. Not a bug.
- **#6 moot:** the e2e matrix is x64-only (win-x64/linux-x64/osx-arm64).
- **#3, #10 are fine as written** (expert concurred).
- **#1/#4:** the struct is derived from the real 1.16.0 `ziti.h` and verified running on all three OSes;
  `opts` stays valid because `Main` blocks in `z4d_uv_run` for the whole process. Optional hardening only.
- **Worth fixing:** #5 (free on `ziti_write` failure) and #9 (cfg buffer size / document).
- **Pre-existing, not added here:** #11 (`OverlaySetup` `dynamic`).
