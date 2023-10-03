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

using OpenZiti;
using OpenZiti.Native;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Net.Sockets;
using System.Text;
using System.Threading.Tasks;

using nAPI = OpenZiti.Native.API;

namespace OpenZiti {
    public class ZitiSocket : IDisposable {
        static ZitiSocket() {
            nAPI.Ziti_lib_init();
        }

        internal IntPtr NativeSocket { get; } = IntPtr.Zero;

        public ZitiSocket(SocketType type) {
            NativeSocket = nAPI.Ziti_socket(type);
        }
        public ZitiSocket(IntPtr nativeSocket) {
            NativeSocket = nativeSocket;
        }

        public void Dispose() {
            nAPI.Ziti_close(NativeSocket);
        }

        /// <summary>
        /// Verifies if the socket is attached to an OpenZiti connection
        /// </summary>
        /// <returns></returns>
        public int CheckSocket() {
            return nAPI.Ziti_check_socket(NativeSocket);
        }

        public int Connect(ZitiContext ztx, string service, string terminator) {
            return nAPI.Ziti_connect(NativeSocket, ztx.NativeContext, service, terminator);
        }

        public int ConnectByAddress(string host, ushort port) {
            return nAPI.Ziti_connect_addr(NativeSocket, host, port);
        }

        public Socket ToSocket() {
            var sockH = new SafeSocketHandle(NativeSocket, true);
            return new Socket(sockH);
        }

        public NetworkStream ToNetworkStream() {
            return new NetworkStream(ToSocket(), ownsSocket: true);
        }
    }
}
