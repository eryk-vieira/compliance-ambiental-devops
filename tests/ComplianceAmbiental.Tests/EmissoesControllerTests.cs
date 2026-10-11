using Xunit;

namespace ComplianceAmbiental.Tests;

public class EmissoesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public EmissoesControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Consolidado_RetornaHttpStatusCode200()
    {
        // Arrange
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var request = "/api/emissoes/consolidado?pageNumber=1&pageSize=10";

        // Act
        var response = await client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // verifica se o status code é 200
    }

    [Fact]
    public async Task Get_Alertas_RetornaHttpStatusCode200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/emissoes/alertas");

        response.EnsureSuccessStatusCode();
    }
}
