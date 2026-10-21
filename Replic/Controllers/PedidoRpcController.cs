using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using Replic.Models;
using Replic.Services;
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
        /// Inserta el pedido en la central, y con el dblink devuelto lo inserta en la base remota.
        /// 200 = insertado en ambas; 502 = central OK pero remota fallo; 422 = central rechazo.
        /// </summary>
        [HttpPost]
        public async Task<IActionResult> Post([FromBody] PedidoRpcRequest request)
        {
            var res = await _service.RegistrarAsync(request);
            if (res.Exito) return Ok(res);
            if (res.InsertadoCentral) return StatusCode(StatusCodes.Status502BadGateway, res);
            return UnprocessableEntity(res);
        }
    }
}
