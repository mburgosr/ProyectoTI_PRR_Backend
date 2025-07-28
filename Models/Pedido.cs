using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Pedido
    {
        [Key]
        public int PedidoId { get; set; }

        [Required]
        public string NumeroPedido { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public int CotizacionId { get; set; }

        [Required]
        public string ClienteCedula { get; set; }

        [Required]
        public string EstadoEntrega { get; set; } 

        [Required]
        public string EstadoPago { get; set; }

        [Required]
        public string EstadoPedido { get; set; }

        public int? FacturaId { get; set; }

        public string? Observaciones { get; set; }

        public int? VolquetaId { get; set; }

        // Relaciones
        [ForeignKey("CotizacionId")]
        public Cotizacion? Cotizacion { get; set; }

        [ForeignKey("ClienteCedula")]
        public Cliente? Cliente { get; set; }

        [ForeignKey("VolquetaId")]
        public RegistroVolqueta? Volqueta { get; set; }

        [ForeignKey("FacturaId")]
        public Factura? Factura { get; set; }
    }
}
