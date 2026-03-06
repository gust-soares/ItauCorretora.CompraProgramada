using Microsoft.EntityFrameworkCore;
using ItauCorretora.Domain.Entities;

namespace ItauCorretora.Infrastructure.Data;

public class CompraProgramadaDbContext : DbContext
{
    public DbSet<Cliente> Clientes { get; set; }
    public DbSet<ContaGrafica> ContasGraficas { get; set; }
    public DbSet<CustodiaFilhote> CustodiasFilhotes { get; set; }

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
    }
}