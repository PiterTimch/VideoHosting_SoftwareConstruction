using Application.Interfaces;

namespace Application.Services;

public class SeederService(
    IRoleSeeder roleSeeder,
    IUserSeeder userSeeder,
    IVideoPrivacySeeder videoPrivacySeeder,
    IVideoSeeder videoSeeder,
    IDatabaseMigrator databaseMigrator) : ISeederService
{
    public Task SeedRolesAsync() => roleSeeder.SeedAsync();

    public Task SeedUsersAsync(string jsonPath) => userSeeder.SeedAsync(jsonPath);

    public Task SeedVideoPrivaciesAsync() => videoPrivacySeeder.SeedAsync();

    public Task SeedVideosAsync(string jsonPath, string videosFolder) => videoSeeder.SeedAsync(jsonPath, videosFolder);

    public Task UpdateDatabase() => databaseMigrator.MigrateAsync();
}
