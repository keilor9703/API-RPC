-- =============================================================================
-- BASES REMOTAS (las 10, mismo script en todas) - esquema USR_CAD.
-- Reemplaza la insercion que antes hacia la central por dblink
-- (P_CALCCONSREMOTO_DBLINK + P_INSPEDIDOSREMOTOS + P_VERIFLLAMADA_DBLINK).
-- Ahora la API se conecta directo a la base remota y ejecuta este procedimiento:
--   * calcula el numero de llamada (consecutivo local por sitio)
--   * inserta en CADPEDIDOS y CADPEDIDOS_CANALES con binds (sin SQL dinamico)
--   * verifica que el registro quedo y devuelve el resultado.
-- Si en tu BD el consecutivo se calcula de otra forma (secuencia/funcion),
-- reemplaza el bloque marcado "CONSECUTIVO".
-- =============================================================================
CREATE OR REPLACE PROCEDURE USR_CAD.P_INS_PEDIDO_RPC_REMOTO(
  p_sitio_graba           IN NUMBER,
  p_hora_caso             IN DATE,
  p_celda_marcacion       IN VARCHAR2,
  p_nume_telefono         IN NUMBER,
  p_disp_telefonico       IN VARCHAR2,
  p_prop_telefono         IN VARCHAR2,
  p_dire_telefono         IN VARCHAR2,
  p_luge_telefono         IN NUMBER,
  p_barr_telefono         IN VARCHAR2,
  p_tipo_pedido           IN VARCHAR2,
  p_cali_pedido           IN VARCHAR2,
  p_tipo_informacion      IN VARCHAR2,
  p_codigo_cooperante     IN NUMBER,
  p_nomb_llamante         IN VARCHAR2,
  p_dire_llamante         IN VARCHAR2,
  p_luge_codigo           IN NUMBER,
  p_dire_caso             IN VARCHAR2,
  p_puca_codigo           IN NUMBER,
  p_codi_barrio           IN NUMBER,
  p_luge_barrio           IN NUMBER,
  p_latitud_caso          IN VARCHAR2,
  p_longitud_caso         IN VARCHAR2,
  p_comentario            IN VARCHAR2,
  p_codi_pedido           IN VARCHAR2,
  p_importancia           IN VARCHAR2,
  p_prioridad             IN VARCHAR2,
  p_estado                IN VARCHAR2,
  p_enviar                IN VARCHAR2,
  p_cadusua_usuario       IN VARCHAR2,
  p_cadpoli_cedu_empleado IN VARCHAR2,
  p_total_canales         IN VARCHAR2,
  p_tipo_genero           IN VARCHAR2,
  p_dblink                IN VARCHAR2,   -- codigo de la base, tal como lo devuelve la central
  p_fuerza                IN NUMBER,
  p_canal                 IN NUMBER,
  p_nume_llamada          OUT NUMBER,    -- numero definitivo asignado en esta base
  p_nro_reg_apl           OUT NUMBER,    -- 1 si el registro quedo grabado
  p_cod_salida            OUT VARCHAR2,
  p_desc_salida           OUT VARCHAR2) IS
  v_sitio      NUMBER;
  v_comentario VARCHAR2(4000);
  v_intentos   PLS_INTEGER := 0;
BEGIN
  p_nume_llamada := 0;
  p_nro_reg_apl  := 0;

  -- Soacha 1 (1228) y Soacha 2 (1229) siempre graban con sitio 1228
  IF p_sitio_graba IN (1228, 1229) THEN
    v_sitio := 1228;
  ELSE
    v_sitio := p_sitio_graba;
  END IF;

  v_comentario := REPLACE(p_comentario, '''', ' ') ||
                  '. Pedido insertado por la Red de Paricipacion ciudadana. RPC con el usuario: ' ||
                  p_cadusua_usuario;

  <<reintento>>
  LOOP
    BEGIN
      -- CONSECUTIVO: siguiente numero de llamada del sitio en esta base
      SELECT NVL(MAX(nume_llamada), 0) + 1
        INTO p_nume_llamada
        FROM usr_cad.cadpedidos
       WHERE sitio_graba = v_sitio;

      INSERT INTO usr_cad.cadpedidos
        (sitio_graba, nume_llamada, hora_caso, disp_telefonico, celda_marcacion,
         nume_telefono, prop_telefono, dire_telefono, luge_telefono, barr_telefono,
         tipo_pedido, cali_pedido, tipo_informacion, codigo_cooperante, nomb_llamante,
         dire_llamante, luge_codigo, dire_caso, puca_codigo, codi_barrio, luge_barrio,
         latitud_caso, longitud_caso, comentario, codi_pedido, importancia, prioridad,
         estado, enviar, cadusua_usuario, cadpoli_cedu_empleado, cadpedi_sitio_graba,
         cadpedi_nume_llamada, fecha_grabacion, fech_caso, total_canales, tipo_genero)
      VALUES
        (v_sitio, p_nume_llamada, p_hora_caso, p_disp_telefonico, p_celda_marcacion,
         p_nume_telefono, REPLACE(p_prop_telefono, '''', ' '),
         REPLACE(p_dire_telefono, '''', ' '), p_luge_telefono, p_barr_telefono,
         p_tipo_pedido, p_cali_pedido, p_tipo_informacion, p_codigo_cooperante,
         REPLACE(p_nomb_llamante, '''', ' '), REPLACE(p_dire_llamante, '''', ' '),
         p_luge_codigo, REPLACE(p_dire_caso, '''', ' '), p_puca_codigo,
         NVL(p_codi_barrio, 0), NVL(p_luge_barrio, 0), p_latitud_caso, p_longitud_caso,
         v_comentario, p_codi_pedido, p_importancia, p_prioridad, p_estado, p_enviar,
         p_dblink,                      -- igual que el flujo anterior: cadusua_usuario = dblink
         p_cadpoli_cedu_empleado, NULL, NULL, SYSDATE, SYSDATE,
         p_total_canales, p_tipo_genero);
      EXIT reintento;
    EXCEPTION
      WHEN DUP_VAL_ON_INDEX THEN
        v_intentos := v_intentos + 1;      -- otra sesion tomo el mismo consecutivo
        IF v_intentos >= 5 THEN
          RAISE;
        END IF;
    END;
  END LOOP reintento;

  INSERT INTO usr_cad.cadpedidos_canales
    (cadpedi_sitiograba, cadpedi_numellamada, cadcana_fuerz_id, cadcana_codigo,
     cali_pedido, comentario, estado, enviar, cadeven_sitio_graba,
     cadeven_nume_evento, fecha_grabacion, fecha_modificacion, usua_modifica)
  VALUES
    (v_sitio, p_nume_llamada, p_fuerza, p_canal, '01',
     'CASO REMITIDO POR SECADRPC. ' || p_cadusua_usuario, 'P', 'S', NULL, NULL,
     SYSDATE, SYSDATE, p_dblink);

  -- Verificar (equivale a P_VERIFLLAMADA_DBLINK)
  SELECT COUNT(*)
    INTO p_nro_reg_apl
    FROM usr_cad.cadpedidos a
   WHERE a.sitio_graba = v_sitio
     AND a.nume_llamada = p_nume_llamada;

  IF p_nro_reg_apl = 0 THEN
    ROLLBACK;
    p_nume_llamada := 0;
    p_cod_salida   := 'E04';
    p_desc_salida  := 'No se inserto el registro.';
  ELSE
    COMMIT;
    p_cod_salida  := '000';
    p_desc_salida := 'Registro Ok.';
  END IF;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    p_nume_llamada := 0;
    p_nro_reg_apl  := 0;
    p_cod_salida   := 'E04';
    p_desc_salida  := SUBSTR(SQLCODE || ' ' || SQLERRM, 1, 500);
END P_INS_PEDIDO_RPC_REMOTO;
/
