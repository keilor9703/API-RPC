# Flujo RPC con base central + bases remotas

1. `POST /api/PedidoRpc` -> la API ejecuta `P_INS_RPC_VINCULANTE2` en la central (`sql/central`).
   Inserta en `USR_SECADRPC.CADPEDIDOS` (nº de llamada provisional negativo) y devuelve `p_dblink`, `p_fuerza`, `p_canal`.
2. Con `p_dblink` la API busca el string en `RemoteDatabases` (appsettings) y ejecuta `USR_CAD.P_INS_PEDIDO_RPC_REMOTO` (`sql/remoto`, igual en las 10 bases).
3. La API llama `P_ACTUALIZA_RPC_REMOTO` en la central con el nº de llamada definitivo y el resultado.

Respuesta: `exito`, `insertadoCentral`, `insertadoRemoto` (+ códigos/mensajes de cada lado).
Orden de despliegue: `01_secuencia.sql`, `02_paquete_central_cambios.sql` (integrar en spec/body del paquete), script remoto en las 10 bases.
