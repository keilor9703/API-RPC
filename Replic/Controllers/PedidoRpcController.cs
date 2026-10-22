using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Replic.Models;
using Replic.Services;
using System;
using System.Threading.Tasks;

namespace Replic.Controllers
{
    [Route("api/[controller]")]
    [ApiController]
    public class PedidoRpcController : ControllerBase
    {
        private readonly IPedidoRpcService _service;

        public PedidoRpcController(IPedidoRpcService service)
        {
            _service = service;
        }

        /// <summary>
        /// Lo invoca el trigger de la central. Inserta el pedido en la base remota indicada por "dblink".
        /// 200 con "result":1 = insertado; 502 con "result":0 = fallo en la remota.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PedidoRpcRequest request)
        {
            var res = await _service.RegistrarAsync(request);
            return res.Exito ? Ok(res) : StatusCode(StatusCodes.Status502BadGateway, res);
        }

        /// <summary>Prueba de alcance desde la base central (PK_RPC_API.P_PROBAR_CONECTIVIDAD).</summary>
        [HttpGet("ping")]
        public IActionResult Ping() => Ok(new { ok = true, servicio = "Replic", hora = DateTime.Now });

        /// <summary>Verifica que la API alcanza la base remota de ese dblink.</summary>
        [HttpGet("ping/{dblink}")]
        public async Task<IActionResult> PingRemota(string dblink)
        {
            var (ok, mensaje) = await _service.ProbarRemotaAsync(dblink);
            return ok ? Ok(new { ok, dblink, mensaje })
                      : StatusCode(StatusCodes.Status502BadGateway, new { ok, dblink, mensaje });
        }
    }
}
