using FluentAssertions;
using identity.api.Infrastructure;
using identity.api.Models;
using identity.api.Repositories;
using Microsoft.EntityFrameworkCore;
using BC = BCrypt.Net.BCrypt;

namespace identity.unit.tests.Users;

public class UserRepositoryTest
{
    private readonly DbContextOptions<IdentityDbContext> _options = new DbContextOptionsBuilder<IdentityDbContext>()
        .UseInMemoryDatabase(databaseName: "UserRepositoryTestDatabase") 
        .Options;

    [Fact(DisplayName = "Should return user data when the user exists")]
    public async Task GetByUsername_UserExists_ReturnUserData()
    {
        // Arrange
        var username = "test.user";
        await using var context = new IdentityDbContext(_options);
        await context.Users.AddRangeAsync(
            new User { Username = username, Email = $"{username}@test.com", HashPassword = BC.HashPassword("test"), IsActive = true, Role = new Role { RoleName = "admin" } },
            new User { Username = "test.user2", Email = "test.user2@test.com", HashPassword = BC.HashPassword("test"), IsActive = true, Role = new Role { RoleName = "admin" }}
        );
        await context.SaveChangesAsync();
        
        var repository = new UserRepository(context);

        // Act
        var user = await repository.GetByUsername(username);

        // Assert
        user.Should().NotBeNull();
        user!.Username.Should().Be(username);
    }
}