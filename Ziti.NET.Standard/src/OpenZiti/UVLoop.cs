using System;
using System.Collections.Generic;
using System.Text;

namespace OpenZiti {
    public class UVLoop {
        public static readonly UVLoop DefaultLoop = new UVLoop(Native.API.z4d_default_loop());

        internal IntPtr nativeUvLoop;
        public UVLoop() : this(Native.API.newLoop()) { 
            //empty
        }
        internal UVLoop(IntPtr nativeLoop) {
            this.nativeUvLoop = nativeLoop;
        }
    }
}
