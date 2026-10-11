using System.Net.Http.Headers;
using System.Net.Http.Json;
using ComplianceAmbiental.Api.ViewModels;

namespace ComplianceAmbiental.Tests;

/// <summary>Utilitário para obter um HttpClient autenticado com token JWT válido.</summary>
public static class AuthenticationHelper
{
    public static async Task<HttpClient> CreateAuthenticatedClientAsync(
        CustomWebApplicationFactory factory, string login = "admin", string senha = "Admin@123")
    {
        var client = factory.CreateClient();

        var response = await client.PostAsJsonAsync("/api/auth/login",
            new LoginViewModel { Login = login, Senha = senha });
        response.EnsureSuccessStatusCode();

        var token = await response.Content.ReadFromJsonAsync<TokenViewModel>();
        client.DefaultRequestHeaders.Authorization =
            new AuthenticationHeaderValue("Bearer", token!.AccessToken);

        return client;
    }
}
