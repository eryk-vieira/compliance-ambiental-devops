namespace ComplianceAmbiental.Api.Models;

/// <summary>
/// Usuário do sistema, utilizado apenas para autenticação/autorização (JWT).
/// Não é o foco funcional do projeto — existe para proteger os endpoints críticos.
/// </summary>
public class Usuario
{
    public int Id { get; set; }

    public string Login { get; set; } = string.Empty;

    /// <summary>Hash SHA-256 (Base64) da senha. Nunca armazenamos a senha em texto puro.</summary>
    public string SenhaHash { get; set; } = string.Empty;

    /// <summary>Perfil do usuário (ex.: "Admin", "Auditor").</summary>
    public string Perfil { get; set; } = "Auditor";
}
