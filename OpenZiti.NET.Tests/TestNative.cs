using System;
using System.Net.Sockets;
using System.Runtime.InteropServices;
using Microsoft.VisualStudio.TestTools.UnitTesting;
using OpenZiti.Native;

namespace OpenZiti.NET.Tests {
    [TestClass]
    public class NativeCodeAlignmentChecker {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        private static void verifyFieldCheck(AlignmentCheck check, string expectedChecksum, uint expectedOffset, uint expectedSize) {
            Assert.AreEqual(expectedChecksum, check.checksum);
            Log.Info($"{check.checksum}, {check.offset},  {check.size}");

            Assert.AreEqual(expectedOffset, check.offset);
            Assert.AreEqual(expectedSize, check.size);
        }

        [TestMethod]
        public void TestCSDKStructAlignments() {
            Log.Info("test begins with: " + Native.API.GetZitiPath());

            IntPtr testData = TestBlitting.z4d_struct_test();
            ziti_types native_structs = Marshal.PtrToStructure<ziti_types>(testData);
            byte[] managedArray = new byte[native_structs.info.total_size];
            Marshal.Copy(testData, managedArray, 0, (int)native_structs.info.total_size);

            Log.Info("----");
#if ZITI_64BIT
            verifyFieldCheck(native_structs.f01_check, "ziti_auth_query_mfa", 24,  48);
            verifyFieldCheck(native_structs.f02_check, "ziti_id_cfg", 88,  24);
            verifyFieldCheck(native_structs.f03_check, "ziti_config", 128,  32);
            verifyFieldCheck(native_structs.f04_check, "api_path", 176,  8);
            verifyFieldCheck(native_structs.f05_check, "ziti_api_versions", 200,  8);
            verifyFieldCheck(native_structs.f06_check, "ziti_version", 224,  32);
            verifyFieldCheck(native_structs.f07_check, "ziti_identity", 272,  24);
            verifyFieldCheck(native_structs.f08_check, "ziti_process", 312,  8);
            verifyFieldCheck(native_structs.f10_check, "ziti_posture_query_set", 416,  32);
            verifyFieldCheck(native_structs.f11_check, "ziti_session_type", 464,  4);
            verifyFieldCheck(native_structs.f12_check, "ziti_service", 488,  64);
            verifyFieldCheck(native_structs.f13_check, "ziti_address_host", 568,  260);
            verifyFieldCheck(native_structs.f14_check, "ziti_address_cidr", 848,  4);
            verifyFieldCheck(native_structs.f15_check, "ziti_client_cfg_v1", 1128,  264);
            verifyFieldCheck(native_structs.f16_check, "ziti_intercept_cfg_v1", 1408,  40);
            verifyFieldCheck(native_structs.f17_check, "ziti_server_cfg_v1", 1464,  24);
            verifyFieldCheck(native_structs.f18_check, "ziti_listen_options", 1504,  48);
            verifyFieldCheck(native_structs.f19_check, "ziti_host_cfg_v1", 1568,  80);
            verifyFieldCheck(native_structs.f20_check, "ziti_host_cfg_v2", 1664,  8);
            verifyFieldCheck(native_structs.f21_check, "ziti_mfa_enrollment", 1688,  24);
            verifyFieldCheck(native_structs.f22_check, "ziti_port_range", 1728,  8);
            verifyFieldCheck(native_structs.f23_check, "ziti_options", 1752,  112);
            verifyFieldCheck(native_structs.f24_check, "ziti_context_event", 1880,  40);
            verifyFieldCheck(native_structs.f25_check, "ziti_router_event", 1936,  40);
            verifyFieldCheck(native_structs.f26_check, "ziti_service_event", 1992,  40);
            verifyFieldCheck(native_structs.f27_check, "ziti_mfa_auth_event", 2048,  40);
            verifyFieldCheck(native_structs.f28_check, "ziti_api_event", 2104,  40);
#else
            verifyFieldCheck(native_structs.f01_check, "ziti_auth_query_mfa", 16, 28);
            verifyFieldCheck(native_structs.f02_check, "ziti_id_cfg", 56, 12);
            verifyFieldCheck(native_structs.f03_check, "ziti_config", 80, 16);
            verifyFieldCheck(native_structs.f04_check, "api_path", 108, 4);
            verifyFieldCheck(native_structs.f05_check, "ziti_api_versions", 124, 4);
            verifyFieldCheck(native_structs.f06_check, "ziti_version", 140, 16);
            verifyFieldCheck(native_structs.f07_check, "ziti_identity", 168, 12);
            verifyFieldCheck(native_structs.f08_check, "ziti_process", 192, 4);
            verifyFieldCheck(native_structs.f10_check, "ziti_posture_query_set", 252, 16);
            verifyFieldCheck(native_structs.f11_check, "ziti_session_type", 280, 4);
            verifyFieldCheck(native_structs.f12_check, "ziti_service", 296, 36);
            verifyFieldCheck(native_structs.f13_check, "ziti_address_host", 344, 260);
            verifyFieldCheck(native_structs.f14_check, "ziti_address_cidr", 616, 4);
            verifyFieldCheck(native_structs.f15_check, "ziti_client_cfg_v1", 888, 264);
            verifyFieldCheck(native_structs.f16_check, "ziti_intercept_cfg_v1", 1164, 20);
            verifyFieldCheck(native_structs.f17_check, "ziti_server_cfg_v1", 1196, 12);
            verifyFieldCheck(native_structs.f18_check, "ziti_listen_options", 1224, 40);
            verifyFieldCheck(native_structs.f19_check, "ziti_host_cfg_v1", 1276, 44);
            verifyFieldCheck(native_structs.f20_check, "ziti_host_cfg_v2", 1332, 4);
            verifyFieldCheck(native_structs.f21_check, "ziti_mfa_enrollment", 1348, 12);
            verifyFieldCheck(native_structs.f22_check, "ziti_port_range", 1372, 8);
            verifyFieldCheck(native_structs.f23_check, "ziti_options", 1392, 64);
            verifyFieldCheck(native_structs.f24_check, "ziti_context_event", 1468, 20);
            verifyFieldCheck(native_structs.f25_check, "ziti_router_event", 1500, 20);
            verifyFieldCheck(native_structs.f26_check, "ziti_service_event", 1532, 20);
            verifyFieldCheck(native_structs.f27_check, "ziti_mfa_auth_event", 1564, 20);
            verifyFieldCheck(native_structs.f28_check, "ziti_api_event", 1596, 20);
#endif
            Assert.AreEqual("type", native_structs.f01_ziti_auth_query_mfa.type_id);
            Assert.AreEqual("provider", native_structs.f01_ziti_auth_query_mfa.provider);
            Assert.AreEqual("http_method", native_structs.f01_ziti_auth_query_mfa.http_method);
            Assert.AreEqual("http_url", native_structs.f01_ziti_auth_query_mfa.http_url);
            Assert.AreEqual(81, native_structs.f01_ziti_auth_query_mfa.min_length);
            Assert.AreEqual(92, native_structs.f01_ziti_auth_query_mfa.max_length);
            Assert.AreEqual("format", native_structs.f01_ziti_auth_query_mfa.format);

            Assert.AreEqual("cert", native_structs.f02_ziti_id_cfg.cert);
            Assert.AreEqual("key", native_structs.f02_ziti_id_cfg.key);
            Assert.AreEqual("ca", native_structs.f02_ziti_id_cfg.ca);

            Assert.AreEqual("controller_url", native_structs.f03_ziti_config.controller_url);
            Assert.AreEqual("cert", native_structs.f03_ziti_config.id.cert);
            Assert.AreEqual("key", native_structs.f03_ziti_config.id.key);
            Assert.AreEqual("ca", native_structs.f03_ziti_config.id.ca);

            Assert.AreEqual("path", native_structs.f04_api_path.path);

            Assert.AreNotEqual(IntPtr.Zero, native_structs.f05_ziti_api_versions.api_path_map);

            Assert.AreEqual("version", native_structs.f06_ziti_version.version);
            Assert.AreEqual("revision", native_structs.f06_ziti_version.revision);
            Assert.AreEqual("build_date", native_structs.f06_ziti_version.build_date);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f06_ziti_version.api_versions);

            Assert.AreEqual("id", native_structs.f07_ziti_identity.id);
            Assert.AreEqual("name", native_structs.f07_ziti_identity.name);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f07_ziti_identity.tags);

            Assert.AreEqual("path", native_structs.f08_ziti_process.path);

            Assert.AreEqual("id", native_structs.f09_ziti_posture_query.id);
            Assert.AreEqual(true, native_structs.f09_ziti_posture_query.is_passing);
            Assert.AreEqual("query_type", native_structs.f09_ziti_posture_query.query_type);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f09_ziti_posture_query.process);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f09_ziti_posture_query.processes);
            Assert.AreEqual(10, native_structs.f09_ziti_posture_query.timeout);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f09_ziti_posture_query.timeoutRemaining);
            int tor = Marshal.ReadInt32(native_structs.f09_ziti_posture_query.timeoutRemaining);
            Assert.AreEqual(tor, 20);
            Assert.AreEqual("updated_at", native_structs.f09_ziti_posture_query.updated_at);

            Assert.AreEqual("policy_id", native_structs.f10_ziti_posture_query_set.policy_id);
            Assert.AreEqual(true, native_structs.f10_ziti_posture_query_set.is_passing);
            Assert.AreEqual("policy_type", native_structs.f10_ziti_posture_query_set.policy_type);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f10_ziti_posture_query_set.posture_queries);

            Assert.AreEqual(ziti_session_type.Dial, native_structs.f11_ziti_session_type);

            Assert.AreEqual("id", native_structs.f12_ziti_service.id);
            Assert.AreEqual("name", native_structs.f12_ziti_service.name);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f12_ziti_service.permissions);
            Assert.AreEqual(true, native_structs.f12_ziti_service.encryption);
            Assert.AreEqual(214, native_structs.f12_ziti_service.perm_flags);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f12_ziti_service.config);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f12_ziti_service.posture_query_set);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f12_ziti_service.posture_query_map);
            Assert.AreEqual("updated_at", native_structs.f12_ziti_service.updated_at);

            Assert.AreEqual(ziti_address_type.Host, native_structs.f13_ziti_address_host.Type);
            Assert.AreEqual("hostname", native_structs.f13_ziti_address_host.Hostname);

            Assert.AreEqual(ziti_address_type.CIDR, native_structs.f14_ziti_address_cidr.Type);
            Assert.AreEqual(AddressFamily.InterNetwork, native_structs.f14_ziti_address_cidr.AF);
            Assert.AreEqual(8, native_structs.f14_ziti_address_cidr.Bits);
            Assert.AreEqual("100.200.50.25", native_structs.f14_ziti_address_cidr.IP.ToString());

            Assert.AreEqual(ziti_address_type.Host, native_structs.f15_ziti_client_cfg_v1.hostname.Type);
            Assert.AreEqual("hostname", native_structs.f15_ziti_client_cfg_v1.hostname.Hostname);
            Assert.AreEqual(80, native_structs.f15_ziti_client_cfg_v1.port);

            Assert.AreNotEqual(IntPtr.Zero, native_structs.f16_ziti_intercept_cfg_v1.protocols);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f16_ziti_intercept_cfg_v1.addresses);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f16_ziti_intercept_cfg_v1.port_ranges);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f16_ziti_intercept_cfg_v1.dial_options_map);
            Assert.AreEqual("source_ip", native_structs.f16_ziti_intercept_cfg_v1.source_ip);

            Assert.AreEqual("protocol", native_structs.f17_ziti_server_cfg_v1.protocol);
            Assert.AreEqual("hostname", native_structs.f17_ziti_server_cfg_v1.hostname);
            Assert.AreEqual(443, native_structs.f17_ziti_server_cfg_v1.port);

            Assert.AreEqual(true, native_structs.f18_ziti_listen_options.bind_with_identity);
            Assert.AreEqual((ulong)1000000, native_structs.f18_ziti_listen_options.connect_timeout);
            Assert.AreEqual(100, native_structs.f18_ziti_listen_options.connect_timeout_seconds);
            Assert.AreEqual(9, native_structs.f18_ziti_listen_options.cost);
            Assert.AreEqual("identity", native_structs.f18_ziti_listen_options.identity);
            Assert.AreEqual(10, native_structs.f18_ziti_listen_options.max_connections);
            Assert.AreEqual("precedence", native_structs.f18_ziti_listen_options.precedence);

            Assert.AreEqual("protocol", native_structs.f19_ziti_host_cfg_v1.protocol);
            Assert.AreEqual(true, native_structs.f19_ziti_host_cfg_v1.forward_protocol);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f19_ziti_host_cfg_v1.allowed_protocols);
            Assert.AreEqual("address", native_structs.f19_ziti_host_cfg_v1.address);
            Assert.AreEqual(true, native_structs.f19_ziti_host_cfg_v1.forward_address);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f19_ziti_host_cfg_v1.allowed_addresses);
            Assert.AreEqual(1090, native_structs.f19_ziti_host_cfg_v1.port);
            Assert.AreEqual(true, native_structs.f19_ziti_host_cfg_v1.forward_port);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f19_ziti_host_cfg_v1.allowed_port_ranges);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f19_ziti_host_cfg_v1.allowed_source_addresses);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f19_ziti_host_cfg_v1.listen_options);

            Assert.AreNotEqual(IntPtr.Zero, native_structs.f20_ziti_host_cfg_v2.terminators);

            Assert.AreEqual(true, native_structs.f21_ziti_mfa_enrollment.is_verified);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f21_ziti_mfa_enrollment.recovery_codes);
            Assert.AreEqual("provisioningUrl", native_structs.f21_ziti_mfa_enrollment.provisioning_url);

            Assert.AreEqual(80, native_structs.f22_ziti_port_range.low);
            Assert.AreEqual(443, native_structs.f22_ziti_port_range.high);

            Assert.AreEqual("config", native_structs.f23_ziti_options.config);
            Assert.AreEqual("controller", native_structs.f23_ziti_options.controller);
            Assert.AreEqual(IntPtr.Zero, native_structs.f23_ziti_options.tls);
            Assert.AreEqual(true, native_structs.f23_ziti_options.disabled);
            Assert.AreNotEqual(IntPtr.Zero, native_structs.f23_ziti_options.config_types);
            Assert.AreEqual((uint)232323, native_structs.f23_ziti_options.api_page_size);
            Assert.AreEqual((int)3322, (int)native_structs.f23_ziti_options.refresh_interval);
            Assert.AreEqual(ziti_metric_type.EWMA_15m, native_structs.f23_ziti_options.metrics_type);
            Assert.AreEqual(111, native_structs.f23_ziti_options.router_keepalive);
            Assert.AreEqual("ctxhere", Marshal.PtrToStringUTF8(native_structs.f23_ziti_options.app_ctx));
            Assert.AreEqual((uint)98, native_structs.f23_ziti_options.events);

            Assert.AreEqual(ziti_event_type.ZitiContextEvent, native_structs.f24_ziti_context_event.ziti_event_type);
            //ziti_context_event ctx = new();
            //ctx = TestBlitting.ToContextEvent(ctx, native_structs.f24_ziti_context_event._union);
            //ctx = native_structs.f24_ziti_context_event;
            //Assert.AreEqual(245, native_structs.f24_ziti_context_event.ContextEvent.ctrl_status);
            //Assert.AreEqual("ziti_context_event_err_0__", native_structs.f24_ziti_context_event.ContextEvent.err);
            Assert.AreEqual(245, native_structs.f24_ziti_context_event.ctrl_status);
            Assert.AreEqual("ziti_context_event_err_0__", native_structs.f24_ziti_context_event.err);

            ziti_router_event rev = native_structs.f25_ziti_router_event;
            //rev = TestBlitting.ToContextEvent(rev, native_structs.f25_ziti_router_event._union);
            Assert.AreEqual(ziti_event_type.ZitiRouterEvent, native_structs.f25_ziti_router_event.ziti_event_type);
            Assert.AreEqual(ziti_router_status.EdgeRouterConnected, rev.status);
            Assert.AreEqual("ere_name", rev.name);
            Assert.AreEqual("ere_address", rev.address);
            Assert.AreEqual("ere_version", rev.version);

            ziti_service_event svcev = native_structs.f26_ziti_service_event;
            //svcev = TestBlitting.ToContextEvent(svcev, native_structs.f26_ziti_service_event._union);
            Assert.AreEqual(ziti_event_type.ZitiServiceEvent, native_structs.f26_ziti_service_event.ziti_event_type);
            Assert.AreNotEqual(IntPtr.Zero, svcev.removed);
            Assert.AreNotEqual(IntPtr.Zero, svcev.changed);
            Assert.AreNotEqual(IntPtr.Zero, svcev.added);
            Assert.AreEqual(svcev.added, svcev.removed);
            Assert.AreEqual(svcev.changed, svcev.removed);
            Assert.AreEqual(svcev.changed, svcev.added);

            ziti_mfa_auth_event mfaae = native_structs.f27_ziti_mfa_auth_event;
            //mfaae = TestBlitting.ToContextEvent(mfaae, native_structs.f27_ziti_mfa_auth_event._union);
            //            Assert.AreEqual(ziti_event_type.ZitiMfaAuthEvent, native_structs.f27_ziti_mfa_auth_event.ziti_event_type);
            //            Assert.AreNotEqual(IntPtr.Zero, mfaae.ziti_auth_query_mfa);
            //            ziti_auth_query_mfa mfa = Marshal.PtrToStructure<ziti_auth_query_mfa>(mfaae.ziti_auth_query_mfa);
            //            Assert.Equal("type", mfa.type_id);
            //            Assert.Equal("provider", mfa.provider);
            //            Assert.Equal("http_method", mfa.http_method);
            //            Assert.Equal("http_url", mfa.http_url);
            //            Assert.Equal(81, mfa.min_length);
            //            Assert.Equal(92, mfa.max_length);
            //            Assert.Equal("format", mfa.format);

            Assert.AreEqual(ziti_event_type.ZitiAPIEvent, native_structs.f28_ziti_api_event.ziti_event_type);

            Log.Info("test complete");
        }
    }
}
