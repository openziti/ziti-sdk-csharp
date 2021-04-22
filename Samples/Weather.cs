/*
Copyright 2019-2020 NetFoundry, Inc.

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
using System.Linq;
using System.Text;
using System.Text.Json;

namespace OpenZiti.Samples {
    public class Weather {
        static MemoryStream ms = new MemoryStream(2 << 16); //a big bucket to hold bytes to display contiguously at the end of the program

        public static void Run(string identityFile) {
            ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent,
                IdentityFile = identityFile,
                ApplicationContext = "weather-svc",
                ConfigurationTypes = new string[] { "weather-config-type" },
            };
            opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
            opts.OnZitiServiceEvent += Opts_OnZitiServiceEvent;

            ZitiIdentity zid = new ZitiIdentity(opts);
            zid.Run();
            Console.WriteLine("=================LOOP IS COMPLETE=================");
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e) {
            if (e.Status.Ok()) {
                //good. carry on.
            } else {
                //something went wrong. inspect the erorr here...
                Console.WriteLine("An error occurred.");
                Console.WriteLine("    ZitiStatus : " + e.Status);
                Console.WriteLine("               : " + e.Status.GetDescription());
            }
        }

        private static void Opts_OnZitiServiceEvent(object sender, ZitiServiceEvent e) {
            var service = e.Added().First(s => s.Name == (string)e.Context);
            if (service != null) {
                service.Dial(onConnected, onData);
            } else {
                Console.WriteLine("ERROR: Could not find the service we want?");
            }
        }

        private static void onConnected(ZitiConnection connection, ZitiStatus status) {
            ZitiUtil.CheckStatus(status);
            
            string cfg = connection.Service.GetConfiguration("weather-config-type");
            string where = null;
            if (cfg == null) {
                where = "London";
                Console.WriteLine("The service does not have a configuration of type 'weather-config-type' - using default: " + where);
            } else {
                where = JsonDocument.Parse(cfg).RootElement.GetProperty("where").ToString();
            }
            byte[] bytes = Encoding.UTF8.GetBytes($"GET /{where} HTTP/1.0\r\n"
                                                + "Accept: *-/*\r\n"
                                                + "Connection: close\r\n"
                                                + "User-Agent: curl/7.59.0\r\n"
                                                + "Host: wttr.in\r\n"
                                                + "\r\n");

            connection.Write(bytes, afterDataWritten, "write context");
        }

        private static void afterDataWritten(ZitiConnection connection, ZitiStatus status, object context) {
            ZitiUtil.CheckStatus(status);
        }

        private static void onData(ZitiConnection connection, ZitiStatus status, byte[] data) {
            if (status == ZitiStatus.OK) {
                ms.Write(data); //collect all the bytes to display contiguously at the end of the program
            } else {
                if (status == ZitiStatus.EOF) {
                    ConsoleHelper.OutputResponseToConsole(ms.ToArray());
                    Console.WriteLine("request completed: " + status.GetDescription());
                    connection.Close();
                    Environment.Exit(0);
                } else {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                ConsoleHelper.OutputResponseToConsole(ms.ToArray());
            }
        }
    }
}