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

using OpenZiti.Management;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using OpenZiti.NET.Samples.Common;
using System.Security.Principal;
using System.Reflection.PortableExecutable;
using System.Net.Sockets;
using OpenZiti.NET.Samples.src.Common;

namespace OpenZiti.NET.Samples.Appetizer {

    [Sample("appetizer-reflect")]
    public class AppetizerSample : SampleBase {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        internal const string REFLECT_SERVICE_NAME = "reflectService";

        public override async Task<object> RunAsync(string[] args) {
            Log.Info("Appetizer reflect demo starts");
            var zitiContext = AppetizerSetup.ContextFromFile(args[1]);
            using Stream stream = ZitifiedNetworkStream.NewStream(zitiContext, REFLECT_SERVICE_NAME, null);
            using var reader = new StreamReader(stream, Encoding.ASCII);
            using var writer = new StreamWriter(stream, Encoding.ASCII);
            while (true) {
                Console.Write("Enter some text to send: ");
                var input = Console.ReadLine();
                await writer.WriteAsync(input);
                await writer.FlushAsync();
                var response = await reader.ReadLineAsync();
                Console.WriteLine($"Received: {response}");
            } //this just loops forever and c# is smart enough not to need a 'return' -- neat
        }
    }
}
