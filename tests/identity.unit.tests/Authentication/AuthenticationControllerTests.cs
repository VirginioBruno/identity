using FluentAssertions;
using identity.api.Controllers;
using identity.api.Models;
using identity.api.Repositories;
using identity.api.Requests;
using identity.api.Responses;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using BC = BCrypt.Net.BCrypt;

namespace identity.unit.tests.Authentication;

public class AuthenticationControllerTests
{
    private readonly IUserRepository _userRepository; 
    private readonly AuthenticationController _controller;
    
    public AuthenticationControllerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _controller = new AuthenticationController(_userRepository);
    }
    
    [Fact(DisplayName = "Should authenticate user when username and password are correct")]
    public async Task Post_ExistentUserAndCorrectPassword_ShouldAuthenticate()
    {
        // Arrange
        var username = "test.user";
        var password = "test.password";
        var request = new AuthenticationRequest()
        {
            Username = username,
            Password = password
        };

        _userRepository.GetByUsername(Arg.Any<string>())
            .Returns(new User()
            {
                Username = username,
                HashPassword = BC.HashPassword(password),
                CreationAt = DateTime.Now,
                IsActive = true,
                Email = "test.user@test.com",
                Id = Guid.NewGuid(),
                RoleId = Guid.NewGuid(),
                Role = new Role() { RoleName = "admin" }
            });

        // Act
        var result = await _controller.Post(request);

        // Assert
        result.Should().NotBeNull();
        result.Result.Should().NotBeNull();
        
        var objectResult = result.Result as ObjectResult;
        objectResult.Should().NotBeNull();
        objectResult!.StatusCode.Should().Be(StatusCodes.Status200OK);
        
        var authenticationResponse = objectResult!.Value as AuthenticationResponse;
        authenticationResponse.Should().NotBeNull();
        authenticationResponse!.Token.Should().NotBeNull();
    }
}