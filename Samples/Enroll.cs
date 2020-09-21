using System;
using System.IO;

using OpenZiti;

namespace OpenZiti.Samples
{
    class Enroll
    {
        public static void ExampleEnrollment()
        {
            ZitiEnrollment.Options opts = new ZitiEnrollment.Options()
            {
                Jwt = @"c:\temp\enrollment-test\csharp.jwt",
            };

            ZitiEnrollment.Enroll(opts, afterEnrollment);
            API.Run();
        }

        private static void afterEnrollment(ZitiEnrollment.EnrollmentResult result)
        {
            string fileName = @"c:\temp\enrollment-test\csharp.json";
            File.WriteAllText(fileName, result.Json);
            Console.WriteLine($"Enrollment successful. File written to {fileName}");
        }
    }
}