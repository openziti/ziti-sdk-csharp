/*
Copyright 2019 NetFoundry, Inc.

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

namespace OpenZiti.Native {
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_enroll_options {
        public string jwt;
        public string enroll_key;
        public string enroll_cert;
    };

    //typedef void (*log_writer)(int level, const char *loc, const char *msg, size_t msglen);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void log_writer(int level, string loc, string msg, uint msglen);
    //typedef void (*ziti_service_cb)(ziti_context ztx, ziti_service*, int status, void* data);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_service_cb(IntPtr ziti_context, IntPtr ziti_service, int status, GCHandle on_service_context);
    // typedef void (* ziti_conn_cb) (ziti_connection conn, int status);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_conn_cb(IntPtr ziti_connection, int status);
    // typedef void (* ziti_conn_cb) (ziti_connection conn, int status);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_listen_cb(IntPtr ziti_connection, int status);
    // typedef void (* ziti_client_cb) (ziti_connection serv, ziti_connection client, int status);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_client_cb(IntPtr ziti_connection_server, IntPtr ziti_connection_client, int status);
    // typedef void (* ziti_write_cb) (ziti_connection conn, ssize_t status, void* write_ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_write_cb(IntPtr ziti_connection, int status, GCHandle write_context);
    // typedef void (* ziti_enroll_cb) (ziti_config* cfg, int status, char* err_message, void* enroll_ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_enroll_cb(IntPtr ziti_config, int status, string errorMessage, GCHandle enroll_context);
    // typedef ssize_t(*ziti_data_cb)(ziti_connection conn, uint8_t* data, ssize_t length);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate int ziti_data_cb(IntPtr conn, IntPtr data, int length);
    //typedef void (*ziti_pr_mac_cb)(ziti_context ztx, char *id, char **mac_addresses, int num_mac);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pr_mac_cb(IntPtr ziti_context, string id, string[] mac_addresses, int num_mac);
    //typedef void (* ziti_pq_mac_cb) (ziti_context ztx, char* id, ziti_pr_mac_cb response_cb);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pq_mac_cb(IntPtr ziti_context, string id, ziti_pr_mac_cb response_cb);
    //typedef void (*ziti_pr_os_cb)(ziti_context ztx, char *id, char *os_type, char *os_version, char *os_build);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pr_os_cb(IntPtr ziti_context, string id, string os_type, string os_version, string os_build);
    //typedef void (*ziti_pq_os_cb)(ziti_context ztx, char *id, ziti_pr_os_cb response_cb);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pq_os_cb(IntPtr ziti_context, string id, ziti_pr_os_cb response_cb);
    //typedef void (* ziti_pr_process_cb) (ziti_context ztx, char* id, char* path, bool is_running, char* sha_512_hash,
    //                                 char** signers, int num_signers);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pr_process_cb(IntPtr ziti_context, string id, string path, bool is_running, string sha_512, string[] signers, int num_signers);
    //typedef void (* ziti_pq_process_cb) (ziti_context ztx, const char* id, const char* path,
    //                                 ziti_pr_process_cb response_cb);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pq_process_cb(IntPtr ziti_context, string id, string path, ziti_pr_process_cb response_cb);
    //typedef void (*ziti_pr_domain_cb)(ziti_context ztx, char *id, char *domain);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pr_domain_cb(IntPtr ziti_context, string id, string domain);
    //typedef void (*ziti_pq_domain_cb)(ziti_context ztx, char *id, ziti_pr_domain_cb response_cb);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_pq_domain_cb(IntPtr ziti_context, string id, ziti_pr_domain_cb response_cb);
    //typedef void (*ziti_ar_mfa_status_cb)(ziti_context ztx, void* mfa_ctx, int status, void* ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_ar_mfa_status_cb(IntPtr ziti_context, IntPtr mfa_ctx, int status, IntPtr ctx);
    //typedef void (*ziti_ar_mfa_cb)(ziti_context ztx, void* mfa_ctx, char* code, ziti_ar_mfa_status_cb ar_mfa_status_cb, void* ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_ar_mfa_cb(IntPtr ziti_context, IntPtr mfa_ctx, string code, ziti_ar_mfa_status_cb ar_mfa_status_cb, IntPtr ctx);
    //typedef void (*ziti_aq_mfa_cb)(ziti_context ztx, void* mfa_ctx, ziti_auth_query_mfa *aq_mfa, ziti_ar_mfa_cb response_cb);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_aq_mfa_cb(IntPtr ziti_context, IntPtr mfa_ctx, IntPtr aq_mfa, ziti_ar_mfa_cb response_cb);
    //typedef void (*ziti_event_cb)(ziti_context ztx, const ziti_event_t *event);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_event_cb(IntPtr ziti_context, IntPtr ziti_event);
    //typedef void (*ziti_close_cb)(ziti_connection conn);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_close_cb(IntPtr conn);

    public class API {
        public const CallingConvention CALL_CONVENTION = CallingConvention.Cdecl;

        public const string Z4D_DLL_PATH = @"ziti4dotnet";
        //public const string Z4D_DLL_PATH = @"c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\library\Debug\ziti4dotnet.dll";

        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_default_loop", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_default_loop();

        //these functions should be declared in the same order as they appear in ziti.h to make diffing easier!
        //defined in C: extern int ziti_enroll(ziti_enroll_opts *opts, uv_loop_t *loop, ziti_enroll_cb enroll_cb, void *enroll_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_enroll")]
        public static extern int ziti_enroll(IntPtr /*ziti_enroll_options*/ opts, IntPtr loop, ziti_enroll_cb enroll_cb, GCHandle enroll_context);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_init", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_init(IntPtr loop, int level, IntPtr/*log_writer*/ logger);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "newLoop", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr newLoop();

        //defined in C: extern int ziti_init_opts(ziti_options *options, uv_loop_t *loop, void *init_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_init_opts", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_init_opts(IntPtr/*ziti_options* */ options, IntPtr loop);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_uv_run", CallingConvention = CALL_CONVENTION)]
        public static extern int z4d_uv_run(IntPtr loop);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_all_config_types", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_all_config_types();

        //defined in C: extern const ziti_identity *ziti_get_identity(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_identity", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_identity(IntPtr ztx);

        //defined in C: extern const char *ziti_get_controller(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_controller", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_controller(IntPtr ztx);

        //defined in C: extern const ziti_version *ziti_get_controller_version(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_controller_version", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_controller_version(IntPtr ztx);

        //defined in C: extern const ziti_version *ziti_get_version();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_version", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_version();

        //defined in C: ziti_service* ziti_service_array_get(ziti_service_array arr, int idx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_array_get", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_service_array_get(IntPtr ziti_service_array, int idx);

        //defined in C: extern int ziti_dial(ziti_connection conn, const char *service, ziti_conn_cb cb, ziti_data_cb data_cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dial", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_dial(IntPtr ziti_connection, string serviceName, ziti_conn_cb conn_cb, ziti_data_cb data_cb);

        //defined in C: extern int ziti_conn_init(ziti_context ztx, ziti_connection* conn, void* data);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_init", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_conn_init(IntPtr ziti_context, out IntPtr ziti_connection, IntPtr connection_context);

        //defined in C: extern int ziti_write(ziti_connection conn, uint8_t *data, size_t length, ziti_write_cb write_cb, void *write_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_write", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_write(IntPtr conn, byte[] data, int length, ziti_write_cb afterData, IntPtr dataContext);

        //defined in C: extern int ziti_accept(ziti_connection clt, ziti_conn_cb cb, ziti_data_cb data_cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_accept", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_accept(IntPtr conn, ziti_conn_cb cb, ziti_data_cb data_cb);

        //defined in z4d helper C dll for C#
        //            : extern int z4d_close_connection(ziti_connection con);
        // 
        //defined in C: extern int ziti_close(ziti_connection *conn);
        // CANNOT find a way to take the address of the IntPtr
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_ziti_close", CallingConvention = CALL_CONVENTION)]
        public static extern int z4d_ziti_close(IntPtr conn);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_config_to_json", CallingConvention = CALL_CONVENTION)]
        public static extern string ziti_config_to_json(IntPtr ziti_config, byte[] rawjson, int maxlen, out int len);

        //defined in C: extern int ziti_listen(ziti_connection serv_conn, const char *service, ziti_listen_cb lcb, ziti_client_cb cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_listen", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_listen(IntPtr serv_conn, string service, ziti_listen_cb lcb, ziti_client_cb cb);

        //defined in C: extern int ziti_init(const char *config, uv_loop_t *loop, ziti_event_cb evnt_cb, int events, void *app_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_init", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_init(string config, IntPtr loop, ziti_event_cb event_cb, int event_flags, IntPtr init_ctx);

        //extern void ziti_set_app_info(const char* app_id, const char* app_version);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_set_app_info", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_set_app_info(string app_id, string app_version);

        //extern void* ziti_app_ctx(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_app_ctx", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_app_ctx(IntPtr ztx);

        //defined in C: extern void ziti_get_transfer_rates(ziti_context ztx, double *up, double *down);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_transfer_rates", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_get_transfer_rates(IntPtr ztx, ref double up, ref double down);

        //defined in C: extern int ziti_set_timeout(ziti_context ztx, int timeout);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_set_timeout", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_set_timeout(IntPtr ztx, int timeout);

        //defined in C: extern int ziti_shutdown(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_shutdown")]
        public static extern int ziti_shutdown(IntPtr ztx);

        //int ziti_ctx_free(ziti_context* ctxp);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_ctx_free", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_ctx_free(IntPtr ztx);


        //defined in C: extern int ziti_close(ziti_connection conn, ziti_close_cb close_cb);
        // CANNOT find a way to take the address of the IntPtr
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_close", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_close(IntPtr conn, ziti_close_cb close_cb);

        //defined in C: extern int ziti_service_available(ziti_context ztx, const char *service, ziti_service_cb cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_available", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_service_available(IntPtr ztx, string service_name, ziti_service_cb cb, IntPtr context);

        //defined in C: ziti_service_get_raw_config(ziti_service* service, const char* cfg_type);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_get_raw_config", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_service_get_raw_config(IntPtr svc, string config_name);

        //defined in C: int char_pointer_len(char *a);
        [DllImport(Z4D_DLL_PATH, CallingConvention = CALL_CONVENTION)]
        public static extern int char_pointer_len(IntPtr nullTerminatedString);

        //defined in C: char* gimme_string();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "gimme_string", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr gimme_string_intptr();

        //defined in C: int size_of();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "size_of", CallingConvention = CALL_CONVENTION)]
        public static extern int size_of(IntPtr something);


        /*
        //defined in C: extern void ziti_dump(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dump")]
        public static extern int ziti_dump(IntPtr native_context);

        //defined in C: extern void *ziti_conn_data(ziti_connection conn);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_data")]
        public static extern IntPtr ziti_conn_data(IntPtr conn);
        */
    }

#pragma warning disable 0649
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_identity {
        public string id;
        public string name;
        public IntPtr tags;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_options {
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string config;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string controller;

        public IntPtr tls;

        //public IntPtr config_types; 
        public IntPtr /*public char**/ config_types;

        public Int32 refresh_interval; //the duration in seconds between checking for updates from the controller
        public RateType metrics_type; //an enum describing the metrics to collect

        public Int32 router_keepalive;

        //posture query cbs
        public ziti_pq_mac_cb pq_mac_cb;
        public ziti_pq_os_cb pq_os_cb;
        public ziti_pq_process_cb pq_process_cb;
        public ziti_pq_domain_cb pq_domain_cb;

        //mfa cbs
        public ziti_aq_mfa_cb aq_mfa_cb;

        public GCHandle app_ctx;

        public uint events;

        public ziti_event_cb event_cb;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_service {
        public string id;
        public string name;
        public IntPtr permissions;
        public int perm_flags;
        public IntPtr config;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tls_context {

    }

    [StructLayout(LayoutKind.Sequential)]
    struct ziti_context_event {
        public int type;
        public int ctrl_status;
        public string err;
    };
    [StructLayout(LayoutKind.Sequential)]
    struct ziti_router_event {
        public int type;
        public int status;
        public string name;
        public string version;
    };
    [StructLayout(LayoutKind.Sequential)]
    struct ziti_service_event {
        public int type;
        public IntPtr removed;
        public IntPtr changed;
        public IntPtr added;
    };
#pragma warning restore 0649
}