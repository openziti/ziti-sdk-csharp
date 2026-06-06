#define ZITI_SDK_MODEL_SUPPORT_H //prevent model_support.h from being processed

#include "ziti/error_defs.h"
/*
This file is generated using the C preprocessor. Do not edit
*/
using System.ComponentModel;

namespace OpenZiti {
#define enum_id(err_id,s) _ziti_##err_id,
    enum err {
        ZITI_ERRORS(enum_id)
    }

#define err_code(err_id,desc) [Description(desc)] err_id = -err._ziti_##err_id,

    public enum ZitiStatus {
        ZITI_ERRORS(err_code)
    }
}