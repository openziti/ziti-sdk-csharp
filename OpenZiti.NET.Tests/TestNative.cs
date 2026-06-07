using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenZiti.Native;
using System.Drawing;

namespace OpenZiti.NET.Tests {
    [TestClass]
    public class NativeCodeAlignmentChecker {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        private void verifyFieldCheck<T>(AlignmentCheck check, string expectedChecksum, uint expectedOffset, uint expectedSize) {
            Assert.AreEqual(expectedChecksum, check.checksum);
            Assert.AreEqual((int)expectedOffset, (int)check.offset);
            Assert.AreEqual(expectedSize, check.size);
        }

        [TestMethod]
        public void TestCSDKStructAlignments() {
            Log.Info("test begins with: " + Native.API.GetZitiPath());
            IntPtr testData = TestBlitting.z4d_struct_test();
            uint size = Marshal.PtrToStructure<uint>(testData);
            byte[] managedArray = new byte[size];
            Marshal.Copy(testData, managedArray, 0, (int)size);
            ziti_types native_structs = Marshal.PtrToStructure<ziti_types>(testData);
            
            // as of 0.35.0, run ./build/library/Debug/zitistructalignment.exe and capture the output
            // use that output to verify the expected offsets below.
            // __IF__ an offset is failing, there's an alignment issue in that struct that must be figured out!
            // often this is a field in the struct that has been removed from the native code that you'll need to
            // remove from the managed struct. Sometimes it'll be due to field alignment issues that are more difficult
            // to debug
            Log.Info("----");
#if ZITI_64BIT
            Assert.AreEqual((int)2120, (int)native_structs.size);
            verifyFieldCheck<ziti_id_cfg>(native_structs.f02_ziti_id_cfg, "ziti_id_cfg",  8, 24);
            verifyFieldCheck<ziti_config>(native_structs.f03_ziti_config, "ziti_config",  24, 48);
            verifyFieldCheck<ziti_api_path>(native_structs.f04_api_path, "api_path",  40, 16);
            verifyFieldCheck<ziti_api_versions>(native_structs.f05_ziti_api_versions, "ziti_api_versions", 56,  16);
            verifyFieldCheck<ziti_version>(native_structs.f06_ziti_version, "ziti_version", 72,  40);
            verifyFieldCheck<ziti_identity>(native_structs.f07_ziti_identity, "ziti_identity", 88,  24);
            verifyFieldCheck<ziti_process>(native_structs.f08_ziti_process, "ziti_process", 104,  8);
            verifyFieldCheck<ziti_posture_query>(native_structs.f10_ziti_posture_query_set, "ziti_posture_query_set", 136,  32);
            verifyFieldCheck<ziti_posture_query_set>(native_structs.f11_ziti_session_type, "ziti_session_type", 152,  4);
            verifyFieldCheck<ziti_session_type>(native_structs.f12_ziti_service, "ziti_service", 168,  80);
            verifyFieldCheck<ziti_service>(native_structs.f13_ziti_address_host, "ziti_address_host", 184,  260);
            verifyFieldCheck<ziti_address>(native_structs.f14_ziti_address_cidr, "ziti_address_cidr", 200,  4);
            verifyFieldCheck<ziti_client_cfg_v1>(native_structs.f15_ziti_client_cfg_v1, "ziti_client_cfg_v1", 216,  272);
            verifyFieldCheck<ziti_intercept_cfg_v1>(native_structs.f16_ziti_intercept_cfg_v1, "ziti_intercept_cfg_v1", 232,  48);
            verifyFieldCheck<ziti_server_cfg_v1>(native_structs.f17_ziti_server_cfg_v1, "ziti_server_cfg_v1", 248,  24);
            verifyFieldCheck<ziti_listen_options>(native_structs.f18_ziti_listen_options, "ziti_listen_options", 264,  56);
            verifyFieldCheck<ziti_host_cfg_v1>(native_structs.f19_ziti_host_cfg_v1, "ziti_host_cfg_v1", 280,  112);
            verifyFieldCheck<ziti_host_cfg_v2>(native_structs.f20_ziti_host_cfg_v2, "ziti_host_cfg_v2", 296,  8);
            verifyFieldCheck<ziti_mfa_enrollment>(native_structs.f21_ziti_mfa_enrollment, "ziti_mfa_enrollment", 312,  24);
            verifyFieldCheck<ziti_port_range>(native_structs.f22_ziti_port_range, "ziti_port_range", 328,  16);
            verifyFieldCheck<ziti_options>(native_structs.f23_ziti_options, "ziti_options", 344,  96);
            verifyFieldCheck<ziti_context_event>(native_structs.f24_ziti_context_event, "ziti_context_event", 360,  56);
            verifyFieldCheck<ziti_router_event>(native_structs.f25_ziti_router_event, "ziti_router_event", 376,  56);
            verifyFieldCheck<ziti_service_event>(native_structs.f26_ziti_service_event, "ziti_service_event", 392,  56);
            verifyFieldCheck<ziti_auth_event>(native_structs.f27_ziti_mfa_auth_event, "ziti_mfa_auth_event", 408,  56);
            verifyFieldCheck<ziti_config_event>(native_structs.f28_ziti_api_event, "ziti_api_event", 424,  56);
#else
            Assert.AreEqual((int)1648, (int)native_structs.size);
            verifyFieldCheck<ziti_id_cfg>(native_structs.f02_ziti_id_cfg, "ziti_id_cfg", 4, 12);
            verifyFieldCheck<ziti_config>(native_structs.f03_ziti_config, "ziti_config", 16, 24);
            verifyFieldCheck<ziti_api_path>(native_structs.f04_api_path, "api_path", 28, 8);
            verifyFieldCheck<ziti_api_versions>(native_structs.f05_ziti_api_versions, "ziti_api_versions", 40, 8);
            verifyFieldCheck<ziti_version>(native_structs.f06_ziti_version, "ziti_version", 52, 20);
            verifyFieldCheck<ziti_identity>(native_structs.f07_ziti_identity, "ziti_identity", 64, 12);
            verifyFieldCheck<ziti_process>(native_structs.f08_ziti_process, "ziti_process", 76, 4);
            verifyFieldCheck<ziti_posture_query>(native_structs.f10_ziti_posture_query_set, "ziti_posture_query_set", 100, 16);
            verifyFieldCheck<ziti_posture_query_set>(native_structs.f11_ziti_session_type, "ziti_session_type", 112, 4);
            verifyFieldCheck<ziti_session_type>(native_structs.f12_ziti_service, "ziti_service", 124, 48);
            verifyFieldCheck<ziti_service>(native_structs.f13_ziti_address_host, "ziti_address_host", 136, 260);
            verifyFieldCheck<ziti_address>(native_structs.f14_ziti_address_cidr, "ziti_address_cidr", 148, 4);
            verifyFieldCheck<ziti_client_cfg_v1>(native_structs.f15_ziti_client_cfg_v1, "ziti_client_cfg_v1", 160, 272);
            verifyFieldCheck<ziti_intercept_cfg_v1>(native_structs.f16_ziti_intercept_cfg_v1, "ziti_intercept_cfg_v1", 172, 24);
            verifyFieldCheck<ziti_server_cfg_v1>(native_structs.f17_ziti_server_cfg_v1, "ziti_server_cfg_v1", 184, 16);
            verifyFieldCheck<ziti_listen_options>(native_structs.f18_ziti_listen_options, "ziti_listen_options", 196, 56);
            verifyFieldCheck<ziti_host_cfg_v1>(native_structs.f19_ziti_host_cfg_v1, "ziti_host_cfg_v1", 208, 64);
            verifyFieldCheck<ziti_host_cfg_v2>(native_structs.f20_ziti_host_cfg_v2, "ziti_host_cfg_v2", 220, 4);
            verifyFieldCheck<ziti_mfa_enrollment>(native_structs.f21_ziti_mfa_enrollment, "ziti_mfa_enrollment", 232, 12);
            verifyFieldCheck<ziti_port_range>(native_structs.f22_ziti_port_range, "ziti_port_range", 244, 16);
            verifyFieldCheck<ziti_options>(native_structs.f23_ziti_options, "ziti_options", 256, 56);
            verifyFieldCheck<ziti_context_event>(native_structs.f24_ziti_context_event, "ziti_context_event", 268, 28);
            verifyFieldCheck<ziti_router_event>(native_structs.f25_ziti_router_event, "ziti_router_event", 280, 28);
            verifyFieldCheck<ziti_service_event>(native_structs.f26_ziti_service_event, "ziti_service_event", 292, 28);
            verifyFieldCheck<ziti_auth_event>(native_structs.f27_ziti_mfa_auth_event, "ziti_mfa_auth_event", 304, 28);
            verifyFieldCheck<ziti_config_event>(native_structs.f28_ziti_api_event, "ziti_api_event", 316, 28);
#endif
            ziti_types_with_values values = Marshal.PtrToStructure<ziti_types_with_values>(testData);
            
            Assert.AreEqual("cert", values.ziti_id_cfg.cert);
            Assert.AreEqual("key", values.ziti_id_cfg.key);
            Assert.AreEqual("ca", values.ziti_id_cfg.ca);
            
            Assert.AreEqual("controller_url", values.ziti_config.controller_url);
            Assert.AreEqual("cert", values.ziti_config.id.cert);
            Assert.AreEqual("key", values.ziti_config.id.key);
            Assert.AreEqual("ca", values.ziti_config.id.ca);
            
            Assert.AreEqual("path", values.ziti_api_path.path);

            Assert.AreNotEqual(IntPtr.Zero, values.ziti_api_versions.edge);

            Assert.AreEqual("version", values.ziti_version.version);
            Assert.AreEqual("revision", values.ziti_version.revision);
            Assert.AreEqual("build_date", values.ziti_version.build_date);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_version.api_versions);

            Assert.AreEqual("id", values.ziti_identity.id);
            Assert.AreEqual("name", values.ziti_identity.name);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_identity.app_data);

            Assert.AreEqual("path", values.ziti_process.path);

            Assert.AreEqual("id", values.ziti_posture_query.id);
            Assert.IsTrue(values.ziti_posture_query.is_passing);
            Assert.AreEqual(ziti_posture_query_type.PC_Domain, values.ziti_posture_query.query_type);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_posture_query.process);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_posture_query.processes);
            Assert.AreEqual(10L, values.ziti_posture_query.timeout);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_posture_query.timeoutRemaining);
            // NOTE: the shim's z4d_struct_test stores &time_remain where time_remain is a STACK local, so the
            // pointee is freed by the time we read it here. Dereferencing it is undefined; only the non-null
            // pointer is checked. (Shim test-data bug: time_remain should be heap-allocated.)
            Assert.AreEqual("updated_at", values.ziti_posture_query.updated_at);
            
            Assert.AreEqual("policy_id", values.ziti_posture_query_set.policy_id);
            Assert.IsTrue(values.ziti_posture_query_set.is_passing);
            Assert.AreEqual("policy_type", values.ziti_posture_query_set.policy_type);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_posture_query_set.posture_queries);
            
            Assert.AreEqual(ziti_session_type.Dial, values.ziti_session_type);
            
            Assert.AreEqual("elem1id", values.ziti_service.id);
            Assert.AreEqual("elem1", values.ziti_service.name);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_service.permissions);
            Assert.IsTrue(values.ziti_service.encryption);
            Assert.AreEqual(111L, values.ziti_service.perm_flags);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_service.config);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_service.posture_query_set);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_service.posture_query_map);
            Assert.AreEqual("updated_at", values.ziti_service.updated_at);
            
            Assert.AreEqual(ziti_address_type.Host, values.ziti_address_host.Type);
            Assert.AreEqual("hostname", values.ziti_address_host.Hostname);
            
            Assert.AreEqual(ziti_address_type.CIDR, values.ziti_address_cidr.Type);
            Assert.AreEqual(AddressFamily.InterNetwork, values.ziti_address_cidr.AF);
            Assert.AreEqual(8, values.ziti_address_cidr.Bits);
            Assert.AreEqual("100.200.50.25", values.ziti_address_cidr.IP.ToString());
            
            Assert.AreEqual(ziti_address_type.Host, values.ziti_client_cfg_v1.hostname.Type);
            Assert.AreEqual("hostname", values.ziti_client_cfg_v1.hostname.Hostname);
            Assert.AreEqual(80L, values.ziti_client_cfg_v1.port);

            Assert.AreNotEqual(IntPtr.Zero, values.ziti_intercept_cfg_v1.protocols);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_intercept_cfg_v1.addresses);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_intercept_cfg_v1.port_ranges);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_intercept_cfg_v1.dial_options);
            Assert.AreEqual("source_ip", values.ziti_intercept_cfg_v1.source_ip);

            Assert.AreEqual("protocol", values.ziti_server_cfg_v1.protocol);
            Assert.AreEqual("hostname", values.ziti_server_cfg_v1.hostname);
            Assert.AreEqual(443L, values.ziti_server_cfg_v1.port);

            Assert.IsTrue(values.ziti_listen_options.bind_with_identity);
            Assert.AreEqual((ulong)1000000, values.ziti_listen_options.connect_timeout);
            Assert.AreEqual(100L, values.ziti_listen_options.connect_timeout_seconds);
            Assert.AreEqual(9L, values.ziti_listen_options.cost);
            Assert.AreEqual("identity", values.ziti_listen_options.identity);
            Assert.AreEqual(10L, values.ziti_listen_options.max_connections);
            Assert.AreEqual("precedence", values.ziti_listen_options.precedence);
            
            Assert.AreEqual("protocol", values.ziti_host_cfg_v1.protocol);
            Assert.IsTrue(values.ziti_host_cfg_v1.forward_protocol);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_host_cfg_v1.allowed_protocols);
            Assert.AreEqual("address", values.ziti_host_cfg_v1.address);
            Assert.IsTrue(values.ziti_host_cfg_v1.forward_address);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_host_cfg_v1.allowed_addresses);
            Assert.AreEqual(1090L, values.ziti_host_cfg_v1.port);
            Assert.IsTrue(values.ziti_host_cfg_v1.forward_port);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_host_cfg_v1.allowed_port_ranges);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_host_cfg_v1.allowed_source_addresses);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_host_cfg_v1.listen_options);
            
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_host_cfg_v2.terminators);
            
            Assert.IsTrue(values.ziti_mfa_enrollment.is_verified);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_mfa_enrollment.recovery_codes);
            Assert.AreEqual("provisioningUrl", values.ziti_mfa_enrollment.provisioning_url);
            
            Assert.AreEqual(80L, values.ziti_port_range.low);
            Assert.AreEqual(443L, values.ziti_port_range.high);

            // 1.16: ziti_options dropped `config` and `router_keepalive`.
            Assert.IsTrue(values.ziti_options.disabled);
            Assert.AreNotEqual(IntPtr.Zero, values.ziti_options.config_types);
            Assert.AreEqual((uint)232323, values.ziti_options.api_page_size);
            Assert.AreEqual((int)3322, (int)values.ziti_options.refresh_interval);
            Assert.AreEqual(ziti_metric_type.EWMA_15m, values.ziti_options.metrics_type);
            Assert.AreEqual("ctxhere", Marshal.PtrToStringUTF8(values.ziti_options.app_ctx));
            Assert.AreEqual((uint)98, values.ziti_options.events);

            // Events: the alignment payload stores the bare union sub-structs (no leading event-type tag).
            Assert.AreEqual(245, values.ziti_context_event.ctrl_status);
            Assert.AreEqual("ziti_context_event_err_0__", values.ziti_context_event.err);

            ziti_router_event rev = values.ziti_router_event;
            Assert.AreEqual(ziti_router_status.EdgeRouterConnected, rev.status);
            Assert.AreEqual("ere_name", rev.name);
            Assert.AreEqual("ere_address", rev.address);
            Assert.AreEqual("ere_version", rev.version);

            ziti_service_event svcev = values.ziti_service_event;
            Assert.AreNotEqual(IntPtr.Zero, svcev.removed);
            Assert.AreNotEqual(IntPtr.Zero, svcev.changed);
            Assert.AreNotEqual(IntPtr.Zero, svcev.added);
            Assert.AreEqual(svcev.added, svcev.removed);
            Assert.AreEqual(svcev.changed, svcev.removed);
            Assert.AreEqual(svcev.changed, svcev.added);
            IntPtr ptr1 = Native.API.z4d_service_array_get(svcev.removed, 0);
            IntPtr ptr2 = Native.API.z4d_service_array_get(svcev.removed, 1);
            ziti_service removed1 = Marshal.PtrToStructure<ziti_service>(ptr1);
            ziti_service removed2 = Marshal.PtrToStructure<ziti_service>(ptr2);

            Assert.AreEqual("elem1", removed1.name);
            Assert.AreEqual("elem2", removed2.name);
            Assert.AreEqual("elem1id", removed1.id);
            Assert.AreEqual("elem2id", removed2.id);
            Assert.AreEqual(111L, removed1.perm_flags);
            Assert.AreEqual(222L, removed2.perm_flags);

            // 1.16: the former ziti_api_event is now ziti_config_event { identity_name, config }.
            ziti_config_event cfgev = values.ziti_config_event;
            Assert.AreEqual("new_ctrl_address", cfgev.identity_name);
            Assert.AreNotEqual(IntPtr.Zero, cfgev.config);

            Log.Info("test complete");
        }
    }
}
