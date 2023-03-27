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
using System.Net.Http;
using System.Text;

namespace OpenZiti.Samples {

    public class Enrollment : SampleBase {
        public static void Run(string[] args) {
            if (args == null || args.Length < 2) {
                throw new Exception("This example expects the second paramter to be an unenrolled .jwt");
            }
            Console.WriteLine("Enrolling the first time. This is expected to succeed");
            Enroll(args[1], Directory.GetCurrentDirectory() + "/enroll.demo.json");

            //now enroll the same exact token again and expect an error
            Console.WriteLine();
            Console.WriteLine("Enrolling the _second_ time. This is __expected__ to fail to");
            Console.WriteLine("    illustrate that enrollment may fail");
            Console.WriteLine();
            try {
                Enroll(args[1], Directory.GetCurrentDirectory() + "/enroll.demo.json");
            } catch (Exception ex) {
                Console.WriteLine($"    EXPECTED ERROR: JWT not accepted by controller");
                Console.WriteLine($"    ERROR RECEIVED: {ex.Message}");
                Console.WriteLine();
            }
        }
    }
}
