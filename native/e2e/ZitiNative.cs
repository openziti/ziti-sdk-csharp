/*
Copyright NetFoundry Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading;

namespace E2ETest;

// Raw P/Invoke into ziti4dotnet's zitilib socket API, mirroring the C SDK's own samples
// (programs/zitilib-samples/server.c and ziti-http-get.c) call-for-call: a PLAIN OS socket is created with
// socket(AF_INET, SOCK_STREAM), handed to Ziti_bind / Ziti_connect, and then read/written like any socket.
// The C samples deliberately use a plain OS socket (the ziti-http-get sample even comments out Ziti_socket);
// using the SDK's own ZitiSocket wrapper for the dial path did not work on linux/mac.
//
// ziti_socket_t is an int fd on posix and a pointer-sized SOCKET on Windows, so it is marshalled as nint.
// ziti_handle_t (the context) is a pointer-sized handle, also nint.
internal static class ZitiNative
{
    private const string Dll = "ziti4dotnet";

    // ---- zitilib --------------------------------------------------------------------------------------
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_lib_init();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_lib_shutdown();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_load_context(out nint ztx, [MarshalAs(UnmanagedType.LPUTF8Str)] string identity);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_bind(nint socket, nint ztx,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string service, [MarshalAs(UnmanagedType.LPUTF8Str)] string terminator);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_listen(nint socket, int backlog);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern nint Ziti_accept(nint socket, byte[] caller, int callerLen);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl, SetLastError = true)]
    private static extern int Ziti_connect(nint socket, nint ztx,
        [MarshalAs(UnmanagedType.LPUTF8Str)] string service, [MarshalAs(UnmanagedType.LPUTF8Str)] string terminator);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_close(nint socket);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int Ziti_last_error();

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern nint ziti_errorstr(int err);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ziti_log_set_level(int level, [MarshalAs(UnmanagedType.LPUTF8Str)] string marker);

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ziti_log_set_logger(nint logger);

    // ---- plain OS socket ------------------------------------------------------------------------------
    private const int AF_INET = 2;
    private const int SOCK_STREAM = 1;

    [DllImport("libc", EntryPoint = "socket", SetLastError = true)]
    private static extern int posix_socket(int domain, int type, int protocol);

    [DllImport("ws2_32.dll", EntryPoint = "socket", SetLastError = true)]
    private static extern nint win_socket(int af, int type, int protocol);

    [DllImport("libc", SetLastError = true)]
    private static extern int fcntl(int fd, int cmd, int arg);

    [DllImport("ws2_32.dll", SetLastError = true)]
    private static extern int ioctlsocket(nint s, int cmd, ref uint argp);

    private const int F_GETFL = 3;
    private const int F_SETFL = 4;

    // Force a socket into blocking mode. ziti's zl_is_blocking() is (fcntl(F_GETFL) & O_NONBLOCK)==0 on posix;
    // when the server socket is blocking, Ziti_accept waits and the SDK hands a dialing client off directly
    // instead of dropping it into a backlog. O_NONBLOCK differs by platform (macOS 0x4, linux 0x800).
    private static void SetBlocking(nint fd)
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            uint blocking = 0; // FIONBIO arg 0 => blocking
            ioctlsocket(fd, unchecked((int)0x8004667E), ref blocking);
            return;
        }
        int oNonBlock = RuntimeInformation.IsOSPlatform(OSPlatform.OSX) ? 0x0004 : 0x0800;
        int flags = fcntl((int)fd, F_GETFL, 0);
        if (flags >= 0) fcntl((int)fd, F_SETFL, flags & ~oNonBlock);
    }

    private static nint NewOsSocket()
    {
        if (RuntimeInformation.IsOSPlatform(OSPlatform.Windows))
        {
            var s = win_socket(AF_INET, SOCK_STREAM, 0);
            if (s == -1) throw new InvalidOperationException("socket() failed (win)");
            return s;
        }
        var fd = posix_socket(AF_INET, SOCK_STREAM, 0);
        if (fd < 0) throw new InvalidOperationException($"socket() failed (posix), errno={Marshal.GetLastWin32Error()}");
        return fd;
    }

    // ---- logging --------------------------------------------------------------------------------------
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    private delegate void LogWriter(int level, nint loc, nint msg, nuint msglen);

    // Kept alive for the process so the native side can call back.
    private static LogWriter _logWriter;
    private static readonly object _logLock = new();

    private static readonly string NativeLogPath = Environment.GetEnvironmentVariable("ZITI_NATIVE_LOG");

    // Opt-in (set ZITI_NATIVE_LOG=/path): route the native ziti SDK log to that file at DEBUG so a dial/connect
    // failure records its real cause (the WARN line and rc from do_ziti_connect). A file, not Console, avoids
    // MSTest swallowing per-test stderr. No-op when the env var is unset, so normal runs pay nothing.
    public static void EnableNativeLogging()
    {
        if (string.IsNullOrEmpty(NativeLogPath)) return;
        _logWriter = (level, loc, msg, len) =>
        {
            var l = Marshal.PtrToStringUTF8(loc) ?? "";
            var m = Marshal.PtrToStringUTF8(msg) ?? "";
            try { lock (_logLock) { System.IO.File.AppendAllText(NativeLogPath, $"[ziti:{level}] {l}\t{m}\n"); } }
            catch { /* logging must never break the test */ }
        };
        ziti_log_set_logger(Marshal.GetFunctionPointerForDelegate(_logWriter));
        ziti_log_set_level(4, null); // DEBUG
    }

    public static void LibInit() => Ziti_lib_init();

    public static void LibShutdown() => Ziti_lib_shutdown();

    private static string ErrStr(int rc)
    {
        var p = ziti_errorstr(rc);
        return p == 0 ? rc.ToString() : (Marshal.PtrToStringUTF8(p) ?? rc.ToString());
    }

    // Blocking load, exactly like the C samples' init_context: returns only once the identity is fully
    // loaded/authenticated. Throws on failure.
    public static nint LoadContext(string identityFile)
    {
        int rc = Ziti_load_context(out var ztx, identityFile);
        if (rc != 0 || ztx == 0)
        {
            throw new InvalidOperationException($"Ziti_load_context failed: {ErrStr(rc)}");
        }
        return ztx;
    }

    // Server side: plain socket -> Ziti_bind -> Ziti_listen. Returns the server socket fd.
    public static nint BindListen(nint ztx, string service, string terminator, int backlog)
    {
        var srv = NewOsSocket();
        int rc = Ziti_bind(srv, ztx, service, terminator);
        if (rc != 0) throw new InvalidOperationException($"Ziti_bind failed: {ErrStr(Ziti_last_error())}");
        rc = Ziti_listen(srv, backlog);
        if (rc != 0) throw new InvalidOperationException($"Ziti_listen failed: {ErrStr(Ziti_last_error())}");
        // Block on accept so the SDK hands a dialing client off directly (see SetBlocking).
        SetBlocking(srv);
        return srv;
    }

    // EAGAIN/EWOULDBLOCK across linux (11), macOS (35) and Windows WSAEWOULDBLOCK (10035).
    private static bool IsWouldBlock(int err) => err == 11 || err == 35 || err == 10035;

    // Server side: poll until a ziti client connects, then return the accepted client fd. Ziti_accept is
    // non-blocking (it resolves with EAGAIN when no client is pending), so the C sample loops on EWOULDBLOCK;
    // we do the same. A real error (e.g. the server socket closed on shutdown) is not would-block, so we throw
    // and let the caller stop. The overall test timeout bounds the wait.
    public static nint Accept(nint server, out string caller)
    {
        var buf = new byte[256];
        while (true)
        {
            Array.Clear(buf);
            var clt = Ziti_accept(server, buf, buf.Length);
            if (clt != -1 && clt.ToInt64() >= 0)
            {
                int z = Array.IndexOf(buf, (byte)0);
                caller = Encoding.UTF8.GetString(buf, 0, z < 0 ? buf.Length : z);
                return clt;
            }
            int err = Ziti_last_error();
            if (!IsWouldBlock(err))
            {
                throw new InvalidOperationException($"Ziti_accept failed: {ErrStr(err)} (errno {err})");
            }
            Thread.Sleep(50);
        }
    }

    // Client side: plain socket -> Ziti_connect by service name on the explicit dialer context. Returns the
    // connected fd. (The C sample uses Ziti_connect_addr because it has a single context; we run binder and
    // dialer contexts in one process, so the by-name form with an explicit ztx selects the right one.)
    public static nint Connect(nint ztx, string service, string terminator)
    {
        var soc = NewOsSocket();
        int rc = Ziti_connect(soc, ztx, service, terminator);
        if (rc != 0)
        {
            int errno = Marshal.GetLastWin32Error();
            Ziti_close(soc);
            throw new InvalidOperationException($"Ziti_connect failed: rc={rc} ({ErrStr(rc)}), errno={errno}");
        }
        return soc;
    }

    public static void Close(nint socket) => Ziti_close(socket);

    // Wrap a zitilib socket fd as a .NET stream for read/write, exactly as the C samples read()/write() the
    // bridged fd. ownsHandle:true so disposing the stream closes the fd.
    public static NetworkStream ToStream(nint fd)
    {
        var handle = new SafeSocketHandle(fd, ownsHandle: true);
        var sock = new Socket(handle);
        return new NetworkStream(sock, ownsSocket: true);
    }
}
