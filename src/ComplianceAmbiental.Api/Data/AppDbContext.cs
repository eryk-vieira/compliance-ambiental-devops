using ComplianceAmbiental.Api.Models;
using Microsoft.EntityFrameworkCore;

namespace ComplianceAmbiental.Api.Data;

/// <summary>
/// Contexto do Entity Framework Core que mapeia o domínio de Compliance Ambiental Logtech.
/// </summary>
public class AppDbContext : DbContext
{
    public AppDbContext(DbContextOptions<AppDbContext> options) : base(options) { }

    public DbSet<Transportadora> Transportadoras => Set<Transportadora>();
    public DbSet<LicencaAmbiental> Licencas => Set<LicencaAmbiental>();
    public DbSet<Veiculo> Veiculos => Set<Veiculo>();
    public DbSet<OperacaoLogistica> Operacoes => Set<OperacaoLogistica>();
    public DbSet<EmissaoCarbono> Emissoes => Set<EmissaoCarbono>();
    public DbSet<Usuario> Usuarios => Set<Usuario>();

    protected override void OnModelCreating(ModelBuilder modelBuilder)
    {
        base.OnModelCreating(modelBuilder);

        modelBuilder.Entity<Transportadora>(e =>
        {
            e.ToTable("TB_TRANSPORTADORA");
            e.HasKey(t => t.Id);
            e.Property(t => t.RazaoSocial).HasMaxLength(150).IsRequired();
            e.Property(t => t.Cnpj).HasMaxLength(14).IsRequired();
            e.Property(t => t.Uf).HasMaxLength(2).IsRequired();
            e.Property(t => t.LimiteMensalCo2Kg).HasPrecision(14, 2);
            e.HasIndex(t => t.Cnpj).IsUnique();
        });

        modelBuilder.Entity<LicencaAmbiental>(e =>
        {
            e.ToTable("TB_LICENCA_AMBIENTAL");
            e.HasKey(l => l.Id);
            e.Property(l => l.NumeroLicenca).HasMaxLength(40).IsRequired();
            e.Property(l => l.OrgaoEmissor).HasMaxLength(60).IsRequired();
            e.Property(l => l.Status).HasConversion<int>();
            e.HasOne(l => l.Transportadora)
                .WithMany(t => t.Licencas)
                .HasForeignKey(l => l.TransportadoraId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(l => new { l.TransportadoraId, l.Status });
        });

        modelBuilder.Entity<Veiculo>(e =>
        {
            e.ToTable("TB_VEICULO");
            e.HasKey(v => v.Id);
            e.Property(v => v.Placa).HasMaxLength(8).IsRequired();
            e.Property(v => v.TipoCombustivel).HasConversion<int>();
            e.Property(v => v.ConsumoMedioKmL).HasPrecision(8, 2);
            e.HasOne(v => v.Transportadora)
                .WithMany(t => t.Veiculos)
                .HasForeignKey(v => v.TransportadoraId)
                .OnDelete(DeleteBehavior.Cascade);
            e.HasIndex(v => v.Placa);
        });

        modelBuilder.Entity<OperacaoLogistica>(e =>
        {
            e.ToTable("TB_OPERACAO_LOGISTICA");
            e.HasKey(o => o.Id);
            e.Property(o => o.Origem).HasMaxLength(120).IsRequired();
            e.Property(o => o.Destino).HasMaxLength(120).IsRequired();
            e.Property(o => o.Modal).HasConversion<int>();
            e.Property(o => o.DistanciaKm).HasPrecision(12, 2);
            e.Property(o => o.CargaToneladas).HasPrecision(12, 2);
            e.Property(o => o.EmissaoCo2Kg).HasPrecision(14, 2);
            e.HasOne(o => o.Veiculo)
                .WithMany(v => v.Operacoes)
                .HasForeignKey(o => o.VeiculoId)
                .OnDelete(DeleteBehavior.Cascade);
            // Índice de apoio à otimização das consultas por data.
            e.HasIndex(o => o.DataOperacao);
        });

        modelBuilder.Entity<EmissaoCarbono>(e =>
        {
            e.ToTable("TB_EMISSAO_CARBONO");
            e.HasKey(c => c.Id);
            e.Property(c => c.TotalCo2Kg).HasPrecision(16, 2);
            e.HasOne(c => c.Transportadora)
                .WithMany(t => t.Emissoes)
                .HasForeignKey(c => c.TransportadoraId)
                .OnDelete(DeleteBehavior.Cascade);
            // Acumulado único por transportadora/ano/mês.
            e.HasIndex(c => new { c.TransportadoraId, c.Ano, c.Mes }).IsUnique();
        });

        modelBuilder.Entity<Usuario>(e =>
        {
            e.ToTable("TB_USUARIO");
            e.HasKey(u => u.Id);
            e.Property(u => u.Login).HasMaxLength(60).IsRequired();
            e.Property(u => u.SenhaHash).HasMaxLength(120).IsRequired();
            e.Property(u => u.Perfil).HasMaxLength(30).IsRequired();
            e.HasIndex(u => u.Login).IsUnique();
        });
    }
}
