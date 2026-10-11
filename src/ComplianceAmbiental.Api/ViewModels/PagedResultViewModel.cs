namespace ComplianceAmbiental.Api.ViewModels;

/// <summary>
/// Envelope padrão para respostas paginadas, com metadados de navegação.
/// </summary>
public class PagedResultViewModel<T>
{
    public IReadOnlyList<T> Items { get; set; } = new List<T>();

    public int PageNumber { get; set; }

    public int PageSize { get; set; }

    public int TotalItems { get; set; }

    public int TotalPages => PageSize == 0 ? 0 : (int)Math.Ceiling(TotalItems / (double)PageSize);

    public bool HasPrevious => PageNumber > 1;

    public bool HasNext => PageNumber < TotalPages;

    public PagedResultViewModel() { }

    public PagedResultViewModel(IReadOnlyList<T> items, int totalItems, int pageNumber, int pageSize)
    {
        Items = items;
        TotalItems = totalItems;
        PageNumber = pageNumber;
        PageSize = pageSize;
    }
}
