using System;
using System.Net;
using System.Net.Sockets;
using System.Threading;
using System.Threading.Channels;
using System.Threading.Tasks;
using Microsoft.AspNetCore.Connections;
using Microsoft.AspNetCore.Server.Kestrel.Transport.Sockets;
using Microsoft.Extensions.Logging;
using OpenZiti;

namespace OpenZiti.Samples.Kestrel;

internal class ZitiConnectionListenerFactory : IConnectionListenerFactory {
    private readonly ILogger<ZitiConnectionListenerFactory> _logger;

    private readonly IConnectionListenerFactory _fallback;

    public ZitiConnectionListenerFactory(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<ZitiConnectionListenerFactory>();
        _fallback = new SocketTransportFactory(
            Microsoft.Extensions.Options.Options.Create(new SocketTransportOptions()),
            loggerFactory
        );
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is ZitiEndPoint) {
            var ziti = endpoint as ZitiEndPoint;

            _logger.LogInformation("Initializing Ziti transport");
            _logger.LogInformation("Identity: {IdentityPath}", ziti.Identity);
            _logger.LogInformation("Service: {ServiceName}", ziti.ServiceName);

            //API.SetLogLevel(logLevel);
            var zitiSocket = new ZitiSocket(SocketType.Stream);
            var ctx = new ZitiContext(ziti.Identity);

            API.Bind(zitiSocket, ctx, ziti.ServiceName, ziti.Terminator);
            _logger.LogInformation("Bound to Ziti service: {ServiceName}", ziti.ServiceName);

            API.Listen(zitiSocket, 100);
            _logger.LogInformation("Listening for Ziti connections");

            return new ValueTask<IConnectionListener>(new ZitiSocketListener(zitiSocket, ctx, _logger));
        } else {
            // ignore non
            return _fallback.BindAsync(endpoint, cancellationToken);
        }
    }

    private class ZitiSocketListener : IConnectionListener {
        private readonly ZitiSocket _zitiSocket;
        private readonly ZitiContext _zitiContext;  // Keep context alive
        private readonly ILogger _logger;
        private readonly SocketConnectionContextFactory _contextFactory;
        private readonly Channel<(ZitiSocket socket, string caller)> _connectionChannel;
        private readonly CancellationTokenSource _acceptLoopCts;
        private readonly Task _acceptLoopTask;
        private bool _disposed;

        public ZitiSocketListener(ZitiSocket zitiSocket, ZitiContext zitiContext, ILogger logger) {
            _zitiSocket = zitiSocket;
            _zitiContext = zitiContext;  // Keep reference to prevent GC
            _logger = logger;
            _contextFactory = new SocketConnectionContextFactory(new SocketConnectionFactoryOptions(), logger);

            // Unbounded channel to buffer incoming connections
            _connectionChannel = Channel.CreateUnbounded<(ZitiSocket, string)>(
                new UnboundedChannelOptions {
                    SingleReader = false,  // Kestrel may call AcceptAsync from multiple threads
                    SingleWriter = true    // Our background thread is the only writer
                });

            _acceptLoopCts = new CancellationTokenSource();

            // Start dedicated background thread for blocking API.Accept calls
            _acceptLoopTask = Task.Run(AcceptLoopAsync, _acceptLoopCts.Token);

            _logger.LogInformation("Started dedicated Ziti accept loop");
        }

        public EndPoint EndPoint => new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// Dedicated thread that blocks in API.Accept for each Ziti client and queues it for Kestrel.
        /// UnbindAsync closes the listening socket to unblock the pending accept on shutdown.
        /// </summary>
        private async Task AcceptLoopAsync() {
            _logger.LogInformation("Ziti accept loop starting");
            try {
                while (!_acceptLoopCts.Token.IsCancellationRequested) {
                    ZitiSocket client;
                    string caller;
                    try {
                        client = API.Accept(_zitiSocket, out caller);
                    } catch (Exception) when (_acceptLoopCts.Token.IsCancellationRequested || _disposed) {
                        break; // listener closed on shutdown -> accept unblocked with an error
                    }
                    _logger.LogInformation("Accepted connection from Ziti client: {Caller}", caller ?? "unknown");
                    await _connectionChannel.Writer.WriteAsync((client, caller ?? "unknown"), _acceptLoopCts.Token);
                }
            } finally {
                _connectionChannel.Writer.Complete();
                _logger.LogInformation("Ziti accept loop stopped");
            }
        }

        /// <summary>
        /// Kestrel calls this to get the next connection asynchronously
        /// We read from our channel that's fed by the background accept thread
        /// </summary>
        public async ValueTask<ConnectionContext> AcceptAsync(CancellationToken cancellationToken = default) {
            try {
                _logger.LogDebug("Kestrel requesting next connection...");

                // Wait asynchronously for next connection from our channel
                var (client, caller) = await _connectionChannel.Reader.ReadAsync(cancellationToken);

                _logger.LogInformation("Processing queued connection from: {Caller}", caller);

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

                var context = _contextFactory.Create(socket);
                _logger.LogDebug("Connection context created for {Caller}", caller);

                return context;

            } catch (ChannelClosedException) {
                _logger.LogDebug("Connection channel closed");
                return null;
            } catch (OperationCanceledException) {
                _logger.LogDebug("Accept cancelled");
                return null;
            } catch (Exception ex) {
                _logger.LogError(ex, "Unexpected error in AcceptAsync");
                return null;
            }
        }

        public ValueTask UnbindAsync(CancellationToken cancellationToken = default) {
            _logger.LogInformation("Unbinding Ziti listener");

            // Signal accept loop to stop accepting new connections
            _acceptLoopCts.Cancel();

            // Close the ZitiSocket to unblock API.Accept
            _zitiSocket?.Dispose();

            return ValueTask.CompletedTask;
        }

        public async ValueTask DisposeAsync() {
            if (_disposed)
                return;

            _disposed = true;
            _logger.LogInformation("Disposing Ziti listener");

            try {
                // UnbindAsync should have already cancelled the loop and closed the socket
                // Just wait for the accept loop thread to finish
                if (_acceptLoopTask != null) {
                    var completed = await Task.WhenAny(_acceptLoopTask, Task.Delay(5000));
                    if (completed != _acceptLoopTask) {
                        _logger.LogWarning("Accept loop did not complete within timeout");
                    }
                }

                // The listener fd was already closed by UnbindAsync (which unblocks the accept); do not
                // close it again here (avoids a double Ziti_close on the same handle).
                _acceptLoopCts?.Dispose();

            } catch (Exception ex) {
                _logger.LogError(ex, "Error disposing Ziti listener");
            }
        }
    }
}
