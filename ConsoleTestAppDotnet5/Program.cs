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
                IdentityFile = @"c:\temp\id.json",
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
            foreach (IntPtr p in e.Removed()) {
                if (p != IntPtr.Zero) {
                    Logger.Error("REMOVED SERVICE:");
                    ziti_service svc = Marshal.PtrToStructure<ziti_service>(p);
                }
            }
            foreach (IntPtr p in e.Changed()) {
                if (p != IntPtr.Zero) {
                    Logger.Error("UPDATED SERVICE:");
                    ziti_service svc = Marshal.PtrToStructure<ziti_service>(p);
                }
            }
            foreach (IntPtr p in e.Added()) {
                if (p != IntPtr.Zero) {
                    ziti_service svc = Marshal.PtrToStructure<ziti_service>(p);
                    Logger.Error("ADDED SERVICE: {0}", svc.name);
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