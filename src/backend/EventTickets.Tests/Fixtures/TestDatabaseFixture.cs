using EventTickets.Infrastructure.Data;
using Microsoft.EntityFrameworkCore;
using Xunit;

namespace EventTickets.Tests.Fixtures;

public class TestDatabaseFixture : IAsyncLifetime
{
    public ApplicationDbContext Context { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        var options = new DbContextOptionsBuilder<ApplicationDbContext>()
            .UseInMemoryDatabase(databaseName: Guid.NewGuid().ToString())
            .Options;

        Context = new ApplicationDbContext(options);
        await Context.Database.EnsureCreatedAsync();
    }

    public async Task DisposeAsync()
    {
        await Context.Database.EnsureDeletedAsync();
        await Context.DisposeAsync();
    }
}
