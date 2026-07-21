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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenZiti;

namespace E2ETest;

// The async twin of IdiomaticTrafficTest: the same overlay round-trip driven through the async idiomatic
// surface a C# dev expects -- API.AcceptAsync / API.ConnectAsync with a CancellationToken, and await
// ReadAsync/WriteAsync over ZitiSocket.ToNetworkStream(). Host and dialer run in-process as two contexts.
[TestClass]
public class IdiomaticAsyncTrafficTest
{
    private const string SvcName = "e2e-idiomatic-async-svc";
    private const string Message = "hello-from-async-client";

    [DllImport("ziti4dotnet", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ziti_get_version();

    // Skip (Inconclusive) only where the bridge is known broken: linux + ziti-sdk-c 1.12.0..1.16.x.
    private static void SkipIfKnownBrokenBridge()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        var p = ziti_get_version();
        var raw = p == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(p, 0));
        var clean = raw?.Split('-', '+')[0];
        if (Version.TryParse(clean, out var v) && v >= new Version(1, 12, 0) && v < new Version(1, 17, 0))
        {
            Assert.Inconclusive(
                $"ziti-sdk-c {raw}: the zitilib bridge is broken on linux from 1.12.0 to 1.16.x (fixed in 1.17.0, " +
                "PR #1047). win/mac run this test normally.");
        }
    }

    [TestMethod]
    [TestCategory("e2e")]
    [TestCategory("socket-bridge")]
    [Timeout(60_000)]
    public async Task Dotnet_Client_Dials_Dotnet_Server_Async()
    {
        SkipIfKnownBrokenBridge();

        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) = await setup.SetupServiceAsync(SvcName);

        using var hostCancel = new CancellationTokenSource(TimeSpan.FromSeconds(50));

        OverlaySetup.Say($"[async-echo] starting async echo server on '{SvcName}' (API.AcceptAsync)");
        var hostTask = RunAsyncHost(binderIdFile, hostCancel.Token);

        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(20)),
            "Host never registered a terminator; the async bind side did not come up.");

        OverlaySetup.Say($"[async-echo] dialing '{SvcName}' (API.ConnectAsync) and sending '{Message}'");
        var reply = await RunAsyncDial(dialerIdFile, Message);
        OverlaySetup.Say($"[async-echo] server echoed back: '{reply.Trim()}'");

        hostCancel.Cancel();

        StringAssert.Contains(reply, Message,
            "Async client did not get its bytes echoed back over the AcceptAsync/ConnectAsync path.");
    }

    // Bind the service and accept one client via AcceptAsync, echoing what it receives with async I/O.
    private static async Task RunAsyncHost(string identityFile, CancellationToken ct)
    {
        var ztx = new ZitiContext(identityFile);
        var server = new ZitiSocket(SocketType.Stream);
        API.Bind(server, ztx, SvcName, "");
        API.Listen(server, 10);

        var client = await API.AcceptAsync(server, ct);
        Assert.IsFalse(string.IsNullOrEmpty(client.Caller), "AcceptAsync should populate the caller identity");
        // ToNetworkStream owns the fd; do not also dispose the ZitiSocket.
        using var stream = client.ToNetworkStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        var received = await reader.ReadLineAsync();
        await writer.WriteLineAsync(received);
    }

    // Dial the service via ConnectAsync, send a line, and return the host's reply -- all async.
    private static async Task<string> RunAsyncDial(string identityFile, string message)
    {
        var ztx = new ZitiContext(identityFile);
        var sock = new ZitiSocket(SocketType.Stream);
        await API.ConnectAsync(sock, ztx, SvcName, "");

        using var stream = sock.ToNetworkStream();
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);
        await writer.WriteLineAsync(message);
        return await reader.ReadLineAsync() ?? string.Empty;
    }
}
