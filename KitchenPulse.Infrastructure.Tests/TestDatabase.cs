using KitchenPulse.Infrastructure.Persistence;
using Microsoft.EntityFrameworkCore;

namespace KitchenPulse.Infrastructure.Tests;

public class TestDatabase : IAsyncDisposable
{
    private readonly string _databaseName;

    public TestDatabase()
    {
        _databaseName = $"KitchenPulseIntegrationTest_{Guid.NewGuid():N}";
    }

    public KitchenPulseDbContext CreateContext()
    {

        string connectionString =
                            $"Server=(localdb)\\MSSQLLocalDB;" +
                            $"Database={_databaseName};" +
                            "Trusted_Connection=True;";

        var options = new DbContextOptionsBuilder<KitchenPulseDbContext>()
                    .UseSqlServer(connectionString)
                    .Options;

        var context = new KitchenPulseDbContext(options);

        return context;

    }

    public async Task InitializeAsync()
    {
        await using var context = CreateContext();

        await context.Database.MigrateAsync();
    }

    public async ValueTask DisposeAsync()
    {
        await using var context = CreateContext();

        await context.Database.EnsureDeletedAsync();
    }
}
