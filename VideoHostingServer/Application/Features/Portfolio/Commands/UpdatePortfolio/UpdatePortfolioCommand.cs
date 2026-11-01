using Application.Models.Portfolio;
using MediatR;

namespace Application.Features.Portfolio.Commands.UpdatePortfolio;

public record UpdatePortfolioCommand(PortfolioUpdateModel Model) : IRequest<PortfolioItemModel>;
