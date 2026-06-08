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
using System.Runtime.InteropServices;
using System.Text;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;
using Newtonsoft.Json;

using OpenZiti;
using OpenZiti.Generated;

namespace E2ETest;

// Minimal, dependency-light overlay configuration for the e2e traffic tests. It does exactly what
// OpenZiti.NET.Samples' SampleSetup/ManagementApiHelper do for the "hosted" case, but inlined so the
// publish gate does not drag in AspNetCore/EF/Petstore. It talks to the controller stood up by
// `ziti edge quickstart` (default https://localhost:1280, admin/admin), all overridable via env vars.
internal sealed class OverlaySetup
{
    private readonly ManagementAPI _mapi;

    private OverlaySetup(ManagementAPI mapi) => _mapi = mapi;

    // Step narration so a test run reads like a story (configuring overlay, enrolling, binding, dialing...).
    // Shown by `dotnet test` at detailed verbosity (run-e2e-test.ps1 turns that on).
    internal static void Say(string message) => Console.WriteLine($">>> {message}");

    public static async Task<OverlaySetup> ConnectAsync()
    {
        EnsureZitiOnPath();

        var baseUrl = EnvOr("ZITI_BASEURL", "localhost:1280");
        var user = EnvOr("ZITI_USERNAME", "admin");
        var pass = EnvOr("ZITI_PASSWORD", "admin");
        Say($"[overlay] connecting to controller https://{baseUrl} as '{user}'");

        // The quickstart controller uses a self-signed PKI; accept it for the test only.
        var handler = new HttpClientHandler
        {
            ClientCertificateOptions = ClientCertificateOption.Manual,
            ServerCertificateCustomValidationCallback = (_, _, _, _) => true,
        };
        var http = new HttpClient(handler);
        var mapi = new ManagementAPI(http) { BaseUrl = $"https://{baseUrl}/edge/management/v1" };

        var auth = new Authenticate { Username = user, Password = pass };
        var detail = await mapi.AuthenticateAsync(auth, Method.Password);
        http.DefaultRequestHeaders.Add("zt-session", detail.Data.Token);
        Say("[overlay] authenticated to the management API");

        return new OverlaySetup(mapi);
    }

    private static string EnvOr(string name, string fallback)
    {
        var v = Environment.GetEnvironmentVariable(name);
        return string.IsNullOrEmpty(v) ? fallback : v;
    }

    // These e2e tests need a live ziti overlay, which means the `ziti` CLI must be on PATH (CI puts it there via
    // the openziti/ziti/setup-cli action; locally `scripts/run-e2e-test.ps1` stands up the overlay for you).
    // If ziti is missing, skip with an actionable message rather than failing with an opaque connection error.
    private static void EnsureZitiOnPath()
    {
        var exe = RuntimeInformation.IsOSPlatform(OSPlatform.Windows) ? "ziti.exe" : "ziti";
        var path = Environment.GetEnvironmentVariable("PATH") ?? string.Empty;
        var onPath = path.Split(Path.PathSeparator).Any(dir =>
        {
            try { return !string.IsNullOrEmpty(dir) && File.Exists(Path.Combine(dir, exe)); }
            catch { return false; }
        });

        if (!onPath)
        {
            Assert.Inconclusive(
                "The 'ziti' CLI is not on your PATH, so no overlay is available for these e2e tests. Put ziti on " +
                "your PATH (install via https://get.openziti.io/quick/getZiti.ps1), then either run " +
                "`ziti edge quickstart` yourself or just run scripts/run-e2e-test.ps1, which stands up the overlay " +
                "and runs these tests for you.");
        }
    }

    // Ensure every identity can reach every router and every router can carry every service. The
    // quickstart usually creates these, but creating them again is idempotent and removes a flaky
    // dependency on quickstart internals.
    public async Task EnsureRouterPoliciesAsync()
    {
        Say("[overlay] ensuring router policies (#all endpoints <-> #all routers <-> #all services)");
        if (await FindAsync(n => _mapi.ListEdgeRouterPoliciesAsync(null, null, n), "all-endpoints-public-routers") == null)
        {
            await _mapi.CreateEdgeRouterPolicyAsync(new EdgeRouterPolicyCreate
            {
                Name = "all-endpoints-public-routers",
                EdgeRouterRoles = new Roles { "#all" },
                IdentityRoles = new Roles { "#all" },
            });
        }
        if (await FindAsync(n => _mapi.ListServiceEdgeRouterPoliciesAsync(null, null, n), "all-routers-all-services") == null)
        {
            await _mapi.CreateServiceEdgeRouterPolicyAsync(new ServiceEdgeRouterPolicyCreate
            {
                Name = "all-routers-all-services",
                EdgeRouterRoles = new Roles { "#all" },
                ServiceRoles = new Roles { "#all" },
            });
        }
    }

    // Create a service plus dial+bind policies, returning enrolled identity files for a dialer and a
    // binder. Re-running deletes any prior objects with the same names so tests are repeatable.
    public async Task<(string binderIdFile, string dialerIdFile)> SetupServiceAsync(string svcName)
    {
        var svcRole = $"{svcName}.service.role";
        var bindRole = $"{svcName}.binders";
        var dialRole = $"{svcName}.dialers";

        var binderIdFile = await BootstrapAndEnrollAsync($"{svcName}-binder", bindRole);
        var dialerIdFile = await BootstrapAndEnrollAsync($"{svcName}-dialer", dialRole);

        Say($"[overlay] creating service '{svcName}' (+ dial and bind service policies)");
        await DeleteServiceByNameAsync(svcName);
        await _mapi.CreateServiceAsync(new ServiceCreate
        {
            Name = svcName,
            RoleAttributes = new[] { svcRole },
            Configs = Array.Empty<string>(),
            EncryptionRequired = true,
        });

        await RecreatePolicyAsync($"{svcName}.sp.dial", DialBind.Dial, dialRole, svcRole);
        await RecreatePolicyAsync($"{svcName}.sp.bind", DialBind.Bind, bindRole, svcRole);

        return (binderIdFile, dialerIdFile);
    }

    // Like SetupServiceAsync, but also attaches an intercept.v1 config so an SDK client can dial the service by
    // an address (e.g. http.server.ziti:80) and have the SDK map it to the service. The service is still
    // SDK-bound (no host.v1, no router bind role), exactly like the kestrel sample.
    public async Task<(string binderIdFile, string dialerIdFile)> SetupInterceptServiceAsync(
        string svcName, string interceptHost, int port)
    {
        var svcRole = $"{svcName}.service.role";
        var bindRole = $"{svcName}.binders";
        var dialRole = $"{svcName}.dialers";

        var binderIdFile = await BootstrapAndEnrollAsync($"{svcName}-binder", bindRole);
        var dialerIdFile = await BootstrapAndEnrollAsync($"{svcName}-dialer", dialRole);

        var interceptCfgId = await RecreateInterceptConfigAsync($"{svcName}.intercept.v1", interceptHost, port);

        Say($"[overlay] creating intercept service '{svcName}' mapping {interceptHost}:{port} -> the service");
        await DeleteServiceByNameAsync(svcName);
        await _mapi.CreateServiceAsync(new ServiceCreate
        {
            Name = svcName,
            RoleAttributes = new[] { svcRole },
            Configs = new[] { interceptCfgId },
            EncryptionRequired = true,
        });

        await RecreatePolicyAsync($"{svcName}.sp.dial", DialBind.Dial, dialRole, svcRole);
        await RecreatePolicyAsync($"{svcName}.sp.bind", DialBind.Bind, bindRole, svcRole);

        return (binderIdFile, dialerIdFile);
    }

    // Create an OTT identity and enroll it through the SDK (API.EnrollIdentity), returning the strong identity
    // file. Used directly by the enrollment test to prove enroll + load works end to end.
    public Task<string> EnrollNewIdentityAsync(string name) => BootstrapAndEnrollAsync(name);

    private async Task<string> RecreateInterceptConfigAsync(string name, string host, int port)
    {
        var existing = await FindAsync(n => _mapi.ListConfigsAsync(null, null, n), name);
        if (existing != null) await _mapi.DeleteConfigAsync(existing);

        var typeId = await FindAsync(n => _mapi.ListConfigTypesAsync(null, null, n), "intercept.v1");
        var data = JsonConvert.DeserializeObject(
            "{\"protocols\":[\"tcp\"],\"addresses\":[\"" + host + "\"]," +
            "\"portRanges\":[{\"low\":" + port + ",\"high\":" + port + "}]}");

        var created = await _mapi.CreateConfigAsync(new ConfigCreate
        {
            Name = name,
            ConfigTypeId = typeId,
            Data = data,
        });
        return created.Data.Id;
    }

    private async Task RecreatePolicyAsync(string name, DialBind type, string identityRole, string svcRole)
    {
        var existing = await FindAsync(n => _mapi.ListServicePoliciesAsync(null, null, n), name);
        if (existing != null) await _mapi.DeleteServicePolicyAsync(existing);
        await _mapi.CreateServicePolicyAsync(new ServicePolicyCreate
        {
            Name = name,
            Type = type,
            IdentityRoles = new Roles { $"#{identityRole}" },
            ServiceRoles = new Roles { $"#{svcRole}" },
        });
    }

    private async Task<string> BootstrapAndEnrollAsync(string name, params string[] roles)
    {
        Say($"[overlay] enrolling identity '{name}'" + (roles.Length > 0 ? $" (roles: {string.Join(", ", roles)})" : ""));
        var existingId = await FindAsync(n => _mapi.ListIdentitiesAsync(null, null, n, null, null), name);
        if (existingId != null) await _mapi.DeleteIdentityAsync(existingId);

        var attrs = new Attributes();
        foreach (var r in roles) attrs.Add(r);

        var created = await _mapi.CreateIdentityAsync(new IdentityCreate
        {
            Name = name,
            Enrollment = new Enrollment { Ott = true },
            RoleAttributes = attrs,
        });
        var detail = await _mapi.DetailIdentityAsync(created.Data.Id);

        // Enroll the OTT JWT through the SDK (exercises the native lib) and persist the strong identity.
        var idJson = API.EnrollIdentity(Encoding.ASCII.GetBytes(detail.Data.Enrollment.Ott.Jwt));
        var outPath = Path.Combine(Path.GetTempPath(), $"{name}-{Guid.NewGuid():N}.json");
        File.WriteAllText(outPath, idJson);
        return outPath;
    }

    // Poll until the service has at least one terminator (i.e. a binder has registered and the service
    // is actually dialable), or the timeout elapses. Returns false on timeout.
    public async Task<bool> WaitForTerminatorAsync(string svcName, TimeSpan timeout)
    {
        Say($"[overlay] waiting for a terminator on '{svcName}' (binder registered, service is dialable)");
        var svcId = await FindAsync(n => _mapi.ListServicesAsync(null, null, n, null, null), svcName);
        if (svcId == null) return false;

        var deadline = DateTime.UtcNow + timeout;
        while (DateTime.UtcNow < deadline)
        {
            var found = await _mapi.ListTerminatorsAsync(null, null, $"service = \"{svcId}\"");
            if (found.Data.Count > 0) return true;
            await Task.Delay(250);
        }
        return false;
    }

    private async Task DeleteServiceByNameAsync(string name)
    {
        var id = await FindAsync(n => _mapi.ListServicesAsync(null, null, n, null, null), name);
        if (id != null) await _mapi.DeleteServiceAsync(id);
    }

    // Run a "name = ..." filtered list and return the first matching object id, or null.
    private static async Task<string> FindAsync<T>(Func<string, Task<T>> list, string name)
        where T : class
    {
        dynamic found = await list($"name = \"{name}\"");
        if (found != null && found.Data != null && found.Data.Count > 0)
        {
            return (string)found.Data[0].Id;
        }
        return null;
    }
}
