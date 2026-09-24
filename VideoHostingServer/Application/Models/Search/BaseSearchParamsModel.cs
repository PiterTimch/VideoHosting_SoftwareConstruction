namespace Application.Models.Search;

public class BaseSearchParamsModel
{
    public int Page { get; set; } = 1;
    public int ItemPerPage { get; set; } = 10;
}
