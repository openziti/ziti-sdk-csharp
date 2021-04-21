using System;
using System.Linq;
using System.Text;
using System.Threading;
using System.Threading.Tasks;
using OpenZiti;

namespace OpenZiti.Samples {
    class HostedService {

        private class HostedContext {
            internal bool isServer;
            internal string serviceName;
        }
        static string hostedServiceName = "HostedExample";
        private static HostedContext server = new HostedContext { isServer = true, serviceName = hostedServiceName };
        private static HostedContext client = new HostedContext { isServer = false, serviceName = hostedServiceName };

        public async static Task RunAsync(string identityFile) {
            await Task.WhenAll(RunAsync2(identityFile, server), RunAsync2(identityFile, client)).ConfigureAwait(false);
            Console.WriteLine("=================LOOP IS COMPLETE=================");
        }

        private async static Task RunAsync2(string identityFile, HostedContext context) {
            ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent,
                IdentityFile = identityFile,
                ApplicationContext = context,
            };
            opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
            opts.OnZitiServiceEvent += Opts_OnZitiServiceEvent;

            ZitiIdentity zid = new ZitiIdentity(opts);
            zid.Loop = API.NewLoop();
            await zid.RunAsync().ConfigureAwait(false);
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e) {
            Console.WriteLine("on ziti context event: " + e.Status + " : " + e.Status.GetDescription());
        }

        private static void Opts_OnZitiServiceEvent(object sender, ZitiServiceEvent e) {
            HostedContext c = (HostedContext)e.Context;
            var service = e.Added().First(s => s.Name == c.serviceName);
            if (service != null) {
                if (c.isServer) {
                    service.Listen(listenCallback, onClientConnect);
                } else {
                    service.Dial(onConnected, onData);
                }
            } else {
                Console.WriteLine("ERROR: Could not find the service we want?");
            }
        }

        private static void onConnected(ZitiConnection connection, ZitiStatus status) {
            connection.Write(Encoding.UTF8.GetBytes("greetings!"), afterDataWritten, "context?");

        }

        private static void onData(ZitiConnection connection, ZitiStatus status, byte[] data) {
            Console.WriteLine(Encoding.UTF8.GetString(data));
        }

        private void spacer() {

































        }

        private static void onServiceChange(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext) {
            if (service.Name == serviceContext.ToString()) {
                //start a listener on the socket
                service.Listen(listenCallback, onClientConnect);
            } else {
                Console.WriteLine($"Service {service.Name} is not the service we're looking for...");
            }
        }

        private static void afterDataWritten(ZitiConnection connection, ZitiStatus status, object context) {
            //don't do anything in c# - nothing to free up here
            Console.WriteLine("data has been written: " + context?.ToString());
        }

        private static void listenCallback(ZitiConnection connection, ZitiStatus status) {
            if (status == ZitiStatus.OK) {
                Console.WriteLine("Byte Counter is ready! %s", status.GetDescription());
            } else {
                Console.WriteLine("ERROR: The Byte Counter could not be started! " + status.GetDescription());
                connection.Close();
                Environment.Exit(0);
            }
        }

        private static void onClientConnect(ZitiConnection serverConnection, ZitiConnection clientConnection, ZitiStatus status) {
            Console.WriteLine("Client is connected");
            clientConnection.Accept(onClientAccept, onClientData);
        }

        private static void onClientAccept(ZitiConnection clientConnection, ZitiStatus status) {
            ZitiUtil.CheckStatus(status);

            string msg = "Hello from byte counter!";
            clientConnection.Write(Encoding.UTF8.GetBytes(msg), msg.Length, afterDataWritten, ZitiUtil.NO_CONTEXT);
        }

        private static void onClientData(ZitiConnection clientConnection, byte[] data, int len, ZitiStatus status) {
            if (status == ZitiStatus.OK) {
                string recd = Encoding.UTF8.GetString(data);
                Console.WriteLine("client sent: " + recd);

                string reply = "counted bytes: " + recd.Length;
                clientConnection.Write(Encoding.UTF8.GetBytes(reply), reply.Length, aftterDataWritten, ZitiUtil.NO_CONTEXT);
            } else {
                if (status == ZitiStatus.EOF) {
                    Console.WriteLine("request completed: " + status.GetDescription());
                } else {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                clientConnection.Close();
                Environment.Exit(0);
            }
        }

        private static void aftterDataWritten(ZitiConnection connection, ZitiStatus status, object context) {
            ZitiUtil.CheckStatus(status);
        }

        public static void CheckUsage(string[] args) {
            if (args.Length < 3) {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} {args[0]} {args[1]} <service-name-to-host>");
                throw new ArgumentException("too few arguments");
            }
        }
    }
    class HostedService2 {

        private class HostedContext {
            internal bool isServer;
            internal string serviceName;
        }
        static string hostedServiceName = "HostedExample";
        static HostedContext server = new HostedContext { isServer = true, serviceName = hostedServiceName };
        static HostedContext client = new HostedContext { isServer = false, serviceName = hostedServiceName };

        public static void Run(string identityFile, bool isServer) {
            if (isServer) {
                ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                    EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent,
                    IdentityFile = identityFile,
                    ApplicationContext = server,
                };
                opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
                opts.OnZitiServiceEvent += Opts_OnZitiServiceEvent;

                ZitiIdentity zid = new ZitiIdentity(opts);
                zid.Run();
                Console.WriteLine("=================LOOP IS COMPLETE=================");
            } else {
                ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                    EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent,
                    IdentityFile = identityFile,
                    ApplicationContext = client,
                };
                opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
                opts.OnZitiServiceEvent += Opts_OnZitiServiceEvent;

                ZitiIdentity zid = new ZitiIdentity(opts);
                zid.Run();
                Console.WriteLine("=================LOOP IS COMPLETE=================");
            }
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e) {
            CommonMethods.CheckStatus(e.Status);
        }

        private static void Opts_OnZitiServiceEvent(object sender, ZitiServiceEvent e) {
            HostedContext c = (HostedContext)e.Context;
            var service = e.Added().First(s => s.Name == c.serviceName);
            if (service != null) {
                service.Listen(listenCallback, onClientConnect);
            } else {
                Console.WriteLine("ERROR: Could not find the service we want?");
            }
        }
        private static void onData(ZitiConnection connection, ZitiStatus status, byte[] data) {

        }

        private void spacer() {

































        }

        private static void onServiceChange(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext) {
            if (service.Name == serviceContext.ToString()) {
                //start a listener on the socket
                service.Listen(listenCallback, onClientConnect);
            } else {
                Console.WriteLine($"Service {service.Name} is not the service we're looking for...");
            }
        }

        private static void afterDataWritten(ZitiConnection connection, ZitiStatus status, object context) {
            //don't do anything in c# - nothing to free up here
            Console.WriteLine("data has been written");
        }

        private static void listenCallback(ZitiConnection connection, ZitiStatus status) {
            if (status == ZitiStatus.OK) {
                Console.WriteLine("Byte Counter is ready! %s", status.GetDescription());
            } else {
                Console.WriteLine("ERROR: The Byte Counter could not be started! " + status.GetDescription());
                connection.Close();
                Environment.Exit(0);
            }
        }

        private static void onClientConnect(ZitiConnection serverConnection, ZitiConnection clientConnection, ZitiStatus status) {
            Console.WriteLine("Client is connected");
            clientConnection.Accept(onClientAccept, onClientData);
        }

        private static void onClientAccept(ZitiConnection clientConnection, ZitiStatus status) {
            ZitiUtil.CheckStatus(status);

            string msg = "Hello from byte counter!";
            clientConnection.Write(Encoding.UTF8.GetBytes(msg), msg.Length, afterDataWritten, ZitiUtil.NO_CONTEXT);
        }

        private static void onClientData(ZitiConnection clientConnection, byte[] data, int len, ZitiStatus status) {
            if (status == ZitiStatus.OK) {
                string recd = Encoding.UTF8.GetString(data);
                Console.WriteLine("client sent: " + recd);

                string reply = "counted bytes: " + recd.Length;
                clientConnection.Write(Encoding.UTF8.GetBytes(reply), reply.Length, aftterDataWritten, ZitiUtil.NO_CONTEXT);
            } else {
                if (status == ZitiStatus.EOF) {
                    Console.WriteLine("request completed: " + status.GetDescription());
                } else {
                    Console.WriteLine("unexpected error: " + status.GetDescription());
                }
                clientConnection.Close();
                Environment.Exit(0);
            }
        }

        private static void aftterDataWritten(ZitiConnection connection, ZitiStatus status, object context) {
            ZitiUtil.CheckStatus(status);
        }

        public static void CheckUsage(string[] args) {
            if (args.Length < 3) {
                string appname = System.AppDomain.CurrentDomain.FriendlyName;
                Console.WriteLine("Usage:");
                Console.WriteLine($"\t{appname} {args[0]} {args[1]} <service-name-to-host>");
                throw new ArgumentException("too few arguments");
            }
        }
    }
}