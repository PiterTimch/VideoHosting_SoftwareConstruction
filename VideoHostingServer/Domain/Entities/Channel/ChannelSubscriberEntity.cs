using Domain.Entities.Identity;

namespace Domain.Entities.Channel;

public class ChannelSubscriberEntity
{
    public long ChannelId { get; set; }
    public virtual ChannelEntity Channel { get; set; } = null!;

    public long UserId { get; set; }
    public virtual UserEntity User { get; set; } = null!;
}
