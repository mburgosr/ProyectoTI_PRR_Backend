using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Factura
    {
        [Key] public int IdFactura { get; set; }

        [Required] public DateTime Fecha { get; set; }

        [Required] public string ClienteCedula { get; set; }

        [Required] public string EstadoPago { get; set; }

        public string? Archivo { get; set; }

        [ForeignKey("ClienteCedula")]
        public Cliente? Cliente { get; set; }
    }
}