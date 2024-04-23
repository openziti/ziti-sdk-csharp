using System.Buffers;
using System.Buffers.Binary;
using System.Diagnostics;
using System.IO.Pipelines;
using System.Net;
using System.Net.Sockets;
using System.Threading.Channels;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using OpenZiti;

internal class ZitiConnectionListenerFactory : IConnectionListenerFactory
{
    private ILogger<ZitiConnectionListenerFactory> _logger;
    private ZitiSocket _zitiSocket;

    public ZitiConnectionListenerFactory(ILogger<ZitiConnectionListenerFactory> logger)
    {
        _logger = logger;
    }

    public async ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default)
    {
        #region OptionZiti
        API.SetLogLevel(ZitiLogLevel.INFO);
        _zitiSocket = new ZitiSocket(SocketType.Stream);
        ZitiContext ctx = new ZitiContext("C:\\OpenZiti\\CSharp-RestApi-Server.json");
        var svcName = "CSharp-Service";
        string terminator = "";

        API.Bind(_zitiSocket, ctx, svcName, terminator);
        Console.WriteLine("Bound to Ziti");
        API.Listen(_zitiSocket, 100);
        Console.WriteLine("Listening on Ziti");
        #endregion

        return new SocketListener(_zitiSocket, _logger);
    }

    class SocketListener : IConnectionListener
    {
        private ZitiSocket _zitiSocket;
        private Socket _socket;
        private readonly Channel<ConnectionContext> _channel = Channel.CreateBounded<ConnectionContext>(20);
        private readonly SocketConnectionContextFactory _contextFactory;

        public SocketListener(ZitiSocket zitiSocket, ILogger logger)
        {
            _zitiSocket = zitiSocket;
            _socket = zitiSocket.ToSocket();
            _contextFactory = new(new(), logger);
        }

        public EndPoint EndPoint => _socket.LocalEndPoint!;

        public async ValueTask DisposeAsync()
        {
        }

        public ValueTask UnbindAsync(CancellationToken cancellationToken = default)
        {
            return default;
        }

        public async ValueTask<ConnectionContext?> AcceptAsync(CancellationToken cancellationToken = default)
        {
            try
            {
                while (true)
                {
                    ZitiSocket client = API.Accept(_zitiSocket, out var caller);
                    Console.WriteLine("Accepted connection from an Authorized Ziti Client");
                    var socket = client.ToSocket();
                    if (socket.RemoteEndPoint is IPEndPoint remoteEndPoint)
                    {
                        string clientIpAddress = remoteEndPoint.Address.ToString();
                        int remotePort = remoteEndPoint.Port;
                        Console.WriteLine($"Connection stablished with Authorized Ziti Client IP Address: {clientIpAddress}, Port: {remotePort}");
                    }
                    if (socket.LocalEndPoint is IPEndPoint localEndPoint)
                    {
                        string localIpAddress = localEndPoint.Address.ToString();
                        int localPort = localEndPoint.Port;
                        Console.WriteLine($"Connection stablished at IP Address: {localIpAddress}, Port: {localPort}");
                    }
                    return _contextFactory.Create(socket);
                }
            }
            catch (ObjectDisposedException)
            {
            }
            catch (SocketException)
            {
            }
            return null;
        }
    }
}