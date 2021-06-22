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
using System.Collections.Generic;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Runtime.Serialization;
using System.Text.Json;
using System.Text.Json.Serialization;
using System.Threading;
using System.Threading.Tasks;

namespace OpenZiti {
	public class ZitiIdentity {
		private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

		private SemaphoreSlim runlock = new SemaphoreSlim(0);
		private SemaphoreSlim streamLock = new SemaphoreSlim(0);
		private bool _isRunning;
		private object _isRunningLock = new object();
		private Exception startException;
		private bool isInitialized;
		private Dictionary<string, ZitiService> services = new Dictionary<string, ZitiService>();

		public bool IsRunning {
			get {
				lock (_isRunningLock) {
					return _isRunning;
				}
			}
			internal set {
				lock (_isRunningLock) {
					_isRunning = value;
				}
			}
		}

		private UVLoop _loop = API.DefaultLoop; //use the default loop unless changed
		private object _loopLock = new object();

		public UVLoop Loop {
			get {
				lock (_loopLock) {
					return _loop;
				}
			}
			set {
				if (IsRunning) {
					throw new System.InvalidOperationException("the loop cannot be changed once it is running");
				} else {
					lock (_loopLock) {
						_loop = value;
					}
				}
			}
		}

		public ZitiStatus InitStats { get; internal set; }
		public string InitStatusError { get; internal set; }
		public string IdentityNameFromController { get; internal set; }
		public string ControllerVersion { get; internal set; }
		public bool ControllerConnected { get; internal set; }
		public object ApplicationContext { get; internal set; }
		public InitOptions InitOpts { get; internal set; }
		internal IntPtr NativeContext;
		internal ZitiContext WrappedContext { get; set; }

		public string ConfigFilePath { get; private set; }

		private Native.IdentityFile nid;
		private UVLoop uvLoop;
		private const int DefaultRefreshInterval = 15;

		public string ControllerURL => nid.ztAPI;

		public ZitiIdentity(InitOptions opts) {
			this.InitOpts = opts;
			if (opts.IdentityFile != null) {
				string json = File.ReadAllText(opts.IdentityFile);
				nid = JsonSerializer.Deserialize<Native.IdentityFile>(json);
			}
		}

		public ZitiService GetService(string serviceName) {
			//ZitiService svc = new ZitiService(null, IntPtr.Zero);;
			throw new NotImplementedException();
		}

		public ZitiConnection NewConnection(string serviceName) {
			if (!isInitialized) {
				throw new ZitiException("This identity is not yet initialized. InitializeAndRun must be called before creating a connection.");
			}
			if (ServiceAvailable(serviceName)) {
				var found = services.FirstOrDefault(kvp => kvp.Value.Name == serviceName);
				if (found.Value == null) {
					throw new KeyNotFoundException("The service named " + serviceName + " was not found");
				}

				return new ZitiConnection(found.Value, WrappedContext, serviceName);
			}
			throw new KeyNotFoundException("The service named " + serviceName + " was not found");
		}

		/// <summary>
		/// Determines if the provided serviceName is available for this identity
		/// </summary>
		/// <param name="serviceName">The service name to verify</param>
		/// <returns>If the service exists - true, false if not</returns>
		public bool ServiceAvailable(string serviceName) {
			IntPtr ctx = default;
			int result = Native.API.ziti_service_available(NativeContext, serviceName, ziti_service_cb, ctx);
			if (result == 0) {
				return true;
			} else if (result == (int)ZitiStatus.SERVICE_UNAVAILABLE) {
				return false;
			}
			throw new ZitiException(((ZitiStatus)result).GetDescription());
		}

		private void ziti_service_cb(IntPtr ziti_context, IntPtr ziti_service, int status, GCHandle on_service_context) {
			//throw new NotImplementedException();
			Logger.Debug("here we are");
		}

		public void Run() {
			RunAsync(DefaultRefreshInterval).Wait();
		}

		public void Run(int refreshInterval) {
			RunAsync(refreshInterval).Wait();
		}

		public async Task RunAsync() {
			await RunAsync(DefaultRefreshInterval).ConfigureAwait(false);
		}

		public async Task RunAsync(int refreshInterval) {
			Configure(refreshInterval); //use default refresh interval if not supplied

			if (this.IsRunning) {
				throw new System.InvalidOperationException("The identity is already running");
			}
			
			new Thread(() => Native.API.z4d_uv_run(Loop.nativeUvLoop)).Start();
			await runlock.WaitAsync().ConfigureAwait(false);
		}

		public void Configure(int refreshInterval) {
			Native.API.ziti_log_init(Loop.nativeUvLoop, 11, Marshal.GetFunctionPointerForDelegate(API.NativeLogger));
			IntPtr cfgs = Native.NativeHelperFunctions.ToPtr(InitOpts.ConfigurationTypes);

			Native.ziti_options ziti_opts = new Native.ziti_options {
				//app_ctx = GCHandle.Alloc(InitOpts.ApplicationContext, GCHandleType.Pinned),
				config = InitOpts.IdentityFile,
				config_types = cfgs,
				refresh_interval = refreshInterval,
				metrics_type = InitOpts.MetricType,
				pq_mac_cb = native_ziti_pq_mac_cb,
				events = InitOpts.EventFlags,
				event_cb = ziti_event_cb,
			};
			ApplicationContext = InitOpts.ApplicationContext;

			InitOpts.OnZitiContextEvent += SaveNativeContext;
			StructWrapper ziti_opts_ptr = new StructWrapper(ziti_opts);

			Native.API.ziti_init_opts(ziti_opts_ptr.Ptr, Loop.nativeUvLoop);
		}

		public void Shutdown() {
			runlock.Release();
			try {
				Native.API.ziti_shutdown(this.NativeContext);

				Logger.Info("Shutdown complete");
			}
			catch (Exception e) {
				Logger.Debug("Shutdown complete with error: " + e.Message);
			}
		}

		private void ziti_event_cb(IntPtr ziti_context, IntPtr ziti_event) {
			int type = Native.NativeHelperFunctions.ziti_event_type_from_pointer(ziti_event);
			switch (type) {
				case ZitiEventFlags.ZitiContextEvent:
					NativeContext = ziti_context;
					WrappedContext = new ZitiContext(ziti_context);

					Native.ziti_context_event ziti_context_event = Marshal.PtrToStructure<Native.ziti_context_event>(ziti_event);
					var vptr = Native.API.ziti_get_controller_version(ziti_context);
					ziti_version v = Marshal.PtrToStructure<ziti_version>(vptr);
					IntPtr ptr = Native.API.ziti_get_controller(ziti_context);
					string name = Marshal.PtrToStringUTF8(ptr);

					ZitiContextEvent evt = new ZitiContextEvent() {
						Name = name,
						Status = (ZitiStatus) ziti_context_event.ctrl_status,
						StatusError = ziti_context_event.err,
						Version = v,
						Identity = this,
					};
					InitOpts.ZitiContextEvent(evt);

					lock (this) {
						Monitor.Pulse(this);
						isInitialized = true;
					}

					break;
				case ZitiEventFlags.ZitiRouterEvent:
					Native.ziti_router_event ziti_router_event = Marshal.PtrToStructure<Native.ziti_router_event>(ziti_event);

					ZitiRouterEvent routerEvent = new ZitiRouterEvent() {
						Name = ziti_router_event.name,
						Type = (RouterEventType) ziti_router_event.status,
						Version = ziti_router_event.version,
					};
					InitOpts.ZitiRouterEvent(routerEvent);
					break;
				case ZitiEventFlags.ZitiServiceEvent:
					Native.ziti_service_event ziti_service_event = Marshal.PtrToStructure<Native.ziti_service_event>(ziti_event);

					ZitiServiceEvent serviceEvent = new ZitiServiceEvent() {
						nativeRemovedList = ziti_service_event.removed,
						nativeChangedList = ziti_service_event.changed,
						nativeAddedList = ziti_service_event.added,
						ziti_ctx = ziti_context,
						Context = this.ApplicationContext,
						id = this,
					};

					if (isStreaming) {
						foreach (var removed in serviceEvent.Removed()) {
							services.Remove(removed.Id);
						}
						foreach (var changed in serviceEvent.Changed()) {
							services[changed.Id] = changed;
						}
						foreach (var added in serviceEvent.Added()) {
							services[added.Id] = added;
						}

						streamLock.Release();
					}

					InitOpts.ZitiServiceEvent(serviceEvent);

					break;
				default:
					Logger.Warn("UNEXPECTED ZitiEventFlags [{0}]! Please report.", type);
					break;
			}
		}

		private void SaveNativeContext(object sender, ZitiContextEvent e) {
			Logger.Error("it's ");
			//this.NativeContext = e.na
		}

		public struct InitOptions {
			public object ApplicationContext;
			public string IdentityFile;

			public string[] ConfigurationTypes;

			//public int RefreshInterval;
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

		private void native_ziti_pq_mac_cb(IntPtr ziti_context, string id, Native.ziti_pr_mac_cb response_cb) {

		}

		private void native_ziti_pr_mac_cb(IntPtr ziti_context, string id, string[] mac_addresses, int num_mac) {

		}


		private void timer(IntPtr handle) {
			//only exists to keep the UVLoop alive.
			Logger.Debug("loop is running...");
		}

		private IntPtr uvTimer;
		private DateTime start;
		private bool isStreaming;

		/// <summary>
		/// Initializes this identity with the NetFoundry network
		/// </summary>
		/// <exception cref="Exception">Thrown when the path to the configuration file no longer exists or if the provided identity file is not valid</exception>
		public void InitializeAndRun() {
			isStreaming = true;
			Task.Run(() => {
				try {
					start = DateTime.Now;
					uvLoop = API.DefaultLoop;
					long interval = 1000; //ms
					uvTimer = Native.API.z4d_registerUVTimer(uvLoop.nativeUvLoop, timer, interval, interval);
					this.Run();
				} catch (Exception e) {
					startException = e;

					lock (this) {
						Monitor.Pulse(this);
					}
				}
			});

			lock (this) {
				Monitor.Wait(this); //lock will be released in the AfterInitialize callback
				if (!isInitialized) {
					if (startException != null) {
						throw startException;
					}
				}
			}
		}

		public void Stop() {
			Native.API.z4d_stop_uv_timer(uvTimer);
		}

		public void WaitSync() {

		}

		public async Task WaitForServices() {
			await streamLock.WaitAsync().ConfigureAwait(false);
		}
	}

	public class ZitiContextEvent {
		public ZitiStatus Status;
		public string StatusError;
		public string Name;
		public ziti_version Version;
		public ZitiIdentity Identity;
	}

	public class ZitiRouterEvent {
		public string Name;
		public RouterEventType Type;
		public string Version;
	}

	public class ZitiServiceEvent {
		internal IntPtr nativeRemovedList;
		internal IntPtr nativeChangedList;
		internal IntPtr nativeAddedList;
		internal IntPtr ziti_ctx;
		internal ZitiIdentity id;

		public object Context { get; internal set; }

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

		public IEnumerable<ZitiService> Removed() {
			foreach (IntPtr p in array_iterator(nativeRemovedList)) {
				ZitiService svc = new ZitiService(id, new ZitiContext(ziti_ctx), p);
				yield return svc;
			}
		}

		public IEnumerable<ZitiService> Changed() {
			foreach (IntPtr p in array_iterator(nativeChangedList)) {
				ZitiService svc = new ZitiService(id, new ZitiContext(ziti_ctx), p);
				yield return svc;
			}
		}

		public IEnumerable<ZitiService> Added() {
			foreach (IntPtr p in array_iterator(nativeAddedList)) {
				ZitiService svc = new ZitiService(id, new ZitiContext(ziti_ctx), p);
				yield return svc;
			}
		}
	}

	public static class ZitiEventFlags {
		public const int ZitiContextEvent = 1;
		public const int ZitiRouterEvent = 1 << 1;
		public const int ZitiServiceEvent = 1 << 2;
	}
}
