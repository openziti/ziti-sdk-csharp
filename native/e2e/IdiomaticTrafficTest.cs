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

// Same overlay-traffic proof as CallbackTrafficTest, but driven entirely through the IDIOMATIC managed SDK
// surface that app developers actually use: ZitiContext + ZitiSocket + API.Bind/Listen/Accept (host) and
// API.Connect (dial), reading/writing over ZitiSocket.ToNetworkStream(). Host and dialer run in-process as two
// contexts on the one zitilib loop (like ProxyBridgeTest), pushing real bytes over the quickstart overlay
// through the fresh native lib.
//
// The idiomatic dial/host go through the zitilib socket bridge (Ziti_connect/Ziti_bind), which is broken on
// linux for ziti-sdk-c 1.12.0 .. 1.16.x and fixed in 1.17.0 (see bridge-regression-report.md). So this test
// self-gates exactly like ProxyBridgeTest: it skips (Inconclusive) only on linux with a native in that broken
// range, runs normally on win/mac today, and re-enables itself on linux once the native moves to 1.17.0.
[TestClass]
public class IdiomaticTrafficTest
{
    private const string SvcName = "e2e-idiomatic-svc";
    private const string HostGreeting = "Hello from the idiomatic dotnet host!";

    [DllImport("ziti4dotnet", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ziti_get_version(); // returns const ziti_version*; first field is the version string

    // Skip (Inconclusive, not Fail) only where the bridge is known broken: linux + ziti-sdk-c 1.12.0..1.16.x.
    // Reading the real linked version means this needs no manual flip when the native moves to 1.17.0.
    private static void SkipIfKnownBrokenBridge()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        var p = ziti_get_version();
        var raw = p == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(p, 0));
        var clean = raw?.Split('-', '+')[0];
        if (Version.TryParse(clean, out var v) && v >= new Version(1, 12, 0) && v < new Version(1, 17, 0))
        {
            Assert.Inconclusive(
                $"ziti-sdk-c {raw}: the zitilib bridge dial/bind (Ziti_connect/Ziti_bind) is broken on linux from " +
                "1.12.0 to 1.16.x (mk_acceptor binds the loopback acceptor without listen() before the " +
                "caller-thread connect; linux RSTs the SYN). Fixed in 1.17.0 (PR #1047). win/mac run this test " +
                "normally. See bridge-regression-report.md.");
        }
    }

    [TestMethod]
    [TestCategory("e2e")]
    [TestCategory("socket-bridge")]
    [Timeout(60_000)]
    public async Task Dotnet_Client_Dials_Dotnet_Server_Idiomatic()
    {
        SkipIfKnownBrokenBridge();

        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) = await setup.SetupServiceAsync(SvcName);

        using var hostCancel = new CancellationTokenSource(TimeSpan.FromSeconds(50));

        // Host side: bind + listen + accept the service through the idiomatic API, then greet whoever dials.
        var hostTask = Task.Run(() => RunIdiomaticHost(binderIdFile, hostCancel.Token), hostCancel.Token);

        // Only dial once the bind terminator exists so the dial resolves promptly.
        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(20)),
            "Host never registered a terminator; the idiomatic bind side did not come up.");

        // Dial side: connect + send + receive through the idiomatic API.
        var reply = await Task.Run(() => RunIdiomaticDial(dialerIdFile, "hello-from-idiomatic-client"));

        hostCancel.Cancel();

        StringAssert.Contains(reply, HostGreeting,
            "Idiomatic client did not receive the host greeting over the ZitiSocket Connect/Bind path.");
    }

    // Bind the service and accept a single client, echoing a fixed greeting. Uses only the public idiomatic API.
    private static void RunIdiomaticHost(string identityFile, CancellationToken ct)
    {
        var ztx = new ZitiContext(identityFile);
        using var server = new ZitiSocket(SocketType.Stream);
        API.Bind(server, ztx, SvcName, "");
        API.Listen(server, 10);

        while (!ct.IsCancellationRequested)
        {
            // API.Accept blocks until a client connects; the test-level timeout bounds a never-arriving dial.
            var client = API.Accept(server, out _);
            using (client)
            using (var stream = client.ToNetworkStream())
            using (var reader = new StreamReader(stream))
            using (var writer = new StreamWriter(stream) { AutoFlush = true })
            {
                _ = reader.ReadLine(); // consume the client's message
                writer.WriteLine(HostGreeting);
            }
            return; // one round trip is enough to prove the path
        }
    }

    // Dial the service, send a line, and return the host's reply. Uses only the public idiomatic API.
    private static string RunIdiomaticDial(string identityFile, string message)
    {
        var ztx = new ZitiContext(identityFile);
        using var sock = new ZitiSocket(SocketType.Stream);
        API.Connect(sock, ztx, SvcName, "");

        using var stream = sock.ToNetworkStream();
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);
        writer.WriteLine(message);
        return reader.ReadLine() ?? string.Empty;
    }
}
