namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>Credenciais de login.</summary>
public class LoginViewModel
{
    public string Login { get; set; } = string.Empty;
    public string Senha { get; set; } = string.Empty;
}

/// <summary>Token JWT emitido após autenticação bem-sucedida.</summary>
public class TokenViewModel
{
    public string AccessToken { get; set; } = string.Empty;
    public string TokenType { get; set; } = "Bearer";
    public DateTime ExpiraEm { get; set; }
    public string Perfil { get; set; } = string.Empty;
}
