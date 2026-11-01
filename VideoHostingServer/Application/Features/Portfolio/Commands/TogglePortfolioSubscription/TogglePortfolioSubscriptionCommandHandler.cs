using Application.Interfaces;
using Domain;
using Domain.Entities.Portfolio;
using MediatR;
using Microsoft.EntityFrameworkCore;

namespace Application.Features.Portfolio.Commands.TogglePortfolioSubscription;

public class TogglePortfolioSubscriptionCommandHandler(
    IGenericRepository<PortfolioEntity, long> portfolios,
    AppDbContext dbContext,
    ICurrentUserService currentUser)
    : IRequestHandler<TogglePortfolioSubscriptionCommand>
{
    public async Task Handle(TogglePortfolioSubscriptionCommand request, CancellationToken cancellationToken)
    {
        var userId = currentUser.GetCurrentUserId();
        if (request.PortfolioId == userId)
            throw new InvalidOperationException("Не можна підписатися на власне портфоліо.");

        var exists = await portfolios.AsQurable().AnyAsync(x => x.Id == request.PortfolioId, cancellationToken);
        if (!exists)
            throw new KeyNotFoundException("Портфоліо не знайдено.");

        var subscription = await dbContext.PortfolioSubscribers
            .FirstOrDefaultAsync(x => x.PortfolioId == request.PortfolioId && x.UserId == userId, cancellationToken);

        if (subscription is not null)
        {
            dbContext.PortfolioSubscribers.Remove(subscription);
            await dbContext.SaveChangesAsync(cancellationToken);
            return;
        }

        await dbContext.PortfolioSubscribers.AddAsync(new PortfolioSubscriberEntity { PortfolioId = request.PortfolioId, UserId = userId }, cancellationToken);
        await dbContext.SaveChangesAsync(cancellationToken);
    }
}
