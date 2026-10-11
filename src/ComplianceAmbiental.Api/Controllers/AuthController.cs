using ComplianceAmbiental.Api.Services;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAmbiental.Api.Controllers;

/// <summary>Autenticação e emissão de tokens JWT.</summary>
[ApiController]
[Route("api/[controller]")]
[Produces("application/json")]
public class AuthController : ControllerBase
{
    private readonly ITokenService _tokenService;

    public AuthController(ITokenService tokenService) => _tokenService = tokenService;

    /// <summary>Autentica um usuário e retorna um token JWT (Bearer).</summary>
    /// <remarks>Credenciais de exemplo (seed): login <c>admin</c> / senha <c>Admin@123</c>.</remarks>
    [HttpPost("login")]
    [AllowAnonymous]
    [ProducesResponseType(typeof(TokenViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status401Unauthorized)]
    public async Task<IActionResult> Login([FromBody] LoginViewModel login, CancellationToken ct)
    {
        var token = await _tokenService.AutenticarAsync(login, ct);
        if (token is null)
            return Unauthorized(new { mensagem = "Login ou senha inválidos." });

        return Ok(token);
    }
}
