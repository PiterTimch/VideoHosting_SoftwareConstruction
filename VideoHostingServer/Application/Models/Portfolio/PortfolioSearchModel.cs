using Application.Models.Search;

namespace Application.Models.Portfolio;

public class PortfolioSearchModel : BaseSearchParamsModel
{
    public string? Q { get; set; }
    public bool? IsSubscribed { get; set; }
}
