using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

namespace ProyectoTI_PRR_Backend.Models
{
    public class CotizacionMaterial
    {
        [Key]
        public int CotizacionMaterialId { get; set; }

        public string MaterialCodigo { get; set; } = string.Empty;

        public int Cantidad { get; set; }

        public decimal PrecioUnitario { get; set; }

        public decimal Subtotal { get; set; }

        public int CotizacionId { get; set; }

        [JsonIgnore]
        public Cotizacion? Cotizacion { get; set; }
    }
}
