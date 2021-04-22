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

namespace OpenZiti {

    public class ZitiIdentityFormat {
        public string ControllerUrl { get; set; }
        public IdMaterial IdMaterial { get; set; }
        public ZitiIdentityFormat() {

        }
        internal ZitiIdentityFormat(ZitiIdentityFormatNative native) {
            this.ControllerUrl = native.ControllerUrl;
            this.IdMaterial = new IdMaterial() {
                CA = native.IdMaterial.CA,
                Certificate = native.IdMaterial.Certificate,
                Key = native.IdMaterial.Key,
            };
        }
    }

    public class IdMaterial {
        public string Certificate { get; set; }
        public string Key { get; set; }
        public string CA { get; set; }
    }

    internal struct ZitiIdentityFormatNative {
        internal string ControllerUrl { get; set; }
#pragma warning disable 0649 //static analysis can't find that this is assigned to during serialization
        internal IdMaterialNative IdMaterial;
#pragma warning restore 0649
    }

    internal struct IdMaterialNative {
        public string Certificate { get; set; }
        public string Key { get; set; }
        public string CA { get; set; }
    }

}
