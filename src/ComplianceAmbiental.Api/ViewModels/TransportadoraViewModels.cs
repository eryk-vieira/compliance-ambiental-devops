namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>Transportadora com indicadores de compliance consolidados.</summary>
public class TransportadoraComplianceViewModel
{
    public int Id { get; set; }
    public string RazaoSocial { get; set; } = string.Empty;
    public string Cnpj { get; set; } = string.Empty;
    public string Uf { get; set; } = string.Empty;
    public decimal LimiteMensalCo2Kg { get; set; }
    public bool PossuiLicencaVigente { get; set; }
    public int QtdLicencas { get; set; }
    public int QtdVeiculos { get; set; }
    /// <summary>Score de compliance de 0 a 100, combinando situação de licença e aderência ao teto de CO2.</summary>
    public int ScoreCompliance { get; set; }
}

/// <summary>Dashboard ESG consolidado de uma transportadora.</summary>
public class DashboardEsgViewModel
{
    public int TransportadoraId { get; set; }
    public string Transportadora { get; set; } = string.Empty;
    public bool PossuiLicencaVigente { get; set; }
    public int QtdVeiculosAtivos { get; set; }
    public int QtdOperacoesTotal { get; set; }
    public decimal Co2TotalKg { get; set; }
    public decimal Co2MesAtualKg { get; set; }
    public decimal LimiteMensalCo2Kg { get; set; }
    public decimal PercentualDoLimiteMesAtual { get; set; }
    public int ScoreCompliance { get; set; }
    public IReadOnlyList<EmissaoConsolidadoViewModel> HistoricoMensal { get; set; } = new List<EmissaoConsolidadoViewModel>();
}

/// <summary>Filtro da listagem de transportadoras.</summary>
public class TransportadoraFilterQuery : PaginationQuery
{
    public string? Uf { get; set; }
    public string? Busca { get; set; }
}
