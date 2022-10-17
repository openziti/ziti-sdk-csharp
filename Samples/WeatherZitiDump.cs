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
using System.Linq;
using System.Text;
using System.Text.Json;
using System.Runtime.InteropServices;
using System.Collections.Generic;
using System.Threading;

namespace OpenZiti.Samples {

    public class WeatherZitiDump {
        static MemoryStream ms = new MemoryStream(2 << 16); //a big bucket to hold bytes to display contiguously at the end of the program
        static ZitiCommand.Options Options = new ZitiCommand.Options();
        static int[] supportedCommands = new int[5] { 0, 1, 5, 12, 13 };

        static ZitiInstance zitiInstance = new ZitiInstance();

        internal static void OnZitiTunnelNextAction(object sender, ZitiCommand.NextAction action) {
            string mfacode;
            string idName = (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile);

            switch (action.command) {
                case 1:
                    if (zitiInstance.Zid?.IdentityNameFromController != null) {
                        Console.WriteLine("Dial First service in " + zitiInstance.Zid?.IdentityNameFromController);
                    } else {
                        Console.WriteLine("Context is not loaded for identity {0}, exiting", zitiInstance.Zid?.InitOpts.IdentityFile);
                        Environment.Exit(0);
                        break;
                    }
                    if (zitiInstance.Services.Count > 0) {
                        ZitiService svc = zitiInstance.Services.First().Value;
                        svc.Dial(onConnected, onData);
                    } else {
                        Console.WriteLine("No service found.");
                        Options.InvokeNextCommand(supportedCommands);
                    }
                    break;
                case 5: {
                        Console.WriteLine("Submit MFA for the identity " + idName);
                        Console.WriteLine("Enter the mfa auth code: ");
                        mfacode = Console.ReadLine();
                        zitiInstance.Zid.SubmitMFA(mfacode);
                        break;
                    }
                case 12: {
                        Console.WriteLine("Ziti Dump To Log for identity {0}", idName);
                        zitiInstance.Zid.ZitiDumpToLog();
                        Options.InvokeNextCommand(supportedCommands);
                        break;
                    }
                case 13: {
                        Console.WriteLine("Ziti Dump To File for identity {0}", idName);
                        // folder should be created before running the program
                        string fileName = "C:\\tmp\\" + idName + ".ziti"; 
                        zitiInstance.Zid.ZitiDumpToFile(fileName);
                        Options.InvokeNextCommand(supportedCommands);
                        break;
                    }
                case 0:
                    Environment.Exit(0);
                    break;
                default:
                    Console.WriteLine("Wrong command received, exiting");
                    break;
            }
        }

        public static void Run(string identityFile) {
            Options.OnNextAction += OnZitiTunnelNextAction;
            Object eventFlags = ZitiEventFlags.All;

            ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                EventFlags = (uint)(int)eventFlags,
                IdentityFile = identityFile,
                ApplicationContext = "weather-svc",
                ConfigurationTypes = new[] { "weather-config-type" },
            };
            opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
            opts.OnZitiServiceEvent += Opts_OnZitiServiceEvent;
            opts.OnZitiMFAEvent += Opts_OnZitiMFAEvent;
            opts.OnZitiMFAStatusEvent += Opts_OnZitiMFAStatusEvent;

            ZitiIdentity zid = new ZitiIdentity(opts);
            zitiInstance.Initialize(zid);
            zid.Run();
            Console.WriteLine("=================LOOP IS COMPLETE=================");
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e) {
            if (e.Status.Ok()) {
                Console.WriteLine("Identity connected event received for the identity {0}", e?.Name);
                //good. carry on.
            } else {
                //something went wrong. inspect the erorr here...
                Console.WriteLine("An error occurred.");
                Console.WriteLine("    ZitiStatus : " + e.Status);
                Console.WriteLine("               : " + e.Status.GetDescription());
            }
        }

        private static void Opts_OnZitiServiceEvent(object sender, ZitiServiceEvent e) {
            string expected = (string)e.Context;
            try {
                IEnumerable<ZitiService> removedServices = e.Removed();
                Console.WriteLine("Removed Services ({0}): ", removedServices.Count());
                foreach (ZitiService svc in removedServices) {
                    if (zitiInstance.Services.ContainsKey(svc.Name)) {
                        zitiInstance.Services.Remove(svc.Name);

                    }
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                IEnumerable<ZitiService> modifiedServices = e.Changed();
                Console.WriteLine("Modified Services ({0}): ", modifiedServices.Count());
                foreach (ZitiService svc in modifiedServices) {
                    if (zitiInstance.Services.ContainsKey(svc.Name)) {
                        zitiInstance.Services.Remove(svc.Name);
                        zitiInstance.Services.Add(svc.Name, svc);
                    }
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                IEnumerable<ZitiService> addedServices = e.Added();
                Console.WriteLine("Available Services ({0}): ", addedServices.Count());
                foreach (ZitiService svc in addedServices) {
                    zitiInstance.Services.Add(svc.Name, svc);
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                    foreach (var entry in svc.PostureQueryMap) {
                        PostureQuerySet pqs = entry.Value;
                        Console.WriteLine("Policy Id {0} of the service - {1} is passing : {2}", pqs.PolicyId, svc.Name, pqs.IsPassing);
                    }
                }
                Options.InvokeNextCommand(supportedCommands);
            } catch (Exception ex) {
                Console.WriteLine("ERROR: Could not find the service we want [" + expected + "]? " + ex.Message);
                Options.InvokeNextCommand(supportedCommands); // show option to retry
            }
        }

        private static void Opts_OnZitiMFAEvent(object sender, ZitiMFAEvent e) {
            string nameOfId = (e.id.IdentityNameFromController != null ? e.id.IdentityNameFromController : e.id.InitOpts.IdentityFile);
            Console.WriteLine("MFA Auth requested for identity {0}", nameOfId);
            Console.WriteLine("Enter the mfa auth code: ");
            string mfacode = Console.ReadLine();
            Console.WriteLine("Authcode for id {0} is {1}", nameOfId, mfacode);
            e.id.SubmitMFA(mfacode);
        }

        private static void Opts_OnZitiMFAStatusEvent(Object sender, ZitiMFAStatusEvent e) {
            ZitiIdentity.InitOptions senderInstance;
            if (sender is ZitiIdentity.InitOptions) {
                senderInstance = (ZitiIdentity.InitOptions)sender;
                Console.WriteLine("MFA status event received for identity {0}", senderInstance.IdentityFile);
            }
            if (e.status.Ok()) {
                Console.WriteLine("Status Event {0} - verified {1}, operation type {2}", e.status, e.isVerified, e.operationType);
                if (e.recoveryCodes != null) {
                    Console.WriteLine("Provisioning URL : {0}", e.provisioningUrl);
                    for (int i = 0; i < e.recoveryCodes.Length; i++) {
                        Console.WriteLine("Recovery Code {0}", e.recoveryCodes[i]);
                    }
                    Options.InvokeNextCommand(supportedCommands); // after enabling mfa, show option to verify
                }

            } else {
                Console.WriteLine("MFA operation {0} failed due to {1}", e.operationType, e.status);
                Options.InvokeNextCommand(supportedCommands); // if mfa auth failed, show option to retry
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
                    Options.InvokeNextCommand(supportedCommands);
                } else {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                ConsoleHelper.OutputResponseToConsole(ms.ToArray());
            }
        }
    }
}
