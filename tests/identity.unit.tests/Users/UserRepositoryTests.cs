using FluentAssertions;
using identity.api.Infrastructure;
using identity.api.Models;
using identity.api.Repositories;
using Microsoft.EntityFrameworkCore;

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
            new User { Username = username, IsActive = true, Role = new Role() },
            new User { Username = "test.user2", IsActive = true, Role = new Role() }
        );
        await context.SaveChangesAsync();
        
        var repository = new UserRepository(context);

        // Act
        var user = await repository.GetByUsername(username);

        // Assert
        user.Should().NotBeNull();
        user.Username.Should().Be(username);
    }
}