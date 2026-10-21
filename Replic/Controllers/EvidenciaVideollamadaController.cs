using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Replic.Context;
using Replic.Models;
using System;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;

namespace Replic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EvidenciaVideollamadaController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EvidenciaVideollamadaController> _logger;

        public EvidenciaVideollamadaController(ApplicationDbContext context, ILogger<EvidenciaVideollamadaController> logger)
        {
            _context = context;
            _logger = logger;
        }

        [HttpPost]
        public async Task<IActionResult> PostEvidencia(EvidenciaVideollamada evidencia)
        {
            try
            {
                using (var conn = new OracleConnection(_context.Database.GetConnectionString()))
                {
                    using (var cmd = new OracleCommand("PK_REPLICACION_DBOPER.ins_evidencia_videollamada", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // AJUSTAR el formato de fecha según cómo la envíe RapidSOS (ISO 8601, epoch, etc.)
                        DateTime fechaVideollamada = DateTime.ParseExact(
                            evidencia.p_fecha_videollamada.Trim(),
                            "dd/MM/yyyy HH:mm:ss",
                            CultureInfo.InvariantCulture);

                        cmd.Parameters.Add("p_fecha_videollamada", OracleDbType.Date).Value = fechaVideollamada;
                        cmd.Parameters.Add("p_caller_id", OracleDbType.Varchar2).Value = evidencia.p_caller_id;
                        cmd.Parameters.Add("p_id_secad", OracleDbType.Varchar2).Value = evidencia.p_id_secad;
                        cmd.Parameters.Add("p_evidence_url", OracleDbType.Varchar2).Value = evidencia.p_evidence_url;

                        cmd.Parameters.Add("SRV_Message", OracleDbType.Clob).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_Resultado", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();

                        int resultado;
                        if (cmd.Parameters["P_Resultado"].Value is OracleDecimal oracleDecimal)
                        {
                            resultado = oracleDecimal.ToInt32();
                        }
                        else
                        {
                            resultado = Convert.ToInt32(cmd.Parameters["P_Resultado"].Value);
                        }

                        OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                        string mensaje = clobMessage.Value;

                        if (resultado == 1)
                        {
                            return Ok(new { Result = resultado, Message = mensaje });
                        }
                        else
                        {
                            return BadRequest(new { Result = resultado, Error = mensaje });
                        }
                    }
                }
            }
            catch (OracleException oex)
            {
                _logger.LogError(oex, "Error de Oracle al registrar evidencia RapidSOS");

                string errorMessage = "Error de Oracle: ";
                foreach (OracleError error in oex.Errors)
                {
                    errorMessage += $"Code: {error.Number}, Message: {error.Message}; ";
                }

                return StatusCode(500, errorMessage);
            }
            catch (Exception ex)
            {
                _logger.LogError(ex, "Error interno al registrar evidencia RapidSOS");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }
    }
}