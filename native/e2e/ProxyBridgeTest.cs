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

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenZiti;

namespace E2ETest;

// The robust traffic proof: a faithful managed recreation of ziti-prox-c. Topology:
//
//   plain TCP client -> [intercept proxy (DIALS svc)] -> router -> [host proxy (BINDS svc)] -> TCP echo
//
// Bytes cross the fresh native lib on BOTH the dial and bind sides, plus a real bidirectional bridge,
// which is exactly what a .NET consumer of the package does in production. Fully self-contained: no
// internet, only loopback TCP + the local quickstart overlay.
[TestClass]
public class ProxyBridgeTest
{
    private const string SvcName = "e2e-proxy-svc";

    [TestMethod]
    [TestCategory("e2e")]
    [Timeout(20_000)]
    public async Task Traffic_Flows_Client_Through_Proxies_To_Backend()
    {
        using var backendCancel = new CancellationTokenSource();
        var backendPort = StartTcpEchoBackend(backendCancel.Token);
        var interceptPort = GetFreeTcpPort();

        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) = await setup.SetupServiceAsync(SvcName);

        // Host proxy hosts the service and forwards to the loopback echo backend (prox-c "-b").
        var hostCtx = ZitiNative.LoadContext(binderIdFile);
        await using var hostProxy = ZitiProxy.StartHost(hostCtx, SvcName, "127.0.0.1", backendPort);

        // Intercept proxy exposes the service as a local TCP port (prox-c "-i").
        var dialCtx = ZitiNative.LoadContext(dialerIdFile);
        await using var interceptProxy = ZitiProxy.StartIntercept(dialCtx, SvcName, interceptPort);

        // Only drive traffic once the host proxy's bind terminator exists, so the dial succeeds promptly.
        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(10)),
            "Host proxy never registered a terminator; the bind side did not come up.");

        var response = await DriveWithRetryAsync(interceptPort, "hello-prox-c", TimeSpan.FromSeconds(8));

        backendCancel.Cancel();

        Assert.AreEqual("echo:hello-prox-c", response,
            "Traffic did not round-trip client -> intercept -> overlay -> host -> backend and back.");
    }

    // A trivial loopback TCP echo server that prefixes "echo:" so we can assert the round trip.
    private static int StartTcpEchoBackend(CancellationToken ct)
    {
        var listener = new TcpListener(IPAddress.Loopback, 0);
        listener.Start();
        var port = ((IPEndPoint)listener.LocalEndpoint).Port;
        ct.Register(listener.Stop);

        _ = Task.Run(async () =>
        {
            while (!ct.IsCancellationRequested)
            {
                TcpClient conn;
                try { conn = await listener.AcceptTcpClientAsync(ct); }
                catch when (ct.IsCancellationRequested) { break; }

                _ = Task.Run(async () =>
                {
                    using var c = conn;
                    using var s = c.GetStream();
                    using var r = new StreamReader(s);
                    using var w = new StreamWriter(s) { AutoFlush = true };
                    var line = await r.ReadLineAsync(ct);
                    if (line != null) await w.WriteLineAsync($"echo:{line}");
                }, ct);
            }
        }, ct);

        return port;
    }

    private static int GetFreeTcpPort()
    {
        var l = new TcpListener(IPAddress.Loopback, 0);
        l.Start();
        var port = ((IPEndPoint)l.LocalEndpoint).Port;
        l.Stop();
        return port;
    }

    private static async Task<string> DriveWithRetryAsync(int port, string message, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception last = null;
        while (DateTime.UtcNow < deadline)
        {
            // Bound each attempt so a connection that opens but never echoes back (e.g. a dial still
            // settling) is abandoned and retried rather than blocking the whole test.
            using var attempt = new CancellationTokenSource(TimeSpan.FromSeconds(3));
            try
            {
                using var client = new TcpClient();
                await client.ConnectAsync(IPAddress.Loopback, port, attempt.Token);
                using var s = client.GetStream();
                using var r = new StreamReader(s);
                using var w = new StreamWriter(s) { AutoFlush = true };
                await w.WriteLineAsync(message.AsMemory(), attempt.Token);
                var read = await r.ReadLineAsync(attempt.Token);
                if (!string.IsNullOrEmpty(read)) return read;
            }
            catch (Exception ex)
            {
                last = ex;
            }
            await Task.Delay(500);
        }
        throw new TimeoutException($"No round-trip response within {timeout}.", last);
    }
}
