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
using System.Threading;
using System.Threading.Tasks;
using System.Collections.Generic;

namespace OpenZiti {
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnInit(ZitiContext zitiContext, ZitiStatus status, object initContext);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnServiceChange(ZitiContext zitiContext, ZitiService service, ZitiStatus status, int flags, object serviceContext);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnZitiConnected(ZitiConnection connection, ZitiStatus status);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnZitiDataReceived(ZitiConnection connection, ZitiStatus status, byte[] data);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnZitiDataWritten(ZitiConnection connection, ZitiStatus status, object context);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnZitiListening(ZitiConnection connection, ZitiStatus status);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnZitiClientConnected(ZitiConnection serverConnection, ZitiConnection clientConnection, ZitiStatus status);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnClientAccept(ZitiConnection clientConnection, ZitiStatus status);
    [UnmanagedFunctionPointer(CallingConvention.Cdecl)]
    public delegate void OnZitiClientData(ZitiConnection clientConnection, byte[] data, int len, ZitiStatus status);

    public enum RateType {
        EWMA_1m, //Exponentially Weighted Moving Average - 1min
        EWMA_5m, //Exponentially Weighted Moving Average - 5min
        EWMA_15m, //Exponentially Weighted Moving Average - 15min
        MMA_1m,  //Modified Moving Average - 1min
        CMA_1m,  //Centered Weighted Moving Average - 1min
        EWMA_5s,  //Exponentially Weighted Moving Average - 5sec
        INSTANT, //Simple average from the last 5 sec
    };

    public class API {

        public static readonly string[] AllConfigs = new string[] { "all" };

        public static class Enrollment {
            public delegate void AfterEnroll(ZitiEnrollment.EnrollmentResult result);

            internal class AfterEnrollWrapper {
                public AfterEnroll AfterEnroll;
                public object Context;

                internal StructWrapper wrapper;
                public AfterEnrollWrapper() { }
            }

            internal static void ziti_enroll_cb_impl(IntPtr ziti_config, int status, string msg, GCHandle enroll_context) {
                if (enroll_context.IsAllocated) {
                    Enrollment.AfterEnrollWrapper w = (Enrollment.AfterEnrollWrapper)enroll_context.Target;
                    w.wrapper.Dispose();

                    ZitiEnrollment.EnrollmentResult r = new ZitiEnrollment.EnrollmentResult(ziti_config) {
                        Status = (ZitiStatus)status,
                        Message = msg,
                        Context = w.Context,
                    };

                    if (r.Status.Ok()) {
                        ZitiIdentityFormatNative fromZiti = Marshal.PtrToStructure<ZitiIdentityFormatNative>(ziti_config);
                        r.ZitiIdentity = new ZitiIdentityFormat(fromZiti);
                    }

                    w.AfterEnroll(r);
                } else {
                    Console.WriteLine("unexpected situation");
                }
            }
        }
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private static UVLoop defaultLoop = new UVLoop() { nativeUvLoop = Native.API.newLoop() };
        public static UVLoop DefaultLoop {
            get {
                return defaultLoop;
            }
        }

        public static Native.log_writer NativeLogger = NoopNativeLogFunction;

        public static void NoopNativeLogFunction(int level, string loc, string msg, uint msglen) { }

        public static void DefaultNativeLogFunction(int level, string loc, string msg, uint msglen) {

            switch (level) {
                case 0:
                    Logger.Warn("SDK_: level 0 should not be logged, please report: {0}", msg); break;
                case 1:
                    Logger.Error("SDKe: {0}\t{1}", loc, msg);
                    break;
                case 2:
                    Logger.Warn("SDKw: {0}\t{1}", loc, msg);
                    break;
                case 3:
                    Logger.Info("SDKi: {0}\t{1}", loc, msg);
                    break;
                case 4:
                    Logger.Debug("SDKd: {0}\t{1}", loc, msg);
                    break;
                case 5:
                    //VERBOSE:5
                    Logger.Trace("SDKv: {0}\t{1}", loc, msg);
                    break;
                case 6:
                    //TRACE:6
                    Logger.Trace("SDKt: {0}\t{1}", loc, msg);
                    break;
                default:
                    Logger.Warn("SDK_: level [%d] NOT recognized: {1}", level, msg);
                    break;
            }
        }
        static Native.ziti_enroll_cb enroll_cb = Enrollment.ziti_enroll_cb_impl;

        public static void Enroll(string identityFile, Enrollment.AfterEnroll afterEnroll, object ctx) {
            var loop = API.DefaultLoop;
            Native.API.ziti_log_init(loop.nativeUvLoop, 11, Marshal.GetFunctionPointerForDelegate(NativeLogger));


            Native.ziti_enroll_options opts = new Native.ziti_enroll_options() {
                jwt = identityFile,
            };

            Enrollment.AfterEnrollWrapper w = new Enrollment.AfterEnrollWrapper() {
                AfterEnroll = afterEnroll,
                wrapper = new StructWrapper(opts),
                Context = ctx,
            };

            Native.API.ziti_enroll(w.wrapper.Ptr, loop.nativeUvLoop, enroll_cb, GCHandle.Alloc(w));
        }

        public static UVLoop NewLoop() {
            return new UVLoop(Native.API.newLoop());
        }

        public static string GetConfiguration(ZitiService svc, string configName) {
            IntPtr nativeConfig = Native.API.ziti_service_get_raw_config(svc.nativeServicePointer, configName);
            return Marshal.PtrToStringUTF8(nativeConfig);
        }

        public static void Run() {
            Native.API.z4d_uv_run(DefaultLoop.nativeUvLoop);
        }

        public static void SubmitMFA(ZitiIdentity zid, string code) {
            OpenZiti.Native.API.ziti_mfa_auth(zid.WrappedContext.nativeZitiContext, code, MFA.AfterMFASubmit, MFA.GetMFAStatusDelegate(zid));
        }

        public static void EnrollMFA(ZitiIdentity zid) {
            OpenZiti.Native.API.ziti_mfa_enroll(zid.WrappedContext.nativeZitiContext, MFA.AfterMFAEnroll, MFA.GetMFAStatusDelegate(zid));
        }

        public static void VerifyMFA(ZitiIdentity zid, string code) {
            OpenZiti.Native.API.ziti_mfa_verify(zid.WrappedContext.nativeZitiContext, code, MFA.AfterMFAVerify, MFA.GetMFAStatusDelegate(zid));
        }

        public static void RemoveMFA(ZitiIdentity zid, string code) {
            OpenZiti.Native.API.ziti_mfa_remove(zid.WrappedContext.nativeZitiContext, code, MFA.AfterMFARemove, MFA.GetMFAStatusDelegate(zid));
        }
    }

    public enum MFAOperationType {
        MFA_AUTH_STATUS,
        ENROLLMENT_VERIFICATION,
        ENROLLMENT_REMOVE,
        ENROLLMENT_CHALLENGE
    }

    public struct MFAEnrollment {
        public bool isVerified;
        public string[] recoveryCodes;
        public string provisioningUrl;
    }

    class MFA {

        internal static IntPtr GetMFAStatusDelegate(ZitiIdentity zid) {
            ZitiIdentity.MFAStatusCB mfaStatusCB = new ZitiIdentity.MFAStatusCB();
            mfaStatusCB.zidOpts = zid.InitOpts;
            ZitiIdentity.MFAStatusCB.ZitiResponseDelegate cbDelegate = mfaStatusCB.ZitiResponse;
            return Marshal.GetFunctionPointerForDelegate(cbDelegate);
        }

        internal static void AfterMFASubmit(IntPtr ziti_context, int status, IntPtr ctx) {
            ZitiIdentity.MFAStatusCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.MFAStatusCB.ZitiResponseDelegate>(ctx);

            ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent() {
                status = (ZitiStatus)status,
                operationType = MFAOperationType.MFA_AUTH_STATUS
            };
            cb?.Invoke(evt);
        }

        internal static void AfterMFAEnroll(IntPtr ziti_context, int status, IntPtr /*ziti_mfa_enrollment*/ enrollment, IntPtr ctx) {
            OpenZiti.Native.ziti_mfa_enrollment ziti_mfa_enrollment = Marshal.PtrToStructure<OpenZiti.Native.ziti_mfa_enrollment>(enrollment);
            ZitiIdentity.MFAStatusCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.MFAStatusCB.ZitiResponseDelegate>(ctx);

            ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent() {
                status = (ZitiStatus)status,
                isVerified = ziti_mfa_enrollment.is_verified,
                operationType = MFAOperationType.ENROLLMENT_CHALLENGE,
                provisioningUrl = ziti_mfa_enrollment.provisioning_url,
            };

            if (ziti_mfa_enrollment.recovery_codes != IntPtr.Zero) {
                evt.recoveryCodes = MarshalUtils<string>.convertPointerToList(ziti_mfa_enrollment.recovery_codes).ToArray();
            }

            cb?.Invoke(evt);

        }

        internal static void AfterMFAVerify(IntPtr ziti_context, int status, IntPtr ctx) {
            ZitiIdentity.MFAStatusCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.MFAStatusCB.ZitiResponseDelegate>(ctx);

            ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent() {
                status = (ZitiStatus)status,
                operationType = MFAOperationType.ENROLLMENT_VERIFICATION
            };
            cb?.Invoke(evt);
        }
        internal static void AfterMFARemove(IntPtr ziti_context, int status, IntPtr ctx) {
            ZitiIdentity.MFAStatusCB.ZitiResponseDelegate cb = Marshal.GetDelegateForFunctionPointer<ZitiIdentity.MFAStatusCB.ZitiResponseDelegate>(ctx);

            ZitiMFAStatusEvent evt = new ZitiMFAStatusEvent() {
                status = (ZitiStatus)status,
                operationType = MFAOperationType.ENROLLMENT_REMOVE
            };
            cb?.Invoke(evt);
        }
    }

    class StructWrapper : IDisposable {
        public IntPtr Ptr { get; private set; }

        public StructWrapper(object obj) {
	        Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
	        Marshal.StructureToPtr(obj, Ptr, false);
        }

        ~StructWrapper() {
            if (Ptr != IntPtr.Zero) {
                Marshal.FreeHGlobal(Ptr);
                Ptr = IntPtr.Zero;
            }
        }

        public void Dispose() {
            Marshal.FreeHGlobal(Ptr);
            Ptr = IntPtr.Zero;
            GC.SuppressFinalize(this);
        }

        public static implicit operator IntPtr(StructWrapper w) {
            return w.Ptr;
        }

    }

    class MarshalUtils<T> {
        public static List<T> convertPointerToList(IntPtr arrayPointer) {
            IntPtr currentArrLoc;
            List<T> result = new List<T>();
            int sizeOfPointer = Marshal.SizeOf(typeof(IntPtr));

            while ((currentArrLoc = Marshal.ReadIntPtr(arrayPointer)) != IntPtr.Zero) {
                T objectT;
                if (typeof(T) == typeof(String)) {
                    objectT = (T)(object)Marshal.PtrToStringUTF8(currentArrLoc);
                } else if (typeof(T).IsValueType && !typeof(T).IsPrimitive) {
                    objectT = Marshal.PtrToStructure<T>(currentArrLoc);
                } else {
                    break;
                }            
                result.Add(objectT);
                arrayPointer = IntPtr.Add(arrayPointer, sizeOfPointer);
            }
            return result;
        }
    }

    /*
    public struct IdentityMaterial {
        public string Certificate;
        public string Key;
        public string CA;
    }*/
    public struct ziti_version {
#pragma warning disable 0649
        internal string version;
        internal string revision;
        internal string build_date;
#pragma warning restore 0649
    }

    public enum RouterEventType {
        EdgeRouterConnected,
        EdgeRouterDisconnected,
        EdgeRouterRemoved,
        EdgeRouterUnavailable,
    }

    public struct ziti_service {
        public string id;
        public string name;
        public IntPtr permissions;
        public bool encryption;
        public int perm_flags;
        public string config;
        public IntPtr /** posture_query_set[] **/ posture_query_set;
        public IntPtr /** Dictionary<string, posture_query_set> **/ posture_query_map;
        public string updated_at;
    }

    public struct posture_query_set {
        public string policy_id;
        public bool is_passing;
        public string policy_type;
        public IntPtr /** posture_query[] **/ posture_queries;
    }
    public struct posture_query {
        public string id;
        public bool is_passing;
        public string query_type;
        public IntPtr /** ziti_process **/ process;
        public int timeout;
    }

    public struct ziti_process {
        public string path;
    }

    public struct ziti_identity {
        internal string id;
        internal string name;
        internal string app_data;
    }

    public struct model_map_impl {
        internal IntPtr /** model_map_entry[] **/ entries;
        internal IntPtr table;
        internal int buckets;
        internal int size;
    }

    public struct model_map_entry {
        internal IntPtr key;
        internal int key_len;
        internal uint key_hash;
        internal IntPtr value;
    }
}
