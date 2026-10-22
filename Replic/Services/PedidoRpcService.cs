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
        Task<(bool ok, string mensaje)> ProbarRemotaAsync(string dblink);
    }

    /// <summary>
    /// Con el dblink recibido de la central elige el string de la base remota y ejecuta
    /// USR_CAD.P_INS_PEDIDO_RPC_REMOTO. La insercion en la central la hace otra API.
    /// </summary>
    public class PedidoRpcService : IPedidoRpcService
    {
        private readonly IRemoteDatabaseResolver _resolver;
        private readonly ILogger<PedidoRpcService> _logger;

        public PedidoRpcService(IRemoteDatabaseResolver resolver, ILogger<PedidoRpcService> logger)
        {
            _resolver = resolver;
            _logger = logger;
        }

        public async Task<PedidoRpcResponse> RegistrarAsync(PedidoRpcRequest r)
        {
            var res = new PedidoRpcResponse
            {
                Dblink = r.Dblink,
                SitioGraba = r.SitioGraba ?? 0,
                NumeLlamada = r.NumeLlamada ?? 0
            };

            if (!_resolver.TryGetConnectionString(r.Dblink, out var remoteConn))
            {
                res.CodigoRemoto = "EAPI_CFG";
                res.MensajeRemoto = $"No hay string de conexion configurado para la base '{r.Dblink}'.";
                res.Mensaje = res.MensajeRemoto;
                return res;
            }

            try
            {
                using var conn = new OracleConnection(remoteConn);
                using var cmd = new OracleCommand("USR_CAD.P_INS_PEDIDO_RPC_REMOTO", conn)
                {
                    CommandType = CommandType.StoredProcedure,
                    BindByName = true
                };
                AddIn(cmd, "p_sitio_graba", OracleDbType.Int64, r.SitioGraba);
                AddIn(cmd, "p_nume_llamada", OracleDbType.Int64, r.NumeLlamada);
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
                AddIn(cmd, "p_prioridad", OracleDbType.Varchar2, r.Prioridad);
                AddIn(cmd, "p_estado", OracleDbType.Varchar2, r.Estado);
                AddIn(cmd, "p_enviar", OracleDbType.Varchar2, r.Enviar);
                AddIn(cmd, "p_cadusua_usuario", OracleDbType.Varchar2, r.CadusuaUsuario);
                AddIn(cmd, "p_cadpoli_cedu_empleado", OracleDbType.Varchar2, r.CadpoliCeduEmpleado);
                AddIn(cmd, "p_total_canales", OracleDbType.Varchar2, r.TotalCanales);
                AddIn(cmd, "p_tipo_genero", OracleDbType.Varchar2, r.TipoGenero);
                AddIn(cmd, "p_dblink", OracleDbType.Varchar2, r.Dblink);
                AddIn(cmd, "p_fuerza", OracleDbType.Int64, r.Fuerza);
                AddIn(cmd, "p_canal", OracleDbType.Int64, r.Canal);

                AddOut(cmd, "p_nro_reg_apl", OracleDbType.Decimal);
                AddOut(cmd, "p_cod_salida", OracleDbType.Varchar2, 200);
                AddOut(cmd, "p_desc_salida", OracleDbType.Varchar2, 4000);

                await conn.OpenAsync();
                await cmd.ExecuteNonQueryAsync();

                res.CodigoRemoto = Str(cmd.Parameters["p_cod_salida"].Value);
                res.MensajeRemoto = Str(cmd.Parameters["p_desc_salida"].Value);
                long nro = ToLong(cmd.Parameters["p_nro_reg_apl"].Value);
                res.InsertadoRemoto = res.CodigoRemoto == "000" && nro > 0;
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error ejecutando el procedimiento en la base remota {Dblink}", r.Dblink);
                res.CodigoRemoto = "EAPI_REMOTA";
                res.MensajeRemoto = ex.Message;
            }

            res.Exito = res.InsertadoRemoto;
            res.Result = res.Exito ? 1 : 0;
            res.Mensaje = res.Exito
                ? $"Pedido {res.SitioGraba}/{res.NumeLlamada} insertado en la base remota {r.Dblink}."
                : $"Fallo la insercion en la base remota {r.Dblink}: {res.MensajeRemoto}";
            return res;
        }

        public async Task<(bool ok, string mensaje)> ProbarRemotaAsync(string dblink)
        {
            if (!_resolver.TryGetConnectionString(dblink, out var cs))
                return (false, $"No hay string de conexion configurado para '{dblink}'.");
            try
            {
                using var conn = new OracleConnection(cs);
                await conn.OpenAsync();
                using var cmd = new OracleCommand("SELECT 1 FROM DUAL", conn);
                await cmd.ExecuteScalarAsync();
                return (true, $"Conexion OK a '{dblink}'.");
            }
            catch (Exception ex)
            {
                return (false, ex.Message);
            }
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
