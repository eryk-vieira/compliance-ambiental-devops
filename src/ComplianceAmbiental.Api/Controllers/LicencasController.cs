using ComplianceAmbiental.Api.Services;
using ComplianceAmbiental.Api.ViewModels;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAmbiental.Api.Controllers;

/// <summary>Gestão de compliance das licenças ambientais.</summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class LicencasController : ControllerBase
{
    private readonly ILicencaService _service;

    public LicencasController(ILicencaService service) => _service = service;

    /// <summary>Lista o status de compliance das licenças (paginada, com filtros de vencimento).</summary>
    [HttpGet("status")]
    [ProducesResponseType(typeof(PagedResultViewModel<LicencaStatusViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Status([FromQuery] LicencaFilterQuery query, CancellationToken ct)
    {
        var resultado = await _service.ListarStatusAsync(query, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Renova uma licença ambiental (endpoint crítico, restrito ao perfil Admin).
    /// </summary>
    [HttpPost("{id:int}/renovar")]
    [Authorize(Roles = "Admin")]
    [ProducesResponseType(typeof(LicencaStatusViewModel), StatusCodes.Status200OK)]
    [ProducesResponseType(StatusCodes.Status404NotFound)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Renovar(int id, [FromBody] LicencaRenovacaoViewModel input, CancellationToken ct)
    {
        var resultado = await _service.RenovarAsync(id, input, ct);
        return Ok(resultado);
    }
}
