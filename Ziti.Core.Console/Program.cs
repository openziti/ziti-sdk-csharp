using System;
using System.IO;
using System.Text;
using System.Threading.Tasks;
using NetFoundry;

namespace Ziti.Core.Example
{
    class Program
    {
        static async Task Main(string[] args)
        {
            byte[] wttrRequestAsBytes = Encoding.UTF8.GetBytes("GET /Rochester HTTP/1.0\r\n"
                                                               + "Accept: *-/*\r\n"
                                                               + "Connection: close\r\n"
                                                               + "User-Agent: curl/7.59.0\r\n"
                                                               + "Host: wttr.in\r\n"
                                                               + "\r\n");


            string path = @"c:/path/to/enrolled.id.json";

            //makes the output pretty - and not jumbly
            Console.OutputEncoding = Encoding.UTF8;

            /* Only needed when debugging
            Environment.SetEnvironmentVariable("ZITI_LOG", "6");
            NetFoundry.Ziti.OutputDebugInformation = true;
            */

            ZitiIdentity id = new ZitiIdentity(path);
            id.InitializeAndRun(); //connect to the Ziti network

            //make a new stream using the identity
            ZitiStream zitiStream = new ZitiStream(id.NewConnection("demo-weather"));

            //send the reqeust
            await zitiStream.WriteAsync(wttrRequestAsBytes, 0, wttrRequestAsBytes.Length);

            using (MemoryStream ms = new MemoryStream())
            using (StreamReader sr = new StreamReader(ms))
            {
                //display the bytes by reading from the stream and writing to the console
                await LocalPumpAsync(zitiStream, System.Console.OpenStandardOutput());

                string output = sr.ReadToEnd();
                System.Diagnostics.Debug.WriteLine(output);
            }
        }

        private const int DefaultStreamPumpBufferSize = 64 * 1024;

        public static async Task LocalPumpAsync(Stream input, Stream destination)
        {
            int count = DefaultStreamPumpBufferSize;
            byte[] buffer = new byte[count];

            int numRead = await input.ReadAsync(buffer, 0, count).ConfigureAwait(false);

            while (numRead > 0)
            {
                destination.Write(buffer, 0, numRead);
                //writes are synchronous for now - without syncronous writes there's a lock that's
                //not freeing up
                //await destination.WriteAsync(buffer, 0, numRead).ConfigureAwait(false);


                numRead = await input.ReadAsync(buffer, 0, count).ConfigureAwait(false);
            }
        }
    }
}
