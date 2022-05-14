



/*
Copyright 2019-2020 NetFoundry, Inc.

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


// @cond

// @endcond
















































































/*
This file is generated using the C preprocessor. Do not edit
*/
using System.ComponentModel;

namespace OpenZiti {

    enum err {
        _ziti_OK, _ziti_CONFIG_NOT_FOUND, _ziti_JWT_NOT_FOUND, _ziti_JWT_INVALID, _ziti_JWT_INVALID_FORMAT, _ziti_PKCS7_ASN1_PARSING_FAILED, _ziti_JWT_SIGNING_ALG_UNSUPPORTED, _ziti_JWT_VERIFICATION_FAILED, _ziti_ENROLLMENT_METHOD_UNSUPPORTED, _ziti_ENROLLMENT_CERTIFICATE_REQUIRED, _ziti_KEY_GENERATION_FAILED, _ziti_KEY_LOAD_FAILED, _ziti_CSR_GENERATION_FAILED, _ziti_INVALID_CONFIG, _ziti_NOT_AUTHORIZED, _ziti_CONTROLLER_UNAVAILABLE, _ziti_GATEWAY_UNAVAILABLE, _ziti_SERVICE_UNAVAILABLE, _ziti_EOF, _ziti_TIMEOUT, _ziti_CONNABORT, _ziti_INVALID_STATE, _ziti_CRYPTO_FAIL, _ziti_CONN_CLOSED, _ziti_INVALID_POSTURE, _ziti_MFA_EXISTS, _ziti_MFA_INVALID_TOKEN, _ziti_MFA_NOT_ENROLLED, _ziti_NOT_FOUND, _ziti_DISABLED, _ziti_PARTIALLY_AUTHENTICATED, _ziti_WTF,
    }



    public enum ZitiStatus {
        [Description("OK")] OK = -err._ziti_OK, [Description("Configuration not found")] CONFIG_NOT_FOUND = -err._ziti_CONFIG_NOT_FOUND, [Description("JWT not found")] JWT_NOT_FOUND = -err._ziti_JWT_NOT_FOUND, [Description("JWT not accepted by controller")] JWT_INVALID = -err._ziti_JWT_INVALID, [Description("JWT has invalid format")] JWT_INVALID_FORMAT = -err._ziti_JWT_INVALID_FORMAT, [Description("PKCS7/ASN.1 parsing failed")] PKCS7_ASN1_PARSING_FAILED = -err._ziti_PKCS7_ASN1_PARSING_FAILED, [Description("unsupported JWT signing algorithm")] JWT_SIGNING_ALG_UNSUPPORTED = -err._ziti_JWT_SIGNING_ALG_UNSUPPORTED, [Description("JWT verification failed")] JWT_VERIFICATION_FAILED = -err._ziti_JWT_VERIFICATION_FAILED, [Description("unsupported enrollment method")] ENROLLMENT_METHOD_UNSUPPORTED = -err._ziti_ENROLLMENT_METHOD_UNSUPPORTED, [Description("enrollment method requires certificate")] ENROLLMENT_CERTIFICATE_REQUIRED = -err._ziti_ENROLLMENT_CERTIFICATE_REQUIRED, [Description("error generating private key")] KEY_GENERATION_FAILED = -err._ziti_KEY_GENERATION_FAILED, [Description("error loading TLS key")] KEY_LOAD_FAILED = -err._ziti_KEY_LOAD_FAILED, [Description("error generating a CSR")] CSR_GENERATION_FAILED = -err._ziti_CSR_GENERATION_FAILED, [Description("Configuration is invalid")] INVALID_CONFIG = -err._ziti_INVALID_CONFIG, [Description("Not Authorized")] NOT_AUTHORIZED = -err._ziti_NOT_AUTHORIZED, [Description("Ziti Controller is not available")] CONTROLLER_UNAVAILABLE = -err._ziti_CONTROLLER_UNAVAILABLE, [Description("Ziti Edge Router is not available")] GATEWAY_UNAVAILABLE = -err._ziti_GATEWAY_UNAVAILABLE, [Description("Service not available")] SERVICE_UNAVAILABLE = -err._ziti_SERVICE_UNAVAILABLE, [Description("Connection closed")] EOF = -err._ziti_EOF, [Description("Operation did not complete in time")] TIMEOUT = -err._ziti_TIMEOUT, [Description("Connection to edge router terminated")] CONNABORT = -err._ziti_CONNABORT, [Description("invalid state")] INVALID_STATE = -err._ziti_INVALID_STATE, [Description("crypto failure")] CRYPTO_FAIL = -err._ziti_CRYPTO_FAIL, [Description("connection is closed")] CONN_CLOSED = -err._ziti_CONN_CLOSED, [Description("failed posture check")] INVALID_POSTURE = -err._ziti_INVALID_POSTURE, [Description("an MFA enrollment already exists")] MFA_EXISTS = -err._ziti_MFA_EXISTS, [Description("the token provided was invalid")] MFA_INVALID_TOKEN = -err._ziti_MFA_INVALID_TOKEN, [Description("the current identity has not completed MFA enrollment")] MFA_NOT_ENROLLED = -err._ziti_MFA_NOT_ENROLLED, [Description("entity no longer exists or is no longer accessible")] NOT_FOUND = -err._ziti_NOT_FOUND, [Description("ziti context is disabled")] DISABLED = -err._ziti_DISABLED, [Description("api session is partially authenticated, waiting for auth query resolution")] PARTIALLY_AUTHENTICATED = -err._ziti_PARTIALLY_AUTHENTICATED, [Description("WTF: programming error")] WTF = -err._ziti_WTF,
    }
}
