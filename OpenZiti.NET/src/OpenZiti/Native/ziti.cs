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
using System.Collections.Generic;
using System.Runtime.InteropServices;

namespace OpenZiti.Native {

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


/* Unmerged change from project 'TestProject'
Before:
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
After:
    internal class MarshalUtils<T>
    {
        internal static List<T> convertPointerToList(IntPtr arrayPointer)
        {
            IntPtr currentArrLoc;
            var result = new List<T>();
            var sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = Marshal.ReadIntPtr(arrayPointer)) != IntPtr.Zero)
            {
                T objectT;
                if (typeof(T) == typeof(string))
*/
    internal class MarshalUtils<T> {
        internal static List<T> convertPointerToList(IntPtr arrayPointer) {
            IntPtr currentArrLoc;
            var result = new List<T>();
            var sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = Marshal.ReadIntPtr(arrayPointer)) != IntPtr.Zero) {
                T objectT;
                if (typeof(T) == typeof(string)) {
                    objectT = (T)(object)Marshal.PtrToStringUTF8(currentArrLoc);
                } else if (typeof(T).IsValueType && !typeof(T).IsPrimitive) {
                    objectT = Marshal.PtrToStructure<T>(currentArrLoc);
                } else {
                    // marshal operations for other types can be added here
                    throw new ZitiException("Marshalling is not yet supported for " + typeof(T));
                }
                result.Add(objectT);
                arrayPointer = IntPtr.Add(arrayPointer, sizeOfPointer);
            }
            return result;
        }

        internal static List<model_map_entry> convertPointerMapToList(IntPtr arrayPointer) {
            IntPtr currentArrLoc;

/* Unmerged change from project 'TestProject'
Before:
            List<model_map_entry> result = new List<model_map_entry>();
            int sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = arrayPointer) != IntPtr.Zero)
            {
                model_map_entry objectT = Marshal.PtrToStructure<model_map_entry>(currentArrLoc);
After:
            var result = new List<model_map_entry>();
            var sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = arrayPointer) != IntPtr.Zero)
            {
                var objectT = Marshal.PtrToStructure<model_map_entry>(currentArrLoc);
*/
            var result = new List<model_map_entry>();
            var sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = arrayPointer) != IntPtr.Zero) {
                var objectT = Marshal.PtrToStructure<model_map_entry>(currentArrLoc);
                result.Add(objectT);
                arrayPointer = objectT._next;
            }
            return result;
        }
    }


}
