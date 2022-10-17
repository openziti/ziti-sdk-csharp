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

using NLog;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Threading;
using System.Threading.Tasks;

namespace OpenZiti.Samples {
    public class WeatherStream {

        public static async Task Run(string identityFile) {

            Logging.SimpleConsoleLogging(LogLevel.Info);

            //uncomment this line to see the logs from the c-sdk
            //API.NativeLogger = API.DefaultNativeLogFunction;

            var opts1 = new ZitiIdentity.InitOptions() {
                EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent,
                IdentityFile = identityFile,
                ApplicationContext = "weather-svc",
                ConfigurationTypes = new[] { "weather-config-type" },
            };

            var zid1 = new ZitiIdentity(opts1);
            zid1.InitializeAndRun();

            //ziti is initialized - now wait for services/identity to be ready
            await zid1.WaitForServices();

            var wttrRequestAsBytes = Encoding.UTF8.GetBytes("GET / HTTP/1.0\r\n"
                                                               + "Accept: *-/*\r\n"
                                                               + "Connection: close\r\n"
                                                               + "User-Agent: curl/7.59.0\r\n"
                                                               + "Host: wttr.in\r\n"
                                                               + "\r\n");


            //makes the output pretty - and not jumbly
            Console.OutputEncoding = Encoding.UTF8;

            using (var zitiStream = new ZitiStream(zid1.NewConnection("weather-svc"))) {
                //write the request
                await zitiStream.WriteAsync(wttrRequestAsBytes, 0, wttrRequestAsBytes.Length);

                //pump the response to the console's standard out
                await zitiStream.PumpAsync(Console.OpenStandardOutput());
            }

            zid1.Shutdown();
        }
    }
}
