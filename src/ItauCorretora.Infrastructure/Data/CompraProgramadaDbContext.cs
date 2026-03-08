using ItauCorretora.Domain.Entities;
using Microsoft.EntityFrameworkCore;
using System.Diagnostics.CodeAnalysis;

namespace ItauCorretora.Infrastructure.Data;

[ExcludeFromCodeCoverage]
public class CompraProgramadaDbContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<ContaGrafica> ContasGraficas { get; set; }
    public DbSet<CustodiaFilhote> CustodiasFilhotes { get; set; }
    public DbSet<CestaTopFive> CestasTopFive { get; set; }
    public DbSet<ItemCesta> ItensCesta { get; set; }

    public CompraProgramadaDbContext(DbContextOptions<CompraProgramadaDbContext> options)
        : base(options) { }

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Cliente>(e =>
        {
            e.ToTable("Clientes");
            e.HasKey(c => c.Id);
            e.Property(c => c.Cpf).HasMaxLength(11).IsRequired();
            e.Property(c => c.ValorMensalAporte).HasPrecision(18, 2);

            e.HasOne(c => c.ContaGrafica)
             .WithOne()
             .HasForeignKey<ContaGrafica>(cg => cg.ClienteId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ContaGrafica>(e =>
        {
            e.ToTable("ContasGraficas");
            e.HasKey(cg => cg.Id);

            e.HasMany(cg => cg.Custodias)
             .WithOne()
             .HasForeignKey(c => c.ContaGraficaId)
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<CustodiaFilhote>(e =>
        {
            e.ToTable("CustodiasFilhotes");
            e.HasKey(c => c.Id);
            e.Property(c => c.Ticker).HasMaxLength(10).IsRequired();

            e.Property(c => c.PrecoMedio).HasPrecision(18, 4);
        });

        modelBuilder.Entity<CestaTopFive>(e =>
        {
            e.ToTable("CestasTopFive");
            e.HasKey(c => c.Id);

            e.HasMany(c => c.Itens)
             .WithOne()
             .OnDelete(DeleteBehavior.Cascade);
        });

        modelBuilder.Entity<ItemCesta>(e =>
        {
            e.ToTable("ItensCesta");
            e.HasKey(i => i.Id);
            e.Property(i => i.Ticker).HasMaxLength(10).IsRequired();
            e.Property(i => i.Percentual).HasPrecision(18, 2);
        });

    }
}