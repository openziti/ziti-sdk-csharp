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
using System.Threading;
using System.Threading.Tasks;

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
                    Console.WriteLine("well what the heck?");
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
    }

    class StructWrapper : IDisposable {
        public IntPtr Ptr { get; private set; }

        public StructWrapper(object obj) {
            if (Ptr != null) {
                Ptr = Marshal.AllocHGlobal(Marshal.SizeOf(obj));
                Marshal.StructureToPtr(obj, Ptr, false);
            } else {
                Ptr = IntPtr.Zero;
            }
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
        public string permissions;
        public bool encryption;
        public int perm_flags;
        public string config;
        //public posture_query_set posture_query_set;
    }

    public struct posture_query_set {
        public string policy_id;
        public bool is_passing;
        public string policy_type;
        public posture_query[] posture_queries;
    }
    public struct posture_query {
        public string id;
        public bool is_passing;
        public string query_type;
        public ziti_process process;
        public int timeout;
    }

    public struct ziti_process {
        public string path;
    }
}
