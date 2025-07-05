using Microsoft.EntityFrameworkCore;
using ProyectoTI_PRR_Backend.Models;

public class ApplicationDbContext : DbContext
{
    public ApplicationDbContext(DbContextOptions<ApplicationDbContext> options) : base(options) { }

    // DbSets para las entidades
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<Materiales> Materiales { get; set; }
    public DbSet<Cotizacion> Cotizaciones { get; set; }
    public DbSet<CotizacionMaterial> CotizacionMateriales { get; set; }
    public DbSet<Factura> Facturas { get; set; }



    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        modelBuilder.Entity<Cliente>()
        .HasKey(c => c.Cedula);
        modelBuilder.Entity<Materiales>()
        .HasKey(c => c.Codigo);
        modelBuilder.Entity<Cotizacion>()
        .HasKey(c => c.NumeroCot);
        modelBuilder.Entity<CotizacionMaterial>()
        .HasKey(c => c.CotizacionNumero);

        // Configuración de las propiedades decimales con precisión y escala
        modelBuilder.Entity<Cotizacion>()
            .Property(c => c.SubTotal)
            .HasColumnType("decimal(10, 2)");

        modelBuilder.Entity<Cotizacion>()
            .Property(c => c.Iva)
            .HasColumnType("decimal(10, 2)");

        modelBuilder.Entity<Cotizacion>()
            .Property(c => c.Total)
            .HasColumnType("decimal(10, 2)");

        modelBuilder.Entity<Materiales>()
            .Property(m => m.CostoSinIva)
            .HasColumnType("decimal(10, 2)");

        // Relaciones entre entidades
        modelBuilder.Entity<Cotizacion>()
            .HasOne(c => c.Cliente)
            .WithMany()
            .HasForeignKey(c => c.ClienteCedula)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionMaterial>()
            .HasKey(cm => new { cm.CotizacionNumero, cm.MaterialCodigo });

        modelBuilder.Entity<CotizacionMaterial>()
            .HasOne(cm => cm.Cotizacion)
            .WithMany()
            .HasForeignKey(cm => cm.CotizacionNumero)
            .OnDelete(DeleteBehavior.Restrict);

        modelBuilder.Entity<CotizacionMaterial>()
            .HasOne(cm => cm.Material)
            .WithMany()
            .HasForeignKey(cm => cm.MaterialCodigo)
            .OnDelete(DeleteBehavior.Restrict);

    }
}

