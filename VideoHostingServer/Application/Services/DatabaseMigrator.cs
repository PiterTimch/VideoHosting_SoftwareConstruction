using Application.Interfaces;
using Domain;
using Microsoft.EntityFrameworkCore;

namespace Application.Services;

public class DatabaseMigrator(AppDbContext appDbContext) : IDatabaseMigrator
{
    public Task MigrateAsync() => appDbContext.Database.MigrateAsync();
}
