using System;
using System.Threading;

using System.Collections.Concurrent;
using System.Runtime.InteropServices;

using System.Buffers;

namespace NetFoundry
{
    /// <summary>
    /// Represents a connection through the Ziti network. Supports both <see cref="System.IO.Stream"/> use-cases
    /// as well as callback-based. 
    /// </summary>
    public class ZitiConnection : IDisposable
    {
        //PUBLIC delegates here

        /// <summary>
        /// A delegate that represents the work to be done after a <see cref="ZitiConnection.Dial"/> operation. 
        /// The result of the <see cref="ZitiConnection.Dial"/> may NOT be successful. 
        /// It is important to verify the result by checking <paramref name="status"/>
        /// </summary>
        /// <param name="zitiConnection">The <see cref="ZitiConnection"/> which as passed to the <see cref="ZitiConnection.Dial"/> method</param>
        /// <param name="status">The <see cref="ZitiStatus"/> representing the outcome of the <see cref="ZitiConnection.Dial"/></param>
        public delegate void OnConnected(ZitiConnection zitiConnection, ZitiStatus status);

        /// <summary>
        /// A delegate that represents the work to be done when data is recieved over the Ziti network. 
        /// Only invoked after a successful <see cref="ZitiConnection.Dial"/>. Each time data is received
        /// it is important to verify the <paramref name="status"/> is still <see cref="ZitiStatus.OK"/>
        /// </summary>
        /// <param name="status">The <see cref="ZitiStatus"/> representing the outcome of the write operation.</param>
        /// <param name="data">A buffer representing the data that was received over the Ziti network. Data will always start at postiion 0.</param>
        /// <param name="count">The number of bytes received in this interation. Data will always start at postiion 0.</param>
        public delegate void OnDataReceived(ZitiStatus status, byte[] data, int count);

        /// <summary>
        /// A delegate that is invoked after data has been put into the event loop. If there are any expensive
        /// resources held this is the callback to release those resources.
        /// Only needed when not using Ziti as a <see cref="System.IO.Stream"/> (callback based Ziti)
        /// </summary>
        /// <param name="status">A <see cref="ZitiStatus"/> that represents the state of the connection which 
        /// initiated this callback. If _NOT_ <see cref="ZitiStatus.OK"/> appropriate actions should be taken</param>
        /// <param name="bytesWritten">A count of how many bytes were able to be written.</param>
        /// <param name="context">The context that was supplied during the <see cref="ZitiConnection.Write(byte[], int, OnDataWritten, object)"/> invocation</param>
        public delegate void OnDataWritten(ZitiStatus status, int bytesWritten, object context);

        private bool isDialed;

        internal OnConnected OnConncetedCallback { get; set; }
        internal OnDataReceived OnDataReceivedCallback { get; set; }

        internal string serviceName;


        internal ZitiIdentity Identity { get; set; }
        internal IntPtr nf_connection { get; set; }
        internal readonly static GCHandle NO_CONTEXT = GCHandle.Alloc(new object());

        internal bool readyForWriting = false;
        internal bool readyForReading = false;

        internal BlockingCollection<byte[]> responses;
        
        private bool isStream = false;
        private ZitiStatus connectionReadyStatus;

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

        private void reset()
        {
            readyForWriting = false;
            readyForReading = false;
            isDialed = false; // < 0 means the connection is no longer usable
        }

        internal ZitiConnection(ZitiIdentity identity, IntPtr nf_context, string serviceName)
        {
            this.Identity = identity;
            this.serviceName = serviceName;
            responses = new BlockingCollection<byte[]>(16);
        }

        private OnZitiConnected ozc;
        private OnZitiDataReceived ozdr;

        /// <summary>
        /// Creates a System.IO.Stream from a ZitiConnection
        /// </summary>
        /// <returns>A System.IO.Stream which and be written to and read from</returns>
        /// <exception cref="InvalidOperationException">Thrown when the connection has had Dial invoked</exception>
        internal void MarkAsStream()
        {
            isStream = true;
            if (isDialed)
            {
                throw new InvalidOperationException("Cannot AsStream on a connection that is already Dialed.");
            }
            
            //assign delegate to a local variable so that it is not elligable for GC
            ozc = (IntPtr nf_connection, int status) =>
            {
                lock (this)
                {
                    if (status < 0)
                    {
                        Ziti.Debug("connection not ready for writing: " + status);
                        this.connectionReadyStatus = (ZitiStatus)status;
                    }
                    else
                    {
                        Ziti.Debug("marking stream ready for writing");
                    }

                    Monitor.Pulse(this);
                    this.readyForWriting = true;
                }
            };

            //assign delegate to a local variable so that it is not elligable for GC
            ozdr = (IntPtr nf_connection, IntPtr rawData, int len) =>
            {
                byte[] data;
                if (len > 0)
                {
                    //need to copy the memory from c - into managed code and then write into the pipeline buffer
                    //it would be much better to just read from 
                    data = new byte[len];
                    Ziti.Debug("got bytes from ziti: " + len + " responses size: " + responses.Count);
                    Marshal.Copy(rawData, data, 0, len);
                    this.responses.Add(data);
                }
                else
                {
                    data = new byte[0];
                    this.responses.CompleteAdding();
                }
            };
            Ziti.Dial(nf_connection, serviceName, ozc, ozdr);
        }

        private const int DefaultBufferSize = 8192;
        static ArrayPool<byte> arrayPool = ArrayPool<byte>.Shared;

        AfterZitiDataWritten azdw;

        /// <summary>
        /// Writes the provided data over the NetFoundry network
        /// </summary>
        /// <param name="data">A buffer holding the information to be sent over the NetFoundry network</param>
        /// <param name="count">How many bytes of the buffer (starting at position 0) to write</param>
        /// <param name="onDataWritten">A callback to be invoked after the data is written to the NetFoundry network</param>
        /// <param name="context">Any object, provided back to the caller of this functionin the onDataWritten callback</param>
        public void Write(byte[] data, int count, OnDataWritten onDataWritten, object context)
        {
            if (isStream)
            {
                throw new InvalidOperationException("Cannot invoke Write on a connection that is already marked as a stream. Write to the stream.");
            }

            if (!readyForWriting)
            {
                lock (this)
                {
                    Ziti.Debug("waiting for connection to be ready for writing");
                    Monitor.Wait(this);

                    //if the connection is NOT ready - throw an exception
                    if (connectionReadyStatus != ZitiStatus.OK)
                    {
                        throw new ZitiException("Connection is not in a usable state: " + connectionReadyStatus.GetDescription());
                    }

                    Ziti.Debug("connection ready for writing");
                }
            }

            byte[] buff = arrayPool.Rent(count);
            Buffer.BlockCopy(data, 0, buff, 0, count);

            Ziti.Debug("writing to ziti:" + count + " bytes");

            //assign delegate to a local variable so that it is not elligable for GC
            azdw = (nf_connection, status, write_ctx) =>
            {
                if (onDataWritten != null)
                {
                    arrayPool.Return(buff);
                    ZitiStatus zStatus;
                    if (status < 0)
                    {
                        reset();
                        zStatus = (ZitiStatus)status;
                    }
                    else
                    {
                        zStatus = ZitiStatus.OK;
                    }
                    onDataWritten(zStatus, status, context);
                }
            };
            Ziti.Write(nf_connection, buff, count, azdw, NO_CONTEXT);
        }

        /// <summary>
        /// Establishes the necessary connecctivity and callbacks to send data through the NetFoundry network
        /// </summary>
        /// <param name="onConnected">Once the connection is established this callback is called</param>
        /// <param name="onDataReceived">Called each time data is received over the NetFoundry network</param>
        /// <exception cref="InvalidOperationException">Thrown when the ZitiConnection has had AsStream invoked previously</exception>
        public void Dial(OnConnected onConnected, OnDataReceived onDataReceived)
        {
            if (isStream)
            {
                throw new InvalidOperationException("Cannot Dial on a connection that is already marked as a stream.");
            }

            isDialed = true;

            OnConncetedCallback = onConnected;
            OnDataReceivedCallback = onDataReceived;

            //assign delegate to a local variable so that it is not elligable for GC
            ozc = (IntPtr nf_connection, int status) =>
            {
                lock (this)
                {
                    Ziti.Debug("marking dialed connection ready for writing");
                    Monitor.Pulse(this);
                    this.readyForWriting = true;
                }
                if (OnConncetedCallback != null)
                {
                    ZitiStatus zstat = (ZitiStatus)status;
                    if (zstat != ZitiStatus.OK)
                    {
                        throw new ZitiException("Connection is not in a usable state: " + zstat.GetDescription());
                    }
                    if (OnConncetedCallback != null)
                    {
                        OnConncetedCallback(this, zstat);
                    }
                }
            };

            //assign delegate to a local variable so that it is not elligable for GC
            ozdr = (IntPtr nf_connection, IntPtr rawData, int len) =>
            {
                byte[] data;
                if (len > 0)
                {
                    //need to copy the memory from c - into managed code and then write into the pipeline buffer
                    //it would be much better to just read from 
                    data = new byte[len];
                    Ziti.Debug("got bytes from ziti: " + len);
                    Marshal.Copy(rawData, data, 0, len);
                    Ziti.Debug("bytes moved to clr: " + len);
                }
                else
                {
                    data = new byte[0];
                }
                if (OnDataReceivedCallback != null)
                {
                    ZitiStatus zstat;
                    if (len < 0)
                    {
                        zstat = (ZitiStatus)len;
                        reset();
                    }
                    else
                    {
                        zstat = ZitiStatus.OK;
                    }
                    Ziti.Debug("sending bytes to callback: " + len);
                    OnDataReceivedCallback(zstat, data, len);
                    Ziti.Debug("callback completes: " + len);
                }
            };
            Ziti.Dial(nf_connection, serviceName, ozc, ozdr);
        }

        /// <summary>
        /// Closes the ZitiConnection and cleans up as needed
        /// </summary>
        public void Dispose()
        {
            Ziti.CloseConnection(this.nf_connection);
            FreeGCHandle(Ziti.GetContext(nf_connection));
        }

        internal bool CheckConnection()
        {
            if(this.connectionReadyStatus != ZitiStatus.OK)
            {
                throw new ZitiException("Connection is not in a usable state: " + connectionReadyStatus.GetDescription());
            }
            return true;
        }
    }
}
