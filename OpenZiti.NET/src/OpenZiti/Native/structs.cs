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
        public const int ZITI_EVENT_UNION_SIZE = TestBlitting.ptr * 5;
#if ZITI_64BIT
        public const int ptr = 8;
#else
        public const int ptr = 4;
#endif
        //Z4D_API ziti_types_t* z4d_struct_test();
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_struct_test", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_struct_test();

        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_ziti_posture_query", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_ziti_posture_query();

        public static void Run() {
            IntPtr testData = z4d_struct_test();
            ziti_types native_structs = Marshal.PtrToStructure<ziti_types>(testData);

            byte[] managedArray = new byte[native_structs.info.total_size];
            Marshal.Copy(testData, managedArray, 0, (int)native_structs.info.total_size);

            //IntPtr q = z4d_ziti_posture_query();
            //ziti_posture_query pq = Marshal.PtrToStructure<ziti_posture_query>(q);

            Log.Info("----");
            //IntPtr p = native_structs.f13_ziti_address_host.c;
            //string s = Marshal.PtrToStringUTF8(native_structs.f13_ziti_address_host.c);

            //Log.Info(native_structs.f15_ziti_client_cfg_v1.hostname.Hostname + ":");
            //Log.Info(native_structs.f15_ziti_client_cfg_v1.hostname.Hostname + ":");

        }


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

        [FieldOffset(0 * TestBlitting.ptr)]
        public string checksum;
        [FieldOffset(TestBlitting.ptr)]
        public uint size;
        [FieldOffset(TestBlitting.ptr + 4)]
        public uint offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_types_info {
        public uint total_size;
        public string total_size_check;
        public uint total_size_size;
        public uint total_size_offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_types {
        public ziti_types_info info;

        public ziti_auth_query_mfa f01_ziti_auth_query_mfa;
        public AlignmentCheck f01_check;

        public ziti_id_cfg f02_ziti_id_cfg;
        public AlignmentCheck f02_check;

        public ziti_config f03_ziti_config;
        public AlignmentCheck f03_check;

        public ziti_api_path f04_api_path;
        public AlignmentCheck f04_check;

        public ziti_api_versions f05_ziti_api_versions;
        public AlignmentCheck f05_check;

        public ziti_version f06_ziti_version;
        public AlignmentCheck f06_check;

        public ziti_identity f07_ziti_identity;
        public AlignmentCheck f07_check;

        public ziti_process f08_ziti_process;
        public AlignmentCheck f08_check;

        public ziti_posture_query f09_ziti_posture_query;
        public AlignmentCheck f09_check;

        public ziti_posture_query_set f10_ziti_posture_query_set;
        public AlignmentCheck f10_check;

        public ziti_session_type f11_ziti_session_type;
        public AlignmentCheck f11_check;

        public ziti_service f12_ziti_service;
        public AlignmentCheck f12_check;

        public ziti_address f13_ziti_address_host;
        public AlignmentCheck f13_check;

        public ziti_address f14_ziti_address_cidr;
        public AlignmentCheck f14_check;

        public ziti_client_cfg_v1 f15_ziti_client_cfg_v1;
        public AlignmentCheck f15_check;

        public ziti_intercept_cfg_v1 f16_ziti_intercept_cfg_v1;
        public AlignmentCheck f16_check;

        public ziti_server_cfg_v1 f17_ziti_server_cfg_v1;
        public AlignmentCheck f17_check;

        public ziti_listen_options f18_ziti_listen_options;
        public AlignmentCheck f18_check;

        public ziti_host_cfg_v1 f19_ziti_host_cfg_v1;
        public AlignmentCheck f19_check;

        public ziti_host_cfg_v2 f20_ziti_host_cfg_v2;
        public AlignmentCheck f20_check;

        public ziti_mfa_enrollment f21_ziti_mfa_enrollment;
        public AlignmentCheck f21_check;

        public ziti_port_range f22_ziti_port_range;
        public AlignmentCheck f22_check;

        public ziti_options f23_ziti_options;
        public AlignmentCheck f23_check;

        public ziti_context_event f24_ziti_context_event;
        public AlignmentCheck f24_check;

        public ziti_router_event f25_ziti_router_event;
        public AlignmentCheck f25_check;

        public ziti_service_event f26_ziti_service_event;
        public AlignmentCheck f26_check;

        public ziti_mfa_auth_event f27_ziti_mfa_auth_event;
        public AlignmentCheck f27_check;

        public ziti_api_event f28_ziti_api_event;
        public AlignmentCheck f28_check;
    }

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    public struct ziti_context_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        public ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public int ctrl_status;
        [FieldOffset(2 * TestBlitting.ptr)]
        public string err;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        //public byte[] _union;
    }

    public enum ziti_router_status {
        EdgeRouterAdded,
        EdgeRouterConnected,
        EdgeRouterDisconnected,
        EdgeRouterRemoved,
        EdgeRouterUnavailable
    }

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    public struct ziti_api_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        public ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public string new_ctrl_address;
    };

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    public struct ziti_mfa_auth_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        public ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public IntPtr ziti_auth_query_mfa;
    };

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    public struct ziti_service_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        public ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public IntPtr removed;
        [FieldOffset(2 * TestBlitting.ptr)]
        public IntPtr changed;
        [FieldOffset(3 * TestBlitting.ptr)]
        public IntPtr added;
    }

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    public struct ziti_router_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        public ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public ziti_router_status status;
        [FieldOffset(2 * TestBlitting.ptr)]
        public string name;
        [FieldOffset(3 * TestBlitting.ptr)]
        public string address;
        [FieldOffset(4 * TestBlitting.ptr)]
        public string version;
    }

    //old..[StructLayout(LayoutKind.Sequential)]
    //old..public struct ziti_context_event {
    //old..    public int ctrl_status;
    //old..    public IntPtr err;
    //old..};
    public enum ziti_event_type {
        ZitiContextEvent = 1,
        ZitiRouterEvent = 1 << 1,
        ZitiServiceEvent = 1 << 2,
        ZitiMfaAuthEvent = 1 << 3,
        ZitiAPIEvent = 1 << 4,
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

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_options {
        public string config;
        public string controller;
        public IntPtr tls;
        public bool disabled;
        public IntPtr /*public char**/ config_types;
        public uint api_page_size;
#if ZITI_64BIT
                public UInt32 refresh_interval; //the duration in seconds between checking for updates from the controller
#else
        public int refresh_interval; //the duration in seconds between checking for updates from the controller
#endif
        public ziti_metric_type metrics_type; //an enum describing the metrics to collect
        public int router_keepalive;

        //posture query cbs
        public ziti_pq_mac_cb pq_mac_cb;
        public ziti_pq_os_cb pq_os_cb;
        public ziti_pq_process_cb pq_process_cb;
        public ziti_pq_domain_cb pq_domain_cb;

        public IntPtr app_ctx;

        public uint events;

        public ziti_event_cb event_cb;


        public ziti_metric_type MetricType {
            get {
                return (ziti_metric_type)metrics_type;
            }
        }
    }



    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_port_range {
        public int low; //, int, none, low, __VA_ARGS__) \
        public int high; //, int, none, high, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_mfa_enrollment {
        public bool is_verified;
        public IntPtr recovery_codes; // convert IntPtr to string array
        public string provisioning_url;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_host_cfg_v2 {
        public IntPtr terminators;//, ziti_host_cfg_v1, list, terminators, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_host_cfg_v1 {
        public string protocol;
        public bool forward_protocol;
        public IntPtr allowed_protocols;
        public string address;
        public bool forward_address;
        public IntPtr allowed_addresses;
        public int port;
        public bool forward_port;
        public IntPtr allowed_port_ranges;//, ziti_port_range, array, allowedPortRanges, __VA_ARGS__) \
        public IntPtr allowed_source_addresses;//, ziti_address, array, allowedSourceAddresses, __VA_ARGS__) \
        public IntPtr listen_options;//, ziti_listen_options, ptr, listenOptions, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_listen_options {
        public bool bind_with_identity;
        public ulong connect_timeout;
        public int connect_timeout_seconds;
        public int cost;
        public string identity;
        public int max_connections;
        public string precedence;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_server_cfg_v1 {
        public string protocol;
        public string hostname;
        public int port;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_intercept_cfg_v1 {
        public IntPtr protocols;
        public IntPtr addresses;
        public IntPtr port_ranges;
        public IntPtr dial_options_map;
        public string source_ip;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_client_cfg_v1 {
        public ziti_address hostname;
        public int port;
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
    [StructLayout(LayoutKind.Explicit, Size = (TestBlitting.ptr * 8))]
    public struct ziti_service {
        [FieldOffset(0 * TestBlitting.ptr)]
        public string id;
        [FieldOffset(1 * TestBlitting.ptr)]
        public string name;
        [FieldOffset(2 * TestBlitting.ptr)]
        public IntPtr permissions;
#if ZITI_64BIT
        [FieldOffset(3 * TestBlitting.ptr)]
        public bool encryption;
        [FieldOffset((3 * TestBlitting.ptr) + 4)]
        public int perm_flags;
        [FieldOffset(4 * TestBlitting.ptr)]
        public IntPtr config;
        [FieldOffset(5 * TestBlitting.ptr)]
        public IntPtr /** posture_query_set[] **/ posture_query_set;
        [FieldOffset(6 * TestBlitting.ptr)]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //public byte[] _union;
        public IntPtr /** Dictionary<string, posture_query_set> **/ posture_query_map;
        [FieldOffset(7 * TestBlitting.ptr)]
        public string updated_at;
#else
        [FieldOffset(3 * TestBlitting.ptr)]
        public bool encryption;
        [FieldOffset(4 * TestBlitting.ptr)]
        public int perm_flags;
        [FieldOffset(5 * TestBlitting.ptr)]
        public IntPtr config;
        [FieldOffset(6 * TestBlitting.ptr)]
        public IntPtr /* posture_query_set[] */ posture_query_set;
        [FieldOffset(7 * TestBlitting.ptr)]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //public byte[] _union;
        public IntPtr /*Dictionary<string, posture_query_set> */ posture_query_map;
        [FieldOffset(8 * TestBlitting.ptr)]
        public string updated_at;
#endif
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //public byte[] _union2;
    }
    public enum ziti_session_type {
        Bind = 1,
        Dial = 2
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ziti_posture_query_set {
        [FieldOffset(0 * TestBlitting.ptr)]
        public string policy_id;
        [FieldOffset(1 * TestBlitting.ptr)]
        public bool is_passing;
        [FieldOffset(2 * TestBlitting.ptr)]
        public string policy_type;
        [FieldOffset(3 * TestBlitting.ptr)]
        public IntPtr posture_queries;
    }

    [StructLayout(LayoutKind.Explicit)]
    public struct ziti_posture_query {
        [FieldOffset(0 * TestBlitting.ptr)]
        public string id;
        [FieldOffset(1 * TestBlitting.ptr)]
        public bool is_passing;
        [FieldOffset(2 * TestBlitting.ptr)]
        public string query_type;
        [FieldOffset(3 * TestBlitting.ptr)]
        public IntPtr process;
        [FieldOffset(4 * TestBlitting.ptr)]
        public IntPtr processes;
        [FieldOffset(5 * TestBlitting.ptr)]
        public int timeout;
        [FieldOffset(6 * TestBlitting.ptr)]
        public IntPtr timeoutRemaining;
        [FieldOffset(7 * TestBlitting.ptr)]
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
        public IntPtr tags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_version {
        public string version;
        public string revision;
        public string build_date;
        public IntPtr api_versions;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_api_versions {
        public IntPtr api_path_map;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_api_path {
        public string path;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_config {
        public string controller_url;
        public ziti_id_cfg id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_id_cfg {
        public string cert;
        public string key;
        public string ca;
    }

#if ZITI_64BIT
    [StructLayout(LayoutKind.Explicit, Size = (TestBlitting.ptr * 5))]
#else
    [StructLayout(LayoutKind.Explicit, Size = (TestBlitting.ptr * 6))]
#endif
    public struct ziti_auth_query_mfa {
        [FieldOffset(0 * TestBlitting.ptr)]
        public string type_id;
        [FieldOffset(1 * TestBlitting.ptr)]
        public string provider;
        [FieldOffset(2 * TestBlitting.ptr)]
        public string http_method;
        [FieldOffset(3 * TestBlitting.ptr)]
        public string http_url;
#if ZITI_64BIT
        [FieldOffset(4 * TestBlitting.ptr)]
        public int min_length;
        [FieldOffset((4 * TestBlitting.ptr) + 4)]
        public int max_length;
        [FieldOffset(5 * TestBlitting.ptr)]
        public string format;
#else
        [FieldOffset(4 * TestBlitting.ptr)]
        public int min_length;
        [FieldOffset(5 * TestBlitting.ptr)]
        public int max_length;
        [FieldOffset(6 * TestBlitting.ptr)]
        public string format;
#endif
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
