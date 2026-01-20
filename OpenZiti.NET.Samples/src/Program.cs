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

using OpenZiti.Debugging;
using OpenZiti.NET.Samples.Common;
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using System.Threading.Tasks;
using MLog = Microsoft.Extensions.Logging;

namespace OpenZiti.NET.Samples {
    public class Program {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private static async Task Main(string[] args) {
            try {
                var logLvl = MLog.LogLevel.Information;
                LoggingHelper.LogToConsole(logLvl);
                API.SetLogLevel(logLvl);
                API.SetLoggerFunc(API.NativeLogger);

                try { Console.Clear(); } catch (Exception) { /*ignore exceptions from this*/ }

                Log.Warn("logging established using log level: {}...", logLvl);

                var currentAssembly = Assembly.GetExecutingAssembly();
                if (args == null || args.Length < 1) {
                    Log.Info("These samples expect a parameter indicating which sample to run.");
                    Log.Info("Available options are:");

                    //find all the classes with the custom property of "OpenZiti.NET.Samples.Common.Sample"
                    //these are the available samples to run
                    foreach (var type in currentAssembly.GetTypes()) {
                        if (Attribute.IsDefined(type, typeof(Sample))) {
                            var sample = (Sample)Attribute.GetCustomAttribute(type, typeof(Sample));
                            Log.Info("  - " + sample?.Name);
                        }
                    }
                    return;
                }

                if ( args[0].Contains(AppDomain.CurrentDomain.FriendlyName )) {
                    Log.Info("args[0] contains the AppDomain.CurrentDomain.FriendlyName, must be using dotnet run.");
                    args = args.Skip(1).ToArray();
                }

                if (args.Length > 1) {
                    SampleSetup.Initialize = args[1]?.ToLower().Trim() != "noinit";
                    if (args.Length > 2) {
                        SampleSetup.IdentityFile = args[2];
                    }
                } else {
                    SampleSetup.Initialize = true;
                }
                
                foreach (var type in currentAssembly.GetTypes())
                    if (Attribute.IsDefined(type, typeof(Sample)))
                    {
                        var attr = (Sample)Attribute.GetCustomAttribute(type, typeof(Sample));
                        if (attr?.Name == args[0]) {
                            var sample = (SampleBase)Activator.CreateInstance(type);
                            await sample.RunAsync(args.Skip(1).ToArray());
                        }
                    }
                
                Log.Info("==============================================================");
                Log.Info("Sample execution completed successfully");
                Log.Info("==============================================================");
            } catch (Exception e) {
                Log.Info("==============================================================");
                Log.Info("Sample failed to execute: " + e.Message);
                Log.Info("");
                Log.Info(e.StackTrace);
                Log.Info("==============================================================");
            }
        }
    }
}
