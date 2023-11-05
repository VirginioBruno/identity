using identity.api.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;
using Testcontainers.PostgreSql;

namespace identity.integration.tests;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly IdentityDbContext DbContext;
    private readonly IServiceScope _scope;
    private readonly PostgreSqlContainer _postgreSqlContainer = new PostgreSqlBuilder()
        .WithName("db")
        .WithPortBinding(5432, 5432)
        .WithDatabase("identity")
        .WithUsername("username")
        .WithPassword("password")
        .Build();

    public TestBase(WebApplicationFactory<Program> factory)
    {
        try
        {
            _postgreSqlContainer.StartAsync().Wait();
            Client = factory.CreateClient();
        
            _scope = factory.Services.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        }
        catch (Exception e)
        {
            Dispose();
        }
    }

    private void Dispose(bool disposing)
    {
        if (disposing)
        {
            _scope.Dispose();
            Client.Dispose();
        }
    }

    public void Dispose()
    {
        _postgreSqlContainer.DisposeAsync().AsTask().Wait();
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}