-- BASE CENTRAL, esquema USR_SECADRPC.
-- Configuracion del servicio web y auditoria de cada envio.
CREATE TABLE RPC_API_CONFIG (
  CLAVE VARCHAR2(50)  PRIMARY KEY,
  VALOR VARCHAR2(500) NOT NULL
);

-- Ajustar host/puerto/protocolo segun como quede publicada la API en srvdockergusof.
-- Se recomienda http (o https con wallet configurado: ver 02_acl_y_conectividad.sql).
INSERT INTO RPC_API_CONFIG VALUES ('URL_API', 'http://srvdockergusof.policia.gov.co/api/PedidoRpc');
INSERT INTO RPC_API_CONFIG VALUES ('TIMEOUT_SEG', '30');
COMMIT;

CREATE SEQUENCE SEQ_RPC_API_AUD START WITH 1 INCREMENT BY 1 NOCACHE;

CREATE TABLE RPC_API_AUDITORIA (
  ID           NUMBER PRIMARY KEY,
  FECHA_HORA   DATE DEFAULT SYSDATE NOT NULL,
  SITIO_GRABA  NUMBER,
  NUME_LLAMADA NUMBER,
  DBLINK       VARCHAR2(100),
  ESTADO       NUMBER(1) NOT NULL,      -- 1 = insertado en remota, 0 = fallo/pendiente
  HTTP_STATUS  NUMBER,
  DESCRIPCION  VARCHAR2(4000),
  JSON         CLOB
);
CREATE INDEX IX_RPC_API_AUD_LLAMADA ON RPC_API_AUDITORIA (SITIO_GRABA, NUME_LLAMADA);
