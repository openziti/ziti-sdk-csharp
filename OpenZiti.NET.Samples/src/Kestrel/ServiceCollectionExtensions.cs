using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Hosting;
using Microsoft.Extensions.DependencyInjection;
using System;
using System.Net;

namespace OpenZiti.Samples.Kestrel;

public static class WebHostBuilderExtensions {
    public static IWebHostBuilder UseZitiTransport(this IWebHostBuilder hostBuilder, string identity, string service) {
        var endpoint = new ZitiEndPoint(identity: identity, serviceName: service);
        hostBuilder.ConfigureServices(services => {
            services.AddSingleton<IConnectionListenerFactory, ZitiConnectionListenerFactoryFixed>();
        });
        hostBuilder.ConfigureKestrel(o => {
            // Clear any URLs configured by UseUrls to avoid conflicts
            // If you want to listen on BOTH Ziti AND regular TCP, add both endpoints here
            o.Listen(endpoint);
        });
        return hostBuilder;
    }

    public static IWebHostBuilder UseZitiTransportAndUrls(
        this IWebHostBuilder hostBuilder,
        string identity,
        string service,
        string url) {

        var zitiEndpoint = new ZitiEndPoint(identity: identity, serviceName: service);

        hostBuilder.ConfigureServices(services => {
            services.AddSingleton<IConnectionListenerFactory, ZitiConnectionListenerFactoryFixed>();
        });

        hostBuilder.ConfigureKestrel(o => {
            // Listen on both Ziti and TCP
            o.Listen(zitiEndpoint);

            // Parse URL and add TCP endpoint
            if (url.StartsWith("http://")) {
                var uri = new Uri(url);
                o.Listen(IPAddress.Parse(uri.Host), uri.Port);
            }
        });

        return hostBuilder;
    }
}
