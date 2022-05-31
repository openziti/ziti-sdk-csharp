using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;
using OpenZiti;

namespace OpenZiti {

    public struct ZitiInstance {
        public ZitiIdentity Zid;
        public Dictionary<string, ZitiService> Services;

        public void Initialize(ZitiIdentity zid) {
            this.Zid = zid;
            this.Services = new Dictionary<string, ZitiService>();
        }
    }

}
