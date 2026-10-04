namespace Application.Models.User;

public class UserSeedModel
{
    public string FirstName { get; set; } = string.Empty;
    public string LastName { get; set; } = string.Empty;
    public string Email { get; set; } = string.Empty;
    public string Password { get; set; } = string.Empty;
    public string? ImagePath { get; set; }
    public List<string> Roles { get; set; } = new();
}
