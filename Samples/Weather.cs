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

    public class Weather {
        static MemoryStream ms = new MemoryStream(2 << 16); //a big bucket to hold bytes to display contiguously at the end of the program
        static ZitiTunnelCommand.Options tunOptions = new ZitiTunnelCommand.Options();

        static ZitiInstance zitiInstance = new ZitiInstance();

        internal static void OnZitiTunnelNextAction(object sender, ZitiTunnelCommand.NextAction action)
		{
            string mfacode;

            switch (action.command)
			{
                case 1:
					{
                        Console.WriteLine("Enable MFA for the identity"); // list the identities and allow user to choose
                        ZitiIdentity.TunnelCB tunnelCB = new ZitiIdentity.TunnelCB();
                        tunnelCB.zidOpts = zitiInstance.Zid.InitOpts;
                        ZitiIdentity.TunnelCB.ZitiResponseDelegate cbDelegate = tunnelCB.ZitiResponse;
                        //StructWrapper tunCB = new StructWrapper(tunnelCB);
                        ZitiMFAService.ziti_mfa_enroll(zitiInstance.Zid.WrappedContext, Marshal.GetFunctionPointerForDelegate(cbDelegate));
                        break;
                    }
                case 2:
					{
                        Console.WriteLine("Verify MFA for the identity");
                        Console.WriteLine("Enter the mfa auth codo: ");
                        mfacode = Console.ReadLine();
                        ZitiIdentity.TunnelCB tunnelCB = new ZitiIdentity.TunnelCB();
                        tunnelCB.zidOpts = zitiInstance.Zid.InitOpts;
                        ZitiIdentity.TunnelCB.ZitiResponseDelegate cbDelegate = tunnelCB.ZitiResponse;
                        //StructWrapper tunCB = new StructWrapper(tunnelCB);
                        ZitiMFAService.verify_mfa(zitiInstance.Zid.WrappedContext, mfacode, Marshal.GetFunctionPointerForDelegate(cbDelegate));
                        break;
                    }
                case 3:
                    {
                        Console.WriteLine("Remove MFA for the identity");
                        Console.WriteLine("Enter the mfa auth codo: ");
                        mfacode = Console.ReadLine();
                        ZitiIdentity.TunnelCB tunnelCB = new ZitiIdentity.TunnelCB();
                        tunnelCB.zidOpts = zitiInstance.Zid.InitOpts;
                        ZitiIdentity.TunnelCB.ZitiResponseDelegate cbDelegate = tunnelCB.ZitiResponse;
                        //StructWrapper tunCB = new StructWrapper(tunnelCB);
                        ZitiMFAService.remove_mfa(zitiInstance.Zid.WrappedContext, mfacode, Marshal.GetFunctionPointerForDelegate(cbDelegate));
                        break;
                    }
                case 4:
					{
                        Console.WriteLine("Submit MFA for the identity");
                        Console.WriteLine("Enter the mfa auth codo: ");
                        mfacode = Console.ReadLine();
                        ZitiIdentity.TunnelCB tunnelCB = new ZitiIdentity.TunnelCB();
                        tunnelCB.zidOpts = zitiInstance.Zid.InitOpts;
                        ZitiIdentity.TunnelCB.ZitiResponseDelegate cbDelegate = tunnelCB.ZitiResponse;
                        // StructWrapper tunCB = new StructWrapper(tunnelCB);
                        ZitiMFAService.submit_mfa(zitiInstance.Zid.WrappedContext, mfacode, Marshal.GetFunctionPointerForDelegate(cbDelegate));
                        break;
                    }
                case 5:
                    Console.WriteLine("Dial First service");
                    if (zitiInstance.Services.Count > 0)
					{
                        ZitiService svc = zitiInstance.Services.First().Value;
                        svc.Dial(onConnected, onData);
                    } else
					{
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

        internal static void OnTunnelResult(object sender, ZitiTunnelCommand.TunnelResult result)
		{
            ZitiIdentity.TunnelCB tunnelCB = new ZitiIdentity.TunnelCB();
            if (zitiInstance.Zid.InitOpts.IdentityFile.Equals(result.id))
			{
                tunnelCB.zidOpts = zitiInstance.Zid.InitOpts;
                tunnelCB.ZitiResponse(result.result);
            } else
			{
                Console.WriteLine("Unknown identity - {0}", result.id);
            }
        }

        public static void Run(string identityFile) {
            tunOptions.OnNextAction += OnZitiTunnelNextAction;
            tunOptions.OnTunnelResult += OnTunnelResult;
            zitiInstance.Initialize();

            ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent | ZitiEventFlags.ZitiMfaAuthEvent | ZitiEventFlags.ZitiAPIEvent,
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
            zid.Run();
            Console.WriteLine("=================LOOP IS COMPLETE=================");
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e) {
            if (e.Status.Ok()) {
                zitiInstance.Zid = e.Identity;
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
                foreach (ZitiService svc in e.Removed())
                {
                    if (zitiInstance.Services.ContainsKey(svc.Name))
					{
                        zitiInstance.Services.Remove(svc.Name);

                    }
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                Console.WriteLine("Modified Services ({0}): ", e.Changed().Count());
                foreach (ZitiService svc in e.Changed())
                {
                    if (zitiInstance.Services.ContainsKey(svc.Name))
                    {
                        zitiInstance.Services.Remove(svc.Name);
                        zitiInstance.Services.Add(svc.Name, svc);
                    }
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                Console.WriteLine("Available Services ({0}): ", e.Added().Count());
                foreach (ZitiService svc in e.Added())
                {
                    zitiInstance.Services.Add(svc.Name, svc);
                    Console.WriteLine("{0} ({1})", svc.Name, svc.Id);
                }
                var service = e.Added().First(s => s.Name == expected);
                
                service.Dial(onConnected, onData);
            } catch(Exception ex) {
		        Console.WriteLine("ERROR: Could not find the service we want [" + expected + "]? " + ex.Message);
	        }
        }

        private static void Opts_OnZitiMFAEvent(object sender, ZitiMFAEvent e)
        {
            Console.WriteLine("MFA Auth requested for identity {0}", e.id?.IdentityNameFromController);
            Console.WriteLine("Enter the mfa auth codo: ");
            string mfacode = Console.ReadLine();
            Console.WriteLine("Authcode for id {0} is {1}", e.id?.IdentityNameFromController, mfacode);
            ZitiIdentity.TunnelCB tunnelCB = new ZitiIdentity.TunnelCB();
            tunnelCB.zidOpts = zitiInstance.Zid.InitOpts;
            StructWrapper tunCB = new StructWrapper(tunnelCB);
            ZitiMFAService.submit_mfa(zitiInstance.Zid.WrappedContext, mfacode, tunCB.Ptr);
        }

        private static void Opts_OnZitiAPIEvent(Object sender, ZitiAPIEvent e)
		{
            Console.WriteLine("API event received for identity {0}", e.id?.IdentityNameFromController);
        }

        private static void Opts_OnZitiMFAStatusEvent(Object sender, ZitiMFAStatusEvent e)
		{
            if(e.status.Ok())
			{
                Console.WriteLine("MFA status event received for identity {0}, mfa operation was successful", e.id?.IdentityNameFromController);
            } else
			{
                Console.WriteLine("MFA status event received for identity {0}, mfa operation failed", e.id?.IdentityNameFromController);
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
                    tunOptions.InvokeNextTunnelCommand();                  
                } else {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                ConsoleHelper.OutputResponseToConsole(ms.ToArray());
            }
        }
    }
}