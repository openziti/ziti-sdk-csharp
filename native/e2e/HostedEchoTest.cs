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
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace E2ETest;

// Simplest traffic proof, built call-for-call on the C SDK's own zitilib samples
// (programs/zitilib-samples/server.c hosts; ziti-http-get.c dials): a server BINDS a ziti service with a
// plain OS socket and echoes a line; a client DIALS the same service with a plain OS socket. All ziti calls
// go through ZitiNative (raw P/Invoke into ziti4dotnet), not the managed wrapper, so this exercises the same
// code path the C samples do.
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

        // Server: load the binder identity (blocking, like the C sample's init_context), bind+listen on a
        // plain OS socket, accept one client, echo the single line it sends.
        var serverCtx = ZitiNative.LoadContext(binderIdFile);
        var serverSock = ZitiNative.BindListen(serverCtx, SvcName, "", 16);
        var serverTask = Task.Run(() =>
        {
            var clt = ZitiNative.Accept(serverSock, out _);
            using var s = ZitiNative.ToStream(clt);
            using var r = new StreamReader(s);
            using var w = new StreamWriter(s) { AutoFlush = true };
            var line = r.ReadLine();
            w.WriteLine($"echo:{line}");
        }, serverCancel.Token);

        // Only dial once the server's bind terminator exists, so the dial succeeds promptly.
        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(10)),
            "Server never registered a terminator; the bind did not come up.");

        // Client: load the dialer identity, dial the service on a plain OS socket, send a line, read the echo.
        var clientCtx = ZitiNative.LoadContext(dialerIdFile);
        var response = await DialSendReceiveWithRetryAsync(clientCtx, "hello-ziti", TimeSpan.FromSeconds(8));

        serverCancel.Cancel();
        ZitiNative.Close(serverSock);
        try { await serverTask; } catch { /* torn down on cancel */ }

        Assert.AreEqual("echo:hello-ziti", response, "Did not receive the expected echo over the overlay.");
    }

    private static async Task<string> DialSendReceiveWithRetryAsync(nint ctx, string message, TimeSpan timeout)
    {
        var deadline = DateTime.UtcNow + timeout;
        Exception last = null;
        while (DateTime.UtcNow < deadline)
        {
            try
            {
                using var s = ZitiNative.ToStream(ZitiNative.Connect(ctx, SvcName, ""));
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
