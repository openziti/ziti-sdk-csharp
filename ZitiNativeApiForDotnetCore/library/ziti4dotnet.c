#include <stdlib.h>
#include <string.h>

#include <ziti/ziti.h>
#include <uv.h>
#include <ziti4dotnet.h>

#if _WIN32
#define strncasecmp _strnicmp
#define strdup _strdup
#endif

int exported_NF_init(const char* config, uv_loop_t* loopIn, ziti_init_cb cb, void* init_ctx)
{
    printf("in exported_NF_init\n");
    char* config_path_copy = strdup(config); //in case the managed memory is collected - duplicate the config
    printf("calling ziti_init\n");
    ziti_init(config_path_copy, loopIn, cb, init_ctx);

    // loop will finish after the request is complete and NF_shutdown is called
    
    printf("calling uv_run\n");
    uv_run(loopIn, UV_RUN_DEFAULT);
    printf("freeing config_path_copy  - should not fire until after the loop exits\n");
    free(config_path_copy); //free the duplicated config path
    printf("========================\n");

    return EXIT_SUCCESS;
}

uv_loop_t* z4d_default_loop()
{
    void* loop = uv_default_loop();
    return uv_default_loop();
}

int z4d_uv_run(void* loop) {
    return uv_run(loop, UV_RUN_DEFAULT);
}

void* z4d_topointer(void* ref) {
    return &ref;
}

int z4d_ziti_close(ziti_connection con) {
    return ziti_close(&con);
}

/*
#include <uv_mbed/um_http.h>
#include <string.h>
#include <uv_mbed/uv_mbed.h>
//#include "common.h"
void resp_cb(um_http_resp_t* resp, void* data) {
    if (resp->code < 0) {
        printf("ERROR: %d(%s)", resp->code, uv_strerror(resp->code));
        exit(-1);
    }
    um_http_hdr* h;
    printf("Response (%d) >>>\nHeaders >>>\n", resp->code);
    LIST_FOREACH(h, &resp->headers, _next) {
        printf("\t%s: %s\n", h->name, h->value);
    }
    printf("\n");
}

void body_cb(um_http_req_t* req, const char* body, ssize_t len) {
    if (len == UV_EOF) {
        printf("\n\n====================\nRequest completed\n");
    }
    else if (len < 0) {
        printf("error(%zd) %s", len, uv_strerror(len));
        exit(-1);
    }
    else {
        printf("%*.*s", len, len, body);
    }
}
*/

/* Ip address used to bind to any port at any interface */
//extern struct sockaddr_in uv_addr_ip4_any_;

int z4d_rando(char** char_array, int len, ziti_options* opts) {

    printf("size of int is: %d\n", sizeof(int));
    printf("size of long is: %d\n", sizeof(long));
    for (int i = 0; i < len; i++) {
        printf("array %d, %s\n", i, char_array[i]);
    }

    printf("from opts: %s\n", opts->controller);

    printf("z4d_rando completes\n\n");
    /*
    uv_mbed_set_debug(5, stdout);
    uv_loop_t* loop = uv_default_loop();

    um_http_t clt;
    um_http_init(loop, &clt, "https://httpbin.org");

    um_http_req_t* r = um_http_req(&clt, "POST", "/post", resp_cb, NULL);
    r->resp.body_cb = body_cb;

    const char* msg = "this is a test";
    um_http_req_data(r, msg, strlen(msg), NULL);

    return uv_run(loop, UV_RUN_DEFAULT);
    */
}
int z4d_rando_set_cfg_types(char** char_array, ziti_options* opts) {
    opts->config_types = char_array;
}

void* registerUVTimer(uv_loop_t * loop, uv_timer_cb timer_cb, uint64_t delay, uint64_t iterations) {
    uv_timer_t * uvt = calloc(1, sizeof(uv_timer_t));
    uv_timer_init(loop, uvt);
    uv_timer_start(uvt, timer_cb, iterations, delay);
    return uvt;
}

const char* ALL_CONFIG_TYPES[] = {
        "all",
        NULL
};
char** z4d_all_config_types() {
    return ALL_CONFIG_TYPES;
}
