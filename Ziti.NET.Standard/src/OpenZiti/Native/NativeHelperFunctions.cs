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
