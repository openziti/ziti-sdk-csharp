/*
Copyright 2019-2020 NetFoundry, Inc.

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
using System.Runtime.InteropServices;

using OpenZiti.Native;

namespace OpenZiti
{
    /// <summary>
    /// <see cref="ZitiOptions"/> is used to initialize a Ziti identity.
    /// </summary>
    public class ZitiOptions
    {
        /// <summary>
        /// A file on the local filesystem that was created from the <see cref="Enrollment.Enroll(Enrollment.Options, string, object)"/> 
        /// process.
        /// 
        /// When using ConfigFile do not supply <see cref="ControllerUrl"/> or an error will occur.
        /// </summary>
        public string ConfigFile { get; set; }

        /// <summary>
        /// This callback is invoked after the <see cref="ZitiIdentity"/> is initialized
        /// </summary>
        public OnInit InitComplete;

        /// <summary>
        /// This callback is invoked whenever a change to a service is detected
        /// </summary>
        public OnServiceChange ServiceChange;

        /// <summary>
        /// Controls how often (in seconds) the Ziti Controller will be polled for service changes. 
        /// If set to 0 <see cref="ServiceChange"/> will not be invoked. Default is 0.
        /// </summary>
        public Int32 ServiceRefreshInterval;

        /// <summary>
        /// An enum indicating the type of metrics which will be recorded.
        /// </summary>
        public RateType MetricsType;

        /// <summary>
        /// Enables TCP keepalive to detect connection drops faster. Default is 0.
        /// </summary>
        public Int32 RouterKeepalive;
        public GCHandle ctx;

#pragma warning disable 0649 //hide these warnings for now
        /// <summary>
        /// The url to use for the Ziti controller
        /// </summary>
        internal string ControllerUrl { get; set; }
        internal IntPtr tls;
        internal IntPtr config_types; //public string[] /*public char**/ config_types;
#pragma warning restore 0649

        private IntPtr nativeptr = IntPtr.Zero;
        private IntPtr nativeCtx = IntPtr.Zero;
        private ZitiContext context = null;

        internal IntPtr ToNative()
        {
            if (nativeptr != IntPtr.Zero) return nativeptr;

            ziti_options opts = new ziti_options
            {
                config = this.ConfigFile,
                controller = this.ControllerUrl,
                tls = this.tls,
                config_types = this.config_types,
                init_cb = after_ziti_init_native,
                service_cb = this.service_available_native,
                metrics_type = this.MetricsType,
                refresh_interval = this.ServiceRefreshInterval,
                ctx = this.ctx,
                router_keepalive = this.RouterKeepalive
            };
            nativeptr = Marshal.AllocHGlobal(Marshal.SizeOf(opts));
            Marshal.StructureToPtr(opts, nativeptr, false);
            return nativeptr;
        }

        internal void Dispose()
        {
            Marshal.FreeHGlobal(nativeptr);
        }

        private int after_ziti_init_native(IntPtr ziti_context, int status, GCHandle init_ctx)
        {
            Util.CheckStatus(status);
            this.nativeCtx = ziti_context;
            context = new ZitiContext(ziti_context);
            this.InitComplete(context, (ZitiStatus)status, init_ctx.Target);
            init_ctx.SafeFreeGCHandle();
            return 0;
        }

        private void service_available_native(IntPtr ziti_context, IntPtr ziti_service, int status, GCHandle on_service_context)
        {
            if (status < 0)
            {
                Util.CheckStatus(status);
            }
            else
            {
                ZitiService svc = new ZitiService(context, ziti_service);
                this.ServiceChange(context, svc, (ZitiStatus)status, status, Util.GetTarget(on_service_context));
                on_service_context.SafeFreeGCHandle();
            }
        }
    }
}
