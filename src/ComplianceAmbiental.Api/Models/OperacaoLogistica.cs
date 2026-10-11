namespace ComplianceAmbiental.Api.Models;

/// <summary>
/// Operação logística (viagem) executada por um veículo.
/// Equivalente à entidade OPERACAO_LOGISTICA do modelo PL/SQL.
/// A emissão de CO2 é calculada automaticamente no momento do registro.
/// </summary>
public class OperacaoLogistica
{
    public int Id { get; set; }

    public int VeiculoId { get; set; }

    public DateTime DataOperacao { get; set; }

    public string Origem { get; set; } = string.Empty;

    public string Destino { get; set; } = string.Empty;

    public ModalTransporte Modal { get; set; } = ModalTransporte.Rodoviario;

    /// <summary>Distância percorrida em quilômetros.</summary>
    public decimal DistanciaKm { get; set; }

    /// <summary>Carga transportada em toneladas.</summary>
    public decimal CargaToneladas { get; set; }

    /// <summary>Emissão de CO2 (kg) calculada automaticamente pela regra de negócio.</summary>
    public decimal EmissaoCo2Kg { get; set; }

    // Relacionamentos
    public Veiculo? Veiculo { get; set; }
}
