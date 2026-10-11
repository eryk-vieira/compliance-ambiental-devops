using ComplianceAmbiental.Api.ViewModels;

namespace ComplianceAmbiental.Api.Services;

public interface IEmissaoService
{
    /// <summary>Consolidação mensal das emissões de CO2 por transportadora (paginada).</summary>
    Task<PagedResultViewModel<EmissaoConsolidadoViewModel>> ConsolidarAsync(EmissaoFilterQuery query, CancellationToken ct = default);

    /// <summary>Lista transportadoras que ultrapassaram (ou estão próximas de) o teto mensal de CO2.</summary>
    Task<PagedResultViewModel<AlertaEmissaoViewModel>> ListarAlertasAsync(EmissaoFilterQuery query, CancellationToken ct = default);
}
