#nullable enable
using System;
using System.Linq;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace OpenZiti.NET.Tests {
    // Every P/Invoke into ziti4dotnet has to resolve in the native under test. A C SDK function that is renamed or
    // removed otherwise surfaces only when the call runs, and most calls need a live overlay the unit tests lack.
    [TestClass]
    public class NativeExportsChecker {
        [TestMethod]
        public void TestEveryPInvokeResolves() {
            var asm = typeof(Native.API).Assembly;
            var handle = NativeLibrary.Load(Native.API.Z4D_DLL_PATH, asm, null);

            const BindingFlags all = BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.Static;
            var entryPoints = asm.GetTypes()
                .SelectMany(t => t.GetMethods(all))
                .Select(m => (method: m, import: m.GetCustomAttribute<DllImportAttribute>()))
                .Where(x => x.import?.Value == Native.API.Z4D_DLL_PATH)
                .Select(x => x.import!.EntryPoint ?? x.method.Name)
                .Distinct()
                .OrderBy(n => n, StringComparer.Ordinal)
                .ToList();
            Assert.AreNotEqual(0, entryPoints.Count, $"no P/Invokes into {Native.API.Z4D_DLL_PATH} found");

            var missing = entryPoints.Where(n => !NativeLibrary.TryGetExport(handle, n, out _)).ToList();
            Assert.AreEqual(0, missing.Count,
                $"{missing.Count} of {entryPoints.Count} P/Invokes do not resolve in {Native.API.Z4D_DLL_PATH}: " +
                string.Join(", ", missing));
        }

        [TestMethod]
        public void TestGetVersion() {
            var version = ZitiUtil.GetVersion(false);
            Assert.IsFalse(string.IsNullOrWhiteSpace(version), "GetVersion(false) returned nothing");
            StringAssert.Contains(ZitiUtil.GetVersion(true), version);
        }
    }
}
