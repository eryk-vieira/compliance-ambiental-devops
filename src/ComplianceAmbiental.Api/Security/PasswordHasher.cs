using System.Security.Cryptography;
using System.Text;

namespace ComplianceAmbiental.Api.Security;

/// <summary>
/// Utilitário simples de hash de senha (SHA-256 + salt fixo de aplicação).
/// Para um sistema de produção real recomenda-se PBKDF2/BCrypt; aqui mantemos
/// uma implementação sem dependências externas suficiente para o escopo do desafio.
/// </summary>
public static class PasswordHasher
{
    private const string Salt = "compliance-ambiental-logtech::v1";

    public static string Hash(string senha)
    {
        var bytes = Encoding.UTF8.GetBytes(Salt + senha);
        var hash = SHA256.HashData(bytes);
        return Convert.ToBase64String(hash);
    }

    public static bool Verify(string senha, string hashArmazenado) =>
        Hash(senha) == hashArmazenado;
}
