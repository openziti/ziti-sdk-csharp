/*
Copyright 2019 NetFoundry, Inc.

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

namespace OpenZiti {
    public class ZitiEnrollment {
        [UnmanagedFunctionPointer(Native.API.CALL_CONVENTION)]
        public delegate void AfterEnrollment(EnrollmentResult result, object context);

        public class EnrollmentResult {
            internal IntPtr nativeConfig;
            public object Context;
            public ZitiStatus Status;
            public string Message { get; internal set; }
            public string Json {
                get {
                    return System.Text.Json.JsonSerializer.Serialize(ZitiIdentity);
                }
            }
            public ZitiIdentityFormat ZitiIdentity { get; set; }

            public EnrollmentResult(IntPtr nativeConfig) {
                this.nativeConfig = nativeConfig;
            }
        }
    }
}