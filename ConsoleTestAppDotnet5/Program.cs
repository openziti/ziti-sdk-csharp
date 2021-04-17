using System;
using System.Threading.Tasks;

using OpenZiti;
using NLog;
using NLog.Config;
using NLog.Targets;
using System.Threading;
using System.Runtime.InteropServices;

namespace ConsoleTestApp {
    class Program {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        static void Main(string[] args) {
            Logging.SimpleConsoleLogging(LogLevel.Info);
            //enrollExample();
            string ctx = "some context";
            loadIdExample(ctx);
        }

        static void enrollExample() {
            API.Enroll(@"c:\temp\id.jwt", afterEnroll);
            API.Run();
        }

        
        static void loadIdExample(object ctx) {

            var zid = new ZitiIdentity();
            ZitiIdentity.InitOptions opts = new ZitiIdentity.InitOptions() {
                EventFlags = ZitiEventFlags.ZitiContextEvent | ZitiEventFlags.ZitiServiceEvent | ZitiEventFlags.ZitiRouterEvent,
                RefreshInterval = 5,
                MetricType = RateType.INSTANT,
                IdentityFile = @"c:\temp\pn.json",
                ApplicationContext = ctx,
                ConfigurationTypes = new string[] { "all" },
            };

            opts.OnZitiContextEvent += Opts_OnZitiContextEvent;
            opts.OnZitiRouterEvent += Opts_OnZitiRouterEvent;
            opts.OnZitiServiceEvent += Opts_OnZitiServiceEvent;
            zid.Run(opts);
            Logger.Error("=================LOOP IS COMPLETE=================");
        }

        private static void Opts_OnZitiContextEvent(object sender, ZitiContextEvent e) {
            Logger.Error("I have a context event: {0}", e.Name);
        }

        private static void Opts_OnZitiServiceEvent(object sender, ZitiServiceEvent e) {
            foreach (ZitiService svc in e.Removed()) {
                Logger.Error("REMOVED SERVICE: {0}", svc.Name);
            }
            foreach (ZitiService svc in e.Changed()) {
                Logger.Error("CHANGED SERVICE: {0}", svc.Name);
            }
            foreach (ZitiService svc in e.Added()) {
                Logger.Error("ADDED SERVICE: {0}", svc.Name);
                if(svc.Name == "eth0mfa") {
                    svc.Dial(onConnected, onData);
                }
            }
        }

        private static void onConnected(ZitiConnection connection, ZitiStatus status) {
            Logger.Error("hurray. we are connected...");
            if (status.Ok()) {
                string payload = @"GET / HTTP/1.0
Host: eth0.me
User-Agent: curl/7.55.1
Accept: */*

";
                connection.Write(System.Text.Encoding.UTF8.GetBytes(payload), onDataWritten, "no context needed");
                Logger.Error("hurray. we wrote some data to ziti... maybe...");
            } else {
                Logger.Error("UGH! something has gone horribly wrong.... {0}", status.GetDescription());
            }
        }

        private static void onDataWritten(ZitiConnection connection, ZitiStatus status, object context) {
            Logger.Error("onDataWritten called but who cares?");
        }

        private static void onData(ZitiConnection connection, ZitiStatus status, byte[] data) {
            if (status.Ok()) {
                Logger.Error("bytes have been received: " + System.Text.Encoding.UTF8.GetString(data));
            } else {
                if (status == ZitiStatus.EOF) {
                    Logger.Error("We are done here! woo hoo");
                } else {
                    Logger.Error("something went terriblywrong");
                }
            }
        }

        private static void Opts_OnZitiRouterEvent(object sender, ZitiRouterEvent e) {
            switch (e.Type) {
                case RouterEventType.EdgeRouterConnected:
                    Logger.Info("Connected to edge router: {0} running: {1}", e.Name, e.Version);
                    break;
                case RouterEventType.EdgeRouterDisconnected:
                    Logger.Info("Disconnected from edge router: {0}", e.Name);
                    break;
                case RouterEventType.EdgeRouterRemoved:
                    Logger.Info("Edge router removed: {0}", e.Name);
                    break;
                case RouterEventType.EdgeRouterUnavailable:
                    Logger.Info("Edge router has become unavailable: {0}", e.Name);
                    break;
                default:
                    Logger.Warn("UNEXPECTED RouterEvents [{0}]! Please report.", e.Type);
                    break;
            }
        }

        static object simpleLock = new object();

        private static void afterEnroll(API.Enrollment.Result result) {
            Logger.Error("ZITI STATUS : " + result.Status);
            Logger.Error("    MESSAGE : " + result.Message);
            if (result.Status == ZitiStatus.OK) {
                Logger.Error("    ID.Cert : {0}[...]", result.IdInfo.IdMaterial.Certificate.Substring(0, 25));
                Logger.Error("     ID.Key : {0}[...]", result.IdInfo.IdMaterial.Key.Substring(0, 25));
                Logger.Error("      ID.CA : {0}[...]", result.IdInfo.IdMaterial.CA.Substring(0, 25));
            } else {
                Logger.Error("    ID.Cert : empty");
                Logger.Error("     ID.Key : empty");
                Logger.Error("      ID.CA : empty");
            }
            lock (simpleLock) {
                Monitor.Pulse(simpleLock);
            }
        }
    }
}