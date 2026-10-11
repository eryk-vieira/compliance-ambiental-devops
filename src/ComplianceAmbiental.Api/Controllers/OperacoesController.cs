using ComplianceAmbiental.Api.Exceptions;
using ComplianceAmbiental.Api.Services;
using ComplianceAmbiental.Api.ViewModels;
using FluentValidation;
using Microsoft.AspNetCore.Authorization;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAmbiental.Api.Controllers;

/// <summary>
/// Operações logísticas: registro com cálculo automático de emissão de CO2,
/// validação de licença ambiental vigente e listagem paginada.
/// </summary>
[ApiController]
[Route("api/[controller]")]
[Authorize]
[Produces("application/json")]
public class OperacoesController : ControllerBase
{
    private readonly IOperacaoService _service;
    private readonly IValidator<OperacaoCreateViewModel> _validator;

    public OperacoesController(IOperacaoService service, IValidator<OperacaoCreateViewModel> validator)
    {
        _service = service;
        _validator = validator;
    }

    /// <summary>Lista operações logísticas com paginação e filtros opcionais.</summary>
    [HttpGet]
    [ProducesResponseType(typeof(PagedResultViewModel<OperacaoViewModel>), StatusCodes.Status200OK)]
    public async Task<IActionResult> Listar([FromQuery] OperacaoFilterQuery query, CancellationToken ct)
    {
        var resultado = await _service.ListarAsync(query, ct);
        return Ok(resultado);
    }

    /// <summary>
    /// Registra uma nova operação logística. A emissão de CO2 é calculada
    /// automaticamente e a operação é bloqueada se a transportadora não possuir
    /// licença ambiental vigente na data informada.
    /// </summary>
    [HttpPost]
    [ProducesResponseType(typeof(OperacaoViewModel), StatusCodes.Status201Created)]
    [ProducesResponseType(StatusCodes.Status400BadRequest)]
    [ProducesResponseType(StatusCodes.Status409Conflict)]
    public async Task<IActionResult> Registrar([FromBody] OperacaoCreateViewModel input, CancellationToken ct)
    {
        var validacao = await _validator.ValidateAsync(input, ct);
        if (!validacao.IsValid)
        {
            var erros = validacao.Errors
                .GroupBy(e => e.PropertyName)
                .ToDictionary(g => g.Key, g => g.Select(e => e.ErrorMessage).ToArray());
            throw new Exceptions.ValidationException(erros);
        }

        var operacao = await _service.RegistrarAsync(input, ct);
        return Created($"/api/operacoes/{operacao.Id}", operacao);
    }
}
