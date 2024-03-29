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

namespace OpenZiti.NET.Samples {
    [Sample("hosted-client")]
    public class HostedServiceClientSample : SampleBase {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override async Task<object> RunAsync(string[] args) {
            Log.Info("HostedServiceClientSample starts");
            //to see the logs from the Native SDK, set the log level
            API.SetLogLevel(ZitiLogLevel.INFO);
            
            var svcName = "hosted-demo-svc";
            var setupResult = await new SampleSetup(new()).SetupHostedClientExample(svcName);
            Log.Info("Identity file located at: " + setupResult);

            ZitiContext ctx = new ZitiContext(setupResult);
            string terminator = "";

            ZitiSocket socketa = new ZitiSocket(SocketType.Stream);
            ZitiSocket socketb = API.Connect(socketa, ctx, svcName, terminator);
            using (var s = socketb.ToNetworkStream())
            using (var r = new StreamReader(s))
            using (var w = new StreamWriter(s)) {
                string line = "initial";
                while (line.Length > 0) {
                    line = Console.ReadLine();
                    await w.WriteLineAsync(line);
                    w.AutoFlush = true;
                    Console.WriteLine("done sending. moving to read response");

                    string read = await r.ReadLineAsync();
                    Console.WriteLine($"Read:\n{read}");
                }
            }
            return null;
        }
    }
}
