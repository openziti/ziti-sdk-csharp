#include "ziti/ziti.h"
#include "ziti4dotnet.h"

int main() {
    printf("\noffset output begins\n");
    ziti_types_t* rtn = z4d_struct_test();
    printf(rtn->ziti_posture_query_set_checksum);
    char* r = model_map_get(&rtn->ziti_service.config, "config_key1");
    printf("\noffset output complete\n\n");
}
