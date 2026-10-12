using Domain;
using Microsoft.EntityFrameworkCore;

namespace UnitTests.Common;

public static class TestDbContextHelper
{
    public static AppDbContext CreateContext(string? dbName = null)
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(dbName ?? Guid.NewGuid().ToString())
            .Options;
        return new AppDbContext(options);
    }
}
