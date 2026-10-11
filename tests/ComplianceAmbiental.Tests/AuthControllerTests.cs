using System.Net;
using System.Net.Http.Json;
using ComplianceAmbiental.Api.ViewModels;
using Xunit;

namespace ComplianceAmbiental.Tests;

public class AuthControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public AuthControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Login_ComCredenciaisValidas_RetornaHttpStatusCode200()
    {
        // Arrange
        var client = _factory.CreateClient();
        var credenciais = new LoginViewModel { Login = "admin", Senha = "Admin@123" };

        // Act
        var response = await client.PostAsJsonAsync("/api/auth/login", credenciais);

        // Assert
        response.EnsureSuccessStatusCode(); // verifica se o status code é 200
        var token = await response.Content.ReadFromJsonAsync<TokenViewModel>();
        Assert.False(string.IsNullOrWhiteSpace(token!.AccessToken));
    }

    [Fact]
    public async Task Login_ComCredenciaisInvalidas_RetornaUnauthorized()
    {
        var client = _factory.CreateClient();
        var credenciais = new LoginViewModel { Login = "admin", Senha = "senha-errada" };

        var response = await client.PostAsJsonAsync("/api/auth/login", credenciais);

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }
}
