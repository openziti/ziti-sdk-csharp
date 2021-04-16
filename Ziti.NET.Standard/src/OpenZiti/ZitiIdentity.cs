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
using System.Collections.Generic;
using System.IO;
using System.Runtime.InteropServices;
using System.Text.Json;
using System.Text.Json.Serialization;

namespace OpenZiti
{
    public class ZitiIdentity {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        public ZitiStatus InitStats;
        public string InitStatusError;
        public string IdentityNameFromController;
        public string ControllerVersion;
        public bool ControllerConnected;
        public InitOptions InitOpts;

        public string ConfigFilePath { get; private set; }

        private Native.IdentityFile nid;

        public string ControllerURL {
            get {
                return nid.ztAPI;
            }
        }

        public ZitiIdentity()
        {
        }

        public static ZitiIdentity FromFile(string identityFile) {

            var jsonUtf8Bytes = File.ReadAllBytes(identityFile);
            var jsonSpan = new ReadOnlySpan<byte>(jsonUtf8Bytes);

            ZitiIdentity zid = new ZitiIdentity();
            zid.ConfigFilePath = identityFile;
            zid.nid = JsonSerializer.Deserialize<Native.IdentityFile>(jsonSpan);
            return zid;
        }

        public void Start() {
            ZitiOptions InitOptions = new ZitiOptions();
            InitOptions.ConfigFile = ConfigFilePath;
        }

        public ZitiConnection Dial(string serviceName) {

            ZitiConnection conn = null;// new ZitiConnection();
            return conn;
        }

        public void Run(InitOptions opts) {
            InitOpts = opts;
            Native.ziti_options ziti_opts = new Native.ziti_options {
                app_ctx = GCHandle.Alloc(opts.ApplicationContext, GCHandleType.Pinned),
                config = opts.IdentityFile,
                config_types = Native.API.z4d_all_config_types(), /*opts.ConfigurationTypes,*/
                refresh_interval = opts.RefreshInterval,
                metrics_type = opts.MetricType,
                pq_mac_cb = native_ziti_pq_mac_cb,
                events = opts.EventFlags,
                event_cb = ziti_event_cb,
            };

            StructWrapper ziti_opts_ptr = new StructWrapper(ziti_opts);
            Native.API.ziti_init_opts(ziti_opts_ptr.Ptr, API.DefaultLoop.nativeUvLoop);
            Native.API.z4d_uv_run(API.DefaultLoop.nativeUvLoop);
        }

        public struct InitOptions {
            public object ApplicationContext;
            public string IdentityFile;
            public string[] ConfigurationTypes;
            public int RefreshInterval;
            public RateType MetricType;
            public uint EventFlags;
            public event EventHandler<ZitiContextEvent> OnZitiContextEvent;
            public event EventHandler<ZitiRouterEvent> OnZitiRouterEvent;
            public event EventHandler<ZitiServiceEvent> OnZitiServiceEvent;

            internal void ZitiContextEvent(ZitiContextEvent evt) {
                OnZitiContextEvent?.Invoke(this, evt);
            }

            internal void ZitiRouterEvent(ZitiRouterEvent evt) {
                OnZitiRouterEvent?.Invoke(this, evt);
            }

            internal void ZitiServiceEvent(ZitiServiceEvent evt) {
                OnZitiServiceEvent?.Invoke(this, evt);
            }
        }

        private void ziti_event_cb(IntPtr ziti_context, IntPtr ziti_event) {
            int type = Native.NativeHelperFunctions.ziti_event_type_from_pointer(ziti_event);
            switch (type) {
                case ZitiEventFlags.ZitiContextEvent:
                    Native.ziti_context_event ziti_context_event = Marshal.PtrToStructure<Native.ziti_context_event>(ziti_event);
                    var vptr = Native.API.ziti_get_controller_version(ziti_context);
                    ziti_version v = Marshal.PtrToStructure<ziti_version>(vptr);
                    IntPtr ptr = Native.API.ziti_get_controller(ziti_context);
                    string name = Marshal.PtrToStringAnsi(ptr);
                    ZitiContextEvent evt = new ZitiContextEvent() {
                        Name = name,
                        Online = ziti_context_event.ctrl_status == 0,
                        StatusError = ziti_context_event.err,
                        Version = v,
                    };
                    InitOpts.ZitiContextEvent(evt);
                    break;
                case ZitiEventFlags.ZitiRouterEvent:
                    Native.ziti_router_event ziti_router_event = Marshal.PtrToStructure<Native.ziti_router_event>(ziti_event);
                    
                    ZitiRouterEvent routerEvent = new ZitiRouterEvent() {
                        Name = ziti_router_event.name,
                        Type = (RouterEventType)ziti_router_event.status,
                        Version = ziti_router_event.version,
                    };
                    InitOpts.ZitiRouterEvent(routerEvent);
                    break;
                case ZitiEventFlags.ZitiServiceEvent:
                    Native.ziti_service_event ziti_service_event = Marshal.PtrToStructure<Native.ziti_service_event>(ziti_event);

                    ZitiServiceEvent serviceEvent = new ZitiServiceEvent() {
                        removed = ziti_service_event.removed,
                        changed = ziti_service_event.changed,
                        added = ziti_service_event.added,
                    };
                    InitOpts.ZitiServiceEvent(serviceEvent);
                    break;
                default:
                    Logger.Warn("UNEXPECTED ZitiEventFlags [{0}]! Please report.", type);
                    break;
            }
        }
        private void native_ziti_pq_mac_cb(IntPtr ziti_context, string id, Native.ziti_pr_mac_cb response_cb) {

        }
        private void native_ziti_pr_mac_cb(IntPtr ziti_context, string id, string[] mac_addresses, int num_mac) {

        }
    }

    public class ZitiContextEvent {
        public bool Online;
        public string StatusError;
        public string Name;
        public ziti_version Version;
    }
    public class ZitiRouterEvent {
        public string Name;
        public RouterEventType Type;
        public string Version;
    }
    public class ZitiServiceEvent {
        internal IntPtr removed;
        internal IntPtr changed;
        internal IntPtr added;

        private IEnumerable<IntPtr> array_iterator(IntPtr arr) {
            int index = 0;
            while (true) {
                IntPtr removedService = Native.API.ziti_service_array_get(arr, index);
                index++;
                if (removedService == IntPtr.Zero) {
                    break;
                }
                yield return removedService;
            }
        }

        public IEnumerable<IntPtr> Removed() {
            foreach(IntPtr p in array_iterator(removed)) {
                yield return p;
            }
        }

        public IEnumerable<IntPtr> Changed() {
            foreach (IntPtr p in array_iterator(changed)) {
                yield return p;
            }
        }

        public IEnumerable<IntPtr> Added() {
            foreach (IntPtr p in array_iterator(added)) {
                yield return p;
            }
        }
    }

    public static class ZitiEventFlags {
        public const int ZitiContextEvent = 1;
        public const int ZitiRouterEvent = 1 << 1;
        public const int ZitiServiceEvent = 1 << 2;
    }
}
