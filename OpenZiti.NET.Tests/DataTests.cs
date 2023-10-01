using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenZiti.Generated;
using System.Data;
using System.Text;
using System.IO;

using MLog=Microsoft.Extensions.Logging;
using OpenZiti.Management;
using Newtonsoft.Json;
using NLog.Config;
using NLog.Targets;
using NLog;
using System.Threading;
using OpenZiti.Debugging;

namespace OpenZiti.NET.Tests {
    [TestClass]
    public class DataTests {

        [ClassInitialize]
#pragma warning disable IDE0060 // Remove unused parameter
        public static void ClassInitialize(TestContext context) {
            // Code to run once before all test methods in the class
            LoggingHelper.SimpleConsoleLogging(MLog.LogLevel.Trace);

            OpenZiti.API.NativeLogger = OpenZiti.API.DefaultNativeLogFunction;
            OpenZiti.API.InitializeZiti();
            //to see the logs from the Native SDK, set the log level
            OpenZiti.API.SetLogLevel(ZitiLogLevel.TRACE);
        }
#pragma warning restore IDE0060 // Remove unused parameter

        public static Logger SimpleConsoleLogging(MLog.LogLevel lvl) {
            NLog.LogLevel logLevel = NLog.LogLevel.Fatal;
            logLevel = lvl switch {
                MLog.LogLevel.Trace => NLog.LogLevel.Trace,
                MLog.LogLevel.Debug => NLog.LogLevel.Debug,
                MLog.LogLevel.Information => NLog.LogLevel.Info,
                MLog.LogLevel.Warning => NLog.LogLevel.Warn,
                MLog.LogLevel.Error => NLog.LogLevel.Error,
                MLog.LogLevel.Critical => NLog.LogLevel.Fatal,
                MLog.LogLevel.None => NLog.LogLevel.Error,// Default to Info if the mapping is not set.
                _ => NLog.LogLevel.Info,// Default to Info if the mapping is not found.
            };
            var config = new LoggingConfiguration();
            var logconsole = new ConsoleTarget("logconsole") {
                Layout = "[${date:format=yyyy-MM-ddTHH\\:mm\\:ss.fff}Z] ${level:uppercase=true:padding=5}\t${message}\t${exception:format=tostring}",
            };

            // Rules for mapping loggers to targets            
            config.AddRule(logLevel, NLog.LogLevel.Fatal, logconsole);

            // Apply config           
            LogManager.Configuration = config;
            LogManager.ReconfigExistingLoggers();
            return LogManager.GetLogger("console");
        }


        [TestMethod]
        public async Task TestWeatherAsync() {
            try {
                var l = SimpleConsoleLogging(MLog.LogLevel.Trace);

                OpenZiti.API.NativeLogger = OpenZiti.API.DefaultNativeLogFunction;
                OpenZiti.API.InitializeZiti();
                //to see the logs from the Native SDK, set the log level
                OpenZiti.API.SetLogLevel(ZitiLogLevel.DEBUG);
                Console.Clear();

                Authenticate auth = new Authenticate();
                Method method = Method.Password;
                auth.Username = Environment.GetEnvironmentVariable("ZITI_USERNAME");
                auth.Password = Environment.GetEnvironmentVariable("ZITI_PASSWORD");


                var handler = new HttpClientHandler {
                    ClientCertificateOptions = ClientCertificateOption.Manual,
                    ServerCertificateCustomValidationCallback =
                    (httpRequestMessage, cert, cetChain, policyErrors) => {
                        return true;
                    }
                };

                var httpReqHandler = new OpenZiti.Debugging.LoggingHandler(handler);

                var nonValidatingHttpClient = new HttpClient(httpReqHandler);
                ManagementAPI mapi = new ManagementAPI(nonValidatingHttpClient) {
                    BaseUrl = "https://appetizer.openziti.io:8441/edge/management/v1"
                };

                CurrentApiSessionDetailEnvelope detail = await mapi.AuthenticateAsync(auth, method);
                Console.WriteLine(detail.Data.Id);
                nonValidatingHttpClient.DefaultRequestHeaders.Add("zt-session", detail.Data.Token); // Example header

                var emptyRoleFilter = new string[] { };
                var ids = await mapi.ListIdentitiesAsync(100, 0, "name = \"test_id\"", emptyRoleFilter, "");
                foreach (var id in ids.Data) {
                    Console.WriteLine(id.Name);
                    await mapi.DeleteIdentityAsync(id.Id);
                }

                var h = new Helper(mapi);
                var obj1 = JsonConvert.DeserializeObject("{'protocol':'tcp', 'address':'wttr.in','port':443}");

                var svcName = $"test-weather-svc";
                var testServices = "test-services-role";
                //create the weather host v1 config
                var cfgId = await h.FindConfigTypeByNameAsync("host.v1");
                await h.DeleteConfigByNameAsync($"{svcName}.config.host.v1");
                var createConfig = new ConfigCreate {
                    Name = $"{svcName}.config.host.v1",
                    ConfigTypeId = cfgId,
                    Data = obj1
                };
                httpReqHandler.DoLogging = true;
                var createdConfig = await mapi.CreateConfigAsync(createConfig);
                httpReqHandler.DoLogging = false;

                //create the weather service
                await h.DeleteServiceByName(svcName);
                var createService = new ServiceCreate {
                    Name = svcName,
                    RoleAttributes = new string[] { $"{testServices}" },
                    Configs = new string[] { createdConfig.Data.Id },
                    EncryptionRequired = true
                };
                await mapi.CreateServiceAsync(createService);

                //create the service policies
                var serviceRoles = new Roles() { $"#{testServices}" };
                var dialRoles = new Roles() { $"#{svcName}.dialers" };
                var bindRoles = new Roles() { $"#{svcName}.binders" };

                await h.DeleteServicePolicyByNameAsync($"{svcName}.sp.dial");
                var createServicePolicy = new ServicePolicyCreate {
                    Name = $"{svcName}.sp.dial",
                    Type = DialBind.Dial,
                    IdentityRoles = dialRoles,
                    ServiceRoles = serviceRoles
                };
                await mapi.CreateServicePolicyAsync(createServicePolicy);

                await h.DeleteServicePolicyByNameAsync($"{svcName}.sp.bind");
                createServicePolicy = new ServicePolicyCreate {
                    Name = $"{svcName}.sp.bind",
                    Type = DialBind.Bind,
                    IdentityRoles = bindRoles,
                    ServiceRoles = serviceRoles
                };
                await mapi.CreateServicePolicyAsync(createServicePolicy);

                var erse = await mapi.ListEdgeRoutersAsync(null, null, null, null, null);
                var ers = erse.Data;
                string routerId = "";
                if (ers.Count == 1) {
                    // expected to have 1 router
                    routerId = ers[0].Id;
                } else {
                    throw new Exception("too many routers defined. expected 1");
                }

                var identity = new IdentityCreate {
                    Name = "test_id",
                    Enrollment = new Enrollment {
                        Ott = true,
                    },
                    RoleAttributes = new Attributes { $"{svcName}.dialers" }
                };
                var createIdentityResult = await mapi.CreateIdentityAsync(identity);
                var createdId = createIdentityResult.Data.Id;
                Console.WriteLine("success: " + createdId);

                var details = await mapi.DetailIdentityAsync(createdId);

                //enroll the new identity
                string zitiId = OpenZiti.API.EnrollIdentity(Encoding.ASCII.GetBytes(details.Data.Enrollment.Ott.Jwt));

                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                System.IO.File.WriteAllBytes(tempFilePath, Encoding.UTF8.GetBytes(zitiId));

                if (!await h.WaitForTerminatorAsync(svcName, TimeSpan.FromSeconds(20))) {
                    throw new Exception("Error while waiting for terminator");
                }

                var c = new ZitiContext(tempFilePath);
                var zitiSocketHandler = c.NewZitiSocketHandler(svcName);
                var client = new HttpClient(new OpenZiti.Debugging.LoggingHandler(zitiSocketHandler));
                client.DefaultRequestHeaders.Add("User-Agent", "curl/7.59.0");

                var result = await client.GetStringAsync("https://wttr.in:443");
                StringAssert.Contains(result, "Weather report"); //verify the test succeeds
                Console.WriteLine(result);
                Console.WriteLine("==============================================================");
                Console.WriteLine("Sample execution completed successfully");
                Console.WriteLine("==============================================================");

            } catch (ApiException<ApiErrorEnvelope> e) {
                Console.WriteLine($"{e.Result.Error.Code}");
                Console.WriteLine($"{e.Result.Error.Message}");
                Console.WriteLine($"{e.Result.Error.Cause.Reason}");
                Console.WriteLine($"{e.Message}");
            } catch (ApiException e) {
                Console.WriteLine($"{e.Message}");
                if (e.InnerException != null) {
                    Console.WriteLine($"{e.InnerException.Message}");
                }
            }
        }
    }

    public class LoggingHttpClientHandler : HttpClientHandler {
        private readonly Logger _logger; // Replace with your logger implementation

        public LoggingHttpClientHandler(Logger logger) {
            _logger = logger ?? throw new ArgumentNullException(nameof(logger));
        }

        protected override async Task<HttpResponseMessage> SendAsync(HttpRequestMessage request, CancellationToken cancellationToken) {
            // Log the request JSON payload before sending
            if (request.Content != null) {
                string requestBody = await request.Content.ReadAsStringAsync();
                _logger.Info($"Request JSON: {requestBody}");
            }

            // Continue with the HTTP request
            var response = await base.SendAsync(request, cancellationToken);

            return response;
        }
    }
}
