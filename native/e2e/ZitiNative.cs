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

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
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

    // Route the native ziti SDK log to stderr at DEBUG, so a dial/connect failure prints its real cause
    // (the WARN line and rc from do_ziti_connect), which CI captures.
    public static void EnableNativeLogging()
    {
        _logWriter = (level, loc, msg, len) =>
        {
            var l = Marshal.PtrToStringUTF8(loc) ?? "";
            var m = Marshal.PtrToStringUTF8(msg) ?? "";
            Console.Error.WriteLine($"[ziti:{level}] {l}\t{m}");
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
        return srv;
    }

    // Server side: block until a ziti client connects; returns the accepted client fd.
    public static nint Accept(nint server, out string caller)
    {
        var buf = new byte[256];
        var clt = Ziti_accept(server, buf, buf.Length);
        if (clt == -1 || (clt.ToInt64() < 0)) throw new InvalidOperationException($"Ziti_accept failed: {ErrStr(Ziti_last_error())}");
        int z = Array.IndexOf(buf, (byte)0);
        caller = Encoding.UTF8.GetString(buf, 0, z < 0 ? buf.Length : z);
        return clt;
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
            Ziti_close(soc);
            throw new InvalidOperationException($"Ziti_connect failed: {ErrStr(rc)}");
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
