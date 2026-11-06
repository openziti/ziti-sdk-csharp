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
using System.Runtime.InteropServices;

namespace OpenZiti {
    internal static class ZitiUtil {
        public static readonly GCHandle NO_CONTEXT = GCHandle.Alloc(new object());
        public static readonly IntPtr NO_CONTEXT_PTR = GCHandle.ToIntPtr(NO_CONTEXT);

        public static void CheckStatus(int status) {
            if ((ZitiStatus)status != ZitiStatus.ZITI_OK) {
                throw ZitiException.Create(status);
            }
        }

        public static object GetTarget(GCHandle handle) {
            if (handle.IsAllocated) {
                return handle.Target;
            }
            return null;
        }

        public static string GetVersion(bool verbose) {
            var v = Marshal.PtrToStructure<OpenZiti.Native.ziti_version>(OpenZiti.Native.API.ziti_get_version());
            if (!verbose) {
                return v.version;
            }
            return $"\n\tVersion:\t{v.version}\n\tRevision:\t{v.revision}\n\tBuild Date:\t{v.build_date}\n";
        }
    }
}
