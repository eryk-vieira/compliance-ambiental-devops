namespace ComplianceAmbiental.Api.Models;

/// <summary>
/// Veículo da frota de uma transportadora.
/// Equivalente à entidade VEICULO do modelo PL/SQL.
/// </summary>
public class Veiculo
{
    public int Id { get; set; }

    public int TransportadoraId { get; set; }

    public string Placa { get; set; } = string.Empty;

    public TipoCombustivel TipoCombustivel { get; set; }

    /// <summary>Consumo médio em km/litro (ou km/kWh para elétricos).</summary>
    public decimal ConsumoMedioKmL { get; set; }

    public bool Ativo { get; set; } = true;

    // Relacionamentos
    public Transportadora? Transportadora { get; set; }
    public ICollection<OperacaoLogistica> Operacoes { get; set; } = new List<OperacaoLogistica>();
}
