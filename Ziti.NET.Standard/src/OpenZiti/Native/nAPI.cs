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
    internal struct ziti_enroll_options
    {
        internal string jwt;
        internal string enroll_key;
        internal string enroll_cert;
    };

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

    internal class API
    {
        internal const string Z4D_DLL_PATH = @"ziti4dotnet";

        [System.Runtime.InteropServices.DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_default_loop")]
        internal static extern IntPtr z4d_default_loop();

        //defined in C: extern const char *ziti_get_controller(ziti_context ztx);
        //defined in C: extern const ziti_version *ziti_get_version();
        //defined in C: extern int ziti_service_available(ziti_context ztx, const char *service, ziti_service_cb cb, void *ctx);
        //defined in C: extern int ziti_set_timeout(ziti_context ztx, int timeout);
        //defined in C: extern int ziti_shutdown(ziti_context ztx);
        //defined in C: extern void ziti_dump(ziti_context ztx);
        //defined in C: extern void ziti_get_transfer_rates(ziti_context ztx, double *up, double *down);

        //defined in C: extern int ziti_init(string config, uv_loop_t *loop, ziti_init_cb init_cb, void* init_ctx);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern int ziti_init(string config, IntPtr loop, ziti_init_cb init_cb, GCHandle init_ctx);

        //defined in C: extern int ziti_conn_init(ziti_context ztx, ziti_connection* conn, void* data);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_conn_init")]
        internal static extern int ziti_conn_init(IntPtr ziti_context, out IntPtr ziti_connection, GCHandle connection_context);

        //defined in C: extern int ziti_dial(ziti_connection conn, const char *service, ziti_conn_cb cb, ziti_data_cb data_cb);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_dial")]
        internal static extern int ziti_dial(IntPtr ziti_connection, string serviceName, ziti_conn_cb conn_cb, ziti_data_cb data_cb);

        //defined in C: extern int ziti_write(ziti_connection conn, uint8_t *data, size_t length, ziti_write_cb write_cb, void *write_ctx);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_write")]
        internal static extern int ziti_write(IntPtr conn, byte[] data, int length, ziti_write_cb afterData, GCHandle dataContext);

        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_shutdown")]
        internal static extern int ziti_shutdown(IntPtr ziti_context);

        //defined in C: extern int ziti_enroll(ziti_enroll_opts *opts, uv_loop_t *loop, ziti_enroll_cb enroll_cb, void *enroll_ctx);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "ziti_enroll")]
        internal static extern int ziti_enroll(ref ziti_enroll_options opts, IntPtr loop, ziti_enroll_cb enroll_cb, GCHandle enroll_context);

        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "json_from_ziti_config")]
        internal static extern int json_from_ziti_config(IntPtr ziti_config, byte[] rawjson, int maxlen, out int len);

        //defined in C: extern int ziti_listen(ziti_connection serv_conn, const char *service, ziti_listen_cb lcb, ziti_client_cb cb);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern int ziti_listen(IntPtr serv_conn, string service, ziti_listen_cb lcb, ziti_client_cb cb);

        //defined in C: extern void *ziti_conn_data(ziti_connection conn);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern IntPtr ziti_conn_data(IntPtr conn);

        //defined in C: extern int ziti_accept(ziti_connection clt, ziti_conn_cb cb, ziti_data_cb data_cb);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern int ziti_accept(IntPtr conn, ziti_conn_cb cb, ziti_data_cb data_cb);

        //defined in C: extern int ziti_init_opts(ziti_options *options, uv_loop_t *loop, void *init_ctx);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern int ziti_init_opts(IntPtr options, IntPtr loop, GCHandle init_ctx);

        //defined in C: extern const ziti_version *ziti_get_controller_version(ziti_context ztx);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern IntPtr ziti_get_controller_version(IntPtr ztx);

        //defined in C: extern const ziti_identity *ziti_get_identity(ziti_context ztx);
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
        internal static extern IntPtr ziti_get_identity(IntPtr ztx);


        //defined in z4d helper C dll for C#
        //            : extern int z4d_close_connection(ziti_connection con);
        // 
        //defined in C: extern int ziti_close(ziti_connection *conn);
        // CANNOT find a way to take the address of the IntPtr
        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_ziti_close")]
        internal static extern int z4d_ziti_close(IntPtr conn);

        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH, EntryPoint = "z4d_uv_run")]
        internal static extern int z4d_uv_run(IntPtr loop);

        [System.Runtime.InteropServices.DllImport(API.Z4D_DLL_PATH)]
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

    internal struct ziti_options
    {
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        internal string config;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        internal string controller;

        internal IntPtr tls;

        internal IntPtr config_types; //internal string[] /*internal char**/ config_types;

        internal ziti_init_cb init_cb;
        internal ziti_service_cb service_cb;

        internal Int32 refresh_interval; //the duration in seconds between checking for updates from the controller
        internal RateType metrics_type; //an enum describing the metrics to collect

        internal Int32 router_keepalive;

        internal GCHandle ctx;
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