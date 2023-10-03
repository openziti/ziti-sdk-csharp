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
using System.Text;
using System.Threading.Tasks;

namespace OpenZiti.NET.Samples {
    [Sample("hosted-client")]
    public class HostedServiceClient : SampleBase {
        public override async Task RunAsync(string[] args) {
            var clientJwt = "";
            string outputPath = "";
            if (clientJwt.EndsWith(".jwt")) {
                outputPath = clientJwt.Replace(".jwt", ".json");
            } else {
                Console.WriteLine("Please provide a file that ends with .jwt");
                return;
            }

            try {
                Enroll(clientJwt, outputPath);
            } catch(Exception e) {
                Console.WriteLine($"WARN: the jwt was not enrolled properly: {e.Message}");
            }

            ZitiContext ctx = new ZitiContext(outputPath);
            string svc = "hosted-svc";
            string terminator = "";

            ZitiSocket socketa = new ZitiSocket(SocketType.Stream);
            ZitiSocket socketb = API.Connect(socketa, ctx, svc, terminator);
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
        }
    }
}
