/*
Copyright NetFoundry Inc.

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
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading;
using System.Threading.Tasks;

namespace OpenZiti.Debugging {
    public class LoggingHandler : DelegatingHandler {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();

        public bool DoLogging { get; set; }

        public LoggingHandler(HttpMessageHandler innerHandler)
            : base(innerHandler) {
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            if (DoLogging) {
                Log.Info("Request:");
                Log.Info(request.ToString());
                if (request.Content != null) {
                    Log.Info(await request.Content.ReadAsStringAsync());
                }
            }
            HttpResponseMessage response = await base.SendAsync(request, cancellationToken);
            if (DoLogging) {
                Log.Info("Response:");
                Log.Info(response.ToString());
                if (response.Content != null) {
                    Log.Info(await response.Content.ReadAsStringAsync());
                }
                Log.Info("===============================================================================================");
            }
            return response;
        }
    }
}
