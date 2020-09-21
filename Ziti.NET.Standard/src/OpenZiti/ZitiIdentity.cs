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

namespace OpenZiti
{
    public class ZitiIdentity
    {
        /// <summary>
        /// The options used to initialize a <see cref="ZitiIdentity"/>
        /// </summary>
        public ZitiOptions InitOptions { get; private set; }

        /// <summary>
        /// Allows adding arbitrary context to the <see cref="ZitiIdentity"/>
        /// </summary>
        public object Context { get; private set; }

        /// <summary>
        /// Creates a new <see cref="ZitiIdentity"/> with the provided <see cref="ZitiOptions"/>
        /// </summary>
        /// <param name="InitOptions"></param>
        public ZitiIdentity(ZitiOptions InitOptions)
        {
            this.InitOptions = InitOptions;
            ZitiUtil.CheckStatus(Native.API.ziti_init_opts(InitOptions.ToNative(), Native.API.z4d_default_loop(), ZitiUtil.NO_CONTEXT));
        }
    }
}
