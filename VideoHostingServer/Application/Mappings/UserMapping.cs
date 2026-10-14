using Application.Models.Account;
using Application.Models.User;
using Domain.Entities.Identity;
using Riok.Mapperly.Abstractions;

namespace Application.Mappings;

[Mapper]
public partial class UserMapping
{
    [MapProperty(nameof(UserEntity.UserRoles), nameof(UserItemModel.Roles))]
    [MapProperty(nameof(UserEntity.UserLogins), nameof(UserItemModel.LoginTypes))]
    public partial UserItemModel MapToItemModel(UserEntity entity);

    [MapProperty(nameof(AccountRegisterModel.Email), nameof(UserEntity.UserName))]
    [MapperIgnoreTarget(nameof(UserEntity.Image))]
    public partial UserEntity MapToEntity(AccountRegisterModel model);

    [MapProperty(nameof(UserSeedModel.Email), nameof(UserEntity.UserName))]
    [MapProperty(nameof(UserSeedModel.ImagePath), nameof(UserEntity.Image))]
    public partial UserEntity MapToEntity(UserSeedModel model);

    [MapperIgnoreTarget(nameof(UserEntity.Image))]
    public partial void MapToEntity(UserEditModel model, UserEntity entity);

    public IQueryable<UserItemModel> ProjectToItemModel(IQueryable<UserEntity> query)
        => query.Select(x => new UserItemModel
        {
            Id = x.Id,
            Email = x.Email ?? string.Empty,
            FirstName = x.FirstName ?? string.Empty,
            LastName = x.LastName ?? string.Empty,
            Image = x.Image ?? string.Empty,
            Roles = x.UserRoles != null
                ? x.UserRoles.Select(ur => ur.Role.Name).ToList()
                : new List<string>(),
            LoginTypes = x.UserLogins != null
                ? x.UserLogins.Select(l => l.LoginProvider).ToList()
                : new List<string>(),
            IsLoginGoogle = x.UserLogins != null && x.UserLogins.Any(l => l.LoginProvider == "Google"),
            IsLoginPassword = x.PasswordHash != null && x.PasswordHash != "",
        });

    private List<string> MapRoles(ICollection<UserRoleEntity> userRoles)
    {
        return userRoles.Select(ur => ur.Role.Name).ToList();
    }

    private List<string> MapLoginTypes(ICollection<UserLoginEntity> logins)
    {
        return logins.Select(l => l.LoginProvider).ToList();
    }

    private void AfterMapToItemModel(UserEntity entity, UserItemModel model)
    {
        model.IsLoginGoogle = model.LoginTypes.Any(x => x.Contains("Google", StringComparison.OrdinalIgnoreCase));
        model.IsLoginPassword = !string.IsNullOrEmpty(entity.PasswordHash);
    }
}
