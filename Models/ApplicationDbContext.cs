using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;


public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets para las entidades
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Materiales> Materiales { get; set; }
    public DbSet<Factura> Facturas { get; set; }
    public DbSet<RegistroVolqueta> RegistroVolquetas{ get; set; }
    public DbSet<ReporteDiario> ReportesDiarios { get; set; }
    public DbSet<DetalleReporte> DetallesReporte { get; set; }
    public DbSet<Cotizacion> Cotizaciones { get; set; }
    public DbSet<CotizacionMaterial> CotizacionMateriales { get; set; }
    public DbSet<Pedido> Pedidos { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
            .HasKey(c => c.Cedula);

        modelBuilder.Entity<Materiales>()
            .HasKey(c => c.Codigo);

        modelBuilder.Entity<Materiales>()
            .Property(m => m.CostoSinIva)
            .HasColumnType("decimal(10, 2)");

        modelBuilder.Entity<ReporteDiario>()
            .HasMany(r => r.Detalles)
            .WithOne(d => d.ReporteDiario!)
            .HasForeignKey(d => d.ReporteDiario_Id)
            .OnDelete(DeleteBehavior.Cascade);

        modelBuilder.Entity<Cotizacion>()
       .HasMany(c => c.Materiales)
       .WithOne(m => m.Cotizacion!)
       .HasForeignKey(m => m.CotizacionId)
       .OnDelete(DeleteBehavior.Cascade);
    }

}

