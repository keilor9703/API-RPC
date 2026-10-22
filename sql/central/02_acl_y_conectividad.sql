-- =============================================================================
-- BASE CENTRAL: permiso de red hacia la API y pruebas de conectividad.
-- Parte A: ejecutar como SYS/DBA.  Parte B: ejecutar como USR_SECADRPC.
-- =============================================================================

-- ---------- PARTE A (DBA): ACL para que USR_SECADRPC pueda salir por UTL_HTTP ----------
BEGIN
  DBMS_NETWORK_ACL_ADMIN.APPEND_HOST_ACE(
    host => 'srvdockergusof.policia.gov.co',
    lower_port => 80, upper_port => 443,          -- cubre 80, 443 y puertos intermedios
    ace  => XS$ACE_TYPE(privilege_list => XS$NAME_LIST('connect', 'resolve'),
                        principal_name => 'USR_SECADRPC',
                        principal_type => XS_ACL.PTYPE_DB));
  COMMIT;
END;
/
-- Si la API publica en otro puerto (ej. 8080, 5000), ajustar lower_port/upper_port.
-- Permiso de ejecucion de los paquetes de red:
-- GRANT EXECUTE ON UTL_HTTP TO USR_SECADRPC;  GRANT EXECUTE ON UTL_INADDR TO USR_SECADRPC;

-- ---------- PARTE B (USR_SECADRPC): comprobar paso a paso que la central alcanza el servidor ----------
SET SERVEROUTPUT ON
-- 1) Resolucion DNS: debe imprimir una IP. Si falla (ORA-29257) -> DNS/ACL 'resolve'.
SELECT UTL_INADDR.GET_HOST_ADDRESS('srvdockergusof.policia.gov.co') AS ip FROM DUAL;

-- 2) Alcance HTTP: despues de crear el paquete PK_RPC_API (03_pk_rpc_api.sql), ejecutar:
--    BEGIN PK_RPC_API.P_PROBAR_CONECTIVIDAD; END;
--    Llama GET <URL_API>/ping y muestra codigo HTTP y respuesta.
--    ORA-24247 = falta la ACL; ORA-12535/12170/29260 = firewall/puerto/red;
--    ORA-29024 = https sin wallet de certificados (usar http o configurar wallet).
