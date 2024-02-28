using identity.api.Infrastructure;
using Microsoft.AspNetCore.Mvc.Testing;
using Microsoft.Extensions.DependencyInjection;

namespace identity.integration.tests;

public class TestBase : IClassFixture<WebApplicationFactory<Program>>, IDisposable
{
    protected readonly HttpClient Client;
    protected readonly IdentityDbContext DbContext;
    private readonly IServiceScope _scope;

    protected TestBase(WebApplicationFactory<Program> factory)
    {
        try
        {
            Client = factory.CreateClient();
        
            _scope = factory.Services.CreateScope();
            DbContext = _scope.ServiceProvider.GetRequiredService<IdentityDbContext>();
        }
        catch (Exception)
        {
            Dispose();
        }
    }

    private void Dispose(bool disposing)
    {
        if (!disposing) return;
        _scope.Dispose();
        Client.Dispose();
    }

    public void Dispose()
    {
        Dispose(true);
        GC.SuppressFinalize(this);
    }
}