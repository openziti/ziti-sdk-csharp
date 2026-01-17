using OpenZiti.NET.Samples.Common;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

namespace OpenZiti.NET.Samples.src.Kestrel;


[Sample("kestrel-client")]
public class KestrelClientSample : SampleBase {
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

    public override async Task<object> RunAsync(string[] args) {
        Log.Info("Kestrel Client starts");
        var svcName = "kestrel-svc";
        var setupResult = await new SampleSetup(new()).SetupKestrelClientExample(svcName);
        Log.Info("Identity file located at: " + setupResult);
        var idFileBytes = File.ReadAllText(setupResult);
        var c = new ZitiContext(idFileBytes); //demonstrates loading an identity via json, not as a file
        var zitiSocketHandler = c.NewZitiSocketHandler(svcName);
        var client = new HttpClient(new Debugging.LoggingHandler(zitiSocketHandler));
        client.DefaultRequestHeaders.Add("User-Agent", "curl/7.59.0");

        var result = client.GetStringAsync(args[0]).Result;
        Log.Info("result: {}", result);

        return result;
    }
}
