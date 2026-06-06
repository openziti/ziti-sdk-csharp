#include "ziti/ziti.h"

#define OFFSET(TYPE, MEMBER) printf("\noffset of " #TYPE "." #MEMBER ": %zu", offsetof(TYPE, MEMBER));

int main() {
    printf("\noffset output begins\n");
    ziti_auth_query_mfa z = {0};
    z.type_id = "type";
    OFFSET(ziti_auth_query_mfa, type_id);
    z.provider = "provider";
    OFFSET(ziti_auth_query_mfa, provider);
    z.http_method = "http_method";
    OFFSET(ziti_auth_query_mfa, http_method);
    z.http_url = "http_url";
    OFFSET(ziti_auth_query_mfa, http_url);
    z.min_length = 1;
    OFFSET(ziti_auth_query_mfa, min_length);
    z.max_length = 2;
    OFFSET(ziti_auth_query_mfa, max_length);
    z.format = "format";
    OFFSET(ziti_auth_query_mfa, format);

    ziti_id_cfg idcfg = {0};
    idcfg.cert = "cert";
    OFFSET(ziti_id_cfg, cert);
    idcfg.key = "key";
    OFFSET(ziti_id_cfg, key);
    idcfg.ca = "ca";
    OFFSET(ziti_id_cfg, ca);

    ziti_config config = {0};
    config.controller_url = "controller_url";
    OFFSET(ziti_config, controller_url);
    config.id = idcfg;
    OFFSET(ziti_config, id);

    api_path apip = {0};
    apip.path = "path";
    OFFSET(api_path, path);

    ziti_api_versions zav = {0};
    model_map edgemap = {0};
    zav.edge = edgemap;
    OFFSET(ziti_api_versions, edge);

    ziti_version v = {0};
    v.version = "version";
    OFFSET(ziti_version, version);
    v.revision = "revision";
    OFFSET(ziti_version, revision);
    v.build_date = "build_date";
    OFFSET(ziti_version, build_date);
    v.api_versions = &zav;
    OFFSET(ziti_version, api_versions);

    ziti_identity zi = {0};
    zi.id = "id";
    OFFSET(ziti_identity, id);
    zi.name = "name";
    OFFSET(ziti_identity, name);
    model_map json_model_map = {0};
    zi.app_data = json_model_map;
    OFFSET(ziti_identity, app_data);

    ziti_process zproc = {0};
    zproc.path = "path";
    OFFSET(ziti_process, path);

    ziti_posture_query zpq = {0};
    zpq.id = "id";
    OFFSET(ziti_posture_query, id);
    zpq.is_passing = true;
    OFFSET(ziti_posture_query, is_passing);
    zpq.query_type = "query_type";
    OFFSET(ziti_posture_query, query_type);
    zpq.process = &zproc;
    OFFSET(ziti_posture_query, process);
    ziti_process_array zpa = {0};
    zpq.processes = zpa;
    OFFSET(ziti_posture_query, processes);
    zpq.timeout = 10;
    OFFSET(ziti_posture_query, timeout);
    int timeremain = 20;
    zpq.timeoutRemaining = &timeremain;
    OFFSET(ziti_posture_query, timeoutRemaining);
    zpq.updated_at = "updated_at";
    OFFSET(ziti_posture_query, updated_at);

    ziti_posture_query_set zpqs = {0};
    zpqs.policy_id = "policy_id";
    OFFSET(ziti_posture_query_set, policy_id);
    zpqs.is_passing = true;
    OFFSET(ziti_posture_query_set, is_passing);
    zpqs.policy_type = "policy_type";
    OFFSET(ziti_posture_query_set, policy_type);
    ziti_posture_query_array zpqa = {0};
    zpqs.posture_queries = zpqa;
    OFFSET(ziti_posture_query_set, posture_queries);

    ziti_posture_query_set_array zpqsa = calloc(sizeof(ziti_posture_query_set), 2);
    zpqsa[0] = &zpqs;
    ziti_session_type_array zsta = calloc(sizeof(ziti_session_type), 2);
    ziti_session_type st = {0};

    ziti_service zs = {0};
    zs.id = "id";
    OFFSET(ziti_service, id);
    zs.name = "name";
    OFFSET(ziti_service, name);
    st = ziti_session_type_Bind;
    zsta[0] = &st;
    zs.permissions = zsta;
    OFFSET(ziti_service, permissions);
    zs.encryption = "encryption";
    OFFSET(ziti_service, encryption);
    zs.perm_flags = 255;
    OFFSET(ziti_service, perm_flags);
    zs.config = json_model_map;
    OFFSET(ziti_service, config);
    zs.posture_query_set = zpqsa;
    OFFSET(ziti_service, posture_query_set);
    model_map zpqm = {0};
    zs.posture_query_map = zpqm;
    OFFSET(ziti_service, posture_query_map);
    zs.updated_at = "updated_at";
    OFFSET(ziti_service, updated_at);

    ziti_address zahost = {0};
    zahost.type = ziti_address_hostname;
    OFFSET(ziti_address, type);
    strncpy(zahost.addr.hostname,"hostname",9);
    OFFSET(ziti_address, addr.hostname);
    ziti_address zacidr = {0};
    zacidr.type = ziti_address_cidr;
    OFFSET(ziti_address, type);
    zacidr.addr.cidr.af = AF_INET;
    OFFSET(ziti_address, addr.cidr.af);
    zacidr.addr.cidr.bits = 8;
    OFFSET(ziti_address, addr.cidr.bits);

    //in6_addr v6addr = {0};
    //zacidr.addr.cidr.ip = "ip";

    ziti_client_cfg_v1 v1 = {0};
    v1.hostname = zahost;
    OFFSET(ziti_client_cfg_v1, hostname);
    v1.port = 80;
    OFFSET(ziti_client_cfg_v1, port);

    ziti_port_range zpr = {0};
    zpr.low = 80;
    OFFSET(ziti_port_range, low);
    zpr.high = 443;
    OFFSET(ziti_port_range, high);

    ziti_port_range pr = {0};
    pr.low = 80;
    OFFSET(ziti_port_range, low);
    pr.high = 442;
    OFFSET(ziti_port_range, high);

    ziti_intercept_cfg_v1 zicv1 = {0};
    model_list protos = {0};
    ziti_protocol zp = ziti_protocol_tcp;
    model_list_append(&protos, &zp);
    zicv1.protocols = protos;
    OFFSET(ziti_intercept_cfg_v1, protocols);
    model_list zads = {0};
    model_list_append(&zads, &zahost);
    model_list_append(&zads, &zacidr);
    zicv1.addresses = zads;
    OFFSET(ziti_intercept_cfg_v1, addresses);
    model_list portranges = {0};
    model_list_append(&portranges, &pr);
    zicv1.port_ranges = portranges;
    OFFSET(ziti_intercept_cfg_v1, port_ranges);
    model_map dopts = {0};
    model_map_set(&dopts, "key", "value");
    zicv1.dial_options = dopts;
    OFFSET(ziti_intercept_cfg_v1, dial_options);
    zicv1.source_ip = "source_ip";
    OFFSET(ziti_intercept_cfg_v1, source_ip);

    ziti_server_cfg_v1 zscfgv1 = {0};
    zscfgv1.protocol = "protocol";
    OFFSET(ziti_server_cfg_v1, protocol);
    zscfgv1.hostname = "hostname";
    OFFSET(ziti_server_cfg_v1, hostname);
    zscfgv1.port = 443;
    OFFSET(ziti_server_cfg_v1, port);

    ziti_listen_options lopts = {0};
    lopts.bind_with_identity = true;
    OFFSET(ziti_listen_options, bind_with_identity);
    duration d = 1000000;
    lopts.connect_timeout = d;
    OFFSET(ziti_listen_options, connect_timeout);
    lopts.connect_timeout_seconds = 100;
    OFFSET(ziti_listen_options, connect_timeout_seconds);
    lopts.cost = 2;
    OFFSET(ziti_listen_options, cost);
    lopts.identity = "identity";
    OFFSET(ziti_listen_options, identity);
    lopts.max_connections = 10;
    OFFSET(ziti_listen_options, max_connections);
    lopts.precendence = "precedence";
    OFFSET(ziti_listen_options, precendence);

    ziti_host_cfg_v1 zhcv1 = {0};
    zhcv1.protocol = "protocol";
    OFFSET(ziti_host_cfg_v1, protocol);
    zhcv1.forward_protocol = true;
    OFFSET(ziti_host_cfg_v1, forward_protocol);
    string_array apa = calloc(sizeof(char*), 3);
    apa[0] = "proto1";
    apa[1] = "proto2";
    zhcv1.allowed_protocols = apa;
    OFFSET(ziti_host_cfg_v1, allowed_protocols);
    zhcv1.address = "address";
    OFFSET(ziti_host_cfg_v1, address);
    zhcv1.forward_address = true;
    OFFSET(ziti_host_cfg_v1, forward_address);
    ziti_address_array allowadds = calloc(sizeof(ziti_address), 2);
    allowadds[0] = &zahost;
    zhcv1.allowed_addresses = allowadds;
    OFFSET(ziti_host_cfg_v1, allowed_addresses);
    zhcv1.port = 1090;
    OFFSET(ziti_host_cfg_v1, port);
    zhcv1.forward_port = true;
    OFFSET(ziti_host_cfg_v1, forward_port);
    ziti_port_range_array zpra = calloc(sizeof(ziti_port_range), 2);
    zpra[0] = &zpr;
    zhcv1.allowed_port_ranges = zpra;
    OFFSET(ziti_host_cfg_v1, allowed_port_ranges);
    zhcv1.allowed_source_addresses = allowadds;
    OFFSET(ziti_host_cfg_v1, allowed_source_addresses);
    zhcv1.listen_options = &lopts;
    OFFSET(ziti_host_cfg_v1, listen_options);

    ziti_host_cfg_v2 hv2 = {0};
    model_list terms = {0};
    model_list_append(&terms, &hv2);
    hv2.terminators = terms;
    OFFSET(ziti_host_cfg_v2, terminators);

    ziti_mfa_enrollment mfae = {0};
    mfae.is_verified = true;
    OFFSET(ziti_mfa_enrollment, is_verified);
    string_array codes = calloc(sizeof(char*), 2);
    codes[0] = "code1";
    mfae.recovery_codes = codes;
    OFFSET(ziti_mfa_enrollment, recovery_codes);
    mfae.provisioning_url = "provisioningUrl";
    OFFSET(ziti_mfa_enrollment, provisioning_url);

    printf("\noffset output complete\n\n");
}






