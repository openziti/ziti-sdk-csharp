#nullable enable
using System;
using System.Collections.Generic;
using System.Reflection;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenZiti.Native;

namespace OpenZiti.NET.Tests {
    internal sealed record NativeFieldLayout(string Name, int Offset, int Size);
    internal sealed record NativeStructLayout(int Size, List<NativeFieldLayout> Fields);

    // Verifies the managed OpenZiti.Native structs against the native struct layout. The EXPECTED values come
    // live from the native library itself (z4d_layout_report, which the native compiler fills via offsetof/
    // sizeof, an independent source of truth); the ACTUAL values come from managed reflection (Marshal.SizeOf /
    // Marshal.OffsetOf, the thing under test). Not circular, and always in sync with the exact native referenced.
    //
    // This is the maintainable successor to TestCSDKStructAlignments (which hardcodes the same numbers by hand).
    // It activates once the referenced OpenZiti.NET.native exports z4d_layout_report; against an older native it
    // reports Inconclusive and TestCSDKStructAlignments remains the active guard.
    [TestClass]
    public class NativeLayoutChecker {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        // C struct name (as emitted by z4d_layout_report) -> managed type name in OpenZiti.Native. Only names that
        // differ need an entry; a leading "struct " is stripped first.
        private static readonly Dictionary<string, string> StructNameMap = new() {
            ["api_path"] = "ziti_api_path",
        };

        // (C struct name, C field name) -> managed field name where they differ. A null managed name means the
        // field has no reflectable public managed counterpart (e.g. a private field); its offset is not asserted,
        // but the struct size and surrounding fields still pin it.
        private static readonly Dictionary<(string, string), string?> FieldNameMap = new() {
            [("ziti_address", "type")] = null,        // managed: private address_type at offset 0
            [("ziti_address", "addr")] = "_union",
            [("ziti_listen_options", "precendence")] = "precedence",
        };

        private static Type? ResolveManagedType(string cName) {
            var name = cName.StartsWith("struct ") ? cName.Substring("struct ".Length) : cName;
            if (StructNameMap.TryGetValue(name, out var mapped)) { name = mapped; }
            return typeof(OpenZiti.Native.API).Assembly.GetType("OpenZiti.Native." + name);
        }

        private static (int arch, Dictionary<string, NativeStructLayout> structs) ParseReport(string report) {
            int arch = 0;
            var structs = new Dictionary<string, NativeStructLayout>();
            NativeStructLayout? current = null;
            string? currentName = null;
            foreach (var raw in report.Split('\n')) {
                var line = raw.Trim();
                if (line.Length == 0) { continue; }
                var p = line.Split('|');
                switch (p[0]) {
                    case "Z4DARCH": arch = int.Parse(p[1]); break;
                    case "Z4DSTRUCT":
                        currentName = p[1];
                        current = new NativeStructLayout(int.Parse(p[2]), new List<NativeFieldLayout>());
                        structs[currentName] = current;
                        break;
                    case "Z4DFIELD":
                        if (current == null || currentName != p[1]) {
                            throw new InvalidOperationException($"Z4DFIELD '{p[2]}' for '{p[1]}' without a matching Z4DSTRUCT");
                        }
                        current.Fields.Add(new NativeFieldLayout(p[2], int.Parse(p[3]), int.Parse(p[4])));
                        break;
                }
            }
            return (arch, structs);
        }

        [TestMethod]
        public void TestStructAlignmentsAgainstNativeLayout() {
            IntPtr ptr;
            try {
                ptr = TestBlitting.z4d_layout_report();
            } catch (EntryPointNotFoundException) {
                Assert.Inconclusive("the referenced OpenZiti.NET.native does not export z4d_layout_report yet; " +
                    "publish a native build that includes it. (TestCSDKStructAlignments still guards layout.)");
                return;
            }

            var report = Marshal.PtrToStringAnsi(ptr);
            Assert.IsFalse(string.IsNullOrEmpty(report), "z4d_layout_report returned empty");

            var (arch, structs) = ParseReport(report!);
            Assert.AreEqual(IntPtr.Size, arch, "native report pointer size disagrees with the running process");
            Assert.AreNotEqual(0, structs.Count, "no structs parsed from z4d_layout_report");

            var failures = new List<string>();
            foreach (var (cStruct, layout) in structs) {
                var t = ResolveManagedType(cStruct);
                if (t == null) { failures.Add($"no managed type for native struct '{cStruct}'"); continue; }

                int managedSize = Marshal.SizeOf(t);
                if (managedSize != layout.Size) {
                    failures.Add($"{t.Name}: size managed={managedSize} native={layout.Size}");
                }

                var key = cStruct.StartsWith("struct ") ? cStruct.Substring("struct ".Length) : cStruct;
                foreach (var f in layout.Fields) {
                    string? managedField = f.Name;
                    if (FieldNameMap.TryGetValue((key, f.Name), out var mapped)) { managedField = mapped; }
                    if (managedField == null) { continue; } // intentionally not offset-checked

                    var fi = t.GetField(managedField, BindingFlags.Public | BindingFlags.Instance);
                    if (fi == null) {
                        failures.Add($"{t.Name}.{managedField}: no public managed field (native offset {f.Offset})");
                        continue;
                    }
                    int managedOffset = Marshal.OffsetOf(t, managedField).ToInt32();
                    if (managedOffset != f.Offset) {
                        failures.Add($"{t.Name}.{managedField}: offset managed={managedOffset} native={f.Offset}");
                    }
                }
            }

            if (failures.Count > 0) {
                Assert.Fail("native layout mismatches:\n  " + string.Join("\n  ", failures));
            }
            Log.Info($"verified {structs.Count} native struct layouts against managed structs");
        }
    }
}
