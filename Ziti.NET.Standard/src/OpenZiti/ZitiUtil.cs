/*
Copyright 2019 NetFoundry, Inc.

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
using System.Runtime.InteropServices;

namespace OpenZiti
{
    public static class ZitiUtil
    {
        public readonly static GCHandle NO_CONTEXT = GCHandle.Alloc(new object());

        public static void CheckStatus(ZitiContext zitiContext, ZitiStatus status, object initContext)
        {
            status.Check();
        }

        public static void CheckStatus(int status)
        {
            if (status < 0)
            {
                CheckStatus((ZitiStatus)status);
            }
        }

        public static void CheckStatus(ZitiStatus status)
        {
            if (status != ZitiStatus.OK)
            {
                throw new ZitiException(status);
            }
        }
        public static void Check(this ZitiStatus status)
        {
            ZitiUtil.CheckStatus(status);
        }

        public static object GetTarget(GCHandle handle)
        {
            if (handle.IsAllocated)
            {
                return handle.Target;
            }
            return null;
        }
    }
}
