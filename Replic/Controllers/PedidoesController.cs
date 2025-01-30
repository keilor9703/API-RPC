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
using System.Text.Json;
using System.Threading.Tasks;

namespace Replic.Controllers
{
    
    // Controlador para manejar las peticiones relacionadas con Pedidos
    [Route("api/[controller]")]
    [ApiController]
    public class PedidosController : ControllerBase
    {
        private readonly ApplicationDbContext _context;
        private readonly ILogger<PedidosController> _logger;
        public PedidosController(ApplicationDbContext context, ILogger<PedidosController> logger)
        {
            _context = context;
            _logger = logger;
        }

        // Método que maneja las peticiones POST en la ruta base api/Pedidos
        [HttpPost]
        public async Task<IActionResult> PostPedido(Pedido pedido)
        {
            try
            {
                using (var conn = new OracleConnection(_context.Database.GetConnectionString()))
                {
                    using (var cmd = new OracleCommand("PK_REPLICA_DBOPER1.Ins_CadPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        cmd.Parameters.Add("P_SitioGraba", OracleDbType.Int32).Value = pedido.P_SitioGraba;
                        cmd.Parameters.Add("P_NumeLlamada", OracleDbType.Int32).Value = pedido.P_NumeLlamada;

                        string horCasoCadena = pedido.P_HoraCaso.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime horaCreacion = DateTime.ParseExact(horCasoCadena, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("P_HoraCaso", OracleDbType.Date).Value = horaCreacion;

                        cmd.Parameters.Add("P_DispTelefonico", OracleDbType.Varchar2).Value = pedido.P_DispTelefonico;
                        cmd.Parameters.Add("P_CeldaMarcacion", OracleDbType.Varchar2).Value = pedido.P_CeldaMarcacion ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_NumeTelefono", OracleDbType.Int64).Value = pedido.P_NumeTelefono;
                        cmd.Parameters.Add("P_PropTelefono", OracleDbType.Varchar2).Value = pedido.P_PropTelefono;
                        cmd.Parameters.Add("P_DireTelefono", OracleDbType.Varchar2).Value = pedido.P_DireTelefono;
                        cmd.Parameters.Add("P_LugeTelefono", OracleDbType.Int32).Value = pedido.P_LugeTelefono;
                        cmd.Parameters.Add("P_BarrTelefono", OracleDbType.Varchar2).Value = pedido.P_BarrTelefono;
                        cmd.Parameters.Add("P_TipoPedido", OracleDbType.Varchar2).Value = pedido.P_TipoPedido;
                        cmd.Parameters.Add("P_CaliPedido", OracleDbType.Varchar2).Value = pedido.P_CaliPedido;
                        cmd.Parameters.Add("P_TipoInformacion", OracleDbType.Varchar2).Value = pedido.P_TipoInformacion;
                        cmd.Parameters.Add("P_CodigoCooperante", OracleDbType.Int32).Value = pedido.P_CodigoCooperante ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_NombLlamante", OracleDbType.Varchar2).Value = pedido.P_NombLlamante ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_DireLlamante", OracleDbType.Varchar2).Value = pedido.P_DireLlamante ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LugeCodigo", OracleDbType.Int32).Value = pedido.P_LugeCodigo;
                        cmd.Parameters.Add("P_DireCaso", OracleDbType.Varchar2).Value = pedido.P_DireCaso;
                        cmd.Parameters.Add("P_PucaCodigo", OracleDbType.Int32).Value = pedido.P_PucaCodigo ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_CodiBarrio", OracleDbType.Int32).Value = pedido.P_CodiBarrio ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LugeBarrio", OracleDbType.Int32).Value = pedido.P_LugeBarrio ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LatitudCaso", OracleDbType.Varchar2).Value = pedido.P_LatitudCaso ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LongitudCaso", OracleDbType.Varchar2).Value = pedido.P_LongitudCaso ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_Comentario", OracleDbType.Varchar2).Value = pedido.P_Comentario;
                        cmd.Parameters.Add("P_CodiPedido", OracleDbType.Varchar2).Value = pedido.P_CodiPedido;
                        cmd.Parameters.Add("P_CodiPedido2", OracleDbType.Varchar2).Value = pedido.P_CodiPedido2 ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_CodiPedidoReal", OracleDbType.Varchar2).Value = pedido.P_CodiPedidoReal ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_Importancia", OracleDbType.Varchar2).Value = pedido.P_Importancia;
                        cmd.Parameters.Add("P_Prioridad", OracleDbType.Varchar2).Value = pedido.P_Prioridad;
                        cmd.Parameters.Add("P_Estado", OracleDbType.Varchar2).Value = pedido.P_Estado;
                        cmd.Parameters.Add("P_Enviar", OracleDbType.Varchar2).Value = pedido.P_Enviar;
                        cmd.Parameters.Add("P_CadusuaUsuario", OracleDbType.Varchar2).Value = pedido.P_CadusuaUsuario;
                        cmd.Parameters.Add("P_CadpoliCeduEmpleado", OracleDbType.Varchar2).Value = pedido.P_CadpoliCeduEmpleado;
                        cmd.Parameters.Add("P_CadPediSitioGraba", OracleDbType.Int32).Value = pedido.P_CadPediSitioGraba ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_CAdPediNumeLlamada", OracleDbType.Int32).Value = pedido.P_CAdPediNumeLlamada ?? (object)DBNull.Value;

                        string fechaGrabacionCadena = pedido.P_FechaGrabacion.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime fechaCreacion = DateTime.ParseExact(fechaGrabacionCadena, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        string fechaCasoCadena = pedido.P_FechCaso.Replace(" a. m.", " AM").Replace(" p. m.", " PM").Replace(".", "").Trim();
                        DateTime fechaCasoCreacion = DateTime.ParseExact(fechaCasoCadena, "dd/MM/yyyy h:mm:ss", CultureInfo.InvariantCulture);

                        cmd.Parameters.Add("P_FechaGrabacion", OracleDbType.Date).Value = fechaCreacion;
                        cmd.Parameters.Add("P_FechCaso", OracleDbType.Date).Value = fechaCasoCreacion;

                        cmd.Parameters.Add("P_TotalCanales", OracleDbType.Varchar2).Value = pedido.P_TotalCanales;
                        cmd.Parameters.Add("P_TipoGenero", OracleDbType.Varchar2).Value = pedido.P_TipoGenero ?? (object)DBNull.Value;

                        cmd.Parameters.Add("SRV_Message", OracleDbType.Clob).Direction = ParameterDirection.Output;
                        cmd.Parameters.Add("P_Resultado", OracleDbType.Int32).Direction = ParameterDirection.Output;

                        // Abrir la conexión y ejecutar el comando asincrónicamente
                        await conn.OpenAsync();
                        await cmd.ExecuteNonQueryAsync();

                        // Variables para almacenar el resultado y el mensaje
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

                        // Convertir el valor del parámetro SRV_Message a una cadena de texto.Este parámetro es una salida del procedimiento almacenado que contiene un mensaje detallado
                        // sobre el resultado de la operación. Es convertido a una cadena de texto para su manipulación y posterior retorno como parte de la respuesta HTTP.
                        mensaje = Convert.ToString(cmd.Parameters["SRV_Message"].Value);

                        // Lógica para devolver respuesta HTTP basada en el resultado del procedimiento almacenado
                        if (resultado == 1)
                        {
                            OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                            string value = clobMessage.Value;
                            mensaje = value;
                            return Ok(new { Result = resultado, Message = mensaje });
                        }
                        else
                        {
                            OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                            string value = clobMessage.Value;
                            mensaje = value;
                            return BadRequest(new { Result = resultado, Error = mensaje });
                        }
                    }
                }
            }
            catch (OracleException oex)
            {
                // Captura de excepciones específicas de Oracle
                _logger.LogError(oex, "Error de Oracle al ejecutar procedimiento almacenado");

                string errorMessage = "Error de Oracle: ";
                foreach (OracleError error in oex.Errors)
                {
                    errorMessage += $"Code: {error.Number}, Message: {error.Message}; ";
                }

                return StatusCode(500, errorMessage);
            }
            catch (JsonException jex)
            {
                // Captura de errores de validación de JSON, como el proporcionado en el ejemplo
                _logger.LogError(jex, "Error al procesar el JSON recibido");
                return BadRequest(new
                {
                    Error = "Error en el formato del JSON recibido.",
                    Descripcion = "El servidor recibió un JSON mal formado. " +
                                  "El mensaje de error es: " + jex.Message
                });
            }
            catch (Exception ex)
            {
                // Captura de excepciones generales
                _logger.LogError(ex, "Error interno al ejecutar procedimiento almacenado");
                return StatusCode(500, $"Error interno del servidor: {ex.Message}");
            }
        }


        // Método que maneja las peticiones PUT en la ruta base api/Pedidos
        [HttpPut("{sitioGraba}/{numeLlamada}")]
        public async Task<IActionResult> PutPedido(int sitioGraba, int numeLlamada, [FromBody] Pedido pedido)
        {
            // Verificar que los parámetros en la URL coincidan con los datos del cuerpo
            if (sitioGraba != pedido.P_SitioGraba || numeLlamada != pedido.P_NumeLlamada)
            {
                return BadRequest("Los parámetros en la URL no coinciden con los datos del cuerpo.");
            }

            try
            {
                using (var conn = new OracleConnection(_context.Database.GetConnectionString()))
                {
                    using (var cmd = new OracleCommand("PK_REPLICA_DBOPER1.Actualizar_CadPedido", conn))
                    {
                        cmd.CommandType = CommandType.StoredProcedure;

                        // Configuración de los parámetros del procedimiento almacenado
                        cmd.Parameters.Add("P_SitioGraba", OracleDbType.Int32).Value = pedido.P_SitioGraba;
                        cmd.Parameters.Add("P_NumeLlamada", OracleDbType.Int32).Value = pedido.P_NumeLlamada;
                        cmd.Parameters.Add("P_HoraCaso", OracleDbType.Date).Value = DateTime.ParseExact(pedido.P_HoraCaso, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("P_DispTelefonico", OracleDbType.Varchar2).Value = pedido.P_DispTelefonico;
                        cmd.Parameters.Add("P_CeldaMarcacion", OracleDbType.Varchar2).Value = pedido.P_CeldaMarcacion ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_NumeTelefono", OracleDbType.Int64).Value = pedido.P_NumeTelefono;
                        cmd.Parameters.Add("P_PropTelefono", OracleDbType.Varchar2).Value = pedido.P_PropTelefono;
                        cmd.Parameters.Add("P_DireTelefono", OracleDbType.Varchar2).Value = pedido.P_DireTelefono;
                        cmd.Parameters.Add("P_LugeTelefono", OracleDbType.Int32).Value = pedido.P_LugeTelefono;
                        cmd.Parameters.Add("P_BarrTelefono", OracleDbType.Varchar2).Value = pedido.P_BarrTelefono;
                        cmd.Parameters.Add("P_TipoPedido", OracleDbType.Varchar2).Value = pedido.P_TipoPedido;
                        cmd.Parameters.Add("P_CaliPedido", OracleDbType.Varchar2).Value = pedido.P_CaliPedido;
                        cmd.Parameters.Add("P_TipoInformacion", OracleDbType.Varchar2).Value = pedido.P_TipoInformacion;
                        cmd.Parameters.Add("P_CodigoCooperante", OracleDbType.Int32).Value = pedido.P_CodigoCooperante ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_NombLlamante", OracleDbType.Varchar2).Value = pedido.P_NombLlamante ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_DireLlamante", OracleDbType.Varchar2).Value = pedido.P_DireLlamante;
                        cmd.Parameters.Add("P_LugeCodigo", OracleDbType.Int32).Value = pedido.P_LugeCodigo;
                        cmd.Parameters.Add("P_DireCaso", OracleDbType.Varchar2).Value = pedido.P_DireCaso;
                        cmd.Parameters.Add("P_PucaCodigo", OracleDbType.Int32).Value = pedido.P_PucaCodigo ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_CodiBarrio", OracleDbType.Int32).Value = pedido.P_CodiBarrio ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LugeBarrio", OracleDbType.Int32).Value = pedido.P_LugeBarrio ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LatitudCaso", OracleDbType.Varchar2).Value = pedido.P_LatitudCaso ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_LongitudCaso", OracleDbType.Varchar2).Value = pedido.P_LongitudCaso ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_Comentario", OracleDbType.Varchar2).Value = pedido.P_Comentario;
                        cmd.Parameters.Add("P_CodiPedido", OracleDbType.Varchar2).Value = pedido.P_CodiPedido;
                        cmd.Parameters.Add("P_CodiPedido2", OracleDbType.Varchar2).Value = pedido.P_CodiPedido2 ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_CodiPedidoReal", OracleDbType.Varchar2).Value = pedido.P_CodiPedidoReal ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_Importancia", OracleDbType.Varchar2).Value = pedido.P_Importancia;
                        cmd.Parameters.Add("P_Prioridad", OracleDbType.Varchar2).Value = pedido.P_Prioridad;
                        cmd.Parameters.Add("P_Estado", OracleDbType.Varchar2).Value = pedido.P_Estado;
                        cmd.Parameters.Add("P_Enviar", OracleDbType.Varchar2).Value = pedido.P_Enviar;
                        cmd.Parameters.Add("P_CadusuaUsuario", OracleDbType.Varchar2).Value = pedido.P_CadusuaUsuario;
                        cmd.Parameters.Add("P_CadpoliCeduEmpleado", OracleDbType.Varchar2).Value = pedido.P_CadpoliCeduEmpleado;
                        cmd.Parameters.Add("P_CadPediSitioGraba", OracleDbType.Int32).Value = pedido.P_CadPediSitioGraba ?? (object)DBNull.Value;
                        cmd.Parameters.Add("P_CAdPediNumeLlamada", OracleDbType.Int32).Value = pedido.P_CAdPediNumeLlamada ?? (object)DBNull.Value;

                        cmd.Parameters.Add("P_FechaGrabacion", OracleDbType.Date).Value = DateTime.ParseExact(pedido.P_FechaGrabacion, "dd/MM/yyyy h:mm:ss tt", CultureInfo.InvariantCulture);
                        cmd.Parameters.Add("P_FechCaso", OracleDbType.Date).Value = DateTime.ParseExact(pedido.P_FechCaso, "dd/MM/yyyy h:mm:ss", CultureInfo.InvariantCulture);

                        cmd.Parameters.Add("P_TotalCanales", OracleDbType.Varchar2).Value = pedido.P_TotalCanales;
                        cmd.Parameters.Add("P_TipoGenero", OracleDbType.Varchar2).Value = pedido.P_TipoGenero ?? (object)DBNull.Value;

                        //cmd.Parameters.Add("P_TotalCanales", OracleDbType.Varchar2).Value = pedido.P_TotalCanales;

                        //cmd.Parameters.Add("P_TipoGenero", OracleDbType.Varchar2).Value = pedido.P_TipoGenero ?? (object)DBNull.Value;


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
                            return Ok(new { Result = resultado, Message = mensaje });
                        }
                        else
                        {
                            OracleClob clobMessage = (OracleClob)cmd.Parameters["SRV_Message"].Value;
                            string value = clobMessage.Value;
                            mensaje = value;
                            //return BadRequest(new { Error = mensaje }); // Devolver un objeto JSON con el mensaje de error
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

