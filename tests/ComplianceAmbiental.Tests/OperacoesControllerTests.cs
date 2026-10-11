using System.Net;
using System.Net.Http.Json;
using ComplianceAmbiental.Api.ViewModels;
using Xunit;

namespace ComplianceAmbiental.Tests;

public class OperacoesControllerTests : IClassFixture<CustomWebApplicationFactory>
{
    private readonly CustomWebApplicationFactory _factory;

    public OperacoesControllerTests(CustomWebApplicationFactory factory) => _factory = factory;

    [Fact]
    public async Task Get_Operacoes_RetornaHttpStatusCode200()
    {
        // Arrange
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var request = "/api/operacoes?pageNumber=1&pageSize=10";

        // Act
        var response = await client.GetAsync(request);

        // Assert
        response.EnsureSuccessStatusCode(); // verifica se o status code é 200
    }

    [Fact]
    public async Task Get_Operacoes_SemAutenticacao_RetornaUnauthorized()
    {
        var client = _factory.CreateClient();

        var response = await client.GetAsync("/api/operacoes");

        Assert.Equal(HttpStatusCode.Unauthorized, response.StatusCode);
    }

    [Fact]
    public async Task Post_OperacaoSemLicencaVigente_RetornaConflict()
    {
        // CargaSul (veículo SUL1D56 -> VeiculoId 4) está com licença vencida: deve bloquear.
        var client = await AuthenticationHelper.CreateAuthenticatedClientAsync(_factory);
        var nova = new OperacaoCreateViewModel
        {
            VeiculoId = 4,
            DataOperacao = DateTime.UtcNow.Date,
            Origem = "Porto Alegre/RS",
            Destino = "Pelotas/RS",
            Modal = Api.Models.ModalTransporte.Rodoviario,
            DistanciaKm = 250m,
            CargaToneladas = 12m
        };

        var response = await client.PostAsJsonAsync("/api/operacoes", nova);

        Assert.Equal(HttpStatusCode.Conflict, response.StatusCode);
    }
}
