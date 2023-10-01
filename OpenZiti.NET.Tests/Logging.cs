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

using NLog;
using NLog.Config;
using NLog.Targets;
using OpenZiti.Native;
using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;
using Microsoft.Extensions.Logging;

namespace OpenZiti.NET.Tests {
    public static class Logging {
        public static void SimpleConsoleLogging(Microsoft.Extensions.Logging.LogLevel lvl) {

            NLog.LogLevel logLevel = NLog.LogLevel.Fatal;
            logLevel = lvl switch {
                Microsoft.Extensions.Logging.LogLevel.Trace => NLog.LogLevel.Trace,
                Microsoft.Extensions.Logging.LogLevel.Debug => NLog.LogLevel.Debug,
                Microsoft.Extensions.Logging.LogLevel.Information => NLog.LogLevel.Info,
                Microsoft.Extensions.Logging.LogLevel.Warning => NLog.LogLevel.Warn,
                Microsoft.Extensions.Logging.LogLevel.Error => NLog.LogLevel.Error,
                Microsoft.Extensions.Logging.LogLevel.Critical => NLog.LogLevel.Fatal,
                Microsoft.Extensions.Logging.LogLevel.None => NLog.LogLevel.Error,// Default to Info if the mapping is not set.
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
}
