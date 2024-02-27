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
using System.IO;
using System.Text;
using System.Threading.Tasks;

using OpenZiti.NET.Samples.Common;

namespace OpenZiti.NET.Samples {
    [Sample("enroll")]

    public class EnrollmentSample : SampleBase {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override async Task<object> RunAsync(string[] args) {
            Log.Info("EnrollmentSample starts");
            var enrollDemoIdentityName = "enroll-demo";
            var s = new SampleSetup();
            var id = await s.BootstrapSampleIdentityAsync(enrollDemoIdentityName, null);
            var jwt = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllBytes(jwt, Encoding.UTF8.GetBytes(id.Enrollment.Ott.Jwt));
            
            Console.WriteLine("Enrolling the first time. This is expected to succeed");
            Enroll(jwt, Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "enroll.demo.json");

            //now enroll the same exact token again and expect an error
            Console.WriteLine("Enrolling the _second_ time. This is __expected__ to fail to");
            Console.WriteLine("    illustrate that enrollment may fail");
            Console.WriteLine();
            try {
                Enroll(jwt, Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "enroll.demo.json");
            } catch (Exception ex) {
                Console.WriteLine( "    EXPECTED ERROR: JWT not accepted by controller");
                Console.WriteLine($"    ERROR RECEIVED: {ex.Message}");
                Console.WriteLine();
            }

            return null;
        }
    }
}
