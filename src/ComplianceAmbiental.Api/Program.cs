using System.Text;
using ComplianceAmbiental.Api.Data;
using ComplianceAmbiental.Api.Middleware;
using ComplianceAmbiental.Api.Security;
using ComplianceAmbiental.Api.Services;
using ComplianceAmbiental.Api.Validators;
using FluentValidation;
using Microsoft.AspNetCore.Authentication.JwtBearer;
using Microsoft.AspNetCore.Diagnostics.HealthChecks;
using Microsoft.EntityFrameworkCore;
using Microsoft.IdentityModel.Tokens;
using Microsoft.OpenApi.Models;

var builder = WebApplication.CreateBuilder(args);

// ----------------------------------------------------------------------------
// Configurações
// ----------------------------------------------------------------------------
builder.Services.Configure<JwtSettings>(builder.Configuration.GetSection("Jwt"));
var jwtSettings = builder.Configuration.GetSection("Jwt").Get<JwtSettings>() ?? new JwtSettings();

// ----------------------------------------------------------------------------
// Banco de dados (EF Core + SQL Server)
// ----------------------------------------------------------------------------
builder.Services.AddDbContext<AppDbContext>(options =>
    options.UseSqlServer(
        builder.Configuration.GetConnectionString("DefaultConnection"),
        sql => sql.EnableRetryOnFailure()));

// ----------------------------------------------------------------------------
// Injeção de dependência dos serviços de domínio
// ----------------------------------------------------------------------------
builder.Services.AddScoped<ITokenService, TokenService>();
builder.Services.AddScoped<IOperacaoService, OperacaoService>();
builder.Services.AddScoped<IEmissaoService, EmissaoService>();
builder.Services.AddScoped<ILicencaService, LicencaService>();
builder.Services.AddScoped<ITransportadoraService, TransportadoraService>();

// FluentValidation
builder.Services.AddValidatorsFromAssemblyContaining<OperacaoCreateValidator>();

// ----------------------------------------------------------------------------
// Autenticação / Autorização (JWT Bearer)
// ----------------------------------------------------------------------------
builder.Services
    .AddAuthentication(JwtBearerDefaults.AuthenticationScheme)
    .AddJwtBearer(options =>
    {
        options.TokenValidationParameters = new TokenValidationParameters
        {
            ValidateIssuer = true,
            ValidateAudience = true,
            ValidateLifetime = true,
            ValidateIssuerSigningKey = true,
            ValidIssuer = jwtSettings.Issuer,
            ValidAudience = jwtSettings.Audience,
            IssuerSigningKey = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(jwtSettings.SecretKey)),
            ClockSkew = TimeSpan.FromMinutes(1)
        };
    });

builder.Services.AddAuthorization();

// ----------------------------------------------------------------------------
// Health checks (usados pelo Docker healthcheck, pelo compose --wait e pelo
// smoke test do pipeline de CI/CD)
//   /health/live  -> processo respondendo (liveness)
//   /health/ready -> processo + banco de dados acessível (readiness)
// ----------------------------------------------------------------------------
builder.Services.AddHealthChecks()
    .AddDbContextCheck<AppDbContext>("database", tags: new[] { "ready" });

// ----------------------------------------------------------------------------
// Controllers + Swagger
// ----------------------------------------------------------------------------
builder.Services.AddControllers();
builder.Services.AddEndpointsApiExplorer();
builder.Services.AddSwaggerGen(options =>
{
    options.SwaggerDoc("v1", new OpenApiInfo
    {
        Title = "Compliance Ambiental Logtech API",
        Version = "v1",
        Description = "API RESTful para governança e compliance ambiental de transportadoras (ESG / Logtech / Comex)."
    });

    var jwtScheme = new OpenApiSecurityScheme
    {
        Name = "Authorization",
        Type = SecuritySchemeType.Http,
        Scheme = "bearer",
        BearerFormat = "JWT",
        In = ParameterLocation.Header,
        Description = "Informe o token JWT obtido em /api/auth/login.",
        Reference = new OpenApiReference { Type = ReferenceType.SecurityScheme, Id = "Bearer" }
    };
    options.AddSecurityDefinition("Bearer", jwtScheme);
    options.AddSecurityRequirement(new OpenApiSecurityRequirement { [jwtScheme] = Array.Empty<string>() });
});

var app = builder.Build();

// ----------------------------------------------------------------------------
// Pipeline HTTP
// ----------------------------------------------------------------------------
app.UseMiddleware<ExceptionHandlingMiddleware>();

app.UseSwagger();
app.UseSwaggerUI(c => c.SwaggerEndpoint("/swagger/v1/swagger.json", "Compliance Ambiental Logtech API v1"));

app.UseAuthentication();
app.UseAuthorization();

app.MapControllers();

// Endpoints de observabilidade (públicos) ------------------------------------
app.MapHealthChecks("/health/live", new HealthCheckOptions { Predicate = _ => false });
app.MapHealthChecks("/health/ready", new HealthCheckOptions { Predicate = check => check.Tags.Contains("ready") });

// Identificação do ambiente/versão em execução. Preenchido pelo pipeline via
// variáveis de ambiente (APP_ENVIRONMENT, APP_VERSION, APP_COMMIT_SHA, APP_BUILD_NUMBER).
app.MapGet("/api/info", (IConfiguration cfg, IWebHostEnvironment env) => Results.Ok(new
{
    aplicacao = "Compliance Ambiental Logtech API",
    ambiente = cfg["APP_ENVIRONMENT"] ?? env.EnvironmentName,
    aspnetEnvironment = env.EnvironmentName,
    versao = cfg["APP_VERSION"] ?? "dev",
    commit = cfg["APP_COMMIT_SHA"] ?? "local",
    build = cfg["APP_BUILD_NUMBER"] ?? "local",
    deployEm = cfg["APP_DEPLOYED_AT"] ?? "n/a",
    horaServidorUtc = DateTime.UtcNow,
    host = Environment.MachineName
})).AllowAnonymous().WithTags("Info");

// ----------------------------------------------------------------------------
// Migração automática + seed (ignorado no ambiente de testes)
// ----------------------------------------------------------------------------
if (!app.Environment.IsEnvironment("Testing"))
{
    using var scope = app.Services.CreateScope();
    var db = scope.ServiceProvider.GetRequiredService<AppDbContext>();
    await db.Database.MigrateAsync();
    await DbSeeder.SeedAsync(db);
}

app.Run();

/// <summary>Exposto como partial public para permitir testes de integração com WebApplicationFactory.</summary>
public partial class Program { }
