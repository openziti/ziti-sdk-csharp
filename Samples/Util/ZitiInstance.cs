using OpenZiti;
using System;
using System.Collections.Generic;
using System.Linq;
using System.Text;
using System.Threading.Tasks;

namespace OpenZiti {

    public struct ZitiInstance {
        public ZitiIdentity Zid;
        public Dictionary<string, ZitiService> Services;

        public void Initialize(ZitiIdentity zid) {
            Zid = zid;
            Services = new Dictionary<string, ZitiService>();
        }
    }

}
