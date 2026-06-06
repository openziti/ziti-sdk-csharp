using System;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace SmokeTest;

// Proves the freshly built ziti4dotnet native library for THIS OS/arch loads and is callable.
// The probe is z4d_all_config_types(): it returns a static, NULL-terminated char** -> { "all", NULL }.
// It has no enrollment, network, allocation, or event-loop prerequisites, so it is safe to call cold.
[TestClass]
public class NativeLoadTests
{
    // The native lib is named ziti4dotnet on every platform; .NET resolves the platform prefix/suffix
    // (ziti4dotnet.dll / libziti4dotnet.so / libziti4dotnet.dylib) from the package's runtimes/<rid>/native.
    [DllImport("ziti4dotnet", CallingConvention = CallingConvention.Cdecl)]
    private static extern IntPtr z4d_all_config_types();

    [TestMethod]
    public void NativeLibrary_Loads_And_ReturnsExpectedConfigTypes()
    {
        Console.WriteLine($"RID: {RuntimeInformation.RuntimeIdentifier}");
        Console.WriteLine($"OS:  {RuntimeInformation.OSDescription} ({RuntimeInformation.OSArchitecture})");

        IntPtr array = z4d_all_config_types();
        Assert.AreNotEqual(IntPtr.Zero, array, "z4d_all_config_types() returned a null pointer.");

        IntPtr firstStringPtr = Marshal.ReadIntPtr(array);
        Assert.AreNotEqual(IntPtr.Zero, firstStringPtr, "First config-type entry was null.");

        string? first = Marshal.PtrToStringAnsi(firstStringPtr);
        Console.WriteLine($"z4d_all_config_types()[0] = '{first}'");

        Assert.AreEqual("all", first, "Unexpected first config type from the native library.");
    }
}
