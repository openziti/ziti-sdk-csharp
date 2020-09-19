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
    public class Enrollment
    {
        [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
        public delegate void AfterEnrollment(EnrollmentResult result);

        public struct EnrollmentResult
        {
            public string Json;
        }

        public static void Enroll(Options opts, AfterEnrollment afterEnrollment)
        {
            ziti_enroll_options native_opts = new ziti_enroll_options()
            {
                jwt = opts.Jwt,
                enroll_cert = opts.EnrollCert,
                enroll_key = opts.EnrollKey,
            };

            Native.API.ziti_enroll(ref native_opts, Native.API.z4d_default_loop(), native_on_ziti_enroll, GCHandle.Alloc(afterEnrollment));
        }

        internal static void Enroll(Options opts, string v, object afterEnrollment)
        {
            throw new NotImplementedException();
        }

        static public void native_on_ziti_enroll(IntPtr ziti_config, int status, string errorMessage, GCHandle context)
        {
            Util.CheckStatus(status);

            int jsonMaxSize = 2 << 16; //64k
            byte[] bytes = new byte[jsonMaxSize];

            int len;
            Native.API.json_from_ziti_config(ziti_config, bytes, jsonMaxSize, out len);

            EnrollmentResult result = new EnrollmentResult()
            {
                Json = Encoding.UTF8.GetString(bytes, 0, len),
            };
            AfterEnrollment cb = (AfterEnrollment)context.Target;
            cb(result);
            context.SafeFreeGCHandle();
        }

        public struct Options
        {
            public string Jwt { get; set; }
            public string EnrollKey { get; set; }
            public string EnrollCert { get; set; }
        }
    }
}
