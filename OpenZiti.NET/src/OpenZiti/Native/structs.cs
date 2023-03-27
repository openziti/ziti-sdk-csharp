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

using System.Runtime.InteropServices;
using System;
using System.Runtime.ConstrainedExecution;
using System.Diagnostics;
using System.Threading;
using System.Drawing;
using System.Text;
using System.Xml.Linq;
using System.Net.Sockets;
using System.Net;
using System.Collections.Generic;
using System.Security.Principal;
using NLog.Fluent;

namespace OpenZiti.Native {
#pragma warning disable 0649
#pragma warning disable 0169

    internal class TestBlitting {
        internal const int ZITI_EVENT_UNION_SIZE = TestBlitting.ptr * 5;
#if ZITI_X64
        internal const int ptr = 8;
#else
        internal const int ptr = 4;
#endif
        //Z4D_API ziti_types_t* z4d_struct_test();
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_struct_test", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_struct_test();

        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_ziti_posture_query", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr z4d_ziti_posture_query();

        public static void Run() {
            IntPtr testData = z4d_struct_test();
            ziti_types native_structs = Marshal.PtrToStructure<ziti_types>(testData);

            byte[] managedArray = new byte[native_structs.total_size];
            Marshal.Copy(testData, managedArray, 0, (int)native_structs.total_size);

            //IntPtr q = z4d_ziti_posture_query();
            //ziti_posture_query pq = Marshal.PtrToStructure<ziti_posture_query>(q);

            Console.WriteLine("----");
            //IntPtr p = native_structs.f13_ziti_address_host.c;
            //string s = Marshal.PtrToStringUTF8(native_structs.f13_ziti_address_host.c);

            //Console.WriteLine(native_structs.f15_ziti_client_cfg_v1.hostname.Hostname + ":");
            //Console.WriteLine(native_structs.f15_ziti_client_cfg_v1.hostname.Hostname + ":");

        }



        public static T ToContextEvent<T>(T desired, IntPtr /*byte[] input*/ input) {
            int size = Marshal.SizeOf(desired);
            IntPtr ptr = IntPtr.Zero;
            try {
                ptr = Marshal.AllocHGlobal(size);
                byte[] destination = new byte[size];
                Marshal.Copy(input, destination, 0, size);
                Marshal.Copy(destination, 0, ptr, size);

                desired = (T)Marshal.PtrToStructure(ptr, desired.GetType());
            } finally {
                Marshal.FreeHGlobal(ptr);
            }

            return desired;
        }
    }

    public struct size_t {
#if ZITI_X64
        public long val;
#else
        public int val;
#endif
    }

    [StructLayout(LayoutKind.Explicit)]
    internal struct AlignmentCheck {

        [FieldOffset(0 * TestBlitting.ptr)]
        internal string checksum;
        [FieldOffset(TestBlitting.ptr)]
        internal UInt32 size;
        [FieldOffset(TestBlitting.ptr + 4)]
        internal UInt32 offset;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_types {
        internal UInt32 total_size;
        internal string total_size_check;
        internal UInt32 total_size_size;
        internal UInt32 total_size_offset;
        
        internal ziti_auth_query_mfa f01_ziti_auth_query_mfa;
        internal AlignmentCheck f01_check;
        
        internal ziti_id_cfg f02_ziti_id_cfg;
        internal AlignmentCheck f02_check;
        
        internal ziti_config f03_ziti_config;
        internal AlignmentCheck f03_check;
        
        internal ziti_api_path f04_api_path;
        internal AlignmentCheck f04_check;
        
        internal ziti_api_versions f05_ziti_api_versions;
        internal AlignmentCheck f05_check;
        
        internal ziti_version f06_ziti_version;
        internal AlignmentCheck f06_check;
        
        internal ziti_identity f07_ziti_identity;
        internal AlignmentCheck f07_check;
        
        internal ziti_process f08_ziti_process;
        internal AlignmentCheck f08_check;
        
        internal ziti_posture_query f09_ziti_posture_query;
        internal AlignmentCheck f09_check;
        
        internal ziti_posture_query_set f10_ziti_posture_query_set;
        internal AlignmentCheck f10_check;
        
        internal ziti_session_type f11_ziti_session_type;
        internal AlignmentCheck f11_check;
        
        internal ziti_service f12_ziti_service;
        internal AlignmentCheck f12_check;
        
        internal ziti_address f13_ziti_address_host;
        internal AlignmentCheck f13_check;
        
        internal ziti_address f14_ziti_address_cidr;
        internal AlignmentCheck f14_check;
        
        internal ziti_client_cfg_v1 f15_ziti_client_cfg_v1;
        internal AlignmentCheck f15_check;

        internal ziti_intercept_cfg_v1 f16_ziti_intercept_cfg_v1;
        internal AlignmentCheck f16_check;

        internal ziti_server_cfg_v1 f17_ziti_server_cfg_v1;
        internal AlignmentCheck f17_check;

        internal ziti_listen_options f18_ziti_listen_options;
        internal AlignmentCheck f18_check;

        internal ziti_host_cfg_v1 f19_ziti_host_cfg_v1;
        internal AlignmentCheck f19_check;

        internal ziti_host_cfg_v2 f20_ziti_host_cfg_v2;
        internal AlignmentCheck f20_check;

        internal ziti_mfa_enrollment f21_ziti_mfa_enrollment;
        internal AlignmentCheck f21_check;

        internal ziti_port_range f22_ziti_port_range;
        internal AlignmentCheck f22_check;

        internal ziti_options f23_ziti_options;
        internal AlignmentCheck f23_check;

        internal ziti_context_event f24_ziti_context_event;
        internal AlignmentCheck f24_check;

        internal ziti_router_event f25_ziti_router_event;
        internal AlignmentCheck f25_check;

        internal ziti_service_event f26_ziti_service_event;
        internal AlignmentCheck f26_check;

        internal ziti_mfa_auth_event f27_ziti_mfa_auth_event;
        internal AlignmentCheck f27_check;

        internal ziti_api_event f28_ziti_api_event;
        internal AlignmentCheck f28_check;
    }

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    public struct ziti_context_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        internal ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        internal int ctrl_status;
        [FieldOffset(2 * TestBlitting.ptr)]
        internal string err;

        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 16)]
        //internal byte[] _union;
    }

    internal enum ziti_router_status {
        EdgeRouterAdded,
        EdgeRouterConnected,
        EdgeRouterDisconnected,
        EdgeRouterRemoved,
        EdgeRouterUnavailable
    }

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    internal struct ziti_api_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        internal ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public string new_ctrl_address;
    };

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    internal struct ziti_mfa_auth_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        internal ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        internal IntPtr ziti_auth_query_mfa;
    };

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    internal struct ziti_service_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        internal ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        public IntPtr removed;
        [FieldOffset(2 * TestBlitting.ptr)]
        public IntPtr changed;
        [FieldOffset(3 * TestBlitting.ptr)]
        public IntPtr added;
    }

    [StructLayout(LayoutKind.Explicit, Size = TestBlitting.ZITI_EVENT_UNION_SIZE)]
    struct ziti_router_event {
        [FieldOffset(0 * TestBlitting.ptr)]
        internal ziti_event_type ziti_event_type;
        [FieldOffset(1 * TestBlitting.ptr)]
        internal ziti_router_status status;
        [FieldOffset(2 * TestBlitting.ptr)]
        internal string name;
        [FieldOffset(3 * TestBlitting.ptr)]
        internal string address;
        [FieldOffset(4 * TestBlitting.ptr)]
        internal string version;
    }

    //old..[StructLayout(LayoutKind.Sequential)]
    //old..internal struct ziti_context_event {
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
#if ZITI_X64
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
        internal int low; //, int, none, low, __VA_ARGS__) \
        internal int high; //, int, none, high, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_mfa_enrollment {
        public bool is_verified;
        public IntPtr recovery_codes; // convert IntPtr to string array
        public string provisioning_url;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_host_cfg_v2 {
        internal IntPtr terminators;//, ziti_host_cfg_v1, list, terminators, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_host_cfg_v1 {
        internal string protocol;
        internal bool forward_protocol;
        internal IntPtr allowed_protocols;
        internal string address;
        internal bool forward_address;
        internal IntPtr allowed_addresses;
        internal int port;
        internal bool forward_port;
        internal IntPtr allowed_port_ranges;//, ziti_port_range, array, allowedPortRanges, __VA_ARGS__) \
        internal IntPtr allowed_source_addresses;//, ziti_address, array, allowedSourceAddresses, __VA_ARGS__) \
        internal IntPtr listen_options;//, ziti_listen_options, ptr, listenOptions, __VA_ARGS__)
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_listen_options {
        internal bool bind_with_identity;
        internal UInt64 connect_timeout;
        internal int connect_timeout_seconds;
        internal int cost;
        internal string identity;
        internal int max_connections;
        internal string precedence;

    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_server_cfg_v1 {
        internal string protocol;
        internal string hostname;
        internal int port;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_intercept_cfg_v1 {
        internal IntPtr protocols;
        internal IntPtr addresses;
        internal IntPtr port_ranges;
        internal IntPtr dial_options_map;
        internal string source_ip;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_client_cfg_v1 {
        internal ziti_address hostname;
        internal int port;
    }
    public enum ziti_address_type {
        Host = 0,
        CIDR = 1
    }

    [StructLayout(LayoutKind.Sequential, Size = 260)]
    public struct ziti_address {
        private int address_type;
        [MarshalAs(UnmanagedType.ByValArray, SizeConst = 256)]
        internal byte[] _union;

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
                return (AddressFamily)BitConverter.ToInt32(new ReadOnlySpan<byte>(_union, 0, 4));
            }
        }

        public int Bits {
            get {
                return BitConverter.ToInt32(new ReadOnlySpan<byte>(_union, 4, 4));
            }
        }

        public IPAddress IP {
            get {
                ReadOnlySpan<byte> ipb = new ReadOnlySpan<byte>(_union, 8, 4);
                IPAddress ip = new IPAddress(ipb);
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
#if ZITI_X64
        [FieldOffset(3 * TestBlitting.ptr)]
        public bool encryption;
        [FieldOffset(3 * TestBlitting.ptr + 4)]
        public int perm_flags;
        [FieldOffset(4 * TestBlitting.ptr)]
        public IntPtr config;
        [FieldOffset(5 * TestBlitting.ptr)]
        public IntPtr /** posture_query_set[] **/ posture_query_set;
        [FieldOffset(6 * TestBlitting.ptr)]
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //internal byte[] _union;
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
        //internal byte[] _union;
        public IntPtr /*Dictionary<string, posture_query_set> */ posture_query_map;
        [FieldOffset(8 * TestBlitting.ptr)]
        public string updated_at;
#endif
        //[MarshalAs(UnmanagedType.ByValArray, SizeConst = 8)]
        //internal byte[] _union2;
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
        internal string version;
        internal string revision;
        internal string build_date;
        internal IntPtr api_versions;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_api_versions {
        internal IntPtr api_path_map;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_api_path {
        internal string path;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_config {
        internal string controller_url;
        internal ziti_id_cfg id;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_id_cfg {
        internal string cert;
        internal string key;
        internal string ca;
    }

#if ZITI_X64
    [StructLayout(LayoutKind.Explicit, Size = (TestBlitting.ptr * 5))]
#else
    [StructLayout(LayoutKind.Explicit, Size = (TestBlitting.ptr * 6))]
#endif
    public struct ziti_auth_query_mfa {
        [FieldOffset(0 * TestBlitting.ptr)]
        internal string type_id;
        [FieldOffset(1 * TestBlitting.ptr)]
        internal string provider;
        [FieldOffset(2 * TestBlitting.ptr)]
        internal string http_method;
        [FieldOffset(3 * TestBlitting.ptr)]
        internal string http_url;
#if ZITI_X64
        [FieldOffset(4 * TestBlitting.ptr)]
        internal int min_length;
        [FieldOffset(4 * TestBlitting.ptr + 4)]
        internal int max_length;
        [FieldOffset(5 * TestBlitting.ptr)]
        internal string format;
#else
        [FieldOffset(4 * TestBlitting.ptr)]
        internal int min_length;
        [FieldOffset(5 * TestBlitting.ptr)]
        internal int max_length;
        [FieldOffset(6 * TestBlitting.ptr)]
        internal string format;
#endif
    }




    // ----- older stuff below





    //TODO: REMOVE
    [StructLayout(LayoutKind.Sequential)]
    public struct model_map_entry {
        public IntPtr key;
        internal char key_pad1;
        internal char key_pad2;
        public size_t key_len;
        public UInt32 key_hash;
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
        bool bind_with_identity;//, bool, none, bindUsingEdgeIdentity, __VA_ARGS__) \
        UInt64 connect_timeout;//, duration, none, connectTimeout, __VA_ARGS__)       \
        int connect_timeout_seconds;//, int, none, connectTimeoutSeconds, __VA_ARGS__) \
        int cost;//, int, none, cost, __VA_ARGS__) \
        string identity;//, string, none, identity, __VA_ARGS__) \
        int max_connections;//, int, none, maxConnections, __VA_ARGS__)\
        string precendence;//, string, none, precendence, __VA_ARGS__)
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
