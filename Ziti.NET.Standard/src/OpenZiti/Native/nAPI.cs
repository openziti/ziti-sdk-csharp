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
using System.Collections.Generic;

namespace OpenZiti.Native {
    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_enroll_options {
        public string jwt;
        public string enroll_key;
        public string enroll_cert;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_mfa_enrollment
	{
        [MarshalAs(UnmanagedType.Bool)]
        public bool is_verified;
        public IntPtr recovery_codes; // convert IntPtr to string array
        [MarshalAs(UnmanagedType.LPStr)]
        public string provisioning_url;
    }

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
    // typedef void (*ziti_mfa_cb)(ziti_context ztx, int status, void *ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void on_mfa_cb(IntPtr ziti_context, int status, IntPtr ctx);
    // typedef void (*ziti_mfa_enroll_cb)(ziti_context ztx, int status, ziti_mfa_enrollment *mfa_enrollment, void *ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void on_enable_mfa(IntPtr ziti_context, int status, IntPtr /* ziti_mfa_enrollment*/ enrollment, IntPtr ctx);
    // typedef void (*ziti_mfa_recovery_codes_cb)(ziti_context ztx, int status, char **recovery_codes, void *ctx);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void on_mfa_recovery_codes(IntPtr ziti_context, int status, IntPtr /* string[] */ recovery_codes, IntPtr ctx);
    //typedef void (*ziti_event_cb)(ziti_context ztx, const ziti_event_t *event);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_event_cb(IntPtr ziti_context, IntPtr ziti_event);
    //typedef void (*ziti_close_cb)(ziti_connection conn);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void ziti_close_cb(IntPtr conn);
    //typedef void (*uv_close_cb)(uv_handle_t* handle);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void OnUVTimer(IntPtr handle);
    //typedef void (* uv_close_cb) (uv_handle_t* handle);
    [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void uv_close_cb(IntPtr handle);


    public class API {
        public const CallingConvention CALL_CONVENTION = CallingConvention.Cdecl;

        public const string Z4D_DLL_PATH = @"ziti4dotnet";
        //public const string Z4D_DLL_PATH = @"c:\git\github\openziti\ziti-sdk-csharp\ZitiNativeApiForDotnetCore\build\x86\library\Debug\ziti4dotnet.dll";

        #region //ziti.h - functions exported from the ziti-sdk-c project from ziti.h

        //these functions should be declared in the same order as they appear in ziti.h to make diffing easier!
        //defined in C: extern int ziti_enroll(ziti_enroll_opts *opts, uv_loop_t *loop, ziti_enroll_cb enroll_cb, void *enroll_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_enroll")]
        public static extern int ziti_enroll(IntPtr /*ziti_enroll_options*/ opts, IntPtr loop, ziti_enroll_cb enroll_cb, GCHandle enroll_context);
        //defined in C: extern void ziti_set_app_info(const char *app_id, const char *app_version);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_set_app_info", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_set_app_info(string app_id, string app_version);
        //defined in C: extern int ziti_init(const char *config, uv_loop_t *loop, ziti_event_cb evnt_cb, int events, void *app_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_init", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_init(string config, IntPtr loop, ziti_event_cb event_cb, int event_flags, IntPtr init_ctx);
        //defined in C: extern int ziti_init_opts(ziti_options *options, uv_loop_t *loop);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_init_opts", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_init_opts(IntPtr options, IntPtr loop);
        //defined in C: extern bool ziti_is_enabled(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_is_enabled", CallingConvention = CALL_CONVENTION)]
        public static extern bool ziti_is_enabled(IntPtr ziti_context);
        // defined in C: extern void ziti_set_enabled(ziti_context ztx, bool enabled);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_set_enabled", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_set_enabled(IntPtr ziti_context, bool enabled);
        // defined in C: extern void *ziti_app_ctx(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_app_ctx", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_app_ctx(IntPtr ztx);
        //defined in C: extern const ziti_version *ziti_get_version();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_version", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_version();
        //defined in C: extern const ziti_version *ziti_get_controller_version(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_controller_version", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_controller_version(IntPtr ztx);
        //defined in C: extern const char *ziti_get_controller(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_controller", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_controller(IntPtr ztx);
        //defined in C: extern const ziti_identity *ziti_get_identity(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_identity", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_identity(IntPtr ztx);
        //defined in C: extern void ziti_get_transfer_rates(ziti_context ztx, double *up, double *down);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_transfer_rates", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_get_transfer_rates(IntPtr ztx, ref double up, ref double down);
        //defined in C: extern int ziti_set_timeout(ziti_context ztx, int timeout);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_set_timeout", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_set_timeout(IntPtr ztx, int timeout);
        //defined in C: extern int ziti_shutdown(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_shutdown")]
        public static extern int ziti_shutdown(IntPtr ztx);
        //defined in C: extern void ziti_dump(ziti_context ztx, int (*printer)(void *ctx, const char *fmt, ...), void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dump")]
        public static extern int ziti_dump(IntPtr ztx, IntPtr varargFunc, IntPtr ctx);
        //defined in C: const char *ziti_get_appdata_raw(ziti_context ztx, const char *key);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_appdata_raw")]
        public static extern IntPtr ziti_get_appdata_raw(IntPtr ztx, string key);
        //defined in C: int ziti_get_appdata(ziti_context ztx, const char* key, void* data, int (* parse_func) (void*, const char*, size_t));
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_appdata")]
        public static extern int ziti_get_appdata(IntPtr ztx, string key, IntPtr data, IntPtr parse_func);
        //defined in C: extern int ziti_conn_init(ziti_context ztx, ziti_connection *conn, void *data);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_init", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_conn_init(IntPtr ziti_context, out IntPtr ziti_connection, IntPtr connection_context);
        //defined in C: extern ziti_context ziti_conn_context(ziti_connection conn);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_context", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_context(IntPtr ziti_conn);
        //defined in C: extern void *ziti_conn_data(ziti_connection conn);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_data")]
        public static extern IntPtr ziti_conn_data(IntPtr ziti_conn);
        //defined in C: extern void ziti_conn_set_data(ziti_connection conn, void *data);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_set_data")]
        public static extern IntPtr ziti_conn_set_data(IntPtr ziti_conn, IntPtr data);
        //defined in C: extern void ziti_conn_set_data_cb(ziti_connection conn, ziti_data_cb cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_set_data_cb")]
        public static extern IntPtr ziti_conn_set_data_cb(IntPtr ziti_conn, IntPtr /*ziti_data_cb*/ cb);
        //defined in C: extern const char *ziti_conn_source_identity(ziti_connection conn);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_source_identity")]
        public static extern IntPtr ziti_conn_source_identity(IntPtr ziti_conn);
        //defined in C: extern int ziti_service_available(ziti_context ztx, const char *service, ziti_service_cb cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_available", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_service_available(IntPtr ztx, string service_name, ziti_service_cb cb, IntPtr context);
        //defined in C: extern int ziti_dial(ziti_connection conn, const char *service, ziti_conn_cb cb, ziti_data_cb data_cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dial", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_dial(IntPtr ziti_connection, string serviceName, ziti_conn_cb conn_cb, ziti_data_cb data_cb);
        //defined in C: extern int ziti_dial_with_options(ziti_connection conn, const char *service, ziti_dial_opts *dial_opts, ziti_conn_cb cb, ziti_data_cb data_cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_dial_with_options", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_dial_with_options(IntPtr ziti_connection, string serviceName, ziti_dial_opts opts, ziti_conn_cb conn_cb, ziti_data_cb data_cb);
        //defined in C: extern int ziti_listen(ziti_connection serv_conn, const char *service, ziti_listen_cb lcb, ziti_client_cb cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_listen", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_listen(IntPtr serv_conn, string service, ziti_listen_cb lcb, ziti_client_cb cb);
        //defined in C: extern int ziti_listen_with_options(ziti_connection serv_conn, const char *service, ziti_listen_opts *listen_opts, ziti_listen_cb lcb, ziti_client_cb cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_listen_with_options", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_listen_with_options(IntPtr serv_conn, string service, ziti_listen_opts opts, ziti_listen_cb lcb, ziti_client_cb cb);
        //defined in C: extern int ziti_accept(ziti_connection clt, ziti_conn_cb cb, ziti_data_cb data_cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_accept", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_accept(IntPtr conn, ziti_conn_cb cb, ziti_data_cb data_cb);
        //defined in C: extern int ziti_close(ziti_connection conn, ziti_close_cb close_cb);
        // CANNOT find a way to take the address of the IntPtr
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_close", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_close(IntPtr conn, ziti_close_cb close_cb);
        //defined in C: extern int ziti_close_write(ziti_connection conn);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_close_write", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_close_write(IntPtr conn);
        //defined in C: extern int ziti_write(ziti_connection conn, uint8_t *data, size_t length, ziti_write_cb write_cb, void *write_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_write", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_write(IntPtr conn, byte[] data, int length, ziti_write_cb afterData, IntPtr dataContext);
        //defined in C: extern int ziti_conn_bridge(ziti_connection conn, uv_stream_t *stream, uv_close_cb on_close);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_bridge", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_conn_bridge(IntPtr conn, IntPtr uv_stream, uv_close_cb on_close);
        //defined in C: extern int ziti_conn_bridge_fds(ziti_connection conn, uv_os_fd_t input, uv_os_fd_t output, void (*close_cb)(void *ctx), void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_conn_bridge_fds", CallingConvention = CALL_CONVENTION)]
        public static extern int ziti_conn_bridge_fds(IntPtr conn, IntPtr input, IntPtr output, uv_close_cb on_close, IntPtr ctx);
        //defined in C: extern void ziti_mfa_enroll(ziti_context ztx, ziti_mfa_enroll_cb enroll_cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_mfa_enroll", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_mfa_enroll(IntPtr ziti_context, on_enable_mfa enroll_cb, IntPtr ctx);
        //defined in C: extern void ziti_mfa_remove(ziti_context ztx, char* code, ziti_mfa_cb remove_cb, void* ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_mfa_remove", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_mfa_remove(IntPtr ziti_context, string code, on_mfa_cb remove_cb, IntPtr ctx);
        //defined in C: extern void ziti_mfa_verify(ziti_context ztx, char *code, ziti_mfa_cb verify_cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_mfa_verify", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_mfa_verify(IntPtr ziti_context, string code, on_mfa_cb verify_cb, IntPtr ctx);
        //defined in C: extern void ziti_mfa_get_recovery_codes(ziti_context ztx, char *code, ziti_mfa_recovery_codes_cb get_cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_mfa_get_recovery_codes", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_mfa_get_recovery_codes(IntPtr ziti_context, string code, on_mfa_recovery_codes get_recovery_codes_cb, IntPtr ctx);
        //defined in C: extern void ziti_mfa_new_recovery_codes(ziti_context ztx, char *code, ziti_mfa_recovery_codes_cb new_cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_mfa_new_recovery_codes", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_mfa_new_recovery_codes(IntPtr ziti_context, string code, on_mfa_recovery_codes new_recovery_codes_cb, IntPtr ctx);
        //defined in C: extern void ziti_mfa_auth(ziti_context ztx, const char *code, ziti_mfa_cb auth_cb, void *ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_mfa_auth", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_mfa_auth(IntPtr ziti_context, string code, on_mfa_cb status_cb, IntPtr status_ctx);
        //defined in C: extern void ziti_endpoint_state_change(ziti_context ztx, bool woken, bool unlocked);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_endpoint_state_change", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_endpoint_state_change(IntPtr ziti_context, bool woken, bool unlocked);
        
        #endregion

        #region //ziti_log.h - functions exported from the ziti-sdk-c project from ziti_log.h

        //defined in C: extern void ziti_log_init(uv_loop_t *loop, int level, log_writer logger);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_init", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_init(IntPtr loop, int level, IntPtr/*log_writer*/ logger);

        //defined in C: extern void ziti_log_set_logger(log_writer logger);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_set_logger", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_set_logger(IntPtr/*log_writer*/ logger);

        //defined in C: extern void ziti_log_set_level(int level);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_set_level", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_set_level(int level);

        //defined in C: extern void ziti_log_set_level_by_label(const char* log_level);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_set_level_by_label", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_set_level_by_label(string log_level);

        //defined in C: extern const char* ziti_log_level_label();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_level_label", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_log_level_label();

        #endregion
        
        #region //functions from ziti4dotnet.h

        //defined in C: extern int z4d_ziti_close(ziti_connection con);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_ziti_close", CallingConvention = CALL_CONVENTION)]
        public static extern int z4d_ziti_close(IntPtr conn);

        //defined in C: extern int z4d_uv_run(void* loop);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_uv_run", CallingConvention = CALL_CONVENTION)]
        public static extern int z4d_uv_run(IntPtr loop);
        //defined in C: void* z4d_new_loop();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_new_loop", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_new_loop();
        //defined in C: extern const char** z4d_all_config_types();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_all_config_types", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_all_config_types();
        //defined in C: uv_loop_t* z4d_default_loop();
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_default_loop", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_default_loop();
        //defined in C: void* z4d_registerUVTimer(uv_loop_t* loop, uv_timer_cb timer_cb, uint64_t iterations, uint64_t delay);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_registerUVTimer", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_registerUVTimer(IntPtr loop, OnUVTimer timer_cb, Int64 delay, Int64 iterations);
        //defined in C: void* z4d_stop_uv_timer(uv_timer_t* t);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_stop_uv_timer", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_stop_uv_timer(IntPtr timer);
        //defined in C: int z4d_event_type_from_pointer(const ziti_event_t *event);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_event_type_from_pointer", CallingConvention = CALL_CONVENTION)]
        public static extern int z4d_event_type_from_pointer(IntPtr /*ziti_event_t* */ ziti_event_t);
        //defined in C: ziti_service* z4d_service_array_get(ziti_service_array arr, int idx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_service_array_get", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_service_array_get(IntPtr ziti_service_array, int idx);
        //defined in C: char** z4d_make_char_array(int size);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_make_char_array", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_make_char_array(int size);
        //defined in C: void z4d_set_char_at(char **a, char *s, int n);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_set_char_at", CallingConvention = CALL_CONVENTION)]
        public static extern void z4d_set_char_at(IntPtr a, string s, int n);
        //defined in C: void z4d_free_char_array(char **a, int size);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_free_char_array", CallingConvention = CALL_CONVENTION)]
        public static extern void z4d_free_char_array(string a, int size);
        //defined in C: void z4d_ziti_dump_log(ziti_context ztx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_ziti_dump_log", CallingConvention = CALL_CONVENTION)]
        public static extern void z4d_ziti_dump_log(IntPtr ztx);
        //defined in C: void z4d_ziti_dump_file(ziti_context ztx, const char* outputFile);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_ziti_dump_file", CallingConvention = CALL_CONVENTION)]
        public static extern void z4d_ziti_dump_file(IntPtr ztx, string outputFile);

        #endregion

        #region //functions from ziti_model.h

        //defined in C: ziti_service_get_raw_config(ziti_service* service, const char* cfg_type);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_get_raw_config", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_service_get_raw_config(IntPtr svc, string config_name);

        //defined in C: extern void ziti_get_transfer_rates(ziti_context ztx, double *up, double *down)
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_transfer_rates", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_get_transfer_rates(IntPtr ziti_context, IntPtr up, IntPtr down);

        #endregion

        #region //functions from ziti/utils.h

        //defined in C: ziti_get_build_version(int verbose);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_build_version", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_build_version(int verbose);

        #endregion

        internal static IntPtr ToPtr(string[] array)
        {
	        if (array == null || array.Length == 0)
	        {
		        return IntPtr.Zero;
	        }
	        IntPtr arr = API.z4d_make_char_array(array.Length);
	        int idx = 0;
	        foreach (string s in array)
	        {
		        API.z4d_set_char_at(arr, s, idx++);
	        }

	        return arr;
        }
    }

    class MarshalUtils<T>
    {
        internal static List<T> convertPointerToList(IntPtr arrayPointer)
        {
            IntPtr currentArrLoc;
            List<T> result = new List<T>();
            int sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = Marshal.ReadIntPtr(arrayPointer)) != IntPtr.Zero)
            {
                T objectT;
                if (typeof(T) == typeof(String))
                {
                    objectT = (T)(object)Marshal.PtrToStringUTF8(currentArrLoc);
                }
                else if (typeof(T).IsValueType && !typeof(T).IsPrimitive)
                {
                    objectT = Marshal.PtrToStructure<T>(currentArrLoc);
                }
                else
                {
                    // marshal operations for other types can be added here
                    throw new ZitiException("Marshalling is not yet supported for " + typeof(T));
                }
                result.Add(objectT);
                arrayPointer = IntPtr.Add(arrayPointer, sizeOfPointer);
            }
            return result;
        }

        internal static List<model_map_entry> convertPointerMapToList(IntPtr arrayPointer)
        {
            IntPtr currentArrLoc;
            List<model_map_entry> result = new List<model_map_entry>();
            int sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = arrayPointer) != IntPtr.Zero)
            {
                model_map_entry objectT = Marshal.PtrToStructure<model_map_entry>(currentArrLoc);
                result.Add(objectT);
                arrayPointer = objectT._next;
            }
            return result;
        }
    }


#pragma warning disable 0649
    [StructLayout(LayoutKind.Sequential)]
	public struct ziti_identity {
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string id;
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string name;
		public IntPtr tags;
	}

	[StructLayout(LayoutKind.Sequential)]
    public struct ziti_options {
        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string config;

        [MarshalAs(UnmanagedType.LPUTF8Str)]
        public string controller;

        public IntPtr tls;

        public bool disabled;

        //public IntPtr config_types;
        public IntPtr /*public char**/ config_types;

        public uint api_page_size;

#if ZITI_X64
        public long refresh_interval; //the duration in seconds between checking for updates from the controller
#else
        public Int32 refresh_interval; //the duration in seconds between checking for updates from the controller
#endif
        public RateType metrics_type; //an enum describing the metrics to collect

        public Int32 router_keepalive;

        //posture query cbs
        public ziti_pq_mac_cb pq_mac_cb;
        public ziti_pq_os_cb pq_os_cb;
        public ziti_pq_process_cb pq_process_cb;
        public ziti_pq_domain_cb pq_domain_cb;

        public GCHandle app_ctx;

        public uint events;

        public ziti_event_cb event_cb;
    };

    [StructLayout(LayoutKind.Sequential)]
    public struct ziti_service {
	    [MarshalAs(UnmanagedType.LPUTF8Str)] public string id;
	    [MarshalAs(UnmanagedType.LPUTF8Str)] public string name;
	    public IntPtr permissions;
	    public bool encryption;
	    public int perm_flags;
	    [MarshalAs(UnmanagedType.LPUTF8Str)] public string config;
	    public IntPtr /** posture_query_set[] **/ posture_query_set;
	    public IntPtr /** Dictionary<string, posture_query_set> **/ posture_query_map;
	    [MarshalAs(UnmanagedType.LPUTF8Str)] public string updated_at;
    }

    [StructLayout(LayoutKind.Sequential)]
    public struct tls_context {

    }

    [StructLayout(LayoutKind.Explicit)]
    struct ziti_context_event {
	    [FieldOffset(0)]
        public int type;
#if ZITI_X64
        [FieldOffset(8)]
#else
	    [FieldOffset(4)]
#endif
        public int ctrl_status;
#if ZITI_X64
        [FieldOffset(16)]
#else
	    [FieldOffset(8)]
#endif
        public IntPtr err;
    };
    [StructLayout(LayoutKind.Explicit)]
    struct ziti_router_event {
	    [FieldOffset(0)]
        public int type;
#if ZITI_X64
        [FieldOffset(8)]
#else
	    [FieldOffset(4)]
#endif
        public int status;
#if ZITI_X64
	    [FieldOffset(16)]
#else
	    [FieldOffset(8)]
#endif
	    public IntPtr name;
#if ZITI_X64
        [FieldOffset(24)]
#else
	    [FieldOffset(12)]
#endif
        public IntPtr version;
    };
    [StructLayout(LayoutKind.Explicit)]
    struct ziti_service_event {
	    [FieldOffset(0)]
        public int type;
#if ZITI_X64
        [FieldOffset(8)]
#else
	    [FieldOffset(4)]
#endif
        public IntPtr removed;
#if ZITI_X64
        [FieldOffset(16)]
#else
	    [FieldOffset(8)]
#endif
        public IntPtr changed;
#if ZITI_X64
        [FieldOffset(24)]
#else
	    [FieldOffset(12)]
#endif
        public IntPtr added;
    };

    [StructLayout(LayoutKind.Explicit)]
    struct ziti_mfa_event
    {
        [FieldOffset(0)]
        public int type;
    };


    [StructLayout(LayoutKind.Explicit)]
    struct ziti_api_event
    {
        [FieldOffset(0)]
        public int type;
#if ZITI_X64
        [FieldOffset(8)]
#else
        [FieldOffset(4)]
#endif
        public IntPtr new_ctrl_address;
    };


	public struct size_t
	{
#if ZITI_X64
		public long val;
#else
	    public int val;
#endif
	}

	public struct posture_query_set {
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string policy_id;
		public bool is_passing;
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string policy_type;
		public IntPtr /** posture_query[] **/ posture_queries;
	}

	public struct posture_query {
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string id;
		public bool is_passing;
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string query_type;
		public IntPtr /** ziti_process **/ process; 
		public int timeout;
	}

	public struct ziti_process {
		[MarshalAs(UnmanagedType.LPUTF8Str)] public string path;
	}

	public struct model_map_impl
	{
		public IntPtr /** model_map_entry[] **/ entries;
		public IntPtr table;
		public int buckets;
		public size_t size;
	}

	public struct model_map_entry
	{
		public IntPtr key;
		public size_t key_len;
		public uint key_hash;
		public IntPtr value;
		public IntPtr _next;
		public IntPtr _tnext;
		public IntPtr _map;
	}

	public struct ziti_dial_opts
	{
#pragma warning disable 0649
#pragma warning disable 0169
        int connect_timeout_seconds;
		[MarshalAs(UnmanagedType.LPUTF8Str)] string identity;
		IntPtr app_data;
		size_t app_data_sz;
#pragma warning restore 0169
#pragma warning restore 0649
    }

    public struct ziti_listen_opts
    {
#pragma warning disable 0649
#pragma warning disable 0169
        UInt16 terminator_cost;
		byte terminator_precedence;
		int connect_timeout_seconds;
		//int max_connections;  // todo implement
		[MarshalAs(UnmanagedType.LPUTF8Str)] string identity;
		bool bind_using_edge_identity;
#pragma warning restore 0169
#pragma warning restore 0649
    }
}
