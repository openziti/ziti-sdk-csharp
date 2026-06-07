#include <stdio.h>
#include <stddef.h>
#include "ziti/ziti.h"
#include "ziti/ziti_events.h"
#include "ziti4dotnet.h"

/*
 * Emit the native struct layout in a machine-readable form so a generator can produce the C# "expected"
 * constants the alignment test checks against (scripts/generate-native-layout.ps1). The data comes from the
 * native compiler (offsetof/sizeof), i.e. an INDEPENDENT source of truth, never from the managed structs.
 *
 * Records (pipe-delimited, one per line), bracketed by Z4D_LAYOUT_BEGIN / Z4D_LAYOUT_END:
 *   Z4DARCH|<sizeof(void*)>
 *   Z4DSTRUCT|<ctype>|<sizeof>
 *   Z4DFIELD|<ctype>|<field>|<offset>|<fieldsize>
 *
 * sizeof(((T*)0)->F) is a compile-time form (operand is unevaluated), so the null deref is safe.
 */
#define EMIT_STRUCT(T)   printf("Z4DSTRUCT|%s|%zu\n", #T, sizeof(T))
#define EMIT_FIELD(T, F) printf("Z4DFIELD|%s|%s|%zu|%zu\n", #T, #F, (size_t)offsetof(T, F), sizeof(((T*)0)->F))

static void emit_machine_readable_layout(void) {
    printf("\nZ4D_LAYOUT_BEGIN\n");
    printf("Z4DARCH|%zu\n", sizeof(void*));

    EMIT_STRUCT(ziti_id_cfg);
    EMIT_FIELD(ziti_id_cfg, cert);
    EMIT_FIELD(ziti_id_cfg, key);
    EMIT_FIELD(ziti_id_cfg, ca);

    EMIT_STRUCT(ziti_config);
    EMIT_FIELD(ziti_config, controller_url);
    EMIT_FIELD(ziti_config, controllers);
    EMIT_FIELD(ziti_config, id);
    EMIT_FIELD(ziti_config, cfg_source);

    EMIT_STRUCT(api_path);
    EMIT_FIELD(api_path, path);
    EMIT_FIELD(api_path, base_urls);

    EMIT_STRUCT(ziti_api_versions);
    EMIT_FIELD(ziti_api_versions, edge);
    EMIT_FIELD(ziti_api_versions, oidc);

    EMIT_STRUCT(ziti_version);
    EMIT_FIELD(ziti_version, version);
    EMIT_FIELD(ziti_version, revision);
    EMIT_FIELD(ziti_version, build_date);
    EMIT_FIELD(ziti_version, capabilities);
    EMIT_FIELD(ziti_version, api_versions);

    EMIT_STRUCT(ziti_identity);
    EMIT_FIELD(ziti_identity, id);
    EMIT_FIELD(ziti_identity, name);
    EMIT_FIELD(ziti_identity, app_data);

    EMIT_STRUCT(ziti_process);
    EMIT_FIELD(ziti_process, path);

    EMIT_STRUCT(ziti_posture_query);
    EMIT_FIELD(ziti_posture_query, id);
    EMIT_FIELD(ziti_posture_query, is_passing);
    EMIT_FIELD(ziti_posture_query, query_type);
    EMIT_FIELD(ziti_posture_query, process);
    EMIT_FIELD(ziti_posture_query, processes);
    EMIT_FIELD(ziti_posture_query, timeout);
    EMIT_FIELD(ziti_posture_query, timeoutRemaining);
    EMIT_FIELD(ziti_posture_query, updated_at);

    EMIT_STRUCT(ziti_posture_query_set);
    EMIT_FIELD(ziti_posture_query_set, policy_id);
    EMIT_FIELD(ziti_posture_query_set, is_passing);
    EMIT_FIELD(ziti_posture_query_set, policy_type);
    EMIT_FIELD(ziti_posture_query_set, posture_queries);

    EMIT_STRUCT(ziti_service);
    EMIT_FIELD(ziti_service, id);
    EMIT_FIELD(ziti_service, name);
    EMIT_FIELD(ziti_service, permissions);
    EMIT_FIELD(ziti_service, encryption);
    EMIT_FIELD(ziti_service, perm_flags);
    EMIT_FIELD(ziti_service, config);
    EMIT_FIELD(ziti_service, posture_query_set);
    EMIT_FIELD(ziti_service, posture_query_map);
    EMIT_FIELD(ziti_service, terminator_strategy);
    EMIT_FIELD(ziti_service, updated_at);

    EMIT_STRUCT(ziti_address);
    EMIT_FIELD(ziti_address, type);
    EMIT_FIELD(ziti_address, addr);

    EMIT_STRUCT(ziti_client_cfg_v1);
    EMIT_FIELD(ziti_client_cfg_v1, hostname);
    EMIT_FIELD(ziti_client_cfg_v1, port);

    EMIT_STRUCT(ziti_intercept_cfg_v1);
    EMIT_FIELD(ziti_intercept_cfg_v1, protocols);
    EMIT_FIELD(ziti_intercept_cfg_v1, addresses);
    EMIT_FIELD(ziti_intercept_cfg_v1, port_ranges);
    EMIT_FIELD(ziti_intercept_cfg_v1, dial_options);
    EMIT_FIELD(ziti_intercept_cfg_v1, source_ip);
    EMIT_FIELD(ziti_intercept_cfg_v1, allowed_source_addresses);

    EMIT_STRUCT(ziti_server_cfg_v1);
    EMIT_FIELD(ziti_server_cfg_v1, protocol);
    EMIT_FIELD(ziti_server_cfg_v1, hostname);
    EMIT_FIELD(ziti_server_cfg_v1, port);

    EMIT_STRUCT(ziti_listen_options);
    EMIT_FIELD(ziti_listen_options, bind_with_identity);
    EMIT_FIELD(ziti_listen_options, connect_timeout);
    EMIT_FIELD(ziti_listen_options, connect_timeout_seconds);
    EMIT_FIELD(ziti_listen_options, cost);
    EMIT_FIELD(ziti_listen_options, identity);
    EMIT_FIELD(ziti_listen_options, max_connections);
    EMIT_FIELD(ziti_listen_options, precendence);

    EMIT_STRUCT(ziti_host_cfg_v1);
    EMIT_FIELD(ziti_host_cfg_v1, protocol);
    EMIT_FIELD(ziti_host_cfg_v1, forward_protocol);
    EMIT_FIELD(ziti_host_cfg_v1, allowed_protocols);
    EMIT_FIELD(ziti_host_cfg_v1, address);
    EMIT_FIELD(ziti_host_cfg_v1, forward_address);
    EMIT_FIELD(ziti_host_cfg_v1, forward_address_translations);
    EMIT_FIELD(ziti_host_cfg_v1, allowed_addresses);
    EMIT_FIELD(ziti_host_cfg_v1, port);
    EMIT_FIELD(ziti_host_cfg_v1, forward_port);
    EMIT_FIELD(ziti_host_cfg_v1, allowed_port_ranges);
    EMIT_FIELD(ziti_host_cfg_v1, allowed_source_addresses);
    EMIT_FIELD(ziti_host_cfg_v1, proxy);
    EMIT_FIELD(ziti_host_cfg_v1, listen_options);

    EMIT_STRUCT(ziti_host_cfg_v2);
    EMIT_FIELD(ziti_host_cfg_v2, terminators);

    EMIT_STRUCT(ziti_mfa_enrollment);
    EMIT_FIELD(ziti_mfa_enrollment, is_verified);
    EMIT_FIELD(ziti_mfa_enrollment, recovery_codes);
    EMIT_FIELD(ziti_mfa_enrollment, provisioning_url);

    EMIT_STRUCT(ziti_port_range);
    EMIT_FIELD(ziti_port_range, low);
    EMIT_FIELD(ziti_port_range, high);

    EMIT_STRUCT(ziti_proxy_server);
    EMIT_FIELD(ziti_proxy_server, address);
    EMIT_FIELD(ziti_proxy_server, type);

    EMIT_STRUCT(ziti_options);
    EMIT_FIELD(ziti_options, disabled);
    EMIT_FIELD(ziti_options, config_types);
    EMIT_FIELD(ziti_options, api_page_size);
    EMIT_FIELD(ziti_options, refresh_interval);
    EMIT_FIELD(ziti_options, metrics_type);
    EMIT_FIELD(ziti_options, pq_mac_cb);
    EMIT_FIELD(ziti_options, pq_os_cb);
    EMIT_FIELD(ziti_options, pq_process_cb);
    EMIT_FIELD(ziti_options, pq_domain_cb);
    EMIT_FIELD(ziti_options, app_ctx);
    EMIT_FIELD(ziti_options, events);
    EMIT_FIELD(ziti_options, event_cb);
    EMIT_FIELD(ziti_options, cert_extension_window);
    EMIT_FIELD(ziti_options, enroll_mode);

    EMIT_STRUCT(struct ziti_context_event);
    EMIT_FIELD(struct ziti_context_event, ctrl_status);
    EMIT_FIELD(struct ziti_context_event, err);
    EMIT_FIELD(struct ziti_context_event, ctrl_count);
    EMIT_FIELD(struct ziti_context_event, ctrl_details);

    EMIT_STRUCT(struct ziti_router_event);
    EMIT_FIELD(struct ziti_router_event, status);
    EMIT_FIELD(struct ziti_router_event, name);
    EMIT_FIELD(struct ziti_router_event, address);
    EMIT_FIELD(struct ziti_router_event, version);

    EMIT_STRUCT(struct ziti_service_event);
    EMIT_FIELD(struct ziti_service_event, removed);
    EMIT_FIELD(struct ziti_service_event, changed);
    EMIT_FIELD(struct ziti_service_event, added);

    EMIT_STRUCT(struct ziti_auth_event);
    EMIT_FIELD(struct ziti_auth_event, action);
    EMIT_FIELD(struct ziti_auth_event, error);
    EMIT_FIELD(struct ziti_auth_event, error_code);
    EMIT_FIELD(struct ziti_auth_event, type);
    EMIT_FIELD(struct ziti_auth_event, detail);
    EMIT_FIELD(struct ziti_auth_event, providers);

    EMIT_STRUCT(struct ziti_config_event);
    EMIT_FIELD(struct ziti_config_event, identity_name);
    EMIT_FIELD(struct ziti_config_event, config);

    printf("Z4D_LAYOUT_END\n");
}

int main() {
    printf("\noffset output begins\n");
    ziti_types_v2* rtn = z4d_struct_test();
    printf("\noffset output complete\n\n");
    free(rtn);

    emit_machine_readable_layout();
}
