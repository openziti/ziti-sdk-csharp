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
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenZiti;

namespace E2ETest;

// Simplest traffic proof: an in-process server BINDS a ziti service and echoes a line back to a client
// that DIALS it. No plain-TCP backend, no proxy bridge -- just SDK bind/dial through the fresh native
// lib. Complements the heavier prox-c ProxyBridgeTest.
[TestClass]
public class HostedEchoTest
{
    private const string SvcName = "e2e-echo-svc";

    [TestMethod]
    [TestCategory("e2e")]
    [Timeout(20_000)]
    public async Task Client_Dials_Server_And_Gets_Echo()
    {
        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) = await setup.SetupServiceAsync(SvcName);

        using var serverCancel = new CancellationTokenSource();

        // Server: bind + accept one client, echo the single line it sends.
        var serverCtx = new ZitiContext(binderIdFile);
        var serverTask = Task.Run(() =>
        {
            var listen = new ZitiSocket(SocketType.Stream);
            API.Bind(listen, serverCtx, SvcName, "");
            API.Listen(listen, 16);
            using var reg = serverCancel.Token.Register(listen.Dispose);

            var client = API.Accept(listen, out var caller);
            using var s = client.ToNetworkStream();
            using var r = new StreamReader(s);
            using var w = new StreamWriter(s) { AutoFlush = true };
            var line = r.ReadLine();
            w.WriteLine($"echo:{line}");
        }, serverCancel.Token);

        // Only dial once the server's bind terminator exists, so the dial succeeds promptly.
        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(10)),
            "Server never registered a terminator; the bind did not come up.");

        // Client: dial + send a line, read the echo.
        var clientCtx = new ZitiContext(dialerIdFile);
        var response = await DialSendReceiveWithRetryAsync(clientCtx, "hello-ziti", TimeSpan.FromSeconds(8));

        serverCancel.Cancel();
        try { await serverTask; } catch { /* torn down on cancel */ }

        Assert.AreEqual("echo:hello-ziti", response, "Did not receive the expected echo over the overlay.");
    }

    private static async Task<string> DialSendReceiveWithRetryAsync(ZitiContext ctx, string message, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var sock = new ZitiSocket(SocketType.Stream);
                API.Connect(sock, ctx, SvcName, "");
                using var s = sock.ToNetworkStream();
                using var r = new StreamReader(s);
                using var w = new StreamWriter(s) { AutoFlush = true };
                w.WriteLine(message);
                return r.ReadLine();
            }
            catch (Exception ex)
            {
                last = ex;
                await Task.Delay(500);
            }
        }
        throw new TimeoutException($"Could not dial {SvcName} within {timeout}.", last);
    }
}
