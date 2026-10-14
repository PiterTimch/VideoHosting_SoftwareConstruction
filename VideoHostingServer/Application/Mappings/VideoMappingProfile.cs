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

    public IQueryable<VideoItemModel> ProjectToItemModel(IQueryable<VideoEntity> query)
        => query.Select(x => new VideoItemModel
        {
            Id = x.Id,
            Title = x.Title,
            Slug = x.Slug,
            Description = x.Description,
            ViewCount = x.ViewCount,
            DateCreated = x.DateCreated.ToString("dd.MM.yyyy'р.'"),
            Image = x.Image,
            Video = x.Video,
            Portfolio = x.Portfolio != null ? new Application.Models.Portfolio.PortfolioItemModel
            {
                Id = x.Portfolio.Id,
                Name = x.Portfolio.Name,
                NickName = x.Portfolio.NickName,
                Description = x.Portfolio.Description,
                Specialization = x.Portfolio.Specialization,
                Contacts = x.Portfolio.Contacts,
                Settings = x.Portfolio.Settings,
                AvatarImage = x.Portfolio.AvatarImage,
                BannerImage = x.Portfolio.BannerImage,
            } : null,
            Privacy = x.Privacy != null ? new VideoPrivacyItemModel
            {
                Id = x.Privacy.Id,
                Name = x.Privacy.Name,
                SystemCode = x.Privacy.SystemCode,
            } : null,
            LikesCount = x.VideoReactions.Count(r => r.IsLike),
            DislikesCount = x.VideoReactions.Count(r => !r.IsLike),
        });

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
