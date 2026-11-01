using Application.Interfaces;
using Application.Mappings;
using Application.Models.Portfolio;
using Domain.Entities.Portfolio;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolio.Queries.GetPortfolio;

public class GetPortfolioQueryHandler(
    IGenericRepository<PortfolioEntity, long> portfolios,
    PortfolioMappingProfile mapper,
    ICurrentUserService currentUser)
    : IRequestHandler<GetPortfolioQuery, PortfolioItemModel?>
{
    public async Task<PortfolioItemModel?> Handle(GetPortfolioQuery request, CancellationToken cancellationToken)
    {
        var portfolio = await portfolios.AsQurable()
            .AsNoTracking()
            .Include(x => x.Subscribers)
            .FirstOrDefaultAsync(x => x.Id == request.Id, cancellationToken);

        var userId = currentUser.TryGetCurrentUserId();
        if (portfolio is null)
            return null;

        var result = mapper.MapToItemModel(portfolio);
        result.OwnerId = portfolio.Id;
        result.IsSubscribed = userId.HasValue && portfolio.Subscribers?.Any(x => x.UserId == userId.Value) == true;
        return result;
    }
}
