using System;

using OpenZiti;
using System.Runtime.InteropServices;
using NLog;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OpenZiti.Samples {
    class Program {
        static async Task Main(string[] args) {
            try {
                Logging.SimpleConsoleLogging(LogLevel.Trace);
                
                //uncomment this line to see the logs from the c-sdk
                //API.NativeLogger = API.DefaultNativeLogFunction;

                Console.Clear();
                args = new[] {"enroll", @"c:\temp\id.jwt" };
                if (args == null || args.Length < 2) {
	                Console.WriteLine("This app requires two parameters to be supplied. {exampleToRun=eth0|weather|enroll|hosted} {path-to-identity-file}");
	                return;
                }
                
                string identityFile = args[1];
                switch (args[0].ToLower()) {
                    case "eth0":
                        Eth0.Run(identityFile);
                        break;
                    case "weather":
                        Weather.Run(identityFile);
                        break;
                    case "enroll":
                        Enrollment.Run(@"c:\temp\id.jwt");
                        break;
                    case "hosted":
                        await HostedService.RunAsync(identityFile);
                        break;
                    default:
                        Console.WriteLine($"Unexpected sample supplied {args[0]}. Running weather sample");
                        Weather.Run(identityFile);
                        break;
                }
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample execution completed successfully");
                Console.WriteLine("==============================================================");
            } catch (Exception e) {
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample failed to execute: " + e.Message);
                Console.WriteLine("==============================================================");
            }
        }
    }

    public static class CommonMethods {
        public static void CheckStatus(ZitiStatus status) {
            if (status.Ok()) {
                //good. carry on.
            } else {
                //something went wrong. inspect the erorr here...
                Console.WriteLine("An error occurred.");
                Console.WriteLine("    ZitiStatus : " + status);
                Console.WriteLine("               : " + status.GetDescription());
            }
        }
    }
}