using Microsoft.AspNetCore.Identity;
using Domain.Entities.Channel;

namespace Domain.Entities.Identity;

public class UserEntity : IdentityUser<long>
{
    public DateTime DateCreated { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    public string? Image { get; set; } = null;

    public virtual ICollection<UserRoleEntity>? UserRoles { get; set; }
    public virtual ICollection<UserLoginEntity>? UserLogins { get; set; }

    public virtual ChannelEntity? Channel { get; set; }
    public virtual ICollection<ChannelSubscriberEntity>? SubscribedChannels { get; set; } = new List<ChannelSubscriberEntity>();

    public bool IsDeleted { get; set; }
}
