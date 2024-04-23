using System.Net;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Core;

namespace Microsoft.AspNetCore.Hosting
{
    public static class ServiceCollectionExtensions
    {
        public static IWebHostBuilder UseDelegatedTransport(this IWebHostBuilder hostBuilder)
        {
            return hostBuilder.ConfigureServices(services =>
            {
                services.AddSingleton<IConnectionListenerFactory, ZitiConnectionListenerFactory>();
            });
        }
    }
}