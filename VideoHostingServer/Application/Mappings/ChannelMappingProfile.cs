using Application.Models.Channel;
using Domain.Entities.Channel;
using Riok.Mapperly.Abstractions;

namespace Application.Mappings;

[Mapper]
public partial class ChannelMappingProfile
{
    [MapProperty(nameof(ChannelEntity.Subscribers), nameof(ChannelItemModel.SubscriberCount))]
    [MapperIgnoreTarget(nameof(ChannelItemModel.IsSubscribed))]
    public partial ChannelItemModel MapToItemModel(ChannelEntity entity);

    private static int MapSubscribersToCount(ICollection<ChannelSubscriberEntity>? subscribers)
        => subscribers?.Count(x => x.User == null || !x.User.IsDeleted) ?? 0;

    [MapperIgnoreTarget(nameof(ChannelEntity.AvatarImage))]
    [MapperIgnoreTarget(nameof(ChannelEntity.BannerImage))]
    public partial ChannelEntity MapToEntity(ChannelCreateModel model);

    [MapperIgnoreTarget(nameof(ChannelEntity.Id))]
    [MapperIgnoreTarget(nameof(ChannelEntity.AvatarImage))]
    [MapperIgnoreTarget(nameof(ChannelEntity.BannerImage))]
    public partial void MapToEntity(ChannelUpdateModel model, ChannelEntity entity);
    
    [MapProperty(nameof(ChannelEntity.Subscribers), nameof(ChannelItemModel.SubscriberCount))]
    [MapperIgnoreTarget(nameof(ChannelItemModel.IsSubscribed))]
    public partial IQueryable<ChannelItemModel> ProjectToItemModel(IQueryable<ChannelEntity> query);
}
