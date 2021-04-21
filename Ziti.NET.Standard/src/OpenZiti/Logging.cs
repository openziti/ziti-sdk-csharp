/*
Copyright 2019 NetFoundry, Inc.

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
