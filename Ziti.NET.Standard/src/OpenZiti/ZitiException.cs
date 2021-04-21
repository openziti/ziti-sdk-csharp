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

namespace OpenZiti
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

        /// <summary>
        /// The basic constructor for creating a ZitiException
        /// </summary>
        /// <param name="message">The message</param>
        public ZitiException(ZitiStatus status) : base(status.GetDescription()) { }
    }
}
