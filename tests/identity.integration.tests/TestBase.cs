using identity.api.Infrastructure;
using Microsoft.Extensions.DependencyInjection;

namespace identity.integration.tests;

public abstract class TestBase : IDisposable
{
    protected readonly HttpClient Client;
    protected readonly IdentityDbContext DbContext;
    private readonly IServiceScope _scope;

    protected TestBase(DatabaseFixture fixture)
    {
        Client = fixture.Factory.CreateClient();
        _scope = fixture.Factory.Services.CreateScope();
        DbContext = _scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        Client.Dispose();
        _scope.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}
