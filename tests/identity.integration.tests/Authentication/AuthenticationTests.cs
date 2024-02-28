using System.Net;
using System.Net.Http.Json;
using FluentAssertions;
using identity.api.Models;
using Microsoft.AspNetCore.Mvc.Testing;
using BC = BCrypt.Net.BCrypt;

namespace identity.integration.tests.Authentication;

[Collection("DatabaseCollection")]
public class AuthenticationTests : TestBase
{
    private const string Route = "api/v1/authentication";
    
    public AuthenticationTests(WebApplicationFactory<Program> factory) : base(factory)
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
        result.Should().NotBeNull();
        result.IsSuccessStatusCode.Should().BeTrue();
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
        result.Should().NotBeNull();
        result.StatusCode.Should().Be(HttpStatusCode.NotFound);
    }
}