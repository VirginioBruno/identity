using identity.api.Infrastructure;
using identity.api.Models;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace identity.api.Utils;

public static class DataInitializer
{
    public static async Task Initialize(WebApplication app)
    {
        var scope = app.Services.CreateScope();
        var context = scope.ServiceProvider.GetService<IdentityDbContext>();
        await context.Database.MigrateAsync();

        var adminUser = await context.Users.SingleOrDefaultAsync(u => u.Username.Equals("admin"));
        if (adminUser is not null) return;

        var role = new Role { RoleName = "admin" };
        await context.Roles.AddAsync(role);
        
        var admin = new User
        {
            Username = "admin",
            Email = "admin@identity.com",
            HashPassword = BC.HashPassword("admin"),
            RoleId = role.Id,
            IsActive = true,
            CreationAt = DateTime.Now
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}