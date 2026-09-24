namespace Application.Models.Search;

public class PaginationModel
{
    public int TotalCount { get; set; }
    public int TotalPages { get; set; }
    public int ItemsPerPage { get; set; }
    public int CurrentPage { get; set; }
}
