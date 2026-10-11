namespace ComplianceAmbiental.Api.Models;

/// <summary>
/// Transportadora monitorada pelo sistema de compliance ambiental.
/// Equivalente à entidade TRANSPORTADORA do modelo PL/SQL.
/// </summary>
public class Transportadora
{
    public int Id { get; set; }

    public string RazaoSocial { get; set; } = string.Empty;

    public string Cnpj { get; set; } = string.Empty;

    public string Uf { get; set; } = string.Empty;

    /// <summary>Teto mensal de emissão de CO2 (kg) imposto pelo órgão ambiental.</summary>
    public decimal LimiteMensalCo2Kg { get; set; }

    public DateTime CriadoEm { get; set; } = DateTime.UtcNow;

    // Relacionamentos
    public ICollection<LicencaAmbiental> Licencas { get; set; } = new List<LicencaAmbiental>();
    public ICollection<Veiculo> Veiculos { get; set; } = new List<Veiculo>();
    public ICollection<EmissaoCarbono> Emissoes { get; set; } = new List<EmissaoCarbono>();
}
