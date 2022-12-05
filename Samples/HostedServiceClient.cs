using System;
using System.IO;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

namespace OpenZiti.Samples {
    public class HostedServiceClient : SampleBase {
        public static async Task Run(string[] args) {
            if (args == null || args.Length < 2) {
                throw new Exception("This example expects the second paramter to be an unenrolled .jwt");
            }
            var outputPath = Directory.GetCurrentDirectory() + "/hosted.demo.client.json";
            try {
                Enroll(args[1], outputPath);
            } catch(Exception e) {
                Console.WriteLine($"WARN: the jwt was not enrolled properly: {e.Message}");
            }

            ZitiContext ctx = new ZitiContext(outputPath);
            string svc = "hosted-svc";
            string terminator = "";

            ZitiSocket socketa = new ZitiSocket(SocketType.Stream);
            ZitiSocket socketb = API.Connect(socketa, ctx, svc, terminator);
            using (var s = socketb.ToNetworkStream())
            using (var r = new StreamReader(s))
            using (var w = new StreamWriter(s)) {
                string line = "initial";
                while (line.Length > 0) {
                    line = Console.ReadLine();
                    await w.WriteLineAsync(line);
                    w.AutoFlush = true;
                    Console.WriteLine("done sending. moving to read response");

                    string read = await r.ReadLineAsync();
                    Console.WriteLine($"Read:\n{read}");
                }
            }
        }
    }
}
