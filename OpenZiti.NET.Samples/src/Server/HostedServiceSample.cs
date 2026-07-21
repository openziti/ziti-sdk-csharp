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

using OpenZiti.NET.Samples.Common;

namespace OpenZiti.NET.Samples.Server {
    [Sample("hosted")]
    public class HostedServiceSample : SampleBase {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override async Task<object> RunAsync(string[] args) {
            Log.Info("HostedServiceClientSample starts");
            var svcName = "hosted-demo-svc";
            var setupResult = await new SampleSetup(new()).SetupHostedExample(svcName);
            Log.Info("Identity file located at: " + setupResult);

            ZitiSocket socket = new ZitiSocket(SocketType.Stream);
            ZitiContext ctx = new ZitiContext(setupResult);
            string terminator = "";
            
            API.Bind(socket, ctx, svcName, terminator);
            API.Listen(socket, 100);

            // Cancelling the token closes the listener and unblocks AcceptAsync (Ctrl-C for graceful shutdown).
            using var cts = new CancellationTokenSource();
            Console.CancelKeyPress += (_, e) => { e.Cancel = true; cts.Cancel(); };

            Log.Info("Beginning async accept loop (Ctrl-C to stop)...");
            try {
                while (!cts.Token.IsCancellationRequested) {
                    ZitiSocket client = await API.AcceptAsync(socket, cts.Token);
                    using (var s = client.ToNetworkStream())
                    using (var r = new StreamReader(s))
                    using (var w = new StreamWriter(s)) {
                        w.AutoFlush = true;
                        Log.Info($"receiving connection from {client.Caller}");
                        string read = await r.ReadLineAsync();
                        while (read != null && read != "EOL") {
                            Log.Info($"{client.Caller} sent {read}");
                            await w.WriteLineAsync($"Hi {client.Caller}. Thanks for sending me: {read}");
                            read = await r.ReadLineAsync();
                        }
                        await w.WriteLineAsync("disconnecting...");
                        Log.Info($"{client.Caller} disconnected");
                    }
                }
            } catch (OperationCanceledException) {
                Log.Info("accept loop cancelled; shutting down");
            }
            return null;
        }
    }
}
