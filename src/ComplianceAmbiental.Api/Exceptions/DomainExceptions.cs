namespace ComplianceAmbiental.Api.Exceptions;

/// <summary>Recurso solicitado não foi encontrado (HTTP 404).</summary>
public class NotFoundException : Exception
{
    public NotFoundException(string message) : base(message) { }
}

/// <summary>Violação de regra de negócio de compliance (HTTP 409).</summary>
public class BusinessRuleException : Exception
{
    public BusinessRuleException(string message) : base(message) { }
}

/// <summary>Erro de validação de dados de entrada (HTTP 400).</summary>
public class ValidationException : Exception
{
    public IReadOnlyDictionary<string, string[]> Errors { get; }

    public ValidationException(IReadOnlyDictionary<string, string[]> errors)
        : base("Um ou mais erros de validação ocorreram.")
    {
        Errors = errors;
    }
}
