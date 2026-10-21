using System;
using System.ComponentModel.DataAnnotations;

namespace Replic.Models
{
    /// <summary>Parametros de entrada de P_INS_RPC_VINCULANTE2 (base central).</summary>
    public class PedidoRpcRequest
    {
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
        public long? CadpediSitioGraba { get; set; }
        public long? CadpediNumeLlamada { get; set; }
        public string TotalCanales { get; set; }
        public string TipoGenero { get; set; }
        public long Bandera { get; set; }
    }

    public class PedidoRpcResponse
    {
        /// <summary>true solo si se inserto en la central Y en la remota.</summary>
        public bool Exito { get; set; }
        public bool InsertadoCentral { get; set; }
        public bool InsertadoRemoto { get; set; }
        public string Dblink { get; set; }
        public long SitioGraba { get; set; }
        public long NumeLlamada { get; set; }
        public string CodigoCentral { get; set; }
        public string MensajeCentral { get; set; }
        public string CodigoRemoto { get; set; }
        public string MensajeRemoto { get; set; }
        /// <summary>true si el numero de llamada definitivo quedo grabado en la central.</summary>
        public bool CentralActualizada { get; set; }
        public string MensajeActualizacionCentral { get; set; }
        public string Mensaje { get; set; }
    }
}
