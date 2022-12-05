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

using nAPI = OpenZiti.Native.API;

namespace OpenZiti {
    public class API {
        public static readonly string[] AllConfigs = new string[] { "all" };
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        public static nAPI.log_writer NativeLogger = NoopNativeLogFunction;
        public static void NoopNativeLogFunction(int level, string loc, string msg, uint msglen) { }
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















        private const int MaxCallerLen = 256;

        static API() {
            var fp = Marshal.GetFunctionPointerForDelegate(API.NativeLogger);
            nAPI.ziti_log_set_logger(fp);
            nAPI.Ziti_lib_init();
        }



        public static int LastError() {
            return nAPI.Ziti_last_error();
        }

        public static string EnrollIdentity(string jwtFile) {
            var t = File.ReadAllBytes(jwtFile);
            return EnrollIdentity(t, null, null);
        }

        public static string EnrollIdentity(byte[] jwt, string key, string cert) {
            var rtn = nAPI.Ziti_enroll_identity(jwt, key, cert, out var id_json_ptr, out var id_json_len);
            ZitiStatus status = (ZitiStatus)rtn;
            if (status != ZitiStatus.OK) {
                throw new ZitiException(status);
            }
            var id_json = Marshal.PtrToStringUTF8(id_json_ptr);
            return id_json;
        }

        public static ZitiContext LoadContext(string identity) {
            return new ZitiContext(nAPI.Ziti_load_context(identity));
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
                ZitiStatus s = (ZitiStatus)rtn;
                throw new ZitiException(s);
            }
            return socket;
        }

        public static ZitiSocket ConnectByAddress(ZitiSocket socket, string host, UInt16 port) {
            var rtn = nAPI.Ziti_connect_addr(socket.NativeSocket, host, port);
            if (rtn < 0) {
                ZitiStatus s = (ZitiStatus)rtn;
                throw new ZitiException(s);
            }
            return socket;
        }

        public static void Bind(ZitiSocket socket, ZitiContext ztx, string service, string terminator) {
            var rtn = nAPI.Ziti_bind(socket.NativeSocket, ztx.NativeContext, service, terminator);
            if (rtn < 0) {
                int be = LastError();
                ZitiStatus s = (ZitiStatus)be;
                throw new ZitiException(s);
            }
        }

        public static void Listen(ZitiSocket socket, int backlog) {
            var rtn = nAPI.Ziti_listen(socket.NativeSocket, backlog);
            if (rtn < 0) {
                int le = LastError();
                ZitiStatus s = (ZitiStatus)le;
                throw new ZitiException(s);
            }
        }

        /// <summary>
        /// Accept a client Ziti connection as a socket
        /// </summary>
        /// <param name="socket"></param>
        /// <param name="caller">An identifier for the calling identity</param>
        /// <returns></returns>
        public static ZitiSocket Accept(ZitiSocket socket, out string caller) {

            IntPtr callerBuff = Marshal.AllocHGlobal(MaxCallerLen);

            IntPtr client_sock = nAPI.Ziti_accept(socket.NativeSocket, callerBuff, MaxCallerLen);

            if (client_sock.ToInt64() < 0) {
                int errNo = LastError();
                //string err = Marshal.PtrToStringUTF8(NAPI.ziti_errorstr(errNo));
                ZitiStatus s = (ZitiStatus)errNo;
                throw new ZitiException(s);
            }

            caller = Marshal.PtrToStringUTF8(callerBuff);
            Marshal.FreeHGlobal(callerBuff);

            return new ZitiSocket(client_sock);
        }

        /// <summary>
        /// Shuts down any loaded contexts and the background thread is removed 
        /// </summary>
        public static void Shutdown() {
            nAPI.Ziti_lib_shutdown();
        }
    }
}
