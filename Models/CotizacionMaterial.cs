using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class CotizacionMaterial
    {
        [Key] public string CotizacionNumero { get; set; }  // Relación con la tabla cotizaciones
        public string MaterialCodigo { get; set; }  // Relación con la tabla materiales
        public int Cantidad { get; set; }

        // Relaciones con las tablas cotizaciones y materiales
        public Cotizacion Cotizacion { get; set; }
        public Material Material { get; set; }
    }
}
