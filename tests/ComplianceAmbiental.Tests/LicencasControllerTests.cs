using System.Net;
using System.Net.Http.Json;
using ComplianceAmbiental.Api.ViewModels;
using Xunit;

namespace ComplianceAmbiental.Tests;

public class LicencasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public LicencasControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Status_RetornaHttpStatusCode200()
    {
        // Arrange
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var request = "/api/licencas/status?pageNumber=1&pageSize=10";

        // Act
        var response = await client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // verifica se o status code é 200
    }

    [Fact]
    public async Task Post_Renovar_ComPerfilAuditor_RetornaForbidden()
    {
        // Endpoint crítico restrito ao perfil Admin: auditor não pode renovar.
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory, "auditor", "Auditor@123");
        var renovacao = new LicencaRenovacaoViewModel { NovaDataValidade = DateTime.UtcNow.AddYears(1) };

        var response = await client.PostAsJsonAsync("/api/licencas/3/renovar", renovacao);

        Assert.Equal(HttpStatusCode.Forbidden, response.StatusCode);
    }

    [Fact]
    public async Task Post_Renovar_ComPerfilAdmin_RetornaHttpStatusCode200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var renovacao = new LicencaRenovacaoViewModel
        {
            NovaDataValidade = DateTime.UtcNow.AddYears(1),
            NovoNumeroLicenca = "FEPAM-2026-1000"
        };

        var response = await client.PostAsJsonAsync("/api/licencas/3/renovar", renovacao);

        response.EnsureSuccessStatusCode();
    }
}
