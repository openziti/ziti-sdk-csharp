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

/// <summary>
/// FIXED VERSION: Uses a dedicated background thread for blocking API.Accept calls
/// and bridges to Kestrel's async model via Channel
/// </summary>
internal class ZitiConnectionListenerFactoryFixed : IConnectionListenerFactory {
    private readonly ILogger<ZitiConnectionListenerFactoryFixed> _logger;

    private readonly IConnectionListenerFactory _fallback;

    public ZitiConnectionListenerFactoryFixed(ILoggerFactory loggerFactory) {
        _logger = loggerFactory.CreateLogger<ZitiConnectionListenerFactoryFixed>();
        _fallback = new SocketTransportFactory(
            Microsoft.Extensions.Options.Options.Create(new SocketTransportOptions()),
            loggerFactory
        );
    }

    public ValueTask<IConnectionListener> BindAsync(EndPoint endpoint, CancellationToken cancellationToken = default) {
        if (endpoint is ZitiEndPoint) {
            var ziti = endpoint as ZitiEndPoint;
            //var identityPath = _options.IdentityPath;
            //var serviceName = _options.ServiceName;
            //var logLevel = _options.LogLevel;
            //var terminator = _options.Terminator ?? string.Empty;

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

            return new ValueTask<IConnectionListener>(
                new ZitiSocketListenerFixed(zitiSocket, ctx, _logger));
        } else {
            // ignore non
            return _fallback.BindAsync(endpoint, cancellationToken);
        }
    }

    private class ZitiSocketListenerFixed : IConnectionListener {
        private readonly ZitiSocket _zitiSocket;
        private readonly ZitiContext _zitiContext;  // Keep context alive
        private readonly Socket _socket;
        private readonly ILogger _logger;
        private readonly SocketConnectionContextFactory _contextFactory;
        private readonly Channel<(ZitiSocket socket, string caller)> _connectionChannel;
        private readonly CancellationTokenSource _acceptLoopCts;
        private readonly Task _acceptLoopTask;
        private bool _disposed;

        public ZitiSocketListenerFixed(ZitiSocket zitiSocket, ZitiContext zitiContext, ILogger logger) {
            _zitiSocket = zitiSocket;
            _zitiContext = zitiContext;  // Keep reference to prevent GC
            _socket = zitiSocket.ToSocket();
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

        public EndPoint EndPoint => _socket.LocalEndPoint ?? new IPEndPoint(IPAddress.Any, 0);

        /// <summary>
        /// Background thread that uses non-blocking accept with select/poll
        /// to allow periodic cancellation checks
        /// </summary>
        private async Task AcceptLoopAsync() {
            _logger.LogInformation("Ziti accept loop starting");

            try {
                // Set socket to non-blocking mode
                _socket.Blocking = false;
                _logger.LogInformation("Socket set to non-blocking mode");

                while (!_acceptLoopCts.Token.IsCancellationRequested) {
                    try {
                        // Poll with timeout to check if socket is readable (connection waiting)
                        // Using 1 second timeout so we can check cancellation frequently
                        _logger.LogDebug("Polling for Ziti connection...");
                        bool ready = _socket.Poll(1_000_000, SelectMode.SelectRead); // 1 second in microseconds

                        if (!ready) {
                            // Timeout - no connection available, loop again and check cancellation
                            continue;
                        }

                        // Socket is readable - accept should not block
                        _logger.LogDebug("Socket ready, calling Accept...");
                        var client = API.Accept(_zitiSocket, out var caller);
                        _logger.LogInformation("Accepted connection from Ziti client: {Caller}", caller ?? "unknown");

                        // Push to channel for Kestrel to consume asynchronously
                        await _connectionChannel.Writer.WriteAsync((client, caller ?? "unknown"), _acceptLoopCts.Token);
                        _logger.LogDebug("Connection queued for processing");

                    } catch (SocketException ex) when (ex.SocketErrorCode == SocketError.WouldBlock && !_disposed) {
                        // Non-blocking accept returned EWOULDBLOCK - try again
                        _logger.LogDebug("Accept would block, retrying...");
                        continue;
                    } catch (SocketException ex) when (!_disposed) {
                        _logger.LogError(ex, "Socket error in accept loop: {ErrorCode}", ex.SocketErrorCode);
                        // Continue accepting, transient error
                    } catch (ObjectDisposedException) {
                        _logger.LogDebug("Socket disposed, exiting accept loop");
                        break;
                    } catch (Exception ex) when (!_disposed) {
                        _logger.LogError(ex, "Unexpected error in accept loop");
                        // Continue accepting
                    }
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

                _socket?.Dispose();
                _acceptLoopCts?.Dispose();

            } catch (Exception ex) {
                _logger.LogError(ex, "Error disposing Ziti listener");
            }
        }
    }
}
