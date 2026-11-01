using Application.Models.Portfolio;
using MediatR;

namespace Application.Features.Portfolio.Queries.GetPortfolio;

public record GetPortfolioQuery(long Id) : IRequest<PortfolioItemModel?>;
