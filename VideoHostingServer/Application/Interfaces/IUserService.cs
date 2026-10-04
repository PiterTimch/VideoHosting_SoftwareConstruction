using Application.Models.User;

namespace Application.Interfaces;

public interface IUserService
{
    Task LoadLoginsAndRolesAsync(List<UserItemModel> users);
}
