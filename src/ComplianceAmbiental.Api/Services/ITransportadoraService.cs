using ComplianceAmbiental.Api.ViewModels;

namespace ComplianceAmbiental.Api.Services;

public interface ITransportadoraService
{
    /// <summary>Lista transportadoras com score de compliance (paginada).</summary>
    Task<PagedResultViewModel<TransportadoraComplianceViewModel>> ListarAsync(TransportadoraFilterQuery query, CancellationToken ct = default);

    /// <summary>Monta o dashboard ESG consolidado de uma transportadora.</summary>
    Task<DashboardEsgViewModel> ObterDashboardAsync(int transportadoraId, CancellationToken ct = default);
}
