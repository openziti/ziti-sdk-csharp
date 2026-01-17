#ifndef ZITI4DOTNET_H
#define ZITI4DOTNET_H

#include <stdlib.h>
#include <string.h>
#include <stdint.h>

#include <ziti/ziti.h>
#include <ziti/ziti_log.h>
#include <ziti/ziti_events.h>
#include <uv.h>

#if _WIN32
#define Z4D_API __declspec(dllexport)
#else
#   define Z4D_API /* nothing */
# endif

#ifdef __cplusplus
extern "C" {
#endif

#define ALIGNMENT_CHECK(FIELD_TYPE)             \
typedef struct ziti_alignment_check_##FIELD_TYPE##_s {       \
                    uint32_t offset;                        \
                    uint32_t size;                          \
                    const char* checksum;                   \
} ziti_alignment_check_##FIELD_TYPE

#define ALIGNMENT_FIELD(FIELD_TYPE, FIELD_NAME) \
  ziti_alignment_check_##FIELD_TYPE FIELD_NAME

ALIGNMENT_CHECK(ziti_id_cfg);
ALIGNMENT_CHECK(ziti_config);
ALIGNMENT_CHECK(api_path);
ALIGNMENT_CHECK(ziti_api_versions);
ALIGNMENT_CHECK(ziti_version);
ALIGNMENT_CHECK(ziti_identity);
ALIGNMENT_CHECK(ziti_process);
ALIGNMENT_CHECK(ziti_posture_query);
ALIGNMENT_CHECK(ziti_posture_query_set);
ALIGNMENT_CHECK(ziti_session_type);
ALIGNMENT_CHECK(ziti_service);
ALIGNMENT_CHECK(ziti_address);
ALIGNMENT_CHECK(ziti_client_cfg_v1);
ALIGNMENT_CHECK(ziti_intercept_cfg_v1);
ALIGNMENT_CHECK(ziti_server_cfg_v1);
ALIGNMENT_CHECK(ziti_listen_options);
ALIGNMENT_CHECK(ziti_host_cfg_v1);
ALIGNMENT_CHECK(ziti_host_cfg_v2);
ALIGNMENT_CHECK(ziti_mfa_enrollment);
ALIGNMENT_CHECK(ziti_port_range);
ALIGNMENT_CHECK(ziti_options);
ALIGNMENT_CHECK(ziti_event_t);

#define ALIGNMENT_DATA(FIELD_TYPE, FIELD_NAME) \
  FIELD_TYPE FIELD_NAME##_data

typedef struct ziti_types_v2_s {
    uint32_t size; //declare as a pointer but use as a value
    // declare all the alignments: offset, size, checksum
    ALIGNMENT_FIELD(ziti_id_cfg, ziti_id_cfg);
    ALIGNMENT_FIELD(ziti_config, ziti_config);
    ALIGNMENT_FIELD(api_path, api_path);
    ALIGNMENT_FIELD(ziti_api_versions, ziti_api_versions);
    ALIGNMENT_FIELD(ziti_version, ziti_version);
    ALIGNMENT_FIELD(ziti_identity, ziti_identity);
    ALIGNMENT_FIELD(ziti_process, ziti_process);
    ALIGNMENT_FIELD(ziti_posture_query, ziti_posture_query);
    ALIGNMENT_FIELD(ziti_posture_query_set, ziti_posture_query_set);
    ALIGNMENT_FIELD(ziti_session_type, ziti_session_type);
    ALIGNMENT_FIELD(ziti_service, ziti_service);
    ALIGNMENT_FIELD(ziti_address, ziti_address_host);
    ALIGNMENT_FIELD(ziti_address, ziti_address_cidr);
    ALIGNMENT_FIELD(ziti_client_cfg_v1, ziti_client_cfg_v1);
    ALIGNMENT_FIELD(ziti_intercept_cfg_v1, ziti_intercept_cfg_v1);
    ALIGNMENT_FIELD(ziti_server_cfg_v1, ziti_server_cfg_v1);
    ALIGNMENT_FIELD(ziti_listen_options, ziti_listen_options);
    ALIGNMENT_FIELD(ziti_host_cfg_v1, ziti_host_cfg_v1);
    ALIGNMENT_FIELD(ziti_host_cfg_v2, ziti_host_cfg_v2);
    ALIGNMENT_FIELD(ziti_mfa_enrollment, ziti_mfa_enrollment);
    ALIGNMENT_FIELD(ziti_port_range, ziti_port_range);
    ALIGNMENT_FIELD(ziti_options, ziti_options);
    ALIGNMENT_FIELD(ziti_event_t, ziti_context_event);
    ALIGNMENT_FIELD(ziti_event_t, ziti_router_event);
    ALIGNMENT_FIELD(ziti_event_t, ziti_service_event);
    ALIGNMENT_FIELD(ziti_event_t, ziti_mfa_auth_event);
    ALIGNMENT_FIELD(ziti_event_t, ziti_api_event);

    // now declare "_data" elemnets - the __ACTUAL__ structs
    ALIGNMENT_DATA(ziti_id_cfg, ziti_id_cfg);
    ALIGNMENT_DATA(ziti_config, ziti_config);
    ALIGNMENT_DATA(api_path, api_path);
    ALIGNMENT_DATA(ziti_api_versions, ziti_api_versions);
    ALIGNMENT_DATA(ziti_version, ziti_version);
    ALIGNMENT_DATA(ziti_identity, ziti_identity);
    ALIGNMENT_DATA(ziti_process, ziti_process);
    ALIGNMENT_DATA(ziti_posture_query, ziti_posture_query);
    ALIGNMENT_DATA(ziti_posture_query_set, ziti_posture_query_set);
    ALIGNMENT_DATA(ziti_session_type, ziti_session_type);
    ALIGNMENT_DATA(ziti_service, ziti_service);
    ALIGNMENT_DATA(ziti_address, ziti_address_host);
    ALIGNMENT_DATA(ziti_address, ziti_address_cidr);
    ALIGNMENT_DATA(ziti_client_cfg_v1, ziti_client_cfg_v1);
    ALIGNMENT_DATA(ziti_intercept_cfg_v1, ziti_intercept_cfg_v1);
    ALIGNMENT_DATA(ziti_server_cfg_v1, ziti_server_cfg_v1);
    ALIGNMENT_DATA(ziti_listen_options, ziti_listen_options);
    ALIGNMENT_DATA(ziti_host_cfg_v1, ziti_host_cfg_v1);
    ALIGNMENT_DATA(ziti_host_cfg_v2, ziti_host_cfg_v2);
    ALIGNMENT_DATA(ziti_mfa_enrollment, ziti_mfa_enrollment);
    ALIGNMENT_DATA(ziti_port_range, ziti_port_range);
    ALIGNMENT_DATA(ziti_options, ziti_options);
    ALIGNMENT_DATA(struct ziti_context_event, ziti_context_event);
    ALIGNMENT_DATA(struct ziti_router_event, ziti_router_event);
    ALIGNMENT_DATA(struct ziti_service_event, ziti_service_event);
    ALIGNMENT_DATA(struct ziti_auth_event, ziti_auth_event);
    ALIGNMENT_DATA(struct ziti_config_event, ziti_config_event);
} ziti_types_v2;

Z4D_API int z4d_ziti_close(ziti_connection con);
Z4D_API int z4d_uv_run(void* loop);
Z4D_API void* z4d_new_loop();
Z4D_API const char** z4d_all_config_types();
Z4D_API uv_loop_t* z4d_default_loop();
Z4D_API void* z4d_registerUVTimer(uv_loop_t* loop, uv_timer_cb timer_cb, uint64_t iterations, uint64_t delay);
Z4D_API int z4d_stop_uv_timer(uv_timer_t* t);
Z4D_API int z4d_event_type_from_pointer(const ziti_event_t *event);
Z4D_API ziti_service* z4d_service_array_get(ziti_service_array arr, int idx);

Z4D_API char** z4d_make_char_array(int size);
Z4D_API void z4d_set_char_at(char **a, char *s, int n);
Z4D_API void z4d_free_char_array(char **a, int size);

Z4D_API void z4d_ziti_dump_log(ziti_context ztx);
Z4D_API void z4d_ziti_dump_file(ziti_context ztx, const char* outputFile);

Z4D_API ziti_types_v2* z4d_struct_test();
Z4D_API ziti_posture_query* z4d_ziti_posture_query();

typedef void (*z4d_cb)(void* context);
Z4D_API void z4d_callback_on_loop(uv_loop_t* loop, void* context, z4d_cb cb);

#ifdef __cplusplus
}
#endif

#endif /* ZITI4DOTNET_H */
