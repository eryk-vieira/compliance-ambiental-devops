using ComplianceAmbiental.Api.Services;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAmbiental.Api.Controllers;

/// <summary>Relatórios de emissão de CO2: consolidação mensal e alertas de ultrapassagem do teto.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class EmissoesController : ControllerBase
{
    private readonly IEmissaoService _service;

    public EmissoesController(IEmissaoService service) => _service = service;

    /// <summary>Consolidação mensal das emissões de CO2 por transportadora (paginada).</summary>
    [HttpGet("consolidado")]
    [ProducesResponseType(typeof(PagedResultViewModel<EmissaoConsolidadoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Consolidado([FromQuery] EmissaoFilterQuery query, CancellationToken ct)
    {
        var resultado = await _service.ConsolidarAsync(query, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Lista alertas de transportadoras que atingiram 80% ou mais do teto mensal de CO2,
    /// com classificação de severidade (ATENCAO, ALTO, CRITICO).
    /// </summary>
    [HttpGet("alertas")]
    [ProducesResponseType(typeof(PagedResultViewModel<AlertaEmissaoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Alertas([FromQuery] EmissaoFilterQuery query, CancellationToken ct)
    {
        var resultado = await _service.ListarAlertasAsync(query, ct);
        return Ok(resultado);
    }
}
