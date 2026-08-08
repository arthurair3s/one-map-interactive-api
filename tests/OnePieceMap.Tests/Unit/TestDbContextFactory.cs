using Microsoft.EntityFrameworkCore;
using OnePieceMap.Infrastructure.Data;

namespace OnePieceMap.Tests.Unit;

// Each call gets its own isolated InMemory database, so unit tests never leak state into each other.
internal static class TestDbContextFactory
{
    public static AppDbContext Create()
    {
        var options = new DbContextOptionsBuilder<AppDbContext>()
            .UseInMemoryDatabase(Guid.NewGuid().ToString())
            .Options;

        return new AppDbContext(options);
    }
}
