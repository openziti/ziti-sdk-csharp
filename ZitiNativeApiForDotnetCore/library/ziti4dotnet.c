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
    ZITI_LOG(TRACE, "running loop with address: %p", loop);
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

void* newLoop() {
    return uv_loop_new();
}

int ziti_event_type_from_pointer(const ziti_event_t *event) {
    return event->type;
}

ziti_service* ziti_service_array_get(ziti_service_array arr, int idx) {
    return arr ? arr[idx] : NULL;
}

char** make_char_array(int size) {
    return calloc(sizeof(char*), size + 1);
}

void set_char_at(char **arr, char *val, int idx) {
    char* dupe = strdup(val);
    arr[idx] = dupe;
}

void free_char_array(char **a, int size) {
    int i;
    for (i = 0; i < size; i++) {
        free(a[i]);
    }
    free(a);
}

int char_pointer_len(char *a) {
    if (a) {
        printf("LENGTH OF: %s is %d\n", a, strlen(a));
        return (int)strlen(a);
    } else {
        printf(" THE PROVIDED POINTER IS NULL \n");
        return 0;
    }
}

int size_of(void* something) {
    if(!something){
        return 0;
    }
    int s = (int)sizeof(something);
    printf("size of this thing is: %d\n", s);
}

char* ziti_context_event_err(const ziti_event_t *e) {
    if (e && e->event.ctx.err) {
        return e->event.ctx.err;
    } else {
        return NULL;
    }
}
int ziti_context_event_status(const ziti_event_t *e) {
    if (e) {
        return e->event.ctx.err;
    } else {
        return 0;
    }
}