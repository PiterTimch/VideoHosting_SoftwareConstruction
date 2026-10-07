using Microsoft.AspNetCore.Identity;
using Domain.Entities.Portfolio;

namespace Domain.Entities.Identity;

public class UserEntity : IdentityUser<long>
{
    public DateTime DateCreated { get; set; } = DateTime.SpecifyKind(DateTime.Now, DateTimeKind.Utc);
    public string? FirstName { get; set; } = null;
    public string? LastName { get; set; } = null;
    public string? Image { get; set; } = null;

    public virtual ICollection<UserRoleEntity>? UserRoles { get; set; }
    public virtual ICollection<UserLoginEntity>? UserLogins { get; set; }

    public virtual PortfolioEntity? Portfolio { get; set; }
    public virtual ICollection<PortfolioSubscriberEntity>? SubscribedPortfolios { get; set; } = new List<PortfolioSubscriberEntity>();

    public bool IsDeleted { get; set; }
}
