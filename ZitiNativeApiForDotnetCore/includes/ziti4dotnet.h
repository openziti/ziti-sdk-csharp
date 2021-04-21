#include <stdlib.h>
#include <string.h>

#include <ziti/ziti.h>
#include <ziti/ziti_log.h>
#include <uv.h>

#if _WIN32
# if defined(ziti4dotnet_EXPORTS)
#   define Z4D_API __declspec(dllexport)
# elif defined(ziti4dotnet_IMPORTS)
#   define Z4D_API __declspec(dllimport)
#endif
#else
#   define Z4D_API /* nothing */
# endif

#ifdef __cplusplus
extern "C" {
#endif

Z4D_API extern int z4d_ziti_close(ziti_connection con);
Z4D_API extern int z4d_uv_run(void* loop);
Z4D_API extern const char** z4d_all_config_types();
Z4D_API extern uv_loop_t* z4d_default_loop();
Z4D_API void* z4d_registerUVTimer(uv_loop_t* loop, uv_timer_cb timer_cb, uint64_t iterations, uint64_t delay);
Z4D_API void passAndPrint(void* anything);
Z4D_API void* newLoop();
Z4D_API int ziti_event_type_from_pointer(const ziti_event_t *event);
Z4D_API ziti_service* ziti_service_array_get(ziti_service_array arr, int idx);

Z4D_API char** make_char_array(int size);
Z4D_API void set_char_at(char **a, char *s, int n);
Z4D_API void free_char_array(char **a, int size);
Z4D_API int char_pointer_len(char *a);
Z4D_API int size_of(void* something);
Z4D_API char* ziti_context_event_err(const ziti_event_t *e);
Z4D_API int ziti_context_event_status(const ziti_event_t *e);

#ifdef __cplusplus
}
#endif
