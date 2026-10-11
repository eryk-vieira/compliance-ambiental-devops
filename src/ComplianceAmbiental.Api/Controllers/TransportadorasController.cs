using ComplianceAmbiental.Api.Services;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAmbiental.Api.Controllers;

/// <summary>Transportadoras e seus indicadores de compliance ambiental.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class TransportadorasController : ControllerBase
{
    private readonly ITransportadoraService _service;

    public TransportadorasController(ITransportadoraService service) => _service = service;

    /// <summary>Lista transportadoras com score de compliance (paginada).</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultViewModel<TransportadoraComplianceViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] TransportadoraFilterQuery query, CancellationToken ct)
    {
        var resultado = await _service.ListarAsync(query, ct);
        return Ok(resultado);
    }

    /// <summary>Dashboard ESG consolidado de uma transportadora, com histórico mensal de emissões.</summary>
    [HttpGet("{id:int}/dashboard")]
    [ProducesResponseType(typeof(DashboardEsgViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    public async Task<IActionResult> Dashboard(int id, CancellationToken ct)
    {
        var resultado = await _service.ObterDashboardAsync(id, ct);
        return Ok(resultado);
    }
}
