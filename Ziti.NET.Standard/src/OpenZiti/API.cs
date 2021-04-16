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
using System.Threading;
using System.Threading.Tasks;

namespace OpenZiti
{
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

    public enum RateType
    {
        EWMA_1m, //Exponentially Weighted Moving Average - 1min
        EWMA_5m, //Exponentially Weighted Moving Average - 5min
        EWMA_15m, //Exponentially Weighted Moving Average - 15min
        MMA_1m,  //Modified Moving Average - 1min
        CMA_1m,  //Centered Weighted Moving Average - 1min
        EWMA_5s,  //Exponentially Weighted Moving Average - 5sec
        INSTANT, //Simple average from the last 5 sec
    };

    public class API {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();
        
        public delegate void AfterEnroll(ZitiStatus status);

        static Native.log_writer logger = logFunction;

        static internal void logFunction(int level, string loc, string msg, uint msglen) {

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
        static Native.ziti_enroll_cb enroll_cb = Callback.ziti_enroll_cb_impl;

        public static void BeginEnroll(IntPtr loop, string identityFile, ref AfterEnroll afterEnroll) {


            Logger.Error("Default Loop: {0}, this loop: {1}", Native.API.z4d_default_loop(), loop);

            Native.API.ziti_log_init(loop, 11, Marshal.GetFunctionPointerForDelegate<Native.log_writer>(logger) );


            Native.ziti_enroll_options opts = new Native.ziti_enroll_options() {
                jwt = identityFile,
            };

            GCHandle enroll_context = GCHandle.Alloc(afterEnroll);
            IntPtr pnt = Marshal.AllocHGlobal(Marshal.SizeOf(opts));
            Marshal.StructureToPtr(opts, pnt, false);
            Native.API.ziti_enroll(pnt, ref loop, ref enroll_cb, GCHandle.FromIntPtr(pnt));
        }

        public static void Run(IntPtr loop)
        {
            Native.API.z4d_uv_run(loop);
        }

        public static IntPtr NewLoop() {
            return Native.API.newLoop();
        }
    }

    internal class Callback {
        
        public static void ziti_enroll_cb_impl(IntPtr ziti_config, int status, string errorMessage, GCHandle enroll_context) {
            if (enroll_context.IsAllocated) {
                API.AfterEnroll cb = (API.AfterEnroll)enroll_context.Target;
                enroll_context.Free();
            } else {
                Console.WriteLine("well what the heck?");
            }
        }
    }
}
