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

using System;
using System.Reflection;
using System.Threading.Tasks;
using MLog = Microsoft.Extensions.Logging;

using OpenZiti.NET.Samples.Common;

namespace OpenZiti.NET.Samples {
    public class Program {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private static async Task Main(string[] args) {
            try {
                try { Console.Clear(); } catch (Exception) { /*ignore exceptions*/ }
                Debugging.LoggingHelper.LogToConsole(MLog.LogLevel.Trace);
                API.NativeLogger = API.DefaultNativeLogFunction;

                var currentAssembly = Assembly.GetExecutingAssembly();
                if (args == null || args.Length < 1) {
                    Log.Info("These samples expect a parameter indicating which sample to run.");
                    Log.Info("Available options are:");
                    
                    foreach (var type in currentAssembly.GetTypes())
                        if (Attribute.IsDefined(type, typeof(Sample)))
                        {
                            var sample = (Sample)Attribute.GetCustomAttribute(type, typeof(Sample));
                            Log.Info("  - " + sample?.Name);
                        }
                    return;
                }
                
                foreach (var type in currentAssembly.GetTypes())
                    if (Attribute.IsDefined(type, typeof(Sample)))
                    {
                        var attr = (Sample)Attribute.GetCustomAttribute(type, typeof(Sample));
                        if (attr?.Name == args[0]) {
                            var sample = (SampleBase)Activator.CreateInstance(type);
                            await sample.RunAsync();
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
