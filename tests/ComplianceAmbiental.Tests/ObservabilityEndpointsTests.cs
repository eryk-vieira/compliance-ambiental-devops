using System.Net;
using System.Net.Http.Json;
using System.Text.Json;
using Xunit;

namespace ComplianceAmbiental.Tests;

/// <summary>
/// Testes dos endpoints de observabilidade adicionados para o pipeline DevOps
/// (health checks consumidos pelo Docker/compose e endpoint de identificação do ambiente).
/// </summary>
public class ObservabilityEndpointsTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public ObservabilityEndpointsTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task HealthLive_RetornaHttpStatusCode200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/live");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task HealthReady_ComBancoDisponivel_RetornaHttpStatusCode200()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/health/ready");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        Assert.Equal("Healthy", await response.Content.ReadAsStringAsync());
    }

    [Fact]
    public async Task Info_SemAutenticacao_RetornaDadosDoAmbiente()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/info");

        Assert.Equal(HttpStatusCode.OK, response.StatusCode);
        var json = await response.Content.ReadFromJsonAsync<JsonElement>();
        Assert.Equal("Compliance Ambiental Logtech API", json.GetProperty("aplicacao").GetString());
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("ambiente").GetString()));
        Assert.False(string.IsNullOrWhiteSpace(json.GetProperty("versao").GetString()));
    }
}
