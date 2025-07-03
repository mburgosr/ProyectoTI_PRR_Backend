using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Materiales
    {
        [Key] public string Codigo { get; set; }
        public string Nombre { get; set; }
        public decimal CostoSinIva { get; set; }
        public string Tipo { get; set; }
    }
}
