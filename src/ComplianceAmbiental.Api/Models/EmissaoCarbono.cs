namespace ComplianceAmbiental.Api.Models;

/// <summary>
/// Acumulado mensal de emissão de CO2 por transportadora.
/// Equivalente à entidade EMISSAO_CARBONO do modelo PL/SQL, consolidada em tempo real.
/// </summary>
public class EmissaoCarbono
{
    public int Id { get; set; }

    public int TransportadoraId { get; set; }

    /// <summary>Ano de referência do acumulado.</summary>
    public int Ano { get; set; }

    /// <summary>Mês de referência (1-12).</summary>
    public int Mes { get; set; }

    /// <summary>Total de CO2 (kg) emitido no mês.</summary>
    public decimal TotalCo2Kg { get; set; }

    /// <summary>Quantidade de operações que compõem o acumulado.</summary>
    public int QtdOperacoes { get; set; }

    public DateTime AtualizadoEm { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public Transportadora? Transportadora { get; set; }
}
