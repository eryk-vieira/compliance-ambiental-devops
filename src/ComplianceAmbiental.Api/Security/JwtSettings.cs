namespace ComplianceAmbiental.Api.Security;

/// <summary>Configurações de emissão/validação do token JWT (seção "Jwt" do appsettings).</summary>
public class JwtSettings
{
    public string Issuer { get; set; } = string.Empty;
    public string Audience { get; set; } = string.Empty;
    public string SecretKey { get; set; } = string.Empty;
    public int ExpiresInMinutes { get; set; } = 120;
}
