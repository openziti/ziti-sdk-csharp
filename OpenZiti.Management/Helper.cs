using OpenZiti.Generated;
using System.Text;

namespace OpenZiti.Management;

public class Helper
{
    internal ManagementAPI mapi { get; set; }

    public Helper(ManagementAPI mapi) {
        this.mapi = mapi;
    }

    public async Task DeleteServiceByName(string name) {
        var id = await FindServiceIdByNameAsync(name);
        if (id != null ) {
            await mapi.DeleteServiceAsync(id);
        }
    }

    public async Task<string?> FindServiceIdByNameAsync(string name) {
        var found = await mapi.ListServicesAsync(null, null, $"name = \"{name}\"", null, null);
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task DeleteConfigByNameAsync(string name) {
        var id = await FindConfigByNameAsync(name);
        if (id != null) {
            await mapi.DeleteConfigAsync(id);
        }
    }

    public async Task<string?> FindConfigByNameAsync(string name) {
        var found = await mapi.ListConfigsAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task DeleteServicePolicyByNameAsync(string name) {
        var id = await FindServicePolicyByNameAsync(name);
        if (id != null) {
            await mapi.DeleteServicePolicyAsync(id);
        }
    }

    public async Task<string?> FindServicePolicyByNameAsync(string name) {
        var found = await mapi.ListServicePoliciesAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task<string?> FindConfigTypeByNameAsync(string name) {
        var found = await mapi.ListConfigTypesAsync(null, null, $"name = \"{name}\"");
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }

    public async Task<bool> WaitForTerminatorAsync(string serviceName, TimeSpan timeout) {
        var timeoutAt = DateTime.Now + timeout;
        var svcId = await FindServiceIdByNameAsync(serviceName);
        if (svcId != null) {
            while (true) {
                if (timeoutAt < DateTime.Now) {
                    return false;
                }
                var found = await mapi.ListTerminatorsAsync(null, null, $"service = \"{svcId}\"");
                if (found.Data.Count > 0) {
                   return true;
                }
                Console.WriteLine("Waiting for termintaor...");
                await Task.Delay(100);
            }
        }
        return false;
    }

    public async Task<string?> FindIdentityByNameAsync(string name) {
        var found = await mapi.ListIdentitiesAsync(null, null, $"name = \"{name}\"", null, null);
        if (found != null && found.Data.Count > 0) {
            return found.Data[0].Id;
        }
        return null;
    }
}
