namespace ComplianceAmbiental.Api.Models;

/// <summary>Situação da licença ambiental de uma transportadora.</summary>
public enum StatusLicenca
{
    Ativa = 1,
    Suspensa = 2,
    Vencida = 3,
    Cancelada = 4
}

/// <summary>Tipo de combustível utilizado pelo veículo (impacta o fator de emissão).</summary>
public enum TipoCombustivel
{
    Diesel = 1,
    DieselS10 = 2,
    Gasolina = 3,
    Etanol = 4,
    GasNatural = 5,
    Eletrico = 6
}

/// <summary>Modal de transporte da operação logística.</summary>
public enum ModalTransporte
{
    Rodoviario = 1,
    Ferroviario = 2,
    Aquaviario = 3,
    Aereo = 4
}
