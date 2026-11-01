using MediatR;

namespace Application.Features.Portfolio.Commands.TogglePortfolioSubscription;

public record TogglePortfolioSubscriptionCommand(long PortfolioId) : IRequest;
