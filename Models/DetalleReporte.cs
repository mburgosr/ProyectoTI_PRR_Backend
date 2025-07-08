using ProyectoTI_PRR_Backend.Models;
using System.ComponentModel.DataAnnotations;
using System.Text.Json.Serialization;

public class DetalleReporte
{
    [Key]
    public int DetalleReporte_Id { get; set; }

    [Required]
    public string Descripcion { get; set; }

    public string? Observacion { get; set; }

    [Required]
    public decimal Total { get; set; }

    public int ReporteDiario_Id { get; set; }

    [JsonIgnore] 
    public ReporteDiario? ReporteDiario { get; set; }
}

