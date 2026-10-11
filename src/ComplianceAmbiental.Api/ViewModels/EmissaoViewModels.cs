namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>Linha do relatório consolidado mensal de emissões.</summary>
public class EmissaoConsolidadoViewModel
{
    public int TransportadoraId { get; set; }
    public string Transportadora { get; set; } = string.Empty;
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal TotalCo2Kg { get; set; }
    public int QtdOperacoes { get; set; }
    public decimal LimiteMensalCo2Kg { get; set; }
    public decimal PercentualDoLimite { get; set; }
}

/// <summary>Alerta de transportadora que ultrapassou (ou está próxima de) o teto mensal de CO2.</summary>
public class AlertaEmissaoViewModel
{
    public int TransportadoraId { get; set; }
    public string Transportadora { get; set; } = string.Empty;
    public int Ano { get; set; }
    public int Mes { get; set; }
    public decimal TotalCo2Kg { get; set; }
    public decimal LimiteMensalCo2Kg { get; set; }
    public decimal ExcedenteCo2Kg { get; set; }
    public decimal PercentualDoLimite { get; set; }
    public string Severidade { get; set; } = string.Empty;
}

/// <summary>Filtro da consolidação mensal.</summary>
public class EmissaoFilterQuery : PaginationQuery
{
    public int? Ano { get; set; }
    public int? Mes { get; set; }
    public int? TransportadoraId { get; set; }
}
