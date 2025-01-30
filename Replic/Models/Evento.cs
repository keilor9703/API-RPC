using System;
using System.ComponentModel.DataAnnotations;

namespace Replic.Models
{
    public class Evento
    {

        [Key]
        public int p_sitiograba { get; set; }
        public int p_numeevento { get; set; } 
        public string p_fechcrea { get; set; }
        public string p_horacrea { get; set; }
        public string p_cadpoliceduempleado { get; set; }
        public int p_cadpedcasitiograba { get; set; }
        public int? p_cadpedcanumellamada { get; set; } 
        public int p_cadpedcafuerzaid { get; set; }
        public int p_cadpedcacanal { get; set; }
        public string p_estadoevento { get; set; }
        public string p_tipodespachador { get; set; }
        public int? p_hechconsecutivo { get; set; } 
        public int? p_hechundecodigo { get; set; } 
        public int? p_cadturnturno { get; set; } 
        public int? p_cadturnturnoaccesorio { get; set; } 
        public string p_cadusuaevengenera { get; set; }
        public string p_cadcodifinal { get; set; }
        public string p_cadclcicodigo { get; set; }
    }
    }



