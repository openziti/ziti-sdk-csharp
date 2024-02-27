using NLog.Fluent;
using System.IO;
using System.Text;

namespace OpenZiti.NET.Samples.src.Common;
internal class AppetizerSetup {
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
    internal static ZitiContext ContextFromFile(string idFile) {
        if (idFile.EndsWith(".json")) {
            // good - we'll just use it
        } else if (idFile.EndsWith(".jwt")) {
            // infer it's a jwt to be enrolled... strip .jwt and find a .json and use THAT if it's here...
            var idFileJson = idFile.Replace(".jwt", ".json");
            if (File.Exists(idFileJson)) {
                // use it
                idFile = idFileJson;
            } else {
                // assume we need to enroll the file
                Log.Info($"{idFileJson} doesn't exist. Assuming this is a token to enroll...");

                var strongIdentity = API.EnrollIdentityFile(idFile);
                File.WriteAllBytes($"{idFileJson}", Encoding.UTF8.GetBytes(strongIdentity));
                Log.Info($"Strong identity written to: {idFileJson}");
                idFile = idFileJson;
            }
        }

        var idFileBytes = File.ReadAllText(idFile);
        var c = new ZitiContext(idFileBytes); //demonstrates loading an identity via json, not as a file
        return c;
    }
}
