#include <stdlib.h>

#include <ziti/ziti.h>
#include <uv.h>
#include "ziti4dotnet.h"

#if _WIN32
#define strncasecmp _strnicmp
#define strdup _strdup
#endif

int z4d_ziti_close(ziti_connection con) {
    return 0;
    //return ziti_close(&con);
}

int z4d_uv_run(void* loop) {
    printf("I AM RUNNING THE LOOP %p\n", loop);
    return uv_run(loop, UV_RUN_DEFAULT);
}

const char* ALL_CONFIG_TYPES[] = {
        "all",
        NULL
};
extern const char** z4d_all_config_types() {
    return ALL_CONFIG_TYPES;
}

uv_loop_t* z4d_default_loop()
{
    return uv_default_loop();
}

void* z4d_registerUVTimer(uv_loop_t * loop, uv_timer_cb timer_cb, uint64_t delay, uint64_t iterations) {
    uv_timer_t * uvt = calloc(1, sizeof(uv_timer_t));
    uv_timer_init(loop, uvt);
    uv_timer_start(uvt, timer_cb, iterations, delay);
    return uvt;
}

/*void ziti_init_with_opts(ziti_options* opts, uv_loop_t* loop) {
    ziti_init_opts(opts, loop);
}*/

void passAndPrint(void* anything){
    printf("I AM PRINTING THE VALUE HERE: %p\n", anything);
    printf("I AM PRINTING THE VALUE HERE: %p\n", anything);
    printf("I AM PRINTING THE VALUE HERE: %p\n", anything);
}

void* newLoop() {
    return uv_loop_new();
}

//typedef void (*uv_timer_cb)(uv_timer_t* handle);
void cb(uv_timer_t* handle){
    printf("yepyep\n");
}
uv_loop_t *loop;
uv_timer_t gc_req;
uv_timer_t fake_job_req;
uv_timer_t timer_req;
void DoSillyLoop(uv_loop_t * loop){

    uv_timer_init(loop, &timer_req);
    uv_timer_start(&timer_req, cb, 500, 2000);
    printf("I started the timer... for loop %p\n", loop);

}
