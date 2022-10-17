using System;
using System.Diagnostics;
using System.IO;
using System.Linq;
using System.Runtime.InteropServices;
using System.Threading;
using System.Threading.Tasks;

namespace OpenZiti {
    /// <summary>
    /// A representation of a standard <see cref="System.IO.Stream"/> which utilizes the NetFoundry network
    /// </summary>
    public class ZitiStream : Stream {
        private static readonly NLog.Logger Logger = NLog.LogManager.GetCurrentClassLogger();

        private readonly ZitiConnection conn;
        private Native.ziti_write_cb azdw;
        private const int DefaultStreamPumpBufferSize = 8192;

        /// <summary>
        /// Creates a <see cref="ZitiStream"/> from the provided <see cref="ZitiConnection"/>
        /// </summary>
        /// <param name="conn">The <see cref="ZitiConnection"/> to create a <see cref="ZitiStream"/> from </param>
        public ZitiStream(ZitiConnection conn) {
            this.conn = conn;
            conn.MarkAsStream();
        }

        /// <summary>
        /// Indicates if the stream can be read from
        /// </summary>
        public override bool CanRead {
            get {
                return !conn.readyForReading;
            }
        }

        /// <summary>
        /// Seeking is not supported
        /// </summary>
        public override bool CanSeek {
            get {
                return false;
            }
        }

        /// <summary>
        /// indicates if the stream is ready for writing
        /// </summary>
        public override bool CanWrite {
            get {
                return !conn.readyForReading;
            }
        }

        /// <summary>
        /// unsupported - always returns 0
        /// </summary>
        public override long Length {
            get {
                return 0;
            }
        }

        /// <summary>
        /// unsupported - always returns 0
        /// </summary>
        /// <exception cref="NotImplementedException">Thrown when calling set</exception>
        public override long Position {
            get {
                return 0;
            }
            set {
                throw new NotImplementedException();
            }
        }

        /// <summary>
        /// Flushes bytes
        /// </summary>
        public override void Flush() {
            //do nothing but also don't throw an exception
        }

        private byte[] current = null;
        private int currentPos = 0;

        /// <summary>
        /// Reads data into the provided buffer
        /// </summary>
        /// <param name="buffer">The buffer to read data into</param>
        /// <param name="offset">The position in the bufer to begin appending data</param>
        /// <param name="count">The number of bytes to append</param>
        /// <returns>Returns the number of bytes read</returns>
        /// <exception cref="NotSupportedException">Thrown if the stream is not ready for reading</exception>
        /// <exception cref="ArgumentNullException">Thrown if the buffer provided is null</exception>
        /// <exception cref="ArgumentException">Thrown if the offset and count provided is larger than the buffer provided</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the offset or count provided is less than 0</exception>
        public override int Read(byte[] buffer, int offset, int count) {
            if (!CanRead) {
                throw new NotSupportedException("The stream is not ready for reading");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer cannot be null");
            }
            if (offset + count > buffer.Length) {
                throw new ArgumentException("offset + count is > buffer.Length");
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset cannot be < 0");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count cannot be < 0");
            }
            if (current != null) {
                return fillFromCache(buffer, offset, count);
            } else {
                while (!conn.responses.IsCompleted && conn.CheckConnection()) {
                    Logger.Debug("reading stream starts. responses size: " + conn.responses.Count);
                    if (conn.responses.TryTake(out var response, 500)) {
                        Logger.Debug("read from conn.responses size: " + conn.responses.Count);
                        //need to copy the bytes into the buffer...
                        if (count > response.Length) {
                            //good that means the provided byte array is large enough to accept all the bytes
                            Buffer.BlockCopy(response, currentPos, buffer, offset, response.Length);
                            return response.Length;
                        } else {
                            //means there will be 'leftover' bytes. need to store the leftovers/position and use on the next read
                            current = response;
                            return fillFromCache(buffer, offset, count);
                        }
                    } else {
                        //yield the thread and carry on
                        Logger.Debug("yielding thread in read");
                        Task.Yield();
                    }
                }
            }
            return 0;
        }

        private int fillFromCache(byte[] buffer, int offset, int count) {
            var remaining = current.Length - currentPos;
            //there are leftover bytes... fill the buffer with the leftover, then fill some more... rinse repeat...
            if (count >= remaining) {
                //means the buffer is bigger than the remaining - return the remaining (keep it simple)
                Buffer.BlockCopy(current, currentPos, buffer, offset, remaining);
                current = null;
                currentPos = 0;
                return remaining;
            } else {
                //more bytes are still left after this read... 
                Buffer.BlockCopy(current, currentPos, buffer, offset, count);
                currentPos += count;
                return count;
            }
        }

        /// <summary>
        /// unsupported
        /// </summary>
        public override long Seek(long offset, SeekOrigin origin) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// unsupported
        /// </summary>
        public override void SetLength(long value) {
            throw new NotImplementedException();
        }

        /// <summary>
        /// Writes the provided buffer over the ZitiNetwork
        /// </summary>
        /// <param name="buffer">The buffer containing data to write</param>
        /// <param name="offset">The position in the buffer to read from</param>
        /// <param name="count">The number of bytes to write from the buffer</param>
        /// <exception cref="NotSupportedException">Thrown if the stream is not ready for writing</exception>
        /// <exception cref="ArgumentNullException">Thrown if the buffer provided is null</exception>
        /// <exception cref="ArgumentException">Thrown if the offset and count provided is larger than the buffer provided</exception>
        /// <exception cref="ArgumentOutOfRangeException">Thrown if the offset or count provided is less than 0</exception>
        public override void Write(byte[] buffer, int offset, int count) {
            if (!CanWrite) {
                throw new NotSupportedException("The stream is not ready for writing");
            }
            if (buffer == null) {
                throw new ArgumentNullException("buffer is null");
            }
            if (offset + count > buffer.Length) {
                throw new ArgumentException("offset + count is > buffer.Length");
            }
            if (offset < 0) {
                throw new ArgumentOutOfRangeException("offset cannot be < 0");
            }
            if (count < 0) {
                throw new ArgumentOutOfRangeException("count cannot be < 0");
            }
            if (!conn.readyForWriting) {
                lock (conn) {
                    //waits until the connection is actually ready before writing
                    Monitor.Wait(conn);
                }
            }
            Logger.Debug("writing to ziti " + count + " bytes");

            //assign delegate to a local variable so that it is not eligible for GC
            azdw = (IntPtr nf_connection, int status, GCHandle write_ctx) => {
                lock (this) {
                    Logger.Debug("Unlocking write lock");
                    Monitor.Pulse(this);
                }
            };

            Native.API.ziti_write(conn.nativeConnection, buffer, count, azdw, ZitiUtil.NO_CONTEXT_PTR/*GCHandle*/);
            lock (this) {
                Logger.Debug("blocking until write is flushed to wire");
                Monitor.Wait(this);
            }
        }

        /// <summary>
        /// Disposes of the <see cref="ZitiStream"/>, cleaning up any retained resources
        /// </summary>
        /// <param name="disposing"></param>
        protected override void Dispose(bool disposing) {
            base.Dispose(disposing);
        }

        /// <summary>
        /// Asynchronously pumps data between the input <see cref="System.IO.Stream"/> and destination <see cref="System.IO.Stream"/>
        /// </summary>
        /// <param name="input">The input stream</param>
        /// <param name="destination">The destination stream</param>
        /// <returns>A <see cref="System.Threading.Tasks.Task"/> which is awaitable</returns>
        public static async Task PumpAsync(Stream input, Stream destination) {
            var count = DefaultStreamPumpBufferSize;
            var buffer = new byte[count];
            var numRead = await input.ReadAsync(buffer, 0, count).ConfigureAwait(false);
            while (numRead > 0) {
                await destination.WriteAsync(buffer, 0, numRead);
                numRead = await input.ReadAsync(buffer, 0, count).ConfigureAwait(false);
            }
        }

        /// <summary>
        /// Asynchronously pumps this <see cref="ZitiStream"/> to/from the destination <see cref="System.IO.Stream"/>
        /// </summary>
        /// <param name="destination"></param>
        /// <returns></returns>
        public async Task PumpAsync(Stream destination) {
            await Task.WhenAny(PumpAsync(this, destination), PumpAsync(destination, this));
        }
    }
}
