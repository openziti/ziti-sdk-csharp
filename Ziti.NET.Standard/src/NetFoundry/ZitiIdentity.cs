using System;
using System.Threading.Tasks;
using System.Threading;
using System.IO;
using System.Runtime.InteropServices;

namespace NetFoundry
{
    /// <summary>
    /// Represents an enrolled identity
    /// </summary>
    public class ZitiIdentity
    {
        /// <summary>
        /// The path to the enrolled identity
        /// </summary>
        public string PathToConfigFile { get; private set; }

        internal int timeout = 0;
        internal IntPtr? uvLoop = null;
        internal bool ready = false;
        internal readonly static GCHandle NO_CONTEXT = GCHandle.Alloc(new object());
        internal IntPtr stored_NF_context = IntPtr.Zero;
        private bool isInitialized = false;
        private Exception startException = null;

        /// <summary>
        /// Creates a new ZitiIdentity using the provided path. The path must point at
        /// a file is the result of the enrollment process.
        /// </summary>
        /// <param name="path">The path to the enrolled Ziti identity</param>
        public ZitiIdentity(string path)
        {
            this.PathToConfigFile = path;
        }

        /// <summary>
        /// Creates a new ZitiIdentity with the provided timeout (ms) using the provided 
        /// path. The path must point at a file is the result of the enrollment process.
        /// </summary>
        /// <param name="path">The path to the enrolled ziti identity</param>
        /// <param name="timeOutInMillis">timeout in milliseconds</param>
        public ZitiIdentity(string path, int timeOutInMillis)
        {
            if(!File.Exists(path))
            {
                throw new ArgumentException("The provided path does not exist: " + path);
            }
            this.PathToConfigFile = path;

            if(timeOutInMillis < 0)
            {
                throw new ArgumentException("The timeout cannot be < 0");
            }
            if (timeout < 1000)
            {
                System.Diagnostics.Debug.Write("timeout is set to under 1000 ms.");
            }
            this.timeout = timeOutInMillis;
        }

        /// <summary>
        /// Creates a new ZitiConnection for this identity
        /// </summary>
        /// <param name="serviceName">The service name to create a ZitiConnection for</param>
        /// <returns>A ZitiConnection that is ready to be Dialed or converted to a stream via AsStream()</returns>
        /// <exception cref="ZitiException">Thrown when the serviceName provided does not exist</exception>
        public ZitiConnection NewConnection(string serviceName)
        {
            if (!isInitialized)
            {
                throw new ZitiException("This identity is not yet initialized. InitializeAndRun must be called before creating a connection.");
            }
            if (ServiceAvailable(serviceName))
            {
                ZitiConnection conn = new ZitiConnection(this, this.stored_NF_context, serviceName);
                GCHandle nf_connection_gc_handle = GCHandle.Alloc(conn);
                int result = Ziti.InitializeConnection(stored_NF_context, out IntPtr zitiManagedConnectionPtr, nf_connection_gc_handle);
                if(result < 0)
                {
                    ZitiStatus status = (ZitiStatus)result;
                    throw new ZitiException(status.GetDescription());
                }
                conn.nf_connection = zitiManagedConnectionPtr;
                return conn;
            }
            else
            {
                throw new ZitiException("The service named: " + serviceName + " does not exist");
            }
        }

        /// <summary>
        /// Determines if the provided serviceName is available for this identity
        /// </summary>
        /// <param name="serviceName">The service name to verify</param>
        /// <returns>If the service exists - true, false if not</returns>
        public bool ServiceAvailable(string serviceName)
        {
            int result = Ziti.ServiceAvailable(stored_NF_context, serviceName);
            if (result == 0)
            {
                return true;
            }
            else if (result == (int)ZitiStatus.SERVICE_UNAVALABLE)
            {
                return false;
            }
            throw new ZitiException(((ZitiStatus)result).GetDescription());
        }

        /// <summary>
        /// Initializes this identity with the NetFoundry network
        /// </summary>
        /// <exception cref="Exception">Thrown when the path to the configuration file no longer exists or if the provided identity file is not valid</exception>
        public void InitializeAndRun()
        {
            Task.Factory.StartNew(()=>
            {
                try
                {
                    uvLoop = Ziti.CreateUVLoop();
                    long interval = 1000; //ms
                    Ziti.RegisterUVTimer(uvLoop.Value, timer, interval, interval);
                    var initializeResult = Ziti.InitializeAndRun(PathToConfigFile, uvLoop.Value, AfterInitialize, NO_CONTEXT);
                    if (initializeResult < (int)ZitiStatus.OK)
                    {
                        throw new ZitiException("An unexpected exception has occurred. Please check standard error for more information");
                    }
                }
                catch (Exception e)
                {
                    startException = e;

                    lock (this)
                    {
                        Monitor.Pulse(this);
                    }
                }
            });

            lock (this)
            {
                Monitor.Wait(this); //lock will be released in the AfterInitialize callback
                if (!isInitialized)
                {
                    if (startException != null)
                    {
                        throw startException;
                    }
                }
            }
        }

        private void AfterInitialize(IntPtr nf_context, int status, GCHandle originalContext)
        {
            try
            {
                if (status < 0)
                {
                    //something went wrong during the initialization.
                    ZitiStatus zstatus = (ZitiStatus)status;
                    startException = new ZitiException("An error has occurred during initialization. " + zstatus.GetDescription());
                }
                else
                {
                    stored_NF_context = nf_context; //todo - do this better?
                    if (this.timeout > 0)
                    {
                        int result = Ziti.SetConnectionTimeout(stored_NF_context, this.timeout);
                        if (result != 0)
                        {
                            //something has gone wrong while setting the timeout
                            ZitiStatus zstatus = (ZitiStatus)status;
                            startException = new ZitiException("An error has occurred during initialization. " + zstatus.GetDescription());
                        }
                    }
                    FreeGCHandle(originalContext);
                    isInitialized = true;
                }
            }
            finally
            {
                lock (this)
                {
                    Monitor.Pulse(this);
                }
            }
        }

        private void FreeGCHandle(GCHandle? handle)
        {
            if (handle.HasValue)
            {
                if (NO_CONTEXT == handle.Value)
                {
                    return; //never call free on the NO_CONTEXT handle...
                }
                if (handle.Value.IsAllocated)
                {
                    handle.Value.Free();
                }
            }
        }

        private void timer(IntPtr handle)
        {
            //only exists to keep the UVLoop alive.
        }
        
        /// <summary>
        /// instructs this identity to disconnect from the NetFoundry network
        /// </summary>
        public void Shutdown()
        {
            Ziti.Shutdown(this.stored_NF_context);
        }

        /// <summary>
        /// Dumps debug information to standard out. Only used when debugging
        /// </summary>
        public void Dump()
        {
            Ziti.Dump(this.stored_NF_context);
        }
    }
}
