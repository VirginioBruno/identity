using identity.api.Infrastructure;
using identity.api.Models;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace identity.api.Utils;

public static class DataInitializer
{
    public static async Task InitializeAsync(WebApplication app)
    {
        await using var scope = app.Services.CreateAsyncScope();
        var context = scope.ServiceProvider.GetRequiredService<IdentityDbContext>();

        await context.Database.MigrateAsync();

        foreach (var roleName in new[] { "admin", "user" })
        {
            if (await context.Roles.AnyAsync(role => role.RoleName == roleName))
                continue;

            await context.Roles.AddAsync(new Role
            {
                RoleName = roleName,
                CreationAt = DateTime.UtcNow,
                IsActive = true
            });
        }

        await context.SaveChangesAsync();

        var configuration = scope.ServiceProvider.GetRequiredService<IConfiguration>();
        var adminPassword = configuration["BootstrapAdmin:Password"];
        if (string.IsNullOrWhiteSpace(adminPassword))
            return;

        var adminUsername = configuration["BootstrapAdmin:Username"] ?? "admin";
        var adminEmail = configuration["BootstrapAdmin:Email"] ?? "admin@localhost";

        var adminUser = await context.Users.SingleOrDefaultAsync(u => u.Username == adminUsername);
        if (adminUser is not null) return;

        var role = await context.Roles.SingleAsync(r => r.RoleName == "admin");
        
        var admin = new User
        {
            Username = adminUsername,
            Email = adminEmail,
            HashPassword = BC.HashPassword(adminPassword),
            RoleId = role.Id,
            IsActive = true,
            CreationAt = DateTime.UtcNow
        };

        await context.Users.AddAsync(admin);
        await context.SaveChangesAsync();
    }
}
