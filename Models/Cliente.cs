using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Cliente
    {
        [Key] public string Cedula { get; set; }
        public string Nombre { get; set; }
        public string Tipo { get; set; }
        public string Telefono { get; set; }
        public string Correo { get; set; }
    }
}
