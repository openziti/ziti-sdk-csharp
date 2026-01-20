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
using System.Collections;
using System.IO;
using System.Net.Http;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using nAPI = OpenZiti.Native.API;

namespace OpenZiti {
    /// <summary>
    /// An opaque handle to a native Ziti context. Required for certain operations
    /// </summary>
    public class ZitiContext {
        internal IntPtr NativeContext = IntPtr.Zero;

        static ZitiContext() {
            API.InitializeZiti();
        }

        internal ZitiContext(IntPtr ptr) {
            NativeContext = ptr;
        }

        public ZitiContext(byte[] identity) {
            nAPI.Ziti_load_context(out NativeContext, identity);
        }

        public ZitiContext(string identityFile) {
            int rc = nAPI.Ziti_load_context(out NativeContext, Encoding.UTF8.GetBytes(identityFile));
            if (rc != 0) {
                var err = API.LastError();
                string s = Marshal.PtrToStringAnsi(nAPI.ziti_errorstr(err));
                throw new ZitiException(s);
            }
        }

        /*
        public ZitifiedNetworkStream NewStream(string serviceName, string identity) {
            var ziti_socket_t = ZitiLibO.Ziti_socket(SocketType.Stream);
            int connectResult = ZitiLibO.Ziti_connect(ziti_socket_t, NativeContext, serviceName, identity);

            int errNo = ZitiLibO.Ziti_last_error();
            if (errNo != 0) {
                string err = Marshal.PtrToStringUTF8(Native.API.ziti_errorstr(errNo));
                throw new Exception(err);
            }

            var sockH = new SafeSocketHandle(ziti_socket_t, true);
            var socket = new Socket(sockH);
            return new ZitifiedNetworkStream(this, serviceName, identity, sockH, socket, FileAccess.ReadWrite, true);
        }*/


        public SocketsHttpHandler NewZitiSocketHandler(string serviceName) {
            return NewZitiSocketHandler(serviceName, null);
        }

        public SocketsHttpHandler NewZitiSocketHandler(string serviceName, string identity) {
            var sh = new SocketsHttpHandler {
                ConnectCallback = (context, token) => {
                    var ns = ZitifiedNetworkStream.NewStream(this, serviceName, identity);
                    return new ValueTask<Stream>(ns);
                }
            };

            return sh;
        }
    }
}
