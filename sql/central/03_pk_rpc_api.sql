-- BASE CENTRAL, esquema USR_SECADRPC.
CREATE OR REPLACE PACKAGE PK_RPC_API IS
  -- Helpers para armar el JSON desde el trigger (devuelven ',"clave":"valor"' o NULL si valor es NULL)
  FUNCTION F_S(p_clave IN VARCHAR2, p_valor IN VARCHAR2) RETURN VARCHAR2;
  FUNCTION F_D(p_clave IN VARCHAR2, p_valor IN DATE) RETURN VARCHAR2;

  -- Resuelve dblink/fuerza/canal por lugar, completa el JSON y lo envia a la API.
  PROCEDURE P_ENVIAR_PEDIDO(p_sitio_graba  IN NUMBER,
                            p_nume_llamada IN NUMBER,
                            p_luge_codigo  IN NUMBER,
                            p_campos       IN CLOB);

  -- Reintenta los envios fallidos (programar con DBMS_SCHEDULER, ej. cada 10 minutos).
  PROCEDURE P_REINTENTAR;

  -- Prueba de alcance a la API (GET <URL_API>/ping). Usar con SET SERVEROUTPUT ON.
  PROCEDURE P_PROBAR_CONECTIVIDAD;
END PK_RPC_API;
/
CREATE OR REPLACE PACKAGE BODY PK_RPC_API IS

  FUNCTION F_ESC(p_valor IN VARCHAR2) RETURN VARCHAR2 IS
    v VARCHAR2(32767) := p_valor;
  BEGIN
    v := REPLACE(v, '\', '\\');
    v := REPLACE(v, '"', '\"');
    v := REPLACE(v, CHR(10), '\n');
    v := REPLACE(v, CHR(13), '\r');
    v := REPLACE(v, CHR(9), '\t');
    RETURN REGEXP_REPLACE(v, '[[:cntrl:]]', ' ');
  END F_ESC;

  FUNCTION F_S(p_clave IN VARCHAR2, p_valor IN VARCHAR2) RETURN VARCHAR2 IS
  BEGIN
    IF p_valor IS NULL THEN RETURN NULL; END IF;
    RETURN ',"' || p_clave || '":"' || F_ESC(p_valor) || '"';
  END F_S;

  FUNCTION F_D(p_clave IN VARCHAR2, p_valor IN DATE) RETURN VARCHAR2 IS
  BEGIN
    IF p_valor IS NULL THEN RETURN NULL; END IF;
    RETURN ',"' || p_clave || '":"' || TO_CHAR(p_valor, 'YYYY-MM-DD"T"HH24:MI:SS') || '"';
  END F_D;

  FUNCTION F_CONFIG(p_clave IN VARCHAR2, p_def IN VARCHAR2 DEFAULT NULL) RETURN VARCHAR2 IS
    v RPC_API_CONFIG.VALOR%TYPE;
  BEGIN
    SELECT VALOR INTO v FROM RPC_API_CONFIG WHERE CLAVE = p_clave;
    RETURN v;
  EXCEPTION WHEN NO_DATA_FOUND THEN RETURN p_def;
  END F_CONFIG;

  -- La auditoria es autonoma: se guarda aunque el INSERT en CADPEDIDOS haga rollback.
  PROCEDURE P_AUDITAR(p_sitio IN NUMBER, p_nume IN NUMBER, p_dblink IN VARCHAR2,
                      p_estado IN NUMBER, p_status IN NUMBER, p_desc IN VARCHAR2,
                      p_json IN CLOB) IS
    PRAGMA AUTONOMOUS_TRANSACTION;
  BEGIN
    INSERT INTO RPC_API_AUDITORIA
      (ID, FECHA_HORA, SITIO_GRABA, NUME_LLAMADA, DBLINK, ESTADO, HTTP_STATUS, DESCRIPCION, JSON)
    VALUES
      (SEQ_RPC_API_AUD.NEXTVAL, SYSDATE, p_sitio, p_nume, p_dblink, p_estado, p_status,
       SUBSTR(p_desc, 1, 4000), p_json);
    COMMIT;
  EXCEPTION WHEN OTHERS THEN ROLLBACK;
  END P_AUDITAR;

  -- POST del JSON a la API. Devuelve estado HTTP y cuerpo de respuesta.
  PROCEDURE P_POST(p_json IN CLOB, p_status OUT NUMBER, p_respuesta OUT CLOB) IS
    l_req  UTL_HTTP.REQ;
    l_resp UTL_HTTP.RESP;
    l_buf  VARCHAR2(32767);
    l_pos  PLS_INTEGER := 1;
    l_len  PLS_INTEGER := DBMS_LOB.GETLENGTH(p_json);
    l_trozo VARCHAR2(8000);
  BEGIN
    UTL_HTTP.SET_TRANSFER_TIMEOUT(TO_NUMBER(F_CONFIG('TIMEOUT_SEG', '30')));
    UTL_HTTP.SET_BODY_CHARSET('UTF-8');
    l_req := UTL_HTTP.BEGIN_REQUEST(F_CONFIG('URL_API'), 'POST', UTL_HTTP.HTTP_VERSION_1_1);
    UTL_HTTP.SET_HEADER(l_req, 'Content-Type', 'application/json; charset=UTF-8');
    UTL_HTTP.SET_HEADER(l_req, 'Transfer-Encoding', 'chunked');   -- evita calcular bytes con tildes
    WHILE l_pos <= l_len LOOP
      l_trozo := DBMS_LOB.SUBSTR(p_json, 4000, l_pos);
      UTL_HTTP.WRITE_TEXT(l_req, l_trozo);
      l_pos := l_pos + 4000;
    END LOOP;
    l_resp := UTL_HTTP.GET_RESPONSE(l_req);
    p_status := l_resp.STATUS_CODE;
    BEGIN
      LOOP
        UTL_HTTP.READ_TEXT(l_resp, l_buf, 32000);
        p_respuesta := p_respuesta || l_buf;
      END LOOP;
    EXCEPTION WHEN UTL_HTTP.END_OF_BODY THEN NULL;
    END;
    UTL_HTTP.END_RESPONSE(l_resp);
  EXCEPTION
    WHEN OTHERS THEN
      BEGIN UTL_HTTP.END_RESPONSE(l_resp); EXCEPTION WHEN OTHERS THEN NULL; END;
      RAISE;
  END P_POST;

  PROCEDURE P_ENVIAR_JSON(p_sitio IN NUMBER, p_nume IN NUMBER, p_dblink IN VARCHAR2, p_json IN CLOB) IS
    l_status NUMBER;
    l_resp   CLOB;
  BEGIN
    P_POST(p_json, l_status, l_resp);
    IF l_status = 200 AND INSTR(l_resp, '"result":1') > 0 THEN
      P_AUDITAR(p_sitio, p_nume, p_dblink, 1, l_status, 'Insertado en remota: ' || SUBSTR(l_resp, 1, 3000), p_json);
    ELSE
      P_AUDITAR(p_sitio, p_nume, p_dblink, 0, l_status, 'No ingresado: ' || SUBSTR(l_resp, 1, 3000), p_json);
    END IF;
  EXCEPTION
    WHEN OTHERS THEN
      P_AUDITAR(p_sitio, p_nume, p_dblink, 0, NULL, 'Fallo la solicitud HTTP: ' || SQLERRM, p_json);
  END P_ENVIAR_JSON;

  PROCEDURE P_ENVIAR_PEDIDO(p_sitio_graba  IN NUMBER,
                            p_nume_llamada IN NUMBER,
                            p_luge_codigo  IN NUMBER,
                            p_campos       IN CLOB) IS
    -- Misma fuente que usaba P_INS_RPC_VINCULANTE2 (bandera = 0).
    v_reg  USR_CADSINAP.snpalarmaslugares%ROWTYPE;
    v_json CLOB;
  BEGIN
    BEGIN
      SELECT * INTO v_reg FROM usr_cadsinap.snpalarmaslugares sal
       WHERE sal.luge_codigo = p_luge_codigo;
    EXCEPTION WHEN NO_DATA_FOUND THEN
      P_AUDITAR(p_sitio_graba, p_nume_llamada, NULL, 0, NULL,
                'E01 Ciudad no habilitada en alarmas lugares: ' || p_luge_codigo, p_campos);
      RETURN;
    END;

    IF v_reg.Databaselink IS NULL THEN
      P_AUDITAR(p_sitio_graba, p_nume_llamada, NULL, 0, NULL,
                'E02 Lugar ' || p_luge_codigo || ' sin dblink/codigo de base remota', p_campos);
      RETURN;
    END IF;

    v_json := '{"dblink":"' || F_ESC(v_reg.Databaselink) || '"' ||
              ',"fuerza":' || NVL(TO_CHAR(v_reg.Fuerza), 'null') ||
              ',"canal":'  || NVL(TO_CHAR(v_reg.Canal), 'null') ||
              ',"sitioGraba":' || p_sitio_graba ||
              ',"numeLlamada":' || p_nume_llamada;
    v_json := v_json || p_campos || '}';

    P_ENVIAR_JSON(p_sitio_graba, p_nume_llamada, v_reg.Databaselink, v_json);
  END P_ENVIAR_PEDIDO;

  PROCEDURE P_REINTENTAR IS
  BEGIN
    -- Ultimo intento por llamada; se reenvia solo si sigue en estado 0 y tiene JSON.
    FOR r IN (SELECT a.SITIO_GRABA, a.NUME_LLAMADA, a.DBLINK, a.JSON
                FROM RPC_API_AUDITORIA a
               WHERE a.ESTADO = 0 AND a.JSON IS NOT NULL AND a.DBLINK IS NOT NULL
                 AND a.ID = (SELECT MAX(b.ID) FROM RPC_API_AUDITORIA b
                              WHERE b.SITIO_GRABA = a.SITIO_GRABA AND b.NUME_LLAMADA = a.NUME_LLAMADA)
                 AND NOT EXISTS (SELECT 1 FROM RPC_API_AUDITORIA c
                                  WHERE c.SITIO_GRABA = a.SITIO_GRABA
                                    AND c.NUME_LLAMADA = a.NUME_LLAMADA AND c.ESTADO = 1)) LOOP
      P_ENVIAR_JSON(r.SITIO_GRABA, r.NUME_LLAMADA, r.DBLINK, r.JSON);
    END LOOP;
  END P_REINTENTAR;

  PROCEDURE P_PROBAR_CONECTIVIDAD IS
    l_req  UTL_HTTP.REQ;
    l_resp UTL_HTTP.RESP;
    l_buf  VARCHAR2(4000);
    l_url  VARCHAR2(600) := F_CONFIG('URL_API') || '/ping';
  BEGIN
    DBMS_OUTPUT.PUT_LINE('URL: ' || l_url);
    UTL_HTTP.SET_TRANSFER_TIMEOUT(10);
    l_req  := UTL_HTTP.BEGIN_REQUEST(l_url, 'GET');
    l_resp := UTL_HTTP.GET_RESPONSE(l_req);
    DBMS_OUTPUT.PUT_LINE('HTTP ' || l_resp.STATUS_CODE || ' ' || l_resp.REASON_PHRASE);
    BEGIN
      UTL_HTTP.READ_TEXT(l_resp, l_buf, 4000);
      DBMS_OUTPUT.PUT_LINE('Respuesta: ' || l_buf);
    EXCEPTION WHEN UTL_HTTP.END_OF_BODY THEN NULL;
    END;
    UTL_HTTP.END_RESPONSE(l_resp);
  EXCEPTION
    WHEN OTHERS THEN
      DBMS_OUTPUT.PUT_LINE('SIN CONECTIVIDAD: ' || SQLERRM);
  END P_PROBAR_CONECTIVIDAD;

END PK_RPC_API;
/
