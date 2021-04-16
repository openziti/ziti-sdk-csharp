using System;
using System.Collections.Generic;
using System.Text;

namespace OpenZiti.Native {
    internal class IdentityFile {
        public string ztAPI { get; set; }
        public Id id { get; set; }

        public string[] configTypes { get; set; }
    }

    internal class Id {
        public string key { get; set; }
        public string cert { get; set; }
        public string ca { get; set; }
    }
}
