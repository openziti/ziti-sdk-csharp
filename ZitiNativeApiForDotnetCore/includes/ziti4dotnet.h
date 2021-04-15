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

Z4D_API extern int z4d_ziti_close(ziti_connection con);
Z4D_API extern int z4d_uv_run(void* loop);
Z4D_API extern const char** z4d_all_config_types();
Z4D_API extern uv_loop_t* z4d_default_loop();
Z4D_API void* z4d_registerUVTimer(uv_loop_t* loop, uv_timer_cb timer_cb, uint64_t iterations, uint64_t delay);
Z4D_API void ziti_init_with_opts(ziti_options* opts, uv_loop_t* loop);

#ifdef __cplusplus
}
#endif
