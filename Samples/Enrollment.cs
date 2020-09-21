using System;
using System.IO;

using OpenZiti;

namespace OpenZiti.Samples
{
    class Enrollment
    {
        public static void Run(string[] args)
        {
            CheckUsage(args);
            ZitiEnrollment.Options opts = new ZitiEnrollment.Options()
            {
                Jwt = args[2],
            };

            ZitiEnrollment.Enroll(opts, afterEnrollment, args[3]);
            API.Run();
        }

        private static void afterEnrollment(ZitiEnrollment.EnrollmentResult result, object context)
        {
            string fileName = context.ToString();
            File.WriteAllText(fileName, result.Json);
            Console.WriteLine($"Enrollment successful. File written to {fileName}");
        }

        public static void CheckUsage(string[] args)
        {
            if (args.Length < 4)
            {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} {args[0]} {args[1]} <path-to-jwt-file> <output-path>");
                throw new ArgumentException("too few arguments");
            }
        }
    }
}