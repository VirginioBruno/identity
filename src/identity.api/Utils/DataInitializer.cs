using identity.api.Models;
using identity.api.Repositories;
using identity.api.Requests;

namespace identity.api.Utils;

public static class DataInitializer
{
    public static async Task Initialize(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var userRepository = scope.ServiceProvider.GetService<IUserRepository>();

        var adminUser = await userRepository.GetByUsername("admin");
        if (adminUser is not null) return;
        
        var admin = new CreateUserRequest()
        {
            Username = "admin",
            Email = "admin@identity.com",
            Password = "admin",
            RoleId = Guid.Parse("44f83de8-99ab-4aaf-b869-ffd9a47130ce")
        };
        
        await userRepository.Insert((User)admin);
    }
}