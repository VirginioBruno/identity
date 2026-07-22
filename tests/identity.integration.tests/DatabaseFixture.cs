using Microsoft.AspNetCore.Hosting;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.Configuration;
using Testcontainers.PostgreSql;

namespace identity.integration.tests;

public sealed class DatabaseFixture : IAsyncLifetime
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder("postgres:17-alpine")
        .WithDatabase("identity")
        .WithUsername("identity")
        .WithPassword("integration-test-password")
        .Build();

    public WebApplicationFactory<Program> Factory { get; private set; } = null!;

    public async Task InitializeAsync()
    {
        await _postgreSqlContainer.StartAsync();

        Factory = new WebApplicationFactory<Program>().WithWebHostBuilder(builder =>
        {
            builder.UseEnvironment("Testing");
            builder.ConfigureAppConfiguration((_, configuration) =>
            {
                configuration.AddInMemoryCollection(new Dictionary<string, string?>
                {
                    ["ConnectionStrings:DefaultConnection"] = _postgreSqlContainer.GetConnectionString(),
                    ["Jwt:Issuer"] = "identity-tests",
                    ["Jwt:Audience"] = "identity-test-client",
                    ["Jwt:SigningKey"] = "integration-tests-only-signing-key-32-chars",
                    ["Jwt:ExpirationMinutes"] = "5"
                });
            });
        });
    }

    public async Task DisposeAsync()
    {
        await Factory.DisposeAsync();
        await _postgreSqlContainer.DisposeAsync();
    }
}
