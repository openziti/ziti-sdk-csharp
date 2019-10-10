using System;
using System.Runtime.InteropServices;

namespace NetFoundry
{
    
    //Internal delegates
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void OnZitiConnected(IntPtr nf_connection, int status);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void OnZitiDataReceived(IntPtr nf_connection, IntPtr data, int len);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void AfterZitiDataWritten(IntPtr nf_connection, int status, GCHandle write_ctx);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void AfterZitiInitialized(IntPtr nf_context, int status, GCHandle originalContext);

    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    internal delegate void OnUVTimer(IntPtr handle);

    /// <summary>
    /// A collection of static helper methods and properties for Ziti.
    /// </summary>
    public class Ziti
    {
        /// <summary>
        /// A property which controls whether or not output is sent to the 
        /// <see cref="System.Diagnostics.Debug"/> output stream. This is a global
        /// flag - when toggled all debug messages will show across any connection.
        /// </summary>
        public static bool OutputDebugInformation { get; set; } = false;

        /// <summary>
        /// A helper method to output messages helpful during debugging Ziti-related
        /// issues.
        /// </summary>
        /// <param name="msg"></param>
        public static void Debug(string msg)
        {
            if (OutputDebugInformation) System.Diagnostics.Debug.WriteLine(msg);
        }

        //expect the ziti_dll.dll file to be colocated to the .NET library
        internal const string DLL_PATH = "ziti_dll.dll";

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_init")]
        internal static extern int InitializeAndRun(string ConfigPath, IntPtr UVLoop, AfterZitiInitialized afterInitializedDelegate, GCHandle context);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_write")]
        internal static extern int Write(IntPtr conn, byte[] data, int length, AfterZitiDataWritten afterData, GCHandle dataContext);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_shutdown")]
        internal static extern int Shutdown(IntPtr nf_context);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_conn_init")]
        internal static extern int InitializeConnection(IntPtr nf_context, out IntPtr nf_connection, GCHandle context);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_dial")]
        internal static extern int Dial(IntPtr nf_connection, string serviceName, OnZitiConnected conn_cb, OnZitiDataReceived data_cb);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_dump")]
        internal static extern int Dump(IntPtr nf_context);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "createUvLoop")]
        internal static extern IntPtr CreateUVLoop();
        
        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_service_available")]
        internal static extern int ServiceAvailable(IntPtr ctx, string serviceName);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_close")]
        internal static extern int CloseConnection(IntPtr nf_connection);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_conn_data")]
        internal static extern GCHandle GetContext(IntPtr nf_connection);

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "exported_NF_set_timeout")]
        internal static extern int SetConnectionTimeout(IntPtr nf_context, int timeout);

        internal static T GetContextFromaConnection<T>(IntPtr nf_connection, bool andFree)
        {
            GCHandle handle = Ziti.GetContext(nf_connection);
            object target = handle.Target;
            if(andFree) handle.Free();

            if(target != null && target.GetType() == typeof(T))
            {
                return (T)target;
            }
            return default(T);
        }

        [System.Runtime.InteropServices.DllImport(DLL_PATH, EntryPoint = "registerUVTimer")]
        internal static extern IntPtr RegisterUVTimer(IntPtr uvLoop, OnUVTimer timer, long iterations, long delay);
    }
}