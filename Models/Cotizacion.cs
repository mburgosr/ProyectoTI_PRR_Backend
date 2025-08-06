using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Cotizacion
    {
        [Key]
        public int CotizacionId { get; set; }

        public string NumeroCot { get; set; } = string.Empty;

        public string ClienteCedula { get; set; } = string.Empty;

        public DateTime Fecha { get; set; }

        public decimal SubTotal { get; set; }

        public decimal IVA { get; set; }

        public decimal Total { get; set; }

        [Required]
        public decimal PorcentIVA { get; set; } = 15.00m;
        public List<CotizacionMaterial> Materiales { get; set; } = new List<CotizacionMaterial>();
    }
}
