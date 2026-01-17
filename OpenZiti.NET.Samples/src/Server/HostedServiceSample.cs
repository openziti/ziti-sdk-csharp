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

            Log.Info("Beginning accept loop...");
            while (true) {
                ZitiSocket client = API.Accept(socket, out var caller);
                using (var s = client.ToNetworkStream())
                using (var r = new StreamReader(s))
                using (var w = new StreamWriter(s)) {
                    w.AutoFlush = true;
                    Log.Info($"receiving connection from {caller}");
                    string read = await r.ReadLineAsync();
                    while (read != "EOL") {
                        Log.Info($"{caller} sent {read}");
                        string resp = $"Hi {caller}. Thanks for sending me: {read}";
                        await w.WriteLineAsync(resp);
                        Log.Info($"replied to {caller}");
                        read = r.ReadLine();
                    }
                    await w.WriteLineAsync("disconnecting...");
                    Log.Info($"{caller} disconnected");
                }
            }
        }
    }
}
