namespace Application.Models.Account;

public class AccountValidateResetTokenModel
{
    public string Email { get; set; } = string.Empty;
    public string Token { get; set; } = string.Empty;
}
