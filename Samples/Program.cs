using System;

using OpenZiti;
using System.Runtime.InteropServices;

namespace OpenZiti.Samples
{
    class Program
    {
        static void Main(string[] args)
        {
            try
            {
                args = new string[] { @"c:\temp\id.json", "weather", "ssh-service:2345" };

                CheckUsage(args);

                // set the log level of the native sdk
                Environment.SetEnvironmentVariable("ZITI_LOG", "3");

                switch (args[1].ToLower())
                {
                    case "weather":
                        Weather.Run(args);
                        break;
                    case "enroll":
                        Enrollment.Run(args);
                        break;
                    case "hosted":
                        HostedService.Run(args);
                        break;
                    case "proxy":
                        TcpProxy.Run(args);
                        break;
                }
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample execution completed successfully");
                Console.WriteLine("==============================================================");
            } 
            catch(Exception e)
            {
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample failed to execute");
                Console.WriteLine("==============================================================");
            }
        }

        public static void CheckUsage(string[] args)
        {
            if (args.Length < 2)
            {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} <path to config file> <sample to run: weather|enroll|hosted|proxy>");
                throw new ArgumentException("too few");
            }
        }
    }
}
