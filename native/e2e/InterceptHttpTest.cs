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

// Intercept-style idiomatic test: a .NET host binds an HTTP responder on a ziti service (no host.v1, the SDK
// binds it), and a .NET client dials the address http.server.ziti:80, which the SDK maps to that service via
// the service's intercept.v1 config (ZitiSocket.ConnectByAddress / Ziti_connect_addr). Proves the SDK's
// address-intercept path end to end over the overlay, through the published native lib.
//
// Goes through the zitilib bridge, so it self-gates on linux for ziti-sdk-c 1.12.0..1.16.x exactly like
// ProxyBridgeTest / IdiomaticTrafficTest, and re-enables on 1.17.0. Runs normally on win/mac.
[TestClass]
public class InterceptHttpTest
{
    private const string SvcName = "e2e-idiomatic-http-svc";
    private const string InterceptHost = "http.server.ziti";
    private const int InterceptPort = 80;
    private const string Body = "hello-over-the-intercept";

    [DllImport("ziti4dotnet", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ziti_get_version();

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
                "PR #1047). win/mac run this test normally. See bridge-regression-report.md.");
        }
    }

    [TestMethod]
    [TestCategory("e2e")]
    [TestCategory("socket-bridge")]
    [Timeout(60_000)]
    public async Task Dotnet_Client_Dials_Intercept_Address()
    {
        SkipIfKnownBrokenBridge();

        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) =
            await setup.SetupInterceptServiceAsync(SvcName, InterceptHost, InterceptPort);

        using var hostCancel = new CancellationTokenSource(TimeSpan.FromSeconds(50));
        OverlaySetup.Say($"[http] starting idiomatic HTTP host on '{SvcName}' (API.Bind/Listen/Accept)");
        var hostTask = Task.Run(() => RunHttpHost(binderIdFile, hostCancel.Token), hostCancel.Token);

        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(20)),
            "Host never registered a terminator; the idiomatic HTTP bind side did not come up.");

        OverlaySetup.Say($"[http] client dialing http://{InterceptHost} via the intercept (API.ConnectByAddress)");
        var response = await Task.Run(() => DialInterceptAndGet(dialerIdFile));
        OverlaySetup.Say($"[http] got {response.Split('\n')[0].Trim()} ({response.Length} bytes)");

        hostCancel.Cancel();

        StringAssert.Contains(response, "200 OK",
            $"Client did not get an HTTP 200 dialing http://{InterceptHost} via the intercept.\nresponse:\n{response}");
        StringAssert.Contains(response, Body,
            "Client did not receive the host's HTTP body over the intercept path.");
    }

    // Bind the service via the idiomatic API and answer one HTTP request with a fixed 200 response.
    private static void RunHttpHost(string identityFile, CancellationToken ct)
    {
        var ztx = new ZitiContext(identityFile);
        using var server = new ZitiSocket(SocketType.Stream);
        API.Bind(server, ztx, SvcName, "");
        API.Listen(server, 10);

        var client = API.Accept(server, out _);
        // ToNetworkStream owns the fd (ownsSocket:true); do not also dispose the ZitiSocket.
        using var stream = client.ToNetworkStream();
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { NewLine = "\r\n", AutoFlush = true };

        // Consume the request line + headers (up to the blank line).
        string line;
        do { line = reader.ReadLine(); } while (!string.IsNullOrEmpty(line) && !ct.IsCancellationRequested);

        writer.Write($"HTTP/1.1 200 OK\r\nContent-Length: {Body.Length}\r\nConnection: close\r\n\r\n{Body}");
    }

    // Dial the intercept ADDRESS (not the service name): the SDK resolves http.server.ziti:80 to the service
    // through its intercept.v1 config. Then speak HTTP over the resulting stream.
    private static string DialInterceptAndGet(string identityFile)
    {
        var ztx = new ZitiContext(identityFile);
        var sock = new ZitiSocket(SocketType.Stream);
        API.ConnectByAddress(sock, InterceptHost, InterceptPort);

        // ToNetworkStream owns the fd; do not also dispose the ZitiSocket.
        using var stream = sock.ToNetworkStream();
        using var writer = new StreamWriter(stream) { NewLine = "\r\n", AutoFlush = true };
        using var reader = new StreamReader(stream);

        writer.Write($"GET / HTTP/1.1\r\nHost: {InterceptHost}\r\nConnection: close\r\n\r\n");
        return reader.ReadToEnd();
    }
}
