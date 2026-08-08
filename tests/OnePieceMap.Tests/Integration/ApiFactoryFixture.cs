using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace OnePieceMap.Tests.Integration;

// Spins up a real Postgres container so integration tests exercise the actual jsonb
// Translations mapping and EF migrations, not just an approximation. Shared across the
// whole collection (see ApiCollection) so the container only starts/stops once.
public class ApiFactoryFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer postgres = new PostgreSqlBuilder("postgres:17-alpine").Build();

    private WebApplicationFactory<Program>? factory;

    public HttpClient CreateClient() => factory!.CreateClient();

    public async Task InitializeAsync()
    {
        await postgres.StartAsync();

        factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            // Development triggers Program.cs's own migrate + seed on startup, so
            // tests reuse the exact same startup path production runs through.
            builder.UseEnvironment("Development");
            builder.ConfigureAppConfiguration((_, config) =>
            {
                config.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = postgres.GetConnectionString()
                });
            });
        });
    }

    public async Task DisposeAsync()
    {
        if (factory is not null)
        {
            await factory.DisposeAsync();
        }

        await postgres.DisposeAsync();
    }
}

[CollectionDefinition("Api")]
public class ApiCollection : ICollectionFixture<ApiFactoryFixture>;
