using System;
using System.ComponentModel.DataAnnotations;

namespace Replic.Models
{
    /// <summary>
    /// Lo que envia el trigger de la base central (TRG_CADPEDIDOS_API) cuando otra API inserta en CADPEDIDOS.
    /// Los numericos pueden llegar como numero o como texto.
    /// </summary>
    public class PedidoRpcRequest
    {
        // Resueltos por la central (snpalarmaslugares)
        [Required] public string Dblink { get; set; }
        public long? Fuerza { get; set; }
        public long? Canal { get; set; }
        [Required] public long? SitioGraba { get; set; }
        [Required] public long? NumeLlamada { get; set; }

        // Columnas de CADPEDIDOS
        [Required] public DateTime? HoraCaso { get; set; }
        public string DispTelefonico { get; set; }
        public string CeldaMarcacion { get; set; }
        public long? NumeTelefono { get; set; }
        public string PropTelefono { get; set; }
        public string DireTelefono { get; set; }
        public long? LugeTelefono { get; set; }
        public string BarrTelefono { get; set; }
        public string TipoPedido { get; set; }
        public string CaliPedido { get; set; }
        public string TipoInformacion { get; set; }
        public long? CodigoCooperante { get; set; }
        public string NombLlamante { get; set; }
        public string DireLlamante { get; set; }
        [Required] public long? LugeCodigo { get; set; }
        public string DireCaso { get; set; }
        public long? PucaCodigo { get; set; }
        public long? CodiBarrio { get; set; }
        public long? LugeBarrio { get; set; }
        public string LatitudCaso { get; set; }
        public string LongitudCaso { get; set; }
        public string Comentario { get; set; }
        [Required] public string CodiPedido { get; set; }
        public string Importancia { get; set; }
        public string Prioridad { get; set; }
        public string Estado { get; set; }
        public string Enviar { get; set; }
        public string CadusuaUsuario { get; set; }
        public string CadpoliCeduEmpleado { get; set; }
        public string TotalCanales { get; set; }
        public string TipoGenero { get; set; }
    }

    public class PedidoRpcResponse
    {
        /// <summary>1 si quedo insertado en la remota (la central busca "result":1 en la respuesta).</summary>
        public int Result { get; set; }
        public bool Exito { get; set; }
        public bool InsertadoRemoto { get; set; }
        public string Dblink { get; set; }
        public long SitioGraba { get; set; }
        public long NumeLlamada { get; set; }
        public string CodigoRemoto { get; set; }
        public string MensajeRemoto { get; set; }
        public string Mensaje { get; set; }
    }
}
