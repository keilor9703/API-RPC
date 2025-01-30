using System;
using System.ComponentModel.DataAnnotations;

namespace Replic.Models
{
    public class Pedido
    {
        public int P_Tipo { get; set; }
        [Key]
        public int P_SitioGraba { get; set; }
        [Key]
        public int P_NumeLlamada { get; set; }
        public string P_HoraCaso { get; set; }
        public string P_DispTelefonico { get; set; }
        public string P_CeldaMarcacion { get; set; }
        public string P_NumeTelefono { get; set; }
        public string P_PropTelefono { get; set; }
        public string P_DireTelefono { get; set; }
        public int P_LugeTelefono { get; set; }
        public string P_BarrTelefono { get; set; }
        public string P_TipoPedido { get; set; }
        public string P_CaliPedido { get; set; }
        public string P_TipoInformacion { get; set; }
        public int? P_CodigoCooperante { get; set; }
        public string P_NombLlamante { get; set; }
        public string P_DireLlamante { get; set; }
        public int P_LugeCodigo { get; set; }
        public string P_DireCaso { get; set; }
        public int? P_PucaCodigo { get; set; }
        public int? P_CodiBarrio { get; set; }
        public int? P_LugeBarrio { get; set; }
        public string P_LatitudCaso { get; set; }
        public string P_LongitudCaso { get; set; }
        public string P_Comentario { get; set; }
        public string P_CodiPedido { get; set; }
        public string P_CodiPedido2 { get; set; }
        public string P_CodiPedidoReal { get; set; }
        public string P_Importancia { get; set; }
        public string P_Prioridad { get; set; }
        public string P_Estado { get; set; }
        public string P_Enviar { get; set; }
        public string P_CadusuaUsuario { get; set; }
        public string P_CadpoliCeduEmpleado { get; set; }
        public int? P_CadPediSitioGraba { get; set; }
        public long? P_CAdPediNumeLlamada { get; set; }

        public string P_FechaGrabacion { get; set; }
        public string P_FechCaso { get; set; }
        public string P_TotalCanales { get; set; }
     
        public string P_TipoGenero { get; set; }
        



    }
}
