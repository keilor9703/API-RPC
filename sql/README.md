# Flujo RPC: trigger en la central -> API -> base remota

1. Otra API inserta en `USR_SECADRPC.CADPEDIDOS` (central).
2. `TRG_CADPEDIDOS_API` (AFTER INSERT) arma los campos y llama `PK_RPC_API.P_ENVIAR_PEDIDO`.
3. El paquete busca dblink/fuerza/canal en `usr_cadsinap.snpalarmaslugares` por `luge_codigo`, arma el JSON y
   hace `POST` (UTL_HTTP) a `RPC_API_CONFIG.URL_API` (`http://srvdockergusof.policia.gov.co/api/PedidoRpc`).
4. La API (`Replic`) elige el string de `RemoteDatabases` segun `dblink` y ejecuta `USR_CAD.P_INS_PEDIDO_RPC_REMOTO`
   en la base remota, usando el mismo sitio y numero de llamada de la central. Responde `"result":1` si quedo.
5. Cada envio queda en `RPC_API_AUDITORIA` (estado 1 ok / 0 fallo). `PK_RPC_API.P_REINTENTAR` reenvia los fallidos.
   Un fallo nunca revierte el INSERT original en CADPEDIDOS.

## Despliegue
Central: `01_tablas.sql`, `02_acl_y_conectividad.sql` (parte A como DBA), `03_pk_rpc_api.sql`, `04_trigger_cadpedidos.sql`.
Remotas (las 10): `sql/remoto/P_INS_PEDIDO_RPC_REMOTO.sql`.
API: publicar en srvdockergusof con `ASPNETCORE_URLS` (ej. `http://+:80`) y los strings en `RemoteDatabases`.

## Verificar que la central alcanza el servidor
1. `SELECT UTL_INADDR.GET_HOST_ADDRESS('srvdockergusof.policia.gov.co') FROM DUAL;`
2. `SET SERVEROUTPUT ON; BEGIN PK_RPC_API.P_PROBAR_CONECTIVIDAD; END;` (llama `GET /api/PedidoRpc/ping`).
3. Desde la API: `GET /api/PedidoRpc/ping/{dblink}` comprueba la conexion a esa base remota.
