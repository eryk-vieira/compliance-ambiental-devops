using ComplianceAmbiental.Api.ViewModels;

namespace ComplianceAmbiental.Api.Services;

public interface ILicencaService
{
    /// <summary>Lista o status de compliance das licenças ambientais (paginada).</summary>
    Task<PagedResultViewModel<LicencaStatusViewModel>> ListarStatusAsync(LicencaFilterQuery query, CancellationToken ct = default);

    /// <summary>Renova uma licença ambiental, atualizando validade e status.</summary>
    Task<LicencaStatusViewModel> RenovarAsync(int licencaId, LicencaRenovacaoViewModel input, CancellationToken ct = default);
}
