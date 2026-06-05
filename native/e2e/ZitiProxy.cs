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
using System.IO;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using OpenZiti;

namespace E2ETest;

// A managed recreation of ziti-sdk-c's ziti-prox-c sample (programs/ziti-prox-c/proxy.c), the most
// commonly used C sample. prox-c is a bidirectional bridge with two modes; this class implements both
// on top of the OpenZiti.NET blocking socket API:
//
//   * Host (bind) mode  -- prox-c "-b service:host:port": binds/hosts a ziti service, and for each ziti
//                          client connects to a backend TCP host:port and bridges bytes both ways.
//   * Intercept (dial)  -- prox-c "-i service:port": opens a plain local TCP listener and for each local
//                          connection dials the ziti service and bridges bytes both ways.
//
// The C SDK bridges with ziti_conn_bridge(); here a "bridge" is simply two Stream.CopyToAsync pumps,
// one per direction, between a plain System.Net NetworkStream and a ziti NetworkStream.
//
// The OpenZiti.NET socket API (Bind/Listen/Accept/Connect) is blocking, so each accept loop runs on a
// background task and is stopped by cancelling the token (which disposes the listening socket).
internal sealed class ZitiProxy : IAsyncDisposable
{
    private readonly CancellationTokenSource _cts = new();
    private readonly Task _loop;

    // The loop factory receives this instance's cancellation token, so DisposeAsync stops the loop.
    private ZitiProxy(Func<CancellationToken, Task> loopFactory) => _loop = Task.Run(() => loopFactory(_cts.Token));

    // Host (bind) side: bind the service, accept ziti clients, dial the backend, bridge.
    public static ZitiProxy StartHost(ZitiContext ctx, string service, string backendHost, int backendPort)
        => new(ct => HostLoop(ctx, service, backendHost, backendPort, ct));

    // Intercept (dial) side: listen on a local TCP port, dial the service per connection, bridge.
    public static ZitiProxy StartIntercept(ZitiContext ctx, string service, int localPort)
        => new(ct => InterceptLoop(ctx, service, localPort, ct));

    private static readonly string LogPath =
        Environment.GetEnvironmentVariable("ZITIPROXY_LOG") ?? Path.Combine(Path.GetTempPath(), "zitiproxy.log");

    private static readonly object LogLock = new();

    private static void Log(string msg)
    {
        var line = $"{DateTime.UtcNow:HH:mm:ss.fff} [ZitiProxy] {msg}";
        Console.Error.WriteLine(line);
        lock (LogLock) { File.AppendAllText(LogPath, line + Environment.NewLine); }
    }

    private static async Task HostLoop(ZitiContext ctx, string service, string backendHost, int backendPort, CancellationToken ct)
    {
        var listener = new ZitiSocket(SocketType.Stream);
        API.Bind(listener, ctx, service, "");
        API.Listen(listener, 16);
        Log($"host: bound+listening on service '{service}'");
        using var reg = ct.Register(listener.Dispose);

        while (!ct.IsCancellationRequested)
        {
            ZitiSocket zitiClient;
            try
            {
                zitiClient = API.Accept(listener, out var caller); // blocking; throws when disposed on cancel
                Log($"host: accepted ziti client from '{caller}'");
            }
            catch when (ct.IsCancellationRequested)
            {
                break;
            }
            catch (Exception ex)
            {
                Log($"host: accept failed: {ex.Message}");
                break;
            }

            // For each ziti client, open a TCP connection to the backend and bridge both ways.
            _ = Task.Run(async () =>
            {
                using var zsock = zitiClient;
                using var backend = new TcpClient();
                await backend.ConnectAsync(backendHost, backendPort, ct);
                Log($"host: connected to backend {backendHost}:{backendPort}, bridging");
                await BridgeAsync(zsock.ToNetworkStream(), backend.GetStream(), ct);
                Log("host: bridge ended");
            }, ct);
        }
    }

    private static async Task InterceptLoop(ZitiContext ctx, string service, int localPort, CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Loopback, localPort);
        listener.Start();
        Log($"intercept: listening on 127.0.0.1:{localPort} for service '{service}'");
        using var reg = ct.Register(listener.Stop);

        while (!ct.IsCancellationRequested)
        {
            TcpClient local;
            try
            {
                local = await listener.AcceptTcpClientAsync(ct);
                Log("intercept: accepted local TCP client");
            }
            catch when (ct.IsCancellationRequested)
            {
                break;
            }

            // For each local TCP connection, dial the ziti service and bridge both ways.
            _ = Task.Run(async () =>
            {
                using var tcp = local;
                using var zsock = new ZitiSocket(SocketType.Stream);
                try
                {
                    API.Connect(zsock, ctx, service, "");
                    Log($"intercept: dialed service '{service}', bridging");
                }
                catch (Exception ex)
                {
                    Log($"intercept: dial failed: {ex.Message}");
                    return;
                }
                await BridgeAsync(tcp.GetStream(), zsock.ToNetworkStream(), ct);
                Log("intercept: bridge ended");
            }, ct);
        }
    }

    // The managed equivalent of ziti_conn_bridge: pump bytes both directions until either side closes,
    // then tear both down so the other pump unblocks.
    private static async Task BridgeAsync(Stream a, Stream b, CancellationToken ct)
    {
        try
        {
            var aToB = a.CopyToAsync(b, ct);
            var bToA = b.CopyToAsync(a, ct);
            await Task.WhenAny(aToB, bToA);
        }
        catch (Exception ex) when (ex is IOException or OperationCanceledException or ObjectDisposedException)
        {
            // Expected on connection close / cancellation.
        }
        finally
        {
            a.Dispose();
            b.Dispose();
        }
    }

    public async ValueTask DisposeAsync()
    {
        _cts.Cancel();
        // The host accept loop blocks in a native call that may not unblock promptly on cancel; never let
        // disposal hang the test waiting for it.
        try { await Task.WhenAny(_loop, Task.Delay(2_000)); } catch { /* loop tears down on cancel */ }
        _cts.Dispose();
    }
}
