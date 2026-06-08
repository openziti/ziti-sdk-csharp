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

using System.IO;
using System.Threading.Tasks;

using Microsoft.VisualStudio.TestTools.UnitTesting;

using OpenZiti;

namespace E2ETest;

// Enrollment idiomatic test: create a one-time-token identity on the controller, enroll it through the SDK
// (API.EnrollIdentity), and prove the resulting strong identity is usable by loading a ZitiContext from it.
// This exercises the enrollment path only (no zitilib bridge), so it runs on every OS with no self-gating.
[TestClass]
public class EnrollmentTest
{
    [TestMethod]
    [TestCategory("e2e")]
    [Timeout(60_000)]
    public async Task Identity_Enrolls_And_Loads()
    {
        var setup = await OverlaySetup.ConnectAsync();

        // Creates an OTT identity and enrolls its JWT via the SDK, writing the strong identity file.
        OverlaySetup.Say("[enroll] creating an OTT identity and enrolling it through the SDK (API.EnrollIdentity)");
        var idFile = await setup.EnrollNewIdentityAsync("e2e-enroll-test");

        Assert.IsTrue(File.Exists(idFile), "enrollment did not produce an identity file");
        var json = File.ReadAllText(idFile);
        StringAssert.Contains(json, "ztAPI",
            "enrolled identity JSON is missing the controller endpoint (ztAPI); enrollment likely failed");

        // Loading a context from the enrolled identity proves the cert/key/ca it produced are usable.
        // ZitiContext(string) throws a ZitiException if the load fails, so reaching the assert means success.
        OverlaySetup.Say("[enroll] loading a ZitiContext from the enrolled strong identity");
        var ztx = new ZitiContext(idFile);
        Assert.IsNotNull(ztx, "failed to load a ZitiContext from the enrolled identity");
        OverlaySetup.Say("[enroll] enrollment succeeded and the identity loads");
    }
}
