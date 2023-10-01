using Microsoft.VisualStudio.TestTools.UnitTesting;

using System;
using System.Net.Http;
using System.Threading.Tasks;
using OpenZiti.Generated;
using System.Data;
using System.Text;
using System.IO;

using Microsoft.Extensions.Logging;
using OpenZiti.Management;

namespace OpenZiti.NET.Tests {
    [TestClass]
    public class DataTests {

        [ClassInitialize]
        public static void ClassInitialize(TestContext context) {
//            Logger l = new Logger(context);

            // Code to run once before all test methods in the class
            Logging.SimpleConsoleLogging(LogLevel.Trace);

            OpenZiti.API.NativeLogger = OpenZiti.API.DefaultNativeLogFunction;
            OpenZiti.API.InitializeZiti();
            //to see the logs from the Native SDK, set the log level
            OpenZiti.API.SetLogLevel(ZitiLogLevel.TRACE);
        }

        [TestMethod]
        public async Task TestMethod1() {
            Authenticate auth = new Authenticate();
            Method method = Method.Password;
            auth.Username = "admin";
            auth.Password = "cdaws8086";

            var handler = new HttpClientHandler {
                ClientCertificateOptions = ClientCertificateOption.Manual,
                ServerCertificateCustomValidationCallback =
                (httpRequestMessage, cert, cetChain, policyErrors) => {
                    return true;
                }
            };

            var nonValidatingHttpClient = new HttpClient(handler);
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

            var identity = new IdentityCreate {
                Name = "test_id",
                Enrollment = new Enrollment {
                    Ott = true,
                }
            };

            Helper h = new Helper(mapi);
            await h.DeleteServiceByName("test-weather");

            var createService = new ServiceCreate {
                Name = "test-weather",
                RoleAttributes = new string[] { "test-services" },
                Configs = new string[] {"host.v1-test-service"}
            };
            

            try {
                var createIdentityResult = await mapi.CreateIdentityAsync(identity);
                var createdId = createIdentityResult.Data.Id;
                Console.WriteLine("success: " + createdId);

                var details = await mapi.DetailIdentityAsync(createdId);

                //enroll the new identity
                string zitiId = OpenZiti.API.EnrollIdentity(Encoding.ASCII.GetBytes(details.Data.Enrollment.Ott.Jwt));

                string tempFilePath = Path.Combine(Path.GetTempPath(), Path.GetRandomFileName());
                System.IO.File.WriteAllBytes(tempFilePath, Encoding.UTF8.GetBytes(zitiId));

                var c = new ZitiContext(tempFilePath);
                var zitiSocketHandler = c.NewZitiSocketHandler("weather-svc");
                var client = new HttpClient(new OpenZiti.NET.LoggingHandler(zitiSocketHandler));
                client.DefaultRequestHeaders.Add("User-Agent", "curl/7.59.0");

                var result = client.GetStringAsync("https://wttr.in:443").Result;


            } catch (Exception e) {
                Console.WriteLine($"{e.Message}");
            }
        }
    }
}
