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
using System.Net;
using System.Text;
using System.Threading.Tasks;

namespace OpenZiti;


public sealed class ZitiEndPoint : EndPoint {
    public string ServiceName { get; }
    public string Terminator { get; }
    public string Identity { get; }

    public ZitiEndPoint(string identity, string serviceName, string terminator = null) {
        if (string.IsNullOrWhiteSpace(serviceName))
            throw new System.ArgumentException("serviceName is required", nameof(serviceName));

        ServiceName = serviceName;
        Terminator = terminator;
        Identity = identity;
    }

    public override string ToString() {
        if (!string.IsNullOrEmpty(Terminator)) {
            return $"ziti:{ServiceName}@{Terminator}";
        }

        return $"ziti:{ServiceName}";
    }

}
