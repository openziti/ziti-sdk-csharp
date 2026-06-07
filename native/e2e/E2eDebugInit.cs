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

using Microsoft.VisualStudio.TestTools.UnitTesting;

namespace E2ETest;

// Initialize the native ziti library once and route its log to stderr at DEBUG, so any dial/connect failure
// prints its real cause (the C SDK's own WARN line + rc), which CI captures.
[TestClass]
public static class E2eDebugInit
{
    [AssemblyInitialize]
    public static void Init(TestContext _)
    {
        ZitiNative.LibInit();
        ZitiNative.EnableNativeLogging();
    }

    // The in-process tests (ProxyBridgeTest, IdiomaticTrafficTest) load ziti contexts into the one shared
    // zitilib loop and there is no per-context unload exposed. Tear the whole lib down once at the end so those
    // contexts and the background thread are released rather than leaked for the life of the test process.
    [AssemblyCleanup]
    public static void Cleanup()
    {
        ZitiNative.LibShutdown();
    }
}
