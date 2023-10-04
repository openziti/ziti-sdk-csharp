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

using Newtonsoft.Json;
using OpenZiti.Generated;
using OpenZiti.Management;
using System;
using System.IO;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenZiti.NET.Samples.Common {
    public class SampleSetup {
        private static readonly NLog.Logger Log = NLog.LogManager.GetCurrentClassLogger();
        
        internal ManagementApiHelper h = null;

        public SampleSetup() : this(new()){
            
        }
        public SampleSetup(ManagementApiHelper h) {
            this.h = h;
        }
        
        public async Task<IdentityDetail> BootstrapSampleIdentityAsync(string name, Attributes roles) {
            await h.DeleteIdentityByName(name);
            var identity = new IdentityCreate {
                Name = name,
                Enrollment = new Enrollment {
                    Ott = true,
                },
                RoleAttributes = roles
            };
            var createIdentityResult = await h.ManagementApi.CreateIdentityAsync(identity);
            var createdId = createIdentityResult.Data.Id;
            var details = await h.ManagementApi.DetailIdentityAsync(createdId);
            return details.Data;
        }

        public async Task<string> BootstrapAndEnrollIdentityAsync(string name, Attributes roles) {
            var detail = await BootstrapSampleIdentityAsync(name, roles);
            var clientJson = OpenZiti.API.EnrollIdentity(Encoding.ASCII.GetBytes(detail.Enrollment.Ott.Jwt));
            var outPath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
            File.WriteAllBytes(outPath, Encoding.UTF8.GetBytes(clientJson));
            return outPath;
        }

        public async Task AddAllEdgeRouterPolicy() {
            if (await h.FindEdgeRouterPolicyByNameAsync("all-endpoints-public-routers") == null) {
                var erp = new EdgeRouterPolicyCreate() {
                    Name = "all-endpoints-public-routers",
                    EdgeRouterRoles = new Roles() { "#all" },
                    IdentityRoles = new Roles() { "#all" }
                };
                await h.ManagementApi.CreateEdgeRouterPolicyAsync(erp);
            }
        }
        
        public async Task AddAllServiceEdgeRouterPolicy() {
            if (await h.FindServiceEdgeRouterPolicyByNameAsync("all-routers-all-services") == null) {
                var serp = new ServiceEdgeRouterPolicyCreate {
                    Name = "all-routers-all-services",
                    EdgeRouterRoles = new Roles() { "#all" },
                    ServiceRoles = new Roles() { "#all" }
                };
                await h.ManagementApi.CreateServiceEdgeRouterPolicyAsync(serp);
            }
        }
        
        public async Task<string> SetupWeatherExample(string svcName) {
            try {
                var svcRole = $"{svcName}.service.role";
                var clientIdentityName = $"{svcName}-client";
                var svcBindRole = $"{svcName}.binders";
                var svcDialRole = $"{svcName}.dialers";

                #region BootstrapClientIdentity
                // create weather client identity
                await h.DeleteIdentityByName(clientIdentityName);
                var rtn = await BootstrapAndEnrollIdentityAsync(clientIdentityName, new Attributes() { svcDialRole });
                #endregion

                #region CreateHostV1Config
                // create the host config if needed
                var hostV1ConfigName = $"{svcName}.config.host.v1";
                var hostV1ConfigTypeId = await h.FindConfigTypeByNameAsync("host.v1");
                var hostV1Config =
                    JsonConvert.DeserializeObject("{\"protocol\":\"tcp\", \"address\":\"wttr.in\",\"port\":443}");
                var foundHostV1ConfigId = await h.FindConfigIdByNameAsync(hostV1ConfigName);
                if (foundHostV1ConfigId == null) {
                    var createConfig = new ConfigCreate {
                        Name = hostV1ConfigName,
                        ConfigTypeId = hostV1ConfigTypeId,
                        Data = hostV1Config
                    };
                    //httpReqHandler.DoLogging = true;
                    var hostConfig = await h.ManagementApi.CreateConfigAsync(createConfig);
                    foundHostV1ConfigId = hostConfig.Data.Id;
                }
                #endregion

                #region CreateInterceptV1Config
                // create an intercept config if needed - not used in the weather example but useful from tunneler clients
                var interceptV1ConfigName = $"{svcName}.config.intercept.v1";
                var interceptConfigTypeId = await h.FindConfigTypeByNameAsync("intercept.v1");
                var interceptV1Config = JsonConvert.DeserializeObject(
                    "{\"protocols\":[\"tcp\"],\"addresses\":[\"wttr.in\"],\"portRanges\":[{\"low\":443, \"high\":443}]}");
                var foundInterceptConfigId = await h.FindConfigIdByNameAsync(interceptV1ConfigName);
                if (foundInterceptConfigId == null) {
                    var createConfig = new ConfigCreate {
                        Name = interceptV1ConfigName,
                        ConfigTypeId = interceptConfigTypeId,
                        Data = interceptV1Config
                    };
                    //httpReqHandler.DoLogging = true;
                    var interceptConfig = await h.ManagementApi.CreateConfigAsync(createConfig);
                    foundInterceptConfigId = interceptConfig.Data.Id;
                }
                #endregion

                #region CreateWeatherService
                //create the weather service
                await h.DeleteServiceByName(svcName);
                var createService = new ServiceCreate {
                    Name = svcName,
                    RoleAttributes = new string[] { svcRole },
                    Configs = new string[] { foundHostV1ConfigId, foundInterceptConfigId },
                    EncryptionRequired = true
                };
                await h.ManagementApi.CreateServiceAsync(createService);
                #endregion

                #region CreateDialPolicy
                // create the dial service policy
                var svcDialPolicyName = $"{svcName}.sp.dial";
                await h.DeleteServicePolicyByNameAsync(svcDialPolicyName);
                var createServicePolicy = new ServicePolicyCreate {
                    Name = svcDialPolicyName,
                    Type = DialBind.Dial,
                    IdentityRoles = new Roles() { $"#{svcDialRole}" },
                    ServiceRoles = new Roles() { $"#{svcRole}" }
                };
                await h.ManagementApi.CreateServicePolicyAsync(createServicePolicy);
                #endregion

                #region CreateBindPolicy
                // create the bind service policy
                var svcBindPolicyName = $"{svcName}.sp.bind";
                await h.DeleteServicePolicyByNameAsync($"{svcBindPolicyName}");
                createServicePolicy = new ServicePolicyCreate {
                    Name = svcBindPolicyName,
                    Type = DialBind.Bind,
                    IdentityRoles = new Roles() { $"#{svcBindRole}" },
                    ServiceRoles = new Roles() { $"#{svcRole}" }
                };
                await h.ManagementApi.CreateServicePolicyAsync(createServicePolicy);
                #endregion

                #region AssignBindRoleToRouterIdentity
                // assign the bind roleAttribute to the bind identity
                var erse = await h.ManagementApi.ListEdgeRoutersAsync(null, null, null, null, null);
                var ers = erse.Data;
                if (ers.Count == 1) {
                    // expected to have 1 router
                    var router = await h.FindIdentityByNameAsync(ers[0].Name);
                    await h.AddAndPatchIdentity(router, svcBindRole);
                } else {
                    throw new Exception("too many or too few routers defined. expected 1, found: " + ers.Count);
                }
                #endregion

                //now just wait and make sure the terminator exists before allowing the program to continue
                if (!await h.WaitForTerminatorAsync(svcName, TimeSpan.FromSeconds(20))) {
                    throw new Exception("Error while waiting for terminator");
                }

                return rtn;
            } catch (ApiException<ApiErrorEnvelope> e) {
                Log.Info($"{e.Result.Error.Code}");
                Log.Info($"{e.Result.Error.Message}");
                Log.Info($"{e.Result.Error.Cause.Reason}");
                Log.Info($"{e.Message}");
                throw;
            } catch (ApiException e) {
                Log.Info($"{e.Message}");
                if (e.InnerException != null) {
                    Log.Info($"{e.InnerException.Message}");
                }
                throw;
            }
        }
        
        public async Task<string> SetupHostedExample(string svcName) {
            try {
                var svcRole = $"{svcName}.service.role";
                var serverIdentityName = $"{svcName}-server";
                var svcBindRole = $"{svcName}.binders";
                var svcDialRole = $"{svcName}.dialers";

                #region BootstrapHostingIdentity
                // create hosted identity
                await h.DeleteIdentityByName(serverIdentityName);
                var rtn = await BootstrapAndEnrollIdentityAsync(serverIdentityName, new Attributes() { svcBindRole });
                #endregion

                #region CreateServiceToHost
                //create the hosted service
                await h.DeleteServiceByName(svcName);
                var createService = new ServiceCreate {
                    Name = svcName,
                    RoleAttributes = new string[] { svcRole },
                    Configs = new string[] { /*not required in this example*/ },
                    EncryptionRequired = true
                };
                await h.ManagementApi.CreateServiceAsync(createService);
                #endregion

                #region CreateDialPolicy
                // create the dial service policy
                var svcDialPolicyName = $"{svcName}.sp.dial";
                await h.DeleteServicePolicyByNameAsync(svcDialPolicyName);
                var createServicePolicy = new ServicePolicyCreate {
                    Name = svcDialPolicyName,
                    Type = DialBind.Dial,
                    IdentityRoles = new Roles() { $"#{svcDialRole}" },
                    ServiceRoles = new Roles() { $"#{svcRole}" }
                };
                await h.ManagementApi.CreateServicePolicyAsync(createServicePolicy);
                #endregion

                #region CreateBindPolicy
                // create the bind service policy
                var svcBindPolicyName = $"{svcName}.sp.bind";
                await h.DeleteServicePolicyByNameAsync($"{svcBindPolicyName}");
                createServicePolicy = new ServicePolicyCreate {
                    Name = svcBindPolicyName,
                    Type = DialBind.Bind,
                    IdentityRoles = new Roles() { $"#{svcBindRole}" },
                    ServiceRoles = new Roles() { $"#{svcRole}" }
                };
                await h.ManagementApi.CreateServicePolicyAsync(createServicePolicy);
                #endregion
                return rtn;
            } catch (ApiException<ApiErrorEnvelope> e) {
                Log.Info($"{e.Result.Error.Code}");
                Log.Info($"{e.Result.Error.Message}");
                Log.Info($"{e.Result.Error.Cause.Reason}");
                Log.Info($"{e.Message}");
                throw;
            } catch (ApiException e) {
                Log.Info($"{e.Message}");
                if (e.InnerException != null) {
                    Log.Info($"{e.InnerException.Message}");
                }
                throw;
            }
        }

        public async Task<string> SetupHostedClientExample(string svcName) {
            try {
                var clientIdentityName = $"{svcName}-client";
                var svcDialRole = $"{svcName}.dialers";

                #region BootstrapClientIdentity
                // create weather client identity
                await h.DeleteIdentityByName(clientIdentityName);
                var rtn = await BootstrapAndEnrollIdentityAsync(clientIdentityName, new Attributes() { svcDialRole });
                #endregion

                return rtn;
            } catch (ApiException<ApiErrorEnvelope> e) {
                Log.Info($"{e.Result.Error.Code}");
                Log.Info($"{e.Result.Error.Message}");
                Log.Info($"{e.Result.Error.Cause.Reason}");
                Log.Info($"{e.Message}");
                throw;
            } catch (ApiException e) {
                Log.Info($"{e.Message}");
                if (e.InnerException != null) {
                    Log.Info($"{e.InnerException.Message}");
                }
                throw;
            }
        }
    }
}
