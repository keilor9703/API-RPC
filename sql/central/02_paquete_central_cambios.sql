-- =============================================================================
-- BASE CENTRAL - cambios al paquete (reemplaza en el body los procedimientos
-- P_INS_RPC_VINCULANTE2, y agrega P_ACTUALIZA_RPC_REMOTO).
-- Ya NO se usan dentro del flujo: F_VERIFICADBLINK, P_CALCCONSREMOTO_DBLINK,
-- P_INSPEDIDOSREMOTOS y P_VERIFLLAMADA_DBLINK (la insercion remota la hace la API).
-- Declarar ambas firmas tambien en el SPEC del paquete.
-- =============================================================================

-- ---------------------------------------------------------------------------
-- 1) Inserta SOLO en la central y devuelve a la API el dblink / datos del sitio
-- ---------------------------------------------------------------------------
PROCEDURE P_INS_RPC_VINCULANTE2(p_hora_caso             IN CADPEDIDOS.hora_caso%TYPE,
                                p_disp_telefonico       IN CADPEDIDOS.disp_telefonico%TYPE,
                                p_celda_marcacion       IN CADPEDIDOS.celda_marcacion%TYPE,
                                p_nume_telefono         IN CADPEDIDOS.nume_telefono%TYPE,
                                p_prop_telefono         IN CADPEDIDOS.prop_telefono%TYPE,
                                p_dire_telefono         IN CADPEDIDOS.dire_telefono%TYPE,
                                p_luge_telefono         IN CADPEDIDOS.luge_telefono%TYPE,
                                p_barr_telefono         IN CADPEDIDOS.barr_telefono%TYPE,
                                p_tipo_pedido           IN CADPEDIDOS.tipo_pedido%TYPE,
                                p_cali_pedido           IN CADPEDIDOS.cali_pedido%TYPE,
                                p_tipo_informacion      IN CADPEDIDOS.tipo_informacion%TYPE,
                                p_codigo_cooperante     IN CADPEDIDOS.codigo_cooperante%TYPE,
                                p_nomb_llamante         IN CADPEDIDOS.nomb_llamante%TYPE,
                                p_dire_llamante         IN CADPEDIDOS.dire_llamante%TYPE,
                                p_luge_codigo           IN CADPEDIDOS.luge_codigo%TYPE,
                                p_dire_caso             IN CADPEDIDOS.dire_caso%TYPE,
                                p_puca_codigo           IN CADPEDIDOS.puca_codigo%TYPE,
                                p_codi_barrio           IN CADPEDIDOS.codi_barrio%TYPE,
                                p_luge_barrio           IN CADPEDIDOS.luge_barrio%TYPE,
                                p_latitud_caso          IN CADPEDIDOS.latitud_caso%TYPE,
                                p_longitud_caso         IN CADPEDIDOS.longitud_caso%TYPE,
                                p_comentario            IN CADPEDIDOS.comentario%TYPE,
                                p_codi_pedido           IN CADPEDIDOS.codi_pedido%TYPE,
                                p_importancia           IN CADPEDIDOS.importancia%TYPE,
                                p_prioridad             IN CADPEDIDOS.prioridad%TYPE,
                                p_estado                IN CADPEDIDOS.estado%TYPE,
                                p_enviar                IN CADPEDIDOS.enviar%TYPE,
                                p_cadusua_usuario       IN CADPEDIDOS.cadusua_usuario%TYPE,
                                p_cadpoli_cedu_empleado IN CADPEDIDOS.cadpoli_cedu_empleado%TYPE,
                                p_cadpedi_sitio_graba   IN CADPEDIDOS.cadpedi_sitio_graba%TYPE,
                                p_cadpedi_nume_llamada  IN CADPEDIDOS.cadpedi_nume_llamada%TYPE,
                                p_total_canales         IN CADPEDIDOS.total_canales%TYPE,
                                p_tipo_genero           IN CADPEDIDOS.tipo_genero%TYPE,
                                p_bandera               IN CADPEDIDOS.codigo_cooperante%TYPE,
                                -- salidas (las 4 primeras conservan el contrato anterior)
                                p_sitio_caso            OUT CADPEDIDOS.SITIO_GRABA%TYPE,
                                p_nume_llamada          OUT CADPEDIDOS.NUME_LLAMADA%TYPE, -- provisional (negativo)
                                p_cod_salida            OUT VARCHAR2,
                                p_desc_salida           OUT VARCHAR2,
                                -- salidas NUEVAS para que la API elija la base remota
                                p_dblink                OUT VARCHAR2,
                                p_fuerza                OUT NUMBER,
                                p_canal                 OUT NUMBER,
                                p_prioridad_final       OUT VARCHAR2) IS

  vRegAlarmaLugares USR_CADSINAP.snpalarmaslugares%rowtype;
  vDpto             number := 0;
  v_prioridad       varchar2(2);
  v_consulta        clob;
  vNumeProv         number;
BEGIN
  p_sitio_caso := 0;
  p_nume_llamada := 0;
  p_dblink := NULL;

  -- Traza de la peticion recibida (igual que antes)
  v_consulta := ' CADPEDIDOS.hora_caso:' ||
                to_char(p_hora_caso, 'DD/MM/YYYY HH24:MI:SS') || chr(13) ||
                ' CADPEDIDOS.disp_telefonico:' || to_char(p_disp_telefonico) || chr(13) ||
                ' CADPEDIDOS.celda_marcacion:' || to_char(p_celda_marcacion) || chr(13) ||
                ' CADPEDIDOS.nume_telefono:' || to_char(p_nume_telefono) || chr(13) ||
                ' CADPEDIDOS.prop_telefono:' || to_char(p_prop_telefono) || chr(13) ||
                ' CADPEDIDOS.dire_telefono:' || to_char(p_dire_telefono) || chr(13) ||
                ' CADPEDIDOS.luge_telefono:' || to_char(p_luge_telefono) || chr(13) ||
                ' CADPEDIDOS.barr_telefono:' || to_char(p_barr_telefono) || chr(13) ||
                ' CADPEDIDOS.tipo_pedido:' || to_char(p_tipo_pedido) || chr(13) ||
                ' CADPEDIDOS.cali_pedido: ' || to_char(p_cali_pedido) || chr(13) ||
                ' CADPEDIDOS.tipo_informacion: ' || to_char(p_tipo_informacion) || chr(13) ||
                ' CADPEDIDOS.codigo_cooperante: ' || to_char(p_codigo_cooperante) || chr(13) ||
                ' CADPEDIDOS.nomb_llamante: ' || to_char(p_nomb_llamante) || chr(13) ||
                ' CADPEDIDOS.dire_llamante: ' || to_char(p_dire_llamante) || chr(13) ||
                ' CADPEDIDOS.luge_codigo: ' || to_char(p_luge_codigo) || chr(13) ||
                ' CADPEDIDOS.dire_caso: ' || to_char(p_dire_caso) || chr(13) ||
                ' CADPEDIDOS.puca_codigo: ' || to_char(p_puca_codigo) || chr(13) ||
                ' CADPEDIDOS.codi_barrio: ' || to_char(p_codi_barrio) || chr(13) ||
                ' CADPEDIDOS.luge_barrio: ' || to_char(p_luge_barrio) || chr(13) ||
                ' CADPEDIDOS.latitud_caso: ' || to_char(p_latitud_caso) || chr(13) ||
                ' CADPEDIDOS.longitud_caso: ' || to_char(p_longitud_caso) || chr(13) ||
                ' CADPEDIDOS.comentario: ' || to_char(p_comentario) || chr(13) ||
                ' CADPEDIDOS.codi_pedido: ' || to_char(p_codi_pedido) || chr(13) ||
                ' CADPEDIDOS.importancia: ' || to_char(p_importancia) || chr(13) ||
                ' CADPEDIDOS.prioridad: ' || to_char(p_prioridad) || chr(13) ||
                ' CADPEDIDOS.estado: ' || to_char(p_estado) || chr(13) ||
                ' CADPEDIDOS.enviar:' || to_char(p_enviar) || chr(13) ||
                ' CADPEDIDOS.cadusua_usuario:' || to_char(p_cadusua_usuario) || chr(13) ||
                ' CADPEDIDOS.cadpoli_cedu_empleado:' || to_char(p_cadpoli_cedu_empleado) || chr(13) ||
                ' CADPEDIDOS.cadpedi_sitio_graba:' || to_char(p_cadpedi_sitio_graba) || chr(13) ||
                ' CADPEDIDOS.cadpedi_nume_llamada:' || to_char(p_cadpedi_nume_llamada) || chr(13) ||
                ' CADPEDIDOS.total_canales:' || to_char(p_total_canales) || chr(13) ||
                ' CADPEDIDOS.tipo_genero:' || to_char(p_tipo_genero) || chr(13);
  insert into tabla_test (ID_TEST, FECHA_TEST, consulta, fechaupdate)
  values (SEQ_TEST.NEXTVAL, to_char(p_hora_caso, 'DD/MM/YYYY HH24:MI:SS'), v_consulta, sysdate);
  commit;

  IF p_codi_pedido = '947' THEN
    v_prioridad := '04';
  ELSE
    v_prioridad := '03';
  END IF;
  p_prioridad_final := v_prioridad;

  -- Resolver el lugar -> dblink / sitio de grabacion / fuerza / canal
  begin
    if p_bandera = 1 THEN
      select aa.dpto into vDpto from vm_luge_cad aa where aa.codigo = p_luge_codigo;
      insert into snerr_rmr (descp, SITIO_GB, NUM_LLA, HORA, fechaupdate)
      values (vDpto, 0, 0, to_char(p_hora_caso, 'DD/MM/YYYY HH24:MI:SS'), sysdate);
      commit;
      select * into vRegAlarmaLugares from snpalarmaslugares1 sal where sal.luge_codigo = vDpto;
    else
      select * into vRegAlarmaLugares from usr_cadsinap.snpalarmaslugares sal
       where sal.luge_codigo = p_luge_codigo;
    end if;
  exception
    when no_data_found then
      p_cod_salida  := 'E01';
      p_desc_salida := 'Ciudad no habilitada en alarmas lugares: ' || p_luge_codigo;
      RETURN;
  end;

  if vRegAlarmaLugares.Databaselink is null then
    p_cod_salida  := 'E02';
    p_desc_salida := 'Lugar ' || p_luge_codigo || ' sin dblink/codigo de base remota configurado';
    RETURN;
  end if;

  -- Insercion en la tabla central. La insercion en la base remota la hace la API.
  begin
    select -SEQ_RPC_NUME_LLAMADA_TMP.NEXTVAL into vNumeProv from dual;

    INSERT INTO USR_SECADRPC.CADPEDIDOS
      (sitio_graba, nume_llamada, hora_caso, disp_telefonico, celda_marcacion,
       nume_telefono, prop_telefono, dire_telefono, luge_telefono, barr_telefono,
       tipo_pedido, cali_pedido, tipo_informacion, codigo_cooperante, nomb_llamante,
       dire_llamante, luge_codigo, dire_caso, puca_codigo, codi_barrio, luge_barrio,
       latitud_caso, longitud_caso, comentario, codi_pedido, importancia, prioridad,
       estado, enviar, cadusua_usuario, cadpoli_cedu_empleado, cadpedi_sitio_graba,
       cadpedi_nume_llamada, fecha_grabacion, fech_caso, total_canales, tipo_genero,
       cod_salida, desc_salida)
    VALUES
      (vRegAlarmaLugares.Sitio_Grabacion, vNumeProv, p_hora_caso, p_disp_telefonico,
       p_celda_marcacion, p_nume_telefono, p_prop_telefono, p_dire_telefono,
       p_luge_telefono, p_barr_telefono, p_tipo_pedido, p_cali_pedido,
       p_tipo_informacion, p_codigo_cooperante, p_nomb_llamante, p_dire_llamante,
       p_luge_codigo, p_dire_caso, p_puca_codigo, p_codi_barrio, p_luge_barrio,
       p_latitud_caso, p_longitud_caso, nvl(p_comentario, ' '), p_codi_pedido,
       p_importancia, v_prioridad, p_estado, p_enviar, p_cadusua_usuario,
       p_cadpoli_cedu_empleado, p_cadpedi_sitio_graba, p_cadpedi_nume_llamada,
       sysdate, p_hora_caso, p_total_canales, p_tipo_genero,
       'PEN', 'Insertado en central; pendiente de insercion en base remota ' ||
              vRegAlarmaLugares.Databaselink);
    COMMIT WORK;

    p_sitio_caso   := vRegAlarmaLugares.Sitio_Grabacion;
    p_nume_llamada := vNumeProv;
    p_dblink       := vRegAlarmaLugares.Databaselink;
    p_fuerza       := vRegAlarmaLugares.Fuerza;
    p_canal        := vRegAlarmaLugares.Canal;
    p_cod_salida   := '000';
    p_desc_salida  := 'Insertado en central. Dblink/base remota: ' || p_dblink;
  exception
    when others then
      ROLLBACK;
      p_sitio_caso   := 0;
      p_nume_llamada := 0;
      p_dblink       := NULL;
      p_cod_salida   := SQLCODE;
      p_desc_salida  := SUBSTR('Error insertando en central: ' || SQLERRM, 1, 4000);
      insert into snerr_rmr (descp, SITIO_GB, NUM_LLA, HORA, fechaupdate)
      values (SUBSTR(p_desc_salida, 1, 3000), 0, 0,
              to_char(p_hora_caso, 'DD/MM/YYYY HH24:MI:SS'), sysdate);
      commit;
  end;
END P_INS_RPC_VINCULANTE2;

-- ---------------------------------------------------------------------------
-- 2) La API lo llama despues de insertar en la remota para dejar el numero de
--    llamada definitivo y el resultado en el registro central.
-- ---------------------------------------------------------------------------
PROCEDURE P_ACTUALIZA_RPC_REMOTO(p_sitio_graba      IN NUMBER,
                                 p_nume_provisional IN NUMBER,
                                 p_nume_definitivo  IN NUMBER,   -- 0 si la remota fallo
                                 p_cod_remoto       IN VARCHAR2,
                                 p_desc_remoto      IN VARCHAR2,
                                 p_cod_salida       OUT VARCHAR2,
                                 p_desc_salida      OUT VARCHAR2) IS
BEGIN
  IF p_nume_definitivo <> 0 THEN
    UPDATE USR_SECADRPC.CADPEDIDOS
       SET nume_llamada = p_nume_definitivo,
           cod_salida   = p_cod_remoto,
           desc_salida  = SUBSTR(p_desc_remoto, 1, 4000)
     WHERE sitio_graba = p_sitio_graba
       AND nume_llamada = p_nume_provisional;
  ELSE
    -- se conserva el numero provisional para poder reintentar/auditar
    UPDATE USR_SECADRPC.CADPEDIDOS
       SET cod_salida  = p_cod_remoto,
           desc_salida = SUBSTR(p_desc_remoto, 1, 4000)
     WHERE sitio_graba = p_sitio_graba
       AND nume_llamada = p_nume_provisional;
  END IF;
  IF SQL%ROWCOUNT = 0 THEN
    ROLLBACK;
    p_cod_salida  := 'E05';
    p_desc_salida := 'No se encontro el registro central a actualizar';
  ELSE
    COMMIT;
    p_cod_salida  := '000';
    p_desc_salida := 'Registro central actualizado';
  END IF;
EXCEPTION
  WHEN OTHERS THEN
    ROLLBACK;
    p_cod_salida  := SQLCODE;
    p_desc_salida := SUBSTR(SQLERRM, 1, 500);
END P_ACTUALIZA_RPC_REMOTO;
