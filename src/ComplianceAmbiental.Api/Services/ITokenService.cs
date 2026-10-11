using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.ViewModels;

namespace ComplianceAmbiental.Api.Services;

/// <summary>Serviço de autenticação e emissão de tokens JWT.</summary>
public interface ITokenService
{
    /// <summary>Valida as credenciais e retorna um token JWT, ou null se inválidas.</summary>
    Task<TokenViewModel?> AutenticarAsync(LoginViewModel login, CancellationToken ct = default);

    /// <summary>Gera um token JWT para um usuário já autenticado.</summary>
    TokenViewModel GerarToken(Usuario usuario);
}
