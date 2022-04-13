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

namespace OpenZiti.Samples {

    public class WeatherMFA {
        static MemoryStream ms = new MemoryStream(2 << 16); //a big bucket to hold bytes to display contiguously at the end of the program
        static ZitiCommand.Options Options = new ZitiCommand.Options();

        static ZitiInstance zitiInstance = new ZitiInstance();

        internal static void OnZitiTunnelNextAction(object sender, ZitiCommand.NextAction action) {
            string mfacode;

            switch (action.command) {
                case 1: {
                        Console.WriteLine("Enable MFA for the identity: " + (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile));
                        API.EnrollMFA(zitiInstance.Zid);
                        break;
                    }
                case 2: {
                        Console.WriteLine("Verify MFA for the identity" + (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile));
                        Console.WriteLine("Enter the mfa auth codo: ");
                        mfacode = Console.ReadLine();
                        API.VerifyMFA(zitiInstance.Zid, mfacode);
                        break;
                    }
                case 3: {
                        Console.WriteLine("Remove MFA for the identity" + (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile));
                        Console.WriteLine("Enter the mfa auth codo: ");
                        mfacode = Console.ReadLine();
                        API.RemoveMFA(zitiInstance.Zid, mfacode);
                        break;
                    }
                case 4: {
                        Console.WriteLine("Submit MFA for the identity " + (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile));
                        Console.WriteLine("Enter the mfa auth codo: ");
                        mfacode = Console.ReadLine();
                        API.SubmitMFA(zitiInstance.Zid, mfacode);
                        break;
                    }
                case 5:
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
                        Console.WriteLine("No service found, exiting");
                        Environment.Exit(0);
                    }
                    break;
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
            zitiInstance.Initialize();
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
            opts.OnZitiAPIEvent += Opts_OnZitiAPIEvent;
            opts.OnZitiMFAStatusEvent += Opts_OnZitiMFAStatusEvent;

            ZitiIdentity zid = new ZitiIdentity(opts);
            zitiInstance.Zid = zid;
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
                Console.WriteLine("Removed Services ({0}): ", e.Removed().Count());
                foreach (ZitiService svc in e.Removed()) {
                    if (zitiInstance.Services.ContainsKey(svc.Name)) {
                        zitiInstance.Services.Remove(svc.Name);

                    }
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                Console.WriteLine("Modified Services ({0}): ", e.Changed().Count());
                foreach (ZitiService svc in e.Changed()) {
                    if (zitiInstance.Services.ContainsKey(svc.Name)) {
                        zitiInstance.Services.Remove(svc.Name);
                        zitiInstance.Services.Add(svc.Name, svc);
                    }
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                Console.WriteLine("Available Services ({0}): ", e.Added().Count());
                foreach (ZitiService svc in e.Added()) {
                    zitiInstance.Services.Add(svc.Name, svc);
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                var service = e.Added().First(s => s.Name == expected);

                Options.InvokeNextCommand();
            } catch (Exception ex) {
                Console.WriteLine("ERROR: Could not find the service we want [" + expected + "]? " + ex.Message);
                Options.InvokeNextCommand(); // show option to retry
            }
        }

        private static void Opts_OnZitiMFAEvent(object sender, ZitiMFAEvent e) {
            Console.WriteLine("MFA Auth requested for identity {0}", (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile));
            Console.WriteLine("Enter the mfa auth codo: ");
            string mfacode = Console.ReadLine();
            Console.WriteLine("Authcode for id {0} is {1}", e.id?.IdentityNameFromController, mfacode);
            API.SubmitMFA(zitiInstance.Zid, mfacode);
        }

        private static void Opts_OnZitiAPIEvent(Object sender, ZitiAPIEvent e) {
            Console.WriteLine("API event received for identity {0}", e.id?.IdentityNameFromController);
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
                    Options.InvokeNextCommand(); // after enabling mfa, show option to verify
                }

            } else {
                Console.WriteLine("MFA operation {0} failed due to {1}", e.operationType, e.status);
                Options.InvokeNextCommand(); // if mfa auth failed, show option to retry
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
                    Options.InvokeNextCommand();
                } else {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                ConsoleHelper.OutputResponseToConsole(ms.ToArray());
            }
        }
    }
}