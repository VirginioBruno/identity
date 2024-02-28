using Testcontainers.PostgreSql;

namespace identity.integration.tests;

public class DatabaseFixture : IDisposable
{
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithName("db")
        .WithPortBinding(5432, 5432)
        .WithDatabase("identity")
        .WithUsername("username")
        .WithPassword("password")
        .Build();

    public DatabaseFixture()
    {
        _postgreSqlContainer.StartAsync().Wait();
    }

    public void Dispose()
    {
        _postgreSqlContainer.DisposeAsync().AsTask().Wait();
    }
}
