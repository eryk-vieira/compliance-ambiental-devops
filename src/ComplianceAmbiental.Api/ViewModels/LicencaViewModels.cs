namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>Status de compliance de uma licença ambiental.</summary>
public class LicencaStatusViewModel
{
    public int Id { get; set; }
    public int TransportadoraId { get; set; }
    public string Transportadora { get; set; } = string.Empty;
    public string NumeroLicenca { get; set; } = string.Empty;
    public string OrgaoEmissor { get; set; } = string.Empty;
    public string Status { get; set; } = string.Empty;
    public DateTime DataValidade { get; set; }
    public int DiasParaVencer { get; set; }
    public string SituacaoCompliance { get; set; } = string.Empty;
}

/// <summary>Dados para renovar uma licença ambiental.</summary>
public class LicencaRenovacaoViewModel
{
    public DateTime NovaDataValidade { get; set; }
    public string? NovoNumeroLicenca { get; set; }
}

/// <summary>Filtro da listagem de status de licenças.</summary>
public class LicencaFilterQuery : PaginationQuery
{
    /// <summary>Quando informado, retorna apenas licenças que vencem nos próximos N dias.</summary>
    public int? VenceEmDias { get; set; }
    public bool ApenasIrregulares { get; set; }
}
