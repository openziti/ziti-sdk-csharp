using System;

using OpenZiti;
using NLog;

namespace ConsoleTestApp {
    class Program {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        static string identityFile = @"c:\temp\pn.json";
        static void Main(string[] args) {
            Logging.SimpleConsoleLogging(LogLevel.Trace);
            Console.Clear();
        }
    }
}