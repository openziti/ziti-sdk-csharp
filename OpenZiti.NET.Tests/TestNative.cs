using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenZiti.Native;
using System.Drawing;

namespace OpenZiti.NET.Tests {
    [TestClass]
    // Marshals the native-populated z4d_struct_test() blob and asserts the managed structs read the KNOWN values
    // the native side wrote (string round-trips, enum values, the ziti_address accessors, bool widths). The
    // expected values are the native's inputs, so this is not circular. Per-field layout (offsets/sizes) is owned
    // by NativeLayoutChecker (live, from z4d_layout_report); this test covers marshalling/accessor faithfulness.
    public class NativeCodeValueChecker {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        [TestMethod]
        public void TestCSDKStructValues() {
            Log.Info("test begins with: " + Native.API.GetZitiPath());
            IntPtr testData = TestBlitting.z4d_struct_test();
            
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
