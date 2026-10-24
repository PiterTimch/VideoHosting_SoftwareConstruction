namespace Application.Interfaces;

public interface IRoleSeeder
{
    Task SeedAsync();
}

public interface IUserSeeder
{
    Task SeedAsync(string jsonPath);
}

public interface IVideoPrivacySeeder
{
    Task SeedAsync();
}

public interface IVideoSeeder
{
    Task SeedAsync(string jsonPath, string videosFolder);
}

public interface IDatabaseMigrator
{
    Task MigrateAsync();
}
