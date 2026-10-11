using System.IdentityModel.Tokens.Jwt;
using System.Security.Claims;
using System.Text;
using ComplianceAmbiental.Api.Data;
using ComplianceAmbiental.Api.Models;
using ComplianceAmbiental.Api.Security;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.EntityFrameworkCore;
using Microsoft.Extensions.Options;
using Microsoft.IdentityModel.Tokens;

namespace ComplianceAmbiental.Api.Services;

/// <inheritdoc />
public class TokenService : ITokenService
{
    private readonly AppDbContext _db;
    private readonly JwtSettings _settings;

    public TokenService(AppDbContext db, IOptions<JwtSettings> settings)
    {
        _db = db;
        _settings = settings.Value;
    }

    public async Task<TokenViewModel?> AutenticarAsync(LoginViewModel login, CancellationToken ct = default)
    {
        var usuario = await _db.Usuarios
            .AsNoTracking()
            .FirstOrDefaultAsync(u => u.Login == login.Login, ct);

        if (usuario is null || !PasswordHasher.Verify(login.Senha, usuario.SenhaHash))
            return null;

        return GerarToken(usuario);
    }

    public TokenViewModel GerarToken(Usuario usuario)
    {
        var key = new SymmetricSecurityKey(Encoding.UTF8.GetBytes(_settings.SecretKey));
        var credentials = new SigningCredentials(key, SecurityAlgorithms.HmacSha256);
        var expiraEm = DateTime.UtcNow.AddMinutes(_settings.ExpiresInMinutes);

        var claims = new[]
        {
            new Claim(JwtRegisteredClaimNames.Sub, usuario.Id.ToString()),
            new Claim(JwtRegisteredClaimNames.UniqueName, usuario.Login),
            new Claim(ClaimTypes.Name, usuario.Login),
            new Claim(ClaimTypes.Role, usuario.Perfil),
            new Claim(JwtRegisteredClaimNames.Jti, Guid.NewGuid().ToString())
        };

        var token = new JwtSecurityToken(
            issuer: _settings.Issuer,
            audience: _settings.Audience,
            claims: claims,
            expires: expiraEm,
            signingCredentials: credentials);

        return new TokenViewModel
        {
            AccessToken = new JwtSecurityTokenHandler().WriteToken(token),
            TokenType = "Bearer",
            ExpiraEm = expiraEm,
            Perfil = usuario.Perfil
        };
    }
}
