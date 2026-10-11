using ComplianceAmbiental.Api.ViewModels;

namespace ComplianceAmbiental.Api.Services;

public interface IOperacaoService
{
    /// <summary>
    /// Registra uma operação logística aplicando as regras de compliance:
    /// valida licença ambiental vigente, calcula a emissão de CO2 e atualiza
    /// o acumulado mensal da transportadora.
    /// </summary>
    Task<OperacaoViewModel> RegistrarAsync(OperacaoCreateViewModel input, CancellationToken ct = default);

    /// <summary>Lista operações com paginação e filtros opcionais.</summary>
    Task<PagedResultViewModel<OperacaoViewModel>> ListarAsync(OperacaoFilterQuery query, CancellationToken ct = default);
}
