using System;
using System.Threading.Tasks;

using OpenZiti;
using NLog;
using NLog.Config;
using NLog.Targets;

namespace ConsoleTestApp {
    class Program {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static IntPtr loop;
        static void Main(string[] args) {

            loop = OpenZiti.API.NewLoop();

            var config = new LoggingConfiguration();
            // Targets where to log to: File and Console
            /*
            var logfile = new FileTarget("logfile") {
                FileName = ExpectedLogPathUI,
                ArchiveEvery = FileArchivePeriod.Day,
                ArchiveNumbering = ArchiveNumberingMode.Rolling,
                MaxArchiveFiles = 7,
                Layout = "[${date:format=yyyy-MM-ddTHH:mm:ss.fff}Z] ${level:uppercase=true:padding=5}\t${logger}\t${message}\t${exception:format=tostring}",
            };
            config.AddRule(LogLevel.Debug, LogLevel.Fatal, logfile);
            */
            var logconsole = new ConsoleTarget("logconsole") {
                 //Layout = "[${date:format=yyyy-MM-ddTHH:mm:ss.fff}Z] ${level:uppercase=true:padding=5}\t${logger}\t${message}\t${exception:format=tostring}",
                //layout = "[${date:universalTime=true:format=yyyy-MM-ddTHH\:mm\:ss.fff}Z] ${level:uppercase=true:padding=5}&#009;${logger}&#009;${message}&#009;${exception:format=tostring}",
                Layout = "[${date:format=yyyy-MM-ddTHH:mm:ss.fff}Z] ${level:uppercase=true:padding=5}\t${message}\t${exception:format=tostring}",

            };

            // Rules for mapping loggers to targets            
            config.AddRule(LogLevel.Trace, LogLevel.Fatal, logconsole);

            // Apply config           
            LogManager.Configuration = config;

            Logger.Info("this is a log message");
            Logger.Info("this is a log message");
            Logger.Info("this is a log message");
            Logger.Info("this is a log message");


            API.AfterEnroll ae = afterEnroll;
            Task.Factory.StartNew(() => {
                API.BeginEnroll(loop, @"c:\temp\id.jwt", ref ae);
                API.Run(loop);
            });
            //ZitiIdentity id = ZitiIdentity.FromFile(@"tests/TestAssets/test-id.json");
            //Assert.AreEqual("https://this-is-my-controller.netfoundry.io", id.ControllerURL);
            //id.Start();

            Task.Delay(30000).Wait();
            Console.WriteLine("=============");
            Console.WriteLine("=============");
            Console.WriteLine("=============");
        }

        private static void afterEnroll(ZitiStatus status) {
            throw new NotImplementedException();
        }
    }
}