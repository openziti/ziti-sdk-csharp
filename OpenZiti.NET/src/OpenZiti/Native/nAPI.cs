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
using System.Net.Sockets;
using System.Runtime.CompilerServices;
using System.Runtime.InteropServices;

[assembly: InternalsVisibleTo("UnitTests")]
namespace OpenZiti.Native {
    public class API {
        public const CallingConvention CALL_CONVENTION = CallingConvention.Cdecl;

        public const string Z4D_DLL_PATH = @"ziti4dotnet";

        public static string GetZitiPath() {
            return Z4D_DLL_PATH;
        }

        #region //ziti.h - functions exported from the ziti-sdk-c project from ziti.h

        //these functions should be declared in the same order as they appear in ziti.h to make diffing easier!
        //defined in C: extern int ziti_enroll(ziti_enroll_opts *opts, uv_loop_t *loop, ziti_enroll_cb enroll_cb, void *enroll_ctx);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_enroll")]
        public static extern int ziti_enroll(IntPtr /*ziti_enroll_options*/ opts, IntPtr loop, ziti_enroll_cb enroll_cb, GCHandle enroll_context);
        //definded in C: extern const char* ziti_errorstr(int err);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_errorstr", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_errorstr(int errNo);
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

        //typedef void (*log_writer)(int level, const char *loc, const char *msg, size_t msglen);
        [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void log_writer(int level, string loc, string msg, uint msglen);

        //defined in C: extern void ziti_log_init(uv_loop_t *loop, int level, log_writer logger);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_init", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_init(IntPtr loop, int level, IntPtr/*log_writer*/ logger);

        //defined in C: extern void ziti_log_set_logger(log_writer logger);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_set_logger", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_set_logger(IntPtr/*log_writer*/ logger);

        //defined in C: extern void ziti_log_set_level(int level, const char *marker);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_log_set_level", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_log_set_level(int level, string marker);

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
        public static extern IntPtr z4d_registerUVTimer(IntPtr loop, OnUVTimer timer_cb, long delay, long iterations);
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





        //typedef void (*z4d_cb)(void* context);
        [UnmanagedFunctionPointer(API.CALL_CONVENTION)] public delegate void z4d_cb(GCHandle context);

        //defined in C: void z4d_callback_on_loop(uv_loop_t* loop, void* context, z4d_cb cb);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "z4d_callback_on_loop", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr z4d_callback_on_loop(IntPtr loop, GCHandle context, z4d_cb cb);

        #endregion

        #region //functions from ziti_model.h

        //defined in C: ziti_service_get_raw_config(ziti_service* service, const char* cfg_type);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_service_get_raw_config", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_service_get_raw_config(IntPtr svc, string config_name);

        //defined in C: extern void ziti_get_transfer_rates(ziti_context ztx, double *up, double *down)
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_transfer_rates", CallingConvention = CALL_CONVENTION)]
        public static extern void ziti_get_transfer_rates(IntPtr ziti_context, IntPtr up, IntPtr down);




        //defined in C:  void* model_map_get_key(const model_map* map, const void* key, size_t key_len);
        //public static extern IntPtr model_map_get_key(IntPtr map, string key)

        //defined in C:  void* model_map_get(const model_map* map, const char* key);
        //[DllImport(Z4D_DLL_PATH, EntryPoint = "model_map_get", CallingConvention = CALL_CONVENTION)]
        //public static extern IntPtr model_map_get(IntPtr map, string key);




        #endregion

        #region //functions from ziti/utils.h

        //defined in C: ziti_get_build_version(int verbose);
        [DllImport(Z4D_DLL_PATH, EntryPoint = "ziti_get_build_version", CallingConvention = CALL_CONVENTION)]
        public static extern IntPtr ziti_get_build_version(int verbose);

        #endregion

        #region //functions from zitilib.h


        // void Ziti_lib_init(void);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_lib_init", CallingConvention = API.CALL_CONVENTION)]
        public static extern void Ziti_lib_init();

        // int Ziti_last_error(void)
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_last_error", CallingConvention = API.CALL_CONVENTION)]
        public static extern int Ziti_last_error();

        //int Ziti_enroll_identity(const char *jwt, const char *key, const char *cert, char **id_json, unsigned long *id_json_len);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_enroll_identity", CallingConvention = API.CALL_CONVENTION)]
        public static extern int Ziti_enroll_identity(byte[] jwt, string key, string cert, out IntPtr id_json, out UInt32 id_json_len);

        //ziti_context Ziti_load_context(const char *identity);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_load_context", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr /*ziti_context*/ Ziti_load_context(string identityPath);

        // ziti_socket_t Ziti_socket(int type);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_socket", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr /*ziti_socket_t*/ Ziti_socket(SocketType type); //returns a handle to a socket



        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_close", CallingConvention = API.CALL_CONVENTION)]
        //int Ziti_close(ziti_socket_t socket);
        public static extern int Ziti_close(IntPtr /*ziti_socket_t*/ socket);

        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_check_socket", CallingConvention = API.CALL_CONVENTION)]
        //int Ziti_check_socket(ziti_socket_t socket);
        public static extern int Ziti_check_socket(IntPtr /*ziti_socket_t*/ socket);

        //int Ziti_connect(ziti_socket_t socket, ziti_context ztx, const char *service, const char *terminator);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_connect", CallingConvention = API.CALL_CONVENTION)]
        public static extern int Ziti_connect(IntPtr ziti_socket_t, IntPtr ziti_context, string service, string terminator);

        //int Ziti_connect_addr(ziti_socket_t socket, const char *host, unsigned int port);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_connect_addr", CallingConvention = API.CALL_CONVENTION)]
        public static extern int Ziti_connect_addr(IntPtr ziti_socket_t, string host, int port);

        //int Ziti_bind(ziti_socket_t socket, ziti_context ztx, const char* service, const char* terminator);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_bind", CallingConvention = API.CALL_CONVENTION)]
        public static extern int Ziti_bind(IntPtr /*ziti_socket_t*/ socket, IntPtr /*ziti_context*/ ztx, string service, string terminator);

        //int Ziti_listen(ziti_socket_t socket, int backlog);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_listen", CallingConvention = API.CALL_CONVENTION)]
        public static extern int Ziti_listen(IntPtr /*ziti_socket_t*/ socket, int backlog);

        //ziti_socket_t Ziti_accept(ziti_socket_t socket, char* caller, int caller_len);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_accept", CallingConvention = API.CALL_CONVENTION)]
        public static extern IntPtr /*ziti_socket_t*/ Ziti_accept(IntPtr /*ziti_socket_t*/ socket, IntPtr caller, int caller_len);

        //void Ziti_lib_shutdown(void);
        [DllImport(API.Z4D_DLL_PATH, EntryPoint = "Ziti_lib_shutdown", CallingConvention = API.CALL_CONVENTION)]
        public static extern void Ziti_lib_shutdown();
        #endregion

        #region //helper functions
        internal static IntPtr ToPtr(string[] array) {
            if (array == null || array.Length == 0) {
                return IntPtr.Zero;
            }
            var arr = API.z4d_make_char_array(array.Length);
            var idx = 0;
            foreach (var s in array) {
                API.z4d_set_char_at(arr, s, idx++);
            }

            return arr;
        }
        #endregion
    }

}
