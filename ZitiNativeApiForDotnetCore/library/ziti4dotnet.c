#include <stdlib.h>

#include <ziti/ziti.h>
#include <uv.h>
#include "ziti4dotnet.h"
#include "ziti/zitilib.h"

#if _WIN32
#define strncasecmp _strnicmp
#define strdup _strdup
#endif

#define MAXBUFFERLEN 4096 * 4096

int z4d_ziti_close(ziti_connection con) {
    return 0;
    //return ziti_close(&con);
}

int z4d_uv_run(void* loop) {
    ZITI_LOG(DEBUG, "running loop with address: %p", loop);
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

void* z4d_stop_uv_timer(uv_timer_t* t) {
    uv_timer_stop(t);
}

void* z4d_new_loop() {
    return uv_loop_new();
}

int z4d_event_type_from_pointer(const ziti_event_t *event) {
    return event->type;
}

ziti_service* z4d_service_array_get(ziti_service_array arr, int idx) {
    return arr ? arr[idx] : NULL;
}

char** z4d_make_char_array(int size) {
    return calloc(sizeof(char*), size + 1);
}

void z4d_set_char_at(char **arr, char *val, int idx) {
    char* dupe = strdup(val);
    arr[idx] = dupe;
}

void z4d_free_char_array(char **a, int size) {
    int i;
    for (i = 0; i < size; i++) {
        free(a[i]);
    }
    free(a);
}

char* ziti_context_event_err(const ziti_event_t *e) {
    if (e && e->event.ctx.err) {
        return e->event.ctx.err;
    } else {
        return NULL;
    }
}
int ziti_context_event_status(const ziti_event_t* e) {
    if (e) {
        return e->event.ctx.err;
    }
    else {
        return 0;
    }
}

typedef int (*printer)(void* arg, const char* fmt, ...);

static int ziti_dump_to_log_op(void* stringsBuilder, const char* fmt, ...) {
    static char line[MAXBUFFERLEN];

    va_list vargs;
    va_start(vargs, fmt);
    vsnprintf(line, sizeof(line), fmt, vargs);
    va_end(vargs);

    // write/append to the buffer
    strncat(stringsBuilder, line, sizeof(line));
    return 0;
}

static int ziti_dump_to_file_op(void* fp, const char* fmt, ...) {
    static char line[MAXBUFFERLEN];

    va_list vargs;
    va_start(vargs, fmt);
    // write/append to file
    vfprintf(fp, fmt, vargs);
    va_end(vargs);

    return 0;
}

void z4d_ziti_dump_log(ziti_context ztx) {
    char* buffer = malloc(MAXBUFFERLEN * sizeof(char));
    buffer[0] = 0;
    ziti_dump(ztx, (printer)ziti_dump_to_log_op, buffer);
    printf("ziti dump to log %s", buffer);
    free(buffer);
}

void z4d_ziti_dump_file(ziti_context ztx, const char* outputFile) {
    FILE* fp;
    fp = fopen(outputFile, "w+");
    if (fp == NULL)
    {
        printf("ziti dump to file failed. Unable to Read / Write / Create File");
        return;
    }
    uv_timeval64_t dump_time;
    uv_gettimeofday(&dump_time);

    char time_str[32];
    struct tm* start_tm = gmtime(&dump_time.tv_sec);
    strftime(time_str, sizeof(time_str), "%a %b %d %Y, %X %p", start_tm);

    fprintf(fp, "Ziti Dump starting: %s\n", time_str);

    //actually invoke ziti_dump here
    ziti_dump(ztx, (printer)ziti_dump_to_file_op, fp);

    fflush(fp);
    fclose(fp);
}


void useUnusedFuncs() {
    //TODO: temporary hack to get the linker to emit 'unused' symbols
    ziti_enroll(NULL, NULL, NULL, NULL);
    ziti_conn_bridge(NULL, NULL, NULL);
    ziti_conn_bridge_fds(NULL, NULL, NULL, NULL, NULL);
    Ziti_lib_init();
}