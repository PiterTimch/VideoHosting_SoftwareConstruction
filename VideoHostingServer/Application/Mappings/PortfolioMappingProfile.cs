using Application.Models.Portfolio;
using Domain.Entities.Portfolio;
using Riok.Mapperly.Abstractions;

namespace Application.Mappings;

[Mapper]
public partial class PortfolioMappingProfile
{
    [MapProperty(nameof(PortfolioEntity.Subscribers), nameof(PortfolioItemModel.SubscriberCount))]
    [MapperIgnoreTarget(nameof(PortfolioItemModel.IsSubscribed))]
    public partial PortfolioItemModel MapToItemModel(PortfolioEntity entity);

    private static int MapSubscribersToCount(ICollection<PortfolioSubscriberEntity>? subscribers)
        => subscribers?.Count(x => x.User == null || !x.User.IsDeleted) ?? 0;

    [MapperIgnoreTarget(nameof(PortfolioEntity.AvatarImage))]
    [MapperIgnoreTarget(nameof(PortfolioEntity.BannerImage))]
    public partial PortfolioEntity MapToEntity(PortfolioCreateModel model);

    [MapperIgnoreTarget(nameof(PortfolioEntity.Id))]
    [MapperIgnoreTarget(nameof(PortfolioEntity.AvatarImage))]
    [MapperIgnoreTarget(nameof(PortfolioEntity.BannerImage))]
    public partial void MapToEntity(PortfolioUpdateModel model, PortfolioEntity entity);
    
    [MapProperty(nameof(PortfolioEntity.Subscribers), nameof(PortfolioItemModel.SubscriberCount))]
    [MapperIgnoreTarget(nameof(PortfolioItemModel.IsSubscribed))]
    public partial IQueryable<PortfolioItemModel> ProjectToItemModel(IQueryable<PortfolioEntity> query);
}
