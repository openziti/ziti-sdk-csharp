using System;
using System.Runtime.InteropServices;

[StructLayout(LayoutKind.Sequential)]
internal struct ziti_version
{
    public IntPtr version;     // const char*
    public IntPtr revision;    // const char*
    public IntPtr build_date;  // const char*
}

internal static class Program
{
    private static void Main()
    {
        IntPtr p = ziti_get_version();
        ziti_version v = Marshal.PtrToStructure<ziti_version>(p);

        string? version    = Marshal.PtrToStringUTF8(v.version);
        string? revision   = Marshal.PtrToStringUTF8(v.revision);
        string? build_date = Marshal.PtrToStringUTF8(v.build_date);

        Console.WriteLine($"version={version}");
        Console.WriteLine($"revision={revision}");
        Console.WriteLine($"build_date={build_date}");
    }

    [DllImport("ziti", EntryPoint = "ziti_get_version", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr ziti_get_version();
}
