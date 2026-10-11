namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>
/// Parâmetros de paginação reaproveitados por todos os endpoints de listagem.
/// Limita o tamanho da página para proteger a aplicação contra consultas abusivas.
/// </summary>
public class PaginationQuery
{
    private const int MaxPageSize = 100;
    private int _pageSize = 20;
    private int _pageNumber = 1;

    /// <summary>Número da página (base 1).</summary>
    public int PageNumber
    {
        get => _pageNumber;
        set => _pageNumber = value < 1 ? 1 : value;
    }

    /// <summary>Quantidade de itens por página (máximo de 100).</summary>
    public int PageSize
    {
        get => _pageSize;
        set => _pageSize = value < 1 ? 1 : (value > MaxPageSize ? MaxPageSize : value);
    }

    public int Skip => (PageNumber - 1) * PageSize;
}
