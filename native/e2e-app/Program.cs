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
using System.Runtime.InteropServices;
using System.Text;

namespace ZitiE2e;

// One small program that is both the ziti client and the ziti server, picked by the first arg:
//   e2e-app host <identity.json> <service>            -> bind the service, greet + echo every client
//   e2e-app dial <identity.json> <service> [message]  -> dial the service, send a message, print the reply
//
// It talks straight to the ziti4dotnet native lib over P/Invoke using the ziti_dial / ziti_listen callback
// API (a direct port of the C SDK's sample-dial.c and sample-host.c). The e2e test runs this twice, once as
// the server and once as the client, to prove the C SDK works in .NET on whatever OS it runs on.
internal static class Program
{
    private const string Dll = "ziti4dotnet";

    private const int ZITI_OK = 0;
    private const int ZITI_EOF = -19;
    private const int ZITI_PARTIALLY_AUTHENTICATED = -31;
    private const int ZitiContextEvent = 1;

    private const string Greeting = "Hello from the dotnet host!";

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void EventCb(IntPtr ztx, IntPtr ev);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void ConnCb(IntPtr conn, int status);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void ClientCb(IntPtr serv, IntPtr client, int status, IntPtr clientCtx);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate IntPtr DataCb(IntPtr conn, IntPtr data, IntPtr len); // ssize_t
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)] private delegate void WriteCb(IntPtr conn, IntPtr status, IntPtr ctx);

    // struct ziti_options (ziti.h). CLong gives C `long` the right width per platform (4 win / 8 linux+mac).
    [StructLayout(LayoutKind.Sequential)]
    private struct ZitiOptions
    {
        public byte disabled;
        public IntPtr config_types;
        public uint api_page_size;
        public CLong refresh_interval;
        public int metrics_type;
        public IntPtr pq_mac_cb;
        public IntPtr pq_os_cb;
        public IntPtr pq_process_cb;
        public IntPtr pq_domain_cb;
        public IntPtr app_ctx;
        public uint events;
        public IntPtr event_cb;
        public uint cert_extension_window;
        public int enroll_mode;
    }

    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_load_config(IntPtr cfg, [MarshalAs(UnmanagedType.LPUTF8Str)] string path);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_context_init(out IntPtr ztx, IntPtr cfg);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_context_set_options(IntPtr ztx, ref ZitiOptions opts);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_context_run(IntPtr ztx, IntPtr loop);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_conn_init(IntPtr ztx, out IntPtr conn, IntPtr connCtx);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_listen(IntPtr serv, [MarshalAs(UnmanagedType.LPUTF8Str)] string service, ConnCb lcb, ClientCb ccb);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_accept(IntPtr client, ConnCb cb, DataCb dataCb);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_dial(IntPtr conn, [MarshalAs(UnmanagedType.LPUTF8Str)] string service, ConnCb connCb, DataCb dataCb);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_write(IntPtr conn, IntPtr data, IntPtr len, WriteCb cb, IntPtr ctx);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_close(IntPtr conn, IntPtr cb);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_close_write(IntPtr conn);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int ziti_shutdown(IntPtr ztx);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ziti_errorstr(int err);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr z4d_default_loop();
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern int z4d_uv_run(IntPtr loop);
    [DllImport(Dll, CallingConvention = CallingConvention.Cdecl)]
    private static extern void ziti_log_init(IntPtr loop, int level, IntPtr logger);

    // delegates + buffers kept alive for the process so the native side can call back / hold pointers
    private static EventCb _eventCb;
    private static ConnCb _onListen, _onConnect;
    private static ClientCb _onClient;
    private static DataCb _onData;
    private static WriteCb _onWrite;
    private static GCHandle _cfgPin, _msgPin;

    private static IntPtr _ztx, _msgPtr;
    private static int _msgLen;
    private static string _service;
    private static bool _server;     // which mode we are in
    private static bool _done;       // dial: reply received, ignore the trailing shutdown event

    private static string Err(int code)
    {
        var p = ziti_errorstr(code);
        return p == IntPtr.Zero ? code.ToString() : (Marshal.PtrToStringUTF8(p) ?? code.ToString());
    }

    private static void Die(int code, string what)
    {
        if (code != ZITI_OK)
        {
            Console.Error.WriteLine($"ERROR: {what} => {Err(code)}");
            Environment.Exit(code);
        }
    }

    private static int Main(string[] args)
    {
        if (args.Length < 3 || (args[0] != "host" && args[0] != "dial"))
        {
            Console.Error.WriteLine("usage: e2e-app host <identity.json> <service>");
            Console.Error.WriteLine("       e2e-app dial <identity.json> <service> [message]");
            return 1;
        }

        _server = args[0] == "host";
        var idFile = args[1];
        _service = args[2];
        var message = Encoding.UTF8.GetBytes(args.Length > 3 ? args[3] : "hello from dotnet-dial");

        _eventCb = OnEvent;
        _onListen = OnListen;
        _onClient = OnClient;
        _onConnect = OnClientConnect;
        _onData = _server ? OnServerData : OnDialData;
        _onWrite = OnWrite;

        if (!_server)
        {
            _msgLen = message.Length;
            _msgPin = GCHandle.Alloc(message, GCHandleType.Pinned);
            _msgPtr = _msgPin.AddrOfPinnedObject();
        }

        var loop = z4d_default_loop();
        int logLevel = int.TryParse(Environment.GetEnvironmentVariable("ZITI_LOG"), out var lv) ? lv : 2; // WARN
        ziti_log_init(loop, logLevel, IntPtr.Zero);

        var cfg = new byte[8192];
        _cfgPin = GCHandle.Alloc(cfg, GCHandleType.Pinned);
        var cfgPtr = _cfgPin.AddrOfPinnedObject();

        Die(ziti_load_config(cfgPtr, idFile), "ziti_load_config");
        Die(ziti_context_init(out _ztx, cfgPtr), "ziti_context_init");

        var opts = new ZitiOptions { events = ZitiContextEvent, event_cb = Marshal.GetFunctionPointerForDelegate(_eventCb) };
        Die(ziti_context_set_options(_ztx, ref opts), "ziti_context_set_options");
        Die(ziti_context_run(_ztx, loop), "ziti_context_run");

        z4d_uv_run(loop); // server: runs until killed. dial: ends after the reply triggers ziti_shutdown.
        return 0;
    }

    // ctx-ready event: server starts listening, client starts dialing.
    private static void OnEvent(IntPtr ztx, IntPtr ev)
    {
        if (_done) return;
        if (Marshal.ReadInt32(ev, 0) != ZitiContextEvent) return;
        int ctrlStatus = Marshal.ReadInt32(ev, 8); // ev->ctx.ctrl_status
        if (ctrlStatus == ZITI_PARTIALLY_AUTHENTICATED) return;
        Die(ctrlStatus, "context event ctrl_status");

        Die(ziti_conn_init(ztx, out var conn, IntPtr.Zero), "ziti_conn_init");
        if (_server) Die(ziti_listen(conn, _service, _onListen, _onClient), "ziti_listen");
        else Die(ziti_dial(conn, _service, _onConnect, _onData), "ziti_dial");
    }

    private static void OnWrite(IntPtr conn, IntPtr status, IntPtr ctx)
    {
        long s = status.ToInt64();
        if (s < 0) Console.Error.WriteLine($"write failed: {Err((int)s)}");
        if (ctx != IntPtr.Zero) Marshal.FreeHGlobal(ctx); // server replies are unmanaged buffers; free them
    }

    // ---- server (host) ----
    private static void OnListen(IntPtr serv, int status)
    {
        if (status == ZITI_OK) Console.WriteLine($"HOST ready, bound '{_service}'");
        else { Console.Error.WriteLine($"could not bind '{_service}': {Err(status)}"); ziti_close(serv, IntPtr.Zero); }
    }

    private static void OnClient(IntPtr serv, IntPtr client, int status, IntPtr clientCtx)
    {
        if (status == ZITI_OK) ziti_accept(client, _onConnect, _onData);
    }

    private static void OnClientConnect(IntPtr conn, int status)
    {
        if (!_server) { OnDialConnect(conn, status); return; }
        if (status == ZITI_OK) SendUnmanaged(conn, Greeting); // greet on connect
    }

    private static IntPtr OnServerData(IntPtr clt, IntPtr data, IntPtr len)
    {
        long n = len.ToInt64();
        if (n > 0) SendUnmanaged(clt, n.ToString());       // reply with the byte count
        else if (n == ZITI_EOF) ziti_close_write(clt);
        else ziti_close(clt, IntPtr.Zero);
        return len;
    }

    private static void SendUnmanaged(IntPtr conn, string text)
    {
        var bytes = Encoding.UTF8.GetBytes(text);
        var buf = Marshal.AllocHGlobal(bytes.Length);
        Marshal.Copy(bytes, 0, buf, bytes.Length);
        ziti_write(conn, buf, (IntPtr)bytes.Length, _onWrite, buf); // freed in OnWrite via ctx
    }

    // ---- client (dial) ----
    private static void OnDialConnect(IntPtr conn, int status)
    {
        Die(status, "dial connect status");
        Console.WriteLine($"connected to service[{_service}], sending message");
        ziti_write(conn, _msgPtr, (IntPtr)_msgLen, _onWrite, IntPtr.Zero);
    }

    private static IntPtr OnDialData(IntPtr conn, IntPtr data, IntPtr len)
    {
        long n = len.ToInt64();
        _done = true;
        if (n > 0)
        {
            var buf = new byte[n];
            Marshal.Copy(data, buf, 0, (int)n);
            Console.WriteLine($"received {n} bytes:");
            Console.WriteLine(Encoding.UTF8.GetString(buf));
        }
        else if (n == ZITI_EOF) Console.WriteLine("connection closed by host");
        else Console.Error.WriteLine($"error receiving data: {Err((int)n)}");

        ziti_close(conn, IntPtr.Zero);
        ziti_shutdown(_ztx); // ends the loop -> process exits
        return len;
    }
}
