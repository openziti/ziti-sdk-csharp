using System;
using System.Collections.Generic;
using System.Text;

namespace OpenZiti {
    public static class ZitiStatusExtensions {
        public static bool Ok(this ZitiStatus s) {
            return s == ZitiStatus.OK;
        }
    }
}
