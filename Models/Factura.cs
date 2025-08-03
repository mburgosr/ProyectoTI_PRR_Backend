using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
using System.Collections.Generic;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Factura
    {
        [Key]
        public int IdFactura { get; set; }

        [Required]
        public string NumeroFactura { get; set; }

        [Required]
        public int PedidoId { get; set; }

        [Required]
        public DateTime Fecha { get; set; }

        [Required]
        public string ClienteCedula { get; set; }

        [Required]
        public string EstadoPago { get; set; }

        public string? Archivo { get; set; }

        public string? Observaciones { get; set; }

        [ForeignKey("PedidoId")]
        public Pedido? Pedido { get; set; }

        [ForeignKey("ClienteCedula")]
        public Cliente? Cliente { get; set; }
        public ICollection<Pago>? Pagos { get; set; }
    }
}