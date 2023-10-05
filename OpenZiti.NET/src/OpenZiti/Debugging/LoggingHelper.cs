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
    public static void LogToConsole(MLog.LogLevel lvl) {

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
