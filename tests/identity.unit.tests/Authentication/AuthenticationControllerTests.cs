using identity.api.Controllers;
using identity.api.Models;
using identity.api.Repositories;
using identity.api.Requests;
using identity.api.Responses;
using identity.api.Utils;
using Microsoft.AspNetCore.Http;
using Microsoft.AspNetCore.Mvc;
using NSubstitute;
using BC = BCrypt.Net.BCrypt;

namespace identity.unit.tests.Authentication;

public class AuthenticationControllerTests
{
    private readonly IUserRepository _userRepository; 
    private readonly ITokenGenerator _tokenGenerator;
    private readonly AuthenticationController _controller;
    
    public AuthenticationControllerTests()
    {
        _userRepository = Substitute.For<IUserRepository>();
        _tokenGenerator = Substitute.For<ITokenGenerator>();
        _tokenGenerator.Generate(Arg.Any<User>()).Returns("test-token");
        _controller = new AuthenticationController(_userRepository, _tokenGenerator);
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
        var objectResult = Assert.IsType<OkObjectResult>(result.Result);
        Assert.Equal(StatusCodes.Status200OK, objectResult.StatusCode);

        var authenticationResponse = Assert.IsType<AuthenticationResponse>(objectResult.Value);
        Assert.Equal("test-token", authenticationResponse.Token);
        _tokenGenerator.Received(1).Generate(Arg.Is<User>(user => user != null && user.Username == username));
    }

    [Fact(DisplayName = "Should return the same unauthorized response for unknown users")]
    public async Task Post_UnknownUser_ShouldReturnUnauthorized()
    {
        var request = new AuthenticationRequest { Username = "unknown", Password = "password" };
        _userRepository.GetByUsername(request.Username).Returns((User?)null);

        var result = await _controller.Post(request);

        var unauthorized = Assert.IsType<UnauthorizedObjectResult>(result.Result);
        Assert.Equal("The username or password is invalid", unauthorized.Value);
        _tokenGenerator.DidNotReceive().Generate(Arg.Any<User>());
    }
}
