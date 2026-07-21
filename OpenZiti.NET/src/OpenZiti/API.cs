/*
Copyright NetFoundry Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.IO;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;
using MLog = Microsoft.Extensions.Logging;

using nAPI = OpenZiti.Native.API;

namespace OpenZiti {
    public class API {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public static readonly string[] AllConfigs = new string[] { "all" };
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static nAPI.log_writer NativeLogger = NoopNativeLogFunction;
        public static void NoopNativeLogFunction(int level, string loc, string msg, uint msglen) {
            // intentionally empty
        }
        public static void DefaultNativeLogFunction(int level, string loc, string msg, uint msglen) {
            switch (level) {
                case 0:
                    Logger.Warn("SDK_: level 0 should not be logged, please report: {0}", msg); break;
                case 1:
                    Logger.Error("SDKe: {0}\t{1}", loc, msg);
                    break;
                case 2:
                    Logger.Warn("SDKw: {0}\t{1}", loc, msg);
                    break;
                case 3:
                    Logger.Info("SDKi: {0}\t{1}", loc, msg);
                    break;
                case 4:
                    Logger.Debug("SDKd: {0}\t{1}", loc, msg);
                    break;
                case 5:
                    //VERBOSE:5
                    Logger.Trace("SDKv: {0}\t{1}", loc, msg);
                    break;
                case 6:
                    //TRACE:6
                    Logger.Trace("SDKt: {0}\t{1}", loc, msg);
                    break;
                default:
                    Logger.Warn("SDK_: level [%d] NOT recognized: {1}", level, msg);
                    break;
            }
        }
        public static void DefaultConsoleLoggerFunction(int level, string loc, string msg, uint msglen) {

            switch (level) {
                case 0:
                    Log.Info("SDK_: level 0 should not be logged, please report: {0}", msg); break;
                case 1:
                    Log.Info("SDKe: {0}\t{1}", loc, msg);
                    break;
                case 2:
                    Log.Info("SDKw: {0}\t{1}", loc, msg);
                    break;
                case 3:
                    Log.Info("SDKi: {0}\t{1}", loc, msg);
                    break;
                case 4:
                    Log.Info("SDKd: {0}\t{1}", loc, msg);
                    break;
                case 5:
                    //VERBOSE:5
                    Log.Info("SDKv: {0}\t{1}", loc, msg);
                    break;
                case 6:
                    //TRACE:6
                    Log.Info("SDKt: {0}\t{1}", loc, msg);
                    break;
                default:
                    Log.Info("SDK_: level [%d] NOT recognized: {1}", level, msg);
                    break;
            }
        }

        private const int MaxCallerLen = 256;

        static API() {
            InitializeZiti();
        }

        public static void InitializeZiti() {
            nAPI.Ziti_lib_init();
            //var fp = Marshal.GetFunctionPointerForDelegate(NativeLogger);
            //nAPI.ziti_log_set_logger(fp);
        }

        public static void SetLoggerFunc(nAPI.log_writer logFunc) {
            var fp = Marshal.GetFunctionPointerForDelegate(logFunc);
            nAPI.ziti_log_set_logger(fp);
        }

        public static void InitializeZiti(MLog.LogLevel level) {
            InitializeZiti();
            SetLogLevel(level);
        }

        public static void SetLogLevel(MLog.LogLevel level) {
            nAPI.ziti_log_set_level((int)level, null);
        }

        public static int LastError() {
            return nAPI.Ziti_last_error();
        }

        public static string EnrollIdentityFile(string jwtFile) {
            var t = File.ReadAllBytes(jwtFile);
            return EnrollIdentity(t, null, null);
        }

        public static string EnrollIdentity(byte[] jwt) {
            return EnrollIdentity(jwt, null, null);
        }

        public static string EnrollIdentity(byte[] jwt, string key, string cert) {
            var rtn = nAPI.Ziti_enroll_identity(jwt, key, cert, out var id_json_ptr, out var id_json_len);
            ZitiStatus status = (ZitiStatus)rtn;
            if (status != ZitiStatus.ZITI_OK) {
                throw ZitiException.Create(rtn);
            }
            var id_json = Marshal.PtrToStringUTF8(id_json_ptr);
            return id_json;
        }

        public static ZitiContext LoadContext(string identity) {
            return new ZitiContext(identity);
        }

        public static ZitiSocket CreateSocket(SocketType type) {
            return new ZitiSocket(nAPI.Ziti_socket(type));
        }

        public static int Close(ZitiSocket socket) {
            return nAPI.Ziti_close(socket.NativeSocket);
        }

        public static int CheckSocket(ZitiSocket socket) {
            return nAPI.Ziti_check_socket(socket.NativeSocket);
        }

        public static ZitiSocket Connect(ZitiSocket socket, ZitiContext ztx, string service, string terminator) {
            var rtn = nAPI.Ziti_connect(socket.NativeSocket, ztx.NativeContext, service, terminator);
            if (rtn < 0) {
                string s = Marshal.PtrToStringAnsi(Native.API.ziti_errorstr(rtn));
                throw new ZitiException(s);
            }
            // The bridge leaves the socket non-blocking after connect; managed NetworkStream I/O needs blocking.
            nAPI.SetBlocking(socket.NativeSocket);
            return socket;
        }

        public static ZitiSocket ConnectByAddress(ZitiSocket socket, string host, UInt16 port) {
            var rtn = nAPI.Ziti_connect_addr(socket.NativeSocket, host, port);
            if (rtn < 0) {
                string s = Marshal.PtrToStringAnsi(Native.API.ziti_errorstr(rtn));
                throw new ZitiException(s);
            }
            // The bridge leaves the socket non-blocking after connect; managed NetworkStream I/O needs blocking.
            nAPI.SetBlocking(socket.NativeSocket);
            return socket;
        }

        public static Task<ZitiSocket> ConnectAsync(ZitiSocket socket, ZitiContext ztx, string service, string terminator, CancellationToken cancellationToken = default) {
            return Task.Run(() => Connect(socket, ztx, service, terminator), cancellationToken);
        }

        public static Task<ZitiSocket> ConnectByAddressAsync(ZitiSocket socket, string host, UInt16 port, CancellationToken cancellationToken = default) {
            return Task.Run(() => ConnectByAddress(socket, host, port), cancellationToken);
        }

        public static void Bind(ZitiSocket socket, ZitiContext ztx, string service, string terminator) {
            var rtn = nAPI.Ziti_bind(socket.NativeSocket, ztx.NativeContext, service, terminator);
            if (rtn < 0) {
                int errNo = LastError();
                throw ZitiException.Create(errNo);
            }
        }

        public static void Listen(ZitiSocket socket, int backlog) {
            var rtn = nAPI.Ziti_listen(socket.NativeSocket, backlog);
            if (rtn < 0) {
                int errNo = LastError();
                throw ZitiException.Create(errNo);
            }
            // The native side hands off an incoming client directly on a blocking listen socket; leaving it
            // non-blocking takes a broken backlog path. Accept unblocks by closing this socket on cancel.
            nAPI.SetBlocking(socket.NativeSocket);
        }

        /// <summary>
        /// Accept a client Ziti connection as a socket, blocking until a client connects.
        /// </summary>
        /// <param name="socket">a bound, listening ziti socket</param>
        /// <param name="caller">an identifier for the calling identity</param>
        public static ZitiSocket Accept(ZitiSocket socket, out string caller) {
            return Accept(socket, CancellationToken.None, out caller);
        }

        /// <summary>
        /// Accept a client Ziti connection as a socket, blocking until a client connects or the token is
        /// cancelled. Cancelling closes the listening socket to unblock the accept, so after cancellation the
        /// listener is no longer usable (like <see cref="System.Net.Sockets.TcpListener.Stop"/>).
        /// </summary>
        /// <param name="socket">a bound, listening ziti socket</param>
        /// <param name="cancellationToken">cancels a waiting accept by closing the listener</param>
        /// <param name="caller">an identifier for the calling identity</param>
        /// <exception cref="OperationCanceledException">the token was cancelled while waiting</exception>
        public static ZitiSocket Accept(ZitiSocket socket, CancellationToken cancellationToken, out string caller) {
            IntPtr callerBuff = Marshal.AllocHGlobal(MaxCallerLen);
            using var reg = cancellationToken.Register(() => nAPI.Ziti_close(socket.NativeSocket));
            try {
                IntPtr client_sock = nAPI.Ziti_accept(socket.NativeSocket, callerBuff, MaxCallerLen);
                if (client_sock.ToInt64() != -1) {
                    caller = Marshal.PtrToStringUTF8(callerBuff);
                    nAPI.SetBlocking(client_sock); // the accepted client feeds a NetworkStream; sync I/O needs blocking
                    return new ZitiSocket(client_sock, caller);
                }
                cancellationToken.ThrowIfCancellationRequested(); // accept was unblocked by the cancel-close above
                throw ZitiException.Create(LastError());
            }
            finally {
                Marshal.FreeHGlobal(callerBuff);
            }
        }

        /// <summary>
        /// Accept a client Ziti connection as a socket, awaiting a client or cancellation. See
        /// <see cref="Accept(ZitiSocket, CancellationToken, out string)"/> for the cancellation semantics; the
        /// caller identity is available on <see cref="ZitiSocket.Caller"/>.
        /// </summary>
        public static Task<ZitiSocket> AcceptAsync(ZitiSocket socket, CancellationToken cancellationToken = default) {
            return Task.Run(() => Accept(socket, cancellationToken, out _), cancellationToken);
        }

        /// <summary>
        /// Shuts down any loaded contexts and the background thread is removed 
        /// </summary>
        public static void Shutdown() {
            nAPI.Ziti_lib_shutdown();
        }
    }

    public enum ZitiLogLevel {
        FATAL = 0,
        ERROR = 1,
        WARN = 2,
        INFO = 3,
        DEBUG = 4,
        VERBOSE = 5,
        TRACE = 6,
    }
}
