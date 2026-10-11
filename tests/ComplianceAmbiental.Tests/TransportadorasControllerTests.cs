using Xunit;

namespace ComplianceAmbiental.Tests;

public class TransportadorasControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public TransportadorasControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Transportadoras_RetornaHttpStatusCode200()
    {
        // Arrange
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var request = "/api/transportadoras?pageNumber=1&pageSize=10";

        // Act
        var response = await client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // verifica se o status code é 200
    }

    [Fact]
    public async Task Get_Dashboard_RetornaHttpStatusCode200()
    {
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);

        var response = await client.GetAsync("/api/transportadoras/1/dashboard");

        response.EnsureSuccessStatusCode();
    }
}
