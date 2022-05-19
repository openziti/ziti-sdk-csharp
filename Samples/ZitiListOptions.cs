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

namespace OpenZiti.Samples
{
    public class ZitiListOptions
    {
        static MemoryStream ms = new MemoryStream(2 << 16); //a big bucket to hold bytes to display contiguously at the end of the program
        static ZitiCommand.Options Options = new ZitiCommand.Options();
        static int[] supportedCommands = new int[3] { 5, 14, 15 };

        static ZitiInstance zitiInstance = new ZitiInstance();

        internal static void OnZitiTunnelNextAction(object sender, ZitiCommand.NextAction action)
        {
            string mfacode;
            string idName = (zitiInstance.Zid?.IdentityNameFromController != null ? zitiInstance.Zid?.IdentityNameFromController : zitiInstance.Zid?.InitOpts.IdentityFile);

            switch (action.command)
            {
                case 5:
                    {
                        Console.WriteLine("Submit MFA for the identity " + idName);
                        Console.WriteLine("Enter the mfa auth code: ");
                        mfacode = Console.ReadLine();
                        zitiInstance.Zid.SubmitMFA(mfacode);
                        break;
                    }
                case 14:
                    {
                        Console.WriteLine("Fetching Ziti version (verbose mode) - {0}", ZitiUtil.GetVersion(true));
                        Console.WriteLine("Fetching Ziti version (non-verbose mode) - {0}", ZitiUtil.GetVersion(false));
                        Options.InvokeNextCommand(supportedCommands);

                    }
                    break;
                case 15:
                    {
                        Console.WriteLine("Fetching Ziti controller version - {0}", zitiInstance.Zid?.ControllerVersion);
                        Options.InvokeNextCommand(supportedCommands);
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

        public static void Run(string identityFile)
        {
            Options.OnNextAction += OnZitiTunnelNextAction;
            Object eventFlags = ZitiEventFlags.All;

            ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions()
            {
                EventFlags = (uint)(int)eventFlags,
                IdentityFile = identityFile,
                ApplicationContext = "weather-svc",
                ConfigurationTypes = new[] { "weather-config-type" },
            };
            opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
            opts.OnZitiMFAEvent += Opts_OnZitiMFAEvent;
            opts.OnZitiMFAStatusEvent += Opts_OnZitiMFAStatusEvent;

            ZitiIdentity zid = new ZitiIdentity(opts);
            zitiInstance.Initialize(zid);
            zid.Run();
            Console.WriteLine("=================LOOP IS COMPLETE=================");
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e)
        {
            if (e.Status.Ok())
            {
                Console.WriteLine("Identity connected event received for the identity {0}", e?.Name);
                //good. carry on.
            }
            else
            {
                //something went wrong. inspect the erorr here...
                Console.WriteLine("An error occurred.");
                Console.WriteLine("    ZitiStatus : " + e.Status);
                Console.WriteLine("               : " + e.Status.GetDescription());

            }
        }

        private static void Opts_OnZitiMFAEvent(object sender, ZitiMFAEvent e)
        {
            string nameOfId = (e.id.IdentityNameFromController != null ? e.id.IdentityNameFromController : e.id.InitOpts.IdentityFile);
            Console.WriteLine("MFA Auth requested for identity {0}", nameOfId);
            Console.WriteLine("Enter the mfa auth codo: ");
            string mfacode = Console.ReadLine();
            Console.WriteLine("Authcode for id {0} is {1}", nameOfId, mfacode);
            e.id.SubmitMFA(mfacode);
        }

        private static void Opts_OnZitiMFAStatusEvent(Object sender, ZitiMFAStatusEvent e)
        {
            ZitiIdentity.InitOptions senderInstance;
            if (sender is ZitiIdentity.InitOptions)
            {
                senderInstance = (ZitiIdentity.InitOptions)sender;
                Console.WriteLine("MFA status event received for identity {0}", senderInstance.IdentityFile);
            }
            if (e.status.Ok())
            {
                Console.WriteLine("Status Event {0} - verified {1}, operation type {2}", e.status, e.isVerified, e.operationType);
            }
            else
            {
                Console.WriteLine("MFA operation {0} failed due to {1}", e.operationType, e.status);
            }
            Options.InvokeNextCommand(supportedCommands);
        }
    }
}
