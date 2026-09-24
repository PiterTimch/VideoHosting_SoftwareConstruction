using MediatR;

namespace Application.Features.Videos.Commands.IncrementView;

public record IncrementViewCommand(long Id) : IRequest;
