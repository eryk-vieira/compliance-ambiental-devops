using ComplianceAmbiental.Api.Data;
using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Configuration;
using Microsoft.Extensions.DependencyInjection;
using Microsoft.Extensions.DependencyInjection.Extensions;

namespace ComplianceAmbiental.Tests;

/// <summary>
/// Fábrica de aplicação para testes de integração. Substitui o SQL Server por um banco
/// EF Core InMemory e popula os dados de exemplo, permitindo executar os testes sem
/// depender de infraestrutura externa.
/// </summary>
public class CustomWebApplicationFactory : WebApplicationFactory<Program>
{
    private readonly string _dbName = $"ComplianceTests_{Guid.NewGuid()}";

    protected override void ConfigureWebHost(IWebHostBuilder builder)
    {
        builder.UseEnvironment("Testing");

        // Garante as configurações de JWT no ambiente de teste.
        builder.ConfigureAppConfiguration((_, config) =>
        {
            config.AddInMemoryCollection(new Dictionary<string, string?>
            {
                ["Jwt:Issuer"] = "ComplianceAmbiental.Api",
                ["Jwt:Audience"] = "ComplianceAmbiental.Client",
                ["Jwt:SecretKey"] = "chave-super-secreta-compliance-ambiental-logtech-2026-fiap-min-32-bytes",
                ["Jwt:ExpiresInMinutes"] = "120"
            });
        });

        builder.ConfigureServices(services =>
        {
            // Remove o registro de DbContext/opções baseado em SQL Server.
            // No EF Core 8 também é necessário remover a configuração open-generic
            // IDbContextOptionsConfiguration<AppDbContext>, senão dois provedores ficam
            // registrados e o EF lança erro em runtime.
            var descriptor = services.SingleOrDefault(d => d.ServiceType == typeof(DbContextOptions<AppDbContext>));
            if (descriptor != null)
                services.Remove(descriptor);

            services.RemoveAll(typeof(IDbContextOptionsConfiguration<AppDbContext>));
            services.RemoveAll(typeof(AppDbContext));

            // Registra o provedor InMemory.
            services.AddDbContext<AppDbContext>(options =>
                options.UseInMemoryDatabase(_dbName));

            // Cria e popula o banco de testes.
            using var scope = services.BuildServiceProvider().CreateScope();
            var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
            db.Database.EnsureCreated();
            DbSeeder.SeedAsync(db).GetAwaiter().GetResult();
        });
    }
}
