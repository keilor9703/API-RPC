using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Microsoft.Extensions.Logging;
using Oracle.ManagedDataAccess.Client;
using Oracle.ManagedDataAccess.Types;
using Replic.Context;
using Replic.Models;
using System.Data;
using System.Globalization;
using System.Threading.Tasks;
using System;
using Microsoft.EntityFrameworkCore;

namespace Replic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class EventosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<EventosController> _logger;

        // Constructor del controlador, recibe el contexto de la aplicación y el logger
        public EventosController(ApplicationDbContext context, ILogger<EventosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método que maneja las peticiones POST en la ruta base api/Eventos
        [HttpPost]
        public async Task<IActionResult> PostEvento(Evento evento)
        {
            try
            {
                // Preparar la conexión y el comando para ejecutar el procedimiento almacenado
                using (var conn = new OracleConnection(_context.Database.GetConnectionString()))
                {
                    // Usando el comando Oracle para ejecutar el procedimiento almacenado específico
                    using (var cmd = new OracleCommand("PK_REPLICA_DBOPER1.ins_cadeventos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Configuración de los parámetros de entrada del procedimiento almacenado
                        cmd.Parameters.Add("p_sitiograba", OracleDbType.Int32).Value = evento.p_sitiograba;
                        cmd.Parameters.Add("p_numeevento", OracleDbType.Int32).Value = evento.p_numeevento;

                        // Normalizar el string que recibe de la base de datos para despues convertirlo en DateTime sin problemas
                        string fechaHoraCadena = evento.p_fechcrea.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime fechaCreacion = DateTime.ParseExact(fechaHoraCadena, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        fechaHoraCadena = evento.p_horacrea.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime horaCreacion = DateTime.ParseExact(fechaHoraCadena, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                        cmd.Parameters.Add("p_fechcrea", OracleDbType.Date).Value = fechaCreacion;
                        cmd.Parameters.Add("p_horacrea", OracleDbType.Date).Value = horaCreacion;
                        cmd.Parameters.Add("p_cadpoliceduempleado", OracleDbType.Varchar2).Value = evento.p_cadpoliceduempleado;
                        cmd.Parameters.Add("p_cadpedcasitiograba", OracleDbType.Int32).Value = evento.p_cadpedcasitiograba;
                        cmd.Parameters.Add("p_cadpedcanumellamada", OracleDbType.Int32).Value = evento.p_cadpedcanumellamada ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadpedcafuerzaid", OracleDbType.Int32).Value = evento.p_cadpedcafuerzaid;
                        cmd.Parameters.Add("p_cadpedcacanal", OracleDbType.Int32).Value = evento.p_cadpedcacanal;
                        cmd.Parameters.Add("p_estadoevento", OracleDbType.Varchar2).Value = evento.p_estadoevento;
                        cmd.Parameters.Add("p_tipodespachador", OracleDbType.Varchar2).Value = evento.p_tipodespachador;
                        cmd.Parameters.Add("p_hechconsecutivo", OracleDbType.Int32).Value = evento.p_hechconsecutivo ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_hechundecodigo", OracleDbType.Int32).Value = evento.p_hechundecodigo ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadturnturno", OracleDbType.Int32).Value = evento.p_cadturnturno ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadturnturnoaccesorio", OracleDbType.Int32).Value = evento.p_cadturnturnoaccesorio ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadusuaevengenera", OracleDbType.Varchar2).Value = evento.p_cadusuaevengenera;
                        cmd.Parameters.Add("p_cadcodifinal", OracleDbType.Varchar2).Value = evento.p_cadcodifinal;
                        cmd.Parameters.Add("p_cadclcicodigo", OracleDbType.Varchar2).Value = evento.p_cadclcicodigo;

                        // Configuración de los parámetros de salida del procedimiento almacenado
                        cmd.Parameters.Add("SRV_Message", OracleDbType.Clob).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_Resultado", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        // Abrir la conexión y ejecutar el comando asincrónicamente
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();

                        // Variables para almacenar el resultado y el mensaje
                        int resultado;
                        string mensaje;

                        // Manejo específico para el parametro de salida P_Resultado, con esto obligo a que este parametro sea convertido en int32
                        if (cmd.Parameters["P_Resultado"].Value is OracleDecimal oracleDecimal)
                        {
                            resultado = oracleDecimal.ToInt32(); // Conversión a Int32 desde OracleDecimal
                        }
                        else
                        {
                            resultado = Convert.ToInt32(cmd.Parameters["P_Resultado"].Value);
                        }

                        // Convertir el valor del parámetro SRV_Message a una cadena de texto.Este parámetro es una salida del procedimiento almacenado que contiene un mensaje detallado
                        // sobre el resultado de la operación. Es convertido a una cadena de texto para su manipulación y posterior retorno como parte de la respuesta HTTP.
                        mensaje = Convert.ToString(cmd.Parameters["SRV_Message"].Value);

                        // Lógica para devolver respuesta HTTP basada en el resultado del procedimiento almacenado
                        if (resultado == 1)
                        {
                            // Si el resultado es exitoso, obtener el mensaje detallado
                            OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                            string value = clobMessage.Value;
                            mensaje = value;

                            //return Ok(new { Message = mensaje }); // Devolver un objeto JSON con el mensaje
                            return Ok(new { Result = resultado, Message = mensaje }); // Devolver un objeto JSON con el resultado y el mensaje
                        }
                        else
                        {
                            //return BadRequest(new { Error = mensaje }); // Devolver un objeto JSON con el mensaje de error
                            // Si el resultado no es exitoso
                            return BadRequest(new { Result = resultado, Error = mensaje }); // Devolver un objeto JSON con el resultado y el mensaje de error
                        }
                    }
                }
            }
            catch (OracleException oex)
            {
                // Captura de excepciones específicas de Oracle
                _logger.LogError(oex, "Error de Oracle al ejecutar procedimiento almacenado");

                // Construcción del mensaje de error detallado para las excepciones de Oracle
                string errorMessage = "Error de Oracle: ";
                foreach (OracleError error in oex.Errors)
                {
                    errorMessage += $"Code: {error.Number}, Message: {error.Message}; ";
                }

                // Devolución de un código de estado 500 con el mensaje de error detallado
                return StatusCode(500, errorMessage);
            }
            catch (Exception ex)
            {
                // Captura de excepciones generales
                _logger.LogError(ex, "Error interno al ejecutar procedimiento almacenado");

                // Devolución de un código de estado 500 con el mensaje de error general
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }

        // Método que maneja las peticiones PUT en la ruta base api/Eventos
        [HttpPut("{sitioGraba}/{numeEvento}")]
        public async Task<IActionResult> PutEvento([FromRoute] int sitioGraba, [FromRoute] int numeEvento, [FromBody] Evento evento)
        {
            // Verificar que los parámetros en la URL coincidan con los datos del cuerpo
            if (sitioGraba != evento.p_sitiograba || numeEvento != evento.p_numeevento)
            {
                return BadRequest("Los parámetros en la URL no coinciden con los datos del cuerpo.");
            }

            try
            {
                using (var conn = new OracleConnection(_context.Database.GetConnectionString()))
                {
                    using (var cmd = new OracleCommand("PK_REPLICA_DBOPER1.Actualizar_CadEventos", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Configuración de los parámetros del procedimiento almacenado
                        cmd.Parameters.Add("p_sitiograba", OracleDbType.Int32).Value = evento.p_sitiograba;
                        cmd.Parameters.Add("p_numeevento", OracleDbType.Int32).Value = evento.p_numeevento;

                        // Normalizar el string que recibe de la base de datos para despues convertirlo en DateTime sin problemas
                        string fechaHoraCadena = evento.p_fechcrea.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime fechaCreacion = DateTime.ParseExact(fechaHoraCadena, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        fechaHoraCadena = evento.p_horacrea.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime horaCreacion = DateTime.ParseExact(fechaHoraCadena, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);

                        cmd.Parameters.Add("p_fechcrea", OracleDbType.Date).Value = fechaCreacion;
                        cmd.Parameters.Add("p_horacrea", OracleDbType.Date).Value = horaCreacion;
                        cmd.Parameters.Add("p_cadpoliceduempleado", OracleDbType.Varchar2).Value = evento.p_cadpoliceduempleado;
                        cmd.Parameters.Add("p_cadpedcasitiograba", OracleDbType.Int32).Value = evento.p_cadpedcasitiograba;
                        cmd.Parameters.Add("p_cadpedcanumellamada", OracleDbType.Int32).Value = evento.p_cadpedcanumellamada ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadpedcafuerzaid", OracleDbType.Int32).Value = evento.p_cadpedcafuerzaid;
                        cmd.Parameters.Add("p_cadpedcacanal", OracleDbType.Int32).Value = evento.p_cadpedcacanal;
                        cmd.Parameters.Add("p_estadoevento", OracleDbType.Varchar2).Value = evento.p_estadoevento;
                        cmd.Parameters.Add("p_tipodespachador", OracleDbType.Varchar2).Value = evento.p_tipodespachador;
                        cmd.Parameters.Add("p_hechconsecutivo", OracleDbType.Int32).Value = evento.p_hechconsecutivo ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_hechundecodigo", OracleDbType.Int32).Value = evento.p_hechundecodigo ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadturnturno", OracleDbType.Int32).Value = evento.p_cadturnturno ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadturnturnoaccesorio", OracleDbType.Int32).Value = evento.p_cadturnturnoaccesorio ?? (object)DBNull.Value;
                        cmd.Parameters.Add("p_cadusuaevengenera", OracleDbType.Varchar2).Value = evento.p_cadusuaevengenera;
                        cmd.Parameters.Add("p_cadcodifinal", OracleDbType.Varchar2).Value = evento.p_cadcodifinal;
                        cmd.Parameters.Add("p_cadclcicodigo", OracleDbType.Varchar2).Value = evento.p_cadclcicodigo;

                        // Parámetros de salida
                        cmd.Parameters.Add("SRV_Message", OracleDbType.Clob).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_Resultado", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();

                        int resultado = 0;
                        string mensaje;

                        // Manejo específico para el parametro de salida P_Resultado, con esto obligo a que este parametro sea convertido en int32
                        if (cmd.Parameters["P_Resultado"].Value is OracleDecimal oracleDecimal)
                        {
                            resultado = oracleDecimal.ToInt32(); // Conversión a Int32 desde OracleDecimal
                        }
                        else
                        {
                            resultado = Convert.ToInt32(cmd.Parameters["P_Resultado"].Value);
                        }

                        // Convertir el valor del parámetro SRV_Message a una cadena de texto.
                        mensaje = Convert.ToString(cmd.Parameters["SRV_Message"].Value);

                        // Lógica para devolver respuesta HTTP basada en el resultado del procedimiento almacenado
                        if (resultado == 1)
                        {
                            OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                            string value = clobMessage.Value;
                            mensaje = value;

                            return Ok(new { Result = resultado, Message = mensaje }); // Devolver un objeto JSON con el resultado y el mensaje
                        }
                        else
                        {
                            OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                            string value = clobMessage.Value;
                            mensaje = value;
                            return BadRequest(new { Result = resultado, Error = mensaje }); // Devolver un objeto JSON con el resultado y el mensaje de error
                        }
                    }
                }
            }
            catch (OracleException oex)
            {
                // Manejo de errores específicos de Oracle
                string errorMessage = "Error de Oracle: ";
                foreach (OracleError error in oex.Errors)
                {
                    errorMessage += $"Code: {error.Number}, Message: {error.Message}; ";
                }

                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = errorMessage });
            }
            catch (Exception ex)
            {
                // Manejo de errores generales
                return StatusCode(StatusCodes.Status500InternalServerError, new { Error = ex.Message });
            }
        }



    }
}
