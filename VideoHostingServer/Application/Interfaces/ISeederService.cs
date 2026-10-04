namespace Application.Interfaces;

public interface ISeederService
{
    Task SeedRolesAsync();
    Task SeedUsersAsync(string jsonPath);
    Task SeedVideoPrivaciesAsync();
    Task SeedVideosAsync(string jsonPath, string videosFolder);
    Task UpdateDatabase();
}
