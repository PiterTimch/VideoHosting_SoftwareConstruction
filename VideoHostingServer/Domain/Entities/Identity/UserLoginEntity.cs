using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class UserLoginEntity : IdentityUserLogin<long>
{
    public virtual UserEntity User { get; set; } = null!;
}
