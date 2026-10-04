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

    [MapPropertyFromSource(nameof(UserItemModel.Roles), Use = nameof(MapRolesQuery))]
    [MapPropertyFromSource(nameof(UserItemModel.LoginTypes), Use = nameof(MapLoginsQuery))]
    [MapPropertyFromSource(nameof(UserItemModel.IsLoginGoogle), Use = nameof(MapIsGoogleQuery))]
    [MapPropertyFromSource(nameof(UserItemModel.IsLoginPassword), Use = nameof(MapIsPasswordQuery))]
    public partial IQueryable<UserItemModel> ProjectToItemModel(IQueryable<UserEntity> query);

    private static readonly System.Linq.Expressions.Expression<Func<UserEntity, List<string>>> MapRolesQuery =
        user => user.UserRoles.Select(ur => ur.Role.Name).ToList();

    private static readonly System.Linq.Expressions.Expression<Func<UserEntity, List<string>>> MapLoginsQuery =
        user => user.UserLogins.Select(l => l.LoginProvider).ToList();

    private static readonly System.Linq.Expressions.Expression<Func<UserEntity, bool>> MapIsGoogleQuery =
        user => user.UserLogins.Any(l => l.LoginProvider == "Google");

    private static readonly System.Linq.Expressions.Expression<Func<UserEntity, bool>> MapIsPasswordQuery =
        user => user.PasswordHash != null && user.PasswordHash != "";

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
