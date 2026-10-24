using Application.Interfaces;
using Domain.Entities.Video;

namespace Infrastructure.Repositories;

public class VideoRepositoryDecorator(
    IGenericRepository<VideoEntity, long> repository,
    IImageService imageService) : IGenericRepository<VideoEntity, long>
{
    public Task<VideoEntity?> GetByIdAsync(long id, bool isSoft = false) => repository.GetByIdAsync(id, isSoft);

    public Task<IReadOnlyList<VideoEntity>> ListAllAsync(bool isSoft = false) => repository.ListAllAsync(isSoft);

    public Task<IReadOnlyList<VideoEntity>> ListAsync(ISpecification<VideoEntity> spec) => repository.ListAsync(spec);

    public Task AddAsync(VideoEntity entity) => repository.AddAsync(entity);

    public Task UpdateAsync(VideoEntity entity) => repository.UpdateAsync(entity);

    public async Task DeleteAsync(long id)
    {
        var video = await repository.GetByIdAsync(id);
        await repository.DeleteAsync(id);

        if (!string.IsNullOrEmpty(video?.Image))
            await imageService.DeleteImageAsync(video.Image);
    }

    public Task<int> SaveChangesAsync() => repository.SaveChangesAsync();

    public IQueryable<VideoEntity> AsQurable(bool isSoft = false) => repository.AsQurable(isSoft);
}
