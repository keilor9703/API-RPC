-- BASE CENTRAL: cuando otra API inserta en CADPEDIDOS, se arma el JSON y se envia
-- a nuestra API para que inserte en la base remota que corresponda.
-- Si el envio falla NO se aborta el INSERT original: queda en RPC_API_AUDITORIA (estado 0)
-- y PK_RPC_API.P_REINTENTAR lo reenvia.
CREATE OR REPLACE TRIGGER TRG_CADPEDIDOS_API
  AFTER INSERT ON USR_SECADRPC.CADPEDIDOS
  FOR EACH ROW
DECLARE
  l_campos CLOB;
BEGIN
  l_campos := PK_RPC_API.F_D('horaCaso', :NEW.hora_caso)
    || PK_RPC_API.F_S('dispTelefonico', :NEW.disp_telefonico)
    || PK_RPC_API.F_S('celdaMarcacion', :NEW.celda_marcacion)
    || PK_RPC_API.F_S('numeTelefono', :NEW.nume_telefono)
    || PK_RPC_API.F_S('propTelefono', :NEW.prop_telefono)
    || PK_RPC_API.F_S('direTelefono', :NEW.dire_telefono)
    || PK_RPC_API.F_S('lugeTelefono', :NEW.luge_telefono)
    || PK_RPC_API.F_S('barrTelefono', :NEW.barr_telefono)
    || PK_RPC_API.F_S('tipoPedido', :NEW.tipo_pedido)
    || PK_RPC_API.F_S('caliPedido', :NEW.cali_pedido)
    || PK_RPC_API.F_S('tipoInformacion', :NEW.tipo_informacion)
    || PK_RPC_API.F_S('codigoCooperante', :NEW.codigo_cooperante)
    || PK_RPC_API.F_S('nombLlamante', :NEW.nomb_llamante)
    || PK_RPC_API.F_S('direLlamante', :NEW.dire_llamante)
    || PK_RPC_API.F_S('lugeCodigo', :NEW.luge_codigo)
    || PK_RPC_API.F_S('direCaso', :NEW.dire_caso)
    || PK_RPC_API.F_S('pucaCodigo', :NEW.puca_codigo)
    || PK_RPC_API.F_S('codiBarrio', :NEW.codi_barrio)
    || PK_RPC_API.F_S('lugeBarrio', :NEW.luge_barrio)
    || PK_RPC_API.F_S('latitudCaso', :NEW.latitud_caso)
    || PK_RPC_API.F_S('longitudCaso', :NEW.longitud_caso)
    || PK_RPC_API.F_S('comentario', :NEW.comentario)
    || PK_RPC_API.F_S('codiPedido', :NEW.codi_pedido)
    || PK_RPC_API.F_S('importancia', :NEW.importancia)
    || PK_RPC_API.F_S('prioridad', :NEW.prioridad)
    || PK_RPC_API.F_S('estado', :NEW.estado)
    || PK_RPC_API.F_S('enviar', :NEW.enviar)
    || PK_RPC_API.F_S('cadusuaUsuario', :NEW.cadusua_usuario)
    || PK_RPC_API.F_S('cadpoliCeduEmpleado', :NEW.cadpoli_cedu_empleado)
    || PK_RPC_API.F_S('totalCanales', :NEW.total_canales)
    || PK_RPC_API.F_S('tipoGenero', :NEW.tipo_genero);

  PK_RPC_API.P_ENVIAR_PEDIDO(:NEW.sitio_graba, :NEW.nume_llamada, :NEW.luge_codigo, l_campos);
EXCEPTION
  WHEN OTHERS THEN
    NULL;  -- nunca bloquear el INSERT original; el detalle queda en RPC_API_AUDITORIA cuando aplica
END TRG_CADPEDIDOS_API;
/
