using Microsoft.AspNetCore.Http;

namespace Application.Models.Account;

public class AccountRegisterModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;

    public IFormFile? ImageFile { get; set; } = null;
}
