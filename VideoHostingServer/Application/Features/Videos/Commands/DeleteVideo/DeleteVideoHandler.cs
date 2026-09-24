using Application.Interfaces;
using Domain.Entities.Video;
using MediatR;

namespace Application.Features.Videos.Commands.DeleteVideo;

public class DeleteVideoHandler(IGenericRepository<VideoEntity, long> repo)
    : IRequestHandler<DeleteVideoCommand>
{
    public async Task Handle(DeleteVideoCommand request, CancellationToken cancellationToken)
    {
        var video = await repo.GetByIdAsync(request.Model.Id);
        if (video == null) return;

        await repo.DeleteAsync(video.Id);
    }
}
