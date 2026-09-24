namespace Application.Interfaces;

public interface ISeederService
{
    Task SeedVideoPrivaciesAsync();
    Task SeedVideosAsync(string jsonPath, string videosFolder);
    Task UpdateDatabase();
}
