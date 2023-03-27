using System;

using OpenZiti;
using System.Runtime.InteropServices;
using NLog;
using System.Threading.Tasks;
using System.Collections.Generic;
using OpenZiti.legacy;

namespace OpenZiti.Samples {
    public class Program {
        static async Task Main(string[] args) {
            try {
                //uncomment these lines to enable logging
                Console.WriteLine("before native call");
                OpenZiti.legacy.Logging.SimpleConsoleLogging(LogLevel.Trace);
                OpenZiti.legacy.API.NativeLogger = OpenZiti.legacy.API.DefaultNativeLogFunction;
                Console.WriteLine("after native call");

                OpenZiti.legacy.API.Enroll(args[1], afterEnrollcb, null);

                if(true) {
                    return;
                }
                Console.Clear();
                
                if (args == null || args.Length < 1) {
	                Console.WriteLine("This app expects the first paramter to be the sample to run: {exampleToRun=weather|enroll|hosted|hosted-client} {path-to-identity-file} {other-options}");
	                return;
                }

                switch (args[0].ToLower()) {
                    case "weather":
                        Weather.Run(args);
                        break;
                    case "enroll":
                        Enrollment.Run(args);
                        break;
                    case "hosted":
                        await HostedService.Run(args);
                        break;
                    case "hosted-client":
                        await HostedServiceClient.Run(args);
                        break;
                    default:
                        Console.WriteLine($"Unexpected sample supplied {args[0]}.");
                        break;
                }
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample execution completed successfully");
                Console.WriteLine("==============================================================");
            } catch (Exception e) {
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample failed to execute: " + e.Message);
                Console.WriteLine("");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("==============================================================");
            }
        }

        private static void afterEnrollcb(ZitiEnrollment.EnrollmentResult result) {

            Console.WriteLine(result.Json);
            Console.WriteLine(result.Message);
        }
    }
}
