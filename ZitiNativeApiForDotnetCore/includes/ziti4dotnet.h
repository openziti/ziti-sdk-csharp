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

#define FIELD_WITH_CHECK(FIELD_TYPE, FIELD_NAME)        \
                    uint32_t FIELD_NAME##_offset;       \
                    uint32_t FIELD_NAME##_size;         \
                    const char* FIELD_NAME##_checksum;  \
                    FIELD_TYPE FIELD_NAME;

typedef struct ziti_types_s {
    FIELD_WITH_CHECK(uint32_t, total_size);

    FIELD_WITH_CHECK(ziti_auth_query_mfa, ziti_auth_query_mfa);
    FIELD_WITH_CHECK(ziti_id_cfg, ziti_id_cfg);
    FIELD_WITH_CHECK(ziti_config, ziti_config);
    FIELD_WITH_CHECK(api_path, api_path);
    FIELD_WITH_CHECK(ziti_api_versions, ziti_api_versions);
    FIELD_WITH_CHECK(ziti_version, ziti_version);
    FIELD_WITH_CHECK(ziti_identity, ziti_identity);
    FIELD_WITH_CHECK(ziti_process, ziti_process);
    FIELD_WITH_CHECK(ziti_posture_query, ziti_posture_query);
    FIELD_WITH_CHECK(ziti_posture_query_set, ziti_posture_query_set);
    FIELD_WITH_CHECK(ziti_session_type, ziti_session_type);
    FIELD_WITH_CHECK(ziti_service, ziti_service);
    FIELD_WITH_CHECK(ziti_address, ziti_address_host);
    FIELD_WITH_CHECK(ziti_address, ziti_address_cidr);
    FIELD_WITH_CHECK(ziti_client_cfg_v1, ziti_client_cfg_v1);
    FIELD_WITH_CHECK(ziti_intercept_cfg_v1, ziti_intercept_cfg_v1);
    FIELD_WITH_CHECK(ziti_server_cfg_v1, ziti_server_cfg_v1);
    FIELD_WITH_CHECK(ziti_listen_options, ziti_listen_options);
    FIELD_WITH_CHECK(ziti_host_cfg_v1, ziti_host_cfg_v1);
    FIELD_WITH_CHECK(ziti_host_cfg_v2, ziti_host_cfg_v2);
    FIELD_WITH_CHECK(ziti_mfa_enrollment, ziti_mfa_enrollment);
    FIELD_WITH_CHECK(ziti_port_range, ziti_port_range);
    FIELD_WITH_CHECK(ziti_options, ziti_options);

    //events
    FIELD_WITH_CHECK(ziti_event_t, ziti_context_event);
    FIELD_WITH_CHECK(ziti_event_t, ziti_router_event);
    FIELD_WITH_CHECK(ziti_event_t, ziti_service_event);
    FIELD_WITH_CHECK(ziti_event_t, ziti_mfa_auth_event);
    FIELD_WITH_CHECK(ziti_event_t, ziti_api_event);


} ziti_types_t;

Z4D_API int z4d_ziti_close(ziti_connection con);
Z4D_API int z4d_uv_run(void* loop);
Z4D_API void* z4d_new_loop();
Z4D_API const char** z4d_all_config_types();
Z4D_API uv_loop_t* z4d_default_loop();
Z4D_API void* z4d_registerUVTimer(uv_loop_t* loop, uv_timer_cb timer_cb, uint64_t iterations, uint64_t delay);
Z4D_API void* z4d_stop_uv_timer(uv_timer_t* t);
Z4D_API int z4d_event_type_from_pointer(const ziti_event_t *event);
Z4D_API ziti_service* z4d_service_array_get(ziti_service_array arr, int idx);

Z4D_API char** z4d_make_char_array(int size);
Z4D_API void z4d_set_char_at(char **a, char *s, int n);
Z4D_API void z4d_free_char_array(char **a, int size);

Z4D_API void z4d_ziti_dump_log(ziti_context ztx);
Z4D_API void z4d_ziti_dump_file(ziti_context ztx, const char* outputFile);

Z4D_API ziti_types_t* z4d_struct_test();
Z4D_API ziti_posture_query* z4d_ziti_posture_query();

typedef void (*z4d_cb)(void* context);
Z4D_API void z4d_callback_on_loop(uv_loop_t* loop, void* context, z4d_cb cb);

#ifdef __cplusplus
}
#endif

#endif /* ZITI4DOTNET_H */