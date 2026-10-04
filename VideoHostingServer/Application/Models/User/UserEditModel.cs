using Microsoft.AspNetCore.Http;

namespace Application.Models.User;

public class UserEditModel
{
    public long Id { get; set; }
    public string Email { get; set; } = string.Empty;
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public IFormFile? Image { get; set; }
    public List<string>? Roles { get; set; }
}
