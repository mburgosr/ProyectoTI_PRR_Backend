using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Factura
    {
        [Key]
        public int IdFactura { get; set; }

        [Required]
        public string NumeroFactura { get; set; }

        [Required]
        public int PedidoId { get; set; } // Foreign key to Pedido

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public string ClienteCedula { get; set; } // Foreign key to Cliente

        [Required]
        public string EstadoPago { get; set; } // Pendiente, Cancelado

        public string? Archivo { get; set; } // Base64 string for PDF/Image

        public string? Observaciones { get; set; }

        // Relaciones
        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [ForeignKey("ClienteCedula")]
        public Cliente? Cliente { get; set; }
    }
}