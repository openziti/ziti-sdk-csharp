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

using OpenZiti.Native;
using System;
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Security.Principal;
using System.Text;
using System.Threading.Tasks;

using nAPI = OpenZiti.Native.API;

namespace OpenZiti {
    public class ZitifiedNetworkStream : NetworkStream {
        static ZitifiedNetworkStream() {
            API.InitializeZiti();
        }
        private SafeSocketHandle nativeSocket = null;
        private Socket socket = null;

        internal ZitifiedNetworkStream(ZitiContext ctx, string serviceName, string identity, SafeSocketHandle nativeSocket, Socket socket, FileAccess access, bool ownsSocket)
            : base(socket, FileAccess.ReadWrite, true) {

            this.nativeSocket = nativeSocket;
            this.socket = socket;
        }

        internal ZitifiedNetworkStream(IntPtr nativeContext, string serviceName, string identity, SafeSocketHandle nativeSocket, Socket socket, FileAccess access, bool ownsSocket)
            : base(socket, FileAccess.ReadWrite, true) {

            this.nativeSocket = nativeSocket;
            this.socket = socket;
        }


        public static ZitifiedNetworkStream NewStream(IntPtr nativeContext, string serviceName, string identity) {
            var ziti_socket_t = nAPI.Ziti_socket(SocketType.Stream);
            int connectResult = nAPI.Ziti_connect(ziti_socket_t, nativeContext, serviceName, identity);

            int errNo = nAPI.Ziti_last_error();
            if (errNo != 0) {
                string err = Marshal.PtrToStringUTF8(Native.API.ziti_errorstr(errNo));
                throw new Exception(err);
            }

            var sockH = new SafeSocketHandle(ziti_socket_t, true);
            var socket = new Socket(sockH);
            return new ZitifiedNetworkStream(nativeContext, serviceName, identity, sockH, socket, FileAccess.ReadWrite, true);
        }

        public static ZitifiedNetworkStream NewStream(ZitiContext ctx, string serviceName, string identity) {
            var ziti_socket_t = nAPI.Ziti_socket(SocketType.Stream);
            int connectResult = nAPI.Ziti_connect(ziti_socket_t, ctx.NativeContext, serviceName, identity);

            int errNo = nAPI.Ziti_last_error();
            if (errNo != 0) {
                string err = Marshal.PtrToStringUTF8(Native.API.ziti_errorstr(errNo));
                throw new Exception(err);
            }

            var sockH = new SafeSocketHandle(ziti_socket_t, true);
            var socket = new Socket(sockH);
            return new ZitifiedNetworkStream(ctx, serviceName, identity, sockH, socket, FileAccess.ReadWrite, true);
        }

        public static ZitifiedNetworkStream NewStreamByIntercept(ZitiContext ctx, string serviceName, string identity) {
            var ziti_socket_t = nAPI.Ziti_socket(SocketType.Stream);
            int connectResult = nAPI.Ziti_connect(ziti_socket_t, ctx.NativeContext, serviceName, identity);

            int errNo = nAPI.Ziti_last_error();
            if (errNo != 0) {
                string err = Marshal.PtrToStringUTF8(Native.API.ziti_errorstr(errNo));
                throw new Exception(err);
            }

            var sockH = new SafeSocketHandle(ziti_socket_t, true);
            var socket = new Socket(sockH);
            return new ZitifiedNetworkStream(ctx, serviceName, identity, sockH, socket, FileAccess.ReadWrite, true);
        }

        public SocketsHttpHandler ToSocketsHttpHandler() {
            var zitifiedHandler = new SocketsHttpHandler {
                ConnectCallback = (context, token) => {
                    return new ValueTask<Stream>(this);
                }
            };

            return zitifiedHandler;
        }

        public override void Close() {
            base.Close();
        }
    }
}
