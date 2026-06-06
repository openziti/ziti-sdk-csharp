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
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace E2ETest;

// Sets up a service on the quickstart overlay, runs native/e2e-app as the server, then as the client, and
// checks the .NET client dials the .NET server and gets its greeting back. Both go through the fresh native
// lib, so this proves the C SDK works in .NET on whatever OS runs it.
[TestClass]
public class CallbackTrafficTest
{
    private const string SvcName = "e2e-callback-svc";

    [TestMethod]
    [TestCategory("e2e")]
    [Timeout(120_000)]
    public async Task Dotnet_Client_Dials_Dotnet_Server()
    {
        var setup = await OverlaySetup.ConnectAsync();
        await setup.EnsureRouterPoliciesAsync();
        var (binderIdFile, dialerIdFile) = await setup.SetupServiceAsync(SvcName);

        var app = ZitiProgram.ResolveApp();

        // Run the app as the server: bind the service via ziti_listen, print "HOST ready ...".
        using var server = ZitiProgram.Start(app, "HOST ready", "host", binderIdFile, SvcName);
        Assert.IsTrue(await server.WaitForReadyAsync(TimeSpan.FromSeconds(40)),
            $"Server never reported ready.\nserver output:\n{server.Output}");

        // Only dial once the bind terminator exists so the dial resolves promptly.
        Assert.IsTrue(await setup.WaitForTerminatorAsync(SvcName, TimeSpan.FromSeconds(20)),
            $"Server never registered a terminator.\nserver output:\n{server.Output}");

        // Run the app as the client: dial via ziti_dial, send a message, print the reply.
        var (exit, output) = await ZitiProgram.RunAsync(
            app, TimeSpan.FromSeconds(25), "dial", dialerIdFile, SvcName, "hello-from-dotnet-client");

        StringAssert.Contains(output, "Hello from the dotnet host!",
            $"Client did not receive the server greeting over the ziti_dial callback path.\n" +
            $"client exit={exit}\nclient output:\n{output}\nserver output:\n{server.Output}");
    }
}
