using System;
using System.ComponentModel.DataAnnotations;
using System.ComponentModel.DataAnnotations.Schema;
namespace ProyectoTI_PRR_Backend.Models
{
    public class RegistroVolqueta
    {
        [Key]
        public int Id { get; set; }

        [Required]
        [MaxLength(10)]
        public string Placa { get; set; }

        [Required]
        [MaxLength(20)]
        public string Tipo { get; set; }

        [Required]
        [MaxLength(10)] 
        public string Capacidad { get; set; }


        [Required]
        [MaxLength(20)]
        public string Estado { get; set; }
    }
}
