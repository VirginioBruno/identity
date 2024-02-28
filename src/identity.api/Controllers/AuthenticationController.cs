using Asp.Versioning;
using identity.api.Repositories;
using identity.api.Requests;
using identity.api.Responses;
using identity.api.Utils;
using identity.api.Validators;
using Microsoft.AspNetCore.Mvc;

namespace identity.api.Controllers;

[ApiController]
[ApiVersion("1.0")]
[Route("api/v{version:apiVersion}/[controller]")]
public class AuthenticationController : ControllerBase
{
    private readonly IUserRepository _userRepository;

    public AuthenticationController(IUserRepository userRepository) => _userRepository = userRepository;

    [HttpPost]
    public async Task<ActionResult<AuthenticationResponse>> Post([FromBody] AuthenticationRequest request)
    {
        var user = await _userRepository.GetByUsername(request.Username);

        if (user is null)
            return NotFound($"The user {request.Username} was not found");

        if (!UserValidator.Validate(user.HashPassword, request.Password))
            return Unauthorized("The username or password is invalid");

        var token = TokenGenerator.Generate(user);
        return Ok(new AuthenticationResponse(token));
    }
}