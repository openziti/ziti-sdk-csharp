using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

using OpenZiti;

namespace OpenZiti.Samples
{
    public class TcpProxy
    {
        static NetworkStream hackyStream;
        static ZitiConnection hackyZitiConn;

        public static void Run(string[] args)
        {
            CheckUsage(args);
            ZitiOptions opts = new ZitiOptions()
            {
                InitComplete = ZitiUtil.CheckStatus,
                ServiceChange = onServiceChange,
                ConfigFile = args[0],
                ServiceRefreshInterval = 15,
                MetricsType = RateType.INSTANT,
                RouterKeepalive = 15,
                Context = args[2],
            };

            ZitiIdentity id = new ZitiIdentity(opts);
            API.Run();
        }

        private static async void onServiceChange(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext)
        {
            if (serviceContext.ToString().StartsWith(service.Name))
            {
                UInt16 port = UInt16.Parse(serviceContext.ToString().Split(":")[1]);
                //start a listener on the socket
                await TcpProxy.RunServerAsync(IPAddress.Any, port, service);
            }
        }

        private static void HandleClientAsync(TcpClient client, ZitiService service)
        {
            var stream = client.GetStream();
            // do stuff
            Console.WriteLine("handled a client... whee");
            NetworkStream clientStream = client.GetStream();

            //make ziti connection/service here
            hackyStream = clientStream;
            service.Dial(onConnected, onData);
        }

        private static async void onConnected(ZitiConnection connection, ZitiStatus status)
        {
            Console.WriteLine("great - we're connected... ok then");
            hackyZitiConn = connection;

            await HandleDataFromClient(hackyStream, hackyZitiConn);
        }

        private static async void onData(ZitiConnection connection, ZitiStatus status, byte[] data)
        {
            if (status == ZitiStatus.OK)
            {
                await hackyStream.WriteAsync(data, 0, data.Length);
            }
            else
            {
                if (status == ZitiStatus.EOF)
                {
                    Console.WriteLine("request completed: " + status.GetDescription());
                }
                else
                {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }

                connection.Close();
                Environment.Exit(0);
            }
        }

        private static void afterDataWritten(ZitiConnection connection, ZitiStatus status, object context)
        {
            // called after data is written. often this callback is used to free resources needed to be
            // freed after writing the data
        }

        private static async Task HandleDataFromClient(NetworkStream stream, ZitiConnection connection)
        {
            Console.WriteLine("make sure 'after data' is set on this connection");
            byte[] buffer = new byte[1024];
            while (true)
            {
                try
                {
                    int read = await stream.ReadAsync(buffer, 0, 1024);
                    connection.Write(buffer, read, afterDataWritten, ZitiUtil.NO_CONTEXT);
                }
                catch(Exception /*e*/)
                {

                }
            }
        }

        /// <summary>
        /// Method to be used on seperate thread.
        /// </summary>
        public static async Task RunServerAsync(IPAddress address, UInt16 port, ZitiService svc)
        {
            Console.WriteLine("S: Server started on port {0}", port);
            var listener = new TcpListener(address, port);
            listener.Start();
            while (true)
            {
                var client = await listener.AcceptTcpClientAsync();
                Console.WriteLine("accepted a connection");
                HandleClientAsync(client, svc);
            }
        }

        public static void CheckUsage(string[] args)
        {
            if (args.Length < 3)
            {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} {args[0]} {args[1]} <service-name>:<port-to-listen>");
                throw new ArgumentException("too few arguments");
            }
        }
    }
}