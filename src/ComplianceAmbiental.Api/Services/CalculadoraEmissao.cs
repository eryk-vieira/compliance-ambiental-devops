using ComplianceAmbiental.Api.Models;

namespace ComplianceAmbiental.Api.Services;

/// <summary>
/// Regra de negócio que calcula a pegada de carbono de uma operação logística.
/// Replica em C# a lógica da trigger TRG_CALCULA_EMISSAO_CO2 do projeto PL/SQL:
/// emissão (kg) = (distância / consumo médio) x fator de emissão do combustível.
/// </summary>
public static class CalculadoraEmissao
{
    /// <summary>
    /// Fator de emissão em kg de CO2 por litro (ou por kWh) de combustível,
    /// baseado em referências do GHG Protocol / inventários nacionais.
    /// </summary>
    private static readonly IReadOnlyDictionary<TipoCombustivel, decimal> FatorPorLitro =
        new Dictionary<TipoCombustivel, decimal>
        {
            [TipoCombustivel.Diesel] = 2.68m,
            [TipoCombustivel.DieselS10] = 2.60m,
            [TipoCombustivel.Gasolina] = 2.31m,
            [TipoCombustivel.Etanol] = 1.51m,
            [TipoCombustivel.GasNatural] = 2.02m,
            [TipoCombustivel.Eletrico] = 0.10m
        };

    public static decimal FatorEmissao(TipoCombustivel combustivel) =>
        FatorPorLitro.TryGetValue(combustivel, out var fator) ? fator : 2.68m;

    /// <summary>
    /// Calcula a emissão de CO2 (kg) de uma operação dado o veículo utilizado.
    /// O resultado é arredondado a 2 casas decimais.
    /// </summary>
    public static decimal Calcular(decimal distanciaKm, Veiculo veiculo)
    {
        if (veiculo.ConsumoMedioKmL <= 0)
            throw new InvalidOperationException(
                $"Veículo {veiculo.Placa} possui consumo médio inválido ({veiculo.ConsumoMedioKmL}).");

        var litrosConsumidos = distanciaKm / veiculo.ConsumoMedioKmL;
        var emissao = litrosConsumidos * FatorEmissao(veiculo.TipoCombustivel);
        return Math.Round(emissao, 2, MidpointRounding.AwayFromZero);
    }
}
