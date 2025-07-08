using System.ComponentModel.DataAnnotations;

namespace ProyectoTI_PRR_Backend.Models
{
    public class ReporteDiario
{
        [Key]
        public int ReporteDiario_Id { get; set; }

        public DateTime Fecha { get; set; }

        public string ClienteCedula { get; set; } = string.Empty;

    public int VolquetaId { get; set; }

    public string Responsable { get; set; } = string.Empty;

    public List<DetalleReporte> Detalles { get; set; } = new List<DetalleReporte>();
}

}

