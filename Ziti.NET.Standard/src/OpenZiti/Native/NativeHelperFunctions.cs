using System;
using System.Collections.Generic;
using System.Runtime.InteropServices;
using System.Text;

namespace OpenZiti.Native { 
    public static class NativeHelperFunctions {


        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_event_type_from_pointer", CallingConvention = API.CALL_CONVENTION)]
        internal static extern int ziti_event_type_from_pointer(IntPtr ziti_event_t);
    }
}
