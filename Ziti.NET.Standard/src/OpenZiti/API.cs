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

using System.Runtime.InteropServices;

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

    public class API
    {
        public static void Run()
        {
            Native.API.z4d_uv_run(Native.API.z4d_default_loop());
        }
    }
}
