using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Cotizacion
    {
        [Key] public string NumeroCot { get; set; }
        public string ClienteCedula { get; set; }  // Relación con la tabla clientes
        public DateTime Fecha { get; set; }
        public decimal SubTotal { get; set; }
        public decimal Iva { get; set; }
        public decimal Total { get; set; }

        // Relación con la tabla Cliente
        public Cliente Cliente { get; set; }
    }
}
