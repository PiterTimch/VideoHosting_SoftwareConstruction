namespace Application.Constants;

public class Roles
{
    public const string Admin = "Admin";
    public const string User = "User";
    public const string Freelancer = "Freelancer";
    public static string[] AllRoles => new[] { Admin, User, Freelancer };
}
