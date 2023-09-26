using System;

using OpenZiti;
using System.Runtime.InteropServices;
using NLog;
using System.Threading.Tasks;
using System.Collections.Generic;
using NLog.Fluent;
using OpenZiti.Native;

namespace OpenZiti.Samples {
    public class Program {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        static async Task Main(string[] args) {
            try {
                OpenZiti.Logging.SimpleConsoleLogging(LogLevel.Trace);

                OpenZiti.API.NativeLogger = OpenZiti.API.DefaultNativeLogFunction;
                OpenZiti.API.InitializeZiti();
                //to see the logs from the Native SDK, set the log level
                OpenZiti.API.SetLogLevel(Logging.ZitiLogLevel.INFO);
                Console.Clear();

                if (args == null || args.Length < 3) {
                    Console.WriteLine("These samples expect at least two params to be supplied:");
                    Console.WriteLine(" param1: the sample to run: {exampleToRun=weather|enroll|hosted|hosted-client}");
                    Console.WriteLine(" param2: the jwt to use: {path-to-identity-file}");
                    Console.WriteLine(" then, any other params needed");
                    return;
                }

                // reminder to devs that these examples are intended to run from the command line. that means args[0]
                // will be the name of the executing assembly. if you "debug" these samples, make sure to add an args[0]
                switch (args[1].ToLower()) {
                    case "weather":
                        Weather.Run(args[2]);
                        break;
                    case "enroll":
                        Enrollment.Run(args[2]);
                        break;
                    case "hosted":
                        await HostedService.Run(args[2]);
                        break;
                    case "hosted-client":
                        await HostedServiceClient.Run(args[2]);
                        break;
                    case "test":
                        TestBlitting.Run();
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
    }
}
