using Application.Models.Video;
using Domain.Entities.Video;
using Riok.Mapperly.Abstractions;

namespace Application.Mappings;

[Mapper]
public partial class VideoMappingProfile
{
    public partial VideoItemModel MapToItemModel(VideoEntity entity);

    public partial IQueryable<VideoItemModel> ProjectToItemModel(IQueryable<VideoEntity> query);

    public partial VideoPrivacyItemModel MapToItemModel(VideoPrivacyEntity entity);
    public partial IQueryable<VideoPrivacyItemModel> ProjectToItemModel(IQueryable<VideoPrivacyEntity> query);

    [MapperIgnoreTarget(nameof(VideoEntity.Image))]
    [MapperIgnoreTarget(nameof(VideoEntity.Video))]
    public partial VideoEntity MapToEntity(VideoSeedModel model);

    [MapperIgnoreTarget(nameof(VideoEntity.Image))]
    [MapperIgnoreTarget(nameof(VideoEntity.Video))]
    public partial VideoEntity MapToEntity(VideoCreateModel model);

    [MapperIgnoreTarget(nameof(VideoEntity.Image))]
    [MapperIgnoreTarget(nameof(VideoEntity.Video))]
    public partial void MapToEntity(VideoUpdateModel model, VideoEntity entity);

    protected static string MapDateCreated(DateTime dateCreated)
        => dateCreated.ToString("dd.MM.yyyy'р.'");
}
