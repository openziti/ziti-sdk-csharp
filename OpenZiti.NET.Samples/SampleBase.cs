using System;
using System.IO;
using System.Text;

namespace OpenZiti.NET.Samples {

    public abstract class SampleBase {
        public static void Enroll(string pathToEnrollmentToken, string outputPath) {
            var strongIdentity = API.EnrollIdentityFile(pathToEnrollmentToken);
            File.WriteAllBytes($"{outputPath}", Encoding.UTF8.GetBytes(strongIdentity));
            Console.WriteLine($"Strong identity enrolled successfully. File saved to: {outputPath}");
        }
    }
}
