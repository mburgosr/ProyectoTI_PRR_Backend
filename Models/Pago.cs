using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Pago
    {
        [Key]
        public int IdPago { get; set; }
        public int FacturaId { get; set; }
        public decimal Monto { get; set; }
        public DateTime FechaPago { get; set; }
        public string Comprobante { get; set; }

        public Factura? Factura { get; set; }
    }
}
