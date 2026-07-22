using System.Net;
using System.Net.Http.Json;
using identity.api.Models;
using identity.api.Responses;
using BC = BCrypt.Net.BCrypt;

namespace identity.integration.tests.Authentication;

[Collection("DatabaseCollection")]
public class AuthenticationTests : TestBase
{
    private const string Route = "api/v1/authentication";
    
    public AuthenticationTests(DatabaseFixture fixture) : base(fixture)
    {
    }

    [Fact(DisplayName = "Should authenticate user when it exists on database")]
    public async Task AuthenticationPost_UserExistsOnDatabase_ShouldAuthenticate()
    {
        //Arrange
        var username = "test";
        var password = "test123";
        
        await DbContext.Users.AddRangeAsync(
            new User { Username = username, Email = $"{username}@test.com", HashPassword = BC.HashPassword(password), IsActive = true, Role = new Role { RoleName = "admin" } },
            new User { Username = "test2", Email = "test2@test.com", HashPassword = BC.HashPassword("test"), IsActive = true, Role = new Role { RoleName = "admin" }}
        );
        await DbContext.SaveChangesAsync();

        //Act
        var result = await Client.PostAsJsonAsync(Route, new
        {
            username,
            password
        });

        //Assert
        Assert.Equal(HttpStatusCode.OK, result.StatusCode);
        var response = await result.Content.ReadFromJsonAsync<AuthenticationResponse>();
        Assert.False(string.IsNullOrWhiteSpace(response?.Token));
    }
    
    [Fact(DisplayName = "Should not authenticate user when it does not exists on database")]
    public async Task AuthenticationPost_UserDoesNotExistsOnDatabase_ShouldNotAuthenticate()
    {
        //Arrange
        //Act
        var result = await Client.PostAsJsonAsync(Route, new
        {
            username = "username",
            password = "password"
        });

        //Assert
        Assert.Equal(HttpStatusCode.Unauthorized, result.StatusCode);
    }
}
