using System;
using System.IO;

using OpenZiti;

namespace OpenZiti.Samples {
    class Enrollment {
        public static void Run(string path) {
            string outputFile = path.Replace(".jwt", ".json");
            API.Enroll(path, afterEnroll, outputFile);
            API.Run();
        }

        private static void afterEnroll(ZitiEnrollment.EnrollmentResult result) {
            Console.WriteLine("ZITI STATUS : " + result.Status);
            Console.WriteLine("    MESSAGE : " + result.Message);
            if (result.Status == ZitiStatus.OK) {
                Console.WriteLine("    ID.Cert : {0}[...]", result.ZitiIdentity.IdMaterial.Certificate.Substring(0, 25));
                Console.WriteLine("     ID.Key : {0}[...]", result.ZitiIdentity.IdMaterial.Key.Substring(0, 25));
                Console.WriteLine("      ID.CA : {0}[...]", result.ZitiIdentity.IdMaterial.CA.Substring(0, 25));
                //write identity to file...
                System.IO.File.WriteAllText((string)result.Context, result.Json);
                Console.WriteLine("written to  : {0}", result.Context);
            } else {
                Console.WriteLine("    ID.Cert : empty");
                Console.WriteLine("     ID.Key : empty");
                Console.WriteLine("      ID.CA : empty");
            }
        }
    }
}