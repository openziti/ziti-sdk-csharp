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
        /** The expected outcome of a successful operation */ _ziti_OK, /** The provided configuration was not found */ _ziti_CONFIG_NOT_FOUND, /** The provided JWT was not found */ _ziti_JWT_NOT_FOUND, /** The provided JWT is not accepted by controller */ _ziti_JWT_INVALID, /** The provided JWT has invalid format */ _ziti_JWT_INVALID_FORMAT, /** PKCS7/ASN.1 parsing failed */ _ziti_PKCS7_ASN1_PARSING_FAILED, /** unsupported JWT signing algorithm */ _ziti_JWT_SIGNING_ALG_UNSUPPORTED, /** JWT verification failed */ _ziti_JWT_VERIFICATION_FAILED, /** unsupported enrollment method */ _ziti_ENROLLMENT_METHOD_UNSUPPORTED, /** enrollment method requires client certificate */ _ziti_ENROLLMENT_CERTIFICATE_REQUIRED, /** Attempt to generate an private key failed */ _ziti_KEY_GENERATION_FAILED, /** Attempt to load TLS key failed */ _ziti_KEY_LOAD_FAILED, /** Attempt to generate a CSR failed */ _ziti_CSR_GENERATION_FAILED, /** Some or all of the provided configuration is incorrect */ _ziti_INVALID_CONFIG, /** Returned when the identity does not have the correct level of access needed.
dentity does not have the correct level of access needed.
    Common causes are:
    * no policy exists granting the identity access
    * the certificates presented are incorrect, out of date, or invalid
    */ _ziti_NOT_AUTHORIZED, /** The SDK has attempted to communicate to the Ziti Controller but the controller
ler
    is offline or did not respond to the request*/ _ziti_CONTROLLER_UNAVAILABLE, /** The SDK cannot send data to the Ziti Network because an Edge Router was not available. Common causes are:
re:
    * the identity connecting is not associated with any Edge Routers
    * the Edge Router in use is no longer responding */ _ziti_GATEWAY_UNAVAILABLE, /** The SDK cannot send data to the Ziti Network because the requested service was not available. Common causes are:
re:
    * the service does not exist
    * the identity connecting is not associated with the given service
    */ _ziti_SERVICE_UNAVAILABLE, /** The connection has been closed gracefully */ _ziti_EOF, /** A connect or write operation did not complete in the alloted timeout. #DEFAULT_TIMEOUT */ _ziti_TIMEOUT, /** The connection has been closed abnormally. */ _ziti_CONNABORT, /** SDK detected invalid state, most likely caaused by improper use. */ _ziti_INVALID_STATE, /** SDK detected invalid cryptographic state of Ziti connection */ _ziti_CRYPTO_FAIL, /** connection was closed */ _ziti_CONN_CLOSED, /** failed posture check */ _ziti_INVALID_POSTURE, /** attempted to start MFA enrollment when it already has been started or completed */ _ziti_MFA_EXISTS, /** attempted to use an MFA token that is invalid */ _ziti_MFA_INVALID_TOKEN, /** attempted to verify or retrieve details of an MFA enrollment that has not been completed */ _ziti_MFA_NOT_ENROLLED, /** not found, usually indicates stale reference or permission */ _ziti_NOT_FOUND, /** Inspired by the Android SDK: What a Terrible Failure. A condition that should never happen. */ _ziti_WTF,
    }
    public enum ZitiStatus {
        /** The expected outcome of a successful operation */ [Description("OK")] OK = -err._ziti_OK, /** The provided configuration was not found */ [Description("Configuration not found")] CONFIG_NOT_FOUND = -err._ziti_CONFIG_NOT_FOUND, /** The provided JWT was not found */ [Description("JWT not found")] JWT_NOT_FOUND = -err._ziti_JWT_NOT_FOUND, /** The provided JWT is not accepted by controller */ [Description("JWT not accepted by controller")] JWT_INVALID = -err._ziti_JWT_INVALID, /** The provided JWT has invalid format */ [Description("JWT has invalid format")] JWT_INVALID_FORMAT = -err._ziti_JWT_INVALID_FORMAT, /** PKCS7/ASN.1 parsing failed */ [Description("PKCS7/ASN.1 parsing failed")] PKCS7_ASN1_PARSING_FAILED = -err._ziti_PKCS7_ASN1_PARSING_FAILED, /** unsupported JWT signing algorithm */ [Description("unsupported JWT signing algorithm")] JWT_SIGNING_ALG_UNSUPPORTED = -err._ziti_JWT_SIGNING_ALG_UNSUPPORTED, /** JWT verification failed */ [Description("JWT verification failed")] JWT_VERIFICATION_FAILED = -err._ziti_JWT_VERIFICATION_FAILED, /** unsupported enrollment method */ [Description("unsupported enrollment method")] ENROLLMENT_METHOD_UNSUPPORTED = -err._ziti_ENROLLMENT_METHOD_UNSUPPORTED, /** enrollment method requires client certificate */ [Description("enrollment method requires certificate")] ENROLLMENT_CERTIFICATE_REQUIRED = -err._ziti_ENROLLMENT_CERTIFICATE_REQUIRED, /** Attempt to generate an private key failed */ [Description("error generating private key")] KEY_GENERATION_FAILED = -err._ziti_KEY_GENERATION_FAILED, /** Attempt to load TLS key failed */ [Description("error loading TLS key")] KEY_LOAD_FAILED = -err._ziti_KEY_LOAD_FAILED, /** Attempt to generate a CSR failed */ [Description("error generating a CSR")] CSR_GENERATION_FAILED = -err._ziti_CSR_GENERATION_FAILED, /** Some or all of the provided configuration is incorrect */ [Description("Configuration is invalid")] INVALID_CONFIG = -err._ziti_INVALID_CONFIG, /** Returned when the identity does not have the correct level of access needed.
dentity does not have the correct level of access needed.
    Common causes are:
    * no policy exists granting the identity access
    * the certificates presented are incorrect, out of date, or invalid
    */ [Description("Not Authorized")] NOT_AUTHORIZED = -err._ziti_NOT_AUTHORIZED, /** The SDK has attempted to communicate to the Ziti Controller but the controller
ler
    is offline or did not respond to the request*/ [Description("Ziti Controller is not available")] CONTROLLER_UNAVAILABLE = -err._ziti_CONTROLLER_UNAVAILABLE, /** The SDK cannot send data to the Ziti Network because an Edge Router was not available. Common causes are:
re:
    * the identity connecting is not associated with any Edge Routers
    * the Edge Router in use is no longer responding */ [Description("Ziti Edge Router is not available")] GATEWAY_UNAVAILABLE = -err._ziti_GATEWAY_UNAVAILABLE, /** The SDK cannot send data to the Ziti Network because the requested service was not available. Common causes are:
re:
    * the service does not exist
    * the identity connecting is not associated with the given service
    */ [Description("Service not available")] SERVICE_UNAVAILABLE = -err._ziti_SERVICE_UNAVAILABLE, /** The connection has been closed gracefully */ [Description("Connection closed")] EOF = -err._ziti_EOF, /** A connect or write operation did not complete in the alloted timeout. #DEFAULT_TIMEOUT */ [Description("Operation did not complete in time")] TIMEOUT = -err._ziti_TIMEOUT, /** The connection has been closed abnormally. */ [Description("Connection to edge router terminated")] CONNABORT = -err._ziti_CONNABORT, /** SDK detected invalid state, most likely caaused by improper use. */ [Description("invalid state")] INVALID_STATE = -err._ziti_INVALID_STATE, /** SDK detected invalid cryptographic state of Ziti connection */ [Description("crypto failure")] CRYPTO_FAIL = -err._ziti_CRYPTO_FAIL, /** connection was closed */ [Description("connection is closed")] CONN_CLOSED = -err._ziti_CONN_CLOSED, /** failed posture check */ [Description("failed posture check")] INVALID_POSTURE = -err._ziti_INVALID_POSTURE, /** attempted to start MFA enrollment when it already has been started or completed */ [Description("an MFA enrollment already exists")] MFA_EXISTS = -err._ziti_MFA_EXISTS, /** attempted to use an MFA token that is invalid */ [Description("the token provided was invalid")] MFA_INVALID_TOKEN = -err._ziti_MFA_INVALID_TOKEN, /** attempted to verify or retrieve details of an MFA enrollment that has not been completed */ [Description("the current identity has not completed MFA enrollment")] MFA_NOT_ENROLLED = -err._ziti_MFA_NOT_ENROLLED, /** not found, usually indicates stale reference or permission */ [Description("entity no longer exists or is no longer accessible")] NOT_FOUND = -err._ziti_NOT_FOUND, /** Inspired by the Android SDK: What a Terrible Failure. A condition that should never happen. */ [Description("WTF: programming error")] WTF = -err._ziti_WTF,
    }
}
