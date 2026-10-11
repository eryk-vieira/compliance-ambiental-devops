using System.Text.Json;
using ComplianceAmbiental.Api.Exceptions;
using Microsoft.AspNetCore.Mvc;

namespace ComplianceAmbiental.Api.Middleware;

/// <summary>
/// Middleware global de tratamento de exceções. Converte exceções de domínio em
/// respostas ProblemDetails (RFC 7807) com o status HTTP adequado, evitando vazar
/// stack traces e padronizando os erros da API.
/// </summary>
public class ExceptionHandlingMiddleware
{
    private readonly RequestDelegate _next;
    private readonly ILogger<ExceptionHandlingMiddleware> _logger;

    public ExceptionHandlingMiddleware(RequestDelegate next, ILogger<ExceptionHandlingMiddleware> logger)
    {
        _next = next;
        _logger = logger;
    }

    public async Task InvokeAsync(HttpContext context)
    {
        try
        {
            await _next(context);
        }
        catch (Exception ex)
        {
            await HandleAsync(context, ex);
        }
    }

    private async Task HandleAsync(HttpContext context, Exception ex)
    {
        ProblemDetails problem;

        switch (ex)
        {
            case ValidationException ve:
                problem = new ValidationProblemDetails(
                    ve.Errors.ToDictionary(kv => kv.Key, kv => kv.Value))
                {
                    Title = "Erro de validação",
                    Status = StatusCodes.Status400BadRequest
                };
                break;

            case NotFoundException nf:
                problem = new ProblemDetails
                {
                    Title = "Recurso não encontrado",
                    Detail = nf.Message,
                    Status = StatusCodes.Status404NotFound
                };
                break;

            case BusinessRuleException br:
                problem = new ProblemDetails
                {
                    Title = "Regra de negócio violada",
                    Detail = br.Message,
                    Status = StatusCodes.Status409Conflict
                };
                break;

            default:
                _logger.LogError(ex, "Erro não tratado ao processar a requisição.");
                problem = new ProblemDetails
                {
                    Title = "Erro interno do servidor",
                    Detail = "Ocorreu um erro inesperado ao processar a requisição.",
                    Status = StatusCodes.Status500InternalServerError
                };
                break;
        }

        context.Response.ContentType = "application/problem+json";
        context.Response.StatusCode = problem.Status ?? StatusCodes.Status500InternalServerError;

        var json = JsonSerializer.Serialize(problem, problem.GetType(),
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase });
        await context.Response.WriteAsync(json);
    }
}
