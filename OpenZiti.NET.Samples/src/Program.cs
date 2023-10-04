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
using OpenZiti.Generated.Petstore;
using System;
using System.Reflection;
using System.Threading.Tasks;
using MLog = Microsoft.Extensions.Logging;

using OpenZiti.NET.Samples.Common;
using System.Collections.Generic;
using System.Net.Http;

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
                    Console.WriteLine("These samples expect a parameter indicating which sample to run.");
                    Console.WriteLine("Available options are:");
                    
                    //find all the classes with the custom property of "OpenZiti.NET.Samples.Common.Sample"
                    //these are the available samples to run
                    foreach (var type in currentAssembly.GetTypes())
                        if (Attribute.IsDefined(type, typeof(Sample)))
                        {
                            var sample = (Sample)Attribute.GetCustomAttribute(type, typeof(Sample));
                            Console.WriteLine("  - " + sample?.Name);
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
                
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample execution completed successfully");
                Console.WriteLine("==============================================================");
            } catch (Exception e) {
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample failed to execute: " + e.Message);
                Console.WriteLine("");
                Console.WriteLine(e.StackTrace);
                Console.WriteLine("==============================================================");
            }
        }
    }
}
