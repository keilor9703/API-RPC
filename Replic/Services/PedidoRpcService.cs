using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Replic.Models;
using System;
using System.Data;
using System.Threading.Tasks;

namespace Replic.Services
{
    public interface IPedidoRpcService
    {
        Task<PedidoRpcResponse> RegistrarAsync(PedidoRpcRequest r);
    }

    /// <summary>
    /// 1) Ejecuta P_INS_RPC_VINCULANTE2 en la base central (inserta y devuelve el dblink).
    /// 2) Con el dblink elige el string de la base remota y ejecuta P_INS_PEDIDO_RPC_REMOTO.
    /// 3) Informa a la central el resultado con P_ACTUALIZA_RPC_REMOTO.
    /// </summary>
    public class PedidoRpcService : IPedidoRpcService
    {
        private readonly string _centralConnection;
        private readonly string _paquete;
        private readonly IRemoteDatabaseResolver _resolver;
        private readonly ILogger<PedidoRpcService> _logger;

        public PedidoRpcService(IConfiguration config, IRemoteDatabaseResolver resolver,
                                ILogger<PedidoRpcService> logger)
        {
            _centralConnection = config.GetConnectionString("ConnectionDBoper");
            _paquete = config["CentralPackage"] ?? "PK_SECADRPCP";
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<PedidoRpcResponse> RegistrarAsync(PedidoRpcRequest r)
        {
            var res = new PedidoRpcResponse();

            // ---------------- 1) CENTRAL ----------------
            string dblink;
            long fuerza, canal;
            string prioridad;
            try
            {
                using var conn = new OracleConnection(_centralConnection);
                using var cmd = new OracleCommand($"{_paquete}.P_INS_RPC_VINCULANTE3", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    BindByName = true
                };
                AddIn(cmd, "p_hora_caso", OracleDbType.Date, r.HoraCaso);
                AddIn(cmd, "p_disp_telefonico", OracleDbType.Varchar2, r.DispTelefonico);
                AddIn(cmd, "p_celda_marcacion", OracleDbType.Varchar2, r.CeldaMarcacion);
                AddIn(cmd, "p_nume_telefono", OracleDbType.Int64, r.NumeTelefono);
                AddIn(cmd, "p_prop_telefono", OracleDbType.Varchar2, r.PropTelefono);
                AddIn(cmd, "p_dire_telefono", OracleDbType.Varchar2, r.DireTelefono);
                AddIn(cmd, "p_luge_telefono", OracleDbType.Int64, r.LugeTelefono);
                AddIn(cmd, "p_barr_telefono", OracleDbType.Varchar2, r.BarrTelefono);
                AddIn(cmd, "p_tipo_pedido", OracleDbType.Varchar2, r.TipoPedido);
                AddIn(cmd, "p_cali_pedido", OracleDbType.Varchar2, r.CaliPedido);
                AddIn(cmd, "p_tipo_informacion", OracleDbType.Varchar2, r.TipoInformacion);
                AddIn(cmd, "p_codigo_cooperante", OracleDbType.Int64, r.CodigoCooperante);
                AddIn(cmd, "p_nomb_llamante", OracleDbType.Varchar2, r.NombLlamante);
                AddIn(cmd, "p_dire_llamante", OracleDbType.Varchar2, r.DireLlamante);
                AddIn(cmd, "p_luge_codigo", OracleDbType.Int64, r.LugeCodigo);
                AddIn(cmd, "p_dire_caso", OracleDbType.Varchar2, r.DireCaso);
                AddIn(cmd, "p_puca_codigo", OracleDbType.Int64, r.PucaCodigo);
                AddIn(cmd, "p_codi_barrio", OracleDbType.Int64, r.CodiBarrio);
                AddIn(cmd, "p_luge_barrio", OracleDbType.Int64, r.LugeBarrio);
                AddIn(cmd, "p_latitud_caso", OracleDbType.Varchar2, r.LatitudCaso);
                AddIn(cmd, "p_longitud_caso", OracleDbType.Varchar2, r.LongitudCaso);
                AddIn(cmd, "p_comentario", OracleDbType.Varchar2, r.Comentario);
                AddIn(cmd, "p_codi_pedido", OracleDbType.Varchar2, r.CodiPedido);
                AddIn(cmd, "p_importancia", OracleDbType.Varchar2, r.Importancia);
                AddIn(cmd, "p_prioridad", OracleDbType.Varchar2, r.Prioridad);
                AddIn(cmd, "p_estado", OracleDbType.Varchar2, r.Estado);
                AddIn(cmd, "p_enviar", OracleDbType.Varchar2, r.Enviar);
                AddIn(cmd, "p_cadusua_usuario", OracleDbType.Varchar2, r.CadusuaUsuario);
                AddIn(cmd, "p_cadpoli_cedu_empleado", OracleDbType.Varchar2, r.CadpoliCeduEmpleado);
                AddIn(cmd, "p_cadpedi_sitio_graba", OracleDbType.Int64, r.CadpediSitioGraba);
                AddIn(cmd, "p_cadpedi_nume_llamada", OracleDbType.Int64, r.CadpediNumeLlamada);
                AddIn(cmd, "p_total_canales", OracleDbType.Varchar2, r.TotalCanales);
                AddIn(cmd, "p_tipo_genero", OracleDbType.Varchar2, r.TipoGenero);
                AddIn(cmd, "p_bandera", OracleDbType.Int64, r.Bandera);

                AddOut(cmd, "p_sitio_caso", OracleDbType.Decimal);
                AddOut(cmd, "p_nume_llamada", OracleDbType.Decimal);
                AddOut(cmd, "p_cod_salida", OracleDbType.Varchar2, 200);
                AddOut(cmd, "p_desc_salida", OracleDbType.Varchar2, 4000);
                AddOut(cmd, "p_dblink", OracleDbType.Varchar2, 200);
                AddOut(cmd, "p_fuerza", OracleDbType.Decimal);
                AddOut(cmd, "p_canal", OracleDbType.Decimal);
                AddOut(cmd, "p_prioridad_final", OracleDbType.Varchar2, 10);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                res.SitioGraba = ToLong(cmd.Parameters["p_sitio_caso"].Value);
                res.NumeLlamada = ToLong(cmd.Parameters["p_nume_llamada"].Value);
                res.CodigoCentral = Str(cmd.Parameters["p_cod_salida"].Value);
                res.MensajeCentral = Str(cmd.Parameters["p_desc_salida"].Value);
                dblink = Str(cmd.Parameters["p_dblink"].Value);
                fuerza = ToLong(cmd.Parameters["p_fuerza"].Value);
                canal = ToLong(cmd.Parameters["p_canal"].Value);
                prioridad = Str(cmd.Parameters["p_prioridad_final"].Value);
                res.Dblink = dblink;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando el procedimiento en la base central");
                res.CodigoCentral = "EAPI_CENTRAL";
                res.MensajeCentral = ex.Message;
                res.Mensaje = "Fallo la insercion en la base central.";
                return res;
            }

            res.InsertadoCentral = res.CodigoCentral == "000" && !string.IsNullOrEmpty(dblink);
            if (!res.InsertadoCentral)
            {
                res.Mensaje = "No se inserto en la base central: " + res.MensajeCentral;
                return res;
            }

            // ---------------- 2) REMOTA ----------------
            long provisional = res.NumeLlamada;
            long sitio = res.SitioGraba;
            if (!_resolver.TryGetConnectionString(dblink, out var remoteConn))
            {
                res.CodigoRemoto = "EAPI_CFG";
                res.MensajeRemoto = $"No hay string de conexion configurado para la base '{dblink}'.";
            }
            else
            {
                try
                {
                    using var conn = new OracleConnection(remoteConn);
                    using var cmd = new OracleCommand("PK_SECADRPCP.P_INS_PEDIDO_RPC_REMOTO2", conn)
                    {
                        CommandType = CommandType.StoredProcedure,
                        BindByName = true
                    };
                    AddIn(cmd, "p_sitio_graba", OracleDbType.Int64, sitio);
                    AddIn(cmd, "p_hora_caso", OracleDbType.Date, r.HoraCaso);
                    AddIn(cmd, "p_celda_marcacion", OracleDbType.Varchar2, r.CeldaMarcacion);
                    AddIn(cmd, "p_nume_telefono", OracleDbType.Int64, r.NumeTelefono);
                    AddIn(cmd, "p_disp_telefonico", OracleDbType.Varchar2, r.DispTelefonico);
                    AddIn(cmd, "p_prop_telefono", OracleDbType.Varchar2, r.PropTelefono);
                    AddIn(cmd, "p_dire_telefono", OracleDbType.Varchar2, r.DireTelefono);
                    AddIn(cmd, "p_luge_telefono", OracleDbType.Int64, r.LugeTelefono);
                    AddIn(cmd, "p_barr_telefono", OracleDbType.Varchar2, r.BarrTelefono);
                    AddIn(cmd, "p_tipo_pedido", OracleDbType.Varchar2, r.TipoPedido);
                    AddIn(cmd, "p_cali_pedido", OracleDbType.Varchar2, r.CaliPedido);
                    AddIn(cmd, "p_tipo_informacion", OracleDbType.Varchar2, r.TipoInformacion);
                    AddIn(cmd, "p_codigo_cooperante", OracleDbType.Int64, r.CodigoCooperante);
                    AddIn(cmd, "p_nomb_llamante", OracleDbType.Varchar2, r.NombLlamante);
                    AddIn(cmd, "p_dire_llamante", OracleDbType.Varchar2, r.DireLlamante);
                    AddIn(cmd, "p_luge_codigo", OracleDbType.Int64, r.LugeCodigo);
                    AddIn(cmd, "p_dire_caso", OracleDbType.Varchar2, r.DireCaso);
                    AddIn(cmd, "p_puca_codigo", OracleDbType.Int64, r.PucaCodigo);
                    AddIn(cmd, "p_codi_barrio", OracleDbType.Int64, r.CodiBarrio);
                    AddIn(cmd, "p_luge_barrio", OracleDbType.Int64, r.LugeBarrio);
                    AddIn(cmd, "p_latitud_caso", OracleDbType.Varchar2, r.LatitudCaso);
                    AddIn(cmd, "p_longitud_caso", OracleDbType.Varchar2, r.LongitudCaso);
                    AddIn(cmd, "p_comentario", OracleDbType.Varchar2, r.Comentario);
                    AddIn(cmd, "p_codi_pedido", OracleDbType.Varchar2, r.CodiPedido);
                    AddIn(cmd, "p_importancia", OracleDbType.Varchar2, r.Importancia);
                    AddIn(cmd, "p_prioridad", OracleDbType.Varchar2, prioridad);
                    AddIn(cmd, "p_estado", OracleDbType.Varchar2, r.Estado);
                    AddIn(cmd, "p_enviar", OracleDbType.Varchar2, r.Enviar);
                    AddIn(cmd, "p_cadusua_usuario", OracleDbType.Varchar2, r.CadusuaUsuario);
                    AddIn(cmd, "p_cadpoli_cedu_empleado", OracleDbType.Varchar2, r.CadpoliCeduEmpleado);
                    AddIn(cmd, "p_total_canales", OracleDbType.Varchar2, r.TotalCanales);
                    AddIn(cmd, "p_tipo_genero", OracleDbType.Varchar2, r.TipoGenero);
                    AddIn(cmd, "p_dblink", OracleDbType.Varchar2, dblink);
                    AddIn(cmd, "p_fuerza", OracleDbType.Int64, fuerza);
                    AddIn(cmd, "p_canal", OracleDbType.Int64, canal);

                    AddOut(cmd, "p_nume_llamada", OracleDbType.Decimal);
                    AddOut(cmd, "p_nro_reg_apl", OracleDbType.Decimal);
                    AddOut(cmd, "p_cod_salida", OracleDbType.Varchar2, 200);
                    AddOut(cmd, "p_desc_salida", OracleDbType.Varchar2, 4000);

                    await conn.OpenAsync();
                    await cmd.ExecuteNonQueryAsync();

                    res.CodigoRemoto = Str(cmd.Parameters["p_cod_salida"].Value);
                    res.MensajeRemoto = Str(cmd.Parameters["p_desc_salida"].Value);
                    long nro = ToLong(cmd.Parameters["p_nro_reg_apl"].Value);
                    long nume = ToLong(cmd.Parameters["p_nume_llamada"].Value);
                    res.InsertadoRemoto = res.CodigoRemoto == "000" && nro > 0 && nume > 0;
                    if (res.InsertadoRemoto) res.NumeLlamada = nume;
                }
                catch (Exception ex)
                {
                    _logger.LogError(ex, "Error ejecutando el procedimiento en la base remota {Dblink}", dblink);
                    res.CodigoRemoto = "EAPI_REMOTA";
                    res.MensajeRemoto = ex.Message;
                }
            }

            // ---------------- 3) Dejar el resultado en la central ----------------
            try
            {
                using var conn = new OracleConnection(_centralConnection);
                using var cmd = new OracleCommand($"{_paquete}.P_ACTUALIZA_RPC_REMOTO", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    BindByName = true
                };
                AddIn(cmd, "p_sitio_graba", OracleDbType.Int64, sitio);
                AddIn(cmd, "p_nume_provisional", OracleDbType.Int64, provisional);
                AddIn(cmd, "p_nume_definitivo", OracleDbType.Int64, res.InsertadoRemoto ? res.NumeLlamada : 0);
                AddIn(cmd, "p_cod_remoto", OracleDbType.Varchar2, res.CodigoRemoto);
                AddIn(cmd, "p_desc_remoto", OracleDbType.Varchar2, res.MensajeRemoto);
                AddOut(cmd, "p_cod_salida", OracleDbType.Varchar2, 200);
                AddOut(cmd, "p_desc_salida", OracleDbType.Varchar2, 4000);
                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();
                var cod = Str(cmd.Parameters["p_cod_salida"].Value);
                var msgAct = Str(cmd.Parameters["p_desc_salida"].Value);
                res.CentralActualizada = cod == "000";
                res.MensajeActualizacionCentral = cod + " " + msgAct;
                if (!res.CentralActualizada)
                    _logger.LogWarning("No se pudo actualizar la central: {Cod} {Msg}", cod, msgAct);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error actualizando el resultado remoto en la central");
                res.MensajeActualizacionCentral = ex.Message;
            }

            res.Exito = res.InsertadoCentral && res.InsertadoRemoto && res.CentralActualizada;
            res.Mensaje = res.Exito
                ? "Pedido insertado en la base central y en la base remota " + dblink + "."
                : res.InsertadoRemoto
                ? "Insertado en remota " + dblink + " (llamada " + res.NumeLlamada + ") pero NO se pudo actualizar el numero en la central: " + res.MensajeActualizacionCentral
                : "Insertado en central, pero fallo la base remota " + dblink + ": " + res.MensajeRemoto;
            return res;
        }

        private static void AddIn(OracleCommand cmd, string name, OracleDbType type, object value)
        {
            var p = cmd.Parameters.Add(name, type);
            p.Direction = ParameterDirection.Input;
            p.Value = value ?? DBNull.Value;
        }

        private static void AddOut(OracleCommand cmd, string name, OracleDbType type, int size = 0)
        {
            var p = cmd.Parameters.Add(name, type);
            if (size > 0) p.Size = size;
            p.Direction = ParameterDirection.Output;
        }

        private static string Str(object v) =>
            v is OracleString s ? (s.IsNull ? null : s.Value) : v?.ToString();

        private static long ToLong(object v)
        {
            if (v is OracleDecimal d) return d.IsNull ? 0 : d.ToInt64();
            return v == null || v == DBNull.Value ? 0 : Convert.ToInt64(v);
        }
    }
}
