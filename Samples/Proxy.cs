using System.Net;

using OpenZiti;

namespace OpenZiti.Samples
{
    class ProxyExample
    {
        public static void Run()
        {
            ZitiOptions opts = new ZitiOptions()
            {
                InitComplete = Util.CheckStatus,
                ServiceChange = proxy_service_cb,
                ConfigFile = @"c:\temp\id.json",
                ServiceRefreshInterval = 15,
                MetricsType = RateType.INSTANT,
                RouterKeepalive = 15,
            };

            ZitiIdentity id = new ZitiIdentity(opts);
            API.Run();
        }

        private static async void proxy_service_cb(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext)
        {
            if (service.Name == "ssh-service")
            {
                //start a listener on the socket
                await TcpProxy.RunServerAsync(IPAddress.Any, 2222, service);
            }
        }
    }


}