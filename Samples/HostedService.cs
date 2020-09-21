using System;
using System.Text;

using OpenZiti;

namespace OpenZiti.Samples
{
    class HostedService
    {
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

        private static void onServiceChange(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext)
        {
            if (service.Name == serviceContext.ToString())
            {
                //start a listener on the socket
                service.Listen(listenCallback, onClientConnect);
            }
            else
            {
                Console.WriteLine($"Service {service.Name} is not the service we're looking for...");
            }
        }

        private static void afterDataWritten(ZitiConnection connection, ZitiStatus status, object context)
        {
            //don't do anything in c# - nothing to free up here
            Console.WriteLine("data has been written");
        }

        private static void listenCallback(ZitiConnection connection, ZitiStatus status)
        {
            if (status == ZitiStatus.OK)
            {
                Console.WriteLine("Byte Counter is ready! %s", status.GetDescription());
            }
            else
            {
                Console.WriteLine("ERROR: The Byte Counter could not be started! " + status.GetDescription());
                connection.Close();
                Environment.Exit(0);
            }
        }

        private static void onClientConnect(ZitiConnection serverConnection, ZitiConnection clientConnection, ZitiStatus status)
        {
            Console.WriteLine("Client is connected");
            clientConnection.Accept(onClientAccept, onClientData);
        }

        private static void onClientAccept(ZitiConnection clientConnection, ZitiStatus status)
        {
            ZitiUtil.CheckStatus(status);

            string msg = "Hello from byte counter!";
            clientConnection.Write(Encoding.UTF8.GetBytes(msg), msg.Length, afterDataWritten, ZitiUtil.NO_CONTEXT);
        }

        private static void onClientData(ZitiConnection clientConnection, byte[] data, int len, ZitiStatus status)
        {
            if (status == ZitiStatus.OK)
            {
                string recd = Encoding.UTF8.GetString(data);
                Console.WriteLine("client sent: " + recd);

                string reply = "counted bytes: " + recd.Length;
                clientConnection.Write(Encoding.UTF8.GetBytes(reply), reply.Length, aftterDataWritten, ZitiUtil.NO_CONTEXT);
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
                clientConnection.Close();
                Environment.Exit(0);
            }
        }

        private static void aftterDataWritten(ZitiConnection connection, ZitiStatus status, object context)
        {
            ZitiUtil.CheckStatus(status);
        }

        public static void CheckUsage(string[] args)
        {
            if (args.Length < 3)
            {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} {args[0]} {args[1]} <service-name-to-host>");
                throw new ArgumentException("too few arguments");
            }
        }
    }
}