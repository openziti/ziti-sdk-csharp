#include <stdlib.h>
#include <string.h>

#include <ziti/ziti.h>
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

Z4D_API extern char** z4d_all_config_types();

Z4D_API extern uv_loop_t* z4d_default_loop();
Z4D_API extern int z4d_uv_run(void* loop);
Z4D_API extern void* z4d_topointer(void* ref);
Z4D_API extern int z4d_ziti_close(ziti_connection con);
//Z4D_API extern int z4d_json_from_ziti_config(ziti_config *cfg, char* buf, size_t maxlen, size_t *len);
Z4D_API extern int z4d_rando(char** char_array, int len, ziti_options* opts);
Z4D_API extern int z4d_rando_set_cfg_types(char** char_array, ziti_options* opts);
//Z4D_API extern int z4d_ziti_enroll(ziti_enroll_opts* opts, uv_loop_t* loop, ziti_enroll_cb enroll_cb, void* enroll_ctx);
Z4D_API void* registerUVTimer(uv_loop_t* loop, uv_timer_cb timer_cb, uint64_t iterations, uint64_t delay);

#ifdef __cplusplus
}
#endif
