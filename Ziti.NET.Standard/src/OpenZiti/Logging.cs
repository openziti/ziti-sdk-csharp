using NLog;
using NLog.Config;
using NLog.Targets;
using System;
using System.Collections.Generic;
using System.Text;

namespace OpenZiti {
    public static class Logging {
        public static void SimpleConsoleLogging(LogLevel min) {

            var config = new LoggingConfiguration();
            var logconsole = new ConsoleTarget("logconsole") {
                Layout = "[${date:format=yyyy-MM-ddTHH\\:mm\\:ss.fff}Z] ${level:uppercase=true:padding=5}\t${message}\t${exception:format=tostring}",
            };

            // Rules for mapping loggers to targets            
            config.AddRule(min, LogLevel.Fatal, logconsole);

            // Apply config           
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
        }
    }
}
