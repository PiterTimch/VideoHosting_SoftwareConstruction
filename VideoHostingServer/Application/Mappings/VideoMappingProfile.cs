using Application.Models.Video;
using Domain.Entities.Video;
using Riok.Mapperly.Abstractions;

namespace Application.Mappings;

[Mapper]
[UseStaticMapper(typeof(PortfolioMappingProfile))]
public partial class VideoMappingProfile
{
    [MapProperty(nameof(VideoEntity.Portfolio), nameof(VideoItemModel.Portfolio))]
    public partial VideoItemModel MapToItemModel(VideoEntity entity);

    [MapPropertyFromSource(nameof(VideoItemModel.LikesCount), Use = nameof(MapLikesCountQuery))]
    [MapPropertyFromSource(nameof(VideoItemModel.DislikesCount), Use = nameof(MapDislikesCountQuery))]
    public partial IQueryable<VideoItemModel> ProjectToItemModel(IQueryable<VideoEntity> query);

    private static readonly System.Linq.Expressions.Expression<Func<VideoEntity, int>> MapLikesCountQuery =
        video => video.VideoReactions.Count(r => r.IsLike);

    private static readonly System.Linq.Expressions.Expression<Func<VideoEntity, int>> MapDislikesCountQuery =
        video => video.VideoReactions.Count(r => !r.IsLike);

    private void AfterMapToItemModel(VideoEntity entity, VideoItemModel model)
    {
        model.LikesCount = entity.VideoReactions?.Count(r => r.IsLike) ?? 0;
        model.DislikesCount = entity.VideoReactions?.Count(r => !r.IsLike) ?? 0;
    }

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
    [MapperIgnoreTarget(nameof(VideoEntity.PortfolioId))]
    public partial void MapToEntity(VideoUpdateModel model, VideoEntity entity);

    public partial VideoReactionEntity MapToEntity(VideoReactionModel model);

    protected static string MapDateCreated(DateTime dateCreated)
        => dateCreated.ToString("dd.MM.yyyy'р.'");

    protected static int MapLikesCount(ICollection<VideoReactionEntity> reactions)
        => reactions.Count(r => r.IsLike);

    protected static int MapDislikesCount(ICollection<VideoReactionEntity> reactions)
        => reactions.Count(r => !r.IsLike);
}
