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
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace E2ETest;

// Echo round-trip exercised only through the native package's public zitilib functions via raw P/Invoke
// (ZitiNative), not the managed OpenZiti.NET SDK. Binds with setBlocking:false to verify the native lib
// leaves the server socket in a state where Ziti_accept still works.
[TestClass]
public class NativeBridgeTrafficTest
{
    private const string SvcName = "e2e-native-bridge-svc";
    private const string Message = "hello-from-native-bridge-client";

    [DllImport("ziti4dotnet", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ziti_get_version(); // returns const ziti_version*; first field is the version string

    // On linux with ziti-sdk-c 1.12.0..1.16.x, marks the test Inconclusive (neither pass nor fail) instead of
    // running it, because that range has a different known bridge bug (mk_acceptor binds without listen()).
    // Runs normally at >= 1.17.0 and on win/mac.
    private static void SkipIfKnownBrokenBridge()
    {
        if (!RuntimeInformation.IsOSPlatform(OSPlatform.Linux)) return;

        var p = ziti_get_version();
        var raw = p == IntPtr.Zero ? null : Marshal.PtrToStringUTF8(Marshal.ReadIntPtr(p, 0));
        var clean = raw?.Split('-', '+')[0];
        if (Version.TryParse(clean, out var v) && v >= new Version(1, 12, 0) && v < new Version(1, 17, 0))
        {
            Assert.Inconclusive(
                $"ziti-sdk-c {raw}: the zitilib bridge is broken on linux from 1.12.0 to 1.16.x in an unrelated " +
                "way (mk_acceptor without listen()). This test reproduces the blocking-mode bug from 1.17.0 on.");
        }
    }

    [TestMethod]
    [TestCategory("e2e")]
    [TestCategory("socket-bridge")]
    [Timeout(60_000)]
    public async Task Native_Client_Dials_Native_Server_NoSetBlocking()
    {
        SkipIfKnownBrokenBridge();

        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) = await setup.SetupServiceAsync(SvcName);

        using var hostCancel = new CancellationTokenSource(TimeSpan.FromSeconds(50));

        OverlaySetup.Say($"[native-echo] starting raw-native echo server on '{SvcName}' (Ziti_bind/listen/accept, no SetBlocking)");
        var hostTask = Task.Run(() => RunNativeHost(binderIdFile), hostCancel.Token);

        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(20)),
            "Host never registered a terminator; the native bind side did not come up.");

        OverlaySetup.Say($"[native-echo] dialing '{SvcName}' (Ziti_connect) and sending '{Message}'");
        var reply = await Task.Run(() => RunNativeDial(dialerIdFile, Message));
        OverlaySetup.Say($"[native-echo] server echoed back: '{reply.Trim()}'");

        hostCancel.Cancel();

        StringAssert.Contains(reply, Message,
            "Native client did not get its bytes echoed back over the raw Ziti_bind/Ziti_accept/Ziti_connect path.");
    }

    // Binds and listens without forcing the socket blocking, accepts one client, and echoes the line back.
    private static void RunNativeHost(string identityFile)
    {
        var ctx = ZitiNative.LoadContext(identityFile);
        var server = ZitiNative.BindListen(ctx, SvcName, "", 10, setBlocking: false);

        var client = ZitiNative.AcceptOnce(server, out _);
        using var stream = ZitiNative.ToStream(client);
        using var reader = new StreamReader(stream);
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        var received = reader.ReadLine();
        writer.WriteLine(received);
    }

    // Dials the service and returns the host's reply.
    private static string RunNativeDial(string identityFile, string message)
    {
        var ctx = ZitiNative.LoadContext(identityFile);
        var fd = ZitiNative.Connect(ctx, SvcName, "");
        using var stream = ZitiNative.ToStream(fd);
        using var writer = new StreamWriter(stream) { AutoFlush = true };
        using var reader = new StreamReader(stream);
        writer.WriteLine(message);
        return reader.ReadLine() ?? string.Empty;
    }
}
