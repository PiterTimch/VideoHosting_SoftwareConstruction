using Microsoft.AspNetCore.Identity;

namespace Domain.Entities.Identity;

public class UserRoleEntity : IdentityUserRole<long>
{
    public UserEntity User { get; set; } = null!;
    public RoleEntity Role { get; set; } = null!;
}
