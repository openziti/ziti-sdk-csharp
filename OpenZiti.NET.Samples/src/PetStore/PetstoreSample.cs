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

using OpenZiti.Debugging;
using OpenZiti.Generated.Petstore;
using OpenZiti.Management;
using System;
using System.IO;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using OpenZiti.NET.Samples.Common;
using System.Collections.Generic;

namespace OpenZiti.NET.Samples.Weather {

    [Sample("petstore")]
    public class PetstoreSample : SampleBase {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        
        public override async Task<object> RunAsync() {
            Log.Info("PetstoreSample starts");
            var svcName = "petstore-demo-svc";
            var desiredIntercept = "my.petstore";
            var port = 20080;
            var interceptAddress = $"http://{desiredIntercept}:{port}/v2"; //entirely fictitious domain name!
            var setupResult = await new SampleSetup(new()).SetupPetStoreExample(svcName, desiredIntercept, "127.0.0.1", port);
            Log.Info("Identity file located at: " + setupResult);
            
            var c = new ZitiContext(setupResult);
            var zitiSocketHandler = c.NewZitiSocketHandler(svcName);
            var httpHandler = new LoggingHandler(zitiSocketHandler);
            var zitifiedHttpClient = new HttpClient(httpHandler);
            
            var pc = new Client(zitifiedHttpClient) {
                BaseUrl = interceptAddress
            };
            var anon = new List<Anonymous>();
            anon.Add(Anonymous.Available);
            // uncomment to see http request/response httpHandler.LogHttpRequestResponse = true;
            var pets = await pc.FindPetsByStatusAsync(anon);
            foreach (var pet in pets) {
                Console.WriteLine(pet.Name);
            }
            
            return null;
        }
    }
}
