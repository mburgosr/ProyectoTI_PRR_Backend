using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Factura
    {
        [Key]
        public int IdFactura { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        [ForeignKey("Cliente")]
        public string ClienteCedula { get; set; }
        public Cliente Cliente { get; set; }

        [Required]
        [MaxLength(20)]
        public string EstadoPago { get; set; }

        public string ArchivoNombre { get; set; }
    }
}
