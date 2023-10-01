using NLog.Config;
using NLog.Targets;
using NLog;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using MLog = Microsoft.Extensions.Logging;

namespace OpenZiti.Debugging;
public class LoggingHelper {
    public static void SimpleConsoleLogging(MLog.LogLevel lvl) {

        NLog.LogLevel logLevel = NLog.LogLevel.Fatal;
        logLevel = lvl switch {
            MLog.LogLevel.Trace => NLog.LogLevel.Trace,
            MLog.LogLevel.Debug => NLog.LogLevel.Debug,
            MLog.LogLevel.Information => NLog.LogLevel.Info,
            MLog.LogLevel.Warning => NLog.LogLevel.Warn,
            MLog.LogLevel.Error => NLog.LogLevel.Error,
            MLog.LogLevel.Critical => NLog.LogLevel.Fatal,
            MLog.LogLevel.None => NLog.LogLevel.Error,// Default to Info if the mapping is not set.
            _ => NLog.LogLevel.Info,// Default to Info if the mapping is not found.
        };
        var config = new LoggingConfiguration();
        var logconsole = new ConsoleTarget("logconsole") {
            Layout = "[${date:format=yyyy-MM-ddTHH\\:mm\\:ss.fff}Z] ${level:uppercase=true:padding=5}\t${message}\t${exception:format=tostring}",
        };

        // Rules for mapping loggers to targets            
        config.AddRule(logLevel, NLog.LogLevel.Fatal, logconsole);

        // Apply config           
        LogManager.Configuration = config;
        LogManager.ReconfigExistingLoggers();
    }
}
