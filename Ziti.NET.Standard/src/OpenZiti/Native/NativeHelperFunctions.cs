/*
Copyright 2021 NetFoundry, Inc.

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
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenZiti.Native { 
    public static class NativeHelperFunctions {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_event_type_from_pointer", CallingConvention = API.CALL_CONVENTION)]
        internal static extern int ziti_event_type_from_pointer(IntPtr ziti_event_t);


        [DllImport(API.Z4D_DLL_PATH, CallingConvention = API.CALL_CONVENTION)]
        internal static extern IntPtr make_char_array(int size);
        [DllImport(API.Z4D_DLL_PATH, CallingConvention = API.CALL_CONVENTION)]
        internal static extern void set_char_at(IntPtr arr, string s, int idx);
        [DllImport(API.Z4D_DLL_PATH, CallingConvention = API.CALL_CONVENTION)]
        internal static extern void free_char_array(IntPtr arr, int size);
        [DllImport(API.Z4D_DLL_PATH, CallingConvention = API.CALL_CONVENTION)]
        internal static extern void test_ziti_opts(IntPtr opts);

        internal static IntPtr ToPtr(string[] array) {
            if (array == null || array.Length == 0) {
                return IntPtr.Zero;
            }
            IntPtr arr = make_char_array(array.Length);
            int idx = 0;
            foreach(string s in array) {
                set_char_at(arr, s, idx++);
            }
            
            return arr;
        }
    }
}
