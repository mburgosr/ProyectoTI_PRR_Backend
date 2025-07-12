using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class Usuario
    {
        [Key]
        public int UsuarioId { get; set; }

        [Required]
        public string NombreUsuario { get; set; }

        [Required]
        public string ContraseñaHash { get; set; }

        public string CorreoElectronico { get; set; }

        public DateTime FechaRegistro { get; set; } = DateTime.Now;
    }
}
