/*
Copyright 2019-2020 NetFoundry, Inc.

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

namespace OpenZiti.Native
{
    [StructLayout(LayoutKind.Sequential)]
    internal struct ziti_enroll_options
    {
        internal string jwt;
        internal string enroll_key;
        internal string enroll_cert;
    };

    //typedef void (*log_writer)(int level, const char *loc, const char *msg, size_t msglen);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)]
    internal delegate void log_writer(int level, string loc, string msg, uint msglen);
    // typedef void (* ziti_init_cb) (ziti_context ztx, int status, void* init_ctx);
    internal delegate int ziti_init_cb(IntPtr ziti_context, int status, GCHandle init_ctx);
    // typedef void (* ziti_service_cb) (ziti_context ztx, ziti_service*, int status, void* data);
    internal delegate void ziti_service_cb(IntPtr ziti_context, IntPtr ziti_service, int status, GCHandle on_service_context);
    // typedef void (* ziti_conn_cb) (ziti_connection conn, int status);
    internal delegate void ziti_conn_cb(IntPtr ziti_connection, int status);
    // typedef void (* ziti_conn_cb) (ziti_connection conn, int status);
    internal delegate void ziti_listen_cb(IntPtr ziti_connection, int status);
    // typedef void (* ziti_client_cb) (ziti_connection serv, ziti_connection client, int status);
    internal delegate void ziti_client_cb(IntPtr ziti_connection_server, IntPtr ziti_connection_client, int status);
    // typedef void (* ziti_write_cb) (ziti_connection conn, ssize_t status, void* write_ctx);
    internal delegate void ziti_write_cb(IntPtr ziti_connection, int status, GCHandle write_context);
    // typedef void (* ziti_enroll_cb) (ziti_config* cfg, int status, char* err_message, void* enroll_ctx);
    internal delegate void ziti_enroll_cb(IntPtr ziti_config, int status, string errorMessage, GCHandle enroll_context);
    // typedef ssize_t(*ziti_data_cb)(ziti_connection conn, uint8_t* data, ssize_t length);
    internal delegate int ziti_data_cb(IntPtr conn, IntPtr data, int length);
    //typedef void (*ziti_pr_mac_cb)(ziti_context ztx, char *id, char **mac_addresses, int num_mac);
    internal delegate void ziti_pr_mac_cb(IntPtr ziti_context, string id, string[] mac_addresses, int num_mac);
    //typedef void (* ziti_pq_mac_cb) (ziti_context ztx, char* id, ziti_pr_mac_cb response_cb);
    internal delegate void ziti_pq_mac_cb(IntPtr ziti_context, string id, ziti_pr_mac_cb response_cb);
    //typedef void (*ziti_pr_os_cb)(ziti_context ztx, char *id, char *os_type, char *os_version, char *os_build);
    internal delegate void ziti_pr_os_cb(IntPtr ziti_context, string id, string os_type, string os_version, string os_build);
    //typedef void (*ziti_pq_os_cb)(ziti_context ztx, char *id, ziti_pr_os_cb response_cb);
    internal delegate void ziti_pq_os_cb(IntPtr ziti_context, string id, ziti_pr_os_cb response_cb);
    //typedef void (* ziti_pr_process_cb) (ziti_context ztx, char* id, char* path, bool is_running, char* sha_512_hash,
    //                                 char** signers, int num_signers);
    internal delegate void ziti_pr_process_cb(IntPtr ziti_context, string id, string path, bool is_running, string sha_512, string[] signers, int num_signers);
    //typedef void (* ziti_pq_process_cb) (ziti_context ztx, const char* id, const char* path,
    //                                 ziti_pr_process_cb response_cb);
    internal delegate void ziti_pq_process_cb(IntPtr ziti_context, string id, string path, ziti_pr_process_cb response_cb);
    //typedef void (*ziti_pr_domain_cb)(ziti_context ztx, char *id, char *domain);
    internal delegate void ziti_pr_domain_cb(IntPtr ziti_context, string id, string domain);
    //typedef void (*ziti_pq_domain_cb)(ziti_context ztx, char *id, ziti_pr_domain_cb response_cb);
    internal delegate void ziti_pq_domain_cb(IntPtr ziti_context, string id, ziti_pr_domain_cb response_cb);
    //typedef void (*ziti_ar_mfa_status_cb)(ziti_context ztx, void* mfa_ctx, int status, void* ctx);
    internal delegate void ziti_ar_mfa_status_cb(IntPtr ziti_context, IntPtr mfa_ctx, int status, IntPtr ctx);
    //typedef void (*ziti_ar_mfa_cb)(ziti_context ztx, void* mfa_ctx, char* code, ziti_ar_mfa_status_cb ar_mfa_status_cb, void* ctx);
    internal delegate void ziti_ar_mfa_cb(IntPtr ziti_context, IntPtr mfa_ctx, string code, ziti_ar_mfa_status_cb ar_mfa_status_cb, IntPtr ctx);
    //typedef void (*ziti_aq_mfa_cb)(ziti_context ztx, void* mfa_ctx, ziti_auth_query_mfa *aq_mfa, ziti_ar_mfa_cb response_cb);
    internal delegate void ziti_aq_mfa_cb(IntPtr ziti_context, IntPtr mfa_ctx, IntPtr aq_mfa, ziti_ar_mfa_cb response_cb);
    //typedef void (*ziti_event_cb)(ziti_context ztx, const ziti_event_t *event);
    internal delegate void ziti_event_cb(IntPtr ziti_context, IntPtr ziti_event);

    internal class API {
        internal const CallingConvention CALL_CONVENTION = CallingConvention.Cdecl;

        internal const string root = @"c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\dlls\";
        internal const string Z4D_DLL_PATH = @"C:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\library\Debug\ziti4dotnet.dll";
//        internal const string ZITI_DLL_PATH = root + @"ziti.dll";
        
        //these functions should be declared in the same order as they appear in ziti.h to make diffing easier!
        //defined in C: extern int ziti_enroll(ziti_enroll_opts *opts, uv_loop_t *loop, ziti_enroll_cb enroll_cb, void *enroll_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_enroll")]
        internal static extern int ziti_enroll(IntPtr /*ziti_enroll_options*/ opts, IntPtr loop, ziti_enroll_cb enroll_cb, GCHandle enroll_context);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_init", CallingConvention = CALL_CONVENTION)]
        internal static extern void ziti_log_init(IntPtr loop, int level, IntPtr/*log_writer*/ logger);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "passAndPrint", CallingConvention = CALL_CONVENTION)]
        internal static extern void passAndPrint(IntPtr anything);

        [DllImport(Z4D_DLL_PATH, EntryPoint = "newLoop", CallingConvention = CALL_CONVENTION)]
        internal static extern IntPtr newLoop();

        [DllImport(Z4D_DLL_PATH, EntryPoint = "DoSillyLoop", CallingConvention = CALL_CONVENTION)]
        internal static extern void DoSillyLoop(IntPtr loop);

        //internal const string Z4D_DLL_PATH = @"ziti4dotnet";
        /*
         copy /y C:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\_deps\ziti-sdk-c-build\library\Debug\ziti.dll C:\git\github\openziti\ziti-sdk-csharp\Ziti.NET.Standard.Tests\bin\Debug\net5.0\ziti.dll
         * */
        //internal const string Z4D_DLL_PATH = @"c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\library\Release\ziti4dotnet.dll";
        //internal const string ZITI_DLL_PATH = @"c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\library\Release\ziti4dotnet.dll";

        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_default_loop", CallingConvention = CALL_CONVENTION)]
        internal static extern IntPtr z4d_default_loop();

        //defined in C: extern int ziti_service_available(ziti_context ztx, const char *service, ziti_service_cb cb, void *ctx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_available")]
        internal static extern int ziti_service_available(IntPtr native_context, string service_name, ziti_service_cb cb, GCHandle context);

        //defined in C: extern int ziti_set_timeout(ziti_context ztx, int timeout);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_set_timeout")]
        internal static extern int ziti_set_timeout(IntPtr native_context, int timeout);

        //defined in C: extern void ziti_dump(ziti_context ztx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dump")]
        internal static extern int ziti_dump(IntPtr native_context);

        //defined in C: extern void ziti_get_transfer_rates(ziti_context ztx, double *up, double *down);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_transfer_rates")]
        internal static extern void ziti_get_transfer_rates(IntPtr native_context, ref double up, ref double down);

        //defined in C: extern int ziti_init(string config, uv_loop_t *loop, ziti_init_cb init_cb, void* init_ctx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_init")]
        internal static extern int ziti_init(string config, IntPtr loop, ziti_init_cb init_cb, GCHandle init_ctx);

        //defined in C: extern int ziti_conn_init(ziti_context ztx, ziti_connection* conn, void* data);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_init")]
        internal static extern int ziti_conn_init(IntPtr ziti_context, out IntPtr ziti_connection, GCHandle connection_context);

        //defined in C: extern int ziti_dial(ziti_connection conn, const char *service, ziti_conn_cb cb, ziti_data_cb data_cb);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dial")]
        internal static extern int ziti_dial(IntPtr ziti_connection, string serviceName, ziti_conn_cb conn_cb, ziti_data_cb data_cb);

        //defined in C: extern int ziti_write(ziti_connection conn, uint8_t *data, size_t length, ziti_write_cb write_cb, void *write_ctx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_write")]
        internal static extern int ziti_write(IntPtr conn, byte[] data, int length, ziti_write_cb afterData, GCHandle dataContext);

        //defined in C: extern int ziti_shutdown(ziti_context ztx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_shutdown")]
        internal static extern int ziti_shutdown(IntPtr ziti_context);

        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "json_from_ziti_config")]
        internal static extern int json_from_ziti_config(IntPtr ziti_config, byte[] rawjson, int maxlen, out int len);

        //defined in C: extern int ziti_listen(ziti_connection serv_conn, const char *service, ziti_listen_cb lcb, ziti_client_cb cb);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_listen")]
        internal static extern int ziti_listen(IntPtr serv_conn, string service, ziti_listen_cb lcb, ziti_client_cb cb);

        //defined in C: extern void *ziti_conn_data(ziti_connection conn);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_data")]
        internal static extern IntPtr ziti_conn_data(IntPtr conn);

        //defined in C: extern int ziti_accept(ziti_connection clt, ziti_conn_cb cb, ziti_data_cb data_cb);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_accept")]
        internal static extern int ziti_accept(IntPtr conn, ziti_conn_cb cb, ziti_data_cb data_cb);

        //defined in C: extern int ziti_init_opts(ziti_options *options, uv_loop_t *loop, void *init_ctx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_init_with_opts")]
        internal static extern int ziti_init_with_opts(IntPtr options, IntPtr loop);

        //defined in C: extern const char *ziti_get_controller(ziti_context ztx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_controller_version")]
        internal static extern string ziti_get_controller_version(IntPtr ztx);

        //defined in C: extern const ziti_identity *ziti_get_identity(ziti_context ztx);
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_identity")]
        internal static extern IntPtr ziti_get_identity(IntPtr ztx);

        //defined in C: extern const ziti_version *ziti_get_version();
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_version")]
        internal static extern IntPtr ziti_get_version();

        //defined in z4d helper C dll for C#
        //            : extern int z4d_close_connection(ziti_connection con);
        // 
        //defined in C: extern int ziti_close(ziti_connection *conn);
        // CANNOT find a way to take the address of the IntPtr
        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_ziti_close")]
        internal static extern int z4d_ziti_close(IntPtr conn);

        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_uv_run")]
        internal static extern int z4d_uv_run(IntPtr loop);

        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "json_from_ziti_config")]
        internal static extern IntPtr z4d_all_config_types();
    }
    internal struct ziti_version
    {
#pragma warning disable 0649
        internal string version;
        internal string revision;
        internal string build_date;
#pragma warning restore 0649
    }
    internal struct ziti_identity
    {
#pragma warning disable 0649
        internal string id;
        internal string name;
        internal IntPtr tags;
#pragma warning restore 0649
    }

    internal struct ziti_options {
#pragma warning disable 0649
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        internal string config;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        internal string controller;

        internal IntPtr tls;

        internal IntPtr config_types; //internal string[] /*internal char**/ config_types;

        internal Int32 refresh_interval; //the duration in seconds between checking for updates from the controller
        internal RateType metrics_type; //an enum describing the metrics to collect

        internal Int32 router_keepalive;

        //posture query cbs
        internal ziti_pq_mac_cb pq_mac_cb;
        internal ziti_pq_os_cb pq_os_cb;
        internal ziti_pq_process_cb pq_process_cb;
        internal ziti_pq_domain_cb pq_domain_cb;

        //mfa cbs
        internal ziti_aq_mfa_cb aq_mfa_cb;

        internal GCHandle app_ctx;

        internal uint events;

        internal ziti_event_cb event_cb;
#pragma warning restore 0649
    };

    internal struct ziti_service
    {
#pragma warning disable 0649
        internal string id;
        internal string name;
        internal IntPtr permissions;
        internal int perm_flags;
        internal IntPtr config;
#pragma warning restore 0649
    }

    internal struct tls_context
    {

    }
}