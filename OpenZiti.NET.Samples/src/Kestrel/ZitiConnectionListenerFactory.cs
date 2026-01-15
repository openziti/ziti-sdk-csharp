using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using OpenZiti;

namespace OpenZiti.Samples.Kestrel;

internal class ZitiConnectionListenerFactory : IConnectionListenerFactory {
    private readonly ILogger<ZitiConnectionListenerFactory> _logger;

    public ZitiConnectionListenerFactory(ILogger<ZitiConnectionListenerFactory> logger) {
        _logger = logger;
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        return BindAsync(endpoint, cancellationToken);

        /*
        _logger.LogInformation("Initializing Ziti transport");
        _logger.LogInformation("Identity: {IdentityPath}", endpoint.id);
        _logger.LogInformation("Service: {ServiceName}", serviceName);
        _logger.LogInformation("Log Level: {LogLevel}", logLevel);

        API.SetLogLevel(logLevel);
        var zitiSocket = new ZitiSocket(SocketType.Stream);
        var ctx = new ZitiContext(identityPath);

        API.Bind(zitiSocket, ctx, serviceName, terminator);
        _logger.LogInformation("Bound to Ziti service: {ServiceName}", serviceName);

        API.Listen(zitiSocket, 100);
        _logger.LogInformation("Listening for Ziti connections");

        return new ValueTask<IConnectionListener>(new ZitiSocketListener(zitiSocket, _logger));
        */
    }

    private class ZitiSocketListener : IConnectionListener {
        private readonly ZitiSocket _zitiSocket;
        private readonly Socket _socket;
        private readonly ILogger _logger;
        private readonly SocketConnectionContextFactory _contextFactory;
        private bool _disposed;

        public ZitiSocketListener(ZitiSocket zitiSocket, ILogger logger) {
            _zitiSocket = zitiSocket;
            _socket = zitiSocket.ToSocket();
            _logger = logger;
            _contextFactory = new SocketConnectionContextFactory(new SocketConnectionFactoryOptions(), logger);
        }

        public EndPoint EndPoint => _socket.LocalEndPoint ?? new IPEndPoint(IPAddress.Any, 0);

        public ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default) {
            try {
                var client = API.Accept(_zitiSocket, out var caller);
                _logger.LogInformation("Accepted connection from Ziti client: {Caller}", caller ?? "unknown");

                var socket = client.ToSocket();

                if (socket.RemoteEndPoint is IPEndPoint remoteEndPoint) {
                    _logger.LogDebug(
                        "Remote endpoint: {Address}:{Port}",
                        remoteEndPoint.Address,
                        remoteEndPoint.Port);
                }

                if (socket.LocalEndPoint is IPEndPoint localEndPoint) {
                    _logger.LogDebug(
                        "Local endpoint: {Address}:{Port}",
                        localEndPoint.Address,
                        localEndPoint.Port);
                }

                return new ValueTask<ConnectionContext>(_contextFactory.Create(socket));
            } catch (ObjectDisposedException) {
                _logger.LogDebug("Socket was disposed during accept");
                return new ValueTask<ConnectionContext>((ConnectionContext)null);
            } catch (SocketException ex) {
                _logger.LogError(ex, "Socket error during accept");
                return new ValueTask<ConnectionContext>((ConnectionContext)null);
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error during accept");
                throw;
            }
        }

        public ValueTask UnbindAsync(CancellationToken cancellationToken = default) {
            _logger.LogInformation("Unbinding Ziti listener");
            return ValueTask.CompletedTask;
        }

        public ValueTask DisposeAsync() {
            if (_disposed)
                return ValueTask.CompletedTask;

            _disposed = true;
            _logger.LogInformation("Disposing Ziti listener");

            try {
                _socket?.Close();
                _socket?.Dispose();
            } catch (Exception ex) {
                _logger.LogError(ex, "Error disposing socket");
            }

            return ValueTask.CompletedTask;
        }
    }
}
