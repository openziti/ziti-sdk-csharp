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
using OpenZiti.NET.Samples.Common;
using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;

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

            Log.Info("Enrolling the first time. This is expected to succeed");
            var outputFile = Directory.GetCurrentDirectory() + Path.DirectorySeparatorChar + "enroll.demo.json";
            Enroll(jwt, outputFile);
            Log.Info("Identity file written to: " + outputFile);

            //now enroll the same exact token again and expect an error
            Log.Info("Enrolling the _second_ time. This is __expected__ to fail to");
            Log.Info("    illustrate that enrollment may fail");
            Log.Info("");
            try {
                Enroll(jwt, outputFile);
            } catch (Exception ex) {
                Log.Info( "    EXPECTED ERROR: JWT not accepted by controller");
                Log.Info($"    ERROR RECEIVED: {ex.Message}");
                Log.Info("");
            }

            return null;
        }
    }
}
