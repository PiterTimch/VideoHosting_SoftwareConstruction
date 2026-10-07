using Application.Models.Portfolio;
using MediatR;

namespace Application.Features.Portfolio.Commands.CreatePortfolio;

public record CreatePortfolioCommand(PortfolioCreateModel Model) : IRequest<PortfolioItemModel>;
