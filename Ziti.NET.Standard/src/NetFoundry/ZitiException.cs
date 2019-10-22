using System;

namespace NetFoundry
{
    /// <summary>
    /// Represents a Ziti-specific exception
    /// </summary>
    public class ZitiException : Exception
    {
        /// <summary>
        /// The basic constructor for creating a ZitiException
        /// </summary>
        /// <param name="message">The message</param>
        public ZitiException(string message) : base(message) { }
    }
}
