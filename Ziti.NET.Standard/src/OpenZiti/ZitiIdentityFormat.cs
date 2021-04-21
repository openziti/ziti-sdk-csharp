using System;
using System.Collections.Generic;
using System.Text;

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
        public string ControllerUrl { get; set; }
        public IdMaterialNative IdMaterial;
    }

    internal struct IdMaterialNative {
        public string Certificate { get; set; }
        public string Key { get; set; }
        public string CA { get; set; }
    }

}
