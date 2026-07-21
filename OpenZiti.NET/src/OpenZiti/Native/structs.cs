/*
Copyright NetFoundry Inc.

Licensed under the Apache License, Version 2.0 (the "License");
you may not use this file except in compliance with the License.
You may obtain a copy of the License at

https://www.apache.org/licenses/LICENSE-2.0

Unless required by applicable law or agreed to in writing, software
distributed under the License is distributed on an "AS IS" BASIS,
WITHOUT WARRANTIES OR CONDITIONS OF ANY KIND, either express or implied.
See the License for the specific language governing permissions and
limitations under the License.
*/

using System;
using System.Runtime.InteropServices;
using System.Text;
using System.Net.Sockets;
using System.Net;
using System.Runtime.CompilerServices;

[assembly: InternalsVisibleTo("OpenZiti.NET.Tests")]
namespace OpenZiti.Native {
#pragma warning disable 0649
#pragma warning disable 0169

    public class TestBlitting {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        public const int ZITI_EVENT_UNION_SIZE = TestBlitting.ptr * 7;
#if ZITI_64BIT
        public const int ptr = 8;
#else
        public const int ptr = 4;
#endif
        //Z4D_API ziti_types_t* z4d_struct_test();
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_struct_test", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_struct_test();

        //Z4D_API const char* z4d_layout_report();
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_layout_report", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_layout_report();

        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_ziti_posture_query", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_ziti_posture_query();

        public static T ToContextEvent<T>(T desired, IntPtr /*byte[] input*/ input) {
            int size = Marshal.SizeOf(desired);
            IntPtr ptr = IntPtr.Zero;
            try {
                ptr = Marshal.AllocHGlobal(size);
                byte[] destination = new byte[size];
                Marshal.Copy(input, destination, 0, size);
                Marshal.Copy(destination, 0, ptr, size);

#pragma warning disable CS8600 // Converting null literal or possible null value to non-nullable type.
#pragma warning disable CS8602 // Dereference of a possibly null reference.
                desired = (T)Marshal.PtrToStructure(ptr, desired.GetType());
#pragma warning restore CS8602 // Dereference of a possibly null reference.
#pragma warning restore CS8600 // Converting null literal or possible null value to non-nullable type.
            } finally {
                Marshal.FreeHGlobal(ptr);
            }

#pragma warning disable CS8603 // Possible null reference return.
            return desired;
#pragma warning restore CS8603 // Possible null reference return.
        }
    }

    public struct size_t {
#if ZITI_64BIT
        public long val;
#else
        public int val;
#endif
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct AlignmentCheck {
        [FieldOffset(0)] public uint offset;
        [FieldOffset(4)] public uint size;
        [FieldOffset(8)] public string checksum;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_types_info {
        public uint total_size;
        public string checksum;
    }


    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_types {
        public IntPtr size;
        public AlignmentCheck /*ziti_id_cfg*/ f02_ziti_id_cfg;
        public AlignmentCheck /*ziti_config*/ f03_ziti_config;
        public AlignmentCheck /*api_path*/ f04_api_path;
        public AlignmentCheck /*ziti_api_versions*/ f05_ziti_api_versions;
        public AlignmentCheck /*ziti_version*/ f06_ziti_version;
        public AlignmentCheck /*ziti_identity*/ f07_ziti_identity;
        public AlignmentCheck /*ziti_process*/ f08_ziti_process;
        public AlignmentCheck /*ziti_posture_query*/ f09_ziti_posture_query;
        public AlignmentCheck /*ziti_posture_query_set*/ f10_ziti_posture_query_set;
        public AlignmentCheck /*ziti_session_type*/ f11_ziti_session_type;
        public AlignmentCheck /*ziti_service*/ f12_ziti_service;
        public AlignmentCheck /*ziti_address*/ f13_ziti_address_host;
        public AlignmentCheck /*ziti_address*/ f14_ziti_address_cidr;
        public AlignmentCheck /*ziti_client_cfg_v1*/ f15_ziti_client_cfg_v1;
        public AlignmentCheck /*ziti_intercept_cfg_v1*/ f16_ziti_intercept_cfg_v1;
        public AlignmentCheck /*ziti_server_cfg_v1*/ f17_ziti_server_cfg_v1;
        public AlignmentCheck /*ziti_listen_options*/ f18_ziti_listen_options;
        public AlignmentCheck /*ziti_host_cfg_v1*/ f19_ziti_host_cfg_v1;
        public AlignmentCheck /*ziti_host_cfg_v2*/ f20_ziti_host_cfg_v2;
        public AlignmentCheck /*ziti_mfa_enrollment*/ f21_ziti_mfa_enrollment;
        public AlignmentCheck /*ziti_port_range*/ f22_ziti_port_range;
        public AlignmentCheck /*ziti_options*/ f23_ziti_options;

        //events
        public AlignmentCheck /*ziti_event_t*/ f24_ziti_context_event;
        public AlignmentCheck /*ziti_event_t*/ f25_ziti_router_event;
        public AlignmentCheck /*ziti_event_t*/ f26_ziti_service_event;
        public AlignmentCheck /*ziti_event_t*/ f27_ziti_mfa_auth_event;
        public AlignmentCheck /*ziti_event_t*/ f28_ziti_api_event;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_types_with_values {
        public ziti_types types;
        public ziti_id_cfg ziti_id_cfg;
        public ziti_config ziti_config;
        public ziti_api_path ziti_api_path;
        public ziti_api_versions ziti_api_versions;
        public ziti_version ziti_version;
        public ziti_identity ziti_identity;
        public ziti_process ziti_process;
        public ziti_posture_query ziti_posture_query;
        public ziti_posture_query_set ziti_posture_query_set;
        public ziti_session_type ziti_session_type;
        public ziti_service ziti_service;
        public ziti_address ziti_address_host;
        public ziti_address ziti_address_cidr;
        public ziti_client_cfg_v1 ziti_client_cfg_v1;
        public ziti_intercept_cfg_v1 ziti_intercept_cfg_v1;
        public ziti_server_cfg_v1 ziti_server_cfg_v1;
        public ziti_listen_options ziti_listen_options;
        public ziti_host_cfg_v1 ziti_host_cfg_v1;
        public ziti_host_cfg_v2 ziti_host_cfg_v2;
        public ziti_mfa_enrollment ziti_mfa_enrollment;
        public ziti_port_range ziti_port_range;
        public ziti_options ziti_options;
        public ziti_context_event ziti_context_event;
        public ziti_router_event ziti_router_event;
        public ziti_service_event ziti_service_event;
        public ziti_auth_event ziti_auth_event;
        public ziti_config_event ziti_config_event;
    }

    // ziti-sdk-c 1.16: ziti_event_t is { ziti_event_type type; union { ...sub-events... } }.
    // These managed structs model the individual union members (the bare sub-event structs), so they
    // carry NO leading event-type field. The type tag lives on the enclosing ziti_event_t and is read
    // separately (z4d_event_type_from_pointer) before the union is interpreted.

    // struct ziti_context_event { int ctrl_status; const char *err; size_t ctrl_count; struct ctrl_detail_s *ctrl_details; }
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_context_event {
        public int ctrl_status;
        public string err;
        public IntPtr ctrl_count;      // size_t
        public IntPtr ctrl_details;    // struct ctrl_detail_s *
    }

    public enum ziti_router_status {
        EdgeRouterAdded,
        EdgeRouterConnected,
        EdgeRouterDisconnected,
        EdgeRouterRemoved,
        EdgeRouterUnavailable
    }

    // struct ziti_config_event { const char *identity_name; const ziti_config *config; }
    // (was ziti_api_event in pre-1.16; the API-changed event is now the config event.)
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_config_event {
        public string identity_name;
        public IntPtr config;          // const ziti_config *
    };

    public enum ziti_auth_action {
        ziti_auth_cannot_continue,
        ziti_auth_prompt_totp,
        ziti_auth_prompt_pin,
        ziti_auth_select_external,
        ziti_auth_login_external
    }

    // struct ziti_auth_event { enum ziti_auth_action action; const char *error; const char *error_code;
    //                          const char *type; const char *detail; ziti_jwt_signer_array providers; }
    // (was ziti_mfa_auth_event in pre-1.16.)
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_auth_event {
        public ziti_auth_action action;
        public string error;
        public string error_code;
        public string type;
        public string detail;
        public IntPtr providers;       // ziti_jwt_signer_array
    };

    // struct ziti_service_event { ziti_service_array removed; ziti_service_array changed; ziti_service_array added; }
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_service_event {
        public IntPtr removed;
        public IntPtr changed;
        public IntPtr added;
    }

    // struct ziti_router_event { ziti_router_status status; const char *name; const char *address; const char *version; }
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_router_event {
        public ziti_router_status status;
        public string name;
        public string address;
        public string version;
    }

    public enum ziti_event_type {
        ZitiContextEvent = 1,
        ZitiRouterEvent = 1 << 1,
        ZitiServiceEvent = 1 << 2,
        ZitiAuthEvent = 1 << 3,
        ZitiConfigEvent = 1 << 4,
    }


    public enum ziti_metric_type {
        EWMA_1m,
        EWMA_5m,
        EWMA_15m,
        MMA_1m,
        CMA_1m,
        EWMA_5s,
        INSTANT,
    }

    public enum ziti_enroll_mode {
        ziti_enroll_none = 0,
        ziti_enroll_cert,
        ziti_enroll_token,
    }

    public enum ziti_crypto_method {
        ziti_crypto_invalid = -1,
        ziti_crypto_none = 0,
        ziti_crypto_libsodium,
        ziti_crypto_aes_gcm,
    }

    // 1.16: ziti_options no longer carries `config` or `router_keepalive`; it gained
    // `cert_extension_window` and `enroll_mode`. NOTE the C `long refresh_interval` is 4 bytes on
    // Windows (LLP64) but 8 bytes on Linux/macOS (LP64). This managed layout matches Windows, which
    // is where the alignment test runs; the field is not used on the modern socket runtime path.
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_options {
        [MarshalAs(UnmanagedType.I1)] public bool disabled;
        public IntPtr /*public char**/ config_types;
        public ziti_crypto_method e2ee_mode; //end-to-end encryption mode
        public uint api_page_size;
        public uint refresh_interval; //C `long`: seconds between controller refreshes (see note above re: width)
        public ziti_metric_type metrics_type; //an enum describing the metrics to collect

        //posture query cbs
        public ziti_pq_mac_cb pq_mac_cb;
        public ziti_pq_os_cb pq_os_cb;
        public ziti_pq_process_cb pq_process_cb;
        public ziti_pq_domain_cb pq_domain_cb;

        public IntPtr app_ctx;

        public uint events;

        public ziti_event_cb event_cb;

        public uint cert_extension_window;
        public ziti_enroll_mode enroll_mode;


        public ziti_metric_type MetricType {
            get {
                return (ziti_metric_type)metrics_type;
            }
        }
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_port_range {
        public long low;  //model_number (int64) low
        public long high; //model_number (int64) high
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_mfa_enrollment {
        [MarshalAs(UnmanagedType.I1)] public bool is_verified;
        public IntPtr recovery_codes; // convert IntPtr to string array
        public string provisioning_url;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_host_cfg_v2 {
        public IntPtr terminators;//, ziti_host_cfg_v1, list, terminators, __VA_ARGS__)
    }

    // DECLARE_ENUM reserves 0 for Unknown; real values start at 1.
    public enum ziti_proxy_server_type {
        Unknown = 0,
        http,
    }

    // struct ziti_proxy_server { const char *address; ziti_proxy_server_type type; }
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_proxy_server {
        public string address;
        public ziti_proxy_server_type type;
    }

    // 1.16: gained `forward_address_translations` (array) and `proxy` (by-value struct); `port` is model_number (int64).
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_host_cfg_v1 {
        public string protocol;
        [MarshalAs(UnmanagedType.I1)] public bool forward_protocol;
        public IntPtr allowed_protocols;
        public string address;
        [MarshalAs(UnmanagedType.I1)] public bool forward_address;
        public IntPtr forward_address_translations;//, ziti_address_translation, array, forwardAddressTranslations
        public IntPtr allowed_addresses;
        public long port;
        [MarshalAs(UnmanagedType.I1)] public bool forward_port;
        public IntPtr allowed_port_ranges;//, ziti_port_range, array, allowedPortRanges, __VA_ARGS__) \
        public IntPtr allowed_source_addresses;//, ziti_address, array, allowedSourceAddresses, __VA_ARGS__) \
        public ziti_proxy_server proxy;//, ziti_proxy_server, none, proxy
        public IntPtr listen_options;//, ziti_listen_options, ptr, listenOptions, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_listen_options {
        [MarshalAs(UnmanagedType.I1)] public bool bind_with_identity;
        public ulong connect_timeout;          //duration (int64)
        public long connect_timeout_seconds;   //model_number (int64)
        public long cost;                      //model_number (int64)
        public string identity;
        public long max_connections;           //model_number (int64)
        public string precedence;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_server_cfg_v1 {
        public string protocol;
        public string hostname;
        public long port; //model_number (int64)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_intercept_cfg_v1 {
        public IntPtr protocols;
        public IntPtr addresses;
        public IntPtr port_ranges;
        public IntPtr dial_options;   //tag map (was dial_options_map)
        public string source_ip;
        public IntPtr allowed_source_addresses; //1.16: ziti_address list
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_client_cfg_v1 {
        public ziti_address hostname;
        public long port; //model_number (int64)
    }
    public enum ziti_address_type {
        Host = 0,
        CIDR = 1
    }

    [StructLayout(LayoutKind.Sequential, Size = 260)]
    public struct ziti_address {
        private int address_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        public byte[] _union;

        public ziti_address_type Type {
            get {
                return (ziti_address_type)address_type;
            }
        }
        public string Hostname {
            get {
                string hostname = Encoding.UTF8.GetString(_union);
                int nullCharPos = hostname.IndexOf('\0');
                return nullCharPos > -1 ? hostname.Substring(0, nullCharPos) : hostname;
            }
        }

        public AddressFamily AF {
            get {
                return (AddressFamily)BitConverter.ToInt32(new ReadOnlySpan<byte>(_union, 0, 4).ToArray(), 0);
            }
        }

        public int Bits {
            get {
                return BitConverter.ToInt32(new ReadOnlySpan<byte>(_union, 4, 4).ToArray(), 0);
            }
        }

        public IPAddress IP {
            get {
                ReadOnlySpan<byte> ipb = new ReadOnlySpan<byte>(_union, 8, 4);
                IPAddress ip = new IPAddress(ipb.ToArray());
                return ip;
            }
        }

    }
    // DECLARE_ENUM reserves 0 for Unknown; real values start at 1.
    public enum ziti_terminator_strategy {
        Unknown = 0,
        random,
        smartrouting,
        sticky,
        weighted,
    }

    // 1.16: `perm_flags` is model_number (int64); gained `terminator_strategy`. Sequential layout lets the
    // marshaller compute offsets correctly for both 64-bit and 32-bit (win-x86) without hand-coded offsets.
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_service {
        public string id;
        public string name;
        public IntPtr permissions;
        [MarshalAs(UnmanagedType.I1)] public bool encryption;
        public long perm_flags;        //model_number (int64)
        public IntPtr /** json map **/ config;
        public IntPtr /** ziti_posture_query_set[] **/ posture_query_set;
        public IntPtr /** map<string, ziti_posture_query_set> **/ posture_query_map;
        public ziti_terminator_strategy terminator_strategy;
        public string updated_at;
    }
    public enum ziti_session_type {
        Bind = 1,
        Dial = 2
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_posture_query_set {
        public string policy_id;
        [MarshalAs(UnmanagedType.I1)] public bool is_passing;
        public string policy_type;
        public IntPtr posture_queries;
    }

    // ziti-sdk-c DECLARE_ENUM reserves 0 for Unknown, so real values start at 1 (cf. ziti_session_type).
    public enum ziti_posture_query_type {
        Unknown = 0,
        PC_Domain,
        PC_OS,
        PC_Process,
        PC_Process_Multi,
        PC_MAC,
        PC_MFA,
        PC_Endpoint_State,
    }

    // 1.16: `query_type` is now a packed enum (was a string); `timeout` is model_number (int64).
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_posture_query {
        public string id;
        [MarshalAs(UnmanagedType.I1)] public bool is_passing;
        public ziti_posture_query_type query_type;
        public IntPtr process;
        public IntPtr processes;
        public long timeout;           //model_number (int64)
        public IntPtr timeoutRemaining;
        public string updated_at;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_process {
        public string path;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_identity {
        public string id;
        public string name;
        public IntPtr app_data;   //1.16: json map (was `tags`)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_version {
        public string version;
        public string revision;
        public string build_date;
        public IntPtr capabilities;   //1.16: ziti_ctrl_cap array
        public IntPtr api_versions;
    }

    // 1.16: api versions split into `edge` and `oidc` maps (was a single api_path_map).
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_api_versions {
        public IntPtr edge;
        public IntPtr oidc;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_api_path {
        public string path;
        public IntPtr base_urls;   //1.16: model_string array
    }

    // 1.16: gained `controllers` (model_string list) after controller_url.
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_config {
        public string controller_url;
        public IntPtr controllers;
        public ziti_id_cfg id;
        public string cfg_source;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_id_cfg {
        public string cert;
        public string key;
        public string ca;
    }



    // ----- older stuff below





    //TODO: REMOVE
    [StructLayout(LayoutKind.Sequential)]
    public struct model_map_entry {
        public IntPtr key;
        public char key_pad1;
        public char key_pad2;
        public size_t key_len;
        public uint key_hash;
        public IntPtr value;
        public IntPtr _next;
        public IntPtr _tnext;
        public IntPtr _map;
    }
    //
    //
    //    [StructLayout(LayoutKind.Sequential)]
    //    public struct tls_context {
    //
    //    }
    //


    public struct ziti_dial_opts {
        private readonly int connect_timeout_seconds;
        private readonly string identity;
        private readonly IntPtr app_data;
        private size_t app_data_sz;
    }

    public struct ziti_listen_opts {
        public bool bind_with_identity;//, bool, none, bindUsingEdgeIdentity, __VA_ARGS__) \
        public ulong connect_timeout;//, duration, none, connectTimeout, __VA_ARGS__)       \
        public int connect_timeout_seconds;//, int, none, connectTimeoutSeconds, __VA_ARGS__) \
        public int cost;//, int, none, cost, __VA_ARGS__) \
        public string identity;//, string, none, identity, __VA_ARGS__) \
        public int max_connections;//, int, none, maxConnections, __VA_ARGS__)\
        public string precendence;//, string, none, precendence, __VA_ARGS__)
    }















    // -- questionable

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_enroll_options {
        public string jwt;
        public string enroll_key;
        public string enroll_cert;
    };
    public struct model_map_impl {
        public IntPtr /* model_map_entry[] */ entries;
        public IntPtr table;
        public int buckets;
        public size_t size;
    }
#pragma warning restore 0169
#pragma warning restore 0649
}
