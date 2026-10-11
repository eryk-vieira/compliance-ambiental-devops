namespace ComplianceAmbiental.Api.Models;

/// <summary>
/// Licença ambiental emitida por órgão (CETESB, IBAMA, etc.) para uma transportadora.
/// Equivalente à entidade LICENCA_AMBIENTAL do modelo PL/SQL.
/// </summary>
public class LicencaAmbiental
{
    public int Id { get; set; }

    public int TransportadoraId { get; set; }

    public string NumeroLicenca { get; set; } = string.Empty;

    public string OrgaoEmissor { get; set; } = string.Empty;

    public DateTime DataEmissao { get; set; }

    public DateTime DataValidade { get; set; }

    public StatusLicenca Status { get; set; } = StatusLicenca.Ativa;

    // Relacionamentos
    public Transportadora? Transportadora { get; set; }

    /// <summary>Indica se a licença está ativa e vigente na data informada.</summary>
    public bool EstaVigenteEm(DateTime data) =>
        Status == StatusLicenca.Ativa && DataEmissao <= data && DataValidade >= data;
}
