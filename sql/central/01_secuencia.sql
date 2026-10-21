-- Base CENTRAL (esquema USR_SECADRPC / donde vive el paquete).
-- El numero de llamada definitivo ahora lo asigna la base REMOTA, asi que el
-- registro central se inserta con un numero PROVISIONAL (negativo) y luego se
-- actualiza con P_ACTUALIZA_RPC_REMOTO. Ajustar si CADPEDIDOS tiene otra PK.
CREATE SEQUENCE SEQ_RPC_NUME_LLAMADA_TMP START WITH 1 INCREMENT BY 1 NOCACHE;
