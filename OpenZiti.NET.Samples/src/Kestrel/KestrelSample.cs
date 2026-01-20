using Microsoft.AspNetCore.Builder;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.Hosting;
using Microsoft.Extensions.Logging;
using NLog.Web;
using OpenZiti;
using OpenZiti.NET.Samples.Common;
using OpenZiti.Samples.Kestrel;
using System;
using System.Net;
using System.Threading;
using System.Threading.Tasks;

namespace OpenZiti.Samples.Kestrel {

    [Sample("kestrel")]
    public class KestrelApp : SampleBase {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public override async Task<object> RunAsync(string[] args) {
            var svcName = "kestrel-svc";
            var identityFile = await new SampleSetup(new()).SetupKestrelExample(svcName);;

            Log.Info("kestrel sample starts");

            var cts = new CancellationTokenSource();

            // Handle Ctrl-C gracefully
            Console.CancelKeyPress += (sender, e) => {
                Log.Info("Shutdown requested. Stopping application...");
                e.Cancel = true;  // Prevent immediate process termination
                cts.Cancel();     // Signal cancellation to app
            };

            var builder = WebApplication.CreateBuilder();

            builder.Logging.ClearProviders();
            builder.Host.UseNLog();

            // Option 1: Listen on BOTH Ziti and TCP
            builder.WebHost.UseZitiTransportAndUrls(identityFile, svcName, "http://127.0.0.1:80");

            // Option 2: Listen ONLY on Ziti (comment out above, uncomment below)
            // builder.WebHost.UseZitiTransport(identityFile, svcName);

            var app = builder.Build().MapSampleEndpoints();

            try {
                Log.Info("Application starting. Press Ctrl-C to stop.");
                await app.RunAsync(cts.Token);
                Log.Info("Application stopped gracefully.");
            }
            catch (OperationCanceledException) {
                Log.Error("Application shutdown completed.");
            }
            finally {
                cts.Dispose();
            }

            return null;
        }
    }

    public static class SampleEndpoints {
        public static WebApplication MapSampleEndpoints(this WebApplication app) {
            app.MapGet("/", () => new {
                service = "kestrel-ziti-sample",
                utc = System.DateTime.UtcNow
            });

            app.MapGet("/echo/{text}", (string text) => new {
                echo = text + " [this is from the kestrel server... proving that it was accepted]",
                utc = System.DateTime.UtcNow
            });


            var counter = 0;

            app.MapGet("/counter", () => $"Counter: {counter}");
            app.MapGet("/increment", () => $"Counter: {++counter}");
            app.MapGet("/decrement", () => $"Counter: {--counter}");

            return app;
        }
    }
}
