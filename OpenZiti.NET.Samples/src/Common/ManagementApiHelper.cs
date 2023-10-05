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
using System.IO;
using System.Linq;
using System.Net.Http;
using System.Text;
using System.Threading.Tasks;

using OpenZiti.Generated;
using OpenZiti.Debugging;

namespace OpenZiti.Management;

public class ManagementApiHelper
{
    private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
    public static string[] EmptyRoleFilter = new string[] { };
    
    public string BaseUrl { get; private set; }

    private ManagementAPI _mapi;
    public ManagementAPI ManagementApi {
        get {
            return _mapi;
        }
    }

    public ManagementApiHelper(string baseUrl) {
        this.BaseUrl = baseUrl;
        setManagementApi().Wait();
    }
    public ManagementApiHelper(ManagementAPI mapi) {
        this._mapi = mapi;
    }

    public ManagementApiHelper() {
        setManagementApi().Wait();
    }

    private async Task setManagementApi() {
        Authenticate auth = new Authenticate();
        Method method = Method.Password;
        auth.Username = Environment.GetEnvironmentVariable("ZITI_USERNAME");
        auth.Password = Environment.GetEnvironmentVariable("ZITI_PASSWORD");
        if (auth.Username is null or "") {
            auth.Username = "admin";
            Log.Info("Using DEFAULT USERNAME (set env var ZITI_USERNAME to override): " + auth.Username);
        }
        if (auth.Password is null or "") {
            auth.Password = "admin";
            Log.Info("Using DEFAULT PASSWORD (set env var ZITI_PASSWORD to override): " + auth.Password);
        }
        if(BaseUrl is null or "") {
            BaseUrl = Environment.GetEnvironmentVariable("ZITI_BASEURL");
            if (BaseUrl is null or "") {
                BaseUrl = "localhost:1280";
                Log.Info("Using DEFAULT url (set env var ZITI_BASEURL to override): " + BaseUrl);
            }
        }

        var handler = new HttpClientHandler() {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => {
                    return true;
                }
        };
        var httpReqHandler = new LoggingHandler(handler);
        var nonValidatingHttpClient = new HttpClient(httpReqHandler);
        _mapi = new ManagementAPI(nonValidatingHttpClient) {
            BaseUrl = $"https://{BaseUrl}/edge/management/v1"
        };

        var detail = await _mapi.AuthenticateAsync(auth, method);
        nonValidatingHttpClient.DefaultRequestHeaders.Add("zt-session", detail.Data.Token);
    }
    
    public void Enroll(string pathToEnrollmentToken, string outputPath) {
        var strongIdentity = API.EnrollIdentityFile(pathToEnrollmentToken);
        File.WriteAllBytes($"{outputPath}", Encoding.UTF8.GetBytes(strongIdentity));
        Log.Info($"Strong identity enrolled successfully. File saved to: {outputPath}");
    }
    
    public async Task DeleteIdentityByName(string name) {
        var id = await FindIdentityIdByNameAsync(name);
        if (id != null ) {
            await _mapi.DeleteIdentityAsync(id);
        }
    }
    
    public async Task DeleteServiceByName(string name) {
        var id = await FindServiceIdByNameAsync(name);
        if (id != null ) {
            await _mapi.DeleteServiceAsync(id);
        }
    }

    public async Task<string> FindServiceIdByNameAsync(string name) {
        var found = await _mapi.ListServicesAsync(null, null, $"name = \"{name}\"", null, null);
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task DeleteConfigByNameAsync(string name) {
        var id = await FindConfigIdByNameAsync(name);
        if (id != null) {
            await _mapi.DeleteConfigAsync(id);
        }
    }

    public async Task<string> FindConfigIdByNameAsync(string name) {
        var found = await _mapi.ListConfigsAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task DeleteServicePolicyByNameAsync(string name) {
        var id = await FindServicePolicyByNameAsync(name);
        if (id != null) {
            await _mapi.DeleteServicePolicyAsync(id);
        }
    }

    public async Task<string> FindServicePolicyByNameAsync(string name) {
        var found = await _mapi.ListServicePoliciesAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task<string> FindConfigTypeByNameAsync(string name) {
        var found = await _mapi.ListConfigTypesAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task<bool> WaitForTerminatorAsync(string serviceName, TimeSpan timeout) {
        Log.Info("Waiting for terminator...");
        var now = DateTime.Now;
        var timeoutAt = now + timeout;
        var svcId = await FindServiceIdByNameAsync(serviceName);
        if (svcId != null) {
            while (true) {
                if (timeoutAt < DateTime.Now) {
                    return false;
                }
                var found = await _mapi.ListTerminatorsAsync(null, null, $"service = \"{svcId}\"");
                if (found.Data.Count > 0) {
                    Log.Info("Waiting for terminator... took: " + (DateTime.Now - now).TotalMilliseconds + "ms");
                   return true;
                }
                await Task.Delay(100);
            }
        }
        return false;
    }

    public async Task<string> FindIdentityIdByNameAsync(string name) {
        var found = await _mapi.ListIdentitiesAsync(null, null, $"name = \"{name}\"", null, null);
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }
    public async Task<IdentityDetail> FindIdentityByNameAsync(string name) {
        var found = await _mapi.ListIdentitiesAsync(null, null, $"name = \"{name}\"", null, null);
        if (found != null && found.Data.Count > 0) {
            return found.Data[0];
        }
        return null;
    }
    
    public async Task<string> FindEdgeRouterPolicyByNameAsync(string name) {
        var found = await _mapi.ListEdgeRouterPoliciesAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }
    public async Task<string> FindServiceEdgeRouterPolicyByNameAsync(string name) {
        var found = await _mapi.ListServiceEdgeRouterPoliciesAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task AddRoleAttributeToIdentityByName(string identityName, string role) {
        var routerIdentityId = await FindIdentityIdByNameAsync(identityName);
        var routerIdentityEnv = await _mapi.DetailIdentityAsync(routerIdentityId);
        var routerIdentity = routerIdentityEnv.Data;

        var idAttrs = new Attributes();
        if (routerIdentity.RoleAttributes != null && routerIdentity.RoleAttributes.Contains($"{role}")) {
            Log.Info($"Router identity already has bind attribute: #{role}");
        } else {
            IdentityPatch patch = new IdentityPatch();
            patch.RoleAttributes = new Attributes();
            foreach (var attr in routerIdentity?.RoleAttributes ?? Enumerable.Empty<string>()) {
                patch.RoleAttributes.Add(attr);
            }
            patch.RoleAttributes.Add(role);
            await _mapi.PatchIdentityAsync(patch, routerIdentityId);
        }
    }

    public async Task AddAndPatchIdentity(IdentityDetail id, string role) {
        IdentityPatch patch = new IdentityPatch();
        patch.RoleAttributes = new Attributes();
        foreach (var attr in id?.RoleAttributes ?? Enumerable.Empty<string>()) {
            patch.RoleAttributes.Add(attr);
        }
        patch.RoleAttributes.Add(role);
        await ManagementApi.PatchIdentityAsync(patch, id.Id);
    }
}

public static class IdentityDetailExtensions {
    public static void AppendRole(this IdentityDetail id, string role) {
        if (id.RoleAttributes != null && ! id.RoleAttributes.Contains($"{role}")) {
            foreach (var attr in id?.RoleAttributes ?? Enumerable.Empty<string>()) {
                id.RoleAttributes.Add(attr);
            }
        }
    }
}
