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
using System.Text;

using OpenZiti.Native;

namespace OpenZiti
{
    public class ZitiEnrollment
    {
        [UnmanagedFunctionPointer(Native.API.CALL_CONVENTION)]
        public delegate void AfterEnrollment(EnrollmentResult result, object context);

        private class EnrollmentContext
        {
            internal AfterEnrollment cb;
            internal object context;
        }

        public struct EnrollmentResult
        {
            public string Json;
        }

        public struct Options
        {
            public string Jwt { get; set; }
            public string EnrollKey { get; set; }
            public string EnrollCert { get; set; }
        }

        static IntPtr loop = Native.API.z4d_default_loop();

        public static void Enroll(Options opts, AfterEnrollment afterEnrollment, object context)
        {
            ziti_enroll_options native_opts = new ziti_enroll_options()
            {
                jwt = opts.Jwt,
                enroll_cert = opts.EnrollCert,
                enroll_key = opts.EnrollKey,
            };

            EnrollmentContext ctx = new EnrollmentContext()
            {
                cb = afterEnrollment,
                context = context,
            };

            
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(native_opts));
            Marshal.StructureToPtr(native_opts, pnt, false);
            Native.API.ziti_enroll(pnt, ref loop, ref ecb, GCHandle.Alloc(ctx));
        }

        static ziti_enroll_cb ecb = native_on_ziti_enroll;

        static internal void native_on_ziti_enroll(IntPtr ziti_config, int status, string errorMessage, GCHandle context)
        {
            ZitiUtil.CheckStatus(status);

            int jsonMaxSize = 2 << 16; //64k
            byte[] bytes = new byte[jsonMaxSize];

            int len;
            Native.API.json_from_ziti_config(ziti_config, bytes, jsonMaxSize, out len);

            EnrollmentResult result = new EnrollmentResult()
            {
                Json = Encoding.UTF8.GetString(bytes, 0, len),
            };

            EnrollmentContext ctx = (EnrollmentContext)context.Target;
            ctx.cb(result, ctx.context);
            context.SafeFreeGCHandle();
        }
    }
}
